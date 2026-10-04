# Cauce

**Tu música, sin perder el hilo.**

Cauce es un reproductor nativo para Windows, escrito en C# con WPF y .NET 10. Su vista previa **0.6.0-alpha.1 está sin firma** y se prepara como ZIP portable para x64 y ARM64.

- Reproduce MP3, WAV y M4A locales mediante Windows; la compatibilidad depende del archivo y del códec.
- Mantiene el género elegido con reglas deterministas de repetición y separación de artistas. Explica cuándo la cola no puede continuar.
- Guarda referencias y metadatos sin copiar ni modificar audio: hasta 10.000 referencias y 16 MiB.
- Distingue archivos reproducibles, ausentes y enlaces HTTPS a servicios; esos enlaces se abren fuera del reproductor.
- Ofrece modo compacto, bandeja, cinco temas, movimiento y transparencia reducibles, tutorial y soporte con borradores revisables.

Puedes escuchar sin cuenta. El cliente OIDC opcional permanece deshabilitado hasta configurar un broker y registros reales para Google, Apple, Facebook y Microsoft. No incluye catálogos de streaming ni sincronización en la nube. La interfaz actual está en español.

Con Windows y el SDK de .NET 10:

```powershell
dotnet run --project src/Cauce.Desktop
```

[Instalación](docs/i18n/es/installation.md) · [Preferencias](docs/i18n/es/configuration.md) · [Controles](docs/i18n/es/commands.md) · [Problemas](docs/i18n/es/troubleshooting.md)

[Arquitectura](docs/i18n/es/architecture.md) · [Seguridad](docs/i18n/es/security.md) · [Publicaciones](docs/i18n/es/releasing.md) · [Firma](docs/i18n/es/code-signing-policy.md)

[Guía completa](docs/cauce.md) · [Cuentas](docs/cauce-accounts.md) · [Código](https://github.com/ManuelPerilla/cauce)

[English](README.md) · Español · [Français](README.fr.md) · [हिन्दी](README.hi.md) · [简体中文](README.zh-CN.md)
