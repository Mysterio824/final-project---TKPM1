using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using static System.Net.Mime.MediaTypeNames;

namespace BenchmarkBuilderTool
{
    class BenchmarkBuilderToolUI : UserControl
    {
        private readonly BenchmarkBuilderTool _tool;
        private ScrollViewer _suitesScrollViewer;
        private StackPanel _suitesContainer;
        private Grid _resultsGrid;
        private TextBox _unitTextBox;

        public BenchmarkBuilderToolUI(BenchmarkBuilderTool tool)
        {
            _tool = tool;
            InitializeUI();
        }

        private void InitializeUI()
        {
            var mainStack = new StackPanel
            {
                Spacing = 15,
                Padding = new Microsoft.UI.Xaml.Thickness(20)
            };

            // Suites section
            var suitesHeaderPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
            };

            var suitesTitle = new TextBlock
            {
                Text = "Benchmark Suites",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };

            var addSuiteButton = new Button
            {
                Content = "Add Suite",
                Margin = new Microsoft.UI.Xaml.Thickness(15, 0, 0, 0)
            };
            addSuiteButton.Click += OnAddSuiteClicked;

            suitesHeaderPanel.Children.Add(suitesTitle);
            suitesHeaderPanel.Children.Add(addSuiteButton);

            // Suites container with horizontal scrolling
            _suitesContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            _suitesScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = _suitesContainer,
                Padding = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10),
                MaxHeight = 280
            };

            // Controls section
            var controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 10),
                Spacing = 15
            };

            var unitPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };

            var unitLabel = new TextBlock
            {
                Text = "Unit:",
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 5, 0)
            };

            _unitTextBox = new TextBox
            {
                Text = _tool.Unit,
                Width = 80
            };
            _unitTextBox.TextChanged += OnUnitTextChanged;

            unitPanel.Children.Add(unitLabel);
            unitPanel.Children.Add(_unitTextBox);

            var resetButton = new Button
            {
                Content = "Reset Suites"
            };
            resetButton.Click += OnResetSuitesClicked;

            controlsPanel.Children.Add(unitPanel);
            controlsPanel.Children.Add(resetButton);

            // Results section
            var resultsTitle = new TextBlock
            {
                Text = "Results",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 10)
            };

            _resultsGrid = new Grid
            {
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
            };

            // Export buttons
            var exportPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };

            var copyMdButton = new Button
            {
                Content = "Copy as Markdown Table"
            };
            copyMdButton.Click += OnCopyMarkdownClicked;

            var copyBulletButton = new Button
            {
                Content = "Copy as Bullet List"
            };
            copyBulletButton.Click += OnCopyBulletListClicked;

            exportPanel.Children.Add(copyMdButton);
            exportPanel.Children.Add(copyBulletButton);

            // Add all sections to main container
            mainStack.Children.Add(suitesHeaderPanel);
            mainStack.Children.Add(_suitesScrollViewer);
            mainStack.Children.Add(controlsPanel);
            mainStack.Children.Add(resultsTitle);
            mainStack.Children.Add(_resultsGrid);
            mainStack.Children.Add(exportPanel);

            // Populate suites and results
            UpdateSuiteCards();
            UpdateResultsTable();

            this.Content = mainStack;
        }

        private void UpdateSuiteCards()
        {
            _suitesContainer.Children.Clear();

            foreach (var suite in _tool.Suites)
            {
                _suitesContainer.Children.Add(CreateSuiteCard(suite));
            }
        }

        private UIElement CreateSuiteCard(BenchmarkSuite suite)
        {
            var card = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(5),
                Padding = new Microsoft.UI.Xaml.Thickness(10),
                Width = 280
            };

            var cardContent = new StackPanel
            {
                Spacing = 8
            };

            // Header with name and delete button
            var headerPanel = new Grid();
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameTextBox = new TextBox
            {
                Text = suite.Name,
                PlaceholderText = "Suite Name",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 5, 0)
            };
            nameTextBox.TextChanged += (s, e) => { suite.Name = nameTextBox.Text; UpdateResultsTable(); };
            Grid.SetColumn(nameTextBox, 0);

            var deleteButton = new Button
            {
                Content = "×",
                Padding = new Microsoft.UI.Xaml.Thickness(5, 0, 5, 0),
                FontSize = 16,
                MinWidth = 30
            };
            deleteButton.Click += (s, e) =>
            {
                _tool.RemoveSuite(suite);
                UpdateSuiteCards();
                UpdateResultsTable();
            };
            Grid.SetColumn(deleteButton, 1);

            headerPanel.Children.Add(nameTextBox);
            headerPanel.Children.Add(deleteButton);

            // Values section
            var valuesTitle = new TextBlock
            {
                Text = "Values:",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 2)
            };

            var valuesContainer = new StackPanel
            {
                Spacing = 5
            };

            // Populate existing values
            foreach (var value in suite.Values)
            {
                valuesContainer.Children.Add(CreateValueControl(suite, value));
            }

            // Add new value button
            var addValueButton = new Button
            {
                Content = "Add Measure",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };
            addValueButton.Click += (s, e) =>
            {
                suite.Values.Add(0);
                valuesContainer.Children.Add(CreateValueControl(suite, 0));
                UpdateResultsTable();
            };

            // Add all elements to card
            cardContent.Children.Add(headerPanel);
            cardContent.Children.Add(valuesTitle);
            cardContent.Children.Add(valuesContainer);
            cardContent.Children.Add(addValueButton);

            card.Child = cardContent;
            return card;
        }

        private UIElement CreateValueControl(BenchmarkSuite suite, double value)
        {
            var valuePanel = new Grid();
            valuePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            valuePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var valueBox = new TextBox
            {
                Text = value.ToString(),
                PlaceholderText = "Value",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 5, 0)
            };

            int valueIndex = suite.Values.IndexOf(value);
            valueBox.TextChanged += (s, e) =>
            {
                if (double.TryParse(valueBox.Text, out double newValue))
                {
                    if (valueIndex >= 0 && valueIndex < suite.Values.Count)
                    {
                        suite.Values[valueIndex] = newValue;
                        UpdateResultsTable();
                    }
                }
            };
            Grid.SetColumn(valueBox, 0);

            var deleteButton = new Button
            {
                Content = "×",
                Padding = new Microsoft.UI.Xaml.Thickness(5, 0, 5, 0),
                FontSize = 16,
                MinWidth = 30
            };
            deleteButton.Click += (s, e) =>
            {
                if (valueIndex >= 0 && valueIndex < suite.Values.Count)
                {
                    suite.Values.RemoveAt(valueIndex);

                    var parent = valuePanel.Parent as StackPanel;
                    if (parent != null)
                    {
                        parent.Children.Remove(valuePanel);
                    }

                    UpdateResultsTable();
                }
            };
            Grid.SetColumn(deleteButton, 1);

            valuePanel.Children.Add(valueBox);
            valuePanel.Children.Add(deleteButton);

            return valuePanel;
        }

        private void UpdateResultsTable()
        {
            _resultsGrid.Children.Clear();
            _resultsGrid.ColumnDefinitions.Clear();
            _resultsGrid.RowDefinitions.Clear();

            var results = _tool.CalculateResults();
            if (results.Count == 0) return;

            // Define columns
            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Position
            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Suite
            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Samples
            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Mean
            _resultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Variance

            // Add header row
            _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddHeaderCell("Position", 0, 0);
            AddHeaderCell("Suite", 0, 1);
            AddHeaderCell("Samples", 0, 2);
            AddHeaderCell($"Mean ({_tool.Unit})", 0, 3);
            AddHeaderCell("Variance", 0, 4);

            // Add data rows
            for (int i = 0; i < results.Count; i++)
            {
                _resultsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int rowIndex = i + 1;

                var result = results[i];

                AddCell(result.Position.ToString(), rowIndex, 0);
                AddCell(result.SuiteName, rowIndex, 1);
                AddCell(result.Samples.ToString(), rowIndex, 2);

                string meanDisplay = result.Mean.ToString("F2");
                if (result.Position > 1)
                {
                    var baseline = results[0];
                    double diff = result.Mean - baseline.Mean;
                    double ratio = result.Mean / (baseline.Mean == 0 ? 1 : baseline.Mean);
                    meanDisplay += $" (+{diff:F2} ; x{ratio:F2})";
                }
                AddCell(meanDisplay, rowIndex, 3);

                AddCell(result.Variance.ToString("F2"), rowIndex, 4);
            }
        }

        private void AddHeaderCell(string text, int row, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Padding = new Microsoft.UI.Xaml.Thickness(8),
            };

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            _resultsGrid.Children.Add(textBlock);
        }

        private void AddCell(string text, int row, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Padding = new Microsoft.UI.Xaml.Thickness(8)
            };

            //if (row % 2 == 0)
            //{
            //    textBlock.Background = new SolidColorBrush(Color.FromArgb(20, 200, 200, 200));
            //}

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            _resultsGrid.Children.Add(textBlock);
        }

        private void OnAddSuiteClicked(object sender, RoutedEventArgs e)
        {
            _tool.AddSuite();
            UpdateSuiteCards();
            UpdateResultsTable();
        }

        private void OnResetSuitesClicked(object sender, RoutedEventArgs e)
        {
            _tool.ResetSuites();
            UpdateSuiteCards();
            UpdateResultsTable();
        }

        private void OnUnitTextChanged(object sender, TextChangedEventArgs e)
        {
            _tool.Unit = _unitTextBox.Text;
            UpdateResultsTable();
        }

        private void OnCopyMarkdownClicked(object sender, RoutedEventArgs e)
        {
            string markdown = _tool.GenerateMarkdownTable();
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(markdown);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }

        private void OnCopyBulletListClicked(object sender, RoutedEventArgs e)
        {
            string bulletList = _tool.GenerateBulletList();
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(bulletList);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
    }
}
