# Cauce code signing policy

[Documentation](README.md) · [Security](security.md) · [Releasing](releasing.md)

This policy governs signing of official Cauce Windows artifacts. The current `0.6.0-rc.1` preview is unsigned. No certificate is included, and SignPath Foundation approval/configuration is not established. The repository has no `LICENSE` file; the owner must choose an appropriate license before pursuing a Foundation route that requires one.

## Scope and roles

Only Cauce artifacts built from [ManuelPerilla/cauce](https://github.com/ManuelPerilla/cauce) source and the maintained pipeline are eligible for project signing. Its identity must not sign unrelated projects, third-party proprietary payloads, unverifiable locally modified binaries or artifacts outside the approved process.

Current governance:

- **Committer:** [ManuelPerilla](https://github.com/ManuelPerilla)
- **Reviewer:** [ManuelPerilla](https://github.com/ManuelPerilla)
- **Signing approver:** [ManuelPerilla](https://github.com/ManuelPerilla)

External pull requests require maintainer review before merge. **Signing requests require explicit human approval by the signing approver.** Update these roles if the team changes.

## Access and provenance

Maintainers with repository/signing access must use multi-factor authentication on GitHub and the signing service. Private keys and credentials must never enter source control, build logs, downloadable artifacts or developer-accessible plaintext files.

Official signed artifacts must:

1. originate from reviewed source in this repository;
2. use the maintained automated Windows build pipeline;
3. preserve consistent Cauce product/version metadata;
4. pass relevant checks and the release build;
5. receive human approval for signing;
6. have signatures and timestamps verified before publication through the official GitHub Release process.

Build scripts, CI permissions, dependencies and account configuration are security-sensitive. Checksums do not substitute for publisher signatures. Unsigned packages must stay clearly labeled unsigned.

## Packaging behavior and privacy

The distribution is a portable x64/ARM64 ZIP. It requires no administrator installation, Program Files changes or `PATH` updates and has no self-updater. Delete the extracted folder to remove the program; manage library metadata separately under `%LOCALAPPDATA%\Cauce` or reset it from **Cuenta**. Reset never removes original music.

Cauce has no telemetry backend or automatic report uploads. Local playback is independent of accounts. Optional broker sign-in and external links have their own boundaries, described in [security and privacy](security.md). Signing does not authorize extra data collection.

## Incident handling

If a signed artifact is suspected of containing malicious or unintended code, pause signing/publication, identify affected versions/hashes, preserve build/signing evidence, investigate source/dependencies/workflows/approvals, coordinate certificate or signature revocation when required, and publish remediation guidance.

This policy documents governance; it does not establish an active certificate, approved signing service or signed public release.
