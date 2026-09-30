# Troubleshooting

## `invalid_request: client_secret is missing`

Google accepted browser authorization, but the token endpoint requires this OAuth client's Client Secret.

In NgMusic 0.5.2 or later:

1. Open **Google Auth Platform → Clients**.
2. Open the same OAuth client whose Client ID you configured.
3. Copy its **Client Secret**.
4. Run `setup`.
5. Enter the Client ID and paste the Client Secret when requested.
6. Run `login` again.

NgMusic stores the Client Secret in Windows Credential Manager, not plaintext JSON.

If Google no longer displays the original secret, rotate/create a new secret when available and configure the new value in NgMusic.

## Other OAuth errors

- `invalid_client`: wrong/deleted Client ID or incorrect Client Secret.
- `invalid_grant`: retry login; the authorization code, PKCE verifier, or redirect may no longer be valid.
- `redirect_uri_mismatch`: verify the client is appropriate for a Desktop/installed flow.
- `access_denied`: consent or organization policy denied access.

## Browser success page

NgMusic 0.5.1+ only reports browser success after token exchange succeeds and tokens have been stored.


## YouTube player says "An error occurred. Please try again later"

NgMusic 0.5.3+ filters search results with YouTube's `videoEmbeddable=true` and `videoSyndicated=true` flags so results should be playable in the embedded player.

The player also reports the IFrame error code directly in the terminal:

- `101` / `150`: the video owner blocks embedded playback.
- `153`: YouTube did not receive the required HTTP Referer or equivalent client identity.
- `100`: the video was removed or is private.
- `5`: HTML5 playback failure.
- `2`: invalid video parameter.

NgMusic sends an explicit `strict-origin-when-cross-origin` referrer policy, matching YouTube's recommendation for embedded players.

If autoplay is blocked by the browser, NgMusic reports that separately in the terminal.


## Player error 2 with a valid 11-character video ID

NgMusic 0.5.4 fixes a startup race in which the terminal could send `loadVideoById` after the `YT.Player` object existed but before YouTube's `onReady` event had fired.

YouTube documents `onReady` as the point at which the player is ready to receive API calls. NgMusic now waits for that event before it polls and consumes queued terminal commands.

If error 2 still appears on 0.5.4+, report the video ID and the complete terminal line.
