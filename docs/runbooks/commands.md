# Runbook: the command rules

Status: reference, written 2026-09-23. Written in ASD-STE100. The byte ceiling of `AGENTS.md` moved these rules here (D-382).

`AGENTS.md` lists the commands of the checkout. This file holds the rules of three command groups. `AGENTS.md` names this file.

## The Game arguments

The Game layer checks the user arguments after `--` at boot. A bad argument ends the boot with exit code 1, and the error line names it (D-313, D-317). The contact sheet and the HUD shot take no other flag, and `--smoke` and `--bot` exclude each other. `--transitions` needs `--bot` and `--frame-log`, and `--seed` and `--frame-shots` need `--bot`. A Game flag before `--` also ends the boot with exit code 1, because the engine ignores it there (D-624). `--armor <set>` starts the run in the four pieces of one armor set: `leathers`, `brigandine`, or `blast` (D-762). A set that the content does not hold ends the boot with exit code 1, and the error names the absent item.

A close of the window ends the session like the test exit: one end line, the frame log, and exit code 0 when the log holds no error (F-161).

## The Godot sessions

`AGENTS.md` names this section. Each command needs a window, and `Godot` is not on the command path of the Mac, so each command uses the full path.

- Bot session with a frame log, for M-3: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --bot --frame-log frames.txt`
- Bot session on one seed of the M-3 table, seed 2 here (D-704, D-706): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --bot --seed 2 --frame-log frames.txt`
- Transition test, PR-18 exit test 6 on the Deck (D-428, D-435): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --bot --frame-log frames.txt --transitions 10`
- Policy session, the frame cost of a timer expiry with the enemies (D-646, RR-P3-16): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --bot --policy timer-tester --frame-log frames.txt`
- Contact sheet, a local run. It shows the body in each armor set too: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --contact-sheet sheet.png`
- Play session in one armor set (D-762): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --armor blast`
- HUD shot, the Deck frame of the HUD fixture (D-133): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --hud-shot hud.png`
- Tier 4 capture of the full-clearer on seed 1 (D-711, D-714): `/Applications/Godot_mono.app/Contents/MacOS/Godot --windowed --fixed-fps 60 --path WhatYouCarry.Game -- --bot --policy full-clearer --seed 1 --frame-shots shots/full-clearer`
- Tier 4 capture of the timer-tester on seed 1 (D-711, D-714): `/Applications/Godot_mono.app/Contents/MacOS/Godot --windowed --fixed-fps 60 --path WhatYouCarry.Game -- --bot --policy timer-tester --seed 1 --frame-shots shots/timer-tester`

The policy flag names one bot policy of Core: `random-walker`, `greedy-descender`, `full-clearer`, `timer-tester`, or `coward`. Without the flag, the bot session drives the greedy descender. The flag needs `--bot`, and the transition test takes no policy, because it loads no enemy (D-437). The end line of a session with a timer expiry holds `expiries` and `expiryMicrosMax`, the slowest frame near each expiry.

The seed flag sets the seed of the bot session: a whole number from 1 to 18446744073709551615, in digits alone (D-705). Without the flag, the session runs seed 1, the first seed of `Main`. The flag needs `--bot`, and the end line of the session names the seed.

The frame shots flag writes one frame of 1280 by 800 pixels, with the HUD, to its directory each second of game time (D-711). The file name holds the floor and the tick, for example `floor-01-tick-000060.png`. The flag needs `--bot`, and the capture needs a window, so a headless run exits 1. The engine flag `--fixed-fps 60` gives one render frame to each tick, so each frame shows the tick before its shot. The interpolation fraction of each frame is 0, so a shot draws the pose of a tick, and never a frame between two ticks (D-733). The end line of the session gives the count of shots in `frameShots`.

## The generated files

`texture-gen` writes `content/textures/atlas.png` and `layout.json` from the palette, the recipes, and the paint files (D-305, D-505). Commit both after each change, because a test compares them with the output. `texture-trace --spec <asset>` writes a recipe with a texel map for each face of `content/textures/traces/<asset>.json`, from the unlit screenshots of the reference folder (D-612). It keeps each recipe that exists, so delete a recipe to trace its face again. `audio-synth` writes a WAV file next to each sound file under `content/audio/sfx/`, and a test compares each one too (D-453). `audio-analyze` writes the band levels of a reference into a spectral layer (D-464). The contact sheet needs a window, so it never runs in CI, and a headless run exits 1 (D-306).

## The Deck runs

The agent runs each Deck test over SSH, after the owner says that the Deck is ready (D-606). The checkout, the SDK, and the Linux Godot .NET binary of D-294 are in the home directory of the user `deck`.

1. Put `~/.dotnet` first on `PATH`, and set `DOTNET_ROOT` to it.
2. Set `DISPLAY=:0`, and set `XAUTHORITY` to the file `/run/user/1000/xauth_*`, the auth file of Xwayland.
3. Build with the binary and `--headless --editor --build-solutions --quit`.
4. Run the command of the section "The Godot sessions" under `timeout`, with the path of the Deck binary. Then copy the frame log to the Mac.

Without `XAUTHORITY`, X11 refuses the SSH session, and Godot falls back to Wayland. Godot 4.7.2 can then hang after the end line of the session (F-147). A player session on the Deck has the auth file, so Godot uses X11 there.

The play shortcut of the README uses the same checkout, and each start checks out `main`. A test run checks out its own commit before step 3. Leave no tracked change after the run, or the next start of the shortcut stops.

The build of step 3 is the Debug configuration, and it compiles optimized code (D-685). A frame log before PR-77 measured code with no optimization. In desktop mode over SSH, a frame of 18 to 19 ms comes every 2.245 seconds, also in an empty scene (F-190). A frame log reads those frames as the stall of the desktop, and not as a cost of the game (D-683).

The swap line of a descent holds `fromWorker`, `digging`, `stagedTrees`, and `builtTrees`. When the loop took the plan of the worker, `digging` reads false and `builtTrees` reads 0 (F-193).

A timing patch for a Deck trace stays out of the branch (G-17). It keeps its numbers in fixed arrays, and it writes them some frames after the frames that it measures. A patch that writes a line or makes a string in each frame moves the collector to other frames. The first write to the console also cost 3.3 milliseconds in its frame (F-193).

## The night notice

A failed night on `main` sends a Pushover notification to the owner (D-642). The job `notify` of `night.yml` reads the repository secrets `PUSHOVER_USER_KEY` and `PUSHOVER_API_TOKEN`. No file holds either value. To change a value, run `gh secret set PUSHOVER_API_TOKEN --repo nkramber/what-you-carry`, and paste the value at the prompt. When the job goes red, the log shows the HTTP answer of Pushover.

