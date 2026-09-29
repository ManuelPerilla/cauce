# NgMusic

A tiny personal Windows music shell: PowerShell-ish commands in the terminal, with YouTube doing playback through its visible official IFrame player.

```text
NgMusic 0.2  // PowerShell-ish YouTube music controller

PS Music:\> login
✓ Connected as you

PS Music:\> search "Massive Attack Teardrop"
 [ 1] Massive Attack - Teardrop  —  Massive Attack
 [ 2] Teardrop (Live)           —  Massive Attack

PS Music:\> play 1
✓ ▶ Massive Attack — Massive Attack - Teardrop

PS Music:\> volume 42
volume 42%
```

## Current features

- Google OAuth 2.0 desktop login with PKCE and a loopback redirect.
- OAuth/refresh tokens stored in **Windows Credential Manager**, not plaintext config.
- YouTube Data API search.
- Visible YouTube IFrame player hosted on localhost and controlled from the terminal.
- `play`, `pause`, `resume`, `stop`, `seek`, `volume`, `next`, `prev`, `queue`, and `now`.
- No media-stream extraction, downloading, or hidden audio scraping.

## Windows support

Release packages target the portable Windows RIDs:

| Platform | Release |
| --- | --- |
| Windows x64 | ✅ MSI + portable ZIP |
| Windows ARM64 | ✅ MSI + portable ZIP |
| Windows x86 | ✅ MSI + portable ZIP (legacy) |

NgMusic targets modern Windows 10/11. Each release is architecture-specific because self-contained .NET single-file applications are platform/architecture-specific.

## Two ways to use it

### 1. MSI installer

Use the MSI matching your machine. It installs NgMusic under Program Files, creates a Start-menu entry, adds `ngmusic` to `PATH`, and supports normal Windows upgrades/uninstall.

### 2. Portable

Download the matching `*-portable.zip`, extract it anywhere, and run `ngmusic.exe`. It does not modify Program Files, shortcuts, or `PATH`.

Both formats are **self-contained**: end users do not need to install .NET separately.

See [Installation](docs/installation.md) for architecture selection and Google OAuth setup.

## Development

Development requires the **.NET 10 SDK**:

```powershell
.\build\dev.ps1
```

Or directly:

```powershell
dotnet run --project .\src\NgMusic\NgMusic.csproj
```

## Build releases

```powershell
.\build\release.ps1 -Version 0.2.0
```

That produces MSI installers and portable ZIPs for x64, ARM64, and x86 under `artifacts/release/`. See [Release process](docs/releasing.md).

Release settings use self-contained single-file publishing plus ReadyToRun. Trimming is intentionally disabled until the full dependency graph is proven trim-safe.

## Why a visible player?

YouTube's public APIs do not provide a general raw-audio YouTube Music playback endpoint. NgMusic keeps playback inside the official embedded player instead of extracting media streams. The terminal controls that player through a small localhost command bridge.

## Security notes

- NgMusic never asks for your Google password.
- Google login happens in your system browser.
- OAuth redirects listen only on loopback (`127.0.0.1`).
- Saved OAuth data lives in Windows Credential Manager under `NgMusic.GoogleOAuth`.
- `logout` removes that saved credential.
- Client IDs/secrets and signing credentials must never be committed.

## Architecture

See [Architecture](docs/architecture.md).

## Roadmap

- Move the visible player from a browser tab into a WebView2 window while preserving the `IPlayer` contract.
- Playlist commands through the YouTube Data API.
- Richer queue state and playback events.
- Aliases/pipeline-ish syntax (`search ... | play 1`).
- Authenticode signing for public release binaries.
