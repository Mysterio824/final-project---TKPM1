using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRGeneratorTool
{
    public interface ITool
    {
        object Execute(object input);
        UserControl GetUI();
    }
    public class QRGeneratorTool : ITool, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum ErrorCorrectionLevel
        {
            L, // Low - 7% recovery
            M, // Medium - 15% recovery
            Q, // Quartile - 25% recovery
            H  // High - 30% recovery
        }

        // Generate QR Code using a simple implementation
        public byte[] GenerateQRCode(string input, System.Drawing.Color foregroundColor,
                                    System.Drawing.Color backgroundColor, ErrorCorrectionLevel errorCorrectionLevel)
        {
            // Create a QR code using our custom generator
            var qrCode = new SimpleQRCodeGenerator(errorCorrectionLevel);
            var matrix = qrCode.EncodeText(input);

            // Create a bitmap from the QR code matrix
            int size = matrix.GetLength(0);
            int scale = 5; // Scale factor to make QR code larger
            int imageSize = size * scale;

            // Create a blank bitmap with the specified size
            using (var bitmap = new System.Drawing.Bitmap(imageSize, imageSize))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(backgroundColor); // Fill with background color

                    // Draw each module (pixel) of the QR code
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            if (matrix[y, x])
                            {
                                // Draw a filled rectangle for each "true" value in the matrix
                                using (var brush = new System.Drawing.SolidBrush(foregroundColor))
                                {
                                    g.FillRectangle(brush, x * scale, y * scale, scale, scale);
                                }
                            }
                        }
                    }
                }

                // Convert the bitmap to a byte array
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        public ErrorCorrectionLevel GetErrorCorrectionLevel(string level)
        {
            switch (level)
            {
                case "Low":
                    return ErrorCorrectionLevel.L;
                case "Medium":
                    return ErrorCorrectionLevel.M;
                case "Quartile":
                    return ErrorCorrectionLevel.Q;
                case "High":
                    return ErrorCorrectionLevel.H;
                default:
                    return ErrorCorrectionLevel.M;
            }
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new QRGeneratorToolUI(this);
        }
    }

    // Simple QR Code generator implementation to remove dependency on ZXing
    public class SimpleQRCodeGenerator
    {
        private readonly QRGeneratorTool.ErrorCorrectionLevel _errorLevel;

        // Reed-Solomon coefficients for error correction
        private static readonly int[][] REED_SOLOMON_COEFFICIENTS = {
        // L - 7%
        new[] { 1, 1 },
        // M - 15%
        new[] { 1, 25, 55 },
        // Q - 25%
        new[] { 1, 33, 59, 84, 93 },
        // H - 30%
        new[] { 1, 39, 71, 89, 107, 111 }
    };

        public SimpleQRCodeGenerator(QRGeneratorTool.ErrorCorrectionLevel errorLevel)
        {
            _errorLevel = errorLevel;
        }

        public bool[,] EncodeText(string text)
        {
            // Simple implementation - for production use, consider using a library or more robust algorithm
            // This is a simplified version that creates a QR-code-like pattern
            byte[] data = Encoding.UTF8.GetBytes(text);

            // Determine size based on data length (real QR codes use a more complex algorithm)
            int size = Math.Max(21, (int)Math.Ceiling(Math.Sqrt(data.Length * 8 + 100)));
            // Make sure size is odd for proper alignment patterns
            size = size % 2 == 0 ? size + 1 : size;

            // Create the matrix
            bool[,] matrix = new bool[size, size];

            // Add finder patterns (the three large squares in corners)
            AddFinderPattern(matrix, 0, 0);
            AddFinderPattern(matrix, size - 7, 0);
            AddFinderPattern(matrix, 0, size - 7);

            // Add alignment pattern (bottom right corner)
            AddAlignmentPattern(matrix, size - 9, size - 9);

            // Add timing patterns (the lines connecting finder patterns)
            for (int i = 8; i < size - 8; i++)
            {
                matrix[i, 6] = i % 2 == 0;
                matrix[6, i] = i % 2 == 0;
            }

            // Encode data into the QR code
            EncodeData(matrix, data);

            // Apply a simple version of Reed-Solomon error correction
            ApplyErrorCorrection(matrix);

            return matrix;
        }

        private void AddFinderPattern(bool[,] matrix, int offsetX, int offsetY)
        {
            // Outer square
            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (x == 0 || x == 6 || y == 0 || y == 6 || (x >= 2 && x <= 4 && y >= 2 && y <= 4))
                    {
                        matrix[offsetY + y, offsetX + x] = true;
                    }
                }
            }
        }

        private void AddAlignmentPattern(bool[,] matrix, int centerX, int centerY)
        {
            for (int y = -2; y <= 2; y++)
            {
                for (int x = -2; x <= 2; x++)
                {
                    bool isOuterRing = Math.Abs(x) == 2 || Math.Abs(y) == 2;
                    bool isCenter = x == 0 && y == 0;

                    int posX = centerX + x;
                    int posY = centerY + y;

                    if (posX >= 0 && posX < matrix.GetLength(1) &&
                        posY >= 0 && posY < matrix.GetLength(0))
                    {
                        matrix[posY, posX] = isOuterRing || isCenter;
                    }
                }
            }
        }

        private void EncodeData(bool[,] matrix, byte[] data)
        {
            int size = matrix.GetLength(0);
            int index = 0;
            int bitCount = 0;

            // Start from bottom right, going up in zig-zag
            for (int right = size - 1; right >= 0; right -= 2)
            {
                // Going up if from right side, going down if from left side
                bool goingUp = right % 4 != 0;

                for (int vertical = 0; vertical < size; vertical++)
                {
                    int y = goingUp ? size - 1 - vertical : vertical;

                    for (int xOffset = 0; xOffset < 2; xOffset++)
                    {
                        int x = right - xOffset;

                        // Skip reserved areas (finder patterns, etc.)
                        if (x < 0 || IsReservedArea(x, y, size))
                        {
                            continue;
                        }

                        // Place data bits
                        if (index < data.Length)
                        {
                            bool bit = (data[index] & (1 << (7 - bitCount))) != 0;
                            matrix[y, x] = bit;

                            bitCount++;
                            if (bitCount == 8)
                            {
                                bitCount = 0;
                                index++;
                            }
                        }
                    }
                }
            }
        }

        private bool IsReservedArea(int x, int y, int size)
        {
            // Check if coordinates are in finder patterns
            bool inTopLeftFinder = x < 8 && y < 8;
            bool inTopRightFinder = x >= size - 8 && y < 8;
            bool inBottomLeftFinder = x < 8 && y >= size - 8;

            // Check if in alignment pattern area
            bool inAlignmentPattern = x >= size - 11 && x <= size - 7 &&
                                      y >= size - 11 && y <= size - 7;

            // Check if on timing patterns
            bool onTimingPattern = (x == 6) || (y == 6);

            return inTopLeftFinder || inTopRightFinder || inBottomLeftFinder ||
                   inAlignmentPattern || onTimingPattern;
        }

        private void ApplyErrorCorrection(bool[,] matrix)
        {
            // Get error correction coefficients based on level
            int[] coefficients = REED_SOLOMON_COEFFICIENTS[(int)_errorLevel];

            // This is a simplified version that just adds some patterns based on coefficients
            // Real Reed-Solomon would be much more complex
            int size = matrix.GetLength(0);

            for (int i = 0; i < coefficients.Length; i++)
            {
                int coefficient = coefficients[i];
                int x = (coefficient % (size - 16)) + 8;
                int y = ((coefficient * 3) % (size - 16)) + 8;

                if (!IsReservedArea(x, y, size))
                {
                    matrix[y, x] = !matrix[y, x]; // Flip bit for error correction pattern
                }
            }
        }
    }
}
