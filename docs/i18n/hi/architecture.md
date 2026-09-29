# Architecture

NgMusic shell, provider, authentication और player को interfaces से अलग रखता है।

- `MusicShell`: commands, parser, queue/history।
- `YouTubeProvider`: YouTube Data API v3।
- `GoogleOAuthService`: OAuth 2.0 + PKCE।
- `WindowsCredentialTokenStore`: token storage।
- `IPlayer`: playback abstraction।
- `YouTubeIframePlayer`: visible IFrame player और localhost bridge।

इस separation से future WebView2 player बिना shell redesign के जोड़ा जा सकता है।

x64, ARM64 और x86 स्वतंत्र रूप से build होते हैं और WiX intermediate directories भी architecture के अनुसार अलग हैं।