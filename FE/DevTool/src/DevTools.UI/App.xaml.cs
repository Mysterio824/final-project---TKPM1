using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using DevTools.UI.Services;
using DevTools.UI.ViewModels;
using DevTools.UI.Models;
using Microsoft.Extensions.DependencyInjection;
using DevTools.UI.Views;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DevTools.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }
        public User? CurrentUser { get; set; } = null;
        public static MainWindow? mainWindow { get; private set; }
        public IServiceProvider serviceProvider { get; private set; }
        public Tool? SelectedTool { get; set; } = null;


        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var builder = new ServiceCollection();
            string basePath = AppContext.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(basePath);
            DirectoryInfo projectDir = dir.Parent?.Parent?.Parent?.Parent?.Parent?.Parent;
            string projectPath = projectDir?.FullName;

            var configuration = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            builder.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true // Use cautiously
            });
            builder.AddHttpClient("UnauthenticatedApiClient", client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true
            });
            builder.AddSingleton(this);
            builder.AddTransient<AuthHandler>();
            builder.AddSingleton<AuthService>();
            builder.AddSingleton<ToolService>();
            builder.AddSingleton<AccountService>();
            builder.AddSingleton<ToolGroupService>();
            builder.AddSingleton<INavigationService, NavigationService>();
            builder.AddSingleton<ICurrentUserService, CurrentUserService>();
            builder.AddSingleton<ToolLoader>();

            // Register pages and ViewModels
            builder.AddTransient<RegisterPage>();
            builder.AddTransient<RegisterViewModel>();
            builder.AddTransient<LoginPage>();
            builder.AddTransient<LoginViewModel>();
            builder.AddTransient<DashboardPage>();
            builder.AddTransient<DashboardViewModel>();
            builder.AddTransient<AdminDashboardPage>();
            builder.AddTransient<AdminDashboardViewModel>();
            builder.AddTransient<ToolDetailPage>();
            builder.AddTransient<ToolDetailViewModel>();

            serviceProvider = builder.BuildServiceProvider();

            mainWindow = new MainWindow(serviceProvider);
            mainWindow.Activate();
        }
    }
}
