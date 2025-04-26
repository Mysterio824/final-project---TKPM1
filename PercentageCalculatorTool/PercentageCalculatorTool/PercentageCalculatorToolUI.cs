using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PercentageCalculatorTool
{
    class PercentageCalculatorToolUI : UserControl
    {
        private readonly PercentageCalculatorTool _tool;

        // First row controls
        private TextBox _percentageTextBox;
        private TextBox _valueTextBox;
        private TextBox _percentageOfResultTextBox;

        // Second row controls
        private TextBox _isValueTextBox;
        private TextBox _ofTotalTextBox;
        private TextBox _percentageResultTextBox;

        // Third row controls
        private TextBox _fromValueTextBox;
        private TextBox _toValueTextBox;
        private TextBox _changeResultTextBox;

        public PercentageCalculatorToolUI(PercentageCalculatorTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main container
            var mainStack = new StackPanel
            {
                Spacing = 20,
                Padding = new Microsoft.UI.Xaml.Thickness(30),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Add the three calculator rows
            mainStack.Children.Add(CreatePercentageOfRow());
            mainStack.Children.Add(CreateIsWhatPercentageRow());
            mainStack.Children.Add(CreatePercentageChangeRow());

            // Set the content of UserControl
            this.Content = mainStack;
        }

        private UIElement CreatePercentageOfRow()
        {
            var rowStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Padding = new Microsoft.UI.Xaml.Thickness(10)
            };

            // "What is" text block
            var whatIsTextBlock = new TextBlock
            {
                Text = "What is",
                VerticalAlignment = VerticalAlignment.Center
            };

            // Initialize TextBox first
            _percentageTextBox = new TextBox
            {
                Width = 80,
                TextAlignment = TextAlignment.Right
            };
            var percentageContainer = CreateNumberInputWithButtons(_percentageTextBox);

            // "% of" text block
            var ofTextBlock = new TextBlock
            {
                Text = "% of",
                VerticalAlignment = VerticalAlignment.Center
            };

            // Initialize TextBox first
            _valueTextBox = new TextBox
            {
                Width = 80,
                TextAlignment = TextAlignment.Right
            };
            var valueContainer = CreateNumberInputWithButtons(_valueTextBox);

            // Initialize Result TextBox first
            _percentageOfResultTextBox = new TextBox
            {
                Width = 120,
                IsReadOnly = true
            };
            var resultContainer = CreateResultWithCopyButton(_percentageOfResultTextBox);

            // Calculate button
            var calculateButton = new Button
            {
                Content = "=",
                Width = 40
            };
            calculateButton.Click += OnCalculatePercentageOf;

            // Add all controls to the row
            rowStack.Children.Add(whatIsTextBlock);
            rowStack.Children.Add(percentageContainer);
            rowStack.Children.Add(ofTextBlock);
            rowStack.Children.Add(valueContainer);
            rowStack.Children.Add(calculateButton);
            rowStack.Children.Add(resultContainer);

            return rowStack;
        }

        private UIElement CreateIsWhatPercentageRow()
        {
            var rowStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Padding = new Microsoft.UI.Xaml.Thickness(10)
            };

            // Initialize TextBox first
            _isValueTextBox = new TextBox
            {
                Width = 80,
                TextAlignment = TextAlignment.Right
            };
            var valueContainer = CreateNumberInputWithButtons(_isValueTextBox);

            // "is what percentage of" text block
            var isWhatPercentTextBlock = new TextBlock
            {
                Text = "is what percentage of",
                VerticalAlignment = VerticalAlignment.Center
            };

            // Initialize TextBox first
            _ofTotalTextBox = new TextBox
            {
                Width = 80,
                TextAlignment = TextAlignment.Right
            };
            var totalContainer = CreateNumberInputWithButtons(_ofTotalTextBox);

            // Initialize Result TextBox first
            _percentageResultTextBox = new TextBox
            {
                Width = 120,
                IsReadOnly = true
            };
            var resultContainer = CreateResultWithCopyButton(_percentageResultTextBox);

            // Calculate button
            var calculateButton = new Button
            {
                Content = "=",
                Width = 40
            };
            calculateButton.Click += OnCalculateIsWhatPercentage;

            // Add all controls to the row
            rowStack.Children.Add(valueContainer);
            rowStack.Children.Add(isWhatPercentTextBlock);
            rowStack.Children.Add(totalContainer);
            rowStack.Children.Add(calculateButton);
            rowStack.Children.Add(resultContainer);

            return rowStack;
        }

        private UIElement CreatePercentageChangeRow()
        {
            var rowStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10
            };
            var textBlock = new TextBlock
            {
                Text = "What is the percentage increase/decrease",
                VerticalAlignment = VerticalAlignment.Center
            };
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
            };
            // Initialize TextBox first
            _fromValueTextBox = new TextBox
            {
                Width = 80,
                TextAlignment = TextAlignment.Right,
                PlaceholderText = "From"
            };
            var fromContainer = CreateNumberInputWithButtons(_fromValueTextBox);

            // Initialize TextBox first
            _toValueTextBox = new TextBox
            {
                Width = 80,
                TextAlignment = TextAlignment.Right,
                PlaceholderText = "To"
            };
            var toContainer = CreateNumberInputWithButtons(_toValueTextBox);

            // Initialize Result TextBox first
            _changeResultTextBox = new TextBox
            {
                Width = 120,
                IsReadOnly = true
            };
            var resultContainer = CreateResultWithCopyButton(_changeResultTextBox);

            // Calculate button
            var calculateButton = new Button
            {
                Content = "=",
                Width = 40
            };
            calculateButton.Click += OnCalculatePercentageChange;

            // Add all controls to the row
            rowStack.Children.Add(textBlock);
            stack.Children.Add(fromContainer);
            stack.Children.Add(toContainer);
            stack.Children.Add(calculateButton);
            stack.Children.Add(resultContainer);
            rowStack.Children.Add(stack);

            return rowStack;
        }

        private UIElement CreateNumberInputWithButtons(TextBox textBox)
        {
            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // Decrease button
            var decreaseButton = new Button
            {
                Content = new FontIcon
                {
                    Glyph = "\uE738" // Minus icon
                },
                Width = 30
            };

            // Increase button
            var increaseButton = new Button
            {
                Content = new FontIcon
                {
                    Glyph = "\u002B" // Plus icon
                },
                Width = 30
            };

            // Add event handlers
            decreaseButton.Click += (s, e) => DecreaseValue(textBox);
            increaseButton.Click += (s, e) => IncreaseValue(textBox);

            // Add controls to container
            container.Children.Add(decreaseButton);
            container.Children.Add(textBox);
            container.Children.Add(increaseButton);

            return container;
        }

        private UIElement CreateResultWithCopyButton(TextBox resultTextBox)
        {
            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // Copy button
            var copyButton = new Button
            {
                Content = new FontIcon
                {
                    Glyph = "\uE8C8" // Copy icon
                },
                Width = 80
            };

            copyButton.Click += (s, e) => CopyResultToClipboard(resultTextBox.Text);

            // Add controls to container
            container.Children.Add(resultTextBox);
            container.Children.Add(copyButton);

            return container;
        }

        private void IncreaseValue(TextBox textBox)
        {
            if (double.TryParse(textBox.Text, out double currentValue))
            {
                textBox.Text = (currentValue + 1).ToString();
            }
            else
            {
                textBox.Text = "1";
            }
        }

        private void DecreaseValue(TextBox textBox)
        {
            if (double.TryParse(textBox.Text, out double currentValue))
            {
                textBox.Text = (currentValue - 1).ToString();
            }
            else
            {
                textBox.Text = "-1";
            }
        }

        private void CopyResultToClipboard(string text)
        {
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
            catch (Exception)
            {
            }
        }

        private void OnCalculatePercentageOf(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_percentageTextBox.Text, out double percentage) &&
                double.TryParse(_valueTextBox.Text, out double value))
            {
                double result = _tool.CalculatePercentageOf(percentage, value);
                _percentageOfResultTextBox.Text = result.ToString("0.##");
            }
            else
            {
                _percentageOfResultTextBox.Text = "Invalid input";
            }
        }

        private void OnCalculateIsWhatPercentage(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_isValueTextBox.Text, out double value) &&
                double.TryParse(_ofTotalTextBox.Text, out double total))
            {
                double result = _tool.CalculateIsWhatPercentageOf(value, total);
                _percentageResultTextBox.Text = result.ToString("0.##") + "%";
            }
            else
            {
                _percentageResultTextBox.Text = "Invalid input";
            }
        }

        private void OnCalculatePercentageChange(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_fromValueTextBox.Text, out double from) &&
                double.TryParse(_toValueTextBox.Text, out double to))
            {
                double result = _tool.CalculatePercentageChange(from, to);
                _changeResultTextBox.Text = result.ToString("0.##") + "%";
            }
            else
            {
                _changeResultTextBox.Text = "Invalid input";
            }
        }
    }
}
