using DevTools.Domain.Entities;

namespace DevTools.DataAccess.Repositories
{
    public interface IToolGroupRepository : IBaseRepository<ToolGroup>
    {
        Task<IEnumerable<ToolGroup>> GetAll();
        Task<ToolGroup?> GetByNameAsync(string name);
        Task<IEnumerable<ToolGroup>> SearchByNameAsync(string name);
        Task<ToolGroup?> GetByIdAsync(int Id);
    }
}
