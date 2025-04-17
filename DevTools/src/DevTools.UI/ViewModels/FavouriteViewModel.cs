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
    public class FavouriteViewModel : ObservableObject
    {
        private readonly ToolService _toolService;
        private readonly INavigationService _navigation;
        public ObservableCollection<Tool> FavouriteTools { get; } = new();

        public FavouriteViewModel(ToolService toolService, INavigationService navigation)
        {
            _toolService = toolService;
            _navigation = navigation;

            if (JwtTokenManager.IsLoggedIn)
                LoadFavouritesAsync();
        }

        private async void LoadFavouritesAsync()
        {
            var tools = await _toolService.GetFavouritesAsync();
            FavouriteTools.Clear();
            foreach (var tool in tools)
                FavouriteTools.Add(tool);
        }

        public void OpenTool(Tool tool)
        {
            _navigation.Navigate(typeof(ToolDetailPage), tool);
        }
    }
}
