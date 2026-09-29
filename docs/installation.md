# Installation and portable builds

NgMusic ships in two forms. Both contain the .NET runtime and do **not** require a separate .NET runtime installation on the target PC.

## Option 1: MSI installer

Choose the MSI matching the Windows architecture:

| Windows architecture | Package |
| --- | --- |
| x64 | `NgMusic-<version>-win-x64.msi` |
| ARM64 | `NgMusic-<version>-win-arm64.msi` |
| x86 (legacy) | `NgMusic-<version>-win-x86.msi` |

The MSI installs NgMusic under Program Files, creates a Start-menu entry, adds the install folder to `PATH`, and registers normal Windows uninstall/upgrade metadata.

After installing, open a **new** terminal and run:

```powershell
ngmusic
```

## Option 2: portable ZIP

Portable releases have the form:

```text
NgMusic-<version>-win-x64-portable.zip
NgMusic-<version>-win-arm64-portable.zip
NgMusic-<version>-win-x86-portable.zip
```

Extract the ZIP anywhere and launch `ngmusic.exe`. Portable mode does not write to Program Files, create shortcuts, or modify `PATH`.

Portable does **not** mean stateless: login tokens are deliberately kept in Windows Credential Manager so they are not left as plaintext next to the executable.

## Which architecture do I need?

Open **Settings → System → About → System type**. Most Windows PCs are x64. Snapdragon/Copilot+ PCs are commonly ARM64. x86 is kept only for older 32-bit Windows machines.

## Google / YouTube configuration

NgMusic requires a Google Cloud OAuth **Desktop app** client and YouTube Data API v3 access.

Set these environment variables before starting NgMusic:

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="your-desktop-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="your-desktop-client-secret" # only if your client has one

# Optional. OAuth can authorize search after login.
$env:NGMUSIC_YOUTUBE_API_KEY="your-api-key"
```

For a persistent per-user setting:

```powershell
[Environment]::SetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID", "your-desktop-client-id", "User")
[Environment]::SetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_SECRET", "your-desktop-client-secret", "User")
```

Do not commit these values to the repository.

## First run

```text
PS Music:\> login
PS Music:\> search "massive attack teardrop"
PS Music:\> play 1
```

Google login opens in the system browser. Playback uses a visible official YouTube IFrame player on localhost.
