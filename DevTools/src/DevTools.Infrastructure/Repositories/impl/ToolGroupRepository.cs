using DevTools.Domain.Entities;
using DevTools.Domain.Exceptions;
using DevTools.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace DevTools.Infrastructure.Repositories.impl
{
    public class ToolGroupRepository(
        DatabaseContext context,
        ILogger<ToolGroupRepository> logger) : BaseRepository<ToolGroup>(context) , IToolGroupRepository 
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<IEnumerable<ToolGroup>> GetAll()
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

        public async Task<ToolGroup?> GetByNameAsync (string name)
        {
            try
            {
                return await GetFirstAsync(t => t.Name.ToLower().Trim() == name.ToLower().Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool group with name {ToolName}", name);
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }

        public async Task<IEnumerable<ToolGroup>> SearchByNameAsync(string name)
        {
            try
            {
                return await GetAllAsync(tool =>
                    tool.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool group");
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }

        public async Task<ToolGroup?> GetByIdAsync(int id)
        {
            try
            {
                return await GetFirstAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tool group with ID {ToolId}", id);
                throw new ResourceNotFoundException(typeof(Tool));
            }
        }
    }
}
