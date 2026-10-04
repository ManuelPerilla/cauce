using System.Text;
using System.Text.Json;
using Cauce.Core;

var tests = new (string Name, Func<Task> Run)[]
{
    ("queue: local available files only", QueueAvailability),
    ("queue: normalized strict genre", QueueGenre),
    ("queue: repeat policy and exhaustion", QueueRepeats),
    ("queue: artist spacing never silently relaxes", QueueSpacing),
    ("store: metadata roundtrip and current availability", StoreRoundtrip),
    ("store: corruption preserved and recovery bounded", StoreCorruption),
    ("store: future versions and unsafe overwrite refused", StoreFuture),
    ("store: malformed version is recoverable", StoreMalformedVersion),
    ("store: clear removes only owned metadata and recovery copies", StoreClear),
    ("import: filter, deduplicate, read tags, keep audio untouched", ImportFiles),
    ("import and save: cancellation", Cancellation)
};
var failures = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (FileLoadException ex) when (ex.HResult == unchecked((int)0x800711C7))
    {
        Console.Error.WriteLine("BLOCKED: Windows Application Control prevented loading the locally built library (0x800711C7). No result is claimed for the remaining checks.");
        return 2;
    }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {ex}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} checks passed.");
return failures == 0 ? 0 : 1;

static Track Song(string id, string artist = "A", string genre = "Rock") => new()
{ Id = id, Title = id, Artist = artist, Genre = genre, Location = Path.GetFullPath(id + ".mp3") };
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
static async Task Throws<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
static Task QueueAvailability()
{
    var planner = new QueuePlanner();
    Track[] tracks = [Song("missing") with { IsAvailable = false }, Song("link") with { Source = TrackSource.ExternalLink }, Song("ok")];
    Check(planner.Next(tracks, new(), []).Track?.Id == "ok", "Unavailable files or links were selected.");
    Check(planner.Next(tracks[..2], new(), []).Track is null, "Unavailable-only library must stop.");
    return Task.CompletedTask;
}
static Task QueueGenre()
{
    var planner = new QueuePlanner();
    Track[] tracks = [Song("rock"), Song("latin", genre: "  Electrónica  Latina ")];
    Check(planner.Next(tracks, new() { Genre = "electronica    latina" }, []).Track?.Id == "latin", "Genre normalization failed.");
    Check(planner.Next(tracks, new() { Genre = "Jazz" }, []).Track is null, "Unmatched genre was silently relaxed.");
    return Task.CompletedTask;
}
static Task QueueRepeats()
{
    var planner = new QueuePlanner();
    Track[] tracks = [Song("a", "A"), Song("b", "B")];
    Check(planner.Next(tracks, new() { ArtistSpacing = 0 }, ["a"]).Track?.Id == "b", "Repeated a played track.");
    var exhausted = planner.Next(tracks, new() { ArtistSpacing = 0 }, ["a", "b"]);
    Check(exhausted.Track is null && exhausted.Reason.Length > 0, "No-repeat exhaustion must explain the stop.");
    Check(planner.Next(tracks, new() { ArtistSpacing = 0, AllowRepeats = true }, ["a", "b"]).Track?.Id == "a", "Repeats should choose least recent.");
    return Task.CompletedTask;
}
static Task QueueSpacing()
{
    var planner = new QueuePlanner();
    Track[] tracks = [Song("a", "Beyoncé"), Song("b", " beyonce "), Song("c", "C")];
    Check(planner.Next(tracks, new(), ["a"]).Track?.Id == "c", "Artist normalization or spacing failed.");
    var stop = planner.Next(tracks, new(), ["a", "c"]);
    Check(stop.Track is null && stop.Reason.Contains("separación"), "Artist spacing was silently relaxed.");
    Check(planner.Next(tracks, new() { ArtistSpacing = 1 }, ["a", "c"]).Track?.Id == "b", "Spacing window used too much history.");
    return Task.CompletedTask;
}
static async Task WithDirectory(Func<string, Task> action)
{
    var directory = Path.Combine(Path.GetTempPath(), "cauce-core-tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    try { await action(directory); }
    finally { Directory.Delete(directory, recursive: true); }
}
static Task StoreRoundtrip() => WithDirectory(async directory =>
{
    var path = Path.Combine(directory, "track.mp3");
    await File.WriteAllBytesAsync(path, [1, 2, 3]);
    var store = new LibraryStore(directory);
    var state = new LibraryState
    {
        Tracks = [Song("one") with { Location = path }, Song("link") with { Location = "https://example.com/music", Source = TrackSource.ExternalLink }],
        Preferences = new() { Genre = "Rock", ReducedMotion = true, OnboardingCompleted = true, ArtistSpacing = 4 }
    };
    await store.SaveAsync(state);
    var loaded = await store.LoadAsync();
    Check(loaded.Tracks.Count == 2 && loaded.Tracks[0].IsAvailable, "Roundtrip lost tracks or availability.");
    Check(!loaded.Tracks[1].IsAvailable && loaded.Preferences == state.Preferences, "Preferences or external availability wrong.");
    using var json = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "library.json")));
    Check(json.RootElement.GetProperty("version").GetInt32() == 1, "Missing schema version.");
    Check(!json.RootElement.GetProperty("tracks")[0].TryGetProperty("isAvailable", out _), "Persisted transient availability.");
    var names = json.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();
    Check(names.SequenceEqual(new[] { "preferences", "tracks", "version" }), "Unexpected data persisted.");
    File.Delete(path);
    Check(!(await store.LoadAsync()).Tracks[0].IsAvailable, "Removed file is still available.");
    await store.SaveAsync(state with { Preferences = state.Preferences with { TipsEnabled = false } });
    Check(!(await store.LoadAsync()).Preferences.TipsEnabled, "Atomic replacement did not save.");
    Check(!Directory.GetFiles(directory, "*.tmp").Any(), "Temporary file was left behind.");
    await Throws<IOException>(() => store.SaveAsync(state with { Preferences = state.Preferences with { ArtistSpacing = 6 } }));
    Check((await store.LoadAsync()).Preferences.ArtistSpacing == 4, "Invalid spacing replaced saved preferences.");
});
static Task StoreCorruption() => WithDirectory(async directory =>
{
    var store = new LibraryStore(directory);
    var path = Path.Combine(directory, "library.json");
    for (var i = 1; i <= 3; i++)
    {
        await File.WriteAllTextAsync(path, $"damaged-{i}");
        var loaded = await store.LoadAsync();
        Check(loaded.Tracks.Count == 0 && store.LastLoadWarning is not null, "Recovery was silent.");
        Check(await File.ReadAllTextAsync(store.RecoveredFilePath!) == $"damaged-{i}", "Original corruption was not preserved.");
    }
    await File.WriteAllTextAsync(path, "damaged-4");
    await Throws<InvalidDataException>(() => store.LoadAsync());
    Check(await File.ReadAllTextAsync(path) == "damaged-4", "Recovery limit discarded the original.");
});
static Task StoreFuture() => WithDirectory(async directory =>
{
    var store = new LibraryStore(directory);
    var path = Path.Combine(directory, "library.json");
    const string future = "{\"version\":999,\"tracks\":[],\"preferences\":{}}";
    await File.WriteAllTextAsync(path, future);
    await Throws<InvalidDataException>(() => store.LoadAsync());
    await Throws<InvalidDataException>(() => store.SaveAsync(new()));
    Check(await File.ReadAllTextAsync(path) == future, "Future schema overwritten.");
    await File.WriteAllTextAsync(path, "broken");
    await Throws<JsonException>(() => store.SaveAsync(new()));
    Check(await File.ReadAllTextAsync(path) == "broken", "Save silently replaced corruption.");
});
static Task StoreMalformedVersion() => WithDirectory(async directory =>
{
    using var store = new LibraryStore(directory);
    var path = Path.Combine(directory, "library.json");
    const string malformed = "{\"version\":\"invalid\",\"tracks\":[],\"preferences\":{}}";
    await File.WriteAllTextAsync(path, malformed);
    Check((await store.LoadAsync()).Tracks.Count == 0 && store.LastLoadWarning is not null, "Invalid version did not recover.");
    Check(await File.ReadAllTextAsync(store.RecoveredFilePath!) == malformed, "Invalid version was not preserved.");
});
static Task StoreClear() => WithDirectory(async directory =>
{
    using var store = new LibraryStore(directory);
    var audio = Path.Combine(directory, "music.wav");
    await File.WriteAllBytesAsync(audio, [1, 2, 3]);
    await store.SaveAsync(new() { Tracks = [Song("music") with { Location = audio }] });
    for (var i = 1; i <= 3; i++)
        await File.WriteAllTextAsync(Path.Combine(directory, $"library.corrupt-{i}.json"), "old metadata");
    var temporary = Path.Combine(directory, $".library-{Guid.NewGuid():N}.tmp");
    await File.WriteAllTextAsync(temporary, "interrupted save");
    var unrelated = Path.Combine(directory, ".library-unrelated.tmp");
    await File.WriteAllTextAsync(unrelated, "leave me");
    var nested = Path.Combine(directory, "other-app");
    Directory.CreateDirectory(nested);
    await File.WriteAllTextAsync(Path.Combine(nested, "library.json"), "unrelated");
    await store.ClearAsync();
    Check(!File.Exists(Path.Combine(directory, "library.json")) && !File.Exists(temporary), "Owned metadata remains.");
    Check(!Directory.GetFiles(directory, "library.corrupt-*.json").Any(), "Recovery metadata remains.");
    Check(File.Exists(audio) && File.Exists(unrelated) && File.Exists(Path.Combine(nested, "library.json")), "Clear removed an unrelated file or audio.");
    Check((await store.LoadAsync()).Tracks.Count == 0, "Cleared store did not load empty.");
});
static Task ImportFiles() => WithDirectory(async directory =>
{
    var mp3 = Path.Combine(directory, "fallback.mp3");
    var bytes = new byte[256];
    Encoding.Latin1.GetBytes("TAG").CopyTo(bytes, 128);
    Encoding.Latin1.GetBytes("Tagged title").CopyTo(bytes, 131);
    Encoding.Latin1.GetBytes("Tagged artist").CopyTo(bytes, 161);
    bytes[255] = 17;
    await File.WriteAllBytesAsync(mp3, bytes);
    var wav = Path.Combine(directory, "local.wav");
    await File.WriteAllBytesAsync(wav, [0]);
    var text = Path.Combine(directory, "ignore.txt");
    await File.WriteAllTextAsync(text, "not music");
    var importer = new TrackImporter();
    var tracks = await importer.ImportFilesAsync([mp3, mp3, wav, text, Path.Combine(directory, "missing.m4a")]);
    Check(tracks.Count == 2, "Unsupported, missing, or duplicate file was imported.");
    Check(tracks[0].Title == "Tagged title" && tracks[0].Artist == "Tagged artist" && tracks[0].Genre == "Rock", "ID3v1 footer not read correctly.");
    Check(tracks[1].Title == "local" && tracks[1].Genre == "" && tracks[1].Artist == "", "Fallback fabricated metadata.");
    Check(tracks[0].Id == (await importer.ImportFilesAsync([mp3]))[0].Id && tracks[0].Id.Length == 64, "IDs are not deterministic SHA256.");
    Check((await File.ReadAllBytesAsync(mp3)).SequenceEqual(bytes), "Importer modified the audio.");
});
static Task Cancellation() => WithDirectory(async directory =>
{
    using var source = new CancellationTokenSource();
    source.Cancel();
    await Throws<OperationCanceledException>(() => new TrackImporter().ImportFilesAsync(["a.mp3"], source.Token));
    await Throws<OperationCanceledException>(() => new LibraryStore(directory).SaveAsync(new(), source.Token));
    Check(!File.Exists(Path.Combine(directory, "library.json")), "Canceled save modified the library.");
});
