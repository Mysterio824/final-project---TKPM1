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
        UserControl GetUI();
        object Execute(object input);
    }
}