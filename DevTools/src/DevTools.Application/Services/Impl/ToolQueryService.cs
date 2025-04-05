using AutoMapper;
using DevTools.Infrastructure.Repositories;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using DevTools.Application.DTOs.Response.Tool;

namespace DevTools.Application.Services.Impl
{
    public class ToolQueryService(
        IToolRepository toolRepository,
        IFavoriteToolRepository favoriteToolRepository,
        IMapper mapper) : IToolQueryService
    {
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
        private readonly IFavoriteToolRepository _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolsAsync(UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetAll();
            var favoriteToolIds = await GetFavoriteToolIds(userId);

            return MapToToolDTOs(toolList, role, favoriteToolIds);
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolFavoriteAsync(UserRole role, int userId)
        {
            var tools = await _toolRepository.GetFavoriteAsync(userId);
            return MapToToolDTOs(tools, role, tools.Select(t => t.Id).ToHashSet());
        }

        public async Task<ToolItemResponseDto?> GetToolByIdAsync(int id, UserRole role, int userId = -1)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null) return null;

            var isFavorite = userId != -1 && await _favoriteToolRepository.GetAsync(userId, id) != null;
            return MapToToolDTO(tool, role, isFavorite);
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolsByNameAsync(string name, UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetByNameAsync(name);
            var favoriteToolIds = await GetFavoriteToolIds(userId);
            return MapToToolDTOs(toolList, role, favoriteToolIds);
        }

        private async Task<HashSet<int>> GetFavoriteToolIds(int userId)
            => userId != -1
                ? (await _favoriteToolRepository.GetAll(userId))?.Select(f => f.ToolId).ToHashSet() ?? new HashSet<int>()
                : new HashSet<int>();

        private IEnumerable<ToolItemResponseDto> MapToToolDTOs(IEnumerable<Tool> tools, UserRole role, HashSet<int> favoriteToolIds)
            => tools.Select(t => MapToToolDTO(t, role, favoriteToolIds.Contains(t.Id)));

        private ToolItemResponseDto MapToToolDTO(Tool tool, UserRole role, bool isFavorite)
        {
            var toolDto = _mapper.Map<ToolItemResponseDto>(tool, opt => opt.Items["UserRole"] = role);

            toolDto.IsFavorite = isFavorite;
            return toolDto;
        }
    }
}