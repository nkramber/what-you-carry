# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 79: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-59, the mine detail pass, the block ids, and the water rule. Branch `feat/pr-59-detail`.

### What this session did, and why

- Started PR-59 from `main` at `45dbaf5`, the squash merge of PR #28, as Session 78 planned. The commit `787c851` holds the code, the tests, and the roadmap note. PR #29 holds the branch.
- `BlockId` declares 2 hewn stone, 3 timber beam, 4 ore vein, 5 still water, 6 rubble, and 7 plank (D-259). The grid bound accepts 0 to 7, and `IsSolid` lets air and still water through (D-258).
- `Core/Procgen/DetailPass.cs` runs after the shafts and before the stairwell, in this order: collapses, pools, walls, pillars. A collapse fills the last stamp of a walker with no dependent, in cells that walker alone dug, with a rubble heap one to three rows high. A pool is a rectangle of two or three cells a side in a chamber of at least twelve cells, over rock, with a dry floor cell beside every pool cell. A wall block replaces rock that borders air with rock over it, by band. A pillar is one column with dry chamber floor on all eight sides, one try per twenty cells.
- `DigPlan` records which walker dug each air cell, which walkers have a dependent (a chamber, a drift, or a later walker on their trail), and where each walker ended.
- `PlayerBody` scales the speeds and the jump velocity by one half and gravity by one quarter while the column under the feet is water: the feet in a water cell, or the body in the air over water (D-261, D-262).
- The simulation version is 5 (D-260). The sweep content holds one template per band on floors 1 to 3, and the known answer moves from `036df5c08e2682e3` to `62c5e1d152fe94fe`.
- The six exit tests of the roadmap entry pass. The reachability sweeps of PR-9 exit test 1 and PR-59 exit test 1 read one dig per seed through a shared report.
- `TunnelCrossSection` steps over chamber cells, water cells, the cells around a pillar, and the cells around a collapse, because those are not tunnel cells (D-166).

### State of the build

- `main` is at `45dbaf5`, the squash merge of PR #28. This branch holds the PR-59 commit `787c851` above it, and this entry above that.
- Remote head: `origin/feat/pr-59-detail` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures. The suite takes about three minutes, and the shared reachability sweep takes most of it.
- `det-lint`: 0 findings. Core 0 in 54 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #29 is open and it holds this branch. The automated pass runs on the push, and the author answers each comment. Then a Codex session reviews the PR at the effective head, which is `787c851` until a substantive push moves it. The review focus is determinism, content, test quality, and replay (roadmap PR-59). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59 is open as PR #29. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- One reading sits inside the roadmap scope and no decision names it: the water probe holds while the body is in the air over water. D-262 says "while the feet stand in a water cell". A probe of the feet cell alone gives an apex of 1.05 blocks at 1.6 times the ticks, because the quarter gravity goes as soon as the feet rise out of the cell. The review or the owner can ask for a decision.
- A collapse must never fill a cell of a walker with a dependent. The first version filled cells of any walker that were dug by that walker alone, and a walker that looped back cut its own path to a chamber it dug earlier. Seed 418 of floor 14 found it.
- The pillars come last in the detail pass, so no wall block lands on a pillar and no pool opens under one. A pool cell needs air over it and rock under it.
- The pillars and the pools are cells with their floor row, not columns, because two chambers can stack on one column at two rows.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Read the gitar comments on PR #29 and answer each one per the `pr-review` skill. Then a Codex session reviews PR #29 at the effective head, reads the PR comments into the review, and writes `docs/reviews/pr-29.md`. The review confirms the simulation version 5 and the bit-identity change under G-20.


## Session 78: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-9 merge, and answer the PR-59 questions before its code. Branch `docs/pr-9-merge-record`.

### What this session did, and why

- The owner merged PR #27 as `336fe4e`, after a Codex review with no finding at the effective head `bd1366e` (Session 77).
- The design doc PR-9 entry reads merged, the roadmap PR-9 entry has its status line, and the sequence marks item 16.
- Asked six owner questions in two batches, and D-258 to D-263 record the answers. OQ-126 to OQ-131 hold the questions.
- D-258: still water is not solid. A pool is a one-block depression, and a body walks and jumps through it more slowly. The search reads water as air.
- D-259: the block ids take the order of D-210: 2 hewn stone, 3 timber beam, 4 ore vein, 5 still water, 6 rubble, 7 plank. D-239 is revised in part.
- D-260: PR-59 raises the simulation version to 5, because a floor with other blocks is another simulation.
- D-261 to D-263: the walk and sprint speeds take the factor one half in water. The jump velocity takes one half and gravity one quarter, so the apex stays at one block and the rise takes twice as long. The three factors are Core constants in `PlayerBody`.
- The automated pass on the first push found that the first text of D-262 gave both numbers one factor of one half, which halves the apex. The owner took the correction, one half for the velocity and one quarter for gravity, and F-86 records it. The correction commit answers the pass.
- The PR-59 roadmap entry holds the ids, the water rules, the version rise, and two new exit tests. The design doc paragraph and gate say the same.

### State of the build

- `main` is at `336fe4e`, the squash merge of PR #27. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-9-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 469 tests, 0 failures, as PR #27 left them. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `036df5c08e2682e3`. The simulation version is 4.

### In flight

PR #28 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The automated pass approved `930b659`, every comment has its answer, and the label is on. The owner asked that the session apply the label itself from now on, after the last push and the pass. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59, PR-10, and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Water changes the body: `PlayerBody` reads the block of the feet cell on each tick, and a water cell scales the speeds and the jump velocity by one half and gravity by one quarter (D-261, D-262). The apex stays at one block, so the reachability search of PR-9 reads water as air and needs no pit rule. One factor on both numbers halves the apex (F-86).
- A wall block of the detail pass replaces rock that borders air and removes no air, so D-166 holds as PR-9 left it. Collapses and pillars remove air, and the PR-9 sweep runs again over the result.
- PR-59 raises the simulation version to 5 and moves the bit-identity known answer (D-260, G-20). The sweep folds three floors of its own content set, so the detail pass moves the hash on its own.
- The block ids are part of the grid (D-259). `VoxelGrid.Set` holds the explicit bound of declared ids, and `EveryDeclaredBlockIsAccepted` walks the enum, so a new value fails the test until the bound names it.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #28. Then a new session starts PR-59 on a short branch: the block ids of D-259 in `BlockId` and the grid bound, the water rule in `PlayerBody`, the detail pass with collapses, pillars, and the blocks by band, the simulation version 5, and the six exit tests, under D-210, D-239, D-253, D-254, and D-258 to D-263.


## Session 77: 2026-09-09, Codex

Author: Codex
Session: review PR-9 as PR #27 at effective head `bd1366e`. Branch `feat/pr-9-dig-plan`.

### What this session did, and why

- Verified the cross-provider gate. Session 76 identifies Claude Code as the author of the substantive PR-9 change and its correction. Codex is the eligible reviewer.
- Read the complete diff, the PR-9 roadmap entry and exit tests, the affected Core callers, content files and validators, replay and stairwell paths, D-159, D-164 to D-167, D-210, D-228, D-236, D-252 to D-257, and G-20.
- Read the automated pass claims and the author replies from the Session 76 handoff. The source-order finding has a correction and a regression test. The CI notices have answers.
- Found no actionable defect. Wrote `docs/reviews/pr-27.md` with the verdict `Ready for owner merge` at effective head `bd1366e`.

### State of the build

- `main` is at `3e5cbd3`, the squash merge of PR #26. The effective PR-27 head is `bd1366e`.
- Local build and test pass. `dotnet build` reports 0 warnings and 0 errors. `dotnet test` reports 469 tests and 0 failures.
- `det-lint` reports 0 findings in 53 Core files and 0 Game files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` returns `036df5c08e2682e3`. The simulation version is 4.
- The Godot 4.7.2 headless build check passes, as recorded in Session 76.

### In flight

PR #27 needs the review record and this handoff entry committed and pushed. The owner can merge after the remote review-gate check reads `Ready for owner merge` at effective head `bd1366e`.

### Traps and gotchas

- The review record must name `bd1366e`, not this metadata commit.
- The bit-identity value changed to `036df5c08e2682e3` because the simulation now includes procgen and floor transition state. G-20 requires simulation version 4.
- The effective head stays `bd1366e` while later commits change only `docs/reviews/`, `docs/session-handoff.md`, or `docs/session-handoff-archive.md`.
- GitHub API access failed in this review context. The next session must verify the published head and review-gate result after push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit the review record and handoff entry. Push the branch. Fetch and verify that the remote head has no ahead count and that the review-gate check reads the approved effective head.

## Session 76: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-9, the mine dig plan, the reachability search, and the stairwell transition. Branch `feat/pr-9-dig-plan`.

### What this session did, and why

- Started PR-9 from `main` at `3e5cbd3`, the squash merge of PR #26, as Session 75 planned. The commit `1c75fc5` holds the code, the content, the tests, and the roadmap note. PR #27 holds the branch.
- The three floor templates gain `sizeX`, `sizeY`, and `sizeZ` by band (D-252). The validator rejects a size past D-164, and a size below 24 by 7 by 24, because the dig plan keeps a shell of rock and needs six rows for one floor.
- `content/chambers/` holds eight chamber kinds with weights from 8 to 40 (D-255). The fields are `id`, `weight`, `boxCountMin`, `boxCountMax`, `boxSizeMin`, and `boxSizeMax`. A box side is at least three (D-166).
- `Core/Procgen/`: `FloorGenerator` digs floor n from the seed, the floor number, and the content set. `ChamberBudget` draws kinds with two feasibility bounds, so the sum lands in the window and the count in the range (D-167). `ChamberFootprint` unions boxes, rounds corners, fills notches, and repairs every cell to a run of three. `DigCanvas` holds the two carve rules. `DigPlan` runs the walkers: a gallery of radius 2, drifts of radius 1, ramps of two to five one-block steps, chambers at the walker, and three by three shafts. `Reachability` is the breadth-first search under D-165.
- The stairwell: the interact bit descends and bit 9 ascends, at the stairwell alone (D-257). `Button.Ascend` is 0x0200, the assigned mask is 0x03FF, and the reserved mask is 0xFC00.
- The loop takes the seed and the content set, and the replay takes the content set (D-236). The state gains the floor number and the run end, the hash reads both after the body, and the simulation version is 4 (G-20). The torn-tail line reads the floor from the state (D-228).
- `Rng.ForStream` gains a floor argument. Floor zero is the run stream of D-159, so every earlier stream and the RNG known answer stand.
- The bit-identity sweep digs three floors of its own content set and replays on a dug floor. The known answer moves from `afed0063a6cf8a50` to `036df5c08e2682e3`.
- The body tests step `PlayerBody` on the flat floor with an explicit yaw, and the loop tests run on dug floors from the repository content. `RepositoryContentSource` is a shared test file now.
- The PR template gains the gitar gate line (D-250).
- The automated pass on `1c75fc5` gave one code finding and two CI notices. The code finding had merit: the loader kept the order of the source, so the chamber list and the budget draw took the order of the file system, and the Windows CI leg found it through `AFloorWithoutOneTemplateIsAnError`. Commit `bd1366e` sorts the content files by ordinal path in the loader, adds the regression test `TheSourceOrderDoesNotReachTheLists`, and names the template that covers floor 1 in the test. The CI notice on the Windows failure was the same defect. The CI notice on the absent review record had no merit (D-251), and it got its reply on the PR. The second pass on `bd1366e` approved the code and repeated the review-record notice, which got the same reply.

### State of the build

- `main` is at `3e5cbd3`, the squash merge of PR #26. This branch holds the PR-9 commit `1c75fc5`, the correction `bd1366e`, and the handoff entries above them.
- Remote head: `origin/feat/pr-9-dig-plan` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 469 tests, 0 failures. The suite takes about ninety seconds, and `EveryChamberReachable` takes about forty-five of them at five thousand seeds.
- `det-lint`: 0 findings. Core 0 in 53 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `036df5c08e2682e3`. The simulation version is 4.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #27 is open and it holds this branch. The automated pass approved `bd1366e`, and every comment has its answer. The PR is ready for a Codex review at the effective head `bd1366e`. The review focus is determinism, content, replay, and test quality (roadmap PR-9). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-8 are merged. PR-9 is open as PR #27. PR-59, PR-10, and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Three choices sit inside the roadmap scope and no decision names them: the run end is a hash field beside the floor number (D-160), an intent that sets both stairwell bits ascends, and the floor stream is a third argument of `Rng.ForStream`. The review or the owner can ask for a decision on any of them.
- The two carve rules are the proof of reachability. A ramp starts past the stamp around the walker, and its landing reaches one brush radius past the new position, so a second ramp right after the first leaves no gap. A job digs no ramp before its first flat stamp, because the job starts on a cell of another walker. The first version lacked both, and seed 2 of floor 3 found the gap.
- A chamber cell needs a run of three along X or along Z, or the cross-section test fails on it. The corner pass can leave a cell without one, and the repair pass adds the two X neighbors.
- `Reachability.Landing` gives minus one for no move. A step up needs a third air cell over the start, for the jump.
- The content hash of a record must match the hash of the content set that the replay takes. The replay tests use `TestWorld.Content.Hash` in the header, and the header tests keep the fixed hash.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band and not the working mine. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The night count of exit test 1 runs when `WYC_NIGHT_SWEEP` is `1`. PR-11 gives the night job its own switch.
- The loop constructor digs floor 1, so a test that builds one thousand loops digs one thousand floors. `CameraNeverInsideSolid` takes seven seconds for that reason.
- The override label goes stale on any push outside the metadata set (D-190). The effective head is the newest commit outside that set.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #27 per the `pr-review` skill, at the effective head `bd1366e`, reads the PR comments and the author replies into the review, and writes `docs/reviews/pr-27.md`. The review confirms the simulation version 4 and the bit-identity change under G-20.


## Session 75: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-8 merge, and answer the PR-9 questions before its code. Branch `docs/pr-8-merge-record`.

### What this session did, and why

- The owner merged PR #23 as `a3bdf20`, after a Codex review with one P2 finding, the correction `4e98470`, and a repeat review with the verdict `Ready for owner merge` (Sessions 72 to 74).
- Audited every document against the merged state. The registers were complete: D-248 to D-251, OQ-117 to OQ-119, F-82 to F-85, and the review records `pr-23.md`, `pr-23-response.md`, and `pr-25.md` all sit on `main`.
- Four stale places are corrected. The design doc PR-8 entry and its sequence line said open, and the roadmap said open. The roadmap sequence had no mark for PR-3 to PR-8, and its open questions still listed OQ-12, which D-210 resolved on 2026-09-08.
- Asked six owner questions in three batches, and D-252 to D-257 record the answers. OQ-120 to OQ-125 hold the questions.
- D-252: the floor size per band in the floor template: 48 by 12 by 48, 72 by 16 by 72, and 96 by 20 by 96.
- D-253: a floor is a mine dig plan: a main gallery, side drifts, chambers as smoothed box unions with pillars, shafts and ramps, and collapses. Every tunnel takes a brush of at least three by three. The owner asked for a floor far more random, varied, and detailed than rectangles, whatever the implications.
- D-254: PR-9 carves raw stone and air with the exit tests, and a new PR-59 adds the detail and the D-210 block ids. D-239 is revised in part.
- D-255: chamber kinds are a content type, `content/chambers/*.json`, with an id, a weight, a box count range, and a box size range.
- D-256: the spawn is in the first chamber, and the stairwell is in the chamber with the longest walkable path.
- D-257: at the stairwell, the interact bit descends and bit 9 ascends, so the record carries the choice. D-232 is revised in part.
- The design doc and the roadmap hold the rewritten PR-9 entry and the new PR-59 entry, with chambers and tunnels in place of rooms and corridors. The roadmap sequence places PR-59 after PR-9.

### State of the build

- `main` is at `a3bdf20`, the squash merge of PR #23. This branch holds the document commit above it.
- Remote head: `origin/docs/pr-8-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 426 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 43 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `afed0063a6cf8a50`. The simulation version is 3.

### In flight

PR #26 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The label goes on after the last push, because a later push makes it stale. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-8 are merged. PR-9, PR-59, PR-10, and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The process since PR #23: gitar comments on every PR after a push (D-250). Answer every comment before the hand-over or the override request. A comment with no merit gets a reply and a resolve. A comment with merit gets the change, a push, and a reply. A comment on the PR body has no thread, so its reply is a PR comment. Tell the owner when the PR is ready for the other provider or for the override.
- The "Review gate / evaluate" job line reads red until an approved review record covers the effective head (D-251). That is the design and not a failure to fix.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.
- Two open PRs that both add a handoff entry conflict at the top of the file, at the end of the register, at the end of the questions, and in the findings table. The second one to merge takes a merge from `main` first. Rebuild the handoff and the archive from the union of the entries, by number, with ten in the handoff and each entry once.
- The effective head is the newest commit outside the metadata set, and a merge from `main` moves it. The review record names that commit.
- PR-9 changes the loop state: the floor number joins the hash after the fields of PR-7, so the simulation version rises to 4 and the bit-identity hash moves (G-20). The replayer then takes the content set in place of the grid and the spawn, which closes D-236.
- PR-9 adds bit 9 to `Button` and the masks: `AssignedMask` becomes 0x03FF and `ReservedMask` becomes 0xFC00. The random intent helper of the tests and the sweep mask follow.
- PR-9 changes the floor template schema. `sizeX`, `sizeY`, and `sizeZ` join the required list, the three content files gain them, and the content hash of the test set moves.
- The generator and the reachability search step in integers over the Procgen stream (D-159), and no DetMath call is needed for the carving.
- The PR template still lacks the gitar gate line of D-250. PR-9 adds it as the next code PR, in `.github/pull_request_template.md`.
- `StateHash` has no `==` operator. Compare `.Value`.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner adds the `review-override` label to PR #26 after the automated pass and merges it. Then a new session starts PR-9 on a short branch: the floor sizes and the chamber templates as content, the dig plan generator, the reachability search, the stairwell transition with the two bits, the loop floor number with the simulation version 4, and the eight exit tests, under D-159, D-164 to D-167, D-210, D-236, and D-252 to D-257. The session settles the field names of the chamber template in its validator, and it files a question only when a choice changes a contract.

## Session 74: 2026-09-09, Codex

Author: Codex
Session: re-review PR #23 after the P2-1 correction. Branch `feat/pr-8-camera`.

### What this session did, and why

- Verified the provider gate again. Claude Code supplied the substantive PR-23 changes, and Codex is the eligible reviewer.
- Compared the new effective head `4e98470` with the prior reviewed head `e6e89aa`. Read the response file, the replay observer, the bit-identity sweep, the related contracts, and all current PR comments and replies.
- Closed P2-1 as fixed in `4e98470`. `RunReplayer` now calls the observer after each complete frame, and the bit-identity sweep folds camera and aim values from that replay traversal. The second live loop is gone.
- The later automated suggestion for a null guard has no merit. Nullable references are enabled and warnings are errors, so the observer parameter is non-nullable at the call boundary.
- Updated `docs/reviews/pr-23.md` with the new effective head and verdict `Ready for owner merge`.

### State of the build

- `main` is at `1d8f8bd`. The effective PR-23 head is `4e98470`, and the current metadata tip is `4d37187`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 426 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings. `bit-identity`: `afed0063a6cf8a50`.
- The Godot 4.7.2 headless build check passes.
- The latest GitHub checks pass for CI, bit identity, determinism lint, STE, and Gitar. The review-gate and evaluate results still refer to the prior unapproved review record until this update reaches the PR head.
- The first test run hit `SocketException (13): Permission denied` in the restricted context. The rerun in the permitted execution context passed.

### In flight

PR #23 is ready for owner merge after this review record reaches the remote PR head. The effective head remains `4e98470` because the commits after it change only review and handoff metadata.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The review record must name `4e98470`, not the metadata tip `f1af5db`.
- The bit-identity value changed to `afed0063a6cf8a50` because the camera fold now reads the replay observer. The simulation version stays 3 because no simulation behavior contract changed.
- The review-gate and evaluate jobs must run again after this review record reaches the PR head.
- The reviewing provider reads automated comments and author replies into the record and does not reply to or resolve the automated comment (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge PR #23 after the remote review-gate check reads `Ready for owner merge` at effective head `4e98470`.

## Session 73: 2026-09-09, Claude Code

Author: Claude Code
Session: answer the PR #23 review. Branch `feat/pr-8-camera`.

### What this session did, and why

- Read the one P2 finding in `docs/reviews/pr-23.md`. Full merit: the sweep folded the camera and the aim ray from a second live loop, and PR-8 exit test 5 names the replay.
- The replay takes an `IReplayObserver` now, and it calls `AfterTick` after each complete frame. The five-argument `Replay` passes a silent observer, so the twelve callers stand.
- The sweep folds the camera pose and the aim ray of every replayed tick through a `CameraFold` observer, beside the end hash and the CRC-32. The second live loop is gone.
- The sweep hash moved from `92ef27ee175b3e7e` to `afed0063a6cf8a50`, because the fold order changed. The simulation version stays 3, because no simulation number changed (G-20).
- Three tests establish the fix, and one of them is new: the observer sees every complete frame and no torn tail, the replay fold equals the live fold over one hundred seeds, and the sweep holds no loop of its own. 426 tests in total.
- F-85 records the finding, and `docs/reviews/pr-23-response.md` records the disposition.
- The automated pass on the push gave one comment with two items, and neither had merit. The CI notice asked for an edit of the reviewer's verdict, which the author never makes (T-4, D-251). The null guard on the observer asked for a case that the compiler rejects, because nullable reference types are on with warnings as errors (D-68). Both got a reply on the PR, and no thread existed to resolve.

### State of the build

- `main` is at `1d8f8bd`, the squash merge of PR #25. This branch holds the correction `4e98470` above the review commit, and this entry above it.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 426 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 43 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `afed0063a6cf8a50`. The PR #23 review moved it from `92ef27ee175b3e7e`, and `BitIdentityKnownAnswer` pins the new value.

### In flight

PR #23 is open and it holds this branch. The automated pass runs on this push, and then a Codex repeat review at the effective head `4e98470` updates the same review record. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `4e98470`. The review record still names `e6e89aa`, and the repeat review updates the head and the verdict together.
- The sweep hash moved without a version change. G-20 ties the version to a simulation number, and the fold order of the sweep is not one.
- The observer runs after each step and inside the frame loop, so a frame that fails its checksum stops the replay before the observer sees it.
- A test observer that folds a hash holds a `StateHash` field and gives it back through a property, because a `ref` cannot cross an interface call.
- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass runs on this push. Then a Codex session repeats the review per the repeat procedure, at the effective head `4e98470`, and updates `docs/reviews/pr-23.md` with the status of P2-1 and a new verdict.

## Session 72: 2026-09-09, Codex

Author: Codex
Session: review PR #23 at effective head `e6e89aa`. Branch `feat/pr-8-camera`.

### What this session did, and why

- Verified the provider gate. Session 71 identifies Claude Code as the author of the substantive PR-23 change, and Codex is the eligible reviewer.
- Read the complete PR diff, the PR description, the Phase 1 PR-8 exit tests, decisions D-241 to D-249, the replay path, the bit-identity sweep, and the existing gitar comment.
- Found one P2 defect. The bit-identity sweep replays the record for the final state hash, but it folds the camera and aim values from a separate live loop. This does not prove the PR-8 replay exit test.
- Wrote `docs/reviews/pr-23.md` with the verdict `Changes required` at effective head `e6e89aa`.

### State of the build

- `main` is at `1d8f8bd`. The effective PR-23 head is `e6e89aa`, and the current metadata tip is `2f365cc`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 425 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings. `bit-identity`: `92ef27ee175b3e7e`.
- The Godot 4.7.2 headless build check passes.
- GitHub CI, bit identity, determinism lint, STE, and Gitar pass at `2f365cc`. The review-gate check is neutral and evaluate fails until an approved review record exists, as D-251 requires.
- `git fetch origin` could not update `.git/FETCH_HEAD` because the checkout denied access. `gh pr view` verified the remote head before this review record.

### In flight

PR #23 is open with one P2 finding. The author must make the camera and aim fold read from the replay traversal, then push a substantive revision. A Codex re-review must compare the new effective head and update this same review record.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks them.

### Traps and gotchas

- The effective head is `e6e89aa`, not the metadata tip `2f365cc`.
- The review-gate check is neutral until the review record reaches the PR head. The evaluate job fails while the record does not approve the effective head.
- The camera fold at `WhatYouCarry.Tools/BitIdentity/BitIdentitySweep.cs:219-235` uses a new live loop. The record replay at lines 208-210 does not supply those camera values.
- The existing gitar comment is recorded in `## PR comments`. The reviewing provider does not reply to or resolve that comment (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Correct P2-1 in PR #23. Then run the automated pass, fetch the new effective head, and complete a Codex re-review with the same finding id.

## Session 71: 2026-09-09, Claude Code

Author: Claude Code
Session: bring `main` into the branch after PR #25, and hand PR #23 to the Codex review. Branch `feat/pr-8-camera`.

### What this session did, and why

- The owner merged PR #25 as `1d8f8bd`, after a Codex review with no finding (Session 70). PR #23 is the second open PR, so it takes `main` first, as Session 69 planned.
- Merged `main` into this branch in `e6e89aa`. Four files conflicted at their append points: the questions, the roadmap header, the handoff, and the archive. Each one holds both sides in id order now: OQ-117 to OQ-119, D-200 to D-251 in the header, and every session entry once.
- The findings table merged on its own but out of order. F-82, F-83, and F-84 read in order now.
- The union of the two handoffs held twelve entries, so Sessions 61, 60, and 59 moved to the archive beside 58 and 57, and this entry keeps the file at ten (D-146).
- The effective head of PR #23 is `e6e89aa`, because the merge brings the workflow, the skill, and the agent files. The code of PR-8 is unchanged since `e6d40e0`, and the bit-identity value stands.
- The review-gate workflow on this push reads the D-251 step from `main`, so the job line of PR #23 reads red until its review record exists. That run is the first proof of D-251 (G-19).

### State of the build

- `main` is at `1d8f8bd`, the squash merge of PR #25. This branch holds the PR-8 commit, two merges from `main`, and the entries above them.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 425 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 42 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `92ef27ee175b3e7e`, as PR-8 set it (G-20).

### In flight

PR #23 is open and it holds this branch. gitar approved the PR-8 head and the first merge, and the pass runs again on this push. The PR changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head `e6e89aa`. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The review-gate job line of PR #23 reads red by design until the review record exists (D-251). The check run is neutral. The review record at the effective head turns both green.
- A merge from `main` can put one session entry on both sides of the handoff and the archive. Rebuild both files from the union of the entries, by number, and keep each entry once.
- The effective head is the merge commit `e6e89aa` and not `e6d40e0`. The review record names the merge commit.
- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250). The `## PR comments` part of the record lists them.
- The hash moves in PR-8, and that is the point of G-20. A review that sees `92ef27ee175b3e7e` must confirm it and not restore `e8ef2b1fad938845`.
- The loop hash reads the buttons, so two loops that differ in the controller aim bit alone give two hashes and one camera pose.
- The march tie order is X, then Y, then Z, so a line through a corner visits the cell at the corner. The fine-walk test allows one millimeter for that.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #23 per the `pr-review` skill, at the effective head `e6e89aa`, reads the existing PR comments into the review, and writes `docs/reviews/pr-23.md`. The review focus is determinism, replay, and test quality, and it confirms the simulation version 3 and the bit-identity change under G-20.

## Session 70: 2026-09-09, Codex

Author: Codex
Session: review PR #25 for the review-gate job result. Branch `fix/review-gate-red-on-neutral`.

### What this session did, and why

- Verified PR #25 at effective head `3e15249` against `main` at `a78e759`.
- Confirmed the provider gate. Session 69 identifies Claude Code as the author, and Codex is the eligible reviewer.
- Read the complete diff, the workflow boundary, the test, the related documents, and the automated review comment.
- Found no in-scope defect. The success-only guard fails on neutral, null, and unexpected conclusions, and passes on success.
- Wrote `docs/reviews/pr-25.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` is at `a78e759`, and the reviewed effective head is `3e15249`.
- `dotnet build` passes with 0 warnings and 0 errors. `dotnet test` passes with 399 tests and 0 failures.
- `det-lint` passes with 0 findings. `ste-check` passes with 0 findings. `bit-identity` gives `e8ef2b1fad938845`.
- The Godot 4.7.2 headless build check passes.
- GitHub CI, bit identity, determinism lint, STE, evaluate, and Gitar pass at `3e15249`. The `review-gate` check is neutral because the base branch still holds the old workflow (D-197).
- Remote head: `3451c24` is the review commit, checked after push.

### In flight

PR #25 is open with the verdict `Ready for owner merge` at effective head `3e15249`. The first PR after this merge must prove that the changed workflow makes a neutral check run red (D-251).

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The workflow runs from the base branch. PR #25 cannot exercise its changed workflow on itself (D-197).
- The review record commit changes only metadata, so the effective head stays `3e15249` (D-184).
- The handoff now holds ten entries. Session 58 moved to the archive (D-146).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge PR #25. After the merge, observe the first PR with no review record and confirm that the review-gate job reads red.
