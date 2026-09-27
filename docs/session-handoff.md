# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 291: 2026-09-27, Codex

Author: Codex
Session: PR-76, reviewer. Branch `feat/pr-76-enemy-models`. PR #117, pending merge. Base `d5f7e00`.

### What this session did, and why

- Reviewed PR #117 at effective head `9a0f932`, as the cross-provider review required by T-4 and D-101.
- Checked the family model field, scavenger model and recipes, model rendering, contact sheet, content errors, and PR-76 exit tests. No in-scope finding remains.
- Added `docs/reviews/pr-117.md` with the verdict and verification record.

### State of the build

- The PR tip before this metadata commit was `e1316ca`. CI, bit identity, smoke, asset QA, determinism lint, STE, doc gate, documents, night gate, bots, and Gitar passed on that tip. The focused local test filter passed 304 tests, and local asset QA reported zero findings.
- `evaluate` and `review-gate` failed because the review record was absent before this commit. The review metadata commit does not change the effective head.
- Local STE check passed with zero findings. The Documents category passed 254 tests.

### In flight

- The review record and this handoff entry need one metadata commit and a push to `feat/pr-76-enemy-models`.

### Traps and gotchas

- The PR tip `e1316ca` is metadata after effective head `9a0f932`. The review applies to `9a0f932`.
- The local full suite did not run. The focused suite passed, and the full CI suite passed on the PR tip.

### Open questions that block progress

None.

### Next concrete action

Push the review metadata commit, then fetch and verify the PR head and remote status.

## Session 290: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-76, author. Branch `feat/pr-76-enemy-models`. PR #117, pending merge. Base `d5f7e00`.

### What this session did, and why

- The session continued the PR-76 author session in a new context, from the resume prompt in the reference folder.
- The Step 1 template of `asset-texture-creation` names no texel grid and no texel count. In concept 03, a count gave texels 4 times too big. An edit of the earlier image is the fix path (D-664).
- `content/models/scavenger.bbmodel`: the base body without the brow, the nose, and the beard. Four hood plates of 1 unit (D-666, D-674), the scarf (D-670), and the sack of 9.5 by 2.5 by 3.5 (D-675). The owner chose the sack size because the D-667 sack shared its back and side planes with the hood back plate.
- `traces/scavenger.json` traces 52 faces. The hand corrections: soot up one step (D-666), clay down one shade (D-669), the grey hood band and the background texels removed, the eyes (D-671), and the belt, knot, and ends (D-672). The torso front names umber for the trousers in the coat opening.
- Hidden faces extend miner recipes with the swap of D-507. The family file names its model (D-673). `EnemyNodes` draws each enemy with its family model, and the contact sheet shows each model.
- The owner approved the contact sheet (D-676, exit test 3).

### State of the build

- Remote `main` is `d5f7e00`. The PR head carries the code and the done marks. Locally: the build, the full suite (2066 of 2067, and the one failure was the STE check of the ignored resume prompt, clean on a worktree), `asset-qa`, `det-lint`, and the Godot build check pass.

### In flight

- CI, the gitar pass, and the cross-provider review of PR #117.
- PR-92 exit test 3: the first night on `main` after `d5f7e00` is the 07:07 UTC cron of 2026-09-28, and it can lag hours. Check that it runs eight sweep jobs and that the `night-results` record names each sweep once.

### Traps and gotchas

- The hand corrections are not idempotent. To trace a scavenger face again, delete its recipe, run `texture-trace`, and apply each correction of this entry once.
- The top view `10` has the model front at the image right, so the up face corners turn by a quarter.
- The STE check reads the ignored reference folder on a local run. The resume prompt there has findings, and CI never sees them.

### Open questions that block progress

None for PR-76.

### Next concrete action

Answer the gitar pass of PR #117, then run `make codex-review PR=117` when every check but the Review gate workflow is green.

## Session 289: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-92, author, merge. Branch `fix/pr-92-review-fixes`. PR #116, pending merge. Base `e9a89aa`.

### What this session did, and why

- The gitar pass of `0f1242e` found one edge case with full merit: the provider gate did not read the handoff archive. `37fca06` fixes it, and the thread is resolved.
- `ci-windows-x64` failed at `37fca06` on a test that split the command output on a bare newline. `a09b9f9` splits it on the newline of the platform.
- Gitar approved `a09b9f9` with no open thread. A PR comment answers its CI note with D-251.
- Review round 1 (session 288) gave `Ready for owner merge` at the effective head `a09b9f9`, with no finding.
- The session asks the owner to confirm the merge with the merge summary (D-533).

### State of the build

- Remote `main` is `e9a89aa`. The effective head is `a09b9f9`. Each check but `review-gate` and `evaluate` passed at `a09b9f9`, and those two read the record of this commit.

### In flight

- The owner confirmation, then `gh pr merge 116 --auto --squash` (D-516).

### Traps and gotchas

- A test that reads console output splits on `Environment.NewLine`, because the Windows leg writes CR LF.

### Open questions that block progress

None for PR-92.

### Next concrete action

After the merge, the next session records exit test 3: the first night on `main` runs eight sweep jobs. The frame costs of F-184 wait for M-3 and PR-77.

## Session 288: 2026-09-27, Codex

Author: Codex
Session: PR-116, reviewer. Branch `fix/pr-92-review-fixes`. PR #116, pending merge. Base `e9a89aa`.

### What this session did, and why

- Reviewed all 60 changed paths of PR #116 against its roadmap, decisions, questions, and PR comments.
- Found no blocking defect. The review record approves effective head `a09b9f9`.
- The author entries name Claude Code. The Codex review passes the provider gate (T-4, D-101).

### State of the build

- Remote `main` is `e9a89aa`. The remote PR head before this metadata commit is `a09b9f9`.
- The local build passed with zero warnings. The focused review tests passed, 426 of 426.
- CI passed the build and test jobs on all three platforms, Smoke, bit identity, bots, the night gate, and the document and asset checks. `evaluate` and `review-gate` await this metadata commit.

### In flight

- The review record and this handoff entry need one metadata commit on `fix/pr-92-review-fixes`.
- The first night on `main` after merge must run eight sweep jobs. Exit test 3 assigns its record check to a later session.

### Traps and gotchas

- The PR comment says the missing review record caused the expected pre-review gate failure. The Gitar provider-gate finding is fixed and resolved.
- The owner accepted the review trust-boundary risk (D-647) and the night-gate window risk (D-654).
- The measured frame-cost fixes remain with M-3 and PR-77 (G-17).

### Open questions that block progress

None for PR-116.

### Next concrete action

Push the review record and this entry together. Verify the remote head and wait for the metadata checks. The owner can then review the merge summary.

## Session 287: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-92, author. Branch `fix/pr-92-review-fixes`. PR #116, pending merge. Base `e9a89aa`.

### What this session did, and why

- The night fixer log printed "the night 36320653722 passed" at the first poll after the merge of PR #109. No night on `main` failed yet, so exit test 23 of PR-91 still waits for the Pushover notice and a fixer session.
- The owner report `/Volumes/SSD-1TB/what-you-carry-repository-review.md` was gone from the disk. The session rebuilt it from the full read of session 285 and the marks of PR #109, and restored it at the same path.
- The owner answered each open question of D-596: D-646 to D-659. PR-92 holds more than one concern (D-646).
- Code: the night keeps each failed seed and runs two shards of the two long sweeps (D-648, D-655). The override label reads the push time (D-653). The replay cuts a torn tail (D-656). `make claude-review` reviews a PR that Codex writes (D-649). The bot session takes `--policy`.
- The Deck found F-178: the move stick had no dead zone, and an idle player died at tick 310. D-658 fixes it. The owner read the HUD on the Deck (D-659).
- The Deck measured, two runs each with the enemies: a transition at 33.1 milliseconds, and the timer expiry at 56.5 and 56.7 (F-184).
- The session turned on Dependabot alerts, Dependabot security updates, secret scanning, and push protection (D-657).

### State of the build

- Remote `main` is `e9a89aa`. The local suite outside `Smoke` passed 2035 of 2036 tests, and the one STE failure has its fix. `ste-check` is clean.

### In flight

- The CI of PR #116, the gitar pass, then `make codex-review PR=116`.
- The first night on `main` after the merge runs eight sweep jobs (exit test 3). A later session records it.

### Traps and gotchas

- `pkill -f` over SSH matches its own command line when the pattern is in it. Run a Deck script from a file.
- GitHub makes a check suite for the last commit of each push alone, so D-653 reads the suites from the work head to the PR head.
- Each review command now refuses a PR whose author entries do not name the other provider.

### Open questions that block progress

None for PR-92.

### Next concrete action

Finish the review loop of PR #116. The frame costs of F-184 wait for M-3 and PR-77.

## Session 286: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-91, author, merge. Branch `feat/pr-91-review-fixes`. PR #109, pending merge. Base `4a20b82`.

### What this session did, and why

- The gitar pass of `50862e5` suggested a guard for the removal of a stale fixer lock. `db8b3c8` added it, and the thread is resolved.
- Review round 5 (session 285) gave `Ready for owner merge` at the effective head `db8b3c8`, with P2-1 to P2-3 fixed.
- The round ran past the limit of 60 minutes of D-627, because the reviewer ran its own smoke stress test. It pushed the record and its handoff first, and `codex-review` then stopped it with a fault. The limit worked as D-627 states.
- The session asks the owner to confirm the merge with the merge summary (D-533).

### State of the build

- Remote `main` is `4a20b82`. The effective head is `db8b3c8`. Each check passed at `db8b3c8`, and the review gate passed at the tip.

### In flight

- The owner confirmation, then `gh pr merge 109 --auto --squash` (D-516).

### Traps and gotchas

- After the merge, the launchd job `com.whatyoucarry.night-fixer` starts to act on each failed night on `main`. `docs/runbooks/night-fixer.md` holds its stop and its removal.
- A review round that runs its own stress test can pass the limit of D-627.

### Open questions that block progress

None for PR-91.

### Next concrete action

After the merge, the next session continues the open findings of D-596, which wait for OQ-195, OQ-196, OQ-199 to OQ-201, a PR-31 decision, or the Deck.

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
