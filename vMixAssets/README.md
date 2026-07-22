# vMix Assets

Everything Tennis26 and vMix need at runtime: GT Title templates, graphics, flags, the vMix project, runtime data, and the ClickOnce installer.

## Structure

```
vMixAssets/
├── data/                 Runtime data: player database, pairings, settings, title board texts, save games
├── flags/                Country flag images (used by the titles and the live view)
├── graphical_templates/  Exported graphics behind the titles
├── images/                Additional background images
├── setup/                 ClickOnce installer for Tennis26 (setup.exe, Tennis26.application)
├── titles/                GT Title templates (.gtzip) for vMix
└── vMix_project/          The vMix project file (tennis24.vmix)
```

## Setup on a new machine

1. Copy the entire contents of this `vMixAssets/` folder to `C:\vmix\tennis\` (so that, for example, `vMixAssets/titles/` becomes `C:\vmix\tennis\titles\`, and so on for every subfolder).
2. Run `setup/setup.exe` (or open `setup/Tennis26.application`) to install Tennis26.
3. Open `vMix_project/tennis24.vmix` in vMix and load the GT Title templates from `C:\vmix\tennis\titles\` if they aren't already linked.

**`C:\vmix\tennis\` is not a suggestion — it's hardcoded into the Tennis26 source itself, with no setting to change it.** The exact paths the app reads and writes are:

| Path | Used for |
|---|---|
| `C:\vmix\tennis\data\tennisdata.xml` | Player database |
| `C:\vmix\tennis\data\pairings.xml` | Prepared match pairings |
| `C:\vmix\tennis\data\titleboards.xml` | Title board texts |
| `C:\vmix\tennis\data\tennis24_live.json` | Live match state (read by [`LiveServer`](../LiveServer)) |
| `C:\vmix\tennis\data\backup\` | Automatic player database backups |
| `C:\vmix\tennis\flags\` | Country flag images |
| `C:\vmix\tennis\graphical_templates\` | Scorebug background images — **see note below** |
| `C:\vmix\tennis\setup\` | ClickOnce publish output |

That's it — Tennis26 and vMix both expect their files under `C:\vmix\tennis\`, so once everything is in place it should run out of the box.

**Known bug:** `Tennis26_Scorer.vb` (`Hidepoints()`/`Showpoints()`) actually looks for the scorebug background images under `C:\vmix\tennis\graphical templates\` — with a **space**, not the underscore this folder actually uses. Since no folder with that exact (space) name exists, those two image sends currently resolve to nothing. Not fixed here — ask if you'd like it corrected in the source.
