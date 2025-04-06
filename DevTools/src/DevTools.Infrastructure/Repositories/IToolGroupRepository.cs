using DevTools.Domain.Entities;

namespace DevTools.Infrastructure.Repositories
{
    public interface IToolGroupRepository : IBaseRepository<ToolGroup> 
    {
        Task<IEnumerable<ToolGroup>> GetAll();
        Task<IEnumerable<ToolGroup>> GetByNameAsync(string name);
        Task<ToolGroup?> GetByIdAsync(int Id);   
    }
}
