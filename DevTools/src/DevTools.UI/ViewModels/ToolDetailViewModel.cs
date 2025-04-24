using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace DevTools.UI.ViewModels
{
    public class ToolDetailViewModel : BaseViewModel
    {
        private readonly ToolService _toolService;
        private readonly ToolLoader _toolLoader = ToolLoader.Instance;
        private Tool _tool;
        private UserControl _toolUI;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isToolLoaded;
        private ITool _toolInstance;
        private object _lastResult;
        private bool _hasResult;

        public Tool Tool
        {
            get => _tool;
            set => SetProperty(ref _tool, value);
        }

        public UserControl ToolUI
        {
            get => _toolUI;
            set => SetProperty(ref _toolUI, value);
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

        public bool IsToolLoaded
        {
            get => _isToolLoaded;
            set => SetProperty(ref _isToolLoaded, value);
        }

        public object LastResult
        {
            get => _lastResult;
            set
            {
                if (SetProperty(ref _lastResult, value))
                {
                    HasResult = value != null;
                }
            }
        }

        public bool HasResult
        {
            get => _hasResult;
            set => SetProperty(ref _hasResult, value);
        }

        public ICommand LoadToolCommand { get; }
        public ICommand ExecuteToolCommand { get; }

        public ToolDetailViewModel(ToolService toolService)
        {
            _toolService = toolService;
            LoadToolCommand = new AsyncCommand<Tool>(LoadToolAsync);
            ExecuteToolCommand = new AsyncCommand<object>(ExecuteToolAsync, CanExecuteTool);
        }

        private bool CanExecuteTool(object input)
        {
            return IsToolLoaded && _toolInstance != null;
        }

        public async Task LoadToolAsync(Tool tool)
        {
            try
            {
                ClearPreviousState();
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (tool == null)
                {
                    ErrorMessage = "Tool is null.";
                    return;
                }

                // Fetch tool data from service if FileData is missing
                if (tool.FileData == null || tool.FileData.Length == 0)
                {
                    Tool = await _toolService.GetToolByIdAsync(tool.Id);
                    if (Tool == null || Tool.FileData == null || Tool.FileData.Length == 0)
                    {
                        ErrorMessage = "Failed to load tool or tool data is missing.";
                        return;
                    }
                    tool = Tool;
                }

                // Use ToolLoader to load the plugin
                _toolInstance = _toolLoader.LoadPlugin(tool, tool.FileData);
                if (_toolInstance == null)
                {
                    ErrorMessage = "Failed to load tool from plugin.";
                    return;
                }

                // Get the tool's UI
                var toolUI = _toolInstance.GetUI() as UserControl;
                if (toolUI == null)
                {
                    ErrorMessage = "Tool UI could not be created.";
                    return;
                }

                ToolUI = toolUI;
                IsToolLoaded = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load tool: {ex.Message}";
                Debug.WriteLine($"Exception details: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ExecuteToolAsync(object input)
        {
            if (!IsToolLoaded || _toolInstance == null)
            {
                ErrorMessage = "Tool is not loaded properly.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Execute the tool
                LastResult = _toolInstance.Execute(input);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error executing tool: {ex.Message}";
                Debug.WriteLine($"Exception details: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearPreviousState()
        {
            Tool = null;
            ToolUI = null;
            IsToolLoaded = false;
            _toolInstance = null;
            LastResult = null;
        }

        public void Cleanup()
        {
            ToolUI = null;
            _toolInstance = null;
        }
    }
}