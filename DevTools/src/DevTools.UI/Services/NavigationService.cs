using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DevTools.UI.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public void NavigateTo(Type pageType, object parameter = null)
        {
            Debug.WriteLine($"Navigating to {pageType.Name}");
            if (_frame == null)
            {
                Debug.WriteLine("Navigation Frame is null.");
                throw new InvalidOperationException("Navigation Frame is not initialized.");
            }

            var page = _serviceProvider.GetService(pageType) as Page;
            if (page == null)
            {
                Debug.WriteLine($"Failed to resolve {pageType.Name} from DI container.");
                throw new InvalidOperationException($"Page type {pageType.Name} not registered in DI container.");
            }

            // Set the Frame's content directly to the resolved page instance
            _frame.Content = page;

            // Store the parameter in the page's Tag for later use if needed
            page.Tag = parameter;

            Debug.WriteLine($"Navigation to {pageType.Name} succeeded");
        }
    }
}
