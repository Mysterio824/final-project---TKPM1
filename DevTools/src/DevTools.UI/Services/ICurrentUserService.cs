using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTools.UI.Models;

namespace DevTools.UI.Services
{
    public interface ICurrentUserService
    {
        User? CurrentUser { get; }
    }
}
