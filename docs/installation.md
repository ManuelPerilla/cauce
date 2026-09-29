# Installation and package behavior

[Documentation home](../README.md) · [Configuration](configuration.md) · [Troubleshooting](troubleshooting.md)

NgMusic supports Windows 10/11 on x64, ARM64, and legacy x86.

## Package matrix

| Format | Installs application | PATH | OAuth setup | Shortcuts |
| --- | --- | --- | --- | --- |
| Setup.exe | Program Files | System | In graphical wizard | Optional Start/Desktop |
| Interactive MSI | Program Files | System | Configurator after MSI finishes | Optional Start/Desktop |
| Silent MSI | Program Files | System | No UI; admin/in-app setup later | None automatically |
| Portable ZIP | Extracted folder | No | First `login` if needed | None automatically |

## Recommended: Setup.exe

Download the architecture-matching `*-setup.exe`.

The wizard:

1. explains requirements;
2. asks for the non-secret Google OAuth Desktop Client ID;
3. offers shortcut options;
4. requests UAC only for the Program Files/MSI installation;
5. saves OAuth configuration to the current user's profile;
6. optionally launches NgMusic.

No separate .NET runtime is required.

## Direct MSI

The MSI is a fully supported installation path, not a reduced package.

It installs:

- `ngmusic.exe`;
- `NgMusicConfigurator.exe`;
- Windows Installer upgrade/uninstall metadata;
- system `PATH` registration.

### Interactive MSI behavior

When the MSI is launched normally with Windows Installer UI, NgMusic opens its post-install configurator after a successful first installation.

The configurator lets the current user set the same OAuth Client ID and shortcut preferences exposed by Setup.exe.

### Silent/managed MSI behavior

When deployed silently, for example:

```powershell
msiexec /i NgMusic-0.5.0-win-x64.msi /qn /norestart
```

the MSI does **not** open post-install UI.

This is intentional for GPO, Intune, SCCM, scripted, or other managed deployment systems.

After silent deployment, configuration can be supplied through environment variables or completed with:

```text
ngmusic
setup
```

## Setup.exe vs MSI

Setup.exe wraps the same architecture-specific MSI. It passes an internal marker to prevent the MSI from opening a second configurator, because Setup.exe already completed that step.

This keeps one application payload and one Windows Installer package per architecture while providing two installer experiences.

## Portable

Portable builds remain self-contained and require no installation.

OAuth setup still works through the in-app wizard.

## Uninstall

Installed editions uninstall through **Settings → Apps → Installed apps → NgMusic**.

OAuth tokens are user credentials and are intentionally not deleted automatically by a machine-wide uninstall. Run `logout` if you want to remove the saved OAuth token first.
