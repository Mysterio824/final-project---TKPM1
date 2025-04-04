namespace DevTools.Domain.Entities
{
    public class ToolGroup
    {
        public int Id { get; set; }
        public required string Title { get; set; }

        public List<Tool> Items { get; } = new List<Tool>();
    }
}
