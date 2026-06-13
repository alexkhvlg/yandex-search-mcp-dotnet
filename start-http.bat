@echo off
cd /d "%~dp0"
pwsh -File "%~dp0start-http.ps1" %*
