using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETACalculatorTool
{
    class ETACalculatorToolUI : UserControl
    {
        private readonly ETACalculatorTool _tool;

        private NumberBox _totalElementsBox;
        private NumberBox _consumedElementsBox;
        private NumberBox _timeSpanBox;
        private ComboBox _timeUnitComboBox;
        private DatePicker _startDatePicker;
        private TimePicker _startTimePicker;
        private TextBlock _totalDurationBlock;
        private TextBlock _endTimeBlock;
        private TextBlock _exampleBlock;

        public ETACalculatorToolUI(ETACalculatorTool tool)
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

            // Example text
            _exampleBlock = new TextBlock
            {
                Text = "With a concrete example, if you wash 5 plates in 3 minutes and you have 500 plates to wash, it will take you 5 hours to wash them all.",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 800,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(_exampleBlock);

            // Total Elements section
            var totalElementsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            var totalElementsLabel = new TextBlock
            {
                Text = "Amount of elements to consume:",
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Width = 300
            };

            _totalElementsBox = new NumberBox
            {
                Value = 100,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                Width = 200,
                Minimum = 1
            };

            totalElementsPanel.Children.Add(totalElementsLabel);
            totalElementsPanel.Children.Add(_totalElementsBox);
            stack.Children.Add(totalElementsPanel);

            // Start date and time section
            var startDateTimePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            var startDateTimeLabel = new TextBlock
            {
                Text = "The consume started at:",
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Width = 300
            };

            var dateTimePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            _startDatePicker = new DatePicker
            {
                Date = DateTime.Now,
                Width = 300
            };

            _startTimePicker = new TimePicker
            {
                Time = DateTime.Now.TimeOfDay,
                Width = 300
            };

            dateTimePanel.Children.Add(_startDatePicker);
            dateTimePanel.Children.Add(_startTimePicker);

            startDateTimePanel.Children.Add(startDateTimeLabel);
            startDateTimePanel.Children.Add(dateTimePanel);
            stack.Children.Add(startDateTimePanel);

            // Consumed elements section
            var consumedPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            var consumedElementsLabel = new TextBlock
            {
                Text = "Amount of unit consumed:",
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Width = 300
            };

            var consumedElementsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };

            _consumedElementsBox = new NumberBox
            {
                Value = 5,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                Width = 100,
                Minimum = 1
            };

            var inTextBlock = new TextBlock
            {
                Text = "in",
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Margin = new Microsoft.UI.Xaml.Thickness(5, 0, 5, 0)
            };

            _timeSpanBox = new NumberBox
            {
                Value = 3,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                Width = 100,
                Minimum = 1
            };

            _timeUnitComboBox = new ComboBox
            {
                Width = 120,
                ItemsSource = new string[] { "milliseconds", "seconds", "minutes", "hours", "days" },
                SelectedIndex = 2 // Default to minutes
            };

            consumedElementsPanel.Children.Add(_consumedElementsBox);
            consumedElementsPanel.Children.Add(inTextBlock);
            consumedElementsPanel.Children.Add(_timeSpanBox);
            consumedElementsPanel.Children.Add(_timeUnitComboBox);

            consumedPanel.Children.Add(consumedElementsLabel);
            consumedPanel.Children.Add(consumedElementsPanel);
            stack.Children.Add(consumedPanel);

            // Calculate Button
            var calculateButton = new Button
            {
                Content = "Calculate ETA",
                Width = 150,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 15, 0, 0)
            };
            calculateButton.Click += OnCalculateEtaClicked;
            stack.Children.Add(calculateButton);

            // Results section
            var resultsPanel = new StackPanel
            {
                Margin = new Microsoft.UI.Xaml.Thickness(0, 15, 0, 0),
                Padding = new Microsoft.UI.Xaml.Thickness(10),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray)
            };

            var totalDurationLabel = new TextBlock
            {
                Text = "Total duration:",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 5)
            };

            _totalDurationBlock = new TextBlock
            {
                Text = "-",
                Margin = new Microsoft.UI.Xaml.Thickness(10, 0, 0, 10)
            };

            var endTimeLabel = new TextBlock
            {
                Text = "It will end:",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 5)
            };

            _endTimeBlock = new TextBlock
            {
                Text = "-",
                Margin = new Microsoft.UI.Xaml.Thickness(10, 0, 0, 0)
            };

            resultsPanel.Children.Add(totalDurationLabel);
            resultsPanel.Children.Add(_totalDurationBlock);
            resultsPanel.Children.Add(endTimeLabel);
            resultsPanel.Children.Add(_endTimeBlock);

            stack.Children.Add(resultsPanel);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnCalculateEtaClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                int totalElements = (int)_totalElementsBox.Value;
                int consumedElements = (int)_consumedElementsBox.Value;
                int timeSpan = (int)_timeSpanBox.Value;
                string timeUnit = _timeUnitComboBox.SelectedItem.ToString();

                var result = _tool.CalculateETA(totalElements, consumedElements, timeSpan, timeUnit);

                _totalDurationBlock.Text = result.TotalDuration;
                _endTimeBlock.Text = result.EndTime;
            }
            catch (Exception ex)
            {
                _totalDurationBlock.Text = "Error calculating ETA";
                _endTimeBlock.Text = ex.Message;
            }
        }
    }
}
