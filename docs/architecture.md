# Cauce architecture

[Documentation](README.md) · [Detailed guide](cauce.md) · [Accounts](cauce-accounts.md)

Cauce separates durable metadata and listening rules from the native Windows interface. The desktop targets `net10.0-windows` with WPF; the core has no UI, network or account dependency.

## Runtime responsibilities

```text
Cauce.Desktop (WPF)
 ├─ MainViewModel → Cauce.Core
 │                  ├─ Track / SessionRules / AppPreferences
 │                  ├─ TrackImporter
 │                  ├─ QueuePlanner
 │                  └─ LibraryStore
 ├─ NativeAudioPlayer → Windows MediaPlayer
 ├─ ThemeManager / ButtonMotion
 ├─ TrayService → notification area and restore shortcut
 └─ AuthenticationService → system browser and optional HTTPS OIDC broker
```

The view model coordinates actions and owns one native audio decoder. Playback calls and callbacks stay on the UI dispatcher. Import runs away from it and reads only a fixed MP3 ID3v1 footer when available. Music playback does not host a browser process.

## Selection rules

`QueuePlanner` considers available local files only. It normalizes genre names for case, accents and spacing, applies the genre, excludes session repeats when disabled and enforces artist spacing. It chooses unheard candidates first, then the least recently selected; library order breaks ties.

An empty candidate set returns a reason rather than relaxed rules. Changing genre clears session history. Missing artist metadata cannot establish that tracks share an artist, so the reason encourages completing it. Service URLs are never audio candidates.

## Persistence and resource limits

`LibraryStore` uses schema-versioned JSON under `%LOCALAPPDATA%\Cauce`. It validates fields, limits data to 10,000 references and 16 MiB, serializes writes and replaces the file atomically. Availability is recomputed from the filesystem. Newer schemas remain untouched; unreadable data can be preserved in up to three recovery files.

Only paths, URLs, small text metadata and preferences are durable. Audio, artwork, listening history and credentials are not stored. Reset removes only the store's own metadata, recovery copies and recognized temporary files. Original audio and unrelated files stay intact.

Library rows are virtualized. Progress updates run once per second during playback, and preference saves are debounced. Glass-inspired surfaces use static resources and brief interaction animations, disabled by reduced-motion settings. There are no looping visualizers or background catalog downloads. Representative CPU/memory measurements are still necessary before performance claims.

## Identity boundary

The optional adapter uses the system browser, PKCE/state/nonce and signed-token validation. Secrets belong at the broker. Its callback listener accepts bounded requests on IPv4 loopback during an attempt. Sign-out clears in-memory identity and cancels pending sign-in; it does not clear browser cookies. See [accounts](cauce-accounts.md).

## Distribution and verification

`build/dev.ps1` runs the desktop. `build/cauce.ps1` produces a self-contained review ZIP. `build/release.ps1` creates versioned x64/ARM64 ZIPs and checksums. Windows workflows verify core rules, authentication boundaries and rendered WPF states; delivery prepares a draft release for review.

The preview has no MSI, setup wizard, self-updater, embedded service player or cloud sync. Signing requires a real certificate and the project's policy. See [building and releasing](releasing.md).
