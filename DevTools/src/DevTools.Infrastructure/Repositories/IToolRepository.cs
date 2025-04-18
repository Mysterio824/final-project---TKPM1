using DevTools.Domain.Entities;

namespace DevTools.DataAccess.Repositories
{
    public interface IToolRepository : IBaseRepository<Tool>
    {
        Task<IEnumerable<Tool>> GetAll();
        Task<IEnumerable<Tool>> GetByGroupAsync(int Id);
        Task<IEnumerable<Tool>> GetFavoriteAsync(int userId);
        Task<Tool?> GetByIdAsync(int id);
        Task<IEnumerable<Tool>> SearchByNameAsync(string name);
        Task<Tool?> GetByNameAsync(string name);
    }
}
