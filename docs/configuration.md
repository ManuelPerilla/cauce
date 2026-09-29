# Configuration and Google OAuth

[Documentation home](../README.md) · [Installation](installation.md) · [Security](security.md)

NgMusic uses Google's OAuth 2.0 Desktop application flow and YouTube Data API v3.

## Recommended setup: interactive wizard

Create a Google Cloud project, enable **YouTube Data API v3**, and create an OAuth client of type **Desktop app**.

Then simply run:

```text
PS Music:\> login
```

If no Client ID is configured, NgMusic automatically starts the setup wizard:

```text
Google OAuth setup
------------------
NgMusic needs a Google OAuth Client ID of type Desktop app.

 [1] Paste Google OAuth Client ID
 [2] Open step-by-step setup instructions
 [3] Cancel
```

Choose option 1 and paste the value ending in `.apps.googleusercontent.com`.

NgMusic stores that non-secret Client ID in:

```text
%LOCALAPPDATA%\NgMusic\config.json
```

The wizard does **not** store OAuth access tokens, refresh tokens, API keys, or client secrets in this file.

## Configuration commands

```text
setup
configure
config show
config path
config reset
```

- `setup` / `configure`: run or replace local OAuth configuration.
- `config show`: display configuration status without printing complete secrets.
- `config path`: print the local config path.
- `config reset`: remove NgMusic's local JSON configuration.

`config reset` does not remove the OAuth login token. Use `logout` for that.

## Environment-variable overrides

Advanced or centrally managed setups can still use environment variables.

| Variable | Required | Purpose |
| --- | --- | --- |
| `NGMUSIC_GOOGLE_CLIENT_ID` | Alternative to local config | OAuth Desktop Client ID |
| `NGMUSIC_GOOGLE_CLIENT_SECRET` | Only if supplied by Google | Optional OAuth client secret |
| `NGMUSIC_YOUTUBE_API_KEY` | Optional | Search without authenticated OAuth |

An environment Client ID takes precedence over the Client ID saved by the wizard.

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="your-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="your-client-secret"
$env:NGMUSIC_YOUTUBE_API_KEY="your-api-key"
```

## OAuth behavior

When `login` is executed:

1. NgMusic checks environment configuration and the local settings file.
2. If no Client ID exists, it launches the interactive setup wizard.
3. NgMusic generates a PKCE verifier/challenge and random OAuth state.
4. A temporary loopback listener starts on `127.0.0.1` using an available port.
5. The system browser opens Google's authorization page.
6. Google redirects back to the loopback listener.
7. NgMusic validates the OAuth state and exchanges the authorization code.
8. OAuth token information is stored in Windows Credential Manager under `NgMusic.GoogleOAuth`.

NgMusic never requests or stores the user's Google password.

## Logout

```text
PS Music:\> logout
```

This removes NgMusic's saved OAuth token from Windows Credential Manager.

## Storage model

| Data | Storage |
| --- | --- |
| Google OAuth Client ID | Local JSON config or environment |
| OAuth access/refresh token | Windows Credential Manager |
| Optional Client Secret | Environment only |
| Optional YouTube API key | Environment only |

Never commit OAuth secrets, API keys, signing credentials, private certificates, or exported token files.
