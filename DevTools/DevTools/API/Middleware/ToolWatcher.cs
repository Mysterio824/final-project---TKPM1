using DevTools.Application.Interfaces.Services;

namespace DevTools.API.Middleware
{
    public class ToolWatcher(string folderPath, IToolCommandService toolService)
    {
        private readonly string _folderPath = folderPath;
        private readonly IToolCommandService _toolService = toolService;

        public void StartWatching()
        {
            FileSystemWatcher watcher = new(_folderPath, "*.dll")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            watcher.Created += async (s, e) => await OnChanged(e.FullPath);
            watcher.Changed += async (s, e) => await OnChanged(e.FullPath);
            watcher.Deleted += async (s, e) => await OnChanged(e.FullPath);

            watcher.EnableRaisingEvents = true;
        }

        private async Task OnChanged(string filePath)
        {
            Console.WriteLine($"🔄 Tool update detected: {filePath}");
            await _toolService.UpdateToolList();
        }
    }
}