using DevTools.Application.Common;
using DevTools.Application.Services;
using DevTools.Application.Utils;
using DevTools.DataAccess.Repositories;
using DevTools.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Helpers
{
    public static class ToolDiscoveryHelper
    {
        public static void ProcessDiscoveredDlls(
            string toolDirectory, 
            HashSet<string> discoveredToolPaths, 
            IEnumerable<Tool> existingTools,
            IFileService fileService,
            ILogger logger)
        {
            foreach (var dllPath in Directory.GetFiles(toolDirectory, "*.dll"))
            {
                try
                {
                    var fileName = Path.GetFileName(dllPath);

                    discoveredToolPaths.Add(fileName);

                    if (!(existingTools
                        .Any(t => t.Name
                            .Equals(fileName, StringComparison.OrdinalIgnoreCase))))
                    {
                        fileService.DeleteFile(dllPath);
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
