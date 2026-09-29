using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using NgMusic.Core;
using NgMusic.Infrastructure;

namespace NgMusic.Playback;

public sealed class YouTubeIframePlayer : IPlayer
{
    private readonly ConcurrentQueue<PlayerCommand> _commands = new();
    private readonly CancellationTokenSource _lifetime = new();
    private TcpListener? _listener;
    private Task? _serverTask;

    public async Task PlayAsync(MusicTrack track, CancellationToken cancellationToken = default)
    {
        await EnsureStartedAsync(cancellationToken);
        _commands.Enqueue(new PlayerCommand("load", track.VideoId));
    }

    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        await EnsureStartedAsync(cancellationToken);
        _commands.Enqueue(new PlayerCommand("pause"));
    }

    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureStartedAsync(cancellationToken);
        _commands.Enqueue(new PlayerCommand("play"));
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await EnsureStartedAsync(cancellationToken);
        _commands.Enqueue(new PlayerCommand("stop"));
    }

    public async Task SeekAsync(int seconds, CancellationToken cancellationToken = default)
    {
        await EnsureStartedAsync(cancellationToken);
        _commands.Enqueue(new PlayerCommand("seek", Seconds: Math.Max(0, seconds)));
    }

    public async Task SetVolumeAsync(int volume, CancellationToken cancellationToken = default)
    {
        await EnsureStartedAsync(cancellationToken);
        _commands.Enqueue(new PlayerCommand("volume", Volume: Math.Clamp(volume, 0, 100)));
    }

    private Task EnsureStartedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_listener is not null)
            return Task.CompletedTask;

        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        var playerUrl = $"http://127.0.0.1:{port}/";
        _serverTask = Task.Run(() => ServerLoopAsync(_lifetime.Token), _lifetime.Token);

        Process.Start(new ProcessStartInfo(playerUrl) { UseShellExecute = true });
        return Task.CompletedTask;
    }

    private async Task ServerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener is not null)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _ = Task.Run(() => HandleClientAsync(client), CancellationToken.None);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                var target = await LoopbackHttp.ReadTargetAsync(client, _lifetime.Token);
                var path = target.Split('?', 2)[0];

                if (path.Equals("/api/command", StringComparison.OrdinalIgnoreCase))
                {
                    var command = _commands.TryDequeue(out var queued)
                        ? queued
                        : new PlayerCommand("noop");
                    var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    await LoopbackHttp.WriteResponseAsync(
                        client,
                        "application/json; charset=utf-8",
                        json,
                        cancellationToken: _lifetime.Token);
                    return;
                }

                if (path is "/" or "/index.html")
                {
                    await LoopbackHttp.WriteResponseAsync(
                        client,
                        "text/html; charset=utf-8",
                        PlayerHtml,
                        cancellationToken: _lifetime.Token);
                    return;
                }

                await LoopbackHttp.WriteResponseAsync(
                    client,
                    "text/plain; charset=utf-8",
                    "Not found",
                    404,
                    "Not Found",
                    _lifetime.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _lifetime.Cancel();
        try { _listener?.Stop(); } catch { }
        if (_serverTask is not null)
        {
            try { await _serverTask; } catch (OperationCanceledException) { }
        }
        _lifetime.Dispose();
    }

    private sealed record PlayerCommand(string Type, string? VideoId = null, int? Seconds = null, int? Volume = null);

    private const string PlayerHtml = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>NgMusic Player</title>
  <style>
    :root { color-scheme: dark; }
    body { margin: 0; background: #0c0c0c; color: #d4d4d4; font-family: Consolas, ui-monospace, monospace; }
    .shell { max-width: 1000px; margin: 28px auto; padding: 0 18px; }
    .bar { display:flex; justify-content:space-between; margin-bottom:10px; color:#9cdcfe; }
    #player { width: 960px; max-width: 100%; aspect-ratio: 16/9; min-height: 200px; background:#000; }
    .hint { margin-top:12px; color:#858585; font-size:13px; }
    .ok { color:#4ec9b0; }
  </style>
</head>
<body>
  <div class="shell">
    <div class="bar"><span>PS Music:\&gt; <span class="ok">player-online</span></span><span>NgMusic</span></div>
    <div id="player"></div>
    <div class="hint">Keep this visible while NgMusic is playing. Control it from the terminal.</div>
  </div>
  <script src="https://www.youtube.com/iframe_api"></script>
  <script>
    let player;
    window.onYouTubeIframeAPIReady = () => {
      player = new YT.Player('player', {
        width: 960,
        height: 540,
        videoId: '',
        playerVars: { playsinline: 1, origin: window.location.origin }
      });
    };

    async function tick() {
      try {
        const command = await fetch('/api/command', { cache: 'no-store' }).then(r => r.json());
        if (!player || !command || command.type === 'noop') return;
        if (command.type === 'load' && command.videoId) player.loadVideoById(command.videoId);
        if (command.type === 'play') player.playVideo();
        if (command.type === 'pause') player.pauseVideo();
        if (command.type === 'stop') player.stopVideo();
        if (command.type === 'seek' && Number.isFinite(command.seconds)) player.seekTo(command.seconds, true);
        if (command.type === 'volume' && Number.isFinite(command.volume)) player.setVolume(command.volume);
      } catch (_) { }
    }
    setInterval(tick, 300);
  </script>
</body>
</html>
""";
}
