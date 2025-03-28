using DevTools.Domain.Enums;

namespace DevTools.Application.DTOs.Response
{
    public class ToolDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ToolType Type { get; set; }
        public bool IsPremium { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFavorite { get; set; }
    }
}
