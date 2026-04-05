using System.Windows;

using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;
using WorkspaceLauncher.ViewModels;
using WorkspaceLauncher.Views;

namespace WorkspaceLauncher;

public partial class App : Application
{
    private TrayService? _trayService;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var profileService = new ProfileService();
        var launchService = new LaunchService();
        UpdateService updateService = new UpdateService();
        var mainVm = new MainViewModel(profileService, launchService);

        StartupService startupService = new StartupService();
        startupService.SetRunOnStartup(mainVm.Settings.RunOnStartup);

        var mainWindow = new MainWindow(mainVm);

        _trayService = new TrayService(mainWindow, mainVm);

        if (mainVm.Settings.StartMinimized)
        {
            mainWindow.Show();
            mainWindow.Hide();

            if (mainVm.Settings.AutoLaunchProfileId.HasValue)
            {
                var profile = mainVm.Profiles.FirstOrDefault(p => p.Model.Id == mainVm.Settings.AutoLaunchProfileId.Value);
                if (profile != null)
                    _ = mainVm.LaunchProfileCommand.ExecuteAsync(profile);
            }
        }
        else
        {
            mainWindow.Show();
        }

        _ = CheckForUpdateInBackgroundAsync(mainWindow, updateService);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        base.OnExit(e);
    }

    private async Task CheckForUpdateInBackgroundAsync(Window owner, UpdateService updateService)
    {
        UpdateInfo? updateInfo = await updateService.CheckForUpdateAsync();
        if (updateInfo == null)
        {
            return;
        }

        UpdateWindow updateWindow = new UpdateWindow(updateInfo, updateService);
        updateWindow.Owner = owner;
        updateWindow.ShowDialog();
    }
}
