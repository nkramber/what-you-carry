#!/usr/bin/env bash
# Waits for the Gitar check run on the head of a PR after a push, and prints the final state (D-575).
# Usage: gitar-wait.sh <pr number>
# Run it at once after the push. It waits 60 seconds, then reads the Gitar check runs of the head every 30 seconds.
# When no check run shows up by 360 seconds, it posts one "Gitar review" comment and reads on. It exits 0 when each
# Gitar check run of the head completes. At 900 seconds it exits 1, and the session stops and tells the owner.
# A failed read exits 1 with its context (T-2). The variables GITAR_WAIT_FIRST, GITAR_WAIT_POLL, GITAR_WAIT_REQUEST,
# and GITAR_WAIT_LIMIT replace the four times, in seconds, for the tests alone.
set -euo pipefail
if [ "$#" -ne 1 ]; then
  echo "Usage: gitar-wait.sh <pr number>" >&2
  exit 2
fi
pr="$1"
first="${GITAR_WAIT_FIRST:-60}"
poll="${GITAR_WAIT_POLL:-30}"
request="${GITAR_WAIT_REQUEST:-360}"
limit="${GITAR_WAIT_LIMIT:-900}"
start=$(date +%s)

repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner) || {
  echo "gitar-wait: the read of the repository name failed." >&2
  exit 1
}
head=$(gh pr view "$pr" --json headRefOid --jq .headRefOid) || {
  echo "gitar-wait: the read of the head of PR #${pr} failed." >&2
  exit 1
}
requested=no
sleep "$first"

while true; do
  # One line for each Gitar check run of the head: the status, the conclusion, the start, and the end.
  runs=$(gh api --paginate "repos/${repo}/commits/${head}/check-runs?per_page=100" \
    --jq '.check_runs[] | select(.app.slug == "gitar-bot") | "\(.status) \(.conclusion) \(.started_at) \(.completed_at)"') || {
    echo "gitar-wait: the read of the check runs on ${head} of PR #${pr} failed." >&2
    exit 1
  }
  elapsed=$(( $(date +%s) - start ))

  if [ -n "$runs" ] && ! grep -qv '^completed ' <<< "$runs"; then
    echo "gitar-wait: each Gitar check run on ${head} of PR #${pr} completed after ${elapsed} s."
    printf '%s\n' "$runs"
    exit 0
  fi

  if [ -z "$runs" ] && [ "$requested" = no ] && [ "$elapsed" -ge "$request" ]; then
    gh pr comment "$pr" --body "Gitar review" > /dev/null || {
      echo "gitar-wait: the Gitar review comment on PR #${pr} failed." >&2
      exit 1
    }
    requested=yes
    echo "gitar-wait: no Gitar check run on ${head} after ${elapsed} s. Posted one Gitar review comment on PR #${pr}."
  fi

  if [ "$elapsed" -ge "$limit" ]; then
    echo "gitar-wait: no completed Gitar check run on ${head} of PR #${pr} after ${elapsed} s. Stop, and tell the owner (D-575)." >&2
    printf '%s\n' "${runs:-no Gitar check run}" >&2
    exit 1
  fi
  sleep "$poll"
done
