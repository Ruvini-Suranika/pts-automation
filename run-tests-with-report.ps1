<#
.SYNOPSIS
    Run PTS.Automation tests, merge Allure history, generate the report, and open it.

.DESCRIPTION
    Runs from the Automation folder root:
      1. dotnet test (optional NUnit --filter)
      2. Copies allure-report\history -> allure-results\history (trend data)
      3. allure generate allure-results --clean -o allure-report
      4. allure open allure-report

    Raw Allure results are produced under the test project bin folder; this script mirrors them
    to .\allure-results at the Automation root so generate/open use the paths above.

.PARAMETER Filter
    Optional NUnit filter (same as dotnet test --filter). Omit to run all tests.

.EXAMPLE
    .\run-tests-with-report.ps1

.EXAMPLE
    .\run-tests-with-report.ps1 -filter "TestCategory=P1&TestCategory=Member"
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [Alias("filter")]
    [string] $Filter = "",

    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Debug",

    [string] $RunSettings = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AutomationRoot = $PSScriptRoot
$Csproj = Join-Path $AutomationRoot "src\PTS.Automation\PTS.Automation.csproj"
$TargetFramework = "net9.0"
$BinAllureResults = Join-Path $AutomationRoot "src\PTS.Automation\bin\$Configuration\$TargetFramework\allure-results"

# Workspace-relative paths used by `allure generate` / `allure open` (per script location).
$WorkspaceAllureResults = Join-Path $AutomationRoot "allure-results"
$WorkspaceAllureReport = Join-Path $AutomationRoot "allure-report"
$PreviousHistory = Join-Path $WorkspaceAllureReport "history"

if (-not (Test-Path -LiteralPath $Csproj)) {
    Write-Error "Test project not found: $Csproj"
}

$testArgs = @("test", $Csproj, "-c", $Configuration, "-v", "n")

if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $testArgs += @("--filter", $Filter)
}

if (-not [string]::IsNullOrWhiteSpace($RunSettings)) {
    $testArgs += @("--settings", (Resolve-Path -LiteralPath $RunSettings -ErrorAction Stop).Path)
}
else {
    $defaultRs = Join-Path $AutomationRoot ".runsettings"
    if (Test-Path -LiteralPath $defaultRs) {
        $testArgs += @("--settings", (Resolve-Path -LiteralPath $defaultRs).Path)
    }
}

Write-Host ">> dotnet $($testArgs -join ' ')" -ForegroundColor Cyan
Push-Location $AutomationRoot
try {
    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "dotnet test exited with code $LASTEXITCODE; continuing with Allure steps."
    }
}
finally {
    Pop-Location
}

if (-not (Test-Path -LiteralPath $BinAllureResults)) {
    Write-Error "Allure results not found after test run: $BinAllureResults`nAdjust -Configuration or run a build first."
}

# Mirror bin output to .\allure-results so `allure generate allure-results ...` works from Automation root.
Write-Host ">> Mirroring Allure results -> $WorkspaceAllureResults" -ForegroundColor Cyan
if (Test-Path -LiteralPath $WorkspaceAllureResults) {
    Remove-Item -LiteralPath $WorkspaceAllureResults -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $WorkspaceAllureResults | Out-Null
Copy-Item -Path (Join-Path $BinAllureResults "*") -Destination $WorkspaceAllureResults -Recurse -Force

# Preserve trends: copy previous report history into the new results folder before generate.
if (Test-Path -LiteralPath $PreviousHistory) {
    $destHistory = Join-Path $WorkspaceAllureResults "history"
    Write-Host ">> Copying allure-report\history -> allure-results\history" -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path $destHistory | Out-Null
    Copy-Item -Path (Join-Path $PreviousHistory "*") -Destination $destHistory -Recurse -Force
}
else {
    Write-Host ">> No previous allure-report\history (first run)." -ForegroundColor DarkGray
}

if (-not (Get-Command allure -ErrorAction SilentlyContinue)) {
    Write-Error "Allure CLI not on PATH. Install from https://github.com/allure-framework/allure2/releases"
}

Push-Location $AutomationRoot
try {
    Write-Host ">> allure generate allure-results --clean -o allure-report" -ForegroundColor Cyan
    & allure generate allure-results --clean -o allure-report
    if ($LASTEXITCODE -ne 0) {
        Write-Error "allure generate failed with exit code $LASTEXITCODE"
    }

    Write-Host ">> allure open allure-report" -ForegroundColor Cyan
    & allure open allure-report
}
finally {
    Pop-Location
}

Write-Host "Done. Report: $WorkspaceAllureReport" -ForegroundColor Green
