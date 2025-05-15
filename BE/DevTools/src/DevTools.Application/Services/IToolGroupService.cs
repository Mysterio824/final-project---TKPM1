using DevTools.Application.DTOs.Request.ToolGroup;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Response.ToolGroup;

namespace DevTools.Application.Services
{
    public interface IToolGroupService
    {
        Task<CreateToolGroupResponseDto> CreateAsync(CreateToolGroupDto request);

        Task<BaseResponseDto> DeleteAsync(int id);

        Task<IEnumerable<ToolGroupResponseDto>> GetAllAsync();

        Task<UpdateToolGroupResponseDto> UpdateAsync(UpdateToolGroupDto request);
    }
}
