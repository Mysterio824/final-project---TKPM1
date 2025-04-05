using DevTools.Domain.Common;

namespace DevTools.Domain.Entities;

public class Tool : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public virtual ToolGroup Group { get; set; }
    public bool IsPremium { get; set; } = false;
    public bool IsEnabled { get; set; } = true;
    public required string DllPath { get; set; }
}