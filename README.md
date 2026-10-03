# Cauce

**Tu música, sin perder el hilo.**

Native Windows music player with a quiet, glass-inspired interface, genre-focused listening sessions, and a library that distinguishes playable files from service links. Cauce is the next direction for NgMusic.

> **0.6.0-alpha.1 — development preview.** The desktop builds and has automated checks. Public signing and real social-provider login require external setup. This is not a finished streaming service.

[Español](README.es.md) · [User guide and architecture](docs/cauce.md) · [Accounts](docs/cauce-accounts.md) · [Legacy NgMusic](docs/ngmusic-legacy.md)

## The experience

- Add local MP3, WAV and M4A without copying your audio. Edit library metadata without modifying files.
- Keep a session within a genre, control artist spacing and repetition, and see why the queue can or cannot continue.
- Save HTTPS references to music services with honest availability: a link is not a playable stream.
- Switch between a full library and a compact player; use tray controls and an optional global shortcut.
- Choose system, light, dark, forest or high-contrast themes. Reduce movement and transparency independently.
- Learn through a repeatable introduction, optional tips and a built-in guide. Review bug reports before sending them.
- Keep a bounded local library with export/reset controls and no listening-history persistence or telemetry.

## Run

On Windows with the .NET 10 SDK:

```powershell
dotnet run --project src/Cauce.Desktop
```

The library is stored in %LOCALAPPDATA%\\Cauce. Listening to local files does not require an account or administrator rights.

## Verify and package

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1
```

The package is explicitly unsigned unless a real signing certificate is supplied. CI produces review artifacts, not public releases. Read the [signing and verification boundaries](docs/cauce.md#signing-readiness).

## Accounts and sources

Google, Apple, Facebook and Microsoft sign-in are wired through a configurable OIDC identity service. Buttons remain disabled until that service and real provider registrations are configured. The native client uses the system browser, PKCE, state/nonce and signed-token validation. No provider secret is embedded in the executable.

The account is optional and separate from music-source authorization. There is no cloud sync or integrated commercial streaming catalog in this preview. See [configuration and prerequisites](docs/cauce-accounts.md).

## Project structure

- src/Cauce.Core — bounded library persistence, metadata, import and deterministic session rules; no UI/network dependencies.
- src/Cauce.Desktop — WPF presentation, native playback, themes, tray and optional authentication.
- tests/Cauce.* — queue/storage, authentication-boundary and rendered-interface checks.
- src/NgMusic* — original terminal player retained during migration.

For legacy YouTube terminal commands and installers, see [the preserved NgMusic guide](docs/ngmusic-legacy.md). Earlier translated guides refer to that version.
