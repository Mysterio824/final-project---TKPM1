using DevTools.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.ViewModels
{
    public class ViewModelLocator
    {
        public DashboardViewModel DashboardViewModel =>
            AppServices.DashboardViewModel;

        public ToolDetailViewModel ToolDetailViewModel =>
            AppServices.ToolDetailViewModel;

        public FavouriteViewModel FavouriteViewModel =>
            AppServices.FavouriteViewModel;

        public AdminDashboardViewModel AdminDashboardViewModel =>
            AppServices.AdminDashboardViewModel;

        public LoginViewModel LoginViewModel =>
            AppServices.LoginViewModel;
    }
}
