# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 168: 2026-09-14, Codex

Author: Codex
Session: review PR-64 as PR #71 at effective head `b5bf2de`. Branch `feat/pr-64-ramp-cells`.

### What this session did, and why

- Reviewed the complete code and test diff for the ramp cells in Core.
- Verified the provider gate. Session 167 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Checked the ramp ids, slope math, collision sweep, player motion, ray march, reachability search, simulation version, and bit-identity course against D-362 to D-367 and the PR-64 roadmap exit tests.
- Found no in-scope defect. Wrote `docs/reviews/pr-71.md` with the verdict `Ready for owner merge` for `b5bf2de`.
- Read the existing Gitar comment. It approved the head and raised no issue.

### State of the build

- `main` and the merge base are `e1076ca`. The effective head is `b5bf2de`. The later `d489060` commit changes only `docs/session-handoff.md` under D-184.
- The focused ramp suite passed 173 tests with 0 failures. A local full-suite and build attempt produced no completion result after more than one minute. Session 167 reports the full local gates as passed.
- Revision-matched CI on `b5bf2de` passed CI, smoke, bit identity on all three platforms, compare, bots, det-lint, asset QA, night gate, and STE check. Gitar approved with no issue comment.
- `evaluate` failed and `review-gate` skipped before this review record existed, as D-251 predicts. They must refresh after the review commit reaches the PR.

### In flight

PR #71 is ready for owner merge after the fresh review-gate check passes. The owner then merges, and a docs PR records the merge (D-297). PR-65 follows in a fresh session (D-121).

### Traps and gotchas

- The effective head is `b5bf2de`, not this metadata commit (D-184).
- The PR is GitHub #71 and roadmap PR-64. PR-65 is the ramp mesh work, and PR-66 is the ramp generator work.
- The local full-suite and build attempts produced no completion result. Do not report those local attempts as passed.
- The next ids are D-368, OQ-174, F-101, PR-68, and Session 169.

### Open questions that block progress

None blocks PR #71. The PR-65 session asks the owner for the tile of a ramp face before the code (D-367). Exit test 4 of PR-65 needs owner approval of a contact sheet with ramps. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the fresh review-gate check. The owner merges PR #71 after the check passes. A fresh session then opens PR-65 and asks the owner for the tile of a ramp face first (D-367).

## Session 167: 2026-09-14, Claude Code

Author: Claude Code
Session: open PR-64, the ramp cells in Core, as PR #71, with the owner answers D-362 to D-367. Branch `feat/pr-64-ramp-cells`.

### What this session did, and why

- The owner merged PR #70, the merge record of PR-67, as `e1076ca` at 00:19 UTC on 2026-09-15, and asked for the next item. The branch came from `origin/main` at `e1076ca`.
- Before the code, the owner answered six questions in two batches (D-345). D-362: the speed along the slope is the flat speed, and the recommendation was the flat horizontal speed. D-363: a walk and a sprint stay on the slope on the way down, and a roll leaves it, in the words of the owner. D-364: no slide. D-365: a jump from a ramp as from flat ground, which revises in part D-235, the ground probe only. D-366: a roll starts on a ramp and climbs it. D-367: the ramp ids 8 to 43 in the block byte.
- `Ramp` reads a ramp id. `VoxelGrid` takes the ramp ids, and `TryGetRamp` and `TryTopUnder` read them. The solid rule reads a ramp as solid.
- `SweptAabb` stands a box on the highest point of the slope under its footprint. It lifts a box onto a slope that rises under the end of a move along X or Z, by at most the rise of a slope of 1:2 over the move and with room above, and never onto a block (D-165). `PlayerBody` takes the slope factor of D-362, and `Move` takes `followSlope` for the walk down of D-363.
- `GridRay` meets the slope inside a ramp cell by the sign of the height over the slope at the entry and the exit of the cell. `Reachability.RampWalk` joins cells whose slopes meet on the shared face, a chain of ramps included, and `Landing` limits the drop and the step from a ramp.
- The simulation version is 10. The bit-identity sweep gains a ramp course along each rise with each run, and the known answer moves from `efcce6816cec980e` to `24c37100cd99edf4`. Before the ramp run and the version rise, the new code passed the suite with `efcce6816cec980e`, so no grid without a ramp moved.
- Tests: `RampTests`, `RampMotionTests`, `RampRayTests`, and `RampSearchTests`, on the course of `RampCourse`. The first form of `CameraNeverEntersARamp` read a boom hit on the side of a ramp as a hit on the slope and failed. The test now asserts the camera contract, and it counts the hits on a slope.
- The automated pass of gitar approved `b5bf2de` at 01:45 UTC with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 157 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `e1076ca`. The effective head of PR #71 is `b5bf2de`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-64-ramp-cells` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 1049 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `24c37100cd99edf4`.
- The PR bot sweep ran locally: the random walker and the greedy descender over seeds 1 to 100, 0 softlocks and 0 crashes, and the descender reached the bottom on every seed.
- CI on `b5bf2de`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last at 01:50 UTC. No macOS leg ended "not acquired". `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `b5bf2de` completed before the push of this entry, so that push cancels nothing (D-356).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC runs on `main` with the dig restart, and it can start hours late (F-95).

### In flight

PR #71: the Codex review per the `pr-review` skill at the effective head `b5bf2de` (T-4). The owner then merges, and a docs PR records the merge (D-297). PR-65 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #71 is PR-64. The roadmap id PR-65 is the ramp meshes in Game, and PR-66 digs the ramps.
- The effective head is `b5bf2de`, and not the metadata commit of this entry (D-184).
- `VoxelGrid.IsSolid` reads a ramp as solid. The mesher of PR-13 draws a ramp as a cube with the tile of its id until PR-65, and `DigCanvas.IsAir` reads a ramp as rock until PR-66.
- `GreedyDescender` jumps at each rise of one row on its path, and the walk onto the low end of a ramp is such a rise. The jump lands on the slope. PR-66 runs the bots on ramps.
- The lift onto a slope reads the footprint at the end of the whole move. A move that a block cuts short can leave the body up to half the move over the slope, and the next tick lands it.
- A stagger passes `followSlope` true and a roll passes false (D-363, D-364).
- D-110: the private helpers of `SweptAabb` call only public methods of `VoxelGrid` and `Ramp`. A helper that calls another helper breaks the rule.
- zsh reserves the variable name `status`, so a command chain that sets it stops with "read-only variable".
- The next ids are D-368, OQ-174, F-101, PR-68, and Session 168.

### Open questions that block progress

None blocks PR #71. The PR-65 session asks the owner for the tile of a ramp face before the code, because a ramp has no block kind of its own (D-367). Exit test 4 of PR-65 needs the owner approval of a contact sheet with the ramps. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #71 per the `pr-review` skill at the effective head `b5bf2de` and writes `docs/reviews/pr-71.md`. The owner then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-65, and it asks the owner for the tile of a ramp face first (D-367).

## Session 166: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-67 as PR #69, in the same invocation as Session 164 (D-297). Branch `docs/pr-67-dig-restart-merge-record`.

### What this session did, and why

- Session 165 approved `80ee5d9` in `docs/reviews/pr-69.md` with no finding. `review-gate` and `evaluate` passed on the tip `28a6705` at 22:55 UTC.
- The owner merged PR #69 as `f2a04e6` at 23:25 UTC on 2026-09-14, and the tree of `f2a04e6` equals the tip `28a6705`. The three commits after `80ee5d9`, `0e69c58`, `bc991cc`, and `28a6705`, change only metadata paths (D-184).
- `main` now holds the job budget of D-359, the four digs of D-360, and the new chamber draw of D-361. `docs/design.md` marks PR-67 and F-98 done, and sequence item 11 names the merge. The Phase 2 roadmap gains the status line of PR-67 and the mark in sequence item 14.
- Session 165 left ten entries in the file, so Session 156 moved to the archive with this one.

### State of the build

- `main` is at `f2a04e6`, the squash merge of PR #69. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-67-dig-restart-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `f2a04e6`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 23:39 UTC. No macOS leg ended "not acquired".
- `dotnet build`: 0 warnings, 0 errors on `f2a04e6`. `dotnet test`: 877 tests, 0 failures, with the five Smoke tests on the local Godot build. `ste-check`: 0 findings in 16 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `f2a04e6`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the dig restart, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-64, the ramp cells in Core, follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #69 is PR-67. The GitHub PR #67 was the concurrency PR, and its merge record used the branch `docs/pr-67-merge-record`.
- The merge marks use the UTC date of the merge, 2026-09-14. This entry uses the local date, also 2026-09-14.
- The Codex review of Session 165 ran in this checkout, and its local build, full test, and STE check gave no completion result there. This session rebuilt `f2a04e6` before its checks.
- The simulation version is 9, so a run record of version 8 fails with the notice of D-151 (D-260).
- The bit-identity known answer is `efcce6816cec980e`. A version rise alone moves it, because the replay header holds the version.
- The next ids are D-362, OQ-174, F-101, PR-68, and Session 167.

### Open questions that block progress

None blocks this PR. The PR-64 session asks the owner for the motion on a ramp before the code: the speed on a slope, the jump, the roll, and the stagger. It also decides the encoding of a ramp cell (D-345). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-64, and it asks the owner for the motion on a ramp first (D-345).

## Session 165: 2026-09-14, Codex

Author: Codex
Session: review PR #69, the dig restart, at effective head `80ee5d9`. Branch `feat/pr-67-dig-restart`.

### What this session did, and why

- Read the PR description, complete diff, affected Core callers, tests, roadmap, design, decisions, questions, prior records, and every PR comment.
- Checked the provider gate. Session 164 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Verified the restart state machine. Each failed dig uses the next draws of the same Procgen stream and a new empty grid, and the first complete plan supplies the shafts and detail.
- The focused restart suite passed 4 tests. The tests cover a complete first dig, budget exhaustion, retry output, and the four-dig contextual error.
- Found no in-scope defect. Wrote `docs/reviews/pr-69.md` with the verdict `Ready for owner merge` for `80ee5d9`.

### State of the build

- `main` and the merge base are `d65823c`. The effective head is `80ee5d9`. The later tips `0e69c58` and `bc991cc` change only metadata paths under D-184.
- Remote head: `origin/feat/pr-67-dig-restart` at `bc991cc`, verified with `gh pr view`. The branch has no ahead count.
- The focused restart tests passed 4 tests. A local full-suite, build, and STE-check attempt produced no completion result. The review records those attempts as unverified.
- Revision-matched CI on `80ee5d9` passed CI, smoke, bit identity on all three platforms, compare, bots, det-lint, asset QA, night gate, and STE check. Gitar approved with no issue comment.
- `evaluate` failed and `review-gate` was neutral before this review record existed, as D-251 predicts. They must refresh after the review commit reaches the PR.

### In flight

PR #69 is ready for owner merge after the fresh required checks pass. The owner then merges, and a docs PR records the merge (D-297). PR-64 follows in a fresh session.

### Traps and gotchas

- The GitHub PR is #69, but the roadmap item is PR-67.
- The effective head is `80ee5d9`, not the metadata tips `0e69c58` and `bc991cc` (D-184).
- The first `git fetch origin` after the review failed because the checkout could not open `.git/FETCH_HEAD`. The initial fetch and PR read succeeded before the failure.
- The local test host first failed with a socket permission error. The elevated focused run passed. The elevated full suite produced no completion result.
- The next ids are D-362, OQ-174, F-101, PR-68, and Session 166.

### Open questions that block progress

None blocks PR #69. PR-64 asks the owner for the motion on a ramp before the code. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the fresh required checks. Verify that `review-gate` passes for effective head `80ee5d9`.

## Session 164: 2026-09-14, Claude Code

Author: Claude Code
Session: open PR-67, the dig restart, as PR #69, with the owner answers D-359 to D-361. Branch `feat/pr-67-dig-restart`.

### What this session did, and why

- The owner merged PR #68, the merge record of the concurrency PR, as `d65823c` at 21:27 UTC on 2026-09-14, and asked for the next item. The branch came from `origin/main` at `d65823c`.
- Before the questions, a scratch program outside the repository dug the 675000 floors of F-98 with a copy of the Core of `main`. The same 7 floors ran the 10000 jobs. The median need is 1 job, one floor in 5500 needs more than 1000, and the largest need that passed is 9952. A replay of each floor over 300 jobs, with a restart at budgets from 500 to 10000, dug every floor on its second dig.
- The owner chose three answers on the recommendation. D-359 sets a job budget of 1000 and supersedes the cap of D-279. D-360 gives a floor 4 digs before the error. D-361 draws the chamber kinds again on each restart.
- `DigPlan.TryDigUntilComplete` replaces `DigUntilComplete` and `MaxJobs`. `FloorGenerator.DigChambers` digs again on an empty grid from the next draws, up to `FloorGenerator.MaxDigs`, and the error names the digs, the budget, and the chambers. The simulation version is 9 (D-260).
- The bit-identity known answer moved from `b00814dbf25e61e8` to `efcce6816cec980e`. The replay header holds the version, and this head with the version set back to 8 prints `b00814dbf25e61e8`, so the version alone moved it.
- `DigRestartTests` replaces `DigPlanJobCapTests`. On a scratch worktree of `main`, the generator fails on all 7 floors of F-98 with "The dig plan ran 10000 jobs" (T-3).
- D-279 carries `Superseded by D-359`, so each other line that cites D-279 names D-359: F-92, F-98, the PR-63 entry, OQ-147, OQ-172, D-353, and the two roadmaps (D-178).
- The automated pass of gitar approved `80ee5d9` at 22:17 UTC with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 154 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `d65823c`. The effective head of PR #69 is `80ee5d9`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-67-dig-restart` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 877 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `efcce6816cec980e`.
- The scratch sweep of this head over the 675000 floors of F-98: 0 generator errors, 674877 floors on the first dig and 123 on the second, and the largest total job count is 1019, on seed 517241 floor 12.
- The PR bot sweep ran locally: the random walker and the greedy descender over seeds 1 to 100, 0 softlocks and 0 crashes, and the descender reached the bottom on every seed.
- CI on `80ee5d9`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last at 22:28 UTC. No macOS leg ended "not acquired". `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `80ee5d9` completed before the push of this entry, so that push cancels nothing (D-356).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC runs on `main` with the cap of D-279, which D-359 supersedes after the merge, and it can start hours late (F-95).

### In flight

PR #69: the Codex review per the `pr-review` skill at the effective head `80ee5d9` (T-4). The owner then merges, and a docs PR records the merge (D-297). PR-64 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #69 is PR-67. The GitHub PR #67 was the concurrency PR.
- The effective head is `80ee5d9`, and not the metadata commit of this entry (D-184).
- The bit-identity known answer moves with the simulation version alone, because the replay header holds the version. A first reading in this session said that the answer stays, and the tests refuted it.
- A floor that needed 1001 to 10000 jobs digs another floor now, so a run record of version 8 fails with the notice of D-151.
- `EveryTailFloorDigs` and `TheBudgetEndsTheFirstDigOfAHeavyFloor` pin dig 2 on ten floors. A change to the dig can move a floor to another dig, and the assert names the seed and the floor.
- The scratch programs are not in the repository. D-359 to D-361 and the PR description hold the measurement.
- The next ids are D-362, OQ-174, F-101, PR-68, and Session 165.

### Open questions that block progress

None blocks PR #69. The PR-64 session asks the owner for the motion on a ramp before the code: the speed on a slope, the jump, the roll, and the stagger. It also decides the encoding of a ramp cell (D-345). PR-64 has no measurement or hardware exit test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #69 per the `pr-review` skill at the effective head `80ee5d9` and writes `docs/reviews/pr-69.md`. The owner then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-64, and it asks the owner for the motion on a ramp first (D-345).

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
