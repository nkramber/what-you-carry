#!/usr/bin/env bash
# The poll of the night fixer (D-643). launchd runs the copy of this file on origin/main every 15 minutes, on the Mac
# Mini of the owner. When the newest night on main failed, and no session handled it yet, the poll starts one Claude
# Code session in a new worktree from origin/main. The session follows docs/runbooks/night-fixer-prompt.md: it fixes the
# night, runs gitar and branch nights, then Codex rounds, and it sends a Pushover notice when the PR is ready to merge or
# when it stops. It never merges, and the owner merges (D-524).
#
# Usage: night-fixer.sh [--dry-run]
# --dry-run prints the decision and starts nothing.
# One session runs at a time: a lock directory holds the process id of the poll that runs the session. An open PR from
# a branch fix/night-* also blocks a new session, and the poll then notes the run in the file queued.
# A failed read exits 1 with its context (T-2). The variables WYC_FIXER_REPO and WYC_FIXER_STATE replace the checkout
# and the state directory, for the tests alone.
set -euo pipefail

dry=no
if [ "$#" -gt 1 ] || { [ "$#" -eq 1 ] && [ "$1" != "--dry-run" ]; }; then
  echo "Usage: night-fixer.sh [--dry-run]" >&2
  exit 2
fi
if [ "$#" -eq 1 ]; then
  dry=yes
fi

repo="nkramber/what-you-carry"
checkout="${WYC_FIXER_REPO:-/Volumes/SSD-1TB/what-you-carry}"
state="${WYC_FIXER_STATE:-${HOME}/Library/Application Support/wyc-night-fixer}"
mkdir -p "$state"
touch "$state/handled" "$state/queued"

# A notice to the owner goes through the workflow notify.yml, which reads the Pushover secrets (D-645).
notify() {
  gh workflow run notify.yml --repo "$repo" --ref main -f title="$1" -f message="$2" -f priority="1" -f url="${3:-}" || {
    echo "night-fixer: the notice '$1' did not start." >&2
    return 1
  }
}

lock="$state/lock"
if ! mkdir "$lock" 2>/dev/null; then
  holder=$(cat "$lock/pid" 2>/dev/null || true)
  if [ -n "$holder" ] && kill -0 "$holder" 2>/dev/null; then
    echo "night-fixer: the session of poll ${holder} runs, so this poll starts none."
    exit 0
  fi
  echo "night-fixer: the lock of poll '${holder}' stays after its end, and this poll removes it."
  rm -rf "$lock"
  mkdir "$lock"
fi
echo "$$" > "$lock/pid"
trap 'rm -rf "$lock"' EXIT

line=$(gh run list --repo "$repo" --workflow night.yml --branch main --limit 1 \
  --json databaseId,status,conclusion,headSha --jq '.[] | "\(.databaseId) \(.status) \(.conclusion) \(.headSha)"') || {
  echo "night-fixer: the read of the newest night on main failed." >&2
  exit 1
}
if [ -z "$line" ]; then
  echo "night-fixer: main has no night run."
  exit 0
fi
read -r run status conclusion sha <<< "$line"
if [ "$status" != "completed" ]; then
  echo "night-fixer: the night ${run} is ${status}."
  exit 0
fi
if [ "$conclusion" = "success" ]; then
  echo "night-fixer: the night ${run} passed."
  exit 0
fi
if grep -qx "$run" "$state/handled"; then
  echo "night-fixer: a session already took the night ${run}."
  exit 0
fi

open=$(gh pr list --repo "$repo" --state open --json headRefName --jq '[.[] | select(.headRefName | startswith("fix/night-"))] | length') || {
  echo "night-fixer: the read of the open PRs failed." >&2
  exit 1
}
if [ "$open" != "0" ]; then
  grep -qx "$run" "$state/queued" || echo "$run" >> "$state/queued"
  echo "night-fixer: a fix PR is open, so the night ${run} waits in the file queued."
  exit 0
fi

if [ "$dry" = "yes" ]; then
  echo "night-fixer: a session would start for the night ${run} at ${sha}."
  exit 0
fi

echo "$run" >> "$state/handled"
branch="fix/night-${run}"
work="${state}/work-${run}"
git -C "$checkout" fetch --quiet origin main
git -C "$checkout" worktree add --quiet -b "$branch" "$work" origin/main
prompt=$(sed -e "s/RUN_ID/${run}/g" -e "s/RUN_SHA/${sha}/g" -e "s#FIX_BRANCH#${branch}#g" "$work/docs/runbooks/night-fixer-prompt.md")
log="${state}/session-${run}.log"
echo "night-fixer: the session for the night ${run} starts in ${work}, and it logs to ${log}."
set +e
(cd "$work" && claude -p "$prompt" --dangerously-skip-permissions > "$log" 2>&1)
rc=$?
set -e
if [ "$rc" -ne 0 ]; then
  notify "What You Carry: the night fixer stopped" "The session for the night ${run} ended with exit code ${rc}. Its log is ${log} on the Mac Mini." \
    "https://github.com/${repo}/actions/runs/${run}"
  exit "$rc"
fi
echo "night-fixer: the session for the night ${run} ended."
