@echo off
setlocal
rem Runs inside the installer extraction dir (deleted after this exits), so
rem the setup script installs to a persistent directory instead of this one.
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup-easyman.ps1" -InstallDir "%USERPROFILE%\easyman"
if errorlevel 1 pause
