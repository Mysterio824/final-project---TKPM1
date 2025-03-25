using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy
{
    public class SetPremiumToolStrategy : IToolActionStrategy
    {
        public string Execute(int id, IToolService toolService)
        {
            toolService.SetPremium(id);
            return SuccessMessage;
        }

        public string SuccessMessage => "Tool is set premium successfully";
    }
}
