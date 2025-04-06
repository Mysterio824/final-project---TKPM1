using DevTools.Application.Common;
using DevTools.Application.Utils;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Helpers
{
    public static class ToolDiscoveryHelper
    {
        public static void ProcessDiscoveredDlls(string toolDirectory, HashSet<string> discoveredToolPaths, IEnumerable<Tool> existingTools, IToolRepository toolRepository, ILogger logger)
        {
            foreach (var dllPath in Directory.GetFiles(toolDirectory, "*.dll"))
            {
                try
                {
                    var fileName = Path.GetFileName(dllPath);

                    if (!ToolValidator.IsValidTool(dllPath))
                        continue;

                    discoveredToolPaths.Add(fileName);

                    if (!(existingTools
                        .Any(t => t.Name
                            .Equals(fileName, StringComparison.OrdinalIgnoreCase))))
                    {
                        FileHelper.DeleteFile(dllPath);
                        logger.LogInformation("Deleted unmatched DLL file: {DllPath}", dllPath);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("Error processing DLL {DllPath}: {ErrorMessage}", dllPath, ex.Message);
                }
            }
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
