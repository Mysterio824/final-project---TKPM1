using DevTools.Application.Interfaces.Core;
using DevTools.Application.Interfaces.Services;

namespace DevTools.Infrastructure.Strategies.ToolStrategies
{
    public class SetFreeToolStrategy(IToolCommandService toolService) : IToolActionStrategy
    {
        private readonly IToolCommandService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.SetFree(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool set to free successfully";
    }
}
