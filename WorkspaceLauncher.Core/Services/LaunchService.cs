using System.Diagnostics;
using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.Services;

public class LaunchService
{
    private readonly Dictionary<Guid, List<Process>> _activeProcesses = new();

    public async Task LaunchProfileAsync(Profile profile, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!_activeProcesses.ContainsKey(profile.Id))
            _activeProcesses[profile.Id] = new List<Process>();

        foreach (var item in profile.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item.DelayMs > 0)
                await Task.Delay(item.DelayMs, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(item.Path);

            var process = LaunchItem(item);
            if (process != null)
            {
                item.ProcessId = process.Id;
                _activeProcesses[profile.Id].Add(process);
            }
        }
    }

    private Process? LaunchItem(LaunchItem item)
    {
        try
        {
            ProcessStartInfo psi;

            switch (item.Type)
            {
                case ItemType.Executable:
                    psi = new ProcessStartInfo(item.Path) { UseShellExecute = true };
                    break;
                case ItemType.Url:
                    psi = new ProcessStartInfo(item.Path) { UseShellExecute = true };
                    break;
                case ItemType.Folder:
                    psi = new ProcessStartInfo("explorer.exe", item.Path);
                    break;
                default:
                    return null;
            }

            return Process.Start(psi);
        }
        catch
        {
            return null;
        }
    }

    public async Task CloseSessionAsync(Profile profile)
    {
        var itemsToClose = profile.Items.Where(i => i.CloseOnSessionEnd).ToList();

        if (!_activeProcesses.TryGetValue(profile.Id, out var processes))
            return;

        var processesToKill = new List<Process>();

        foreach (var item in itemsToClose)
        {
            if (item.ProcessId.HasValue)
            {
                try
                {
                    var process = Process.GetProcessById(item.ProcessId.Value);
                    process.CloseMainWindow();
                    processesToKill.Add(process);
                }
                catch
                {
                    // Process already exited or not found
                }
            }
        }

        if (processesToKill.Count > 0)
        {
            await Task.Delay(3000);

            foreach (var process in processesToKill)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        foreach (var item in itemsToClose)
            item.ProcessId = null;

        processes.RemoveAll(p =>
        {
            try { return p.HasExited; }
            catch { return true; }
        });
    }

    public void ClearSession(Guid profileId)
    {
        if (_activeProcesses.TryGetValue(profileId, out var processes))
        {
            processes.Clear();
            _activeProcesses.Remove(profileId);
        }
    }
}
