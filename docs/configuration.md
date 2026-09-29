# Configuration and Google OAuth

NgMusic supports Google OAuth Desktop clients that use only a Client ID and clients that also require a Client Secret.

## Values from Google Cloud

From **Google Auth Platform → Clients → your Desktop client**, copy:

- **Client ID**
- **Client Secret**, if Google shows/provides one

Google documents `client_secret` as optional for installed-app token exchange, but some client configurations can require it.

## Storage model

| Value | Storage |
| --- | --- |
| OAuth Client ID | `%LOCALAPPDATA%\NgMusic\config.json` |
| OAuth Client Secret | Windows Credential Manager |
| OAuth access/refresh token | Windows Credential Manager |
| Optional YouTube API key | Environment variable |

The Client Secret is never written to NgMusic's JSON configuration.

## Setup.exe and MSI configurator

Both graphical configuration flows now include:

- Google OAuth Client ID
- Google OAuth Client Secret (optional)

If Google provided a Client Secret for your client, paste it. Otherwise leave the field empty.

## Terminal setup

Run:

```text
PS Music:\> setup
```

NgMusic asks for the Client ID and then:

```text
Google OAuth Client Secret (optional, press Enter if Google did not provide one):
```

The secret is typed without being echoed to the terminal.

## Environment overrides

Advanced deployments may still use:

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="..."
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="..."
```

Environment values take precedence over locally stored values.
