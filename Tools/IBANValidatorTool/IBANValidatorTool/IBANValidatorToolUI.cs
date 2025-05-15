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
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;

namespace IBANValidatorTool
{
    class IBANValidatorToolUI : UserControl
    {
        private readonly IBANValidatorTool _tool;
        private TextBox _ibanInput;
        private Button _validateButton;
        private TextBlock _validityResult;
        private TextBlock _qrIbanResult;
        private TextBlock _countryCodeResult;
        private TextBlock _bbanResult;
        private TextBlock _formattedIbanResult;
        private readonly string[] _exampleIbans = {
        "FR7630006000011234567890189", // French IBAN
        "DE89370400440532013000",      // German IBAN
        "CH9300762011623852957"        // Swiss IBAN
    };

        public IBANValidatorToolUI(IBANValidatorTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            try
            {
                // Main container
                var mainPanel = new StackPanel
                {
                    Spacing = 12,
                    Padding = new Microsoft.UI.Xaml.Thickness(20)
                };

                // Input section
                var inputSection = new StackPanel
                {
                    Spacing = 10,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // IBAN Input
                _ibanInput = new TextBox
                {
                    Header = "Enter IBAN",
                    PlaceholderText = "e.g. FR76 3000 6000 0112 3456 7890 189",
                    Width = 400
                };

                // Validate button
                _validateButton = new Button
                {
                    Content = "Validate IBAN",
                    Width = 200,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                _validateButton.Click += OnValidateButtonClicked;

                // Add elements to input section
                inputSection.Children.Add(_ibanInput);
                inputSection.Children.Add(_validateButton);

                // Results section
                var resultsSection = new StackPanel
                {
                    Spacing = 8,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
                    Padding = new Microsoft.UI.Xaml.Thickness(15),
                    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    CornerRadius = new CornerRadius(5)
                };

                // Result fields - make sure to init each one
                _validityResult = new TextBlock();
                _qrIbanResult = new TextBlock();
                _countryCodeResult = new TextBlock();
                _bbanResult = new TextBlock();
                _formattedIbanResult = new TextBlock();

                // Create the result section layout
                var resultsHeader = new TextBlock
                {
                    Text = "Results",
                    FontWeight = FontWeights.Bold,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 5)
                };

                resultsSection.Children.Add(resultsHeader);

                // Add each result row with proper layout
                resultsSection.Children.Add(CreateResultRow("Is IBAN valid:", _validityResult));
                resultsSection.Children.Add(CreateResultRow("Is QR-IBAN:", _qrIbanResult));
                resultsSection.Children.Add(CreateResultRow("Country code:", _countryCodeResult));
                resultsSection.Children.Add(CreateResultRow("BBAN:", _bbanResult));
                resultsSection.Children.Add(CreateResultRow("IBAN friendly format:", _formattedIbanResult));

                // Reset all result fields to default state
                ResetResultFields();

                // Examples section
                var examplesSection = new StackPanel
                {
                    Spacing = 10,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 20, 0, 0)
                };

                examplesSection.Children.Add(new TextBlock
                {
                    Text = "Example IBANs",
                    FontWeight = FontWeights.Bold
                });

                foreach (var example in _exampleIbans)
                {
                    var examplePanel = new Grid();

                    // Define columns
                    examplePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    examplePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // IBAN example text
                    var exampleText = new TextBlock
                    {
                        Text = FormatIbanExample(example),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(exampleText, 0);

                    // Copy button
                    var copyButton = new Button
                    {
                        Content = "Copy",
                        Padding = new Microsoft.UI.Xaml.Thickness(8, 4, 8, 4)
                    };
                    copyButton.Tag = example;
                    copyButton.Click += OnCopyButtonClicked;
                    Grid.SetColumn(copyButton, 1);

                    examplePanel.Children.Add(exampleText);
                    examplePanel.Children.Add(copyButton);
                    examplesSection.Children.Add(examplePanel);
                }

                // Add all sections to main panel
                mainPanel.Children.Add(inputSection);
                mainPanel.Children.Add(resultsSection);
                mainPanel.Children.Add(examplesSection);

                // Set the content of UserControl
                this.Content = mainPanel;
            }
            catch (Exception ex)
            {
                // Handle exception - at minimum display it somewhere to help with debugging
                var errorPanel = new StackPanel
                {
                    Padding = new Microsoft.UI.Xaml.Thickness(20)
                };

                errorPanel.Children.Add(new TextBlock
                {
                    Text = "Error initializing UI: " + ex.Message,
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });

                this.Content = errorPanel;
            }
        }

        // Helper method to create a result row with label and value
        private UIElement CreateResultRow(string label, TextBlock resultTextBlock)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };

            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Width = 150
            });

            panel.Children.Add(resultTextBlock);
            return panel;
        }

        // Reset all result fields to initial state
        private void ResetResultFields()
        {
            _validityResult.Text = "N/A";
            _qrIbanResult.Text = "N/A";
            _countryCodeResult.Text = "N/A";
            _bbanResult.Text = "N/A";
            _formattedIbanResult.Text = "N/A";
        }

        private TextBlock CreateResultTextBlock(string label)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };

            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Width = 150
            });

            var resultText = new TextBlock();
            panel.Children.Add(resultText);

            return resultText;
        }

        private void OnValidateButtonClicked(object sender, RoutedEventArgs e)
        {
            string iban = _ibanInput.Text;

            var result = _tool.ValidateIBAN(iban);

            // Update UI with results
            _validityResult.Text = result.IsValid ? "Yes" : "No";
            _validityResult.Foreground = result.IsValid
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);

            if (result.IsValid)
            {
                _qrIbanResult.Text = result.IsQRIBAN ? "Yes" : "No";
                _countryCodeResult.Text = result.CountryCode;
                _bbanResult.Text = result.BBAN;
                _formattedIbanResult.Text = result.FormattedIBAN;
            }
            else
            {
                _qrIbanResult.Text = "N/A";
                _countryCodeResult.Text = "N/A";
                _bbanResult.Text = "N/A";
                _formattedIbanResult.Text = "N/A";
            }
        }

        private async void OnCopyButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string iban)
                {
                    var dataPackage = new DataPackage();
                    dataPackage.SetText(iban);
                    Clipboard.SetContent(dataPackage);

                    // Optionally: Set the IBAN directly to the input field
                    _ibanInput.Text = iban;

                    // Visual feedback that copy worked
                    var originalContent = button.Content;
                    button.Content = "Copied!";

                    // Reset button text after a short delay
                    await Task.Delay(1500);
                    if (button != null)
                    {
                        button.Content = originalContent;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any clipboard-related exceptions
                if (sender is Button button)
                {
                    button.Content = "Copy failed";
                }

                // You could log the exception here
                Debug.WriteLine($"Copy operation failed: {ex.Message}");
            }
        }

        private string FormatIbanExample(string iban)
        {
            StringBuilder formattedIBAN = new StringBuilder();

            for (int i = 0; i < iban.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formattedIBAN.Append(' ');

                formattedIBAN.Append(iban[i]);
            }

            return formattedIBAN.ToString();
        }
    }
}
