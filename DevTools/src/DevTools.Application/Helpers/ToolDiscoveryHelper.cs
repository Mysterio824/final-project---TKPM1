using DevTools.Application.Common;
using DevTools.Application.Utils;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Helpers
{
    public static class ToolDiscoveryHelper
    {
        public static async Task ProcessDiscoveredDlls(string toolDirectory, HashSet<string> discoveredToolPaths, IEnumerable<Tool> existingTools, IToolRepository toolRepository, ILogger logger)
        {
            foreach (var dllPath in Directory.GetFiles(toolDirectory, "*.dll"))
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

                    await AddNewTool(dllPath, toolInstance, toolRepository);
                }
                catch (Exception ex)
                {
                    logger.LogError("Error processing DLL {DllPath}: {ErrorMessage}", dllPath, ex.Message);
                }
            }
        }

        private static ITool CreateToolInstance(Type toolType)
            => (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

        private static bool ExistingToolWithSameName(IEnumerable<Tool> existingTools, ITool toolInstance)
        {
            var duplicate = existingTools.Any(t =>
                t.Name.Equals(toolInstance.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                return true;
            }
            return false;
        }

        private static async Task AddNewTool(string dllPath, ITool toolInstance, IToolRepository toolRepository)
        {
            var newTool = new Tool
            {
                Name = toolInstance.Name,
                Description = toolInstance.Description,
                DllPath = dllPath,
                IsEnabled = true,
                IsPremium = false
            };
            await toolRepository.AddAsync(newTool);
        }

        public static async Task RemoveUnusedTools(IEnumerable<Tool> existingTools, HashSet<string> discoveredToolPaths, IToolRepository toolRepository)
        {
            var toolsToRemove = existingTools
                .Where(t => !discoveredToolPaths.Contains(Path.GetFileName(t.DllPath)))
                .ToList();

            foreach (var toolToRemove in toolsToRemove)
            {
                await toolRepository.DeleteAsync(toolToRemove);
            }
        }
    }
}
