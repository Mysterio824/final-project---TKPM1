using DevTools.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public static class AppServices
    {
        public static ApiService ApiService { get; } = new ApiService();
        //public static AuthService AuthService { get; } = new AuthService();
        public static AuthService AuthService { get; } = App.Services.GetRequiredService<AuthService>();
        //public static FileService FileService { get; } = new FileService();
        //public static ToolService ToolService { get; } = new ToolService();
        //public static ToolUploadService ToolUploadService { get; } = new ToolUploadService();
        //public static ToolLoaderService ToolLoaderService { get; } = new ToolLoaderService();
        public static FileService FileService { get; } = App.Services.GetRequiredService<FileService>();
        public static ToolService ToolService { get; } = App.Services.GetRequiredService<ToolService>();
        public static ToolUploadService ToolUploadService { get; } = App.Services.GetRequiredService<ToolUploadService>();
        public static ToolLoaderService ToolLoaderService { get; } = App.Services.GetRequiredService<ToolLoaderService>();

        // Navigation
        public static NavigationService NavigationService { get; } = App.Services.GetRequiredService<NavigationService>();

        // ViewModels - created on demand
        private static DashboardViewModel _dashboardViewModel;
        public static DashboardViewModel DashboardViewModel =>
            _dashboardViewModel ??= new DashboardViewModel(ToolService, NavigationService);

        private static AdminDashboardViewModel _adminDashboardViewModel;
        public static AdminDashboardViewModel AdminDashboardViewModel =>
            _adminDashboardViewModel ??= new AdminDashboardViewModel(ToolService, ToolUploadService);

        private static LoginViewModel _loginViewModel;
        public static LoginViewModel LoginViewModel =>
            _loginViewModel ??= new LoginViewModel(AuthService);

        private static FavouriteViewModel _favouriteViewModel;
        public static FavouriteViewModel FavouriteViewModel =>
            _favouriteViewModel ??= new FavouriteViewModel(ToolService, NavigationService);

        private static ToolDetailViewModel _toolDetailViewModel;
        public static ToolDetailViewModel ToolDetailViewModel =>
            _toolDetailViewModel ??= new ToolDetailViewModel(FileService, ToolService, ToolLoaderService);
    }
}
