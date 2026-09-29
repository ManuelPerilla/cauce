namespace NgMusic.Auth;

public sealed record StoredToken(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAt,
    string? DisplayName = null,
    string? Email = null);
