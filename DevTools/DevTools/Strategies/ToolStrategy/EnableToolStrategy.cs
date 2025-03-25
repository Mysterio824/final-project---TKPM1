using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class EnableToolStrategy : IToolActionStrategy
    {
        public string Execute(int id, IToolService toolService)
        {
            toolService.EnableTool(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool enabled successfully";
    }
}
