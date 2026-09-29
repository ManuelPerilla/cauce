# Code signing policy

[Documentation home](../README.md) · [Security](security.md) · [Releasing](releasing.md)

This page defines the code-signing governance for official NgMusic Windows releases.

## Status

SignPath Foundation approval is currently pending.

When SignPath signing is active for NgMusic official releases:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

Unsigned historical releases must not be represented as signed.

## Scope

Only artifacts produced from the official `ManuelPerilla/ngmusic` repository and its maintained build scripts are eligible for project signing.

NgMusic must not use its signing identity to sign unrelated projects, third-party proprietary code, locally modified binaries with unverifiable provenance, or artifacts produced outside the approved release pipeline.

## Team roles

Current project governance:

- **Committer:** [ManuelPerilla](https://github.com/ManuelPerilla)
- **Reviewer:** [ManuelPerilla](https://github.com/ManuelPerilla)
- **Signing approver:** [ManuelPerilla](https://github.com/ManuelPerilla)

External pull requests must be reviewed by the maintainer before merge. Signing requests require an explicit human approval by the signing approver.

If the maintainer team grows, this document must be updated to list the applicable committers, reviewers, and approvers or link to maintained GitHub teams.

## Access requirements

Maintainers with repository or signing access are required to use multi-factor authentication on GitHub and the signing service.

Signing credentials/private keys must never be exported into source control, GitHub logs, build artifacts, or developer-accessible plaintext secrets.

## Build provenance

Official signed artifacts must:

1. originate from source in this repository;
2. be produced by the maintained automated Windows build pipeline;
3. preserve consistent product/version metadata;
4. pass the normal release build before signing;
5. be manually approved for signing;
6. be published through the official GitHub Release process.

Build scripts and CI configuration are security-sensitive and must receive the same review attention as application code.

## Privacy policy

See [Security and privacy](security.md).

NgMusic has no project-operated telemetry backend. Network transfers to Google/YouTube occur only when requested by user-facing functionality such as authentication, search, or playback.

Third-party Google/YouTube services remain governed by their respective privacy policies and terms.

## System changes

The MSI installs application files under Program Files, registers uninstall/upgrade information, and modifies the system `PATH` to expose the `ngmusic` command. The portable distribution does not make these installation changes.

These behaviors must remain documented on the download/installation page.

## Uninstallation

The MSI must support standard Windows uninstallation. Portable installations can be removed by deleting the extracted application directory; saved OAuth credentials can be removed with `logout` or through Windows Credential Manager.

## Incident handling

If a signed release is suspected of containing malicious or unintended code, the maintainer must:

1. stop or pause further signing/releases;
2. identify affected versions/artifacts;
3. preserve logs and build provenance;
4. investigate source, dependencies, build scripts, and signing approvals;
5. coordinate certificate/signature revocation with the signing provider when required;
6. publish a clear remediation notice for users.
