<#
.SYNOPSIS
    One-shot verification that Member login works against the configured QA env.

.DESCRIPTION
    Sets the required credential env vars in this session, runs only the
    Member_logs_in_with_valid_credentials test in headed mode, writes the
    full console output to TestResults/login-verification.txt, and finally
    wipes the creds from the shell session.

.PARAMETER Username
    QA Member username.

.PARAMETER Password
    QA Member password.

.PARAMETER Headless
    If set, runs without opening a visible browser window. Default: headed.

.EXAMPLE
    .\build\verify-member-login.ps1 -Username "alice@example.com" -Password "s3cret!"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Username,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [switch]$Headless
)

$ErrorActionPreference = 'Stop'

# Anchor paths to the script's location, regardless of caller's cwd.
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Resolve-Path (Join-Path $scriptDir '..')
$outDir    = Join-Path $repoRoot 'TestResults'
$outFile   = Join-Path $outDir  'login-verification.txt'

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

try {
    Write-Host "==> Setting credentials in this shell session" -ForegroundColor Cyan
    $env:PTS_Users__Member__Username = $Username
    $env:PTS_Users__Member__Password = $Password
    if (-not $Headless) { $env:HEADED = '1' } else { Remove-Item Env:HEADED -ErrorAction SilentlyContinue }

    # Sanity check — PASSWORD VALUE IS NEVER PRINTED, only presence.
    $diag = @(
        "==> Environment diagnostic"
        "Username set?       $([bool]$env:PTS_Users__Member__Username)"
        "Password set?       $([bool]$env:PTS_Users__Member__Password)"
        "Username length:    $($env:PTS_Users__Member__Username.Length)"
        "Password length:    $($env:PTS_Users__Member__Password.Length)"
        "Headed mode:        $(if ($env:HEADED) { 'on' } else { 'off' })"
        "Working directory:  $repoRoot"
        "Test output file:   $outFile"
        "Timestamp:          $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        ""
    )
    $diag | ForEach-Object { Write-Host $_ }
    $diag | Out-File -FilePath $outFile -Encoding utf8

    Push-Location $repoRoot
    try {
        Write-Host "==> Running Member login test (filtered, single test)" -ForegroundColor Cyan
        Write-Host ""

        # Run dotnet test and tee its stdout+stderr to our output file.
        $cmd = 'dotnet'
        $args = @(
            'test'
            '--filter'
            'FullyQualifiedName=PTS.Automation.Features.Smoke.MemberLoginSmokeTests.Member_logs_in_with_valid_credentials'
            '--logger'
            'console;verbosity=detailed'
            '--nologo'
        )

        & $cmd @args 2>&1 | Tee-Object -FilePath $outFile -Append
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    "" | Out-File -FilePath $outFile -Append
    "==> dotnet test exit code: $exitCode" | Tee-Object -FilePath $outFile -Append | Out-Null

    Write-Host ""
    Write-Host "==> Full console captured to:" -ForegroundColor Cyan
    Write-Host "    $outFile"

    exit $exitCode
}
finally {
    # Always wipe credentials from the shell, even if the test threw.
    Remove-Item Env:PTS_Users__Member__Username -ErrorAction SilentlyContinue
    Remove-Item Env:PTS_Users__Member__Password -ErrorAction SilentlyContinue
    Remove-Item Env:HEADED                       -ErrorAction SilentlyContinue
    Write-Host "==> Credentials removed from shell session" -ForegroundColor DarkGray
}
