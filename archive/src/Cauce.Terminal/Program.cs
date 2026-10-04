using Cauce.Terminal.Auth;
using Cauce.Terminal.Playback;
using Cauce.Terminal.Core;
using Cauce.Terminal.Providers;
using Cauce.Terminal.Settings;
using Cauce.Terminal.Shell;

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("Cauce.Terminal currently targets Windows 10/11.");
    return;
}

using var http = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};
http.DefaultRequestHeaders.UserAgent.ParseAdd($"Cauce.Terminal/{AppInfo.Version}");

var settingsStore = new UserSettingsStore();
var credentials = new GoogleCredentials(settingsStore);
var tokenStore = new WindowsCredentialTokenStore("Cauce.Terminal.GoogleOAuth");
var auth = new GoogleOAuthService(http, credentials, tokenStore);
var provider = new YouTubeProvider(http, credentials, auth);
await using var player = new YouTubeIframePlayer();
var shell = new MusicShell(auth, provider, player, credentials);

using var quit = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    quit.Cancel();
};

await shell.RunAsync(quit.Token);
