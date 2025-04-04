using DevTools.Application.Exceptions;
using DevTools.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Services.Impl
{
    public class FavoriteToolService(
        IFavoriteToolRepository favoriteToolRepository,
        IToolRepository toolRepository,
        ILogger<FavoriteToolService> logger) : IFavoriteToolService
    {
        private readonly IFavoriteToolRepository _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
        private readonly ILogger<FavoriteToolService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task AddFavoriteToolAsync(int userId, int toolId)
        {
            _logger.LogInformation("Starting to add favorite tool for user {UserId} and tool {ToolId}", userId, toolId);

            var tool = await _toolRepository.GetByIdAsync(toolId)
                ?? throw new NotFoundException($"Tool with ID {toolId} not found.");

            var existingFavorite = await _favoriteToolRepository.GetAsync(userId, toolId);
            if (existingFavorite != null)
            {
                _logger.LogInformation("Tool {ToolId} is already a favorite for user {UserId}", toolId, userId);
                return;
            }

            await _favoriteToolRepository.AddAsync(userId, toolId);
            _logger.LogInformation("Successfully added tool {ToolId} to favorites for user {UserId}", toolId, userId);
        }

        public async Task RemoveFavoriteToolAsync(int userId, int toolId)
        {
            _logger.LogInformation("Removing favorite tool {ToolId} for user {UserId}", toolId, userId);
            await _favoriteToolRepository.DeleteAsync(userId, toolId);
        }
    }
}