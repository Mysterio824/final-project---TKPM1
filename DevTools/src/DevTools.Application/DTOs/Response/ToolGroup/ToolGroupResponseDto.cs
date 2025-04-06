using DevTools.Domain.Enums;

namespace DevTools.Application.DTOs.Response.ToolGroup
{
    public class ToolGroupResponseDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required bool IsPremium { get; set; }
        public required bool IsEnabled { get; set; }
        public required bool IsFavorite { get; set; }
    }
}
