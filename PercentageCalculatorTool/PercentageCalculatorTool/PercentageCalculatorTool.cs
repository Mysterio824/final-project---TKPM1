using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PercentageCalculatorTool
{
    class PercentageCalculatorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public double CalculatePercentageOf(double percentage, double value)
        {
            return (percentage / 100) * value;
        }

        public double CalculateIsWhatPercentageOf(double value, double total)
        {
            if (total == 0)
                return 0;
            return (value / total) * 100;
        }

        public double CalculatePercentageChange(double from, double to)
        {
            if (from == 0)
                return 0;
            return ((to - from) / from) * 100;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new PercentageCalculatorToolUI(this);
        }
    }
}
