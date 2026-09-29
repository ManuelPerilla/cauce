using NgMusic.Auth;
using NgMusic.Playback;
using NgMusic.Core;
using NgMusic.Providers;
using NgMusic.Shell;

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("NgMusic currently targets Windows 10/11.");
    return;
}

using var http = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};
http.DefaultRequestHeaders.UserAgent.ParseAdd($"NgMusic/{AppInfo.Version}");

var credentials = GoogleCredentials.FromEnvironment();
var tokenStore = new WindowsCredentialTokenStore("NgMusic.GoogleOAuth");
var auth = new GoogleOAuthService(http, credentials, tokenStore);
var provider = new YouTubeProvider(http, credentials, auth);
await using var player = new YouTubeIframePlayer();
var shell = new MusicShell(auth, provider, player);

using var quit = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    quit.Cancel();
};

await shell.RunAsync(quit.Token);
