# NgMusic

**Controlador musical para Windows inspirado en PowerShell, centrado en terminal, con OAuth de Google y reproducción mediante el reproductor embebido compatible de YouTube.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **Estado del proyecto:** etapa temprana pero utilizable. Las versiones públicas se compilan automáticamente con GitHub Actions para Windows x64, ARM64 y x86.

## Qué es NgMusic

NgMusic es un shell musical personal para Windows, pensado para quienes prefieren teclado y terminal antes que una interfaz multimedia convencional. Ofrece comandos inspirados en PowerShell para buscar música, controlar reproducción, volumen, posición, cola y autenticación con Google.

La reproducción permanece dentro del reproductor IFrame oficial y visible de YouTube. NgMusic no extrae flujos multimedia, no descarga pistas y no crea una capa oculta de extracción de audio.

## Funciones

- OAuth 2.0 de Google con PKCE y redirección loopback.
- Tokens OAuth guardados en **Windows Credential Manager**.
- Búsqueda mediante YouTube Data API v3.
- Reproducción visible con YouTube IFrame controlada desde terminal.
- Cola e historial de reproducción.
- Distribuciones MSI y ZIP portable para x64, ARM64 y x86.
- Releases self-contained, sin requerir .NET en el equipo del usuario.
- Pipeline reproducible en GitHub Actions con hashes SHA-256.

## Descargar

Usa la [última Release de GitHub](https://github.com/ManuelPerilla/ngmusic/releases/latest).

| Arquitectura | Instalador | Portable |
| --- | --- | --- |
| x64 | `*-win-x64.msi` | `*-win-x64-portable.zip` |
| ARM64 | `*-win-arm64.msi` | `*-win-arm64-portable.zip` |
| x86 | `*-win-x86.msi` | `*-win-x86-portable.zip` |

El MSI instala NgMusic en Program Files y agrega `ngmusic` al `PATH` del sistema. Puede pedir permisos de administrador. La versión portable no modifica Program Files ni el `PATH`.

Consulta [Instalación](docs/i18n/es/installation.md).

## Configuración inicial

NgMusic necesita un cliente OAuth de Google de tipo Desktop y acceso a YouTube Data API v3.

```powershell
$env:NGMUSIC_GOOGLE_CLIENT_ID="tu-client-id"
$env:NGMUSIC_GOOGLE_CLIENT_SECRET="tu-client-secret"
$env:NGMUSIC_YOUTUBE_API_KEY="tu-api-key"
```

No subas credenciales al repositorio.

## Documentación

- [Instalación](docs/i18n/es/installation.md)
- [Configuración y OAuth](docs/i18n/es/configuration.md)
- [Referencia de comandos](docs/i18n/es/commands.md)
- [Arquitectura](docs/i18n/es/architecture.md)
- [Seguridad y privacidad](docs/i18n/es/security.md)
- [Solución de problemas](docs/i18n/es/troubleshooting.md)
- [Builds y releases](docs/i18n/es/releasing.md)
- [Política de firma de código](docs/i18n/es/code-signing-policy.md)

La versión inglesa es la fuente canónica. Si una traducción discrepa de ella, prevalece la documentación inglesa.

## Estado de firma

La aprobación de SignPath Foundation está pendiente. Cuando la firma esté habilitada para releases oficiales:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**
