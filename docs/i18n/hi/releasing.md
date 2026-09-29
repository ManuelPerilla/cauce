# Build और release

Maintainer को Windows environment, .NET 10 SDK, network access और WiX Toolset SDK चाहिए।

```powershell
.\build\release.ps1 -Version 0.2.0
```

यह x64, ARM64 और x86 के MSI और portable ZIP बनाता है।

Release self-contained, single-file और ReadyToRun है। Debug symbols हटे रहते हैं और trimming अभी disabled है।

Checklist: source review, version verify, सभी architectures build, artifacts compare, SHA-256 generate, signing, GitHub Release publish, links/signatures verify और smoke test।