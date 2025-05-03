using DevTools.Application.Services;
using DevTools.DataAccess.Repositories;
using DevTools.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Helpers
{
    public static class ToolDiscoveryHelper
    {
        public static HashSet<string> ProcessDiscoveredDlls(
            string toolDirectory,
            IEnumerable<Tool> existingTools,
            IFileService fileService,
            ILogger logger)
        {
            HashSet<string> discoveredToolPaths = new HashSet<string>();

            foreach (var filePath in Directory.GetFiles(toolDirectory))
            {
                try
                {
                    var fileName = Path.GetFileName(filePath);
                    var extension = Path.GetExtension(filePath);

                    if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        fileService.DeleteFile(filePath);
                        logger.LogInformation("Deleted non-DLL file: {FilePath}", filePath);
                        continue;
                    }


                    var isMatched = existingTools.Any(t =>
                        Path.GetFileName(t.DllPath).Equals(fileName, StringComparison.OrdinalIgnoreCase));

                    if (!isMatched)
                    {
                        fileService.DeleteFile(filePath);
                        logger.LogInformation("Deleted unmatched DLL file: {FilePath}", filePath);
                    } else
                    {
                        discoveredToolPaths.Add(fileName);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing file: {FilePath}", filePath);
                    return new HashSet<string>();
                }
            }
            return discoveredToolPaths;
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
