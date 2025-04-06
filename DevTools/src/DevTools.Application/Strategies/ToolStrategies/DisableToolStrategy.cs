using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.Services;
using DevTools.Application.Strategies.Core;

namespace DevTools.Application.Strategies.ToolStrategies
{
    public class DisableToolStrategy(IToolCommandService toolService) : IToolActionStrategy
    {
        private readonly IToolCommandService _toolService = toolService;

        public async Task<UpdateToolResponseDto> ExecuteAsync(int id)
        {
            await _toolService.DisableTool(id);
            return await _toolService.DisableTool(id);
        }
    }
}
