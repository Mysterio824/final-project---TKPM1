using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneParserTool
{
    class PhoneParserToolUI : UserControl
    {
        private readonly PhoneParserTool _tool;
        private TextBox _phoneNumberInput;
        private ComboBox _countryCodeComboBox;
        private Button _parseButton;
        private Grid _resultsGrid;
        private PhoneParseResult _currentResult = new PhoneParseResult();

        // Country code data for ComboBox
        private readonly List<CountryCodeItem> _countryCodes = new List<CountryCodeItem>
    {
        new CountryCodeItem { CountryName = "United States", Code = "+1" },
        new CountryCodeItem { CountryName = "United Kingdom", Code = "+44" },
        new CountryCodeItem { CountryName = "Australia", Code = "+61" },
        new CountryCodeItem { CountryName = "Canada", Code = "+1" },
        new CountryCodeItem { CountryName = "China", Code = "+86" },
        new CountryCodeItem { CountryName = "France", Code = "+33" },
        new CountryCodeItem { CountryName = "Germany", Code = "+49" },
        new CountryCodeItem { CountryName = "India", Code = "+91" },
        new CountryCodeItem { CountryName = "Indonesia", Code = "+62" },
        new CountryCodeItem { CountryName = "Italy", Code = "+39" },
        new CountryCodeItem { CountryName = "Japan", Code = "+81" },
        new CountryCodeItem { CountryName = "Mexico", Code = "+52" },
        new CountryCodeItem { CountryName = "Russia", Code = "+7" },
        new CountryCodeItem { CountryName = "South Korea", Code = "+82" },
        new CountryCodeItem { CountryName = "Spain", Code = "+34" },
        new CountryCodeItem { CountryName = "Thailand", Code = "+66" },
        new CountryCodeItem { CountryName = "Turkey", Code = "+90" },
        new CountryCodeItem { CountryName = "Vietnam", Code = "+84" },
        new CountryCodeItem { CountryName = "Brazil", Code = "+55" },
        new CountryCodeItem { CountryName = "Philippines", Code = "+63" },
        new CountryCodeItem { CountryName = "Malaysia", Code = "+60" },
        new CountryCodeItem { CountryName = "Singapore", Code = "+65" },
        new CountryCodeItem { CountryName = "Netherlands", Code = "+31" },
        new CountryCodeItem { CountryName = "Sweden", Code = "+46" },
        new CountryCodeItem { CountryName = "Switzerland", Code = "+41" },
        new CountryCodeItem { CountryName = "Belgium", Code = "+32" },
        new CountryCodeItem { CountryName = "Denmark", Code = "+45" },
        new CountryCodeItem { CountryName = "Finland", Code = "+358" },
        new CountryCodeItem { CountryName = "Norway", Code = "+47" },
        new CountryCodeItem { CountryName = "Poland", Code = "+48" },
        new CountryCodeItem { CountryName = "Austria", Code = "+43" },
        new CountryCodeItem { CountryName = "Greece", Code = "+30" },
        new CountryCodeItem { CountryName = "Hong Kong", Code = "+852" },
        new CountryCodeItem { CountryName = "Ireland", Code = "+353" },
        new CountryCodeItem { CountryName = "Israel", Code = "+972" },
        new CountryCodeItem { CountryName = "New Zealand", Code = "+64" },
        new CountryCodeItem { CountryName = "Portugal", Code = "+351" },
        new CountryCodeItem { CountryName = "Saudi Arabia", Code = "+966" },
        new CountryCodeItem { CountryName = "South Africa", Code = "+27" },
        new CountryCodeItem { CountryName = "United Arab Emirates", Code = "+971" }
        // More countries can be added here
    };

        public PhoneParserToolUI(PhoneParserTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main panel
            var mainPanel = new StackPanel
            {
                Spacing = 15,
                Padding = new Microsoft.UI.Xaml.Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Country code selection
            var countryCodeLabel = new TextBlock
            {
                Text = "Country",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 5)
            };

            _countryCodeComboBox = new ComboBox
            {
                Width = 350,
                ItemsSource = _countryCodes,
                DisplayMemberPath = "DisplayText",
                SelectedIndex = 0 // Default to first item
            };

            // Phone number input
            var phoneLabel = new TextBlock
            {
                Text = "Phone Number",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 5)
            };

            _phoneNumberInput = new TextBox
            {
                PlaceholderText = "Enter phone number",
                Width = 350
            };

            // Parse button
            _parseButton = new Button
            {
                Content = "Parse Phone Number",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 20)
            };
            _parseButton.Click += OnParseButtonClicked;

            var resultsLabel = new TextBlock
            {
                Text = "Results",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
            };

            // Grid for displaying results
            _resultsGrid = new Grid
            {
                Width = 350
            };

            // Set up the grid rows and columns
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto });

            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(140) });
            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });

            AddResultRow("Country (Short):", "", 0);
            AddResultRow("Country (Full):", "", 1);
            AddResultRow("Country Calling Code:", "", 2);
            AddResultRow("Is Valid:", "", 3);
            AddResultRow("Is Possible:", "", 4);
            AddResultRow("Type:", "", 5);
            AddResultRow("International Format:", "", 6);
            AddResultRow("National Format:", "", 7);
            AddResultRow("E.164 Format:", "", 8);
            AddResultRow("RFC3966 Format:", "", 9);

            mainPanel.Children.Add(countryCodeLabel);
            mainPanel.Children.Add(_countryCodeComboBox);
            mainPanel.Children.Add(phoneLabel);
            mainPanel.Children.Add(_phoneNumberInput);
            mainPanel.Children.Add(_parseButton);
            mainPanel.Children.Add(resultsLabel);
            mainPanel.Children.Add(_resultsGrid);

            Content = mainPanel;
        }

        private void AddResultRow(string label, string value, int row)
        {
            var labelBlock = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 5, 5)
            };

            var valueBlock = new TextBlock
            {
                Text = value,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 5, 0, 5)
            };

            // Add the tooltip and hover functionality
            ToolTipService.SetToolTip(valueBlock, "Click to copy");

            // Add the tap/click handler for copying
            valueBlock.PointerEntered += (s, e) => valueBlock.Opacity = 0.7;
            valueBlock.PointerExited += (s, e) => valueBlock.Opacity = 1.0;
            valueBlock.Tapped += (s, e) => CopyToClipboard(valueBlock.Text);

            Grid.SetRow(labelBlock, row);
            Grid.SetColumn(labelBlock, 0);

            Grid.SetRow(valueBlock, row);
            Grid.SetColumn(valueBlock, 1);

            _resultsGrid.Children.Add(labelBlock);
            _resultsGrid.Children.Add(valueBlock);
        }

        private void UpdateResultRow(string value, int row)
        {
            // Find the value TextBlock at this row
            foreach (var child in _resultsGrid.Children)
            {
                if (child is TextBlock textBlock && Grid.GetRow(textBlock) == row && Grid.GetColumn(textBlock) == 1)
                {
                    textBlock.Text = value;
                    break;
                }
            }
        }

        private void UpdateResults(PhoneParseResult result)
        {
            _currentResult = result;

            UpdateResultRow(result.CountryShort, 0);
            UpdateResultRow(result.CountryFull, 1);
            UpdateResultRow(result.CountryCallingCode, 2);
            UpdateResultRow(result.IsValid, 3);
            UpdateResultRow(result.IsPossible, 4);
            UpdateResultRow(result.Type, 5);
            UpdateResultRow(result.InternationalFormat, 6);
            UpdateResultRow(result.NationalFormat, 7);
            UpdateResultRow(result.E164Format, 8);
            UpdateResultRow(result.RFC3966Format, 9);
        }

        private void OnParseButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedCountryCode = (_countryCodeComboBox.SelectedItem as CountryCodeItem)?.Code ?? "+1";
            var phoneNumber = _phoneNumberInput.Text;

            var input = new PhoneParseInput
            {
                PhoneNumber = phoneNumber,
                CountryCode = selectedCountryCode
            };

            var result = _tool.Execute(input) as PhoneParseResult;
            if (result != null)
            {
                UpdateResults(result);
            }
        }

        private void CopyToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
        }
    }

    // Helper class for country code items
    public class CountryCodeItem
    {
        public string CountryName { get; set; }
        public string Code { get; set; }

        public string DisplayText => $"{CountryName} ({Code})";
    }
}
