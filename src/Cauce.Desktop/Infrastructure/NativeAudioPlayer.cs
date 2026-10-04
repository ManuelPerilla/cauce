using System.Windows.Media;

namespace Cauce.Desktop.Infrastructure;

/// <summary>Owns one native decoder. All calls and callbacks stay on the UI dispatcher.</summary>
public sealed class NativeAudioPlayer : IDisposable
{
    private readonly MediaPlayer player = new();
    public event Action? Opened;
    public event Action? Ended;
    public event Action? Failed;
    public bool IsPlaying { get; private set; }
    public bool HasTrack { get; private set; }
    public TimeSpan Position => player.Position;
    public TimeSpan Duration => player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : TimeSpan.Zero;
    public double Volume { get => player.Volume; set => player.Volume = Math.Clamp(value, 0, 1); }

    public NativeAudioPlayer()
    {
        player.MediaOpened += (_, _) => { HasTrack = true; IsPlaying = true; player.Play(); Opened?.Invoke(); };
        player.MediaEnded += (_, _) => { IsPlaying = false; Ended?.Invoke(); };
        player.MediaFailed += (_, _) => { IsPlaying = false; HasTrack = false; Failed?.Invoke(); };
    }

    public void Open(string path)
    {
        Stop();
        player.Open(new Uri(System.IO.Path.GetFullPath(path), UriKind.Absolute));
    }
    public void Pause() { player.Pause(); IsPlaying = false; }
    public void Resume() { if (HasTrack) { player.Play(); IsPlaying = true; } }
    public void Stop() { player.Close(); IsPlaying = false; HasTrack = false; }
    public void Dispose() => player.Close();
}
