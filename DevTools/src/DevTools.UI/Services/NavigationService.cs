using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class NavigationService : INavigationService
    {
        private Frame? _frame;

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public bool Navigate(Type pageType, object? parameter = null)
        {
            return _frame?.Navigate(pageType, parameter) ?? false;
        }

        public void GoBack()
        {
            if (_frame?.CanGoBack == true)
                _frame.GoBack();
        }
    }
}
