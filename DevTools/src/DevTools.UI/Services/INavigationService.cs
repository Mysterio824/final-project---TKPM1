using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public interface INavigationService
    {
        void SetFrame(Frame frame);
        bool Navigate(Type pageType, object? parameter = null);
        void GoBack();
    }
}
