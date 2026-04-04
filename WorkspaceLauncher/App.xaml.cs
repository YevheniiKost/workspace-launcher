using System.Windows;
using WorkspaceLauncher.Services;
using WorkspaceLauncher.ViewModels;
using WorkspaceLauncher.Views;

namespace WorkspaceLauncher;

public partial class App : Application
{
    private TrayService? _trayService;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        bool startMinimized = e.Args.Contains("--minimized");

        var profileService = new ProfileService();
        var launchService = new LaunchService();
        var mainVm = new MainViewModel(profileService, launchService);

        var mainWindow = new MainWindow(mainVm);

        _trayService = new TrayService(mainWindow, mainVm);

        if (startMinimized || mainVm.Settings.StartMinimized)
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
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        base.OnExit(e);
    }
}
