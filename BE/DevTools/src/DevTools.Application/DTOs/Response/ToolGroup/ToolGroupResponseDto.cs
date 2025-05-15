using DevTools.Domain.Enums;

namespace DevTools.Application.DTOs.Response.ToolGroup
{
    public class ToolGroupResponseDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
