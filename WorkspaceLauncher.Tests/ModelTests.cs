using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.Tests;

public class ModelTests
{
    [Fact]
    public void LaunchItem_DefaultValues_AreCorrect()
    {
        var item = new LaunchItem();

        Assert.Equal(ItemType.Executable, item.Type);
        Assert.Equal(string.Empty, item.Path);
        Assert.Equal(0, item.DelayMs);
        Assert.False(item.CloseOnSessionEnd);
        Assert.Null(item.ProcessId);
    }

    [Fact]
    public void Profile_DefaultValues_AreCorrect()
    {
        var profile = new Profile();

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(string.Empty, profile.Name);
        Assert.Equal("#4A90E2", profile.Color);
        Assert.Equal("📁", profile.IconPath);
        Assert.NotNull(profile.Items);
        Assert.Empty(profile.Items);
    }

    [Fact]
    public void AppSettings_DefaultValues_AreCorrect()
    {
        var settings = new AppSettings();

        Assert.False(settings.RunOnStartup);
        Assert.False(settings.StartMinimized);
        Assert.Null(settings.AutoLaunchProfileId);
    }

    [Fact]
    public void Profile_EachInstance_HasUniqueId()
    {
        var p1 = new Profile();
        var p2 = new Profile();

        Assert.NotEqual(p1.Id, p2.Id);
    }

    [Fact]
    public void LaunchItem_ProcessId_IsNotSerializedByDefault()
    {
        var item = new LaunchItem { ProcessId = 1234 };

        var json = System.Text.Json.JsonSerializer.Serialize(item);

        Assert.DoesNotContain("processId", json.ToLower());
        Assert.DoesNotContain("ProcessId", json);
    }
}
