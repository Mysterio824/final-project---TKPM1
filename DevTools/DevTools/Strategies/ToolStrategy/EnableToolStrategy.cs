using DevTools.Interfaces;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class EnableToolStrategy : IToolActionStrategy
    {
        private readonly IToolService _toolService;

        public EnableToolStrategy(IToolService toolService)
        {
            _toolService = toolService;
        }

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.EnableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool enabled successfully";
    }
}
