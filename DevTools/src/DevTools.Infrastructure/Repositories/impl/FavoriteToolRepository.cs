using DevTools.DataAccess.Persistence;
using DevTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevTools.DataAccess.Repositories.impl
{
    public class FavoriteToolRepository(DatabaseContext context) : BaseRepository<FavoriteTool>(context), IFavoriteToolRepository
    {
        public async Task<IEnumerable<FavoriteTool>> GetAll(int userId)
            => await GetAllAsync(ti => ti.UserId == userId);

        public async Task<FavoriteTool?> GetAsync(int userId, int toolId)
            => await GetFirstAsync(ti => ti.UserId == userId && ti.ToolId == toolId);
    }
}
