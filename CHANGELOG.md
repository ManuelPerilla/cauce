# Cauce changelog

## 0.6.0-alpha.1

Native Windows desktop development preview built with C#/.NET 10 and WPF.

- Local MP3, WAV and M4A playback using Windows media support.
- Reference-only import with MP3 ID3v1 metadata reading and library-only artist/genre edits.
- Deterministic genre sessions with repeat control, artist spacing and explanations when the queue cannot continue.
- Separate availability states for local files, missing files and saved HTTPS service links.
- Compact player, notification-area controls and an optional Alt+Shift+C restore shortcut.
- Five appearance themes, reduced motion, opaque surfaces, a repeatable introduction and optional tips.
- Bounded, schema-versioned metadata with atomic saves, corruption recovery, export and reset controls.
- User-reviewed GitHub bug-report drafts with no automatic upload.
- Optional OIDC broker client with system-browser authorization, PKCE/state/nonce and signed-identity validation. Social-provider buttons stay disabled without configuration.
- Core, authentication and rendered-interface checks, plus self-contained x64/ARM64 portable packaging and draft release delivery.

The preview is unsigned. Live provider registrations, an identity broker, cloud synchronization, integrated streaming catalogs and a signed installer are not included.
