using DevTools.Domain.Entities;
using DevTools.Infrastructure.Persistence;

namespace DevTools.Infrastructure.Repositories.impl
{
    public class ToolGroupRepository(DatabaseContext context) 
        : BaseRepository<ToolGroup>(context) , IToolGroupRepository { }
}
