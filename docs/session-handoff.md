# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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
