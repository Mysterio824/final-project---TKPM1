using DevTools.UI.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public class ToolGroup : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        private ObservableCollection<Tool> _tools;
        public ObservableCollection<Tool> Tools
        {
            get => _tools;
            set
            {
                if (_tools != value)
                {
                    _tools = value;
                    OnPropertyChanged(nameof(Tools));
                }
            }
        }

        public ToolGroup()
        {
            Tools = new ObservableCollection<Tool>();
            IsExpanded = true;
        }
    }
}
