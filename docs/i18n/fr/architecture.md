# Architecture

NgMusic sépare le shell, le fournisseur musical, l'authentification et le lecteur au moyen d'interfaces.

- `MusicShell` : commandes, parsing, queue/historique.
- `YouTubeProvider` : YouTube Data API v3.
- `GoogleOAuthService` : OAuth 2.0 + PKCE.
- `WindowsCredentialTokenStore` : stockage dans Credential Manager.
- `IPlayer` : abstraction de lecture.
- `YouTubeIframePlayer` : lecteur IFrame visible et bridge localhost.

Cette séparation permet de remplacer le lecteur par WebView2 sans refaire le shell.

x64, ARM64 et x86 sont compilés séparément, y compris les répertoires intermédiaires WiX.