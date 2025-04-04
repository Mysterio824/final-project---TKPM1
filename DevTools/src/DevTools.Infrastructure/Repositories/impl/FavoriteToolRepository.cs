using DevTools.Domain.Entities;
using DevTools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Infrastructure.Repositories.impl
{
    public class FavoriteToolRepository(DatabaseContext context) : IFavoriteToolRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<IEnumerable<FavoriteTool>> GetAll(int userId) =>
            await _context.FavoriteTools
                            .Where(x => x.UserId == userId)
                            .ToListAsync();

        public async Task AddAsync(int userId, int toolId)
        {
            var tool = new FavoriteTool
            {
                UserId = userId,
                ToolId = toolId
            };
            _context.FavoriteTools.Add(tool);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId, int toolId)
        {
            var tool = await _context.FavoriteTools.FindAsync(userId, toolId);
            if (tool != null)
            {
                _context.FavoriteTools.Remove(tool);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FavoriteTool?> GetAsync(int userId, int toolId)
            => await _context.FavoriteTools.FindAsync(userId, toolId);
    }
}
