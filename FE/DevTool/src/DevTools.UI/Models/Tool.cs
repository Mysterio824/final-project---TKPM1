using DevTools.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public class Tool : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPremium { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFavorite { get; set; }
        public byte[] FileData { get; set; }
        private string _symbolGlyph = "\uE774"; // Default tool icon
        public string SymbolGlyph
        {
            get => _symbolGlyph;
            set
            {
                if (_symbolGlyph != value)
                {
                    _symbolGlyph = value;
                    OnPropertyChanged(nameof(SymbolGlyph));
                }
            }
        }

        public string GroupName { get; set; }
    }
}
