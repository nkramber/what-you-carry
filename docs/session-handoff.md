# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 129: 2026-09-11, Claude Code

Author: Claude Code
Session: record the merge of PR-12 as PR #49 and bring every document up to date, in the same run as Sessions 125 and 127. Branch `docs/pr-12-merge-record`.

### What this session did, and why

- The owner merged PR #49 as `9313358` at 20:41 UTC, with the verdict `Ready for owner merge` for `066ce0e`. OQ-157 and OQ-158 stayed open at the merge, so `main` holds the two sensitivity constants of the recommendation and the `actions/cache` step with no decision entry yet.
- `docs/design.md` marks PR-12 merged in the roadmap entry and in sequence item 10. The Phase 2 roadmap gains the status line of PR-12, the mark in sequence item 2, and the new state of OQ-157 and OQ-158 in section 6.
- `docs/questions.md` gains a dated addendum on OQ-157 and on OQ-158: the merge came with both open.
- `CLAUDE.md` and `AGENTS.md` gain the `smoke` job in the PR gate.
- Session 119 moved to the archive.

### State of the build

- `main` is at `9313358`, the squash merge of PR #49. On that commit the smoke workflow and bit identity passed. CI was in progress at the time of this entry, behind the smoke job on the macOS runner.
- Remote head: `origin/docs/pr-12-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. `dotnet test`: 584 tests, 0 failures. No code changed. `bit-identity`: `6ec00e90c1c85cdb`.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. The owner answers OQ-157 and OQ-158 in `docs/decisions.md` as the next two ids. If the OQ-157 answer differs from the recommendation, a PR changes `MouseHundredthsPerPixel` and `StickHundredthsPerTick` in `IntentBuilder.cs` and the tests that read them.

### Traps and gotchas

- `dotnet test` with no filter runs `SmokeSessionPasses`, which starts the Godot build at the path that `CLAUDE.md` names, or the one that `WYC_GODOT` names. The three CI jobs filter the Smoke category out, and the smoke workflow runs it.
- A `dotnet test --no-build` after a build of the Game project alone reads a stale copy of the Game assembly in the test output. Build the solution before a test of a Game change.
- The full local suite takes about three and a half minutes on this Mac. A run with a two-minute timeout reads as a stall.
- The Windows CI suite took 9 min 27 s on PR #49, near the ten-minute bound of the M-1 procedure (OQ-145).
- The engine reports the two shift keys as one key and the two control keys as one key. Block, throwable, reload, satchel, and amulet from D-289 have no button bit yet.
- The next ids are D-291, OQ-159, F-96, and Session 130.

### Open questions that block progress

OQ-43 and OQ-49 block PR-13. OQ-157 and OQ-158 are open on `main` and block nothing, and OQ-158 needs its decision entry under G-16. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. The owner answers OQ-157 and OQ-158, then OQ-43 and OQ-49. A session then opens PR-13 from `main` per the Phase 2 roadmap: the Blockbench loader and the greedy mesher.

## Session 128: 2026-09-11, Codex

Author: Codex
Session: re-review PR #49 at effective head `066ce0e`.

### What this session did, and why

- Verified the provider gate. The substantive PR work and the P2-1 correction came from Claude Code, so Codex is the eligible reviewer under T-4 and D-101.
- Read the prior review, the response, the complete correction diff, the roadmap, the affected input callers, the tests, and every PR comment.
- P2-1 has full merit at `91d1b6f`. The correction at `066ce0e` tracks the latest look event instead of the held stick state.
- The regression test fails on the old behavior and passes on the correction. No finding remains at the new effective head.
- Updated `docs/reviews/pr-49.md` with the earlier verdict and the current `Ready for owner merge` verdict.

### State of the build

- `main` and the merge base are `e1cf847`. The effective implementation head is `066ce0e`. Review and handoff metadata commits remain outside the effective diff.
- The focused reader suite passes 8 tests, and the focused smoke test passes. The author reports 584 full-suite tests with 0 failures, and remote Linux, Windows, and macOS CI pass.
- Local det-lint, STE check, bit identity, Godot editor build, and the smoke session pass. The local full build and full test commands did not complete in this execution context after more than 60 seconds without output.

### In flight

The repeat review record needs a commit and push. The review-gate and evaluate checks must refresh against the new `Ready for owner merge` verdict. OQ-158 still blocks the merge.

### Traps and gotchas

- The effective head is `066ce0e`, not a later metadata commit.
- A held stick keeps controller look state until a new mouse or stick event names a device.
- A full local test command may stall after the Godot project build. Use the focused tests and remote full-suite evidence.

### Open questions that block progress

OQ-158 blocks PR #49 under G-16. OQ-157 remains open and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open and blocks nothing.

### Next concrete action

Commit and push this repeat review and handoff. Then verify the remote head and the refreshed review-gate result.

## Session 127: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #49 review, P2-1 (D-243), in the same run as Session 125. Branch `feat/pr-12-game-skeleton`.

### What this session did, and why

- Read `docs/reviews/pr-49.md` at the reviewed head `91d1b6f`. P2-1 has full merit: `Read` set the controller flag from the held stick on each tick, so a later mouse event lost the look device.
- The look device now follows the latest look event (D-243). `Main` hands the mouse motion and the joypad motion events to the reader as plain values, `AddLookStickMotion` names the controller on a look axis event past the dead zone, and `Read` reads the deflection alone.
- The reader polls through `IInputPoll`: `EnginePoll` over the engine, and a test poll in `InputReaderTests`, the two callers of D-111. Eight reader tests pin the device transitions and every D-289 binding with a bit. The regression test fails on the old line, 1 failed and 7 passed, and passes on the correction.
- `docs/reviews/pr-49-response.md` records the disposition, the correction, and the regression check.
- Sessions 117 and 116 moved to the archive, because the file held eleven entries.

### State of the build

- `main` is at `e1cf847`. The effective head is the correction commit, the one commit above the review commits `42bd504`, `307f025`, and `735f7fd`.
- Remote head: `origin/feat/pr-12-game-skeleton` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 584 tests, 0 failures, with the smoke test on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 11 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. Core did not change.

### In flight

PR #49 needs a repeat Codex review of the correction, the owner answers to OQ-157 and OQ-158, and the owner merge. The automated pass runs again on the push.

### Traps and gotchas

- A `dotnet test --no-build` after a build of the Game project alone reads the stale copy of the Game assembly in the test output. Build the solution before a test of a Game change.
- A stick moved past the dead zone and released keeps the look with the controller until the mouse moves, because the release event is inside the dead zone.
- The effective head is the correction commit, not a later metadata commit.
- The next ids are D-291, OQ-159, F-96, and Session 128.

### Open questions that block progress

OQ-158 blocks the merge of this PR (G-16). OQ-157 binds the two constants and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass, then a Codex session reviews the correction per the repeat review procedure of the `pr-review` skill and sets the verdict for the new effective head. The owner answers OQ-157 and OQ-158, and merges.

## Session 126: 2026-09-11, Codex

Author: Codex
Session: review PR #49 at effective head `91d1b6f`.

### What this session did, and why

- Verified the provider gate. The substantive PR work came from Claude Code, so Codex is the eligible reviewer under T-4 and D-101.
- Read the handoff, the project rules, the design, the relevant decisions and questions, the Phase 2 roadmap, the complete PR diff, the affected Core callers, and every PR comment.
- Found P2-1. A held controller stick overrides a later mouse look event, so the input frame can set the controller aim bit for the wrong device.
- Added `docs/reviews/pr-49.md` with a `Changes required` verdict for the effective head.

### State of the build

- `main` and the merge base are `e1cf847`. The effective implementation head is `91d1b6f`. The current branch tip before this session is `dfdc68e`, which holds metadata only.
- The focused Game, input, render, smoke, and shape tests pass with 41 tests. The remote three-platform CI and smoke checks pass on the effective head. Gitar passes, while `evaluate` and `review-gate` fail for the recorded `Changes required` verdict.
- Local det-lint, STE check, bit identity, Godot editor build, and the headless smoke session pass. Local `dotnet build` did not complete after more than 80 seconds without output.

### In flight

PR #49 needs the P2-1 correction and a regression test. OQ-158 also needs an owner decision before merge. The review-gate check must refresh after the review record reaches the PR head.

### Traps and gotchas

- The effective head is `91d1b6f`, not the metadata tip.
- `InputReader.Read` checks the current stick after mouse input has set the flag false. A held stick can therefore override the last mouse event.
- The first `dotnet build` attempt produced no output for more than 80 seconds. Remote CI gives the build evidence for this head.

### Open questions that block progress

OQ-158 blocks PR #49 under G-16. OQ-157 remains open and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open and blocks nothing.

### Next concrete action

The author corrects the input-device state and adds the regression test. Then the author pushes the fix, and a later review checks the new effective head.

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
