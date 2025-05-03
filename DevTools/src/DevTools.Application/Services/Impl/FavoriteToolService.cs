using AutoMapper;
using DevTools.Application.DTOs.Response;
using DevTools.Application.Exceptions;
using DevTools.DataAccess.Repositories;
using DevTools.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Services.Impl
{
    public class FavoriteToolService(
        IFavoriteToolRepository favoriteToolRepository,
        IToolRepository toolRepository,
        IMapper mapper,
        ILogger<FavoriteToolService> logger) : IFavoriteToolService
    {
        private readonly IFavoriteToolRepository _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<FavoriteToolService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<BaseResponseDto> AddFavoriteToolAsync(int userId, int toolId)
        {
            _logger.LogInformation("Starting to add favorite tool for user {UserId} and tool {ToolId}", userId, toolId);

            var tool = await _toolRepository.GetByIdAsync(toolId)
                ?? throw new NotFoundException($"Tool with ID {toolId} not found.");

            var existingFavorite = await _favoriteToolRepository.GetAsync(userId, toolId);
            if (existingFavorite != null)
            {
                _logger.LogInformation("Tool {ToolId} is already a favorite for user {UserId}", toolId, userId);
                throw new BadRequestException($"Tool {tool.Name} is already in favorites.");
            }

            var favoriteTool = new FavoriteTool
            {
                UserId = userId,
                ToolId = toolId
            };

            var favTool = await _favoriteToolRepository.AddAsync(favoriteTool);
            _logger.LogInformation("Successfully added tool {ToolId} to favorites for user {UserId}", toolId, userId);
            return _mapper.Map<BaseResponseDto>(favTool);
        }

        public async Task<BaseResponseDto> RemoveFavoriteToolAsync(int userId, int toolId)
        {
            _logger.LogInformation("Removing favorite tool {ToolId} for user {UserId}", toolId, userId);

            var favoriteTool = await _favoriteToolRepository.GetAsync(userId, toolId)
                ?? throw new NotFoundException($"Favorite tool with ID {toolId} not found for user {userId}.");

            await _favoriteToolRepository.DeleteAsync(favoriteTool);

            return _mapper.Map<BaseResponseDto>(favoriteTool);
        }
    }
}