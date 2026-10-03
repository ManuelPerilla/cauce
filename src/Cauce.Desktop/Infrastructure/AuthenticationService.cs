using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Duende.IdentityModel.OidcClient.Results;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Cauce.Desktop.Infrastructure;

/// <summary>A public native OIDC client. Provider credentials belong to the broker, never this app.</summary>
public sealed class AuthenticationService : IDisposable
{
    private static readonly string[] ProviderNames = ["google", "apple", "facebook", "microsoft"];
    private static readonly HashSet<string> RoutingParameters = new(StringComparer.Ordinal)
    {
        "connection", "acr_values", "kc_idp_hint"
    };
    private readonly object _sync = new();
    private readonly AuthConfiguration? _configuration;
    private CancellationTokenSource? _activeLogin;
    private bool _disposed;
    private string? _displayName;

    public bool IsConfigured => _configuration is not null;
    public string ConfigurationStatus { get; }

    public AuthenticationService() : this(Path.Combine(AppContext.BaseDirectory, "auth.json")) { }

    public AuthenticationService(string configurationPath)
    {
        try
        {
            if (!File.Exists(configurationPath))
            {
                ConfigurationStatus = "Las cuentas aún no están habilitadas. Puedes usar tu música sin una cuenta.";
                return;
            }
            if (new FileInfo(configurationPath).Length > 32_768)
                throw new InvalidDataException();

            var configuration = JsonSerializer.Deserialize<AuthConfiguration>(File.ReadAllText(configurationPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    MaxDepth = 8
                });
            if (configuration is null || !IsValid(configuration))
                throw new InvalidDataException();

            _configuration = configuration;
            ConfigurationStatus = "Inicio de sesión disponible en tu navegador. La sesión se conserva sólo mientras Cauce está abierto.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            ConfigurationStatus = "La configuración de cuentas no es válida. Tu biblioteca local sigue disponible.";
        }
    }

    public async Task<string> SignInAsync(string provider, CancellationToken cancellationToken = default)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ConfigurationStatus);
        var providerKey = provider?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!configuration.Providers.TryGetValue(providerKey, out var routing))
            throw new ArgumentException("Este proveedor no está configurado.", nameof(provider));

        CancellationTokenSource attempt;
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_activeLogin is not null)
                throw new InvalidOperationException("Ya hay un inicio de sesión abierto en el navegador.");
            attempt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attempt.CancelAfter(TimeSpan.FromSeconds(configuration.TimeoutSeconds));
            _activeLogin = attempt;
        }

        try
        {
            attempt.Token.ThrowIfCancellationRequested();
            using var browser = new LoopbackBrowser(configuration.LoopbackPort);
            using var stopListener = attempt.Token.Register(browser.Dispose);
            var protocol = new IdentityProtocolValidator();
            var nonce = protocol.GenerateNonce();
            var options = new OidcClientOptions
            {
                Authority = configuration.Authority,
                ClientId = configuration.ClientId,
                Scope = "openid profile",
                RedirectUri = browser.RedirectUri,
                Browser = browser,
                LoadProfile = false,
                ClockSkew = TimeSpan.FromMinutes(1),
                BackchannelTimeout = TimeSpan.FromSeconds(20),
                RefreshDiscoveryDocumentForLogin = true,
                RefreshDiscoveryOnSignatureFailure = true,
                IdentityTokenValidator = new SignedIdentityTokenValidator(protocol, nonce),
                Policy = new Policy { RequireIdentityTokenSignature = true }
            };
            var parameters = new Parameters { { "nonce", nonce } };
            foreach (var parameter in routing)
                parameters.Add(parameter.Key, parameter.Value);

            var result = await new OidcClient(options).LoginAsync(new LoginRequest
            {
                BrowserTimeout = configuration.TimeoutSeconds,
                FrontChannelExtraParameters = parameters
            }, attempt.Token).ConfigureAwait(false);

            attempt.Token.ThrowIfCancellationRequested();
            if (result.IsError || string.IsNullOrWhiteSpace(result.IdentityToken) ||
                string.IsNullOrWhiteSpace(result.User?.FindFirst("sub")?.Value))
                throw new InvalidOperationException("No se pudo verificar el inicio de sesión. Inténtalo de nuevo o consulta soporte.");

            var displayName = GetDisplayName(result.User);
            lock (_sync)
            {
                attempt.Token.ThrowIfCancellationRequested();
                ObjectDisposedException.ThrowIf(_disposed, this);
                _displayName = displayName;
            }
            // Tokens are not retained, logged, refreshed or written to disk. This does not enable cloud sync.
            return displayName;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("El inicio de sesión se canceló o agotó su tiempo.");
        }
        catch (Exception) when (attempt.IsCancellationRequested)
        {
            throw new OperationCanceledException(attempt.Token);
        }
        catch (Exception exception) when (exception is not OperationCanceledException and not ObjectDisposedException)
        {
            // Provider exceptions can contain authorization responses or claims; do not expose those to UI/logs.
            throw new InvalidOperationException("No se pudo completar el inicio de sesión. Comprueba tu conexión y la configuración de cuentas.");
        }
        finally
        {
            lock (_sync)
            {
                _activeLogin = null;
                attempt.Dispose();
            }
        }
    }

    public void SignOut()
    {
        lock (_sync)
        {
            _displayName = null;
            _activeLogin?.Cancel();
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            _displayName = null;
            _activeLogin?.Cancel();
        }
    }

    private static string GetDisplayName(ClaimsPrincipal user)
    {
        var name = user.FindFirst("name")?.Value ?? user.FindFirst("given_name")?.Value;
        if (string.IsNullOrWhiteSpace(name)) return "Cuenta conectada";
        var safeName = new string(name.Where(character => !char.IsControl(character)).Take(80).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "Cuenta conectada" : safeName;
    }

    private static bool IsValid(AuthConfiguration configuration)
    {
        if (!Uri.TryCreate(configuration.Authority, UriKind.Absolute, out var authority) ||
            authority.Scheme != Uri.UriSchemeHttps || authority.UserInfo.Length > 0 ||
            authority.Query.Length > 0 || authority.Fragment.Length > 0 ||
            string.IsNullOrWhiteSpace(configuration.ClientId) || configuration.ClientId.Length > 512 ||
            configuration.ClientId.Any(char.IsControl) || configuration.LoopbackPort is < 0 or > 65535 ||
            configuration.TimeoutSeconds is < 30 or > 600 || configuration.Providers is null ||
            configuration.Providers.Count != ProviderNames.Length)
            return false;

        return ProviderNames.All(provider => configuration.Providers.TryGetValue(provider, out var routing) &&
            routing is { Count: > 0 and <= 3 } && routing.All(parameter =>
                RoutingParameters.Contains(parameter.Key) && !string.IsNullOrWhiteSpace(parameter.Value) &&
                parameter.Value.Length <= 256 && !parameter.Value.Any(char.IsControl)));
    }

    private sealed class AuthConfiguration
    {
        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public int LoopbackPort { get; set; }
        public int TimeoutSeconds { get; set; } = 180;
        public Dictionary<string, Dictionary<string, string>> Providers { get; set; } = new(StringComparer.Ordinal);
    }

    /// <summary>Adapts maintained Microsoft signature/protocol validation to Duende's extension point.</summary>
    private sealed class SignedIdentityTokenValidator(IdentityProtocolValidator protocol, string nonce) : IIdentityTokenValidator
    {
        public Task<IdentityTokenValidationResult> ValidateAsync(string identityToken, OidcClientOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var keySet = options.ProviderInformation.KeySet;
                if (keySet is null)
                    return Task.FromResult(new IdentityTokenValidationResult { Error = "invalid_signature" });
                var signingKeys = new JsonWebKeySet(keySet.RawData ?? JsonSerializer.Serialize(keySet)).GetSigningKeys();
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false, MaximumTokenSizeInBytes = 65_536 };
                var user = handler.ValidateToken(identityToken, new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys,
                    ValidAlgorithms = options.Policy.ValidSignatureAlgorithms,
                    ValidateIssuer = true,
                    ValidIssuer = options.ProviderInformation.IssuerName,
                    ValidateAudience = true,
                    ValidAudience = options.ClientId,
                    IgnoreTrailingSlashWhenValidatingAudience = false,
                    RequireAudience = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = options.ClockSkew,
                    NameClaimType = "name",
                    AuthenticationType = "OIDC",
                    IncludeTokenOnFailedValidation = false
                }, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwt)
                    return Task.FromResult(new IdentityTokenValidationResult { Error = "invalid_identity_token" });
                protocol.ValidateIdentity(jwt, nonce, options.ClientId);
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(new IdentityTokenValidationResult { User = user, SignatureAlgorithm = jwt.Header.Alg });
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                return Task.FromResult(new IdentityTokenValidationResult { Error = "invalid_signature" });
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Task.FromResult(new IdentityTokenValidationResult { Error = "invalid_signature" });
            }
            catch (Exception exception) when (exception is SecurityTokenException or OpenIdConnectProtocolException or ArgumentException or JsonException)
            {
                return Task.FromResult(new IdentityTokenValidationResult { Error = "invalid_identity_token" });
            }
        }
    }

    private sealed class IdentityProtocolValidator : OpenIdConnectProtocolValidator
    {
        public void ValidateIdentity(JwtSecurityToken token, string nonce, string clientId)
        {
            // Duende validates code/state/PKCE and at_hash. Microsoft validates required OIDC claims and nonce.
            RequireAzp = token.Audiences.Skip(1).Any();
            var context = new OpenIdConnectProtocolValidationContext
            {
                ValidatedIdToken = token,
                Nonce = nonce,
                ClientId = clientId
            };
            ValidateIdToken(context);
            ValidateNonce(context);
        }
    }

    private sealed class LoopbackBrowser : IBrowser, IDisposable
    {
        private const string CallbackPath = "/callback/";
        private const int MaximumHeadersLength = 16_384;
        private readonly TcpListener _listener;
        private readonly string _host;
        public string RedirectUri { get; }

        public LoopbackBrowser(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Server.ExclusiveAddressUse = true;
            _listener.Start(4);
            var actualPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _host = "127.0.0.1:" + actualPort.ToString(CultureInfo.InvariantCulture);
            RedirectUri = "http://" + _host + CallbackPath;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(options.StartUrl, UriKind.Absolute, out var start) || start.Scheme != Uri.UriSchemeHttps ||
                start.UserInfo.Length != 0 || !string.Equals(options.EndUrl, RedirectUri, StringComparison.Ordinal))
                throw new InvalidOperationException("La dirección de inicio de sesión no es válida.");

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.Timeout);
            using var process = Process.Start(new ProcessStartInfo(start.AbsoluteUri) { UseShellExecute = true });

            while (true)
            {
                using var client = await _listener.AcceptTcpClientAsync(timeout.Token).ConfigureAwait(false);
                if (client.Client.RemoteEndPoint is not IPEndPoint remote || !remote.Address.Equals(IPAddress.Loopback))
                    continue;
                using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
                requestTimeout.CancelAfter(TimeSpan.FromSeconds(5));
                try
                {
                    var target = await ReadCallbackAsync(client.GetStream(), requestTimeout.Token).ConfigureAwait(false);
                    if (target is null)
                    {
                        await RespondAsync(client.GetStream(), false, requestTimeout.Token).ConfigureAwait(false);
                        continue;
                    }
                    // This page confirms receipt only. Token validation happens after returning to OidcClient.
                    await RespondAsync(client.GetStream(), true, requestTimeout.Token).ConfigureAwait(false);
                    return new BrowserResult { ResultType = BrowserResultType.Success, Response = "http://" + _host + target };
                }
                catch (OperationCanceledException) when (!timeout.IsCancellationRequested) { }
                catch (IOException) { }
                catch (SocketException) { }
            }
        }

        private async Task<string?> ReadCallbackAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var bytes = new byte[MaximumHeadersLength];
            var length = 0;
            while (length < bytes.Length)
            {
                var count = await stream.ReadAsync(bytes.AsMemory(length, 1), cancellationToken).ConfigureAwait(false);
                if (count == 0) return null;
                var value = bytes[length++];
                if (value > 126 || (value < 32 && value is not 13 and not 10)) return null;
                if (length >= 4 && bytes[length - 4] == 13 && bytes[length - 3] == 10 &&
                    bytes[length - 2] == 13 && bytes[length - 1] == 10) break;
            }
            if (length == bytes.Length) return null;
            var lines = Encoding.ASCII.GetString(bytes, 0, length).Split("\r\n", StringSplitOptions.None);
            var request = lines[0].Split(' ', StringSplitOptions.None);
            if (request.Length != 3 || request[0] != "GET" ||
                request[2] is not "HTTP/1.1" and not "HTTP/1.0" ||
                !request[1].StartsWith(CallbackPath + "?", StringComparison.Ordinal) || request[1].Contains('#'))
                return null;

            var hostSeen = false;
            for (var index = 1; index < lines.Length && lines[index].Length > 0; index++)
            {
                var separator = lines[index].IndexOf(':');
                if (separator <= 0 || char.IsWhiteSpace(lines[index][0])) return null;
                var key = lines[index][..separator];
                var value = lines[index][(separator + 1)..].Trim();
                if (key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    if (hostSeen || !value.Equals(_host, StringComparison.OrdinalIgnoreCase)) return null;
                    hostSeen = true;
                }
                if (key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
                    (key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) && value != "0")) return null;
            }
            return hostSeen ? request[1] : null;
        }

        private static async Task RespondAsync(NetworkStream stream, bool received, CancellationToken cancellationToken)
        {
            var body = Encoding.UTF8.GetBytes(received
                ? "<!doctype html><html lang=\"es\"><meta charset=\"utf-8\"><title>Cauce</title><p>Puedes regresar a Cauce. La aplicación comprobará la respuesta.</p></html>"
                : "Solicitud no válida.");
            var status = received ? "200 OK" : "400 Bad Request";
            var headers = Encoding.ASCII.GetBytes("HTTP/1.1 " + status + "\r\nContent-Type: text/html; charset=utf-8\r\n" +
                "Content-Length: " + body.Length.ToString(CultureInfo.InvariantCulture) + "\r\nCache-Control: no-store\r\n" +
                "Referrer-Policy: no-referrer\r\nContent-Security-Policy: default-src 'none'; frame-ancestors 'none'\r\n" +
                "X-Content-Type-Options: nosniff\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(headers, cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(body, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose() => _listener.Stop();
    }
}
