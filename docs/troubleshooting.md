# Troubleshooting

## OAuth browser says success but terminal reports an error

NgMusic 0.5.1 and later only shows a successful browser page after the authorization code has been exchanged for tokens and saved.

If Google rejects the token exchange, NgMusic now prints Google's actual OAuth error code and description in the terminal, for example `invalid_grant` or `invalid_request`.

This is important because a generic HTTP 400 is not specific enough to diagnose OAuth configuration.

## Common Google OAuth errors

- `invalid_grant`: authorization code, PKCE verifier/challenge, or redirect URI did not match what Google expected. Retry `login`; if it repeats, report the complete NgMusic error line.
- `invalid_request`: a required OAuth parameter is missing or malformed.
- `invalid_client`: verify that the Client ID is a **Desktop app** OAuth client and has not been deleted/disabled.
- `redirect_uri_mismatch`: verify the OAuth client type is **Desktop app**, not Web application.
- `access_denied`: the user or organization denied the requested permissions.

Google's Desktop flow supports loopback redirects on `127.0.0.1` with a dynamic port and PKCE.

## `ngmusic` is not recognized

Open a new terminal after MSI/Setup installation so Windows reloads the system `PATH`.

## Search requires authentication

Run `login`, or configure the optional YouTube API key.

## SmartScreen / Defender warning

Use official GitHub Release artifacts and verify `SHA256SUMS.txt`. Do not disable antivirus globally.
