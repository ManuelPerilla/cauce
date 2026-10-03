# Core verification

This console test suite uses only .NET 10 framework libraries. It does not need a test framework, NuGet packages, network access, music collection, or account.

From the repository root:

```powershell
dotnet restore tests/Cauce.Core.Tests/Cauce.Core.Tests.csproj --configfile tests/Cauce.Core.Tests/NuGet.Config
dotnet run --project tests/Cauce.Core.Tests/Cauce.Core.Tests.csproj --configuration Release --no-restore
```

The checks cover queue eligibility, genre normalization, repeats, artist spacing, persistent-data minimization, current file availability, safe corruption recovery, future-version protection, selective data deletion, import deduplication and metadata, and cancellation. Tests create and remove only their own unique temporary directories.

Exit codes: `0` means all checks passed; `1` means a test failed; `2` means Windows Application Control blocked loading the test library, so execution could not be verified. Compilation alone does not establish a passing test result.
