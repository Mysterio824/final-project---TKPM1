using DevTools.Data;
using DevTools.Entities;
using DevTools.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Repositories
{
    public class ToolRepository : IToolRepository
    {
        private readonly ApplicationDbContext _context;

        public ToolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tool>> GetAllAsync()
        {
            return await _context.Tools.ToListAsync();
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
