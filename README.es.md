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


## Client Secret de Google

Algunos clientes OAuth de Google exigen también un **Client Secret** durante el intercambio del token.

Desde NgMusic 0.5.2 puedes configurarlo con `setup`, Setup.exe o el configurador del MSI. El secret se guarda en **Windows Credential Manager**, nunca en `config.json`.
