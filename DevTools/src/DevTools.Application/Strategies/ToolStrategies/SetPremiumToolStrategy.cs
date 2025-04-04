using DevTools.Application.Strategies.Core;
using DevTools.Application.Services;

namespace DevTools.Application.Strategies.ToolStrategies
{
    public class SetPremiumToolStrategy(IToolCommandService toolService) : IToolActionStrategy
    {
        private readonly IToolCommandService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.SetPremium(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool set to premium successfully";
    }
}
