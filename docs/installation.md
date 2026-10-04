# Install and run Cauce

[Documentation](README.md) · [Preferences](configuration.md) · [Troubleshooting](troubleshooting.md)

Cauce is a Windows WPF application. Portable packages target **x64** and **ARM64** and include their .NET runtime. Choose the package matching your Windows device. The release workflow also builds EXE setup and MSI installers for both architectures. The current `0.6.0-alpha.1` preview is unsigned.

## Portable package

1. Obtain the Cauce ZIP from a maintained workflow artifact or a reviewed [repository release](https://github.com/ManuelPerilla/cauce/releases), when available. Prereleases are public preview downloads.
2. Compare its SHA-256 hash with `SHA256SUMS.txt` from the same release. A hash is not a publisher signature.
3. Extract **the complete ZIP** to a folder you can read. Keep the runtime and other files beside the executable.
4. Open `Cauce.exe`, finish or dismiss the introduction, and use **Biblioteca → Añadir archivos** to choose local music.

No account, separate .NET runtime or administrator installation is required for local playback. This package does not install into Program Files, change `PATH`, create shortcuts automatically or register an updater.

Windows may warn about an unsigned download. Verify its source and checksum; do not disable Windows security controls to run it. Code signing remains pending under the [signing policy](code-signing-policy.md).

## EXE and MSI installers

1. Download the EXE setup or MSI matching your architecture from the [repository releases](https://github.com/ManuelPerilla/cauce/releases).
2. Verify its SHA-256 hash against the release's `SHA256SUMS.txt`.
3. Run the EXE setup wizard or open the MSI. Both require administrator permission, include the .NET runtime and create a Start menu shortcut.
4. Launch Cauce from the Start menu and import your local music.

Choose one installer format per device. EXE and MSI use separate directories under Program Files and separate uninstall entries. To uninstall either format, use Windows Settings → Apps → Installed apps. Uninstalling application binaries preserves your local library and audio files.

## NuGet library

GitHub Packages distributes **Cauce.Core** as a NuGet library for .NET 10 projects. It is a developer dependency, not the desktop application or a Windows installer. Desktop users should download a ZIP, EXE or MSI from Releases.

## Develop from source

Install the .NET 10 SDK on Windows. From the repository root:

```powershell
./build/dev.ps1
```

To produce an unsigned self-contained review package:

```powershell
./build/cauce.ps1 -Runtime win-x64
# Choose the matching target for an ARM64 device:
./build/cauce.ps1 -Runtime win-arm64
```

Build output lives under `artifacts/cauce`. See [release packaging](releasing.md) for versioned x64/ARM64 delivery.

## Files and accounts

Music stays in the location you imported. Moving or deleting it makes the saved reference unavailable; import its new path if you move it. The library and preferences live in `%LOCALAPPDATA%\Cauce\library.json`, independently of the extracted application folder.

Account buttons remain disabled until a distributor provides valid `auth.json` broker configuration beside `Cauce.exe`. Users do not need to register provider applications or paste secrets to listen to local files. See [accounts](cauce-accounts.md).

## Remove Cauce

Exit the player, including its notification-area icon, then delete the extracted application folder. To remove metadata and preferences first, use **Cuenta → Restablecer datos locales** and confirm. This removes Cauce data and recovery files without deleting audio. Export before resetting if you need to keep your references. Deleting the application folder alone leaves the local library available for a later copy of Cauce.
