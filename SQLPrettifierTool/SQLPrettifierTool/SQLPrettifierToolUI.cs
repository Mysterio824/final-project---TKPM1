using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLPrettifierTool
{
    class SQLPrettifierToolUI : UserControl
    {
        private readonly SQLPrettifierTool _tool;
        private TextBox _inputBox;
        private ComboBox _dialectComboBox;
        private ComboBox _keywordCaseComboBox;
        private ComboBox _indentStyleComboBox;
        private TextBox _outputBox;

        public SQLPrettifierToolUI(SQLPrettifierTool tool)
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
                Padding = new Microsoft.UI.Xaml.Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Input section
            var inputLabel = new TextBlock
            {
                Text = "SQL Input",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 5)
            };

            _inputBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Height = 150,
                Width = 500,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
            };

            // Options section
            var optionsLabel = new TextBlock
            {
                Text = "Formatting Options",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 5)
            };

            var optionsGrid = new Grid();

            // Define columns
            optionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            optionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            optionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Define rows
            optionsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength() });
            optionsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength() });

            // Dialect selection
            var dialectLabel = new TextBlock
            {
                Text = "SQL Dialect",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 5)
            };
            Grid.SetRow(dialectLabel, 0);
            Grid.SetColumn(dialectLabel, 0);

            _dialectComboBox = new ComboBox
            {
                Width = 160,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 10),
                ItemsSource = new string[] {
                "Standard SQL",
                "GCP BigQuery",
                "IBM DB2",
                "Apache Hive",
                "MariaDB",
                "MySQL",
                "Couchbase N1QL",
                "Oracle PL/SQL",
                "PostgreSQL",
                "Amazon Redshift",
                "Spark",
                "SQLite",
                "SQL Server Transact-SQL"
            },
                SelectedIndex = 0
            };
            Grid.SetRow(_dialectComboBox, 1);
            Grid.SetColumn(_dialectComboBox, 0);

            // Keyword case selection
            var keywordCaseLabel = new TextBlock
            {
                Text = "Keyword Case",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 5)
            };
            Grid.SetRow(keywordCaseLabel, 0);
            Grid.SetColumn(keywordCaseLabel, 1);

            _keywordCaseComboBox = new ComboBox
            {
                Width = 160,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 10),
                ItemsSource = new string[] {
                "Uppercase",
                "lowercase",
                "preserve"
            },
                SelectedIndex = 0
            };
            Grid.SetRow(_keywordCaseComboBox, 1);
            Grid.SetColumn(_keywordCaseComboBox, 1);

            // Indent style selection
            var indentStyleLabel = new TextBlock
            {
                Text = "Indent Style",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 5)
            };
            Grid.SetRow(indentStyleLabel, 0);
            Grid.SetColumn(indentStyleLabel, 2);

            _indentStyleComboBox = new ComboBox
            {
                Width = 160,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 10, 10),
                ItemsSource = new string[] {
                "Standard",
                "tabular left",
                "tabular right"
            },
                SelectedIndex = 0
            };
            Grid.SetRow(_indentStyleComboBox, 1);
            Grid.SetColumn(_indentStyleComboBox, 2);

            // Add elements to options grid
            optionsGrid.Children.Add(dialectLabel);
            optionsGrid.Children.Add(_dialectComboBox);
            optionsGrid.Children.Add(keywordCaseLabel);
            optionsGrid.Children.Add(_keywordCaseComboBox);
            optionsGrid.Children.Add(indentStyleLabel);
            optionsGrid.Children.Add(_indentStyleComboBox);

            // Format button
            var formatButton = new Button
            {
                Content = "Format SQL",
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                Width = 200,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 10)
            };
            formatButton.Click += OnFormatButtonClicked;

            // Output section
            var outputLabel = new TextBlock
            {
                Text = "Formatted SQL",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 5)
            };

            _outputBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                IsReadOnly = true,
                Height = 200,
                Width = 500
            };

            // Add all components to main stack
            mainStack.Children.Add(inputLabel);
            mainStack.Children.Add(_inputBox);
            mainStack.Children.Add(optionsLabel);
            mainStack.Children.Add(optionsGrid);
            mainStack.Children.Add(formatButton);
            mainStack.Children.Add(outputLabel);
            mainStack.Children.Add(_outputBox);

            // Set the content of UserControl
            this.Content = mainStack;
        }

        private void OnFormatButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get input values
                var sqlInput = _inputBox.Text;
                var selectedDialect = _dialectComboBox.SelectedItem.ToString();
                var selectedKeywordCase = _keywordCaseComboBox.SelectedItem.ToString();
                var selectedIndentStyle = _indentStyleComboBox.SelectedItem.ToString();

                // Format the SQL
                var formattedSql = _tool.PrettifySql(sqlInput, selectedDialect, selectedKeywordCase, selectedIndentStyle);

                // Display the formatted SQL
                _outputBox.Text = formattedSql;
            }
            catch (Exception ex)
            {
                _outputBox.Text = $"Error formatting SQL: {ex.Message}";
            }
        }
    }
}
