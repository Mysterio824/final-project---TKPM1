using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IPv4RangeExpanderTool
{
    class IPv4RangeExpanderToolUI : UserControl
    {
        private readonly IPv4RangeExpanderTool _tool;
        private TextBox _startIpBox;
        private TextBox _endIpBox;
        private Button _clearStartButton;
        private Button _clearEndButton;
        private TextBlock _startWarning;
        private TextBlock _endWarning;
        private Grid _resultGrid;

        public IPv4RangeExpanderToolUI(IPv4RangeExpanderTool tool)
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
                Padding = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create start IP input with header and warning
            var startIpPanel = new StackPanel { Spacing = 5 };
            var startHeaderPanel = new StackPanel { Orientation = Orientation.Horizontal };
            startHeaderPanel.Children.Add(new TextBlock { Text = "Start IP Address", Width = 150 });
            startIpPanel.Children.Add(startHeaderPanel);

            var startInputPanel = new Grid();
            startInputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            startInputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _startIpBox = new TextBox
            {
                PlaceholderText = "e.g. 192.168.1.1",
                Width = 350
            };
            _startIpBox.TextChanged += OnIpTextChanged;

            _clearStartButton = new Button
            {
                Content = "✕",
                Width = 40,
                Margin = new Thickness(5, 0, 0, 0)
            };
            _clearStartButton.Click += (s, e) => { _startIpBox.Text = ""; };

            Grid.SetColumn(_startIpBox, 0);
            Grid.SetColumn(_clearStartButton, 1);
            startInputPanel.Children.Add(_startIpBox);
            startInputPanel.Children.Add(_clearStartButton);
            startIpPanel.Children.Add(startInputPanel);

            _startWarning = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed
            };
            startIpPanel.Children.Add(_startWarning);
            stack.Children.Add(startIpPanel);

            // Create end IP input with header and warning
            var endIpPanel = new StackPanel { Spacing = 5 };
            var endHeaderPanel = new StackPanel { Orientation = Orientation.Horizontal };
            endHeaderPanel.Children.Add(new TextBlock { Text = "End IP Address", Width = 150 });
            endIpPanel.Children.Add(endHeaderPanel);

            var endInputPanel = new Grid();
            endInputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            endInputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _endIpBox = new TextBox
            {
                PlaceholderText = "e.g. 192.168.6.255",
                Width = 350
            };
            _endIpBox.TextChanged += OnIpTextChanged;

            _clearEndButton = new Button
            {
                Content = "✕",
                Width = 40,
                Margin = new Thickness(5, 0, 0, 0)
            };
            _clearEndButton.Click += (s, e) => { _endIpBox.Text = ""; };

            Grid.SetColumn(_endIpBox, 0);
            Grid.SetColumn(_clearEndButton, 1);
            endInputPanel.Children.Add(_endIpBox);
            endInputPanel.Children.Add(_clearEndButton);
            endIpPanel.Children.Add(endInputPanel);

            _endWarning = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed
            };
            endIpPanel.Children.Add(_endWarning);
            stack.Children.Add(endIpPanel);

            // Add result grid
            _resultGrid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = Visibility.Collapsed
            };

            // Define columns for the grid
            _resultGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _resultGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _resultGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Define rows for the grid
            for (int i = 0; i < 5; i++)
            {
                _resultGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Add headers
            AddGridText(_resultGrid, "", 0, 0);
            AddGridText(_resultGrid, "Original Value", 0, 1, true);
            AddGridText(_resultGrid, "New Value (Subnet)", 0, 2, true);

            // Add row headers
            AddGridText(_resultGrid, "Start Address", 1, 0, true);
            AddGridText(_resultGrid, "End Address", 2, 0, true);
            AddGridText(_resultGrid, "Addresses in Range", 3, 0, true);
            AddGridText(_resultGrid, "CIDR Notation", 4, 0, true);

            stack.Children.Add(_resultGrid);

            this.Content = stack;
        }

        private void AddGridText(Grid grid, string text, int row, int column, bool isHeader = false)
        {
            var block = new TextBlock
            {
                Text = text,
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap
            };

            if (isHeader)
            {
                block.FontWeight = FontWeights.Bold;
            }

            Grid.SetRow(block, row);
            Grid.SetColumn(block, column);
            grid.Children.Add(block);
        }

        private void UpdateGridText(Grid grid, string text, int row, int column)
        {
            foreach (var child in grid.Children)
            {
                if (child is TextBlock block &&
                    child is FrameworkElement fe &&
                    Grid.GetRow(fe) == row &&
                    Grid.GetColumn(fe) == column)
                {
                    block.Text = text;
                    return;
                }
            }

            // If we didn't find the TextBlock, add it
            AddGridText(grid, text, row, column);
        }

        private void OnIpTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var isStartIp = textBox == _startIpBox;
            var warningBlock = isStartIp ? _startWarning : _endWarning;

            // Validate IP address
            if (!string.IsNullOrWhiteSpace(textBox.Text) && !IPAddress.TryParse(textBox.Text, out _))
            {
                textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                warningBlock.Text = "Invalid IPv4 address";
                warningBlock.Visibility = Visibility.Visible;
            }
            else
            {
                textBox.ClearValue(Border.BorderBrushProperty);
                warningBlock.Visibility = Visibility.Collapsed;
            }

            // Try to calculate subnet if both IPs are valid
            CalculateAndDisplaySubnet();
        }

        private void CalculateAndDisplaySubnet()
        {
            // Clear previous results if any input is invalid
            if (string.IsNullOrWhiteSpace(_startIpBox.Text) ||
                string.IsNullOrWhiteSpace(_endIpBox.Text) ||
                !IPAddress.TryParse(_startIpBox.Text, out _) ||
                !IPAddress.TryParse(_endIpBox.Text, out _))
            {
                _resultGrid.Visibility = Visibility.Collapsed;
                return;
            }

            // Calculate subnet
            var result = _tool.CalculateSubnet(_startIpBox.Text, _endIpBox.Text);

            // Check if calculation was successful
            if (result == null)
            {
                _endWarning.Text = "End IP must be greater than Start IP";
                _endWarning.Visibility = Visibility.Visible;
                _endIpBox.BorderBrush = new SolidColorBrush(Colors.Red);
                _resultGrid.Visibility = Visibility.Collapsed;
                return;
            }

            // Clear any error states
            _endWarning.Visibility = Visibility.Collapsed;
            _endIpBox.ClearValue(Border.BorderBrushProperty);

            // Update result grid
            UpdateGridText(_resultGrid, result["StartAddress"], 1, 1);
            UpdateGridText(_resultGrid, result["NewStartAddress"], 1, 2);
            UpdateGridText(_resultGrid, result["EndAddress"], 2, 1);
            UpdateGridText(_resultGrid, result["NewEndAddress"], 2, 2);
            UpdateGridText(_resultGrid, result["AddressCount"], 3, 1);
            UpdateGridText(_resultGrid, result["NewAddressCount"], 3, 2);
            UpdateGridText(_resultGrid, "N/A", 4, 1);
            UpdateGridText(_resultGrid, result["CIDR"], 4, 2);

            // Show result grid
            _resultGrid.Visibility = Visibility.Visible;
        }
    }
}
