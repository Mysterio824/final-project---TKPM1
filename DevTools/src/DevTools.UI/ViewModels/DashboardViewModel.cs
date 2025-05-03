using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using DevTools.UI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DevTools.UI.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ToolService _toolService;
        private readonly ToolGroupService _toolGroupService;
        private readonly AccountService _accountService;
        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;

        private string _searchQuery;
        private ObservableCollection<Tool> _allTools;
        private ObservableCollection<ToolGroup> _toolGroups;
        private Tool _selectedTool;
        private User _currentUser;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isSidePanelExpanded = true;
        private bool _showFavoritesOnly;
        private Tool _activeToolContent;
        private bool _isToolContentVisible;
        private bool _isToolDetailMode;
        private bool _showHeader;
        private ObservableCollection<Tool> _filteredTools;
        private string _sortBy = "Name";
        private string _filterGroup = "All";
        private ObservableCollection<string> _filterGroups;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }
        public bool ShowHeader
        {
            get => _showHeader;
            set => SetProperty(ref _showHeader, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        FilterAndSortTools();
                    }
                }
            }
        }

        public ObservableCollection<Tool> AllTools
        {
            get => _allTools;
            set => SetProperty(ref _allTools, value);
        }

        public ObservableCollection<ToolGroup> ToolGroups
        {
            get => _toolGroups;
            set => SetProperty(ref _toolGroups, value);
        }

        public Tool SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public bool IsToolDetailMode
        {
            get => _isToolDetailMode;
            set => SetProperty(ref _isToolDetailMode, value);
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsSidePanelExpanded
        {
            get => _isSidePanelExpanded;
            set => SetProperty(ref _isSidePanelExpanded, value);
        }

        public bool ShowFavoritesOnly
        {
            get => _showFavoritesOnly;
            set
            {
                if (SetProperty(ref _showFavoritesOnly, value))
                {
                    FilterAndSortTools();
                }
            }
        }

        public Tool ActiveToolContent
        {
            get => _activeToolContent;
            set => SetProperty(ref _activeToolContent, value);
        }

        public bool IsToolContentVisible
        {
            get => _isToolContentVisible;
            set => SetProperty(ref _isToolContentVisible, value);
        }

        public ObservableCollection<Tool> FilteredTools
        {
            get => _filteredTools;
            set => SetProperty(ref _filteredTools, value);
        }

        public string SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                {
                    FilterAndSortTools();
                }
            }
        }

        public string FilterGroup
        {
            get => _filterGroup;
            set
            {
                if (SetProperty(ref _filterGroup, value))
                {
                    FilterAndSortTools();
                }
            }
        }

        public ObservableCollection<string> FilterGroups
        {
            get => _filterGroups;
            set => SetProperty(ref _filterGroups, value);
        }

        public bool IsAuthenticated => CurrentUser != null;
        public bool IsPremium => CurrentUser?.IsPremium ?? false;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
        public ICommand LoadToolGroupsWithToolsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddToFavoritesCommand { get; }
        public ICommand RemoveFromFavoritesCommand { get; }
        public ICommand UploadToolCommand { get; }
        public ICommand RequestPremiumCommand { get; }
        public ICommand RevokePremiumCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ToggleFavoritesCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand CloseToolContentCommand { get; }

        public Action<string> ShowMessage { get; set; }
        public Action ShowPremiumRequired { get; set; }
        public Action ShowToolUnavailable { get; set; }

        public DashboardViewModel(
            ToolService toolService,
            ToolGroupService toolGroupService,
            AccountService accountService,
            AuthService authService,
            INavigationService navigationService)
        {
            _toolService = toolService ?? throw new ArgumentNullException(nameof(toolService));
            _toolGroupService = toolGroupService ?? throw new ArgumentNullException(nameof(toolGroupService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            var app = Application.Current as App;
            CurrentUser = app.CurrentUser;

            AllTools = new ObservableCollection<Tool>();
            FilteredTools = new ObservableCollection<Tool>();
            ToolGroups = new ObservableCollection<ToolGroup>();
            FilterGroups = new ObservableCollection<string> { "All" };

            LoadToolGroupsWithToolsCommand = new AsyncCommand(LoadToolGroupsWithToolsAsync);
            SearchCommand = new AsyncCommand(ExecuteSearchAsync);
            AddToFavoritesCommand = new AsyncCommand<Tool>(AddToFavoritesAsync, CanModifyFavorites);
            RemoveFromFavoritesCommand = new AsyncCommand<Tool>(RemoveFromFavoritesAsync, CanModifyFavorites);
            UploadToolCommand = new AsyncCommand<Tool>(UploadToolAsync, CanUploadTool);
            RequestPremiumCommand = new AsyncCommand(RequestPremiumAsync, () => IsAuthenticated && !IsPremium);
            RevokePremiumCommand = new AsyncCommand(RevokePremiumAsync, () => IsAuthenticated && IsPremium);
            LogoutCommand = new AsyncCommand(LogoutAsync, () => IsAuthenticated);
            ToggleFavoritesCommand = new RelayCommand(ToggleFavorites, () => IsAuthenticated);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            CloseToolContentCommand = new RelayCommand(CloseToolContent);
        }

        public async Task LoadToolGroupsWithToolsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var toolGroups = await _toolGroupService.GetAllToolGroupsAsync();
                ToolGroups.Clear();
                AllTools.Clear();
                FilterGroups.Clear();
                FilterGroups.Add("All");

                foreach (var group in toolGroups)
                {
                    if (!FilterGroups.Contains(group.Name))
                    {
                        FilterGroups.Add(group.Name);
                    }

                    var tools = await _toolGroupService.GetToolsByGroupIdAsync(group.Id);
                    group.Tools.Clear();

                    foreach (var tool in tools)
                    {
                        tool.GroupName = group.Name;
                        if (string.IsNullOrEmpty(tool.SymbolGlyph))
                        {
                            tool.SymbolGlyph = "\u002B";
                        }

                        group.Tools.Add(tool);
                        AllTools.Add(tool);
                    }

                    ToolGroups.Add(group);
                }

                FilterAndSortTools();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading tool groups with tools: {ex.Message}");
                ErrorMessage = "Failed to load tool categories. Please try again later.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanModifyFavorites(Tool tool) => IsAuthenticated && tool != null;

        private bool CanUploadTool(Tool tool)
        {
            if (tool == null) return false;
            if (!tool.IsEnabled) return false;
            if (tool.IsPremium) return IsPremium;
            return true;
        }

        private async Task ExecuteSearchAsync()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    FilterAndSortTools();
                    return;
                }

                var results = await _toolService.SearchToolsAsync(SearchQuery);

                AllTools.Clear();
                foreach (var tool in results)
                {
                    var group = ToolGroups.FirstOrDefault(g => g.Tools.Any(t => t.Id == tool.Id));
                    if (group != null)
                    {
                        tool.GroupName = group.Name;
                    }

                    AllTools.Add(tool);
                }

                FilterAndSortTools();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching tools: {ex.Message}");
                ErrorMessage = "Search failed. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddToFavoritesAsync(Tool tool)
        {
            if (!IsAuthenticated)
            {
                NavigateToLogin();
                return;
            }

            try
            {
                IsLoading = true;
                var (succeeded, error) = await _accountService.AddToFavoritesAsync(tool.Id);
                if (succeeded)
                {
                    tool.IsFavorite = true;
                    UpdateToolCollections(tool);
                    if (ShowFavoritesOnly)
                    {
                        FilterAndSortTools();
                    }
                }
                else
                {
                    ErrorMessage = $"Failed to add tool to favorites: {error}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to favorites: {ex.Message}");
                ErrorMessage = "Failed to add tool to favorites.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemoveFromFavoritesAsync(Tool tool)
        {
            if (!IsAuthenticated)
            {
                NavigateToLogin();
                return;
            }

            try
            {
                IsLoading = true;
                var (succeeded, error) = await _accountService.RemoveFromFavoritesAsync(tool.Id);
                if (succeeded)
                {
                    tool.IsFavorite = false;
                    UpdateToolCollections(tool);
                    if (ShowFavoritesOnly)
                    {
                        FilterAndSortTools();
                    }
                }
                else
                {
                    ErrorMessage = $"Failed to remove tool from favorites: {error}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing from favorites: {ex.Message}");
                ErrorMessage = "Failed to remove tool from favorites.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateToolCollections(Tool tool)
        {
            var allToolItem = AllTools.FirstOrDefault(t => t.Id == tool.Id);
            if (allToolItem != null) allToolItem.IsFavorite = tool.IsFavorite;

            var filteredToolItem = FilteredTools.FirstOrDefault(t => t.Id == tool.Id);
            if (filteredToolItem != null) filteredToolItem.IsFavorite = tool.IsFavorite;

            if (ActiveToolContent?.Id == tool.Id)
            {
                ActiveToolContent.IsFavorite = tool.IsFavorite;
            }
        }

        private async Task UploadToolAsync(Tool tool)
        {
            if (tool.IsPremium && !IsPremium)
            {
                ShowPremiumRequired();
                return;
            }

            try
            {
                IsLoading = true;
                var fullTool = await _toolService.GetToolByIdAsync(tool.Id);
                if (fullTool?.FileData != null)
                {
                    SaveToolToLocalStorage(fullTool);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading tool: {ex.Message}");
                ErrorMessage = "Failed to download tool. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SaveToolToLocalStorage(Tool tool)
        {
            try
            {
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{tool.Id}_{tool.Name}.dll");
                using var filestream = new FileStream(dllPath, FileMode.Create, FileAccess.Write, FileShare.None);
                filestream.Write(tool.FileData, 0, tool.FileData.Length);
                Debug.WriteLine($"Tool {tool.Name} saved to local storage at {dllPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving tool to local storage: {ex.Message}");
                ErrorMessage = "Failed to save tool locally.";
            }
        }

        private async Task RequestPremiumAsync()
        {
            if (!IsAuthenticated)
            {
                NavigateToLogin();
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var (success, error) = await _accountService.RequestPremiumUpgradeAsync();
                if (success)
                {
                    ErrorMessage = "Premium request submitted successfully! Please logout and login again to get the full access!";
                }
                else
                {
                    ErrorMessage = $"Failed to submit premium request: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RevokePremiumAsync()
        {
            if (!IsAuthenticated || !IsPremium)
            {
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var (success, error) = await _accountService.RevokePremiumAsync();

                if (success)
                {
                    ErrorMessage = "Premium status revoked successfully!";
                    CurrentUser = new User
                    {
                        Id = CurrentUser.Id,
                        Name = CurrentUser.Name,
                        Email = CurrentUser.Email,
                        Token = CurrentUser.Token,
                        RefreshToken = CurrentUser.RefreshToken,
                        Role = 1 // Set to regular user
                    };

                    await LoadToolGroupsWithToolsAsync();
                }
                else
                {
                    ErrorMessage = $"Failed to revoke premium status: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (CurrentUser?.Token != null)
                {
                    await _authService.LogoutAsync(CurrentUser.Token);
                }

                CurrentUser = null;
                var app = Application.Current as App;
                app.CurrentUser = null;
                ShowFavoritesOnly = false;

                if (IsToolContentVisible)
                {
                    IsToolContentVisible = false;
                    ActiveToolContent = null;
                }

                await LoadToolGroupsWithToolsAsync();

                // Navigate to LoginPage instead of invoking onLogout
                _navigationService.NavigateTo(typeof(LoginPage));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error during logout: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ToggleSidePanel()
        {
            IsSidePanelExpanded = !IsSidePanelExpanded;
        }

        private void FilterAndSortTools()
        {
            var query = AllTools.AsEnumerable();

            if (ShowFavoritesOnly)
            {
                query = query.Where(t => t.IsFavorite);
            }

            if (!string.IsNullOrEmpty(FilterGroup) && FilterGroup != "All")
            {
                query = query.Where(t => t.GroupName == FilterGroup);
            }

            switch (SortBy)
            {
                case "Name":
                    query = query.OrderBy(t => t.Name);
                    break;
                case "GroupName":
                    query = query.OrderBy(t => t.GroupName).ThenBy(t => t.Name);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string searchLower = SearchQuery.ToLowerInvariant();
                query = query.Where(t =>
                    t.Name.ToLowerInvariant().Contains(searchLower) ||
                    (t.Description != null && t.Description.ToLowerInvariant().Contains(searchLower)));
            }

            FilteredTools.Clear();
            foreach (var tool in query)
            {
                FilteredTools.Add(tool);
            }
        }

        private void ToggleFavorites()
        {
            if (!IsAuthenticated)
            {
                NavigateToLogin();
                return;
            }

            ShowFavoritesOnly = !ShowFavoritesOnly;
        }

        public void NavigateToLogin()
        {
            _navigationService.NavigateTo(typeof(LoginPage));
        }

        public void NavigateToRegister()
        {
            _navigationService.NavigateTo(typeof(RegisterPage));
        }

        private void CloseToolContent()
        {
            IsToolContentVisible = false;
            ActiveToolContent = null;
        }
    }
}
