# NgMusic

**Controlador musical para Windows inspirado en PowerShell, centrado en terminal, con OAuth de Google y reproducción mediante el reproductor embebido compatible de YouTube.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **Estado del proyecto:** etapa temprana pero utilizable.

## Configuración sin variables de entorno

Desde la versión 0.3, lo normal es simplemente ejecutar:

```text
PS Music:\> login
```

Si falta configuración, NgMusic abre automáticamente el asistente:

```text
Google OAuth setup
------------------
 [1] Paste Google OAuth Client ID
 [2] Open step-by-step setup instructions
 [3] Cancel
```

Pega una vez tu Google OAuth Client ID de tipo **Desktop app**. NgMusic guarda únicamente ese identificador no secreto en:

```text
%LOCALAPPDATA%\NgMusic\config.json
```

Los tokens OAuth siguen guardándose en **Windows Credential Manager**.

También puedes usar:

```text
setup
config show
config path
config reset
```

Las variables de entorno siguen disponibles para configuración avanzada y tienen prioridad sobre el archivo local.

## Funciones

- OAuth 2.0 de Google con PKCE.
- Asistente interactivo de configuración.
- Tokens OAuth en Windows Credential Manager.
- Búsqueda mediante YouTube Data API v3.
- Player IFrame visible controlado desde terminal.
- Cola e historial.
- MSI y portable para x64, ARM64 y x86.
- Builds reproducibles con SHA-256.

## Descargar

[Última Release](https://github.com/ManuelPerilla/ngmusic/releases/latest)

## Documentación

- [Instalación](docs/i18n/es/installation.md)
- [Configuración y OAuth](docs/i18n/es/configuration.md)
- [Comandos](docs/i18n/es/commands.md)
- [Arquitectura](docs/i18n/es/architecture.md)
- [Seguridad](docs/i18n/es/security.md)
- [Troubleshooting](docs/i18n/es/troubleshooting.md)
- [Releases](docs/i18n/es/releasing.md)
- [Firma de código](docs/i18n/es/code-signing-policy.md)

La documentación inglesa es canónica.
