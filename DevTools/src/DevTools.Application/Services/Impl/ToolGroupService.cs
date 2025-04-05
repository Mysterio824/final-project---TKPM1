using DevTools.Application.DTOs.Request.ToolGroup;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Response.ToolGroup;

namespace DevTools.Application.Services.Impl
{
    public class ToolGroupService : IToolGroupService
    {
        public async Task<CreateToolGroupDto> CreateAsync(CreateToolGroupDto createTodoListModel)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponseDto> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ToolItemResponseDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<UpdateToolGroupResponseDto> UpdateAsync(int id, UpdateToolGroupDto updateTodoListModel)
        {
            throw new NotImplementedException();
        }
    }
}
