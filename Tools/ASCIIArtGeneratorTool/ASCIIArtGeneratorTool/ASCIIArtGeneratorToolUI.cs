using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCIIArtGeneratorTool
{
    class ASCIIArtGeneratorToolUI : UserControl
    {
        private readonly ASCIIArtGeneratorTool _tool;
        private TextBox _inputBox;
        private ComboBox _fontComboBox;
        private Slider _widthSlider;
        private TextBlock _widthValueBlock;
        private TextBox _outputBox;

        public ASCIIArtGeneratorToolUI(ASCIIArtGeneratorTool tool)
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

            _inputBox = new TextBox
            {
                Header = "Input Text",
                PlaceholderText = "Type your text here",
                Width = 350,
                Height = 60,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            _fontComboBox = new ComboBox
            {
                Header = "Select Font",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
                ItemsSource = new string[] { "1Row", "3-D", "3x5", "5 Line Oblique" },
                SelectedIndex = 0
            };

            var widthPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            var widthLabel = new TextBlock
            {
                Text = "Width: ",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 0)
            };

            _widthSlider = new Slider
            {
                Minimum = 10,
                Maximum = 100,
                Value = 50,
                Width = 250,
                VerticalAlignment = VerticalAlignment.Center
            };

            _widthValueBlock = new TextBlock
            {
                Text = "50",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Microsoft.UI.Xaml.Thickness(10, 0, 0, 0)
            };

            _widthSlider.ValueChanged += OnWidthSliderValueChanged;

            widthPanel.Children.Add(widthLabel);
            widthPanel.Children.Add(_widthSlider);
            widthPanel.Children.Add(_widthValueBlock);

            var generateButton = new Button
            {
                Content = "Generate ASCII Art",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            generateButton.Click += OnGenerateArtClicked;

            _outputBox = new TextBox
            {
                Header = "ASCII Art Output",
                Width = 350,
                Height = 250,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas, Courier New, monospace"),
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            stack.Children.Add(_inputBox);
            stack.Children.Add(_fontComboBox);
            stack.Children.Add(widthPanel);
            stack.Children.Add(generateButton);
            stack.Children.Add(_outputBox);

            this.Content = stack;
        }

        private void OnWidthSliderValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            _widthValueBlock.Text = ((int)e.NewValue).ToString();
        }

        private void OnGenerateArtClicked(object sender, RoutedEventArgs e)
        {
            var inputText = _inputBox.Text ?? string.Empty;
            var selectedFont = _fontComboBox.SelectedItem?.ToString() ?? "1Row";
            int width = (int)_widthSlider.Value;

            var asciiArt = _tool.GenerateASCIIArt(inputText, selectedFont, width);

            _outputBox.Text = asciiArt;
        }
    }
}
