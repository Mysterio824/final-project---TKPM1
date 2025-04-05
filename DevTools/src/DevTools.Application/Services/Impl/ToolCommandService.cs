using DevTools.Application.Common;
using DevTools.Infrastructure.Repositories;
using DevTools.Domain.Entities;
using DevTools.Application.Exceptions;
using DevTools.Application.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Request.Tool;
using AutoMapper;
using DevTools.Application.DTOs.Response;

namespace DevTools.Application.Services.Impl
{
    public class ToolCommandService : IToolCommandService
    {
        private readonly IToolRepository _toolRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<ToolCommandService> _logger;
        private readonly string _toolDirectory;
        private readonly SemaphoreSlim _toolLock;
        private readonly IMapper _mapper;

        public ToolCommandService(
            IToolRepository toolRepository,
            IFileService fileService,
            ILogger<ToolCommandService> logger,
            IMapper mapper,
            string toolDirectory = "Tools")
        {
            _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _toolDirectory = toolDirectory;
            _toolLock = new SemaphoreSlim(1, 1);

            EnsureToolDirectoryExists();
        }

        private void EnsureToolDirectoryExists()
            => Directory.CreateDirectory(_toolDirectory);

        public async Task<CreateToolResponseDto> AddToolAsync(CreateToolDto request)
        {
            await _toolLock.WaitAsync();
            try
            {
                ValidateToolFile(request.File);
                string filePath = _fileService.SaveFile(request.File, request.Name);

                var (toolType, toolInstance) = ValidateAndCreateToolInstance(filePath);
                var newTool = CreateToolEntity(toolInstance, filePath);

                await EnsureToolIsUnique(newTool, filePath);
                await _toolRepository.AddAsync(newTool);

                _logger.LogInformation("Tool {ToolName} added successfully.", newTool.Name);
                return _mapper.Map<CreateToolResponseDto>(newTool);
            }
            finally
            {
                _toolLock.Release();
            }
        }

        public async Task<UpdateToolResponseDto> UpdateToolAsync(UpdateToolDto request)
        {
            await _toolLock.WaitAsync();
            try
            {
                var tool = await _toolRepository.GetByIdAsync(request.Id);
                if (tool == null)
                {
                    _logger.LogWarning("Tool with ID {ToolId} not found.", request.Id);
                    throw new NotFoundException($"Tool {request.Id} not found.");
                }

                ValidateToolFile(request.File);
                string oldFilePath = tool.DllPath;
                _fileService.DeleteFile(oldFilePath);
                string filePath = _fileService.SaveFile(request.File, request.Name);

                tool = _mapper.Map<Tool>(request);
                tool.DllPath = filePath;

                await _toolRepository.UpdateAsync(tool);

                return _mapper.Map<UpdateToolResponseDto>(tool);
            }
            finally
            {
                _toolLock.Release();
            }
        }

        public async Task<BaseResponseDto> DeleteToolAsync(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                _logger.LogWarning("Tool with ID {ToolId} not found.", id);
                throw new NotFoundException($"Tool {id} not found.");
            }

            _fileService.DeleteFile(tool.DllPath);
            await _toolRepository.DeleteAsync(tool);

            _logger.LogInformation("Deleted tool with ID {ToolId}.", id);
            return _mapper.Map<BaseResponseDto>(tool);
        }

        private static void ValidateToolFile(FormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (Path.GetExtension(file.FileName)?.ToLower() != ".dll")
                throw new ArgumentException("Only DLL files are allowed.");
        }

        private static (Type, ITool) ValidateAndCreateToolInstance(string filePath)
        {
            if (!ToolValidator.IsValidTool(filePath, out Type? toolType) || toolType == null)
            {
                File.Delete(filePath);
                throw new BadRequestException("Invalid tool DLL. No valid implementation of ITool found.");
            }

            var toolInstance = (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

            return (toolType, toolInstance);
        }

        private static Tool CreateToolEntity(ITool toolInstance, string filePath)
            => new()
            {
                Name = toolInstance.Name,
                Description = toolInstance.Description,
                DllPath = filePath,
                IsEnabled = true,
                IsPremium = false,
            };

        private async Task EnsureToolIsUnique(Tool newTool, string filePath)
        {
            var existingTools = await _toolRepository.GetAll();
            if (existingTools.Contains(newTool, new ToolComparer()))
            {
                _fileService.DeleteFile(filePath);
                throw new BadRequestException("A tool with the same name already exists.");
            }
        }

        public async Task<UpdateToolResponseDto> DisableTool(int id)
            => await ToggleToolStatus(id, false);

        public async Task<UpdateToolResponseDto> EnableTool(int id)
            => await ToggleToolStatus(id, true);

        private async Task<UpdateToolResponseDto> ToggleToolStatus(int id, bool enable)
        {
            var tool = await _toolRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Tool {id} not found.");

            if (tool.IsEnabled == enable)
                throw new BadRequestException($"Tool is already {(enable ? "enabled" : "disabled")}.");

            tool.IsEnabled = enable;
            await _toolRepository.UpdateAsync(tool);
            return _mapper.Map<UpdateToolResponseDto>(tool);
        }

        public async Task<UpdateToolResponseDto> SetPremium(int id)
            => await TogglePremiumStatus(id, true);

        public async Task<UpdateToolResponseDto> SetFree(int id)
            => await TogglePremiumStatus(id, false);

        private async Task<UpdateToolResponseDto> TogglePremiumStatus(int id, bool isPremium)
        {
            var tool = await _toolRepository.GetByIdAsync(id)
                        ?? throw new NotFoundException($"Tool {id} not found.");

            if (tool.IsPremium == isPremium)
                throw new BadRequestException($"Tool is already {(isPremium ? "premium" : "free")}.");

            tool.IsPremium = isPremium;
            await _toolRepository.UpdateAsync(tool);
            return _mapper.Map<UpdateToolResponseDto>(tool);
        }

        public async Task UpdateToolList()
        {
            await _toolLock.WaitAsync();
            try
            {
                var discoveredToolPaths = new HashSet<string>();
                var existingTools = await _toolRepository.GetAll();

                await ProcessDiscoveredDlls(discoveredToolPaths, existingTools);
                await RemoveUnusedTools(existingTools, discoveredToolPaths);
            }
            finally
            {
                _toolLock.Release();
            }
        }   

        private async Task ProcessDiscoveredDlls(HashSet<string> discoveredToolPaths, IEnumerable<Tool> existingTools)
        {
            foreach (var dllPath in Directory.GetFiles(_toolDirectory, "*.dll"))
            {
                try
                {
                    var fileName = Path.GetFileName(dllPath);
                    if (!ToolValidator.IsValidTool(dllPath, out Type? toolType) || toolType == null)
                        continue;

                    var toolInstance = CreateToolInstance(toolType);
                    discoveredToolPaths.Add(fileName);

                    if (ExistingToolWithSameName(existingTools, toolInstance))
                        continue;

                    await AddNewTool(dllPath, toolInstance);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error processing DLL {DllPath}: {ErrorMessage}", dllPath, ex.Message);
                }
            }
        }

        private static ITool CreateToolInstance(Type toolType)
            => (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

        private bool ExistingToolWithSameName(IEnumerable<Tool> existingTools, ITool toolInstance)
        {
            var duplicate = existingTools.Any(t =>
                t.Name.Equals(toolInstance.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                _logger.LogWarning("Skipping {ToolName}: A tool with the same name already exists.", toolInstance.Name);
                return true;
            }
            return false;
        }

        private async Task AddNewTool(string dllPath, ITool toolInstance)
        {
            var newTool = new Tool
            {
                Name = toolInstance.Name,
                Description = toolInstance.Description,
                DllPath = dllPath,
                IsEnabled = true,
                IsPremium = false
            };
            await _toolRepository.AddAsync(newTool);
        }

        private async Task RemoveUnusedTools(IEnumerable<Tool> existingTools, HashSet<string> discoveredToolPaths)
        {
            var toolsToRemove = existingTools
                .Where(t => !discoveredToolPaths.Contains(Path.GetFileName(t.DllPath)))
                .ToList();

            foreach (var toolToRemove in toolsToRemove)
            {
                await _toolRepository.DeleteAsync(toolToRemove);
            }
        }
    }
}