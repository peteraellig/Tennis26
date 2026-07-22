@echo off
cd /d "%~dp0"
if exist "%~dp0install\venv\Scripts\python.exe" (
    "%~dp0install\venv\Scripts\python.exe" tennis_live_server.py
) else (
    python tennis_live_server.py
)
pause
