@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

rem Allure JSON is written under bin\<Configuration>\<TFM>\allure-results (see Properties\AllurePathBootstrap.cs + allureConfig.json).
set "CONFIG=Debug"
set "TFM=net9.0"
set "ALLURE_RESULTS=%CD%\src\PTS.Automation\bin\%CONFIG%\%TFM%\allure-results"
set "ALLURE_REPORT=%CD%\allure-report"
set "SLN=%CD%\PTS.Automation.sln"

:menu
echo ========================================
echo     PTS Automation Test Runner
echo ========================================
echo.
echo  1. Run ALL tests
echo  2. Run P1 Critical tests only
echo  3. Run Member system tests only
echo  4. Run Admin system tests only
echo  5. Run P1 Member tests only
echo  6. Run P1 Admin tests only
echo  7. Run UI tests only
echo  8. Run Hybrid tests only
echo  9. Run single test by name
echo  0. Exit
echo.
set /p choice=Enter your choice: 

set "FILTER="
if "%choice%"=="1" goto :run_tests
if "%choice%"=="2" set "FILTER=TestCategory=P1" & goto :run_tests
if "%choice%"=="3" set "FILTER=TestCategory=Member" & goto :run_tests
if "%choice%"=="4" set "FILTER=TestCategory=Admin" & goto :run_tests
if "%choice%"=="5" set "FILTER=TestCategory=P1&TestCategory=Member" & goto :run_tests
if "%choice%"=="6" set "FILTER=TestCategory=P1&TestCategory=Admin" & goto :run_tests
if "%choice%"=="7" set "FILTER=TestCategory=UI" & goto :run_tests
if "%choice%"=="8" set "FILTER=TestCategory=Hybrid" & goto :run_tests
if "%choice%"=="9" (
    set /p "FILTER=Enter test name: "
    goto :run_tests
)
if "%choice%"=="0" exit /b 0

echo Invalid choice. Try again.
echo.
goto :menu

:run_tests
if exist "%ALLURE_RESULTS%" (
    echo Cleaning previous Allure results...
    rmdir /s /q "%ALLURE_RESULTS%"
)

set HEADED=1
set PLAYWRIGHT_SLOWMO=500
echo Starting browser in headed mode...

if not defined FILTER (
    echo Running ALL tests...
    dotnet test "%SLN%" -c %CONFIG%
) else (
    echo Running tests with filter: !FILTER!
    dotnet test "%SLN%" -c %CONFIG% --filter "!FILTER!"
)
REM Continue even if tests fail to generate report

if not exist "%ALLURE_RESULTS%" (
    echo.
    echo WARNING: Allure results folder not found after tests:
    echo   %ALLURE_RESULTS%
    echo Build output TFM is %TFM%. If you use a different target framework, edit CONFIG/TFM in this script.
    echo Skipping allure generate / open.
    echo.
    goto :EndPause
)

where allure >nul 2>&1
if errorlevel 1 (
    echo WARNING: Allure CLI is not on PATH. Install Allure 2 and add its bin folder to PATH.
    echo   https://github.com/allure-framework/allure2/releases
    goto :EndPause
)

echo Tests complete. Generating report...
call allure generate "%ALLURE_RESULTS%" --clean -o "%ALLURE_REPORT%"
if errorlevel 1 (
    echo ERROR: allure generate failed ^(exit code !ERRORLEVEL!^).
    goto :EndPause
)
echo Opening Allure report...
call allure open "%ALLURE_REPORT%"

:EndPause
pause
endlocal
