using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public class ToolGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPremium { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFavorite { get; set; }
    }
}
