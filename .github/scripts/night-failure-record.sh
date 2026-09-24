#!/usr/bin/env bash
# Writes the failure record of a night that has no night-record binary, as after a broken build (D-273).
# Usage: night-failure-record.sh <commit> <record of main> <output>
# The record keeps the failed seeds of the record of main, so each later night still runs them until a night passes
# them (D-567). Without them, the next window can pass and end the block with no fix (PR #100 review P1-1). An
# unreadable record of main fails the script, and no record replaces it (T-2).
set -euo pipefail
if [ "$#" -ne 3 ]; then
  echo "Usage: night-failure-record.sh <commit> <record of main> <output>" >&2
  exit 2
fi
commit="$1"
main="$2"
output="$3"
ended="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
jq -c --arg commit "$commit" --arg ended "$ended" \
  '{commit: $commit, endedAt: $ended, status: "failure", failedSeeds: (.failedSeeds // {})}' "$main" > "$output"
echo "night-failure-record: ${output} holds commit ${commit} with status failure, and the failed seeds of ${main}."
