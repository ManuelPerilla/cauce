namespace NgMusic.Auth;

public sealed record GoogleCredentials(string? ClientId, string? ClientSecret, string? ApiKey)
{
    public static GoogleCredentials FromEnvironment() => new(
        Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID"),
        Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_SECRET"),
        Environment.GetEnvironmentVariable("NGMUSIC_YOUTUBE_API_KEY"));

    public void EnsureOAuthConfigured()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException(
                "OAuth is not configured. Set NGMUSIC_GOOGLE_CLIENT_ID first.");
        }
    }
}
