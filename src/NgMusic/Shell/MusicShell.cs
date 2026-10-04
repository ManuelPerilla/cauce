using System.Diagnostics;
using NgMusic.Auth;
using NgMusic.Core;

namespace NgMusic.Shell;

public sealed class MusicShell(
    IGoogleAuthService auth,
    IMusicProvider provider,
    IPlayer player,
    GoogleCredentials credentials)
{
    private const string SetupGuideUrl =
        "https://github.com/ManuelPerilla/cauce/blob/main/docs/configuration.md";

    private IReadOnlyList<MusicTrack> _searchResults = [];
    private readonly Queue<MusicTrack> _queue = new();
    private readonly Stack<MusicTrack> _history = new();
    private MusicTrack? _current;
    private int _volume = 70;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.Title = "NgMusic";
        Banner();

        while (!cancellationToken.IsCancellationRequested)
        {
            Prompt();
            var input = Console.ReadLine();
            if (input is null)
                break;

            var args = CommandLineParser.Parse(input);
            if (args.Count == 0)
                continue;

            try
            {
                var shouldExit = await ExecuteAsync(args, cancellationToken);
                if (shouldExit)
                    break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }

    private async Task<bool> ExecuteAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        var command = args[0].ToLowerInvariant();
        switch (command)
        {
            case "help":
            case "?":
                Help();
                break;
            case "setup":
            case "configure":
                RunSetupWizard(force: true);
                break;
            case "config":
                Config(args.Skip(1).ToArray());
                break;
            case "login":
                await LoginAsync(cancellationToken);
                break;
            case "logout":
                await auth.LogoutAsync(cancellationToken);
                Success("Signed out. OAuth token removed from Windows Credential Manager.");
                break;
            case "whoami":
                await WhoAmIAsync(cancellationToken);
                break;
            case "search":
            case "s":
                await SearchAsync(args.Skip(1), cancellationToken);
                break;
            case "play":
            case "p":
                await PlayAsync(args.Skip(1).ToArray(), cancellationToken);
                break;
            case "pause":
                await player.PauseAsync(cancellationToken);
                Muted("paused");
                break;
            case "resume":
                await player.ResumeAsync(cancellationToken);
                Muted("resumed");
                break;
            case "stop":
                await player.StopAsync(cancellationToken);
                _current = null;
                Muted("stopped");
                break;
            case "next":
            case "n":
                await NextAsync(cancellationToken);
                break;
            case "prev":
            case "previous":
                await PreviousAsync(cancellationToken);
                break;
            case "seek":
                await SeekAsync(args.Skip(1).FirstOrDefault(), cancellationToken);
                break;
            case "volume":
            case "vol":
                await VolumeAsync(args.Skip(1).FirstOrDefault(), cancellationToken);
                break;
            case "queue":
            case "q":
                await QueueAsync(args.Skip(1).ToArray(), cancellationToken);
                break;
            case "now":
            case "np":
                NowPlaying();
                break;
            case "clear":
            case "cls":
                Console.Clear();
                Banner();
                break;
            case "exit":
            case "quit":
                return true;
            default:
                Error($"Unknown command '{args[0]}'. Try 'help'.");
                break;
        }

        return false;
    }

    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        credentials.Reload();

        if (!credentials.IsOAuthConfigured && !RunSetupWizard(force: false))
            return;

        Muted("Opening Google OAuth in your browser...");
        var status = await auth.LoginAsync(cancellationToken);
        Success($"Connected as {status.DisplayName ?? status.Email ?? "Google user"}");
    }

    private bool RunSetupWizard(bool force)
    {
        credentials.Reload();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Google OAuth setup");
        Console.WriteLine("------------------");
        Console.ResetColor();

        if (credentials.IsOAuthConfigured)
        {
            Success($"OAuth is configured via {credentials.ClientIdSource}.");

            if (credentials.HasEnvironmentClientId)
            {
                Muted("NGMUSIC_GOOGLE_CLIENT_ID currently takes precedence over local config.");
                Muted("Remove that environment variable if you want NgMusic to use a locally saved Client ID.");
                return true;
            }

            if (!force)
                return true;

            Console.Write("Replace the saved Client ID? [y/N]: ");
            var replace = Console.ReadLine()?.Trim();
            if (!string.Equals(replace, "y", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(replace, "yes", StringComparison.OrdinalIgnoreCase))
            {
                Muted("configuration unchanged");
                return true;
            }
        }
        else
        {
            Console.WriteLine("NgMusic needs a Google OAuth Client ID of type Desktop app.");
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine(" [1] Paste Google OAuth Client ID");
            Console.WriteLine(" [2] Open step-by-step setup instructions");
            Console.WriteLine(" [3] Cancel");
            Console.WriteLine();
            Console.Write("Choose an option: ");

            var choice = Console.ReadLine()?.Trim();
            switch (choice)
            {
                case "1":
                    return ReadAndSaveClientId();

                case "2":
                    OpenSetupInstructions();
                    Muted("Instructions opened in your browser. Return here when your Desktop Client ID is ready.");
                    break;

                case "3":
                case "":
                    Muted("OAuth setup cancelled.");
                    return false;

                default:
                    Error("Choose 1, 2, or 3.");
                    break;
            }
        }
    }

    private bool ReadAndSaveClientId()
    {
        Console.WriteLine();
        Console.Write("Google OAuth Client ID: ");
        var clientId = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(clientId))
        {
            Error("Client ID cannot be empty.");
            return false;
        }

        if (!clientId.EndsWith(".apps.googleusercontent.com", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("! That does not look like a standard Google OAuth Client ID.");
            Console.ResetColor();
            Console.Write("Save it anyway? [y/N]: ");

            var saveAnyway = Console.ReadLine()?.Trim();
            if (!string.Equals(saveAnyway, "y", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(saveAnyway, "yes", StringComparison.OrdinalIgnoreCase))
            {
                Muted("Client ID was not saved.");
                return false;
            }
        }

        credentials.SetLocalClientId(clientId);
        Success("Google OAuth Client ID saved.");

        Console.Write("Google OAuth Client Secret (optional, press Enter if Google did not provide one): ");
        var clientSecret = ReadSecretFromConsole();
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            credentials.SetLocalClientSecret(clientSecret);
            Success("Google OAuth Client Secret saved securely in Windows Credential Manager.");
        }

        Muted($"config: {credentials.ConfigPath}");
        Muted("Client ID is stored in local config. Client Secret and OAuth tokens are stored in Windows Credential Manager.");
        return true;
    }

    private static string ReadSecretFromConsole()
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine()?.Trim() ?? string.Empty;

        var value = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return value.ToString().Trim();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (value.Length > 0)
                    value.Length--;
                continue;
            }

            if (!char.IsControl(key.KeyChar))
                value.Append(key.KeyChar);
        }
    }

    private static void OpenSetupInstructions()
    {
        try
        {
            Process.Start(new ProcessStartInfo(SetupGuideUrl)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Error($"Could not open the browser: {ex.Message}");
            Muted(SetupGuideUrl);
        }
    }

    private void Config(IReadOnlyList<string> args)
    {
        var subcommand = args.FirstOrDefault()?.ToLowerInvariant() ?? "show";
        credentials.Reload();

        switch (subcommand)
        {
            case "show":
                Console.WriteLine($"OAuth Client ID : {(credentials.IsOAuthConfigured ? MaskClientId(credentials.ClientId!) : "not configured")}");
                Console.WriteLine($"Source          : {credentials.ClientIdSource}");
                Console.WriteLine($"Client secret   : {(string.IsNullOrWhiteSpace(credentials.ClientSecret) ? "not configured" : $"configured via {credentials.ClientSecretSource}")}");
                Console.WriteLine($"YouTube API key : {(string.IsNullOrWhiteSpace(credentials.ApiKey) ? "not configured" : "configured via environment")}");
                Console.WriteLine($"Config file     : {credentials.ConfigPath}");
                break;

            case "path":
                Console.WriteLine(credentials.ConfigPath);
                break;

            case "reset":
                credentials.ClearLocalConfig();
                Success("Local NgMusic configuration removed.");
                if (credentials.HasEnvironmentClientId)
                    Muted("NGMUSIC_GOOGLE_CLIENT_ID is still set and will continue to be used.");
                break;

            default:
                Error("Usage: config [show|path|reset]");
                break;
        }
    }

    private static string MaskClientId(string value)
    {
        if (value.Length <= 18)
            return value;

        return value[..8] + "…" + value[^18..];
    }

    private async Task WhoAmIAsync(CancellationToken cancellationToken)
    {
        var status = await auth.GetStatusAsync(cancellationToken);
        if (!status.IsAuthenticated)
        {
            Muted("not authenticated");
            return;
        }

        Success($"{status.DisplayName ?? "Google user"}  <{status.Email ?? "email unavailable"}>");
    }

    private async Task SearchAsync(IEnumerable<string> queryParts, CancellationToken cancellationToken)
    {
        var query = string.Join(' ', queryParts).Trim();
        if (query.Length == 0)
        {
            Error("Usage: search \"artist song\"");
            return;
        }

        Muted($"searching: {query}");
        _searchResults = await provider.SearchAsync(query, cancellationToken);
        if (_searchResults.Count == 0)
        {
            Muted("no results");
            return;
        }

        Console.WriteLine();
        for (var i = 0; i < _searchResults.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" [{i + 1,2}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_searchResults[i].Title);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  —  {_searchResults[i].Artist}");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    private async Task PlayAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        if (args.Count == 0)
        {
            if (_current is null)
                Error("Usage: play <search result number>");
            else
                await player.ResumeAsync(cancellationToken);
            return;
        }

        if (!int.TryParse(args[0], out var index) || index < 1 || index > _searchResults.Count)
        {
            Error("play expects a search result number, for example: play 1");
            return;
        }

        await PlayTrackAsync(_searchResults[index - 1], true, cancellationToken);
    }

    private async Task PlayTrackAsync(MusicTrack track, bool rememberCurrent, CancellationToken cancellationToken)
    {
        if (rememberCurrent && _current is not null && _current.VideoId != track.VideoId)
            _history.Push(_current);

        _current = track;
        await player.PlayAsync(track, cancellationToken);
        await player.SetVolumeAsync(_volume, cancellationToken);
        Success($"▶ {track.Artist} — {track.Title}");
    }

    private async Task NextAsync(CancellationToken cancellationToken)
    {
        if (_queue.Count == 0)
        {
            Muted("queue is empty");
            return;
        }

        await PlayTrackAsync(_queue.Dequeue(), true, cancellationToken);
    }

    private async Task PreviousAsync(CancellationToken cancellationToken)
    {
        if (_history.Count == 0)
        {
            Muted("no previous track");
            return;
        }

        await PlayTrackAsync(_history.Pop(), false, cancellationToken);
    }

    private async Task SeekAsync(string? value, CancellationToken cancellationToken)
    {
        if (!TryParseTime(value, out var seconds))
        {
            Error("Usage: seek 90  or  seek 1:30");
            return;
        }

        await player.SeekAsync(seconds, cancellationToken);
        Muted($"seek → {TimeSpan.FromSeconds(seconds):mm\\:ss}");
    }

    private async Task VolumeAsync(string? value, CancellationToken cancellationToken)
    {
        if (!int.TryParse(value, out var volume) || volume is < 0 or > 100)
        {
            Error("Usage: volume <0-100>");
            return;
        }

        _volume = volume;
        await player.SetVolumeAsync(volume, cancellationToken);
        Muted($"volume {volume}%");
    }

    private async Task QueueAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        if (args.Count == 0 || args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            PrintQueue();
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "add":
                if (args.Count < 2 || !int.TryParse(args[1], out var index) || index < 1 || index > _searchResults.Count)
                {
                    Error("Usage: queue add <search result number>");
                    return;
                }
                _queue.Enqueue(_searchResults[index - 1]);
                Success($"queued: {_searchResults[index - 1].Title}");
                break;
            case "clear":
                _queue.Clear();
                Muted("queue cleared");
                break;
            case "play":
                await NextAsync(cancellationToken);
                break;
            default:
                Error("Usage: queue [list|add <n>|clear|play]");
                break;
        }
    }

    private void PrintQueue()
    {
        if (_queue.Count == 0)
        {
            Muted("queue is empty");
            return;
        }

        var i = 1;
        foreach (var track in _queue)
            Console.WriteLine($" [{i++,2}] {track.Artist} — {track.Title}");
    }

    private void NowPlaying()
    {
        if (_current is null)
        {
            Muted("nothing playing");
            return;
        }

        Success($"▶ {_current.Artist} — {_current.Title}  |  volume {_volume}%");
    }

    private static bool TryParseTime(string? value, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        if (int.TryParse(value, out seconds) && seconds >= 0)
            return true;

        var parts = value.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var minutes) &&
            int.TryParse(parts[1], out var secs) &&
            minutes >= 0 &&
            secs is >= 0 and < 60)
        {
            seconds = checked(minutes * 60 + secs);
            return true;
        }

        return false;
    }

    private static void Banner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"NgMusic {AppInfo.Version}  // PowerShell-ish YouTube music controller");
        Console.ResetColor();
        Console.WriteLine("Type 'help' to see commands.\n");
    }

    private static void Help()
    {
        Console.WriteLine("""
 setup                      Configure Google OAuth interactively
 config [show|path|reset]   Inspect/reset local configuration
 login                      Connect Google/YouTube via OAuth
 logout                     Remove the saved OAuth token
 whoami                     Show the connected Google account
 search <query>             Search music videos (alias: s)
 play <n>                   Play a search result (alias: p)
 pause | resume | stop      Playback controls
 next | prev                Move through queued/history tracks
 seek <sec|mm:ss>           Seek inside the current video
 volume <0-100>             Set player volume (alias: vol)
 queue                      Show queue
 queue add <n>              Queue a search result
 queue clear                Clear queue
 now                        Show current track (alias: np)
 clear                      Clear terminal
 exit                       Quit NgMusic
""");
    }

    private static void Prompt()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("PS ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Music:\\> ");
        Console.ResetColor();
    }

    private static void Success(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {text}");
        Console.ResetColor();
    }

    private static void Error(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"! {text}");
        Console.ResetColor();
    }

    private static void Muted(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}

