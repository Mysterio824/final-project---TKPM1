using DevTools.Application.DTOs.Response;
using DevTools.Domain.Enums;

namespace DevTools.Application.Interfaces.Services
{
    public interface IToolQueryService
    {
        Task<IEnumerable<ToolDTO>> GetToolsAsync(UserRole role, int userId = -1);
        Task<IEnumerable<ToolDTO>> GetToolFavoriteAsync(UserRole role, int userId);
        Task<ToolDTO?> GetToolByIdAsync(int id, UserRole role, int userId = -1);
        Task<IEnumerable<ToolDTO>> GetToolsByNameAsync(string name, UserRole role, int userId = -1);
    }
}