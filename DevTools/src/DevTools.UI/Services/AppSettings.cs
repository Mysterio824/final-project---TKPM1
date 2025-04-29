using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "en";

        public async Task LoadAsync()
        {
        }

        public async Task SaveAsync()
        {
        }
    }
}
