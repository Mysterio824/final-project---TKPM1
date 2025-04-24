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
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using DevTools.UI.Services;
using DevTools.UI.ViewModels;

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
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var builder = new ServiceCollection();

            // Register HttpClient and AuthService
            builder.AddSingleton<HttpClient>(new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true
            }));
            builder.AddSingleton<AuthService>(provider =>
            {
                // Provide the base URL for the API
                var httpClient = provider.GetRequiredService<HttpClient>();
                var baseUrl = "https://localhost:5000";
                return new AuthService(httpClient, baseUrl);
            });
            builder.AddSingleton<ToolService>(provider =>
            {
                // Provide the base URL for the API
                var httpClient = provider.GetRequiredService<HttpClient>();
                var baseUrl = "https://localhost:5000";
                return new ToolService(httpClient, baseUrl);
            });
            builder.AddSingleton<AccountService>(provider =>
            {
                // Provide the base URL for the API
                var httpClient = provider.GetRequiredService<HttpClient>();
                var baseUrl = "https://localhost:5000";
                return new AccountService(httpClient, baseUrl);
            });
            builder.AddSingleton<ToolGroupService>(provider =>
            {
                // Provide the base URL for the API
                var httpClient = provider.GetRequiredService<HttpClient>();
                var baseUrl = "https://localhost:5000";
                return new ToolGroupService(httpClient, baseUrl);
            });
            builder.AddTransient<LoginViewModel>();
            builder.AddTransient<RegisterViewModel>();
            builder.AddTransient<AdminDashboardViewModel>();
            builder.AddTransient<DashboardViewModel>();
            builder.AddTransient<ToolDetailViewModel>();

            ServiceProvider = builder.BuildServiceProvider();
            
            m_window = new MainWindow();
            m_window.Activate();
        }
        public static Window? m_window;
    }
}
