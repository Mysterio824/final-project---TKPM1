using DevTools.Data;
using DevTools.Entities;
using DevTools.Enums;
using DevTools.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevTools.Repositories
{
    public class ToolRepository : IToolRepository
    {
        private readonly ApplicationDbContext _context;

        public ToolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tool>> GetAllAsync() => await _context.Tools.ToListAsync();

        public async Task<IEnumerable<Tool>> GetFavoriteAsync(int userId)
        {
            var favoriteTools = await _context.Tools
                .Join(_context.FavoriteTools,
                      tool => tool.Id,
                      favorite => favorite.ToolId,
                      (tool, favorite) => new { Tool = tool, Favorite = favorite })
                .Where(joinResult => joinResult.Favorite.UserId == userId)
                .Select(joinResult => joinResult.Tool).ToListAsync();


            return favoriteTools;
        }

        public async Task<Tool?> GetByIdAsync(int id)
        {
            return await _context.Tools.FindAsync(id);
        }

        public async Task AddAsync(Tool tool)
        {
            _context.Tools.Add(tool);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tool tool)
        {
            _context.Tools.Update(tool);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tool = await _context.Tools.FindAsync(id);
            if (tool != null)
            {
                _context.Tools.Remove(tool);
                await _context.SaveChangesAsync();
            }
        }
    }
}
