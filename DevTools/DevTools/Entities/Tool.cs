// DevTools/Entities/Tool.cs
using DevTools.Enums;

namespace DevTools.Entities;

public class Tool
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ToolType Type { get; set; }
    public bool IsPremium { get; set; } = false;
    public bool IsEnabled { get; set; } = true;
    public required string DllPath { get; set; }
}