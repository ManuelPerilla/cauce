# Arquitectura

NgMusic separa la experiencia de terminal, proveedor musical, autenticación y reproductor mediante interfaces.

- `MusicShell`: comandos, parser, cola e historial.
- `IMusicProvider`: contrato para búsquedas musicales.
- `YouTubeProvider`: usa YouTube Data API v3.
- `IGoogleAuthService`: autenticación OAuth 2.0 + PKCE.
- `WindowsCredentialTokenStore`: guarda tokens en Credential Manager.
- `IPlayer`: contrato de reproducción.
- `YouTubeIframePlayer`: player visible con IFrame API y puente localhost.

La separación permite sustituir el reproductor actual por WebView2 sin rediseñar el shell.

Cada arquitectura (x64, ARM64, x86) genera un payload independiente y de ahí salen MSI y ZIP portable. Los intermediates de WiX están aislados por arquitectura para impedir reutilización cruzada accidental.