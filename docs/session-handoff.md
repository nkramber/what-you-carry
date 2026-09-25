# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 250: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-88, correction author. Branch `fix/pr-88-review-fixes`. PR #104, pending merge. Base `a3590ba`.

### What this session did, and why

- Round 1 of the cross-provider review gave `Changes required` at `4029e36` with P1-1 and P1-2. Both had full merit, and `docs/reviews/pr-104-response.md` holds the evidence.
- P1-1: a finding status now reads its complete form, so `fixed.` with no revision is a fault (F-125, D-514).
- P1-2: only a fence of the same character and at least the same length closes a fenced block (F-116).
- Each new test failed on the parsers of `4029e36`.
- Gitar approved `35c389c` with one finding, the numbers of the sequence tail and the exit tests. Commit `35c389c` fixed it, and the thread is resolved.
- The owner added five small concerns while the Mac runner ran the night: F-129, the ramp march test, and F-131, the band at load, a rotated locator, an empty string, and the pose at a keyframe. Each new test failed on the old code or on a mutant of the march.
- The owner then added nine findings of the review while the night held the Mac runner: F-132 to F-140, with F-141 to F-146 for the six merges past a failed gate. The owner revised D-208 in D-582: a Core method entry names its parameter types.
- Gitar raised a fourth finding at `d4b8d01`: the blend between two keyframes near the float limit overflowed. Commit `9dcd440` weights each keyframe.
- Gitar approved `4c55f16` with a second finding: the fence parse took any indent, and Markdown takes three spaces at most. It had full merit. The fence now opens after no more than three spaces, and `ReviewGateReadsNoFenceAfterFourSpaces` fails on the parser of `f280b72`.

### State of the build

- The known answer is `9c79047da9c82a0e`: the sweep now folds one real floor and two recorded runs. It moves with the content numbers too.
- The full suite passed 1749 of 1749, Smoke included. The review tests passed 176 of 176.
- Smoke and Bit identity skip on a documents head. Their runs on the code came from a re-run on `7207164`, and each passed on the three platforms.

### In flight

- The gitar pass and CI of the new head, then round 2 of `make codex-review`.

### Traps and gotchas

- PR #102 and this PR both add a marker to the D-284 row and an entry before `### PR-75`. The PR that merges second joins them by hand.
- OQ-67 still rests on the Mac as the CI runner, which D-572 ends.
- A push of documents right after a code push cancels the Smoke and Bit identity runs of the code (D-356), and the later heads skip them. Re-run the cancelled runs before the review.

### Open questions that block progress

None for PR-88.

### Next concrete action

After the gitar pass and green CI, run `make codex-review PR=104` for round 2.

## Session 249: 2026-09-25, Codex

Author: Codex
Session: PR-88, reviewer. Branch `fix/pr-88-review-fixes`. PR #104, Changes required. Base `a3590ba`.

### What this session did, and why

- Reviewed PR #104 at effective head `4029e36` to check the fixes of F-113 to F-127.
- Added two P1 findings for review gates that accept malformed closed finding statuses or fake verdict sections inside valid code fences.
- Read the PR comments. The Gitar thread about the roadmap order is resolved.

### State of the build

- The focused review-gate tests passed 129 of 129.
- The Documents tests passed 181 of 181. `ste-check` found 0 issues, and the final `doc-gate` passed over 78 paths.
- CI platform jobs, sweeps, documents, asset QA, determinism lint, STE, doc-gate, night-gate, and Gitar passed at `35c389c`.
- The first review metadata push was `da8c2c6`. Its Gitar wait passed, and `gh pr view` confirmed that remote head.
- The CI, Smoke, and Bit identity jobs skipped the metadata-only head. The previous code head had passing CI, Smoke, and Bit identity results. `review-gate` and `evaluate` fail because the verdict requires changes.
- Smoke and bit-identity passed at `756d539`. Later commits changed paths in the skip set of D-475.
- The PR tip at review start was `35c389c`. Its effective head remains `4029e36`.

### In flight

- The author must correct the two findings before this PR can pass the review gate.

### Traps and gotchas

- This checkout is detached. The review record and session handoff were pushed to `origin/fix/pr-88-review-fixes`.
- Review the effective head `4029e36`, not the later document commits.

### Open questions that block progress

OQ-195 to OQ-205 remain open. D-581 records the accepted runner risk. These questions do not block this review.

### Next concrete action

The author corrects P1-1 and P1-2, then starts a fresh cross-provider review of the new effective head.

## Session 248: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-88, author. Branch `fix/pr-88-review-fixes`. PR #104, pending merge. Base `a3590ba`.

### What this session did, and why

- The owner asked for the fixes of the repository review of 2026-09-24, with more than one concern in one PR (D-580). The id and the order are PR-88, before PR-86 (D-578).
- Each finding was reproduced or traced at `a3590ba` before a change. Each fix has a test that fails on the old code:
  - F-113 and F-114: a death at the stairwell stays a death, and a descend on the deepest floor does nothing (D-322, D-579).
  - F-115: each engine callback catches every exception and quits with exit code 1.
  - F-116: `review-gate` reads the verdict from the first line of its section.
  - F-117 to F-119: the seed sweep, the repeated JSON key, and the depth of the asset gate.
  - F-120 to F-127: content bounds, the error context of a run, the session end, the follower hash, the test claims, the gate tool edge cases, the runbook temporary files, and the Core tables.
- The owner skipped the interim fork approval change of the review (D-581).
- OQ-195 to OQ-205 hold the owner choices of the review that no register settles.
- F-128 to F-130 record three defects that this work found and did not fix.

### State of the build

- The simulation version is 17. The known answer is `f1c35ddccb2cd0bb`. The version moved it first, and the hash of each follower moved it again.
- The local checks and their results are in the PR description. The PR head and the remote head come from `gh pr view`.

### In flight

- CI, the gitar pass, and the cross-provider review of PR-88.

### Traps and gotchas

- Worker sessions in `.claude/worktrees/` did part of the work. `ste-check` reads a worktree under the checkout, so remove each one before the check.
- A file that a backup restores keeps its old time, and an incremental build then skips it. Build with `--no-incremental` after such a restore.
- PR #102 (PR-85) edits the same registers, section 7 of the design doc, and the Phase 2 order list. The PR that merges second resolves the conflict.
- A review record needs `**<verdict>.**` at the start of the first line of its Verdict section.

### Open questions that block progress

None for PR-88. OQ-195 to OQ-205 block other work.

### Next concrete action

Wait for CI and the gitar pass, then run `make codex-review` for this PR. The PR description holds the disposition of each finding of the review.

## Session 247: 2026-09-25, Codex

Author: Codex
Session: PR-87, reviewer. Branch `chore/pr-87-gitar-reenable`. PR #103, pending merge. Base `a68348b`.

### What this session did, and why

- Reviewed the full change and its exit tests for PR-87.
- The cross-provider review found no actionable finding. The verdict is `Ready for owner merge` at effective head `f6292c0`.
- Committed the review record with this handoff entry as one metadata commit (D-182).

### State of the build

- PR head: `ef8536b`. Effective head: `f6292c0`. CI, smoke, asset QA, STE, det-lint, night gate, doc gate, and Gitar passed. The review gate awaited the review record.
- Focused local tests passed 11 of 11. The script syntax and diff checks passed.
- The metadata commit was pushed to `origin/chore/pr-87-gitar-reenable` and verified with `gh pr view`.

### In flight

- The owner merge decision.

### Traps and gotchas

- Gitar approved the head and found no issue. Its other comment names only the missing review record, which this commit supplies.
- The effective head skips later document changes under D-534.

### Open questions that block progress

None.

### Next concrete action

Give the owner the merge summary in questions and answers (D-533, D-552).

## Session 246: 2026-09-24, Claude Code

Author: Claude Code
Session: PR-87, author. Branch `chore/pr-87-gitar-reenable`. PR #103, pending merge. Base `a68348b`.

### What this session did, and why

- The owner ended the gitar pause of D-542: "It is time to re-enable and re-require gitar code reviews". The automated pass is a gate again (D-574). The alert rule ended too.
- Added `make gitar-wait PR=<n>` and `.github/scripts/gitar-wait.sh` (D-575). The wait is 60 seconds, then a read of the Gitar check runs every 30 seconds. With no check run at 6 minutes it posts one `Gitar review` comment. At 15 minutes it exits 1.
- Removed each pause text of D-542 from the agent files, the PR template, three skills, and the runbook. `NoInstructionTextHoldsTheGitarPause` holds that.
- The first live wait on PR #103 ended too early. The check run of the head `36e4d23` completed at 05:07:56 UTC, and the gitar dashboard came at 05:08:49 UTC. The wait now also needs a dashboard edit after the run started, as the start checks of `codex-review` do. `ACompletedCheckRunWithNoNewDashboardIsNoReview` fails on the first form.
- Gitar approved `36e4d23` at 05:08:55 UTC with no finding. Its summary names PR-85 as the source of the pause, which was PR-80. A summary is no finding (D-550).
- The session started `make codex-review` after the gitar pass while five CI jobs ran. The owner forbade that (D-577). The session stopped the round, removed its worktree, and confirmed that no record reached the branch.
- D-576: PR-87 goes after PR-85 and before PR-86. PR-85 is open as PR #102 in another session, which uses session number 245.

### State of the build

- Full suite after the fix of the end condition: 1622 of 1622 passed in 8 minutes 22 seconds. The first run of the session failed only on the entry count before `handoff-rotate`.
- `ste-check` found 0 issues.
- A run of the script against PR #102 read its completed Gitar check run and exited 0.

### In flight

- Gitar approved `ef8536b` with no finding. `make gitar-wait` ended on its dashboard edit after 62 seconds.
- CI run 36098578048 passed on `ef8536b`. Only the Review gate workflow was red before the record.
- `make codex-review PR=103` approved the effective head `f6292c0` with no finding (`docs/reviews/pr-103.md`). The record commit is `64119bb`.
- The owner decides the merge after the merge summary (D-524, D-552).

### Traps and gotchas

- PR #102 edits the same order list of the Phase 2 roadmap and section 7 of the design doc. The PR that merges second resolves the conflict.
- The check run of `e6abe26`, the first commit of PR #103, still read "Working" after the approval. The wait reads the head alone for that reason.
- The first read of the wait is at 60 seconds. The Gitar check of PR #102 ran 6 minutes 11 seconds, so a normal pass ends inside the limit of 15 minutes.
- The old push wait of the skill cited D-160 of another repository. D-160 of this repository is the state hash.

### Open questions that block progress

None.

### Next concrete action

When the owner confirms, run `gh pr merge 103 --auto --squash`, and write the prompt of `merge-prompt.md` at the merge. PR-85 (PR #102) then carries the gitar wait and D-577 too. The next PR after both is PR-86.

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
