# XLinkViewer Launcher Script
# This script collects all passed DWG file arguments and launches XLinkViewer once

param(
    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]$Files
)

# Get the directory where this script is located
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Path to the actual XLinkViewer.exe
$exePath = Join-Path $scriptDir "XLinkViewer.exe"

# Check if XLinkViewer is already running
$runningProcess = Get-Process -Name "XLinkViewer" -ErrorAction SilentlyContinue

if ($runningProcess) {
    # Already running - show message
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show(
        "XLink Viewer is already running. Please use the existing window.",
        "Already Running",
        [System.Windows.MessageBoxButton]::OK,
        [System.Windows.MessageBoxImage]::Information
    )
    exit
}

# Launch XLinkViewer with all collected file arguments
if ($Files.Count -gt 0) {
    & $exePath $Files
} else {
    & $exePath
}
