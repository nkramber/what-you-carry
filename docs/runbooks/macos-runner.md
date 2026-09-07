# Runbook: the macOS self-hosted runner

Status: procedure, written 2026-09-07 for the registration on 2026-09-08 (D-157, D-171). Written in ASD-STE100.

Revised 2026-09-07: this runbook records the tool versions and the path risk of the launch agent (D-189).

Completed 2026-09-07: the SSD came one day early, and the registration ran on 2026-09-07 (D-192). The runner `mac-mini-m4` is online. A Full Disk Access grant was necessary (D-193, F-54).

This runbook registers the Mac Mini as a self-hosted GitHub Actions runner for the repository `nkramber/what-you-carry`. The runner has the label `macos-arm64-self-hosted`. It runs as a launch agent. Its work directory is on the external SSD.

Facts checked on 2026-09-07:

- The machine is arm64 on macOS 26.5.2.
- The repository has zero runners.
- The `gh` command is logged in as the owner with the `repo` scope, which the registration token needs.
- No external volume is mounted yet.
- The runner service script `runsvc.sh` reads a `.path` file and sets the path from it. Source: `actions/runner`, checked 2026-09-07.
- The `actions/setup-dotnet` action supports a self-hosted runner and reads `global.json`. Source: the action README, checked 2026-09-07.

## Before you start

1. Open Disk Utility and erase the SSD. Select the format APFS (Case-sensitive) and the scheme GUID Partition Map (D-191).
2. Do not encrypt the volume (D-191).
3. Give the volume the name `SSD-1TB`. Each command below then needs no change.
4. Mount the SSD.
5. Open a terminal in any directory.

NOTE: A case-sensitive volume makes the macOS CI leg agree with the Linux leg on file name case (D-135, D-191). The repository has no file name collision, and the checkout moves without a conflict.

## Procedure

1. Make the two folders:

   ```sh
   mkdir -p /Volumes/SSD-1TB/actions-runner /Volumes/SSD-1TB/actions-work
   ```

2. Read the latest runner version:

   ```sh
   VER=$(gh api repos/actions/runner/releases/latest -q .tag_name | sed 's/^v//')
   echo "$VER"
   ```

3. Download and unpack the runner into the runner folder:

   ```sh
   cd /Volumes/SSD-1TB/actions-runner
   curl -L -o runner.tar.gz "https://github.com/actions/runner/releases/download/v${VER}/actions-runner-osx-arm64-${VER}.tar.gz"
   tar xzf runner.tar.gz
   rm runner.tar.gz
   ```

4. Get a registration token. The token is valid for one hour.

   ```sh
   TOKEN=$(gh api -X POST repos/nkramber/what-you-carry/actions/runners/registration-token -q .token)
   ```

5. Configure the runner without prompts:

   ```sh
   ./config.sh --unattended \
     --url https://github.com/nkramber/what-you-carry \
     --token "$TOKEN" \
     --name mac-mini-m4 \
     --labels macos-arm64-self-hosted \
     --work /Volumes/SSD-1TB/actions-work \
     --replace
   ```

6. Grant Full Disk Access, before the service starts (D-193, F-54). macOS denies a launch agent every path on an external volume without this grant:

   1. Open System Settings, then Privacy and Security, then Full Disk Access.
   2. Click the add button, and press Command-Shift-G in the file selector.
   3. Add `/bin/bash`, and turn the switch on.
   4. Add `/Volumes/SSD-1TB/actions-runner/externals/node20/bin/node`, and turn the switch on.

   WARNING: This grant gives every bash process on the machine access to every file. A move of the runner to the internal drive removes the need for the grant.

7. Install the launch agent and start it:

   ```sh
   ./svc.sh install
   ./svc.sh start
   ```

8. Verify that the runner is online:

   ```sh
   gh api repos/nkramber/what-you-carry/actions/runners -q '.runners[] | {name, status, labels: [.labels[].name]}'
   ```

   The status must be `online`.

9. Record the volume name and the runner name in `docs/decisions.md` as the effect of D-171. D-192 holds the record of 2026-09-07.

## Keep the runner available

The launch agent starts when the owner logs in. Two settings keep it available for the night jobs (D-115, D-117):

1. Open System Settings, then Users and Groups, and turn on automatic login for the owner account.
2. Open System Settings, then Energy, and turn off sleep while the machine is on power.

### CAUTION: the volume must mount before a job starts

The work directory is on the external SSD. An unencrypted volume mounts at login, and the launch agent starts at the same time (D-191). Check the runner status after each restart of the machine. A job that starts without the volume fails, and the message names the work directory.

NOTE: A runner job runs with the owner's user permissions and shares the machine with the harness (D-105).

## Tools that the jobs need

PR-1 and PR-12 add jobs that need tools on this machine. This machine has both tools:

| Tool | Version | Location | Date | Decision |
|---|---|---|---|---|
| .NET SDK | 10.0.400 | `~/.dotnet` | 2026-09-07 | D-173 |
| Godot .NET editor | 4.7.2.stable.mono | `/Applications/Godot_mono.app` | 2026-09-07 | D-61 |

The Homebrew cask `dotnet-sdk` installs a package file, and that file needs an administrator password. The Microsoft script `dotnet-install.sh` put the SDK in `~/.dotnet`, and that script needs no password. The Homebrew cask `godot-mono` put the editor in `/Applications`. A wrapper is at `/opt/homebrew/bin/godot-mono`.

The file `~/.zshrc` sets `DOTNET_ROOT` and adds `~/.dotnet` to the path. This applies to a login shell only.

A test on 2026-09-07 made a class library and an xUnit project. The commands `dotnet build` and `dotnet test` were successful. The default target framework is `net10.0`.

### CAUTION: the launch agent does not read the login shell path

A launch agent starts with a minimal path. It does not read `~/.zshrc`. The path holds neither `~/.dotnet` nor `/opt/homebrew/bin`. A CI job that calls `dotnet` directly fails, and the message is `command not found`.

The workflow calls `actions/setup-dotnet` with the `global-json-file` input (D-189). The action installs the pinned SDK for each job. No job depends on the path of this machine, and all three platforms read one file.

NOTE: The runner also reads a `.path` file in its directory. D-189 does not use that file, because a version in the repository is easier to audit than a version on one machine.

## Remove or move the runner

1. Stop and uninstall the launch agent:

   ```sh
   cd /Volumes/SSD-1TB/actions-runner
   ./svc.sh stop
   ./svc.sh uninstall
   ```

2. Get a removal token and remove the registration:

   ```sh
   TOKEN=$(gh api -X POST repos/nkramber/what-you-carry/actions/runners/remove-token -q .token)
   ./config.sh remove --token "$TOKEN"
   ```

3. Run the procedure again for a new location.
