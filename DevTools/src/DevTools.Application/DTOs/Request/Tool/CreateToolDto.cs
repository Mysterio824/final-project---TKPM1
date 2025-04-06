using Microsoft.AspNetCore.Http;

namespace DevTools.Application.DTOs.Request.Tool
{
    public class CreateToolDto
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public bool IsPremium { get; set; } = false;

        public required int GroupId { get; set; }

        public bool IsEnabled { get; set; } = true;

        public IFormFile File { get; set; }
    }
}
