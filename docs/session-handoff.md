# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 153: 2026-09-13, Codex

Author: Codex
Session: review PR #62, the player and the first sword, at effective head `6e35bc5`.

### What this session did, and why

- Checked the provider gate. Session 152 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the full effective diff, the design contract, decisions D-318 to D-339, the questions register, the Phase 2 roadmap, affected callers, tests, and all PR comments.
- Found P2-1. `WeaponDefinition.AssetPath` accepts dot-segment paths and the wrong file extension because it checks only the `models/` prefix. The record gives the correction and regression check.
- Added `docs/reviews/pr-62.md` with the effective head and the verdict `Changes required`.

### State of the build

- `main` and the merge base are `aeb91df`. The effective head is `6e35bc5`. The review and handoff are pushed in metadata commit `37f519d`.
- The focused Player, Animation, and Content tests passed 138 tests. The serial build passed with 0 warnings and 0 errors.
- The full local test host failed to bind its socket. The non-Smoke full suite did not produce a result in the local execution window. These are execution-context limits.
- Revision-matched remote build-and-test, bit identity, smoke, bots, det-lint, asset QA, STE check, night gate, and Gitar checks passed on `6e35bc5`. `evaluate` and `review-gate` do not approve the head until this record reaches the PR.

### In flight

The author must correct P2-1, run the regression check, and request a re-review at the new effective head. Exit test 7 remains the owner play test.

### Traps and gotchas

- Under D-184, the review head is `6e35bc5`, not a later metadata tip.
- The art quality roadmap item also uses PR-62. It is not GitHub PR #62 and remains after PR-20 under D-339.
- The local full suite has a test-host socket restriction. Do not report it as passed.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 154.

### Open questions that block progress

None blocks the review record. P2-1 blocks merge. Exit test 7 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The author fixes P2-1, runs the regression check, and requests the repeat review. The owner also records exit test 7.

## Session 152: 2026-09-13, Claude Code

Author: Claude Code
Session: write `README.md` with the launch steps, and open PR-15, the player and the first sword, as PR #62. Branch `feat/pr-15-player-first-weapon`.

### What this session did, and why

- PR #61 merged as `aeb91df` at 18:50 UTC on 2026-09-13, after Session 151. The owner asked for a root `README.md` with the steps to launch the game, and then for the next work, PR-15.
- A fresh clone of `aeb91df` with no `.godot` directory built in 4 seconds, ran the headless smoke session to tick 1000, and ended the test exit at tick 101. `README.md` holds those steps, the controls, and the notes for a bad argument. `README.md` is outside the override set of D-190, so the owner chose to carry it in PR-15 (D-318).
- Before the code, the owner answered the gaps of the PR-15 scope in six batches, D-319 to D-337. The sprint conflict of D-231 and D-315 went to 7 m/s (D-319). The attack bit swings the first weapon, and the Phase 1 shot ends, so D-320 supersedes D-265. Against the recommendation, the owner chose free move and live aim for the swing (D-324) and a roll that no hit lands on (D-328). The owner added a guard against a stunlock to the stagger (D-326), chose a sword model with a new metal tile (D-330), and replaced a half roll in water with no roll in water (D-337).
- Core: `Player` holds health, the roll, the stagger and its guard, and the swing. `MeleeWeapon` holds the exact wedge test of the arc. The loop hashes the run end kind as one byte and the player after the projectiles, and the simulation version is 7. `WeaponDefinition` is the `weapon` content type. `Int32.MaxValue` gave three det-lint findings, so the weapon type stores its ticks as `long` with the `UInt32.MaxValue` bound, as the projectile type does, and needs no allowlist entry.
- Content and Game: `sword-basic.json`, `sword-basic.bbmodel`, the metal rule and the atlas, the `weapon` locator, and the swing, roll, and stagger clips. Asset QA found the right forearm in the torso at tick 26 of the first swing clip, and the keyframe moved out to the right. The Game layer plays the clips, walks the limbs from the distance traveled, holds the sword in the right hand, and stands the lowest box corner of the pose on the feet. The smoke script swings and rolls.
- The bit-identity sweep folds a projectile run and the arc test of its own, because no intent fires a shot now. The known answer moved from `6ec00e90c1c85cdb` to `2258ba8b9cc94b3f`.
- The owner read the contact sheet and asked whether these were test materials. The art is the first output of the approved pipeline, and no roadmap item raised it. D-338 keeps the PR-15 sheet as a first pass and closes exit test 8. D-339 adds PR-62, an art quality pass after PR-20 (OQ-171, F-96).
- The owner parked exit test 7, the play test, and runs the Codex review and the merge after the automated pass.
- The automated pass of gitar approved `932cf0a` with one suggestion: an arc with fewer hundredths than active ticks. It had partial merit. The coverage claim did not hold, and a wedge of no turn accepted a point on its line behind the body. `6e35bc5` rejects that arc and that wedge with regression tests, the reply on the thread names the evidence, and the thread is resolved. The pass approved `6e35bc5` at 22:31 UTC with the finding resolved, and its reply on the thread accepted the reasoning. The watcher posted `Gitar review` at 22:35 UTC, because the check run of the head was not listed yet, and gitar answered `On it`. The approved check run on the head is the pass of D-250.
- Session 142 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `aeb91df`. The effective head of PR #62 is `6e35bc5`, the fix commit above the code commit `932cf0a`. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-15-player-first-weapon` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` on the code of `932cf0a`: 841 tests with the five Smoke tests on the local Godot build, 0 failures after the correction of four sentences that `SteCheckTests` found before the commit. The fix commit passed the 250 tests of the classes it touches.
- `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `ste-check`: 0 findings in 16 files. `asset-qa`: 0 findings in 2 models and 3 animations. The Godot build check passed, and the contact sheet rendered in a window.
- On `932cf0a`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and gitar passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251). On `6e35bc5`, CI, bit identity, and smoke passed on the three platforms again, and bots, det-lint, asset-qa, STE check, the night gate, and gitar passed.
- The night gate reads the success of run 34759337453 at `4a1048c`, ended 14:13 UTC on 2026-09-13. It turns red at 14:13 UTC on 2026-09-15 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-14, and it can start hours late (F-95).

### In flight

PR #62: the Codex review per the `pr-review` skill at the effective head `6e35bc5`, the play test of the owner for exit test 7, and then the owner merge. A docs PR then records the merge (D-297).

### Traps and gotchas

- Exit test 7 of PR-15 is open: the owner plays the sword and records the result as a decision before the merge. Exit test 8 closed as a first pass (D-338).
- The bit-identity known answer is `2258ba8b9cc94b3f`. The hash reads the run end as one byte of its kind and the player after the projectiles, and `StairwellTests.HashWith` holds that order.
- Nothing in the loop deals damage before PR-16. A test deals damage with `loop.Player.TakeHit`, which the record does not hold, so a replay cannot see it. PR-16 deals damage inside `Step`.
- The swing clip keeps the right arm in front of its own shoulder, and the torso twist gives the sweep. A straight arm across the chest clips the torso or the head at a keyframe, and asset QA reports it.
- The walk amount follows the horizontal speed up to the walk speed, so a body at rest stands straight. D-333 names the stride and the angles alone, and the play test judges that rule.
- The model root stands the lowest box corner of the pose on the feet, so the roll lifts the model about 0.55 m at its middle.
- The GitHub PR #62 is PR-15. The roadmap id PR-62 is the art quality pass.
- The branch came from `origin/main` and tracked it at first. The first push set the upstream to the feature branch, and a push names the branch.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 153.

### Open questions that block progress

None blocks PR #62. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #62 per the `pr-review` skill at the effective head `6e35bc5` and writes `docs/reviews/pr-62.md`. The owner plays the sword for exit test 7, then merges, and a docs PR records the merge (D-297).

## Session 151: 2026-09-13, Claude Code

Author: Claude Code
Session: record the merge of PR-61 as PR #60, in the same invocation as Session 149 (D-297). Branch `docs/pr-61-merge-record`.

### What this session did, and why

- Session 150 approved `1e3dba8` in `docs/reviews/pr-60.md` with no finding, and `review-gate` passed. The owner merged PR #60 as `4a1048c` at 07:46 UTC on 2026-09-13.
- `docs/design.md` marks PR-61 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-61 and the mark in sequence item 10.
- No decision and no question changed in this PR. D-317 entered the register with PR #60.
- Session 141 moved to the archive, because the file held eleven entries with this one. Session 150 moved Session 140 before this session.

### State of the build

- `main` is at `4a1048c`, the squash merge of PR #60. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-61-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- On `4a1048c`, CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed.
- `ste-check`: 0 findings in 15 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `4a1048c`.
- `dotnet test`: 778 tests, 0 failures, with the five Smoke tests on the local Godot build, on the final documents of this PR.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass (D-188, D-190). Then a fresh session opens PR-15 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- PR-15 builds the zero-weight case alone (D-316). Its exit test 7 needs the owner to play the sword, and PR-22 holds `WeightSlowsDodge` and `HeavyArmorResistsStagger`.
- No decision names the author of the first animation files of PR-15. The PR-15 scope lists them as work of the PR, in the format of D-298.
- A PR that adds a Game flag adds it to the table of `UserArguments`, and to the ignored flag rule when a session ignores it (D-313, D-317). `SessionCommandsParse` reads every Godot command of `CLAUDE.md`.
- Session 150 reports that the local build, the full test, and `det-lint` stopped with no output in its checkout. The same commands completed in Session 149, so remote CI and the author checks carry that evidence.
- The merge marks use the UTC date of the merge, 2026-09-13, and so does this entry.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 152.

### Open questions that block progress

None blocks this PR or PR-15. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-15 from `main` per the Phase 2 roadmap and D-314 to D-316.

## Session 150: 2026-09-13, Codex

Author: Codex
Session: review PR #60 at effective head `1e3dba8`.

### What this session did, and why

- Checked the provider gate. Session 149 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, affected callers and tests, roadmap, design, decisions, questions, review records, and all PR comments.
- Found no in-scope defect. The parser rejects unknown words, unknown flags, repeated flags, short flags, and ignored combinations with contextual errors.
- Added `docs/reviews/pr-60.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `98e3c47`. The effective head is `1e3dba8`. The later handoff commit is metadata under D-184.
- The focused tests passed, 37 tests with 0 failures. The local build, full test, and det-lint commands produced no output and did not complete. Revision-matched remote CI passed the required code, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar checks.
- The fresh `evaluate` and `review-gate` checks pass after the review record reached the PR. Duplicate post-metadata platform jobs remain pending, while their prior revision-matched checks pass.

### In flight

The review record and this handoff entry are pushed in `8d9477a`. The owner can merge after the duplicate platform jobs finish, if branch protection requires them.

### Traps and gotchas

- The effective head is `1e3dba8`, not the later metadata tip, under D-184.
- A new flag needs an entry in `UserArguments` and an ignored-flag rule when a session ignores it (D-313, D-317).
- The local dotnet commands can stop without output in this checkout. Remote CI provides separate evidence.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 151.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-99 remains open but blocks no work.

### Next concrete action

Commit and push this review and handoff. Verify the fresh review-gate result and the synchronized remote head.

## Session 149: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-61, the user argument check, as PR #60. Branch `feat/pr-61-user-argument-check`.

### What this session did, and why

- PR #59 merged as `98e3c47` at 02:21 UTC on 2026-09-13, and the Session 148 handoff named PR-61 as the next action. This session opened it from `main` per D-313.
- Before the code, the owner answered one scope question. A known flag that the session ignores passed in silence: `--contact-sheet sheet.png --smoke` rendered the sheet and ignored `--smoke`, and `--smoke --bot` never used the bot. D-317 puts that check in PR-61, with the recommendation.
- `WhatYouCarry.Game/UserArguments.cs` reads the user arguments once at boot. Its table holds each flag and its count of words. The parse stops on an unknown word, an unknown flag, a repeated flag, a short flag, and an ignored flag. Each error names the word or the flags.
- `Main` parses first in the boot, so a bad argument is a boot failure with exit code 1. `SmokeSession`, `BotSession`, `FrameLog`, `ContactSheet`, and `TestExit` read their flags through the parser. The trailing word check of `TestExit.PressOf` moved into the parser.
- `UserArgumentsTests.cs` holds exit tests 1 to 5 and 7, and `SmokeSessionTests.BadArgumentEndsTheSession` is exit test 6. The flag tests of four older test files read through the parser, and none passes the unknown flag `--other` now.
- `docs/decisions.md` gains D-317. `docs/design.md` and the Phase 2 roadmap name D-317 in the PR-61 entry, and the roadmap gains exit test 7. `CLAUDE.md` and `AGENTS.md` describe the check.
- The automated pass of gitar ran on `1e3dba8` after the push. Its check run passed, and it approved with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and no `Gitar review` comment was necessary.
- Session 139 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `98e3c47`. The effective head of PR #60 is `1e3dba8`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-61-user-argument-check` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 778 tests, 0 failures, with the five Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 28 files. `ste-check`: 0 findings in 15 files. The Godot build check passed.
- On `1e3dba8`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and gitar passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

PR #60: the Codex review per the `pr-review` skill at the effective head `1e3dba8`, then the owner merge. A docs PR then records the merge (D-297).

### Traps and gotchas

- Every user argument after `--` passes the parser now. An engine flag after the separator, such as `--windowed`, stops the boot, so it must stand before `--`.
- A word that starts with `--` is always a flag. A file path that starts with `--` needs a prefix, such as `./`.
- The contact sheet takes no other flag, and `--smoke` and `--bot` exclude each other (D-317). A PR that adds a flag adds it to the table of `UserArguments`, and to the ignored flag rule when a session ignores it.
- `SessionCommandsParse` reads the Godot commands of `CLAUDE.md`. A new command there with the separator must parse.
- A Godot command with no `--path` opens the project manager window and never quits. This session started one by mistake and stopped the process.
- The jobs of CI, bit identity, smoke, bots, and asset-qa on `1e3dba8` waited about 28 minutes for runners before they started. A long pending state there is the queue, and not a failure.
- `gh pr checks` exits with code 1 as soon as `evaluate` fails, while other checks still run. A wait loop must read the word `pending` in the output, and not the exit code.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 150.

### Open questions that block progress

None blocks PR #60. PR-15 has no open blocker, and its exit test 7 needs the owner to play the sword. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #60 per the `pr-review` skill at the effective head `1e3dba8` and writes `docs/reviews/pr-60.md`. The owner then merges, and a docs PR records the merge (D-297). PR-15 follows.

## Session 148: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-60 as PR #58 and four owner answers, D-313 to D-316 (D-297). Branch `docs/pr-60-merge-record`.

### What this session did, and why

- The owner asked the session to address the PR #58 re-review feedback. The PR had no open feedback at `9803a8b`: Session 146 fixed P2-2, and gitar approved the head. The trigger `--smoke --press escape 100 unexpected` ended at boot with exit code 1 and named `unexpected`. The review commit `e5a3846` raised P2-2, so the cross-provider repeat review of the fix was still necessary.
- Session 147 approved `9803a8b` in `e35d5e9`, and `review-gate` passed on that head. The owner merged PR #58 as `94f0897` at 00:16 UTC on 2026-09-13. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-60 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-60 and the mark in sequence item 8.
- The owner answered three questions, each with the recommendation. D-313 resolves OQ-170: one parser checks the whole user argument list, in PR-61 before PR-15. D-314 resolves OQ-5: heavy armor resists stagger, and light armor does not. D-315 resolves OQ-46: the initial combat numbers.
- PR-15 has no armor, so its exit test 1 and D-314 needed weight numbers that no decision held. The owner chose D-316: PR-15 builds the zero-weight case, and PR-22 sets the growth of the dodge cooldown with weight and the weight at which armor resists stagger, with the first armor sets.
- `docs/design.md` and the Phase 2 roadmap gain the PR-61 entry, and the Phase 2 sequence puts PR-61 at item 10. The later items move down by one. The PR-15 scope and exit test 1 follow D-316. The Phase 3 roadmap gives PR-22 the two weight numbers and exit test 7, `HeavyArmorResistsStagger`.
- Session 138 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `94f0897`, the squash merge of PR #58. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-60-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `94f0897`.
- `dotnet test`: 770 tests, 0 failures, with the four Smoke tests on the local Godot build. That run started before the last Phase 2 list fix and this entry. The run without the Smoke category passed on the final documents, 766 tests and 0 failures.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass (D-188, D-190). Then a fresh session opens PR-61 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- The Phase 2 sequence moved down by one after item 9. An older handoff that names item 10 or a later item means the item one number higher now.
- PR-61 moves the trailing word check of `TestExit.PressOf` into the shared parser. `SmokeSessionPasses`, `EscapeEndsTheSession`, and `StartButtonEndsTheSession` pass user arguments, so each must still pass.
- PR-15 builds the zero-weight case alone (D-316). Its exit test 1 has no weight clause, and PR-22 holds `WeightSlowsDodge` and `HeavyArmorResistsStagger`.
- The merge marks use the UTC date of the merge, so PR-60 reads 2026-09-13. The decisions and this entry use the local date 2026-09-12, as D-295 and Session 147 did.
- No decision names the author of the first animation files of PR-15. The PR-15 scope lists them as work of the PR, in the format of D-298.
- The next ids are D-317, OQ-171, F-96, PR-62, and Session 149.

### Open questions that block progress

None blocks this PR or PR-61. PR-15 has no open blocker, and its exit test 7 needs the owner to play the sword. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-61 from `main` per the Phase 2 roadmap and D-313. PR-15 follows it.
