# What You Carry

What You Carry is a solo third-person dungeon crawler for Steam. It is in development, and this build is not a release.

## Launch the game

### What you need

- The .NET SDK 10.0.400. The file `global.json` pins that version, and the build accepts no other version.
- Godot 4.7.2 in the .NET edition. The standard edition of Godot cannot run C#.
- A copy of this repository.

The Godot download is the release [4.7.2-stable](https://github.com/godotengine/godot/releases/tag/4.7.2-stable) on GitHub. Each platform has one .NET zip file:

| Platform | Zip file | Program in the zip file |
|---|---|---|
| macOS | `Godot_v4.7.2-stable_mono_macos.universal.zip` | `Godot_mono.app` |
| Windows | `Godot_v4.7.2-stable_mono_win64.zip` | `Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe` |
| Linux | `Godot_v4.7.2-stable_mono_linux_x86_64.zip` | `Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64` |

On macOS, move `Godot_mono.app` to `/Applications`. The commands in this file use that path.

### Procedure

1. Open a terminal in the root directory of the repository.
2. Build the solution: `dotnet build WhatYouCarry.slnx`
3. Start the game: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game`

On Windows and on Linux, write the program path from the table in place of the macOS path.

The first build restores packages from NuGet, so it needs a network connection. The Godot editor is not necessary (D-63).

### In the game

The window opens in borderless fullscreen at the resolution of the display (D-310), and the game captures the mouse. To end the session, push Escape, or Start on a controller (D-311).

To play in a window, put the engine flag `--windowed` before `--path`:

`/Applications/Godot_mono.app/Contents/MacOS/Godot --windowed --path WhatYouCarry.Game`

The controls come from D-289:

| Action | Keyboard and mouse | Controller |
|---|---|---|
| Move | W, A, S, D | Left stick |
| Look | Mouse | Right stick |
| Jump | Space | A |
| Sprint | Left Shift | Left bumper |
| Dodge | Left Control | B |
| Attack | Left mouse button | Right trigger |
| Interact | E | X |
| End the session | Escape | Start |

Attack swings the sword. Dodge rolls 3 meters, on the ground and out of water. The roll has a cooldown of 45 ticks, which is 0.75 seconds.

This build has no enemies, no HUD, and no sound. Those arrive later in Phase 2 of `docs/roadmaps/phase-2-first-playable.md`.

### When the game does not start

- The game reads the words after `--` as its own flags. An unknown word stops the boot with exit code 1, and the error line names the word (D-313).
- An engine flag, such as `--windowed`, goes before `--`.
- The game reads the `content` directory next to `WhatYouCarry.Game`, so start it from a complete copy of the repository.

## Other sessions

`CLAUDE.md` gives the commands of the smoke session, the bot session, the test exit, and the contact sheet.
