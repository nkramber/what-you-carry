# Runbook: the macOS self-hosted runner

Status: procedure, written 2026-09-07 for the registration on 2026-09-08 (D-157, D-171). Written in ASD-STE100.

This runbook registers the Mac Mini as a self-hosted GitHub Actions runner for the repository `nkramber/roguelite-game`. The runner has the label `macos-arm64-self-hosted`. It runs as a launch agent. Its work directory is on the external SSD.

Facts checked on 2026-09-07:

- The machine is arm64 on macOS 26.5.2.
- The repository has zero runners.
- The `gh` command is logged in as the owner with the `repo` scope, which the registration token needs.
- No external volume is mounted yet.

## Before you start

1. Mount the SSD.
2. Note its volume name. The steps below use `/Volumes/Work` as the example. Replace it with the real name.
3. Open a terminal in any directory.

## Procedure

1. Make the two folders:

   ```sh
   mkdir -p /Volumes/Work/actions-runner /Volumes/Work/actions-work
   ```

2. Read the latest runner version:

   ```sh
   VER=$(gh api repos/actions/runner/releases/latest -q .tag_name | sed 's/^v//')
   echo "$VER"
   ```

3. Download and unpack the runner into the runner folder:

   ```sh
   cd /Volumes/Work/actions-runner
   curl -L -o runner.tar.gz "https://github.com/actions/runner/releases/download/v${VER}/actions-runner-osx-arm64-${VER}.tar.gz"
   tar xzf runner.tar.gz
   rm runner.tar.gz
   ```

4. Get a registration token. The token is valid for one hour.

   ```sh
   TOKEN=$(gh api -X POST repos/nkramber/roguelite-game/actions/runners/registration-token -q .token)
   ```

5. Configure the runner without prompts:

   ```sh
   ./config.sh --unattended \
     --url https://github.com/nkramber/roguelite-game \
     --token "$TOKEN" \
     --name mac-mini-m4 \
     --labels macos-arm64-self-hosted \
     --work /Volumes/Work/actions-work \
     --replace
   ```

6. Install the launch agent and start it:

   ```sh
   ./svc.sh install
   ./svc.sh start
   ```

7. Verify that the runner is online:

   ```sh
   gh api repos/nkramber/roguelite-game/actions/runners -q '.runners[] | {name, status, labels: [.labels[].name]}'
   ```

   The status must be `online`.

8. Record the volume name and the runner name in `docs/decisions.md` as the effect of D-171.

## Keep the runner available

The launch agent starts when the owner logs in. Two settings keep it available for the night jobs (D-115, D-117):

1. Open System Settings, then Users and Groups, and turn on automatic login for the owner account.
2. Open System Settings, then Energy, and turn off sleep while the machine is on power.

NOTE: A runner job runs with the owner's user permissions and shares the machine with the harness (D-105).

## Tools that the jobs need

PR-1 and PR-12 add jobs that need tools on this machine. Install them before those PRs:

1. The .NET 8 SDK (D-169).
2. The Godot 4.7.2 .NET editor binary (D-61), on the path.

## Remove or move the runner

1. Stop and uninstall the launch agent:

   ```sh
   cd /Volumes/Work/actions-runner
   ./svc.sh stop
   ./svc.sh uninstall
   ```

2. Get a removal token and remove the registration:

   ```sh
   TOKEN=$(gh api -X POST repos/nkramber/roguelite-game/actions/runners/remove-token -q .token)
   ./config.sh remove --token "$TOKEN"
   ```

3. Run the procedure again for a new location.
