# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 262: 2026-09-25, Codex

Author: Codex
Session: PR-86, reviewer, round 2. Branch `feat/pr-86-hosted-macos`. PR #105, Ready for owner merge. Base `0d99e2c`.

### What this session did, and why

- Re-reviewed PR #105 after the author answered P2-1.
- Confirmed that the corrected roadmap text passes the finding’s regression check. Updated the existing review record and kept the earlier verdict.
- The review record and this entry form one metadata commit (D-182).

### State of the build

- Effective head `1678ef5`; PR tip `817dad3`. The current document checks and Gitar pass. Code jobs skip after the documents-only change.
- `evaluate` and `review-gate` still read the earlier review record. Recheck them after this metadata commit reaches the PR.

### In flight

- No review work remains. The owner gives the merge summary and confirms the merge (D-533, D-552).

### Traps and gotchas

- The correction changes documents alone. The earlier workflow checks still cover the same implementation head.
- PR #105 has one Gitar approval summary with no specific item, and no review threads.

### Open questions that block progress

None for PR-86.

### Next concrete action

The owner reads the merge summary and confirms whether to merge PR #105.

## Session 261: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-86, author, the answer to round 1. Branch `feat/pr-86-hosted-macos`. PR #105, pending merge. Base `0d99e2c`. Session 259 holds the earlier work of this session.

### What this session did, and why

- Round 1 gave `Changes required` with P2-1. It had full merit: the plain-English paragraph of the PR-86 entry said that three checks still run on the Mac of the owner. The paragraph now puts that state before the change. `docs/reviews/pr-105-response.md` records the answer.
- The owner asked why the review waits for the night of PR-85 exit test 8, and then started the review. That night is a record for PR-85, and no part of PR-86 depends on it.

### State of the build

- Effective head `1678ef5`. The correction changes documents alone (D-475, D-534). The gitar pass approved `d063bae` with no finding.

### In flight

- Round 2 of the cross-provider review, and the owner merge decision (D-533, D-552).
- PR-85 exit test 8: the night of 2026-09-26 from the 07:07 UTC cron has not started. A later session reads its start time, the wall time of each sweep job, its result, and its slice (6001-6500 for each bot policy, 120001-130000 for reachability).

### Traps and gotchas

- Session 259 lists the traps of this PR.

### Open questions that block progress

None for PR-86.

### Next concrete action

Complete the gitar pass of the new head, then run `make codex-review PR=105`.

## Session 260: 2026-09-25, Codex

Author: Codex
Session: PR-86, reviewer. Branch `feat/pr-86-hosted-macos`. PR #105, Changes required. Base `0d99e2c`.

### What this session did, and why

- Reviewed the full change and its exit tests for PR-86.
- Found P2-1: the plain-English roadmap summary says the checks still run on the owner’s Mac.
- Committed the review record with this handoff entry as one metadata commit (D-182).

### State of the build

- PR head `d063bae`. Effective head `1678ef5`. CI passed on `6dfe7f8`; later document-only runs passed their applicable checks.
- The full local suite passed 1820 of 1820 tests. The runner API returned 0, and `launchctl` showed no runner agent.
- The review record names `1678ef5` and requires correction of P2-1.

### In flight

- The author must answer P2-1 and request a new review round.

### Traps and gotchas

- The latest CI runs skip heavy jobs after document-only changes. The `6dfe7f8` run contains the passing hosted macOS legs and bit-identity comparison.
- Gitar posted an approval summary with no specific item to address (D-550).

### Open questions that block progress

None for PR-86.

### Next concrete action

Correct the roadmap summary, then request a new review round.

## Session 259: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-86, author. Branch `feat/pr-86-hosted-macos`. PR #105, pending merge. Base `0d99e2c`.

### What this session did, and why

- PR-85 exit test 8: PR #102 merged at 18:25 UTC on 2026-09-25, after the 07:07 cron of that day. The first scheduled night from the `7 7 * * *` cron is 2026-09-26, and it has not started. The scheduled run 36141884980 at 13:35 UTC came from the old cron on `main`.
- The owner chose the floating label `macos-latest` (D-583), and the removal of the runner in this PR (D-584). The re-run rule of D-358 now covers each runner fault of any job (D-585).
- The three macOS legs run on `macos-latest`. Each one first fails on a machine that is not arm64. Two new shape tests fail on the workflows of `main`.
- The runbook, the design doc, the roadmap, and the registers retire the runner. D-572 supersedes D-157, and D-584 supersedes D-192.
- After the hosted legs passed, the session stopped and uninstalled the launch agent, and removed the registration. The repository has 0 runners, and `launchctl list` holds no runner agent (exit test 6).

### State of the build

- Effective head `1678ef5`, the code commit, because each later commit changes documents alone (D-534). Every check of `6dfe7f8` passes but `evaluate` and `review-gate`, which wait for the review record (D-251).
- The hosted macOS legs ran on the image `macos-26-arm64` 20260907.0351, and each log shows "runs on arm64". Wall times: `ci-macos-arm64` 11 min 29 s, `smoke-macos-arm64` 42 s, `bit-identity-macos-arm64` 25 s. The three platforms agree on `9c79047da9c82a0e`.
- The local suite passed 1820 of 1820, Smoke included. The gitar pass approved `6dfe7f8` with no finding.

### In flight

- The cross-provider review, and the owner merge decision (D-533, D-552).

### Traps and gotchas

- A citation of D-157 or D-192 now needs its superseder on the same line (D-178).
- The merge of PR #104 dropped the title line and the rule line of this file. `doc-gate` reads the first entry after a line break, so it read session 257 as the newest. This PR puts both lines back.
- The macOS shell has no `timeout` command, so a wait wrapped in it ends at once.
- The Full Disk Access grant of `/bin/bash` and of the runner `node` stays on the Mac Mini until the owner removes it in System Settings.

### Open questions that block progress

None for PR-86.

### Next concrete action

Read the night of 2026-09-26 for PR-85 exit test 8, then run `make codex-review PR=105`.

## Session 258: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-88, author. Branch `fix/pr-88-review-fixes`. PR #104, pending merge. Base `4e9b59d`.

### What this session did, and why

- Round 2 gave `Changes required` with P1-3 and P2-1. Both had full merit: a backtick in a fence info string opens no fence, and the string scan joins the const names of every Game file. Commit `d027fa8` holds both, with tests that fail on the old code.
- PR #102 merged into `main`, and the owner asked for a merge from `main`. Commit `5e43e89` merges it. The conflicts were the D-284 and D-285 rows, the handoff, and the archive.
- Both branches used the session numbers 249 and 251. The entries of this branch took 253 to 256, and the archive holds the union of both sides.
- Round 3 approved the effective head `5e43e89`. The first record named `d027fa8`, and the review itself corrected the head in `a066510` before the session stopped its process.
- The repository review report marks each finding that this PR fixed, in full or in part, as complete in PR #104.

### State of the build

- Remote head `a066510`. Effective head `5e43e89`. Every check passes, `review-gate` included.
- The full suite passed 1816 of 1816 on the merged tree, Smoke included. The known answer is `9c79047da9c82a0e`.

### In flight

- The owner merge decision after the merge summary (D-533, D-552).

### Traps and gotchas

- A review round can go quiet for minutes after its push and then push a last record commit. Read the record on the branch before a stop.

### Open questions that block progress

None for PR-88. OQ-195 to OQ-205 block other work.

### Next concrete action

When the owner confirms, run `gh pr merge 104 --auto --squash`, wait on the checks, and write the prompt of `merge-prompt.md` at the merge.

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
- The first metadata head `7d37b76` had a stale review head. The corrected metadata head `597d41d` passed `evaluate`, `review-gate`, Gitar, doc-gate, Documents, STE, and all other applicable checks.
- Heavy code checks skipped at `597d41d` under the documents-only rule. Required code checks passed at effective head `5e43e89` (D-357, D-475).
- The review record and this handoff publish in one metadata commit (D-182).
- The latest push and PR head were verified with `gh pr view`.

### In flight

- The review is ready for owner merge at effective head `5e43e89`.
- No review correction remains in flight.

### Traps and gotchas

- PR head `5e43e89` merges the updated base `4e9b59d`. Its effective-head diff from the base is the already reviewed F-134 test in `RepositoryShapeTests`.
- The local regression run used the Debug configuration and passed all 73 selected tests.
- The first Documents run found the handoff archive out of rotation. `handoff-rotate` moved Session 244, then all 192 Documents tests passed.
- The earlier Gitar claims remain resolved. Its current dashboard reported only the stale review-gate verdict.

### Open questions that block progress

None for PR-88. OQ-195 to OQ-205 do not block this review.

### Next concrete action

Give the owner the review verdict and the merge evidence for PR #104.

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
