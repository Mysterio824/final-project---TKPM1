using Microsoft.AspNetCore.Http;

namespace DevTools.Application.DTOs.Response
{
    public class ToolResponseDto
    {
        public int ToolId { get; set; }
        public string? StrOutput { get; set; }
        public IFormFile? FileOutput { get; set; }
        public required bool IsFile { get; set; }
        public bool IsDone { get; set; }
    }
}
