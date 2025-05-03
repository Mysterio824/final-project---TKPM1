using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathEvaluatorTool
{
    class MathEvaluatorToolUI : UserControl
    {
        private readonly MathEvaluatorTool _tool;
        private TextBox _expressionBox;
        private TextBlock _resultBlock;

        public MathEvaluatorToolUI(MathEvaluatorTool tool)
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
                Padding = new Microsoft.UI.Xaml.Thickness(20)
            };

            // Create and configure TextBox for expression input
            _expressionBox = new TextBox
            {
                Header = "Math Expression",
                PlaceholderText = "Enter expression (e.g., 2+2 or sqrt(16))",
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };
            _expressionBox.TextChanged += OnExpressionTextChanged;

            // Create and configure TextBlock for result output
            _resultBlock = new TextBlock
            {
                Text = "Result will appear here...",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Width = 350,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 15, 0, 0)
            };

            // Add controls to StackPanel
            stack.Children.Add(_expressionBox);
            stack.Children.Add(_resultBlock);

            // Set the content of UserControl
            this.Content = stack;
        }

        private void OnExpressionTextChanged(object sender, TextChangedEventArgs e)
        {
            var expressionText = _expressionBox.Text;

            // Only evaluate if there's actual text
            if (!string.IsNullOrWhiteSpace(expressionText))
            {
                var result = _tool.EvaluateMathExpression(expressionText);
                _resultBlock.Text = result;
            }
            else
            {
                _resultBlock.Text = "Result will appear here...";
            }
        }
    }
}
