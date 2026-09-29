using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NgMusic.Core;
using NgMusic.Infrastructure;

namespace NgMusic.Auth;

public sealed class GoogleOAuthService(
    HttpClient http,
    GoogleCredentials credentials,
    ITokenStore tokenStore) : IGoogleAuthService
{
    private static readonly string[] Scopes =
    [
        "openid",
        "email",
        "profile",
        "https://www.googleapis.com/auth/youtube.readonly"
    ];

    public async Task<AuthStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var token = await tokenStore.ReadAsync(cancellationToken);
        if (token is null)
            return AuthStatus.SignedOut;

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(accessToken)
                ? AuthStatus.SignedOut
                : new AuthStatus(true, token.DisplayName, token.Email);
        }
        catch
        {
            return AuthStatus.SignedOut;
        }
    }

    public async Task<AuthStatus> LoginAsync(CancellationToken cancellationToken = default)
    {
        credentials.EnsureOAuthConfigured();

        var verifier = Base64Url(RandomNumberGenerator.GetBytes(32));
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        var state = Base64Url(RandomNumberGenerator.GetBytes(24));

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var redirectUri = $"http://127.0.0.1:{port}/";

        var authUrl = BuildAuthorizationUrl(redirectUri, challenge, state);
        Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));

        TcpClient client;
        try
        {
            client = await listener.AcceptTcpClientAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Google login timed out. Run 'login' again.");
        }

        using (client)
        {
            var target = await LoopbackHttp.ReadTargetAsync(client, timeout.Token);
            var callback = new Uri("http://127.0.0.1" + target);
            var query = ParseQuery(callback.Query);
            query.TryGetValue("state", out var returnedState);
            query.TryGetValue("code", out var code);
            query.TryGetValue("error", out var error);
            query.TryGetValue("error_description", out var errorDescription);

            if (!string.IsNullOrWhiteSpace(error))
            {
                var message = $"Google OAuth returned {error}" +
                              (string.IsNullOrWhiteSpace(errorDescription) ? "." : $": {errorDescription}");
                await WriteBrowserResultAsync(client, false, message, timeout.Token);
                throw new InvalidOperationException(message);
            }

            if (!string.Equals(returnedState, state, StringComparison.Ordinal))
            {
                const string message = "OAuth state validation failed.";
                await WriteBrowserResultAsync(client, false, message, timeout.Token);
                throw new InvalidOperationException(message);
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                const string message = "Google did not return an authorization code.";
                await WriteBrowserResultAsync(client, false, message, timeout.Token);
                throw new InvalidOperationException(message);
            }

            try
            {
                var token = await ExchangeCodeAsync(code, verifier, redirectUri, cancellationToken);

                // Profile lookup is convenient for the shell prompt but must never invalidate
                // an otherwise successful OAuth grant.
                var profile = await TryFetchProfileAsync(token.AccessToken, cancellationToken);
                token = token with { DisplayName = profile.DisplayName, Email = profile.Email };

                await tokenStore.WriteAsync(token, cancellationToken);
                await WriteBrowserResultAsync(
                    client,
                    true,
                    "La autorización de Google terminó correctamente. Puedes volver a la terminal.",
                    timeout.Token);

                return new AuthStatus(true, token.DisplayName, token.Email);
            }
            catch (Exception ex)
            {
                await WriteBrowserResultAsync(client, false, BrowserSafeError(ex), timeout.Token);
                throw;
            }
        }
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) =>
        tokenStore.DeleteAsync(cancellationToken);

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await tokenStore.ReadAsync(cancellationToken);
        if (token is null)
            return null;

        if (token.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            return token.AccessToken;

        if (string.IsNullOrWhiteSpace(token.RefreshToken))
            return null;

        credentials.EnsureOAuthConfigured();
        var refreshed = await RefreshAsync(token, cancellationToken);
        await tokenStore.WriteAsync(refreshed, cancellationToken);
        return refreshed.AccessToken;
    }

    private string BuildAuthorizationUrl(string redirectUri, string challenge, string state)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = credentials.ClientId!,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = string.Join(' ', Scopes),
            ["code_challenge"] = challenge,
            ["code_challenge_method"] = "S256",
            ["state"] = state,
            ["access_type"] = "offline",
            ["prompt"] = "consent"
        };

        return "https://accounts.google.com/o/oauth2/v2/auth?" + string.Join('&',
            query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
    }

    private async Task<StoredToken> ExchangeCodeAsync(
        string code,
        string verifier,
        string redirectUri,
        CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, string>
        {
            ["client_id"] = credentials.ClientId!,
            ["code"] = code,
            ["code_verifier"] = verifier,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        if (!string.IsNullOrWhiteSpace(credentials.ClientSecret))
            values["client_secret"] = credentials.ClientSecret;

        using var response = await http.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(values),
            cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw BuildGoogleOAuthException("token exchange", response.StatusCode, json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var accessToken = root.GetProperty("access_token").GetString()!;
        var refreshToken = root.TryGetProperty("refresh_token", out var refresh)
            ? refresh.GetString()
            : null;
        var expiresIn = root.TryGetProperty("expires_in", out var expires)
            ? expires.GetInt32()
            : 3600;

        return new StoredToken(
            accessToken,
            refreshToken,
            DateTimeOffset.UtcNow.AddSeconds(expiresIn));
    }

    private async Task<StoredToken> RefreshAsync(StoredToken current, CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, string>
        {
            ["client_id"] = credentials.ClientId!,
            ["refresh_token"] = current.RefreshToken!,
            ["grant_type"] = "refresh_token"
        };

        if (!string.IsNullOrWhiteSpace(credentials.ClientSecret))
            values["client_secret"] = credentials.ClientSecret;

        using var response = await http.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(values),
            cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw BuildGoogleOAuthException("token refresh", response.StatusCode, json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var accessToken = root.GetProperty("access_token").GetString()!;
        var expiresIn = root.TryGetProperty("expires_in", out var expires)
            ? expires.GetInt32()
            : 3600;

        return current with
        {
            AccessToken = accessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
        };
    }

    private async Task<(string? DisplayName, string? Email)> TryFetchProfileAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://openidconnect.googleapis.com/v1/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (null, null);

            using var doc = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;

            return (
                root.TryGetProperty("name", out var name) ? name.GetString() : null,
                root.TryGetProperty("email", out var email) ? email.GetString() : null);
        }
        catch when (!cancellationToken.IsCancellationRequested)
        {
            return (null, null);
        }
    }

    private static Exception BuildGoogleOAuthException(
        string stage,
        HttpStatusCode statusCode,
        string body)
    {
        string? error = null;
        string? description = null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorElement))
                error = errorElement.GetString();

            if (root.TryGetProperty("error_description", out var descriptionElement))
                description = descriptionElement.GetString();
        }
        catch
        {
            // Google normally returns JSON. Keep the HTTP status if it does not.
        }

        var details = !string.IsNullOrWhiteSpace(error)
            ? error
            : $"HTTP {(int)statusCode} {statusCode}";

        if (!string.IsNullOrWhiteSpace(description))
            details += $": {description}";

        return new InvalidOperationException($"Google OAuth {stage} failed ({details}).");
    }

    private static async Task WriteBrowserResultAsync(
        TcpClient client,
        bool success,
        string detail,
        CancellationToken cancellationToken)
    {
        try
        {
            await LoopbackHttp.WriteResponseAsync(
                client,
                "text/html; charset=utf-8",
                BrowserResponseHtml(success, detail),
                cancellationToken: cancellationToken);
        }
        catch
        {
            // The terminal still receives the authoritative result.
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

    private static string BrowserResponseHtml(bool success, string detail)
    {
        var title = success ? "NgMusic conectado." : "NgMusic no pudo completar el login.";
        var encodedDetail = WebUtility.HtmlEncode(detail);
        var accent = success ? "#4ec9b0" : "#f48771";

        return $"""
<!doctype html>
<html>
<head>
  <meta charset="utf-8">
  <title>NgMusic OAuth</title>
</head>
<body style="font-family:Consolas;background:#0c0c0c;color:#ddd;padding:40px">
  <h1 style="color:{accent}">{title}</h1>
  <p>{encodedDetail}</p>
  <p style="color:#858585">Puedes cerrar esta pestaña y volver a la terminal.</p>
</body>
</html>
""";
    }

    private static string BrowserSafeError(Exception ex)
    {
        var message = ex.Message;
        const int maxLength = 350;
        return message.Length <= maxLength ? message : message[..maxLength] + "…";
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
