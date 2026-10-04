# Cauce security policy

## Supported versions

Cauce is an early-stage desktop preview, currently `0.6.0-alpha.1`. Security fixes target the maintained development code and current preview. Alpha packages have no long-term support commitment.

## Reporting a vulnerability

Do not post credentials, authorization codes, access tokens, signing material or exploitable vulnerability details in a public issue. Contact the [repository maintainer](https://github.com/ManuelPerilla) privately using an available GitHub contact mechanism. A dedicated private advisory channel is not currently configured.

Include the affected version, Windows architecture, reproduction steps, security impact and any proposed mitigation. Remove secrets and personal paths from supporting material. Ordinary playback or interface bugs can go to [GitHub Issues](https://github.com/ManuelPerilla/cauce/issues).

## Release integrity

The maintained Windows workflows verify Cauce and produce portable ZIPs with SHA-256 checksums. The release workflow delivers a **draft** for maintainer review. A checksum detects a changed download when compared with a trusted published value; it is not a publisher signature.

Current preview packages are unsigned. Public signing needs a real certificate or approved signing service, reviewable build provenance and human approval under [the signing policy](docs/code-signing-policy.md). The repository has no `LICENSE` file, and approval for the SignPath Foundation route is not established.

Provider secrets, signing keys and private credentials must never enter source control, build logs or downloadable artifacts. Optional `auth.json` contains only public client configuration; placing secrets in it is unsupported.

## Privacy and trust boundaries

Local music playback needs no account. Library metadata stays under `%LOCALAPPDATA%\Cauce`; audio is referenced rather than copied. Optional sign-in opens the system browser and uses a broker over HTTPS. External music links open in their own service and do not become in-app streams.

See [security and privacy](docs/security.md), [account configuration](docs/cauce-accounts.md) and [architecture](docs/architecture.md) for the implemented boundaries.
