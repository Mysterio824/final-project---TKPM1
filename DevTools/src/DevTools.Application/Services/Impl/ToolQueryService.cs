using DevTools.Application.DTOs.Response;
using DevTools.Infrastructure.Repositories;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DevTools.Application.Services.Impl
{
    public class ToolQueryService(
        IToolRepository toolRepository,
        IFavoriteToolRepository favoriteToolRepository,
        ILogger<ToolQueryService> logger) : IToolQueryService
    {
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
        private readonly IFavoriteToolRepository _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
        private readonly ILogger<ToolQueryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<ToolDto>> GetToolsAsync(UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetAllAsync();
            var favoriteToolIds = await GetFavoriteToolIds(userId);
            return MapToToolDTOs(toolList, role, favoriteToolIds);
        }

        public async Task<IEnumerable<ToolDto>> GetToolFavoriteAsync(UserRole role, int userId)
        {
            var tools = await _toolRepository.GetFavoriteAsync(userId);
            return MapToToolDTOs(tools, role, tools.Select(t => t.Id).ToHashSet());
        }

        public async Task<ToolDto?> GetToolByIdAsync(int id, UserRole role, int userId = -1)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null) return null;

            var isFavorite = userId != -1 && await _favoriteToolRepository.GetAsync(userId, id) != null;
            return MapToToolDTO(tool, role, isFavorite);
        }

        public async Task<IEnumerable<ToolDto>> GetToolsByNameAsync(string name, UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetByNameAsync(name);
            var favoriteToolIds = await GetFavoriteToolIds(userId);
            return MapToToolDTOs(toolList, role, favoriteToolIds);
        }

        private async Task<HashSet<int>> GetFavoriteToolIds(int userId)
            => userId != -1
                ? (await _favoriteToolRepository.GetAll(userId))?.Select(f => f.ToolId).ToHashSet() ?? []
                : [];

        private static IEnumerable<ToolDto> MapToToolDTOs(IEnumerable<Tool> tools, UserRole role, HashSet<int> favoriteToolIds)
            => tools.Select(t => MapToToolDTO(t, role, favoriteToolIds.Contains(t.Id)));

        private static ToolDto MapToToolDTO(Tool tool, UserRole role, bool isFavorite)
            => new()
            {
                Id = tool.Id,
                Name = tool.Name,
                Description = tool.Description,
                IsEnabled = IsToolAccessible(tool, role),
                IsPremium = tool.IsPremium,
                IsFavorite = isFavorite
            };

        private static bool IsToolAccessible(Tool tool, UserRole role)
            => !tool.IsPremium || role != UserRole.User && role != UserRole.Anonymous && tool.IsEnabled;
    }
}