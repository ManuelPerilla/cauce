# Installation

[Documentation home](../README.md) · [Configuration](configuration.md) · [Troubleshooting](troubleshooting.md)

NgMusic targets modern Windows 10 and Windows 11 systems.

## Choose the correct package

| System type | Recommended package |
| --- | --- |
| Intel/AMD 64-bit | `NgMusic-<version>-win-x64.msi` |
| Windows on ARM | `NgMusic-<version>-win-arm64.msi` |
| 32-bit Windows | `NgMusic-<version>-win-x86.msi` |

Portable ZIP files use the same architecture names.

To check your architecture, open **Settings → System → About → System type**.

## MSI installation

1. Download the MSI matching your architecture from GitHub Releases.
2. Verify the SHA-256 digest if desired using `SHA256SUMS.txt`.
3. Run the MSI.
4. Approve administrator elevation if Windows requests it.
5. Open a **new** terminal after installation.
6. Run `ngmusic`.

The MSI installs NgMusic under Program Files, registers Windows Installer upgrade/uninstall metadata, and appends the NgMusic installation directory to the system `PATH`.

The current installer does **not** create a Start-menu shortcut.

### Uninstall

Use **Settings → Apps → Installed apps → NgMusic → Uninstall**, or uninstall the MSI through standard Windows Installer tools.

## Portable installation

1. Download the matching `*-portable.zip`.
2. Extract it to a folder you control.
3. Run `ngmusic.exe`.

Portable mode does not modify Program Files or `PATH`. Authentication tokens are still stored in Windows Credential Manager rather than inside the portable folder.

## Runtime requirements

Official releases are self-contained. A user running a release does not need to install the .NET runtime or SDK.

Developers building from source need the .NET 10 SDK.

## First run

After setting the Google OAuth configuration described in [Configuration](configuration.md):

```text
PS Music:\> login
PS Music:\> search "massive attack teardrop"
PS Music:\> play 1
```

The `login` command opens the system browser. Playback opens a visible YouTube player in the browser.
