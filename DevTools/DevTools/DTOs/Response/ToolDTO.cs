using DevTools.Enums;

namespace DevTools.DTOs.Response
{
    public class ToolDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public ToolType Type { get; set; }
        public bool IsPremium { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFavorite { get; set; }
    }
}
