# NgMusic

**Reproductor musical de terminal para Windows inspirado en PowerShell, con Google OAuth y reproducción de YouTube.**

## Paquetes

| Paquete | Experiencia |
| --- | --- |
| Setup.exe | Recomendado: asistente gráfico |
| MSI | Instalador nativo de Windows + configurador al terminar |
| Portable ZIP | Extraer y ejecutar |

El MSI interactivo ya deja el mismo estado funcional que Setup.exe: instala NgMusic, agrega `PATH` y abre un configurador para Client ID y accesos directos.

El MSI silencioso permanece silencioso para despliegues empresariales.

Después de Setup.exe o MSI interactivo:

```text
ngmusic
login
search "..."
play 1
```

[Instalación](docs/i18n/es/installation.md)
