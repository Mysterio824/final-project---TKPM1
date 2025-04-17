using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using DevTools.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using DevTools.UI.Models;
using DevTools.UI.Utils;
using DevTools.UI.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ToolDetailPage : Page
    {
        private ToolDetailViewModel ViewModel => DataContext as ToolDetailViewModel;

        public ToolDetailPage()
        {
            this.InitializeComponent();
            DataContext = AppServices.ToolDetailViewModel;
            this.Loaded += ToolDetailPage_Loaded;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Tool tool)
            {
                LoadingRing.IsActive = true;
                await ViewModel.LoadToolAsync(tool);
                LoadingRing.IsActive = false;
            }
        }

        private void ToolDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Add binding for login state visibility
            var binding = new Microsoft.UI.Xaml.Data.Binding();
            binding.Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay;
            binding.Source = JwtTokenManager.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            // Manually set the IsUserLoggedIn property on ViewModel
            var property = typeof(ToolDetailViewModel).GetProperty("IsUserLoggedIn");
            if (property != null)
            {
                property.SetValue(ViewModel, JwtTokenManager.IsLoggedIn);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            AppServices.NavigationService.Navigate(typeof(LoginPage));
        }
    }
}