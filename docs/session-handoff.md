# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 174: 2026-09-16, Claude Code

Author: Claude Code
Session: finish PR-68, the shaft landing fix of D-370, and open it as PR #75, with the owner answer D-372. Branch `feat/pr-68-shaft-landing`.

### What this session did, and why

- Session 173 left the fix in the working tree with no commit, no push, and no PR. This session ran the gates, corrected one stale test, committed, pushed, and opened the PR.
- The full suite found a failure that Session 173 never ran. `SimulationTests.TheConstantsHold` pins the simulation version, and it still read 10. The assert now reads 11, and its remark names the pillar rule of F-101 in place of the dig restart of PR-67, which PR-64 already left stale.
- The owner answered the night gate of this PR. D-372: PR-68 merges although the `night-gate` job is red, as D-371 merged the PR-65 merge record. OQ-175 records the question.
- The `night-gate` job cannot go green on this PR. D-275 fails a record whose commit is not an ancestor of the base branch, so a hand night on this branch reads as foreign. `main` holds the defect, and seed 79146 is deterministic, so every night on `main` fails until this fix merges. The two rules make a deadlock, and D-372 breaks it.
- The PR-68 gate line of the Phase 2 roadmap names the red `night-gate` and D-372.
- The automated pass of gitar approved `2d4be05` with one finding of the Quality kind (D-250). `CheckShaftLandings` threw on two conditions and named one cause, so a landing that a pillar or a heap of rubble takes away read as an unreachable cell. Each condition now has its own message and a `cause` key (D-113, T-2). A second pass, which the comment `Gitar review` started, approved `cb3c2e2` with that finding closed and no new one (D-303).
- A hand run of `night.yml` on this branch passed at `cb3c2e2` and gives the CI evidence of exit test 2: run 35067529373, 1 hour 18 minutes (D-370). The two bot sets of 5000 seeds and the sweep of 100000 seeds all passed. The record on `night-results` now reads `cb3c2e2` with the status success, and the `night-gate` job stays red, because that commit is not on `main` (D-275). The first dispatch, run 35066777588, stood at `2d4be05`. A cancel of it before its start left the record untouched.
- Session 164 and Session 163 moved to the archive, because the file held twelve entries with this one.

### State of the build

- `main` is at `7345c9c`. The effective head of PR #75 is `cb3c2e2`, the answer to the automated pass. `2d4be05` below it holds the fix and the registers. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-68-shaft-landing` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 1116 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 36 files. `asset-qa`: 0 findings, 2 models, 0 overlays, 3 animations. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `a0b32bad006b3dfe`.
- The night sweep of 100000 seeds passed on this head in a local run of Session 173: 2 tests and 0 failures in 43 minutes. That is the local evidence of exit test 2.
- The bot sweep of seeds 1 to 100 passed on both policies: 0 softlocks and 0 crashes.
- CI on `cb3c2e2`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, and STE check passed, and the automated pass of gitar approved the head. No macOS leg ended "not acquired". `night-gate` fails, and D-372 carries the merge. `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `cb3c2e2` completed before the push of this entry, so that push cancels nothing (D-356).

### In flight

PR #75: the Codex review per the `pr-review` skill at the effective head `cb3c2e2` (T-4). The owner then merges with the red `night-gate` (D-372), and a docs PR records the merge (D-297). PR-66 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #75 is PR-68. The roadmap id PR-66 digs the ramps and the tiers, and it comes next.
- The effective head is `cb3c2e2`, and not the metadata commit of this entry (D-184). The register lines of D-372 and OQ-175 are outside the metadata set, so they sit in the code commit.
- `night-gate` is red on this PR by design, and D-372 carries the merge. A reviewer reads that line as an answer and not as a miss.
- The hand night replaced the one record on `night-results`. It now reads `cb3c2e2` with the status success, in place of the `4bc8cd4` failure. The gate stays red for every PR either way, because the new record commit is not on `main`.
- The simulation version is 11, and the bit-identity known answer is `a0b32bad006b3dfe`. A test that pins the version by a literal breaks on the next rise. `SimulationTests.TheConstantsHold` is the one such test.
- `IsUnderShaft` reads every shaft of the plan and not the shafts over the chamber alone. A column under any shaft loses its pillar, which costs a few pillars and keeps the rule simple (T-1).
- Rubble is solid, so a collapse in a shaft column can fill a landing by the same path as a pillar. The sweep of 100000 seeds found no such floor, and `CheckShaftLandings` now makes any such floor a loud error (T-2).
- The PR sweep reads 5000 seeds and never reads seed 79146, so a PR run passes while a night fails.
- The next ids are D-373, OQ-176, F-102, PR-69, and Session 175.

### Open questions that block progress

None blocks PR #75. D-370 carries the fix, and D-372 carries the merge. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #75 per the `pr-review` skill at the effective head `cb3c2e2` and writes `docs/reviews/pr-75.md`. The owner then merges with the red `night-gate` (D-372), and a docs PR records the merge (D-297). A fresh session then opens PR-66, and it asks the owner for the shape of a tier first (D-350).

## Session 173: 2026-09-16, Claude Code

Author: Claude Code
Session: open PR-68, the shaft landing fix, with the owner answer D-370. Branch `feat/pr-68-shaft-landing`. The owner reset the context in the middle of the work, so this entry hands over a working tree with no commit, no push, and no PR.

### What this session did, and why

- The branch came from `origin/main` at `7345c9c`, the merge of PR #74.
- A scratch dump outside the repository named the cause of F-101. Seed 79146, floor 7 holds chamber 1 with its floor at row 9 and chamber 3 under it with its floor at row 3. `DigPlan.TryDigShaft` proves two air rows over the landing of each of the nine columns of a hole, and it then carves the shaft at the column (25, 48). The detail pass runs after it, and `DetailPass.RaisePillars` raised a pillar of chamber 3 at the floor cell (25, 3, 48), the column of that shaft. The pillar filled the rows 4 to 8, so its top took the landing air row 8.
- The landing became the top of a pillar, one cell wide, five rows over the chamber floor. A body drops onto it and cannot climb back, so the search of PR-9 reaches no path to it. Nothing excluded a shaft column from a pillar.
- The fix: `DetailPass.IsUnderShaft` answers whether a shaft drops through a column, and `RaisePillars` skips such a column, beside the anchor of a chamber. The check comes after the draw, as the anchor check does, so the draw order does not change.
- `FloorGenerator.CheckShaftLandings` confirms that the spawn reaches the landing of every shaft, and the error names the chamber, the column, and the row (D-112, T-2). The class remark names the new confirmation.
- The simulation version rose to 11, and `bit-identity` prints `a0b32bad006b3dfe` (D-260, G-20). `BitIdentityTests.ExpectedHash` holds the new answer, and its remark names the move.
- `ProcgenTests.ShaftOfSeed79146LandsOnAReachableFloor` is the regression test of exit test 1. On `4bc8cd4` with that test, it fails with "The shaft at Column { X = 25, Z = 48 } lands at row 8, and the spawn does not reach it". It passes with the fix.
- `docs/design.md` names the cause in the F-101 row.
- The night sweep of 100000 seeds passed on this head, with the variable `WYC_NIGHT_SWEEP=1`: `EveryChamberReachable` and `DetailKeepsEveryChamberReachable`, 2 tests and 0 failures, in 43 minutes. That is the local evidence of exit test 2.

### State of the build

- `main` is at `7345c9c`. The branch `feat/pr-68-shaft-landing` stands on it with no commit. Nothing is pushed, and no PR exists.
- The working tree holds six modified files, and no untracked file: `WhatYouCarry.Core/Procgen/DetailPass.cs`, `WhatYouCarry.Core/Procgen/FloorGenerator.cs`, `WhatYouCarry.Core/Simulation/SimulationVersion.cs`, `WhatYouCarry.Tests/BitIdentityTests.cs`, `WhatYouCarry.Tests/ProcgenTests.cs`, and `docs/design.md`.
- `dotnet build`: 0 warnings, 0 errors. The focused run of `ShaftOfSeed79146LandsOnAReachableFloor`, `EveryChamberReachable`, and `DetailKeepsEveryChamberReachable` passed 3 tests with the PR sweep of 5000 seeds.
- The bot sweep of seeds 1 to 100 passed: the random walker ended by budget with 0 softlocks and 0 crashes, and the greedy descender reached the bottom on all 100.
- `bit-identity` prints `a0b32bad006b3dfe`. The full suite, `det-lint`, `asset-qa`, `ste-check`, and the Godot build check did not run yet on this head.
- The night sweep of 100000 seeds passed on this head: 2 tests and 0 failures in 43 minutes. No floor of those seeds holds an unreachable shaft landing, and none holds an unreachable chamber floor cell. The generator threw on no floor of the sweep.
- The record on the branch `night-results` holds `4bc8cd4` with the status failure, so the `night-gate` job fails on every PR until a night passes (D-115, D-177).

### In flight

PR-68 is unfinished. No commit exists. The night sweep of exit test 2 passed on this head already. The next session runs the other gates, commits, pushes, opens the PR, dispatches a night on the branch, and hands the PR to a Codex review (T-4).

### Traps and gotchas

- This file holds eleven entries with this one. Move Session 163 to the archive with the commit of the next entry (D-146).
- The scratch files of the old session stand outside the repository, in `/private/tmp/claude-501/-Volumes-SSD-1TB-what-you-carry/dc03dda0-997a-4585-8aa6-7c7379eff1a1/scratchpad/`. `pr68-body.md` is a draft PR description with the placeholders `{NIGHT_SWEEP}`, `{SWEEPS}`, and `{LOCAL_CHECKS}`. `detail-dump.txt` and `shaft-dump.txt` hold the diagnosis. A new session reads them by that absolute path, or writes the description again from this entry.
- A scratch test in `WhatYouCarry.Tests/` never reaches a commit. Delete it, check `git status`, and build again before any commit.
- The PR sweep reads 5000 seeds and never reads seed 79146, so a PR run passes while a night fails. The night sweep needs the variable `WYC_NIGHT_SWEEP=1`, and it takes about 35 minutes.
- `night.yml` takes a manual event, so a night runs on the branch before the merge (D-370).
- The version rise alone moves the bit-identity answer, because the replay header holds the version. The old session did not measure the answer with the version held at 10, so it does not know whether a floor of the sweep also moved.
- The generator now throws on an unreachable shaft landing. A floor with another cause of such a landing becomes a loud error, and the night sweep names its seed.
- The next ids are D-372, OQ-175, F-102, PR-69, and Session 174.

### Open questions that block progress

None blocks PR-68. D-370 carries the fix. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Finish PR-68 in this order.

1. Read this entry and `git status`. The six files above hold the whole change, and the night sweep of exit test 2 passed on them.
2. Run the gates: `dotnet build`, `dotnet test WhatYouCarry.slnx --no-build`, `det-lint`, `asset-qa`, `ste-check`, and the Godot build check. The night sweep needs no second local run.
3. Add the Session 174 entry, move Session 163 to the archive, and commit the six files with the entry.
4. Push, open the PR, and answer the automated pass (D-250).
5. Dispatch a night on the branch with `gh workflow run night.yml --ref feat/pr-68-shaft-landing`, and wait for a success record (D-115, D-370). The `night-gate` job stays red until a night passes.
6. Hand the PR to a Codex review at the effective head (T-4).

## Session 172: 2026-09-15, Claude Code

Author: Claude Code
Session: record the merge of PR-65 as PR #73, the night failure of F-101, and the owner answers D-370 and D-371, in the same invocation as Session 170 (D-297). Branch `docs/pr-65-merge-record`.

### What this session did, and why

- Session 171 approved `720c7a9` in `docs/reviews/pr-73.md` with no finding. The owner merged PR #73 as `4bc8cd4` at 05:45 UTC on 2026-09-15, and the tree of `4bc8cd4` equals the tip `5062ea2`. The three commits after `720c7a9` change only metadata paths (D-184).
- `main` now holds the ramp meshes of D-368 and the contact sheet of D-369. `docs/design.md` marks PR-65 done, and sequence item 11 names the merge. The Phase 2 roadmap gains the status line of PR-65 and the mark in sequence item 16.
- The night of 2026-09-15 at `4bc8cd4` failed. `EveryChamberReachable` and `DetailKeepsEveryChamberReachable` report seed 79146, floor 7: the shaft at column (25, 48) lands on an unreachable floor at row 8. The bot sweeps of that night passed, and the dig reported no error.
- A scratch test outside the repository dug that seed and floor at four revisions. At `4bc8cd4`, at `e1076ca`, and at `d65823c` the grid hash is `ff14f981092fdf5a`, and the landing is unreachable. At `d2ef347`, before PR-63, the grid is 72 by 16 by 72 with no shaft. The dig sizes of PR-63 make this floor, and PR-64, PR-65, and PR-67 did not.
- The last green night, at `f487401` on 2026-09-14, ran before PR-63 merged, so the night of 2026-09-15 is the first night on the wide sizes. The PR sweep of 5000 seeds never reads seed 79146.
- The owner answered two questions. D-370: PR-68, a fix PR of its own, comes before PR-66, and seed 79146 becomes a regression test. D-371: this merge record merges at once, although the `night-gate` job is red.
- F-101 records the night failure, and OQ-174 records the question. The Phase 2 roadmap gains the PR-68 entry, the F-101 row, and the sequence item 17, and `docs/design.md` gains the PR-68 entry.
- Session 162 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `4bc8cd4`, the squash merge of PR #73. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-65-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `4bc8cd4`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, and STE check passed, the last at 05:57 UTC. The night of 2026-09-15 failed at 14:40 UTC (F-101).
- `dotnet test`: 1115 tests, 0 failures, with the five Smoke tests on the local Godot build. `ste-check`: 0 findings in 16 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `4bc8cd4`.
- The record on the branch `night-results` holds `4bc8cd4` with the status failure, so the `night-gate` job fails on every PR until a night passes (D-115, D-177).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). The owner merges it with the red `night-gate` (D-371). PR-68 follows in a fresh session (D-121, D-370), and PR-66 comes after it.

### Traps and gotchas

- The GitHub PR #73 is PR-65. The roadmap id PR-68 is the shaft landing fix, and PR-66 digs the ramps and the tiers.
- The merge marks use the UTC date of the merge, 2026-09-15. D-370, D-371, and this entry use the local date, also 2026-09-15.
- The `night-gate` job fails on every PR until a night passes. `night.yml` takes a manual event, so PR-68 can run one before it merges.
- The reachability failure is no regression of PR-64, PR-65, or PR-67. The grid hash of seed 79146, floor 7 is the same at three revisions, and the floor first appears with the dig sizes of PR-63.
- The PR sweep of 5000 seeds never reads seed 79146, so a PR run passes while the night fails.
- The simulation version stays 10, and the bit-identity known answer stays `24c37100cd99edf4`.
- The next ids are D-372, OQ-175, F-102, PR-69, and Session 173.

### Open questions that block progress

None blocks this PR. D-370 resolves OQ-174. The PR-68 session diagnoses the shaft landing of seed 79146 and runs a night before the merge. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label and the red `night-gate` (D-371). A fresh session then opens PR-68: it digs seed 79146, floor 7, finds why the shaft lands on an unreachable floor, corrects the dig, adds the regression test, and runs a night before the merge (D-370).

## Session 171: 2026-09-15, Codex

Author: Codex
Session: review PR #73, the ramp meshes in Game, at effective head `720c7a9`. Branch `feat/pr-65-ramp-meshes`.

### What this session did, and why

- Reviewed the complete code and test diff for the ramp mesh, face coverage, ambient occlusion, mesh triangle, greedy sweep, and contact-sheet changes.
- Verified the provider gate. Session 170 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Checked the ramp slope planes, side and end coverage, chunk borders, triangle winding, texture density, occlusion, mesh budget path, contact-sheet layout, and no-ramp mesh preservation against D-368, D-369, and the PR-65 exit tests.
- The focused mesher and contact-sheet suite passed 86 tests. Found no in-scope defect.
- Wrote `docs/reviews/pr-73.md` with the verdict `Ready for owner merge` for `720c7a9`.
- Read the existing automated-review comment. It approved the head and raised no issue.

### State of the build

- `main` is at `a4bf6d6`. The effective head of PR #73 is `720c7a9`. The later `f4747ac` commit changes only `docs/session-handoff.md` and `docs/session-handoff-archive.md` under D-184.
- Remote head: `origin/feat/pr-65-ramp-meshes` is `5e62041` after the review push, verified with `git fetch`, clean status, and `gh pr view`.
- The focused suite passed 86 tests. Local full build and gate commands produced no completion result because the .NET process hung without output. Session 170 reports the full gates and revision-matched CI as passed on `720c7a9`.
- GitHub checks after the review push are in progress, including `evaluate`; no completed post-review verdict is available yet.

### In flight

PR #73 is ready for owner merge after the review commit reaches the PR and the review-gate refreshes. PR-66 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #73 is roadmap PR-65. The roadmap id PR-66 digs the ramps and tiers.
- The effective head is `720c7a9`, not the metadata tip `f4747ac` (D-184).
- A mesh with the side of a ramp holds triangle faces. Read `TriangleCount` for triangles, because `QuadCount` counts quads alone.
- The local .NET hang is an execution-context limitation, not a passed check. Use the revision-matched CI evidence from Session 170.
- The next ids are D-370, OQ-174, F-101, PR-68, and Session 172.

### Open questions that block progress

None blocks PR #73. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the post-review checks, including `evaluate` and `review-gate`, to complete. The owner then merges PR #73. A fresh session opens PR-66 and asks the owner for the shape of a tier first (D-350).

## Session 170: 2026-09-15, Claude Code

Author: Claude Code
Session: open PR-65, the ramp meshes in Game, as PR #73, with the owner answers D-368 and D-369. Branch `feat/pr-65-ramp-meshes`.

### What this session did, and why

- The owner merged PR #72, the merge record of PR-64, as `a4bf6d6` at 03:30 UTC on 2026-09-15, and asked for the next item. The branch came from `origin/main` at `a4bf6d6`.
- Before the code, the owner answered the tile of a ramp face (D-367). D-368: every face of a ramp takes the raw stone tile, because the floor of each tunnel and chamber is raw stone in all three bands. The recommendation stood.
- `FaceShape` reads the part of each side of a cell that its solid fills, in twelfths of a block. `GreedyMesher.FaceVisible` hides a face only when the side of the neighbor covers it, so a wall beside a ramp shows over the slope.
- `RampFaces` gives the slopes, merged in each row by plane and occlusion through `GreedySweep`, and each end, side, and bottom of a ramp cell that shows. The block faces now read their masks through `GreedySweep` too. The side of a run comes to a point at the low end, so `MeshData` gains `AddTriangle` and `TriangleCount`, and `QuadCount` counts the calls of `AddQuad`.
- `AmbientOcclusion.Occludes` reads the upper half of a ramp run as a block and the lower half as air. `CornerLevel` moved from the mesher to `AmbientOcclusion`, so the slopes and the block faces share it.
- The contact sheet adds a ramp of each slope in the whole render, with the camera at the foot. The owner approved the sheet as drawn, and D-369 closes exit test 4.
- A scratch test outside the repository hashed every chunk mesh of floors 1, 6, and 11 of seeds 1 to 8. `main` at `a4bf6d6` and this head both give `364F7B57EACE4F4F2D3034FD1C5A2A85339839351A0341BF8C838EFA157BCD89` over 210966 indices, so a grid with no ramp keeps every bit of its mesh.
- The first `det-lint` run found the plain string `"length"` in the error context of `GreedySweep`, and a named constant replaced it before the commit.
- The automated pass of gitar approved `720c7a9` at 04:55 UTC with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 160 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `a4bf6d6`. The effective head of PR #73 is `720c7a9`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-65-ramp-meshes` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 1115 tests, 0 failures, with the five Smoke tests on the local Godot build. After the last edits of the decisions and the roadmap, a run without the Smoke category passed 1110 tests. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 36 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `24c37100cd99edf4`, because Core does not change.
- CI on `720c7a9`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last at 05:07 UTC. No macOS leg ended "not acquired". `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `720c7a9` completed before the push of this entry, so that push cancels nothing (D-356).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the ramp code and the dig restart, and it can start hours late (F-95).

### In flight

PR #73: the Codex review per the `pr-review` skill at the effective head `720c7a9` (T-4). The owner then merges, and a docs PR records the merge (D-297). PR-66 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #73 is PR-65. The roadmap id PR-66 digs the ramps and the tiers.
- D-368 and D-369 carry 2026-09-14, the local date of the owner answers. This entry carries 2026-09-15, the local date when it was written.
- The effective head is `720c7a9`, and not the metadata commit of this entry (D-184).
- A mesh with the side of a ramp holds triangle faces. Read `TriangleCount` for the triangles, because `QuadCount` counts quads alone, and a test that reads faces by a stride of four vertices breaks on such a mesh.
- A slope reads the occlusion of the cell over the ramp. An end or a side of a ramp cell reads the occlusion of the whole side of its cell, also where the face is lower than the cell.
- The contact sheet is 2400 by 2000 pixels. The area to the right of the block cells is empty and renders black.
- The Godot build check writes a `.uid` file for each new script in Game. Commit the file with the script.
- `det-lint` reads a plain string literal in Game as a string that a player sees, also in an error context. Put a context key in a named constant.
- The simulation version stays 10, and the bit-identity known answer stays `24c37100cd99edf4`.
- The next ids are D-370, OQ-174, F-101, PR-68, and Session 171.

### Open questions that block progress

None blocks PR #73. The PR-66 session asks the owner for the shape of a tier before the code (D-350), and the gate of PR-66 needs the owner to confirm the ramps and the tiers in play. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #73 per the `pr-review` skill at the effective head `720c7a9` and writes `docs/reviews/pr-73.md`. The owner then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-66, and it asks the owner for the shape of a tier first (D-350).

## Session 169: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-64 as PR #71, in the same invocation as Session 167 (D-297). Branch `docs/pr-64-merge-record`.

### What this session did, and why

- Session 168 approved `b5bf2de` in `docs/reviews/pr-71.md` with no finding.
- The owner merged PR #71 as `43f14eb` at 03:05 UTC on 2026-09-15, and the tree of `43f14eb` equals the review tip `78470d0`. The two commits after `b5bf2de`, `d489060` and `78470d0`, change only metadata paths (D-184).
- `main` now holds the ramp cells of D-367 and the motion on a ramp of D-362 to D-366. `docs/design.md` marks PR-64 done, and sequence item 11 names the merge. The Phase 2 roadmap gains the status line of PR-64 and the mark in sequence item 15. F-97 stays open for PR-65 and PR-66.
- The file held twelve entries with this one, so Sessions 159 and 158 moved to the archive.

### State of the build

- `main` is at `43f14eb`, the squash merge of PR #71. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-64-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `43f14eb`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 03:19 UTC. No macOS leg ended "not acquired".
- `ste-check`: 0 findings in 16 files. `dotnet test`: 1049 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `43f14eb`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the ramp code and the dig restart, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-65, the ramp meshes in Game, follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #71 is PR-64. The roadmap id PR-65 is the ramp meshes in Game, and PR-66 digs the ramps and the tiers.
- The merge marks use the UTC date of the merge, 2026-09-15. This entry uses the local date, 2026-09-14.
- The Codex review of Session 168 ran the focused ramp suite of 173 tests, and its local full suite and build gave no completion result there. Session 167 ran the full gates on the effective head.
- The simulation version is 10, so a run record of version 9 fails with the notice of D-151 (D-260).
- The bit-identity known answer is `24c37100cd99edf4`. A version rise alone moves it, because the replay header holds the version.
- The mesher of PR-13 draws a ramp as a cube with the tile of its id. PR-65 draws the slope and its two sides.
- The next ids are D-368, OQ-174, F-101, PR-68, and Session 170.

### Open questions that block progress

None blocks this PR. The PR-65 session asks the owner for the tile of a ramp face before the code (D-367), and exit test 4 of PR-65 needs the owner approval of a contact sheet with the ramps. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-65, and it asks the owner for the tile of a ramp face first (D-367).

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
