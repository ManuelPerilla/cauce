# Contributing to Cauce

Cauce is a native Windows desktop preview. Keep contributions focused on dependable local playback, understandable listening rules, honest source availability and a small, intentional data footprint.

## Before opening a pull request

- Read the [architecture](docs/architecture.md) and preserve the boundary between `Cauce.Core` and the WPF desktop adapters.
- Describe the user-visible problem and resulting behavior. Update the affected guides and translations when behavior changes.
- Run the relevant [checks](docs/releasing.md#verification). A successful build does not establish that tests executed successfully.
- Keep generated binaries, local library files, personal account configuration, tokens, provider secrets, certificates and private signing material out of commits.
- Document the purpose and limits of any new persistent data or network activity.

External pull requests require maintainer review before merge. Changes to authentication, dependencies, release automation, storage boundaries and signing need particular care because they affect user data and release integrity.

## Development and verification

Use Windows with the .NET 10 SDK:

```powershell
./build/dev.ps1
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
```

Core checks use framework libraries and synthetic temporary data. Authentication checks use synthetic identities and loopback sockets, without provider credentials. Desktop smoke checks render the WPF interface using isolated data. Live account integration and real audio decoding need separate validation on representative Windows devices; record what was actually verified in the pull request.

## Documentation and licensing

English is the canonical documentation source. Translations should describe the same current application, with working links and equivalent limitations. Do not present unsigned packages as signed or disabled account buttons as working provider integration.

The repository does not currently contain a `LICENSE` file. The maintainer must choose a license before representing the project as licensed open-source software. Signing requests follow the explicit human approval requirement in the [code signing policy](docs/code-signing-policy.md).

## Reporting problems

Use [GitHub Issues](https://github.com/ManuelPerilla/cauce/issues) for ordinary bugs, with the app version, Windows architecture and reproduction steps. Review screenshots and exports for personal paths and URLs before sharing. Follow [SECURITY.md](SECURITY.md) for sensitive vulnerabilities.
