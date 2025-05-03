using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public T Result { get; set; }
        public List<string> Errors { get; set; }
    }
}
