# Configuration and Google OAuth

[Documentation home](../README.md) · [Installation](installation.md) · [Security](security.md)

NgMusic uses Google's OAuth 2.0 Desktop application flow and YouTube Data API v3.

## Easiest configuration: graphical installer

The recommended `Setup.exe` asks for the Google OAuth **Desktop app Client ID** during installation.

The value normally ends with:

```text
.apps.googleusercontent.com
```

The installer saves only this non-secret Client ID to:

```text
%LOCALAPPDATA%\NgMusic\config.json
```

After that, launch NgMusic and run:

```text
PS Music:\> login
```

Google authorization opens in the system browser.

## Terminal fallback

If the graphical installer did not configure OAuth, `login` automatically starts the terminal setup wizard.

You can also run:

```text
setup
config show
config path
config reset
```

## Environment-variable override

Advanced or centrally managed deployments can still use:

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="your-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="your-client-secret"
$env:NGMUSIC_YOUTUBE_API_KEY="your-api-key"
```

Environment configuration takes precedence over the local Client ID.

## Storage model

| Data | Storage |
| --- | --- |
| OAuth Desktop Client ID | Local JSON config or environment |
| OAuth access/refresh token | Windows Credential Manager |
| Optional Client Secret | Environment only |
| Optional YouTube API key | Environment only |

NgMusic never requests or stores the user's Google password.
