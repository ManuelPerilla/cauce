# Building and releasing

[Documentation home](../README.md) · [Architecture](architecture.md) · [Code signing policy](code-signing-policy.md)

## Release outputs

For every selected Windows architecture, the release script produces:

- graphical `Setup.exe`;
- direct MSI installer;
- portable ZIP.

The graphical setup executable embeds the matching MSI, so each setup binary is architecture-specific.

## Build

```powershell
.\build\release.ps1 -Version 0.4.0
```

For each architecture the pipeline:

1. publishes the self-contained NgMusic application;
2. creates the portable ZIP;
3. builds an architecture-specific MSI;
4. builds an architecture-specific WinForms Setup.exe embedding that MSI;
5. generates SHA-256 checksums;
6. uploads the GitHub Actions package;
7. publishes GitHub Release assets.

## Release validation

The release must verify that:

- x64, ARM64, and x86 application payloads differ where expected;
- each MSI embeds the correct architecture;
- each Setup.exe embeds its matching MSI;
- the graphical setup can save the current user's OAuth Client ID;
- installation completes after UAC approval;
- `ngmusic` resolves from a newly opened terminal;
- uninstall works through Windows Settings.

## Signing

When code signing is active, sign the NgMusic application payload and installation artifacts in the approved pipeline. Setup executables and MSI packages should both receive Authenticode signatures before public release.
