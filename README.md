# TENNIS26

**Live Scoring for vMix and the Production Team**

One control point. Rules-compliant scoring. Two live output channels.

`Windows Forms` · `GT Titles` · `JSON` · `Flask`

---

## Overview

Tennis26 is a Windows Forms (.NET Framework 4.8) live tennis-scoring application built for broadcast production with vMix. A single operator interface manages players, prepares upcoming matches, enters points live, and drives vMix graphics (GT Titles) — while a live JSON feed makes the same match state available to a companion web display and any other system that can read HTTP JSON.

## Documentation

- [Full presentation (English)](https://github.com/peteraellig/Tennis26/blob/master/Documentation/Tennis26_vMix_Scorer_Presentation_EN.pdf)
- [Vollständige Präsentation (Deutsch)](https://github.com/peteraellig/Tennis26/blob/master/Documentation/Tennis26_vMix_Scorer_Praesentation_v1.0.1.6.pdf)

## The Main Form manages players and the current production context

**Player data:** last name, first name, country, ISO3 code, age, height, ranking, points, and association are managed centrally (import, edit, save). Selected players are passed to the scorer as the Home/Away pairing, including metadata for titles and JSON.

## Match preparation — four matches ready before the broadcast

- Four independent pairing slots
- Assign players by drag and drop, choose singles or doubles
- Doubles supported with two partners per side
- Activate a prepared match with one click; the active pairing is highlighted
- Stored in `pairings.xml`

## Settings connect match rules, vMix, and broadcast design

| Section | Covers |
|---|---|
| **vMix** | IP address, HTTP/TCP ports, protocol, and overlay channels |
| **Match** | Best of 3/5, match tiebreak, set freeze, and winner color |
| **Content** | Tournament, venue, round, referees, commentators, sponsors, and info text |
| **Data** | Enable the JSON file, and verify its path and accessible feed URL |

Configuration is saved and reloaded automatically at the next startup.

## Live operation — one interface for the whole match

Enter points, change the server, switch the scorebug, and trigger graphics without moving between applications.

- Undo · Save match · Restore
- Player, pairing, sponsor, and information titles directly accessible

## Match engine

**Scoring** — 0 · 15 · 30 · 40 · Advantage, games, sets, and tiebreaks, serving and changeovers.

**Match modes** — Best of 3 / Best of 5, match tiebreak in the deciding set, mid-match entry (start scoring an already-running match at any point).

**Live statistics** — breaks and break points, mini-breaks and service games, points-won percentage and longest game.

Every point updates the same match state, which feeds the graphics, the statistics window, and the network outputs consistently.

## GT Titles cover the complete broadcast workflow

`Scorebug` · `Lower Third` · `Match Pairing` · `Large Result` · `Info` · `Sponsor`

The scorer populates and switches these titles using `SetText` and overlay commands — including dedicated doubles variants where a template needs to show two players per side.

## Live JSON feed

Tennis26 writes the full match state to a local JSON file after every point (and on a heartbeat while nothing changes, so a consumer can tell a real pause apart from a frozen application). A companion Python/Flask application ([`LiveServer/`](LiveServer)) reads that file and serves it over the LAN:

```
Operator            Match-State              vMix
Tennis26 GUI  ──▶  Engine + State Store ──▶  HTTP or TCP
                          │
                          ▼
                    JSON file (tennis24_live.json)
                          │
                          ▼
                  Python · Flask :42100
                          │
              ┌───────────┴───────────┐
              ▼                       ▼
      HTML live view           JSON feed (CORS-enabled)
   (score, server, stats,     for vMix Data Source, web/
    updates every second)     mobile displays, scoreboards,
                              LED systems, stats/automation
```

A failure or restart of the web extension does not affect the core scoring and vMix functions — Tennis26 keeps working independently.

## Repository contents

| Path | Contents |
|---|---|
| [`Tennis26/`](Tennis26) | The WinForms application (VB.NET, .NET Framework 4.8) |
| [`LiveServer/`](LiveServer) | Companion Python/Flask app: `tennis_live_server.py`, `start_tennis_live_server.bat`, HTML live view |
| [`vMixAssets/titles/`](vMixAssets/titles) | GT Title templates (`.gtzip`) for vMix |
| [`vMixAssets/graphical_templates/`](vMixAssets/graphical_templates) | Exported graphics behind the titles |
| [`vMixAssets/flags/`](vMixAssets/flags) | Country flag images used by the titles and the live view |
| [`vMixAssets/vMix_project/`](vMixAssets/vMix_project) | The vMix project file (`tennis24.vmix`) |
| [`vMixAssets/data/`](vMixAssets/data) | Runtime data: player database, pairings, settings, title board texts, save games |
| [`vMixAssets/setup/`](vMixAssets/setup) | ClickOnce installer output |
| [`Documentation/`](Documentation) | Presentation slides (see above) |

`LiveServer/tennis_live_server.py` has two paths hardcoded near the top (`DATA_FILE`, `FLAGS_DIR`) that point at this machine's local vMix folder — adjust them to your own setup before running the server elsewhere.

To set up the LiveServer on a new machine, see [`LiveServer/install/README.md`](LiveServer/install/README.md) — it walks through the PowerShell installer that provisions Python and the required packages.

## Reliability

- **Responsiveness** — a persistent TCP connection avoids an HTTP handshake for every command; HTTP and TCP senders share the same interface and remain interchangeable.
- **Safety** — an undo stack, save/load, and automatic crash/power-outage recovery protect against operator errors and restarts.
- **Data quality** — the JSON feed carries a heartbeat interval so consumers can detect stale data, and the app keeps working locally even if vMix or the JSON feed server is unreachable.

## Production workflow

1. Select players & pairing
2. Set the match mode
3. Enter points live
4. Graphics & web update automatically
5. Show result & statistics

The operator focuses on the match while the technical distribution runs in the background.

## Requirements

- Windows, .NET Framework 4.8
- [vMix](https://www.vmix.com/) with GT Titles for graphics output
- Visual Studio (or MSBuild) to build the solution

## At a glance

**1** operator · **2** live output channels (HTTP/TCP to vMix) · **24** GT title variants

## License

Copyright (C) 2026 Peter Aellig

This program is free software: you can redistribute it and/or modify it under the terms of the [GNU General Public License, version 3](LICENSE) as published by the Free Software Foundation. Anyone can use, study, share, and build on this project — as long as derivative works stay licensed under GPLv3 too.

---

*Peter Aellig*
