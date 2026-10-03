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

On macOS, move `Godot_mono.app` to `/Applications`. On Windows and on the Steam Deck, the launch script finds Godot at the path of the next sections.

### Procedure on the Mac

1. Open a terminal in the root directory of the repository.
2. Build and start the game: `make play`

`make windowed` starts the game in a window. Without `make`, build with `dotnet build WhatYouCarry.slnx`, then start the game:

`/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game`

The Mac commands start the checkout as it is. They do not update it, because the work sessions use the same checkout.

### Procedure on Windows

1. Install Git and the .NET SDK 10.0.400.
2. Clone the repository: `git clone https://github.com/nkramber/what-you-carry.git`
3. In PowerShell, go to the root directory of the repository.
4. Run the install one time: `powershell -ExecutionPolicy Bypass -File launch\what-you-carry.ps1 -Install`
5. Open a new PowerShell window.
6. Start the game from any directory: `what-you-carry`

The install downloads the Windows zip of Godot 4.7.2 to `godot` in your home directory, and it checks the SHA-512 of the zip. It then writes `what-you-carry.cmd` to the directory `%LOCALAPPDATA%\Microsoft\WindowsApps`, which is on the command path of each new window. The command needs no change to the execution policy.

### Procedure on the Steam Deck

Do these steps one time, in Desktop Mode, in a Konsole window.

1. Clone the repository to `~/what-you-carry`.
2. Install the .NET SDK 10.0.400 in `~/.dotnet` with the `dotnet-install.sh` script of Microsoft.
3. Unzip the Linux zip of Godot to `~/godot`.
4. Run the install: `bash ~/what-you-carry/launch/what-you-carry.sh --install`

The install adds the command `~/.local/bin/what-you-carry`, and the shortcut "What You Carry" on the desktop. Open the shortcut to start the game. When a step fails, the terminal stays open and shows the error.

### Each start of the launch script

The command `what-you-carry`, on Windows and on the Deck, does these steps:

1. It updates the checkout to the newest `main`.
2. It builds the solution.
3. It runs the Godot import and the C# build.
4. It starts the game.

The update stops before it changes the checkout when a tracked file has a change, so it never discards work. To start the checkout as it is, add `-NoUpdate` on Windows or `--no-update` on the Deck. The environment variable `WYC_GODOT` names another Godot binary.

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

This build has the enemies, the Overseer, the HUD, and the sounds of Phase 2. The rest of Phase 2 is in `docs/roadmaps/phase-2-first-playable.md`.

### When the game does not start

- The game reads the words after `--` as its own flags. An unknown word stops the boot with exit code 1, and the error line names the word (D-313).
- An engine flag, such as `--windowed`, goes before `--`.
- The game reads the `content` directory next to `WhatYouCarry.Game`, so start it from a complete copy of the repository.

## Other sessions

`CLAUDE.md` gives the commands of the smoke session, the bot session, the test exit, and the contact sheet.
