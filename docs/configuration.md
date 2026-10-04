# Cauce preferences and accounts

[Documentation](README.md) · [Privacy](security.md) · [Accounts](cauce-accounts.md)

Local listening works without configuration or an account. Change preferences through the desktop controls; avoid editing `library.json` while Cauce is running.

## Listening and appearance

| Setting | Default | Behavior |
| --- | --- | --- |
| Genre | **Todos** | Restricts automatic selection when a specific genre is chosen; changing it starts a new session |
| Allow repeats | Off | Excludes tracks already selected in the current session |
| Artist spacing | 2 songs | Excludes artists among the most recent selected tracks; range 0–5 |
| Theme | **Sistema** | Follows Windows appearance; **Claro**, **Oscuro**, **Bosque** and **Alto contraste** are also available |
| Reduced motion | Off | Disables interaction animations |
| Reduced transparency | Off | Uses opaque surfaces |
| Tips | On | Shows optional interface help |

These settings, introduction completion and library references are saved in `%LOCALAPPDATA%\Cauce\library.json`. Current playback, volume, compact-view state, account identity and listening history are not persisted. The versioned library schema limits data to 10,000 references and 16 MiB and protects newer-version data against overwriting.

Artist and genre edits affect Cauce metadata only. The importer reads MP3 ID3v1 data when available; it does not infer genre, write tags or fetch metadata remotely. Fill missing artist information to make spacing meaningful.

## Optional account integration

The distributor can place public `auth.json` configuration beside `Cauce.exe` to connect a trusted HTTPS OIDC broker. The client uses the system browser, Authorization Code with PKCE, state/nonce and signed-token validation. The broker handles Google, Apple, Facebook and Microsoft registrations and provider secrets.

Without valid configuration, the buttons stay disabled. A valid file enables only the client flow; it does not prove the broker is reachable or provider registrations work. Do not put passwords, client secrets, private keys or tokens into that file. Never weaken certificate, issuer, audience or signature checks to resolve errors.

See [complete broker configuration and validation](cauce-accounts.md). Sign-in retains a display identity for the running process only. It does not synchronize the library, grant subscription access or turn service links into streams.

## Export and reset

**Cuenta → Exportar mis datos** saves a JSON copy of metadata and preferences to a location you choose. It includes file paths and service URLs; review it before sharing. Importing that JSON is not an in-app feature in this preview.

**Cuenta → Restablecer datos locales** asks for confirmation and clears Cauce metadata, recovery files and the local account session. It keeps original music and unrelated files intact. The running app then stores fresh default metadata. Repeat the introduction from **Guía**.
