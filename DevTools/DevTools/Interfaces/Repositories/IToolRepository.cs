using DevTools.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevTools.Interfaces.Repositories
{
    public interface IToolRepository
    {
        Task<IEnumerable<Tool>> GetAllAsync();
        Task<Tool?> GetByIdAsync(int id);
        Task AddAsync(Tool tool);
        Task UpdateAsync(Tool tool);
        Task DeleteAsync(int id);
    }
}
