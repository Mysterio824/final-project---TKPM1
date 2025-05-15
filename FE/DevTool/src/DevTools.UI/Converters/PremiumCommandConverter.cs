using DevTools.UI.ViewModels;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Converters
{
    public class PremiumCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isPremium = (bool)value;
            var viewModel = parameter as DashboardViewModel;
            return isPremium ? viewModel.RevokePremiumCommand : viewModel.RequestPremiumCommand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
