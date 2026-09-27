#!/usr/bin/env bash
# Sends one Pushover notice to the owner through the workflow notify.yml, and waits for its delivery (D-645).
# Usage: notify-owner.sh <title> <message> [url]
# A dispatch only puts a run in the queue, so the script gives the run a unique id, finds that run, and waits for it.
# It exits 0 only when the run succeeds, and the Pushover action fails that run on a failed send (D-642). Each failure
# exits 1 with its context (T-2). The variables WYC_NOTIFY_TRIES and WYC_NOTIFY_PAUSE replace the count and the pause of
# the search for the run, for the tests alone.
set -euo pipefail
if [ "$#" -lt 2 ] || [ "$#" -gt 3 ]; then
  echo "Usage: notify-owner.sh <title> <message> [url]" >&2
  exit 2
fi
title="$1"
message="$2"
url="${3:-}"
repo="nkramber/what-you-carry"
tries="${WYC_NOTIFY_TRIES:-20}"
pause="${WYC_NOTIFY_PAUSE:-6}"
id="$(date +%s)-$$-${RANDOM}"

gh workflow run notify.yml --repo "$repo" --ref main -f title="$title" -f message="$message" -f priority=1 -f url="$url" -f id="$id" || {
  echo "notify-owner: the dispatch of the notice '${title}' failed." >&2
  exit 1
}

run=""
for _ in $(seq 1 "$tries"); do
  run=$(gh run list --repo "$repo" --workflow notify.yml --event workflow_dispatch --limit 20 \
    --json databaseId,displayTitle --jq ".[] | select(.displayTitle | endswith(\"${id}\")) | .databaseId") || run=""
  run="${run%%$'\n'*}"
  if [ -n "$run" ]; then
    break
  fi
  sleep "$pause"
done
if [ -z "$run" ]; then
  echo "notify-owner: no run of notify.yml carries the id ${id}, so the notice '${title}' has no proof of delivery." >&2
  exit 1
fi

gh run watch "$run" --repo "$repo" --exit-status > /dev/null || {
  echo "notify-owner: the run ${run} of notify.yml failed, so the notice '${title}' did not reach the owner." >&2
  exit 1
}
echo "notify-owner: the run ${run} delivered the notice '${title}'."
