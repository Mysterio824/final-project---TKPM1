using DevTools.Interfaces;
using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class SetPremiumToolStrategy : IToolActionStrategy
    {
        private readonly IToolService _toolService;

        public SetPremiumToolStrategy(IToolService toolService)
        {
            _toolService = toolService;
        }

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.SetPremium(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool set to premium successfully";
    }
}
