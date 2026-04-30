@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

set "CSPROJ=src\PTS.Automation\PTS.Automation.csproj"
set "FILTER=TestCategory=P1&TestCategory=Member"
set "CONFIG=Debug"
set "TFM=net9.0"
rem Allure JSON is emitted next to PTS.Automation.dll (see Properties\AllurePathBootstrap.cs).
set "BINRESULTS=src\PTS.Automation\bin\%CONFIG%\%TFM%\allure-results"
set "RUNSETTINGS=%CD%\.runsettings"

echo =============================================================================
echo   Member P1 tests  ^(headless / fast — filter: %FILTER%^)
echo =============================================================================
echo.

if not exist "%CSPROJ%" (
    echo ERROR: Project file not found:
    echo   %CD%\%CSPROJ%
    echo.
    goto :EndPause
)

rem Headless: do not force headed mode ^(override if parent shell had HEADED set^).
set "HEADED="
set "PLAYWRIGHT_SLOWMO="

if exist "%RUNSETTINGS%" (
    dotnet test "%CSPROJ%" -c %CONFIG% -v n --filter "%FILTER%" --settings "%RUNSETTINGS%"
) else (
    dotnet test "%CSPROJ%" -c %CONFIG% -v n --filter "%FILTER%"
)

if errorlevel 1 (
    echo.
    echo =============================================================================
    echo   ERROR: dotnet test failed ^(exit code !ERRORLEVEL!^).
    echo.
    echo   The Allure report was NOT generated. Fix failing tests and run this
    echo   script again.
    echo =============================================================================
    echo.
    goto :EndPause
)

echo.
echo dotnet test completed successfully.
echo.

if not exist "%BINRESULTS%" (
    echo ERROR: Allure results folder not found after tests:
    echo   %CD%\%BINRESULTS%
    echo.
    echo   Skipping Allure report generation.
    echo.
    goto :EndPause
)

where allure >nul 2>&1
if errorlevel 1 (
    echo WARNING: Allure CLI is not on PATH. Skipping report generation.
    echo   Install Allure 2 and add its bin folder to PATH, then re-run this script.
    echo.
    goto :EndPause
)

echo Mirroring Allure results to .\allure-results ...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$src='%CD%\%BINRESULTS%'; $dst='%CD%\allure-results'; if (-not (Test-Path -LiteralPath $src)) { exit 2 }; if (Test-Path -LiteralPath $dst) { Remove-Item -LiteralPath $dst -Recurse -Force }; Copy-Item -LiteralPath $src -Destination $dst -Recurse -Force"
if errorlevel 1 (
    echo ERROR: Failed to mirror Allure results from bin output.
    goto :EndPause
)

if exist "allure-report\history" (
    echo Merging previous Allure history...
    powershell -NoProfile -ExecutionPolicy Bypass -Command ^
      "$h='%CD%\allure-report\history'; $d='%CD%\allure-results\history'; New-Item -ItemType Directory -Force -Path $d ^| Out-Null; Copy-Item -Path (Join-Path $h '*') -Destination $d -Recurse -Force"
)

echo Generating Allure report...
call allure generate allure-results --clean -o allure-report
if errorlevel 1 (
    echo.
    echo ERROR: allure generate failed ^(exit code !ERRORLEVEL!^).
    echo.
    goto :EndPause
)

echo Opening Allure report...
call allure open allure-report
echo.
echo Report folder: %CD%\allure-report
echo.

:EndPause
pause
endlocal
