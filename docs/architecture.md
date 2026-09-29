# Architecture

NgMusic separates runtime behavior from distribution.

## Runtime

```text
MusicShell
 ├─ GoogleOAuthService
 ├─ YouTubeProvider
 ├─ WindowsCredentialTokenStore
 └─ IPlayer → YouTubeIframePlayer
```

OAuth tokens live in Windows Credential Manager. The non-secret Client ID may live in LocalAppData configuration.

## Distribution

```text
NgMusic application payload
        │
        ├──── portable ZIP
        │
        └──── MSI
              ├─ ngmusic.exe
              ├─ NgMusicConfigurator.exe
              └─ PATH / uninstall metadata
                    │
                    └──── embedded by Setup.exe
```

The graphical Setup.exe and direct MSI therefore share the same MSI/application payload.

### Setup.exe path

Setup.exe collects current-user configuration first, then elevates only the silent MSI installation. It marks the MSI invocation so the MSI does not open its own post-install configurator.

### Interactive MSI path

A direct interactive MSI installs the product first and then launches `NgMusicConfigurator.exe` in the installing user's context.

### Silent MSI path

No UI is launched. This preserves predictable behavior for enterprise deployment systems.
