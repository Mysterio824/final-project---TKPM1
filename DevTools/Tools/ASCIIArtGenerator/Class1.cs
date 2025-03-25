using DevTools.Enums;
using DevTools.Interfaces;
using System.Text;

namespace ASCIIArtGenerator
{
    public class AsciiArtGeneratorTool : ITool
    {
        public string Name => "ASCII Art Generator";
        public string Description => "Converts input text into simple ASCII art";
        public ToolType Type => ToolType.String;

        public string Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "Please provide text to convert to ASCII art.";
            }

            input = input.ToUpper();
            var output = new StringBuilder();

            // Simple block-style ASCII art generation
            output.AppendLine(new string('*', input.Length + 4));
            output.Append("* ");
            output.Append(input);
            output.AppendLine(" *");
            output.AppendLine(new string('*', input.Length + 4));

            return output.ToString();
        }
    }

}
