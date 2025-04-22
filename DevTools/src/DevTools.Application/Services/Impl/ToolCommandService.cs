using DevTools.Application.DTOs.Request.Tool;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Domain.Entities;
using DevTools.Application.Helpers;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DevTools.Application.Exceptions;
using DevTools.Application.Utils;
using DevTools.DataAccess.Repositories;

namespace DevTools.Application.Services.Impl
{
    public class ToolCommandService : IToolCommandService
    {
        private readonly IToolRepository _toolRepository;
        private readonly IFileService _fileService;
        private readonly IToolGroupRepository _toolGroupRepository;
        private readonly ILogger<ToolCommandService> _logger;
        private readonly string _toolDirectory;
        private readonly SemaphoreSlim _toolLock;
        private readonly IMapper _mapper;

        public ToolCommandService(
            IToolRepository toolRepository,
            IFileService fileService,
            IToolGroupRepository toolGroupRepository,
            ILogger<ToolCommandService> logger,
            IMapper mapper,
            string toolDirectory = "Tools")
        {
            _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _toolGroupRepository = toolGroupRepository ?? throw new ArgumentNullException(nameof(toolGroupRepository));
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

                var newTool = _mapper.Map<Tool>(request);
                var toolGroup = await _toolGroupRepository.GetByIdAsync(request.GroupId);
                if (toolGroup == null)
                {
                    _fileService.DeleteFile(filePath);
                    throw new NotFoundException($"Tool group {request.GroupId} not found.");
                }
                newTool.Group = toolGroup;

                var existingTool = await _toolRepository.GetByNameAsync(request.Name);
                if (existingTool != null && toolGroup == existingTool.Group)
                {
                    _fileService.DeleteFile(filePath);
                    throw new BadRequestException("A tool with the same name already exists.");
                }

                newTool.DllPath = filePath;


                var existingTools = await _toolRepository.GetAll();
                if (existingTools.Contains(newTool, new ToolComparer()))
                {
                    _fileService.DeleteFile(filePath);
                    throw new BadRequestException("A tool with the same name already exists.");
                }

                newTool = await _toolRepository.AddAsync(newTool);

                _logger.LogInformation("Tool {ToolName} added successfully.", newTool.Name);
                return new CreateToolResponseDto
                {
                    Id = newTool.Id
                };
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

                var toolGroup = await _toolGroupRepository.GetByIdAsync(request.GroupId)
                    ?? throw new NotFoundException($"Tool group {request.GroupId} not found.");

                string filePath = tool.DllPath;

                // Handle file update if provided
                if (request.File != null)
                {
                    FileHelper.ValidateToolFile(request.File);
                    string oldFilePath = tool.DllPath;
                    _fileService.DeleteFile(oldFilePath);
                    filePath = _fileService.SaveFile(request.File, request.Name);
                }

                // Update tool properties
                tool.Name = request.Name;
                tool.Description = request.Description;
                tool.IsPremium = request.IsPremium;
                tool.IsEnabled = request.IsEnabled;
                tool.Group = toolGroup;
                tool.DllPath = filePath;

                await _toolRepository.UpdateAsync(tool);

                return new UpdateToolResponseDto
                {
                    Id = tool.Id
                };
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
            return new UpdateToolResponseDto { Id = tool.Id };
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
            return new UpdateToolResponseDto
            {
                Id = tool.Id
            };
        }

        public async Task UpdateToolList()
        {
            _logger.LogInformation("Updating tool list...");
            await _toolLock.WaitAsync();
            try
            {
                var existingTools = await _toolRepository.GetAll();

                var discoveredToolPaths = ToolDiscoveryHelper.ProcessDiscoveredDlls(_toolDirectory, existingTools, _fileService, _logger);

                await ToolDiscoveryHelper.RemoveUnusedTools(existingTools, discoveredToolPaths, _toolRepository);
            }
            finally
            {
                _toolLock.Release();
            }
        }
    }
}