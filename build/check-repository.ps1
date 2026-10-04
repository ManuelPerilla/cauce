param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$errors = [Collections.Generic.List[string]]::new()

function Require-Path([string] $relativePath) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $relativePath))) {
        $errors.Add("Required repository path is missing: $relativePath")
    }
}

@(
    'README.md', 'README.es.md', 'README.fr.md', 'README.hi.md', 'README.zh-CN.md',
    'docs/README.md', 'docs/installation.md', 'docs/releasing.md', 'docs/cauce-accounts.md',
    'src/Cauce.Core/Cauce.Core.csproj', 'src/Cauce.Desktop/Cauce.Desktop.csproj',
    'tests/Cauce.Core.Tests/Cauce.Core.Tests.csproj',
    'tests/Cauce.Auth.Tests/Cauce.Auth.Tests.csproj',
    'tests/Cauce.Desktop.Smoke/Cauce.Desktop.Smoke.csproj',
    'build/common.ps1', 'build/dev.ps1', 'build/cauce.ps1', 'build/release.ps1',
    '.github/workflows/cauce.yml', '.github/workflows/release.yml'
) | ForEach-Object { Require-Path $_ }

[xml] $props = Get-Content -LiteralPath (Join-Path $repoRoot 'Directory.Build.props') -Raw
foreach ($property in @('Company', 'Product')) {
    $node = $props.SelectSingleNode("/Project/PropertyGroup/$property")
    if ($null -eq $node -or $node.InnerText -cne 'Cauce') {
        $errors.Add("Directory.Build.props must declare $property as Cauce.")
    }
}
$repository = $props.SelectSingleNode('/Project/PropertyGroup/RepositoryUrl')
if ($null -eq $repository -or $repository.InnerText -cne 'https://github.com/ManuelPerilla/cauce') {
    $errors.Add('The canonical repository URL must identify Cauce.')
}
. (Join-Path $PSScriptRoot 'common.ps1')
$version = Resolve-CauceVersion

foreach ($project in @(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'src') -Recurse -Filter '*.csproj' -File)) {
    if ($project.FullName -match '[\\/](bin|obj)[\\/]') { continue }
    [xml] $projectXml = Get-Content -LiteralPath $project.FullName -Raw
    foreach ($property in @('RootNamespace', 'AssemblyName')) {
        $node = $projectXml.SelectSingleNode("/Project/PropertyGroup/$property")
        if ($null -eq $node -or $node.InnerText -cnotmatch '^Cauce(?:\.|$)') {
            $errors.Add("$($project.Name) must use a Cauce $property.")
        }
    }
    foreach ($reference in @($projectXml.SelectNodes('/Project/ItemGroup/ProjectReference'))) {
        $target = Join-Path $project.DirectoryName $reference.GetAttribute('Include')
        if (-not (Test-Path -LiteralPath $target -PathType Leaf)) {
            $errors.Add("Broken project reference in $($project.Name): $target")
        }
    }
}

$markdownFiles = @(Get-ChildItem -LiteralPath $repoRoot -Recurse -Filter '*.md' -File | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj|artifacts|\.git)[\\/]'
})
$linkCount = 0
foreach ($file in $markdownFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ([string]::IsNullOrWhiteSpace($content)) {
        $errors.Add("Empty documentation: $($file.FullName)")
        continue
    }
    foreach ($match in [regex]::Matches($content, '\[[^\]\r\n]*\]\(([^)\r\n]+)\)')) {
        $destination = $match.Groups[1].Value.Trim()
        if ($destination.StartsWith('<')) {
            $destination = ($destination -split '>', 2)[0].Substring(1)
        } else {
            $destination = ($destination -split '\s+["'']', 2)[0]
        }
        if ($destination -match '^(?:[a-z][a-z0-9+.-]*:|#|/)') { continue }
        $destination = [Uri]::UnescapeDataString(($destination -split '[?#]', 2)[0])
        if (-not $destination) { continue }
        $linkCount++
        $target = Join-Path $file.DirectoryName $destination
        if (-not (Test-Path -LiteralPath $target)) {
            $relativeFile = [IO.Path]::GetRelativePath($repoRoot, $file.FullName)
            $errors.Add("Broken documentation link in ${relativeFile}: $destination")
        }
    }
}

if ($errors.Count -gt 0) {
    throw ("Repository checks failed:`n" + ($errors -join "`n"))
}
Write-Host "Cauce ${version}: canonical metadata, project references and $linkCount local links in $($markdownFiles.Count) documents verified."
