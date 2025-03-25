using DevTools.DTOs.Response;
using DevTools.Entities;
using DevTools.Enums;

namespace DevTools.Interfaces.Services
{
    public interface IToolService
    {
        Task<IEnumerable<ToolDTO>> GetToolsAsync(UserRole role, int userId = -1);
        Task<IEnumerable<ToolDTO>> GetToolFavoriteAsync(UserRole role, int userId);
        Task<ToolDTO?> GetToolByIdAsync(int id, UserRole role, int userId = -1);
        Task AddToolAsync(Tool tool);
        Task DisableTool(int id);
        Task EnableTool(int id);
        Task SetPremium(int id);
        Task SetFree(int id);
        Task UpdateToolAsync(Tool tool);
        Task DeleteToolAsync(int id);
        Task UpdateToolList();
        string ExecuteTool(int toolId, string input, UserRole role = UserRole.User);
    }
}