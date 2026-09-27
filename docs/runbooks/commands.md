# Runbook: the command rules

Status: reference, written 2026-09-23. Written in ASD-STE100. The byte ceiling of `AGENTS.md` moved these rules here (D-382).

`AGENTS.md` lists the commands of the checkout. This file holds the rules of three command groups. `AGENTS.md` names this file.

## The Game arguments

The Game layer checks the user arguments after `--` at boot. A bad argument ends the boot with exit code 1, and the error line names it (D-313, D-317). The contact sheet and the HUD shot take no other flag, `--smoke` and `--bot` exclude each other, and `--transitions` needs `--bot` and `--frame-log`. A Game flag before `--` also ends the boot with exit code 1, because the engine ignores it there (D-624).

A close of the window ends the session like the test exit: one end line, the frame log, and exit code 0 when the log holds no error (F-161).

## The generated files

`texture-gen` writes `content/textures/atlas.png` and `layout.json` from the palette, the recipes, and the paint files (D-305, D-505). Commit both after each change, because a test compares them with the output. `texture-trace --spec <asset>` writes a recipe with a texel map for each face of `content/textures/traces/<asset>.json`, from the unlit screenshots of the reference folder (D-612). It keeps each recipe that exists, so delete a recipe to trace its face again. `audio-synth` writes a WAV file next to each sound file under `content/audio/sfx/`, and a test compares each one too (D-453). `audio-analyze` writes the band levels of a reference into a spectral layer (D-464). The contact sheet needs a window, so it never runs in CI, and a headless run exits 1 (D-306).

## The Deck runs

The agent runs each Deck test over SSH, after the owner says that the Deck is ready (D-606). The checkout, the SDK, and the Linux Godot .NET binary of D-294 are in the home directory of the user `deck`.

1. Put `~/.dotnet` first on `PATH`, and set `DOTNET_ROOT` to it.
2. Set `DISPLAY=:0`, and set `XAUTHORITY` to the file `/run/user/1000/xauth_*`, the auth file of Xwayland.
3. Build with the binary and `--headless --editor --build-solutions --quit`.
4. Run the command of `AGENTS.md` under `timeout`, then copy the frame log to the Mac.

Without `XAUTHORITY`, X11 refuses the SSH session, and Godot falls back to Wayland. Godot 4.7.2 can then hang after the end line of the session (F-147). A player session on the Deck has the auth file, so Godot uses X11 there.

## The night notice

A failed night on `main` sends a Pushover notification to the owner (D-642). The job `notify` of `night.yml` reads the repository secrets `PUSHOVER_USER_KEY` and `PUSHOVER_API_TOKEN`. No file holds either value. To change a value, run `gh secret set PUSHOVER_API_TOKEN --repo nkramber/what-you-carry`, and paste the value at the prompt. When the job goes red, the log shows the HTTP answer of Pushover.

