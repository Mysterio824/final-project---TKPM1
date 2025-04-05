using DevTools.Application.DTOs.Request.ToolGroup;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Response.ToolGroup;

namespace DevTools.Application.Services
{
    public interface IToolGroupService
    {
        Task<CreateToolGroupDto> CreateAsync(CreateToolGroupDto createTodoListModel);

        Task<BaseResponseDto> DeleteAsync(int id);

        Task<IEnumerable<ToolItemResponseDto>> GetAllAsync();

        Task<UpdateToolGroupResponseDto> UpdateAsync(int id, UpdateToolGroupDto updateTodoListModel);
    }
}
