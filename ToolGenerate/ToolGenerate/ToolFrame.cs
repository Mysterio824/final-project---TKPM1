using System.Text;
using DevTools.DTOs.Response;
using DevTools.Enums;
using DevTools.Interfaces.Core;

namespace ToolGenerate
{
    //public class ToolResponse
    //{
    //    public int ToolId { get; set; }
    //    public string? Output { get; set; } // string output
    //    public required bool IsFile { get; set; } // "text" or "file"
    //    public byte[]? FileContent { get; set; } // Binary data for file
    //    public string? FileName { get; set; } // Name of the file
    //    public string? ContentType { get; set; } // MIME type of file
    //}
    public class ToolFrame : ITool
    {
        public string Name => "ASCII Art Generator";
        public string Description => "Converts input text into simple ASCII art";
        public ToolType Type => ToolType.String;

        public ToolResponse Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ToolResponse
                {
                    Output = "Please provide text to convert to ASCII art.",
                    IsFile = false
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
                Output = output.ToString(),
                IsFile = false
            };
        }

        public ToolResponse Execute(byte[] fileBytes) { return new ToolResponse { IsFile = true }; }
    }
}
