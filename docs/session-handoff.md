# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 245: 2026-09-24, Claude Code

Author: Claude Code
Session: PR-85, author. Branch `feat/pr-85-night-hosted-linux`. PR #102, pending merge. Base `a68348b`.

### What this session did, and why

- Read the two runs on `main` for PR-84:
  - `night-promote.yml` run 36059305679 at `a68348b` ended green with the case `code-changed`. The branch night ran at `35da819`, and the merge differs in four paths outside the skip set, `.github/` included. The gate re-run step skipped, and `night-results` still holds `4ac067b` (`55b6d3f`, success), so the run wrote nothing.
  - PR-84 exit test 11: the first night on `main` after `a68348b` has not run. Its cron is 08:07 UTC on 2026-09-25, and the read was at 21:22 UTC on 2026-09-24. Its record must name the slice of 2026-09-25: 5501-6000 for each bot policy, 110001-120000 for reachability.
- PR-85 moves the night to 07:07 UTC on hosted Linux (D-571 to D-573). `night.yml` holds a plan job, a matrix of six sweep jobs on `NightSeeds.Sweeps`, and a record job. The record job alone holds write permissions.
- `.github/scripts/night-gather.sh` joins the sweep artifacts, and gives the status: success only when the plan, every sweep, and the record job read success.
- D-284, D-285, and D-288 carry "Revised in part by D-571". The roadmap entry of PR-85 holds nine exit tests.
- Branch night run 36062699698 at `9d5c36b` failed on infrastructure, not on a seed. The runner of the full clearer received a shutdown signal at 23:23:57 UTC, 1 h 45 min into `bot-run`. The five other sweeps passed: random walker 20 min, coward 23 min, timer tester 24 min, greedy descender 46 min, reachability 1 h 31 min. The record job wrote a failure record with no failure line of the full clearer, as the design asks.
- A local run of 200 full clearer seeds held 169 MB and 4 KB of log for each seed, so memory and disk did not cause the shutdown.
- The sweep uploads now take `overwrite: true`. Without it, a re-run of the failed jobs fails when the sweep of attempt 1 uploaded its result.
- Exit test 7 passes. Branch night run 36077051456 at `e5e164f` ended green at 04:12:22 UTC on 2026-09-25, in 3 h 52 min. Sweep jobs: random walker 20 min, coward 22 min, timer tester 31 min, greedy descender 1 h 17 min, reachability 2 h 31 min, full clearer 3 h 51 min. The record job took 29 s. The record names the slice of 2026-09-25 (5501-6000, and 110001-120000 for reachability), the deaths of all five policies, and no carried or failed seed.

### State of the build

- Local: the full suite passed 1614 of 1614 at each code commit, Smoke included. The new shape tests fail on the old `night.yml` (7 failures). `ste-check` reads 0.

### In flight

- PR-84 exit test 11: the night on `main` from the 08:07 UTC cron. Read it before the review.
- Then `make codex-review PR=102 -- --skip-gitar-review`, and the merge summary in Q/A form (D-552).

### Traps and gotchas

- `download-artifact` with a pattern puts each artifact in a directory of its own name. The gather script reads `<results>/*/`.
- A shutdown signal on a hosted runner is an infrastructure fault. Re-run the failed jobs of the night: the plan keeps its date, and the record job runs again.
- A sweep job creates its empty result files before the build, so a broken build still uploads a result, and the record reads "did not end".

### Open questions that block progress

None.

### Next concrete action

Read the night on `main` of 2026-09-25 for PR-84 exit test 11, and state it here. Then export the PR comments (command F of `gitar-review`), and run `make codex-review PR=102 -- --skip-gitar-review`.

## Session 244: 2026-09-24, Codex

Author: Codex
Session: PR-84, reviewer, round 2. Branch `feat/pr-84-night-fixed-seeds`. PR #100, pending owner merge. Base `55b6d3f`.

### What this session did, and why

- Reviewed the fix for P1-1. The failure record script keeps the failed seeds from the record of `main` when a build fails before `night-record` runs.
- Updated `docs/reviews/pr-100.md`. P1-1 is fixed at `b07e6ed`. The review found no new defects.
- Exported the PR comments. The only comment is a gitar notice, which needs no answer (D-550).

### State of the build

- The focused regression test passed: 1 test, 0 warnings, 0 errors.
- The full suite passed 1612 of 1612 tests in 5 minutes 26 seconds. `ste-check` found 0 issues.
- CI and the night gate passed after the fix at `ab566b0`. The current PR tip `9eee277` has green document, lint, asset, and night-gate checks. Heavy jobs skipped under the documents-only rule.
- The current `evaluate` and `review-gate` runs failed before this review record approved the head. This session does no push wait under D-542.

### In flight

- The review record, archive rotation, and this handoff entry are in one metadata commit on the PR branch.
- Post-push check results are not observed.

### Traps and gotchas

- The focused test runs the script with `bash` and `jq`. The Windows test leg checks the script text only.
- The branch night passed at `35da819`, before the P1-1 fallback fix. The focused regression test covers the fallback path.

### Open questions that block progress

None.

### Next concrete action

After the review commit reaches the PR branch, the owner can read the merge summary and decide whether to merge PR #100.

## Session 243: 2026-09-24, Claude Code

Author: Claude Code
Session: PR-84, author, the answer to review round 1. Branch `feat/pr-84-night-fixed-seeds`. PR #100, pending merge. Base `55b6d3f`.

### What this session did, and why

- Review round 1 (session 242) read `Changes required` with P1-1. After a broken build, the failure record dropped the failed seeds of `main`, so the carry of D-567 could end with no fix. Full merit.
- Added `.github/scripts/night-failure-record.sh`. It writes the failure record with `jq` and keeps the `failedSeeds` of the record of `main`. The publish step fetches that record itself. The answer is `docs/reviews/pr-100-response.md`.
- Added `TheFailureRecordOfABrokenBuildKeepsTheCarriedSeeds`. It runs the script, and the next plan and the promotion check then read the kept seed.
- Session 241 holds exit test 6 of PR-83 (pass), the owner answers D-564 to D-569, and the cloud move D-570 to D-573.
- Exit test 10 passes. Branch night run 36030984588 at `35da819` ended green at 20:33 UTC. Its record names the slice of 2026-09-24, and no carried or failed seed. The fix of P1-1 changes the publish step of a broken build alone.

### State of the build

- Local after the fix: the full suite passed 1612 of 1612, Smoke included. `det-lint` and `ste-check` read 0.
- The new test first sat in `NightGateTests`. That class holds the fixture literal `"docs/decisions.md"`, so the text rule of D-476 read a document read. The test moved to `NightSeedsTests`.
- The effective head moves to the push of this entry, because the fix changes `.github/`.

### In flight

- CI of the fix push, then review round 2 through `make codex-review PR=100 -- --skip-gitar-review`.
- The merge summary in Q/A form after an approved review (D-524, D-552).

### Traps and gotchas

- Day 0 of the slices is 2026-09-24. The first slice runs seeds that no night ran before, so it can fail on an old fault. A fix of such a seed is a PR of its own.
- The failure record script needs `bash` and `jq`. The Mac runner has `/usr/bin/jq`, and the hosted Linux image has `jq`.
- `night-record` needs `--date`, `--failures`, and `--carry`.
- A documents push skips the heavy jobs only after the runs on the previous head end green (CI skip rule 2).

### Open questions that block progress

None.

### Next concrete action

This session: wait on CI and review round 2 of PR #100, then give the merge summary.

After PR #100 merges, the next session makes PR-85 alone (D-570, D-571, D-572, D-573). It goes ahead of PR-75 in the Phase 2 order.

- The night cron of `.github/workflows/night.yml` moves from `7 8 * * *` to `7 7 * * *`, 07:07 UTC. Update the time comment and `NightWorkflowRunsAtTwoCentralStandardTime`, and revise D-284, D-285, and D-288 in part.
- The night moves to `ubuntu-latest` as parallel jobs, one for each sweep of `NightSeeds.Sweeps`. A hosted job stops at 6 hours. With the slice, the full clearer took 91 minutes on the Mac Mini and the night 3 h 35 min, and the hosted Linux test step ran about 2.3 times slower (F-109).
- One job plans the seeds once, and each sweep job gets the date and the record of `main` from it. Each sweep job keeps its logs, its summary line, and its failure line as artifacts. One last job writes and publishes the record, and it re-runs the gates.
- Keep the carry rules of D-567 and D-569, and the failure record script of a broken build.
- Run a branch night of PR-85 on hosted Linux before the review.

Then PR-86: the macOS legs of `ci.yml`, `smoke.yml`, and `bit-identity.yml` move to the hosted macOS arm64 runner. The Free plan runs 5 macOS jobs at once. PR-86 retires `docs/runbooks/macos-runner.md` and the Mac runner rules. It revises D-100, D-157, D-192, and D-358, and it updates the cost model and the agent files. PR-75 follows.

## Session 242: 2026-09-24, Codex

Author: Codex
Session: PR-84, reviewer. Branch `feat/pr-84-night-fixed-seeds`. PR #100, pending merge. Base `55b6d3f`.

### What this session did, and why

- Reviewed PR #100 at effective head `198a0c9` under D-564 to D-569.
- Found that the shell failure record drops carried seeds when the night does not write `night.json`. This can let a later night clear a failure without rerunning its seeds.
- Added `docs/reviews/pr-100.md` with one P1 finding and the verdict `Changes required`.

### State of the build

- Local at `35da819`: build passed, and the full suite passed 1611 of 1611 tests, Smoke included. `ste-check`, `det-lint`, and `asset-qa` reported 0 findings.
- CI, bit identity, and Smoke passed at `2135f46`. The latest document checks passed at `35da819`; the code jobs skipped under D-475.
- The branch night run 36030984588 at `35da819` is still in progress. The pre-review remote head was `35da819`.
- The review record and this entry are published to the PR branch, and `gh pr view` confirms the remote head.

### In flight

- PR #100 needs the author to fix P1-1 and run its regression check.
- The branch night for exit test 10 still needs to finish.

### Traps and gotchas

- A shell-written failure record has no seed fields when the build or plan step fails. Keep the failed seeds from `night-results` in that record (D-567, D-569).
- A later metadata push does not change the reviewed code head, but a new code or workflow commit needs review.

### Open questions that block progress

None.

### Next concrete action

The author loads `review-response`, fixes P1-1, adds the regression test, and reruns the branch night. Then the review checks the fix at its new effective head.

## Session 241: 2026-09-24, Claude Code

Author: Claude Code
Session: PR-84, author. Branch `feat/pr-84-night-fixed-seeds`. PR #100, pending merge. Base `55b6d3f`.

### What this session did, and why

- Exit test 6 of PR-83 passes. `night-promote.yml` run 36005414678 at `55b6d3f` ended green with `branch-absent`, and it wrote nothing. The first night on `main` after it, run 36005945062, passed at 16:47 UTC. Its `night-publish-check` gave `write`, it pushed `4ac067b` with a lease, and it re-ran the gate of PR #100.
- The owner answered OQ-190 to OQ-194: D-564 to D-569. The fixed set stays the gate, and a slice of one tenth runs each night (D-564, D-566). A slice failure blocks (D-565), against the recommendation. From that answer came the carry (D-567) and the promotion check (D-569). The id is PR-84 (D-568).
- Built `NightSeeds`, `night-seeds`, the seed list of `bot-run`, the seed fields of `night-record`, the case `carry-missing`, and the plan step of `night.yml`.
- The night of 2026-09-24 had the cron time 08:07 UTC, started at 13:28 UTC, and held the Mac runner until 16:48 UTC. The owner then chose the cloud move: D-570 to D-573.

### State of the build

- Local: the full suite passed 1611 of 1611, Smoke included. `det-lint`, `ste-check`, and `asset-qa` read 0.
- CI at `2135f46`: every job passed on the three platforms, `night-gate` included. `evaluate` and `review-gate` wait for the review record.
- Code head and effective head: `198a0c9`. Later commits change documents alone. The remote head: the push of this entry.

### In flight

- A branch night of this PR for exit test 10, and review round 1 through `make codex-review PR=100 -- --skip-gitar-review`.
- The merge summary in Q/A form after an approved review (D-524, D-552).

### Traps and gotchas

- Day 0 of the slices is 2026-09-24. The first slice runs seeds that no night ran before, so it can fail on an old fault. A fix of such a seed is a PR of its own.
- A failed plan step leaves no seed fields. The shell failure record of the publish step then drops the carried seeds of `main`.
- `night-record` needs `--date`, `--failures`, and `--carry`.
- A documents push skips the heavy jobs only after the runs on the previous head end green (CI skip rule 2). A push while the macOS legs wait cancels them and runs the full suite again.

### Open questions that block progress

None.

### Next concrete action

This session: wait on the branch night and review round 1 of PR #100, then give the merge summary.

After PR #100 merges, the next session makes PR-85 alone (D-570, D-571, D-572, D-573). It goes ahead of PR-75 in the Phase 2 order.

- The night cron of `.github/workflows/night.yml` moves from `7 8 * * *` to `7 7 * * *`, 07:07 UTC. Update the time comment and `NightWorkflowRunsAtTwoCentralStandardTime`, and revise D-284, D-285, and D-288 in part.
- The night moves to `ubuntu-latest` as parallel jobs, one for each sweep of `NightSeeds.Sweeps`. A hosted job stops at 6 hours. The full clearer took 84 minutes on the Mac Mini, and the hosted Linux test step ran about 2.3 times slower (F-109).
- One job plans the seeds once, and each sweep job gets the date and the record of `main` from it. Each sweep job keeps its logs, its summary line, and its failure line as artifacts. One last job writes and publishes the record, and it re-runs the gates.
- Keep the carry rules of D-567 and D-569: a sweep job that did not end writes no failure line.
- Run a branch night of PR-85 on hosted Linux before the review.

Then PR-86: the macOS legs of `ci.yml`, `smoke.yml`, and `bit-identity.yml` move to the hosted macOS arm64 runner. The Free plan runs 5 macOS jobs at once. PR-86 retires `docs/runbooks/macos-runner.md` and the Mac runner rules. It revises D-100, D-157, D-192, and D-358, and it updates the cost model and the agent files. PR-75 follows.

## Session 240: 2026-09-24, Codex

Author: Codex
Session: PR-83, reviewer. Branch `feat/pr-83-night-record-promotion`. PR #99, pending merge. Base `811c1c9`.

### What this session did, and why

- Reviewed PR #99 at effective head `2b872f9`, the green branch night promotion change.
- The author is Claude Code, as session 239 records. This Codex review meets the cross-provider rule (T-4, D-101).
- Reviewed all changed paths, the PR-83 exit tests, decisions D-555 to D-563, the PR comments, the workflows, and the record rules.
- Added `docs/reviews/pr-99.md` with the verdict `Ready for owner merge`.

### State of the build

- The focused promotion, publication, and workflow tests passed 11 of 11.
- GitHub checks at `3236111` passed for builds, sweeps, asset QA, bit identity, bots, det-lint, documents, doc-gate, night-gate, smoke, and ste-check.
- `evaluate` and `review-gate` failed because the review record did not exist at that head. They must rerun after this metadata push.
- The first metadata push `d7f90c5` matched the PR head in `gh pr view`. This entry records that publication check before its final evidence update.

### In flight

- The review record and this handoff entry share one amended metadata commit on `feat/pr-83-night-record-promotion`.
- The review verdict applies to effective head `2b872f9`.

### Traps and gotchas

- The current PR tip `3236111` changes only skipped documentation paths after effective head `2b872f9`.
- The only PR comment is the gitar notice “Gitar is working”. D-550 says that notice needs no answer.

### Open questions that block progress

None.

### Next concrete action

The final amended metadata commit is pushed, and its hash matches the PR head.

## Session 239: 2026-09-24, Claude Code

Author: Claude Code
Session: PR-83, author. Branch `feat/pr-83-night-record-promotion`. PR #99, pending merge. Base `811c1c9`.

### What this session did, and why

- The owner asked that a green branch night count for `main` after the merge of its PR. PR #97 merged with a green branch night at `4282206`, and PR #98 still read the red night of `e069e16`.
- The owner answered OQ-183 to OQ-189, and each recommendation stood: D-555 to D-563. The PR takes the id PR-83, before the fixed seeds (D-560). The PR-81 night gets no promotion (D-561).
- Added the tool `night-promote` and the workflow `night-promote.yml`. A push to `main` promotes the branch night of the merged PR when the trees differ only in the skip set (D-555 to D-558).
- Added `night-publish-check` to `night.yml`. A night on `main` keeps a record at a later commit (D-562). Each write of `night-results` takes a lease.
- Added the action `rerun-night-gates`. A promotion and a night on `main` re-run the gate of each open PR (D-559).
- PR #98 merged first as `811c1c9`. This branch rebased onto it. The PR-82 line on the fixed seeds now names the PR after PR-83 (D-560), and this entry took session 239 (D-187).

### State of the build

- Local before the rebase: the full suite passed 1596 of 1596, Smoke included. `det-lint` and `ste-check` read 0. `doc-gate` passed.
- CI before the rebase, at `af39f08`: every build, test, smoke, bit-identity, bots, lint, and document job passed on the three platforms.
- The night on `main` at `2071cb6` passed and ended at 2026-09-24T05:06:18Z (run 35944586534). A re-run of the `night-gate` job of this PR read it green. That record stays inside 48 hours until 2026-09-26T05:06Z, so this PR needs no branch night.
- A read-only dry run of `night-promote` at `2071cb6` gave `promote`. It wrote nothing (D-561).
- CI at `3236111`, after the rebase: every build, test, smoke, bit-identity, bots, lint, document, and `night-gate` job passed on the three platforms.
- The review through `make codex-review PR=99 -- --skip-gitar-review` approved the effective head `2b872f9` with no finding (`docs/reviews/pr-99.md`).
- The remote head: the push of this entry. `origin/main` is `811c1c9`.

### In flight

- The merge confirmation of the owner, after the merge summary in Q/A form (D-524, D-552).
- A first review run started at `af39f08` and stopped on the owner request before it wrote anything. The owner asked for the rebase first, then the review after CI.

### Traps and gotchas

- `night-promote.yml` runs first on the merge commit of this PR. This PR has no branch night, so that run ends with `branch-absent` and writes nothing. Its first promotion comes with a later PR. Exit test 6 reads both runs.
- The night checkout now takes the full history (`fetch-depth: 0`) for the order check of D-562.
- The only gitar comment is a plan notice with no item (D-550).

### Open questions that block progress

None.

### Next concrete action

After the merge, read the run of `night-promote.yml` at the merge commit for exit test 6. It names `branch-absent` and writes nothing, because this PR had no branch night. The next PR is the fixed seeds of the night (D-560).

## Session 238: 2026-09-24, Claude Code

Author: Claude Code
Session: PR-82, author. Branch `chore/pr-82-template-gitar-notice`. PR #98, pending merge. Base `2071cb6`.

### What this session did, and why

- Ran a night by hand on `main`, run 35944586534 at `2071cb6`. It passed and ended at 05:06 UTC. The record of `main` is now a success at `2071cb6`, and it replaced the red night of `e069e16`.
- A night on `main` does not re-run the `night-gate` of an open PR. The re-run step of `night.yml` skips `main` (D-548). The session re-ran the `night-gate` run of this PR by hand, and it passed.
- Review round 1 read `Blocked` with no finding: `night-gate` was red, and CI was pending. Review round 2 approves the effective head `bc51dde`.
- The owner asked for a prompt of a parallel PR. A green branch night then counts for `main` after the merge, when the merge commit differs from the tested commit in skip-set paths alone. The prompt went to the owner in chat. That PR has no D-# or OQ-# yet.

### State of the build

- All required checks are green at the PR tip, `night-gate` and smoke on three platforms included.
- Code head: `bc51dde`. Later commits of this PR change documents only.

### In flight

- The owner merge confirmation, then the auto-merge.

### Traps and gotchas

- The Linux smoke failure in the round 1 record came from a run in progress. The final run passed.
- After each red night on `main`, each open PR needs a re-run of its `night-gate` by hand, until a PR changes that rule.
- The parallel PR can collide with this PR on D-# ids and session numbers. This PR holds D-553, D-554, and sessions 235 to 238.

### Open questions that block progress

None.

### Next concrete action

After the merge: the fixed seeds of the night (D-551), or the parallel PR of the night record promotion, as the owner orders them. Follow the next concrete action of Session 232 for the fixed seeds.

## Session 237: 2026-09-24, Codex

Author: Codex
Session: PR-98, reviewer. Branch `chore/pr-82-template-gitar-notice`. PR #98, Ready for owner merge. Base `2071cb6`.

### What this session did, and why

- Re-reviewed PR #98 at effective head `bc51dde` after the night and required product checks passed.
- Updated `docs/reviews/pr-98.md` and kept the earlier Blocked verdict in its history.
- Found no in-scope defect in the template line or its equality test.

### State of the build

- The Documents tests passed 141 of 141. `ste-check` and local `doc-gate` passed.
- GitHub at `804ec44` showed all product checks, including `night-gate`, bit identity, sweeps, and smoke, as passed.
- `evaluate` and `review-gate` still read the prior Blocked record. The remote head at review start was `804ec44`.

### In flight

- PR #98 awaits the updated `review-gate` and `evaluate` results. They must read this review record.

### Traps and gotchas

- The Gitar comment says only “Gitar is working.” D-550 says that notice needs no answer.
- D-542 pauses the Gitar wait. The review record and this entry form one metadata commit.

### Open questions that block progress

None.

### Next concrete action

The author gives the merge summary in questions and answers, then asks the owner to confirm the merge (D-552).

## Session 236: 2026-09-23, Codex

Author: Codex
Session: PR-98, reviewer. Branch `chore/pr-82-template-gitar-notice`. PR #98, blocked. Base `2071cb6`.

### What this session did, and why

- Reviewed PR #98 at effective head `bc51dde`. The template line and its equality test meet the D-550 contract.
- Added `docs/reviews/pr-98.md` with the review evidence and verdict.
- The review found no in-scope defect. Required CI evidence blocks approval.

### State of the build

- The focused template test passed, and the Documents tests passed 141 of 141. `ste-check` and local `doc-gate` passed.
- GitHub at `d158af7` had a failed `night-gate`. The base night record failed, and the branch-night record was absent.
- GitHub showed Linux smoke as failed, but its run was still in progress and its failure log was unavailable. Required platform jobs, sweeps, bots, and macOS smoke remained pending.
- The remote head at hand-over is `d158af7`. Later PR commits change documents only, so the effective head remains `bc51dde`.

### In flight

- PR #98 remains blocked until the required night and CI evidence passes.

### Traps and gotchas

- The Gitar comment is a free-plan notice with no claim. D-550 says to ignore it.
- The review record and this entry form one metadata commit. The commit does not change the effective head.

### Open questions that block progress

None.

### Next concrete action

Resolve the night-gate and remaining CI results, then request a fresh review if the effective head changes.
