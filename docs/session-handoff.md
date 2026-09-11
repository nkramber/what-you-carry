# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 106: 2026-09-11, Codex

Author: Codex
Session: review PR #42 at effective head `b616357`.

### What this session did, and why

- Verified the base, merge base, effective head, complete diff, D-284, the Phase 1 roadmap, the workflow, the shape test, the handoff files, the agent files, and every PR comment.
- The cron reads `0 8 * * *`. The shape test checks the cron, the hand trigger, and the 02:00 Central Standard Time comment. No finding remains.
- The metadata commit adds `docs/reviews/pr-42.md` and this entry. The effective head stays `b616357` under D-184.

### State of the build

- `main` is at `5455e5d`. The effective implementation head is `b616357`. The PR metadata tip is `b9a5d5d` after this review commit.
- `dotnet build`: 0 warnings, 0 errors. The focused shape test passes. The full local suite did not complete after the test runner socket error and an approved retry that produced no output.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. The Godot 4.7.2 headless build passes.
- Remote Linux and Windows checks, bit identity, bots, det-lint, night-gate, STE check, and Gitar pass. The macOS CI job remains pending. The review-gate check waits for this record.

### In flight

The review record and this handoff entry are pushed. The owner must wait for the pending macOS CI result and the review-gate result.

### Traps and gotchas

- The effective head is `b616357`, not the metadata tip. Only the review and handoff paths are metadata under D-184.
- The first local test run failed on a socket permission error. The approved full-suite retry did not complete. Treat both as execution-context results.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the macOS CI and review-gate results before the owner merges.

## Session 105: 2026-09-11, Claude Code

Author: Claude Code
Session: the night time of D-284. Branch `chore/night-time`.

### What this session did, and why

- The owner merged PR #41 as `5455e5d`. This PR moves the cron of the night workflow to `0 8 * * *`, which is 08:00 UTC and 02:00 Central Standard Time (D-284). The comment in the workflow names the time and the summer shift to 03:00 Central Daylight Time.
- `NightWorkflowRunsAtTwoCentralStandardTime` reads the workflow and asserts the cron, the hand trigger, and the comment.
- Hand run 3 passed in 59 minutes at `5bdef87`, and its record is on `night-results`. Hand run 4 started at 23:05 UTC. The dispatcher starts runs 5 and 6 in turn.
- Session 95 moved to the archive.

### State of the build

- `main` is at `5455e5d`, the squash merge of PR #41. This branch holds the workflow commit above it, and this entry above that.
- Remote head: `origin/chore/night-time` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 533 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR, then the night logs PR of D-280 from the local branch `chore/night-logs-artifact` at `0e71413`. Hand runs 4 to 6 hold the Mac runner in turns until about 02:15 UTC. The old cron fires at 03:00 UTC if this PR merges after that time, and the new one at 08:00 UTC otherwise. Either is the first scheduled night of D-283.

### Traps and gotchas

- The cron of GitHub reads UTC alone. The 08:00 UTC line is 02:00 Central Standard Time and 03:00 Central Daylight Time. A line that follows the local clock all year needs two cron lines and a guard step.
- The effective head of this PR is the workflow commit. This entry is a metadata commit.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, and the shape test. After the merge, the night logs PR opens. On 2026-09-11, after the first scheduled night, a session adds the five rows to the M-2 table and the owner signs Gate 1.

## Session 104: 2026-09-10, Claude Code

Author: Claude Code
Session: record the PR-58 merge, the Gate 1 timing, and the night time. Branch `docs/pr-58-merge-record`.

### What this session did, and why

- The owner merged PR #40 as `5bdef87`. The design doc PR-58 entry reads merged, F-48 reads done, the roadmap entry has its status line, and the sequences mark items 20 to 22 and the gate condition.
- Asked one owner question on the Gate 1 timing, and the owner answered with two of their own: why seven scheduled nights, and what is playable today. Answered both. The runs are deterministic on one machine, so a scheduled night adds no measurement, and nothing renders before PR-12.
- D-283: four more nights run by hand today, the M-2 table counts the six hand runs and the first scheduled night, and Gate 1 signs on 2026-09-11 after that night passes on its own. OQ-151 holds the question. D-278 is revised in part, the M-2 count only.
- D-284: the night runs at 08:00 UTC, which is 02:00 Central Standard Time. OQ-152 holds the question. D-278 is revised in part, the time only. The workflow change comes in a PR of its own.
- Started the four hand runs at 22:06 UTC, one after another, from a script that dispatches the next when the previous ends, so the PR jobs queued between them still reach the runner. The M-2 section holds a table with the two rows so far.
- Session 94 moved to the archive.

### State of the build

- `main` is at `5bdef87`, the squash merge of PR #40. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-58-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 532 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The night gate is live on every PR. The record on `night-results` is the success of hand run 2 at `a2799f2` until the next night overwrites it.

### In flight

This PR changes no code, so the `review-override` label covers it (D-188, D-190). The four hand runs hold the Mac runner in turns until about 02:30 UTC on 2026-09-11. Two workflow PRs follow from prepared local branches: the night time of D-284, then the night logs of D-280 on `chore/night-logs-artifact` at `0e71413`. The cron stays at 03:00 UTC until the night time PR merges.

### Traps and gotchas

- A macOS CI job queued during a hand run waits for that run, up to about an hour. The script dispatches the next night only after the previous one ends, so the queued PR jobs run between them.
- The M-2 rows of the four hand runs and the scheduled night come from the runs API: `run_started_at` to `updated_at`. The jobs API gave null times for the self-hosted job.
- Gate 1 signs in `docs/decisions.md` by the owner, after the first scheduled night passes on its own. The night gate on the PRs reads the same record.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

After this PR merges, open the night time PR from `main`: the cron `0 8 * * *` with a comment that names 02:00 Central Standard Time, and a shape test (D-284). Then the night logs PR. On 2026-09-11, after the first scheduled night, a session adds the five rows to the M-2 table and the owner signs Gate 1 in the register.

## Session 103: 2026-09-10, Codex

Author: Codex
Session: repeat review PR #40 at effective head `d943cb3`. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- Recomputed the PR identity. The base and merge base are `a2799f2`. The effective head is `d943cb3`. The tip `bcb934e` changes review and handoff metadata only.
- Verified the cross-provider gate. Claude Code authored the substantive PR change and its correction. Codex is the eligible reviewer.
- Reproduced P1-1 at the correction boundary. A planted checkout record fails when the remote has no branch. A branch without `night.json` is absent. A remote failure record wins over the planted success record. An unreachable remote reports an error.
- Verified the adjacent byte-order-mark parser case, command exit cases, workflow shape test, full-history checkout, and the remote-read path. P1-1 has full merit at the prior head and is corrected at `d943cb3`.
- Updated `docs/reviews/pr-40.md` with the finding disposition and the verdict `Ready for owner merge`.

### State of the build

- `main` is at `a2799f2`. The PR branch is at `bcb934e` before this review commit. The effective implementation head is `d943cb3`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 532 tests, 0 failures, 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`.
- The Godot 4.7.2 headless build passes. The real `night-gate` probe passes against `origin/night-results` at `2026-09-10T22:00:00Z`.
- The review commit is `0d10eba`. Its Gitar, det-lint, night-gate, STE check, and review-gate checks passed. The metadata tip is now `48ede21`, and its fresh checks are pending after the metadata push.
- Remote head: `origin/feat/pr-58-night-gate` at `48ede21`, checked after the push. The checkout has no ahead count.

### In flight

PR #40 waits for the review-gate check to read this record at the effective head. No other PR is open.

### Traps and gotchas

- The review record must name `d943cb3`, not the metadata tip. The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` (D-184).
- The real-remote probe first failed because the sandbox could not resolve `github.com`. The approved retry passed. Treat the first result as an execution-context failure.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge after the pending CI, bit identity, and bots checks pass. The review-gate check reads `Ready for owner merge` at `d943cb3`.

## Session 102: 2026-09-10, Claude Code

Author: Claude Code
Session: answer the PR #40 review. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- Read the one P1 finding in `docs/reviews/pr-40.md`. Full merit: the fetch step of the workflow left a `night.json` from the PR checkout in place when the fetch failed, and the command read it, so a PR could carry a fresh success record and pass the gate with no night.
- The fetch is in the tool now. `NightGateFacts.Gather` takes the checkout, the remote, the base ref, and the time. It asks the remote for the branch with `ls-remote --exit-code`, fetches it, and reads `night.json` from `FETCH_HEAD` through git, never from the working tree. An absent branch or an absent file is the absent case with the reason, and any other git failure throws with the command (T-2). The workflow has no shell step.
- The parser accepts a leading byte-order mark, because the two records on `night-results` carry one and git does not strip it.
- `NightGateReadsTheRecordFromTheRemoteAndNeverFromTheCheckout` plants a fresh success record in the checkout and asserts the absent case, the reason of a branch without the file, the win of the branch record over the planted file, and the error of an unreachable remote. The exit code test plants the file too. The tests use one temporary repository as the remote of another.
- F-93 records the finding, and `docs/reviews/pr-40-response.md` records the disposition. The tool ran against the real remote from this checkout: pass, at `a2799f2`.
- Session 92 moved to the archive.

### State of the build

- `main` is at `a2799f2`, the squash merge of PR #39. This branch holds the PR-58 commit, the byte-order-mark fix, the documents commit `2253e53`, the review commit `3e964a2`, and the correction that holds this entry above them.
- Remote head: `origin/feat/pr-58-night-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 532 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

PR #40 waits for the automated pass on the correction, then a Codex repeat review at the effective head, which is the correction commit. No other PR is open.

### Traps and gotchas

- The effective head is the correction commit, because it changes code, the register, and the roadmap. The review record still names `2253e53`, and the repeat review updates the head and the verdict together, with one verdict name in the Verdict section (D-269).
- The night gate reads the record through git. A test of it needs a remote with the orphan branch, and `PublishNight` in the tests makes one from a temporary repository.
- `git ls-remote --exit-code` exits 2 for no matching ref. Every other nonzero exit is an error, and the tool throws. The job then fails with the git message and not with a gate case.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure at the effective head, verifies P1-1 against its trigger and the regression test, and updates `docs/reviews/pr-40.md` with the status of P1-1 and a new verdict.
