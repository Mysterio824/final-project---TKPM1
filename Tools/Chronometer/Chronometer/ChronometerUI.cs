using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronometer
{
    class ChronometerUI : UserControl
    {
        private readonly Chronometer tool;
        private TextBlock timeDisplay;

        public ChronometerUI(Chronometer tool)
        {
            this.tool = tool;
            InitializeUI();
            this.tool.PropertyChanged += Tool_PropertyChanged;
        }

        private void Tool_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Chronometer.CurrentTime))
            {
                timeDisplay.Text = tool.CurrentTime;
            }
        }

        private void InitializeUI()
        {
            // Creating StackPanel to hold other controls
            var stack = new StackPanel
            {
                Spacing = 10,
                Padding = new Microsoft.UI.Xaml.Thickness(20)
            };

            // Create time display card
            var card = new Border
            {
                Background = new SolidColorBrush(Colors.LightGray),
                CornerRadius = new CornerRadius(5),
                Padding = new Microsoft.UI.Xaml.Thickness(20, 15, 20, 15),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 20)
            };

            // Time display
            timeDisplay = new TextBlock
            {
                Text = "00:00.000",
                FontSize = 36,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            card.Child = timeDisplay;

            // Button panel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 20
            };

            // Create and configure Start Button
            var startButton = new Button
            {
                Content = "Start",
                Width = 160,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            startButton.Click += OnStartClicked;

            // Create and configure Reset Button
            var resetButton = new Button
            {
                Content = "Reset",
                Width = 160,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
            };
            resetButton.Click += OnResetClicked;

            // Add buttons to panel
            buttonPanel.Children.Add(startButton);
            buttonPanel.Children.Add(resetButton);

            // Add controls to main StackPanel
            stack.Children.Add(card);
            stack.Children.Add(buttonPanel);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnStartClicked(object sender, RoutedEventArgs e)
        {
            tool.Start();
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            tool.Reset();
        }
    }
}
