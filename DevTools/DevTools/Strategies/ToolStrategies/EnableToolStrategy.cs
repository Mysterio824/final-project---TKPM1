using DevTools.Interfaces.Core;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategies
{
    public class EnableToolStrategy(IToolService toolService) : IToolActionStrategy
    {
        private readonly IToolService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.EnableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool enabled successfully";
    }
}
