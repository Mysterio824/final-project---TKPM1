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
        private readonly ToolService _toolService;
        private readonly INavigationService _navigation;
        public ObservableCollection<Tool> Tools { get; } = new();

        public DashboardViewModel(ToolService toolService, INavigationService navigation)
        {
            _toolService = toolService;
            LoadToolsAsync();
            _navigation = navigation;
        }

        private async void LoadToolsAsync()
        {
            var tools = await _toolService.GetToolsAsync();
            Tools.Clear();
            foreach (var tool in tools)
                Tools.Add(tool);
        }

        public void NavigateToTool(Tool tool)
        {
            _navigation.Navigate(typeof(ToolDetailPage), tool);
        }
    }
}
