# Security policy

## Supported versions

NgMusic is an early-stage project. Security fixes are applied to the current development branch and newest published release when practical.

## Reporting a vulnerability

Please do not publish credentials, access tokens, signing material, or exploitable vulnerability details in a public issue.

For security-sensitive reports, contact the repository owner privately through an appropriate GitHub contact mechanism. A dedicated private advisory workflow may be added later.

Include, when possible:

- affected version;
- Windows architecture;
- reproduction steps;
- security impact;
- logs with secrets removed;
- suggested mitigation, if known.

## Supply-chain security

Official release artifacts are built through the repository's GitHub Actions workflow and accompanied by SHA-256 checksums.

Code signing is governed by [docs/code-signing-policy.md](docs/code-signing-policy.md). SignPath Foundation approval is currently pending.

Signing keys, OAuth credentials, and other private material must never be stored in the repository or release artifacts.

## Privacy

See [docs/security.md](docs/security.md) for NgMusic's privacy and data-handling model.
