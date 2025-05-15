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
using Microsoft.UI.Text;
using System.IO;
using Microsoft.UI.Xaml.Shapes;
using System.Drawing;

namespace QRGeneratorTool
{
    class QRGeneratorToolUI : UserControl
    {
        private readonly QRGeneratorTool _tool;
        private TextBox _inputBox;
        private ComboBox _errorLevelComboBox;
        private TextBox _foregroundColorBox;
        private Microsoft.UI.Xaml.Shapes.Rectangle _foregroundColorPreview;
        private TextBox _backgroundColorBox;
        private Microsoft.UI.Xaml.Shapes.Rectangle _backgroundColorPreview;
        private Microsoft.UI.Xaml.Controls.Image _qrCodeImage;
        private byte[] _currentQRCodeBytes;

        public QRGeneratorToolUI(QRGeneratorTool tool)
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
                Padding = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create and configure TextBox (Input)
            _inputBox = new TextBox
            {
                Header = "Input Text or URL",
                Width = 350,
                Margin = new Thickness(0, 5, 0, 0),
                PlaceholderText = "Enter text or URL to encode"
            };

            // Create error level selector
            _errorLevelComboBox = new ComboBox
            {
                Header = "Error Correction Level",
                Width = 350,
                Margin = new Thickness(0, 10, 0, 0),
                ItemsSource = new string[] { "Low", "Medium", "Quartile", "High" },
                SelectedIndex = 1 // Default to Medium
            };

            // Create foreground color selector
            var foregroundPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            _foregroundColorBox = new TextBox
            {
                Header = "Foreground Color (Hex)",
                PlaceholderText = "#000000",
                Text = "#000000",
                Width = 250
            };

            _foregroundColorPreview = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = 80,
                Height = 30,
                Margin = new Thickness(10, 20, 0, 0),
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Black)
            };

            _foregroundColorBox.TextChanged += (s, e) => {
                try
                {
                    var colorText = _foregroundColorBox.Text;
                    if (!colorText.StartsWith("#"))
                        colorText = "#" + colorText;

                    var color = HexToColor(colorText);
                    _foregroundColorPreview.Fill = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, color.R, color.G, color.B));
                }
                catch
                {
                    // Invalid color code - keep current preview
                }
            };

            foregroundPanel.Children.Add(_foregroundColorBox);
            foregroundPanel.Children.Add(_foregroundColorPreview);

            // Create background color selector
            var backgroundPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            _backgroundColorBox = new TextBox
            {
                Header = "Background Color (Hex)",
                PlaceholderText = "#FFFFFF",
                Text = "#FFFFFF",
                Width = 250
            };

            _backgroundColorPreview = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = 80,
                Height = 30,
                Margin = new Thickness(10, 20, 0, 0),
                Fill = new SolidColorBrush(Microsoft.UI.Colors.White)
            };

            _backgroundColorBox.TextChanged += (s, e) => {
                try
                {
                    var colorText = _backgroundColorBox.Text;
                    if (!colorText.StartsWith("#"))
                        colorText = "#" + colorText;

                    var color = HexToColor(colorText);
                    _backgroundColorPreview.Fill = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, color.R, color.G, color.B));
                }
                catch
                {
                    // Invalid color code - keep current preview
                }
            };

            backgroundPanel.Children.Add(_backgroundColorBox);
            backgroundPanel.Children.Add(_backgroundColorPreview);

            // Create and configure Button (Generate QR Code)
            var generateQRButton = new Button
            {
                Content = "Generate QR Code",
                Width = 350,
                Margin = new Thickness(0, 10, 0, 0)
            };
            generateQRButton.Click += OnGenerateQRClicked;

            // Create image to display QR code
            _qrCodeImage = new Microsoft.UI.Xaml.Controls.Image
            {
                Width = 300,
                Height = 300,
                Margin = new Thickness(0, 10, 0, 0),
                Stretch = Stretch.Uniform
            };

            // Create download button
            var downloadButton = new Button
            {
                Content = "Download QR Code",
                Width = 350,
                Margin = new Thickness(0, 10, 0, 0),
                IsEnabled = false,
                Name = "downloadButton"
            };
            downloadButton.Click += OnDownloadQRClicked;

            // Add controls to StackPanel
            stack.Children.Add(_inputBox);
            stack.Children.Add(_errorLevelComboBox);
            stack.Children.Add(foregroundPanel);
            stack.Children.Add(backgroundPanel);
            stack.Children.Add(generateQRButton);
            stack.Children.Add(_qrCodeImage);
            stack.Children.Add(downloadButton);

            // Set the content of UserControl
            this.Content = stack;
        }

        // Helper method to convert hex color string to Color
        private Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");

            if (hex.Length == 6)
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }

            return Color.Black; // Default
        }

        private void OnGenerateQRClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get inputs
                var inputText = _inputBox.Text;
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    ShowMessage("Please enter text or URL");
                    return;
                }

                // Get selected error level
                var selectedErrorLevel = _errorLevelComboBox.SelectedItem.ToString();

                // Get colors
                var foreColor = HexToColor(_foregroundColorBox.Text);
                var backColor = HexToColor(_backgroundColorBox.Text);

                // Generate QR code
                _currentQRCodeBytes = _tool.GenerateQRCode(
                    inputText,
                    foreColor,
                    backColor,
                    _tool.GetErrorCorrectionLevel(selectedErrorLevel));

                // Display the QR code
                var image = new BitmapImage();
                using (var stream = new MemoryStream(_currentQRCodeBytes))
                {
                    stream.Position = 0;
                    image.SetSource(stream.AsRandomAccessStream());
                }
                _qrCodeImage.Source = image;

                // Enable download button
                var downloadButton = FindName("downloadButton") as Button;
                if (downloadButton != null)
                {
                    downloadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error generating QR code: {ex.Message}");
            }
        }

        private void OnDownloadQRClicked(object sender, RoutedEventArgs e)
        {
            if (_currentQRCodeBytes == null || _currentQRCodeBytes.Length == 0)
            {
                ShowMessage("No QR code to download. Please generate a QR code first.");
                return;
            }

            try
            {
                // Use a custom popup for saving
                var savePopup = new Popup
                {
                    Child = CreateSavePopupContent(),
                    Width = 400,
                    Height = 200
                };

                //savePopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                ShowMessage($"Error saving QR code: {ex.Message}");
            }
        }

        private UIElement CreateSavePopupContent()
        {
            var grid = new Grid
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20)
            };

            var stack = new StackPanel
            {
                Spacing = 10
            };

            var title = new TextBlock
            {
                Text = "Save QR Code",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var fileNameBox = new TextBox
            {
                Header = "File Name",
                PlaceholderText = "Enter file name",
                Text = "QRCode",
                Width = 350
            };

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100
            };

            var saveButton = new Button
            {
                Content = "Save",
                Width = 100
            };

            cancelButton.Click += (s, e) => ((Popup)((FrameworkElement)grid.Parent).Parent).IsOpen = false;

            saveButton.Click += (s, e) => {
                try
                {
                    // Get the file name
                    var fileName = fileNameBox.Text;
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        fileName = "QRCode";
                    }

                    // Ensure it has a .png extension
                    if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += ".png";
                    }

                    // Let Windows decide where to save the file
                    // In a real app, you'd want to use FileSavePicker, but we're using a simpler approach here
                    var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var filePath = System.IO.Path.Combine(downloadsPath, fileName);

                    // Save the file
                    File.WriteAllBytes(filePath, _currentQRCodeBytes);

                    // Show success message
                    ShowMessage($"QR code saved to {filePath}");

                    // Close the popup
                    ((Popup)((FrameworkElement)grid.Parent).Parent).IsOpen = false;
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error saving file: {ex.Message}");
                }
            };

            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(saveButton);

            stack.Children.Add(title);
            stack.Children.Add(fileNameBox);
            stack.Children.Add(buttonsPanel);

            grid.Children.Add(stack);

            return grid;
        }

        private void ShowMessage(string message)
        {
            // Since we can't use XamlRoot directly, we'll use a Popup instead
            var popup = new Popup
            {
                Child = CreateMessagePopupContent(message),
                Width = 400,
                Height = 200
            };

            //popup.IsOpen = true;
        }

        private UIElement CreateMessagePopupContent(string message)
        {
            var grid = new Grid
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20)
            };

            var stack = new StackPanel
            {
                Spacing = 10
            };

            var title = new TextBlock
            {
                Text = "QR Code Generator",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };

            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            okButton.Click += (s, e) => ((Popup)((FrameworkElement)grid.Parent).Parent).IsOpen = false;

            stack.Children.Add(title);
            stack.Children.Add(messageText);
            stack.Children.Add(okButton);

            grid.Children.Add(stack);

            return grid;
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
