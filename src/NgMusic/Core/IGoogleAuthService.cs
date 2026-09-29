namespace NgMusic.Core;

public interface IGoogleAuthService
{
    Task<AuthStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<AuthStatus> LoginAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
