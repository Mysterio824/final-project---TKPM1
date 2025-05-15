using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureConverterTool
{
    class TemperatureConverterToolUI : UserControl
    {
        private readonly TemperatureConverterTool _tool;
        private Dictionary<string, (TextBox, Button, Button)> _temperatureControls;
        private bool _isUpdating = false;

        public TemperatureConverterToolUI(TemperatureConverterTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Create scrollable container
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            // Main container
            var mainPanel = new StackPanel
            {
                Spacing = 10,
                Padding = new Thickness(20)
            };

            // Initialize dictionary to store controls
            _temperatureControls = new Dictionary<string, (TextBox, Button, Button)>();

            // Temperature scales to display
            string[] scales = { "Kelvin", "Celsius", "Fahrenheit", "Rankine", "Delisle", "Newton", "Réaumur", "Rømer" };

            // Create UI for each temperature scale
            foreach (var scale in scales)
            {
                // Create row container
                var rowPanel = new Grid();

                // Define columns
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Label
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // TextBox
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Minus button
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Plus button

                // Scale label
                var label = new TextBlock
                {
                    Text = scale,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(label, 0);

                // Temperature input box
                var textBox = new TextBox
                {
                    PlaceholderText = "0",
                    VerticalAlignment = VerticalAlignment.Center
                };
                textBox.TextChanged += (s, e) => OnTemperatureTextChanged(scale, textBox);
                Grid.SetColumn(textBox, 1);

                // Minus button
                var minusButton = new Button
                {
                    Content = "-",
                    Width = 40,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                minusButton.Click += (s, e) => DecrementTemperature(scale, textBox);
                Grid.SetColumn(minusButton, 2);

                // Plus button
                var plusButton = new Button
                {
                    Content = "+",
                    Width = 40,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                plusButton.Click += (s, e) => IncrementTemperature(scale, textBox);
                Grid.SetColumn(plusButton, 3);

                // Add elements to row
                rowPanel.Children.Add(label);
                rowPanel.Children.Add(textBox);
                rowPanel.Children.Add(minusButton);
                rowPanel.Children.Add(plusButton);

                // Add row to main panel
                mainPanel.Children.Add(rowPanel);

                // Store controls for this scale
                _temperatureControls.Add(scale, (textBox, minusButton, plusButton));
            }

            // Set initial value (0 Kelvin)
            SetTemperature("Kelvin", 0);

            // Set content
            scrollViewer.Content = mainPanel;
            this.Content = scrollViewer;
        }

        private void OnTemperatureTextChanged(string scale, TextBox textBox)
        {
            // Prevent recursive updates
            if (_isUpdating) return;

            if (double.TryParse(textBox.Text, out double value))
            {
                SetTemperature(scale, value);
            }
        }

        private void IncrementTemperature(string scale, TextBox textBox)
        {
            if (double.TryParse(textBox.Text, out double value))
            {
                SetTemperature(scale, value + 1);
            }
            else
            {
                SetTemperature(scale, 1);
            }
        }

        private void DecrementTemperature(string scale, TextBox textBox)
        {
            if (double.TryParse(textBox.Text, out double value))
            {
                SetTemperature(scale, value - 1);
            }
            else
            {
                SetTemperature(scale, -1);
            }
        }

        private void SetTemperature(string scale, double value)
        {
            try
            {
                _isUpdating = true;

                // Convert to all temperature scales
                var allTemperatures = _tool.ConvertTemperature(scale, value);

                // Update all textboxes
                foreach (var tempScale in _temperatureControls.Keys)
                {
                    var textBox = _temperatureControls[tempScale].Item1;
                    textBox.Text = Math.Round(allTemperatures[tempScale], 2).ToString();
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}
