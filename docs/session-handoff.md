# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 163: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of the concurrency PR as PR #67, in the same invocation as Session 161 (D-297). Branch `docs/pr-67-merge-record`.

### What this session did, and why

- Session 162 approved `d774ab9` in `docs/reviews/pr-67.md` with no finding. The owner merged PR #67 as `10ba70c` at 20:58 UTC on 2026-09-14, and the tree of `10ba70c` equals the review tip `d8bc858`.
- `main` now holds the concurrency groups of D-356, the evidence rule of D-357, and the re-run rule of D-358. `docs/design.md` marks F-99 and F-100 done, and F-99 keeps the note that the groups do not stop a lost leg.
- The file held twelve entries with this one, so Sessions 153 and 152 moved to the archive.

### State of the build

- `main` is at `10ba70c`, the squash merge of PR #67. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-67-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `10ba70c`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 21:11 UTC. No macOS leg ended "not acquired".
- `ste-check`: 0 findings in 16 files. `dotnet test`: 875 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `10ba70c`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the new sizes, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-67, the dig restart, follows in a fresh session (D-121, D-353).

### Traps and gotchas

- The GitHub PR #67 is the concurrency PR. The roadmap id PR-67 is the dig restart.
- The merge marks use the UTC date of the merge, 2026-09-14. This entry uses the local date, also 2026-09-14.
- The review gate runs its workflow from `main` (D-197), so its concurrency group acts from this PR on. It runs on a push and on a label event, and a newer one cancels an older review gate run of that PR that is still in progress (D-356).
- A self-hosted leg can end "not acquired" (F-100). Re-run the failed jobs of that run (D-358). The runner log is under `/Volumes/SSD-1TB/actions-runner/_diag` on the Mac mini.
- The next ids are D-359, OQ-174, F-101, PR-68, and Session 164.

### Open questions that block progress

None blocks this PR. PR-67 asks the owner for the job budget and the count of digs before the code (D-353). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-67, and it asks the owner for the job budget and the count of digs first (D-353).

## Session 162: 2026-09-14, Codex

Author: Codex
Session: review PR #67, the workflow concurrency groups, at effective head `d774ab9`. Branch `chore/pr-run-concurrency`.

### What this session did, and why

- Checked the provider gate. Session 161 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, all ten workflows, affected tests and documents, roadmap, design, decisions, questions, and every PR comment.
- Verified the nine pull request workflows use the D-356 group, and `night.yml` has no group.
- Verified the group uses the PR number for pull request events and the commit for push events. The cancellation expression acts only on pull request events.
- Found no in-scope defect. The focused regression passes on the head and fails on `main` for `asset-qa.yml`, so it rejects the old workflow shape.
- Added `docs/reviews/pr-67.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `5b85b70`. The effective head is `d774ab9`. The later tip commit `65940c7` changes only metadata paths under D-184.
- `dotnet build` passed with 0 warnings and 0 errors. The focused workflow regression passed 2 tests. STE check, det-lint, asset QA, and bit identity passed.
- The local full test command did not emit a completion result because the test host did not complete in this execution context. Revision-matched CI on the effective code head passed CI, smoke, bit identity on all three platforms, asset QA, bots, det-lint, the night gate, and STE check.
- Gitar approved the head with no issue comment. The review gate was neutral or skipped before the review record existed, as D-251 expects.
- Remote head: `origin/chore/pr-run-concurrency`, verified with `gh pr view` after the review commit.

### In flight

PR #67 is ready for owner merge. The next session opens the dig restart item in the Phase 2 roadmap and asks for the job budget and the count of digs before code, as D-353 requires.

### Traps and gotchas

- The GitHub PR #67 is the concurrency PR. The roadmap item PR-67 is the dig restart.
- The effective head is `d774ab9`, not the later metadata tip `65940c7`.
- A local full test without a completion result is an execution-context limit. Do not report it as a passed local gate.
- The review gate becomes green after this review record reaches the PR head.

### Open questions that block progress

None blocks PR #67. OQ-173 is resolved by D-358. The dig restart still needs the owner choices recorded by D-353.

### Next concrete action

The owner merges PR #67. A fresh session starts the roadmap dig restart item and asks for its job budget and dig count before code.

## Session 161: 2026-09-14, Claude Code

Author: Claude Code
Session: open the concurrency groups of D-356 and the evidence rule of D-357 as PR #67, in the same invocation as Sessions 158 and 160 (D-355), and record the owner answer on a lost self-hosted leg, D-358. Branch `chore/pr-run-concurrency`.

### What this session did, and why

- The owner merged PR #66, the PR-63 merge record, as `5b85b70` at 19:21 UTC on 2026-09-14, and asked for the concurrency PR (D-355).
- The branch rebased onto `origin/main` with no conflict. The code commit `704ce9f` became `5381e3f`, and `git range-diff` shows the same change.
- The permission rules of the session refused an amend of the rebased commit, so the F-99 mark went into a second commit, `66ebefb`.
- A scratch worktree of `main` at `5b85b70` took the new `RepositoryShapeTests.cs`. There `EveryPullRequestWorkflowCancelsItsOlderRuns` fails on `asset-qa.yml`, and `TheNightNeverCancels` passes, so the new test fails on the old workflows (T-3).
- The push with `--force-with-lease` replaced `704ce9f` on the remote, and PR #67 opened at 19:33 UTC. The automated pass of gitar approved `66ebefb` at 19:36 UTC with no comment. Its comment shows the trial pause note, and the completed check run on that head made a `Gitar review` comment unnecessary.
- The macOS leg of Bit identity on `66ebefb` ended "not acquired" at 19:42 UTC with no other run in its group, and the compare job skipped. The runner log on the Mac mini shows `acquirejob` HTTP 409 conflicts and skipped job messages in that window and in the F-99 window. A re-run of the failed jobs at 19:45 UTC passed, compare included.
- The F-99 mark of `66ebefb` said corrected, and the lost leg showed that it overstated the change. The owner answered two questions on the recommendation. D-358: the author re-runs the failed jobs of a run with a lost self-hosted leg, and the re-run counts as CI for that head. F-99 goes back to 🔧, F-100 records the log evidence, and OQ-173 records the question.
- Commit `d774ab9` holds D-358, F-100, OQ-173, the F-99 mark, and the D-358 line in `CLAUDE.md`, `AGENTS.md`, and the pr-review skill.
- The pause note showed on `d774ab9`, and no automatic pass started in five minutes. The `Gitar review` comment at 20:05 UTC ran a pass, and its check run passed at 20:06 UTC. It approved with no comment, so 0 comments needed an answer (D-250, D-303).
- Session 151 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `5b85b70`. The effective head of PR #67 is `d774ab9`, the register commit above `66ebefb` and the code commit `5381e3f`. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/chore/pr-run-concurrency` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` on `d774ab9`: 875 tests, 0 failures, with the five Smoke tests on the local Godot build. `ste-check`: 0 findings in 16 files. A YAML parse of the ten workflows finds the group in the nine PR workflows and none in `night.yml`.
- CI on `66ebefb`: CI, smoke, and bit identity passed on the three platforms, bit identity on its second attempt. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed. `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251).
- CI on `d774ab9`: CI, smoke, and bit identity passed on the three platforms, bit identity on its first attempt. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last run at 20:10 UTC. `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the new sizes, and it can start hours late (F-95).

### In flight

PR #67: the Codex review per the `pr-review` skill at the effective head `d774ab9` (T-4, D-355). The owner then merges. PR-67 follows in a fresh session (D-121, D-353).

### Traps and gotchas

- The GitHub PR #67 is the concurrency PR. The roadmap id PR-67 is the dig restart (D-353).
- The effective head is `d774ab9`, and not `66ebefb` or `5381e3f`. The registers and the agent files are outside the metadata set (D-184).
- A macOS leg can end "not acquired" while the runner is online (F-100). Re-run the failed jobs of that run, and do not read the loss as a code failure (D-358).
- The Mac runner is the launchd service `actions.runner.nkramber-what-you-carry.mac-mini-m4` on the Mac mini. Its log is under `/Volumes/SSD-1TB/actions-runner/_diag`.
- Every run on `d774ab9` completed before the push of this entry, so that push cancels nothing. Under D-356, a later push to PR #67 cancels the runs of the earlier head that are still in progress. That is the change at work and not a failure, and D-357 makes CI on the tip the evidence.
- The review gate runs its workflow from the base branch (D-197). On this PR it runs with no group, and its group acts only after the merge.
- The Codex reviews of Sessions 153, 155, and 159 started in the Codex desktop app. The `codex` command on the command path is version 0.39.0, which is older than the app.
- The next ids are D-359, OQ-174, F-101, PR-68, and Session 162.

### Open questions that block progress

None blocks PR #67. D-358 resolves OQ-173. PR-67 asks the owner for the job budget and the count of digs before the code (D-353). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #67 per the `pr-review` skill at the effective head `d774ab9` and writes `docs/reviews/pr-67.md`. The owner then merges. A fresh session then opens PR-67, and it asks the owner for the job budget and the count of digs first (D-353).

## Session 160: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-63 as PR #65, the play test of the owner, D-354, and the owner answers on the concurrency groups of the workflows, D-355 to D-357, in the same invocation as Session 158 (D-297). Branch `docs/pr-63-merge-record`.

### What this session did, and why

- Session 159 approved `6f0d6f2` in `docs/reviews/pr-65.md` with no finding. The owner merged PR #65 as `002054a` at 18:10 UTC on 2026-09-14.
- The macOS leg of Bit identity on the review tip `d66fd24` never started. The self-hosted runner did not acquire the job in 20 minutes, and the compare job skipped. Four pushes in 15 minutes queued about 12 macOS jobs on the one Mac runner. The same code passed that leg on `6f0d6f2`, `e1d58db`, `1e85f94`, and `7028406`.
- The owner asked for a concurrency group in the workflows, so that a newer push cancels the older runs of a PR. The owner answered three questions. D-355 lets this run open that code PR as a one-time exception to D-121, after this PR merges. D-356 cancels older runs on PR events alone, keyed on the PR number, and the night never cancels. D-357 lets CI on the tip count for the effective head when every later commit is a metadata commit.
- The concurrency change is ready on the branch `chore/pr-run-concurrency`, commit `704ce9f` on `002054a`, pushed to origin with no PR. It holds the block in the nine PR workflows, the D-357 line in `CLAUDE.md`, `AGENTS.md`, and the pr-review skill, and two tests in `RepositoryShapeTests`. Locally it passed 875 tests with the five Smoke tests, `ste-check`, and a YAML parse of every workflow, and the new test fails on the workflows of `main`.
- The owner then asked that this PR carry every document that a fresh context needs, so it records D-355 to D-357 and F-99 ahead of the concurrency PR.
- The owner played floor 1 and confirms that the spaces no longer feel cramped. D-354 closes exit test 6 of PR-63.
- `docs/design.md` marks PR-63 merged in its entry and in sequence item 11, and it gains F-99. The Phase 2 roadmap gains the status line of PR-63 and the mark in sequence item 13.
- Session 150 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `002054a`, the squash merge of PR #65, and its tree equals the review tip `d66fd24`. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-63-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `002054a`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 18:25 UTC.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 873 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `002054a`.
- PR #66 on its first head `0b31f4e`: the automated pass approved with no comment, and every check passed, the review gate on the `review-override` label included. The register commit above it is newer than the label event and lies outside the metadata set, so the label came off and goes on again after the next pass (D-190).
- The concurrency branch: `origin/chore/pr-run-concurrency` at `704ce9f`, with no PR. No workflow runs on a push to a branch other than `main`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the new sizes, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on again after the last push and the automated pass (D-188, D-190). After this PR merges, the concurrency PR opens from `chore/pr-run-concurrency`, rebased onto `main`, with its own handoff entry, and it takes a Codex review (D-188, D-355). PR-67 follows in a fresh session (D-121, D-353).

### Traps and gotchas

- The GitHub PR #65 is PR-63. The roadmap id PR-65 is the ramp meshes in Game.
- About one floor in 96000 fails to dig on the new sizes (F-98). The floors of the first night on them dug with no error in the measurement of Session 158. A change that moves the floors of the night can fail a night before PR-67 lands (D-353).
- The merge marks use the UTC date of the merge, 2026-09-14. D-354 and this entry use the local date, also 2026-09-14.
- The concurrency commit `704ce9f` sits on `002054a`, and `main` gains this PR first. Rebase the branch onto `origin/main` before the PR opens. The rebase has no conflict, because this PR touches no file of that commit, and the push after it needs `--force-with-lease`.
- D-355 to D-357 and F-99 reach `main` with this PR, so the concurrency PR adds no decision row. It marks F-99 corrected in `docs/design.md`, its handoff entry is Session 161, and its description names the three decisions.
- Under D-356, the handoff push of the concurrency PR cancels the runs of its code commit. That is the change at work and not a failure, and D-357 makes CI on the tip the evidence.
- The review gate runs from the base branch (D-197), so its concurrency group starts to act only after the concurrency PR merges.
- The exact block in each of the nine PR workflows, before `jobs:`:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.event.pull_request.number || github.sha }}
  cancel-in-progress: ${{ github.event_name == 'pull_request' || github.event_name == 'pull_request_target' }}
```

- The next ids are D-358, OQ-173, F-100, PR-68, and Session 161.

### Open questions that block progress

None blocks this PR or the concurrency PR. PR-67 asks the owner for the job budget and the count of digs before the code (D-353). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. This run, or a fresh session, then opens the concurrency PR: check out `chore/pr-run-concurrency`, rebase it onto `origin/main`, run the build, `dotnet test`, and `ste-check`, push with `--force-with-lease`, open the PR, and add Session 161. The PR takes the automated pass and a Codex review. A fresh session then opens PR-67, and it asks the owner for the job budget and the count of digs first (D-353).

## Session 159: 2026-09-14, Codex

Author: Codex
Session: review PR #65, the dig sizes in the floor template, at effective head `6f0d6f2`. Branch `feat/pr-63-dig-sizes`.

### What this session did, and why

- Checked the provider gate. Session 158 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, affected callers and tests, roadmap, design, decisions, questions, and all PR comments.
- Found no in-scope defect. The template validates the six dig sizes, the plan uses the template sizes, the detail pass uses each tunnel height, and the tests check the changed contract.
- Added `docs/reviews/pr-65.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `d2ef347`. The effective head is `6f0d6f2`. The later handoff commit is metadata under D-184.
- Revision-matched CI passed on all three platforms for build-and-test, bit identity, and smoke. Asset QA, bots, det-lint, STE check, and the night gate passed. The compare job passed with `b00814dbf25e61e8`.
- The local test host could not bind its socket. This execution-context failure does not provide local test evidence. Remote CI provides revision-matched test evidence.
- The automated pass approved the head with no issue comment. The review gate was neutral before the review record existed, and `evaluate` failed for that expected reason.

### In flight

PR #65 is ready for owner merge after this review record reaches the branch. Exit test 6 still needs the owner play test of floor 1. PR-67 follows in a fresh session (D-121).

### Traps and gotchas

- The effective head is `6f0d6f2`, not the later metadata tip, under D-184.
- About one floor in 96000 reaches the dig job cap on these sizes (F-98). D-353 assigns the restart to PR-67.
- The owner play test remains open even though the automated checks pass.
- The next ids are D-354, OQ-173, F-99, PR-68, and Session 160.

### Open questions that block progress

None blocks PR #65. Exit test 6 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner plays floor 1 for exit test 6, then merges PR #65. A docs PR records the merge (D-297). A fresh session then opens PR-67.

## Session 158: 2026-09-14, Claude Code

Author: Claude Code
Session: open PR-63, the dig sizes in the floor template, as PR #65. Branch `feat/pr-63-dig-sizes`.

### What this session did, and why

- The owner merged PR #64, the record of D-341 to D-351, as `d2ef347`, and asked for PR-63. The branch came from `origin/main` at `d2ef347`.
- Before the code, the owner chose D-352 on the recommendation: the validator rejects an even tunnel width, a width past the rock shell, and a dig height that leaves fewer than three rows of the floor. The dig plan also stops on an even width in a template that no validator read.
- `FloorTemplate` gains the six dig sizes, and `DigPlan` reads them in place of five Core constants. Each walker carries the radius and the height of its tunnel, a collapse heap reaches the height of its own tunnel, and `FloorPlan` lists the tunnel stamps. The content takes D-341, D-343, and D-344. The simulation version is 8, and the bit-identity known answer moved from `2258ba8b9cc94b3f` to `b00814dbf25e61e8`.
- `TunnelCrossSection` checks each stamp against the width and the height of its template, and it keeps the D-166 window check on a mask in place of a hash set. `EveryBandHasOneFloorSize` replaces `FloorSizeGrowsWithDepth`. `ADigSizeOutsideItsBoundsIsAnError` covers each bound with its reason, and `AnEvenTunnelWidthStopsThePlan` covers the guard in the plan.
- A scratch program outside the repository dug the floors of the night: 175000 floors with 0 errors, so 5 to 9 rooms fit (D-344). The median need is 1 job, and the largest is 5890. 500000 more floors found 7 that ran the 10000 jobs of D-279 with a chamber still in rock (F-98). The session filed OQ-172, and the owner chose D-353 on the recommendation: PR-63 keeps the cap, and PR-67, a dig restart, comes right after it.
- `DigPlanJobCapTests` pins the three floors of the largest need, 5890, 5662, and 5568 jobs, in place of the F-92 floors of the old sizes.
- The design doc and the Phase 2 roadmap gain F-98 and the PR-67 entry, and the Phase 2 sequence puts PR-67 at item 14.
- The automated pass of gitar ran on `6f0d6f2` after the push. Its check run passed at 17:05 UTC, and it approved with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 148 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `d2ef347`. The effective head of PR #65 is `6f0d6f2`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-63-dig-sizes` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 873 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed.
- The PR bot sweep ran locally on the code: the random walker and the greedy descender over seeds 1 to 100, 0 softlocks and 0 crashes, and the descender reached the bottom on every seed.
- CI on `6f0d6f2`: build-and-test passed on the three platforms, the last at 17:15 UTC. Bit identity passed on the three platforms with the compare job, so `b00814dbf25e61e8` holds on each. Smoke passed on the three platforms, and asset-qa, bots, det-lint, the night gate, STE check, and gitar passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251).
- The scheduled night of 2026-09-14, run 34858986484 at `f487401`, passed at 15:58 UTC. The night gate reads it until 15:58 UTC on 2026-09-16.

### In flight

PR #65: the Codex review per the `pr-review` skill at the effective head `6f0d6f2`. Exit test 6 needs the owner to play floor 1 and confirm that the spaces no longer feel cramped, recorded as a decision. The owner then merges, and a docs PR records the merge (D-297). PR-67 follows in a fresh session (D-121).

### Traps and gotchas

- About one floor in 96000 fails to dig on these sizes (F-98). The night passes because its floors are fixed. A change that moves the floors of the night, such as PR-66, can fail a night before PR-67 lands (D-353).
- `DigPlanJobCapTests` digs the three heaviest floors twice each, so the class is the slowest of the procgen tests. `TheCapHoldsTheMeasuredTail` pins the three counts, and a change to the dig moves them.
- A tunnel width in a floor template is odd (D-352). Job 0 is the gallery, and `TunnelStamp.Gallery` reads it.
- The scratch measurement program is not in the repository. F-98 names the seven failed floors, and `FloorGenerator.Generate` on one of them gives the error again.
- zsh does not split a variable that holds a command and its arguments into words. Two command chains of this session failed on it, so write each command in full.
- PR-67 asks the owner for the job budget and the count of digs before the code (D-353).
- The next ids are D-354, OQ-173, F-99, PR-68, and Session 159.

### Open questions that block progress

None blocks PR #65, and exit test 6 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #65 per the `pr-review` skill at the effective head `6f0d6f2` and writes `docs/reviews/pr-65.md`. The owner plays floor 1 for exit test 6, then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-67.

## Session 157: 2026-09-14, Claude Code

Author: Claude Code
Session: record the owner answers on the dig sizes, the ramps, and the chamber tiers, D-341 to D-351, in the same invocation as Sessions 152, 154, and 156 (D-297). Branch `docs/dig-sizes-and-ramps`.

### What this session did, and why

- The owner merged PR #63, the PR-15 merge record, as `f487401` at 14:07 UTC on 2026-09-14.
- The owner then asked whether the tunnels of the demo have the real size of the first playable, because they feel very cramped, and whether ramps with different slopes can replace steps. The demo runs the real generator on floor 1 of seed 1: drifts of 3 by 3, a gallery of 5 by 3, chambers 3 to 4 high, and one-block steps that need a jump (D-165, D-253). F-97 records it.
- The owner answered eleven questions in three batches, D-341 to D-351: the wide sizes, the sizes in the floor template, one floor size and one room count on every band, ramps with true slopes of 1:2, 1:3, and 1:4, no one-block steps in tunnels, chamber tiers 2 blocks high by a chance per kind, and the dig work before PR-16.
- The preset named chamber boxes of 5 to 15 in all. D-341 gives each kind its old range times one and a half, rounded up. The owner can correct a range in the PR-63 session.
- D-351 splits the work into four roadmap items by concern (G-10): PR-63 the dig sizes, PR-64 the ramp cells in Core, PR-65 the ramp meshes in Game, and PR-66 the ramps and the tiers in the generator. The design doc and the Phase 2 roadmap carry the entries, and PR-16 reads ramps in its move rule and its first exit test.
- The Effect columns of D-78, D-165, D-252, D-253, and D-255 carry the marks of the parts that changed (D-186).
- Session 147 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `f487401`, the squash merge of PR #63. This branch holds one docs commit above it.
- Remote head: `origin/docs/dig-sizes-and-ramps` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 855 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `f487401`.
- The scheduled night of 2026-09-14 started at 14:57 UTC as run 34858986484 at `f487401`, 6 h 50 min after the cron, and ran still at 15:06 UTC. Until a night passes, the night gate reads run 34759337453 at `4a1048c`, which turns red at 14:13 UTC on 2026-09-15 (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-63 is next, and a fresh session opens it (D-121). OQ-9 can take its answer at any time before PR-16.

### Traps and gotchas

- The demo floor is the output of the real generator, so a change to a dig size moves the simulation version and the bit-identity known answer (G-20).
- `FloorSizeGrowsWithDepth` and `TunnelCrossSection` in `ProcgenTests.cs` hold D-252 and the old constants. PR-63 replaces the first and updates the second.
- No measurement shows that 5 to 9 rooms fit a floor of 64 by 64 with the chambers of D-341. PR-63 files a question when the seed sweep fails (D-344).
- A ramp needs a direction, a slope, and a place along the slope, and the block id of D-164 is one byte. The PR-64 session decides the encoding before the code.
- The owner answers the motion on a ramp in the PR-64 session, and the shape of a tier in the PR-66 session, before the code (D-345, D-350).
- The next ids are D-352, OQ-172, F-98, PR-67, and Session 158.

### Open questions that block progress

None blocks this PR or PR-63. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. A fresh session then opens PR-63 per the Phase 2 roadmap.

## Session 156: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-15 as PR #62 and the play test of the owner, D-340, in the same invocation as Sessions 152 and 154 (D-297). Branch `docs/pr-15-merge-record`.

### What this session did, and why

- Session 155 approved `38d1fac` in `docs/reviews/pr-62.md` with P2-1 fixed. The owner merged PR #62 as `3f3e8bf` at 03:16 UTC on 2026-09-14. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- No decision recorded exit test 7 before the merge. The owner played the sword, first asked whether the roll was in the game, found it on Left Control, and confirmed that the sword feels committed and readable. D-340 closes exit test 7.
- `docs/design.md` marks PR-15 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-15 and the mark in sequence item 11.
- Session 146 moved to the archive, because the file held eleven entries with this one. Session 155 moved Session 145 before this session.

### State of the build

- `main` is at `3f3e8bf`, the squash merge of PR #62. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-15-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 855 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `3f3e8bf`.
- The night gate reads the success of run 34759337453 at `4a1048c`, ended 14:13 UTC on 2026-09-13. It turns red at 14:13 UTC on 2026-09-15 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-14, and it can start hours late (F-95). At 13:51 UTC on 2026-09-14, no run of it had started.

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-16 is next, OQ-9 blocks it, and a fresh session opens it (D-121).

### Traps and gotchas

- The merge marks use the UTC date of the merge, 2026-09-14. D-340 and this entry use the local date, also 2026-09-14.
- The roll needs the ground, dry feet, and a ready cooldown, and its key is Left Control (D-289, D-329, D-337). A player who misses the key thinks the roll is absent, as the owner did first.
- The GitHub PR #62 is PR-15. The roadmap id PR-62 is the art quality pass after PR-20 (D-339).
- The next ids are D-341, OQ-172, F-97, PR-63, and Session 157.

### Open questions that block progress

None blocks this PR. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. The owner then answers OQ-9, the enemy families, and a fresh session opens PR-16 per the Phase 2 roadmap.

## Session 155: 2026-09-14, Codex

Author: Codex
Session: re-review PR #62 at effective head `38d1fac`.

### What this session did, and why

- Recomputed the effective head. The correction commit is `38d1fac`. Later review and handoff commits are metadata under D-184.
- Read the response file, the correction diff, the original trigger, the new regression tests, affected callers, and current PR comments.
- Verified that P2-1 is fixed. The focused Player, Animation, and Content suite passed 149 tests, including the traversal, separator, extension, and valid subdirectory cases.
- Updated `docs/reviews/pr-62.md` in place. P2-1 is `fixed in 38d1fac`, and the current verdict is `Ready for owner merge`.

### State of the build

- The revision-matched remote build, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar checks pass on `38d1fac`.
- Fresh `review-gate` and `evaluate` checks pass after the review update reaches the PR.
- Duplicate platform CI jobs remain in progress or queued after the metadata push. No pending job is reported as passed.
- The local full-suite run did not produce a final result in the execution window. Session 154 reports 855 tests passed with five Smoke tests, and remote CI provides revision-matched evidence.

### In flight

PR #62 is ready for owner merge after the remaining duplicate CI jobs finish. Exit test 7 remains the owner play test. A docs PR records the merge (D-297).

### Traps and gotchas

- The effective head is `38d1fac`, not the later metadata tip.
- Keep P2-1 and its original trigger in later review history.
- The art quality roadmap item also uses PR-62. It is not GitHub PR #62.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 156.

### Open questions that block progress

None blocks PR #62. Exit test 7 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the remaining duplicate CI jobs. The owner then plays the sword, merges PR #62, and records the merge in a docs PR.

## Session 154: 2026-09-13, Claude Code

Author: Claude Code
Session: answer the PR #62 review, P2-1. Branch `feat/pr-15-player-first-weapon`.

### What this session did, and why

- Read `docs/reviews/pr-62.md` at the reviewed head `6e35bc5`. P2-1 has full merit: `WeaponDefinition.AssetPath` checked the `models/` prefix alone, so a '..' segment and the extension of the other kind passed Core validation.
- Nine regression cases in `ContentTests.AWeaponOutsideItsBoundsIsAnError` failed against the validator of `6e35bc5` before the correction, with 16 other cases passed.
- `AssetPath` takes the extension of its kind, and it rejects a backslash, an empty, '.', or '..' segment, and a file name without the extension after a name. The extensions of a model and an animation moved into `ContentLoader`, and `AssetPaths` reads them. `AWeaponAssetPathUnderTheModelDirectoryLoads` keeps a path in a subdirectory of `models/` valid.
- `docs/reviews/pr-62-response.md` records the disposition, the evidence, and the checks.
- The review session left eleven entries in the file. Sessions 144 and 143 moved to the archive, so the file holds ten with this one.

### State of the build

- `main` is at `aeb91df`. The effective head is the correction commit above the review commits `37f519d` and `5b8f03a`, and it holds this entry, the response file, and the corrected files in one commit (D-182).
- Remote head: `origin/feat/pr-15-player-first-weapon` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 855 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files.
- The night gate reads the success of run 34759337453 at `4a1048c`, ended 14:13 UTC on 2026-09-13. It turns red at 14:13 UTC on 2026-09-15 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-14, and it can start hours late (F-95).

### In flight

PR #62: the automated pass of gitar on the correction head, then the repeat Codex review of P2-1 per the `pr-review` skill. The owner then plays the sword for exit test 7 and merges. A docs PR records the merge (D-297). The automated pass runs after the push, and the PR carries its result (D-250).

### Traps and gotchas

- The effective head is the correction commit, and not a later metadata commit (D-184).
- A weapon asset path names its kind by its extension: a model ends in `.bbmodel`, and a swing clip ends in `.json`. A path in a subdirectory of `models/` still loads.
- `review-gate` reads red until the repeat review approves the correction head (D-181).
- Exit test 7 of PR-15 is still open: the owner plays the sword and records the result as a decision before the merge.
- Session 153 reported a local test host that could not bind its socket. The full local suite ran in this session, and the state of the build gives its result.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 155.

### Open questions that block progress

None blocks PR #62. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews the correction of P2-1 per the repeat review procedure of the `pr-review` skill at the correction head and sets the verdict. The owner plays the sword for exit test 7, then merges, and a docs PR records the merge (D-297).
