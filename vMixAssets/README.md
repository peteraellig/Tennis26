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

That's it — Tennis26 and vMix both expect their files under `C:\vmix\tennis\`, so once everything is in place it should run out of the box.
