using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.ViewModels
{
    public class ToolDetailViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly ToolService _toolService;
        private readonly ToolLoaderService _loader;
        private Tool _toolMetadata;
        private bool _isUserLoggedIn;

        public Tool ToolMetadata
        {
            get => _toolMetadata;
            private set => SetProperty(ref _toolMetadata, value);
        }

        public bool IsUserLoggedIn
        {
            get => _isUserLoggedIn;
            set => SetProperty(ref _isUserLoggedIn, value);
        }

        private ITool? _toolInstance;
        public ITool? ToolInstance
        {
            get => _toolInstance;
            private set => SetProperty(ref _toolInstance, value);
        }

        public UserControl? ToolUI => ToolInstance?.GetUI();
        public RelayCommand AddToFavoriteCommand { get; }

        public ToolDetailViewModel(FileService fileService, ToolService toolService, ToolLoaderService loader)
        {
            _fileService = fileService;
            _toolService = toolService;
            _loader = loader;

            // Initialize with current login state
            IsUserLoggedIn = JwtTokenManager.IsLoggedIn;

            AddToFavoriteCommand = new RelayCommand(async () => await AddToFavouritesAsync(), () => IsUserLoggedIn);
        }

        public async Task LoadToolAsync(Tool tool)
        {
            try
            {
                ToolMetadata = tool;
                // Download the DLL for the tool
                var dll = await _fileService.DownloadToolDllAsync(tool.Id);
                // Load the tool instance
                ToolInstance = await _loader.LoadAsync(dll);
                // Notify UI of changes
                OnPropertyChanged(nameof(ToolUI));
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., log them or show a message to the user)
                Console.WriteLine($"Error loading tool: {ex.Message}");
            }
        }

        private async Task AddToFavouritesAsync()
        {
            if (ToolMetadata == null)
                return;
            try
            {
                var success = await _toolService.AddToFavouritesAsync(ToolMetadata.Id);
                if (success)
                {
                    // Optionally, notify the user or update the UI
                    Console.WriteLine($"Tool '{ToolMetadata.Name}' added to favorites.");
                }
                else
                {
                    Console.WriteLine($"Failed to add tool '{ToolMetadata.Name}' to favorites.");
                }
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., log them or show a message to the user)
                Console.WriteLine($"Error adding tool to favorites: {ex.Message}");
            }
        }
    }
}
