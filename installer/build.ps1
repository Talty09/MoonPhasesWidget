# Publishes the widget as a self-contained single exe and compiles the Inno Setup installer.
# Output: artifacts\MoonPhasesWidget-Setup-<version>.exe
# -Version overrides <Version> from the csproj (the release workflow passes the tag's version).
param([string]$Version)
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'src\MoonPhasesWidget\MoonPhasesWidget.csproj'
$publishDir = Join-Path $root 'artifacts\publish'

$iscc = @(
    (Get-Command ISCC.exe -ErrorAction SilentlyContinue)?.Source
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $iscc) {
    throw 'Inno Setup 6 not found. Install it with: winget install JRSoftware.InnoSetup'
}

$version = if ($Version) { $Version } else { (dotnet msbuild $project -getProperty:Version).Trim() }

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish $project -c Release -r win-x64 --self-contained `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$version -o $publishDir
if ($LASTEXITCODE) { throw "dotnet publish failed ($LASTEXITCODE)" }

& $iscc "/DAppVersion=$version" "/DPublishDir=$publishDir" (Join-Path $PSScriptRoot 'MoonPhasesWidget.iss')
if ($LASTEXITCODE) { throw "ISCC failed ($LASTEXITCODE)" }
