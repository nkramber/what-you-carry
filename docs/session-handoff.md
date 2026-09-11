# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 115: 2026-09-11, Claude Code

Author: Claude Code
Session: put the state of the nights in the documents before a fresh session, which listens for the first scheduled night. Branch `docs/night-state-for-gate-1`.

### What this session did, and why

- The owner merged PR #43 as `557568a` and PR #44 as `57bc164`. The night keeps the logs of a failed run (D-280), and the cron reads `7 8 * * *`, which is 08:07 UTC and 02:07 Central Standard Time (D-284, D-285).
- The M-2 table holds the rows of hand runs 3 to 6: 59, 65, 63, and 68 minutes, all green. Only the row of the first scheduled night is missing.
- OQ-154 is open: the Gate 1 sign-off, after the first scheduled night passes on its own (D-283). The next session resolves it with the owner and records the sign-off as the next decision.
- No scheduled night has ever fired. The 03:00 UTC run of 2026-09-11 never came (F-94), and the 08:07 UTC line has not had its first chance yet.
- Session 105 moved to the archive.

### State of the build

- `main` is at `57bc164`, the squash merge of PR #44. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/night-state-for-gate-1` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The record on `night-results` is the success of hand run 6 at `5455e5d`, ended 02:21 UTC on 2026-09-11. The night gate turns red on every PR at 02:21 UTC on 2026-09-13 unless a night refreshes it.
- No PR is open.

### In flight

The first scheduled night, at 08:07 UTC on 2026-09-11. It takes about 65 minutes. Read it with these commands:

- `gh run list --workflow=night.yml --event schedule --limit 1 --json databaseId,status,conclusion,createdAt`: an empty list means the schedule has not fired.
- `gh api repos/nkramber/what-you-carry/actions/runs/<id> --jq '.run_started_at, .updated_at'`: the minutes of the row come from these two times, because the jobs API gives null times for the self-hosted job.
- `git fetch origin night-results && git show FETCH_HEAD:night.json`: the record, with the commit, the end time, and the status.

### Traps and gotchas

- GitHub delays or drops a schedule at the start of an hour under load, and this repository has never seen a schedule run. If no run exists by 09:00 UTC, that is a new question for the owner, and not a repeat of F-94: name what the runner and the workflow page show.
- The Mac runner takes a queued PR job only when no night is queued. A PR opened while the night runs waits about an hour for its macOS jobs.
- The record of `night-record` on `main` writes no byte-order mark since PR-58, and the gate reads the older records with one through git.
- The next ids are D-286, OQ-155, F-95, and Session 116. The scratchpad of the last session is gone: the M-2 rows above are the only copy of the hand run times, and the Gate 1 edit list below is the only copy of the plan.
- A docs-only PR takes the `review-override` label after its last push and the automated pass, and the review gate reads green then (D-188, D-190).

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12, and the register holds a full recommendation. OQ-99 is open, and it blocks nothing.

### Next concrete action

When the scheduled night passes, ask the owner two things in one batch: the Gate 1 sign-off, and the answer to OQ-47. Then one docs PR, the Gate 1 record, with these edits:

1. The M-2 table: the row `Scheduled night 1, 2026-09-11`, the run id, the commit, the minutes, and the status. Below it: `Status: table complete 2026-09-11, seven nights (D-283).` The design doc M-2 entry reads ✅ with the same words.
2. `docs/decisions.md`: D-286, the Gate 1 sign-off in the words of the owner. Resolves OQ-154. Applies D-150 and D-283. The next decision holds the answer to OQ-47.
3. The Phase 1 roadmap: the status header reads `focused roadmap, complete`, sequence item 22 reads ✅ for M-2, item 23 reads ✅ signed with the D-# id, the correction note gains a dated sentence, and the open questions list moves OQ-154 to a resolved line.
4. The design doc: sequence item 8 reads ✅ for M-2, item 9 reads ✅ signed with the D-# id, and the Phase 1 heading gains the sign-off.
5. Session 116 handoff entry.

After the merge, Phase 2 starts: a session opens PR-12 from `main` per the Phase 2 roadmap entry and the answer to OQ-47.

## Session 114: 2026-09-11, Codex

Author: Codex
Session: review PR #44 at effective head `0bfafbd`. Branch `chore/night-minute`.

### What this session did, and why

- Verified the provider gate. Session 113 identifies Claude Code as the author of the substantive PR-44 change. Codex is the eligible reviewer.
- Verified the base, merge base, effective head, complete diff, D-284, D-285, F-94, the Phase 1 roadmap, and every PR comment.
- The workflow runs at `7 8 * * *`, the shape test checks the line and F-94, and the changed registers and handoff agree with the decision.
- The focused repository-shape suite passes 14 tests. The review found no defect and wrote `docs/reviews/pr-44.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `557568a`. The effective implementation head is `0bfafbd`. The metadata tip is `838c544`.
- Local det-lint, STE check, bit identity, and the Godot headless build pass. The focused suite passes 14 tests with 0 failures and 0 skips.
- The local full build and full test run did not complete in the execution context. Remote platform builds and tests, bit identity, bots, det-lint, STE check, night-gate, and Gitar pass on the PR tip.

### In flight

The review record and this handoff entry are pushed. The fresh review-gate result must pass at effective head `0bfafbd` before the owner merges.

### Traps and gotchas

- The effective head is `0bfafbd`, not the handoff-only tip `838c544` (D-184).
- The current review-gate failure is the expected missing-record state. It is not a product failure. The gate must rerun after `docs/reviews/pr-44.md` reaches the PR.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the fresh checks. Verify that the remote has no ahead count, and confirm that the fresh review-gate result passes.

## Session 113: 2026-09-11, Claude Code

Author: Claude Code
Session: the night minute, off the start of the hour (D-285). Branch `chore/night-minute`.

### What this session did, and why

- The 03:00 UTC scheduled night of 2026-09-11 never fired. No run of the schedule event exists, the runner was free from 02:45 UTC, the old cron line stood on `main` until PR #42 merged at 03:54 UTC, and the workflow reads active on GitHub. F-94 records it. GitHub documents a delay or a drop at the start of an hour under load.
- Asked the owner, and D-285 records the answer: the night runs at 08:07 UTC, which is 02:07 Central Standard Time, in a PR of its own. OQ-153 holds the question. D-284 is revised in part, the minute only.
- The cron reads `7 8 * * *`, the comment names the minute and F-94, and `NightWorkflowRunsAtTwoCentralStandardTime` asserts the new line in place, so this PR adds no test at the anchor that PR #43 also touches.
- PR #43, the night logs of D-280, is open from Session 109 with a Codex review pending. This PR is the second open PR, because the owner wants the minute on `main` before 08:00 UTC.
- Session 103 moved to the archive at the rebase onto the PR #43 merge. This entry was Session 110 on the branch, and the rebase renumbered it to 113, because `main` holds a Session 110 from the review of PR #43 and D-187 allows one heading per number.

### State of the build

- `main` is at `557568a`, the squash merge of PR #43. This branch holds the workflow commit above it, and this entry above that.
- Remote head: `origin/chore/night-minute` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 533 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR, rebased onto the PR #43 merge. A watch reads the next scheduled run, at 08:00 UTC if this PR merges after that hour and at 08:07 UTC otherwise. After it passes, a session adds the five rows to the M-2 table and the owner signs Gate 1 (D-283).

### Traps and gotchas

- No scheduled run has ever fired for this repository. The first one is the proof D-283 asks for, and a second miss at 08:07 UTC is a new question, not a repeat of F-94.
- Two PRs touched `night.yml` in different hunks, and both added a handoff entry. This one merged second and needed a rebase for the handoff alone, with a renumber.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, and the shape test. The owner merges it when the review allows.

## Session 112: 2026-09-11, Codex

Author: Codex
Session: repeat review PR #43 at effective head `e63c35b`. Branch `chore/night-logs-artifact`.

### What this session did, and why

- Read `docs/reviews/pr-43-response.md` and checked the provider gate again.
- Verified the new effective head `e63c35b` and the diff since `d89338c`. The workflow did not change. The test now checks the upload step after the walker, descender, and reachability sweep.
- Reproduced the original trigger with the new mutation test. The focused repository-shape suite passes 14 tests, with no failures or skips.
- Updated `docs/reviews/pr-43.md`. P2-1 is fixed in `e63c35b`, and the verdict is `Ready for owner merge` after the fresh review gate passes.

### State of the build

- `main` and the merge base are `f19fe2e`. The effective implementation head is `e63c35b`.
- Local det-lint, STE check, bit identity, focused tests, and Godot build pass. The local full suite did not complete after 30 seconds with no output. The author reports 535 tests, 0 failures, and remote platform jobs pass.
- The evaluate and review-gate jobs still read the prior review verdict. They must run again after this review record is pushed.

### In flight

The repeat-review record and this handoff entry need a push. Then the fresh review-gate result must pass before merge.

### Traps and gotchas

- The workflow commit remains `8ea990f`. The new effective head is the test correction `e63c35b`, not a metadata tip.
- The review gate failed before this update because the record still held the prior verdict. That is a stale metadata result, not a product failure.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Push the repeat-review record and handoff. Refresh the review-gate result and confirm the owner can merge when all required checks pass.

## Session 111: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #43 review. Branch `chore/night-logs-artifact`.

### What this session did, and why

- Read the one P2 finding in `docs/reviews/pr-43.md`. Full merit: the shape test compared the upload step with the greedy-descender step alone, so a step moved to a place before the sweep passed the test while a failed sweep ran with no upload.
- `UploadStepDefect` reads a workflow text and names the first defect: no action, a wrong condition, a wrong path, a wrong missing-files rule, or a place before any of the three bot steps. `NightWorkflowUploadStepMustFollowEveryBotStep` moves the step before the sweep and before the walker in the real workflow text with `MoveStepBefore`, and asserts the name of each defect. The workflow did not change.
- `docs/reviews/pr-43-response.md` records the disposition. No new id.
- Session 101 moved to the archive.
- PR #44, the night minute of D-285, is open beside this PR with its own Session 110 entry. The review entry of this branch is Session 110 too, so the second PR to merge renumbers at its rebase, and the entry that renumbers says so (D-187).

### State of the build

- `main` is at `f19fe2e`, the squash merge of PR #42. This branch holds the workflow commit `8ea990f`, the Session 109 entry, the slice fix `d89338c`, the three review commits, and the correction that holds this entry above them.
- Remote head: `origin/chore/night-logs-artifact` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- No scheduled night has fired yet. The next chance is 08:00 UTC on the line of `main`, or 08:07 UTC once PR #44 merges.

### In flight

PR #43 waits for the automated pass on the correction, then a Codex repeat review at the effective head, which is the correction commit. PR #44 waits for its Codex review. The night watch reads the first scheduled run.

### Traps and gotchas

- The effective head is the correction commit, because it changes a test. The review record still names `d89338c`, and the repeat review updates the head and the verdict together, with one verdict name in the Verdict section (D-269).
- Two open branches each carry a Session 110 heading, one a review entry and one an author entry. The session-number check of the STE checker fails `main` on two headings with one number, so the rebase of the second PR renumbers before the push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure at the effective head, verifies P2-1 against its trigger and the regression case, and updates `docs/reviews/pr-43.md` with the status of P2-1 and a new verdict.

## Session 110: 2026-09-11, Codex

Author: Codex
Session: review PR #43 at effective head `d89338c`. Branch `chore/night-logs-artifact`.

### What this session did, and why

- Verified the base, merge base, effective head, provider gate, complete diff, D-280, the PR-11 night contract, the workflow, the shape test, the handoff files, and every PR comment.
- Gitar's step-slice comment is answered at `d89338c`. The slice now ends at the next step name.
- Found P2-1. The shape test checks that the upload follows the greedy descender, but it does not check that it follows the reachability sweep.
- Added `docs/reviews/pr-43.md`. The verdict is `Changes required` for `d89338c`.

### State of the build

- `main` and the merge base are `f19fe2e`. The effective implementation head is `d89338c`.
- Remote Linux, Windows, macOS, bots, det-lint, STE check, night-gate, and Gitar pass. The evaluate and review-gate results were unavailable because the review file did not exist before this session.
- Local det-lint, STE check, bit identity, and Godot build pass. The local build did not complete, and the local test runner stopped on a VSTest socket permission error. These are execution-context results.

### In flight

PR #43 needs the shape-test order correction, a regression check, a fresh push, and a repeat review.

### Traps and gotchas

- The upload step must follow the reachability sweep as well as the two bot runs. Checking only the greedy-descender step does not prove the full order.
- The review record names `d89338c`, not a later metadata tip, under D-184.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The author corrects P2-1 and pushes the shape test, review response, and handoff. A Codex session repeats the review at the new effective head.

## Session 109: 2026-09-11, Claude Code

Author: Claude Code
Session: the night logs of D-280. Branch `chore/night-logs-artifact`.

### What this session did, and why

- The owner merged PR #42, so the cron of the night reads 08:00 UTC on `main` now (D-284). This PR adds the one step of D-280 to the night workflow: on a failed step, `actions/upload-artifact@v4` keeps the `bot-logs` directory as a run artifact, and a night without logs says so with a warning instead of a silent pass.
- `NightWorkflowKeepsTheLogsOfAFailedNight` reads the workflow and asserts the step, its `failure()` condition, its path, its missing-files rule, and its place after the bot steps.
- D-280 is the dependency entry of the action (G-16).
- Hand runs 3 to 6 passed in 59, 65, 63, and 68 minutes, so six hand runs ran on 2026-09-10 and five passed. The record on `night-results` is the success of hand run 6 at `5455e5d`.
- Session 99 moved to the archive.

### State of the build

- `main` is at `f19fe2e`, the squash merge of PR #42. This branch holds the workflow commit above it, and this entry above that.
- Remote head: `origin/chore/night-logs-artifact` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 534 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR. The 03:00 UTC cron of 2026-09-11 did not fire by 04:00 UTC, while the runner was free and the old line still stood on `main`. The next chance is 08:00 UTC on the new cron, and a watch reads its result. After it passes, a session adds the five rows to the M-2 table and the owner signs Gate 1 (D-283).

### Traps and gotchas

- The upload step runs on `failure()` alone. A cancelled night keeps no logs, and a green night keeps none, by design.
- A change to the cron on `main` takes effect when GitHub reads the new file. GitHub delays or drops a schedule at the top of the hour under load, and the 03:00 UTC run of 2026-09-11 did not come. A cron off the top of the hour is the documented cure.
- The Mac runner takes a queued PR job only when no night is queued, as the six hand runs showed: five macOS jobs waited through two nights. Expect a PR's macOS job to wait for a whole night when one is queued.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the step, its condition, and the shape test. After the first scheduled night passes, a session records M-2 and the Gate 1 sign-off.

## Session 108: 2026-09-11, Codex

Author: Codex
Session: repeat review PR #42 at effective head `b616357`.

### What this session did, and why

- Read the response file and checked the provider gate again.
- The new commits change review and handoff metadata only. The effective head stays `b616357` under D-184.
- The macOS CI job now passes. The response file records 533 passed tests on the effective head. The focused shape test passes again.
- The earlier `Blocked` verdict is replaced with `Ready for owner merge`. No finding is open.
- Session 98 moved to the archive.

### State of the build

- `main` is at `5455e5d`. The effective implementation head is `b616357`. The PR tip before this review commit is `bc7c457`.
- The focused shape test passes. The response file records 533 passed tests, 0 failures, and 0 skips on the effective head.
- Remote Linux, Windows, and macOS CI, bit identity, bots, det-lint, night-gate, STE check, and Gitar pass. The review-gate check waits for this updated record.

### In flight

The repeat review record and this handoff entry are pushed. The review-gate check passes on this record. Duplicate platform jobs from the metadata push remain pending.

### Traps and gotchas

- The effective head is `b616357`, not the metadata tip. The new diff contains only paths in the D-184 metadata set.
- The review-gate failure at the prior tip named the old `Blocked` verdict. It did not report a product failure.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge after the pending duplicate platform jobs complete.

## Session 107: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #42 review. Branch `chore/night-time`.

### What this session did, and why

- Read `docs/reviews/pr-42.md`. No finding. The verdict is `Blocked` on two pieces of evidence: the macOS CI job, and the full suite, which the reviewer's sandbox could not run.
- `docs/reviews/pr-42-response.md` states where each stands. The macOS jobs of this branch are queued behind hand run 5 of D-283, which holds the Mac runner until about 01:15 UTC, and they run before hand run 6. The Linux and Windows CI jobs ran the full suite on this branch and passed, and the author ran it on the effective head: 533 passed.
- No code changed. The effective head stays `b616357`.
- Sessions 96 and 97 moved to the archive. The file held eleven entries, because the review entry above came without a rotation, and D-146 keeps ten.

### State of the build

- `main` is at `5455e5d`, the squash merge of PR #41. This branch holds the workflow commit `b616357`, the Session 105 entry, the three review commits, and this metadata commit above them.
- Remote head: `origin/chore/night-time` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 533 tests, 0 failures, on the effective head before the push of Session 105.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- Hand runs 3 and 4 passed in 59 and 64 minutes. Hand run 5 is on its sweep step, and run 6 follows it.

### In flight

PR #42 waits for its macOS jobs, then for a Codex repeat review that reads them and sets the verdict at the effective head `b616357`. Then the night logs PR of D-280 opens from the local branch `chore/night-logs-artifact` at `0e71413`. No other PR is open.

### Traps and gotchas

- A `Blocked` verdict on pending evidence needs a repeat review after the evidence lands, and the author never sets the verdict. The response file names the evidence so the repeat review finds it in one place.
- Every push to a PR during the hand runs queues a macOS job behind the current night. The queue runs in creation order, so a job pushed before the next dispatch runs before that night.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

When the macOS jobs of PR #42 pass, a Codex session repeats the review per the repeat procedure, reads the macOS result and the response file, and updates `docs/reviews/pr-42.md` with a new verdict at `b616357`.
