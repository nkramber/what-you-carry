# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 275: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-90, author, merge. Branch `feat/pr-90-texture-resolution`. PR #108, pending merge. Base `60a23ec`.

### What this session did, and why

- Review round 2 gave `Ready for owner merge` at the effective head `e4d3543`, with P2-1 fixed. The gitar pass of `1ce21f1` approved with no finding, and no review thread is open.
- The session asks the owner to confirm the merge with the merge summary (D-533).

### State of the build

- Effective head `e4d3543`. Each check passed at `e4d3543`. At the tip, `review-gate`, `evaluate`, `night-gate`, and Gitar passed, and the code jobs skipped on the documents heads (D-357).

### In flight

- The owner confirmation, then `gh pr merge 108 --auto --squash` (D-516).

### Traps and gotchas

- A Deck run over SSH sets `XAUTHORITY`, or Godot 4.7.2 can hang at exit on the Wayland fallback (F-147).

### Open questions that block progress

None for PR-90.

### Next concrete action

After the merge, the next session starts the open review findings of D-596, as the phase 2 roadmap orders.

## Session 274: 2026-09-26, Codex

Author: Codex
Session: PR-90, reviewer. Branch `feat/pr-90-texture-resolution`. PR #108, pending merge. Base `60a23ec`.

### What this session did, and why

- Re-reviewed PR #108 after the author fixed P2-1. `e4d3543` checks the `IHDR` length before it reads fixed offsets.
- Ran the focused screenshot-reader regression tests at `e4d3543`. All 5 passed, including the short-header case.
- Updated `docs/reviews/pr-108.md`. P2-1 is fixed, and the verdict approves the effective head `e4d3543`.

### State of the build

- The remote head before this metadata commit was `1ce21f1`. The focused tests passed, and the Documents category passed 196 of 196. STE check passed. The local full suite stayed silent for more than three minutes, so this session interrupted it. The prior handoff reports 1867 of 1867 tests passed at `e4d3543`.
- The live checks at `1ce21f1` pass for `det-lint`, `doc-gate`, `documents`, `night-gate`, and `ste-check`. The documents-only rule skips the full suite, smoke, asset QA, and bit identity on this metadata tip. `evaluate` and `review-gate` wait for this review record.

### In flight

- The review record and this entry need one metadata commit and a push to `feat/pr-90-texture-resolution`.

### Traps and gotchas

- The effective head is `e4d3543`; later commits change documents only.
- The source screenshots are absent from this worktree. D-616 records owner approval of the contact sheet.

### Open questions that block progress

None. OQ-208 is resolved by D-603 to D-608.

### Next concrete action

Commit this entry with `docs/reviews/pr-108.md`, push to the PR branch, and verify the remote head.

## Session 273: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-90, author, review response. Branch `feat/pr-90-texture-resolution`. PR #108, pending merge. Base `60a23ec`.

### What this session did, and why

- Answered round 1 of the review in `docs/reviews/pr-108-response.md`. P2-1 has full merit. `e4d3543` rejects a header chunk that is not 13 bytes, and a regression case failed on the old reader.
- Ran exit test 2 at the PR head `e4d3543` on the Deck over SSH, on X11, after the owner said the Deck was ready (D-606). After the first 10 s, no frame is over 11111 µs (D-295). In the first 10 s, 67 frames were over, and the worst was 27270 µs.
- The owner asked for a fix of the exit hang now (F-147). Under gdb, the main thread waits in `pthread_join`, and the thread "Wayland Events" waits in `poll`. On the Wayland fallback of an SSH session, 5 of 14 runs hung, `main` too. With `XAUTHORITY` set, 0 of 9 runs hung.
- `docs/runbooks/commands.md` holds the Deck run procedure with `XAUTHORITY`, and the agent files name it.
- The gitar pass of `e4d3543` approved with no finding.

### State of the build

- Effective head `e4d3543`. Each check of `e4d3543` passed except `evaluate` and `review-gate`, which wait for the review record (D-251). The local full suite passed 1867 of 1867.
- The documents commit of this entry, the response, the runbook, and F-147 comes after `e4d3543`.

### In flight

- The gitar pass of the documents commit, then round 2 of `make codex-review PR=108`.

### Traps and gotchas

- The Deck checkout is on the PR-90 branch at `e4d3543`, and its build is current.
- The probe `~/hang-probe2.sh <driver> <runs>` on the Deck runs the session under gdb, because `ptrace_scope` is 1. It stops at the first hang.
- The official Godot binary has no symbols. The libc frames map to a symbol through `readelf -S` and `nm -D` on the Deck.

### Open questions that block progress

None for PR-90.

### Next concrete action

Run `make codex-review PR=108` when each check but the Review gate workflow is green. After a `Ready for owner merge` verdict, give the owner the merge summary (D-533).

## Session 272: 2026-09-26, Codex

Author: Codex
Session: PR-90, reviewer. Branch `feat/pr-90-texture-resolution`. PR #108, pending merge. Base `60a23ec`.

### What this session did, and why

- Reviewed PR #108 at head `4edcd2c`. The provider gate passed because session 271 names Claude Code as the author.
- Added P2-1. `ScreenshotPng` reads `IHDR` fields before it checks that the chunk has 13 bytes.
- The review record names the missing review gate record comment and the author response. It verifies that the gate was red before this record.
- The source screenshots under `artifacts/reference/` are not in this worktree, so the review could not compare each traced map with its source image.

### State of the build

- At remote work head `4edcd2c`, asset QA, bit identity on three platforms, the bot checks, CI on three platforms, determinism lint, doc gate, documents, night gate, smoke on three platforms, and STE passed.
- `evaluate` and `review-gate` failed because the review record was absent. Focused local tests passed: 39 `TextureTraceTests`, and 126 tests across texture trace, texture generation, and asset QA.
- The handoff reports the Deck frame logs at `763efd5` and `60a23ec`. No run at `4edcd2c` appears in the evidence.

### In flight

- P2-1 needs a length check and a regression test. Exit test 2 needs a Deck frame log at the PR head.
- The review record and this entry need one metadata commit and a push to the PR branch (D-182).

### Traps and gotchas

- The trace screenshots are gitignored files under `artifacts/reference/` and are absent from this worktree.
- D-606 requires an owner readiness check before a Deck connection. The latest handoff reports that the Deck checkout is on the PR branch.
- The first local test command used `--no-restore` and gave no result. A later restore and focused run passed.

### Open questions that block progress

No owner decision is open. The current-head Deck run and the source screenshots remain unavailable review evidence.

### Next concrete action

Fix P2-1 with a short-`IHDR` regression test. Run the Deck frame log at the PR head after the owner confirms readiness, then request a repeat review.

## Session 271: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-90, author. Branch `feat/pr-90-texture-resolution`. PR #108, pending merge. Base `60a23ec`.

### What this session did, and why

- PR-85 exit test 8 passed. The first night of the `7 7 * * *` cron is run 36241114010 at `60a23ec`. It started at 12:10:52 UTC, about 5 hours after the cron time. The plan job took 9 s, and the record job took 39 s.
- The sweep jobs: coward 8 min 32 s, timer-tester 19 min 27 s, random-walker 20 min 2 s, greedy-descender 1 h 17 min 43 s, full-clearer 1 h 21 min 41 s, and reachability 2 h 30 min 7 s. Each job passed, with no runner fault. The record names the slice 6001 to 6500 for each bot policy, and 120001 to 130000 for reachability. It carries no seed.
- The owner answered OQ-208 (D-603 to D-608). Every face has 64 texels per meter, in an atlas of 1024.
- The owner rejected the first repaint at 64. The `texture-trace` command and the `map` layer now trace each visible face of the body and the sword from unlit Meshy views (D-612 to D-614).
- The owner approved the traced sheet (D-616), after three rounds on the blade (D-615). The blade flat is mid steel with lighter edges. The skin moved from bone to clay.
- `asset-qa` skips the trace specs, because a spec names screenshots outside the repository (D-617).
- The seven procedural recipes that the traces replaced are gone, and the grip box reads `leather`.
- An earlier session did the work up to the sheet, and a connection failure ended it. This session continued the same PR from the blade review.

### State of the build

- The local full suite, `det-lint`, `asset-qa`, and `ste-check` passed at the work head. See the PR checks for CI.
- Exit test 2: four bot sessions with `--frame-log` on the Deck over SSH. Three ran at `763efd5`, and one ran on `main` at `60a23ec` as a control. `763efd5` holds the atlas and layout of the head, byte for byte.
- After the first 10 s, each of the four runs holds 11111 µs, 90 frames per second, with no frame over (D-295). In the first 10 s, the PR-90 runs had 44, 12, and 303 frames over, and `main` had 16. Most were under 0.6 ms over. The worst was 26.7 ms.

### In flight

- The automated pass, then `make codex-review` (D-511).

### Traps and gotchas

- The Deck is at 10.0.0.46, user `deck`. Export `~/.dotnet` on the path, `DOTNET_ROOT`, `DISPLAY=:0`, `XDG_RUNTIME_DIR=/run/user/1000`, and `WAYLAND_DISPLAY=wayland-0`. X11 refuses the SSH session, and Godot falls back to Wayland.
- One Deck run did not exit after its end line, and a stop signal ended it. Two later runs exited in 45 s. Wrap each run in `timeout`.
- The frame misses of the first 10 s vary from 12 to 303 at one head. Compare a head with a control run of `main` on the same Deck.
- The Deck checkout is on the PR-90 branch now.
- The trace keeps each recipe that exists. The blade maps are a hand remap of the trace: dark iron to mid steel, rust to pits (D-615). A new trace of the blade loses that remap.

### Open questions that block progress

None for PR-90.

### Next concrete action

Answer the automated pass, then run `make codex-review PR=108`. After a `Ready for owner merge` verdict, give the owner the merge summary (D-533).

## Session 270: 2026-09-26, Claude Code

Author: Claude Code
Session: PR-89, author, merge. Branch `feat/pr-89-truecolor-atlas`. PR #107, pending merge. Base `9e4833b`.

### What this session did, and why

- Review round 2 gave `Ready for owner merge` at effective head `6afe1db`, with P2-1 fixed. The automated pass of `2faf0c0` approved with no finding.
- The owner read the merge summary and confirmed the merge (D-533). The session runs `gh pr merge 107 --auto --squash` after this entry.
- PR-85 exit test 8 stays unread. At 05:57 UTC on 2026-09-26, the first night of the `7 7 * * *` cron has not started. The owner chose to merge before that night ends.

### State of the build

- Effective head `6afe1db`. The code checks passed at `2faf0c0`, and `review-gate` passed at `90b719a`. The local full suite passed 1826 of 1826.

### In flight

- The auto-merge of PR #107.

### Traps and gotchas

- The schedule of a night can start hours after 07:07 UTC. Read `gh run list --workflow night.yml` before you call a miss.

### Open questions that block progress

None for PR-89. OQ-208 blocks PR-90.

### Next concrete action

Read the night of 2026-09-26 for PR-85 exit test 8: its start time, the wall time of each sweep job, its result, and the slices of its record (6001 to 6500 for each bot policy, 120001 to 130000 for reachability). If a job ends with a runner fault, re-run the failed jobs (D-585). Then start PR-90 after the owner answers OQ-208.

## Session 269: 2026-09-26, Codex

Author: Codex
Session: PR-89, reviewer, round 2. Branch `feat/pr-89-truecolor-atlas`. PR #107, pending merge. Effective head `6afe1db`.

### What this session did, and why

- Re-reviewed PR #107 after the author fixed P2-1 from round 1.
- `LinearLight.Blend` now rejects a count of parts that is not positive. Its regression test passed.
- Updated `docs/reviews/pr-107.md`. P2-1 is fixed, and the verdict is `Ready for owner merge` at `6afe1db`.
- CI at tip `2faf0c0` passed for code, smoke, bit identity, bots, assets, lint, documents, and the night gate. `evaluate` and `review-gate` failed because the record still named the earlier verdict.

### State of the build

- Effective head `6afe1db`. The focused linear-light test passed. The current tip is `2faf0c0` on `origin/feat/pr-89-truecolor-atlas`.

### In flight

- Publish this review record and handoff in one metadata commit. Then wait for fresh checks of the new tip.

### Traps and gotchas

- The review uses the code head `6afe1db`. Later commits change only review metadata.

### Open questions that block progress

None for PR-89. OQ-208 blocks PR-90.

### Next concrete action

After the metadata push, confirm `evaluate`, `review-gate`, and Gitar on the new tip. The owner can then use the merge summary of D-533.

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
