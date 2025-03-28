using DevTools.Interfaces.Core;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategies
{
    public class SetPremiumToolStrategy(IToolService toolService) : IToolActionStrategy
    {
        private readonly IToolService _toolService = toolService;

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.SetPremium(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool set to premium successfully";
    }
}
