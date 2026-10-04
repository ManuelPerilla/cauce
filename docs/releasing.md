# Building and releasing Cauce

[Documentation](README.md) · [Installation](installation.md) · [Signing policy](code-signing-policy.md)

Cauce targets native Windows with .NET 10 and WPF. Maintained packaging produces self-contained **x64** and **ARM64** portable ZIPs, without an MSI, setup wizard or x86 distribution. `Directory.Build.props` is the version source; the current preview is `0.6.0-alpha.1`.

## Development and review packages

Use Windows with the .NET 10 SDK. From the repository root:

```powershell
./build/dev.ps1
./build/cauce.ps1 -Runtime win-x64
./build/cauce.ps1 -Runtime win-arm64
```

The development script launches `src/Cauce.Desktop`. The review script publishes it with its runtime into isolated staging under `artifacts/cauce`, writes package-content checksums and creates `Cauce-<version>-<runtime>-unsigned.zip`. It cleans only its verified staging folder. Keep the full payload together.

## Verification

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet build src/Cauce.Desktop -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
```

Core checks cover queues, import, storage limits, recovery and selective reset. Authentication checks use synthetic tokens and loopback requests; they do not establish live provider integration. Desktop smoke checks use isolated synthetic references, render WPF views/themes, inspect bindings and verify preferences. Inspect its screenshots during visual review.

Verify real file playback on representative Windows devices and codecs. Measure CPU/memory during startup, large-library use, sustained/minimized playback and repeated theme/view changes before performance claims. Test live accounts only after real broker/provider registration. If Windows Application Control blocks an executable, report the blocked execution and preserve the control; compilation alone is not a passing test.

## Versioned portable release

```powershell
./build/release.ps1 -Version 0.6.0-alpha.1 -Architectures x64,arm64
```

The release script packages each architecture under `artifacts/release` and emits SHA-256 download checksums. Its version defaults to the shared project version when omitted. ZIP names and binary product/version metadata must agree. Confirm both packages contain `Cauce.exe`, dependencies and runtime. They remain unsigned unless signed under the policy.

## Workflows

- **`.github/workflows/cauce.yml`** verifies Cauce on Windows and uploads review artifacts. Pull-request checks do not publish releases.
- **`.github/workflows/release.yml`** accepts manual release preparation or a version tag, builds x64/ARM64 packages and creates a **draft GitHub Release**. Review commit, checks, version, architecture payloads, checksums and notes before publishing the draft.

Keep permissions minimal, with release upload access limited to delivery. Do not put provider secrets, personal `auth.json`, library exports or signing keys into logs or packages. Release links use [source](https://github.com/ManuelPerilla/cauce), [issues](https://github.com/ManuelPerilla/cauce/issues) and [releases](https://github.com/ManuelPerilla/cauce/releases).

## Signing and public-release prerequisites

`build/cauce.ps1` supports `-SigningThumbprint` for an installed code-signing certificate and `-SignTool` for the tool path. It signs Cauce binaries, timestamps and verifies them before labeling a review ZIP `signed`. No certificate is provided. Approval and provenance requirements are defined in [code-signing-policy.md](code-signing-policy.md).

The preview is unsigned; workflow success does not establish publisher trust. A license has not been chosen, the SignPath Foundation route is not approved/configured, and Store/MSIX packaging is future work. Do not label a ZIP as a signed installer or Store package. Release notes must retain optional broker sign-in, external-link handling and the absence of cloud sync/integrated streaming.
