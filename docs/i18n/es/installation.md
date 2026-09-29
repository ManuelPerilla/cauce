# Instalación

NgMusic está pensado para Windows 10 y Windows 11 modernos.

## Elegir paquete

| Sistema | Paquete recomendado |
| --- | --- |
| Intel/AMD 64-bit | `NgMusic-<versión>-win-x64.msi` |
| Windows ARM | `NgMusic-<versión>-win-arm64.msi` |
| Windows 32-bit | `NgMusic-<versión>-win-x86.msi` |

## MSI

1. Descarga el MSI de tu arquitectura.
2. Opcionalmente verifica el hash con `SHA256SUMS.txt`.
3. Ejecuta el MSI.
4. Acepta la elevación de administrador si Windows la solicita.
5. Abre una terminal nueva.
6. Ejecuta `ngmusic`.

El MSI instala NgMusic en Program Files, registra upgrade/desinstalación y añade su carpeta al `PATH` del sistema. La versión actual no crea acceso directo en Inicio.

## Portable

Descarga el ZIP portable, extráelo y ejecuta `ngmusic.exe`. No modifica Program Files ni `PATH`.

Los tokens de autenticación siguen guardándose en Windows Credential Manager.

## Runtime

Las releases oficiales son self-contained. No necesitas instalar .NET para ejecutarlas. Para compilar desde código sí necesitas .NET 10 SDK.

Consulta también [Configuración](configuration.md).