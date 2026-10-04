# Cauce

**Tu música, sin perder el hilo.**

A native Windows music player built with C#/.NET 10 and WPF. Cauce keeps listening sessions within the genre you choose, explains its queue decisions and distinguishes playable local files from external service links.

> **0.6.0-alpha.1 — development preview.** The portable preview is unsigned. Account sign-in requires an identity broker and real provider registrations. Integrated streaming and cloud synchronization are not available.

[Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md) · [Documentation](docs/README.md)

## What Cauce does

- Plays local MP3, WAV and M4A using Windows media support. Importing stores references; it never copies or edits your audio.
- Applies deterministic genre, artist-spacing and repeat rules. If no song meets them, the player explains why instead of silently changing the session.
- Saves HTTPS service links separately. Opening a link delegates playback to that service; it does not add a stream to Cauce's local queue.
- Offers a compact player, notification-area controls and the optional **Alt+Shift+C** restore shortcut.
- Includes system, light, dark, forest and high-contrast themes, reduced motion and opaque surfaces.
- Provides a repeatable introduction, optional tips, a guide and bug-report drafts you review before sending.
- Stores bounded metadata and preferences locally, with export and reset controls. There is no persistent listening history or telemetry.

## Run and verify

Development requires Windows and the .NET 10 SDK. From the repository root:

```powershell
./build/dev.ps1
```

Run the checks and create an unsigned, self-contained review ZIP:

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1 -Runtime win-x64
```

The portable package includes its .NET runtime. Extract the complete ZIP and run `Cauce.exe`; no account or administrator installation is needed for local playback. See [installation](docs/installation.md) and [building and releasing](docs/releasing.md).

## Accounts and data

The library lives in `%LOCALAPPDATA%\Cauce`. Cauce stores file paths, service URLs, small text metadata and preferences, capped at 10,000 references and 16 MiB. Audio remains in its original location. Exports contain paths and URLs, so review them before sharing.

Google, Apple, Facebook and Microsoft buttons use an optional configurable OIDC identity broker. They remain disabled without valid configuration; provider credentials belong on the broker, never inside the Windows client. The account identifies you only during the running process and does not synchronize your library or authorize a music subscription. Read [accounts and prerequisites](docs/cauce-accounts.md) and [security and privacy](docs/security.md).

## Project structure

| Path | Responsibility |
| --- | --- |
| `src/Cauce.Core` | Library models, bounded persistence, reference import and deterministic queue rules |
| `src/Cauce.Desktop` | WPF interface, native audio, themes, tray controls and optional account adapter |
| `tests/Cauce.Core.Tests` | Queue, import and storage checks |
| `tests/Cauce.Auth.Tests` | Identity and callback checks using synthetic data |
| `tests/Cauce.Desktop.Smoke` | Rendered interface, bindings, themes and preference checks |
| `build` | Development, portable review and release packaging |
| `.github/workflows` | Windows verification and draft release delivery |

The `archive/` directory contains reference code excluded from the maintained builds and distributed packages.

Start with the [detailed guide](docs/cauce.md), [contribution guide](CONTRIBUTING.md) or [security policy](SECURITY.md). The repository currently has no `LICENSE` file; public access to source does not imply an open-source license.
