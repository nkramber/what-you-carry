# Runbook: the night fixer

Status: reference, written 2026-09-26. Written in ASD-STE100. It holds the install, the logs, the stop, and the removal of the night fixer of D-643.

## What it does

A launchd job on the Mac Mini of the owner runs every 15 minutes. It fetches `origin/main`, copies the folder `.github/scripts` of `origin/main`, and runs the copy of `night-fixer.sh`. So the job runs the reviewed scripts of `main` alone, and it does nothing before the merge of PR #109. The poll sends each notice with `notify-owner.sh` from the same copy. That script waits for the run of `notify.yml`, and a notice counts only when that run succeeds.

When the newest night on `main` failed, and no session took that night, the script starts one Claude Code session. The session runs in a new worktree on the branch `fix/night-<run>`, with the prompt `docs/runbooks/night-fixer-prompt.md`. It skips each permission prompt, because the owner chose that (D-643).

The session has no background tasks, and a command in the foreground can run 6 hours (D-769). A reply of the session ends it, so a background command stops with that reply. The session id is `00000000-0000-4000-8000-` and the run id with zeros in front to 12 digits.

The session writes one line to the end mark after its last notice. When the session exits 0 with no end mark, the poll resumes the same session up to 3 times. Then it sends the notice "the night fixer stopped" and exits 1 (D-769). A session that exits with an error sends that notice with no resume (D-645).

The session fixes the night, runs the gitar pass and branch nights, and then runs Codex review rounds. It sends a Pushover notice when the PR is ready to merge, and when it stops (D-645). It never merges. The owner merges (D-524).

## Files on the Mac

| Path | What it holds |
|---|---|
| `~/Library/LaunchAgents/com.whatyoucarry.night-fixer.plist` | The launchd job |
| `~/Library/Application Support/wyc-night-fixer/` | The copy of the script, the lock, the files `handled` and `queued`, each worktree, each session log, and each end mark `end-<run>` |
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

1. Read the process id: `readlink ~/Library/Application\ Support/wyc-night-fixer/lock`.
2. Stop the poll first with `kill <pid>`, so it starts no resume.
3. Stop the session with `pkill -f '<session id>'`. The poll log names the session id.
4. The next poll removes the lock. The night of that session stays in the file `handled`, so no new session starts for it.

## Procedure: continue a session by hand

1. Go to the worktree `~/Library/Application Support/wyc-night-fixer/work-<run>`.
2. Run `claude --resume <session id>`. The poll log names the session id at the start of the session.

## Procedure: remove

1. Unload the job: `launchctl bootout gui/501/com.whatyoucarry.night-fixer`.
2. Delete the plist file.
3. Delete `~/Library/Application Support/wyc-night-fixer/` after you read the logs that you want to keep.

## Test the poll

Run `bash .github/scripts/night-fixer.sh --dry-run` from the checkout. It names the decision for the newest night on `main`, and it starts nothing. The tests of `NightFixerTests` run the same mode with a fake `gh`.
