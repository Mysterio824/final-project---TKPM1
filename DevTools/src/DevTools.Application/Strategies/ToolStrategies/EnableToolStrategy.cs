using DevTools.Application.Strategies.Core;
using DevTools.Application.Services;

namespace DevTools.Infrastructure.Strategies.ToolStrategies
{
    public class EnableToolStrategy(IToolCommandService toolService) : IToolActionStrategy
    {
        private readonly IToolCommandService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.EnableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool enabled successfully";
    }
}
