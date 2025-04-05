using DevTools.Domain.Common;

namespace DevTools.Domain.Entities
{
    public class FavoriteTool : BaseEntity
    {
        public int UserId { get; set; }
        public int ToolId { get; set; }
    }
}