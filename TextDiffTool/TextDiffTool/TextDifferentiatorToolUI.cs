using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextDiffTool
{
    class TextDifferentiatorToolUI : UserControl
    {
        private readonly TextDifferentiatorTool _tool;
        private TextBox _textBoxA;
        private TextBox _textBoxB;
        private ComboBox _diffTypeComboBox;
        private TextBlock _outputBlock;
        private ScrollViewer _outputScrollViewer;

        public TextDifferentiatorToolUI(TextDifferentiatorTool tool)
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
                Padding = new Microsoft.UI.Xaml.Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Text A input
            _textBoxA = new TextBox
            {
                Header = "Text A",
                Width = 350,
                Height = 100,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            // Text B input
            _textBoxB = new TextBox
            {
                Header = "Text B",
                Width = 350,
                Height = 100,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            // Diff type selection
            _diffTypeComboBox = new ComboBox
            {
                Header = "Difference Type",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
                ItemsSource = new string[] { "Character Differences", "Word Differences", "Line Differences" },
                SelectedIndex = 0 // Default
            };

            // Compare button
            var compareButton = new Button
            {
                Content = "Compare Texts",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            compareButton.Click += OnCompareClicked;

            // Output area with scroll viewer
            _outputBlock = new TextBlock
            {
                Text = "Comparison results will appear here...",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350
            };

            _outputScrollViewer = new ScrollViewer
            {
                Content = _outputBlock,
                Width = 350,
                Height = 200,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            // Add controls to StackPanel
            stack.Children.Add(_textBoxA);
            stack.Children.Add(_textBoxB);
            stack.Children.Add(_diffTypeComboBox);
            stack.Children.Add(compareButton);
            stack.Children.Add(_outputScrollViewer);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnCompareClicked(object sender, RoutedEventArgs e)
        {
            // Get inputs
            var textA = _textBoxA.Text ?? string.Empty;
            var textB = _textBoxB.Text ?? string.Empty;
            var diffType = _diffTypeComboBox.SelectedItem.ToString();

            try
            {
                // Generate the diff using the tool's method
                var diffResult = _tool.DifferentiateText(textA, textB, diffType);

                // Display the results
                _outputBlock.Text = diffResult;
            }
            catch (Exception ex)
            {
                _outputBlock.Text = $"Error comparing texts: {ex.Message}";
            }
        }
    }
}
