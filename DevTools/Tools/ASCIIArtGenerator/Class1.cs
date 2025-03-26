using DevTools.DTOs.Response;
using DevTools.Enums;
using DevTools.Interfaces;
using DevTools.Repositories;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace ASCIIArtGenerator
{
    public class AsciiArtGeneratorTool : ITool
    {
        public string Name => "ASCII Art Generator";
        public string Description => "Converts input text into simple ASCII art";
        public ToolType Type => ToolType.String;

        public ToolResponse Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                new ToolResponse
                {
                    Status = 500,
                    Output = "Please provide text to convert to ASCII art.",
                    ResultType = "text/plain"
                };
            }

            input = input.ToUpper();
            var output = new StringBuilder();

            // Simple block-style ASCII art generation
            output.AppendLine(new string('*', input.Length + 4));
            output.Append("* ");
            output.Append(input);
            output.AppendLine(" *");
            output.AppendLine(new string('*', input.Length + 4));

            output.ToString();

            return new ToolResponse
            {
                Status = 200,
                Output = output.ToString(),
                ResultType = "text/plain"
            };
        }

        public ToolResponse Execute(byte[] fileBytes) { return new ToolResponse(); }
    }
}
