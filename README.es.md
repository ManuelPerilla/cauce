# Cauce

**Tu música, sin perder el hilo.**

Reproductor nativo para Windows con vidrio sutil, sesiones que respetan el género elegido y una biblioteca que diferencia archivos reproducibles de enlaces a servicios.

> Vista previa 0.6.0-alpha.1. La firma pública y los accesos sociales reales necesitan configuración externa. El reproductor anterior NgMusic se conserva durante la transición.

- Reproducción local de MP3, WAV y M4A mediante Windows.
- Reglas de género, separación de artistas y repeticiones, con motivos comprensibles cuando la cola se agota.
- Biblioteca de referencias sin copiar audios, edición de artista/género, enlaces HTTPS y disponibilidad explícita.
- Modo compacto y bandeja; cinco apariencias, alto contraste, movimiento y transparencia reducibles.
- Tutorial repetible, consejos opcionales, guía, reporte de errores revisable y controles de exportación/borrado.
- Cuenta opcional mediante un servicio OIDC preparado para Google, Apple, Facebook y Microsoft. No hay sincronización en la nube en esta versión.

Necesitas el SDK de .NET 10 en Windows para ejecutar desde el código:

```powershell
dotnet run --project src/Cauce.Desktop
```

[Guía, datos y arquitectura](docs/cauce.md) · [Configurar cuentas](docs/cauce-accounts.md) · [NgMusic anterior](docs/ngmusic-legacy.md)
