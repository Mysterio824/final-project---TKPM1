using DevTools.DTOs.Response;
using DevTools.Entities;
using DevTools.Enums;
using DevTools.Exceptions;
using DevTools.Interfaces;
using DevTools.Interfaces.Repositories;
using DevTools.Interfaces.Services;
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
                IsEnabled = (t.IsPremium && (role == UserRole.User || role == UserRole.Anonymous)) ? true : t.IsEnabled,
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
                IsEnabled = (t.IsPremium && (role == UserRole.User || role == UserRole.Anonymous)) ? true : t.IsEnabled,
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

            var favTool = userId == -1 
                ? await _favoriteToolRepository.GetAsync(userId, id)
                : null;

            var toolDTO = new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                Description = tool.Description,
                IsEnabled = (tool.IsPremium && (role == UserRole.User || role == UserRole.Anonymous)) ? true : tool.IsEnabled,
                IsPremium = tool.IsPremium,
                Type = tool.Type,
                IsFavorite = (favTool != null)
            };
            return toolDTO;
        }

        public async Task AddToolAsync(Tool tool) => await _toolRepository.AddAsync(tool);

        public async Task UpdateToolAsync(Tool tool) => await _toolRepository.UpdateAsync(tool);

        public async Task DeleteToolAsync(int id) => await _toolRepository.DeleteAsync(id);

        /// Fetch tool DLLs and update DB
        public async Task UpdateToolList()
        {
            var dllFiles = Directory.GetFiles(_toolDirectory, "*.dll")
                .Select(Path.GetFileName)
                .ToHashSet();

            var existingTools = await _toolRepository.GetAllAsync();
            var discoveredToolPaths = new HashSet<string>();

            // Process tools found in DLL files
            foreach (var dllPath in Directory.GetFiles(_toolDirectory, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var toolType = assembly.GetTypes()
                        .FirstOrDefault(t =>
                            typeof(ITool).IsAssignableFrom(t) &&
                            !t.IsInterface &&
                            !t.IsAbstract);

                    if (toolType == null) continue;

                    var toolInstance = (ITool)Activator.CreateInstance(toolType);
                    var fileName = Path.GetFileName(dllPath);
                    discoveredToolPaths.Add(fileName);

                    // Check if tool already exists in database
                    var existingTool = existingTools
                        .FirstOrDefault(t => Path.GetFileName(t.DllPath) == fileName);

                    if (existingTool == null)
                    {
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

        public string ExecuteTool(int toolId, string input, UserRole role = UserRole.User)
        {
            var tool = _toolRepository.GetByIdAsync(toolId).Result;
            if (tool == null || !File.Exists(tool.DllPath))
                throw new ArgumentNullException("Tool not found or missing DLL.");

            if(tool.IsEnabled == false)
                throw new UnauthorizedAccessException("Tool is disabled.");

            if (tool.IsPremium && (role == UserRole.User || role == UserRole.Anonymous))
                throw new UnauthorizedAccessException("Tool is premium.");

            var assembly = Assembly.LoadFrom(tool.DllPath);

            var toolType = assembly.GetTypes().FirstOrDefault(t =>
                t.GetInterfaces().Any(i => i.Name == "ITool") && !t.IsInterface && !t.IsAbstract);

            if (toolType == null)
                throw new Exception("Invalid tool DLL: No ITool implementation found.");

            var toolInstance = Activator.CreateInstance(toolType);
            var executeMethod = toolType.GetMethod("Execute");

            if (executeMethod == null)
                throw new Exception("Invalid tool DLL: Missing Execute method.");

            return (string)executeMethod.Invoke(toolInstance, new object[] { input });
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
