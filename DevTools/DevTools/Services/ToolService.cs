using DevTools.DTOs.Response;
using DevTools.Entities;
using DevTools.Enums;
using DevTools.Exceptions;
using DevTools.Interfaces.Core;
using DevTools.Interfaces.Repositories;
using DevTools.Interfaces.Services;
using DevTools.Utils;
using System.Reflection;

namespace DevTools.Services
{
    public class ToolService : IToolService
    {
        private readonly IToolRepository _toolRepository;
        private readonly IFavoriteToolRepository _favoriteToolRepository;
        private readonly ILogger<ToolService> _logger;
        private readonly string _toolDirectory;
        private readonly SemaphoreSlim _toolLock;

        public ToolService(
            IToolRepository toolRepository,
            IFavoriteToolRepository favoriteToolRepository,
            ILogger<ToolService> logger,
            string toolDirectory = "Tools")
        {
            _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
            _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _toolDirectory = toolDirectory;
            _toolLock = new SemaphoreSlim(1, 1);

            EnsureToolDirectoryExists();
        }

        private void EnsureToolDirectoryExists() 
            => Directory.CreateDirectory(_toolDirectory);

        public async Task<IEnumerable<ToolDTO>> GetToolsAsync(UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetAllAsync();
            var favoriteToolIds = await GetFavoriteToolIds(userId);

            return MapToToolDTOs(toolList, role, favoriteToolIds);
        }

        public async Task<IEnumerable<ToolDTO>> GetToolFavoriteAsync(UserRole role, int userId)
        {
            var tools = await _toolRepository.GetFavoriteAsync(userId);
            return MapToToolDTOs(tools, role, tools.Select(t => t.Id).ToHashSet());
        }

        public async Task<ToolDTO?> GetToolByIdAsync(int id, UserRole role, int userId = -1)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null) return null;

            var isFavorite = userId != -1 && await _favoriteToolRepository.GetAsync(userId, id) != null;

            return MapToToolDTO(tool, role, isFavorite);
        }

        public async Task<IEnumerable<ToolDTO>> GetToolsByNameAsync(string name, UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetByNameAsync(name);
            var favoriteToolIds = await GetFavoriteToolIds(userId);

            return MapToToolDTOs(toolList, role, favoriteToolIds);
        }

        private async Task<HashSet<int>> GetFavoriteToolIds(int userId)
            => userId != -1
                ? (await _favoriteToolRepository.GetAll(userId))?.Select(f => f.ToolId).ToHashSet()
                ?? []
                : [];

        private static IEnumerable<ToolDTO> MapToToolDTOs(IEnumerable<Tool> tools, UserRole role, HashSet<int> favoriteToolIds) 
            => tools.Select(t => MapToToolDTO(t, role, favoriteToolIds.Contains(t.Id)));

        private static ToolDTO MapToToolDTO(Tool tool, UserRole role, bool isFavorite)
            => new()
                {
                    Id = tool.Id,
                    Name = tool.Name,
                    Description = tool.Description,
                    IsEnabled = IsToolAccessible(tool, role),
                    IsPremium = tool.IsPremium,
                    Type = tool.Type,
                    IsFavorite = isFavorite
                };
        

        private static bool IsToolAccessible(Tool tool, UserRole role)
            => !tool.IsPremium || (role != UserRole.User && role != UserRole.Anonymous) && tool.IsEnabled;
        

        public async Task AddToolAsync(IFormFile file)
        {
            await _toolLock.WaitAsync();
            try
            {
                ValidateToolFile(file);
                string filePath = SaveToolFile(file);

                var (toolType, toolInstance) = ValidateAndCreateToolInstance(filePath);
                var newTool = CreateToolEntity(toolInstance, filePath);

                await EnsureToolIsUnique(newTool, filePath);
                await _toolRepository.AddAsync(newTool);

                _logger.LogInformation("Tool {ToolName} added successfully.", newTool.Name);
            }
            finally
            {
                _toolLock.Release();
            }
        }

        private static void ValidateToolFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (Path.GetExtension(file.FileName)?.ToLower() != ".dll")
                throw new ArgumentException("Only DLL files are allowed.");
        }

        private string SaveToolFile(IFormFile file)
        {
            string filePath = Path.Combine(_toolDirectory, file.FileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);
            return filePath;
        }

        private static (Type, ITool) ValidateAndCreateToolInstance(string filePath)
        {
            if (!ToolValidator.IsValidTool(filePath, out Type? toolType) || toolType == null)
            {
                File.Delete(filePath);
                throw new InvalidOperationException("Invalid tool DLL. No valid implementation of ITool found.");
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
                    Type = toolInstance.Type
                };

        private async Task EnsureToolIsUnique(Tool newTool, string filePath)
        {
            var existingTools = await _toolRepository.GetAllAsync();
            if (existingTools.Contains(newTool, new ToolComparer()))
            {
                File.Delete(filePath);
                throw new InvalidOperationException("A tool with the same name already exists.");
            }
        }

        public async Task UpdateToolAsync(Tool tool) 
            => await _toolRepository.UpdateAsync(tool);

        public async Task DeleteToolAsync(int id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                _logger.LogWarning("Tool with ID {ToolId} not found.", id);
                return;
            }

            DeleteToolDllFile(tool);
            await _toolRepository.DeleteAsync(id);
            _logger.LogInformation("Deleted tool with ID {ToolId}.", id);
        }

        private void DeleteToolDllFile(Tool tool)
        {
            var dllPath = tool.DllPath;
            if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
            {
                try
                {
                    File.Delete(dllPath);
                    _logger.LogInformation("Deleted DLL file: {DllPath}", dllPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to delete DLL {DllPath}: {ErrorMessage}", dllPath, ex.Message);
                }
            }
        }

        public async Task UpdateToolList()
        {
            await _toolLock.WaitAsync();
            try
            {
                var discoveredToolPaths = new HashSet<string>();
                var existingTools = await _toolRepository.GetAllAsync();

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
                IsPremium = false,
                Type = toolInstance.Type
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
                await _toolRepository.DeleteAsync(toolToRemove.Id);
            }
        }

        public async Task<ToolResponse> ExecuteToolAsync(int toolId, string? input, IFormFile? file, UserRole role)
        {
            ValidateToolExecution(input, file);
            var tool = await GetAndValidateTool(toolId, role);

            var assembly = Assembly.LoadFrom(tool.DllPath);
            var toolType = GetToolType(assembly);
            var toolInstance = CreateToolInstance(toolType);

            return await ExecuteTool(toolInstance, input, file, toolId);
        }

        private static void ValidateToolExecution(string? input, IFormFile? file)
        {
            if (input == null && file == null)
                throw new ArgumentException("No input provided.");
        }

        private async Task<Tool> GetAndValidateTool(int toolId, UserRole role)
        {
            var tool = await _toolRepository.GetByIdAsync(toolId);
            if (tool == null || !File.Exists(tool.DllPath))
                throw new ArgumentException("Tool not found or missing DLL.");

            ValidateToolAccess(tool, role);
            return tool;
        }

        private static void ValidateToolAccess(Tool tool, UserRole role)
        {
            if (!tool.IsEnabled)
                throw new UnauthorizedAccessException("Tool is disabled.");

            if (tool.IsPremium && (role == UserRole.User || role == UserRole.Anonymous))
                throw new UnauthorizedAccessException("Tool is premium.");
        }

        private static Type GetToolType(Assembly assembly)
            => assembly.GetTypes()
                .FirstOrDefault(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                ?? throw new InvalidOperationException("Invalid tool DLL: No ITool implementation found.");
        

        private static async Task<ToolResponse> ExecuteTool(ITool toolInstance, string? input, IFormFile? file, int toolId)
        {
            ToolResponse response = file != null
                ? await ExecuteToolWithFile(toolInstance, file)
                : ExecuteToolWithInput(toolInstance, input!);

            response.ToolId = toolId;
            return response;
        }

        private static async Task<ToolResponse> ExecuteToolWithFile(ITool toolInstance, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            return toolInstance.Execute(fileBytes);
        }

        private static ToolResponse ExecuteToolWithInput(ITool toolInstance, string input)
            => toolInstance.Execute(input);
        

        public async Task DisableTool(int id) 
            => await ToggleToolStatus(id, false);

        public async Task EnableTool(int id) 
            => await ToggleToolStatus(id, true);

        private async Task ToggleToolStatus(int id, bool enable)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
                throw new NotFoundException($"Tool {id} not found.");

            if (tool.IsEnabled == enable)
                throw new InvalidOperationException($"Tool is already {(enable ? "enabled" : "disabled")}.");

            tool.IsEnabled = enable;
            await _toolRepository.UpdateAsync(tool);
        }

        public async Task SetPremium(int id) 
            => await TogglePremiumStatus(id, true);

        public async Task SetFree(int id) 
            => await TogglePremiumStatus(id, false);

        private async Task TogglePremiumStatus(int id, bool isPremium)
        {
            var tool = await _toolRepository.GetByIdAsync(id)
                        ?? throw new NotFoundException($"Tool {id} not found.");

            if (tool.IsPremium == isPremium)
                throw new InvalidOperationException($"Tool is already {(isPremium ? "premium" : "free")}.");

            tool.IsPremium = isPremium;
            await _toolRepository.UpdateAsync(tool);
        }
    }
}