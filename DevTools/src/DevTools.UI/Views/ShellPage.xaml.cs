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
using DevTools.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            AppServices.NavigationService.SetFrame(ContentFrame);
            AppServices.NavigationService.Navigate(typeof(DashboardPage));
            UpdateUIForAuthState();

            // Set header text
            HeaderText.Text = "Dashboard";
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null)
                return;

            string tag = args.SelectedItemContainer.Tag.ToString();
            HeaderText.Text = args.SelectedItemContainer.Content.ToString();

            switch (tag)
            {
                case "dashboard":
                    AppServices.NavigationService.Navigate(typeof(DashboardPage));
                    break;
                case "favorites":
                    if (JwtTokenManager.IsLoggedIn)
                        AppServices.NavigationService.Navigate(typeof(FavouritePage));
                    else
                        AppServices.NavigationService.Navigate(typeof(LoginPage), "favorites");
                    break;
                case "admin":
                    AppServices.NavigationService.Navigate(typeof(AdminDashboardPage));
                    break;
            }
        }

        private void UpdateUIForAuthState()
        {
            if (JwtTokenManager.IsLoggedIn)
            {
                LoginButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;

                if (JwtTokenManager.IsAdmin)
                    AdminItem.Visibility = Visibility.Visible;
                else
                    AdminItem.Visibility = Visibility.Collapsed;

                if (!JwtTokenManager.IsPremium)
                    UpgradeButton.Visibility = Visibility.Visible;
                else
                    UpgradeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                UpgradeButton.Visibility = Visibility.Collapsed;
                AdminItem.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.NavigationService.Navigate(typeof(LoginPage));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.AuthService.Logout();
            UpdateUIForAuthState();
            AppServices.NavigationService.Navigate(typeof(DashboardPage));
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.NavigationService.Navigate(typeof(UpgradePage));
        }
    }
}