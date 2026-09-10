# Session handoff archive

## Session 88: 2026-09-10, Claude Code

Author: Claude Code
Session: the PR-11 merge record and the PR-58 questions. Branch `docs/pr-11-merge-record`.

### What this session did, and why

- Recorded the PR-11 merge and filed the two PR-58 questions.
- The owner had not answered the questions when this session ended.

### State of the build

- `main` was at `7487473`. The document branch held the merge record and the handoff entry.
- The build, tests, determinism lint, and STE check passed.

### In flight

The first scheduled night and the answers to OQ-142 and OQ-143 were in flight.

### Traps and gotchas

The night publish step uses an orphan branch and a worktree in the runner checkout.

### Open questions that block progress

OQ-142 and OQ-143 blocked PR-58.

### Next concrete action

Record the owner answers, then wait for the first night record before opening PR-58.

## Session 87: 2026-09-10, Codex

Author: Codex
Session: review PR #32 at effective head `60bdb17`. Branch `fix/review-gate-one-verdict`.

### What this session did, and why

- Verified the provider gate. Session 86 identifies Claude Code as the author of the substantive PR-32 change. Codex is the eligible reviewer.
- Recomputed the effective head. `7a388f6` holds the implementation, and `60bdb17` is the newest substantive commit because the skill file lies outside the metadata set of D-184. Later commits change only review and handoff metadata.
- Read the complete diff, the review-gate callers and tests, D-269, OQ-137, the roadmap, the PR comments and author replies, and the current remote checks.
- Found no actionable defect. Wrote `docs/reviews/pr-32.md` with the verdict `Ready for owner merge` at effective head `60bdb17`.

### State of the build

- `main` is at `1ce78c6`, the squash merge of PR #31. The effective PR-32 head is `60bdb17`.
- Local build and test pass. `dotnet build` reports 0 warnings and 0 errors. `dotnet test` reports 509 tests and 0 failures.
- `det-lint` reports 0 findings in 57 Core files and 0 Game files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` returns `d8943df12fefcbee`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.
- The code, lint, STE, Gitar, and Windows checks pass. The review-gate check waits for the review record, and the macOS checks were pending before the metadata push.

### In flight

The review record and this handoff entry are pushed in `5adec71`. The owner can merge after all platform checks pass; the remote review-gate check already reads `Ready for owner merge` at effective head `60bdb17`.

### Traps and gotchas

- The effective head is `60bdb17`, not the remote tip after the review metadata commit. D-184 excludes only the review and handoff paths.
- The parser counts verdict names in the full Verdict section. The review skill now requires the reason to name no other verdict (D-269).
- The review-gate check failed once because the record used a literal marker before its heading. Commit `5adec71` removes that marker, and the new evaluate and review-gate checks pass.

### Open questions that block progress

None. OQ-99 remains open, and it blocks nothing.

### Next concrete action

Wait for the pending platform checks. Then the owner can merge PR #32.

## Session 86: 2026-09-10, Claude Code

Author: Claude Code
Session: harden the review gate against a second verdict name (D-269). Branch `fix/review-gate-one-verdict`.

### What this session did, and why

- The owner merged PR #31 as `1ce78c6`, after a Codex repeat review with the verdict `Ready for owner merge` at the effective head `1b58d3a` (Session 85).
- The gate read that record as "Changes required" twice: the record kept a previous-verdict line inside the Verdict section, and the gate took the first verdict name after the first text match of the heading, so a history section named "Verdict history" matched as a prefix. Two format edits of the record by the author, with the owner's choice and a PR comment, let the gate read the approval.
- The owner asked how to stop that and chose three layers. The commit `7a388f6` holds them. PR #32 holds the branch.
- `ReviewRecord.FindVerdict` reads the section under the line that is exactly `## Verdict`, up to the next heading, and it fails a section with two verdict names with a message that names both in document order (D-269, F-89).
- `EveryRepositoryReviewRecordHoldsOneVerdict` parses every review record of the checkout, so a reviewer sees a second name in `dotnet test` before the push. Two more tests cover the prefix match and the two names.
- The `pr-review` skill repeat procedure keeps one verdict name in the Verdict section and puts an earlier verdict under a heading that starts with another word.
- D-269 records the rule, OQ-137 the question, and F-89 the finding.

### State of the build

- `main` is at `1ce78c6`, the squash merge of PR #31. This branch holds the fix commit `7a388f6` above it, and this entry above that.
- Remote head: `origin/fix/review-gate-one-verdict` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 509 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 57 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`, as PR #31 left it. No Core change. The simulation version is 6.

### In flight

PR #32 is open and it holds this branch. It changes the tools, so it needs a Codex review at the effective head `60bdb17`, the skill edit that answered the automated pass. The pass approved `7a388f6` with one suggestion, and every comment has its answer. The merge record of PR-10 and the PR-11 questions come in the next session, after this fix or beside it. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-10 and PR-59 are merged. PR-11 remains, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The gate now fails a review record whose Verdict section names two verdicts, and it names both. A repeat review keeps one name there and puts the earlier verdict in a section such as `## Earlier verdicts`, above it. The count reads the prose too, so the reason after the verdict names no other verdict. The automated pass raised that edge, and the skill says it now.
- The gate parses `docs/reviews/pr-31.md` with the heading `## Earlier verdicts` that the format edits gave it, and the repository test reads every record, so a record that breaks the rule fails `dotnet test` on every branch.
- The effective head is `60bdb17`, because the skill file lies outside the metadata set of D-190. This PR changes no Core file, so the bit-identity hash stands.
- The PR-10 merge record and the PR-11 questions are still to do: the design doc PR-10 entry, the roadmap status line, and the sequence item 18.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #32 per the `pr-review` skill at the effective head `60bdb17`, reads the PR comments and the author reply into the review, and writes `docs/reviews/pr-32.md`. In parallel or after, a session records the PR-10 merge and asks the PR-11 questions before its code.

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
- The repeat review approved `1b58d3a`, and the gate still read "Changes required": the record held a previous-verdict line above the new verdict, and the gate takes the first verdict name after the heading. With the owner's choice, this session moved that line into an "Earlier verdicts" section above the Verdict section. A first try named that section "Verdict history", and the gate read it as the Verdict section, because the parser matches the heading text as a prefix. The verdict text is as the reviewer wrote it, and the gate output names the editing commit (D-198).

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. This branch holds the PR-10 commit `c214d03`, the review commits, the correction `1b58d3a`, and this entry above them.
- Remote head: `origin/feat/pr-10-projectiles` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 506 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 57 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`. The simulation version is 6.

### In flight

PR #31 is open and it holds this branch. The repeat review approved the effective head `1b58d3a`, and the record reads that verdict after the format edit. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 is open as PR #31. PR-11 remains, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `1b58d3a`, and the review record names it with the verdict `Ready for owner merge`.
- The review gate reads the first verdict name after the first heading that starts with "## Verdict", so a section named "Verdict history" counts as the Verdict section. A note that names an earlier verdict belongs in a section whose heading starts with another word, above the Verdict section. A follow-up PR makes the parser match the exact heading and refuse a section with two verdict names.
- The sweep hash moved without a version change, because the version rose in this PR already. A review that sees `d8943df12fefcbee` must confirm it and not restore `3220e92dcbca55a2`.
- The spread is uniform in the angle from the axis and not in the solid angle of the cone, so shots gather near the axis less than a uniform disc would. D-266 names the half angle alone.
- The cone sides come from the world up, or from the world right for a vertical direction, so a direction within 2.6 degrees of vertical takes the second axis. The angle from the axis is exact either way.
- The reviewing provider reads the existing PR comments and author replies into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge PR #31 after the remote review-gate check reads `Ready for owner merge` at the effective head `1b58d3a`.

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

- Started PR-10 from `main` at `4687081` and added the projectile simulation, arc solver, attack-bit shot, tests, content, and roadmap note.
- The projectile spread, shot origin, fire timing, replay state, and simulation version 6 follow D-265 to D-268 and G-20.
- The six PR-10 exit tests pass, with 500 tests in total.

### State of the build

- `main` is at `4687081`. The PR-10 implementation commit is `c214d03`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 500 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings. The Godot 4.7.2 headless build check passes.
- `bit-identity`: `3220e92dcbca55a2`. The simulation version is 6.

### In flight

PR #31 is open. A Codex session reviews the PR at the effective head.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 is open. PR-11 remains, followed by M-1, M-2, and PR-58.

### Traps and gotchas

- The Projectile stream is not part of the loop state hash. A replay comparison must include a tick with a projectile in flight.
- The shot starts at the marched shoulder point. A shot from inside rock reports an error.
- The fixed-step projectile path lands a little short of the continuous arc.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #31 per the `pr-review` skill and writes `docs/reviews/pr-31.md`.

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

PR #26 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The label goes on after the automated pass, because a later push makes it stale. No other PR is open.

### Traps and gotchas

- The process since PR #23: gitar comments on every PR after a push (D-250). Answer every comment before the hand-over or the override request. A comment with no merit gets a reply and a resolve. A comment with merit gets the change, a push, and a reply. Tell the owner when the PR is ready for the other provider or for the override.
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

## Session 69: 2026-09-09, Claude Code

Author: Claude Code
Session: make the review-gate job red on a neutral verdict (D-251). Branch `fix/review-gate-red-on-neutral`.

### What this session did, and why

- The owner saw the "Review gate / evaluate" job green on PR #23 with no review record. The check run was neutral, and the job stayed green because a job cannot be neutral by its exit code (F-84).
- Asked one owner question with three options, and D-251 records the answer: the job fails on a neutral conclusion too. OQ-119 holds the question. D-181 is revised in part, the job result only.
- The last step of the workflow exits 1 on every conclusion but `success`, and it prints the summary then. `ReviewGateJobFailsOnNeutral` reads the file and fails on the old step.
- The automated pass on this PR gave one comment with one finding, with merit: the first guard read the two strings `failure` and `neutral`, so a missing conclusion passed the job green (T-2). The guard reads `success` alone now, in the second commit, and the reply on the thread names it.
- The design doc, the roadmap, the skill, and the agent files say that the job line reads red on grey. The roadmap PR-1 exit tests gain number 26.
- The workflow runs from the base branch (D-197), so this PR cannot exercise its own change. The proof runs on the first PR after the merge.

### State of the build

- `main` is at `a78e759`, the squash merge of PR #24. This branch holds two commits above it.
- Remote head: `origin/fix/review-gate-red-on-neutral` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 399 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `e8ef2b1fad938845`, the value of `main`. PR #23 moves it to `92ef27ee175b3e7e`.

### In flight

- PR #25 is open and it holds this branch. It changes `.github/`, so it needs the automated pass and then a Codex review with the verdict `Ready for owner merge` at the effective head.
- PR #23 is open on `feat/pr-8-camera`, ready for the Codex review at the effective head `32cad0d`.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Two open PRs again: this one and PR #23. Both append at the handoff top, the register end, the questions end, and the findings table. The second one to merge needs a merge from `main` first, and the handoff then holds more than ten entries, so the oldest move to the archive until ten stay.
- The workflow file on a PR comes from the base branch, so a PR that changes the workflow sees the old behavior on itself. Read the first run after the merge.
- A documentation PR with a valid override label stays green, because the override gives `success` and not `neutral`. A stale label after a push turns red, as on PR #24.
- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass runs on PR #25. Then a Codex session reviews PR #25 per the `pr-review` skill and writes `docs/reviews/pr-25.md`, with the review focus on the CI boundary and test quality. PR #23 waits for its own Codex review. The second PR to merge takes a merge from `main` first.

## Session 68: 2026-09-09, Claude Code

Author: Claude Code
Session: run the automated pass on PR #23, and bring `main` into the branch before the Codex review. Branch `feat/pr-8-camera`.

### What this session did, and why

- The owner set the automated review pass of gitar (D-250), and PR #24 recorded it. The first pass on PR #23 gave one comment: approved, no issue. No change followed.
- Merged `main` at `a78e759` into this branch, so the reviewer's checkout holds the `pr-review` skill with the gitar rule. The handoff and the register hold both sides in order: Session 67 above Session 66, and D-248, D-249, D-250.
- The merge conflicted in two files, at the places both branches added to: the top of the handoff and the rows after D-247. The archive merged on its own, because both sides moved Session 56 with one text.
- This entry makes twelve, so Sessions 58 and 57 move to the archive (D-146).
- The effective head of PR #23 is the merge commit `32cad0d`, because the merge brings the skill and the agent files. The code of PR-8 is unchanged since `e6d40e0`, and the bit-identity value stands.

### State of the build

- `main` is at `a78e759`, the squash merge of PR #24. This branch holds the PR-8 commit, the merge, and this entry above it.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 424 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 42 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `92ef27ee175b3e7e`, as PR-8 set it (G-20).

### In flight

PR #23 is open and it holds this branch. gitar approved the PR-8 head, and the pass runs again on this push. The PR changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250). The review record gains a `## PR comments` part, and the skill on this branch holds the rule.
- The effective head is the merge commit `32cad0d` and not `e6d40e0`. The review record names the merge commit.
- The hash moves in PR-8, and that is the point of G-20. A review that sees `92ef27ee175b3e7e` must confirm it and not restore `e8ef2b1fad938845`.
- The loop hash reads the buttons, so two loops that differ in the controller aim bit alone give two hashes and one camera pose.
- The march tie order is X, then Y, then Z, so a line through a corner visits the cell at the corner. The fine-walk test allows one millimeter for that.
- Two open PRs that both add a handoff entry conflict at the top of the file, and the second one to merge needs a merge from `main` first. Merge before the review, so the review head stands.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #23 per the `pr-review` skill, at the effective head `32cad0d`, reads the existing PR comments into the review, and writes `docs/reviews/pr-23.md`. The review focus is determinism, replay, and test quality, and it confirms the simulation version 3 and the bit-identity change under G-20.

## Session 67: 2026-09-09, Claude Code

Author: Claude Code
Session: record the automated review pass of gitar (D-250), and run its first cycles on PR #23 and on this PR. Branch `docs/gitar-review-pass`.

### What this session did, and why

- The owner added an automated reviewer, gitar, that comments on every PR after a push, and gave the rule for it. D-250 records the rule as an owner instruction.
- The `pr-review` skill gained two procedures: "The automated pass" for the author, and "Do not address the automated reviewer" for the reviewing provider. The review record skeleton gained a `## PR comments` part, and the scope limits name a reply to gitar as an external message.
- The agent files gained the "Automated review pass" section and a gate line. Design section 3.14 names the pass.
- The first pass on PR #23 gave one comment: approved, no issue. PR #23 is ready for the Codex review with no change.
- The pass on this PR gave one comment with one finding: the register jumps from D-247 to D-250. No merit. D-248 and D-249 live in PR #23, and ids never change, so D-250 stays. The reply on the thread says so, and the thread is resolved. The owner asked for the reply alone and no note in the register.
- This branch comes from `main`, so it holds neither D-248, nor D-249, nor Session 66. Each of those lands with PR #23.

### State of the build

- `main` is at `9209fb5`, the squash merge of PR #22. This branch holds three document commits above it.
- Remote head: `origin/docs/gitar-review-pass` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 398 tests, 0 failures, on the code of `main`.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `e8ef2b1fad938845`, the value of `main`. PR #23 moves it to `92ef27ee175b3e7e`.

### In flight

- PR #24 is open and it holds this branch. It changes documents, skills, and the agent files alone, so the `review-override` label covers it (D-190). The owner added the label before the second commit, so the label needs a new add after the last push.
- PR #23 is open on `feat/pr-8-camera`, with the effective head `e6d40e0`. It is ready for the Codex review.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Two open PRs both add a handoff entry at the top of the file and a decision row after D-247, and the second one to merge needs a merge from `main` first. After this PR merges, merge `main` into `feat/pr-8-camera` and order the entries and the rows by number: Session 68, 67, 66, and D-248, D-249, D-250.
- The override label is stale after any push outside the metadata set (D-190). Add it after the last push, and not before.
- gitar posts one comment on the PR with its verdict inside a details block, and one review thread on the line of each finding. The thread has a GraphQL node id, and `addPullRequestReviewThreadReply` answers it. The REST list of pull comments was empty while the pass still ran.
- A reply to gitar names no provider (T-6). It states the evidence and the commit. A gap or a fact that an open PR explains gets the reply alone, and no note in a register.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner adds the `review-override` label to PR #24 again and merges it. Then merge `main` into `feat/pr-8-camera`, order the entries and the rows by number, and add the Session 68 entry that records the pass on PR #23. A Codex session then reviews PR #23 per the `pr-review` skill, reads the existing PR comments into the review, and never addresses gitar (D-250).

## Session 66: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-8, the orbit camera, the ray march, the aim ray, and aim assist. Branch `feat/pr-8-camera`.

### What this session did, and why

- The owner merged PR #22 as `9209fb5`. The handoff named PR-8 as the next action, under D-241 to D-247.
- Asked two owner questions in one batch, and D-248 and D-249 record the answers. OQ-117 and OQ-118 hold the questions.
- D-248: a positive pitch looks up.
- D-249: the camera runs two marches. The first pulls the shoulder point in at a wall, and the second runs the boom. The shoulder point of D-242 sits inside rock when the player hugs a right wall, and the pivot never does.
- Wrote `GridRay`, the grid traversal of Amanatides and Woo, with a tie order of X, then Y, then Z. Wrote `CameraPose`, `OrbitCamera`, `AimRay`, and `AimAssist`. `Vector3` gained subtraction, a scale, the dot and cross products, and the length.
- The loop gives `Camera()` and `Aim(targets)` on demand, and neither is state (D-245). The pitch clamp is 8000 (D-241), and bit 8 is the controller aim flag (D-243).
- The assist is a spherical interpolation by the strength, with the angle from `Atan2` of the cross length and the dot product, so a small angle stays exact.
- The simulation version is 3, and the bit-identity hash moved from `e8ef2b1fad938845` to `92ef27ee175b3e7e` on purpose (G-20). The sweep folds in the camera pose and the aim ray of every tick.
- No allowlist entry. 26 new tests, 424 in total. Opened PR #23.

### State of the build

- `main` is at `9209fb5`, the squash merge of PR #22. This branch holds one commit above it.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 424 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 42 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files.
- `bit-identity`: `92ef27ee175b3e7e`. PR-8 moved it from `e8ef2b1fad938845` on purpose, and `BitIdentityKnownAnswer` pins the new value.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #23 is open and it holds this branch. It changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The hash moves in this PR, and that is the point of G-20. A review that sees the new value must confirm it and not restore the old one.
- The loop hash reads the buttons, so two loops that differ in the controller aim bit alone give two hashes and one camera pose. A test that expects one hash there fails.
- The march tie order is X, then Y, then Z. A line through a corner visits the cell at the corner, so the march can hit a block that a point sample of the line misses. The fine-walk test allows one millimeter for that.
- A miss carries the whole segment length as its distance, and a zero-length segment carries zero.
- The camera range check reads the loop ranges. A yaw of 36000 or a pitch of 8001 is an error, and a test that builds a pose by hand stays inside them.
- The sweep grid holds one-block steps and two-block pillars, and the boom meets them on many ticks. A change to `ReplayGrid` or to the sweep targets moves the hash.
- `StateHash` has no `==` operator. The camera tests compare `.Value`.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #23 per the `pr-review` skill, at the effective head, and writes `docs/reviews/pr-23.md`. The review focus is determinism, replay, and test quality, and it confirms the simulation version 3 and the bit-identity change under G-20.

## Session 65: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-7 merge, and answer the PR-8 questions before its code. Branch `docs/pr-7-merge-record`.

### What this session did, and why

- The owner merged PR #21 as `d5f20ce`, after a Codex review with no finding.
- Audited every document against the merged state. The registers were complete: D-235 to D-240, OQ-104 to OQ-109, and F-82 and F-83 all landed with the PR.
- Three status lines were stale. The design doc PR-7 entry and its sequence line said open, and the focused roadmap said open. All three name the merge now.
- The handoff held ten entries, because Session 64 archived one. This entry makes eleven, so Session 55 moves to the archive (D-146).
- Asked seven owner questions in two batches, and D-241 to D-247 record the answers. OQ-110 to OQ-116 hold the questions.
- D-241: the pitch limit is plus or minus 80 degrees for the loop and the camera. D-227 is revised in part, the pitch limit only.
- D-242: the pivot sits 1.5 meters over the feet, the shoulder point is 0.6 right and 0.3 up, and the boom is 3 meters.
- D-243: bit 8 of the buttons marks a controller aim on that tick. D-232 is revised in part, bit 8 only.
- D-244: aim assist pulls the ray toward the nearest target inside a 5 degree cone, by half the angle.
- D-245: Core derives the camera on each tick, and the camera holds no state.
- D-246: the boom sweep is a ray march along the line, with a camera radius of 0.25 meters.
- D-247: the aim ray starts at the camera, along the look direction.
- The roadmap PR-8 scope cites the seven decisions, and it names the simulation version rise to 3 (G-20).

### State of the build

- `main` is at `d5f20ce`, the squash merge of PR #21. This branch holds the document commit above it.
- Remote head: `origin/docs/pr-7-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 398 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `e8ef2b1fad938845`.

### In flight

PR #22 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- PR-8 changes the loop pitch clamp from 9000 to 8000 (D-241). `PitchClamps` in `SimulationTests`, the `PitchLimit` constant, and the D-227 remark in `SimulationLoop` change with it, and the simulation version rises to 3.
- Bit 8 joins the assigned set (D-243). `Button.AssignedMask` becomes 0x01FF and `Button.ReservedMask` becomes 0xFE00. The random intent helper of the tests and the button mask of the bit-identity sweep follow, or the reserved-bit check throws.
- The boom sweep is a ray march and not the PR-7 sweep (D-246). The PR-7 sweep runs one axis at a time and follows a staircase, so a diagonal boom through it ends beside the line.
- The camera holds no state (D-245). The exit test hashes the aim ray and the camera position after a replay, and the loop hash gains no field.
- The bit-identity hash moves again in PR-8, on purpose, and `BitIdentityKnownAnswer` pins the new value (G-20).
- The pitch sign is not in any decision. PR-8 states which sign looks up, in the camera remark and in a test.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #22 with the `review-override` label. Then a new session starts PR-8 on a short branch: the orbit camera, the ray march of the boom, the aim ray, aim assist, and the five exit tests, under D-13, D-14, D-75, D-77, D-88, and D-241 to D-247.

## Session 64: 2026-09-09, Codex

Author: Codex
Session: review PR-7 for the voxel grid, swept box, and player body. Branch `feat/pr-7-world-collision`.

### What this session did, and why

- Verified PR #21 against `main` at `01e68c3` and reviewed effective head `4207548`.
- Confirmed that Session 63 identifies Claude Code as the implementation provider. Codex is the eligible reviewer.
- Inspected the complete implementation, test, tool, and document diff, affected callers, replay paths, Core boundary, and PR-7 contracts.
- Confirmed the simulation version 2 change and the intentional bit-identity change from `283aa4b8cd1281be` to `e8ef2b1fad938845` under G-20.
- Found no blocking defect. Wrote `docs/reviews/pr-21.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` is at `01e68c3`. The reviewed effective head is `4207548`.
- `dotnet build` passes with 0 warnings and 0 errors. `dotnet test` passes with 398 tests and 0 failures.
- `det-lint` passes with 0 findings. `ste-check` passes with 0 findings.
- `bit-identity` gives `e8ef2b1fad938845`.
- The Godot 4.7.2 headless build check passes.
- Remote head: `origin/feat/pr-7-world-collision` at `bd57f34`, which holds the review record and this handoff entry.

### In flight

PR #21 is open with the verdict `Ready for owner merge` at effective head `4207548`. The owner can merge it.

### Traps and gotchas

- PR-7 raises the simulation version and changes the bit-identity value on purpose. Keep both values when the owner merges the PR.
- The sweep uses a contact skin and runs Y, then X, then Z. A face on a block is contact, not overlap.
- The loop and replay take the grid and spawn from the caller until PR-9, as D-236 requires.

### Open questions that block progress

None. OQ-99 remains open, and it blocks nothing.

### Next concrete action

The owner can merge PR #21. A new session starts after the owner merges it.

## Session 63: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-7, the voxel grid, the swept box, and the player body. Branch `feat/pr-7-world-collision`.

### What this session did, and why

- The owner merged PR #20 as `01e68c3`. The handoff named PR-7 as the next action, under D-164, D-165, and D-231 to D-234.
- A float check before the code refuted an exact contact on a block face. For twelve integer faces below 130, `(c - h) + h` is one ulp off (F-82). That made the position representation an owner question.
- Asked six owner questions in two batches, and D-235 to D-240 record the answers. OQ-104 to OQ-109 hold the questions.
- D-235: float meters, feet center, and a contact skin of 2^-10 meters before every face. The ground is a probe two skins below the feet.
- D-236: the loop and the replayer take the grid and the spawn point from the caller until PR-9.
- D-237: a cell outside the grid is solid, and a direct read there is an error.
- D-238: the intent sets the horizontal velocity on every tick, in the air too.
- D-239: air and raw stone are the two block ids of PR-7.
- D-240: no terminal velocity.
- Wrote `VoxelGrid`, `BlockId`, `Vector3`, `Aabb`, `Axis`, `SweptAabb`, `Button`, and `PlayerBody`. The loop holds the grid and the body, and the hash adds the position and the vertical velocity after the five fields of D-227.
- The sweep runs Y, then X, then Z, and a test pins the order. It reads every cell in the path, so a move of 20 meters in one tick still stops at the first block.
- The simulation version is 2, and the bit-identity hash moved from `283aa4b8cd1281be` to `e8ef2b1fad938845` on purpose (G-20).
- The fixed-step apex of a jump is 1.17 meters, and D-231 said 1.23. D-231 carries the dated correction, and F-83 records it.
- No allowlist entry. 62 new tests, 398 in total. Opened PR #21.

### State of the build

- `main` is at `01e68c3`, the squash merge of PR #20. This branch holds two commits above it.
- Remote head: `origin/feat/pr-7-world-collision` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 398 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files.
- `bit-identity`: `e8ef2b1fad938845`. PR-7 moved it from `283aa4b8cd1281be` on purpose, and `BitIdentityKnownAnswer` pins the new value.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #21 is open and it holds this branch. It changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-6 are merged. PR-7 is open. PR-8 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The hash moves in this PR, and that is the point of G-20. A review that sees the new value must confirm it and not restore the old one.
- A float contact cannot sit on a block face. Twelve integer faces below 130 are one ulp off after `(c - h) + h`, at binade edges such as 16 and 128 (F-82). The sweep stops one skin before a face, and a test that asserts a face on the block fails.
- A box on a face is contact and not overlap. `Overlaps` reads the open interval, and `-Floor(-max) - 1` names the cell below a face that sits on a block boundary.
- The sweep runs Y first. A diagonal move toward a one-block step falls beside the step and stops at its wall, and the other order lands on top. `TheOrderIsYThenXThenZ` pins it.
- The fixed-step apex is 1.17 meters and not 1.23. Assert the one-block clear and the two-block fail, and never the apex.
- The random intent helper masks the buttons to the eight assigned bits. A test that passes a random `ushort` throws on a reserved bit (D-232).
- The loop constructor takes a grid and a spawn. A test that wrote `new SimulationLoop(seed)` uses `TestWorld.NewLoop(seed)`.
- The record does not carry the grid. A replay with another grid gives another hash, and the gap closes in PR-9 (D-236).
- `StateHash` has no `==` operator. Compare `.Value`, or use `Assert.Equal`.
- The Godot check prints `[ DONE ] dotnet_build_project` on a pass. The shell variable `PIPESTATUS` is a bash name, and zsh reads `pipestatus`, so read the line and not the exit code through a pipe.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #21 per the `pr-review` skill, at the effective head, and writes `docs/reviews/pr-21.md`. The review focus is determinism, the Core boundary, and test quality, and it confirms the simulation version 2 and the bit-identity change under G-20.

## Session 62: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-6 merge, and answer the PR-7 questions before its code. Branch `docs/pr-6-merge-record`.

### What this session did, and why

- The owner merged PR #19 as `3f7e3c2`, after a Codex review with no finding.
- Audited every document against the merged state. The registers were complete: D-224 to D-230, OQ-92 to OQ-99, and F-80 and F-81 all landed with the PR.
- Two status lines were stale. The design doc PR-6 entry and its sequence line said open, and the focused roadmap said open. All three name the merge now.
- The handoff held eleven entries. Session 61 added its entry and moved none to the archive. This session moved sessions 51 and 52 to the archive, so the file holds ten again (D-146).
- Asked four owner questions that block the PR-7 code, and D-231 to D-234 record the answers. OQ-100 to OQ-103 hold the questions.
- D-231: gravity 20, walk 4, sprint 6.5, jump 7. The apex is 1.23 meters, so a jump clears one block and never two (D-165).
- D-232: the button bits. Bit 0 is jump, bit 1 is sprint, and bits 2 to 7 hold dodge, attack, use, interact, and the two quick slot moves. Bits 8 to 15 are reserved, and a set one is an error.
- D-233: the movement is strafe and forward, each over 127, rotated by the yaw sum, with the length clamped to 1.
- D-234: Core uses the frame of Godot. Right-handed, Y up, meters, and forward at yaw zero is minus Z.
- The roadmap PR-7 scope cites the four decisions, and it names the simulation version rise to 2 (G-20).

### State of the build

- `main` is at `3f7e3c2`, the squash merge of PR #19. This branch holds the document commits above it.
- Remote head: `origin/docs/pr-6-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 336 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 29 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `283aa4b8cd1281be`.

### In flight

PR #20 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-6 are merged. PR-7 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- A review session adds an entry and can leave eleven in the handoff. Count the entries at the start of a session, and archive down to ten.
- PR-7 raises the simulation version to 2 and moves the bit-identity hash, because the state gains a position. `BitIdentityKnownAnswer` pins the number, and the PR updates it on purpose (G-20).
- The apex of a jump under fixed-step Euler differs from the closed form by a fraction of a tick. Assert the one-block clear and the two-block fail in a test, and never the apex value alone.
- The movement fraction of -128 clamps to -127, so the two directions have one magnitude.
- The state hash order of D-160 grows in PR-7. Add the new fields after the five of D-227, so the order test of PR-6 still reads.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #20 with the `review-override` label. Then a new session starts PR-7 on a short branch: the voxel grid, the swept box, the player body, and the five exit tests, under D-164, D-165, and D-231 to D-234.

## Session 61: 2026-09-09, Codex

Author: Codex
Session: review PR #19 for the simulation loop, the intent frame, the run record, and the replay. Branch `feat/pr-6-loop-record`.

### What this session did, and why

- Verified PR #19 against `main` at `c11fb41` and reviewed effective head `0850d32`.
- Confirmed that Session 60 identifies Claude Code as the implementation provider. Codex is the eligible reviewer.
- Inspected the complete diff, callers, tests, replay contracts, error paths, Core boundary, and Phase 1 documents.
- Confirmed the intentional bit-identity change from `4d6385bb92454694` to `283aa4b8cd1281be`.
- Found no blocking defect. Wrote `docs/reviews/pr-19.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` is at `c11fb41`. The reviewed effective head is `0850d32`.
- The branch held metadata commit `0850d32` before this review. Review commit `1247769` is on the remote branch.
- `dotnet test` passes with 336 tests and 0 failures.
- `det-lint` passes with 0 findings. `ste-check` passes with 0 findings.
- `bit-identity` gives `283aa4b8cd1281be`.
- The Godot 4.7.2 headless build check passes.
- The local build command stalled without compiler output. The Linux, macOS, and Windows CI build and test jobs pass.

### In flight

PR #19 is open with the verdict `Ready for owner merge` at effective head `0850d32`. The owner can merge it after the review record reaches the remote branch.

### Traps and gotchas

- The effective head is `0850d32` because that commit changes design and roadmap files outside the D-184 metadata set. The later review commits do not change the effective head.
- The bit-identity value changes on purpose because the sweep replays one fixed record.
- The simulation version stays at 1 because PR-6 sets its first value.

### Open questions that block progress

None. OQ-99 remains open, and it blocks nothing.

### Next concrete action

The owner merges PR #19, or requests a review of a new effective head.

## Session 60: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-6, the simulation loop, the intent frame, the run record, the recorder, and the replay. Branch `feat/pr-6-loop-record`.

### What this session did, and why

- The owner merged PR #18 as `c11fb41`. The handoff named PR-6 as the next action, with two questions before the code.
- Asked seven owner questions in three batches, and D-224 to D-230 record the answers. OQ-92 to OQ-98 hold the questions.
- D-224: Core holds a table-driven CRC-32 in `Crc32.cs`. No dependency and no allowlist entry.
- D-225: the recorder writes through an `IRunRecordSink`, as the logger and the content loader do. Core opens no file.
- D-226: the design doc said "length-prefixed, checksummed tick frames" in sections 3.9 and 7, and D-162 fixes the frame at 16 bytes. Both lines name the fixed frame now (F-80).
- D-227: the Phase 1 loop state is the seed, the tick, the yaw and pitch sums in hundredths of a degree, and the buttons. The loop reads no movement byte until PR-7 has collision.
- D-228: the torn-tail log line names floor 1, because every run starts there and a Phase 1 run never leaves it. PR-31 reads the floor from the state.
- D-229: the JSON reader gives an empty list and a null as kinds, so the header carries `loadout`, `tree`, and `amulet` from the first record. A list with an item is an error until Phase 3.
- D-230: 1 type and 6 members enter the allowlist. A removal check proved each one in use.
- Found one defect outside the new files. The validator message wrote an enum value inside an interpolated string, and the runtime formats one through its metadata. An explicit switch replaces it (F-81), and OQ-99 asks whether `det-lint` gains a rule in its own PR.
- The bit-identity sweep records and replays one fixed run now, so the three platforms compare the replay (exit test 7). The known answer moved on purpose (G-20).
- 56 new tests. The total is 336. Opened PR #19.

### State of the build

- `main` is at `c11fb41`, the squash merge of PR #18. This branch holds two commits above it.
- Remote head: `origin/feat/pr-6-loop-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 336 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 29 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files.
- `bit-identity`: `283aa4b8cd1281be`. PR-6 moved it from `4d6385bb92454694` on purpose, and `BitIdentityKnownAnswer` pins the new value.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #19 is open and it holds this branch. It changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-5 are merged. PR-6 is open. PR-7 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The bit-identity hash moves in this PR, and that is the point of G-20. A review that sees the new value must confirm it and not restore the old one.
- The simulation version stays at 1. It is the first value, so there is no bump to confirm. The next Core PR that moves a simulation number raises it to 2.
- An intent delta is an `int16`, so a yaw near 360 degrees takes two intents in a test. The first draft of `YawWraps` passed 35990 as one delta, and the compiler stopped it.
- The content error text says "file" now, not "content file", because the run record header goes through the same reader. A test that asserts the old words fails.
- The validator names a kind with an explicit switch now. A test that asserts the enum name `Number` fails, and one that asserts `number` passes.
- A Python patch script cannot hold a C# raw string literal inside a Python triple-quoted string. Write the script to a file, or escape the quotation marks.
- The replayer adds the frame index to an error from the decode or the loop and rethrows the same object. The message then holds the tick, both checksums or both ticks, and the frame.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing: the lint rule for an enum inside an interpolation belongs to its own PR.

### Next concrete action

A Codex session reviews PR #19 per the `pr-review` skill, at the effective head, and writes `docs/reviews/pr-19.md`. The review focus is determinism, replay, errors, and test quality, and it confirms the bit-identity change under G-20.

## Session 59: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-5 merge, and prepare the documents for a fresh session. Branch `docs/pr-5-merge-record`.

### What this session did, and why

- The owner merged PR #17 as `e0deb94`, after a Codex review approved the corrected head.
- Audited every document against the merged state. The registers were complete: D-219 to D-223, OQ-88 to OQ-91, and F-78 and F-79 all landed with the PR.
- Three status lines were stale. The focused roadmap said that PR-5 was open, the design doc marked it open, and the Phase 1 sequence did not mark it. All three name the merge now, and the correction passes record the PR-5 outcome.
- The owner then asked for a full document check before a fresh session. That check found three defects in the agent files, which are the first files that a session reads.
- The Godot command in the agent files could not run. The name `Godot` is not on the command path of this machine, and the command needs `/Applications/Godot_mono.app/Contents/MacOS/Godot`.
- The agent files named no `det-lint` command and no `bit-identity` command, and both are required gates. Both are in the command list now.
- The PR gate said "the lint tool and the STE checker pass" as one line. It holds one line for each check now, and the `det-lint` line names G-8 and G-21 beside G-2.
- Added a code rule for the allowlist. A new entry needs a decision, and a session verifies it by removing the entry and running `det-lint`. That check found two dead entries and one live gap in PR-4, and a reading of the list found neither.
- Ran every command in the agent files word for word. All six pass.

### State of the build

- `main` is at `e0deb94`, the squash merge of PR #17. This branch holds two commits above it.
- Remote head: `origin/docs/pr-5-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 280 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 21 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes with the full path above.

### In flight

PR #18 is open and it holds this branch. It changes `docs/`, `CLAUDE.md`, and `AGENTS.md`, and every one of those paths is in the eligible set, so the `review-override` label covers it (D-190). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-5 are merged. PR-6 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- `dotnet test` runs the STE checker over every document, through `RepositoryDocumentsPass`. A document edit needs the test suite, and the checker alone passes a sentence that the suite rejects.
- The session number check of D-187 compares the numbers in one file. Fetch the remote and read the handoff again before the entry, because the other provider adds an entry while a session works.
- `det-lint` reports two counts now. The Game count is 0 files today, because the Game project holds no source file. The string rule has fixture tests until a PR writes Game code.
- The Game string rule is syntactic. A `Get` call takes an id only when its receiver names the string table, and the PR #17 response states that limit.
- An allowlist entry needs a removal check. A reading of the list finds neither a dead entry nor a gap.
- PR-6 is the first PR that makes a simulation number, so it moves the bit-identity hash. `BitIdentityKnownAnswer` pins that number, and the PR updates it on purpose (G-20).

### Open questions that block progress

No open question blocks PR-6 to PR-11. OQ-1, OQ-14, and the later ids belong to Phase 2 and beyond.

### Next concrete action

The owner merges PR #18 with the `review-override` label. Then a new session starts PR-6: the fixed-step loop at 60 Hz, the 16-byte intent frame, the run record, the recorder, and the replay (D-73, D-151, D-162, D-163, G-5). Two questions come before that code. The first is the checksum of D-162, which names CRC32 and no implementation, and the allowlist holds none. The second is whether the recorder writes through a sink, as the logger does under D-211 and the content loader does under D-219.

## Session 58: 2026-09-08, Codex

Author: Codex
Session: repeat review PR #17 for PR-5. Branch `feat/pr-5-content`.

### What this session did, and why

- Re-reviewed PR #17 at effective head `7143fd3` against base and merge base `a37f0af`.
- Confirmed the provider gate. Sessions 55 and 57 identify Claude Code as the author and correction author. Codex is the eligible reviewer.
- Verified the three fixes. The hash frames each file. The JSON reader keeps number text until validation. The string rule checks the receiver of `Get`.
- Verified the new regression tests and found no new in-scope defect.
- Updated `docs/reviews/pr-17.md` with fixed statuses and the verdict `Ready for owner merge`.

### State of the build

- `main` is at `a37f0af`. The reviewed effective head is `7143fd3`.
- Remote head: `origin/feat/pr-5-content` is `9d99dd0`, verified after the review metadata commit.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 280 tests and 0 failures.
- `det-lint` reports 0 findings. `ste-check` reports 0 findings. `bit-identity` gives `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes. The build, test, comparison, lint, and STE checks pass at `7143fd3`.

### In flight

The repeat-review record is published. The review-gate and evaluate checks pass.

### Traps and gotchas

- The effective head is the correction commit `7143fd3`. Review metadata commits do not change it.
- The prior review-gate failure named the old effective head. The updated record must name `7143fd3`.
- The response keeps the syntactic limit of the Game string rule because this PR has no Game source file or engine compilation path.

### Open questions that block progress

No owner question is needed. No open question blocks PR-6 to PR-11.

### Next concrete action

The owner can merge PR #17.

## Session 57: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #17 review. Branch `feat/pr-5-content`.

### What this session did, and why

- Read the three P2 findings in `docs/reviews/pr-17.md`. Each one reproduces, so each one has full merit.
- P2-1 is the most serious. The content hash appended each path and each byte sequence with no length, so the path `a` with the bytes `bc` and the path `ab` with the byte `c` gave one hash. Two content sets shared one hash, and this hash exists to tell two sets apart. Each file enters the input with a length, its path, a length, and its bytes now (F-78).
- P2-2. A fractional or out-of-range number raised a `FormatException`, and the catch held `JsonException` alone, so the error left Core with no file and no field. The reader keeps the token text through `ValueSpan` now, and the validator names the file, the field, and the reason. That is the contract that D-220 states (F-78).
- P2-3. The string rule exempted every method named `Get`, so `inventory.Get("You died")` gave no finding. A `Get` call takes an id only when its receiver names the string table now (F-79).
- The P2-3 correction stays syntactic, and the response states the limit. The Game project needs the engine assemblies for a symbol read, and it holds no source file yet.
- 10 new tests. The total is 280.

### State of the build

- `main` is at `a37f0af`. The branch holds the PR-5 work, two review commits, and this correction commit.
- Remote head: `origin/feat/pr-5-content` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 280 tests and 0 failures.
- `det-lint` reports 0 findings: Core 0 in 21 files, Game 0 in 0 files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A hash over a concatenation needs a boundary for each part. Without a length the path and the bytes run together, and two sets share one input.
- A platform conversion raises its own error type, and a catch of one type misses another. `GetInt64` raises `FormatException`, and the catch held `JsonException`.
- A method name is not a method. Every type can hold a `Get`, and the rule needs the receiver.
- `System.Array` returned to the allowlist. PR-4 left it out because Core used no array member, and the P2-1 correction reads one. That is D-207 working as written.

### Open questions that block progress

No new owner question. No open question blocks PR-6 to PR-11.

### Next concrete action

A Codex session re-reviews PR #17 per the repeat review procedure and updates `docs/reviews/pr-17.md` to the new effective head.

## Session 56: 2026-09-08, Codex

Author: Codex
Session: review PR #17 for PR-5. Branch `feat/pr-5-content`.

### What the session did, and why

- Reviewed PR #17 at effective head `614ce49` against base and merge base `a37f0af`.
- Confirmed the provider gate. Session 55 identifies Claude Code as the author, and Codex is the eligible reviewer.
- Inspected the content source, JSON reader, validators, typed records, content hash, string table, Game string lint rule, tests, content files, and project documents.
- Found three P2 defects. The content hash has ambiguous file framing. A malformed number can escape without content context. The string lint rule exempts unrelated `Get` methods.
- Wrote `docs/reviews/pr-17.md` with the findings and the verdict `Changes required`.

### State of the build

- `main` is at `a37f0af`. The reviewed effective head is `614ce49`.
- Remote head: `origin/feat/pr-5-content` is `ace29d4`, verified after the review commit.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 270 tests and 0 failures.
- `det-lint` reports 0 findings. `ste-check` reports 0 findings. `bit-identity` gives `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes. All applicable GitHub build, test, lint, STE, comparison, and bit-identity checks pass at `614ce49`.

### In flight

PR #17 needs corrections for P2-1, P2-2, and P2-3, followed by a repeat review.

### Traps and gotchas

- Hash each path and byte sequence with unambiguous boundaries. Concatenation alone can give one input to two content sets.
- `Utf8JsonReader.GetInt64()` can throw `FormatException` for a JSON number that does not fit `Int64`. Catch or avoid that conversion before the contextual error boundary.
- The string rule must distinguish `Strings.Get` from an unrelated method named `Get`.
- The Game directory has no source file yet. The fixture rule must still falsify false exemptions.

### Open questions that block progress

No owner question is needed. The author can correct all three findings within the current scope.

### Next concrete action

The author corrects the three findings and requests a repeat review at the new effective head.

## Session 55: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-5, the content loader, the schemas, the content hash, and the string table. Branch `feat/pr-5-content`.

### What this session did, and why

- Started PR-5 after the owner merged PR #16. Four owner questions came before the code, and D-219 to D-223 record the answers.
- D-219: Core takes the bytes from an `IContentSource` and opens no file. This applies D-211 one PR later, and it keeps a directory enumeration order out of the content hash.
- D-220: `Utf8JsonReader` reads the JSON. It uses no serializer and no reflection, and it gives the position that D-92 needs for an error message. A hand-written parser holds the number, escape, and surrogate rules, and PR-4 took three review passes on the escape rules alone.
- D-221: SHA-256 for the content hash, and FNV-1a stays for the state hash. One needs resistance, and the other needs speed.
- D-222: one command, two rule sets. `det-lint` reads Core with the determinism rules and Game with the string rule of G-8.
- D-223 records the allowlist additions: 9 types and 27 members. The lists hold 27 types and 53 members now.
- Wrote `IContentSource`, `ContentFile`, `JsonObjectReader`, `ContentValidator`, `ContentError`, `ContentHash`, `Strings`, `FloorTemplate`, `ProjectileDefinition`, and `ContentLoader` in `Core/Content/`.
- Wrote the first content: three floor templates for the three bands of D-210, two projectile definitions, and the string table.
- Wrote `GameStringScan` in the lint tool, and the command reports the two counts.
- 36 new tests. The total is 270. Exit tests 1 to 6 each have a test.
- The tests caught one real defect. A malformed file let `JsonReaderException` out of Core, and that error names no file. Every content failure names the file, the field, and the reason now (D-92, T-2).

### State of the build

- `main` is at `a37f0af`. The branch holds the PR-5 work above it.
- Remote head: `origin/feat/pr-5-content` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 270 tests and 0 failures.
- `det-lint` reports 0 findings: Core 0 in 21 files, Game 0 in 0 files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. PR-5 adds no simulation number.

### In flight

PR-5 waits for a Codex review (T-4). It changes code, so no override applies.

### Traps and gotchas

- A platform reader raises its own error type, and that error names no file. Wrap it, or the file name never reaches the owner.
- The Game project holds no source file yet, so the Game scan reads nothing on this checkout. The rule has fixture tests, and `LintPassesGame` guards the real directory.
- The three floor bands must cover floors 1 to 15 with no gap and no overlap. `EveryContentFileLoads` counts each depth.
- An optional content field needs a default that the record states. `areaCentimetres` is zero when the file omits it.
- A `Utf8JsonReader` is a ref struct, and it lives inside the try block that catches its error.

### Open questions that block progress

No new owner question. No open question blocks PR-6 to PR-11.

### Next concrete action

A Codex session reviews PR-5 per `.claude/skills/pr-review/SKILL.md`, under the scope rules of D-209, and writes `docs/reviews/pr-<number>.md`.

## Session 54: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-4 merge across the documents. Branch `docs/pr-4-merge-record`.

### What this session did, and why

- The owner merged PR #15 as `1749463`, after a Codex review approved effective head `94afeea`.
- Audited every document against the merged state. The registers were complete: D-211 to D-217, OQ-82 to OQ-86, and F-71 to F-77 all landed with the PR.
- The owner then answered the one open item of PR-4. D-218 ratifies the log name and value rule, and OQ-87 records the question.
- Three lines were stale, and this session corrects each one. The focused roadmap had no status line for PR-4, and its header cited D-200 to D-216 while the PR added D-217. The design doc marked PR-4 as open, in the marker and in the Phase 1 sequence.
- The correction passes of the roadmap now record the PR-4 outcome: five owner questions before the code, three review passes on the logger, and the description rule of D-217.
- The build on `main` is healthy: 234 tests, 0 lint findings, 0 checker findings.

### State of the build

- `main` is at `1749463`, the squash merge of PR #15. This branch holds one commit above it.
- Remote head: `origin/docs/pr-4-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`.

### In flight

This PR changes only `docs/`, so the `review-override` label covers it (D-190). PR-5 starts after the merge.

### Traps and gotchas

- The owner ratified the PR-4 name and value rule as D-218. A field name is an identifier and must hold valid text. A value and a message take the replacement character, because a throw on content loses a crash report at the moment that the owner needs it most.
- D-218 reaches the registers and the two plan documents in this PR. The code comments cite F-77, and F-77 cites D-218, so this PR changes no code and keeps the override.
- PR-5 needs owner answers before its code. The content loader reads files, and D-211 kept `System.IO` out of Core for the logger. The loader also needs a JSON reader and SHA-256, and the D-207 list holds neither. The lint rule of G-8 also reads the Game project, and the tool reads Core alone today.
- A merged PR leaves a status line in three places: the focused roadmap, the design doc marker, and the Phase 1 sequence.

### Open questions that block progress

No open question blocks PR-5 from starting. Four questions come before its code, and the next session files them.

### Next concrete action

The owner merges this documentation PR with the `review-override` label. It carries the merge record and D-218, and the owner asked for one PR. Then a new session starts PR-5: the content loader, the schemas, the content hash, and the string table (D-91, D-92, D-98, D-163, D-168).

## Session 53: 2026-09-08, Codex

Author: Codex
Session: third repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `94afeea` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the P2-7 correction, and Codex reviewed it.
- Closed P2-7. `LogFields` rejects unmatched surrogates in field names with a position context. Valid paired-surrogate names remain accepted, and values and messages keep the replacement character.
- Corrected the stale effective head in the PR description from `d530f4c` to `94afeea` under D-217.
- Updated the review verdict to `Ready for owner merge` for `94afeea`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `94afeea`.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub CI, lint, STE, and bit-identity checks pass at `94afeea`. The review gate reflects the prior review until this metadata commit runs it again.

### In flight

PR #15 is ready for owner merge after the metadata review commit and its review-gate check pass.

### Traps and gotchas

- Replacement is correct for content and wrong for an identifier. Invalid field-name text must fail before serialization.
- The effective head is the code commit `94afeea`; the review record and handoff commit are metadata commits.
- The PR description had a stale effective-head fact even though its code and test counts were current.

### Open questions that block progress

No open question blocks PR-5 to PR-11.

### Next concrete action

Publish the metadata review commit, then verify the review-gate result and leave the owner to merge.

## Session 52: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the second PR #15 repeat review. Branch `feat/pr-4-logging`.

### What this session did, and why

- The review closed P2-3, P2-5, and P2-6, and it opened P2-7. That one has full merit, and the F-73 correction caused it.
- P2-7. The replacement character stood in place of a lone surrogate everywhere, and a field name took it too. A field named with a lone high surrogate and one named with a lone low surrogate both reached the object as one property name, which is the ambiguity that F-72 removed.
- The split is the fix. A field name is an identifier that the code writes, and it must reach the line unchanged, so invalid text in one is an error at the add. A value and a message carry content from the run, and those keep the replacement and never throw (F-77).
- The same pass flattened the add path. `AddField` called `Has`, which put it two levels below the caller, and the new name scan would have made a third. `AddField` calls no method now (D-110).
- The review used D-217 for the first time. It made four edits to the PR description and recorded each one under `## Description edits`. Every edit is correct, and none changes what the PR says it does.
- 1 new test. The total is 234.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, three review commits, and three correction commits.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head.

### Traps and gotchas

- A fix for content can break an identifier. The replacement character is right for a message and wrong for a field name, because two names can then reach the object as one.
- The response file states the name and value split as a Core contract, and no owner decision holds it. The owner can make it a decision.
- A depth fix and a new check meet. `AddField` was already two levels below its caller, and the name scan would have made a third, so the method calls nothing now.
- D-217 works. The reviewer corrected four stale facts in the description in the same pass that found P2-7.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure, and it updates `docs/reviews/pr-15.md` to the new effective head.

## Session 51: 2026-09-08, Codex

Author: Codex
Session: second repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `d530f4c` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the correction, and Codex reviewed it.
- Closed P2-3. Four labeled surrogate rows run through five text positions, and both parsing and string reading succeed.
- Closed P2-5. All three assertion call-site names are reserved, and the trusted copy path writes a complete report.
- Closed P2-6. D-214, the response, and the primary PR evidence agree on the head, the test total, and the allowlist counts.
- Added P2-7. Distinct accepted field names with unmatched high and low surrogates both serialize as U+FFFD. The resulting JSON object holds two equal property names, against the no-ambiguity contract of `LogFields` and T-2.
- Corrected four stale facts in the PR description under D-217, and recorded each edit in `docs/reviews/pr-15.md`.
- Updated the review verdict to `Changes required` for `d530f4c`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `d530f4c`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 233 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `d530f4c`. The review gate reflects the prior review until this metadata commit runs it again.

### In flight

PR #15 needs a correction for P2-7, then another repeat review.

### Traps and gotchas

- Replacement is not injective. Two invalid UTF-16 field names can become one valid JSON property name.
- A parser accepts duplicate JSON property names. Count the parsed properties or reject the input before serialization.
- The surrogate regression test puts invalid text in a field name, but it does not assert that distinct accepted names stay distinct.
- D-217 permits a reviewer to correct verified stale facts in the PR description. It does not permit changes to an owner-ticked gate line or to the author's substantive claims.

### Open questions that block progress

No open question blocks the P2-7 correction. The correction can reject invalid UTF-16 in field names or preserve a unique emitted name.

### Next concrete action

Correct P2-7 and add a regression test with distinct unmatched high- and low-surrogate field names. Then request another repeat review.

## Session 50: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #15 repeat review, and give the reviewer the PR description. Branch `feat/pr-4-logging`.

### What this session did, and why

- The repeat review closed P2-1, P2-2, and P2-4, and it kept P2-3 open and opened P2-5 and P2-6. All three have full merit.
- P2-3. The review found that three of four theory rows ran. xUnit takes the display name from the data, two rows of raw surrogate text give one name, and the low-surrogate row never ran. Each row carries a label and a code unit now, and the body builds five shapes for each one.
- Those rows then failed, and they showed the first fix was incomplete. `JsonDocument.Parse` accepts the `\u` escape of a lone surrogate, and `GetString` on that value throws. The first probe called `Parse` alone, so it passed and hid the rest.
- `AppendQuoted` writes the replacement character in place of a lone surrogate now. The line parses, and a reader takes the value back with one visible mark (F-73).
- P2-5. A caller field named `assertFile` stopped the assertion report, because the copy already held that name. This is P2-1 one level out: the first fix reserved the two names the logger writes and left the three the report writes. All five are reserved now, and `CopyWithCallSite` writes the call site on a path that no caller field reaches (F-74).
- P2-6. D-214 said 8 types and 16 members while the code held 9 and 18. A count of the lists gave the true numbers, and D-214 states them with the totals after them (F-75).
- The owner asked for a skill change so a reviewer can correct a stale PR description directly. D-217 records it, and the `pr-review` skill gains the rules and a `## Description edits` section in the record (F-76).
- 7 new tests. The total is 233.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, two review commits, and two correction commits.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 233 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head. That review can correct the PR description directly under D-217.

### Traps and gotchas

- Two xUnit theory rows of raw invalid text take one test id, and one row never runs. Give each row a label, and build the text in the body.
- `JsonDocument.Parse` and `GetString` are two gates. A line can parse and still fail when a reader asks for the value. Assert both.
- A reserved-name rule needs every name that the code writes. The first pass covered the logger and missed the assertion report.
- A count in a decision drifts from the code. State the totals, and count the list before you write them.
- A correction can be incomplete and still pass its first probe. Write the probe against the contract, not against the fix.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure, and it updates `docs/reviews/pr-15.md` to the new effective head.

## Session 49: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `e42a0a8` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the corrections, and Codex reviewed them.
- Closed P2-1. `LogFields` rejects the two logger names before a line exists.
- Closed P2-2. The assertion report uses a copy, so two safe failures preserve the caller fields.
- Kept P2-3 open. The encoder correction passes, but xUnit skips one required surrogate row because two rows have one test id.
- Closed P2-4. `BuildLine` calls two leaf helpers, and the prior nested helpers are absent.
- Added P2-5. A caller field with an assertion call-site name prevents every report and changes a safe failure to an exception.
- Added P2-6. D-214, the response, and the PR description disagree about the member count, test count, and final head.
- Updated `docs/reviews/pr-15.md` with the verdict `Changes required` for `e42a0a8`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `e42a0a8`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` reports 226 passes and 0 failures.
- The test run warns that xUnit skips one duplicate-id surrogate row. The P2-3 regression check is incomplete.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `e42a0a8`.

### In flight

PR #15 needs corrections for P2-3, P2-5, and P2-6, then another repeat review.

### Traps and gotchas

- xUnit can omit a theory row before execution when two invalid strings produce one display id.
- The assertion call-site names are logger-owned names, like `level` and `message`.
- A correction that adds an allowlist member must update the count and every current revision record.

### Open questions that block progress

No new owner question. No open question blocks the corrections.

### Next concrete action

Correct P2-3, P2-5, and P2-6, add the regression checks, and request another repeat Codex review.

## Session 48: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #15 review. Branch `feat/pr-4-logging`.

### What this session did, and why

- Read the four P2 findings in `docs/reviews/pr-15.md`. Each one reproduces, so each one has full merit.
- The first draft of this entry took the number 47, which the review session already held. The D-187 check caught it, and the entry is 48.
- This is the first review under D-209. It used the `## Out of scope` section for the Game-layer sink of PR-31 and PR-55 and for the simulation version constant of PR-7, and it gave neither a severity. Both readings are correct.
- P2-1. A caller field named `level` or `message` gave a line with two properties of one name. `LogFields` now reserves both names and rejects them at the add, so no such line reaches the sink (F-72).
- P2-2. A safe assertion added its call site to the caller field set, so a second safe assertion threw on the repeated name. A safe assertion promises to continue, and this turned the second one into an exception. The report takes a copy now (F-72).
- P2-3. A value with an unpaired surrogate gave a line that the parser rejected. The escape pass reads by index, keeps a matched pair as one character, and escapes every other surrogate. A check showed that `JsonDocument.Parse` accepts the escape form, so the fix keeps the value and never throws on a message (F-73).
- P2-4. The JSON write path ran three helper levels below its operation. `AppendText`, `AppendRaw`, and `HexDigit` are gone, and `BuildLine` calls only leaves (D-110, F-73).
- The P2-3 fix needs `System.String.this[]` for the one-character lookahead, and the owner approved that entry. D-214 records it.
- 7 new tests. The total is 226.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, the review commit, and this correction commit.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 226 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head.

### Traps and gotchas

- A helper that takes a caller object and adds to it changes that object for every later call. A report takes a copy.
- The logger writes its own `level` and `message`, so those two names need a rule of their own. A duplicate-name check over the caller fields alone does not reach them.
- An unpaired surrogate is not text that UTF-8 can hold. A parser rejects the raw form and accepts the `\u` escape form.
- D-110 counts every level. `BuildLine` to `AppendText` to `AppendQuoted` to `HexDigit` is three, and one is the limit.
- A patch script that fails part way leaves the file unchanged, and the build then passes on the old code. Read the file after a large scripted edit.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure and updates `docs/reviews/pr-15.md` to the new effective head.

## Session 47: 2026-09-08, Codex

Author: Codex
Session: review PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Reviewed PR #15 at effective head `41206bb` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the substantive change, and Codex reviewed it.
- Inspected the complete diff, the six exit tests, all changed documents, and the current callers.
- Added P2-1. Caller fields can add a second `level` or `message` property to the JSON object.
- Added P2-2. One safe assertion changes the caller fields, so a second safe assertion throws on `assertFile`.
- Added P2-3. An unpaired surrogate passes the logger and produces JSON that `JsonDocument` rejects.
- Added P2-4. The JSON write path has nested helper calls beyond the one level that D-110 permits.
- Wrote `docs/reviews/pr-15.md` with the verdict `Changes required`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `41206bb`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 219 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `41206bb`.
- Three focused review probes fail and reproduce P2-1, P2-2, and P2-3.

### In flight

PR #15 needs corrections for P2-1 to P2-4, then a repeat Codex review.

### Traps and gotchas

- `LogFields` checks duplicates only inside the caller set. It does not reserve the logger keys.
- `Invariant.Assert` adds its call-site fields to the object that the caller owns.
- A JSON control-character check does not cover invalid UTF-16 surrogate sequences.
- A green broad suite did not cover repeated safe failures or the complete string domain.

### Open questions that block progress

No new owner question. No open question blocks the corrections.

### Next concrete action

Correct P2-1 to P2-4, add regression tests, and request a repeat Codex review.

## Session 46: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-4, the logger, the error context, and the assertions. Branch `feat/pr-4-logging`.

### What this session did, and why

- Started PR-4 after the owner merged PR #14. No open question blocked it, and PR-4 is the first PR with no absent check, so the D-148 clause does not apply.
- Five owner questions came before the code, and D-211 to D-216 record the answers.
- D-211: Core builds a line and hands it to an `ILogSink`. Core opens no file, so a disk failure never reaches the simulation thread, and `System.IO` stays out of the allowlist.
- D-212: every line carries a level and a message beside its context fields. No line carries a wall clock, because the tick is the only time in Core.
- D-213: `StringBuilder` builds the line, because the escape pass appends one character at a time.
- D-215: the assertion report names the call site from the compiler, and it walks no stack. A stack trace changes with the build and the platform. This revises the PR-4 scope line, which said "the stack".
- Wrote `LogLevel`, `LogContextKind`, `ILogSink`, `LogFields`, `ContextException`, `JsonlLogger`, and `Invariant` in `Core/Logging/`.
- 26 new tests. The total is 219. Exit tests 1 to 6 each have a test, and the JSON tests parse each line with a real parser.
- D-214 records the allowlist additions: 8 types and 16 members. A removal check proved each one in use. It also found two dead entries in the PR-3 list, `System.Object` and `List.new/0`, and both are gone.
- The removal check found a live gap. `CultureInfo c = new("en-US", true);` gave no finding, and `new CultureInfo("en-US", true)` gave one, so the P2-9 case was reachable by another spelling. A `base` initializer and an indexer passed the same way (F-71). D-216 closes all three, and the owner chose the fix in this branch over a separate PR.
- `det-lint` caught one real defect in the new Core code. `context.ToString()` on an enum reads the enum metadata, which G-2 bans. An explicit switch replaces it.

### State of the build

- `main` is at `51b3de7`. This branch holds the PR-4 work above it.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 219 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. PR-4 adds no simulation number.

### In flight

PR-4 waits for a Codex review (T-4). It changes code, so no override applies.

### Traps and gotchas

- The allowlist needs a removal check, and not a reading. Two entries of the PR-3 list were dead, and one gap hid behind a spelling that the rule never read.
- A dead allowlist entry widens the boundary in silence. Check each new entry by removal before the PR opens.
- `det-lint` reads the new Core code as it lands. It caught an enum `ToString` in this session, which is reflection under G-2.
- A collection expression, `[]`, calls no constructor that the source names, so `List.new/0` stayed dead.
- An xUnit lambda with every path throwing binds to `Func<Task>` and not to `Action`. Name the delegate type.
- A test that asserts a lint finding breaks when a later PR approves that type. Two such tests moved to a type that stays unapproved.

### Open questions that block progress

No new owner question. OQ-82 to OQ-86 are resolved by D-211 to D-216. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session reviews PR-4 per `.claude/skills/pr-review/SKILL.md`, under the scope rules of D-209, and writes `docs/reviews/pr-<number>.md`.

## Session 45: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-3 merge across the documents. Branch `docs/pr-3-merge-record`.

### What this session did, and why

- The owner merged PR #12 as `9e6fd5f`, after a Codex review gave the verdict `Ready for owner merge` at effective head `4d7cb3e`.
- Audited every document against the merged state. The registers were complete: D-200 to D-209, OQ-73 to OQ-81, and F-60 to F-70 all landed with the PR.
- Four documents held a stale line, and this session corrects each one.
- `docs/roadmaps/phase-1-foundations.md`: the PR-3 status line said "opened", and it names the merge commit now. The header cited D-200 to D-203, and it cites D-200 to D-209 now. The correction passes record the PR-3 outcome.
- `docs/design.md`: the PR-2 and PR-3 markers now name the merge, as the PR-1 marker does. The Phase 1 sequence marks PR-2 and PR-3 as merged.
- `dotnet test` caught the one defect in this session. `RepositoryDocumentsPass` runs the STE checker over the repository, and a new sentence of 26 words failed it. The sentence is three sentences now. A second sentence failed the same limit later, for the biome entry.
- The owner answered OQ-12, the last open question of Phase 1 before PR-9. The v1 biome is a collapsed deep mine (D-210). The block set holds seven blocks, and the three depth bands are floors 1 to 5, 6 to 10, and 11 to 15.
- The biome answer reaches four documents: the decision register, the questions register, the design doc world section and PR-9 entry, and the PR-9 scope in the focused roadmap.
- D-210 feeds four open questions that a later phase needs: OQ-1 the palette, OQ-61 the silhouettes, OQ-64 the props, and PR-14 the texture generator. The three props of OQ-64 belong in a mine with no change.

### State of the build

- `main` is at `9e6fd5f`, the squash merge of PR #12. This branch holds one commit above it.
- Remote head: `origin/docs/pr-3-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs on `main` at `9e6fd5f`.

### In flight

This PR changes only `docs/`, so the `review-override` label covers it (D-190). It carries two concerns, the merge record and the biome decision, and the owner asked for one PR. PR-4 starts after the merge.

### Traps and gotchas

- `RepositoryDocumentsPass` fails the test suite on any STE finding. A document edit needs `dotnet test`, and not the checker alone.
- A merged PR leaves a status line in two places: the focused roadmap and the design doc. The design doc also holds the Phase 1 sequence, which is a third place.
- The roadmap header lists the decisions that the file applies. A PR that adds a decision extends that list.
- Phase 1 has no gate before PR-11. Gate 1 needs the bit-identity job, `dotnet test`, and the night sweep of PR-58.

### Open questions that block progress

No new owner question. D-210 resolves OQ-12, so no open question blocks any PR of Phase 1. PR-4 to PR-11 each have every answer they need.

### Next concrete action

The owner merges this documentation PR with the `review-override` label. Then a new session starts PR-4: the JSONL logger, the error context sets, and the assertion helper (D-68, D-112, D-113). PR-4 is the first PR with no absent check, so the check clause of D-148 does not apply to it.

## Session 44: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #12 at the changed effective head. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Reloaded the `pr-review` skill after the owner approved its change as a second concern on this PR.
- Re-reviewed PR #12 at effective head `4d7cb3e` against base and merge base `86078b8`.
- Inspected the D-208 member allowlist correction and the D-209 review-scope rules. The provider gate passes because Claude Code wrote the substantive changes and Codex reviewed them.
- Closed P2-6. The PR description now identifies the effective head alone, its decision counts agree with its D-200 to D-209 table, and the P2-6 row describes the correction that the page contains.
- Closed P2-9. A focused Core probe reported the original comparer, runtime-feature, and current-culture paths, and it reported the later culture-constructor and string-intern paths.
- Added the required `## Out of scope` section to the review record. This review found no concern to put there.
- Set the verdict to `Ready for owner merge` for effective head `4d7cb3e`.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `4d7cb3e`.
- Remote head: `origin/feat/pr-3-determinism` will hold the metadata commit for this entry after the session-end push. The effective head stays `4d7cb3e` under D-184.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `4d7cb3e`. The review-gate results still reflect the prior verdict until this metadata commit runs them again.

### In flight

PR #12 is ready for the owner to merge after the review-gate checks turn green for this review record.

### Traps and gotchas

- The effective head includes the skill change because `.claude/skills/pr-review/SKILL.md` is outside the metadata set.
- D-209 closes an existing finding when its stated trigger and regression check pass. A different trigger takes a new id and must pass the PR-scope test before it becomes a finding.
- The focused probe is temporary. Remove it before the clean lint and build gates.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9 and does not block this review.

### Next concrete action

Wait for the review-gate checks on the metadata commit. The owner can then squash-merge PR #12.

## Session 43: 2026-09-08, Claude Code

Author: Claude Code
Session: the scope rules for the `pr-review` skill. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- The owner asked for a skill change after seven review passes on PR #12. That review reopened one finding four times and another one three times (F-70).
- Each pass found a real defect, and each correction was right. The sum went past the PR-3 scope, which names a lint tool for the banned symbols of G-2 and G-21. Five owner decisions, D-204 to D-208, came from that one boundary.
- Added a "Stay inside the pull request" section. The roadmap entry for the PR and its exit tests set the boundary. Four tests say a concern is in scope, and four say a later PR holds it.
- Added the `## Out of scope` heading to the review record skeleton. A line there names the PR that holds it, takes no severity, and never gives the verdict `Changes required`.
- Added the rule for a new check. A PR that creates a check must pass that check (G-19), and the check does not cover the whole platform on the first day.
- Added a "When a finding closes" section. A finding closes when its stated trigger and its regression check pass. A new trigger of the same class takes a new id. The third assessment of one id stops, and the owner settles the scope.
- Added two rows to the author push-back table, for a finding outside the scope and for a third reopen.
- The section states in two places that it never lowers the standard for the code that a PR changes. Every PR #12 finding stays a defect under these rules.
- This session first opened PR #13 for the skill change, on the G-10 reading that a skill change and the determinism work are two concerns. The owner asked for it on PR #12 instead. PR #13 is closed, and its branch is deleted.

### State of the build

- `main` is at `86078b8`. This branch holds the PR-3 work and this skill change above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. This commit changes no code.

### In flight

PR #12 needs a repeat review at the new effective head, under the new scope rules.

### Traps and gotchas

- `git checkout <file>` restores from HEAD and drops every uncommitted edit in that file. This session lost the whole skill change that way and wrote it again. Commit first, or copy the file.
- A scope rule can hide a real defect. Each rule here names the code that the PR changes as the part that keeps the full standard.
- The skill file was identical on `main` and on this branch, so the change moved between branches as a whole file. Check that before a copy.
- A second PR for a second concern is the G-10 reading, and the owner decides when one PR carries both.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 under the new scope rules, and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 42: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the seventh PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The seventh review kept P2-6 and P2-9 open. Both reproduce, so both have full merit.
- P2-9, third pass. The type allowlist approved every member of an approved type. `new CultureInfo("en-US", useUserOverride: true)`, `string.Intern(v)`, and `string.IsInterned(v)` each gave no finding, and the .NET contract names the user settings and the process intern pool.
- The gap moved from the namespace to the type, and the type still approved its whole surface. A member denylist would miss `new CultureInfo("en-US")`, which has the same behavior as the two-argument form.
- A measurement found six members and two constructors of an outside type in Core. The owner chose the member allowlist with the overload arity (D-208, OQ-81, F-69).
- The arity closes a gap that no trigger named. `UInt64.ToString/2` takes a format provider, and `ToString/0` reads the current culture. The same split binds `Parse`, `TryParse`, and `Compare`.
- Two corrections came from a run and not from the finding. A named argument carries a containing type and is not a member of it, so `useUserOverride:` gave a false finding until the rule read a method, a property, a field, and an event alone. `Object.GetType` joined the member denylist, so it keeps the `L-REFLECTION` id.
- P2-6, fourth pass. The decision section counted rows and not ids, the P2-6 row named a session that the description no longer holds, and the summary named the pass count and the newest review head. All three are corrected.
- 193 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds seven review commits and seven correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- The boundary moved four times: namespace, then `System` type, then every type, then every member. Each level approved the whole of the level below it. A member entry with an overload arity is the first form with nothing below it to leak.
- A named argument and a local both carry a containing type. Read a method, a property, a field, and an event alone.
- Keep a specific rule id beside the wide one. `Object.GetType` sits in the member denylist, so the finding says reflection and not member.
- An overload is not a behavior. `ToString/0` and `ToString/2` differ, and so do the `Parse` and `Compare` families.
- A description that names a pass count or a review head goes stale at the next review. Name the effective head alone.

### Open questions that block progress

No new owner question. OQ-81 is resolved by D-208. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 41: 2026-09-08, Codex

Author: Codex
Session: sixth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `e30ddfd` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that the original P2-9 triggers now report `L-TYPE`, `L-TYPE`, and `L-CLOCK`.
- Kept P2-9 open. `CultureInfo` and `String` are approved types, and their other machine-dependent members still pass.
- A probe used the `CultureInfo` constructor with user overrides. It also used `String.Intern` and `String.IsInterned`.
- The probe compiled, and `det-lint` reported 0 findings. The .NET contracts confirm that these APIs read user or process state.
- Kept P2-6 open. The PR description gives the wrong decision count and contains two claims that contradict its current content.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `e30ddfd`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 187 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `e30ddfd`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-6 and P2-9, then another repeat review.

### Traps and gotchas

- A type allowlist approves every member of an approved type unless another rule limits the members.
- `CultureInfo` has the required `InvariantCulture` member and constructors that read user settings.
- `String.Intern` changes the process intern pool. `String.IsInterned` reads that process state.
- Volatile pass counts and review heads become stale when the required next review completes.

### Open questions that block progress

No new owner question was filed in this review. The P2-9 correction can require a revision to D-207. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-6 and P2-9. Add regression tests for the three new triggers, then request another repeat review.

## Session 40: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the sixth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The sixth review closed P2-7 and P2-8, and it kept P2-6 open and opened P2-9. Both reproduce, so both have full merit.
- P2-9. `EqualityComparer<string>.Default.GetHashCode(v)` gave no finding. The member belongs to `EqualityComparer`, and D-205 approved `System.Collections.Generic` as a whole namespace, which D-207 supersedes. The reviewer ran three processes and got three hashes for one string.
- The cause is the same shape as P2-7, one level out. Each approved namespace holds a machine-dependent type beside the one Core needs.
- A run with the four namespaces removed named one type: `CultureInfo`. The owner chose the one allowlist with that number in hand (D-207, OQ-80, F-68).
- D-207 supersedes D-205 and D-206. Core approves every external type by full name, and no namespace passes as a whole. The list holds ten entries, and the tool is smaller: one list serves the type rule and the import rule.
- `CultureInfo` needs both rules. Core formats with `InvariantCulture`, and the same type holds `CurrentCulture`. The member denylist holds the five culture members that read the user, the process, or the machine.
- `System.Array` and the collection types are absent, because Core uses neither today. PR-7 will add `System.Array` with its decision.
- P2-6, third pass. The description listed four owner decisions while the same page named seven. It lists every decision now, with the finding that produced it, and it names no session number.
- 187 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds six review commits and six correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 187 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A whole-namespace approval fails at every level. The namespace list leaked `Guid`, and then the `System` type list leaked `EqualityComparer` through the other namespaces. Approve each type by name.
- A type allowlist does not remove the member denylist. `CultureInfo` holds `InvariantCulture` and `CurrentCulture`, and a type with both kinds of member needs both rules.
- A superseded decision needs the superseding id on every line that cites it (D-178). The reference check found four such lines in one commit, and the checker is the only reason they did not ship.
- A large edit by text slice can delete a neighbor. This session removed five helpers with one block replacement, and the build named each one.
- Measure the friction before the question. Seven types settled D-206, and one type settled D-207.

### Open questions that block progress

No new owner question. OQ-80 is resolved by D-207. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 39: 2026-09-08, Codex

Author: Codex
Session: fifth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `9ad2a4c` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-7 is fixed. `Guid.NewGuid()` now reports `L-RANDOM`.
- Confirmed that P2-8 is fixed. An unused `using System.Text;` now reports `L-NAMESPACE`.
- Kept P2-6 open. The PR description says that four owner decisions were needed and lists D-200 to D-203, but the same description identifies D-200 to D-206 as the decisions for the PR.
- Added P2-9. `EqualityComparer<string>.Default.GetHashCode(value)` compiles in Core and gives no lint finding. Three processes gave three values for the same string.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `9ad2a4c`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 183 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `9ad2a4c`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-6 and P2-9, then another repeat review.

### Traps and gotchas

- A member ban reads the type that declares the member. `EqualityComparer<string>.GetHashCode` reaches the string hash, but the member belongs to the comparer and not to `String`.
- A whole-namespace approval can admit process state through a type that the decision never names.
- Run a process-randomized hash probe in separate processes. Repeated calls inside one process use one seed.
- A session number in the PR description becomes stale when the required review adds the next handoff entry.

### Open questions that block progress

No new owner question was filed in this review. The correction for P2-9 can require a revision to D-205. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-6 and P2-9, add a regression test for the comparer trigger, and request another repeat review.

## Session 38: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the fifth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The fifth review closed P2-3 and P2-6, and it opened P2-7 and P2-8. Both reproduce, so both have full merit.
- P2-7. `System.Guid.NewGuid()` compiles in Core and gave no finding. D-205 approved `System` as a whole namespace, and `System` is the one broad namespace that Core uses, so the rest of the nondeterminism sits inside it.
- Before the question, this session measured the cost. A run with `System` removed from the allowlist named seven `System` types in Core. The owner chose the type allowlist with that number in hand (D-206, OQ-79, F-67).
- D-206 revises D-205 in part. `System` is approved by type now, and the other approved namespaces stand as whole namespaces.
- `Guid` and `HashCode` also join the type denylist, so each reports its own rule. The review asks for a randomness finding on `Guid.NewGuid()`, and it reads `L-RANDOM`.
- One correction goes past the finding. `Object.GetHashCode` and `String.GetHashCode` report `L-IDENTITY`, because a member of an approved type can still read the machine. A hash of an address changes an iteration order between two runs of one seed.
- The first form of the `System` rule was wrong, and the check caught it. The rule also read the type a method gives back, and `det-lint` reported 24 findings on clean Core code. The allowlist binds the names that Core writes, never a return type.
- P2-8. `using System.Text;` gave no finding, because `AddImportFinding` read the denylist alone. It calls the allowlist now.
- 183 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds five review commits and five correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 183 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- An allowlist at the namespace level leaves the one namespace that the project uses. `System` holds `Guid`, `HashCode`, `GC`, `OperatingSystem`, `Console`, and `AppContext`. Approve that namespace by type.
- A type rule must bind the names that the source writes, not the type a method gives back. The first form flagged every method that gives back a bool. Run the tool on clean code before you keep a rule.
- A member of an approved type can still read the machine. `Object.GetHashCode` gives an address, and `String.GetHashCode` takes a new seed in each process.
- An allowlist binds each entry point on its own. The import check and the use check are two entry points, and P2-8 was the one that nobody updated.
- Measure the friction before you ask the owner for a stricter rule. Seven types was the number that settled the question.

### Open questions that block progress

No new owner question. OQ-79 is resolved by D-206. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 37: 2026-09-08, Codex

Author: Codex
Session: fourth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `60d678e` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-3 is fixed. The allowlist blocks the prior reflection and metadata probes.
- Confirmed that P2-6 is fixed at this effective head. The PR description identifies the current correction evidence.
- Added P2-7. `Guid.NewGuid()` compiles in Core and gives no lint finding against the seed-only rule.
- Added P2-8. An unapproved `using System.Text;` directive compiles and gives no `L-NAMESPACE` finding.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `60d678e`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 172 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `60d678e`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-7 and P2-8, then another repeat review.

### Traps and gotchas

- The exact `System` namespace contains `Guid.NewGuid()` and other nondeterministic APIs.
- A namespace allowlist still needs symbol bans inside each approved namespace.
- `AddImportFinding` applies the old namespace denylist, but it does not apply the new allowlist.
- An unused `using` directive compiles without a warning in the Core project.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-7 and P2-8, add their regression tests, and request another repeat review.

## Session 36: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the fourth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The fourth review kept P2-3 and P2-6 open. Both reproduce, so both have full merit.
- P2-3 reopened for the fourth time. `System.ComponentModel.TypeDescriptor.GetProperties` compiles in Core and gave no finding. Each pass named one more type: namespace text, member words, `System.Enum`, then `TypeDescriptor`.
- The denylist was the defect, not its contents. `System.Linq.Expressions`, `System.Text.Json`, `System.Runtime.Serialization`, and `System.Dynamic` all reach type metadata under no name that a rule held. A fifth pass was likely.
- The owner chose the namespace allowlist (D-205, OQ-78, F-66). Core uses `System`, `System.Collections.Generic`, `System.Globalization`, `System.Numerics`, `System.Runtime.CompilerServices`, and any namespace under `WhatYouCarry.`. Each entry matches one namespace and never its children, so `System` does not approve `System.ComponentModel`.
- The type denylist stays for the cases inside an approved namespace. `TypeDescriptor` joined it too, so the review's regression check reads `L-REFLECTION` and not the wider rule.
- Core used two namespaces, so the rule changed no Core file. An adversarial run reported all five planted uses, and three of them name a namespace that no denylist ever held.
- P2-6, second pass. The description named head `5316033` and session 28 after the first correction. It now names the effective head and the current session.
- 172 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds four review commits and four correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 172 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A denylist over a library the size of the class library never ends. Four review passes proved it on this PR. Turn the boundary around and approve what enters.
- An allowlist entry matches one namespace and never its children. `System` must not approve `System.ComponentModel`, so the match is exact.
- Keep the type denylist beside the allowlist. `System.Math` and `System.Type` sit inside an approved namespace, and only the denylist reaches them.
- A specific rule id carries more than a wide one. `TypeDescriptor` sits in both lists, so the finding says reflection and not namespace.
- The PR description is part of the record (D-118), and a review reads it. Update it with each correction, and name the effective head in it.

### Open questions that block progress

No new owner question. OQ-78 is resolved by D-205. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 35: 2026-09-08, Codex

Author: Codex
Session: third repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `549c75c` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-5 is fixed. The lint reports active and inactive `#if` directives under D-204.
- Kept P2-3 open. `TypeDescriptor.GetProperties(object)` compiles in Core and gives no reflection finding.
- Kept P2-6 open. The PR description omits effective head `549c75c` and incorrectly identifies Session 28 as current.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `549c75c`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 160 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `549c75c`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-3 and P2-6, then another repeat review.

### Traps and gotchas

- `TypeDescriptor` accesses type metadata without a `System.Reflection` symbol or a `System.Type` result.
- A finite banned-type table needs probes against all BCL metadata-access surfaces.
- The PR description must identify the effective head, not only the prior review head.
- A session number in the PR description becomes stale after each review response.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3, then update the PR description for P2-6. Request another repeat review.

## Session 34: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the third PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The third review kept P2-3 open and added P2-5 and P2-6. All three reproduce, so all three have full merit.
- P2-3, third pass. The symbol read was correct, and the table was short. `System.Enum.IsDefined` gave no finding, and session 28 had already named `Enum.IsDefined` as reflection when it removed the call from `Rng.ForStream`. The tool contradicted the code it guards.
- The table gains `System.Enum`, `System.Attribute`, `System.AppDomain`, `System.Delegate`, `System.MulticastDelegate`, the three runtime handle types, and `RuntimeHelpers`. The scan also reports the `typeof` keyword, because no symbol carries that name and a type value can reach a place where no name is banned.
- An ordinary enum stays legal. The ban reads `System.Enum`, never the Core enum that declares the values, and `AnOrdinaryEnumIsNotAFinding` holds that.
- P2-5. A `System.Math.Sin` call inside `#if NET10_0` compiles in the Core build and gave no finding. The owner chose the ban on conditional compilation over the build symbols (D-204, OQ-77, F-65). The other option cannot be complete, because Debug and Release are both real builds and a lint parses one.
- `det-lint` reports each `#if` as `L-CONDITIONAL`. A `#nullable`, `#region`, or `#pragma` directive stays legal. Core held no conditional directive, so no Core file changed.
- P2-6. The PR description named the superseded word list, 144 tests, and an old head. It now holds a table of all six findings, the symbol read, and the current evidence.
- 160 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds three review commits and three correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 160 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- A symbol table is a list, and a list is never complete on its own. Check the table against the code the project already rejected. Session 28 removed `Enum.IsDefined`, and the tool did not know it.
- A ban on a platform type is not a ban on the language feature. `System.Enum` owns the metadata methods, and a Core enum owns its values. Test the ordinary use.
- `#if` is trivia. A node walk does not reach it, and `DescendantNodes(descendIntoTrivia: true)` does.
- The lint parse defines no preprocessor symbol, so an active branch reads as empty. That is why D-204 bans the directive and does not read the branch.
- The PR description is part of the record (D-118). Update it with each correction, not at the end.

### Open questions that block progress

No new owner question. OQ-77 is resolved by D-204. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 33: 2026-09-08, Codex

Author: Codex
Session: second repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `5316033` against base and merge base `86078b8`.
- Confirmed that the provider gate still passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that the semantic scan fixes the two prior P2-3 probes and the F-# citations.
- Kept P2-3 open. `Enum.IsDefined` compiles in Core and gives no reflection finding, against G-2 and the Session 28 record.
- Added P2-5. A `System.Math.Sin` call under active `#if NET10_0` code builds, but the lint reports no finding.
- Added P2-6. The PR description still reports the removed member word list, 144 tests, and the old effective head.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `5316033`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 146 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `5316033`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-3, P2-5, and P2-6, then another repeat review.

### Traps and gotchas

- Reflection APIs also exist outside `System.Type`, `System.Activator`, and the `System.Reflection` namespace.
- A semantic scan only reads the branch that its parse symbols select.
- The Core build defines target-framework symbols that the lint compilation does not define.
- The PR description is part of the evidence record. Update it after a correction changes the implementation.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3 and P2-5, then update the PR description for P2-6. Request another repeat review.

## Session 32: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #12 repeat review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The repeat review closed P2-1, P2-2, and P2-4, and it kept P2-3 open with two probes. Both reproduce, so the finding has full merit.
- `Type.GetEvents()` gave no finding, because `GetEvents` was absent from the member word list. A Core `probe.GetMethods()` gave a false `L-REFLECTION`, because the word matched.
- No word list can fix both. A Core class that declares `public new string GetType()` compiles, and this session checked that. The list itself was the defect.
- `CoreSourceScan` now compiles the Core sources with `CSharpCompilation` and reads the semantic model. The rules match the type that owns a symbol, the namespace of that type, and the type a method or property gives back. That last rule catches `object.GetType()`, which belongs to `System.Object` and not to `System.Type`.
- `BannedSymbols` holds full type names now. The member word list is gone.
- Two T-2 guards. The compilation checks that `System.Math` and `System.Reflection.Assembly` resolve, because without references every name resolves to nothing and the scan would pass every file. The repository scan reports every compiler error, because a Core file that does not compile resolves no symbol.
- Corrected the F-# citations. The reflection comments cited F-62, the `Atan2` defect. F-64 is the register entry, and it now records both correction passes. D-202 carries a dated note.
- Rewrote the lint tests. Each fragment compiles on its own now, so the tests assert real symbol behavior and not an unresolved name.
- 146 tests pass. The bit-identity hash is unchanged, because this pass changed no Core number.

### State of the build

- `main` is at `86078b8`. The branch holds the two review commits and two correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 146 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- An adversarial Core tree with nine planted uses gave 10 findings, and no finding for a Core `Vector3` in the same file.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- A lint that matches words has two failure modes at once, and each fix makes the other worse. Ask the compiler.
- A semantic scan with no metadata reference resolves nothing and reports nothing. That is a silent pass, so the canary check is not optional.
- `var` resolves to the type the compiler inferred, so it reports the same use a second time. Skip it.
- A test fragment that names an undefined type resolves no symbol, so it gives no finding. A lint test must compile, or it passes for the wrong reason.
- `object.GetType()` belongs to `System.Object`. The owner rule cannot see it, and the rule for the type it gives back can.
- The scan reads the last name of a dotted chain. `System.Math.Sin` holds three names for one call.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 31: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `1bc665b` against base and merge base `86078b8`.
- Confirmed that the provider gate still passes. Claude Code wrote the corrections, and Codex reviewed them.
- Confirmed that P2-1, P2-2, and P2-4 are fixed, with regression coverage.
- Kept P2-3 open. The new name list gives both a false negative and a false positive.
- A probe with `Type.GetEvents()` gave no finding. A user-defined `probe.GetMethods()` call gave `L-REFLECTION`.
- Updated `docs/reviews/pr-12.md` with the changed hash and the repeat-review evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `1bc665b`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 144 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `1bc665b`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs a complete P2-3 correction and another repeat review.

### Traps and gotchas

- Member text does not identify the member symbol. A Core type can declare a method with a reflection-like name.
- A finite reflection member list can miss a supported `System.Type` API.
- The P2-3 code comments cite F-62. F-64 is the register entry for the reflection defect.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3 with complete reflection detection and tests for both probe cases. Then request another repeat review.

## Session 30: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Read the four P2 findings in `docs/reviews/pr-12.md`. Each one reproduces, and each one has full merit.
- P2-1. `Atan2` read the sign of y with `y < 0.0f`, and negative zero is not below zero. `Atan2(-0, -1)` gave pi against the reference -pi, an error of two pi. The sign now comes from `float.IsNegative` (F-62).
- P2-2. A `StateHash` from `default` held zero and not the FNV offset basis, and it accepted fields in silence. The struct now rejects a hash that `Start` did not make (T-2, F-63).
- P2-3. The reflection rule read the namespace text alone, so `typeof(x).GetMethods()` gave no finding. A reflection member name is now a finding on its own. The list holds no name that a Core type can hold too, so `block.Type` stays clean.
- P2-4. The `MathF` exemption compared the file name alone. It now compares the whole Core-relative path (F-64).
- The bit-identity sweep never made a negative zero, so the three-platform check could not have caught P2-1. The sweep now holds the four sign pairs. The pinned hash moved from `ef592d4148eb8ba0` to `4d6385bb92454694`. Core gives the same numbers for every input the old sweep read, so this is a wider check and not a behavior change.
- Wrote `docs/reviews/pr-12-response.md` with the disposition and the evidence for each finding.
- 11 new tests. The total is 144, and all pass.

### State of the build

- `main` is at `86078b8`. The branch holds the review commit and this correction commit above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 144 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- Each of the five review triggers failed against `c2ba592` before the corrections.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- Negative zero passes every comparison against zero. Read the sign bit with `float.IsNegative` when the sign matters.
- A struct that `default` makes skips every factory. A hash, a counter, or any accumulator with a non-zero start needs a guard.
- A determinism sweep proves only what it reads. The `Atan2` grid held no negative zero, so it could not see the defect. Widen the sweep with each defect it missed.
- A first correction for P2-1 added a branch for a negative zero x. A check showed the branch changed no result, and it is gone. Test a defensive branch before you keep it.
- The pinned bit-identity hash changes when the sweep grows, not only when Core changes. Say which one it was.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 29: 2026-09-08, Codex

Author: Codex
Session: review PR #12. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Reviewed PR #12 at effective head `c2ba592` against base and merge base `86078b8`.
- Confirmed the provider gate. Claude Code wrote the substantive change, and Codex reviewed it.
- Found four P2 defects in DetMath, StateHash, and the determinism lint.
- Wrote `docs/reviews/pr-12.md` with the verdict `Changes required`.

### State of the build

- The local build passes with 0 warnings and 0 errors.
- The local test suite passes with 133 tests and 0 failures.
- The lint, STE, bit-identity, and Godot checks pass.
- The remote branch contains the review record and this entry.

### In flight

PR #12 needs P2-1 through P2-4 corrected and a repeat review.

### Traps and gotchas

- A numeric grid that contains zero does not necessarily contain negative zero.
- A public struct always permits default initialization.
- A namespace scan does not detect all reflection calls.
- A file-name check does not identify one canonical path.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 through P2-4, add regression tests, and request a repeat Codex review.

## Session 28: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-3, the seeded RNG, DetMath, the lint tool, and the bit-identity job. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Started PR-3 after the owner merged PR #11. Session 27 named it as the next action.
- Four owner questions blocked the start, and the owner answered all four (D-200 to D-203, OQ-73 to OQ-76).
- The largest one refutes D-161. That decision named a reduction to [-pi, pi], degree-7 minimax polynomials, and an absolute error of at most 1e-6. A Remez fit of degree 7 on [-pi, pi] reaches 2.5e-4 for sine, 250 times the target. D-203 folds to [-pi/4, pi/4] and a quadrant instead, and the degree and the target stand (F-60).
- Wrote `Rng.cs` (xoshiro128** with SplitMix64 seeding), `DetMath.cs`, `StateHash.cs`, and `RngStream.cs` in Core.
- Wrote the `det-lint` command, which parses each Core file with the C# compiler API (D-202), and the `bit-identity` command (D-201).
- Wrote `.github/workflows/bit-identity.yml` and `.github/workflows/det-lint.yml`. The bit-identity workflow passes each hash up as a job output, so it needs no artifact action.
- 70 new tests. The total is 133, and all pass. Exit tests 1 to 7 each have a test, and the CI run proved exit test 8.
- Corrected exit test 5. The roadmap asked for the opposite of D-160, and no FNV-1a hash can hold the roadmap form (F-61).
- Updated the PR template. PR-3 creates the lint tool and the bit-identity job, so the two bootstrap notes are gone.

### State of the build

- `main` is at `86078b8`, the squash merge of PR #11. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 133 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The bit-identity job on PR #12 proved exit test 8. Linux x64, Windows x64, and macOS arm64 each answered `ef592d4148eb8ba0`, and the compare job agreed. This is the first live proof of G-9.
- Every check on PR #12 is green: CI on the three platforms, `bit-identity` on the three platforms and its compare job, `det-lint`, and `ste-check`. The `review-gate` check is neutral with the title "No review record", which is the advisory grey of D-181.
- The Godot 4.7.2 headless build check passes with `/Applications/Godot_mono.app/Contents/MacOS/Godot`. The name `Godot` is not on the command path.

### In flight

PR #12 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- Two numbers proved a documented plan wrong this session. Measure a numeric claim before you build on it.
- .NET never fuses a multiply and an add, so every float intermediate is the same on the three platforms. That is the whole basis of DetMath.
- The angle limit is 4096 radians. At 65536 the error reaches 9.6e-07 and the 1e-6 gate has no margin left.
- `Enum.IsDefined` reads the enum through reflection, which G-2 bans in Core. `Rng.ForStream` uses an explicit bound, and `EveryDeclaredStreamIsAccepted` guards it.
- A Roslyn scan of `System.Math.Sin` puts `Math` in the `Name` of an inner member access, not in the `Expression`. `Vector128<float>` is a `GenericNameSyntax` and never an `IdentifierNameSyntax`. Both defects passed the first build and failed the tests.
- An RngStream number is part of the seed. A new subsystem appends a value, and it never renumbers one.
- The bit-identity hash is pinned in `BitIdentityTests`. A deliberate Core change updates it, and G-20 asks the review to confirm the version bump.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #12 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-12.md`. After the merge, PR-4 starts: the logger, the error context, and the assertions.

## Session 27: 2026-09-08, Claude Code

Author: Claude Code
Session: the session end gate for the `pr-review` skill. Branch `docs/pr-review-push-gate`, PR #11.

### What this session did, and why

- The owner asked for a fix after the reviewer left its commit unpushed on the shared checkout twice on PR #10 (F-59). The skill said "push" in three places, with no verification step and no evidence trail.
- Added a "Session end gate" section to the skill. Four commands after the commit, and the evidence comes from the remote: `git status --short --branch` shows no `[ahead N]`, and `gh pr view --json headRefOid` equals `git rev-parse HEAD`.
- Added the push line as a required part of the Verification section in the review skeleton. A record with no push line is incomplete.
- Added the failure path. A denied push does not end the session. The session asks the owner to approve it and says in the handoff that the record is unpushed. A sandbox that blocks the network denies a push in silence, so the status line is the evidence and not the push output.
- Added the start-of-session check for both roles. If the checkout is ahead with the other provider's commit, push it first and record that.
- The owner also chose the rule for every session, not only reviews. D-199 records it, and revises in part D-146: the state of the build names the remote head. `CLAUDE.md` and `AGENTS.md` carry the rule in the session handoff section.
- This is the second PR of one harness invocation, after PR #10, on the owner instruction. D-121 names one PR per session.

### State of the build

- `main` is at `d5eb298`, the squash merge of PR #10. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/docs/pr-review-push-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` and `dotnet test` pass: 63 tests, 0 failures. The checker reports 0 findings.

### In flight

PR #11 changes only `docs/`, `CLAUDE.md`, `AGENTS.md`, and `.claude/skills/`, so the `review-override` label covers it (D-190). The owner asked for the label in the instruction, and this session added it.

### Traps and gotchas

- The evidence for a push is the remote, never the local checkout. A sandbox denial gives no git error.
- A revision of one part of a decision needs the `Revised in part by` marker on the old row, and the reference check does not flag it (D-186).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #11 with the `review-override` label. Then a new session starts PR-3: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 26: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `b9bc6cd` against base and merge base `94aadc5`.
- Confirmed that P2-1 to P2-4 are resolved.
- Updated `docs/reviews/pr-10.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 is ready for owner merge.

### Traps and gotchas

- The mask pass must protect every delimiter inside an earlier masked span.
- The review commit is metadata. The review head remains `b9bc6cd` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #10. Then PR-3 starts with the seeded RNG, DetMath, the lint tool, and the bit-identity job.

## Session 25: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 repeat review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the repeat review in `docs/reviews/pr-10.md`. P2-1 to P2-3 are resolved, and P2-4 is new.
- P2-4 has full merit. The quote pass masked the text inside a quote, but it left a quoted `)` as a plain character, so the parenthesis pass paired the outer `(` with it. `MaskChar` now masks both parentheses, both quote forms, and the backtick inside a span, and `Unmask` restores them. Four new assertions in `NestedParenthesesAreOneOpaqueWord` cover it.
- Updated `docs/reviews/pr-10-response.md` with the P2-4 disposition.
- The review commit `712b2e0` was on the local checkout and not on the remote, as `29b1066` was before it. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code correction too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- A mask pass must mask every delimiter inside its span, not only the sentence punctuation. A later pass reads any character that the earlier pass left plain.
- An unbalanced quote or an unclosed parenthesis stays plain text. Each token then counts on its own.
- The reviewer's commit was unpushed for the second time. Check `git status` before you start, and push it.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 at the new effective head and updates `docs/reviews/pr-10.md`.

## Session 24: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `d55666d` against base and merge base `94aadc5`.
- Confirmed that the three prior P2 findings are resolved.
- Found P2-4. A quoted close parenthesis can end an outer parenthesized span.
- Updated `docs/reviews/pr-10.md` with the new finding and the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot check did not complete because Godot could not write its macOS support file.

### In flight

PR #10 needs P2-4 corrected and a repeat review at the new effective head.

### Traps and gotchas

- The masking passes run in sequence. A later pass can read delimiters that an earlier pass already made opaque.
- `FindSpanEnd` fixes nested parentheses of one type. It does not protect against delimiters from another span type.
- The review commit is metadata. The review head remains `d55666d` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-4, add the regression tests, and request another repeat Codex review.

## Session 23: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the three P2 findings in `docs/reviews/pr-10.md` and assessed each against the evidence.
- P2-1 has partial merit. The splitter requires a space after a colon on purpose: the `ste-writing` skill holds a bare URL in prose, and a split at every colon cuts it, and cuts every time and ratio. The roadmap and the skill over-claimed with "everywhere". Both now state the space condition, and `ColonInsideAWordDoesNotEndASentence` covers it.
- P2-2 has full merit. Nested parentheses ended the span early. `FindSpanEnd` now counts depth, and `NestedParenthesesAreOneOpaqueWord` covers it.
- P2-3 has full merit. A trailing argument after `--root` was silent. The loop now rejects every argument that is not a `--root <value>` pair, with exit 2 and a message that names the argument. Three new assertions cover it.
- Wrote `docs/reviews/pr-10-response.md`.
- The review commit `29b1066` was on the local checkout and not on the remote. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code corrections too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- The reviewer commits on this checkout when both providers share it. Check `git status` for an unpushed commit before you start, and push it.
- `Sentence.Words` holds the masked tokens. Unmask a word before you compare it to text.
- Count the words of a test sentence with the splitter, not in your head. Three expectations in this PR were off by one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 per the repeat review procedure and updates `docs/reviews/pr-10.md` to the new effective head.

## Session 22: 2026-09-07, Codex

Author: Codex
Session: PR-10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Reviewed PR #10 at effective head `3ea5f23` against base and merge base `94aadc5`.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Found three P2 defects in the sentence splitter and command argument parser.
- Wrote `docs/reviews/pr-10.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 61 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 needs the three P2 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The checker only treats a colon as a sentence end when whitespace or the line end follows it. The project rule says that every colon ends a sentence.
- Nested parentheses do not remain one opaque word. The mask pairs the first opening parenthesis with the first closing parenthesis.
- `ste-check` ignores a trailing argument after a valid `--root` pair and returns the repository result.
- The review commit is metadata. The review head remains `3ea5f23` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 to P2-3, add the regression tests, and request a repeat Codex review.

## Session 21: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-2, the STE checker. Branch `feat/pr-2-ste-check`, PR #10.

### What this session did, and why

- Started PR-2 after the owner merged PR #9. Session 19 named it as the next action, and nothing blocked it.
- Wrote the `ste-check` command of `WhatYouCarry.Tools` (D-130, D-139). One reader turns a Markdown file into prose lines, one splitter turns a line into sentences and words, and one rules class holds the seven STE rules. The reference check (D-178, D-186) and the session number check (D-187) are two more classes. `RepositoryCheck` runs all three over one checkout.
- The splitter masks text in backticks, double quotes, and parentheses. Each span is one opaque word (8.5, 8.6), and no grammar rule reads inside it. A colon ends a sentence everywhere (8.4).
- Wrote 25 tests. Exit tests 1 to 9 of the roadmap each have a test, and the command test runs `Program.Main` on a throwaway checkout for the exit codes 0, 1, and 2.
- Wrote `.github/workflows/ste-check.yml` with the one job `ste-check`. It runs on Linux only, because the checks read text.
- The first run found 77 findings in 15 files: 52 passive, 15 helper verbs, 5 over 25 words, and 5 -ing forms. Two were rule gaps. A hyphenated identifier in a skill front matter matched the -ing rule, and the noun "finding" matched it after "per". Both are exclusions now, with tests. The other 75 were real, and each sentence is rewritten with the same meaning.
- Rewrote the checker section of the `ste-writing` skill. It gives the command, a table of the rule ids, the exemptions, and the Markdown conventions the checker needs.
- Marked PR-1 and PR-2 done in the design doc and the roadmap. PR-1 was still marked planned after its merge.

### State of the build

- `main` is at `94aadc5`. The branch holds one code commit, `3ea5f23`, and this entry.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 61 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. CI, `ste-check`, and `review-gate` run on it from the trunk workflows.

### In flight

PR #10 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- The passive rule is a heuristic: an auxiliary, an optional adverb, and a word that ends in "ed" or is on the irregular list. "is required" and "is closed" are findings. Rewrite with the actor as the subject.
- "have" before a participle is a complex tense, so "the docs have dated records" is a finding. Use "hold".
- The checker reads each line alone. A sentence that wraps to a second line counts as two short ones.
- The reference check skips the exempt paths. A dated record cites old decisions as history, and a rewrite to name the reviser falsifies it.
- `perl -p` reads one line at a time. A multi-line replacement needs `-0`, and an argument that starts with a hyphen needs `--` before it. Both failed silently this session before the fix.
- A `## Procedure` or `## Sequence` heading gives every numbered item below it the 20-word limit, until the next heading at any level.
- To add a technical name that ends in -ing, add it to `NotIngForms` in `SteRules.cs` with a test.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #10 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-10.md`. After the merge, PR-3 starts: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 20: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Re-reviewed PR #6 at effective head `03e6a29` against base and merge base `546a70a`.
- Confirmed P1-1 is fixed in `9624cfa`. The `pull_request_target` workflow runs the base-branch evaluator and fetches the PR head as data only.
- Confirmed P1-2 remains an accepted risk under D-198. The output names the commit that last changed the review file, and the repository cannot prove provider identity with its shared unsigned commits.
- Found no new finding. Updated `docs/reviews/pr-6.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 35 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI at head `03e6a29`.
- The review-gate check does not run on PR #6 before the workflow reaches `main`, as recorded in D-197 and F-58.

### In flight

The owner can merge PR #6. After the merge, run the adversarial proof against `main` that D-197 requires.

### Traps and gotchas

- The `pull_request_target` workflow is available only after its file reaches the default branch.
- The review-record identity risk remains accepted under D-198.
- The effective head is `03e6a29`, because the latest register changes are outside the metadata paths.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #6. Then run the adversarial proof against `main` and record its result.

## Session 19: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #6 review. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Read the two P1 findings in `docs/reviews/pr-6.md` and assessed each against the evidence.
- P1-1 has full merit. The `pull_request` event ran the tool from the PR head with `checks: write`. The owner chose `pull_request_target` (D-197). The workflow now checks out the base, fetches the PR head as data, and runs the base-branch tool. `ReviewGateRunsOnPullRequestTarget` covers it.
- P1-2 has partial merit. The rewrite of a review file reproduces, and the requested identity check cannot be built, because every commit has one identity and no signature. The owner accepted the risk under D-190 (D-198). The output now names the commit that last changed the review file. Two tests cover it.
- Opened PR #7, a throwaway adversarial PR against the PR-1 branch, to prove the trusted evaluator live. GitHub produced no run. The events reference says `pull_request_target` triggers only when the workflow file exists on the default branch (F-58). Closed PR #7 and deleted the branch.
- Wrote `docs/reviews/pr-6-response.md`, D-197, D-198, F-56 to F-58, and the roadmap corrections.
- After the owner merged PR #6 as `a3b20e2`, opened PR #8, a throwaway adversarial PR against `main`. The trusted tool from `main` answered `neutral` on the adversarial head, and the head's approval never posted. Run 34180347093. Closed PR #8 and deleted the branch. The response file records the proof.

### State of the build

- `main` is at `a3b20e2`, the squash merge of PR #6. The CI workflow and the review-gate workflow are on the trunk.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 35 tests, 0 failures. CI passed on the three platforms at `9624cfa`.
- The `review-gate` check now runs on every PR against `main` from the trunk workflow (D-197). PR #8 proved it.

### In flight

PR #6 is merged. The branch `docs/d-197-proof` records the proof in the response file and this entry. It changes documents only, so the owner adds the `review-override` label (D-190). That label run is the first live use of the override path.

### Traps and gotchas

- `pull_request_target` reads the trigger from the default branch. A workflow on a feature branch never runs for that event, and a PR against that branch gives no error, only silence (F-58).
- A `pull_request_target` run lists the base branch as its head branch in `gh run list`. Do not filter by the PR branch.
- The gate cannot see PR #6 until the merge. The owner merges PR #6 on the review record alone, as for PR #1 to PR #5.
- The identity of a commit proves nothing about the provider (D-198). Read the "Review file last changed by" line in the check output.
- The adversarial proof must never merge. Close the PR and delete the branch after the run.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner adds the `review-override` label to the proof PR and merges it. Then a new session starts PR-2, the STE checker (D-121: one session, one PR).

Entries older than the 10 newest sessions move here from `docs/session-handoff.md` (D-146). Newest first.

## Session 18: 2026-09-08, Codex

Author: Codex
Session: review PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Reviewed PR #6 at head `39db1f9` against base and merge base `546a70a`.
- Confirmed the opposite-provider gate. The PR author is Claude Code, and the reviewer is Codex.
- Found two P1 defects in the review gate. The workflow runs PR-controlled evaluator code with `checks: write`. A PR author can replace an approved review record in a metadata-only commit.
- Wrote `docs/reviews/pr-6.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 32 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI for PR #6. The custom review-gate check is neutral until the review record approves the head.
- The Godot command did not run because `Godot` is not on this checkout's command path.

### In flight

PR #6 needs both P1 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The review-gate workflow checks out the PR head and runs `WhatYouCarry.Tools` from that head.
- The effective-head rule excludes `docs/reviews/`, so it does not detect a review-file replacement.
- PR #6 has GitHub base `546a70a`, while `main` now points to `4ec9708`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P1-1 and P1-2, then request a repeat review of PR #6.

## Session 17: 2026-09-07, Claude Code

Author: Claude Code
Session: PR-1, the repository scaffold. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Started PR-1 from `/Volumes/SSD-1TB/what-you-carry` after the owner merged PR #5. Session 16 named it as the next action, and nothing blocked it.
- Created `WhatYouCarry.slnx` with the four projects (D-108, D-194), `global.json` on SDK 10.0.400 (D-173), and `Directory.Build.props` with nullable on, warnings as errors, and one language version (D-68).
- Put one directory per project at the root. `project.godot` lives in `WhatYouCarry.Game/`, next to its project file, so the Godot editor scans the Game directory and not the whole checkout. The solution stays at the root. This is a layout judgment, and not a decision.
- Wrote the `review-gate` tool as the `review-gate` command of `WhatYouCarry.Tools` (D-65). The rules are one pure function. Git reads are one class. The workflow only gathers the inputs and posts the check run.
- Wrote 32 tests. Every exit test with a name in the roadmap has a test with that name. The git-backed tests build throwaway repositories and run the real `git log` pathspec command.
- Wrote `ci.yml` with one job per platform (D-71, D-100, D-157, D-189) and `review-gate.yml` on the five event types (D-190).
- Wrote the PR template with the gate checklist, the absent checks, and the document lines (D-118, D-148).
- Ran the Godot 4.7.2 editor headless with `--build-solutions` on the Game project. It built, made no solution file, and left the project file unchanged.
- Ran the tool against this checkout with `origin/main` as the base. It found that the base has no mode file, and it gave a failure that names the file. That was OQ-72. The owner chose option (b), and this session put the one-line file on `main` in `4ec9708` on that instruction (D-196). The owner asked first whether a Codex approval plus the override label gives green. It does not: the mode rule runs first, and D-190 keeps code paths out of the override.
- Added the build and test commands to `CLAUDE.md` and `AGENTS.md` (D-122).

### State of the build

- `main` is at `4ec9708`, which holds only the mode file above `546a70a`. The branch `feat/pr-1-scaffold` holds the scaffold commit, the D-196 records, and this entry.
- `dotnet build WhatYouCarry.slnx` and `dotnet test WhatYouCarry.slnx --no-build` pass on the Mac Mini: 32 tests, 0 failures.
- PR #6 is open. The first CI run passed on all three platforms, with 32 tests on each. The `review-gate` workflow posted its check run on the head commit, with the OQ-72 failure.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #6 is open and waits for a Codex review (T-4). After D-196 the `review-gate` check on PR #6 reads `advisory` from `main` and shows grey until the review record lands. The CI workflow passed its first run on this PR on all three platforms.

### Traps and gotchas

- The Godot editor writes `TargetFramework` `net8.0` into a project file that has none, and it keeps a `.csproj.old` copy. Each project file names `net10.0` for that reason. Do not move the target framework into `Directory.Build.props`.
- The Godot SDK knows the configurations `Debug`, `ExportDebug`, and `ExportRelease`. CI builds the default `Debug`. A `--configuration Release` build of the solution is untested.
- `git cat-file -e <rev>:<path>` exits 128 for an absent path, and not 1. The tool uses `git ls-tree`, which prints nothing and exits 0 for an absent path.
- The tool reads the mode file from the base branch, so the base must hold it. D-196 put it there before PR-1 merged. A repository rebuild must keep that file on the trunk.
- One commit went to `main` without a PR on the owner instruction (D-196). That is the exception, not the rule (D-126, D-170).
- A merge of `main` into a PR branch is a commit outside the metadata set, so it moves the effective head and needs a repeat review. A rebase does the same.
- Git marks its object files read-only. The temp-repository helper clears the attribute before delete, or Windows refuses the delete.
- `dotnet run` prints the build output to stdout, so the tool writes its result to a file and never to stdout.
- The workflow uses `jq --slurp` to make one array from the paginated timeline. An empty timeline gives `[]`.
- `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are in the test project as the xUnit stack under D-66. They are the packages that `dotnet test` needs to run xUnit. No separate decision entry exists for them.
- `timeout` does not exist on macOS. `perl -e 'alarm N; exec @ARGV'` does the same job.

### Open questions that block progress

No open question blocks PR-1. OQ-72 is resolved by D-196. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #6 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-<number>.md`. After the merge, PR-2 starts: the STE checker.

## Session 16: 2026-09-07, Claude Code

Author: Claude Code
Session: the toolchain install, the runner registration, and the documentation PR override. Branches `docs/runner-path-and-doc-override` (PR #2, merged) and `docs/runner-registration` (PR #3).

### What this session did, and why

- PR #1 merged as `5c6d7b5`. The next action of session 15 is complete.
- Installed the .NET 10 SDK, version 10.0.400, and the Godot 4.7.2 .NET editor. The runbook names both tools, and this machine had neither.
- The Homebrew cask for the SDK installs a package file that needs an administrator password. This session cannot enter a password, so the Microsoft script `dotnet-install.sh` installed the SDK to `~/.dotnet`.
- Made a test solution with a class library and an xUnit project. `dotnet build` and `dotnet test` were successful, and the default target framework is `net10.0`.
- Found a risk. A launch agent starts with a minimal path, so a CI job on the self-hosted runner cannot find `dotnet`. The owner chose `actions/setup-dotnet` with the `global-json-file` input (D-189).
- The owner gave an override for a documentation-only PR (D-188). This session recorded it in the decision register, `CLAUDE.md`, and `AGENTS.md`.
- Found that D-188 could not work at launch. In enforced mode the `review-gate` job fails a PR with no review record (D-181, D-185). The owner chose the label `review-override` and a wider eligible path set, and D-190 records the mechanism. PR-1 implements it, and the roadmap now holds four more exit tests.
- The SSD came one day early. The owner formatted it as case-sensitive APFS with no encryption, and named the volume `SSD-1TB` (D-191). A probe confirmed the case sensitivity, the write access, and that the volume keeps a file mode.
- Registered the runner on 2026-09-07, one day before the date in D-171 (D-192). The name is `mac-mini-m4` and the version is 2.337.0.
- The launch agent failed at once with `Operation not permitted`, and it exited 126. macOS denies a launch agent every path on an external volume. A launchd probe repeated the denial, and a login shell read the same path correctly (F-54).
- The owner chose the Full Disk Access grant over a move to the internal drive, and added `/bin/bash` and the runner `node` binary (D-193). The runner is online and listens for jobs.
- Ran a smoke job on the runner from a throwaway branch, and then deleted the branch. The job proved five things: the runner accepts a job, the launch agent reads the external volume in a real job, `actions/checkout` works, `actions/setup-dotnet` resolves 10.0.400 from `global.json`, and a build and test cycle passes. The macOS leg of PR-1 is no longer a guess.
- The smoke job found that the .NET 10 SDK creates a `.slnx` file, and the roadmap named `WhatYouCarry.sln` (F-55). A probe proved that Godot 4.7.2 builds from a `.slnx` and creates no `.sln`. The owner chose `.slnx` (D-194).
- Moved the checkout to `/Volumes/SSD-1TB/what-you-carry` (D-195). `git fsck` reported no corruption. This completes the SSD step in the Phase 1 sequence.
- Deleted every merged branch, and pruned the stale remote-tracking refs.

### State of the build

- `main` is at `4ddf2d5`. No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The .NET SDK and the Godot editor are ready on the Mac Mini.
- The runner `mac-mini-m4` is online, and it is not busy. The work directory is `/Volumes/SSD-1TB/actions-work`.
- One smoke job passed on the runner on 2026-09-07. No workflow is in the repository, because the smoke branch is deleted. PR-1 adds the first tracked workflow.

### In flight

PR #2, PR #3, and PR #4 are merged. PR #5 holds the checkout path. This session made four PRs, against the one PR rule of D-121. A direct push to `main` needed a rewind, the smoke job produced a decision, and the checkout move produced another.

This PR changes documentation only. The owner gives the D-188 override and merges it without a cross-provider review. The PR is also eligible under the D-190 path set, so the rule covers its own PR.

### Traps and gotchas

- A launch agent does not read `~/.zshrc`. Do not expect a login shell path on the runner.
- A launch agent reads no external volume without Full Disk Access (F-54, D-193). A machine rebuild repeats the grant, or every job fails.
- Every bash process on the Mac Mini now reads every file. That is the cost of the work directory on the SSD (D-193).
- `actions/setup-dotnet` installs to `~/.dotnet` on this runner, and it reported `already installed` for 10.0.400. No job downloads the SDK again.
- `dotnet new sln` gives a `.slnx` file on .NET 10. A command that names a `.sln` file fails with MSB1009 (F-55).
- The checkout is on the SSD. A session needs the volume mounted, or no file opens (D-195).
- D-190 closes the launch hole in D-188. PR-1 must build the override path, or an overridden PR turns red at launch.
- The `review-override` label does not survive a new commit. A push outside the metadata set after the label needs the label again (D-190).
- The agents hold the owner GitHub token. The label stops an accident, and it does not stop an attack (D-190).
- The `dotnet-sdk` cask needs an administrator password. The Microsoft script needs none.
- This PR holds four concerns, which is against G-10. The session named the conflict, and the owner chose one PR.
- The file held 11 entries before this session. This entry restores the limit of 10 (D-146).
- A commit went to `main` directly, against D-126. The owner merged PR #2 and moved the local checkout to `main`. The uncommitted work moved with the checkout, and the next commit and push landed on the trunk. Check the branch name before each commit.
- The owner chose a rewind. `main` returned to `2d69270`, and the work came back as PR #3. A force push to a trunk is safe only while no other clone holds the old commit.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Start PR-1 from `/Volumes/SSD-1TB/what-you-carry`. The runner, both tools, the CI chain, and the checkout move are complete, so no owner purchase or setup blocks it. PR-1 has 23 exit tests, and the roadmap holds the full scope.

## Session 15: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the P2-3 correction. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Re-reviewed the diff through effective head `c255a17`.
- Confirmed P2-3 is fixed. The F-51 row now names `.github/review-gate-mode` and the base-branch read.
- Confirmed the sweep leaves only the historical D-181 decision row for `REVIEW_GATE_MODE`, with its revision marker.
- Updated `docs/reviews/pr-1.md` to approve the effective head.

### State of the build

- No code, solution, or CI workflow exists. PR-1 creates them.
- The effective-head check resolves to `c255a17` after excluding the D-184 metadata set.
- `AGENTS.md` and `CLAUDE.md` remain byte-identical.
- The review verdict is Ready for owner merge.

### In flight

PR #1 is ready for the owner to merge. The owner must complete the runner and SSD actions before implementation work starts (D-145, D-171).

### Traps and gotchas

- The current mode source is `.github/review-gate-mode`, not `REVIEW_GATE_MODE` (D-185).
- The review record must name the effective head, not the metadata commit (D-184).
- The review and handoff commits must be pushed to the PR branch (D-182, D-183).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #1 after confirming the review-gate and other PR gate conditions. Then run `docs/runbooks/macos-runner.md` on 2026-09-08.

## Session 14: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the P2-3 finding. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The review at head `e51e272` raised P2-3. It has full merit. The F-51 row in the design register still said that `REVIEW_GATE_MODE` selects the mode, and D-185 replaced that variable with the tracked file `.github/review-gate-mode`.
- The defect came from the D-185 citation pass in session 12. That pass added the `D-185` id to every line that cited `D-181` by script, and it did not read the prose beside the id. A mechanical citation edit does not make the sentence true.
- Corrected the F-51 row. It now names the tracked file and the base-branch read.
- Ran the sweep that the finding specifies across every current document. It returned two further hits in `docs/decisions.md`, and neither is a defect. The D-181 row records what D-181 said and carries its revision marker. The D-185 row named the thing it replaced, and its Effect now says "Revises the mode source in D-181", which D-186 requires.
- Used the D-187 procedure for the first time. A fetch showed session 13 on the remote, so this entry is session 14. No collision.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds the review commits and this session's commit.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review. P2-3 is corrected in documents only, because no workflow exists yet.

### Traps and gotchas

- A mechanical citation pass must read the sentence that holds the citation. The script that added `D-185` beside `D-181` left one sentence false, and the review caught it.
- The sweep for a removed name belongs with the citation pass, not after the next review.
- The D-187 procedure works. Fetch, read the highest number, then add one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff through `e51e272` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 13: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the D-184 and D-185 corrections. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Re-reviewed the diff through effective head `e51e272`.
- Confirmed P1-3 is fixed: the tracked `.github/review-gate-mode` file supplies `advisory`, and the workflow reads it from the base branch (D-185).
- Confirmed P1-4 is fixed: the review, handoff, and archive paths are metadata, so the required review commit does not change the effective head (D-184).
- Added P2-3: the F-51 row in `docs/design.md` still names the removed `REVIEW_GATE_MODE` variable as the mode source.

### State of the build

- No code, solution, or CI workflow exists. PR-1 creates them.
- The effective-head check resolves to `e51e272` after excluding the D-184 metadata set.
- `AGENTS.md` and `CLAUDE.md` remain byte-identical.
- The review verdict is Changes required.

### In flight

PR #1 needs the F-51 mode-source text corrected in `docs/design.md`.

### Traps and gotchas

- The current mode source is `.github/review-gate-mode`, not `REVIEW_GATE_MODE` (D-185).
- The review record must name the effective head, not the metadata commit (D-184).
- The review and handoff commits must be pushed to the PR branch (D-182, D-183).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct the F-51 row, then request another repeat review of PR #1.

## Session 12: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the P1-3 and P1-4 findings, and add the push-back rule to the review skill. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The review at head `6e45d6f` raised P1-3 and P1-4. Both have full merit. Recorded D-184 and D-185 and wrote the dispositions in `docs/reviews/pr-1-response.md`.
- P1-4: D-182 requires the reviewer to commit the review record with the handoff entry, and the effective head excluded only `docs/reviews/`. The review commit therefore became the effective head and rejected the record it published. Reproduced it. The old rule returned `eab18db` while the record named `6e45d6f`.
- D-184 defines the metadata set: `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. With that set excluded the effective head resolves to `8efb267`, the last substantive commit. The archive is in the set because a handoff rollover writes it in the same commit.
- P1-3: a workflow cannot create a repository variable, so PR-1 could not pass the check it creates, against G-19. D-185 moves the mode to the tracked file `.github/review-gate-mode`. PR-1 creates it with `advisory`. The workflow reads the file from the base branch, so a PR cannot change the mode that judges it.
- The owner asked that a request to address review findings load the `pr-review` skill, and that the skill state that a finding is a claim, not a fact. Added an "Address review findings" section with a seven-step procedure and a push-back table. Widened the skill description so the request triggers it.
- D-185 revises D-181, which made fifteen citations stale. The D-178 check found each one. They now name D-185.
- The owner then asked how to stop the two recurring problems. Recorded D-186 and D-187, and F-52 and F-53.
- D-186 splits the revision marker. `Superseded by D-N` replaces the whole answer, and every citation must name D-N. `Revised in part by D-N` changes one named part, and the decision stays citable. The Effect column must name the part that changed and the parts that stand.
- Reclassified the seven revisions. D-94, D-158, D-169, and D-172 are superseded. D-136, D-179, and D-181 are revised in part. The register already used `Superseded by` for D-10 and D-95, so the verb is not new.
- The D-178 check now keys on `Superseded by` only. Partial revisions carried 33 of the 48 citations, and they produced all three rounds of churn.
- D-187 fixes the session number. Fetch the remote and read the handoff again immediately before the handoff commit, then take the highest number and add one. Every session pushes (D-183), so a push serializes the writers. The PR-2 checker fails on a duplicate number.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds the review commits and this session's commit.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review. P1-3 and P1-4 are corrected in documents only, because no workflow exists yet.

### Traps and gotchas

- The effective head excludes three paths now, not one (D-184). A review commit that touches only those paths is metadata.
- The mode file is read from the base branch, never the PR head (D-185). A PR that edits the mode file does not change its own mode.
- A finding is a claim, not a fact. Assess it, and refute it with evidence when it does not hold.
- A partial revision still marks the whole decision as revised, so every citation must name the reviser. D-181 needed fifteen edits.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff since `6e45d6f` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 11: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the neutral review-gate change.

### What this session did, and why

- Rechecked PR #1 at effective head `6e45d6f`, including the D-181, D-182, and D-183 changes.
- Confirmed the prior four findings remain fixed.
- Added P1-3 to `docs/reviews/pr-1.md`: PR-1 does not define how `REVIEW_GATE_MODE=advisory` is created.
- Added P1-4: the required review commit changes the handoff path and invalidates its own effective head.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- The review verdict is Changes required.

### In flight

PR #1 needs an owner setup step or bootstrap mechanism for the required review-gate repository variable, and a metadata-path correction for the effective-head rule.

### Traps and gotchas

- D-181 forbids a default for an absent or unknown `REVIEW_GATE_MODE`.
- The PR-1 roadmap says it sets the variable, but the scope contains no repository-state action.
- D-182 and D-183 require the review record and handoff in one commit, but D-179 excludes only `docs/reviews/`.
- The effective head at review start is `6e45d6f`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Define the advisory-mode variable setup, correct the metadata-path rule, revise PR #1, and request another repeat review.

## Session 10: 2026-09-07, Claude Code

Author: Claude Code
Session: the neutral grey state for `review-gate`. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The owner asked where the red check appears, then asked for a neutral grey state instead of a permanent red one. `review-gate` failed whenever no review record existed, so a PR stayed red for most of its life and a red rollup masked a real build failure.
- Verified first that GitHub counts a neutral or skipped conclusion as a success for a required status check. Source: docs.github.com, "About status checks", verified 2026-09-07. A plain grey-when-absent gate would stop blocking a merge at launch. Recorded F-51.
- D-181 gives the check three conclusions and two modes. In `advisory` mode a missing review file is neutral. In `enforced` mode it is a failure. The repository variable `REVIEW_GATE_MODE` selects the mode. An absent or unknown value fails the job and names the variable (T-2).
- A workflow job cannot set a neutral conclusion by its exit code. The job publishes a check run through the Checks API, so the workflow needs `checks: write`.
- D-181 revises D-179. D-180 is not revised, because D-181 only adds the mode step to its launch procedure.
- Marking D-179 as revised made nine citations stale. The D-178 check found each one. They now name D-181.
- Updated the PR-1 scope and exit tests to sixteen, both agent files, the `pr-review` skill with a color table, Phase 5 step 11, and the design register.
- Recorded D-182 after the handoff and review record of sessions 8 and 9 reached this session uncommitted. A later commit absorbed them, and this session then reported the wrong verdict. The `pr-review` skill now requires a commit of the review record with its handoff entry, and the agent files carry the same rule.
- D-182 narrows the scope limit in the skill, which listed a commit as an unauthorized action. A code fix, a merge, and an external message stay unauthorized.
- D-183 lets the reviewer push its own review commit to the PR branch. A push is the only way `review-gate` reads the record, because the gate reads the PR head. The reviewer never pushes to `main`.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds fifteen commits, all on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

**PR #1 is not ready to merge.** Session 9 approved head `223aae8`. This session pushed `4f7796e` after that approval, and it changes eight files outside `docs/reviews/`, including both agent files, the decision register, and the review skill. The approval does not cover the effective head. Rule 3 of D-179 applies.

### Traps and gotchas

- The verdict in `docs/reviews/pr-1.md` reads `Ready for owner merge`, and it applies to head `223aae8` only. Read the head field, not the verdict alone.
- Grey is correct only in advisory mode. Never use a neutral conclusion for an enforced gate (F-51).
- The `review-gate` job stays green itself. The check run it publishes carries the color, so the Checks list holds two rows.
- Never write a decision range that spans a revised id. D-179 is revised, so a header says `D-176 to D-178, D-180, and D-181`.
- Read the review file before a commit that sweeps it in. This session committed an approval it had not read, and then reported the wrong verdict.
- One session is one handoff entry (D-146). Add a new entry. Do not append to an older one after another provider writes above it.
- A review record that is not committed is invisible to `review-gate`, because the gate reads the PR head (D-182).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff from `223aae8` to `4f7796e` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 9: 2026-09-07, Codex

Author: Codex
Session: final repeat review of PR #1 on `docs/roadmaps`.

### What this session did, and why

- Rechecked PR #1 at effective head `223aae8`.
- Confirmed the P2-2 fix and reviewed the effective-head command clarification.
- Updated `docs/reviews/pr-1.md` with a Ready for owner merge verdict.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- All four prior findings are fixed. No new finding remains.

### In flight

PR #1 is ready for owner merge. Build and CI checks remain deferred because this PR defines the solution and workflows that PR-1 creates.

### Traps and gotchas

- The reviewed effective head is `223aae8`.
- The review file is machine-read. Keep its head field and verdict exact.
- The owner must still register the runner and move the checkout to the SSD before PR-1.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner can merge PR #1. Then complete the SSD and runner actions before starting PR-1.

## Session 8: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 on `docs/roadmaps`.

### What this session did, and why

- Rechecked PR #1 at effective head `60087b0` against the prior review and the author response.
- Confirmed fixes for P1-1, P1-2, and P2-1.
- Added P2-2 to `docs/reviews/pr-1.md` because the Phase 1 header omits PR-58, D-177, and D-178.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- The review verdict remains Changes required.

### In flight

PR #1 needs a small roadmap header correction. The review record now names head `60087b0`.

### Traps and gotchas

- The effective head is the newest commit outside `docs/reviews/`.
- The old findings stay in the review record with fixed dispositions.
- The Phase 1 sequence includes PR-58, but the roadmap header still states PR-1 to PR-11 only.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct the Phase 1 roadmap header and request a final repeat review against the new effective head.

## Session 7: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the PR #1 review. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Read `docs/reviews/pr-1.md` and checked each of the three findings against the branch, the registers, and the PR.
- P1-2 and P2-1 have full merit. P1-1 has partial merit. The owner approved every disposition. Recorded D-176 to D-178 and F-47 to F-49. Wrote `docs/reviews/pr-1-response.md`.
- P1-1: T-6 and D-137 prohibit text that names an agent, harness, or model as the source of the work. A tool name that identifies a configured file is not attribution, so the broad reading in the finding would also condemn D-172, D-175, OQ-16, F-15, the PR description, and the settings file path. D-176 states the boundary. The body of the head commit is rewritten, because one clause implied that an agent wrote the commits. PR-1 exit test 6 now scans every subject and body, not only trailers.
- P1-2: PR-11 created the night job and the `night-gate` job together, so the gate had no result to read on its first run, against G-19. D-177 splits them. PR-11 publishes a result record. The new PR-58 adds the gate after one night runs. An absent, stale, cancelled, or failed record fails the gate.
- P2-1: fixed all six stale references. The review named five. A sweep found a sixth at `phase-1-foundations.md:412`. D-178 adds a reference check to the PR-2 checker, so the next revision cannot leak.
- The owner asked for a GitHub merge criterion that blocks a merge without the review files. GitHub returns 403 for branch protection and for rulesets on a private free repository, verified this session. No hard block is possible today.
- Recorded D-179 and D-180 and F-50. PR-1 gains a `review-gate` job. It reads `docs/reviews/pr-<number>.md`, requires the verdict `Ready for owner merge`, and requires the recorded head to be the effective head. The job is advisory until launch. Phase 5 step 11 makes it a required check after the repository becomes public.
- The owner asked for the head to match the PR head. The `pr-review` skill says the opposite: do not require the review file to hold its own hash. The effective head reconciles both. The effective head is the newest commit outside `docs/reviews/`.
- The owner chose not to require the response file. The gate reads the review file only.
- Codex re-reviewed at head `60087b0`. P1-1, P1-2, and P2-1 are marked fixed. One new finding, P2-2, is open: the Phase 1 roadmap header omits PR-58 and the new decisions, and it still says `Correction passes: none yet`.
- P2-2 has full merit. Fixed line 3 and line 9 of the Phase 1 roadmap. Gave `phase-5-early-access.md` the same treatment, because this PR added its sequence step 11. The review did not name that file.
- Ran the regression check that P2-2 specifies. It found three more defects of the same class. The new phase-1 range `D-156 to D-168` swallowed the revised D-158, `phase-2-first-playable.md` had the same defect in `D-157 to D-168`, and phase-1 line 465 cited D-158 with no revision marker. All three are fixed.
- A wider sweep found two older ones: `docs/design.md:28` cited D-136 alone, and `phase-3-full-loop.md:372` cited D-94 alone. Both now name the revising decision.
- Refined D-178. A line passes the reference check when it holds a revision word or when it names the revising decision. Without that clause the check fails on F-34, F-46, and four correct `D-94, D-152` pairs.
- Rewrote `.claude/skills/pr-review/SKILL.md` for the format. It now holds a review file skeleton, the three machine-read fields, a finding format with stable `P<severity>-<n>` ids, the attribution boundary of D-176, the gate rules, a ten-step repeat review procedure, and the response file contract.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds thirteen commits. The head commit of session 5 was amended, and the branch was force pushed.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review by Codex against the new head.

### Traps and gotchas

- The head commit was amended. The review file `docs/reviews/pr-1.md` names head `9459534`, which no longer exists.
- D-176 fixes the attribution reading. Do not strip a tool name that identifies a configured file, a schema, or a version. Strip a claim about the source of the work.
- The owner squash-merges (D-126). GitHub fills the squash body with every commit message. Check that body before the merge.
- PR-58 is new. Phase 1 now ends with PR-11, one scheduled night, PR-58, then the measurements and Gate 1.
- Ids never change. PR-58 sits after PR-11 in the sequence, not after PR-57.
- The PR-2 reference check skips a line that holds `revises`, `revised by`, or `supersedes`. The two F-15 history lines were reworded to hold that word.
- Branch protection and rulesets both return 403 on this repository. Do not plan a hard merge block before launch (D-180).
- `review-gate` reads three exact things: the file name, the `- Head: ` line, and the verdict name. A reworded verdict fails the job.
- The effective head ignores a commit that changes only `docs/reviews/`. A review file commit does not invalidate its own approval.
- `docs/reviews/pr-1.md` now records head `60087b0` and holds four findings. P2-2 is the open one, and this session fixed it.
- A decision range in a header can swallow a revised decision. Write `D-156, D-157, D-159 to D-168`, not `D-156 to D-168`, when D-158 is revised.
- The roadmap header is a scope summary. Update line 3 and line 9 whenever a PR entry, a measurement, or a governing decision changes.
- Never write a decision range that spans a revised id. D-179 is revised, so a header says `D-176 to D-178, D-180, and D-181`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #1 a third time against the new head, per `.claude/skills/pr-review/SKILL.md`. P2-2 is the only finding to confirm. Sessions 8 and 9 did that work.

## Session 6: 2026-09-07, Codex

Author: Codex
Session: review PR #1 on `docs/roadmaps`.

### What this session did, and why

- Read the handoff, agent rules, review skill, STE skill, design, decisions, questions, existing reviews, and focused roadmaps.
- Verified PR #1 at base `1c16c45` and head `9459534`.
- Wrote `docs/reviews/pr-1.md` with three findings and a Changes required verdict.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff passes `git diff --check`.
- The agent files remain byte-identical. The settings file parses as JSON.

### In flight

PR #1 needs a revision. The review identifies prohibited attribution in a commit body, an undefined first-run path for `night-gate`, and stale references to D-172 and the unresolved OQ-2 state.

### Traps and gotchas

- T-6 applies to commit bodies as well as commit subjects.
- D-175 supersedes D-172. D-173 supersedes D-169.
- PR-11 creates the night result and the gate. The empty-result case needs an explicit contract.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Revise PR #1, then run a repeat review against the new head. Recheck the commit history, the first night-gate run, and every current-status reference to the revised decisions.

## Session 5: 2026-09-07, Claude Code

Author: Claude Code
Session: fix the attribution setting, no new PR. Branch `docs/roadmaps`, pushed, part of PR #1.

### What this session did, and why

- Found why the Claude Code startup dialog reported that `.claude/settings.json` failed to parse. The file is valid JSON, but `attribution.commit` and `attribution.pr` were booleans. The schema requires strings. When one value fails validation, the harness ignores the whole file, so the co-author trailer stayed on, against T-6.
- Set both fields to the empty string, which hides the attribution (D-175). Verified against the settings reference, the schema in the VS Code extension 2.1.263, and the validator in the CLI 2.1.261.
- Marked D-172 as revised. Updated OQ-16 and F-15. Pushed one commit to `docs/roadmaps`, so PR #1 carries the fix.

### State of the build

- `main` has one commit, `1c16c45`, on the remote. The branch `docs/roadmaps` holds the six commits of session 4 and one commit of this session, all on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.

### In flight

PR #1 from `docs/roadmaps` to `main` is open and waits for the other provider's review (T-4). The attribution fix is part of it.

### Traps and gotchas

- `attribution.commit` and `attribution.pr` are strings. A boolean makes the harness ignore the whole settings file, and the startup dialog calls it a parse failure.
- A settings file that fails validation loses every setting in it, not only the bad field. Run `/doctor` to see what the harness dropped.
- The traps in the session 4 entry still apply.

### Open questions that block progress

No new question. The session 4 entry lists the open ones.

### Next concrete action

Unchanged from session 4. A Codex session reviews PR #1 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-1.md`. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 4: 2026-09-07, Claude Code

Author: Claude Code
Session: initial commit, the five focused roadmaps, and PR #1. Branch `docs/roadmaps`, pushed.

### What this session did, and why

- Reset HEAD to `main`, added `.gitignore` and `.gitattributes`, and pushed every file to `origin/main` as commit `1c16c45`, message "initial commit" (D-156). The owner asked for a direct push to `main`.
- Created the branch `docs/roadmaps` from `main` for the focused roadmaps (D-156).
- Wrote `docs/roadmaps/phase-1-foundations.md`. Each PR entry has scope, out of scope, exit tests, review focus, a check clause, a gate, and a plain-English paragraph. It adds M-1 and M-2 procedures and a Phase 1 sequence.
- Found two gaps of the audit R-2 class and handled them under D-149. PR-10's gate named a weapon roster that does not exist until Phase 3 (F-38). A test-only definitions file fixes it. The macOS CI leg needs a self-hosted runner that nobody had listed (F-39, OQ-31).
- Added G-21 to the design guardrails: no `System.Random` or wall-clock reads in Core.
- Filed OQ-31 to OQ-42 in `docs/questions.md`: the owner actions and the technical choices that Phase 1 PRs need before they start, each with a recommendation.
- Linked the roadmap from `docs/design.md` section 7.
- Asked the owner OQ-31 to OQ-42 in three batches and recorded D-157 to D-168. The Phase 1 roadmap now cites those decisions instead of the questions.
- Wrote the four later roadmaps in the same format: `phase-2-first-playable.md`, `phase-3-full-loop.md`, `phase-4-content-complete.md`, and `phase-5-early-access.md`. Each has per-PR scope, exit tests, review focus, a check clause, a gate, and a sequence with the owner questions placed before the PR that needs them.
- Filed OQ-43 to OQ-71 for those phases, each with a recommendation. Added F-40 to F-45 to the design register for gaps the roadmaps exposed: wall fade in the mesher, no Deck unit named, no rarity tiers named, no Tier 3 model or budget, no source for the Deck checklist, and no cloud save file set after D-152.
- Pushed every commit on `docs/roadmaps` to the remote after the owner pushed the branch.
- Closed OQ-2 (D-169, .NET 8 LTS, revised the same day to .NET 10 LTS as D-173), OQ-16 (D-172, `.claude/settings.json` with attribution off), and the runner timing (D-171, tomorrow on the SSD). Wrote `docs/runbooks/macos-runner.md`.
- Found that branch protection needs GitHub Pro or a public repository. The owner deferred it until launch (D-170, F-46).
- Renamed the GitHub repository to `nkramber/what-you-carry` and moved the checkout to `/Users/nate/Repos/what-you-carry` (D-174). The runbook cites the new name.

### State of the build

- `main` has one commit, `1c16c45`, on the remote. The branch `docs/roadmaps` holds six commits of this session and is on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The STE checker does not exist until PR-2. This session scanned the changed documents by script.

### In flight

All five roadmaps are on the branch. PR #1 from `docs/roadmaps` to `main` is open and waits for the other provider's review (T-4).

### Traps and gotchas

- The roadmap never restates a decision. It cites D-# ids. Read the `Effect` column before you cite an early decision.
- PR-1 cannot merge until the runner exists (D-157, D-171) and the SSD holds the checkout (D-145). OQ-2 and OQ-16 are closed.
- Nothing on GitHub stops a push to `main` (D-170). The rule in the agent files is the only guard. Never push to `main`.
- The .NET pin is .NET 10 LTS (D-173). D-169 stays in the register as revised.
- The checkout path changed on 2026-09-07 (D-174). Open `/Users/nate/Repos/what-you-carry` in the editor. The session memory for the old path was copied to the new path.
- Each Phase 1 PR has owner questions listed before it in the roadmap sequence. Ask them before the PR starts, not inside it (D-124).
- `Sqrt` in DetMath wraps the IEEE square root. The lint tool must allow `MathF` inside `DetMath.cs` only.
- The night-gate job in PR-11 reads the latest scheduled run. A red night blocks the next merge by design (D-115).

### Open questions that block progress

`docs/questions.md` holds OQ-1 to OQ-71. One stays open for Phase 1: OQ-12 blocks PR-9. One owner action precedes PR-1: the runner registration on 2026-09-08 per the runbook (D-171). The SSD arrives the same day (D-145). Each later roadmap lists its own open questions in its section 6, with the owner's answer placed in the sequence before the PR that needs it.

### Next concrete action

A Codex session reviews PR #1 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-1.md`. The author of this branch is Claude Code, so Claude Code cannot review it. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md` with the volume name. After the owner merges PR #1, PR-1 starts from `phase-1-foundations.md`.

## Session 3: 2026-09-07, Claude Code

Author: Claude Code
Session: audit remediation, no PR. The repository has no commits.

### What this session did, and why

- Assessed the nine findings of `docs/reviews/2026-09-07-repository-audit.md`. All nine have merit. Asked the owner seven questions and recorded D-148 to D-154.
- Wrote `docs/reviews/2026-09-07-repository-audit-response.md` with each disposition.
- Edited `docs/design.md`: header correction (R-8), F-28 to F-37, G-19 and G-20, roadmap entries PR-1, PR-2, PR-3, PR-6, PR-7, PR-9, PR-11, PR-12, PR-16, PR-17, PR-18, PR-22, PR-27, PR-28, PR-30, PR-31, PR-32, PR-49, M-5, a new PR-57, and section 8 rewritten.
- Created `docs/questions.md` as the open questions register (D-144). Section 9 of the design doc now links to it. Added OQ-30.
- Recorded D-144 to D-147 from the owner's message: the questions file, the SSD order, the ten-session handoff rule, and audit-first order. A Codex session added D-155 in parallel.
- Converted this file to the ten-session format (D-146) and created `docs/session-handoff-archive.md`.
- Updated `CLAUDE.md`, `AGENTS.md`, and both skills for the questions file, the handoff rule, and the gate clause.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists. No commits.
- HEAD points at the unborn branch `docs/repository-audit`. The first commit lands there unless the owner resets HEAD (OQ-30, F-37).
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The STE checker does not exist until PR-2. This session scanned the changed documents by script for semicolons, contractions, verb -ing forms, sentences over 25 words, and passive markers.

### In flight

Nothing is half done. Every audit finding has a decision and a document change.

### Traps and gotchas

- D-152 revises D-94: one profile file plus one run record, not three files. Cite D-152.
- D-150 revises D-136: Gate 1 is a foundation gate with no playtest.
- D-149 moves PR-32 before PR-27 and inserts PR-57 after PR-13. Section 8 is the order. Ids never change.
- The design header keeps the refuted Godot 4.7.1 text with a dated note. Do not delete it.
- The handoff now keeps 10 sessions. Add an entry at the top. Do not rewrite the file.
- The attribution rule stands (T-6). OQ-16 remains open.

### Open questions that block progress

`docs/questions.md` holds OQ-1 to OQ-30. PR-1 depends on OQ-2, OQ-16, OQ-30, and the SSD (D-145, arrives 2026-09-08).

### Next concrete action

The owner said the focused roadmaps begin once the audit findings are addressed (D-147). They are addressed. The next action is `docs/roadmaps/phase-1-foundations.md`: expand PR-1 to PR-11 and M-1 to M-2 with per-PR exit tests, under D-148 and D-149. Load `ste-writing` and `design-doc-style` first. Confirm with the owner that Phase 1 is the first roadmap.

## Session 2: 2026-09-07, Codex

Author: Codex
Session: repository audit, no PR. The local repository has no commits.

### What this session did, and why

- Examined the design, all 143 decisions, the archive, both agent files, and both local skills.
- Wrote `docs/reviews/2026-09-07-repository-audit.md` with nine findings, evidence, recommendations, and verification limits.
- Added OQ-25 to OQ-29 in `docs/design.md` section 9. These cover gates, replay state, save transactions, empty-bank recovery, and economy tests.
- Created the local branch `docs/repository-audit` under D-126. All project files remain untracked.
- Recorded the project skill location as D-155. Both agent files now require `.claude/skills/` for current and new project skills.
- Added direct access instructions for a required skill that is absent from the skill list.
- Added no code. No commit, PR, or merge occurred.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists.
- No build or test ran. The repository has nothing executable to check.
- `AGENTS.md` and `CLAUDE.md` are byte-identical. The comparison returned success (D-122).
- The STE checker does not exist until PR-2. The new text received a manual checklist review.
- The previous design interview produced `docs/design.md` v2 and decisions D-1 to D-143.
- The unchanged v1 archive remains at `docs/archive/design-v1-2026-09-06.md`.
- A Git remote URL exists. This session did not verify remote history or backups.

### In flight

The audit and skill location update are complete. New decisions D-144 to D-154 address several audit findings.
The design and agent rules still need the corresponding audit corrections. D-147 requires those corrections before focused roadmaps.

The audit identified four highest-priority findings:

1. The initial PRs require merge checks that PR-2 and PR-3 create later.
2. Several feature gates precede their test tools or game systems.
3. The replay contract omits the initial loadout, skill state, and simulation and content identities.
4. The save contract lacks a shared commit and recovery rule across the three files.

### Traps and gotchas

- The review file records design gaps, not observed runtime defects. No game code exists.
- The five-second rewind in D-97 is an accepted tradeoff, F-17. This audit does not reopen it.
- The Godot 4.7.2 release exists. The design header incorrectly separates stable 4.7.1 from .NET 4.7.2.
- Official links and the verification date appear in the audit. OQ-2 remains open for the .NET version.
- The agent merge gate and the roadmap disagree about the initial checks. OQ-25 records the conflict.
- Section 8 puts palette approval before PR-1. OQ-1 says it blocks PR-14. The owner must settle that scope.
- Keep the attribution rule in every future commit and PR (D-137). OQ-16 remains open.
- The owner owns every open question (D-124). Ask before a choice, and record each answer (D-138).
- Project skills live in `.claude/skills/`. Read their `SKILL.md` files directly when required (D-155).
- Decisions after D-143 revise the audit state. Consult the current decision register before an audit correction.
- D-97 supersedes D-10 and D-95. D-129 and D-132 revise D-120. D-141 keeps decisions in one file until about 300 rows.
- Review ids stay local to the review file. They do not enter the design finding register.

### Open questions that block progress

`docs/design.md` section 9 still contains the audit questions. D-144 moves the register to `docs/questions.md`.
D-148 to D-154 answer several audit issues. Reconcile the register with those decisions before the next owner question.

PR-1 still depends on OQ-2, OQ-16, and the SSD prerequisite in OQ-18. D-145 records the SSD order.
The palette prerequisite has conflicting scopes, as noted above.
D-151 to D-154 resolve OQ-26 to OQ-29. The affected roadmap entries need those contracts before implementation.

### Next concrete action

Continue the audit corrections under D-147. Apply the current decisions before new owner questions.
Keep the two agent files identical (D-122). Check the current register before each new decision id.

The prior sequence remains design v2, then focused roadmaps, then code.
The next planned artifact is `docs/roadmaps/phase-1-foundations.md`.
It needs per-PR exit tests for PR-1 to PR-11 and measurements M-1 to M-2.
Load the local `ste-writing` and `design-doc-style` skills before that work.

## Session 1: 2026-09-07, Claude Code

Author: Claude Code
Session: design interview, no PR. The repository had no commits.

### What this session did, and why

- Read the v1 design document and asked the owner 143 questions in batches. The owner wanted every assumption confirmed before any code.
- Recorded every answer in `docs/decisions.md` as D-1 to D-143.
- Wrote `docs/design.md` v2 from those decisions, in the design-doc-style template and in ASD-STE100.
- Moved the v1 document to `docs/archive/design-v1-2026-09-06.md` unchanged.
- Created `.claude/skills/ste-writing/SKILL.md` and `.claude/skills/design-doc-style/SKILL.md`, adapted to this project (D-131).
- Created `CLAUDE.md` and the identical `AGENTS.md` (D-122).
- Created the empty folders `docs/reviews/` and `docs/roadmaps/`.

### State of the build

- No code. No solution. No commits. The working tree held only documents, skills, and agent files.
- The STE checker did not exist. The session checked the documents by script.

### Traps and gotchas

- The harness default adds a co-author trailer to commits. T-6 forbids it (OQ-16).
- Every open question belongs to the owner (D-124).
- D-97 supersedes D-10 and D-95. D-129 and D-132 supersede the file layout in D-120.
- The owner named the game "What You Carry" (D-11). The v1 name "Descent" appears only in the archive.
- The design doc uses `M-#` for measurements. The five milestones are the roadmap phases, not `M-#` ids.

### Next concrete action at the time

A focused roadmap for Phase 1. Superseded by session 2, the audit.

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
