# Compilación y publicaciones

Necesitas Windows y el SDK de .NET 10. Desde la raíz:

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet build src/Cauce.Desktop -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1 -Runtime win-x64
./build/cauce.ps1 -Runtime win-arm64
./build/release.ps1 -Version 0.6.0-rc.1
```

Las suites verifican biblioteca, cola, identidad y estados WPF. Las pruebas sintéticas no validan proveedores de cuentas reales ni garantizan rendimiento en otros equipos.

Los scripts preparan ZIPs portables con runtime y checksums. La vista previa es sin firma salvo que se use un certificado autorizado y se verifique la firma. No hay instalador ni paquete de Store.

`build/cauce.ps1` admite `-Version` y escribe en `artifacts/cauce/`; `build/release.ps1` usa x64 y ARM64 por defecto y reúne los ZIPs y `SHA256SUMS.txt` en `artifacts/release/`.

`.github/workflows/cauce.yml` comprueba Cauce y conserva artefactos de revisión. `release.yml`, iniciado manualmente o con una etiqueta de versión, prepara paquetes y crea una publicación **en borrador**. Revisa versión, arquitectura, checksums, resultados y notas antes de publicar. Las solicitudes de firma siguen la [política de aprobación](code-signing-policy.md).

[Inicio](../../../README.es.md) · [Guía en inglés](../../releasing.md)
