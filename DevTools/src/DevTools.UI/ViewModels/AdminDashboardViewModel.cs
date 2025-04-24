using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DevTools.UI.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private readonly ToolService _toolService;
        private readonly ToolGroupService _toolGroupService;

        private bool _isLoading;
        private string _errorMessage;
        private ObservableCollection<Tool> _tools;
        private ObservableCollection<ToolGroup> _groups;
        private Tool _selectedTool;
        private ToolGroup _selectedGroup;

        private string _toolName;
        private string _toolDescription;
        private bool _isPremium;
        private bool _isEnabled;
        private int? _selectedGroupId;
        private string _groupName;
        private string _groupDescription;
        private IFormFile _toolFile;

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

        public ObservableCollection<Tool> Tools
        {
            get => _tools;
            set => SetProperty(ref _tools, value);
        }

        public ObservableCollection<ToolGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public Tool SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        public ToolGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value);
        }

        public string ToolName
        {
            get => _toolName;
            set
            {
                SetProperty(ref _toolName, value);
                (AddToolCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ToolDescription
        {
            get => _toolDescription;
            set => SetProperty(ref _toolDescription, value);
        }

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public string GroupDescription
        {
            get => _groupDescription;
            set => SetProperty(ref _groupDescription, value);
        }
        public bool IsPremium
        {
            get => _isPremium;
            set => SetProperty(ref _isPremium, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public int? SelectedGroupId
        {
            get => _selectedGroupId;
            set
            {
                SetProperty(ref _selectedGroupId, value);
                (AddToolCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public IFormFile ToolFile
        {
            get => _toolFile;
            set
            {
                SetProperty(ref _toolFile, value);
                (AddToolCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadToolsCommand { get; }
        public ICommand LoadGroupsCommand { get; }
        public ICommand AddToolCommand { get; }
        public ICommand UpdateToolCommand { get; }
        public ICommand DeleteToolCommand { get; }
        public ICommand EnableToolCommand { get; }
        public ICommand DisableToolCommand { get; }
        public ICommand SetPremiumCommand { get; }
        public ICommand SetFreeCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand UpdateGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }

        public AdminDashboardViewModel(ToolService toolService, ToolGroupService toolGroupService, User adminUser)
        {
            _toolService = toolService;
            _toolGroupService = toolGroupService;

            Tools = new ObservableCollection<Tool>();
            Groups = new ObservableCollection<ToolGroup>();

            LoadToolsCommand = new AsyncCommand(LoadToolsAsync);
            LoadGroupsCommand = new AsyncCommand(LoadGroupsAsync);
            AddToolCommand = new AsyncCommand(AddToolAsync, CanAddTool);
            UpdateToolCommand = new AsyncCommand(UpdateToolAsync, CanUpdateTool);
            DeleteToolCommand = new AsyncCommand<Tool>(DeleteToolAsync);
            EnableToolCommand = new AsyncCommand<Tool>(EnableToolAsync);
            DisableToolCommand = new AsyncCommand<Tool>(DisableToolAsync);
            SetPremiumCommand = new AsyncCommand<Tool>(SetToolPremiumAsync);
            SetFreeCommand = new AsyncCommand<Tool>(SetToolFreeAsync);
            AddGroupCommand = new AsyncCommand(AddGroupAsync);
            UpdateGroupCommand = new AsyncCommand(UpdateGroupAsync);
            DeleteGroupCommand = new AsyncCommand<ToolGroup>(DeleteGroupAsync);

            _toolService.SetAuthToken(adminUser.Token);
            _toolGroupService.SetAuthToken(adminUser.Token);

            IsEnabled = true;
        }

        private bool CanAddTool() =>
            !string.IsNullOrWhiteSpace(ToolName) &&
            SelectedGroupId > 0 &&
            ToolFile != null;

        private bool CanUpdateTool() =>
            SelectedTool != null &&
            !string.IsNullOrWhiteSpace(ToolName) &&
            SelectedGroupId > 0;

        public async Task LoadToolsAsync()
        {
            try
            {
                IsLoading = true;
                var tools = await _toolService.GetAllToolsAsync();
                Tools.Clear();
                foreach (var tool in tools)
                {
                    Tools.Add(tool);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading tools: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadGroupsAsync()
        {
            try
            {
                IsLoading = true;
                var groups = await _toolGroupService.GetAllToolGroupsAsync();
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading groups: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddToolAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var toolId = await _toolService.AddToolAsync(
                    ToolName,
                    ToolDescription,
                    IsPremium,
                    SelectedGroupId?? 0,
                    IsEnabled,
                    ToolFile);

                if (toolId > 0)
                {
                    await LoadToolsAsync();
                    ClearToolForm();
                }
                else
                {
                    ErrorMessage = "Failed to add tool.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding tool: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateToolAsync()
        {
            try
            {
                if (SelectedTool == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.EditToolAsync(
                    SelectedTool.Id,
                    ToolName,
                    ToolDescription,
                    IsPremium,
                    SelectedGroupId?? 0,
                    IsEnabled,
                    ToolFile);

                if (success)
                {
                    await LoadToolsAsync();
                    ClearToolForm();
                }
                else
                {
                    ErrorMessage = "Failed to update tool.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating tool: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteToolAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.DeleteToolAsync(tool.Id);
                if (success)
                {
                    Tools.Remove(tool);
                    if (SelectedTool == tool)
                    {
                        ClearToolForm();
                    }
                }
                else
                {
                    ErrorMessage = "Failed to delete tool.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting tool: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EnableToolAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.UpdateToolStatusAsync(tool.Id, "enable");
                if (success)
                {
                    tool.IsEnabled = true;
                    OnPropertyChanged(nameof(Tools));
                }
                else
                {
                    ErrorMessage = "Failed to enable tool.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error enabling tool: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DisableToolAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.UpdateToolStatusAsync(tool.Id, "disable");
                if (success)
                {
                    tool.IsEnabled = false;
                    OnPropertyChanged(nameof(Tools));
                }
                else
                {
                    ErrorMessage = "Failed to disable tool.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error disabling tool: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SetToolPremiumAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.UpdateToolStatusAsync(tool.Id, "setpremium");
                if (success)
                {
                    tool.IsPremium = true;
                    OnPropertyChanged(nameof(Tools));
                }
                else
                {
                    ErrorMessage = "Failed to set tool as premium.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error setting tool as premium: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SetToolFreeAsync(Tool tool)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.UpdateToolStatusAsync(tool.Id, "setfree");
                if (success)
                {
                    tool.IsPremium = false;
                    OnPropertyChanged(nameof(Tools));
                }
                else
                {
                    ErrorMessage = "Failed to set tool as free.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error setting tool as free: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddGroupAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ToolName)) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var groupId = await _toolGroupService.AddToolGroupAsync(ToolName, ToolDescription);
                if (groupId > 0)
                {
                    await LoadGroupsAsync();
                    ClearToolForm();
                }
                else
                {
                    ErrorMessage = "Failed to add tool group.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding tool group: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateGroupAsync()
        {
            try
            {
                if (SelectedGroup == null || string.IsNullOrWhiteSpace(ToolName)) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolGroupService.UpdateToolGroupAsync(SelectedGroup.Id, ToolName, ToolDescription);
                if (success)
                {
                    await LoadGroupsAsync();
                    ClearToolForm();
                }
                else
                {
                    ErrorMessage = "Failed to update tool group.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating tool group: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteGroupAsync(ToolGroup group)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolGroupService.DeleteToolGroupAsync(group.Id);
                if (success)
                {
                    Groups.Remove(group);
                    if (SelectedGroup == group)
                    {
                        ClearToolForm();
                    }
                }
                else
                {
                    ErrorMessage = "Failed to delete tool group.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting tool group: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SelectTool(Tool tool)
        {
            SelectedTool = tool;
            if (tool != null)
            {
                ToolName = tool.Name;
                ToolDescription = tool.Description;
                IsPremium = tool.IsPremium;
                IsEnabled = tool.IsEnabled;

                var group = Groups.FirstOrDefault(g => g.Name == tool.Name);
                SelectedGroupId = group?.Id ?? 0;
            }
            else
            {
                ClearToolForm();
            }
        }

        public void SelectGroup(ToolGroup group)
        {
            SelectedGroup = group;
            if (group != null)
            {
                ToolName = group.Name;
                ToolDescription = group.Description;
            }
            else
            {
                ClearToolForm();
            }
        }

        private void ClearToolForm()
        {
            ToolName = string.Empty;
            ToolDescription = string.Empty;
            IsPremium = false;
            IsEnabled = true;
            SelectedGroupId = 0;
            ToolFile = null;
            SelectedTool = null;
            SelectedGroup = null;
        }
    }
}
