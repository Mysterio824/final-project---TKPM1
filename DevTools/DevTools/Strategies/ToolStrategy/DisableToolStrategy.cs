using DevTools.Interfaces;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class DisableToolStrategy : IToolActionStrategy
    {
        private readonly IToolService _toolService;

        public DisableToolStrategy(IToolService toolService)
        {
            _toolService = toolService;
        }

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.DisableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool disabled successfully";
    }
}
