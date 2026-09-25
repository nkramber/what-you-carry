#!/usr/bin/env bash
# Gathers the results of the sweep jobs of a night into the two files that night-record reads, and prints the status
# of the night (D-572, PR-85).
# Usage: night-gather.sh <results directory> <output directory> <job result>...
# The results directory holds one directory for each sweep job that uploaded its result, and each holds bot-deaths.txt
# and seed-failures.txt. A sweep that did not end leaves no failure line, so the record keeps the failed seeds of the
# record of main for it (D-567). The status is success when each job result is success, cancelled when one is
# cancelled, and failure otherwise, so a skipped or unknown result never reads success (T-2).
set -euo pipefail
if [ "$#" -lt 3 ]; then
  echo "Usage: night-gather.sh <results directory> <output directory> <job result>..." >&2
  exit 2
fi
results="$1"
output="$2"
shift 2
if [ ! -d "$results" ]; then
  echo "night-gather: the results directory ${results} is absent." >&2
  exit 2
fi
mkdir -p "$output"
: > "${output}/bot-deaths.txt"
: > "${output}/seed-failures.txt"
shopt -s nullglob
gathered=0
for sweep in "${results}"/*/; do
  cat "${sweep}bot-deaths.txt" >> "${output}/bot-deaths.txt"
  cat "${sweep}seed-failures.txt" >> "${output}/seed-failures.txt"
  gathered=$((gathered + 1))
done
echo "night-gather: ${gathered} sweep results in ${results}, and the job results $*." >&2
status="success"
for result in "$@"; do
  if [ "$result" = "cancelled" ]; then
    status="cancelled"
    break
  fi
  if [ "$result" != "success" ]; then
    status="failure"
  fi
done
echo "$status"
