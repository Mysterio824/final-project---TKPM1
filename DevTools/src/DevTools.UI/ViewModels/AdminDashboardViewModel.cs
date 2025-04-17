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
        public ObservableCollection<Tool> Tools { get; } = new();

        public AdminDashboardViewModel()
        {
            LoadAsync();
        }

        private async void LoadAsync()
        {
            var tools = await AppServices.ToolService.GetToolsAsync();
            Tools.Clear();
            foreach (var tool in tools)
                Tools.Add(tool);
        }

        public async void SetToolEnabled(Tool tool)
        {
            await AppServices.ToolUploadService.EnableToolAsync(tool.Id, tool.IsEnabled);
        }
    }
}
