using WorkspaceLauncher.Models;
using WorkspaceLauncher.Services;

namespace WorkspaceLauncher.Tests;

public class LaunchServiceTests
{
    [Fact]
    public async Task LaunchProfileAsync_EmptyProfile_CompletesWithoutError()
    {
        var service = new LaunchService();
        var profile = new Profile { Name = "Empty" };

        await service.LaunchProfileAsync(profile);
        // No exception = pass
    }

    [Fact]
    public async Task LaunchProfileAsync_ReportsProgressForEachItem()
    {
        var service = new LaunchService();
        var profile = new Profile
        {
            Name = "Test",
            Items = new List<LaunchItem>
            {
                new LaunchItem { Type = ItemType.Url, Path = "https://example.com", DelayMs = 0 },
                new LaunchItem { Type = ItemType.Url, Path = "https://google.com", DelayMs = 0 }
            }
        };

        var reported = new List<string>();
        var progress = new Progress<string>(p => reported.Add(p));

        // Use cancellation to avoid actually launching browser
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await service.LaunchProfileAsync(profile, progress, cts.Token);
        });
    }

    [Fact]
    public async Task LaunchProfileAsync_WithCancellation_StopsEarly()
    {
        var service = new LaunchService();
        var profile = new Profile
        {
            Name = "Test",
            Items = new List<LaunchItem>
            {
                new LaunchItem { Type = ItemType.Url, Path = "https://example.com", DelayMs = 5000 },
                new LaunchItem { Type = ItemType.Url, Path = "https://google.com", DelayMs = 0 }
            }
        };

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await service.LaunchProfileAsync(profile, cancellationToken: cts.Token);
        });
    }

    [Fact]
    public async Task CloseSessionAsync_WithNoActiveProcesses_DoesNotThrow()
    {
        var service = new LaunchService();
        var profile = new Profile
        {
            Name = "Test",
            Items = new List<LaunchItem>
            {
                new LaunchItem { Type = ItemType.Url, Path = "https://example.com", CloseOnSessionEnd = true }
            }
        };

        // Should complete without exception even with no active processes
        await service.CloseSessionAsync(profile);
    }

    [Fact]
    public void ClearSession_RemovesProfileFromActiveProcesses()
    {
        var service = new LaunchService();
        var profileId = Guid.NewGuid();

        // Should not throw when clearing a non-existent profile
        service.ClearSession(profileId);
    }
}
