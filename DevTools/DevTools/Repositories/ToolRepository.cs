using DevTools.Data;
using DevTools.Entities;
using DevTools.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Repositories
{
    public class ToolRepository(ApplicationDbContext context) : IToolRepository
    {
        private readonly ApplicationDbContext _context = context;

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

        public async Task<IEnumerable<Tool>> GetByNameAsync(string name)
        {
            return await _context.Tools.Where(tool => tool.Name.ToLower().Contains(name.ToLower())) .ToListAsync();
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
