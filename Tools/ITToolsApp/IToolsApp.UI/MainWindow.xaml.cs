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
using IToolsApp.Core.Services;
using IToolsApp.Core.Interfaces;
using Windows.Storage;

namespace IToolsApp.UI
{
    public sealed partial class MainWindow : Window
    {
        private readonly ToolLoader _toolLoader = ToolLoader.Instance;
        public List<ITool> Tools { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
            LoadTools();
            _toolLoader.ToolAdded += ToolLoader_ToolAdded;
        }

        private void ToolLoader_ToolAdded(object sender, ToolAddedEventArgs e)
        {
            AddToolToUI(e.Tool);
        }

        private void LoadTools()
        {
            Tools = _toolLoader.GetAllTools().ToList();
            foreach (var tool in Tools)
            {
                AddToolToUI(tool);
            }
        }

        private void AddToolToUI(ITool tool)
        {
            var toolItem = new ListViewItem
            {
                Content = tool.Name,
                Tag = tool,
                Margin = new Thickness(0, 0, 0, 5)
            };
            toolItem.Tapped += ToolItem_Tapped;
            ToolsListView.Items.Add(toolItem);
        }

        private void ToolItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is ListViewItem item && item.Tag is ITool tool)
            {
                ContentFrame.Content = tool.GetUI();
            }
        }

        private async void UploadPlugin_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add(".dll");

            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(this));

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var bytes = await FileIO.ReadBufferAsync(file);
                _toolLoader.LoadPlugin(bytes.ToArray());
            }
        }
    }
}
