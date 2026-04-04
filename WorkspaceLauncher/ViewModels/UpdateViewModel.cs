using CommunityToolkit.Mvvm.ComponentModel;

using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;

namespace WorkspaceLauncher.ViewModels;

public partial class UpdateViewModel : ObservableObject
{
    public UpdateInfo UpdateInfo { get; }

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public bool IsNotDownloading => !IsDownloading;

    public string CurrentVersionText => UpdateService.CurrentVersion.ToString();
    public string NewVersionText => UpdateInfo.LatestVersion.ToString();
    public string UpdateButtonText => UpdateInfo.CanSelfUpdate ? "Update Now" : "Download";

    public UpdateViewModel(UpdateInfo updateInfo)
    {
        UpdateInfo = updateInfo;
    }

    partial void OnIsDownloadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotDownloading));
    }
}

