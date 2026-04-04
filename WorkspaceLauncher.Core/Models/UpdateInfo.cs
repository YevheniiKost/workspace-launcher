namespace WorkspaceLauncher.Models;

public class UpdateInfo
{
    public Version LatestVersion { get; }
    public string TagName { get; }
    public string ReleaseNotes { get; }
    public string DownloadUrl { get; }
    public bool CanSelfUpdate { get; }

    public UpdateInfo(
        Version latestVersion,
        string tagName,
        string releaseNotes,
        string downloadUrl,
        bool canSelfUpdate)
    {
        LatestVersion = latestVersion;
        TagName = tagName;
        ReleaseNotes = releaseNotes;
        DownloadUrl = downloadUrl;
        CanSelfUpdate = canSelfUpdate;
    }
}

