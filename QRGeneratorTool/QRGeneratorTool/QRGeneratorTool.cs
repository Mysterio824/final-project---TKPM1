using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QRGeneratorTool
{
    class QRGeneratorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Reed-Solomon error correction levels
        public enum ErrorCorrectionLevel
        {
            Low,        // 7% of codewords can be restored
            Medium,     // 15% of codewords can be restored
            Quartile,   // 25% of codewords can be restored
            High        // 30% of codewords can be restored
        }

        // QR Code version (size)
        private const int QRVersion = 5; // Version 5 (37x37 modules)
        private const int ModulesPerSide = 37; // Version 5 has 37x37 modules

        // Generate QR code as byte array
        public byte[] GenerateQRCode(string input, Color foregroundColor, Color backgroundColor, ErrorCorrectionLevel errorLevel)
        {
            // Create matrix for QR code (true = black, false = white)
            bool[,] qrMatrix = GenerateQRMatrix(input, errorLevel);

            // Create a bitmap with the QR code
            using (var bitmap = new Bitmap(ModulesPerSide * 8, ModulesPerSide * 8))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    // Fill background
                    using (var brush = new SolidBrush(backgroundColor))
                    {
                        g.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
                    }

                    // Draw QR modules
                    using (var brush = new SolidBrush(foregroundColor))
                    {
                        for (int y = 0; y < ModulesPerSide; y++)
                        {
                            for (int x = 0; x < ModulesPerSide; x++)
                            {
                                if (qrMatrix[x, y])
                                {
                                    g.FillRectangle(brush, x * 8, y * 8, 8, 8);
                                }
                            }
                        }
                    }
                }

                // Convert bitmap to byte array
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }

        // Generate QR matrix (simplified implementation)
        private bool[,] GenerateQRMatrix(string input, ErrorCorrectionLevel errorLevel)
        {
            bool[,] matrix = new bool[ModulesPerSide, ModulesPerSide];

            // Add finder patterns (the three large squares in corners)
            AddFinderPattern(matrix, 0, 0);
            AddFinderPattern(matrix, ModulesPerSide - 7, 0);
            AddFinderPattern(matrix, 0, ModulesPerSide - 7);

            // Add alignment pattern (for version 5+)
            AddAlignmentPattern(matrix, 28, 28);

            // Add timing patterns (the lines connecting finder patterns)
            for (int i = 8; i < ModulesPerSide - 8; i++)
            {
                matrix[i, 6] = i % 2 == 0;
                matrix[6, i] = i % 2 == 0;
            }

            // Generate data from input string
            byte[] data = Encoding.UTF8.GetBytes(input);

            // Add data to QR code (simplified - in reality this involves complex encoding)
            int dataIndex = 0;
            // Simple data filling pattern - zigzag from bottom right
            for (int i = ModulesPerSide - 1; i >= 0; i -= 2)
            {
                // Even columns move up, odd columns move down
                if (i % 4 == 0)
                {
                    for (int j = ModulesPerSide - 1; j >= 0; j--)
                    {
                        if (!IsReservedModule(i, j) && dataIndex < data.Length * 8)
                        {
                            matrix[i, j] = GetBit(data, dataIndex++);
                        }
                        if (!IsReservedModule(i - 1, j) && dataIndex < data.Length * 8)
                        {
                            matrix[i - 1, j] = GetBit(data, dataIndex++);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < ModulesPerSide; j++)
                    {
                        if (!IsReservedModule(i, j) && dataIndex < data.Length * 8)
                        {
                            matrix[i, j] = GetBit(data, dataIndex++);
                        }
                        if (!IsReservedModule(i - 1, j) && dataIndex < data.Length * 8)
                        {
                            matrix[i - 1, j] = GetBit(data, dataIndex++);
                        }
                    }
                }
            }

            // Apply a mask pattern (to avoid patterns that make scanning difficult)
            ApplyMask(matrix);

            return matrix;
        }

        // Check if a module is reserved (part of finder patterns, etc.)
        private bool IsReservedModule(int x, int y)
        {
            // Check if out of bounds
            if (x < 0 || y < 0 || x >= ModulesPerSide || y >= ModulesPerSide)
                return true;

            // Finder patterns (including separator)
            if ((x < 9 && y < 9) ||
                (x >= ModulesPerSide - 9 && y < 9) ||
                (x < 9 && y >= ModulesPerSide - 9))
                return true;

            // Alignment pattern
            if (x >= 28 - 2 && x <= 28 + 2 && y >= 28 - 2 && y <= 28 + 2)
                return true;

            // Timing patterns
            if (x == 6 || y == 6)
                return true;

            return false;
        }

        // Get a bit from byte array
        private bool GetBit(byte[] data, int bitIndex)
        {
            int byteIndex = bitIndex / 8;
            int bitInByte = bitIndex % 8;

            if (byteIndex >= data.Length)
                return false;

            return ((data[byteIndex] >> (7 - bitInByte)) & 1) == 1;
        }

        // Add finder pattern to matrix
        private void AddFinderPattern(bool[,] matrix, int offsetX, int offsetY)
        {
            // Outer border
            for (int i = 0; i < 7; i++)
            {
                matrix[offsetX + i, offsetY] = true;
                matrix[offsetX + i, offsetY + 6] = true;
                matrix[offsetX, offsetY + i] = true;
                matrix[offsetX + 6, offsetY + i] = true;
            }

            // Inner square
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    matrix[offsetX + 1 + i, offsetY + 1 + j] = (i == 0 || i == 4 || j == 0 || j == 4);
                }
            }

            // Center square
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    matrix[offsetX + 2 + i, offsetY + 2 + j] = true;
                }
            }

            // Add separator (white space around finder patterns)
            for (int i = 0; i < 8; i++)
            {
                if (offsetX + i < ModulesPerSide && offsetY + 7 < ModulesPerSide)
                    matrix[offsetX + i, offsetY + 7] = false;
                if (offsetX + 7 < ModulesPerSide && offsetY + i < ModulesPerSide)
                    matrix[offsetX + 7, offsetY + i] = false;
            }
        }

        // Add alignment pattern
        private void AddAlignmentPattern(bool[,] matrix, int centerX, int centerY)
        {
            // Outer border
            for (int i = -2; i <= 2; i++)
            {
                matrix[centerX + i, centerY - 2] = true;
                matrix[centerX + i, centerY + 2] = true;
                matrix[centerX - 2, centerY + i] = true;
                matrix[centerX + 2, centerY + i] = true;
            }

            // Inner pattern
            matrix[centerX, centerY] = true;
            matrix[centerX - 1, centerY - 1] = false;
            matrix[centerX, centerY - 1] = false;
            matrix[centerX + 1, centerY - 1] = false;
            matrix[centerX - 1, centerY] = false;
            matrix[centerX + 1, centerY] = false;
            matrix[centerX - 1, centerY + 1] = false;
            matrix[centerX, centerY + 1] = false;
            matrix[centerX + 1, centerY + 1] = false;
        }

        // Apply mask pattern to the QR code
        private void ApplyMask(bool[,] matrix)
        {
            // Use mask pattern 0: (x + y) mod 2 == 0
            for (int y = 0; y < ModulesPerSide; y++)
            {
                for (int x = 0; x < ModulesPerSide; x++)
                {
                    // Skip reserved areas
                    if (IsReservedModule(x, y))
                        continue;

                    // Apply mask formula - toggle bit if formula is true
                    if ((x + y) % 2 == 0)
                    {
                        matrix[x, y] = !matrix[x, y];
                    }
                }
            }
        }

        // Helper method to convert ErrorCorrectionLevel string to enum
        public ErrorCorrectionLevel GetErrorCorrectionLevel(string level)
        {
            switch (level)
            {
                case "Low":
                    return ErrorCorrectionLevel.Low;
                case "Medium":
                    return ErrorCorrectionLevel.Medium;
                case "Quartile":
                    return ErrorCorrectionLevel.Quartile;
                case "High":
                    return ErrorCorrectionLevel.High;
                default:
                    return ErrorCorrectionLevel.Medium; // Default to Medium
            }
        }

        // ITool implementation
        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new QRGeneratorToolUI(this);
        }

        // INotifyPropertyChanged implementation
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
