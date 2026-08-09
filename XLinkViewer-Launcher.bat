@echo off
setlocal enabledelayedexpansion

REM Get the directory where this batch file is located
set "APP_DIR=%~dp0"
set "EXE_PATH=%APP_DIR%XLinkViewer.exe"
set "LOCK_FILE=%TEMP%\XLinkViewer_Launch.lock"
set "ARGS_FILE=%TEMP%\XLinkViewer_Args.txt"

REM Check if already running
tasklist /FI "IMAGENAME eq XLinkViewer.exe" 2>NUL | find /I /N "XLinkViewer.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo XLink Viewer is already running.
    exit /b
)

REM Check if we're in a batch collection window (within 2 seconds of last call)
if exist "%LOCK_FILE%" (
    REM Add this file to the arguments list
    echo %~1>> "%ARGS_FILE%"
    exit /b
)

REM Start a new collection window
echo %~1> "%ARGS_FILE%"
echo lock> "%LOCK_FILE%"

REM Wait for additional files (Windows Explorer calls context menu commands rapidly)
timeout /t 1 /nobreak >nul

REM Collect all arguments from the file
set "ALL_ARGS="
for /f "usebackq delims=" %%a in ("%ARGS_FILE%") do (
    set "ALL_ARGS=!ALL_ARGS! "%%a""
)

REM Clean up
del "%LOCK_FILE%" 2>nul
del "%ARGS_FILE%" 2>nul

REM Launch XLinkViewer with all collected arguments
start "" "%EXE_PATH%" %ALL_ARGS%
