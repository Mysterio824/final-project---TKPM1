using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenGeneratorTool
{
    class TokenGeneratorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string GenerateToken(int length, bool includeLowercase, bool includeUppercase,
                                   bool includeNumbers, bool includeSymbols)
        {
            if (length <= 0)
                return string.Empty;

            if (!includeLowercase && !includeUppercase && !includeNumbers && !includeSymbols)
                return string.Empty;

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            const string symbols = "!@#$%^&*()-_=+[]{}|;:,.<>?/";

            var charPool = new StringBuilder();
            if (includeLowercase) charPool.Append(lowercase);
            if (includeUppercase) charPool.Append(uppercase);
            if (includeNumbers) charPool.Append(numbers);
            if (includeSymbols) charPool.Append(symbols);

            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(charPool[random.Next(charPool.Length)]);
            }

            return result.ToString();
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new TokenGeneratorToolUI(this);
        }
    }
}
