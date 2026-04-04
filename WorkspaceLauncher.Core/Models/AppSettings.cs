namespace WorkspaceLauncher.Models;

public class AppSettings
{
    public bool RunOnStartup { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public Guid? AutoLaunchProfileId { get; set; } = null;
}
