# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 268: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-89, author, answer to review round 1. Branch `feat/pr-89-truecolor-atlas`. PR #107, pending merge. Base `9e4833b`.

### What this session did, and why

- Round 1 gave `Changes required` at effective head `e4a097e` with P2-1: `LinearLight.Blend` divided by zero on a count of 0 parts.
- P2-1 had full merit. Commit `6afe1db` rejects a count of parts that is not positive with a context error. The regression test failed on the old code with `DivideByZeroException`. `docs/reviews/pr-107-response.md` records it.
- The automated pass of `2aa88bd` approved with no finding. Its red-gate note got the D-251 reply in comment 5843313742. No `Gitar review` comment was sent.
- PR-85 exit test 8: at 05:19 UTC on 2026-09-26, the first night of the `7 7 * * *` cron has not started. The newest scheduled night is still run 36141884980 at `a3590ba`, from before the merge of PR-85.

### State of the build

- Effective head `6afe1db`. The full suite passed 1826 of 1826, and `ste-check` found nothing. At `2aa88bd`, each check passed except `evaluate` and `review-gate`, which waited for the record.

### In flight

- The gitar pass of `6afe1db`, CI, and review round 2.
- PR-85 exit test 8: the night of 2026-09-26.

### Traps and gotchas

- The review of round 1 ran `handoff-rotate`, which moved Session 257 to the archive in commit `0230098`.

### Open questions that block progress

None for PR-89. OQ-208 blocks PR-90.

### Next concrete action

After the gitar pass and green CI, run `make codex-review PR=107`. On approval, give the owner the merge summary (D-533). Read the night of exit test 8 when it ends.

## Session 267: 2026-09-26, Codex

Author: Codex
Session: PR-89, reviewer. Branch `feat/pr-89-truecolor-atlas`. PR #107, changes required. Effective head `e4a097e`.

### What this session did, and why

- Reviewed PR #107 against its atlas, palette, and grain contracts (D-598 to D-602).
- Added P2-1 to `docs/reviews/pr-107.md`: `LinearLight.Blend` accepts zero parts, then divides by zero.

### State of the build

- Remote PR head before this review publication: `2aa88bd`. CI, smoke, bit identity, bots, asset QA, determinism lint, document gate, night gate, and STE passed for the effective head. The review gate awaited this record.
- The focused texture and recipe tests passed 126 of 126.

### In flight

- The review record and this handoff entry need one metadata commit and a push.
- P2-1 needs a correction and regression test before approval.

### Traps and gotchas

- `LinearLight.Blend` uses 256 parts for grain today. Its public method still accepts zero and divides by zero.
- The review covers effective head `e4a097e`. Later PR commits changed only paths in the skip set of D-475.

### Open questions that block progress

None. OQ-208 blocks PR-90 only.

### Next concrete action

Correct P2-1, test zero parts, and request a new review round.

## Session 266: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-89, author. Branch `feat/pr-89-truecolor-atlas`. PR #107, pending merge. Base `9e4833b`.

### What this session did, and why

- PR-85 exit test 8: the first night from the `7 7 * * *` cron has not started. At 04:47 UTC on 2026-09-26, the newest scheduled night is run 36141884980 of 13:35 UTC on 2026-09-25, at `a3590ba`. That commit is older than the merge of PR-85 at 18:25 UTC, so it ran the old single job.
- The owner answered OQ-207 and the questions that follow from it: D-598 to D-602.
- Commit `8fec673`: the atlas PNG stores three sRGB bytes for each pixel, the palette has no count limit, and soot joins at flat indices 76 to 79. The new atlas holds the color of each pixel of the indexed atlas, and the contact sheet did not change.
- Commit `e4a097e`: a grain moves a pixel in parts of a fine step, and `Palette.ColorAt` blends the two fine shades in linear light with the whole-number table of `LinearLight`.
- A fixture holds the indexed atlas at the base. A test proves that each pixel keeps its ramp and lies between the two fine shades next to its old shade (D-601).
- The owner approved the new sheet (D-602). The images are in `artifacts/reference/pr-89/` of the main checkout.

### State of the build

- Remote head: the push of this branch. The local checks: the texture and recipe tests passed 126 of 126, and `det-lint`, `asset-qa`, and `ste-check` found nothing.

### In flight

- The PR, CI, the automated pass, and the cross-provider review.
- PR-85 exit test 8: read the night of 2026-09-26 when it ends.

### Traps and gotchas

- The atlas grows from about 260 to about 790 kilobytes, because stored deflate blocks compress nothing (D-305). At an atlas of 2048, PR-90 writes about 12 megabytes for each change. OQ-208 can weigh that.
- A blend of one ramp can give the color of a blend of another ramp. A test reads a pixel on a named ramp through `PaletteShades.TryPlace`.
- The fixture test compares the layout of PR-75. PR-90 changes the layout and replaces that test.
- Every face recipe of the body and the sword has a grain, so the exact check of D-601 covers the blocks alone today.
- Run the Godot build check in a new worktree before a contact sheet. The sheet path must be absolute.

### Open questions that block progress

None for PR-89. OQ-208 blocks PR-90.

### Next concrete action

Wait for the automated pass and the cross-provider review, and answer each finding. Read the night of exit test 8 when it ends.

## Session 265: 2026-09-25, Codex

Author: Codex
Session: PR-75, reviewer. Branch `feat/pr-75-sword-art`. PR #106, Ready for owner merge at effective head `4aebdb9`. Base `602708d`.

### What this session did, and why

- Reviewed PR #106 against its owner decisions and exit tests.
- Traced the locator rotation through the loader and Game node tree, and checked the sword, paint, palette, atlas, and tests.
- Added the revision-specific review record with no findings.
- Generated and inspected the contact sheet. The sword stays clear of the floor in both body views.

### State of the build

- At review start, branch tip `9ac2b19` held all green checks except `review-gate`, which lacked the review record. The Gitar check passed.
- The focused local tests passed 89 of 89. Asset QA and determinism lint found no issues. Texture generation matched the committed atlas and layout.
- The review record and this entry are in one metadata commit pushed to the PR branch. The post-push checks and remote head were verified.

### In flight

- The owner confirms the merge summary after the post-push gates pass (D-533).

### Traps and gotchas

- `4aebdb9` is the effective code head. `9ac2b19` adds only documents in the D-475 skip set.
- OQ-206 holds the roll pose. It blocks no PR. OQ-207 and OQ-208 block PR-89 and PR-90.

### Open questions that block progress

None for PR-75.

### Next concrete action

The owner checks the new `review-gate` result, then reads the merge summary before confirming the merge.

## Session 264: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-75, author. Branch `feat/pr-75-sword-art`. PR #106, pending merge. Base `602708d`.

### What this session did, and why

- The sword of PR-15 is now a worn steel arming sword of eight boxes, from one concept image (D-586 to D-590). The owner skipped the 3D reference (D-590).
- The concept is about 1 meter long, and a sword that hangs straight down reaches into the floor. The `weapon` locator tilts 45 degrees forward, and the loader and `ModelNodes` read the tilt (D-591). The F-131 test now checks that the loader reads the rotation.
- The owner found the slate blade too blue. The owner chose ten new ramps from twelve candidates (D-592, D-593), and then the darkest of three steel settings (D-594). The owner approved the sheet (D-597).
- Eleven ramps pass the 256 colors of an indexed PNG, so soot waits. PR-89, a truecolor atlas, and PR-90, a texture resolution, come next, before any more art (D-595). OQ-207 and OQ-208 block them.
- The owner asked if the repository review of 2026-09-24 is complete. It is not: 13 findings stay open in full and 9 in part. PR-86 fixed RR-P1-1, and the report does not mark it. These findings follow PR-90 (D-596).
- PR-85 exit test 8: the first night from the `7 7 * * *` cron starts at 07:07 UTC on 2026-09-26. At 22:27 UTC on 2026-09-25 it had not started. The newest night, run 36141884980 at 13:35 UTC, is the old night of one job at `a3590ba`.

### State of the build

- Effective head `4aebdb9`. Round 1 gives `Ready for owner merge` with no finding (`eb09d5f`). The gitar pass approved `9ac2b19` with no finding.
- Every check of `9ac2b19` passes but `evaluate` and `review-gate`, which waited for the record. Local: 1822 of 1822 tests pass, Smoke included.
- Core does not change, so the known answer of the sweep stays `9c79047da9c82a0e`.

### In flight

- The owner merge decision after the merge summary (D-533, D-552).
- PR-85 exit test 8. A later session reads the start time, the wall time of each sweep job, the result, and the slice: 6001-6500 for each bot policy, and 120001-130000 for reachability. After a runner fault, re-run the failed jobs (D-585).

### Traps and gotchas

- A new ramp shifts the atlas index of every shade, and the colors stay the same. `GrainPaintsTheSameBytesOnEachPlatform` pins those indices.
- The roll puts the held sword under the floor, as it did with the sword of PR-15 (OQ-206). `TheHeldSwordStaysOverTheFloor` leaves the roll out.
- The Codex CLI 0.157.0 gave `401 Incorrect API key` for about 50 minutes on 2026-09-25 while `codex login status` read ChatGPT. It passed at 23:44 UTC with no new login. A probe every 18 minutes found the end.
- The reference images sit in `artifacts/reference/meshy-sword-2026-09-25/` of the main checkout, which git ignores.

### Open questions that block progress

None for PR-75. OQ-207 blocks PR-89, and OQ-208 blocks PR-90.

### Next concrete action

When the owner confirms, run `gh pr merge 106 --auto --squash`, wait on the checks, and write the prompt of `merge-prompt.md` at the merge. Then read PR-85 exit test 8.

## Session 263: 2026-09-25, Claude Code

Author: Claude Code
Session: PR-86, author, the hand-over. Branch `feat/pr-86-hosted-macos`. PR #105, pending merge. Base `0d99e2c`. Sessions 259 and 261 hold the earlier work of this session.

### What this session did, and why

- Round 2 approved the effective head `1678ef5` with `Ready for owner merge`, and P2-1 is fixed. The gitar pass approved `817dad3` with no finding, and no review thread exists.

### State of the build

- Effective head `1678ef5`. Every check of the tip passes, `evaluate` and `review-gate` included.
- The repository has 0 runners, and the Mac Mini holds no runner agent (D-584).

### In flight

- The owner merge decision after the merge summary (D-533, D-552).
- PR-85 exit test 8: the night of 2026-09-26 from the 07:07 UTC cron has not started. A later session reads its start time, the wall time of each sweep job, its result, and its slice (6001-6500 for each bot policy, 120001-130000 for reachability).

### Traps and gotchas

- Session 259 lists the traps of this PR.

### Open questions that block progress

None for PR-86.

### Next concrete action

When the owner confirms, run `gh pr merge 105 --auto --squash`, wait on the checks, and write the prompt of `merge-prompt.md` at the merge.

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
