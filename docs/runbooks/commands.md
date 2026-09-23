# Runbook: the command rules

Status: reference, written 2026-09-23. Written in ASD-STE100. The byte ceiling of `AGENTS.md` moved these rules here (D-382).

`AGENTS.md` lists the commands of the checkout. This file holds the rules of two command groups. `AGENTS.md` names this file.

## The Game arguments

The Game layer checks the user arguments after `--` at boot. A bad argument ends the boot with exit code 1, and the error line names it (D-313, D-317). The contact sheet and the HUD shot take no other flag, `--smoke` and `--bot` exclude each other, and `--transitions` needs `--bot` and `--frame-log`.

## The generated files

`texture-gen` writes `content/textures/atlas.png` and `layout.json` from the palette, the recipes, and the paint files (D-305, D-505). Commit both after each change, because a test compares them with the output. `audio-synth` writes a WAV file next to each sound file under `content/audio/sfx/`, and a test compares each one too (D-453). `audio-analyze` writes the band levels of a reference into a spectral layer (D-464). The contact sheet needs a window, so it never runs in CI, and a headless run exits 1 (D-306).
