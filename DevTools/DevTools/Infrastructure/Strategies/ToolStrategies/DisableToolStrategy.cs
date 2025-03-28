using DevTools.Application.Interfaces.Core;
using DevTools.Application.Interfaces.Services;

namespace DevTools.Infrastructure.Strategies.ToolStrategies
{
    public class DisableToolStrategy(IToolCommandService toolService) : IToolActionStrategy
    {
        private readonly IToolCommandService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.DisableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool disabled successfully";
    }
}
