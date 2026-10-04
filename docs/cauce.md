# Cauce desktop guide

Cauce `0.6.0-rc.1` is a native Windows music-player preview built with C#/.NET 10 and WPF. It offers quiet local playback, understandable listening rules and clear source availability. The portable preview is unsigned; real account integration requires external broker/provider setup.

[Documentation index](README.md) · [Installation](installation.md) · [Accounts](cauce-accounts.md)

## What works in the preview

- Local MP3, WAV and M4A playback through Windows media support, with visible failure handling for unsupported files/codecs.
- Reference-only import. Audio stays in place and is never copied or retagged. MP3 ID3v1 metadata is read when present; other files initially use their filename. Artist and genre can be edited inside Cauce.
- Deterministic genre queues with repeat control and artist spacing. A session that runs out explains why instead of changing its rules.
- HTTPS service references stored separately from playable files. A link opens its service; it is not an integrated stream or verified catalog item.
- System, light, dark, forest and high-contrast appearance, reduced motion, opaque surfaces, compact transport and notification-area controls.
- A repeatable four-step introduction, optional tips, common questions, export/reset controls and user-reviewed report drafts.
- An optional OIDC client using the system browser, disabled without valid broker configuration. See [accounts](cauce-accounts.md).

## First listen

1. Open **Biblioteca → Añadir archivos** and select music on your device.
2. Select a reference and fill missing artist or genre using **Guardar datos**. Edits affect Cauce only.
3. In **Escuchar**, choose a genre and press **Reproducir**. **Todos** allows all local genres.
4. Set artist spacing between zero and five songs. Allow repeats if you want the session to reuse songs.

Changing genre starts a fresh in-memory session. **Siguiente** honors the same rules and explains when no candidate is eligible. Unheard songs are preferred, then the least recently selected; library order breaks ties. Missing artist metadata cannot establish artist spacing, so complete it for predictable sessions.

To save a service reference, expand **Guardar un enlace de música** in **Biblioteca**, enter its title and HTTPS URL, and press **Guardar**. **Abrir enlace seleccionado** opens the service. Those references stay outside the local queue; Cauce does not stream remote music, authenticate subscriptions or query a service catalog.

## Discreet playback and appearance

**Compacto** switches to a small transport view; **Ampliar** restores the full interface. Minimizing hides the window in the notification area while playback can continue. Double-click the icon or choose **Abrir Cauce** to restore it. **Alt+Shift+C** also restores it when Windows permits registration. Closing the window or choosing **Salir** exits and attempts to save pending preferences.

Choose **Sistema**, **Claro**, **Oscuro**, **Bosque** or **Alto contraste** in **Apariencia**. Reduced motion disables interaction animations; reduced transparency uses opaque surfaces. Tips are optional. Repeat the introduction from **Guía → Volver a ver la bienvenida**. See [interface actions](commands.md).

## Data and privacy

Only references, small text metadata and preferences are saved in `%LOCALAPPDATA%\Cauce\library.json`. Versioned JSON is atomically replaced and capped at 10,000 references and 16 MiB. History exists only during the current session. There is no telemetry, ad identifier, artwork cache, automatic report upload or audio duplication.

Availability is checked against the filesystem. Moving a file makes its reference unavailable; restore the path or import the new location. Corrupt metadata is preserved in at most three named recovery files with a visible warning. Newer-schema data is not overwritten. When recovery slots are full, the original is preserved for investigation.

**Cuenta → Exportar mis datos** writes JSON to a chosen location. It includes personal paths and URLs; review it before sharing. JSON import is not implemented in the interface. **Restablecer datos locales** asks for confirmation, clears references/preferences/recovery files and local identity, and saves fresh defaults. Original audio and unrelated files stay intact.

Account identity lasts only for the running process. Tokens and provider secrets are not stored in the library. Sign-in does not synchronize music/preferences, authorize a music service or create a persistent account session. Browser cookies are managed independently. Read [security and privacy](security.md).

## Support and bug reports

Use **Soporte → Preparar reporte de un problema**. Cauce opens a [GitHub draft](https://github.com/ManuelPerilla/cauce/issues/new) with app/Windows versions and editable prompts. It submits nothing automatically and attaches no logs or files. Review screenshots and personal paths before sending. Sensitive vulnerabilities follow [SECURITY.md](../SECURITY.md); other problems are covered in [troubleshooting](troubleshooting.md).

## Build and verify

Development requires Windows and the .NET 10 SDK. From the repository root:

```powershell
./build/dev.ps1
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1 -Runtime win-x64
```

The portable ZIP includes its runtime and needs no administrator installation. CI verifies the desktop and uploads review artifacts. The release workflow builds versioned x64/ARM64 ZIPs and prepares a draft for review. See [building and releasing](releasing.md).

Synthetic core/authentication checks and rendered-interface smoke tests verify their stated boundaries. They do not establish real codec playback, live sign-in or performance on user hardware. A policy-blocked executable is not a passed test; preserve Windows protection.

## Architecture and resource budget

`Cauce.Core` owns models, import, queue selection and bounded metadata without WPF, network or OAuth dependencies. `Cauce.Desktop` owns presentation and native audio/account adapters. `MainViewModel` coordinates commands; one native decoder handles playback. Import runs away from the UI dispatcher, reads bounded MP3 metadata and avoids loading entire audio into memory.

Glass-inspired surfaces use static translucent resources, frozen brushes and restrained highlights. Button animations are brief, triggered by interaction and disabled by reduced motion. There are no looping visualizers or web rendering processes for music. Rows are virtualized, progress updates run once per second during playback and preference saves are debounced. Read [architecture](architecture.md).

CPU/memory claims need measurements on representative devices: startup, a 10,000-entry library, sustained/minimized playback and repeated theme/view changes. Check memory stabilization across use, not only a small startup number.

## Signing readiness

The desktop has explicit Cauce product/version metadata, an `asInvoker` manifest and no self-update or downloaded-executable behavior. `build/cauce.ps1` can use an installed code-signing certificate, timestamp/verify binary signatures and package checksums. No signing secret belongs in source control.

The preview is unsigned. A valid certificate or approved signing route remains necessary before signed public distribution. SignPath Foundation approval/configuration is not established; Store/MSIX packaging is future work. There is no `LICENSE` file, so the owner must choose a license before pursuing a route that requires it. These source changes do not grant a license.

Signing requests require explicit human approval under the [code signing policy](code-signing-policy.md). A checksum or CI success is not a publisher signature. The portable ZIP is not a signed installer or Store package.

## Public-release boundaries

Real Google/Apple/Facebook/Microsoft sign-in needs registrations, a trusted HTTPS broker and live checks of consent, callbacks, cancellation, logout, key rotation and error paths. External links stay external. Integrated catalogs, cloud sync, ID3v2/FLAC tagging, verified catalog equivalence and signed installer delivery are not included. Keep these limits explicit in product descriptions and release notes.
