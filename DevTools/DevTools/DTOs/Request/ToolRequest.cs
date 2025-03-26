namespace DevTools.DTOs.Request
{
    public class ToolRequest
    {
        public int ToolId { get; set; }
        public string? InputText { get; set; }
        public IFormFile? UploadedFile { get; set; }
    }
}
