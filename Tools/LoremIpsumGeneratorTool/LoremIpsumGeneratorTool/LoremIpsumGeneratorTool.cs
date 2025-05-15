using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoremIpsumGeneratorTool
{
    class LoremIpsumGeneratorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly string[] _loremWords = new string[]
        {
        "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit",
        "sed", "do", "eiusmod", "tempor", "incididunt", "ut", "labore", "et", "dolore",
        "magna", "aliqua", "enim", "ad", "minim", "veniam", "quis", "nostrud", "exercitation",
        "ullamco", "laboris", "nisi", "ut", "aliquip", "ex", "ea", "commodo", "consequat",
        "duis", "aute", "irure", "dolor", "in", "reprehenderit", "in", "voluptate", "velit",
        "esse", "cillum", "dolore", "eu", "fugiat", "nulla", "pariatur", "excepteur", "sint",
        "occaecat", "cupidatat", "non", "proident", "sunt", "in", "culpa", "qui", "officia",
        "deserunt", "mollit", "anim", "id", "est", "laborum", "at", "vero", "eos", "et",
        "accusamus", "dignissimos", "ducimus", "blanditiis", "praesentium", "voluptatum",
        "deleniti", "atque", "corrupti", "quos", "dolores", "quas", "molestias", "excepturi",
        "occaecati", "cupiditate", "provident", "similique", "sunt", "culpa", "officia",
        "deserunt", "mollitia", "animi", "id", "laborum", "et", "dolorum", "fuga"
        };

        private readonly Random _random = new Random();

        // Method to generate Lorem Ipsum text
        public string GenerateLoremIpsum(int paragraphs, int sentencesPerParagraph, int wordsPerSentence, bool startWithLorem, bool asHtml)
        {
            var result = new StringBuilder();
            string separator = asHtml ? "<p>" : "";
            string endSeparator = asHtml ? "</p>" : "\n\n";

            for (int p = 0; p < paragraphs; p++)
            {
                if (!string.IsNullOrEmpty(separator))
                    result.Append(separator);

                for (int s = 0; s < sentencesPerParagraph; s++)
                {
                    var sentence = new StringBuilder();

                    // First sentence of first paragraph should start with "Lorem ipsum" if requested
                    if (p == 0 && s == 0 && startWithLorem)
                    {
                        sentence.Append("Lorem ipsum ");
                        for (int w = 2; w < wordsPerSentence; w++) // Already added 2 words
                        {
                            sentence.Append(_loremWords[_random.Next(_loremWords.Length)]).Append(" ");
                        }
                    }
                    else
                    {
                        for (int w = 0; w < wordsPerSentence; w++)
                        {
                            sentence.Append(_loremWords[_random.Next(_loremWords.Length)]).Append(" ");
                        }
                    }

                    // Capitalize first letter
                    if (sentence.Length > 0)
                    {
                        sentence[0] = char.ToUpper(sentence[0]);
                    }

                    // Remove the last space and add period
                    if (sentence.Length > 0)
                    {
                        sentence.Length--; // Remove last space
                        sentence.Append(". ");
                    }

                    result.Append(sentence);
                }

                // Remove the last space before adding paragraph separator
                if (result.Length > 0)
                {
                    result.Length--;
                }

                result.Append(endSeparator);
            }

            return result.ToString().Trim();
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new LoremIpsumGeneratorToolUI(this);
        }
    }
}
