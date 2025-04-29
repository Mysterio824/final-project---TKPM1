using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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
        public bool IsGroupListEmpty => Groups == null || !Groups.Any();
        public bool IsToolListEmpty => Tools == null || !Tools.Any();

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
            set
            {
                if (SetProperty(ref _groups, value))
                {
                    // Subscribe to collection change events
                    if (_groups != null)
                    {
                        _groups.CollectionChanged += Groups_CollectionChanged;
                    }

                    OnPropertyChanged(nameof(IsGroupListEmpty)); // Ensure it’s set initially
                }
            }
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
                (UpdateToolCommand as AsyncCommand)?.RaiseCanExecuteChanged();
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
            set
            {
                SetProperty(ref _groupName, value);
                (AddGroupCommand as AsyncCommand)?.RaiseCanExecuteChanged();
                (UpdateGroupCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
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
                (UpdateToolCommand as AsyncCommand)?.RaiseCanExecuteChanged();
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
        private void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsGroupListEmpty));
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

        public AdminDashboardViewModel(ToolService toolService, ToolGroupService toolGroupService)
        {
            _toolService = toolService ?? throw new ArgumentNullException(nameof(toolService));
            _toolGroupService = toolGroupService ?? throw new ArgumentNullException(nameof(toolGroupService));

            // Initialize collections
            Tools = new ObservableCollection<Tool>();
            Groups = new ObservableCollection<ToolGroup>();

            // Set default values
            IsEnabled = true;

            // Initialize commands
            LoadToolsCommand = new AsyncCommand(LoadToolsAsync);
            LoadGroupsCommand = new AsyncCommand(LoadGroupsAsync);
            AddToolCommand = new AsyncCommand(AddToolAsync, CanAddTool);
            UpdateToolCommand = new AsyncCommand(UpdateToolAsync, CanUpdateTool);
            DeleteToolCommand = new AsyncCommand<Tool>(DeleteToolAsync, CanDeleteTool);
            EnableToolCommand = new AsyncCommand<Tool>(EnableToolAsync);
            DisableToolCommand = new AsyncCommand<Tool>(DisableToolAsync);
            SetPremiumCommand = new AsyncCommand<Tool>(SetToolPremiumAsync);
            SetFreeCommand = new AsyncCommand<Tool>(SetToolFreeAsync);
            AddGroupCommand = new AsyncCommand(AddGroupAsync, CanAddGroup);
            UpdateGroupCommand = new AsyncCommand(UpdateGroupAsync, CanUpdateGroup);
            DeleteGroupCommand = new AsyncCommand<ToolGroup>(DeleteGroupAsync, CanDeleteGroup);

            // Set auth tokens if needed
            //if (adminUser?.Token != null)
            //{
            //    _toolService.SetAuthToken(adminUser.Token);
            //    _toolGroupService.SetAuthToken(adminUser.Token);
            //}
        }

        private bool CanAddTool() =>
            !string.IsNullOrWhiteSpace(ToolName) &&
            SelectedGroupId.HasValue &&
            SelectedGroupId.Value > 0 &&
            ToolFile != null;

        private bool CanUpdateTool() =>
            SelectedTool != null &&
            !string.IsNullOrWhiteSpace(ToolName) &&
            SelectedGroupId.HasValue &&
            SelectedGroupId.Value > 0;

        private bool CanDeleteTool(Tool tool) => tool != null;

        private bool CanAddGroup() => !string.IsNullOrWhiteSpace(GroupName);

        private bool CanUpdateGroup() =>
            SelectedGroup != null &&
            !string.IsNullOrWhiteSpace(GroupName);

        private bool CanDeleteGroup(ToolGroup group) => group != null;

        public async Task LoadToolsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

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
                Debug.WriteLine($"Exception in LoadToolsAsync: {ex}");
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
                ErrorMessage = string.Empty;

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
                Debug.WriteLine($"Exception in LoadGroupsAsync: {ex}");
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
                if (!CanAddTool()) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var toolId = await _toolService.AddToolAsync(
                    ToolName,
                    ToolDescription,
                    IsPremium,
                    SelectedGroupId.Value,
                    IsEnabled,
                    ToolFile);

                if (toolId > 0)
                {
                    await LoadToolsAsync();
                    ClearToolForm();
                }
                else
                {
                    ErrorMessage = "Failed to add tool. Please check your input and try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding tool: {ex.Message}";
                Debug.WriteLine($"Exception in AddToolAsync: {ex}");
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
                if (!CanUpdateTool()) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolService.EditToolAsync(
                    SelectedTool.Id,
                    ToolName,
                    ToolDescription,
                    IsPremium,
                    SelectedGroupId.Value,
                    IsEnabled,
                    ToolFile);

                if (success)
                {
                    await LoadToolsAsync();
                    ClearToolForm();
                }
                else
                {
                    ErrorMessage = "Failed to update tool. Please check your input and try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating tool: {ex.Message}";
                Debug.WriteLine($"Exception in UpdateToolAsync: {ex}");
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
                if (tool == null) return;

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
                    ErrorMessage = "Failed to delete tool. It may be in use by the system.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting tool: {ex.Message}";
                Debug.WriteLine($"Exception in DeleteToolAsync: {ex}");
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
                if (tool == null) return;

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
                Debug.WriteLine($"Exception in EnableToolAsync: {ex}");
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
                if (tool == null) return;

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
                Debug.WriteLine($"Exception in DisableToolAsync: {ex}");
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
                if (tool == null) return;

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
                Debug.WriteLine($"Exception in SetToolPremiumAsync: {ex}");
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
                if (tool == null) return;

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
                Debug.WriteLine($"Exception in SetToolFreeAsync: {ex}");
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
                if (!CanAddGroup()) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var groupId = await _toolGroupService.AddToolGroupAsync(GroupName, GroupDescription);

                if (groupId > 0)
                {
                    await LoadGroupsAsync();
                    ClearGroupForm();
                }
                else
                {
                    ErrorMessage = "Failed to add tool group.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding tool group: {ex.Message}";
                Debug.WriteLine($"Exception in AddGroupAsync: {ex}");
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
                if (!CanUpdateGroup()) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolGroupService.UpdateToolGroupAsync(
                    SelectedGroup.Id,
                    GroupName,
                    GroupDescription);

                if (success)
                {
                    await LoadGroupsAsync();
                    ClearGroupForm();
                }
                else
                {
                    ErrorMessage = "Failed to update tool group.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating tool group: {ex.Message}";
                Debug.WriteLine($"Exception in UpdateGroupAsync: {ex}");
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
                if (group == null) return;

                // Check if any tools are using this group before deletion
                bool toolsUsingGroup = Tools.Any(t => t.GroupName == group.Name);
                if (toolsUsingGroup)
                {
                    ErrorMessage = "Cannot delete group that has associated tools. Remove or reassign the tools first.";
                    return;
                }

                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _toolGroupService.DeleteToolGroupAsync(group.Id);

                if (success)
                {
                    Groups.Remove(group);
                    if (SelectedGroup == group)
                    {
                        ClearGroupForm();
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
                Debug.WriteLine($"Exception in DeleteGroupAsync: {ex}");
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
                // Populate form fields with tool data
                ToolName = tool.Name;
                ToolDescription = tool.Description;
                IsPremium = tool.IsPremium;
                IsEnabled = tool.IsEnabled;

                // Find the correct group by ID
                SelectedGroupId = SelectedGroup.Id;
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
                // Populate form fields with group data
                GroupName = group.Name;
                GroupDescription = group.Description;
            }
            else
            {
                ClearGroupForm();
            }
        }

        public void ClearToolForm()
        {
            ToolName = string.Empty;
            ToolDescription = string.Empty;
            IsPremium = false;
            IsEnabled = true;
            SelectedGroupId = null;
            ToolFile = null;
            SelectedTool = null;
        }

        public void ClearGroupForm()
        {
            GroupName = string.Empty;
            GroupDescription = string.Empty;
            SelectedGroup = null;
        }
    }
}