# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 101: 2026-09-10, Codex

Author: Codex
Session: review PR #40, the night gate. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- Verified the provider gate. Session 100 identifies Claude Code as the provider of the substantive PR-40 change. Codex is the eligible reviewer.
- Verified the base, merge base, effective head `2253e53`, complete diff, PR-58 scope, exit tests, decisions, and all PR comments and replies.
- Found P1-1. The workflow catches every fetch failure as an absent record but leaves a `night.json` from the pull request checkout in place. A pull request can then supply a fresh success record and bypass the absent-record gate.
- Wrote `docs/reviews/pr-40.md` with the verdict `Changes required` for effective head `2253e53`.

### State of the build

- `main` is at `a2799f2`, the squash merge of PR #39. The effective PR-40 head is `2253e53`.
- Remote checks at the PR head passed for CI, bit identity, bots, det-lint, STE check, night-gate, and Gitar. The review-gate check was neutral before the review record existed.
- `git diff --check` passed. A local serial build and test retry hit a stuck execution-context process after an earlier parallel file-copy contention and was cancelled. The remote checks are the revision-specific build and test evidence.

### In flight

PR #40 needs the workflow correction for P1-1 and a repeat review at the new effective head. No other PR is open.

### Traps and gotchas

- The fetch step must not read a file from the pull request checkout when `night-results` is absent or the fetch fails. Use a temporary path or remove the checkout file before the fetch.
- The review record names the effective head `2253e53`, not the metadata tip rule in a future review commit (D-184).

### Open questions that block progress

P1-1 blocks PR #40. OQ-99 is open, and it blocks no other work.

### Next concrete action

Correct the `night-gate` workflow so only a successfully fetched `night-results` record can reach the command. Add the missing workflow regression test, push, and request a repeat review.

## Session 100: 2026-09-10, Claude Code

Author: Claude Code
Session: the second night by hand, and PR-58, the night gate. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- The owner merged PR #39 as `a2799f2`. Started the night by hand on `main` at 19:00 UTC under D-281: run 34517749543, 63 minutes, success. The record on `night-results` names `a2799f2` and ended at 2026-09-10T20:03:22Z.
- PR-58 from `main` at `a2799f2`. `Tools/NightGate/` holds the parser, the facts, the rules, and the command `night-gate`. The rules read the cases in order: absent, malformed, stale, foreign, cancelled, failed, pass. The command exits 0, 1, or 2, and every failure line names the case, the commit, and the time (D-274, D-275).
- The workflow `night-gate.yml` runs one job on Linux with the full history, fetches `night-results`, and passes the record and the base ref to the command. An absent branch reads as an absent record.
- `GitRepository` gains `HasCommit` and `IsAncestor`, each with the exit codes that git documents for a no. `night-record` writes no byte-order mark now, and PR-11 exit test 7 asserts the first byte.
- Exit tests 1 to 6 and 8 pass among 530 tests, with the malformed case, the exit codes, and a workflow shape test beside them. Exit test 7 is the job on this PR. The agent files gain the `night-gate` line of the PR gate.
- The roadmap PR-58 entry holds the scope notes, and the design doc names the malformed case. The M-2 section holds the note of the second hand run.
- Session 90 moved to the archive.

### State of the build

- `main` is at `a2799f2`, the squash merge of PR #39. This branch holds the PR-58 commit, the byte-order-mark fix, and the documents commit above them, and this entry is in the documents commit.
- Remote head: `origin/feat/pr-58-night-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 530 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- The branch `night-results` holds the record of the second hand run.

### In flight

PR-58 is open as the PR that holds this branch. A Codex session reviews it at the effective head, which is the documents commit, because it changes the roadmap and the design doc. After the merge, the night logs PR of D-280 opens from the local branch `chore/night-logs-artifact`, then M-2 collects seven scheduled nights, then Gate 1.

### Traps and gotchas

- The `night-gate` job fails every PR within 48 hours of a red or missing night, a documentation PR too. A hand run on `main` restores it (D-274, D-278).
- The record commit must be on the base branch (D-275). A hand run on a feature branch writes a record that fails every PR to `main`.
- The effective head of this PR is the documents commit, not the code commit, because the roadmap and the design doc lie outside the metadata set (D-184).
- The night holds the one Mac runner for its whole run, so the macOS CI jobs of every open PR wait for it.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR-58 per the `pr-review` skill at the effective head, with the focus on errors, CI boundaries, and test quality. It confirms the job passed on the PR against the real record (exit test 7, G-19).

## Session 99: 2026-09-10, Claude Code

Author: Claude Code
Session: correct the head field of the PR #39 review record, one time (D-282). Branch `fix/dig-plan-job-budget`.

### What this session did, and why

- The review record of Session 98 named the fix commit `0685c4e` as the effective head, and the gate refused it. The effective head was `90ab13e`, because that commit adds entries to the decisions, design, questions, and roadmap files, which lie outside the metadata set (D-184). The hand-over of Session 97 gave the wrong head, and the review followed it.
- The owner chose an author correction over a repeat review, one time. OQ-150 holds the question, and D-282 records the answer.
- The register commit `34cbc39768611d795fb656b6fa6d65a17432931e` holds D-282 and OQ-150, and this commit sets the head field of the record to it and adds a dated note above the Verdict section. This commit changes the record and the two handoff files alone, so the effective head stays `34cbc39768611d795fb656b6fa6d65a17432931e`.
- Ran the review-gate command locally against the head of this commit before the push: conclusion success.
- Session 89 moved to the archive.

### State of the build

- `main` is at `736e466`, the squash merge of PR #38. This branch holds the fix `0685c4e`, the registers commit `90ab13e`, the review commit `ac5f8d9`, the register commit `34cbc39768611d795fb656b6fa6d65a17432931e`, and this entry above them.
- Remote head: `origin/fix/dig-plan-job-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 520 tests, 0 failures. No code changed in this session.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The branch `night-results` holds the failure record of the first night at `fb080ca`.

### In flight

PR #39 waits for the gate to read this head, then for the owner merge. After the merge, the second night by hand on `main` (D-281), then PR-58 from the local branch `feat/pr-58-night-gate` at `32dcbcb`, then the night logs PR of D-280 from the local branch `chore/night-logs-artifact` at `5bef2c8`.

### Traps and gotchas

- The metadata set of D-184 is `docs/reviews/` and the two handoff files alone. A commit that adds a D-#, an F-#, an OQ-#, or a roadmap line moves the effective head. Name the hash that the gate rule gives at every hand-over: the newest commit in the range outside those three paths.
- A decision that a PR itself needs goes in a commit before the review record commit, so the record can name it. The record commit then changes the metadata paths alone.
- D-282 is one time. On every other PR the reviewer owns the review record, and a wrong head field takes a repeat review.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #39 when the review-gate check reads green on this head. Then a session starts the night by hand on `main` (D-281), reads `night.json` on `night-results`, and opens PR-58 on a success record.

## Session 98: 2026-09-10, Codex

Author: Codex
Session: review PR #39 at effective head `0685c4e`. Branch `fix/dig-plan-job-budget`.

### What this session did, and why

- Reviewed the measured dig-plan cap fix for F-92 under D-253 and D-279.
- Verified the exact PR base, merge base, substantive head, provider gate, complete diff, caller, regression tests, decisions, roadmap, and automated pass.
- Added `docs/reviews/pr-39.md`. The review found no issue and records `Ready for owner merge` for effective head `0685c4e`.

### State of the build

- `main` is at `736e466`. The PR branch is at metadata tip `90ab13e`, with effective head `0685c4e`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 520 tests, 0 failures, 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #39 waits for the owner to confirm the pending remote checks and merge it. After the merge, run the second night by hand on `main` under D-281.

### Traps and gotchas

- The first local test attempt failed before test execution because VSTest could not bind its local socket. The elevated retry passed.
- The PR review-gate result was neutral before this review record existed. GitHub API access failed during the final status refresh, so the complete remote status remains unverified.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner confirms the remote checks and merges PR #39. Then run the second night by hand on `main` and read its `night.json` record.

## Session 97: 2026-09-10, Claude Code

Author: Claude Code
Session: the first night failed on the dig job cap, and this fix raises the cap on the measurement (F-92, D-279). Branch `fix/dig-plan-job-budget`.

### What this session did, and why

- The hand-run night 34499677095 on `fb080ca` failed at the greedy descender step at 16:34 UTC: bottom 4997, crash 3 of 5000, and the record on `night-results` reads failure. The reachability sweep did not run.
- Reproduced the three crashes locally in ten parallel chunks of 500 seeds. Each is the same fault: the dig plan ran its 400 jobs and a chamber stayed in rock, on seed 2170 floor 10, seed 3000 floor 4, and seed 4786 floor 10. The PR sweep digs floor 11, 1, and 2 for those seeds, so it never met them.
- Measured with the cap lifted: the three floors need 2817, 1661, and 615 jobs. Of 5000 sweep floors, 4969 finish inside 25 jobs, and the largest other need is 208. The three floors generate, play to bottom, and hold 0.033 to 0.036 air against a normal mean of 0.040 to 0.052, so the extra jobs were walkers with no room.
- Asked three owner questions in one batch. D-279 sets `DigPlan.MaxJobs` to 10000, D-280 uploads the bot logs of a failed night as a run artifact, and D-281 runs a second night by hand after this fix merges. OQ-147 to OQ-149 hold them.
- `DigUntilComplete` gives the job count now, and `DigPlanJobCapTests` digs the three floors and fails on the old cap. F-92 is in the register, and the M-2 section holds the note of the first hand run.
- The bit-identity hash stays `6ec00e90c1c85cdb`, because no floor that dug inside 400 jobs changes. The simulation version stays 6 for the same reason.
- Session 87 moved to the archive.

### State of the build

- `main` is at `736e466`, the squash merge of PR #38. This branch holds the fix commit above it, and this entry above that.
- Remote head: `origin/fix/dig-plan-job-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 520 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The branch `night-results` holds one record: `fb080ca`, 16:34:28 UTC, failure. PR-58 is written on the local branch `feat/pr-58-night-gate` at `d6f7c6b` above `fb080ca`, with 528 tests green, and it waits for a success record.

### In flight

This PR, the fix of F-92. A Codex review comes next. After the merge, a second hand run of the night (D-281), then PR-58 opens from `main` with the local branch rebased, then the night logs PR of D-280. No other PR is open.

### Traps and gotchas

- The night runs on the one Mac runner, so the macOS CI jobs of every open PR wait for it. A hand run at daytime holds them for about half an hour, and a green run with the sweep takes longer.
- The runner checkout cleans the bot logs at the next job. Until D-280 lands, reproduce a failed night locally: ten parallel `bot-run` chunks of 500 seeds take about three minutes.
- The sweep digs one floor per seed, `1 + seed % 15`, and the bots dig all fifteen floors of a seed, so the bots cover floors the sweep never digs.
- The record of `night-record` starts with a byte-order mark, because `File.WriteAllText` with `Encoding.UTF8` writes one. `File.ReadAllText` strips it, so the gate of PR-58 reads it, and the PR-58 branch corrects the writer.
- `DigUntilComplete` returns the job count. A caller that ignores it compiles, so the regression test is the only reader.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cap, the measurement, the regression test, and the hash claim. After the merge, start the night by hand on `main` (D-281), and when its record reads success, open PR-58 from the rebased local branch.

## Session 96: 2026-09-10, Claude Code

Author: Claude Code
Session: record the first night by hand (D-278), and start that night. Branch `docs/night-by-hand`.

### What this session did, and why

- The owner merged PR #37 as `fb080ca`, then asked for a one-time hand run of the night, so that PR-58 opens before the first scheduled night. OQ-146 holds the question, and D-278 records the answer. D-274 is revised in part, the sequence wait only, and its gate rule stands.
- Started the night workflow on `main` at `fb080ca` by `workflow_dispatch` at 16:03 UTC. The run is 34499677095. It writes `night.json` to `night-results` whatever its outcome.
- Sequence item 20 reads "scheduled or by hand" in the roadmap and the design doc, and so does the PR-58 entry. The M-2 procedure counts the seven scheduled nights, with a note beside the table for the hand run.
- Session 86 moved to the archive.

### State of the build

- `main` is at `fb080ca`, the squash merge of PR #37. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/night-by-hand` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The night run 34499677095 was in progress on the runner `mac-mini-m4` when this entry was written. The branch `night-results` appears when its publish step runs.

### In flight

The night run on `main`. This PR holds D-278 and changes no code, so the `review-override` label covers it (D-188, D-190). No other PR is open. The scheduled night still runs at 03:00 UTC on 2026-09-11 and overwrites the record.

### Traps and gotchas

- The night record names the commit it ran on, `fb080ca`. The PR-58 gate checks that the record commit is on the base branch (D-275), and a later merge to `main` keeps it there.
- A failed hand run leaves a failure record. No gate reads it yet, so it blocks nothing, and the scheduled night overwrites it. Read the run log before a second hand run.
- The night runs on this Mac, and the reachability sweep of one hundred thousand seeds is the long step. The M-2 bound is six hours.
- The publish step makes a worktree and an orphan branch in the runner checkout. The scheduled night tomorrow is the second run on that checkout, and its publish step is the one to watch.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

When the night run ends, read `night.json` on `night-results`. On a success record, open PR-58 from `main` per the roadmap entry, D-274, and D-275, and note the hand run duration beside the M-2 table. On a failure record, read the run log and correct the cause in a PR before the next hand run.

## Session 95: 2026-09-10, Claude Code

Author: Claude Code
Session: the M-1 table, and the seed count decision. Branch `docs/m-1-table`.

### What this session did, and why

- The owner merged PR #36 as `0ba348c`. D-276 lets the M-1 table fill before PR-58, and the ten Phase 1 PRs since PR-3 merged, so the table is complete in the Phase 1 roadmap.
- The rows read the push run on `main` of the squash-merge commit of each PR, first attempt, in seconds per job. The CI job grew at PR-9 with the reachability sweep, and again at PR-10 and PR-11.
- The Windows CI job of PR-10 took 601 seconds, one second over the ten-minute bound of the M-1 procedure. F-91 records the measurement, OQ-145 holds the question, and D-277 keeps the 5000 PR seeds of D-116. The bound reads again at Gate 1.
- The design doc M-1 entry reads complete. Sequence item 22 marks M-1 in the roadmap, and item 8 marks it in the design doc.
- Session 85 moved to the archive.

### State of the build

- `main` is at `0ba348c`, the squash merge of PR #36. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/m-1-table` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- No night has run. The branch `night-results` does not exist yet.

### In flight

This PR holds the M-1 table and D-277. It changes no code, so the `review-override` label covers it (D-188, D-190). The first scheduled night runs at 03:00 UTC on 2026-09-11. No other PR is open. After the night, PR-58 opens, and M-2 starts with the first night duration. Then Gate 1.

### Traps and gotchas

- The M-1 rows come from the push runs on `main`, not the pull request runs. A PR runs its jobs on every push, so a PR has many runs, and the push run on `main` is one per PR.
- The M-1 numbers are job durations from the start of the job to its end, as the workflow API reports them. The queue time before the job is not in the number.
- A job past eleven minutes files a new question on D-116 (D-277). The Windows CI job is the one to watch, at 574 to 601 seconds on the last two PRs.
- The M-2 table has no rows until the first night. The night duration comes from the same API, on the night workflow run.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

After the first scheduled night writes `night.json` to `night-results`, a session opens PR-58 from `main` per the roadmap entry, D-274, and D-275. The same session starts the M-2 table with the duration of that night.
