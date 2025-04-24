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
using DevTools.UI.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using DevTools.UI.ViewModels;
using Microsoft.AspNetCore.Http.Internal;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminDashboardPage : Page
    {
        public AdminDashboardViewModel ViewModel { get; private set; }

        public AdminDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is AdminDashboardViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            LoadData();
        }

        private async void LoadData()
        {
            await ViewModel.LoadGroupsAsync();
            await ViewModel.LoadToolsAsync();
        }

        private async void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            filePicker.FileTypeFilter.Add(".dll");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                var formFile = await ConvertToFormFileAsync(file);
                ViewModel.ToolFile = formFile;
            }
        }

        private async Task<IFormFile> ConvertToFormFileAsync(StorageFile file)
        {
            var stream = await file.OpenStreamForReadAsync();
            var formFile = new FormFile(stream, 0, stream.Length, file.Name, file.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = file.ContentType
            };

            return formFile;
        }

        private void OnClearFormClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectTool(null);
            ViewModel.SelectGroup(null);
        }

        private void OnToolSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Tool tool)
            {
                ViewModel.SelectTool(tool);
            }
        }

        private void OnGroupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ToolGroup group)
            {
                ViewModel.SelectGroup(group);
            }
        }

        private void OnEditToolClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Tool tool)
            {
                ViewModel.SelectTool(tool);
            }
        }
    }
}