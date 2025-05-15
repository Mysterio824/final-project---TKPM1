using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace LoremIpsumGeneratorTool
{
    class LoremIpsumGeneratorToolUI : UserControl
    {
        private readonly LoremIpsumGeneratorTool _tool;
        private Slider _paragraphsSlider;
        private Slider _sentencesSlider;
        private Slider _wordsSlider;
        private CheckBox _startWithLoremCheckBox;
        private CheckBox _asHtmlCheckBox;
        private TextBlock _outputTextBlock;
        private TextBox _outputTextBox;
        private Button _copyButton;
        private Button _refreshButton;

        public LoremIpsumGeneratorToolUI(LoremIpsumGeneratorTool tool)
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

            // Create sliders for configuration
            var paragraphsTextBlock = new TextBlock
            {
                Text = "Number of Paragraphs: 3",
                
            };
            _paragraphsSlider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = 3,
                StepFrequency = 1,
                Width = 350
            };
            _paragraphsSlider.ValueChanged += (s, e) => { paragraphsTextBlock.Text = $"Number of Paragraphs: {(int)e.NewValue}"; };

            var sentencesTextBlock = new TextBlock { Text = "Sentences per Paragraph: 5" };
            _sentencesSlider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                StepFrequency = 1,
                Width = 350
            };
            _sentencesSlider.ValueChanged += (s, e) => { sentencesTextBlock.Text = $"Sentences per Paragraph: {(int)e.NewValue}"; };

            var wordsTextBlock = new TextBlock { Text = "Words per Sentence: 8" };
            _wordsSlider = new Slider
            {
                Minimum = 3,
                Maximum = 15,
                Value = 8,
                StepFrequency = 1,
                Width = 350
            };
            _wordsSlider.ValueChanged += (s, e) => { wordsTextBlock.Text = $"Words per Sentence: {(int)e.NewValue}"; };

            // Create checkboxes for options
            _startWithLoremCheckBox = new CheckBox { Content = "Start with 'Lorem ipsum'", IsChecked = true };
            _asHtmlCheckBox = new CheckBox { Content = "Generate as HTML" };

            // Create buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };

            _refreshButton = new Button { Content = "Generate Text", Width = 170 };
            _refreshButton.Click += OnGenerateTextClicked;

            _copyButton = new Button { Content = "Copy to Clipboard", Width = 170 };
            _copyButton.Click += OnCopyToClipboardClicked;

            buttonPanel.Children.Add(_refreshButton);
            buttonPanel.Children.Add(_copyButton);

            // Create TextBox for output
            _outputTextBlock = new TextBlock
            {
                Text = "Generated Lorem Ipsum will appear here...",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 5, 0, 0)
            };

            _outputTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Height = 200,
                Width = 350,
                IsReadOnly = true
            };

            // Add all controls to the stack panel
            stack.Children.Add(paragraphsTextBlock);
            stack.Children.Add(_paragraphsSlider);
            stack.Children.Add(sentencesTextBlock);
            stack.Children.Add(_sentencesSlider);
            stack.Children.Add(wordsTextBlock);
            stack.Children.Add(_wordsSlider);
            stack.Children.Add(_startWithLoremCheckBox);
            stack.Children.Add(_asHtmlCheckBox);
            stack.Children.Add(buttonPanel);
            stack.Children.Add(_outputTextBlock);
            stack.Children.Add(_outputTextBox);

            // Set the content of UserControl
            this.Content = stack;

            // Generate initial text
            GenerateLoremIpsum();
        }

        private void OnGenerateTextClicked(object sender, RoutedEventArgs e)
        {
            GenerateLoremIpsum();
        }

        private void OnCopyToClipboardClicked(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_outputTextBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void GenerateLoremIpsum()
        {
            int paragraphs = (int)_paragraphsSlider.Value;
            int sentencesPerParagraph = (int)_sentencesSlider.Value;
            int wordsPerSentence = (int)_wordsSlider.Value;
            bool startWithLorem = _startWithLoremCheckBox.IsChecked ?? true;
            bool asHtml = _asHtmlCheckBox.IsChecked ?? false;

            var loremText = _tool.GenerateLoremIpsum(
                paragraphs,
                sentencesPerParagraph,
                wordsPerSentence,
                startWithLorem,
                asHtml
            );

            _outputTextBox.Text = loremText;
        }
    }
}
