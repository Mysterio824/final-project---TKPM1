using Microsoft.AspNetCore.Http;

namespace DevTools.Application.DTOs.Request.Tool
{
    public class UpdateToolDto
    {
        public required int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public bool IsPremium { get; set; } = false;

        public bool IsEnabled { get; set; } = true;

        public required int GroupId { get; set; }

        public FormFile File { get; set; }
    }
}
