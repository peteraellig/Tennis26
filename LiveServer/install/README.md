# Installation des Tennis-Live-Servers

Dieses Projekt verwendet eine eigene Python-Umgebung. Dadurch werden Flask und
weitere Pakete nicht systemweit installiert, sondern nur für dieses Projekt.

## Ordnerstruktur

Die Python-Datei muss im Projekt-Hauptordner liegen, also neben dem Ordner
`install`:

```text
PiPo WEF HTML\
├── tennis_live_server.py
├── requirements.txt
└── install\
    ├── install_python_env.ps1
    └── venv\                 ← wird durch das Installationsskript erstellt
```

Die Datei `tennis_live_server.py` muss **nicht** in `install\venv` kopiert
werden. Sie bleibt im Projekt-Hauptordner.

## Einmalige Installation

1. PowerShell im Projekt-Hauptordner öffnen.

2. Das Installationsskript ausführen:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\install\install_python_env.ps1
   ```

3. Das Skript prüft zunächst Python 3.12. Falls es noch nicht vorhanden ist,
   wird es über `winget` installiert.

4. Danach erstellt das Skript die lokale virtuelle Umgebung unter
   `install\venv` und installiert die benötigten Pakete:

   - Flask
   - psutil

Für die Python-Installation kann Windows beziehungsweise `winget` eine
Bestätigung anzeigen. Diese bitte zulassen.

## Server starten

Nach erfolgreicher Installation im Projekt-Hauptordner ausführen:

```powershell
.\install\venv\Scripts\python.exe .\tennis_live_server.py
```

Der Server liest die Tennisdaten aus:

```text
C:\vmix\tennis\data\tennis24_live.json
```

Standardmäßig ist die Live-Ansicht anschließend über Port `42100` erreichbar,
zum Beispiel unter `http://localhost:42100/`.

## Bei Änderungen an Python-Abhängigkeiten

Neue externe Pakete bitte in `requirements.txt` eintragen. Anschließend das
Installationsskript erneut starten; es aktualisiert die vorhandene lokale
Umgebung.
