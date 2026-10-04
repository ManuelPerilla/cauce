using System.Text.Json;

namespace Cauce.Terminal.Settings;

public sealed class UserSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UserSettingsStore()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DirectoryPath = Path.Combine(localAppData, "Cauce.Terminal");
        FilePath = Path.Combine(DirectoryPath, "config.json");
    }

    public string DirectoryPath { get; }
    public string FilePath { get; }

    public CauceTerminalSettings Load()
    {
        if (!File.Exists(FilePath))
            return new CauceTerminalSettings();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<CauceTerminalSettings>(json, JsonOptions) ?? new CauceTerminalSettings();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Cauce.Terminal configuration is invalid: {FilePath}. Run 'config reset' or fix the JSON file.",
                ex);
        }
    }

    public void Save(CauceTerminalSettings settings)
    {
        Directory.CreateDirectory(DirectoryPath);

        var tempPath = FilePath + ".tmp";
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, FilePath, overwrite: true);
    }

    public void Delete()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
}
