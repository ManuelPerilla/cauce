# Configuración y OAuth de Google

NgMusic utiliza OAuth 2.0 para aplicaciones Desktop y YouTube Data API v3.

## Variables

| Variable | Uso |
| --- | --- |
| `NGMUSIC_GOOGLE_CLIENT_ID` | Obligatoria para login |
| `NGMUSIC_GOOGLE_CLIENT_SECRET` | Solo si Google entrega uno |
| `NGMUSIC_YOUTUBE_API_KEY` | Opcional para búsquedas sin OAuth |

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="tu-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="tu-client-secret"
$env:NGMUSIC_YOUTUBE_API_KEY="tu-api-key"
```

## Flujo OAuth

1. NgMusic genera PKCE y un estado aleatorio.
2. Abre un listener temporal en `127.0.0.1`.
3. Abre Google en el navegador del sistema.
4. Google redirige al listener local.
5. NgMusic valida el estado y canjea el código.
6. El token se guarda en Windows Credential Manager como `NgMusic.GoogleOAuth`.

NgMusic nunca solicita ni almacena tu contraseña de Google.

`logout` elimina el token guardado de NgMusic.

Nunca subas secretos, claves API, certificados de firma ni tokens al repositorio.