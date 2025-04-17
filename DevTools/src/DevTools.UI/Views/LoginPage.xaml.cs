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
using DevTools.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using DevTools.UI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private LoginViewModel ViewModel => DataContext as LoginViewModel;
        private string _returnDestination;

        public LoginPage()
        {
            this.InitializeComponent();
            DataContext = AppServices.LoginViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string destination)
            {
                _returnDestination = destination;
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Visibility = Visibility.Collapsed;

            bool success = await ViewModel.LoginAsync();

            if (success)
            {
                switch (_returnDestination)
                {
                    case "favorites":
                        AppServices.NavigationService.Navigate(typeof(FavouritePage));
                        break;
                    case "premium":
                        AppServices.NavigationService.Navigate(typeof(UpgradePage));
                        break;
                    default:
                        AppServices.NavigationService.Navigate(typeof(DashboardPage));
                        break;
                }
            }
            else
            {
                // Show error message
                ErrorMessage.Text = "Login failed. Please check your credentials and try again.";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
