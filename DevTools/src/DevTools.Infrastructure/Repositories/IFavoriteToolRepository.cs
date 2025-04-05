using DevTools.Domain.Entities;

namespace DevTools.Infrastructure.Repositories
{
    public interface IFavoriteToolRepository : IBaseRepository<FavoriteTool>
    {
        Task<IEnumerable<FavoriteTool>> GetAll(int userId);
        Task<FavoriteTool?> GetAsync(int userId, int toolId);
    }
}
