"""Stellt die aktuelle vMix-Tennisdatei als JSON und als Live-Ansicht bereit."""

import ipaddress
import json
import logging
import os
import shutil
import socket
import sys
import threading
import time
from datetime import datetime, timezone
from pathlib import Path

import psutil
from flask import Flask, Response, jsonify, request, send_from_directory

PORT = 42100
DATA_FILE = Path(r"C:\vmix\tennis\data\tennis24_live.json")
FLAGS_DIR = Path(r"C:\vmix\tennis\flags")
APP_START = time.time()
BASE_DIR = Path(__file__).resolve().parent

app = Flask(__name__, static_folder=str(BASE_DIR / "static"))
request_state = {"text": "WAITING FOR DATA", "is_error": False}
request_state_lock = threading.Lock()


def port_is_busy(port: int) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        return sock.connect_ex(("127.0.0.1", port)) == 0


def lan_ips() -> list[str]:
    addresses = set()
    try:
        for result in socket.getaddrinfo(socket.gethostname(), None, socket.AF_INET):
            address = result[4][0]
            parsed = ipaddress.ip_address(address)
            if not parsed.is_loopback and not parsed.is_unspecified:
                addresses.add(address)
    except socket.gaierror:
        pass
    private = sorted(ip for ip in addresses if ipaddress.ip_address(ip).is_private)
    return private or sorted(addresses) or ["127.0.0.1"]


def pretty_uptime(seconds: int) -> str:
    days, remainder = divmod(seconds, 86400)
    hours, remainder = divmod(remainder, 3600)
    minutes, seconds = divmod(remainder, 60)
    return f"{days} Tage, {hours:02}:{minutes:02}:{seconds:02}"


def heartbeat_state(live_state: dict) -> tuple[bool, float | None, float]:
    try:
        interval = max(1.0, float(live_state.get("heartbeatIntervalSeconds", 5)))
    except (TypeError, ValueError):
        interval = 5.0
    timeout = max(10.0, interval * 3.0)
    updated_at = live_state.get("updatedAt")
    if not isinstance(updated_at, str) or not updated_at.strip():
        return False, None, timeout
    try:
        timestamp = datetime.fromisoformat(updated_at.strip().replace("Z", "+00:00"))
        if timestamp.tzinfo is None:
            timestamp = timestamp.replace(tzinfo=timezone.utc)
        age = max(0.0, (datetime.now(timezone.utc) - timestamp).total_seconds())
    except ValueError:
        return False, None, timeout
    return age <= timeout, age, timeout


def terminal_status() -> None:
    psutil.cpu_percent(None)  # erster Aufruf initialisiert die Messung
    while True:
        os.system("cls" if os.name == "nt" else "clear")
        width = max(48, min(shutil.get_terminal_size((80, 20)).columns - 1, 92))
        line = "-" * width
        ips = lan_ips()
        process = psutil.Process(os.getpid())
        now = datetime.now().strftime("%d.%m.%Y %H:%M:%S")
        cpu = psutil.cpu_percent(None)
        ram = process.memory_info().rss / 1024 / 1024
        with request_state_lock:
            fetch_status = request_state["text"]

        print("TENNIS 24 LIVE  |  JSON SERVER  |  1000ms update")
        print(fetch_status)
        print(line)
        print(f"Zeit       {now}")
        print(f"Uptime     {pretty_uptime(int(time.time() - APP_START))}")
        print(f"System     CPU {cpu:5.1f}%  |  RAM {ram:7.1f} MB")
        print()
        print(f"JSON       http://{ips[0]}:{PORT}/tennis24_live.json")
        print(f"Live       http://{ips[0]}:{PORT}/")
        if len(ips) > 1:
            print(f"Weitere IPs {', '.join(ips[1:])}")
        print()
        print(f"Quelle     {DATA_FILE}")
        print(line, flush=True)
        time.sleep(1)


@app.after_request
def update_fetch_status(response):
    if request.path in ("/api/live", "/tennis24_live.json"):
        now = datetime.now().strftime("%H:%M:%S")
        if response.status_code < 400:
            text = f"DATA FETCHED OK  |  {now}"
        else:
            text = f"DATA ERROR {response.status_code}  |  {now}"
        with request_state_lock:
            request_state["text"] = text
            request_state["is_error"] = response.status_code >= 400
    return response


@app.get("/")
def live_page():
    return send_from_directory(app.static_folder, "index.html")


@app.get("/flags/<path:filename>")
def flag_image(filename: str):
    return send_from_directory(FLAGS_DIR, filename)


@app.get("/tennis24_live.json")
@app.get("/api/live")
def live_json():
    if not DATA_FILE.is_file():
        return jsonify(error="Die Quelldatei ist noch nicht vorhanden.", file=str(DATA_FILE)), 404
    if DATA_FILE.stat().st_mtime < APP_START:
        return jsonify(
            error="Die Quelldatei enthaelt noch keine Daten der aktuellen Server-Sitzung.",
            file=str(DATA_FILE),
        ), 404
    try:
        raw = DATA_FILE.read_text(encoding="utf-8-sig")
        data = json.loads(raw)  # Syntax vor dem Ausliefern prüfen
    except UnicodeDecodeError:
        raw = DATA_FILE.read_text(encoding="cp1252")
        try:
            data = json.loads(raw)
        except json.JSONDecodeError as error:
            return jsonify(error="Ungültiges JSON in der Quelldatei.", detail=str(error)), 500
    except json.JSONDecodeError as error:
        return jsonify(error="Ungültiges JSON in der Quelldatei.", detail=str(error)), 500
    # vMix requires an array at the JSON data-source endpoint. The browser
    # view uses the live-state object contained in that array.
    if isinstance(data, list):
        if not data or not isinstance(data[0], dict):
            return jsonify(error="Das JSON-Array enthaelt kein Live-State-Objekt."), 500
        live_state = data[0]
        response_data = data[0] if request.path == "/api/live" else data
    elif isinstance(data, dict):
        # Keep accepting source files in the previous object-only format.
        live_state = data
        response_data = data if request.path == "/api/live" else [data]
    else:
        return jsonify(error="Das JSON muss ein Objekt oder ein Array mit einem Objekt enthalten."), 500

    heartbeat_ok, heartbeat_age, heartbeat_timeout = heartbeat_state(live_state)
    if not heartbeat_ok:
        return jsonify(
            error="Tennis26 reagiert nicht mehr oder hat noch keinen aktuellen Heartbeat geliefert.",
            heartbeat_age_seconds=round(heartbeat_age, 1) if heartbeat_age is not None else None,
            heartbeat_timeout_seconds=heartbeat_timeout,
        ), 503

    return Response(
        json.dumps(response_data, ensure_ascii=False, separators=(",", ":")),
        content_type="application/json; charset=utf-8",
        headers={"Access-Control-Allow-Origin": "*", "Cache-Control": "no-store"},
    )


@app.get("/health")
def health():
    source_exists = DATA_FILE.is_file()
    source_ready = source_exists and DATA_FILE.stat().st_mtime >= APP_START
    return jsonify(
        status="ok",
        source_exists=source_exists,
        source_ready=source_ready,
        source=str(DATA_FILE),
    )


if __name__ == "__main__":
    if port_is_busy(PORT):
        sys.exit(f"FEHLER: Port {PORT} ist bereits belegt.")
    # Keine endlose Liste einzelner HTTP-Aufrufe im Terminal anzeigen.
    logging.getLogger("werkzeug").disabled = True
    app.logger.disabled = True
    threading.Thread(target=terminal_status, daemon=True).start()
    app.run(host="0.0.0.0", port=PORT, debug=False, use_reloader=False)
