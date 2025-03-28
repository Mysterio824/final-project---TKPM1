using DevTools.Interfaces.Core;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategies
{
    public class DisableToolStrategy(IToolService toolService) : IToolActionStrategy
    {
        private readonly IToolService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.DisableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool disabled successfully";
    }
}
