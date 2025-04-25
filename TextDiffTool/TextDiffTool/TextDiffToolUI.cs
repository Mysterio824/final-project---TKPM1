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
    class TextDiffToolUI : UserControl
    {
        private readonly TextDiffTool _tool;
        private TextBox _originalTextBox;
        private TextBox _modifiedTextBox;
        private TextBlock _diffResultBlock;

        public TextDiffToolUI(TextDiffTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Create main layout
            var grid = new Grid();

            // Define rows
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Create and configure TextBox for original text
            _originalTextBox = new TextBox
            {
                Header = "Original Text",
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Create and configure TextBox for modified text
            _modifiedTextBox = new TextBox
            {
                Header = "Modified Text",
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Create and configure Button for comparing texts
            var compareButton = new Button
            {
                Content = "Compare Texts",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 10)
            };
            compareButton.Click += OnCompareButtonClicked;

            // Create and configure TextBlock for diff results
            _diffResultBlock = new TextBlock
            {
                Text = "Differences will appear here...",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(20),
                FontFamily = new FontFamily("Consolas, Courier New, monospace")
            };

            // Add scrolling capability to the diff results
            var scrollViewer = new ScrollViewer
            {
                Content = _diffResultBlock,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            // Add controls to Grid
            Grid.SetRow(_originalTextBox, 0);
            Grid.SetRow(_modifiedTextBox, 1);
            Grid.SetRow(compareButton, 2);
            Grid.SetRow(scrollViewer, 3);

            grid.Children.Add(_originalTextBox);
            grid.Children.Add(_modifiedTextBox);
            grid.Children.Add(compareButton);
            grid.Children.Add(scrollViewer);

            // Set the content of UserControl
            this.Content = grid;
        }

        private void OnCompareButtonClicked(object sender, RoutedEventArgs e)
        {
            var originalText = _originalTextBox.Text ?? "";
            var modifiedText = _modifiedTextBox.Text ?? "";

            // Generate the diff using the tool's method
            var diffResult = _tool.ComputeDiff(originalText, modifiedText);

            // Display the diff result
            _diffResultBlock.Text = diffResult;
        }
    }
}
