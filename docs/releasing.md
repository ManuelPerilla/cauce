# Building releases

The release pipeline creates self-contained Windows packages for `win-x64`, `win-arm64`, and `win-x86`.

## Requirements for maintainers

- Windows 10/11 build machine or GitHub Actions Windows runner.
- .NET 10 SDK.
- Internet access during restore so NuGet can obtain the WiX SDK.

Users installing a release do **not** need the .NET SDK or runtime.

## Build all release formats

From PowerShell at the repository root:

```powershell
.\build\release.ps1 -Version 0.2.0
```

Outputs appear under `artifacts/release/`:

```text
NgMusic-0.2.0-win-x64.msi
NgMusic-0.2.0-win-x64-portable.zip
NgMusic-0.2.0-win-arm64.msi
NgMusic-0.2.0-win-arm64-portable.zip
NgMusic-0.2.0-win-x86.msi
NgMusic-0.2.0-win-x86-portable.zip
```

To build only portable packages:

```powershell
.\build\release.ps1 -Version 0.2.0 -SkipInstaller
```

To target one architecture:

```powershell
.\build\release.ps1 -Version 0.2.0 -Architectures x64
```

## Release optimization policy

Release publishing enables:

- self-contained deployment so the target PC does not need .NET installed;
- single-file output;
- ReadyToRun for faster startup;
- Release compilation with debug symbols removed;
- architecture-specific output for x64, ARM64, and x86.

`PublishTrimmed` is intentionally disabled. Trimming is useful only after the entire dependency graph has been validated for trim compatibility; enabling it blindly can remove code reached through reflection or interop.

The installed and portable packages use the same published application payload. That prevents the two distribution modes from drifting apart.

## Versioning

Use semantic versions (`MAJOR.MINOR.PATCH`). Keep release work grouped into meaningful commits rather than generating a commit per build artifact. Binary release files should be attached to GitHub Releases, not committed to Git.

## Signing

The project is ready to add Authenticode signing later. Signing should happen after build and before publishing release artifacts. Never place signing certificates or private keys in the repository.
