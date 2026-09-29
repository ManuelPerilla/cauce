# Installation

[Documentation home](../README.md) · [Configuration](configuration.md) · [Troubleshooting](troubleshooting.md)

NgMusic targets modern Windows 10 and Windows 11.

## Recommended: graphical Setup.exe

Download the setup executable matching your architecture:

| System type | Recommended package |
| --- | --- |
| Intel/AMD 64-bit | `NgMusic-<version>-win-x64-setup.exe` |
| Windows on ARM | `NgMusic-<version>-win-arm64-setup.exe` |
| 32-bit Windows | `NgMusic-<version>-win-x86-setup.exe` |

The graphical installer is designed for a normal **Next → Next → Install** flow.

### What the wizard does

- Explains the prerequisites.
- Collects the Google OAuth Desktop Client ID.
- Can open the step-by-step OAuth guide.
- Installs the architecture-matching MSI silently after Windows UAC approval.
- Installs NgMusic under Program Files.
- Adds the NgMusic directory to system `PATH`.
- Saves the Client ID for the current user in `%LOCALAPPDATA%\NgMusic\config.json`.
- Optionally creates a Start-menu shortcut.
- Optionally creates a desktop shortcut.
- Optionally launches NgMusic when setup finishes.

The installer does not store Google passwords, OAuth access/refresh tokens, API keys, or signing secrets.

### Why UAC appears only during install

The setup wizard runs initially as the current user so the Client ID is saved to the correct Windows profile. It elevates only the MSI installation step that needs access to Program Files and the system `PATH`.

## Direct MSI installation

The MSI remains available for managed deployment or users who prefer Windows Installer directly.

It installs NgMusic under Program Files and adds it to the system `PATH`.

If you use the MSI directly, configure OAuth on first `login` through NgMusic's terminal wizard, or use the documented environment variables.

## Portable ZIP

Extract the portable ZIP anywhere and run `ngmusic.exe`.

Portable mode does not modify Program Files, shortcuts, or `PATH`. On first login, the terminal setup wizard can save the Google OAuth Client ID for the current user.

## Runtime requirements

Official releases are self-contained. No separate .NET installation is required.

## Uninstall

For Setup.exe or MSI installations, use **Settings → Apps → Installed apps → NgMusic → Uninstall**.

Portable installations are removed by deleting the extracted folder. Use `logout` first if you also want to remove NgMusic's OAuth token from Windows Credential Manager.
