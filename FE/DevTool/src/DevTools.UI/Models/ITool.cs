using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public interface ITool : INotifyPropertyChanged
    {
        UserControl GetUI();
        object Execute(object input);
    }
}
