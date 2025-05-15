using AutoMapper;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.Exceptions;
using Microsoft.Extensions.Logging;
using DevTools.DataAccess.Repositories;

namespace DevTools.Application.Services.Impl
{
    public class ToolQueryService(
        IToolRepository toolRepository,
        IFavoriteToolRepository favoriteToolRepository,
        ILogger<ToolQueryService> logger,
        IMapper mapper
    ) : IToolQueryService
    {
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
        private readonly IFavoriteToolRepository _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
        private readonly ILogger<ToolQueryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolsByGroupIdAsync(int groupId, UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetByGroupAsync(groupId);
            var favoriteToolIds = await GetFavoriteToolIds(userId);
            return MapToToolItemDTOs(toolList, role, favoriteToolIds);
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolsAsync(UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.GetAll();
            var favoriteToolIds = await GetFavoriteToolIds(userId);

            return MapToToolItemDTOs(toolList, role, favoriteToolIds);
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolByGroupIdAsync(int id, UserRole role, int userId = -1)
        {
            var result = await _toolRepository.GetByGroupAsync(id);
            var favoriteToolIds = await GetFavoriteToolIds(userId);

            return MapToToolItemDTOs(result, role, favoriteToolIds);
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolFavoriteAsync(UserRole role, int userId)
        {
            var tools = await _toolRepository.GetFavoriteAsync(userId);
            return MapToToolItemDTOs(tools, role, tools.Select(t => t.Id).ToHashSet());
        }

        public async Task<ToolResponseDto?> GetToolByIdAsync(int id, UserRole role, int userId = -1)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null) throw new NotFoundException($"Tool with id {id} not found.");

            var isFavorite = userId != -1 && await _favoriteToolRepository.GetAsync(userId, id) != null;
            var res = await MapToToolDTO(tool, role, isFavorite);
            if (res.IsEnabled == false)
            {
                if (role == UserRole.Admin)
                {
                    return res;
                }
                throw new BadRequestException($"Tool with id {id} is disabled.");
            }
            return res;
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetToolsByNameAsync(string name, UserRole role, int userId = -1)
        {
            var toolList = await _toolRepository.SearchByNameAsync(name);
            var favoriteToolIds = await GetFavoriteToolIds(userId);
            return MapToToolItemDTOs(toolList, role, favoriteToolIds);
        }

        private async Task<HashSet<int>> GetFavoriteToolIds(int userId)
            => userId != -1
                ? (await _favoriteToolRepository.GetAll(userId))?.Select(f => f.ToolId).ToHashSet() ?? new HashSet<int>()
                : new HashSet<int>();

        private IEnumerable<ToolItemResponseDto> MapToToolItemDTOs(IEnumerable<Tool> tools, UserRole role, HashSet<int> favoriteToolIds)
            => tools.Select(t => MapToToolItemDTO(t, role, favoriteToolIds.Contains(t.Id)));

        private ToolItemResponseDto MapToToolItemDTO(Tool tool, UserRole role, bool isFavorite)
        {
            var toolDto = _mapper.Map<ToolItemResponseDto>(tool, opt => opt.Items["UserRole"] = role);

            toolDto.IsFavorite = isFavorite;
            return toolDto;
        }
        private async Task<ToolResponseDto> MapToToolDTO(Tool tool, UserRole role, bool isFavorite)
        {
            var toolDto = _mapper.Map<ToolResponseDto>(tool, opt => opt.Items["UserRole"] = role);

            toolDto.IsFavorite = isFavorite;

            var fileBytes = await File.ReadAllBytesAsync(tool.DllPath);

            toolDto.File = fileBytes;
            return toolDto;
        }
    }
}