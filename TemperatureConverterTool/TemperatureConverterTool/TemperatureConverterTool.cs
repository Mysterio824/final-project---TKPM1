using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureConverterTool
{
    class TemperatureConverterTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to convert temperatures
        public Dictionary<string, double> ConvertTemperature(string scale, double value)
        {
            double kelvin;
            switch (scale)
            {
                case "Kelvin":
                    kelvin = value;
                    break;
                case "Celsius":
                    kelvin = value + 273.15;
                    break;
                case "Fahrenheit":
                    kelvin = (value + 459.67) * 5 / 9;
                    break;
                case "Rankine":
                    kelvin = value * 5 / 9;
                    break;
                case "Delisle":
                    kelvin = 373.15 - (value * 2 / 3);
                    break;
                case "Newton":
                    kelvin = value * 100 / 33 + 273.15;
                    break;
                case "Réaumur":
                    kelvin = value * 5 / 4 + 273.15;
                    break;
                case "Rømer":
                    kelvin = (value - 7.5) * 40 / 21 + 273.15;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scale), scale, null);
            }

            // Convert Kelvin to all scales
            var results = new Dictionary<string, double>
            {
                ["Kelvin"] = kelvin,
                ["Celsius"] = kelvin - 273.15,
                ["Fahrenheit"] = kelvin * 9 / 5 - 459.67,
                ["Rankine"] = kelvin * 9 / 5,
                ["Delisle"] = (373.15 - kelvin) * 3 / 2,
                ["Newton"] = (kelvin - 273.15) * 33 / 100,
                ["Réaumur"] = (kelvin - 273.15) * 4 / 5,
                ["Rømer"] = (kelvin - 273.15) * 21 / 40 + 7.5
            };

            return results;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new TemperatureConverterToolUI(this);
        }
    }
}
