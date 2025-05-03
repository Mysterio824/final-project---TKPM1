using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashGeneratorTool
{
    class HashGeneratorToolUI : UserControl
    {
        private readonly HashGeneratorTool _tool;
        private TextBox _inputBox;
        private ComboBox _hashTypeComboBox;
        private TextBlock _outputBlock;

        public HashGeneratorToolUI(HashGeneratorTool tool)
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
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            // Create and configure TextBox (Input)
            _inputBox = new TextBox
            {
                Header = "Input Text",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            _hashTypeComboBox = new ComboBox
            {
                Header = "Hash Algorithm",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
                ItemsSource = new string[] { "MD5", "SHA1", "SHA256", "SHA224", "SHA512", "SHA384", "SHA3", "RIPEMD160" },
                SelectedIndex = 0 // Default to MD5
            };

            // Create and configure Button (Generate Hash)
            var generateHashButton = new Button
            {
                Content = "Generate Hash",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            // Event handler for button click
            generateHashButton.Click += OnGenerateHashClicked;

            // Create and configure TextBlock (Output)
            _outputBlock = new TextBlock
            {
                Text = "Hash will appear here...",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            // Add controls to StackPanel
            stack.Children.Add(_inputBox);
            stack.Children.Add(_hashTypeComboBox);
            stack.Children.Add(generateHashButton);
            stack.Children.Add(_outputBlock);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnGenerateHashClicked(object sender, RoutedEventArgs e)
        {
            // Get the input text from the TextBox
            var inputText = _inputBox.Text;
            var selectedHashType = _hashTypeComboBox.SelectedItem.ToString();

            // Generate the hash using the tool's method
            var hashResult = _tool.GenerateHash(inputText, selectedHashType);

            // Display the hash result in the TextBlock
            _outputBlock.Text = hashResult ?? "Error generating hash";
        }
    }
}