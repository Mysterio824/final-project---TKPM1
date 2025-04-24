using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using DevTools.UI.Views;
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
        private readonly Action _onLogout;

        private string _searchQuery;
        private ObservableCollection<Tool> _tools;
        private ObservableCollection<ToolGroup> _toolGroup;
        private Tool _selectedTool;
        private User _currentUser;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isSidePanelExpanded = true;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                    SearchCommand.Execute(null);
            }
        }
        public ObservableCollection<Tool> Tools
        {
            get => _tools;
            set => SetProperty(ref _tools, value);
        }
        public ObservableCollection<ToolGroup> ToolGroups
        {
            get => _toolGroup;
            set => SetProperty(ref _toolGroup, value);
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

        public bool IsAuthenticated => _currentUser != null;
        public bool IsPremium => _currentUser?.IsPremium ?? false;
        public bool IsAdmin => _currentUser?.IsAdmin ?? false;

        public ICommand LoadToolGroupsWithToolsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddToFavoritesCommand { get; }
        public ICommand RemoveFromFavoritesCommand { get; }
        public ICommand UploadToolCommand { get; }
        public ICommand RequestPremiumCommand { get; }
        public ICommand RevokePremiumCommand { get; }
        public ICommand LogoutCommand { get; }

        public DashboardViewModel(ToolService toolService, ToolGroupService toolGroupService, AccountService accountService, AuthService authService, Action onLogout, User currentUser = null)
        {
            _toolService = toolService;
            _toolGroupService = toolGroupService;
            _accountService = accountService;
            _authService = authService;
            CurrentUser = currentUser;
            _onLogout = onLogout;
            Tools = new ObservableCollection<Tool>();
            ToolGroups = new ObservableCollection<ToolGroup>();

            LoadToolGroupsWithToolsCommand = new AsyncCommand(LoadToolGroupsWithToolsAsync);
            SearchCommand = new AsyncCommand(ExecuteSearchAsync);
            AddToFavoritesCommand = new AsyncCommand<Tool>(AddToFavoritesAsync, CanModifyFavorites);
            RemoveFromFavoritesCommand = new AsyncCommand<Tool>(RemoveFromFavoritesAsync, CanModifyFavorites);
            UploadToolCommand = new AsyncCommand<Tool>(UploadToolAsync, CanUploadTool);
            RequestPremiumCommand = new AsyncCommand(RequestPremiumAsync, () => !IsPremium);
            RevokePremiumCommand = new AsyncCommand(RevokePremiumAsync, () => IsPremium);
            LogoutCommand = new AsyncCommand(LogoutAsync);

            if (IsAuthenticated)
            {
                _toolService.SetAuthToken(_currentUser.Token);
                _accountService.SetAuthToken(_currentUser.Token);
            }
        }
        public async Task LoadToolGroupsWithToolsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var toolGroups = await _toolGroupService.GetAllToolGroupsAsync();

                ToolGroups.Clear();

                foreach (var group in toolGroups)
                {
                    group.IsExpanded = group.IsExpanded;

                    var tools = await _toolGroupService.GetToolsByGroupIdAsync(group.Id);

                    group.Tools.Clear();
                    foreach (var tool in tools)
                    {
                        tool.GroupName = group.Name;
                        if (string.IsNullOrEmpty(tool.SymbolGlyph))
                        {
                            tool.SymbolGlyph = "\uE774";
                        }

                        group.Tools.Add(tool);
                    }

                    ToolGroups.Add(group);
                }
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
                    //await LoadToolsAsync();
                    return;
                }

                var results = await _toolService.SearchToolsAsync(SearchQuery);
                Tools.Clear();
                foreach (var tool in results)
                {
                    Tools.Add(tool);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching tools: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddToFavoritesAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                var success = await _accountService.AddToFavoritesAsync(tool.Id);
                if (success)
                {
                    tool.IsFavorite = true;
                    OnPropertyChanged(nameof(Tools));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to favorites: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemoveFromFavoritesAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                var success = await _accountService.RemoveFromFavoritesAsync(tool.Id);
                if (success)
                {
                    tool.IsFavorite = false;
                    OnPropertyChanged(nameof(Tools));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing from favorites: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UploadToolAsync(Tool tool)
        {
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
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void SaveToolToLocalStorage(Tool tool)
        {
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{tool.Id}_{tool.Name}.dll");
            using var filestream = new FileStream(dllPath, FileMode.Create, FileAccess.Write, FileShare.None);
            filestream.Write(tool.FileData, 0, tool.FileData.Length);
            Debug.WriteLine($"Tool {tool.Name} saved to local storage");
        }

        private async Task RequestPremiumAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                //var success = await _accountService.RequestPremiumUpgradeAsync();
                //stimulation
                var success = true;
                if (success)
                {
                    ErrorMessage = "Premium request submitted successfully!";
                }
                else
                {
                    ErrorMessage = "Failed to submit premium request.";
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
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _accountService.RevokePremiumAsync();
                if (success)
                {
                    // Update local user state
                    ErrorMessage = "Premium status revoked successfully!";
                }
                else
                {
                    ErrorMessage = "Failed to revoke premium status.";
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

                await _authService.LogoutAsync(CurrentUser.Token);
                _onLogout();
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
            OnPropertyChanged(nameof(IsSidePanelExpanded));
        }
    }
}
