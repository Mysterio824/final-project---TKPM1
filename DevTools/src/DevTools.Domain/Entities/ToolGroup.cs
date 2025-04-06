using DevTools.Domain.Common;

namespace DevTools.Domain.Entities
{
    public class ToolGroup : BaseEntity
    {
        public required string Name { get; set; }

        public List<Tool> Items { get; } = new List<Tool>();
    }
}
