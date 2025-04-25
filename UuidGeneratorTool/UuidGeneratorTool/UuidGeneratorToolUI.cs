using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace UuidGeneratorTool
{
    class UuidGeneratorToolUI : UserControl
    {
        private readonly UuidGeneratorTool _tool;
        private ComboBox _versionComboBox;
        private NumberBox _quantityBox;
        private TextBox _namespaceBox;
        private TextBox _nameBox;
        private TextBox _outputBox;
        private Button _copyButton;
        private Button _refreshButton;

        public UuidGeneratorToolUI(UuidGeneratorTool tool)
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
                Spacing = 30,
                HorizontalAlignment = HorizontalAlignment.Center                
            };
            _versionComboBox = new ComboBox
            {
                Header = "UUID Version",
                Width = 100,
                ItemsSource = new string[] { "NIL", "v1", "v3", "v4", "v5" },
                SelectedIndex = 3
            };
            _versionComboBox.SelectionChanged += OnVersionSelectionChanged;

            _quantityBox = new NumberBox
            {
                Header = "Quantity",
                Width = 100,
                Value = 1,
                Minimum = 1,
                Maximum = 100,
                SpinButtonPlacementMode = Microsoft.UI.Xaml.Controls.NumberBoxSpinButtonPlacementMode.Inline
            };
            stack1.Children.Add(_versionComboBox);
            stack1.Children.Add(_quantityBox);

            _namespaceBox = new TextBox
            {
                Header = "Namespace (for v3/v5)",
                PlaceholderText = "dns, url, oid, x500, or custom namespace",
                Width = 350,
                IsEnabled = false
            };

            _nameBox = new TextBox
            {
                Header = "Name (for v3/v5)",
                PlaceholderText = "Enter name to hash",
                Width = 350,
                IsEnabled = false
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _refreshButton = new Button
            {
                Content = "Generate UUIDs",
                Width = 170
            };
            _refreshButton.Click += OnGenerateUuidsClicked;

            _copyButton = new Button
            {
                Content = "Copy to Clipboard",
                Width = 170
            };
            _copyButton.Click += OnCopyClicked;

            buttonPanel.Children.Add(_refreshButton);
            buttonPanel.Children.Add(_copyButton);

            _outputBox = new TextBox
            {
                Header = "Generated UUIDs",
                Width = 350,
                Height = 200,
                AcceptsReturn = true,
                IsReadOnly = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };

            stack.Children.Add(stack1);
            stack.Children.Add(_namespaceBox);
            stack.Children.Add(_nameBox);
            stack.Children.Add(buttonPanel);
            stack.Children.Add(_outputBox);

            this.Content = stack;

            GenerateUuids();
        }

        private void OnVersionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedVersion = _versionComboBox.SelectedItem.ToString();
            bool isNameBased = selectedVersion == "v3" || selectedVersion == "v5";

            _namespaceBox.IsEnabled = isNameBased;
            _nameBox.IsEnabled = isNameBased;
        }

        private void OnGenerateUuidsClicked(object sender, RoutedEventArgs e)
        {
            GenerateUuids();
        }

        private void OnCopyClicked(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_outputBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void GenerateUuids()
        {
            try
            {
                string version = _versionComboBox.SelectedItem.ToString();
                int quantity = (int)_quantityBox.Value;
                string namespaceName = _namespaceBox.Text;
                string name = _nameBox.Text;

                List<string> uuids = _tool.GenerateUuids(version, quantity, namespaceName, name);
                _outputBox.Text = string.Join(Environment.NewLine, uuids);
            }
            catch (Exception ex)
            {
                _outputBox.Text = $"Error: {ex.Message}";
            }
        }
    }
}
