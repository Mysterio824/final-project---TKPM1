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
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Animation;

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

            ViewModel.LoadToolGroupsWithToolsCommand.Execute(null);
            if (Window.Current != null && Window.Current.Content is FrameworkElement rootElement)
            {
                ThemeToggle.IsChecked = rootElement.ActualTheme == ElementTheme.Dark;
                ThemeIcon.Glyph = ThemeToggle.IsChecked == true ? "\uE708" : "\uE793";
            }
            else
            {
                // Handle potential issues with Window.Current being null.
                Debug.WriteLine("Window.Current is null. Ensure that the window is properly initialized.");
            }
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
        private void ToggleSidePanel_Click(object sender, RoutedEventArgs e)
        {
            string newGlyph;

            if (ViewModel.IsSidePanelExpanded)
            {
                SidePanel.Width = (double)Resources["SidePanelCollapsedWidth"];
                newGlyph = "\uE76C";
            }
            else
            {
                SidePanel.Width = (double)Resources["SidePanelExpandedWidth"];
                newGlyph = "\uE76B";
            }

            TogglePanelIcon.Glyph = newGlyph;

            ViewModel.ToggleSidePanel();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear search and reload all tools
            ViewModel.SearchQuery = string.Empty;
            ViewModel.LoadToolGroupsWithToolsCommand.Execute(null);
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            //if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            //{
            //    // Debounce search or provide suggestions as user types
            //    if (string.IsNullOrWhiteSpace(sender.Text))
            //    {
            //        sender.ItemsSource = null;
            //    }
            //    else if (sender.Text.Length >= 2)
            //    {
            //        // Async search for suggestions
            //        SearchForSuggestions(sender.Text);
            //    }
            //}
        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Tool selectedTool)
            {
                ViewModel.SelectedTool = selectedTool;
                sender.Text = selectedTool.Name;
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is Tool selectedTool)
            {
                // Navigate to the selected tool
                Frame.Navigate(typeof(ToolDetailPage), selectedTool);
            }
            else
            {
                // Execute search with current query
                ViewModel.SearchCommand.Execute(null);
            }
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton.IsChecked == true)
            {
                ThemeIcon.Glyph = "\uE708"; // Moon icon
                                            // Apply dark theme
                if (Window.Current != null && Window.Current.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = ElementTheme.Dark;
                }
            }
            else
            {
                ThemeIcon.Glyph = "\uE793"; // Sun icon
                                            // Apply light theme
                if (Window.Current != null && Window.Current.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = ElementTheme.Light;
                }
            }
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle language change
            var combo = sender as ComboBox;
            if (combo != null)
            {
                string selectedLanguage = (combo.SelectedItem as ComboBoxItem)?.Content?.ToString();
                // Apply language change logic
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowInfoDialog();
        }

        private async void ShowInfoDialog()
        {
            ContentDialog infoDialog = new ContentDialog
            {
                Title = "About IT Tools",
                Content = "IT Tools is a comprehensive collection of utilities for IT professionals. Version 1.0.0",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };

            await infoDialog.ShowAsync();
        }

        private void SidebarToolItem_Click(object sender, ItemClickEventArgs e)
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
    }
}