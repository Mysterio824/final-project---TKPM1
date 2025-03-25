using DevTools.Data;
using DevTools.Entities;
using DevTools.Interfaces;
using DevTools.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Repositories
{
    public class FavoriteToolRepository : IFavoriteToolRepository
    {
        private readonly ApplicationDbContext _context;

        public FavoriteToolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

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
            var tmp = new FavoriteTool
            {
                UserId = userId,
                ToolId = toolId
            };
            var tool = await _context.FavoriteTools.FindAsync(tmp);
            if (tool != null)
            {
                _context.FavoriteTools.Remove(tool);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FavoriteTool?> GetAsync(int userId, int toolId)
        {
            var tool = new FavoriteTool
            {
                UserId = userId,
                ToolId = toolId
            };
            var existedTool = await _context.FavoriteTools.FindAsync(tool);
            return existedTool;
        }
    }
}
