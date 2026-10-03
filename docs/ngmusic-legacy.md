# NgMusic

**PowerShell-inspired terminal music player for Windows with Google OAuth, YouTube playback, graphical/MSI installers, and portable builds.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **Repository description:** PowerShell-inspired terminal music player for Windows with Google OAuth, YouTube playback, MSI/Setup.exe and portable builds.

## Installation options

All three distribution channels are supported and lead to the same usable NgMusic configuration.

| Package | Experience | OAuth setup |
| --- | --- | --- |
| `*-setup.exe` | Recommended graphical Next → Next → Install wizard | During installer wizard |
| `*.msi` | Native Windows Installer | Post-install configurator opens automatically for interactive installs |
| `*-portable.zip` | Extract and run | First `login` launches terminal setup |

### Graphical Setup.exe

The recommended option. It collects the Google OAuth Desktop Client ID, installs the matching MSI, configures shortcuts, adds NgMusic to `PATH`, and can launch NgMusic when finished.

### Direct MSI

The MSI now installs the same application payload and also includes the same configuration capability.

When you double-click the MSI interactively, Windows installs NgMusic and then opens **Finish setting up NgMusic**, where you can:

- enter the Google OAuth Desktop Client ID;
- create a Start-menu shortcut;
- create a desktop shortcut;
- launch NgMusic.

For managed silent deployments, the MSI remains silent and does not unexpectedly open configuration windows. Administrators can provide environment configuration or users can run `setup` later.

### Portable

Extract and run `ngmusic.exe`. No Program Files or `PATH` changes. If OAuth is not configured, the first `login` opens the terminal setup wizard.

## Ready-to-use flow

After either Setup.exe or an interactive MSI installation:

```text
PS C:\> ngmusic

PS Music:\> login
PS Music:\> search "Massive Attack Teardrop"
PS Music:\> play 1
```

Official releases are self-contained, so users do not need to install .NET separately.

## OAuth storage

The non-secret Google OAuth Desktop Client ID is stored in:

```text
%LOCALAPPDATA%\NgMusic\config.json
```

OAuth access/refresh tokens remain in **Windows Credential Manager**.

Client secrets and API keys are not written to NgMusic's JSON configuration.

## Windows packages

NgMusic releases x64, ARM64, and x86 variants of:

- graphical Setup.exe;
- MSI;
- portable ZIP.

Use the [latest GitHub Release](https://github.com/ManuelPerilla/ngmusic/releases/latest).

## Documentation

- [Installation and package behavior](docs/installation.md)
- [Google OAuth configuration](docs/configuration.md)
- [Command reference](docs/commands.md)
- [Architecture](docs/architecture.md)
- [Security and privacy](docs/security.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Building and releasing](docs/releasing.md)
- [Code signing policy](docs/code-signing-policy.md)

## Code signing

SignPath Foundation approval is pending. When official signing is enabled:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**


## Google OAuth Client Secret

Some Google OAuth clients require a Client Secret during token exchange. NgMusic 0.5.2+ supports this securely through Setup.exe, the MSI configurator, or the `setup` command.

The Client Secret is stored in Windows Credential Manager and is never written to `config.json`.
