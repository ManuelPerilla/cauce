# Changelog

## 0.5.3

- Search now returns only videos YouTube marks as embeddable and syndicated.
- Added explicit strict-origin-when-cross-origin Referrer-Policy for the embedded player.
- Added terminal diagnostics for YouTube IFrame player errors 2, 5, 100, 101/150 and 153.
- Added terminal notification when browser autoplay policy blocks scripted playback.
- Explicitly enables the JavaScript IFrame API and preserves the local origin.

## 0.5.2

- Added secure Google OAuth Client Secret support.
- Client Secret is stored in Windows Credential Manager.
- Added optional Client Secret fields to Setup.exe and MSI configurator.
- Terminal setup now accepts hidden Client Secret input.
- Missing-client-secret OAuth errors now include an actionable hint.
- Documented recovery for `client_secret is missing`.

## 0.5.1

- Fixed misleading OAuth browser success page shown before token exchange completed.
- Added detailed Google OAuth error reporting instead of generic HTTP 400 messages.
- Made OpenID profile lookup non-fatal after a successful token exchange.
- Improved OAuth troubleshooting documentation.


## 0.5.0

- Aligned interactive MSI installation with the graphical Setup.exe flow.
- Added a shared post-install configurator for Google OAuth Client ID and shortcuts.
- Interactive MSI installs now launch the configurator after installation.
- Silent/managed MSI installs remain fully silent.
- Setup.exe suppresses the MSI configurator because it already performs the same configuration.
- Kept MSI, graphical Setup.exe, and portable ZIP as supported distribution channels.
- Expanded installation/distribution documentation.

## 0.4.0

- Added a graphical Windows Setup.exe with a Next → Next → Install wizard.
- Setup collects the Google OAuth Desktop Client ID before NgMusic first runs.
- Setup installs the matching architecture-specific MSI after UAC approval.
- Setup can create Start-menu and desktop shortcuts.
- Setup can launch NgMusic when installation finishes.
- Direct MSI and portable ZIP distributions remain available.
- Updated multilingual installation documentation.

## 0.3.0

- Added interactive Google OAuth setup on first `login`.
- Added `setup` / `configure` commands.
- Added `config show`, `config path`, and `config reset`.
- Added local non-secret Client ID storage under `%LOCALAPPDATA%\NgMusic\config.json`.
- Kept OAuth access/refresh tokens in Windows Credential Manager.
- Kept environment-variable configuration as a higher-priority override.
- Updated multilingual documentation for the new setup flow.

## 0.2.0

- First packaged Windows release.
- Added x64, ARM64, and x86 MSI installers and portable builds.
- Added automated GitHub Actions release pipeline and SHA-256 checksums.
