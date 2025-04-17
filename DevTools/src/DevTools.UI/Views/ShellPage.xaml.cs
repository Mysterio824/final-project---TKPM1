using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DevTools.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using DevTools.UI.Utils;
using System.Linq;

namespace DevTools.UI.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            AppServices.NavigationService.SetFrame(ContentFrame);
            AppServices.NavigationService.Navigate(typeof(DashboardPage));
            UpdateUIForAuthState();

            HeaderText.Text = "Dashboard";
            NavList.SelectedIndex = 0;
            NavList.SelectionChanged += NavList_SelectionChanged;
            AppServices.AuthService.OnLoginStatusChanged += UpdateUIForAuthState;
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem == null)
                return;

            var selectedItem = NavList.SelectedItem as ListViewItem;
            string tag = selectedItem.Tag.ToString();

            // Update header text
            HeaderText.Text = (selectedItem.Content as StackPanel)
                .Children
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text ?? "Dashboard";

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