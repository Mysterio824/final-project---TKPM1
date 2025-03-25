using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class SetFreeToolStrategy : IToolActionStrategy
    {
        public string Execute(int id, IToolService toolService)
        {
            toolService.SetFree(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool is set free successfully";
    }
}
