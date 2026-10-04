namespace Cauce.Terminal.Core;

public interface IMusicProvider
{
    Task<IReadOnlyList<MusicTrack>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
