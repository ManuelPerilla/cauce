# NgMusic

**A PowerShell-inspired, terminal-first music controller for Windows, using Google OAuth and YouTube's supported embedded player.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **Project status:** early-stage but usable. Public releases are built automatically with GitHub Actions for Windows x64, ARM64, and x86.

## Recommended install: Setup.exe

For the smoothest experience, download the architecture-matching graphical installer:

```text
NgMusic-<version>-win-x64-setup.exe
NgMusic-<version>-win-arm64-setup.exe
NgMusic-<version>-win-x86-setup.exe
```

The wizard walks through:

1. Welcome
2. Google OAuth Desktop Client ID
3. Optional shortcuts
4. Windows installation
5. Finish / launch NgMusic

The installer embeds the matching MSI, installs the self-contained app under Program Files, adds `ngmusic` to system `PATH`, saves the non-secret Google OAuth Client ID for the current user, and optionally creates Start-menu/Desktop shortcuts.

After setup, the intended first session is:

```text
PS C:\> ngmusic
PS Music:\> login
PS Music:\> search "Massive Attack Teardrop"
PS Music:\> play 1
```

No manual .NET installation or Client-ID environment variable is required.

## Other distribution options

| Option | Best for |
| --- | --- |
| `*-setup.exe` | Recommended guided installation |
| `*.msi` | Direct/managed Windows Installer deployment |
| `*-portable.zip` | No installation / removable folder |

All official builds are self-contained.

## What NgMusic is

NgMusic is a personal Windows music shell for people who prefer keyboard and terminal workflows. It provides PowerShell-like commands for search, playback, volume, seeking, queue management, and Google authentication.

Playback remains inside YouTube's visible official IFrame player. NgMusic does not extract raw media streams or download tracks.

## OAuth and local configuration

The graphical installer can save the Google OAuth **Desktop app Client ID** before NgMusic ever runs.

That Client ID is not secret and is stored in:

```text
%LOCALAPPDATA%\NgMusic\config.json
```

OAuth access/refresh tokens remain in **Windows Credential Manager**.

If setup was skipped or the Client ID was not supplied, `login` still falls back to NgMusic's interactive terminal setup wizard.

## Documentation

- [Installation](docs/installation.md)
- [Configuration and Google OAuth](docs/configuration.md)
- [Command reference](docs/commands.md)
- [Architecture](docs/architecture.md)
- [Security and privacy](docs/security.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Building and releasing](docs/releasing.md)
- [Code signing policy](docs/code-signing-policy.md)

## Code signing status

SignPath Foundation approval is pending. If official signing becomes active:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**
