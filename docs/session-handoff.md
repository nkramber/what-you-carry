# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 160: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-63 as PR #65 and the play test of the owner, D-354, in the same invocation as Session 158 (D-297). Branch `docs/pr-63-merge-record`.

### What this session did, and why

- Session 159 approved `6f0d6f2` in `docs/reviews/pr-65.md` with no finding. The owner merged PR #65 as `002054a` at 18:10 UTC on 2026-09-14.
- The macOS leg of Bit identity on the review tip `d66fd24` never started. The self-hosted runner did not acquire the job in 20 minutes, and the compare job skipped. Four pushes in 15 minutes queued about 12 macOS jobs on the one Mac runner. The same code passed that leg on `6f0d6f2`, `e1d58db`, `1e85f94`, and `7028406`.
- The owner asked for a concurrency group in the workflows, so that a newer push cancels the older runs of a PR. The change is code, so it takes its own PR and a Codex review. The owner chose to open it in this run as a one-time exception to D-121, after this PR merges, and that PR records the decisions.
- The owner played floor 1 and confirms that the spaces no longer feel cramped. D-354 closes exit test 6 of PR-63.
- `docs/design.md` marks PR-63 merged in its entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-63 and the mark in sequence item 13.
- Session 150 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `002054a`, the squash merge of PR #65, and its tree equals the review tip `d66fd24`. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-63-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `002054a`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 18:25 UTC.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 873 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `002054a`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the new sizes, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). The concurrency PR of this run opens from `main` after this PR merges, and it takes a Codex review. PR-67 follows in a fresh session (D-121, D-353).

### Traps and gotchas

- The GitHub PR #65 is PR-63. The roadmap id PR-65 is the ramp meshes in Game.
- About one floor in 96000 fails to dig on the new sizes (F-98). The floors of the first night on them dug with no error in the measurement of Session 158. A change that moves the floors of the night can fail a night before PR-67 lands (D-353).
- The merge marks use the UTC date of the merge, 2026-09-14. D-354 and this entry use the local date, also 2026-09-14.
- The next ids are D-355, OQ-173, F-99, PR-68, and Session 161.

### Open questions that block progress

None blocks this PR or the concurrency PR. PR-67 asks the owner for the job budget and the count of digs before the code (D-353). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. This session then opens the concurrency PR from `main` for a Codex review. A fresh session then opens PR-67.

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
