using DevTools.DataAccess.Persistence;
using DevTools.DataAccess.Repositories;
using DevTools.Domain.Entities;
using DevTools.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevTools.DataAccess.Repositories.impl
{
    public class ToolRepository(
        DatabaseContext context,
        ILogger<ToolRepository> logger) : BaseRepository<Tool>(context), IToolRepository
    {
        private readonly ILogger<ToolRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<Tool>> GetAll()
        {
            try
            {
                return await GetAllAsync(x => true);
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
                var listFav = await Context.FavoriteTools
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ToolId)
                    .ToListAsync();

                return await GetAllAsync(t => listFav.Contains(t.Id));
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
                return await GetFirstAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool with ID {ToolId}", id);
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }

        public async Task<IEnumerable<Tool>> GetByGroupAsync(int Id)
        {
            try
            {
                return await GetAllAsync(t => t.Group.Id == Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tools by group ID {GroupId}", Id);
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }

        public async Task<IEnumerable<Tool>> SearchByNameAsync(string name)
        {
            try
            {
                return await GetAllAsync(tool =>
                    tool.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool");
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }

        public async Task<Tool?> GetByNameAsync(string name)
        {
            try
            {
                return await GetFirstAsync(t => t.Name.ToLower().Trim() == name.ToLower().Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool with name {ToolName}", name);
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }
    }
}