# Runbook: the night fixer

Status: reference, written 2026-09-26. Written in ASD-STE100. It holds the install, the logs, the stop, and the removal of the night fixer of D-643.

## What it does

A launchd job on the Mac Mini of the owner runs every 15 minutes. It fetches `origin/main`, copies the folder `.github/scripts` of `origin/main`, and runs the copy of `night-fixer.sh`. So the job runs the reviewed scripts of `main` alone, and it does nothing before the merge of PR #109. The poll sends each notice with `notify-owner.sh` from the same copy. That script waits for the run of `notify.yml`, and a notice counts only when that run succeeds.

When the newest night on `main` failed, and no session took that night, the script starts one Claude Code session. The session runs in a new worktree on the branch `fix/night-<run>`, with the prompt `docs/runbooks/night-fixer-prompt.md`. It skips each permission prompt, because the owner chose that (D-643).

The session fixes the night, runs the gitar pass and branch nights, and then runs Codex review rounds. It sends a Pushover notice when the PR is ready to merge, and when it stops (D-645). It never merges. The owner merges (D-524).

## Files on the Mac

| Path | What it holds |
|---|---|
| `~/Library/LaunchAgents/com.whatyoucarry.night-fixer.plist` | The launchd job |
| `~/Library/Application Support/wyc-night-fixer/` | The copy of the script, the lock, the files `handled` and `queued`, each worktree, and each session log |
| `~/Library/Logs/wyc-night-fixer.log` | The output of each poll |

## Procedure: install

1. Write the plist below to `~/Library/LaunchAgents/com.whatyoucarry.night-fixer.plist`.
2. Check it: `plutil -lint ~/Library/LaunchAgents/com.whatyoucarry.night-fixer.plist`.
3. Load it: `launchctl bootstrap gui/501 ~/Library/LaunchAgents/com.whatyoucarry.night-fixer.plist`.
4. Check the job: `launchctl print gui/501/com.whatyoucarry.night-fixer`.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>com.whatyoucarry.night-fixer</string>
  <key>ProgramArguments</key>
  <array>
    <string>/bin/bash</string>
    <string>-c</string>
    <string>set -euo pipefail; state="$HOME/Library/Application Support/wyc-night-fixer"; mkdir -p "$state"; cd /Volumes/SSD-1TB/what-you-carry; git fetch --quiet origin main; rm -rf "$state/scripts"; mkdir -p "$state/scripts"; git archive origin/main .github/scripts | tar -x -C "$state/scripts"; if [ ! -f "$state/scripts/.github/scripts/night-fixer.sh" ]; then echo "night-fixer: origin/main holds no poll script yet."; exit 0; fi; exec /bin/bash "$state/scripts/.github/scripts/night-fixer.sh"</string>
  </array>
  <key>EnvironmentVariables</key>
  <dict>
    <key>PATH</key>
    <string>/Users/nate/.local/bin:/Users/nate/.dotnet:/Users/nate/.nvm/versions/node/v20.17.0/bin:/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin</string>
    <key>DOTNET_ROOT</key>
    <string>/Users/nate/.dotnet</string>
  </dict>
  <key>StartInterval</key>
  <integer>900</integer>
  <key>RunAtLoad</key>
  <false/>
  <key>StandardOutPath</key>
  <string>/Users/nate/Library/Logs/wyc-night-fixer.log</string>
  <key>StandardErrorPath</key>
  <string>/Users/nate/Library/Logs/wyc-night-fixer.log</string>
</dict>
</plist>
```

## Procedure: stop a session

1. Read the process id in `~/Library/Application Support/wyc-night-fixer/lock/pid`.
2. Stop that process and its session with `pkill -P <pid>`, then `kill <pid>`.
3. The next poll removes the lock. The night of that session stays in the file `handled`, so no new session starts for it.

## Procedure: remove

1. Unload the job: `launchctl bootout gui/501/com.whatyoucarry.night-fixer`.
2. Delete the plist file.
3. Delete `~/Library/Application Support/wyc-night-fixer/` after you read the logs that you want to keep.

## Test the poll

Run `bash .github/scripts/night-fixer.sh --dry-run` from the checkout. It names the decision for the newest night on `main`, and it starts nothing. The tests of `NightFixerTests` run the same mode with a fake `gh`.
