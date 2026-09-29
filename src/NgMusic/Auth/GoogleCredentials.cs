using NgMusic.Settings;

namespace NgMusic.Auth;

public sealed class GoogleCredentials
{
    private readonly UserSettingsStore _settingsStore;
    private NgMusicSettings _settings;

    public GoogleCredentials(UserSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _settings = settingsStore.Load();
    }

    public string? ClientId =>
        FirstNonEmpty(
            Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID"),
            _settings.GoogleClientId);

    public string? ClientSecret =>
        Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_SECRET");

    public string? ApiKey =>
        Environment.GetEnvironmentVariable("NGMUSIC_YOUTUBE_API_KEY");

    public bool IsOAuthConfigured => !string.IsNullOrWhiteSpace(ClientId);

    public bool HasEnvironmentClientId =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID"));

    public string ClientIdSource =>
        HasEnvironmentClientId
            ? "environment variable"
            : !string.IsNullOrWhiteSpace(_settings.GoogleClientId)
                ? "NgMusic local config"
                : "not configured";

    public string ConfigPath => _settingsStore.FilePath;

    public void SetLocalClientId(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Google OAuth Client ID cannot be empty.", nameof(clientId));

        _settings = _settings with { GoogleClientId = clientId.Trim() };
        _settingsStore.Save(_settings);
    }

    public void ClearLocalConfig()
    {
        _settingsStore.Delete();
        _settings = new NgMusicSettings();
    }

    public void Reload()
    {
        _settings = _settingsStore.Load();
    }

    public void EnsureOAuthConfigured()
    {
        if (!IsOAuthConfigured)
        {
            throw new InvalidOperationException(
                "OAuth is not configured. Run 'setup' to configure your Google OAuth Client ID.");
        }
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
