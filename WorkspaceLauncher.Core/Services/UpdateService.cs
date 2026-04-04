using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.Services;

public class UpdateService
{
    public const string GitHubOwner = "your-username";
    public const string GitHubRepo = "workspace-launcher";
    public const string ReleaseAssetName = "WorkspaceLauncher.zip";

    private const string GitHubApiBase = "https://api.github.com";
    private const string HttpUserAgent = "WorkspaceLauncher-UpdateChecker";

    public static readonly Version CurrentVersion = new Version(1, 0, 0);

    private static readonly HttpClient s_httpClient = BuildHttpClient();

    private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private static HttpClient BuildHttpClient()
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(HttpUserAgent);
        return client;
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string url = $"{GitHubApiBase}/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            string json = await s_httpClient.GetStringAsync(url, cancellationToken);

            GitHubRelease? release = JsonSerializer.Deserialize<GitHubRelease>(json, s_jsonOptions);
            if (release == null)
            {
                return null;
            }

            string versionString = release.TagName.TrimStart('v', 'V');
            if (!Version.TryParse(versionString, out Version? latestVersion))
            {
                return null;
            }

            if (latestVersion <= CurrentVersion)
            {
                return null;
            }

            GitHubAsset? zipAsset = FindZipAsset(release.Assets);
            bool canSelfUpdate = zipAsset != null;
            string downloadUrl = canSelfUpdate ? zipAsset!.BrowserDownloadUrl : release.HtmlUrl;

            return new UpdateInfo(latestVersion, release.TagName, release.Body, downloadUrl, canSelfUpdate);
        }
        catch
        {
            return null;
        }
    }

    public async Task DownloadAndApplyUpdateAsync(
        UpdateInfo update,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), "WorkspaceLauncher_update");
        string zipPath = Path.Combine(Path.GetTempPath(), "WorkspaceLauncher_update.zip");
        string scriptPath = Path.Combine(Path.GetTempPath(), "wl_update.ps1");

        await DownloadFileAsync(update.DownloadUrl, zipPath, progress, cancellationToken);

        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, recursive: true);
        }

        ZipFile.ExtractToDirectory(zipPath, tempDirectory);

        string installDirectory = AppContext.BaseDirectory;
        int currentProcessId = Environment.ProcessId;
        string exePath = Path.Combine(installDirectory, "WorkspaceLauncher.exe");

        string script = BuildUpdaterScript(currentProcessId, tempDirectory, installDirectory, exePath);
        File.WriteAllText(scriptPath, script);

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -NonInteractive -WindowStyle Hidden -File \"{scriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }

    private static GitHubAsset? FindZipAsset(List<GitHubAsset> assets)
    {
        GitHubAsset? result = null;
        foreach (GitHubAsset asset in assets)
        {
            if (string.Equals(asset.Name, ReleaseAssetName, StringComparison.OrdinalIgnoreCase))
            {
                result = asset;
                break;
            }
        }

        return result;
    }

    private static async Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await s_httpClient.GetAsync(
            url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using FileStream fileStream = File.OpenWrite(destinationPath);

        byte[] buffer = new byte[81920];
        long bytesRead = 0;

        while (true)
        {
            int bytesInChunk = await contentStream.ReadAsync(buffer, cancellationToken);
            if (bytesInChunk == 0)
            {
                break;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesInChunk), cancellationToken);
            bytesRead += bytesInChunk;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                progress?.Report((double)bytesRead / totalBytes.Value * 100.0);
            }
        }
    }

    private static string BuildUpdaterScript(
        int processId,
        string sourceDirectory,
        string destinationDirectory,
        string exePath)
    {
        return
            $"Wait-Process -Id {processId} -ErrorAction SilentlyContinue\r\n" +
            $"Start-Sleep -Seconds 1\r\n" +
            $"Copy-Item -Path \"{sourceDirectory}\\*\" " +
                $"-Destination \"{destinationDirectory}\" -Recurse -Force\r\n" +
            $"Start-Process \"{exePath}\"\r\n";
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = new List<GitHubAsset>();
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}

