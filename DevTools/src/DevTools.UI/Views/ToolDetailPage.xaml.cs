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
using Microsoft.Extensions.DependencyInjection;
using DevTools.UI.Utils;
using DevTools.UI.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ToolDetailPage : Page
    {
        public ToolDetailViewModel ViewModel { get; private set; }

        public ToolDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ToolDetailViewModel viewModel)
            {
                ViewModel = viewModel;
            }
            else if (e.Parameter is Tool tool)
            {
                ViewModel = App.ServiceProvider.GetService<ToolDetailViewModel>();
                LoadTool(tool);
            }
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel?.Cleanup();
        }

        private void LoadTool(Tool tool)
        {
            if (ViewModel.LoadToolCommand is AsyncCommand<int> asyncCommand)
            {
                asyncCommand.Execute(tool);
            }
            else
            {
                ViewModel.LoadToolCommand.Execute(tool);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Tool != null)
            {
                ViewModel.LoadToolCommand.Execute(ViewModel.Tool.Id);
            }
        }
    }
}