namespace NgMusic.Auth;

public interface ITokenStore
{
    Task<StoredToken?> ReadAsync(CancellationToken cancellationToken = default);
    Task WriteAsync(StoredToken token, CancellationToken cancellationToken = default);
    Task DeleteAsync(CancellationToken cancellationToken = default);
}
