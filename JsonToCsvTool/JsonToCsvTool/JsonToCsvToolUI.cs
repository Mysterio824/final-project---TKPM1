using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace JsonToCsvTool
{
    class JsonToCsvToolUI : UserControl
    {
        private readonly JsonToCsvTool _tool;
        private TextBox _inputBox;
        private TextBox _outputBox;
        private TextBlock _validationMessage;

        public JsonToCsvToolUI(JsonToCsvTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Creating StackPanel to hold other controls
            var stack = new StackPanel
            {
                Spacing = 10,
                Padding = new Microsoft.UI.Xaml.Thickness(20)
            };

            // Create and configure TextBox (Input)
            _inputBox = new TextBox
            {
                Header = "Input JSON",
                Width = 500,
                Height = 200,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };
            _inputBox.TextChanged += OnInputTextChanged;

            // Create validation message text block
            _validationMessage = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            // Create and configure Button (Convert)
            var convertButton = new Button
            {
                Content = "Convert JSON to CSV",
                Width = 200,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            convertButton.Click += OnConvertClicked;

            // Create and configure TextBox (Output)
            _outputBox = new TextBox
            {
                Header = "CSV Output",
                IsReadOnly = true,
                Width = 500,
                Height = 200,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            // Add controls to StackPanel
            stack.Children.Add(_inputBox);
            stack.Children.Add(_validationMessage);
            stack.Children.Add(convertButton);
            stack.Children.Add(_outputBox);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateJson(_inputBox.Text);
        }

        private void OnConvertClicked(object sender, RoutedEventArgs e)
        {
            var jsonInput = _inputBox.Text;
            if (string.IsNullOrWhiteSpace(jsonInput))
            {
                _outputBox.Text = string.Empty;
                return;
            }

            var csvResult = _tool.ConvertJsonToCsv(jsonInput);
            if (csvResult == null)
            {
                _validationMessage.Text = "Provided JSON is not valid";
                _validationMessage.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                _outputBox.Text = string.Empty;
            }
            else
            {
                _validationMessage.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                _outputBox.Text = csvResult;
            }
        }

        private void ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                _validationMessage.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    throw new JsonException(); // Force error if not an array

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                        throw new JsonException(); // Ensure each item is an object
                }

                _validationMessage.Visibility = Visibility.Collapsed;
            }
            catch
            {
                _validationMessage.Text = "Provided JSON is not valid";
                _validationMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
