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
using DevTools.UI.Services;
using DevTools.UI.ViewModels;
using DevTools.UI.Models;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; private set; }

        public LoginPage()
        {
            this.InitializeComponent();
            this.Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply animation when page loads
            EntranceAnimation.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var authService = App.ServiceProvider.GetService(typeof(AuthService)) as AuthService;
            ViewModel = new LoginViewModel(authService, OnLoginSuccess);
            EmailTextBox.Focus(FocusState.Programmatic);
        }

        private void OnLoginSuccess(User user)
        {
            var exitAnimation = new Storyboard();
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };
            Storyboard.SetTarget(fadeOut, MainPanel);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");
            exitAnimation.Children.Add(fadeOut);

            var toolService = App.ServiceProvider.GetService(typeof(ToolService)) as ToolService;
            var toolGroupService = App.ServiceProvider.GetService(typeof(ToolGroupService)) as ToolGroupService;
            if (user.IsAdmin)
            {
                var adminViewModel = new AdminDashboardViewModel(
                    toolService,
                    toolGroupService,
                    user
                );

                exitAnimation.Completed += (s, e) =>
                {
                    Frame.Navigate(typeof(AdminDashboardPage), adminViewModel);
                };
            }
            else
            {
                var accountService = App.ServiceProvider.GetService(typeof(AccountService)) as AccountService;
                var authService = App.ServiceProvider.GetService(typeof(AuthService)) as AuthService;
                var viewModel = new DashboardViewModel(
                    toolService,
                    toolGroupService,
                    accountService,
                    authService,
                    onLogout: () =>
                    {
                        Frame.Navigate(typeof(DashboardPage), new DashboardViewModel(toolService, toolGroupService, accountService, authService, () => { }));
                    },
                    user
                );

                exitAnimation.Completed += (s, e) =>
                {
                    Frame.Navigate(typeof(DashboardPage), viewModel);
                };
            }

            exitAnimation.Begin();
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }
    }
}