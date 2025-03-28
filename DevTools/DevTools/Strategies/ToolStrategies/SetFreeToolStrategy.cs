using DevTools.Interfaces.Core;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategies
{
    public class SetFreeToolStrategy(IToolService toolService) : IToolActionStrategy
    {
        private readonly IToolService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.SetFree(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool set to free successfully";
    }
}
