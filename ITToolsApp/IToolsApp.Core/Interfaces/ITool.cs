using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IToolsApp.Core.Interfaces
{
    public interface ITool : INotifyPropertyChanged
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        bool IsPremium { get; set; }
        bool IsFavorite { get; set; }
        string Category { get; set; }
        bool IsAvailable { get; set; }
        UserControl GetUI();
        object Execute(object input);
    }
}