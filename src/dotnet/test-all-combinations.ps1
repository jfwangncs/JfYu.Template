<#
.SYNOPSIS
    Test all valid symbol combinations for JfYuWebApi dotnet template.
    For each combination: dotnet new (generate) + dotnet build.

.USAGE
    .\test-all-combinations.ps1
    .\test-all-combinations.ps1 -KeepFailures   # keep failed output dirs for inspection
    .\test-all-combinations.ps1 -KeepAll        # keep all output dirs
#>
param(
    [switch]$KeepFailures,
    [switch]$KeepAll
)

$ErrorActionPreference = "Stop"
$templateRoot = Join-Path $PSScriptRoot "content"
$tempRoot     = Join-Path $PSScriptRoot "_test_combinations"

# ── 1. Install/reinstall template ──────────────────────────────────────────────
Write-Host "`n=== Installing template from: $templateRoot ===" -ForegroundColor Cyan
dotnet new install $templateRoot --force | Out-Null
Write-Host "Template installed." -ForegroundColor Green

# ── 2. Build combination matrix ────────────────────────────────────────────────
# JwtOption: None | Basic | Redis
# EnableWeChat, EnableTelemetry, EnableUnitTest, EnableRBAC: true | false
# Constraint: EnableRBAC=true requires JwtOption != None

$jwtOptions   = @("None", "Basic", "Redis")
$boolValues   = @($true, $false)
$combinations = [System.Collections.Generic.List[hashtable]]::new()

foreach ($jwt in $jwtOptions) {
    foreach ($weChat in $boolValues) {
        foreach ($telemetry in $boolValues) {
            foreach ($unitTest in $boolValues) {
                foreach ($rbac in $boolValues) {
                    # Skip invalid: RBAC requires JWT
                    if ($rbac -and $jwt -eq "None") { continue }
                    $combinations.Add(@{
                        JwtOption       = $jwt
                        EnableWeChat    = $weChat
                        EnableTelemetry = $telemetry
                        EnableUnitTest  = $unitTest
                        EnableRBAC      = $rbac
                    })
                }
            }
        }
    }
}

$total  = $combinations.Count
Write-Host "Total valid combinations: $total`n" -ForegroundColor Cyan

# ── 3. Run each combination ────────────────────────────────────────────────────
if (Test-Path $tempRoot) { Remove-Item $tempRoot -Recurse -Force }
New-Item $tempRoot -ItemType Directory | Out-Null

$results = [System.Collections.Generic.List[PSCustomObject]]::new()
$idx = 0

foreach ($combo in $combinations) {
    $idx++
    $tag  = "c{0:D3}_jwt_{1}_wechat_{2}_tel_{3}_test_{4}_rbac_{5}" -f `
        $idx,
        $combo.JwtOption.ToLower(),
        ($combo.EnableWeChat    -as [string]).ToLower(),
        ($combo.EnableTelemetry -as [string]).ToLower(),
        ($combo.EnableUnitTest  -as [string]).ToLower(),
        ($combo.EnableRBAC      -as [string]).ToLower()
    $shortName = "c{0:D3}" -f $idx          # short dir name avoids Windows MAX_PATH
    $outDir    = Join-Path $tempRoot $shortName

    Write-Host "[$idx/$total] $tag" -ForegroundColor DarkCyan -NoNewline

    # Build dotnet new arguments
    $newArgs = @(
        "new", "JfYuWebApi",
        "-o", $outDir,
        "--force",
        "--JwtOption",       $combo.JwtOption,
        "--EnableWeChat",    $combo.EnableWeChat.ToString().ToLower(),
        "--EnableTelemetry", $combo.EnableTelemetry.ToString().ToLower(),
        "--EnableUnitTest",  $combo.EnableUnitTest.ToString().ToLower(),
        "--EnableRBAC",      $combo.EnableRBAC.ToString().ToLower()
    )

    # Generate
    $genOutput = & dotnet @newArgs 2>&1
    $genOk     = $LASTEXITCODE -eq 0

    $buildOk  = $false
    $buildMsg = "skipped"

    if ($genOk) {
        # Find .slnx or .sln to build
        $slnFile = Get-ChildItem $outDir -Filter "*.slnx" -ErrorAction SilentlyContinue |
                   Select-Object -First 1
        if (-not $slnFile) {
            $slnFile = Get-ChildItem $outDir -Filter "*.sln" -ErrorAction SilentlyContinue |
                       Select-Object -First 1
        }

        if ($slnFile) {
            $buildOutput = & dotnet build $slnFile.FullName --nologo -v m 2>&1
            $buildOk     = $LASTEXITCODE -eq 0

            # Collect warnings (CS*/nullable/etc.) — skip NU package vulnerability notices
            $warnLines = $buildOutput | Where-Object { $_ -match ': warning ' -and $_ -notmatch ': warning NU\d' }
            $warnCount = $warnLines.Count

            # Collect errors
            $errLines  = $buildOutput | Where-Object { $_ -match ': error ' }

            $buildMsg  = if ($buildOk -and $warnCount -eq 0) {
                "OK"
            } elseif ($buildOk -and $warnCount -gt 0) {
                "OK+WARN($warnCount): " + ($warnLines | ForEach-Object {
                    if ($_ -match ': warning (CS\w+)') { $Matches[1] }
                } | Sort-Object -Unique) -join ","
            } else {
                "FAIL: " + ($errLines | Select-Object -First 3) -join " | "
            }
        } else {
            $buildMsg = "no sln found"
        }
    } else {
        $buildMsg = ($genOutput | Select-String "error|Error" | Select-Object -First 2) -join " | "
    }

    $status = if ($genOk -and $buildOk -and $warnCount -eq 0) { "PASS" }
              elseif ($genOk -and $buildOk -and $warnCount -gt 0) { "WARN" }
              else { "FAIL" }
    $color  = switch ($status) { "PASS" { "Green" } "WARN" { "Yellow" } "FAIL" { "Red" } }
    Write-Host "  [$status]" -ForegroundColor $color

    $results.Add([PSCustomObject]@{
        "#"          = $idx
        JwtOption    = $combo.JwtOption
        WeChat       = $combo.EnableWeChat
        Telemetry    = $combo.EnableTelemetry
        UnitTest     = $combo.EnableUnitTest
        RBAC         = $combo.EnableRBAC
        Generate     = if ($genOk) { "OK" } else { "FAIL" }
        Build        = if (-not $genOk) { "FAIL" } elseif ($buildOk) { if ($warnCount -gt 0) { "WARN($warnCount)" } else { "OK" } } else { "FAIL" }
        Details      = if ($status -eq "PASS") { "" } else { $buildMsg }
        _dir         = $outDir
        _pass        = $genOk -and $buildOk
        _warn        = $warnCount -gt 0
    })

    # Cleanup
    if ($KeepAll) { <# keep #> }
    elseif ($KeepFailures -and -not ($genOk -and $buildOk)) { <# keep failure #> }
    else { Remove-Item $outDir -Recurse -Force -ErrorAction SilentlyContinue }
}

# ── 4. Summary ────────────────────────────────────────────────────────────────
$passed   = ($results | Where-Object { $_._pass -and -not $_._warn }).Count
$warned   = ($results | Where-Object { $_._pass -and $_._warn }).Count
$failed   = ($results | Where-Object { -not $_._pass }).Count

Write-Host "`n=== RESULTS ===" -ForegroundColor Cyan
$results | Select-Object "#", JwtOption, WeChat, Telemetry, UnitTest, RBAC, Generate, Build, Details |
    Format-Table -AutoSize

Write-Host "Clean:    $passed / $total" -ForegroundColor Green
if ($warned -gt 0) {
    Write-Host "Warnings: $warned / $total" -ForegroundColor Yellow
    Write-Host "`nCombinations with warnings:" -ForegroundColor Yellow
    $results | Where-Object { $_._pass -and $_._warn } |
        Select-Object "#", JwtOption, WeChat, Telemetry, UnitTest, RBAC, Details |
        Format-Table -AutoSize
}
if ($failed -gt 0) {
    Write-Host "Failed:   $failed / $total" -ForegroundColor Red
    Write-Host "`nFailed combinations:" -ForegroundColor Red
    $results | Where-Object { -not $_._pass } |
        Select-Object "#", JwtOption, WeChat, Telemetry, UnitTest, RBAC, Details |
        Format-Table -AutoSize
}

if (-not $KeepAll -and -not $KeepFailures) {
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
} elseif ($KeepFailures -and $failed -eq 0 -and $warned -eq 0) {
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
} else {
    Write-Host "`nOutput dirs kept at: $tempRoot" -ForegroundColor DarkYellow
}

# Return non-zero exit code if any failures
if ($failed -gt 0) { exit 1 }
