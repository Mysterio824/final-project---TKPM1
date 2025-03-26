using DevTools.DTOs.Response;
using DevTools.Entities;
using DevTools.Enums;
using DevTools.Exceptions;
using DevTools.Interfaces;
using DevTools.Interfaces.Repositories;
using DevTools.Interfaces.Services;
using DevTools.Utils;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using StackExchange.Redis;
using System.Reflection;

namespace DevTools.Services
{
    public class ToolService : IToolService
    {
        private readonly IToolRepository _toolRepository;
        private readonly string _toolDirectory = "Tools";
        private readonly IFavoriteToolRepository _favoriteToolRepository;
        private readonly ILogger<ToolService> _logger;

        public ToolService(
            IToolRepository toolRepository,
            IFavoriteToolRepository favoriteToolRepository,
            ILogger<ToolService> logger
        ){
            _toolRepository = toolRepository;
            _favoriteToolRepository = favoriteToolRepository;
            _logger = logger;

            if (!Directory.Exists(_toolDirectory))
                Directory.CreateDirectory(_toolDirectory);
        }

        public async Task<IEnumerable<ToolDTO>> GetToolsAsync(UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetAllAsync();
            var favoriteToolIds = userId != -1
                ? (await _favoriteToolRepository.GetAll(userId))?.Select(f => f.ToolId).ToHashSet() ?? new HashSet<int>()
                : new HashSet<int>();

            var toolDTOs = toolList.Select(t => new ToolDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsEnabled = !t.IsPremium || (role != UserRole.User && role != UserRole.Anonymous) && t.IsEnabled,
                IsPremium = t.IsPremium,
                Type = t.Type,
                IsFavorite = favoriteToolIds.Contains(t.Id)
            });

            return toolDTOs;
        }

        public async Task<IEnumerable<ToolDTO>> GetToolFavoriteAsync(UserRole role, int userId) { 
            var tools = await _toolRepository.GetFavoriteAsync(userId); 
            var toolDTOs = tools.Select(t => new ToolDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsEnabled = !t.IsPremium || (role != UserRole.User && role != UserRole.Anonymous) && t.IsEnabled,
                IsPremium = t.IsPremium,
                Type = t.Type,
                IsFavorite = true
            });
            
            return toolDTOs;
        }

        public async Task<ToolDTO?> GetToolByIdAsync(int id, UserRole role, int userId = -1) { 
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                return null;
            }

            var favTool = userId != -1 
                ? await _favoriteToolRepository.GetAsync(userId, id)
                : null;

            var toolDTO = new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                Description = tool.Description,
                IsEnabled = !tool.IsPremium || (role != UserRole.User && role != UserRole.Anonymous) && tool.IsEnabled,
                IsPremium = tool.IsPremium,
                Type = tool.Type,
                IsFavorite = (favTool != null)
            };
            return toolDTO;
        }

        public async Task<IEnumerable<ToolDTO>> GetToolsByNameAsync(String name, UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetByNameAsync(name);
            var favoriteToolIds = userId != -1
                ? (await _favoriteToolRepository.GetAll(userId))?.Select(f => f.ToolId).ToHashSet() ?? new HashSet<int>()
                : new HashSet<int>();

            var toolDTOs = toolList.Select(t => new ToolDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsEnabled = !t.IsPremium || (role != UserRole.User && role != UserRole.Anonymous) && t.IsEnabled,
                IsPremium = t.IsPremium,
                Type = t.Type,
                IsFavorite = favoriteToolIds.Contains(t.Id)
            });

            return toolDTOs;
        }

        public async Task AddToolAsync(IFormFile file)
        {
            _logger.LogInformation("Adding tool...");
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (Path.GetExtension(file.FileName)?.ToLower() != ".dll")
                throw new ArgumentException("Only DLL files are allowed.");

            string filePath = Path.Combine(_toolDirectory, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (!ToolValidator.IsValidTool(filePath, out Type? toolType) || toolType == null)
            {
                File.Delete(filePath);
                throw new InvalidOperationException("Invalid tool DLL. No valid implementation of ITool found.");
            }

            var toolInstance = (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

            var newTool = new Tool
            {
                Name = toolInstance.Name,
                Description = toolInstance.Description,
                DllPath = filePath,
                IsEnabled = true,
                IsPremium = false,
                Type = toolInstance.Type
            };

            var existingTools = await _toolRepository.GetAllAsync();
            if (existingTools.Contains(newTool, new ToolComparer()))
            {
                File.Delete(filePath);
                throw new InvalidOperationException("A tool with the same name already exists.");
            }

            await _toolRepository.AddAsync(newTool);
            _logger.LogInformation($"Tool {newTool.Name} added successfully.");
        }

        public async Task UpdateToolAsync(Tool tool) => await _toolRepository.UpdateAsync(tool);

        public async Task DeleteToolAsync(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                _logger.LogWarning($"Tool with ID {id} not found.");
                return;
            }

            var dllPath = tool.DllPath;
            if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
            {
                try
                {
                    File.Delete(dllPath);
                    _logger.LogInformation($"Deleted DLL file: {dllPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to delete DLL {dllPath}: {ex.Message}");
                }
            }
            await _toolRepository.DeleteAsync(id);
            _logger.LogInformation($"Deleted tool with ID {id}.");
        }

        /// Fetch tool DLLs and update DB
        public async Task UpdateToolList()
        {
            var dllFiles = Directory.GetFiles(_toolDirectory, "*.dll")
                .Select(Path.GetFileName)
                .ToHashSet();

            var existingTools = await _toolRepository.GetAllAsync();
            var discoveredToolPaths = new HashSet<string>();

            foreach (var dllPath in Directory.GetFiles(_toolDirectory, "*.dll"))
            {
                try
                {
                    if (!ToolValidator.IsValidTool(dllPath, out Type? toolType) || toolType == null)
                        continue;

                    var toolInstance = (ITool?)Activator.CreateInstance(toolType)
                        ?? throw new InvalidCastException("Failed to create tool instance.");

                    var fileName = Path.GetFileName(dllPath);
                    discoveredToolPaths.Add(fileName);

                    if (existingTools.Any(t => t.Name.Equals(toolInstance.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning($"Skipping {toolInstance.Name}: A tool with the same name already exists.");
                        continue;
                    }

                    var newTool = new Tool
                    {
                        Name = toolInstance.Name,
                        Description = toolInstance.Description,
                        DllPath = dllPath,
                        IsEnabled = true,
                        IsPremium = false,
                        Type = toolInstance.Type
                    };
                    await _toolRepository.AddAsync(newTool);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing DLL {dllPath}: {ex.Message}");
                }
            }

            var toolsToRemove = existingTools
                .Where(t => !discoveredToolPaths.Contains(Path.GetFileName(t.DllPath)))
                .ToList();

            foreach (var toolToRemove in toolsToRemove)
            {
                await _toolRepository.DeleteAsync(toolToRemove.Id);
            }
        }

        public async Task<ToolResponse> ExecuteToolAsync(int toolId, string? input, IFormFile? file, UserRole role)
        {
            if (input == null && file == null)
                throw new ArgumentException("No input provided.");

            var tool = await _toolRepository.GetByIdAsync(toolId);
            if (tool == null || !File.Exists(tool.DllPath))
                throw new ArgumentException("Tool not found or missing DLL.");

            if (!tool.IsEnabled)
                throw new UnauthorizedAccessException("Tool is disabled.");

            if (tool.IsPremium && (role == UserRole.User || role == UserRole.Anonymous))
                throw new UnauthorizedAccessException("Tool is premium.");

            var assembly = Assembly.LoadFrom(tool.DllPath);
            var toolType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                ?? throw new InvalidOperationException("Invalid tool DLL: No ITool implementation found.");

            var toolInstance = (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

            ToolResponse response;
            if (file != null)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                response = toolInstance.Execute(fileBytes);
            }
            else
            {
                response = toolInstance.Execute(input!);
            }

            response.ToolId = toolId;

            return response;
        }

        public async Task DisableTool(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if(tool == null)
            {
                throw new NotFoundException("Tool " + nameof(id) + " not found.");
            }
            if (!tool.IsEnabled)
            {
                throw new InvalidOperationException("Tool is already disabled.");
            }
            tool.IsEnabled = false;
            await _toolRepository.UpdateAsync(tool);
        }

        public async Task EnableTool(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                throw new NotFoundException("Tool " + nameof(id) + " not found.");
            }
            if (tool.IsEnabled)
            {
                throw new InvalidOperationException("Tool is already enabled.");
            }
            tool.IsEnabled = true;
            await _toolRepository.UpdateAsync(tool);
        }

        public async Task SetPremium(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                throw new NotFoundException("Tool " + nameof(id) + " not found.");
            }
            if (tool.IsPremium)
            {
                throw new InvalidOperationException("Tool is already premium.");
            }
            tool.IsPremium = true;
            await _toolRepository.UpdateAsync(tool);
        }

        public async Task SetFree(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                throw new NotFoundException("Tool " + nameof(id) + " not found.");
            }
            if (!tool.IsPremium)
            {
                throw new InvalidOperationException("Tool is already free.");
            }
            tool.IsPremium = false;
            await _toolRepository.UpdateAsync(tool);
        }

    }
}
