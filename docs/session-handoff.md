# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 285: 2026-09-27, Codex

Author: Codex
Session: PR-91, reviewer. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Completed review round 5 at effective head `db8b3c8`. P2-2 and P2-3 are fixed, and their focused tests pass.
- Reviewed the mesh wrapper correction F-177 and the PR comments. Updated `docs/reviews/pr-109.md` to `Ready for owner merge`.
- The 20 focused `NightFixerTests` passed. Two hundred headless smoke sessions under eight busy processes also passed.

### State of the build

- Remote metadata head: `e97c381`. The reviewed code head is `db8b3c8`. Fresh checks passed for Gitar, evaluate, review-gate, asset-qa, det-lint, ste-check, documents, doc-gate, and night-gate. CI, Smoke, bit identity, and bot jobs skipped under the documents-only rule.

### In flight

- A metadata update records the push result. Fresh document checks and the Gitar wait follow that push.

### Traps and gotchas

- The worktree is detached. Compare its pushed commit with the PR head; the local status has no branch or ahead count.
- The reviewer does not answer comments or merge the PR.

### Open questions that block progress

None for PR-109.

### Next concrete action

Push the metadata update, wait for fresh checks and Gitar, then give the owner the merge summary and request merge confirmation.

## Session 284: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-91, author. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- The gitar pass of `c505fa9` found that the fixer marked a night handled before its setup. `d32da6b` fixed it, and the thread is resolved.
- Review round 4 (session 283) gave `Changes required` at `d32da6b` with P2-2 (a queued notice counted as delivered) and P2-3 (a race in the lock). `09c8695` fixes both, and `docs/reviews/pr-109-response.md` records them.
- The suite of that correction found F-177, an engine crash at exit from leaked mesh wrappers. Under load, the PR head crashed in 3 of 100 smoke sessions, `main` in 0 of 100. The owner chose the fix of the cause, and 200 sessions then gave 0 crashes.
- The launchd job now copies the folder `.github/scripts` of `origin/main`, so the helper `notify-owner.sh` lies beside the poll. The job is reloaded on this Mac.

### State of the build

- Remote `main` is `4a20b82`. The code head is `09c8695`. The full suite passed 1937 of 1937 at `09c8695` on macOS.

### In flight

- The push, the gitar pass, CI, and review round 5.

### Traps and gotchas

- The auto mode of the harness came back on by itself several times and blocked the fixer work. The owner switched it off each time.
- F-177 shows under load alone: 0 of 70 plain runs crashed. A stress loop with eight `yes` processes gives the rate.

### Open questions that block progress

None for PR-91.

### Next concrete action

Push, finish the gitar pass and CI, run review round 5, then ask the owner to confirm the merge.

## Session 283: 2026-09-27, Codex

Author: Codex
Session: PR-91, reviewer. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Review round 4 checked the effective head `d32da6b` and the new night fixer, its notice path, tests, and runbooks.
- The setup failure finding from Gitar is fixed. Two findings remain: the poll can count a notice dispatch as delivery, and concurrent polls can remove a lock before its PID exists.
- The focused night fixer tests passed 15 of 15 on macOS.

### State of the build

- Remote `main` is `4a20b82`. The effective head is `d32da6b`. GitHub CI, Smoke, bit identity, bots, asset QA, document gates, and Gitar passed. `evaluate` and `review-gate` fail because the review record requires changes.

### In flight

- The review record and this handoff entry are ready for one metadata commit and push to `feat/pr-91-review-fixes`.

### Traps and gotchas

- `gh workflow run notify.yml` starts a workflow. It does not confirm that Pushover sent the notice.
- A lock directory without a PID can belong to a poll that has not finished startup.

### Open questions that block progress

None for this review.

### Next concrete action

Correct P2-2 and P2-3, then request another review round.

## Session 282: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-91, author. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- The owner asked for a Pushover notice of a failed night (D-642) and a night fixer on the Mac Mini (D-643 to D-645). Both joined PR #109.
- The repository secrets `PUSHOVER_USER_KEY` and `PUSHOVER_API_TOKEN` hold the keys, and a test send returned HTTP 200.
- The auto mode of the harness blocked the fixer build as an unsafe agent. The owner added allow rules to `.claude/settings.local.json` and moved the session to manual mode, then approved each step.
- This session installed the launchd job `com.whatyoucarry.night-fixer` on the Mac Mini. It runs the poll of `origin/main`, so it does nothing before the merge. A dry run against GitHub read the newest night on `main` as passed.

### State of the build

- Remote `main` is `4a20b82`. The code head is `fa435b2`. The full suite passed 1931 of 1931 at `fa435b2` on macOS.
- Review round 3 approved `69c946a`. The notice and the fixer moved the effective head, so review round 4 is due.

### In flight

- The push of `fa435b2`, the gitar pass, CI, and review round 4.

### Traps and gotchas

- `gh workflow run notify.yml` works only after the merge, because a dispatch needs the workflow on `main`.
- The night fixer runs `claude -p` with no permission prompts under the account of the owner (D-643). `docs/runbooks/night-fixer.md` holds the stop and the removal.

### Open questions that block progress

None for PR-91.

### Next concrete action

Push, finish the gitar pass and CI, run review round 4, then ask the owner to confirm the merge.

## Session 281: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-91, author, merge. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Review round 2 gave `Blocked` at `69c946a` with P2-1 fixed: a CI note of the Gitar dashboard had no author reply (D-250). Comments 5852158126 and 5852184443 answer both notes.
- Review round 3 gave `Ready for owner merge` at the effective head `69c946a`.
- The owner report `what-you-carry-repository-review.md` marks each finding of PR #109 as complete, and RR-P3-20 as no longer applicable.
- The session asks the owner to confirm the merge with the merge summary (D-533).

### State of the build

- Remote `main` is `4a20b82`. The effective head is `69c946a`. Each code check passed at `55458cc`, whose code is that of `69c946a`. Gitar approved `55458cc` with no thread.

### In flight

- The owner confirmation, then `gh pr merge 109 --auto --squash` (D-516).

### Traps and gotchas

- A Gitar CI note that names a red review gate is an item. Answer each one before a review round, or the round blocks (D-250).
- Session 276 lists the other traps of this PR.

### Open questions that block progress

None for PR-91.

### Next concrete action

After the merge, the next session continues the open findings of D-596. They wait for OQ-195, OQ-196, OQ-199 to OQ-201, a PR-31 decision, or the Deck.

## Session 280: 2026-09-26, Codex

Author: Codex
Session: PR-91, reviewer. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Re-reviewed PR #109 after the author answered the Gitar CI note.
- Confirmed that P2-1 stays fixed at `69c946a` and that Gitar approves the code.
- Updated the review record to `Ready for owner merge` for the effective head.

### State of the build

- Remote `main` is `4a20b82`. The effective code head is `69c946a`; the remote PR tip before this review commit is `4ec288a`.
- Code, Smoke, and bit-identity checks passed at `55458cc`. Current metadata checks pass except `evaluate` and `review-gate`, which read the old `Blocked` verdict.

### In flight

- The review record and this entry need one metadata commit and a push to `feat/pr-91-review-fixes`.
- The check wait and the Gitar push wait need a result after the push.

### Traps and gotchas

- The current Gitar pass approves the code. The author answered the CI note at comment 5852184443.

### Open questions that block progress

None for PR-109. OQ-195, OQ-196, and OQ-199 to OQ-201 remain open for other findings.

### Next concrete action

Publish this review and handoff, then read the checks of the new metadata tip.

## Session 279: 2026-09-26, Codex

Author: Codex
Session: PR-91, reviewer. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Re-reviewed PR #109 after the author corrected P2-1 from round 1.
- Verified that `69c946a` requires the CRC field to end the run-record header line. The focused regression test passed 2/2 cases.
- Updated `docs/reviews/pr-109.md`. P2-1 is fixed. The verdict is `Blocked` because a specific Gitar item has no author reply.

### State of the build

- Remote `main` is `4a20b82`. The effective code head is `69c946a`; the remote PR tip before this review commit is `55458cc`.
- CI, smoke, bit identity, and repository checks passed on the metadata tip. `evaluate` and `review-gate` failed because the published review record still had the round 1 verdict.

### In flight

- The review record and this entry need one metadata commit and a push to `feat/pr-91-review-fixes`.
- The Gitar pass and `evaluate` and `review-gate` need a new result after the push.

### Traps and gotchas

- The latest Gitar comment has a specific request about the stale review verdict. The reviewer records the claim but does not answer Gitar (D-250).

### Open questions that block progress

- The author must answer Gitar comment 5852038161 before the automated pass is complete.

### Next concrete action

Answer the Gitar comment, then read the new pass and gate results for PR #109.

## Session 278: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-91, author. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Review round 1 (session 277) gave `Changes required` at `6b327c9` with P2-1: the run record reader accepted a CRC field before later fields (D-637).
- Correction `69c946a` requires the CRC field to end the line. `docs/reviews/pr-109-response.md` records the disposition and the regression check.
- The gitar pass of `6b327c9` approved with no finding. One reply answered the CI note of the red review gate (D-251).

### State of the build

- Remote `main` is `4a20b82`. The code head is `69c946a`.
- At `6b327c9`, every check passed except `evaluate` and `review-gate`, which wait for an approving record.

### In flight

- The gitar pass of `69c946a`, the CI of that head, and review round 2 with `make codex-review PR=109`.

### Traps and gotchas

- Session 276 lists the traps of this PR: the smoke cache miss, Dependabot, the lock files of D-641, and the abort at exit of one macOS smoke session.

### Open questions that block progress

None for PR-91.

### Next concrete action

Push, finish the gitar pass, wait for CI, and run review round 2.

## Session 277: 2026-09-26, Codex

Author: Codex
Session: PR-91, reviewer. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- Reviewed PR #109 at effective head `6b327c9` as the cross-provider reviewer.
- Found that the run-record reader accepts a CRC field before later header fields, which leaves those fields outside the checksum (D-637).
- Recorded finding P2-1 and the verdict `Changes required` in `docs/reviews/pr-109.md`.

### State of the build

- Remote `main` is `4a20b82`. The PR code head is `6b327c9`; the review metadata commit is the head of `origin/feat/pr-91-review-fixes` and `gh pr view` confirmed it.
- Every code check on `6b327c9` passed. After the metadata push, `Gitar`, `asset-qa`, `det-lint`, `doc-gate`, `documents`, `night-gate`, and `ste-check` passed. `evaluate` and `review-gate` failed because P2-1 gives the verdict `Changes required`.

### In flight

- P2-1 needs a correction and a repeat review.

### Traps and gotchas

- `CheckHeaderCrc` checks the bytes before `headerCrc`, but does not check that the CRC is the final field.
- The existing bit-flip test covers the canonical writer order. Add a reader case with a CRC before a changed trailing field.

### Open questions that block progress

None for this review. OQ-195, OQ-196, and OQ-199 to OQ-201 concern other open findings.

### Next concrete action

Correct P2-1, add its regression test, and request a repeat review of PR #109.

## Session 276: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-91, author. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- PR-91 fixes open findings of the repository review of 2026-09-24 (D-596). The owner lifted one concern per PR for it (D-618), so it holds 26 fixes and two owner answers with no code (D-640, D-641).
- The owner answered each open choice in the session: D-618 to D-641. OQ-159, OQ-160, OQ-197, OQ-198, OQ-202 to OQ-205 are resolved.
- The owner corrected two answers after new evidence: the bot floor cap (D-625, the boss timers pass 18000 ticks) and the review time limit (D-627, a PR #104 round took about 32 minutes).
- Each regression test failed on the old behavior in a scratch worktree. A windowed run proved the close of the window (F-161).
- The findings are F-148 to F-175 in the design register.

### State of the build

- Remote `main` is `4a20b82`. The branch head before this entry is `efd48a8`.
- At `efd48a8`, the full suite passed 1915 of 1915 with the Smoke category, on macOS. `det-lint` and `asset-qa` gave 0 findings. `ste-check` gave 0 findings.
- The simulation version is 18 (D-628), and the run record format version is 2 (D-637). The bit-identity answer is `e202e84e0f5c188a` in Debug and Release on macOS.

### In flight

- PR #109 is open with the done marks. The gitar pass, the CI checks, and `make codex-review` come next.

### Traps and gotchas

- The first CI run of `smoke.yml` misses the cache, because each cache key now holds the pinned SHA-512 (D-626). Each job downloads and checks its zip one time.
- The lock files of D-638 failed CI with NU1403: the Godot packages of the Godot app and of nuget.org differ in bytes. D-641 drops them.
- Dependabot now opens a PR for each new action pin (D-636). Each such PR needs every gate of a PR.
- At `6a2d1a8`, one of 11 engine sessions of `smoke-macos-arm64` aborted at exit with code 134 (`mutex lock failed`) after a clean end line. 30 local runs passed. The close handler then moved from an override of `_Notification` to the close signal of the root window. The cause is not proven, so watch each macOS smoke run.

### Open questions that block progress

None for PR-91. The findings that stay open wait for OQ-195, OQ-196, OQ-199 to OQ-201, a PR-31 decision, or the Deck.

### Next concrete action

Finish the gitar pass of PR #109, then run `make codex-review PR=109` when each check but the Review gate workflow is green.
