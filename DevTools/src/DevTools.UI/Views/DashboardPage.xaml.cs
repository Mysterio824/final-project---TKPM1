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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Windows.UI;
using System.Security.AccessControl;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel, INavigationService navigationService)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;

            ViewModel.LoadToolGroupsWithToolsCommand.Execute(null);

            // Setup ViewModel action handlers
            ViewModel.ShowMessage = ShowMessage;
            ViewModel.ShowPremiumRequired = async () => ShowPremiumUpgradeDialog();
            ViewModel.ShowToolUnavailable = () => ShowToolUnavailableMessage();

            // Initialize UI
            InitializeUI();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void InitializeUI()
        {
            // Setup theme toggle based on current theme
            if (Window.Current != null && Window.Current.Content is FrameworkElement rootElement)
            {
                ThemeToggle.IsOn = rootElement.ActualTheme == ElementTheme.Dark;
                ThemeIcon.Glyph = ThemeToggle.IsOn == true ? "\uE708" : "\uE793";
            }

            // Update side panel state
            UpdateSidePanelDisplay();

            // Setup UI based on authentication state
            UpdateAuthenticationUI();

            // Setup favorites button state
            UpdateFavoritesButtonStyle();

            // Register for property changes
            ViewModel.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ViewModel.CurrentUser))
                {
                    UpdateAuthenticationUI();
                }
                else if (e.PropertyName == nameof(ViewModel.ShowFavoritesOnly))
                {
                    UpdateFavoritesButtonStyle();
                }
                else if (e.PropertyName == nameof(ViewModel.IsSidePanelExpanded))
                {
                    UpdateSidePanelDisplay();
                }
            };
        }
        private void UpdateAuthenticationUI()
        {
            // Update UI elements based on authentication state
            if (ViewModel.IsAuthenticated)
            {
                // Show logged-in user UI
                AnonymousUserPanel.Visibility = Visibility.Collapsed;
                AuthenticatedUserPanel.Visibility = Visibility.Visible;
                UserNameTextBlock.Text = ViewModel.CurrentUser.Name;

                // Show/hide premium UI
                PremiumPanel.Visibility = ViewModel.IsPremium ? Visibility.Visible : Visibility.Collapsed;
                RegularUserPanel.Visibility = ViewModel.IsPremium ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                // Show anonymous user UI
                AnonymousUserPanel.Visibility = Visibility.Visible;
                AuthenticatedUserPanel.Visibility = Visibility.Collapsed;
                PremiumPanel.Visibility = Visibility.Collapsed;
                RegularUserPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToLoginCommand.Execute(null);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToRegisterCommand.Execute(null);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LogoutCommand.Execute(null);
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsAuthenticated)
            {
                ShowLoginRequiredDialog();
                return;
            }

            ViewModel.ToggleFavoritesCommand.Execute(null);
        }

        private void UpdateFavoritesButtonStyle()
        {
            // Style the favorites button based on state
            if (ViewModel.ShowFavoritesOnly)
            {
                FavoritesButton.Background = new SolidColorBrush(Colors.LightGray);
                FavoriteIcon.Foreground = new SolidColorBrush(Colors.Gold);
            }
            else
            {
                FavoritesButton.Background = new SolidColorBrush(Colors.Transparent);
                FavoriteIcon.Foreground = new SolidColorBrush((Color)Resources["SystemBaseHighColor"]);
            }
        }

        private void ToolsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Tool tool)
            {
                if (!tool.IsEnabled)
                {
                    ShowToolUnavailableMessage();
                    return;
                }
                if (tool.IsPremium && !ViewModel.IsPremium)
                {
                    ShowPremiumUpgradeDialog();
                    return;
                }
                // Set the active tool
                ViewModel.ActiveToolContent = tool;

                // Set tool detail mode
                ViewModel.IsToolDetailMode = true;

                (Application.Current as App).SelectedTool = tool;
                var page = (Application.Current as App).serviceProvider.GetService(typeof(ToolDetailPage)) as Page;
                ContentFrame.Content = page;
            }
        }

        private async void ShowPremiumUpgradeDialog()
        {
            ContentDialog premiumDialog = new ContentDialog
            {
                Title = "Premium Feature",
                Content = "This tool is available only for premium users. Would you like to upgrade to premium?",
                PrimaryButtonText = ViewModel.IsAuthenticated ? "Upgrade" : "Login",
                CloseButtonText = "Not Now",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await premiumDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (ViewModel.IsAuthenticated)
                {
                    ViewModel.RequestPremiumCommand.Execute(null);
                }
                else
                {
                    Frame.Navigate(typeof(LoginPage));
                }
            }
        }

        private async void ShowToolUnavailableMessage()
        {
            ContentDialog unavailableDialog = new ContentDialog
            {
                Title = "Tool Unavailable",
                Content = "This tool is currently unavailable. Please try again later.",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await unavailableDialog.ShowAsync();
        }

        private async void ShowLoginRequiredDialog()
        {
            ContentDialog loginDialog = new ContentDialog
            {
                Title = "Login Required",
                Content = "You need to log in to use this feature.",
                PrimaryButtonText = "Login",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await loginDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.NavigateToLoginCommand.Execute(null);
            }
        }

        private void ToggleSidePanel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ToggleSidePanel();
        }

        private void UpdateSidePanelDisplay()
        {
            if (ViewModel.IsSidePanelExpanded)
            {
                SidePanel.Width = (double)Resources["SidePanelExpandedWidth"];
                TogglePanelIcon.Glyph = "\uE76B";  // Collapse icon

                // Show the text elements in side panel
                SidePanelHeaderText.Visibility = Visibility.Visible;
            }
            else
            {
                SidePanel.Width = (double)Resources["SidePanelCollapsedWidth"];
                TogglePanelIcon.Glyph = "\uE76C";  // Expand icon

                // Hide the text elements in side panel
                SidePanelHeaderText.Visibility = Visibility.Collapsed;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear search and reload all tools
            ViewModel.SearchQuery = string.Empty;
            ViewModel.ShowFavoritesOnly = false;
            ViewModel.IsToolContentVisible = false;
            ViewModel.IsToolDetailMode = false;
            ContentFrame.Content = null;
            // Continue from where the previous code ended
            ViewModel.LoadToolGroupsWithToolsCommand.Execute(null);
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (Window.Current != null && Window.Current.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ThemeToggle.IsOn == true
                    ? ElementTheme.Dark
                    : ElementTheme.Light;

                ThemeIcon.Glyph = ThemeToggle.IsOn == true
                    ? "\uE708"  // Moon icon
                    : "\uE793"; // Sun icon
            }
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            // Language selection functionality would go here
            // For example, show a flyout with language options
            LanguageFlyout.ShowAt(sender as FrameworkElement);
        }

        private void LanguageOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                string language = item.Tag.ToString();
                // Set the application language
                // This would involve more complex localization logic
                LanguageButton.Content = item.Text;

                // Close the flyout
                LanguageFlyout.Hide();
            }
        }

        private void GithubButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowserTo("https://github.com/Mysterio824/final-project---TKPM1");
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            // Show about dialog
            ContentDialog aboutDialog = new ContentDialog
            {
                Title = "About DevTools",
                Content = "DevTools is a comprehensive suite of utilities for developers and professionals. A project for Software Design - 22CLC02 - HCMUS. Author: 22127254 - 22127268." +
                         "Version 1.0.0\n\n" +
                         "© 2025 HCMUS. All rights reserved.",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await aboutDialog.ShowAsync();
        }

        private async void ShowMessage(string message)
        {
            ContentDialog messageDialog = new ContentDialog
            {
                Title = "Information",
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await messageDialog.ShowAsync();
        }

        private async void OpenBrowserTo(string url)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update search query in ViewModel
            ViewModel.SearchQuery = SearchBox.Text;

            // Show or hide suggestion box based on text existence
            SearchSuggestionBox.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Only update search if not typing
            _searchDebounceTimer?.Stop();
            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDebounceTimer.Tick += (s, args) =>
            {
                _searchDebounceTimer.Stop();
                // Execute search
                ViewModel.SearchCommand.Execute(null);
            };
            _searchDebounceTimer.Start();

            // Update suggestions
            UpdateSearchSuggestions();
        }

        private DispatcherTimer _searchDebounceTimer;

        private void UpdateSearchSuggestions()
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchSuggestionBox.Visibility = Visibility.Collapsed;
                return;
            }

            string searchText = SearchBox.Text.ToLowerInvariant();
            var suggestions = ViewModel.AllTools
                .Where(t => t.Name.ToLowerInvariant().Contains(searchText) ||
                            (t.Description != null && t.Description.ToLowerInvariant().Contains(searchText)))
                .Take(5)
                .ToList();

            SearchSuggestionsList.ItemsSource = suggestions;
            SearchSuggestionBox.Visibility = suggestions.Any()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SearchSuggestionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Tool tool)
            {
                if (!tool.IsEnabled)
                {
                    ShowToolUnavailableMessage();
                    return;
                }
                if (tool.IsPremium && !ViewModel.IsPremium)
                {
                    ShowPremiumUpgradeDialog();
                    return;
                }
       
                // Clear search and select the tool
                SearchBox.Text = "";
                SearchSuggestionBox.Visibility = Visibility.Collapsed;
                ViewModel.ActiveToolContent = tool;

                // Set tool detail mode
                ViewModel.IsToolDetailMode = true;

                // Navigate to tool detail page with the tool ID
                (Application.Current as App).SelectedTool = tool;
                var page = (Application.Current as App).serviceProvider.GetService(typeof(ToolDetailPage)) as Page;
                ContentFrame.Content = page;
            }
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Execute search
                ViewModel.SearchCommand.Execute(null);
                SearchSuggestionBox.Visibility = Visibility.Collapsed;
                SearchBox.Focus(FocusState.Programmatic); // Keep focus in the search box
            }
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && SortByComboBox.SelectedItem is string sortBy)
            {
                ViewModel.SortBy = sortBy;
            }
        }

        private void FilterGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && FilterGroupComboBox.SelectedItem is string filterGroup)
            {
                ViewModel.FilterGroup = filterGroup;
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tool tool)
            {
                if (!ViewModel.IsAuthenticated)
                {
                    ShowLoginRequiredDialog();
                    return;
                }
                Debug.WriteLine(tool.IsFavorite);
                if (tool.IsFavorite)
                {
                    ViewModel.RemoveFromFavoritesCommand.Execute(tool);
                }
                else
                {
                    ViewModel.AddToFavoritesCommand.Execute(tool);
                }
                ViewModel.LoadToolGroupsWithToolsCommand.Execute(null);
            }
        }

        private void BackToHomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Close tool content and return to grid view
            ViewModel.CloseToolContentCommand.Execute(null);
        }

        private void PremiumRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsAuthenticated)
            {
                ShowLoginRequiredDialog();
                return;
            }

            ViewModel.RequestPremiumCommand.Execute(null);
        }

        private void GroupHeader_Click(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ShowHeader = !ViewModel.ShowHeader;
            // Toggle group expansion when header is clicked
            if (sender is FrameworkElement element && element.DataContext is ToolGroup group)
            {
                group.IsExpanded = !group.IsExpanded;
            }
        }

        private void ToolItem_Click(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Tool tool)
            {
                if (!tool.IsEnabled)
                {
                    ShowToolUnavailableMessage();
                    return;
                }
                if (tool.IsPremium && !ViewModel.IsPremium)
                {
                    ShowPremiumUpgradeDialog();
                    return;
                }
                
                // Set the active tool
                ViewModel.ActiveToolContent = tool;

                // Set tool detail mode
                ViewModel.IsToolDetailMode = true;

                // Navigate to tool detail page with the tool ID
                (Application.Current as App).SelectedTool = tool;
                var page = (Application.Current as App).serviceProvider.GetService(typeof(ToolDetailPage)) as Page;
                ContentFrame.Content = page;
            }
        }
    }
}