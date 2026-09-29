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
