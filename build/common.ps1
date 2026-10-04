# Shared version and staging rules for the Cauce development and packaging scripts.
Set-StrictMode -Version Latest

function Resolve-CauceVersion {
    param([string]$Version)
    if ([string]::IsNullOrWhiteSpace($Version)) {
        [xml]$properties = Get-Content -LiteralPath (Join-Path (Split-Path $PSScriptRoot -Parent) 'Directory.Build.props') -Raw
        $prefix = [string]$properties.SelectSingleNode('/Project/PropertyGroup/VersionPrefix').InnerText
        $suffixNode = $properties.SelectSingleNode('/Project/PropertyGroup/VersionSuffix')
        $suffix = if ($null -ne $suffixNode) { [string]$suffixNode.InnerText } else { '' }
        $Version = if ($suffix) { "$prefix-$suffix" } else { $prefix }
    }
    # SemVer 2.0: numeric prerelease identifiers cannot contain leading zeroes.
    $number = '(0|[1-9][0-9]*)'
    $identifier = '(0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)'
    $pattern = "\A$number\.$number\.$number(?:-$identifier(?:\.$identifier)*)?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z"
    if ($Version -cnotmatch $pattern) { throw 'Version must be a valid SemVer value, such as 0.6.0-rc.1.' }
    return $Version
}

function Remove-CauceStage {
    param([string]$Path, [string]$OutputRoot)
    if (-not (Test-Path -LiteralPath $Path)) { return }
    $resolvedStage = (Resolve-Path -LiteralPath $Path).Path
    $resolvedOutput = (Resolve-Path -LiteralPath $OutputRoot).Path.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolvedStage.StartsWith($resolvedOutput, [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path $resolvedStage -Leaf) -notmatch '^stage-win-(x64|arm64)-[a-f0-9]{32}$') {
        throw 'Refusing to clean an unexpected staging directory.'
    }
    Remove-Item -LiteralPath $resolvedStage -Recurse -Force
}
