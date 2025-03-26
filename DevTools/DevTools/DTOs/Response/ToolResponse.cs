namespace DevTools.DTOs.Response
{
    public class ToolResponse
    {
        public int ToolId { get; set; }
        public string? Output { get; set; } // ASCII Art, Converted String, etc.
        public string? ResultType { get; set; } // "text" or "file"
        public int? Status { get; set; }
    }
}