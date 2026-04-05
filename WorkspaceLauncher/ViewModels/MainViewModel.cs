using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;

namespace WorkspaceLauncher.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ProfileService _profileService;
    private readonly LaunchService _launchService;

    [ObservableProperty]
    private ObservableCollection<ProfileViewModel> _profiles = new();

    [ObservableProperty]
    private AppSettings _settings = new();

    [ObservableProperty]
    private bool _isLaunching;

    [ObservableProperty]
    private string _launchStatus = string.Empty;

    [ObservableProperty]
    private ProfileViewModel? _launchingProfile;

    public string AppVersionText => $"v{UpdateService.CurrentVersion}";

    public MainViewModel() : this(new ProfileService(), new LaunchService()) { }

    public MainViewModel(ProfileService profileService, LaunchService launchService)
    {
        _profileService = profileService;
        _launchService = launchService;
        LoadProfiles();
    }

    public void LoadProfiles()
    {
        var data = _profileService.Load();
        Settings = data.Settings;
        Profiles = new ObservableCollection<ProfileViewModel>(
            data.Profiles.Select(p => new ProfileViewModel(p)));
    }

    public void SaveProfiles()
    {
        var profiles = Profiles.Select(p => p.Model).ToList();
        _profileService.Save(profiles, Settings);
    }

    [RelayCommand]
    private void AddProfile()
    {
        var profile = new Profile { Name = "New Profile" };
        var vm = new ProfileViewModel(profile);
        var editor = new Views.ProfileEditorWindow(vm);
        if (editor.ShowDialog() == true)
        {
            Profiles.Add(vm);
            SaveProfiles();
        }
    }

    [RelayCommand]
    private void EditProfile(ProfileViewModel vm)
    {
        var editor = new Views.ProfileEditorWindow(vm);
        if (editor.ShowDialog() == true)
        {
            SaveProfiles();
        }
    }

    [RelayCommand]
    private void DeleteProfile(ProfileViewModel vm)
    {
        var result = MessageBox.Show(
            $"Delete profile \"{vm.Name}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Profiles.Remove(vm);
            SaveProfiles();
        }
    }

    [RelayCommand]
    private async Task LaunchProfile(ProfileViewModel vm)
    {
        if (IsLaunching) return;

        IsLaunching = true;
        LaunchingProfile = vm;
        vm.IsActive = true;

        var progress = new Progress<string>(path =>
        {
            LaunchStatus = path;
        });

        try
        {
            await _launchService.LaunchProfileAsync(vm.Model, progress);
        }
        finally
        {
            IsLaunching = false;
            LaunchingProfile = null;
            LaunchStatus = string.Empty;
        }
    }

    [RelayCommand]
    private async Task CloseSession(ProfileViewModel vm)
    {
        vm.IsActive = false;
        await _launchService.CloseSessionAsync(vm.Model);
    }

    [RelayCommand]
    private void OpenSettings()
    {
        bool originalRunOnStartup = Settings.RunOnStartup;
        bool originalStartMinimized = Settings.StartMinimized;
        Guid? originalAutoLaunchProfileId = Settings.AutoLaunchProfileId;

        var settingsVm = new SettingsViewModel(Settings, Profiles.Select(p => p.Model).ToList());
        var settingsWindow = new Views.SettingsWindow(settingsVm);
        if (settingsWindow.ShowDialog() == true)
        {
            Settings = settingsVm.Settings;
            SaveProfiles();
        }
        else
        {
            Settings.RunOnStartup = originalRunOnStartup;
            Settings.StartMinimized = originalStartMinimized;
            Settings.AutoLaunchProfileId = originalAutoLaunchProfileId;
        }
    }
}
