using DevTools.Application.Strategies.Core;
using DevTools.Application.Services;
using DevTools.Application.DTOs.Response.Tool;

namespace DevTools.Application.Strategies.ToolStrategies
{
    public class SetPremiumToolStrategy(IToolCommandService toolService) : IToolActionStrategy
    {
        private readonly IToolCommandService _toolService = toolService;

        public async Task<UpdateToolResponseDto> ExecuteAsync(int id)
            => await _toolService.SetPremium(id);
    }
}
