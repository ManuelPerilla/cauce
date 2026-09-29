# Changelog

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
