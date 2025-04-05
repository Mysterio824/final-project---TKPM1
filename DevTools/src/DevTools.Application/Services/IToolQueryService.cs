using DevTools.Application.DTOs.Response.Tool;
using DevTools.Domain.Enums;

namespace DevTools.Application.Services
{
    public interface IToolQueryService
    {
        Task<IEnumerable<ToolItemResponseDto>> GetToolsAsync(UserRole role, int userId = -1);
        Task<IEnumerable<ToolItemResponseDto>> GetToolFavoriteAsync(UserRole role, int userId);
        Task<ToolItemResponseDto?> GetToolByIdAsync(int id, UserRole role, int userId = -1);
        Task<IEnumerable<ToolItemResponseDto>> GetToolsByNameAsync(string name, UserRole role, int userId = -1);
    }
}