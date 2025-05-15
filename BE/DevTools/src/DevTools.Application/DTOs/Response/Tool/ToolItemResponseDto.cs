namespace DevTools.Application.DTOs.Response.Tool
{
    public class ToolItemResponseDto : BaseResponseDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required bool IsPremium { get; set; }
        public required bool IsEnabled { get; set; }
        public required bool IsFavorite { get; set; }
    }
}
