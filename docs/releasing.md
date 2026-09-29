# Building and releasing

[Documentation home](../README.md) · [Architecture](architecture.md) · [Code signing policy](code-signing-policy.md)

## Requirements for maintainers

- Windows build environment or GitHub Actions Windows runner.
- .NET 10 SDK.
- Network access for package restore.
- WiX Toolset SDK used by the installer project.

End users do not need the SDK or .NET runtime.

## Local release build

```powershell
.\build\release.ps1 -Version 0.2.0
```

Outputs are written to `artifacts/release/`.

The build produces MSI and portable packages for x64, ARM64, and x86.

## Build characteristics

Release publishing uses:

- Release configuration;
- self-contained deployment;
- single-file publishing;
- ReadyToRun;
- no debug symbols in distributed builds;
- no trimming until trim compatibility is verified;
- isolated WiX intermediate directories per architecture.

The MSI and portable ZIP for a given architecture are generated from the same published application payload.

## GitHub Actions

The release workflow builds on a Windows runner, generates SHA-256 checksums, uploads an Actions artifact, and publishes release assets.

Build outputs must not be manually edited after the automated build.

## Versioning

NgMusic uses semantic-style versions: `MAJOR.MINOR.PATCH`.

Binary release files belong in GitHub Releases, not in source-control commits.

## Release checklist

1. Review source/build-script changes.
2. Confirm version metadata.
3. Build all architectures.
4. Confirm each architecture has distinct, expected artifacts.
5. Generate and publish SHA-256 checksums.
6. Run signing/approval when code signing is available.
7. Publish GitHub Release.
8. Verify download links and signatures/checksums.
9. Smoke-test x64 at minimum and architecture-specific builds where hardware/emulation is available.

## Code signing

Signing keys must never be committed to the repository.

When SignPath signing is active, signing requests require explicit human approval and must originate from verifiable automated builds. See [Code signing policy](code-signing-policy.md).
