using DevTools.Application.Interfaces.Repositories;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Infrastructure.Repositories
{
    public class FavoriteToolRepository(ApplicationDbContext context) : IFavoriteToolRepository
    {
        private readonly ApplicationDbContext _context = context;

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
