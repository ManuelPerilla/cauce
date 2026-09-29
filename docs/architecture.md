# Architecture

```text
┌────────────────────────────────────┐
│            MusicShell              │
│ PowerShell-ish parser + queue UX   │
└───────────┬───────────────┬────────┘
            │               │
            ▼               ▼
┌─────────────────┐   ┌────────────────────┐
│ IMusicProvider  │   │      IPlayer       │
│ YouTube Data API│   │ YouTube IFrame API │
└────────┬────────┘   │ localhost bridge   │
         │            └────────────────────┘
         ▼
┌──────────────────────────┐
│ IGoogleAuthService       │
│ OAuth 2.0 + PKCE         │
│ refresh-token lifecycle  │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ Windows Credential Mgr   │
└──────────────────────────┘
```

The main seam is `IPlayer`. The current implementation uses a browser-hosted visible IFrame player; a future WebView2 implementation can replace it without changing shell commands or provider code.

## Distribution architecture

The application is published once per Windows architecture:

```text
source
  │
  ├── dotnet publish win-x64   ─┬─> x64 MSI
  │                             └─> x64 portable ZIP
  ├── dotnet publish win-arm64 ─┬─> ARM64 MSI
  │                             └─> ARM64 portable ZIP
  └── dotnet publish win-x86   ─┬─> x86 MSI
                                └─> x86 portable ZIP
```

The MSI and portable ZIP for a given architecture consume the **same publish directory**. This keeps runtime behavior identical regardless of whether NgMusic is installed or extracted.

### Release choices

- **Self-contained:** users do not install .NET separately.
- **Single-file:** minimizes loose runtime files and makes portable use simple.
- **ReadyToRun:** improves cold startup at the cost of some binary size.
- **No trimming yet:** avoids removing reflection/interop-reachable code before trim compatibility is verified.
- **Architecture-specific RIDs:** `win-x64`, `win-arm64`, and `win-x86` are built independently.

## State

Application binaries can be portable, but credentials should not be. OAuth tokens are stored in Windows Credential Manager for the current user. This prevents a copied portable folder from carrying reusable plaintext authentication tokens to another machine.
