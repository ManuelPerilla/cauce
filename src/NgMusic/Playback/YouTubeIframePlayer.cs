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
    private static readonly object ConsoleLock = new();
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

                if (path.Equals("/api/event", StringComparison.OrdinalIgnoreCase))
                {
                    HandlePlayerEvent(target);
                    await LoopbackHttp.WriteResponseAsync(
                        client,
                        "text/plain; charset=utf-8",
                        "ok",
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

    private static void HandlePlayerEvent(string target)
    {
        var uri = new Uri("http://127.0.0.1" + target);
        var query = ParseQuery(uri.Query);

        query.TryGetValue("type", out var type);
        query.TryGetValue("code", out var code);
        query.TryGetValue("videoId", out var videoId);

        if (string.Equals(type, "error", StringComparison.OrdinalIgnoreCase))
        {
            var message = code switch
            {
                "2" => "invalid video parameter",
                "5" => "HTML5 playback error",
                "100" => "video removed or private",
                "101" or "150" => "video owner does not allow embedded playback",
                "153" => "YouTube did not receive the required Referer/client identity",
                _ => "unknown player error"
            };

            WritePlayerMessage(
                $"YouTube player error {code ?? "?"}: {message}" +
                (string.IsNullOrWhiteSpace(videoId) ? string.Empty : $" (video {videoId})"),
                ConsoleColor.Red);
            return;
        }

        if (string.Equals(type, "autoplayBlocked", StringComparison.OrdinalIgnoreCase))
        {
            WritePlayerMessage(
                "YouTube/browser blocked scripted autoplay. Click the player once, then retry 'play' or 'resume'.",
                ConsoleColor.Yellow);
        }
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(pieces[0].Replace('+', ' '));
            var value = pieces.Length == 2
                ? Uri.UnescapeDataString(pieces[1].Replace('+', ' '))
                : string.Empty;
            result[key] = value;
        }
        return result;
    }

    private static void WritePlayerMessage(string text, ConsoleColor color)
    {
        lock (ConsoleLock)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine();
            Console.WriteLine($"! {text}");
            Console.ForegroundColor = previous;
            Console.Write("PS Music:\\> ");
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

    private sealed record PlayerCommand(
        string Type,
        string? VideoId = null,
        int? Seconds = null,
        int? Volume = null);

    private const string PlayerHtml = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <meta name="referrer" content="strict-origin-when-cross-origin" />
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
    let playerReady = false;
    let currentVideoId = '';

    function report(type, code = '') {
      const url = '/api/event?type=' + encodeURIComponent(type)
        + '&code=' + encodeURIComponent(String(code))
        + '&videoId=' + encodeURIComponent(currentVideoId || '');
      fetch(url, { cache: 'no-store' }).catch(() => {});
    }

    window.onYouTubeIframeAPIReady = () => {
      player = new YT.Player('player', {
        width: 960,
        height: 540,
        videoId: '',
        playerVars: {
          enablejsapi: 1,
          playsinline: 1,
          origin: window.location.origin
        },
        events: {
          onReady: () => {
            playerReady = true;
            report('ready');
          },
          onError: event => report('error', event.data),
          onAutoplayBlocked: () => report('autoplayBlocked')
        }
      });
    };

    async function tick() {
      try {
        // Do not consume queued commands until YouTube explicitly says the
        // player is ready to receive API calls. Otherwise the first play can
        // race the iframe initialization and surface error 2 even for a valid ID.
        if (!player || !playerReady) return;

        const command = await fetch('/api/command', { cache: 'no-store' }).then(r => r.json());
        if (!command || command.type === 'noop') return;

        if (command.type === 'load' && command.videoId) {
          currentVideoId = command.videoId;
          player.loadVideoById({
            videoId: command.videoId,
            startSeconds: 0
          });
        }
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
