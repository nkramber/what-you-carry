# PR-40 response

Date: 2026-09-10

## Identity

- PR: 40
- Reviewed head: `2253e53`
- Branch: `feat/pr-58-night-gate`

## Push check

`git fetch` and `git pull --ff-only` brought the review commit `3e964a2` into the checkout, and `git status --short --branch` showed it level with the remote, so no push came first (F-59).

## P1-1: The workflow can use a `night.json` supplied by the pull request when the fetch fails

Disposition: full merit.

Evidence: the fetch step wrote `night.json` into the checkout on a successful fetch, and its `else` branch printed a line and went on. A pull request that carries a `night.json` at its root, at a fresh time and a commit on the base branch, then reached the command through that file whenever the fetch failed, which includes the absent branch of the bootstrap case and any network or authentication failure. The command read the file it was given. The review traced it, and the trace holds: the old command had no way to know where the file came from.

Correction: the fetch moves into the tool, and the workflow has no shell step. `NightGateFacts.Gather` takes the checkout, the remote name, the base ref, and the time. It runs `git ls-remote --exit-code --heads <remote> night-results`, and an exit of 2 is an absent branch. It then runs `git fetch --quiet <remote> night-results` and reads `night.json` from `FETCH_HEAD` through git, so the working tree of the checkout is never read. A branch with no such file is absent too, and the absent message names the reason. Any other git failure, such as a remote that does not exist or a fetch that fails, throws with the command, the exit code, and stderr (T-2). `GitRepository` gains `HasRemoteBranch` and `Fetch` for it. The workflow passes `--remote origin` and keeps the full history for the ancestry check. The parser also accepts a leading byte-order mark, because the two records on `night-results` carry one from the writer before its correction, and the tool reads them through git now and not through `File.ReadAllText`, which stripped it.

Regression check: `NightGateReadsTheRecordFromTheRemoteAndNeverFromTheCheckout` plants a fresh success record at the root of a checkout whose remote has no `night-results`, and asserts the absent case. It then publishes a branch with no `night.json` and asserts the absent case with the reason, publishes a failure record and asserts that the branch record decides and not the planted file, and asserts that a remote git cannot reach throws with its name. `NightGateCommandReportsEachExitCode` plants the same file and asserts exit 1 before a branch exists. The shape test asserts that the workflow holds no `git fetch` and no `night.json`. The old command took a file path and read it, so the planted file passed it, and the old workflow left the file in place on a failed fetch. `dotnet test WhatYouCarry.slnx --no-build`: 532 passed, 0 failed. The tool also ran against the real remote from this checkout: pass, at `a2799f2`, through the byte-order mark of the current record.

## New ids

F-93 records the finding. No new decision and no new question.

## Final head

The correction is the one commit above the review commit, and it holds the code, the tests, the workflow, this response, F-93, the roadmap notes, and the Session 102 handoff entry. It is the effective head.
