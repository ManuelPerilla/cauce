# Cauce security and privacy

[Documentation](README.md) · [Accounts](cauce-accounts.md) · [Signing policy](code-signing-policy.md)

Cauce plays local music without an account and has no telemetry backend, ad identifiers or automatic bug uploads. Optional account integration and user-opened links create separate network boundaries.

## Data handled

| Data | Handling |
| --- | --- |
| Local audio | Opened in its original location for native playback; never copied or edited |
| Library metadata | Paths, service URLs, title/artist/genre and preferences in `%LOCALAPPDATA%\Cauce\library.json` |
| Listening session | Selection history in memory; not persisted |
| Account identity | Display identity in memory for the current process; no cloud sync |
| OIDC tokens/codes | Temporarily processed to validate sign-in; not persisted, refreshed or included in reports |
| Bug report | GitHub draft for user review, with app/Windows versions and editable prompts; no attached logs or files |
| Export | User-chosen JSON with references and preferences, including paths and URLs |

Metadata is limited to 10,000 references and 16 MiB. The versioned JSON schema has field validation, atomic replacement and bounded recovery copies. A newer schema is not overwritten. Exports and screenshots can reveal personal paths or URLs; review them before sharing.

## Network activity

Local playback, genre selection and themes do not require remote requests. Account sign-in, when configured and requested, opens the system browser and uses HTTPS to the selected identity broker and its providers. Discovery/JWKS requests belong to that flow. The temporary callback listener binds only to IPv4 loopback and closes on completion, cancellation or timeout.

Opening a saved HTTPS music link delegates the action to the system browser; that service governs its own playback and privacy. Cauce does not query streaming catalogs, fetch artwork, validate subscriptions or automatically test links. Preparing a support report opens GitHub; submission requires the user's action there.

## Account safeguards

The native client is public and has no embedded secret. Provider secrets and private keys belong at the broker. The client fixes its scope to `openid profile`, does not request `offline_access`, verifies PKCE/state/nonce and requires signed identity tokens with the expected issuer/audience and valid expiry. Never weaken those checks or TLS validation to make login pass.

Account sign-in does not authorize a music service. Sign-out removes local identity and cancels pending sign-in; browser cookies and the provider's global session remain governed by the browser/provider. Callback limits and registration prerequisites are documented in [accounts](cauce-accounts.md).

## Release and reporting boundaries

The preview is unsigned. Build artifacts and draft releases use SHA-256 checksums, which are integrity values rather than publisher signatures. Secrets and signing keys must not appear in commits, logs or packages. See [releasing](releasing.md) and [the signing policy](code-signing-policy.md).

Use [GitHub Issues](https://github.com/ManuelPerilla/cauce/issues) for ordinary bugs. Sensitive details follow [SECURITY.md](../SECURITY.md); do not post exploitable details or credentials publicly.
