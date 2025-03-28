using DevTools.Domain.Entities;

namespace DevTools.Application.Interfaces.Repositories
{
    public interface IToolRepository
    {
        Task<IEnumerable<Tool>> GetAllAsync();
        Task<IEnumerable<Tool>> GetFavoriteAsync(int userId);
        Task<Tool?> GetByIdAsync(int id);
        Task<IEnumerable<Tool>> GetByNameAsync(string name);
        Task AddAsync(Tool tool);
        Task UpdateAsync(Tool tool);
        Task DeleteAsync(int id);
    }
}
