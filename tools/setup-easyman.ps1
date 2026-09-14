param(
    [string]$InstallDir = "$env:USERPROFILE\easyman"
)

$ErrorActionPreference = "Stop"

$exe = Join-Path $PSScriptRoot "Easyman.exe"
$mm = Join-Path $PSScriptRoot "middleman.exe"
$cfg = Join-Path $PSScriptRoot "allowed_emu.txt"

if (-not (Test-Path $exe)) { throw "Easyman.exe not found in $PSScriptRoot" }
if (-not (Test-Path $mm)) { throw "middleman.exe not found in $PSScriptRoot" }
if (-not (Test-Path $cfg)) { throw "allowed_emu.txt not found in $PSScriptRoot" }

# A re-run usually happens while the app is still open; Windows cannot
# overwrite a running executable, so stop it and wait for the handle to drop.
foreach ($name in @("Easyman", "middleman")) {
    if (Get-Process -Name $name -ErrorAction SilentlyContinue) {
        Write-Host "stopping running $name..."
        Stop-Process -Name $name -Force -ErrorAction SilentlyContinue
        Wait-Process -Name $name -Timeout 10 -ErrorAction SilentlyContinue
    }
}

New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
# Retry the copy in case the process handle has not fully dropped yet.
for ($i = 0; $i -lt 5; $i++) {
    try {
        Copy-Item $exe, $mm, $cfg -Destination $InstallDir -Force -ErrorAction Stop
        break
    } catch {
        if ($i -eq 4) { throw }
        Start-Sleep -Milliseconds 500
    }
}

$desktop = [Environment]::GetFolderPath("Desktop")
$lnkPath = Join-Path $desktop "Easyman.lnk"
$ws = New-Object -ComObject WScript.Shell
$lnk = $ws.CreateShortcut($lnkPath)
$lnk.TargetPath = Join-Path $InstallDir "Easyman.exe"
$lnk.WorkingDirectory = $InstallDir
$lnk.IconLocation = "$env:SystemRoot\System32\shell32.dll,18"
$lnk.Description = "Easyman - Project 1999 login middleman"
$lnk.Save()

Write-Host "installed to $InstallDir"
Write-Host "desktop shortcut: $lnkPath"
Start-Process (Join-Path $InstallDir "Easyman.exe")
