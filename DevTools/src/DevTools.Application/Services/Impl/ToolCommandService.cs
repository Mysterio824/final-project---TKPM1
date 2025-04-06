using DevTools.Application.DTOs.Request.Tool;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Repositories;
using DevTools.Application.Helpers;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DevTools.Application.Exceptions;

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
                FileHelper.ValidateToolFile(request.File);
                string filePath = _fileService.SaveFile(request.File, request.Name);

                var (toolType, toolInstance) = ToolHelper.ValidateAndCreateToolInstance(filePath);
                var newTool = ToolHelper.CreateToolEntity(toolInstance, filePath);

                await ToolUniquenessHelper.EnsureToolIsUnique(newTool, filePath, _toolRepository, _fileService);

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

                FileHelper.ValidateToolFile(request.File);
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

                await ToolDiscoveryHelper.ProcessDiscoveredDlls(_toolDirectory, discoveredToolPaths, existingTools, _toolRepository, _logger);

                await ToolDiscoveryHelper.RemoveUnusedTools(existingTools, discoveredToolPaths, _toolRepository);
            }
            finally
            {
                _toolLock.Release();
            }
        }
    }
}