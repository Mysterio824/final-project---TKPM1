using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls.Primitives;
using QRGeneratorTool.WinRT.Interop;
using System.Runtime.InteropServices;

namespace QRGeneratorTool
{
    public class QRGeneratorToolUI : UserControl
    {
        private readonly QRGeneratorTool _tool;
        private TextBox _inputBox;
        private ComboBox _errorCorrectionComboBox;
        private Button _fgColorButton;
        private Button _bgColorButton;
        private Image _qrCodeImage;
        private byte[] _currentQRCodeBytes;
        private Popup _popup;
        private System.Drawing.Color _foregroundColor = System.Drawing.Color.Black;
        private System.Drawing.Color _backgroundColor = System.Drawing.Color.White;
        private Button _downloadButton;

        public QRGeneratorToolUI(QRGeneratorTool tool)
        {
            _tool = tool;
            this.Loaded += QRGeneratorToolUI_Loaded;
        }

        private void QRGeneratorToolUI_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize UI after the control is loaded in the visual tree
            InitializeUI();
        }

        private void InitializeUI()
        {
            var stack = new StackPanel { Spacing = 10, Padding = new Thickness(20) };
            _popup = new Popup
            {
                IsOpen = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                XamlRoot = this.XamlRoot // Set XamlRoot when the control is loaded
            };

            _inputBox = new TextBox
            {
                Header = "Input Text or URL",
                Width = 350,
                Height = 100,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var colorGrid = new Grid();
            colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _fgColorButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                {
                    new Border
                    {
                        Width = 20,
                        Height = 20,
                        Background = new SolidColorBrush(Microsoft.UI.Colors.Black),
                        Margin = new Thickness(0, 0, 10, 0)
                    },
                    new TextBlock { Text = "Foreground: #000000" }
                }
                },
                Margin = new Thickness(0, 10, 5, 0)
            };
            _fgColorButton.Click += OnFgColorButtonClicked;

            _bgColorButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                {
                    new Border
                    {
                        Width = 20,
                        Height = 20,
                        Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                        BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(0, 0, 10, 0)
                    },
                    new TextBlock { Text = "Background: #FFFFFF" }
                }
                },
                Margin = new Thickness(5, 10, 0, 0)
            };
            _bgColorButton.Click += OnBgColorButtonClicked;

            Grid.SetColumn(_fgColorButton, 0);
            Grid.SetColumn(_bgColorButton, 1);
            colorGrid.Children.Add(_fgColorButton);
            colorGrid.Children.Add(_bgColorButton);

            _errorCorrectionComboBox = new ComboBox
            {
                Header = "Error Correction Level",
                Width = 350,
                Margin = new Thickness(0, 10, 0, 0),
                ItemsSource = new string[] { "Low", "Medium", "Quartile", "High" },
                SelectedIndex = 1 // Default to Medium
            };

            var generateQRButton = new Button
            {
                Content = "Generate QR Code",
                Width = 350,
                Margin = new Thickness(0, 10, 0, 0)
            };
            generateQRButton.Click += OnGenerateQRClicked;

            _qrCodeImage = new Image
            {
                Width = 300,
                Height = 300,
                Margin = new Thickness(0, 20, 0, 0),
                Stretch = Stretch.Uniform
            };

            _downloadButton = new Button
            {
                Content = "Download QR Code",
                Width = 350,
                Margin = new Thickness(0, 10, 0, 0),
                IsEnabled = false
            };
            _downloadButton.Click += OnDownloadQRClicked;

            stack.Children.Add(_inputBox);
            stack.Children.Add(colorGrid);
            stack.Children.Add(_errorCorrectionComboBox);
            stack.Children.Add(generateQRButton);
            stack.Children.Add(_qrCodeImage);
            stack.Children.Add(_downloadButton);

            this.Content = stack;
        }

        private void OnFgColorButtonClicked(object sender, RoutedEventArgs e)
        {
            // Make sure the popup has an XamlRoot
            _popup.XamlRoot = this.XamlRoot;
            if (_popup.XamlRoot == null)
            {
                ShowInlineMessage("Cannot open color picker: Control not properly loaded in UI");
                return;
            }

            var colorPicker = new ColorPicker
            {
                Color = ToWindowsColor(_foregroundColor),
                IsColorPreviewVisible = true,
                IsColorSliderVisible = true,
                IsColorChannelTextInputVisible = true,
                IsHexInputVisible = true,
                Width = 300,
                MinWidth = 300
            };

            var btn1 = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Margin = new Thickness(0, 10, 0, 0),
            };
            btn1.Click += (s, args) =>
            {
                _foregroundColor = ToDrawingColor(colorPicker.Color);
                UpdateColorButton(_fgColorButton, colorPicker.Color, "Foreground");
                _popup.IsOpen = false; // Close the popup
            };

            var btn2 = new Button
            {
                Content = "Cancel",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Margin = new Thickness(10, 10, 0, 0),
            };
            btn2.Click += (s, args) => _popup.IsOpen = false;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            stackPanel.Children.Add(btn1);
            stackPanel.Children.Add(btn2);

            var textBlock = new TextBlock
            {
                Text = "Select Foreground Color",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel2 = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            stackPanel2.Children.Add(textBlock);
            stackPanel2.Children.Add(colorPicker);
            stackPanel2.Children.Add(stackPanel);

            var border = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Width = 350,
                Height = 350
            };
            border.Child = stackPanel2;

            _popup.Child = border;
            _popup.IsOpen = true;
        }

        private void OnBgColorButtonClicked(object sender, RoutedEventArgs e)
        {
            // Make sure the popup has an XamlRoot
            _popup.XamlRoot = this.XamlRoot;
            if (_popup.XamlRoot == null)
            {
                ShowInlineMessage("Cannot open color picker: Control not properly loaded in UI");
                return;
            }

            var colorPicker = new ColorPicker
            {
                Color = ToWindowsColor(_backgroundColor),
                IsColorPreviewVisible = true,
                IsColorSliderVisible = true,
                IsColorChannelTextInputVisible = true,
                IsHexInputVisible = true,
                Width = 300,
                MinWidth = 300
            };

            var btn1 = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Margin = new Thickness(0, 10, 0, 0),
            };
            btn1.Click += (s, args) =>
            {
                _backgroundColor = ToDrawingColor(colorPicker.Color);
                UpdateColorButton(_bgColorButton, colorPicker.Color, "Background");
                _popup.IsOpen = false; // Close the popup
            };

            var btn2 = new Button
            {
                Content = "Cancel",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Margin = new Thickness(10, 10, 0, 0),
            };
            btn2.Click += (s, args) => _popup.IsOpen = false;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            stackPanel.Children.Add(btn1);
            stackPanel.Children.Add(btn2);

            var textBlock = new TextBlock
            {
                Text = "Select Background Color",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel2 = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            stackPanel2.Children.Add(textBlock);
            stackPanel2.Children.Add(colorPicker);
            stackPanel2.Children.Add(stackPanel);

            var border = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Width = 350,
                Height = 350
            };
            border.Child = stackPanel2;

            _popup.Child = border;
            _popup.IsOpen = true;
        }

        private void UpdateColorButton(Button button, Windows.UI.Color color, string type)
        {
            var hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            var stackPanel = (StackPanel)button.Content;
            var border = (Border)stackPanel.Children[0];
            var textBlock = (TextBlock)stackPanel.Children[1];

            border.Background = new SolidColorBrush(color);
            textBlock.Text = $"{type}: {hexColor}";
        }

        private void OnGenerateQRClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var inputText = _inputBox.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(inputText))
                {
                    ShowInlineMessage("Please enter some text or URL.");
                    return;
                }

                var errorLevel = _errorCorrectionComboBox.SelectedItem.ToString();
                var errorCorrectionLevel = _tool.GetErrorCorrectionLevel(errorLevel);

                // Generate QR code
                _currentQRCodeBytes = _tool.GenerateQRCode(inputText, _foregroundColor, _backgroundColor, errorCorrectionLevel);

                // Display the QR code
                var bitmapImage = new BitmapImage();
                using (var stream = new InMemoryRandomAccessStream())
                {
                    using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(_currentQRCodeBytes);
                        writer.StoreAsync().GetResults();
                    }
                    bitmapImage.SetSource(stream);
                }

                _qrCodeImage.Source = bitmapImage;
                _downloadButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ShowInlineMessage($"Error generating QR code: {ex.Message}");
            }
        }

        private async void OnDownloadQRClicked(object sender, RoutedEventArgs e)
        {
            if (_currentQRCodeBytes == null || _currentQRCodeBytes.Length == 0)
            {
                ShowInlineMessage("No QR code has been generated yet.");
                return;
            }

            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
                savePicker.SuggestedFileName = "QRCode";

                // WinUI 3 requires initialization of the window handle for pickers
                // Get the window handle from the XamlRoot
                if (this.XamlRoot != null)
                {
                    // Find the window that contains this control
                    var window = GetWindowForElement(this);
                    if (window != null)
                    {
                        // Initialize the picker with the window handle
                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                    }
                    else
                    {
                        ShowInlineMessage("Could not access window for file picker.");
                        return;
                    }
                }
                else
                {
                    ShowInlineMessage("Cannot save file: Control not properly loaded in UI");
                    return;
                }

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteBytesAsync(file, _currentQRCodeBytes);
                    ShowInlineMessage("QR code saved successfully!");
                }
            }
            catch (Exception ex)
            {
                ShowInlineMessage($"Error saving QR code: {ex.Message}");
            }
        }

        // Helper method to find the window that contains an element
        private Window GetWindowForElement(UIElement element)
        {
            return Window.Current;
        }


        // Use inline message display instead of popup for better compatibility
        private void ShowInlineMessage(string message)
        {
            // Create message UI elements
            var messageBox = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightYellow),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Orange),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 5),
                Width = 350,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            messageBox.Child = messageText;

            // Find stack panel to add message to
            var stackPanel = this.Content as StackPanel;
            if (stackPanel != null)
            {
                // Look for existing message
                bool messageExists = false;
                for (int i = 0; i < stackPanel.Children.Count; i++)
                {
                    if (stackPanel.Children[i] is Border border &&
                        border.Child is TextBlock)
                    {
                        // Replace existing message
                        stackPanel.Children[i] = messageBox;
                        messageExists = true;
                        break;
                    }
                }

                if (!messageExists)
                {
                    // Insert after input box
                    int insertIndex = 1; // After input box
                    stackPanel.Children.Insert(insertIndex, messageBox);
                }

                // Auto-remove message after 5 seconds
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    if (stackPanel.Children.Contains(messageBox))
                    {
                        stackPanel.Children.Remove(messageBox);
                    }
                };
                timer.Start();
            }
        }

        private Windows.UI.Color ToWindowsColor(System.Drawing.Color color)
        {
            return new Windows.UI.Color
            {
                A = color.A,
                R = color.R,
                G = color.G,
                B = color.B
            };
        }

        private System.Drawing.Color ToDrawingColor(Windows.UI.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }

    // Helper class for WinUI 3 file picker initialization
    namespace WinRT.Interop
    {
        public static class WindowNative
        {
            // P/Invoke declaration for getting the window handle
            [System.Runtime.InteropServices.DllImport("user32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, PreserveSig = true, SetLastError = true)]
            public static extern IntPtr GetActiveWindow();

            public static IntPtr GetWindowHandle(object window)
            {
                // This is a helper method - in a real implementation,
                // you would need to use proper COM interop to get the handle
                if (window is Microsoft.UI.Xaml.Window winuiWindow)
                {
                    var windowHandle = Microsoft.UI.Win32Interop.GetWindowFromWindowId(winuiWindow.AppWindow.Id);
                    return windowHandle;
                }

                // Fallback to active window
                return GetActiveWindow();
            }
        }

        public static class InitializeWithWindow
        {
            // P/Invoke declaration for initializing the picker
            [System.Runtime.InteropServices.DllImport("shell32.dll", SetLastError = true)]
            public static extern void IInitializeWithWindow_Initialize(IntPtr hwnd, IntPtr window);

            public static void Initialize(object picker, IntPtr hwnd)
            {
                // Using COM interop to initialize the picker with the window handle
                var factory = picker as IInitializeWithWindow;
                if (factory != null)
                {
                    factory.Initialize(hwnd);
                }
                else
                {
                    // Fallback using reflection
                    Type type = picker.GetType();
                    var method = type.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(picker, new object[] { hwnd });
                    }
                }
            }
        }

        // COM interface for IInitializeWithWindow
        [System.Runtime.InteropServices.ComImport]
        [System.Runtime.InteropServices.Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        public interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }
    }
}
