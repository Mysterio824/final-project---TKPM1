using DevTools.Domain.Entities;

namespace DevTools.Infrastructure.Repositories
{
    public interface IToolRepository : IBaseRepository<Tool>
    {
        Task<IEnumerable<Tool>> GetAll();
        Task<IEnumerable<Tool>> GetFavoriteAsync(int userId);
        Task<Tool?> GetByIdAsync(int id);
        Task<IEnumerable<Tool>> GetByNameAsync(string name);
    }
}
