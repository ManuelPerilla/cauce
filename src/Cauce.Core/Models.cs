namespace Cauce.Core;

public enum TrackSource { LocalFile, ExternalLink }

public sealed record Track
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Artist { get; init; } = "";
    public string Genre { get; init; } = "";
    public string Location { get; init; } = "";
    public TrackSource Source { get; init; } = TrackSource.LocalFile;
    public bool IsAvailable { get; init; } = true;
}

public sealed record SessionRules
{
    public string Genre { get; init; } = "Todos";
    public int ArtistSpacing { get; init; } = 2;
    public bool AllowRepeats { get; init; }
}

public sealed record QueueChoice(Track? Track, string Reason);

public sealed record AppPreferences
{
    public string Theme { get; init; } = "Sistema";
    public bool ReducedMotion { get; init; }
    public bool ReducedTransparency { get; init; }
    public bool TipsEnabled { get; init; } = true;
    public bool OnboardingCompleted { get; init; }
    public string Genre { get; init; } = "Todos";
    public bool AllowRepeats { get; init; }
    public int ArtistSpacing { get; init; } = 2;
}

public sealed record LibraryState
{
    public List<Track> Tracks { get; init; } = [];
    public AppPreferences Preferences { get; init; } = new();
}
