using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyFormatterTool
{
    class CurrencyFormatterToolUI : UserControl
    {
        private readonly CurrencyFormatterTool _tool;
        private TextBox _amountBox;
        private TextBox _currencyCodeBox;
        private TextBlock _outputBlock;
        private TextBlock _validationBlock;

        public CurrencyFormatterToolUI(CurrencyFormatterTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            var stack = new StackPanel
            {
                Spacing = 10,
                Padding = new Microsoft.UI.Xaml.Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _amountBox = new TextBox
            {
                Header = "Amount",
                PlaceholderText = "e.g. 1000.50",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            _currencyCodeBox = new TextBox
            {
                Header = "Currency Code (ISO 4217)",
                PlaceholderText = "e.g. USD, EUR, GBP",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            _currencyCodeBox.TextChanged += OnCurrencyCodeChanged;

            _validationBlock = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 2, 0, 0)
            };

            var formatButton = new Button
            {
                Content = "Format Currency",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            formatButton.Click += OnFormatCurrencyClicked;

            var examplesBlock = new TextBlock
            {
                Text = "Common codes: USD (US Dollar), EUR (Euro), GBP (British Pound), JPY (Japanese Yen)",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 10)
            };

            _outputBlock = new TextBlock
            {
                Text = "Formatted amount will appear here...",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            stack.Children.Add(_amountBox);
            stack.Children.Add(_currencyCodeBox);
            stack.Children.Add(_validationBlock);
            stack.Children.Add(examplesBlock);
            stack.Children.Add(formatButton);
            stack.Children.Add(_outputBlock);

            this.Content = stack;
        }

        private void OnCurrencyCodeChanged(object sender, TextChangedEventArgs e)
        {
            string currencyCode = _currencyCodeBox.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(currencyCode))
            {
                _validationBlock.Text = "";
                return;
            }

            bool isValid = _tool.ValidateCurrencyCode(currencyCode);

            if (isValid)
            {
                _validationBlock.Text = "✓ Valid currency code";
                _validationBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                _validationBlock.Text = "✗ Invalid currency code";
                _validationBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private void OnFormatCurrencyClicked(object sender, RoutedEventArgs e)
        {
            string amountText = _amountBox.Text;
            string currencyCode = _currencyCodeBox.Text.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(amountText) || string.IsNullOrWhiteSpace(currencyCode))
            {
                _outputBlock.Text = "Please enter both amount and currency code";
                return;
            }

            if (!_tool.ValidateCurrencyCode(currencyCode))
            {
                _outputBlock.Text = $"Invalid currency code: {currencyCode}";
                return;
            }

            if (!decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
            {
                _outputBlock.Text = "Invalid amount format. Please enter a valid number.";
                return;
            }

            string formattedCurrency = _tool.FormatCurrency(amount, currencyCode);
            _outputBlock.Text = formattedCurrency;
        }
    }
}
