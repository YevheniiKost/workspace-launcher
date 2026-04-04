using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;

namespace WorkspaceLauncher.Tests;

public class ProfileServiceTests : IDisposable
{
    private readonly string _tempDir;

    public ProfileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "WLTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    private ProfileService CreateService() => new ProfileService(_tempDir);

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsDefaults()
    {
        var service = CreateService();
        var data = service.Load();

        Assert.NotNull(data);
        Assert.NotNull(data.Profiles);
        Assert.Empty(data.Profiles);
        Assert.NotNull(data.Settings);
        Assert.False(data.Settings.RunOnStartup);
        Assert.False(data.Settings.StartMinimized);
        Assert.Null(data.Settings.AutoLaunchProfileId);
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesProfiles()
    {
        var service = CreateService();

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Name = "Test Profile",
            Color = "#4A90E2",
            IconPath = "💼",
            Items = new List<LaunchItem>
            {
                new LaunchItem
                {
                    Type = ItemType.Executable,
                    Path = "C:\\test.exe",
                    DelayMs = 500,
                    CloseOnSessionEnd = true
                }
            }
        };

        var settings = new AppSettings
        {
            RunOnStartup = true,
            StartMinimized = false,
            AutoLaunchProfileId = profile.Id
        };

        service.Save(new List<Profile> { profile }, settings);
        var loaded = service.Load();

        Assert.Single(loaded.Profiles);
        var loadedProfile = loaded.Profiles[0];
        Assert.Equal(profile.Id, loadedProfile.Id);
        Assert.Equal(profile.Name, loadedProfile.Name);
        Assert.Equal(profile.Color, loadedProfile.Color);
        Assert.Equal(profile.IconPath, loadedProfile.IconPath);
        Assert.Single(loadedProfile.Items);

        var loadedItem = loadedProfile.Items[0];
        Assert.Equal(ItemType.Executable, loadedItem.Type);
        Assert.Equal("C:\\test.exe", loadedItem.Path);
        Assert.Equal(500, loadedItem.DelayMs);
        Assert.True(loadedItem.CloseOnSessionEnd);

        Assert.True(loaded.Settings.RunOnStartup);
        Assert.Equal(profile.Id, loaded.Settings.AutoLaunchProfileId);
    }

    [Fact]
    public void Save_IsAtomic_DoesNotLeaveTemporaryFiles()
    {
        var service = CreateService();
        var profiles = new List<Profile> { new Profile { Name = "Test" } };
        var settings = new AppSettings();

        service.Save(profiles, settings);

        var files = Directory.GetFiles(_tempDir);

        Assert.Single(files);
        Assert.EndsWith("profiles.json", files[0]);
    }

    [Fact]
    public void Load_WhenFileIsCorrupted_ReturnsDefaults()
    {
        var service = CreateService();
        File.WriteAllText(Path.Combine(_tempDir, "profiles.json"), "{ this is not valid json");
        var data = service.Load();

        Assert.NotNull(data);
        Assert.NotNull(data.Profiles);
        Assert.Empty(data.Profiles);
    }

    [Fact]
    public void Save_MultipleProfiles_PreservesAll()
    {
        var service = CreateService();

        var profiles = new List<Profile>
        {
            new Profile { Name = "Work", Color = "#4A90E2", IconPath = "💼" },
            new Profile { Name = "Music", Color = "#E24A4A", IconPath = "🎵" },
            new Profile { Name = "Personal", Color = "#4AE24A", IconPath = "🏠" }
        };

        service.Save(profiles, new AppSettings());
        var loaded = service.Load();

        Assert.Equal(3, loaded.Profiles.Count);
        Assert.Equal("Work", loaded.Profiles[0].Name);
        Assert.Equal("Music", loaded.Profiles[1].Name);
        Assert.Equal("Personal", loaded.Profiles[2].Name);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
