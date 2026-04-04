namespace WorkspaceLauncher.Models;

public class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#4A90E2";
    public string IconPath { get; set; } = "📁";
    public List<LaunchItem> Items { get; set; } = new();
}
