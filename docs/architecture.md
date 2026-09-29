# Architecture

[Documentation home](../README.md) · [Security](security.md)

NgMusic is intentionally split into replaceable layers so the terminal UX is not tightly coupled to one playback implementation.

```text
┌────────────────────────────────────┐
│            MusicShell              │
│ parser · commands · queue/history  │
└───────────┬───────────────┬────────┘
            │               │
            ▼               ▼
┌─────────────────┐   ┌────────────────────┐
│ IMusicProvider  │   │      IPlayer       │
│ YouTube Data API│   │ YouTube IFrame API │
└────────┬────────┘   │ localhost bridge   │
         │            └────────────────────┘
         ▼
┌──────────────────────────┐
│ IGoogleAuthService       │
│ OAuth 2.0 + PKCE         │
│ token refresh lifecycle  │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ Windows Credential Mgr   │
└──────────────────────────┘
```

## Shell

`MusicShell` owns user interaction, parsing, queue/history behavior, and presentation. It depends on interfaces rather than concrete provider/player implementations.

## Provider

`YouTubeProvider` uses YouTube Data API v3 for search. It can authenticate requests with an OAuth access token and optionally use an API key for unauthenticated search.

## Authentication

`GoogleOAuthService` implements the installed-app authorization-code flow with PKCE. The redirect listener binds only to loopback on an ephemeral port.

`WindowsCredentialTokenStore` persists OAuth token data in Windows Credential Manager.

## Playback

`YouTubeIframePlayer` hosts a small local HTTP bridge and launches a visible browser page containing the official YouTube IFrame player.

Terminal commands are translated into small local player commands such as load, pause, play, stop, seek, and volume.

The current `IPlayer` abstraction makes a future WebView2 player possible without redesigning the shell.

## Distribution

For each architecture, the same published payload is used by both MSI and portable packaging:

```text
source
  ├─ win-x64   ─┬─ MSI
  │             └─ portable ZIP
  ├─ win-arm64 ─┬─ MSI
  │             └─ portable ZIP
  └─ win-x86   ─┬─ MSI
                └─ portable ZIP
```

Release publishing is self-contained, single-file, and ReadyToRun. Trimming remains disabled until all runtime/reflection paths are demonstrated to be trim-safe.

WiX build intermediates are isolated per architecture to prevent cross-architecture package reuse.
