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
using DevTools.UI.Models;
using DevTools.UI.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; private set; }
        private readonly Action<User> onLogout;

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel = e.Parameter as DashboardViewModel;

            ViewModel.LoadToolsCommand.Execute(null);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(FavoritesPage), ViewModel);
        }

        private void ToolsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tool = e.ClickedItem as Tool;
            if (tool != null)
            {
                if (tool.IsPremium && !ViewModel.IsPremium)
                {
                    ShowPremiumUpgradeDialog();
                }
                else if (!tool.IsEnabled)
                {
                    ShowToolUnavailableMessage();
                }
                else
                {
                    Frame.Navigate(typeof(ToolDetailPage), tool);
                }
            }
        }

        private async void ShowPremiumUpgradeDialog()
        {
            ContentDialog premiumDialog = new ContentDialog
            {
                Title = "Premium Feature",
                Content = "This tool is available only for premium users. Would you like to upgrade to premium?",
                PrimaryButtonText = "Upgrade",
                CloseButtonText = "Not Now",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await premiumDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.RequestPremiumCommand.Execute(null);
            }
        }

        private async void ShowToolUnavailableMessage()
        {
            ContentDialog unavailableDialog = new ContentDialog
            {
                Title = "Tool Unavailable",
                Content = "This tool is currently unavailable. Please try again later.",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };

            await unavailableDialog.ShowAsync();
        }
    }
}