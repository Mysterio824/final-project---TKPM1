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
using DevTools.UI.Views;
using DevTools.UI.Services;
using DevTools.UI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            var toolService = App.ServiceProvider.GetService(typeof(ToolService)) as ToolService;
            var toolGroupService = App.ServiceProvider.GetService(typeof(ToolGroupService)) as ToolGroupService;
            var accountService = App.ServiceProvider.GetService(typeof(AccountService)) as AccountService;
            var authService = App.ServiceProvider.GetService(typeof(AuthService)) as AuthService;
            var viewModel = new DashboardViewModel
            (
                toolService,
                toolGroupService,
                accountService,
                authService,
                onLogout: () => { },
                null
            );
            shell.Navigate(typeof(DashboardPage), viewModel);
        }
    }
}
