using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacAddressLookupTool
{
    class MacAddressLookupToolUI : UserControl
    {
        private readonly MacAddressLookupTool _tool;
        private TextBox _macAddressBox;
        private Button _clearButton;
        private TextBlock _validationMessage;
        private TextBlock _vendorInfoBlock;
        private Button _copyButton;
        private SolidColorBrush _errorBrush;
        private SolidColorBrush _defaultBrush;

        public MacAddressLookupToolUI(MacAddressLookupTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Create color brushes
            _errorBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            _defaultBrush = new SolidColorBrush(Microsoft.UI.Colors.Black);

            // Creating StackPanel to hold other controls
            var stack = new StackPanel
            {
                Spacing = 10,
                Padding = new Microsoft.UI.Xaml.Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // MAC Address input section
            var inputPanel = new Grid();
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Create and configure TextBox (Input)
            _macAddressBox = new TextBox
            {
                PlaceholderText = "Enter MAC address (e.g., 20:37:06:12:34:56)",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };
            _macAddressBox.TextChanged += OnMacAddressTextChanged;
            Grid.SetColumn(_macAddressBox, 0);
            inputPanel.Children.Add(_macAddressBox);

            // Clear button
            _clearButton = new Button
            {
                Content = "✕",
                Width = 32,
                Height = 32,
                Margin = new Microsoft.UI.Xaml.Thickness(5, 5, 0, 0)
            };
            _clearButton.Click += OnClearButtonClicked;
            Grid.SetColumn(_clearButton, 1);
            inputPanel.Children.Add(_clearButton);

            // Validation message
            _validationMessage = new TextBlock
            {
                Foreground = _errorBrush,
                Visibility = Visibility.Collapsed,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            // Vendor info section
            _vendorInfoBlock = new TextBlock
            {
                Text = "Enter a MAC address to see vendor information",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 15, 0, 0)
            };

            // Copy button
            _copyButton = new Button
            {
                Content = "Copy Vendor Info",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
                IsEnabled = false
            };
            _copyButton.Click += OnCopyButtonClicked;

            // Add controls to StackPanel
            stack.Children.Add(inputPanel);
            stack.Children.Add(_validationMessage);
            stack.Children.Add(_vendorInfoBlock);
            stack.Children.Add(_copyButton);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnMacAddressTextChanged(object sender, TextChangedEventArgs e)
        {
            string macAddress = _macAddressBox.Text;

            if (string.IsNullOrEmpty(macAddress))
            {
                // Reset UI state
                _validationMessage.Visibility = Visibility.Collapsed;
                _macAddressBox.BorderBrush = null; // Default border
                _vendorInfoBlock.Text = "Enter a MAC address to see vendor information";
                _copyButton.IsEnabled = false;
                return;
            }

            if (_tool.IsValidMacAddress(macAddress))
            {
                // Valid MAC address
                _validationMessage.Visibility = Visibility.Collapsed;
                _macAddressBox.BorderBrush = null; // Default border

                // Look up vendor info
                string vendorInfo = _tool.LookupVendor(macAddress);
                _vendorInfoBlock.Text = vendorInfo;

                // Enable/disable copy button based on whether vendor is known
                _copyButton.IsEnabled = !vendorInfo.Contains("Unknown vendor");
            }
            else
            {
                // Invalid MAC address
                _validationMessage.Text = "Invalid MAC address";
                _validationMessage.Visibility = Visibility.Visible;
                _macAddressBox.BorderBrush = _errorBrush;
                _vendorInfoBlock.Text = "Enter a valid MAC address to see vendor information";
                _copyButton.IsEnabled = false;
            }
        }

        private void OnClearButtonClicked(object sender, RoutedEventArgs e)
        {
            _macAddressBox.Text = string.Empty;
            _validationMessage.Visibility = Visibility.Collapsed;
            _macAddressBox.BorderBrush = null; // Default border
            _vendorInfoBlock.Text = "Enter a MAC address to see vendor information";
            _copyButton.IsEnabled = false;
        }

        private void OnCopyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_vendorInfoBlock.Text) && _copyButton.IsEnabled)
            {
                try
                {
                    Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dataPackage.SetText(_vendorInfoBlock.Text);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
