using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace TokenGeneratorTool
{
    class TokenGeneratorToolUI : UserControl
    {
        private readonly TokenGeneratorTool _tool;
        private CheckBox _lowercaseCheck;
        private CheckBox _uppercaseCheck;
        private CheckBox _numbersCheck;
        private CheckBox _symbolsCheck;
        private TextBlock _outputBlock;
        private Slider _lengthSlider;

        public TokenGeneratorToolUI(TokenGeneratorTool tool)
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

            var stack1 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Padding = new Microsoft.UI.Xaml.Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _lowercaseCheck = new CheckBox
            {
                Content = "Include lowercase letters (a-z)",
                IsChecked = true,
                Width = 350
                //HorizontalAlignment = HorizontalAlignment.Center
            };

            _uppercaseCheck = new CheckBox
            {
                Content = "Include uppercase letters (A-Z)",
                IsChecked = true,
                Width = 350,
                //HorizontalAlignment = HorizontalAlignment.Center
            };
            stack1.Children.Add(_lowercaseCheck);
            stack1.Children.Add(_uppercaseCheck);
            var stack2 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Padding = new Microsoft.UI.Xaml.Thickness(10)
            };
            _numbersCheck = new CheckBox
            {
                Content = "Include numbers (0-9)",
                IsChecked = true,
                Width = 350,
                //HorizontalAlignment = HorizontalAlignment.Center
            };

            _symbolsCheck = new CheckBox
            {
                Content = "Include symbols (!@#$%^&*...)",
                IsChecked = false,
                Width = 350,
                //HorizontalAlignment = HorizontalAlignment.Center
            };
            stack2.Children.Add(_numbersCheck);
            stack2.Children.Add(_symbolsCheck);

            var stack3 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Padding = new Microsoft.UI.Xaml.Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var _lengthLabel = new TextBlock
            {
                Text = "Token Length: 64",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            _lengthSlider = new Slider
            {
                Minimum = 1,
                Maximum = 512,
                Value = 64,
                Width = 350,
                TickFrequency = 1,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _lengthSlider.ValueChanged += (s, e) =>
            {
                _lengthLabel.Text = $"Token Length: {(int)_lengthSlider.Value}";
                OnGenerateTokenClicked(s, e);
            };
            stack3.Children.Add(_lengthLabel);
            stack3.Children.Add(_lengthSlider);

            _outputBlock = new TextBlock
            {
                Text = "Token will appear here...",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
            };
            var copyButton = new Button
            {
                Content = "Copy to Clipboard",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            copyButton.Click += OnCopyToClipboardClicked;
            var refreshButton = new Button
            {
                Content = "Refresh",
                Width = 170
            };
            refreshButton.Click += OnGenerateTokenClicked;

            buttonsPanel.Children.Add(copyButton);
            buttonsPanel.Children.Add(refreshButton);

            stack.Children.Add(stack1);
            stack.Children.Add(stack2);
            stack.Children.Add(stack3);
            stack.Children.Add(_outputBlock);
            stack.Children.Add(buttonsPanel);

            this.Content = stack;
        }

        private void OnGenerateTokenClicked(object sender, RoutedEventArgs e)
        {
            var length = (int)_lengthSlider.Value;
            if (length <= 0)
            {
                _outputBlock.Text = "Please enter a valid positive number for length";
                return;
            }

            bool includeLowercase = _lowercaseCheck.IsChecked == true;
            bool includeUppercase = _uppercaseCheck.IsChecked == true;
            bool includeNumbers = _numbersCheck.IsChecked == true;
            bool includeSymbols = _symbolsCheck.IsChecked == true;

            if (!includeLowercase && !includeUppercase && !includeNumbers && !includeSymbols)
            {
                _outputBlock.Text = "Please select at least one character type";
                return;
            }

            var token = _tool.GenerateToken(length, includeLowercase, includeUppercase,
                                           includeNumbers, includeSymbols);
            _outputBlock.Text = token;
        }

        private void OnCopyToClipboardClicked(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_outputBlock.Text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
