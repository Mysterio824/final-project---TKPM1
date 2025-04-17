using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.ViewModels
{
    public class AdminDashboardViewModel : ObservableObject
    {
        private readonly ToolService _toolService;
        private readonly ToolUploadService _toolUploadService;
        public ObservableCollection<Tool> Tools { get; } = new();

        public AdminDashboardViewModel(ToolService toolService, ToolUploadService toolUploadService)
        {
            _toolService = toolService;
            _toolUploadService = toolUploadService;
            LoadAsync();
        }

        private async void LoadAsync()
        {
            var tools = await _toolService.GetToolsAsync();
            Tools.Clear();
            foreach (var tool in tools)
                Tools.Add(tool);
        }

        public async void SetToolEnabled(Tool tool)
        {
            await _toolUploadService.EnableToolAsync(tool.Id, tool.IsEnabled);
        }
    }
}
