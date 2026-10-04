using System.Security.Cryptography;
using System.Text;

namespace Cauce.Core;

/// <summary>Imports references only. Audio stays in its original location.</summary>
public sealed class TrackImporter
{
    public const int MaximumFiles = 10_000;
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".mp3", ".wav", ".m4a" };

    public Task<IReadOnlyList<Track>> ImportFilesAsync(IEnumerable<string> files, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        cancellationToken.ThrowIfCancellationRequested();
        // Path checks and opening files can block, particularly on disconnected network drives.
        // Keep the entire bounded batch off the desktop dispatcher, including WAV/M4A fallback.
        return Task.Run(() => ImportCoreAsync(files, cancellationToken), cancellationToken);
    }

    private static async Task<IReadOnlyList<Track>> ImportCoreAsync(IEnumerable<string> files, CancellationToken cancellationToken)
    {
        var results = new List<Track>();
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var examined = 0;
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (++examined > MaximumFiles)
                throw new ArgumentException($"Una importación admite como máximo {MaximumFiles} archivos.", nameof(files));
            if (string.IsNullOrWhiteSpace(file)) continue;
            try
            {
                var path = Path.GetFullPath(file).Normalize(NormalizationForm.FormC);
                if (!Extensions.Contains(Path.GetExtension(path)) || !File.Exists(path) || !seen.Add(path)) continue;
                var title = Path.GetFileNameWithoutExtension(path);
                var artist = "";
                var genre = "";
                // ID3v1 is a fixed 128-byte footer; never load the audio into memory.
                if (Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.Asynchronous);
                    if (stream.Length >= 128)
                    {
                        stream.Seek(-128, SeekOrigin.End);
                        var footer = new byte[128];
                        await stream.ReadExactlyAsync(footer, cancellationToken).ConfigureAwait(false);
                        if (footer[0] == 'T' && footer[1] == 'A' && footer[2] == 'G')
                        {
                            var taggedTitle = ReadTag(footer, 3, 30);
                            if (taggedTitle.Length > 0) title = taggedTitle;
                            artist = ReadTag(footer, 33, 30);
                            if (footer[127] < Id3Genres.Length) genre = Id3Genres[footer[127]];
                        }
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
                var identity = OperatingSystem.IsWindows() ? path.ToUpperInvariant() : path;
                results.Add(new Track
                {
                    Id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))).ToLowerInvariant(),
                    Title = title, Artist = artist, Genre = genre, Location = path,
                    Source = TrackSource.LocalFile, IsAvailable = true
                });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                // A missing or unreadable selected file does not discard the rest of the batch.
            }
        }
        return results;
    }

    private static string ReadTag(byte[] bytes, int offset, int count) => Encoding.Latin1.GetString(bytes, offset, count).Trim('\0', ' ');

    // Standard ID3v1 genre indexes; unknown indexes stay unknown rather than being inferred.
    private static readonly string[] Id3Genres =
    [
        "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop", "Jazz", "Metal",
        "New Age", "Oldies", "Other", "Pop", "R&B", "Rap", "Reggae", "Rock", "Techno", "Industrial",
        "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk",
        "Fusion", "Trance", "Classical", "Instrumental", "Acid", "House", "Game", "Sound Clip", "Gospel", "Noise",
        "AlternRock", "Bass", "Soul", "Punk", "Space", "Meditative", "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic",
        "Darkwave", "Techno-Industrial", "Electronic", "Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy", "Cult", "Gangsta",
        "Top 40", "Christian Rap", "Pop/Funk", "Jungle", "Native American", "Cabaret", "New Wave", "Psychadelic", "Rave", "Showtunes",
        "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock"
    ];
}
