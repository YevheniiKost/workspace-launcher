using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Controls;
using WorkspaceLauncher.ViewModels;

namespace WorkspaceLauncher.Services;

public class TrayService : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private readonly Window _mainWindow;
    private readonly MainViewModel _mainViewModel;

    public TrayService(Window mainWindow, MainViewModel mainViewModel)
    {
        _mainWindow = mainWindow;
        _mainViewModel = mainViewModel;
        InitializeTray();
    }

    private void InitializeTray()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "WorkspaceLauncher",
            Visibility = Visibility.Visible
        };

        _trayIcon.TrayMouseDoubleClick += (_, _) => ShowMainWindow();
        _trayIcon.ContextMenu = BuildContextMenu();

        _mainWindow.StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_mainWindow.WindowState == WindowState.Minimized)
            _mainWindow.Hide();
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();

        foreach (var profile in _mainViewModel.Profiles)
        {
            var item = new MenuItem { Header = profile.Name };
            var capturedProfile = profile;
            item.Click += async (_, _) => await _mainViewModel.LaunchProfileCommand.ExecuteAsync(capturedProfile);
            menu.Items.Add(item);
        }

        menu.Items.Add(new Separator());

        var openItem = new MenuItem { Header = "Open WorkspaceLauncher" };
        openItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(openItem);

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => Application.Current.Shutdown();
        menu.Items.Add(exitItem);

        return menu;
    }

    public void ShowMainWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    public void Dispose()
    {
        if (_trayIcon != null)
        {
            _mainWindow.StateChanged -= MainWindow_StateChanged;
            _trayIcon.Dispose();
            _trayIcon = null;
        }
    }
}
