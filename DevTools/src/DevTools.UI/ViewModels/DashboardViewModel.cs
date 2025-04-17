using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using DevTools.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        public ObservableCollection<Tool> Tools { get; } = new();

        public DashboardViewModel()
        {
            LoadToolsAsync();
        }

        private async void LoadToolsAsync()
        {
            var tools = await AppServices.ToolService.GetToolsAsync();
            Tools.Clear();
            foreach (var tool in tools)
                Tools.Add(tool);
        }

        public void NavigateToTool(Tool tool)
        {
            AppServices.NavigationService.Navigate(typeof(ToolDetailPage), tool);
        }
    }
}
