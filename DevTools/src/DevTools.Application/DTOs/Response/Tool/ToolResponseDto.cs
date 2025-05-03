using Microsoft.AspNetCore.Http;

namespace DevTools.Application.DTOs.Response.Tool
{
    public class ToolResponseDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required bool IsPremium { get; set; }
        public required bool IsEnabled { get; set; }
        public required bool IsFavorite { get; set; }

        public required byte[] File { get; set; }
    }
}
