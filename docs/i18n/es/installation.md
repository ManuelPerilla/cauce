# Instalación de Cauce

La vista previa 0.6.0-alpha.1 se distribuye como ZIP portable para Windows x64 o ARM64. Está **sin firma** mientras se completan los requisitos externos de firma. Usa el paquete que corresponda a la arquitectura de tu equipo.

1. Descarga el ZIP de una publicación oficial y comprueba su SHA-256.
2. Extrae el paquete en una carpeta donde puedas escribir.
3. Abre `Cauce.exe`. El paquete incluye su runtime de .NET.
4. En **Biblioteca → Añadir archivos**, selecciona MP3, WAV o M4A. Completa su género y escucha una sesión desde **Escuchar**.

No requiere cuenta, permisos de administrador ni modificaciones de PATH. Los datos se guardan en `%LOCALAPPDATA%\Cauce`, independientemente de la carpeta del programa. Para quitar la aplicación, cierra Cauce y elimina su carpeta; **Borrar datos locales** elimina la biblioteca y sus archivos de recuperación sin borrar música.

Desde el código, necesitas Windows y el SDK de .NET 10:

```powershell
dotnet run --project src/Cauce.Desktop
```

La interfaz actual está en español; estas guías traducen las instrucciones, no la aplicación.

[Inicio](../../../README.es.md) · [Guía en inglés](../../installation.md)
