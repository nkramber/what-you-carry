# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 94: 2026-09-10, Claude Code

Author: Claude Code
Session: record the PR-58 decisions and the M-1 timing. Branch `docs/pr-58-decisions`.

### What this session did, and why

- The owner merged PR #35 as `1dc7e42`. Asked the two PR-58 questions and one on the M-1 timing in one batch, and D-274 to D-276 record the answers. OQ-144 holds the third question.
- D-274: any night record counts for the gate of PR-58, whatever event ran the night. The gate rule in the design doc and the roadmap reads "night" now, and the sequence still waits for one scheduled night before PR-58 opens (G-19).
- D-275: the gate checks the time and the commit. A record whose commit is not on the base branch of the PR fails, and the roadmap PR-58 entry gains exit test 8 for it. Exit test 6 names the commit in every failure message.
- D-276: the M-1 table fills before PR-58. The next PR holds the table.
- Session 84 moved to the archive.
- The automated pass asked for the resolved Phase 1 questions grouped under the heading of their date, and `2ce994f` does. The list under the 2026-09-08 heading held later resolutions too.

### State of the build

- `main` is at `1dc7e42`, the squash merge of PR #35. This branch holds the document commit above it, this entry above that, and the correction `2ce994f` above the entry.
- Remote head: `origin/docs/pr-58-decisions` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- No night has run. The branch `night-results` does not exist yet.

### In flight

This PR holds the three decisions. It changes no code, so the `review-override` label covers it (D-188, D-190). The M-1 table comes in the next PR (D-276). The first scheduled night runs at 03:00 UTC on 2026-09-11. No other PR is open.

### Traps and gotchas

- The PR-58 job needs the base branch for the ancestry check of D-275. A checkout of depth one does not hold it, so the job fetches the base ref before the check.
- A hand run of the night counts for the gate now (D-274), at the commit of the branch it runs on. The commit check of D-275 fails a record from a feature branch, so a hand run belongs on `main`.
- The PR-58 exit tests are eight now, and the design doc gate names five bad records.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A session fills the M-1 table in the Phase 1 roadmap from the push runs on `main` for the ten merged PRs since PR-3, one row per PR with the CI job durations per platform (D-276). After the first scheduled night, a session opens PR-58 from `main`.

## Session 93: 2026-09-10, Claude Code

Author: Claude Code
Session: record the PR-11 merge, and file the PR-58 questions before its code. Branch `docs/pr-11-merge-record`.

### What this session did, and why

- The owner merged PR #34 as `7487473` at 12:43 UTC, after the Codex repeat review of Session 92. The design doc PR-11 entry reads merged, the roadmap PR-11 entry has its status line, and the sequence marks item 19. The correction note records F-90.
- Exit tests 1 to 5 and 7 of PR-11 passed before the merge. Exit test 6 reads the first scheduled night, and the roadmap status line says so.
- Read the PR-58 entry for the questions that its code needs. The record of D-273 holds no event, and a hand run of the night workflow writes the same record, so the gate cannot tell a scheduled night from a hand run. The roadmap says nothing about the commit that a record names. OQ-142 and OQ-143 hold the two questions, each with a recommendation, and the session picked no default (D-124).
- No owner answer arrived, so `docs/decisions.md` did not change.
- Session 83 moved to the archive.

### State of the build

- `main` is at `7487473`, the squash merge of PR #34. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-11-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- The push checks on `7487473` passed: CI on the three platforms, bit identity, bots, determinism lint, and STE check.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The runner `mac-mini-m4` is online, from `/Volumes/SSD-1TB/actions-runner` on this machine. The branch `night-results` does not exist yet, and no night has run.

### In flight

This PR holds the merge record and the two questions. It changes no code, so the `review-override` label covers it (D-188, D-190). The first scheduled night runs at 03:00 UTC on 2026-09-11 on the runner, and it writes `night.json` to `night-results`. No other PR is open. PR-1 to PR-11 and PR-59 are merged. One scheduled night runs, then PR-58 opens, and M-1 and M-2 reach Gate 1. The M-1 table has no rows yet, and M-2 starts with the first night.

### Traps and gotchas

- A hand run of the night workflow writes the same record as a scheduled night, at the commit of the branch it runs on. Wait for the scheduled night, so that the first record is one that the PR-58 gate reads, whatever the answers to OQ-142 and OQ-143.
- The publish step of the night adds a worktree and an orphan branch inside the checkout of the runner, and the runner keeps that checkout between jobs. No second night has run yet. If a later night fails at the publish step, read `git worktree list` and `git branch` in the runner checkout first.
- The night workflow has `contents: write` and force-pushes `night-results` (D-273). Nothing else pushes there.
- The `review-override` label goes on after the last push and after the automated pass.

### Open questions that block progress

OQ-142 and OQ-143 block PR-58. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner answers OQ-142 and OQ-143, and a session records the answers as the next D-# ids. After the first scheduled night writes `night.json` to `night-results`, a session opens PR-58 from `main`, and its exit test 7 reads the real record. The same session starts the M-2 table with the duration of the first night.

## Session 92: 2026-09-10, Codex

Author: Codex
Session: repeat review PR-34 at effective head `4e1ea9e`. Branch `feat/pr-11-bots`.

### What this session did, and why

- Verified the provider gate. Sessions 89 and 91 identify Claude Code as the author of the PR-34 change and its correction. Codex is the eligible reviewer.
- Recomputed the effective head. `4e1ea9e` is the newest substantive commit. Later commits change only review and handoff metadata.
- Read the correction diff, the response record, the PR comments and replies, the PR-11 roadmap, the console collection, and the affected tests.
- Closed P1-1. Three independent full test runs pass 518 tests with no failure or skip. The collection serializes all four console-touching test classes, and the shape test scans every test directory.
- Updated `docs/reviews/pr-34.md` with the fixed finding and the verdict `Ready for owner merge` at effective head `4e1ea9e`.

### State of the build

- `main` is at `d3093cf`, the squash merge of PR #33. The effective PR-34 head is `4e1ea9e`.
- Remote head: `origin/feat/pr-11-bots` is `945004a`, pending the final metadata verification push.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures, 0 skips, three times in a row.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #34 needs this repeat-review record pushed. The substantive CI checks pass. The evaluate and review-gate checks must rerun after this metadata commit.

### Traps and gotchas

- The effective head is `4e1ea9e`, not the current metadata tip. D-184 excludes the review and session handoff paths.
- P1-1 stays fixed only while new tests that touch the process console carry the `Console` collection. The shape test enforces this for files under the test directories.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit the repeat-review record and this handoff entry. Push the branch. Fetch and verify the remote head and the review-gate result.

## Session 91: 2026-09-10, Claude Code

Author: Claude Code
Session: answer the PR #34 review. Branch `feat/pr-11-bots`.

### What this session did, and why

- Read the one P1 finding in `docs/reviews/pr-34.md`. Full merit: the bot tests call a command that writes its summary to the process console, and the bit-identity command test captures that console, so a full run could read the wrong line.
- Every test class that calls a command or redirects the console carries `[Collection(ConsoleCollection.Name)]` now, so xUnit runs the four one after another. `ConsoleCollectionTests.EveryConsoleTestIsInTheCollection` reads every test source and fails on a class that touches the console outside the collection.
- Three full runs with `-m:1` on the correction: 518 passed, 0 failed, 0 skipped, each time.
- F-90 records the finding, and `docs/reviews/pr-34-response.md` records the disposition. The correction is `f6ca5f6`, and `4e1ea9e` answers the automated pass on it: the shape test scans every test directory.

### State of the build

- `main` is at `d3093cf`, the squash merge of PR #33. This branch holds the PR-11 commit `8a7367f`, the correction `0536f7e`, the review commits, the correction `f6ca5f6`, and this entry above them.
- Remote head: `origin/feat/pr-11-bots` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures, three times in a row.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.

### In flight

PR #34 is open and it holds this branch. The automated pass approved `f6ca5f6` with one suggestion, and `4e1ea9e` answers it, so the effective head is `4e1ea9e`. A Codex repeat review at that head updates the same review record. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-10 and PR-59 are merged. PR-11 is open as PR #34. After its merge, one scheduled night runs on its own, then PR-58 opens, and M-1 and M-2 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `4e1ea9e`. The review record still names `0536f7e`, and the repeat review updates the head and the verdict together, with one verdict name in the Verdict section (D-269).
- A test that calls a command of the Tools project, or redirects the console, goes in the console collection, or the shape test fails.
- xUnit runs the classes of one collection one after another, so the console collection takes a little longer than the parallel run of four classes. The full suite still takes under three and a half minutes.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure, at the effective head `4e1ea9e`, and updates `docs/reviews/pr-34.md` with the status of P1-1 and a new verdict.
