using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTools.UI.Models;

namespace DevTools.UI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly App _app;

        public CurrentUserService(App app)
        {
            _app = app;
        }

        public User? CurrentUser => _app.CurrentUser;
    }
}
