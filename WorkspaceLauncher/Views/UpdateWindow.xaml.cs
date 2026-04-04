using System.Diagnostics;
using System.Windows;

using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;
using WorkspaceLauncher.ViewModels;

namespace WorkspaceLauncher.Views;

public partial class UpdateWindow : Window
{
    private readonly UpdateViewModel _viewModel;
    private readonly UpdateService _updateService;

    public UpdateWindow(UpdateInfo updateInfo, UpdateService updateService)
    {
        InitializeComponent();
        _updateService = updateService;
        _viewModel = new UpdateViewModel(updateInfo);
        DataContext = _viewModel;
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.UpdateInfo.CanSelfUpdate)
        {
            OpenBrowserDownload();
            return;
        }

        _ = PerformSelfUpdateAsync();
    }

    private void RemindLater_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OpenBrowserDownload()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _viewModel.UpdateInfo.DownloadUrl,
            UseShellExecute = true
        });
        Close();
    }

    private async Task PerformSelfUpdateAsync()
    {
        try
        {
            _viewModel.IsDownloading = true;
            _viewModel.StatusText = "Downloading...";

            IProgress<double> progress = new Progress<double>(value =>
            {
                _viewModel.DownloadProgress = value;
                _viewModel.StatusText = $"Downloading... {value:F0}%";
            });

            await _updateService.DownloadAndApplyUpdateAsync(_viewModel.UpdateInfo, progress);

            _viewModel.StatusText = "Restarting...";
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _viewModel.IsDownloading = false;
            _viewModel.StatusText = string.Empty;
            MessageBox.Show(
                $"Update failed: {ex.Message}",
                "Update Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

