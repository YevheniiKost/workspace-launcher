using System.Text.Json.Serialization;

namespace WorkspaceLauncher.Models;

public enum ItemType
{
    Executable,
    Url,
    Folder
}

public class LaunchItem
{
    public ItemType Type { get; set; } = ItemType.Executable;
    public string Path { get; set; } = string.Empty;
    public int DelayMs { get; set; } = 0;
    public bool CloseOnSessionEnd { get; set; } = false;

    [JsonIgnore]
    public int? ProcessId { get; set; }
}
