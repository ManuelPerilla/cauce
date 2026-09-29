# Building and releasing

## Distribution architecture

For each RID (`win-x64`, `win-arm64`, `win-x86`) the release pipeline builds:

1. the self-contained NgMusic application;
2. the portable ZIP;
3. the self-contained post-install configurator;
4. the MSI containing NgMusic + configurator;
5. the graphical Setup.exe embedding that MSI.

This means Setup.exe and MSI use the same application payload rather than independently packaged copies.

## MSI configurator rules

Interactive first-time MSI installation schedules `NgMusicConfigurator.exe` after `InstallFinalize`.

The custom action:

- runs only for a first install;
- runs only when Windows Installer UI is present;
- runs in the installing user's context;
- is asynchronous so MSI completion is not blocked by the configurator;
- is suppressed when MSI is invoked by NgMusic Setup.exe;
- is suppressed for silent deployments.

## Build command

```powershell
.\build\release.ps1 -Version 0.5.0
```

## Release validation checklist

- Build x64, ARM64 and x86.
- Confirm Setup.exe, MSI and portable ZIP exist for every architecture.
- Confirm architecture hashes differ where expected.
- Confirm interactive MSI launches the post-install configurator.
- Confirm silent MSI does not launch UI.
- Confirm Setup.exe does not produce a duplicate configurator.
- Confirm Client ID is saved under the current user's LocalAppData.
- Confirm `ngmusic` resolves from a new terminal after installed editions.
- Confirm Windows uninstall succeeds.
- Generate SHA-256 checksums.
- Sign all executable/MSI artifacts when signing is available.
