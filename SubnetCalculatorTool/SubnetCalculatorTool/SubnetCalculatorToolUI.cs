using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubnetCalculatorTool
{
    class SubnetCalculatorToolUI : UserControl
    {
        private readonly SubnetCalculatorTool _tool;
        private TextBox _inputBox;
        private TextBlock _errorBlock;
        private StackPanel _resultPanel;
        private SubnetInfo _currentSubnetInfo;

        // Result TextBlocks
        private TextBlock _networkAddressBlock;
        private TextBlock _networkMaskBlock;
        private TextBlock _networkMaskBinaryBlock;
        private TextBlock _cidrBlock;
        private TextBlock _wildcardMaskBlock;
        private TextBlock _networkSizeBlock;
        private TextBlock _firstAddressBlock;
        private TextBlock _lastAddressBlock;
        private TextBlock _broadcastAddressBlock;
        private TextBlock _ipClassBlock;

        public SubnetCalculatorToolUI(SubnetCalculatorTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Creating main StackPanel to hold other controls
            var mainStack = new StackPanel
            {
                Spacing = 10,
                Padding = new Thickness(20)
            };

            // Create the input section
            var inputStack = new StackPanel
            {
                Spacing = 5
            };

            // Create and configure TextBox (Input)
            _inputBox = new TextBox
            {
                Header = "Enter IPv4 Address (with or without CIDR notation)",
                PlaceholderText = "Example: 192.168.0.1/24",
                Width = 350
            };
            _inputBox.TextChanged += OnInputTextChanged;

            // Create error message block
            _errorBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Red),
                Width = 350,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };

            inputStack.Children.Add(_inputBox);
            inputStack.Children.Add(_errorBlock);

            // Create navigation buttons
            var navButtonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 10)
            };

            var prevBlockButton = new Button
            {
                Content = "Previous Block",
                Width = 170
            };
            prevBlockButton.Click += OnPreviousBlockClicked;

            var nextBlockButton = new Button
            {
                Content = "Next Block",
                Width = 170
            };
            nextBlockButton.Click += OnNextBlockClicked;

            navButtonsPanel.Children.Add(prevBlockButton);
            navButtonsPanel.Children.Add(nextBlockButton);

            // Create result section
            _resultPanel = new StackPanel
            {
                Spacing = 5,
                Visibility = Visibility.Collapsed
            };

            // Create result text blocks
            var resultHeaderBlock = new TextBlock
            {
                Text = "Subnet Information:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };

            _networkAddressBlock = CreateResultTextBlock("Network Address:");
            _networkMaskBlock = CreateResultTextBlock("Network Mask:");
            _networkMaskBinaryBlock = CreateResultTextBlock("Network Mask (Binary):");
            _cidrBlock = CreateResultTextBlock("CIDR Notation:");
            _wildcardMaskBlock = CreateResultTextBlock("Wildcard Mask:");
            _networkSizeBlock = CreateResultTextBlock("Network Size:");
            _firstAddressBlock = CreateResultTextBlock("First Usable Address:");
            _lastAddressBlock = CreateResultTextBlock("Last Usable Address:");
            _broadcastAddressBlock = CreateResultTextBlock("Broadcast Address:");
            _ipClassBlock = CreateResultTextBlock("IP Class:");

            _resultPanel.Children.Add(resultHeaderBlock);
            _resultPanel.Children.Add(_networkAddressBlock);
            _resultPanel.Children.Add(_networkMaskBlock);
            _resultPanel.Children.Add(_networkMaskBinaryBlock);
            _resultPanel.Children.Add(_cidrBlock);
            _resultPanel.Children.Add(_wildcardMaskBlock);
            _resultPanel.Children.Add(_networkSizeBlock);
            _resultPanel.Children.Add(_firstAddressBlock);
            _resultPanel.Children.Add(_lastAddressBlock);
            _resultPanel.Children.Add(_broadcastAddressBlock);
            _resultPanel.Children.Add(_ipClassBlock);

            // Add all sections to main stack
            mainStack.Children.Add(inputStack);
            mainStack.Children.Add(navButtonsPanel);
            mainStack.Children.Add(_resultPanel);

            // Set the content of UserControl
            this.Content = mainStack;
        }

        private TextBlock CreateResultTextBlock(string label)
        {
            return new TextBlock
            {
                Text = label,
                TextWrapping = TextWrapping.Wrap,
                Width = 350,
                Margin = new Thickness(0, 2, 0, 2)
            };
        }

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateAndDisplaySubnet(_inputBox.Text);
        }

        private void OnPreviousBlockClicked(object sender, RoutedEventArgs e)
        {
            if (_currentSubnetInfo != null && _currentSubnetInfo.IsValid)
            {
                var previousBlock = _tool.GetPreviousBlock(_currentSubnetInfo);
                DisplaySubnetInfo(previousBlock);
                _inputBox.Text = $"{previousBlock.NetworkAddress}/{previousBlock.Cidr}";
            }
        }

        private void OnNextBlockClicked(object sender, RoutedEventArgs e)
        {
            if (_currentSubnetInfo != null && _currentSubnetInfo.IsValid)
            {
                var nextBlock = _tool.GetNextBlock(_currentSubnetInfo);
                DisplaySubnetInfo(nextBlock);
                _inputBox.Text = $"{nextBlock.NetworkAddress}/{nextBlock.Cidr}";
            }
        }

        private void CalculateAndDisplaySubnet(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _errorBlock.Text = "Please enter an IPv4 address";
                _errorBlock.Visibility = Visibility.Visible;
                _resultPanel.Visibility = Visibility.Collapsed;
                return;
            }

            var subnetInfo = _tool.CalculateSubnet(input);
            DisplaySubnetInfo(subnetInfo);
        }

        private void DisplaySubnetInfo(SubnetInfo subnetInfo)
        {
            _currentSubnetInfo = subnetInfo;

            if (!subnetInfo.IsValid)
            {
                _errorBlock.Text = subnetInfo.ErrorMessage;
                _errorBlock.Visibility = Visibility.Visible;
                _resultPanel.Visibility = Visibility.Collapsed;
                return;
            }

            // Hide error and show results
            _errorBlock.Visibility = Visibility.Collapsed;
            _resultPanel.Visibility = Visibility.Visible;

            // Update result text blocks
            _networkAddressBlock.Text = $"Network Address: {subnetInfo.NetworkAddress}";
            _networkMaskBlock.Text = $"Network Mask: {subnetInfo.NetworkMask}";
            _networkMaskBinaryBlock.Text = $"Network Mask (Binary): {subnetInfo.NetworkMaskBinary}";
            _cidrBlock.Text = $"CIDR Notation: {subnetInfo.CidrNotation}";
            _wildcardMaskBlock.Text = $"Wildcard Mask: {subnetInfo.WildcardMask}";
            _networkSizeBlock.Text = $"Network Size: {subnetInfo.NetworkSize}";
            _firstAddressBlock.Text = $"First Usable Address: {subnetInfo.FirstAddress}";
            _lastAddressBlock.Text = $"Last Usable Address: {subnetInfo.LastAddress}";
            _broadcastAddressBlock.Text = $"Broadcast Address: {subnetInfo.BroadcastAddress}";
            _ipClassBlock.Text = $"IP Class: {subnetInfo.IpClass}";
        }
    }
}
