using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class SetFreeToolStrategy : IToolActionStrategy
    {
        private readonly IToolService _toolService;

        public SetFreeToolStrategy(IToolService toolService)
        {
            _toolService = toolService;
        }

        public async Task<string> ExecuteAsync(int id)
        {
            await _toolService.SetFree(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool set to free successfully";
    }
}
