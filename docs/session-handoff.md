# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 85: 2026-09-09, Codex

Author: Codex
Session: repeat review PR #31 at effective head `1b58d3a`. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Recomputed the effective head. The fix commit `1b58d3a` changes Core code, tests, and the design record. Later commits change only review, handoff, and archive metadata.
- Read the author response, the new diff, the full changed source and tests, the roadmap, the decisions, the questions, the PR comments, and the current checks.
- Verified P2-1 fixed in `1b58d3a`. The spread now samples one angle and one roll, and the test checks five axes against the exact half angle.
- Verified P2-2 fixed in `1b58d3a`. The arc solver rejects invalid and non-finite speed, gravity, and point inputs with context.
- Updated `docs/reviews/pr-31.md` with both findings marked fixed and the verdict `Ready for owner merge` at effective head `1b58d3a`.

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. The PR effective head is `1b58d3a`. The remote tip before this review record update was `33dfb2a`. The review record was published in `e5cb108`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 506 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes after the sandbox permission failure is rerun in the required execution context.
- Current GitHub checks pass for bit identity, compare, CI, determinism lint, STE, and Gitar. The review-gate evaluate job fails on the old verdict and must rerun after this record update.

### In flight

PR #31 is ready for owner merge after the updated review record reaches the remote branch and the review-gate check reads the new verdict. No other PR is open.

### Traps and gotchas

- The effective head is `1b58d3a`, not the remote metadata tip. D-184 excludes the review, session handoff, and archive paths only.
- The spread is uniform in the angle, not in the solid angle. D-266 names the half angle, so this is valid.
- The sweep hash is `d8943df12fefcbee`. Do not restore the prior hash `3220e92dcbca55a2`.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The review record and this handoff entry are published in `e5cb108`. Fetch, verify that the branch has no ahead count, and check that the review-gate result approves effective head `1b58d3a`.

## Session 84: 2026-09-09, Claude Code

Author: Claude Code
Session: answer the PR #31 review. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Read the two P2 findings in `docs/reviews/pr-31.md`. Both have full merit.
- P2-1: the spread drew a yaw offset and a pitch offset, so a shot could leave the cone of D-266 by up to 6 degrees at the corner. The draw is one angle from the direction and one roll around it now, so every shot stays inside the half angle. The draw count per shot stays two.
- P2-2: the arc solver took a speed of zero, a negative gravity, or a value that is not finite. It rejects each with a context error now (T-2).
- The sweep hash moved from `3220e92dcbca55a2` to `d8943df12fefcbee`, because every shot of the sweep turns another way. The simulation version stays 6, because it rose in this PR already (G-20).
- `SpreadStaysInsideTheCone` reads the half angle from the definition over five axes, and `ArcSolverRejectsABadSpeedOrGravity` covers seven bad inputs. 506 tests in total.
- F-87 and F-88 record the findings, and `docs/reviews/pr-31-response.md` records the dispositions.

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. This branch holds the PR-10 commit `c214d03`, the review commits, the correction `1b58d3a`, and this entry above them.
- Remote head: `origin/feat/pr-10-projectiles` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 506 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 57 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`. The simulation version is 6.

### In flight

PR #31 is open and it holds this branch. The automated pass runs on this push, and then a Codex repeat review at the effective head `1b58d3a` updates the same review record. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 is open as PR #31. PR-11 remains, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `1b58d3a`. The review record still names `c214d03`, and the repeat review updates the head and the verdict together.
- The sweep hash moved without a version change, because the version rose in this PR already. A review that sees `d8943df12fefcbee` must confirm it and not restore `3220e92dcbca55a2`.
- The spread is uniform in the angle from the axis and not in the solid angle of the cone, so shots gather near the axis less than a uniform disc would. D-266 names the half angle alone.
- The cone sides come from the world up, or from the world right for a vertical direction, so a direction within 2.6 degrees of vertical takes the second axis. The angle from the axis is exact either way.
- The reviewing provider reads the existing PR comments and author replies into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass runs on this push. Then a Codex session repeats the review per the repeat procedure, at the effective head `1b58d3a`, and updates `docs/reviews/pr-31.md` with the status of P2-1 and P2-2 and a new verdict.


## Session 83: 2026-09-09, Codex

Author: Codex
Session: review PR #31 at effective head `c214d03`. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Verified the provider gate. Session 82 identifies Claude Code as the author of PR-10, and Codex is the eligible reviewer.
- Read the complete PR diff, the PR-10 roadmap entry and exit tests, the affected Core callers, the content validator, the replay hash, the bit-identity sweep, the decisions, the questions, and the PR comments and author reply.
- Found two P2 findings. The spread code samples a square of yaw and pitch offsets, so a shot can leave the declared cone. `ArcSolver` accepts zero or negative physics inputs without a contextual error.
- Wrote `docs/reviews/pr-31.md` with the verdict `Changes required` for effective head `c214d03`.

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. The PR tip is `b48cc71`, with metadata commits `ec76f0e` and `b48cc71` after effective head `c214d03`.
- `dotnet build`: pass. `dotnet test`: 500 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `3220e92dcbca55a2`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.
- GitHub CI has passing bit-identity, compare, CI, determinism lint, STE, and Gitar checks. The review-gate evaluate job fails because the review file was absent before this session. The review record now names the two required corrections.

### In flight

PR #31 needs the two findings corrected and a new cross-provider review at the new effective head. The review record and this handoff entry are pushed at `b15c784`.

### Traps and gotchas

- The effective head is `c214d03`, not the PR tip, because the two later commits change only the metadata paths allowed by D-184.
- The spread test permits 22 degrees for a 15-degree definition. That threshold hides the square-sampling defect.
- The handoff records the one-tick lifetime grace described by the implementation. This review did not raise it because the prior session treats it as intentional and the projectile age remains within the lifetime.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The author corrects P2-1 and P2-2, runs the full gates, and asks for a repeat review. The repeat review keeps the finding ids and updates the same review record to the new effective head.

## Session 82: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-10, the projectile simulation, the arc solver, and the shot of the attack bit. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Started PR-10 from `main` at `4687081`, the squash merge of PR #30, as Session 81 planned. The commit `c214d03` holds the code, the content, the tests, and the roadmap note. PR #31 holds the branch.
- The projectile schema gains the required field `spreadHundredths` (D-266), with bounds: a speed of at least one, a gravity scale of at least zero, and a spread from 0 to 18000. The two plain definitions gain a spread, and four test-only files hold the extremes of D-149. The ordinal order puts `arrow` first, so it is the Phase 1 shot (D-265).
- `Core/Projectiles/ProjectileSimulation.cs`: a flat list in flight order. A projectile is a point. Each tick ages it, applies the gravity of D-231 scaled by its definition, and sweeps one segment with the grid ray march of PR-8 and a slab test against the entity boxes. The nearest hit ends it at the point. A projectile never hits the box of its owner.
- `Core/Projectiles/ArcSolver.cs`: the low-arc direction with DetMath alone, a vertical case, a gravity-free case, and a clear report for a target out of reach.
- The loop fires the first definition once per press of the attack bit, from the shoulder point toward the first solid cell that the crosshair ray meets within 100 meters (D-267, D-268). `CameraPose` carries the shoulder point now. The loop exposes the ends of the last tick, which is not state. The projectiles end with the floor at a descent.
- The projectiles join the hash after the run end, in flight order, and the simulation version is 6 (G-20). The sweep content holds one definition with a spread, so the attack bit of the sweep intents fires shots. The known answer moves from `62c5e1d152fe94fe` to `3220e92dcbca55a2`.
- The six exit tests of the roadmap entry pass, with unit tests for the spread cone, the press edge, the shoulder origin, the segment test, the end of the floor, and the errors. 500 tests in total.

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. This branch holds the PR-10 commit `c214d03` above it, and this entry above that.
- Remote head: `origin/feat/pr-10-projectiles` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 500 tests, 0 failures, in about three and a half minutes.
- `det-lint`: 0 findings. Core 0 in 57 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `3220e92dcbca55a2`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #31 is open and it holds this branch. The automated pass approved `c214d03` with no finding, and its CI notice on the absent review record has its reply (D-251). A Codex session reviews the PR at the effective head `c214d03`. The review focus is determinism, errors, and test quality (roadmap PR-10). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 is open as PR #31. PR-11 remains, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The Projectile stream of the run is state that no hash reads: `Rng` exposes no state word. Two loops with one intent stream draw the same values in the same order, so a replay matches, and a hash compare at a tick with no projectile in flight cannot tell a run with shots from one without. `ProjectilesAreDeterministic` compares the hashes while a projectile is up.
- The random intents of every loop test set the attack bit on some ticks, so every replay test fires shots now. A content set without a projectile definition makes the attack bit an error (D-265).
- The crosshair ray of the shot is the camera forward without aim assist, because no target exists before PR-16. PR-16 gives the shot the assisted ray.
- The shot origin is the shoulder point after the first march of D-249, so a shot never starts inside rock. The grid ray march throws on a start inside a solid cell, and a test that fires from inside rock sees that error.
- A projectile ends at its lifetime with one tick of grace: the end fires when the age passes the lifetime, so a definition of N ticks flies N ticks.
- The arc solver gives the continuous arc, and the fixed-step flight lands a little short, as the jump of F-83 does. Exit test 3 allows one block.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #31 per the `pr-review` skill at the effective head `c214d03`, reads the PR comments and the author reply into the review, and writes `docs/reviews/pr-31.md`. The review confirms the simulation version 6 and the bit-identity change under G-20.


## Session 81: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-59 merge, and answer the PR-10 questions before its code. Branch `docs/pr-59-merge-record`.

### What this session did, and why

- The owner merged PR #29 as `5ae0a24`, after a Codex review with no finding at the effective head `923e2a4` (Session 80).
- The design doc PR-59 entry reads merged, the roadmap PR-59 entry has its status line, and the sequence marks item 17.
- Asked four owner questions in one batch, and D-265 to D-268 record the answers. OQ-133 to OQ-136 hold the questions.
- D-265: until PR-15 gives the loadout a weapon, the attack bit fires the first projectile definition of the content set, in ordinal path order.
- D-266: the projectile schema gains the required field `spreadHundredths`, the half angle of the spread cone in hundredths of a degree. The Projectile stream draws the spread of each shot.
- D-267: a shot fires once per press of the attack bit: a tick where the bit is set and was clear on the tick before.
- D-268: a shot starts at the shoulder point of D-242 and flies toward the first solid cell the aim ray meets within 100 meters, or the point of the ray at 100 meters.
- The PR-10 roadmap entry holds the four files of the test-only set, the spread field, the shot rules, and the projectile state in the hash with the simulation version 6. The design doc paragraph says the same.

### State of the build

- `main` is at `5ae0a24`, the squash merge of PR #29. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-59-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures, as PR #29 left them. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.

### In flight

PR #30 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The session applies the label after the automated pass, when every comment has its answer. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- PR-10 changes the loop state: the live projectiles join the hash after the run end, in flight order, so the simulation version rises to 6 and the bit-identity hash moves (G-20). The sweep intents set the attack bit on some ticks already, so the sweep fires shots on its own once the loop reads the bit.
- PR-10 changes the projectile schema: `spreadHundredths` joins the required list, the two existing files gain a value, and the content hash of the test set moves.
- The first projectile definition of the content set is the first in ordinal path order, so the file names of the test-only set decide which one the attack bit fires (D-265).
- The aim ray march for the shot target reads the grid with `GridRay`, as the boom sweep does. A ray that meets no solid cell within 100 meters ends at the point of the ray at 100 meters (D-268).
- A shot that starts at the shoulder point can start inside rock when the player hugs a right wall (OQ-118). D-249 marches the shoulder offset first, and the shot origin takes that marched point.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #30. Then a new session starts PR-10 on a short branch: the spread field, the four test-only definitions, the projectile simulation, the arc solver, the shot from the attack bit, the projectiles in the hash with the simulation version 6, and the six exit tests, under D-159, D-231, D-242, D-247, and D-265 to D-268.


## Session 80: 2026-09-09, Codex

Author: Codex
Session: review PR-59 as PR #29 at effective head `923e2a4`. Branch `feat/pr-59-detail`.

### What this session did, and why

- Verified the provider gate. Session 79 identifies Claude Code as the author of the substantive PR-59 commits, and Codex is the eligible reviewer.
- Read the complete diff, the PR-59 roadmap entry and exit tests, the affected Core callers, content and validators, reachability, replay version use, bit-identity sweep, decisions, questions, and the PR comments and replies.
- Found no actionable defect. The detail pass preserves tunnel reachability, the water probe follows D-264, the simulation version rises to 5, and the block set follows D-259.
- Wrote `docs/reviews/pr-29.md` with the verdict `Ready for owner merge` at effective head `923e2a4`.

### State of the build

- `main` is at `45dbaf5`, the squash merge of PR #28. The reviewed effective head is `923e2a4`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 54 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.
- The Godot 4.7.2 headless build check passes.
- GitHub CI, both bit-identity platform jobs and the compare job, determinism lint, STE check, evaluate, and Gitar pass at the PR head. The review-gate result is neutral until the review record exists.

### In flight

PR #29 holds the review record and needs the metadata commit and the session handoff pushed. The owner can merge after the remote review-gate check reads `Ready for owner merge` at effective head `923e2a4`.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59 is open as PR #29. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `923e2a4`, because its design and roadmap changes are substantive under D-184. The handoff and review commits remain metadata commits.
- The review-gate result is neutral before this review record exists. The metadata commit should trigger the gate again.
- D-264 revises only the water probe wording of D-262. The half jump velocity, quarter gravity, and one-block apex still apply.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit the review record and handoff entry. Push the branch. Fetch and verify that the remote head has no ahead count and that the review-gate check reads the approved effective head.

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
- The automated pass on `787c851` approved the code with two suggestions and one CI notice. The first suggestion asked for a decision on the water probe, and the owner gave it: D-264 revises D-262 in part, and OQ-132 holds the question. The second asked for a direct assertion of the quarter gravity in the air over water, and the test has it now. The CI notice on the absent review record has no merit (D-251). The correction commit holds the decision, the test, and this note.

### State of the build

- `main` is at `45dbaf5`, the squash merge of PR #28. This branch holds the PR-59 commit `787c851` above it, and this entry above that.
- Remote head: `origin/feat/pr-59-detail` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures. The suite takes about three minutes, and the shared reachability sweep takes most of it.
- `det-lint`: 0 findings. Core 0 in 54 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #29 is open and it holds this branch. The automated pass approved `787c851`, and the correction commit answers its two suggestions. The effective head is that commit, and a Codex session reviews the PR at it. The review focus is determinism, content, test quality, and replay (roadmap PR-59). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59 is open as PR #29. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The water probe reads the column under the feet (D-264). A probe of the feet cell alone gives an apex of 1.05 blocks at 1.6 times the ticks, because the quarter gravity goes as soon as the feet rise out of the cell.
- A collapse must never fill a cell of a walker with a dependent. The first version filled cells of any walker that were dug by that walker alone, and a walker that looped back cut its own path to a chamber it dug earlier. Seed 418 of floor 14 found it.
- The pillars come last in the detail pass, so no wall block lands on a pillar and no pool opens under one. A pool cell needs air over it and rock under it.
- The pillars and the pools are cells with their floor row, not columns, because two chambers can stack on one column at two rows.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #29 per the `pr-review` skill at the effective head, reads the PR comments and the author replies into the review, and writes `docs/reviews/pr-29.md`. The review confirms the simulation version 5 and the bit-identity change under G-20.


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
