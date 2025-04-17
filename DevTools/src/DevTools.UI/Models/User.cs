using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsPremium { get; set; }
    }
}
