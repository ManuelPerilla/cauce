# NgMusic

**A PowerShell-inspired, terminal-first music controller for Windows, using Google OAuth and YouTube's supported embedded player.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **Project status:** early-stage but usable. Public releases are built automatically on GitHub Actions for Windows x64, ARM64, and x86.

## What NgMusic is

NgMusic is a personal Windows music shell designed for people who prefer a keyboard and a terminal over a conventional media-player UI. It provides PowerShell-like commands for search, playback, volume, seeking, queue management, and Google authentication.

Playback remains inside YouTube's visible official IFrame player. NgMusic does not extract raw media streams, download tracks, or create a hidden audio-scraping layer.

```text
NgMusic 0.2.0

PS Music:\> login
✓ Connected as you

PS Music:\> search "Massive Attack Teardrop"
 [ 1] Massive Attack - Teardrop  —  Massive Attack

PS Music:\> play 1
✓ ▶ Massive Attack — Massive Attack - Teardrop

PS Music:\> volume 42
volume 42%
```

## Features

- Google OAuth 2.0 desktop authentication with PKCE and loopback redirect.
- OAuth tokens stored in **Windows Credential Manager**, not plaintext files.
- Search through YouTube Data API v3.
- Visible YouTube IFrame playback controlled from the terminal.
- Queue and playback history.
- Commands including `play`, `pause`, `resume`, `stop`, `seek`, `volume`, `next`, `prev`, `queue`, and `now`.
- Self-contained Windows releases: the end user does not need to install .NET.
- MSI and portable ZIP distributions for x64, ARM64, and x86.
- Reproducible release pipeline on GitHub Actions with SHA-256 checksums.

## Download

Use the [latest GitHub Release](https://github.com/ManuelPerilla/ngmusic/releases/latest).

| Windows architecture | Installer | Portable |
| --- | --- | --- |
| x64 | `*-win-x64.msi` | `*-win-x64-portable.zip` |
| ARM64 | `*-win-arm64.msi` | `*-win-arm64-portable.zip` |
| x86 | `*-win-x86.msi` | `*-win-x86-portable.zip` |

The MSI is a per-machine installation under Program Files and adds NgMusic to the system `PATH`. It may require administrator approval. The portable ZIP modifies neither Program Files nor `PATH`.

See [Installation](docs/installation.md).

## First-time configuration

NgMusic needs a Google OAuth Desktop application client and YouTube Data API v3 access.

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="your-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="your-client-secret" # only if provided
$env:NGMUSIC_YOUTUBE_API_KEY="your-api-key"            # optional after OAuth login
```

Do not commit credentials to the repository. See [Configuration](docs/configuration.md).

## Documentation

- [Installation](docs/installation.md)
- [Configuration and Google OAuth](docs/configuration.md)
- [Command reference](docs/commands.md)
- [Architecture](docs/architecture.md)
- [Security and privacy](docs/security.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Building and releasing](docs/releasing.md)
- [Code signing policy](docs/code-signing-policy.md)

Translated documentation is available under [docs/i18n](docs/i18n/).

## Security and privacy

NgMusic has no project-operated analytics or telemetry backend. Network requests are made only as part of user-facing functionality such as Google login, YouTube search, and video playback. OAuth tokens are stored in Windows Credential Manager.

The local player and OAuth redirect servers bind only to loopback (`127.0.0.1`) on ephemeral ports.

Read the full [security and privacy documentation](docs/security.md).

## Code signing policy

NgMusic maintains a documented release-signing policy and separates source control, automated builds, release artifacts, and signing approval.

**Code signing status:** SignPath Foundation approval is pending. If code signing is enabled for official releases: **Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

See [Code signing policy](docs/code-signing-policy.md).

## Development

Development requires the .NET 10 SDK.

```powershell
.\build\dev.ps1
```

Or:

```powershell
dotnet run --project .\src\NgMusic\NgMusic.csproj
```

## Release builds

```powershell
.\build\release.ps1 -Version 0.2.0
```

The release pipeline produces architecture-specific MSI installers, portable ZIP files, a combined Windows package, and SHA-256 checksums. See [Building and releasing](docs/releasing.md).

## Current limitations

- Playback currently opens a visible browser-hosted IFrame player rather than an integrated WebView2 window.
- Playlist-management commands are not yet implemented.
- Public code signing is not active until a signing provider approves the project.
- x86 is maintained primarily for legacy compatibility.

## Contributing

Issues and pull requests are welcome. Keep changes focused, avoid committing credentials or generated release binaries, and document user-visible behavior.

External pull requests require maintainer review before merge.

## Project links

- Repository: https://github.com/ManuelPerilla/ngmusic
- Releases: https://github.com/ManuelPerilla/ngmusic/releases
- Build history: https://github.com/ManuelPerilla/ngmusic/actions
