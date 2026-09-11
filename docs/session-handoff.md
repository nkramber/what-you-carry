# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 125: 2026-09-11, Claude Code

Author: Claude Code
Session: PR-12, the Game skeleton and input (D-63, D-73, D-77, D-114, D-149, D-289). Branch `feat/pr-12-game-skeleton`.

### What this session did, and why

- PR #48 merged as `e1cf847` at 17:38 UTC, and Gate 1 is signed (D-288), so PR-12 opened from `main` per the Phase 2 roadmap.
- `WhatYouCarry.Game/Main.cs` is the root node, and `Main.tscn` is the one text scene (D-63). It steps one `SimulationLoop` per physics frame at 60 Hz, and `project.godot` pins the physics tick at 60. It draws the player box and the camera between the last two ticks with the interpolation fraction of the engine (D-73, D-245).
- `Input/IntentBuilder.cs` holds the pure logic: the linear mouse, the cubic stick curve with the 15 percent dead zone (D-289), a carry of the fraction between ticks, and the controller aim bit from the device of the look (D-243). `Input/InputReader.cs` reads the engine once per tick with the bindings of D-289 for the five actions that have a bit: jump, sprint, dodge, attack, and interact.
- `Smoke/SmokeSession.cs` is the script of one thousand ticks in four parts. `Main` runs it on `--smoke` and quits with exit code 0 only when the print sink counted no error line (D-114). The engine ends the session in under one second with `--fixed-fps 60`. A reserved bit at tick 500, set by hand one time, gave exit code 1 and an error line with the tick.
- `Content/DirectoryContentSource.cs` reads the content directory of the checkout, next to the project directory (D-219). `Logging/PrintLogSink.cs` prints each line and counts the error lines (D-211).
- `.github/workflows/smoke.yml` runs the one test of the Smoke category on the three platforms with the pinned Godot binary from `actions/cache`. `ci.yml` leaves that category out. The test project references the Game project for the pure logic.
- Filed OQ-157, the two sensitivity numbers, and OQ-158, the cache action as a dependency (G-16). The code holds the recommendation of OQ-157 as two named constants.
- PR #49 opened at the effective head `91d1b6f`. The automated pass approved the head with no code finding. Its one comment reads the red review-gate before a review record exists, which D-251 designs, and the reply on the PR names that. No commit answered it.
- Session 115 moved to the archive.

### State of the build

- `main` is at `e1cf847`. This branch holds the work commit `91d1b6f` above it, and the handoff commits above that (D-184).
- PR #49: CI green on the three platforms with 575 tests, the suite minus the smoke test. Bit identity, det-lint, ste-check, night-gate, bots, and smoke are green. The smoke workflow passed on its first run, with a cache miss and a download on each platform. The Windows suite took 9 min 27 s, near the ten-minute bound of the M-1 procedure (OQ-145).
- Remote head: `origin/feat/pr-12-game-skeleton` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 576 tests, 0 failures, with the smoke test on the local Godot build. The full suite takes about four minutes on this Mac.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 9 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- The Godot editor build check passes. It wrote `Main.cs.uid`, and the checkout keeps that file.

### In flight

This PR needs a Codex review, the owner answers to OQ-157 and OQ-158, and the owner merge. The automated pass is complete. The first run of the engine in CI passed on the three hosted and self-hosted runners.

### Traps and gotchas

- `dotnet test` with no filter runs `SmokeSessionPasses`, which starts the Godot build at the path that `CLAUDE.md` names, or the one that `WYC_GODOT` names. The three CI jobs filter the Smoke category out.
- The engine reports the two shift keys as one key and the two control keys as one key, so the right keys sprint and dodge too.
- Block, throwable, reload, satchel, and amulet from D-289 have no button bit yet. The PR that assigns each bit adds the binding.
- The namespace `WhatYouCarry.Game.Input` hides the engine class `Input`, so the reader writes `Godot.Input`. `Button` needs the alias `CoreButton` next to the engine type of that name.
- The Windows smoke job names the console executable of Godot, because the window executable writes nothing to standard output.
- The next ids are D-291, OQ-159, F-96, and Session 126.

### Open questions that block progress

OQ-158 blocks the merge of this PR (G-16). OQ-157 binds the two constants and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #49 at the effective head `91d1b6f` per the `pr-review` skill: the Core boundary, the determinism of the builder, the input and CI boundaries, and the presentation. The owner answers OQ-157 and OQ-158 in `docs/decisions.md`, and a commit sets the two constants if the answer differs from the recommendation. After the merge, the owner answers OQ-43 and OQ-49, and a session opens PR-13.

## Session 124: 2026-09-11, Codex

Author: Codex
Session: re-review PR #48 at effective head `4786cfa`. Branch `chore/night-back-to-0807`.

### What this session did, and why

- Verified that the effective head remains `4786cfa`. The later commits change only review and handoff metadata under D-184.
- Read the prior review, the complete implementation diff, all PR comments, D-286, D-288, D-290, F-94, F-95, the Phase 1 roadmap, and the changed workflow and shape test.
- No finding remains. The required Linux, Windows, and macOS CI jobs, three-platform bit identity, bots, det-lint, STE check, night-gate, and Gitar pass.
- Updated `docs/reviews/pr-48.md` with the earlier `Blocked` verdict and the current `Ready for owner merge` verdict.

### State of the build

- `main` and the merge base are `b700296`. The effective implementation head is `4786cfa`. The current metadata tip is `ccabf74` before this re-review commit.
- The prior local build and focused suite passed. The local full test run did not complete after 120 seconds with no output. Remote platform CI passed the full suite.
- Evaluate and review-gate failed only because the prior review record held `Blocked`. They must refresh after this re-review record reaches the PR head.

### In flight

The re-review record and this handoff entry need a commit and push. The fresh review-gate result must pass against `Ready for owner merge`.

### Traps and gotchas

- The effective head is `4786cfa`, not the metadata tip.
- The prior blocked result was correct while Windows CI was pending. The current verdict can approve only after all required platform checks pass.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the re-review record and handoff entry. Fetch the remote. Confirm that the remote has no ahead count and that the fresh review-gate result passes.

## Session 123: 2026-09-11, Codex

Author: Codex
Session: review PR #48 at effective head `4786cfa`. Branch `chore/night-back-to-0807`.

### What this session did, and why

- Verified that the substantive PR change came from Claude Code in Session 122. Codex is the eligible reviewer under T-4 and D-101.
- Verified the base, merge base, effective head, complete diff, D-286, D-288, D-290, F-94, F-95, the Phase 1 roadmap, the workflow, the shape test, the handoff files, and every PR comment.
- Found no defect. The workflow returns to `7 8 * * *`, keeps `workflow_dispatch`, and records the late scheduled run. The shape test asserts the cron, the hand trigger, the return phrase, and both finding ids.
- The local focused suite, build, det-lint, STE check, and bit identity pass. The local full suite did not complete after 120 seconds with no output. Remote Linux and macOS CI pass, and Windows CI remains pending.
- Added `docs/reviews/pr-48.md` with a `Blocked` verdict until the pending required check and the fresh review-gate result complete.

### State of the build

- `main` and the merge base are `b700296`. The effective implementation head is `4786cfa`. The handoff commit `80eca7a` and the review commit are metadata under D-184.
- The remote branch matched the local branch at `80eca7a` before this review record was added.
- The review record names the pending Windows CI job and the expected pre-review evaluate and review-gate results.

### In flight

The review record and this handoff entry need a commit and push. The Windows CI job must pass. The review-gate job must refresh against this record.

### Traps and gotchas

- The effective head is `4786cfa`, not the metadata tip.
- The review remains blocked until the required Windows CI result passes and the fresh review-gate result reads this record.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the review record and handoff entry. Fetch the remote. Confirm that the remote has no ahead count and that the fresh review-gate result passes.

## Session 122: 2026-09-11, Claude Code

Author: Claude Code
Session: return the night to 08:07 UTC (D-286, D-288, D-290), in the same run as Sessions 116, 118, 120, and 121. Branch `chore/night-back-to-0807`.

### What this session did, and why

- The owner merged PR #47 as `b700296` at 17:02 UTC. Gate 1 is signed (D-288), and Phase 2 can start.
- The cron of `night.yml` reads `7 8 * * *` again, which is 02:07 Central Standard Time. The comment names D-288 and corrects the text of PR #46: the 08:07 UTC run of 2026-09-11 started 4 h 40 min late, at 12:47 UTC (F-95).
- `NightWorkflowRunsAtTwoCentralStandardTime` replaces `NightWorkflowRunsAtTheScheduleTestTime`. It asserts the line, the hand trigger, the phrase `08:07 UTC, which is 02:07 Central Standard Time`, and both finding ids. It fails on the `21 17 * * *` workflow of `main`.
- No new decision: D-286 bound the return to the first scheduled pass, D-288 holds the words of the owner, and D-290 sends the return through this PR with a Codex review.
- Session 112 moved to the archive.

### State of the build

- `main` is at `b700296`, the squash merge of PR #47. The cron on `main` reads `21 17 * * *` until this PR merges. This branch holds the work commit above it, and this entry above that.
- Remote head: `origin/chore/night-back-to-0807` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR needs the automated pass, a Codex review, and the owner merge before 08:07 UTC on 2026-09-12. The 17:21 UTC scheduled run of 2026-09-11 stays (D-290). It can start hours late, and it holds the Mac runner for about an hour.

### Traps and gotchas

- A merge after 08:07 UTC on 2026-09-12 leaves the night of that day at 17:21 UTC.
- A schedule run here can start hours after its cron. Do not read a miss from one hour of silence (F-95).
- D-286 turned the workflow off and on for the test slot alone. This return has no such step.
- The next ids are D-291, OQ-157, F-96, and Session 123.

### Open questions that block progress

None blocks PR-12. OQ-43 and OQ-49 block PR-13. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, and the shape test. The owner merges it before 08:07 UTC on 2026-09-12. After that, a session opens PR-12 from `main` per the Phase 2 roadmap and D-289.

## Session 121: 2026-09-11, Claude Code

Author: Claude Code
Session: record the Gate 1 sign-off, the M-2 table, and the OQ-47 bindings (D-288, D-289, D-290), in the same run as Sessions 116, 118, and 120. Branch `docs/gate-1-record`.

### What this session did, and why

- The owner merged PR #46 as `b1399fc` at 16:43 UTC. The reset turned Night off and on at 16:44 UTC, and the workflow record read active with a new `updated_at`.
- A watch then found the first scheduled night: run 34600758086, the 08:07 UTC cron on `095ce5e`, created at 12:47:32 UTC, 4 h 40 min after the cron. It passed in 58 minutes, and the record on `night-results` reads success at 13:45 UTC. The 09:00 UTC deadline of Session 116 read that late run as a miss, and no check ran again until 16:44 UTC.
- The owner signed Gate 1 (D-288) and asked to move all scheduled runs back to 08:07 UTC. The M-2 table holds seven rows and reads complete. F-94, F-95, and OQ-155 gain dated refutations.
- D-289 resolves OQ-47: the recommendation, with sprint on Left Shift and dodge on Left Control. The owner first wrote "slide" for Control and corrected it to dodge, so D-27 stands.
- D-290 resolves OQ-156: this run opens three PRs, PR #46, this record, and the return to 08:07 UTC with a Codex review. The 17:21 UTC run of today stays.
- Session 111 moved to the archive.

### State of the build

- `main` is at `b1399fc`, the squash merge of PR #46. The cron on `main` reads `21 17 * * *`. This branch holds one docs commit above it.
- Remote head: `origin/docs/gate-1-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed. `bit-identity`: `6ec00e90c1c85cdb`.
- The record on `night-results` is the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. The night gate turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it.

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. Then the return PR: the cron back to `7 8 * * *`, the shape test, and the workflow comment, from `main` after this PR merges, with a Codex review, merged before 08:07 UTC on 2026-09-12. The 17:21 UTC scheduled run of today stays, and it can start hours late.

### Traps and gotchas

- A schedule run here can start hours after its cron. Check `gh run list --workflow=night.yml --event schedule` again for several hours before a miss, and filter a watch by `createdAt`.
- The 17:21 UTC run holds the Mac runner for about an hour when it starts, and macOS PR jobs wait behind it.
- The return PR must merge before 08:07 UTC on 2026-09-12, or the night of that day runs at 17:21 UTC again.
- The next ids are D-291, OQ-157, F-96, and Session 122.

### Open questions that block progress

None blocks PR-12 once this PR merges. OQ-43 and OQ-49 block PR-13, the item after it. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. This run then opens the return PR from `main`: `7 8 * * *` in `night.yml`, the shape test back on the 08:07 UTC line, the workflow comment, F-95 at ✅, and a handoff entry, with a Codex review. After both merge, a session opens PR-12 from `main` per the Phase 2 roadmap and D-289.

## Session 120: 2026-09-11, Claude Code

Author: Claude Code
Session: move the PR #46 test slot to 17:21 UTC with no repeat review (D-286, D-287), in the same run as Sessions 116 and 118. Branch `chore/night-schedule-reset`.

### What this session did, and why

- The 15:21 UTC slot passed before a merge. The owner moved the test to 17:21 UTC, which is 12:21 Central Daylight Time, and waived the Codex repeat review of the swap.
- The swap commit changes the cron to `21 17 * * *`, the matching strings of `NightWorkflowRunsAtTheScheduleTestTime`, and the times in D-286, OQ-154, OQ-155, F-95, and the Phase 1 roadmap. The shape test fails on the 15:21 UTC workflow.
- The `review-override` label cannot turn the gate green here, because the gate fails the label on a PR that changes a workflow or a test (D-190). D-287 records the owner waiver: the author sets the head field of `docs/reviews/pr-46.md` to the swap commit, with a dated note, one time, as D-282 did for PR #39.
- Session 110 moved to the archive.

### State of the build

- `main` is at `095ce5e`. The effective head is the swap commit. The commit above it holds the head correction, this entry, and the archive move, all metadata (D-184).
- Remote head: `origin/chore/night-schedule-reset` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. The repository shape suite: 14 tests, 0 failures. `ste-check`: 0 findings in 15 files. The full suite ran 535 tests with 0 failures on `862fd4c`, and CI runs it on this head.

### In flight

This PR waits for the checks and the automated pass on the new head, then the owner merge before 17:21 UTC. After the merge, `gh workflow disable night.yml` and then `gh workflow enable night.yml` run, and a watch reads the 17:21 UTC night.

### Traps and gotchas

- A merge or an enable after 17:21 UTC moves the first test to 17:21 UTC on 2026-09-12 (D-286).
- If no run of the schedule event exists by 18:15 UTC, that is a third miss. Stop and ask the owner.
- The handoff entries of Sessions 116 to 119 and the review record name 15:21 UTC. They are dated records, and D-286 names the move.
- The next ids are D-288, OQ-156, F-96, and Session 121. The Gate 1 sign-off is D-288 now.

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #46 before 17:21 UTC. Then the disable and the enable run, and a watch reads the 17:21 UTC night. When it passes, the Gate 1 record follows the edit list of Session 115, with D-288 for the sign-off, the row of the 17:21 UTC night, and F-95 at ✅. A PR returns the cron to 08:07 UTC.

## Session 119: 2026-09-11, Codex

Author: Codex
Session: repeat review PR #46 at effective head `eebb605`. Branch `chore/night-schedule-reset`.

### What this session did, and why

- Read `docs/reviews/pr-46-response.md` and checked the provider gate again. The substantive correction is from Claude Code, so Codex remains eligible.
- Verified the new effective head `eebb605`, the diff since `862fd4c`, the correction trigger, the handoff archive rotation, the workflow, and every PR comment.
- P2-1 is fixed. The shape test now asserts `08:07 UTC, which is 02:07 Central Standard Time`. The prior `09:07 UTC` trigger and a removed return phrase fail the test, and the branch suite passes.
- The automated pass approved the correction and reported the handoff rotation issue. Session 118 moved Sessions 108 and 107 to the archive, and the review verified ten current handoff entries.
- Updated `docs/reviews/pr-46.md` with the fixed finding and the verdict `Ready for owner merge` for `eebb605`.

### State of the build

- `main` and the merge base are `095ce5e`. The effective implementation head is `eebb605`. The review metadata tip is this commit after publication.
- `dotnet build`: 0 warnings, 0 errors. The repository-shape suite passes 14 tests, 0 failures, and 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. The Godot 4.7.2 headless build passes.
- Remote CI, bots, bit identity on all platforms, night-gate, STE check, det-lint, and Gitar pass at `eebb605`. The evaluate and review-gate checks failed because the prior review record still held `862fd4c` and `Changes required`. They must refresh after this review record is pushed.

### In flight

The repeat-review record and this handoff entry need a commit and push. After the fresh review-gate result passes, the owner can merge before the schedule-test deadline or wait for the next slot.

### Traps and gotchas

- The effective head is `eebb605`, not this metadata tip. It is the test correction commit under D-184.
- The review-gate result that reads the old record is stale by design. Do not treat it as a product failure.
- OQ-154 still blocks Phase 2 until the first scheduled night passes and Gate 1 is signed.

### Open questions that block progress

OQ-154 blocks Phase 2. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the updated review record and this entry. Then verify the fresh review-gate result and the remote head.

## Session 118: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #46 review, in the same run as Session 116. Branch `chore/night-schedule-reset`.

### What this session did, and why

- Read P2-1 in `docs/reviews/pr-46.md`. Full merit: the shape test asserted `02:07 Central Standard Time` and not the 08:07 UTC return time, while the PR description and Session 116 said that it did. A workflow comment with the return time changed to 09:07 UTC passed the test.
- The review asked for an assertion on `08:07 UTC`. That text alone passes on the same trigger, because the F-95 sentence of the comment holds `08:07 UTC` too. The test asserts `08:07 UTC, which is 02:07 Central Standard Time` instead. The trigger fails it, and so does a comment with the return time removed.
- `docs/reviews/pr-46-response.md` records the disposition. No new id.
- The automated pass on the review commit left one comment, with merit: Session 117 entered the handoff without an archive move, so the file held 11 entries (D-146). Sessions 108 and 107 moved to the archive with this entry, and the file holds the 10 newest entries. The reply on the thread names this commit.
- This run wrote Session 116 before the review. Session 117 came above it while the run continued, so this entry is a new one at the top, and Session 116 stays as the review read it.

### State of the build

- `main` is at `095ce5e`. This branch holds the work commit `862fd4c`, the Session 116 entry `d8ee1dd`, the review commit `c76d54a`, and the correction that holds this entry above them.
- Remote head: `origin/chore/night-schedule-reset` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. The repository shape suite: 14 tests, 0 failures. `ste-check`: 0 findings in 15 files. The full suite ran 535 tests with 0 failures on `862fd4c`, and CI runs it on this head.
- `det-lint`: 0 findings. `bit-identity`: `6ec00e90c1c85cdb`. Core did not change.

### In flight

This PR waits for the automated pass on the correction, then a Codex repeat review at the effective head, which is the correction commit. The owner merge must land before 15:21 UTC for the test to run today, and the disable and the enable follow the merge.

### Traps and gotchas

- The effective head is the correction commit, because it changes a test. The review record names `862fd4c`, and the repeat review updates the head and the verdict together (D-269).
- A merge or an enable after 15:21 UTC moves the first test to 15:21 UTC on 2026-09-12 (D-286).
- The traps of Session 116 stand: the third-miss deadline at 16:15 UTC, the two PRs after the pass, and the next ids D-287, OQ-156, and F-96. The next session number is 119.

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure at the effective head, verifies P2-1 against its trigger and the regression case, and updates `docs/reviews/pr-46.md` with the status of P2-1 and a new verdict.

## Session 117: 2026-09-11, Codex

Author: Codex
Session: review PR #46 at effective head `862fd4c`. Branch `chore/night-schedule-reset`.

### What this session did, and why

- Verified the provider gate. Session 116 identifies Claude Code as the author of the substantive PR-46 change. Codex is the eligible reviewer.
- Verified the base, merge base, effective head, complete diff, D-285, D-286, F-94, F-95, the Phase 1 roadmap, the workflow, the shape test, the registers, the handoff files, and every PR comment.
- The cron reads `21 15 * * *`, and the workflow comment records the temporary test and the return to 08:07 UTC.
- Found P2-1: the shape test checks `02:07 Central Standard Time` but does not assert the required `08:07 UTC` return text. The focused suite passes 14 tests, but the missing assertion leaves the return-time contract unguarded.
- Wrote `docs/reviews/pr-46.md` with the verdict `Changes required` for `862fd4c`.

### State of the build

- `main` and the merge base are `095ce5e`. The effective implementation head is `862fd4c`. The current metadata tip is `d8ee1dd` before this review commit.
- `dotnet build`: 0 warnings, 0 errors. The focused shape suite passes 14 tests, 0 failures, and 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. The Godot 4.7.2 headless build passes.
- The local full test run did not complete after about 90 seconds with no output. Remote Linux and Windows CI, macOS bit identity, and several other checks were still pending when observed. The review-gate failure is the expected missing-record state until this review is pushed.

### In flight

The review record and this handoff entry need a commit and push. The author must add the direct `08:07 UTC` assertion, rerun the focused suite, and request a repeat review at the new effective head.

### Traps and gotchas

- The effective head is `862fd4c`, not the metadata tip. The first commit changes the workflow, test, and registers. The second commit changes only metadata under D-184.
- `git fetch origin` could not update `.git/FETCH_HEAD` because the execution context denied access. The local branch matched `origin/chore/night-schedule-reset` at `d8ee1dd` before this review commit.
- The owner must merge only after the P2-1 correction, a repeat review, the fresh review-gate result, and all required platform checks pass.

### Open questions that block progress

OQ-154 blocks Phase 2 until the first scheduled night passes. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the review record and this entry. Then wait for the author correction and perform the repeat review.

## Session 116: 2026-09-11, Claude Code

Author: Claude Code
Session: listen for the first scheduled night, then move the night to one schedule test at 15:21 UTC (D-286). Branch `chore/night-schedule-reset`.

### What this session did, and why

- A background watch read the 08:07 UTC night of 2026-09-11. No run of the schedule event existed at 09:00 UTC. The runner was online and idle from 07:15 UTC, the `7 8 * * *` line stood on `main` from 06:27 UTC, the workflow read active, and Actions was on. F-95 records it, and F-94 gains a dated note: the minute was not the whole cause.
- Asked the owner, and D-286 records the answer (OQ-155). The cron moves to one test slot, and after the merge `gh workflow disable` and `gh workflow enable` turn the workflow off and on. Gate 1 signs after the first scheduled night passes, and D-283 stands. After that pass, a PR returns the cron to 08:07 UTC.
- The owner first named 11:21 UTC. At 11:16 UTC that slot was out of reach before a merge, and the owner moved the test to 15:21 UTC, which is 10:21 Central Daylight Time.
- The cron reads `21 15 * * *`. `NightWorkflowRunsAtTheScheduleTestTime` replaces `NightWorkflowRunsAtTwoCentralStandardTime`. It asserts the line, the comment, both finding ids, and the 08:07 UTC return time, and it fails on the workflow of `main` at the cron line.
- D-285 carries a `Revised in part by D-286` marker, and OQ-154 gains a dated note. Session 106 moved to the archive.

### State of the build

- `main` is at `095ce5e`, the squash merge of PR #45. This branch holds the work commit above it, and this entry above that.
- Remote head: `origin/chore/night-schedule-reset` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- The record on `night-results` is still the success of hand run 6 at `5455e5d`, ended 02:21 UTC on 2026-09-11. The night gate turns red on every PR at 02:21 UTC on 2026-09-13 unless a night refreshes it.

### In flight

This PR. It needs the automated pass, a Codex review, and the owner merge before 15:21 UTC on 2026-09-11. After the merge, `gh workflow disable night.yml` and then `gh workflow enable night.yml` run, with a read of the workflow state after each. A watch then reads the 15:21 UTC night with the three commands of Session 115.

### Traps and gotchas

- A merge or an enable after 15:21 UTC moves the first test to 15:21 UTC on 2026-09-12 (D-286). The night gate stays green until 02:21 UTC on 2026-09-13.
- The night holds the Mac runner from 15:21 UTC for about 65 minutes. A macOS PR job waits in that window.
- If no run of the schedule event exists by 16:15 UTC, that is a third miss. Stop and ask the owner. The `launchd` timer on the Mac Mini was an option in OQ-155.
- After the pass, two PRs follow: the Gate 1 record, docs alone with the `review-override` label, and the return of the cron to 08:07 UTC, code with a Codex review. Ask the owner the order then.
- The ids moved: the Gate 1 sign-off is D-287 now, and the answer to OQ-47 is the decision after it. The next ids are D-287, OQ-156, F-96, and Session 117.
- D-284 and D-278 carry no marker for the partial revisions of D-285 and D-283. This PR adds the marker for D-286 alone.

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12, and the register holds a full recommendation. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, the shape test, and the registers. The owner merges it before 15:21 UTC. Then the disable and the enable run, and a watch reads the 15:21 UTC night. When it passes, the Gate 1 record follows the edit list of Session 115, with D-287 for the sign-off, the row of the 15:21 UTC night, and F-95 at ✅.
