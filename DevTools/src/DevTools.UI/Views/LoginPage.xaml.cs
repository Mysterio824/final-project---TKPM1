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
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }
        private readonly INavigationService _navigationService;

        public LoginPage(LoginViewModel viewModel, INavigationService navigationService)
        {
            this.InitializeComponent();
            _navigationService = navigationService;
            ViewModel = viewModel;
            DataContext = ViewModel;
            this.Loaded += LoginPage_Loaded;

            RegisterLink.Click += (s, e) => navigationService.NavigateTo(typeof(RegisterPage));
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply animation when page loads
            EntranceAnimation.Begin();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //var authService = App.ServiceProvider.GetService(typeof(AuthService)) as AuthService;
            //ViewModel = new LoginViewModel(authService, OnLoginSuccess);
            EmailTextBox.Focus(FocusState.Programmatic);
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard exitAnimation = new Storyboard();
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            Storyboard.SetTarget(fadeOut, MainPanel);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");
            exitAnimation.Children.Add(fadeOut);

            exitAnimation.Begin();
            await Task.Delay(180);

            // Use navigationService instead of Frame.Navigate
            _navigationService.NavigateTo(typeof(DashboardPage));
        }
    }
}