using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace JsonToXmlTool
{
    class JsonToXmlToolUI : UserControl
    {
        private readonly JsonToXmlTool tool;
        private TextBox inputBox;
        private TextBox outputBox;

        public JsonToXmlToolUI(JsonToXmlTool tool)
        {
            this.tool = tool;
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

            // Create and configure TextBox for JSON input
            inputBox = new TextBox
            {
                Header = "JSON Input",
                Width = 450,
                Height = 150,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                PlaceholderText = "Enter JSON here...",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            // Create and configure Button (Convert)
            var convertButton = new Button
            {
                Content = "Convert to XML",
                Width = 450,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            convertButton.Click += OnConvertClicked;

            // Create and configure TextBox for XML output
            outputBox = new TextBox
            {
                Header = "XML Output",
                IsReadOnly = true,
                Width = 450,
                Height = 200,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            // Create copy button for XML output
            var copyButton = new Button
            {
                Content = "Copy XML",
                Width = 150,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };
            copyButton.Click += OnCopyClicked;

            // Add controls to StackPanel
            stack.Children.Add(inputBox);
            stack.Children.Add(convertButton);
            stack.Children.Add(outputBox);
            stack.Children.Add(copyButton);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnConvertClicked(object sender, RoutedEventArgs e)
        {
            // Get the input text from the TextBox
            var jsonText = inputBox.Text;

            // Convert JSON to XML using the tool's method
            var xmlResult = tool.ConvertJsonToXml(jsonText);

            // Display the XML result in the TextBox
            outputBox.Text = xmlResult ?? "Error converting JSON to XML";
        }

        private void OnCopyClicked(object sender, RoutedEventArgs e)
        {
            // Copy the XML to clipboard
            var dataPackage = new DataPackage();
            dataPackage.SetText(outputBox.Text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
