# Builds et releases

Le mainteneur a besoin d'un environnement Windows, du SDK .NET 10, d'un accès réseau et du WiX Toolset SDK.

```powershell
.\build\release.ps1 -Version 0.2.0
```

Cela produit MSI et ZIP portable pour x64, ARM64 et x86.

Les releases utilisent self-contained, single-file et ReadyToRun, sans symboles debug et sans trimming pour le moment.

Checklist : revue du code, version, builds de toutes architectures, contrôle des artefacts, SHA-256, signature, publication GitHub Release, vérification des liens/signatures et smoke test.