# NgMusic

**Controlador musical para Windows inspirado en PowerShell y centrado en terminal.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

## Instalación recomendada: Setup.exe

La opción recomendada ahora es el instalador gráfico:

```text
NgMusic-<versión>-win-x64-setup.exe
NgMusic-<versión>-win-arm64-setup.exe
NgMusic-<versión>-win-x86-setup.exe
```

El asistente hace el flujo típico **Siguiente → Siguiente → Instalar** y deja preparado:

- NgMusic en Program Files.
- `ngmusic` en el `PATH`.
- Google OAuth Desktop Client ID.
- Acceso directo en Inicio opcional.
- Acceso directo en escritorio opcional.
- Lanzar NgMusic al terminar.

El Client ID no es secreto y se guarda en:

`%LOCALAPPDATA%\NgMusic\config.json`

Los tokens OAuth siguen en Windows Credential Manager.

Después de instalar, la idea es:

```text
ngmusic
login
search "..."
play 1
```

MSI directo y ZIP portable siguen disponibles como opciones alternativas.

[Documentación de instalación](docs/i18n/es/installation.md)
