# Security and privacy

[Documentation home](../README.md) · [Configuration](configuration.md) · [Code signing policy](code-signing-policy.md)

## Threat model and trust boundaries

NgMusic is a desktop client. It does not operate a project-controlled backend service.

The most important security boundaries are:

- Google OAuth authentication in the system browser.
- OAuth token storage in Windows Credential Manager.
- Local HTTP listeners restricted to `127.0.0.1`.
- Requests to Google/YouTube services over HTTPS.
- GitHub Actions release builds and, when available, release code signing.

## Data handled by NgMusic

NgMusic may process:

- search queries entered by the user;
- Google OAuth access/refresh tokens;
- basic Google profile information returned by OpenID Connect;
- YouTube video IDs, titles, channel names, and playback commands.

NgMusic does not intentionally collect project analytics, advertising identifiers, or first-party telemetry.

## Network behavior

Network activity occurs as part of user-visible functionality:

- Google account authorization;
- OAuth token exchange and refresh;
- OpenID Connect profile lookup;
- YouTube Data API searches;
- loading and playing YouTube's embedded player.

The local OAuth callback and player command bridge bind only to loopback and use dynamically selected ports.

Third-party services may process data under their own policies. Users should review Google's privacy policy and YouTube terms when using those services.

## Credential storage

OAuth tokens are stored in Windows Credential Manager under `NgMusic.GoogleOAuth`.

Google client IDs, optional client secrets, and API keys are supplied through environment variables and must not be committed to Git.

## Privacy statement

**NgMusic itself will not transfer information to project-operated network systems because the project operates no telemetry or account backend.** Information is transferred to Google/YouTube only when the user invokes functionality that requires those services, such as login, search, or playback.

## Vulnerability reporting

Do not publish sensitive credentials, access tokens, or exploitable security details in a public issue.

For non-sensitive bugs, use GitHub Issues. For a security-sensitive report, contact the repository owner privately through an appropriate GitHub contact channel until a dedicated security advisory/contact process is configured.

## Release integrity

Official releases are produced by the repository's GitHub Actions workflow and publish SHA-256 checksums.

When code signing becomes active, signed artifacts must originate from the same automated build/release process described in the [code signing policy](code-signing-policy.md).
