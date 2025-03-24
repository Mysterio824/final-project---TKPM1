using DevTools.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevTools.Interfaces.Services
{
    public interface IToolService
    {
        Task<IEnumerable<Tool>> GetToolsAsync();
        Task<Tool?> GetToolByIdAsync(int id);
        Task AddToolAsync(Tool tool);
        Task UpdateToolAsync(Tool tool);
        Task DeleteToolAsync(int id);
        Task UpdateToolList();
        string ExecuteTool(int toolId, string input);
    }
}