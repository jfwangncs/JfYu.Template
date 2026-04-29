#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

$DotnetRoot = $PSScriptRoot
$RepoRoot = Resolve-Path (Join-Path $DotnetRoot "..\..")
$PackCsproj = Join-Path $DotnetRoot "JfYu.WebApi.Template.csproj"
$ArtifactsDir = Join-Path $RepoRoot "artifacts"

if (-not (Test-Path $PackCsproj)) {
  throw "Pack project not found: $PackCsproj"
}

New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null

Write-Host "[1/3] Uninstall existing template (ignore if not installed)..." -ForegroundColor Cyan
$previousErrorAction = $ErrorActionPreference
$ErrorActionPreference = "Continue"
dotnet new uninstall JfYu.WebApi.Template 2>&1 | Out-Null
$ErrorActionPreference = $previousErrorAction
$global:LASTEXITCODE = 0

Write-Host "[2/3] Packing template..." -ForegroundColor Cyan
dotnet pack $PackCsproj -o $ArtifactsDir --nologo
if ($LASTEXITCODE -ne 0) {
  throw "dotnet pack failed."
}

$nupkg = Get-ChildItem -Path $ArtifactsDir -Filter "JfYu.WebApi.Template.*.nupkg" |
  Sort-Object LastWriteTime -Descending |
  Select-Object -First 1

if (-not $nupkg) {
  throw "No nupkg generated under: $ArtifactsDir"
}

Write-Host "[3/3] Installing $($nupkg.Name)..." -ForegroundColor Cyan
dotnet new install $nupkg.FullName
if ($LASTEXITCODE -ne 0) {
  throw "dotnet new install failed."
}

Write-Host "Template updated successfully." -ForegroundColor Green
Write-Host "Package: $($nupkg.FullName)"

$sampleOutput = Join-Path $env:TEMP "JfYuWebApi-Demo"
$createCommand = "dotnet new JfYuWebApi -n MyApi -o `"$sampleOutput`" --JwtOption Basic --EnableRBAC true --EnableWeChat true --EnableTelemetry true --EnableUnitTest true"

Write-Host ""
Write-Host "Copy and run this command to generate a project (includes all optional parameters):" -ForegroundColor Yellow
Write-Host $createCommand -ForegroundColor Yellow
Write-Host "JwtOption values: None | Basic | Redis" -ForegroundColor DarkYellow
