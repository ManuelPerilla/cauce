# Builds y releases

## Requisitos de mantenedor

- Windows o runner Windows de GitHub Actions.
- .NET 10 SDK.
- Acceso de red para restore.
- WiX Toolset SDK.

## Build local

```powershell
.\build\release.ps1 -Version 0.2.0
```

Genera MSI y ZIP portable para x64, ARM64 y x86.

Las releases usan self-contained, single-file, ReadyToRun, sin símbolos debug y sin trimming por ahora.

## Checklist

1. Revisar código y scripts de build.
2. Verificar versión.
3. Compilar todas las arquitecturas.
4. Verificar que los artefactos por arquitectura sean distintos.
5. Generar SHA-256.
6. Firmar cuando el servicio esté disponible.
7. Publicar Release.
8. Verificar enlaces, firmas y hashes.
9. Hacer smoke test.

Los binarios se publican como assets de GitHub Releases, no se comitean al repositorio.