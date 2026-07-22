#requires -Version 5.1
<#
Installiert Python 3.12 (falls erforderlich) und eine lokale virtuelle Umgebung
für dieses Projekt. Die Pakete werden nur in .\install\venv abgelegt.

Ausführen aus PowerShell:
  powershell -ExecutionPolicy Bypass -File .\install\install_python_env.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$installDirectory = Split-Path -Parent $PSCommandPath
$projectDirectory = Split-Path -Parent $installDirectory
$venvDirectory = Join-Path $installDirectory 'venv'
$requirementsFile = Join-Path $projectDirectory 'requirements.txt'

function Update-ProcessPath {
    $machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    $env:Path = ($machinePath, $userPath, $env:Path -join ';').Trim(';')
}

function Find-Python312 {
    $pyCommand = Get-Command py.exe -ErrorAction SilentlyContinue
    if ($pyCommand) {
        & $pyCommand.Source -3.12 --version *> $null
        if ($LASTEXITCODE -eq 0) {
            return [pscustomobject]@{ Executable = $pyCommand.Source; Arguments = @('-3.12') }
        }
    }

    $knownPython = Join-Path $env:LocalAppData 'Programs\Python\Python312\python.exe'
    if (Test-Path -LiteralPath $knownPython) {
        return [pscustomobject]@{ Executable = $knownPython; Arguments = @() }
    }
    return $null
}

Write-Host 'Prüfe Python 3.12 ...' -ForegroundColor Cyan
$pythonCommand = Find-Python312
if ($null -eq $pythonCommand) {
    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if ($null -eq $winget) {
        throw 'Python 3.12 wurde nicht gefunden und winget ist nicht verfügbar. Bitte Python 3.12 von python.org installieren und dieses Skript erneut ausführen.'
    }

    Write-Host 'Installiere Python 3.12 über winget ...' -ForegroundColor Yellow
    & $winget.Source install --id Python.Python.3.12 --exact --source winget --accept-package-agreements --accept-source-agreements
    if ($LASTEXITCODE -ne 0) {
        throw "Die Installation von Python 3.12 ist fehlgeschlagen (Exit-Code $LASTEXITCODE)."
    }

    Update-ProcessPath
    $pythonCommand = Find-Python312
    if ($null -eq $pythonCommand) {
        throw 'Python 3.12 wurde installiert, aber in dieser Sitzung nicht gefunden. Bitte ein neues PowerShell-Fenster öffnen und das Skript erneut starten.'
    }
}

$pythonVersion = & $pythonCommand.Executable @($pythonCommand.Arguments) --version
Write-Host "Verwende $pythonVersion" -ForegroundColor Green

if (-not (Test-Path -LiteralPath $venvDirectory)) {
    Write-Host "Erstelle virtuelle Umgebung: $venvDirectory" -ForegroundColor Cyan
    & $pythonCommand.Executable @($pythonCommand.Arguments) -m venv $venvDirectory
    if ($LASTEXITCODE -ne 0) { throw 'Die virtuelle Python-Umgebung konnte nicht erstellt werden.' }
}

$venvPython = Join-Path $venvDirectory 'Scripts\python.exe'
if (-not (Test-Path -LiteralPath $venvPython)) {
    throw "Python in der virtuellen Umgebung wurde nicht gefunden: $venvPython"
}

Write-Host 'Aktualisiere pip und installiere Projektpakete ...' -ForegroundColor Cyan
& $venvPython -m pip install --upgrade pip
if ($LASTEXITCODE -ne 0) { throw 'Das pip-Upgrade ist fehlgeschlagen.' }
& $venvPython -m pip install --requirement $requirementsFile
if ($LASTEXITCODE -ne 0) { throw 'Die Installation der Python-Pakete ist fehlgeschlagen.' }

Write-Host ''
Write-Host 'Installation abgeschlossen.' -ForegroundColor Green
Write-Host "Server starten: `"$venvPython`" `"$projectDirectory\tennis_live_server.py`""
