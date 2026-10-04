namespace Cauce.Terminal.Core;

public sealed record MusicTrack(string VideoId, string Title, string Artist)
{
    public string YouTubeUrl => $"https://www.youtube.com/watch?v={VideoId}";
}
