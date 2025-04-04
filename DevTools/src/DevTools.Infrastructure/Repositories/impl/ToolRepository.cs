using DevTools.Domain.Entities;
using DevTools.Domain.Exceptions;
using DevTools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevTools.Infrastructure.Repositories.impl
{
    public class ToolRepository(
        DatabaseContext context,
        ILogger<ToolRepository> logger) : IToolRepository
    {
        private readonly DatabaseContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<ToolRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<Tool>> GetAllAsync()
        {
            try
            {
                return await _context.Tools
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all tools");
                throw new ResourceNotFoundException(typeof(IEnumerable<Tool>));
            }
        }

        public async Task<IEnumerable<Tool>> GetFavoriteAsync(int userId)
        {
            try
            {
                return await _context.Tools
                    .AsNoTracking()
                    .Where(t => _context.FavoriteTools
                        .Any(f => f.UserId == userId && f.ToolId == t.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve favorite tools for user {UserId}", userId);
                throw new ResourceNotFoundException(typeof(IEnumerable<Tool>));
            }
        }

        public async Task<Tool?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Tools
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool with ID {ToolId}", id);
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }

        public async Task<IEnumerable<Tool>> GetByNameAsync(string name)
            => await _context.Tools
                .Where(tool =>
                    tool.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase)
                )
                .ToListAsync();

        public async Task AddAsync(Tool tool)
        {
            if (tool == null)
                throw new ResourceNotFoundException(typeof(IEnumerable<Tool>));

            try
            {
                await _context.Tools.AddAsync(tool);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tool {ToolId} added successfully", tool.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add tool {ToolName}", tool.Name);
                throw new InvalidOperationException("Error adding tool to database", ex);
            }
        }

        public async Task UpdateAsync(Tool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool), "Tool cannot be null");

            try
            {
                _context.Entry(tool).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tool {ToolId} updated successfully", tool.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await ToolExistsAsync(tool.Id))
                    throw new ResourceNotFoundException(typeof(Tool));

                _logger.LogError(ex, "Concurrency error updating tool {ToolId}", tool.Id);
                throw new InvalidOperationException("Concurrency error updating tool", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update tool {ToolId}", tool.Id);
                throw new InvalidOperationException("Error updating tool in database", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var tool = await _context.Tools.FindAsync(id)
                    ?? throw new ResourceNotFoundException(typeof(Tool));

                _context.Tools.Remove(tool);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tool {ToolId} deleted successfully", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete tool {ToolId}", id);
                throw new InvalidOperationException("Error deleting tool from database", ex);
            }
        }

        private async Task<bool> ToolExistsAsync(int id)
            => await _context.Tools.AnyAsync(t => t.Id == id);
    }
}