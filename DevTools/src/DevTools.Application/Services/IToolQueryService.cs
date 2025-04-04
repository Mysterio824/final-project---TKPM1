using DevTools.Application.DTOs.Response;
using DevTools.Domain.Enums;

namespace DevTools.Application.Services
{
    public interface IToolQueryService
    {
        Task<IEnumerable<ToolDto>> GetToolsAsync(UserRole role, int userId = -1);
        Task<IEnumerable<ToolDto>> GetToolFavoriteAsync(UserRole role, int userId);
        Task<ToolDto?> GetToolByIdAsync(int id, UserRole role, int userId = -1);
        Task<IEnumerable<ToolDto>> GetToolsByNameAsync(string name, UserRole role, int userId = -1);
    }
}