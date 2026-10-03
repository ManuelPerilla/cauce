# Cauce — desktop preview

Cauce is the native Windows successor being developed alongside the original NgMusic terminal application. Its purpose is a quiet music player with understandable listening rules and honest source availability. This preview is not yet a signed public release.

## What works in this preview

- Native playback of local MP3, WAV and M4A using Windows media support. Individual files/codecs can still be unsupported by Windows; playback errors are shown without a crash.
- A local reference library: importing never copies or edits the audio. MP3 ID3v1 metadata is read when present; other files use their filenames until you edit the genre in Cauce.
- Genre-restricted queues, session repeat control and artist spacing. A queue that runs out explains why; it does not silently change genres. Choosing a genre starts a new listening session.
- HTTPS service links saved separately from playable local audio. Links are neither catalog integrations nor verified stream availability.
- Light, dark, forest, system and high-contrast appearances; reduced motion and opaque surfaces; a compact player and tray controls.
- A repeatable four-step introduction, contextual help, FAQ, local-data export/reset and a user-reviewed bug-report draft.
- Configurable broker-based OIDC sign-in using the system browser. It is disabled until real provider registrations and a trusted HTTPS broker are configured; see [accounts](cauce-accounts.md).

## First listen

Open **Biblioteca**, choose **Añadir archivos**, then select music on your device. Set a genre for unclassified files in the library. In **Escuchar**, choose the genre and start playback. Turn on repeats only if you want the session to reuse songs. Use **Abrir enlace** for a saved service reference; that service handles its own playback.

The compact view keeps transport controls close. Minimizing moves the app to the notification area; use its menu to restore or exit. Alt+Shift+C restores the window when Windows allows that shortcut registration. Closing the window exits and saves pending preferences.

## Data and privacy

Only references, small text metadata and preferences are saved under `%LOCALAPPDATA%\Cauce`. Schema-versioned JSON is atomically replaced, capped at 10,000 references and 16 MiB. Listening history is held in memory for the current session, not persisted. The player has no telemetry, ad identifiers, automatic error uploads, audio duplication or artwork cache.

Unreadable library files are preserved in at most three named recovery files, with a visible warning. A file from a newer schema is never overwritten. **Borrar datos locales** removes Cauce's library and those recovery files, without deleting music. Export includes file paths and service URLs; inspect it before sharing.

Account identity is held only for the running process. There is no cloud synchronization in this preview. Provider access tokens, passwords, and private keys are not stored in the library. Signing into Cauce is separate from authorizing any music service.

## Support and bug reports

Use **Soporte → Reportar un error**. Cauce opens a GitHub draft with app/Windows versions. It sends nothing automatically and attaches no logs or files. Review your report before submitting. Security reports follow [SECURITY.md](../SECURITY.md).

## Build and verify

Requires the .NET 10 SDK on Windows.

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet build src/Cauce.Desktop -c Release
dotnet run --project src/Cauce.Desktop
./build/cauce.ps1
```

The portable review package contains its Windows runtime and does not require administrator installation. The CI workflow only creates review artifacts; it does not publish a release or sign anything without signing credentials.

## Architecture and resource budget

`Cauce.Core` owns tracks, rules, candidate selection and bounded durable metadata. It has no WPF, network or OAuth dependency. `Cauce.Desktop` owns presentation and native audio adapters; the view model coordinates user actions. Authentication is an optional adapter that opens the system browser only when requested. The legacy terminal app is preserved during this migration.

Glass is approximated with static translucent surfaces, frozen brushes and restrained highlights. Interaction animations are short, triggered by actual pointer/focus activity and disabled by reduced-motion settings. There are no looping decorative animations, web rendering processes or per-frame audio visualizers. Library items are virtualized. Progress updates run at one-second intervals while playing.

Memory and CPU claims require measurements on representative hardware and libraries; an attractive mockup is not a performance result. Before release, profile startup, a 10,000-entry library, ten minutes of playback, minimized playback and repeated theme/view changes. Check that memory stabilizes rather than merely reporting a small startup number.

## Signing readiness

The desktop has explicit product/version metadata, an `asInvoker` manifest and no self-update/downloading executable behavior. `build/cauce.ps1` can sign Cauce binaries using an already installed certificate, timestamp the signature, verify it and then package checksums. No certificate or signing secret belongs in source control.

Public signing remains an external release prerequisite: a valid certificate, an approved SignPath Foundation project, or a Microsoft Store/MSIX submission. The existing NgMusic signing policy is not evidence that Cauce is already signed. Store packaging and submission are future work; do not describe this ZIP as a Store package. See [Microsoft signing guidance](https://learn.microsoft.com/en-us/windows/msix/package/signing-package-overview) and [SignPath requirements](https://signpath.org/terms.html).

## Boundaries before a public release

Real Google/Apple/Facebook/Microsoft login requires configured provider accounts and an HTTPS identity service. Live sign-ins, expiry, logout and error paths must be tested against those real registrations. Streaming integrations, cloud sync, ID3v2/FLAC tagging, verified catalog equivalence and a signed installer are not implemented by this foundation. Preserve this distinction in product copy and release notes.
