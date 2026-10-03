using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cauce.Core;

/// <summary>Versioned local metadata only. No audio, credentials, or listening history are stored.</summary>
public sealed class LibraryStore : IDisposable
{
    public const int SchemaVersion = 1;
    public const int MaximumTracks = 10_000;
    public const int MaximumBytes = 16 * 1024 * 1024;
    private readonly string _directory;
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 16,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter<TrackSource>() }
    };

    public string? LastLoadWarning { get; private set; }
    public string? RecoveredFilePath { get; private set; }

    public LibraryStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = Path.GetFullPath(directory);
        _path = Path.Combine(_directory, "library.json");
    }

    public async Task<LibraryState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            LastLoadWarning = null;
            RecoveredFilePath = null;
            if (!File.Exists(_path)) return new();
            try
            {
                var state = await ReadAsync(cancellationToken).ConfigureAwait(false);
                // Availability is machine state, never trusted from the saved file.
                return state with
                {
                    Tracks = state.Tracks.Select(t => t with
                    {
                        IsAvailable = t.Source == TrackSource.LocalFile && File.Exists(t.Location)
                    }).ToList()
                };
            }
            catch (Exception ex) when (ex is JsonException or CorruptLibraryException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var recovery = FindRecoveryPath();
                File.Move(_path, recovery, overwrite: false);
                RecoveredFilePath = recovery;
                LastLoadWarning = $"No pudimos leer la biblioteca. Conservamos el original en {Path.GetFileName(recovery)} y abrimos una biblioteca vacía.";
                return new();
            }
        }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(LibraryState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        state = state with { Tracks = state.Tracks?.ToList()! };
        Validate(state);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporaryPath = null;
        try
        {
            // Refuse to replace externally damaged or newer data; LoadAsync can preserve corruption.
            if (File.Exists(_path)) await ReadAsync(cancellationToken).ConfigureAwait(false);
            Directory.CreateDirectory(_directory);
            var data = JsonSerializer.SerializeToUtf8Bytes(new Envelope
            {
                Version = SchemaVersion,
                Tracks = state.Tracks.Select(t => new StoredTrack
                {
                    Id = t.Id, Title = t.Title, Artist = t.Artist, Genre = t.Genre,
                    Location = t.Location, Source = t.Source
                }).ToList(),
                Preferences = state.Preferences
            }, Options);
            if (data.Length > MaximumBytes) throw new InvalidDataException("La biblioteca supera el límite de almacenamiento local.");
            temporaryPath = Path.Combine(_directory, $".library-{Guid.NewGuid():N}.tmp");
            await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                8192, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(_path)) File.Replace(temporaryPath, _path, destinationBackupFileName: null);
            else File.Move(temporaryPath, _path);
            temporaryPath = null;
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try { File.Delete(temporaryPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            _gate.Release();
        }
    }

    /// <summary>Deletes only this store's metadata and recovery copies; audio and unrelated files stay untouched.</summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(_directory)) return;
            var paths = new List<string> { _path };
            for (var i = 1; i <= 3; i++) paths.Add(Path.Combine(_directory, $"library.corrupt-{i}.json"));
            foreach (var candidate in Directory.EnumerateFiles(_directory, ".library-*.tmp", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(candidate);
                // Only the exact UUID form generated by SaveAsync belongs to this store.
                if (name.Length == 45 && Guid.TryParseExact(name.AsSpan(9, 32), "N", out _)) paths.Add(candidate);
            }
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.Delete(path);
            }
            LastLoadWarning = null;
            RecoveredFilePath = null;
        }
        finally { _gate.Release(); }
    }

    private async Task<LibraryState> ReadAsync(CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous);
        if (stream.Length > MaximumBytes) throw new CorruptLibraryException("La biblioteca supera el límite de tamaño.");
        // The handle excludes writers, so the size checked above also bounds the parse.
        using var document = await JsonDocument.ParseAsync(stream, new JsonDocumentOptions { MaxDepth = 16 }, cancellationToken).ConfigureAwait(false);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            !document.RootElement.TryGetProperty("version", out var version) ||
            version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var number))
            throw new CorruptLibraryException("La biblioteca no contiene una versión válida.");
        if (number > SchemaVersion)
            throw new InvalidDataException("Esta biblioteca pertenece a una versión más reciente de Cauce. No se modificó el archivo.");
        if (number != SchemaVersion) throw new CorruptLibraryException("Versión de biblioteca no válida.");
        var envelope = document.RootElement.Deserialize<Envelope>(Options)
            ?? throw new CorruptLibraryException("La biblioteca está vacía.");
        if (envelope.Tracks is null || envelope.Preferences is null || envelope.Tracks.Any(t => t is null))
            throw new CorruptLibraryException("Faltan datos obligatorios de la biblioteca.");
        var state = new LibraryState
        {
            Tracks = envelope.Tracks.Select(t => new Track
            {
                Id = t.Id, Title = t.Title, Artist = t.Artist, Genre = t.Genre,
                Location = t.Location, Source = t.Source
            }).ToList(),
            Preferences = envelope.Preferences
        };
        Validate(state);
        return state;
    }

    private string FindRecoveryPath()
    {
        for (var i = 1; i <= 3; i++)
        {
            var candidate = Path.Combine(_directory, $"library.corrupt-{i}.json");
            if (!File.Exists(candidate)) return candidate;
        }
        throw new InvalidDataException("Ya existen tres copias de recuperación. Conservamos la biblioteca sin modificar; revisa esas copias antes de continuar.");
    }

    private static void Validate(LibraryState state)
    {
        if (state.Tracks is null || state.Preferences is null || state.Tracks.Count > MaximumTracks)
            throw new CorruptLibraryException("La biblioteca no es válida o contiene demasiadas canciones.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var locations = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var track in state.Tracks)
        {
            if (track is null || !TextValid(track.Id, 256, required: true) || !ids.Add(track.Id) ||
                !TextValid(track.Title, 2048) || !TextValid(track.Artist, 2048) || !TextValid(track.Genre, 256) ||
                !TextValid(track.Location, 4096, required: true) || !Enum.IsDefined(track.Source))
                throw new CorruptLibraryException("Hay una canción inválida o duplicada en la biblioteca.");
            if (track.Source == TrackSource.LocalFile)
            {
                if (!Path.IsPathFullyQualified(track.Location) || !locations.Add(track.Location))
                    throw new CorruptLibraryException("Una ubicación de archivo no es válida o está duplicada.");
            }
            else if (!Uri.TryCreate(track.Location, UriKind.Absolute, out var uri) ||
                     (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) || !string.IsNullOrEmpty(uri.UserInfo))
                throw new CorruptLibraryException("El enlace externo no es válido.");
        }
        if (!TextValid(state.Preferences.Theme, 32, required: true) || !TextValid(state.Preferences.Genre, 256) ||
            state.Preferences.ArtistSpacing is < 0 or > 5)
            throw new CorruptLibraryException("Las preferencias guardadas no son válidas.");
    }

    private static bool TextValid(string? value, int maximum, bool required = false) =>
        value is not null && value.Length <= maximum && (!required || !string.IsNullOrWhiteSpace(value));

    /// <summary>Dispose after outstanding load/save operations complete.</summary>
    public void Dispose() => _gate.Dispose();

    private sealed class CorruptLibraryException(string message) : IOException(message);

    private sealed record Envelope
    {
        public int Version { get; init; }
        public List<StoredTrack> Tracks { get; init; } = null!;
        public AppPreferences Preferences { get; init; } = null!;
    }

    private sealed record StoredTrack
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public string Artist { get; init; } = "";
        public string Genre { get; init; } = "";
        public string Location { get; init; } = "";
        public TrackSource Source { get; init; }
    }
}
