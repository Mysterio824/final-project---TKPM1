using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace DevTools.UI.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void NavigateTo(Type pageType, object parameter = null);
    }
}
