using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextDiffTool
{
    class TextDifferentiatorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to differentiate text
        public string DifferentiateText(string textA, string textB, string diffType)
        {
            switch (diffType)
            {
                case "Character Differences":
                    return GetCharacterDifferences(textA, textB);
                case "Word Differences":
                    return GetWordDifferences(textA, textB);
                case "Line Differences":
                    return GetLineDifferences(textA, textB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(diffType), diffType, null);
            }
        }

        // Character-by-character difference
        private string GetCharacterDifferences(string textA, string textB)
        {
            var result = new StringBuilder();
            int maxLength = Math.Max(textA.Length, textB.Length);

            for (int i = 0; i < maxLength; i++)
            {
                char charA = i < textA.Length ? textA[i] : '\0';
                char charB = i < textB.Length ? textB[i] : '\0';

                if (charA == charB)
                {
                    result.Append(charA);
                }
                else
                {
                    if (charA != '\0')
                        result.Append($"[-{charA}]");
                    if (charB != '\0')
                        result.Append($"[+{charB}]");
                }
            }

            return result.ToString();
        }

        // Word-by-word difference
        private string GetWordDifferences(string textA, string textB)
        {
            var wordsA = textA.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var wordsB = textB.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new StringBuilder();
            var commonWords = new List<string>();
            var uniqueToA = new List<string>();
            var uniqueToB = new List<string>();

            // Find common and unique words
            foreach (var word in wordsA)
            {
                if (wordsB.Contains(word))
                    commonWords.Add(word);
                else
                    uniqueToA.Add(word);
            }

            foreach (var word in wordsB)
            {
                if (!wordsA.Contains(word))
                    uniqueToB.Add(word);
            }

            result.AppendLine("Common Words:");
            result.AppendLine(string.Join(", ", commonWords));
            result.AppendLine();

            result.AppendLine("Unique to Text A:");
            result.AppendLine(string.Join(", ", uniqueToA));
            result.AppendLine();

            result.AppendLine("Unique to Text B:");
            result.AppendLine(string.Join(", ", uniqueToB));

            return result.ToString();
        }

        // Line-by-line difference
        private string GetLineDifferences(string textA, string textB)
        {
            var linesA = textA.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var linesB = textB.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var result = new StringBuilder();

            int maxLines = Math.Max(linesA.Length, linesB.Length);

            for (int i = 0; i < maxLines; i++)
            {
                result.AppendLine($"Line {i + 1}:");

                if (i < linesA.Length && i < linesB.Length)
                {
                    if (linesA[i] == linesB[i])
                    {
                        result.AppendLine($"  Same: {linesA[i]}");
                    }
                    else
                    {
                        result.AppendLine($"  Text A: {linesA[i]}");
                        result.AppendLine($"  Text B: {linesB[i]}");
                    }
                }
                else if (i < linesA.Length)
                {
                    result.AppendLine($"  Text A only: {linesA[i]}");
                    result.AppendLine($"  Text B: <empty>");
                }
                else
                {
                    result.AppendLine($"  Text A: <empty>");
                    result.AppendLine($"  Text B only: {linesB[i]}");
                }

                result.AppendLine();
            }

            return result.ToString();
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new TextDifferentiatorToolUI(this);
        }
    }
}
