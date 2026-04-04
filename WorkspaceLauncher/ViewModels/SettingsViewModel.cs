using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;

namespace WorkspaceLauncher.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly StartupService _startupService = new();

    public AppSettings Settings { get; }

    [ObservableProperty]
    private bool _runOnStartup;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private ObservableCollection<ProfileComboItem> _profileItems;

    [ObservableProperty]
    private ProfileComboItem? _selectedAutoLaunchProfile;

    public SettingsViewModel(AppSettings settings, List<Profile> profiles)
    {
        Settings = settings;
        _runOnStartup = settings.RunOnStartup;
        _startMinimized = settings.StartMinimized;

        _profileItems = new ObservableCollection<ProfileComboItem>();
        _profileItems.Add(new ProfileComboItem(null, "Do not auto-launch"));
        foreach (var p in profiles)
            _profileItems.Add(new ProfileComboItem(p.Id, p.Name));

        _selectedAutoLaunchProfile = _profileItems.FirstOrDefault(x => x.Id == settings.AutoLaunchProfileId)
            ?? _profileItems[0];
    }

    partial void OnRunOnStartupChanged(bool value)
    {
        Settings.RunOnStartup = value;
        _startupService.SetRunOnStartup(value);
    }

    partial void OnStartMinimizedChanged(bool value) => Settings.StartMinimized = value;

    partial void OnSelectedAutoLaunchProfileChanged(ProfileComboItem? value)
    {
        Settings.AutoLaunchProfileId = value?.Id;
    }
}

public record ProfileComboItem(Guid? Id, string Name);
