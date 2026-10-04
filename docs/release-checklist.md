# Cauce 0.6.0 release candidate

[Documentation](README.md) · [Installation](installation.md) · [Releasing](releasing.md)

The current candidate is `0.6.0-rc.1`. Its stable scope is local Windows playback, reference libraries and deterministic genre sessions. Cloud synchronization and integrated streaming are outside this release. Social sign-in is optional and requires a configured broker.

## Automated evidence

The release workflow must pass Core, authentication and desktop smoke checks, build all six Windows packages and publish Cauce.Core. It also installs the x64 EXE and MSI on a disposable runner, opens the installed application, checks shortcuts, reinstalls/repairs, uninstalls and verifies preservation of a synthetic library. Inspect the installer-verification artifact and its logs.

These checks establish packaging and startup behavior only. They do not demonstrate audio playback, representative device performance or upgrades from older releases.

## Device validation before stable

Record the candidate tag, Windows version, architecture, installer format, result and relevant logs for each case. An unchecked item is pending, not passed.

- [ ] On a clean Windows x64 device, test EXE and MSI separately: install, launch, shortcut, uninstall.
- [ ] On a native Windows ARM64 device, repeat the installation checks and portable startup.
- [ ] Upgrade from a previous version of the same installer format; confirm user library and preferences remain usable.
- [ ] Play representative MP3, WAV and M4A files; verify pause/resume, seek, volume, next track and track completion.
- [ ] Test missing, moved, unsupported and damaged files; confirm actionable errors and an intact library.
- [ ] Test genre filtering, queue exhaustion, artist spacing and repeat behavior with a real library.
- [ ] Verify tray restore, compact mode, shortcut and repeated theme changes.
- [ ] Check export, restart and data reset using disposable metadata; audio must stay untouched.
- [ ] Record sustained/minimized playback and large-library behavior on representative hardware.

## Distribution decisions

- [ ] Choose and document the source and binary license. No license is selected by this candidate.
- [ ] Decide the signing route and validate signed output, or explicitly disclose unsigned distribution.
- [ ] Review release notes, download links and supported Windows versions against tested devices.

After evidence is reviewed and blocking issues are fixed, remove the version suffix in Directory.Build.props, update current-version documentation, publish `v0.6.0` and mark it Latest. Keep the candidate assets as historical releases.
