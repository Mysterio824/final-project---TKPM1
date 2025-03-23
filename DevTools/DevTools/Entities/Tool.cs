// DevTools/Entities/Tool.cs
namespace DevTools.Entities;

public class Tool
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int GroupId { get; set; }
    public bool IsPremium { get; set; }
    public bool IsEnabled { get; set; }
    public string DllPath { get; set; }
}