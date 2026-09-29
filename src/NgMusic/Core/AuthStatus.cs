namespace NgMusic.Core;

public sealed record AuthStatus(bool IsAuthenticated, string? DisplayName = null, string? Email = null)
{
    public static AuthStatus SignedOut { get; } = new(false);
}
