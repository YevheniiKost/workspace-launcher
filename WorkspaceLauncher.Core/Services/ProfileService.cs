using System.IO;
using System.Text.Json;
using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.Services;

public class ProfileData
{
    public AppSettings Settings { get; set; } = new();
    public List<Profile> Profiles { get; set; } = new();
}

public class ProfileService
{
    private readonly string _appDataFolder;
    private readonly string _profilesFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ProfileService(string? dataFolder = null)
    {
        _appDataFolder = dataFolder ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorkspaceLauncher");
        _profilesFilePath = Path.Combine(_appDataFolder, "profiles.json");
    }

    public ProfileData Load()
    {
        if (!File.Exists(_profilesFilePath))
            return new ProfileData();

        try
        {
            var json = File.ReadAllText(_profilesFilePath);
            return JsonSerializer.Deserialize<ProfileData>(json, JsonOptions) ?? new ProfileData();
        }
        catch
        {
            return new ProfileData();
        }
    }

    public void Save(List<Profile> profiles, AppSettings settings)
    {
        Directory.CreateDirectory(_appDataFolder);

        var data = new ProfileData
        {
            Settings = settings,
            Profiles = profiles
        };

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempFile = _profilesFilePath + ".tmp";

        File.WriteAllText(tempFile, json);
        File.Move(tempFile, _profilesFilePath, overwrite: true);
    }
}
