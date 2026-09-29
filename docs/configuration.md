# Configuration and Google OAuth

[Documentation home](../README.md) · [Installation](installation.md) · [Security](security.md)

NgMusic uses Google's OAuth 2.0 Desktop application flow and the YouTube Data API v3.

## Required Google configuration

Create a Google Cloud project, enable **YouTube Data API v3**, and create an OAuth client of type **Desktop app**.

NgMusic reads the following environment variables:

| Variable | Required | Purpose |
| --- | --- | --- |
| `NGMUSIC_GOOGLE_CLIENT_ID` | Yes for login | OAuth desktop client ID |
| `NGMUSIC_GOOGLE_CLIENT_SECRET` | Only if supplied by Google | OAuth client secret |
| `NGMUSIC_YOUTUBE_API_KEY` | Optional | Search without an authenticated OAuth session |

### Temporary PowerShell configuration

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="your-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="your-client-secret"
$env:NGMUSIC_YOUTUBE_API_KEY="your-api-key"
ngmusic
```

### Persistent per-user configuration

```powershell
[Environment]::SetEnvironmentVariable(
  "NGMUSIC_GOOGLE_CLIENT_ID",
  "your-client-id",
  "User"
)
```

Open a new terminal after changing persistent environment variables.

## OAuth behavior

When `login` is executed:

1. NgMusic generates a PKCE verifier/challenge and random state.
2. A temporary loopback listener starts on `127.0.0.1` using an available port.
3. The system browser opens Google's authorization page.
4. Google redirects back to the loopback listener.
5. NgMusic validates the OAuth state and exchanges the authorization code.
6. The resulting token information is stored in Windows Credential Manager under `NgMusic.GoogleOAuth`.

NgMusic never requests or stores the user's Google password.

## Logout

```text
PS Music:\> logout
```

This removes NgMusic's saved OAuth token from Windows Credential Manager. It does not delete or change the Google account.

## Credentials and source control

Never commit OAuth client secrets, API keys, signing credentials, private certificates, or exported token files.

The repository's build and release process must remain functional without embedding private credentials in source code.
