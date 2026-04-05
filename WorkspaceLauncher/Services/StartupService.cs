using Microsoft.Win32;

namespace WorkspaceLauncher.Services;

public class StartupService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WorkspaceLauncher";

    public bool SetRunOnStartup(bool enable)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
            if (key == null)
            {
                return false;
            }

            if (enable)
            {
                string? exePath = Environment.ProcessPath
                    ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

                if (exePath == null)
                {
                    return false;
                }

                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsRunOnStartupEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            object? storedValue = key?.GetValue(AppName);
            if (storedValue == null)
            {
                return false;
            }

            string? currentExePath = Environment.ProcessPath
                ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

            if (currentExePath == null)
            {
                return false;
            }

            string storedString = storedValue.ToString() ?? string.Empty;
            return storedString.StartsWith($"\"{currentExePath}\"", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
