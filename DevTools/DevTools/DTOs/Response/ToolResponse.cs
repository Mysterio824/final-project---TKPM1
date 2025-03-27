namespace DevTools.DTOs.Response
{
    public class ToolResponse
    {
        public int ToolId { get; set; }
        public string? Output { get; set; } // string output
        public required bool IsFile { get; set; } // "text" or "file"
        public byte[]? FileContent { get; set; } // Binary data for file
        public string? FileName { get; set; } // Name of the file
        public string? ContentType { get; set; } // MIME type of file
    }
}