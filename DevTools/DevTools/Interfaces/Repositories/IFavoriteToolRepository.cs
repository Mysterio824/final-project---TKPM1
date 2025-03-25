using DevTools.Entities;

namespace DevTools.Interfaces.Repositories
{
    public interface IFavoriteToolRepository
    {
        Task<IEnumerable<FavoriteTool>> GetAll(int userId);
        Task<FavoriteTool?> GetAsync(int userId, int toolId);
        Task AddAsync(int userId, int toolId);
        Task DeleteAsync(int userId, int id);
    }
}
