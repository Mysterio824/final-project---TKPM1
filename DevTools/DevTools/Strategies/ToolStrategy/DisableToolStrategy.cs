using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class DisableToolStrategy : IToolActionStrategy
    {
        public string Execute(int id, IToolService toolService)
        {
            toolService.DisableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool disabled successfully";
    }
}
