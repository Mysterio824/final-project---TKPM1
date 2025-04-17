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
using DevTools.UI.Models;
using DevTools.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using DevTools.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        private DashboardViewModel ViewModel => AppServices.DashboardViewModel;

        public DashboardPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        private void ToolItem_Click(object sender, ItemClickEventArgs e)
        {
            var selectedTool = e.ClickedItem as Tool;

            // Verify access for premium tools
            if (selectedTool.IsPremium && !JwtTokenManager.IsPremium)
            {
                if (!JwtTokenManager.IsLoggedIn)
                {
                    AppServices.NavigationService.Navigate(typeof(LoginPage), "premium");
                }
                else
                {
                    AppServices.NavigationService.Navigate(typeof(UpgradePage));
                }
                return;
            }

            // Navigate to the tool detail page
            AppServices.NavigationService.Navigate(typeof(ToolDetailPage), selectedTool);
        }
    }
}
