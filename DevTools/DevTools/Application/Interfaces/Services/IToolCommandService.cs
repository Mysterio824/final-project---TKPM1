using DevTools.Domain.Entities;

namespace DevTools.Application.Interfaces.Services
{
    public interface IToolCommandService
    {
        Task AddToolAsync(IFormFile file);
        Task UpdateToolAsync(Tool tool);
        Task DeleteToolAsync(int id);
        Task UpdateToolList();
        Task DisableTool(int id);
        Task EnableTool(int id);
        Task SetPremium(int id);
        Task SetFree(int id);
    }
}