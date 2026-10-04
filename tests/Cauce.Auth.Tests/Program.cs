using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cauce.Desktop.Infrastructure;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var count = 0;
void Check(bool result, string name) { if (!result) throw new Exception(name); Console.WriteLine("PASS " + name); count++; }
var tempPath = Path.Combine(AppContext.BaseDirectory, "configuration-test.json");
using (var service = new AuthenticationService(tempPath)) Check(!service.IsConfigured, "Missing configuration remains unavailable");
var config = new Dictionary<string, object?>
{
    ["authority"] = "https://broker.example", ["clientId"] = "cauce-test",
    ["providers"] = new Dictionary<string, object>
    {
        ["google"] = new { connection = "g" }, ["apple"] = new { connection = "a" },
        ["facebook"] = new { connection = "f" }, ["microsoft"] = new { connection = "m" }
    }
};
void WriteConfig() => File.WriteAllText(tempPath, JsonSerializer.Serialize(config));
WriteConfig();
using (var service = new AuthenticationService(tempPath)) Check(service.IsConfigured, "Valid public broker configuration accepted");
config["clientSecret"] = "do-not-store"; WriteConfig();
using (var service = new AuthenticationService(tempPath)) Check(!service.IsConfigured, "Client secrets rejected");
config.Remove("clientSecret"); config["authority"] = "http://broker.example"; WriteConfig();
using (var service = new AuthenticationService(tempPath)) Check(!service.IsConfigured, "Non HTTPS authority rejected");
config["authority"] = "https://broker.example"; config["loopbackPort"] = 65536; WriteConfig();
using (var service = new AuthenticationService(tempPath)) Check(!service.IsConfigured, "Invalid port rejected");
File.Delete(tempPath);

var serviceType = typeof(AuthenticationService);
var protocolType = serviceType.GetNestedType("IdentityProtocolValidator", BindingFlags.NonPublic)!;
var protocol = (OpenIdConnectProtocolValidator)Activator.CreateInstance(protocolType, true)!;
var nonce = protocol.GenerateNonce();
var validatorType = serviceType.GetNestedType("SignedIdentityTokenValidator", BindingFlags.NonPublic)!;
var validator = (IIdentityTokenValidator)Activator.CreateInstance(validatorType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
    null, [protocol, nonce], null)!;
using var rsa = RSA.Create(2048);
using var unrelatedRsa = RSA.Create(2048);
var publicParameters = rsa.ExportParameters(false);
var jwks = JsonSerializer.Serialize(new { keys = new[] { new { kty = "RSA", kid = "test", use = "sig", alg = "RS256",
    n = Base64UrlEncoder.Encode(publicParameters.Modulus!), e = Base64UrlEncoder.Encode(publicParameters.Exponent!) } } });
var options = new OidcClientOptions
{
    ClientId = "cauce-test", ClockSkew = TimeSpan.Zero,
    ProviderInformation = new ProviderInformation { IssuerName = "https://broker.example", KeySet = new Duende.IdentityModel.Jwk.JsonWebKeySet(jwks) },
    Policy = new Policy { RequireIdentityTokenSignature = true }
};
string Token(string? givenNonce = null, string issuer = "https://broker.example", string audience = "cauce-test",
    RSA? signer = null, bool expired = false, bool unsigned = false, bool missingSub = false,
    bool missingNonce = false, bool multipleAudiences = false, string? authorizedParty = null)
{
    var claims = new List<Claim> { new("name", "Test") };
    if (!missingNonce) claims.Add(new("nonce", givenNonce ?? nonce));
    if (!missingSub) claims.Add(new("sub", "test-subject"));
    if (multipleAudiences) claims.Add(new("aud", "other-client"));
    if (authorizedParty is not null) claims.Add(new("azp", authorizedParty));
    return new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims), Issuer = issuer, Audience = audience,
        IssuedAt = DateTime.UtcNow.AddMinutes(-2), NotBefore = DateTime.UtcNow.AddMinutes(-2),
        Expires = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddMinutes(5),
        SigningCredentials = unsigned ? null : new SigningCredentials(new RsaSecurityKey(signer ?? rsa) { KeyId = "test" }, SecurityAlgorithms.RsaSha256)
    });
}
Check(!(await validator.ValidateAsync(Token(), options)).IsError, "Signed ID token and nonce accepted");
Check((await validator.ValidateAsync(Token(givenNonce: "wrong"), options)).IsError, "Nonce mismatch rejected");
Check((await validator.ValidateAsync(Token(missingNonce: true), options)).IsError, "Missing nonce rejected");
Check((await validator.ValidateAsync(Token(issuer: "https://evil.example"), options)).IsError, "Wrong issuer rejected");
Check((await validator.ValidateAsync(Token(audience: "other-client"), options)).IsError, "Wrong audience rejected");
Check((await validator.ValidateAsync(Token(audience: "cauce-test/"), options)).IsError, "Audience compared exactly");
Check((await validator.ValidateAsync(Token(multipleAudiences: true), options)).IsError, "Multiple audiences require authorized party");
Check(!(await validator.ValidateAsync(Token(multipleAudiences: true, authorizedParty: "cauce-test"), options)).IsError, "Multiple audiences with matching authorized party accepted");
Check((await validator.ValidateAsync(Token(authorizedParty: "other-client"), options)).IsError, "Wrong authorized party rejected");
Check((await validator.ValidateAsync(Token(signer: unrelatedRsa), options)).IsError, "Wrong signature rejected");
Check((await validator.ValidateAsync(Token(expired: true), options)).IsError, "Expired ID token rejected");
Check((await validator.ValidateAsync(Token(unsigned: true), options)).IsError, "Unsigned ID token rejected");
Check((await validator.ValidateAsync(Token(missingSub: true), options)).IsError, "Missing subject rejected");

var noNetwork = new NoNetworkHandler();
options.ProviderInformation.AuthorizeEndpoint = "https://broker.example/authorize";
options.ProviderInformation.TokenEndpoint = "https://broker.example/token";
options.RedirectUri = "http://127.0.0.1:43821/callback/";
options.Scope = "openid profile";
options.BackchannelHandler = noNetwork;
var oidcClient = new OidcClient(options);
var preparedLogin = await oidcClient.PrepareLoginAsync(new Parameters { { "nonce", nonce } });
var authorizationQuery = new Uri(preparedLogin.StartUrl).Query.TrimStart('?').Split('&')
    .Select(entry => entry.Split('=', 2)).ToDictionary(entry => Uri.UnescapeDataString(entry[0]), entry => Uri.UnescapeDataString(entry[1]));
Check(authorizationQuery["response_type"] == "code" && authorizationQuery["code_challenge_method"] == "S256", "Code flow uses PKCE S256");
Check(authorizationQuery["code_challenge"] == Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(preparedLogin.CodeVerifier))), "PKCE verifier matches challenge");
Check(authorizationQuery["nonce"] == nonce && !string.IsNullOrWhiteSpace(preparedLogin.State), "Nonce and state included in authorization");
var wrongState = await oidcClient.ProcessResponseAsync(options.RedirectUri + "?code=test&state=wrong", preparedLogin);
Check(wrongState.IsError && noNetwork.Calls == 0, "State mismatch rejected before any token request");
var missingState = await oidcClient.ProcessResponseAsync(options.RedirectUri + "?code=test", preparedLogin);
Check(missingState.IsError && noNetwork.Calls == 0, "Missing state rejected before any token request");

var browserType = serviceType.GetNestedType("LoopbackBrowser", BindingFlags.NonPublic)!;
Uri closedCallback;
using (var browser = (IDisposable)Activator.CreateInstance(browserType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [0], null)!)
{
    var callback = new Uri((string)browserType.GetProperty("RedirectUri")!.GetValue(browser)!);
    closedCallback = callback;
    Check(callback.Host == "127.0.0.1" && callback.Port > 0 && callback.AbsolutePath == "/callback/", "Listener chooses bound IPv4 loopback port");
    var listener = (TcpListener)browserType.GetField("_listener", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(browser)!;
    var read = browserType.GetMethod("ReadCallbackAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
    async Task<string?> Parse(string request)
    {
        using var outgoing = new TcpClient();
        await outgoing.ConnectAsync(IPAddress.Loopback, callback.Port);
        using var incoming = await listener.AcceptTcpClientAsync();
        await outgoing.GetStream().WriteAsync(Encoding.ASCII.GetBytes(request));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        return await (Task<string?>)read.Invoke(browser, [incoming.GetStream(), timeout.Token])!;
    }
    Check(await Parse($"GET /callback/?code=test&state=state HTTP/1.1\r\nHost: {callback.Authority}\r\n\r\n") is not null, "Expected callback accepted");
    Check(await Parse("GET /callback/?code=test HTTP/1.1\r\nHost: evil.example\r\n\r\n") is null, "Wrong callback Host rejected");
    Check(await Parse($"POST /callback/?code=test HTTP/1.1\r\nHost: {callback.Authority}\r\n\r\n") is null, "POST callback rejected");
    Check(await Parse($"GET /callback/?code=test HTTP/1.1\r\nHost: {callback.Authority}\r\nContent-Length: 1\r\n\r\n") is null, "Callback body rejected");
    Check(await Parse($"GET /other/?code=test HTTP/1.1\r\nHost: {callback.Authority}\r\n\r\n") is null, "Wrong callback path rejected");
    Check(await Parse($"GET /callback/?code=test HTTP/1.1\r\nHost: {callback.Authority}\r\nHost: {callback.Authority}\r\n\r\n") is null, "Duplicate Host rejected");
    Check(await Parse($"GET /callback/?code=test HTTP/1.1\r\nHost: {callback.Authority}\r\nTransfer-Encoding: chunked\r\n\r\n") is null, "Chunked request rejected");
    var oversizedHeader = new string('a', 16_384);
    Check(await Parse($"GET /callback/?code=test HTTP/1.1\r\nHost: {callback.Authority}\r\nX-Test: {oversizedHeader}\r\n\r\n") is null, "Oversized headers rejected");
    using var unfinishedClient = new TcpClient();
    await unfinishedClient.ConnectAsync(IPAddress.Loopback, callback.Port);
    using var unfinishedRequest = await listener.AcceptTcpClientAsync();
    using var requestCancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
    var cancelled = false;
    try { await (Task<string?>)read.Invoke(browser, [unfinishedRequest.GetStream(), requestCancellation.Token])!; }
    catch (OperationCanceledException) { cancelled = true; }
    Check(cancelled, "Partial callback can be cancelled");
}
using (var connectionAfterDispose = new TcpClient())
{
    var refused = false;
    try { await connectionAfterDispose.ConnectAsync(IPAddress.Loopback, closedCallback.Port); }
    catch (SocketException) { refused = true; }
    Check(refused, "Listener is closed on disposal");
}
Console.WriteLine($"{count} authentication checks passed.");

sealed class NoNetworkHandler : HttpMessageHandler
{
    public int Calls { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        throw new InvalidOperationException("Authentication tests must never contact a remote endpoint.");
    }
}
