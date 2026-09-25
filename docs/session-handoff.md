## Session 257: 2026-09-25, Codex

Author: Codex
Session: PR-88, reviewer. Branch `fix/pr-88-review-fixes`. PR #104, Ready for owner merge. Base `4e9b59d`.

### What this session did, and why

- Round 3 reviewed PR #104 at effective head `5e43e89` and checked P1-3 and P2-1.
- Both findings are fixed in `d027fa8`. The review record now approves this head.
- The targeted review tests passed 73 cases, and det-lint found zero issues.
- STE passed with zero findings. The Documents category passed 192 tests after handoff rotation. Doc-gate found zero problems.

### State of the build

- Required CI, Smoke, bit identity, bots, asset QA, det-lint, Documents, STE, doc-gate, and night-gate passed at PR head `5e43e89`.
- The first metadata head `7d37b76` passed Gitar. `evaluate` and `review-gate` failed because the record named `d027fa8`, not expected effective head `5e43e89`.
- The review record and this entry publish in one metadata commit (D-182).
- The metadata push and current PR head are verified with `gh pr view`.

### In flight

- The review is ready for owner merge at effective head `5e43e89`.
- The corrected review record and this handoff need one metadata commit and a push.

### Traps and gotchas

- PR head `5e43e89` merges the updated base `4e9b59d`. Its effective-head diff from the base is the already reviewed F-134 test in `RepositoryShapeTests`.
- The local regression run used the Debug configuration and passed all 73 selected tests.
- The first Documents run found the handoff archive out of rotation. `handoff-rotate` moved Session 244, then all 192 Documents tests passed.
- The earlier Gitar claims remain resolved. Its current dashboard reported only the stale review-gate verdict.

### Open questions that block progress

None for PR-88. OQ-195 to OQ-205 do not block this review.

### Next concrete action

Push the corrected metadata, wait for the checks, then verify the review gate and give the owner the review verdict.

# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 256: 2026-09-25, Codex

Author: Codex
Session: PR-88, reviewer. Branch `fix/pr-88-review-fixes`. PR #104, Changes required. Base `a3590ba`.

### What this session did, and why

- Round 2 reviewed PR #104 at effective head `d9c6413` and checked the two findings from round 1.
- P1-1 is fixed in `f280b72`. P1-2 is fixed in `0d219b1`.
- P1-3 found a fence info string that can hide a visible `Blocked` verdict from the gate.
- P2-1 found a cross-file const text that can bypass the Game string lint.
- The Gitar claims on the roadmap order, four-space fence, and keyframe blend were verified.

### State of the build

- CI, Smoke, bit identity, asset QA, det-lint, documents, STE, doc-gate, night-gate, and Gitar passed at remote head `d9c6413`.
- The local Documents category passed 190 tests. STE passed with zero findings, and doc-gate found zero problems.
- `evaluate` and `review-gate` failed because the round 1 record did not approve this head.
- The review record and this entry publish in one metadata commit (D-182).
- `make gitar-wait PR=104` passed after the metadata push. Gitar reports the expected review-gate failure because this verdict still requires changes.

### In flight

- P1-3 and P2-1 need correction and regression tests.
- The review verdict is `Changes required` for `d9c6413`.

### Traps and gotchas

- The Codex process of round 2 pushed its record and then stayed open with no output. Read the branch for the record after ten quiet minutes, and stop the process.
- The filtered local test command exited 0 but gave no runner summary. It does not count as test evidence.
- The code head has green required CI. The two findings still block approval.
- Gitar has no open code finding. Its dashboard reports the expected review-gate failure, and the author has no answer in this round.

### Open questions that block progress

None for PR-88. OQ-195 to OQ-205 do not block this review.

### Next concrete action

Correct P1-3 and P2-1. Add each regression test, then run the exact reproducer and its adjacent boundary.

## Session 255: 2026-09-25, Claude Code

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

- Round 2 gave `Changes required` at `d9c6413` with P1-3 and P2-1, and closed P1-1 and P1-2. Both new findings had full merit, and the response file holds the evidence.
- The gitar pass and CI of the new head, then round 3 of `make codex-review`.

### Traps and gotchas

- PR #102 and this PR both add a marker to the D-284 row and an entry before `### PR-75`. The PR that merges second joins them by hand.
- OQ-67 still rests on the Mac as the CI runner, which D-572 ends.
- A push of documents right after a code push cancels the Smoke and Bit identity runs of the code (D-356), and the later heads skip them. Re-run the cancelled runs before the review.

### Open questions that block progress

None for PR-88.

### Next concrete action

After the gitar pass and green CI, run `make codex-review PR=104` for round 2.

## Session 254: 2026-09-25, Codex

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

## Session 253: 2026-09-25, Claude Code

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

## Session 252: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-85, author. Branch `feat/pr-85-night-hosted-linux`. PR #102, pending merge. Base `a68348b`.

### What this session did, and why

- Read the two runs on `main` for PR-84:
  - `night-promote.yml` run 36059305679 at `a68348b` ended green with the case `code-changed`. The branch night ran at `35da819`, and the merge differs in four paths outside the skip set, `.github/` included. The gate re-run step skipped, and `night-results` still holds `4ac067b` (`55b6d3f`, success), so the run wrote nothing.
  - PR-84 exit test 11 passes. Scheduled night run 36141884980 on `main` at `a3590ba` (PR-87 merged after `a68348b`) started at 13:35 UTC, 5 h 28 min after its cron, and ended green at 17:27 UTC on the Mac. Its record names the slice of 2026-09-25: 5501-6000 for each bot policy, 110001-120000 for reachability. It wrote `night-results` and re-ran the gate of each open PR.
  - The owner chose to hold the review until that night ended (2026-09-25).
- PR-85 moves the night to 07:07 UTC on hosted Linux (D-571 to D-573). `night.yml` holds a plan job, a matrix of six sweep jobs on `NightSeeds.Sweeps`, and a record job. The record job alone holds write permissions.
- `.github/scripts/night-gather.sh` joins the sweep artifacts, and gives the status: success only when the plan, every sweep, and the record job read success.
- D-284, D-285, and D-288 carry "Revised in part by D-571". The roadmap entry of PR-85 holds nine exit tests.
- Branch night run 36062699698 at `9d5c36b` failed on infrastructure, not on a seed. The runner of the full clearer received a shutdown signal at 23:23:57 UTC, 1 h 45 min into `bot-run`. The five other sweeps passed: random walker 20 min, coward 23 min, timer tester 24 min, greedy descender 46 min, reachability 1 h 31 min. The record job wrote a failure record with no failure line of the full clearer, as the design asks.
- A local run of 200 full clearer seeds held 169 MB and 4 KB of log for each seed, so memory and disk did not cause the shutdown.
- The sweep uploads now take `overwrite: true`. Without it, a re-run of the failed jobs fails when the sweep of attempt 1 uploaded its result.
- Exit test 7 passes. Branch night run 36077051456 at `e5e164f` ended green at 04:12:22 UTC on 2026-09-25, in 3 h 52 min. Sweep jobs: random walker 20 min, coward 22 min, timer tester 31 min, greedy descender 1 h 17 min, reachability 2 h 31 min, full clearer 3 h 51 min. The record job took 29 s. The record names the slice of 2026-09-25 (5501-6000, and 110001-120000 for reachability), the deaths of all five policies, and no carried or failed seed.
- The night on `main` at `a3590ba` (Mac) and the branch night at `e5e164f` (hosted Linux) wrote the same deaths, causes, and ascends for each policy. The move keeps the results bit for bit (G-9).
- Review round 1 (Codex) read `Changes required` with P2-1: the PR held `docs/reviews/repository-review-prompts.md`, a repository audit prompt outside PR-85. Full merit. Commit `e5e164f` staged it with `git add -A` from the shared checkout, where another session had left it untracked. The file leaves the PR. The answer is `docs/reviews/pr-102-response.md`.
- The main checkout lost that file when it left this branch. Its text stays at `e5e164f:docs/reviews/repository-review-prompts.md`. The owner decides where it goes.
- Review round 2 (Codex, session 251) reads `Ready for owner merge` for effective head `69f5308`, with P2-1 fixed in `784583a`.
- The gitar pass of `69f5308` approved with no finding. Its one CI item, the missing review record, has a reply that cites D-251.
- Merged `main` at `a3590ba` into the branch. PR-87 ended the gitar pause (D-574), so exit test 9 now reads `make codex-review PR=102` with no flag, after a green gitar pass and green CI (D-575, D-577). The roadmap keeps the PR-85 entry and the PR-87 entry, in that order (D-576).

### State of the build

- CI of `69f5308` passed each check, and the metadata push `784583a` passed its checks (D-474). The remote head before this entry is `873bdb4`, the round 2 record.
- Local: the full suite passed 1614 of 1614 at each code commit, and 1624 of 1624 after the merge of `main`, Smoke included. The new shape tests fail on the old `night.yml` (7 failures). `ste-check` reads 0.

### In flight

- The merge summary in Q/A form, and the confirmation of the owner (D-524, D-552). Then `gh pr merge 102 --auto --squash` (D-516).
- CI of the tip `873bdb4` and later metadata commits: documents alone, so the heavy jobs skip after the green head (D-474).

### Traps and gotchas

- `download-artifact` with a pattern puts each artifact in a directory of its own name. The gather script reads `<results>/*/`.
- A shutdown signal on a hosted runner is an infrastructure fault. Re-run the failed jobs of the night: the plan keeps its date, and the record job runs again.
- Stage named paths alone. `git add -A` in a checkout that another session shares takes its untracked files into this PR (P2-1).
- Another session switched the main checkout to `fix/pr-88-review-fixes`. This session works in the worktree `/Volumes/SSD-1TB/wyc-pr85`.
- A sweep job creates its empty result files before the build, so a broken build still uploads a result, and the record reads "did not end".

### Open questions that block progress

None.

### Next concrete action

After the owner confirms, run `gh pr merge 102 --auto --squash`. After the merge, the next session reads PR-85 exit test 8: the first scheduled night on `main` from the 07:07 UTC cron on hosted Linux. It states the start time and the result in its handoff entry. PR-86 follows (D-576).

## Session 251: 2026-09-25, Codex

Author: Codex
Session: PR-85, reviewer. Branch `feat/pr-85-night-hosted-linux`. PR #102, Ready for owner merge. Base `a3590ba`.

### What this session did, and why

- Re-reviewed PR #102 after the author answered P2-1.
- Confirmed the unrelated audit prompt is absent from the PR tip. Updated the existing review record to approve effective head `69f5308`.
- The review record and this entry form one metadata commit (D-182).

### State of the build

- Documents tests passed 147 of 147. `ste-check` found 0 issues. `doc-gate` passed with 0 problems.
- At review start, remote code head was `69f5308`, and remote PR tip was `784583a`. Code, smoke, and bit-identity checks passed at the effective head. Document checks and Gitar passed at the PR tip.
- `evaluate` and `review-gate` were red while the review record was absent. The code, smoke, and bit-identity jobs skipped at the PR tip because later changes were documents only.

### In flight

- No review work remains. The author must give the merge summary and get the owner's confirmation.

### Traps and gotchas

- The prior finding concerned an unrelated file under `docs/reviews/`. Check the PR tip, because the effective code head predates its deletion.
- Gitar approved code head `69f5308` and reported no code finding.

### Open questions that block progress

None.

### Next concrete action

The author gives the What, How, CI, and review summary. The owner confirms the merge.

## Session 249: 2026-09-25, Codex

Author: Codex
Session: PR-85, reviewer. Branch `feat/pr-85-night-hosted-linux`. PR #102, Changes required. Base `a3590ba`.

### What this session did, and why

- Reviewed PR #102 at effective head `69f5308`.
- Found one P2 finding: the PR adds repository-wide review prompts outside its night workflow scope.
- The review record and this entry form one metadata commit (D-182).

### State of the build

- The Documents tests passed 147 of 147. `ste-check` found 0 issues. The gather script fixture checks and shell syntax check passed.
- GitHub checks passed for CI, bit identity, smoke, asset QA, night gate, doc gate, STE, det-lint, and Gitar. `evaluate` and `review-gate` await the review record.
- The remote PR head at review start was `69f5308`.

### In flight

- The author must remove the unrelated review prompt file or move it to a separate PR, then request a repeat review.

### Traps and gotchas

- Gitar approved head `69f5308` with no code finding. Its missing-record notice ends when this metadata commit reaches the PR.
- The author must use `review-response` to answer the review finding (D-381).

### Open questions that block progress

None.

### Next concrete action

The author removes the unrelated file from PR #102 and requests a repeat review.

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
