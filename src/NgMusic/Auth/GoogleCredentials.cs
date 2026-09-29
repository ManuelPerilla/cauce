using NgMusic.Settings;
using NgMusic.Shared;

namespace NgMusic.Auth;

public sealed class GoogleCredentials
{
    private readonly UserSettingsStore _settingsStore;
    private readonly WindowsCredentialSecretStore _secretStore =
        new("NgMusic.GoogleOAuth.ClientSecret");
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
        FirstNonEmpty(
            Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_SECRET"),
            _secretStore.Read());

    public string? ApiKey =>
        Environment.GetEnvironmentVariable("NGMUSIC_YOUTUBE_API_KEY");

    public bool IsOAuthConfigured => !string.IsNullOrWhiteSpace(ClientId);

    public bool HasEnvironmentClientId =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID"));

    public bool HasEnvironmentClientSecret =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_SECRET"));

    public string ClientIdSource =>
        HasEnvironmentClientId
            ? "environment variable"
            : !string.IsNullOrWhiteSpace(_settings.GoogleClientId)
                ? "NgMusic local config"
                : "not configured";

    public string ClientSecretSource =>
        HasEnvironmentClientSecret
            ? "environment variable"
            : !string.IsNullOrWhiteSpace(_secretStore.Read())
                ? "Windows Credential Manager"
                : "not configured";

    public string ConfigPath => _settingsStore.FilePath;

    public void SetLocalClientId(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Google OAuth Client ID cannot be empty.", nameof(clientId));

        _settings = _settings with { GoogleClientId = clientId.Trim() };
        _settingsStore.Save(_settings);
    }

    public void SetLocalClientSecret(string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Google OAuth Client Secret cannot be empty.", nameof(clientSecret));

        _secretStore.Write(clientSecret.Trim());
    }

    public void ClearLocalConfig()
    {
        _settingsStore.Delete();
        _secretStore.Delete();
        _settings = new NgMusicSettings();
    }

    public void Reload() => _settings = _settingsStore.Load();

    public void EnsureOAuthConfigured()
    {
        if (!IsOAuthConfigured)
            throw new InvalidOperationException(
                "OAuth is not configured. Run 'setup' to configure your Google OAuth Client ID.");
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
