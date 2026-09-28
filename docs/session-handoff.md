# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 303: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-95, author. Branch `fix/pr-95-seed-2-transition`. PR #122, pending merge. Base `5ca1e1e`.

### What this session did, and why

- Traced the seed 2 transition on the Deck over SSH, after the owner said that the Deck was ready (D-606, G-17). The timing patch stayed out of the branch.
- The trace named two costs in the tick of the descent (F-193). The dig task of floor 3 started in that tick, and its allocations started a full collection of 4.1 to 5.0 ms there. The enemy trees of floor 2 took 3.2 to 3.5 ms.
- `e538dd9`: the next call of `ChunkSwap.UploadSome` starts the dig, after the frame log reads the frame. One run in three still missed (24823), with a gen 0 collection inside the tree build.
- `34c2ddf`: `EnemyNodes` builds the trees of the next floor hidden, one each frame, from the plan that the worker offers. The descent shows them. The swap line gains `digging`, `stagedTrees`, and `builtTrees`.
- Tests: a source test of the dig start in `TransitionTests`, and the stairwell smoke test asserts `digging:false` and `builtTrees:0`. Both fail on the code of `main`.
- The M-3 table at `34c2ddf`: seeds 1, 2, 2, 2, 3 meet D-709 and D-635. The transition frame takes 15.7 to 16.4 ms. M-3 and PR-95 are marked done, and PR-13 exit test 7 passes.
- The raw logs, the traces before and after, and an index are in `docs/reviews/pr-122-deck/`.

### State of the build

- `main` is `5ca1e1e`. The code head is `34c2ddf`. The full suite passed locally, 2110 tests, with `det-lint`, `asset-qa`, and `ste-check` clean.

### In flight

- PR #122: CI, the automated pass, then `make codex-review`.
- PR-92 exit test 3: the 07:07 UTC night of 2026-09-28 on `main` did not start by 14:05 UTC. Its record on `night-results` must name each sweep once, over eight sweep jobs.

### Traps and gotchas

- A timing patch that writes lines or makes strings in the frames moves the collector, and the first console write cost 3.3 ms. Keep the numbers in fixed arrays, and write them later (runbook, "The Deck runs").
- On seed 2 the end line `transitionMicrosMax` is the F-190 stall at frame 2019, inside the window. Read the transition frame in the frame log.
- The Deck went to sleep once during the session. `systemd-inhibit` over SSH needs interactive authentication.
- `timeout` does not exist on the Mac. zsh does not split `set -- $p`.

### Open questions that block progress

None.

### Next concrete action

After the merge, read the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`.

## Session 302: 2026-09-28, Claude Code

Author: Claude Code
Session: M-3, author. Branch `docs/m-3-deck-frame-time`. PR #121, pending merge. Base `5c3310e`.

### What this session did, and why

- Asked the owner answers that the roadmap names: the builds of the table, the p99 reading, and the pass rule. The strict p99 of 11111 µs was not reachable at the 90 Hz vsync (F-194). D-707 to D-709 record the answers.
- Ran the bot session on the Deck over SSH at `5c3310e`, on seeds 1, 2, and 3 (D-606, D-706). Each seed meets D-709, with a filtered p99 of 11437 to 11492 µs.
- The transition of seed 2 took 27725, 25948, and 25110 µs in three runs, over the 22222 µs of D-635 (F-193). The owner held M-3 open, and PR-95 fixes the transition and closes M-3 (D-710).
- Wrote the table and the method in the M-3 entry, the PR-95 entry, F-193, and F-194. The raw logs and the end lines are in `docs/reviews/pr-121-deck/`.

### State of the build

- `main` is `5c3310e`. This PR changes documents alone, so the checks of D-491 apply.

### In flight

- PR #121: the automated pass, then the `review-override` label (D-652).
- PR-92 exit test 3: the 07:07 UTC night of 2026-09-28 on `main` did not start before this entry. Its record on `night-results` names each sweep once, over eight sweep jobs.

### Traps and gotchas

- The end line of a frame log gives the raw p99. The filtered p99 of D-708 needs the method in the M-3 entry.
- The session of seed 2 ends at 23 seconds, so its filtered p99 reads about 1190 frames.

### Open questions that block progress

None.

### Next concrete action

After the merge, read the night of PR-92 exit test 3 on `night-results`. Then start PR-95: a trace of the seed 2 transition on the Deck before a change (G-17).

## Session 301: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-94, author. Branch `feat/pr-94-seed-flag`. PR #120, pending merge. Base `d566bda`.

### What this session did, and why

- Completed the gitar pass of the work head `f12021b`: an approval with no finding. The CI note on the absent review record got one reply with D-251.
- Ran `make codex-review PR=120`. The record gives `Ready for owner merge` for the effective head `f8d06f5`, with no finding (session 300).

### State of the build

- Each check of PR #120 is green, `evaluate` and `review-gate` included, after the metadata commits of the review. Effective head `f8d06f5`, work head `f12021b`.

### In flight

- The owner confirmation of the merge of PR #120 (D-524, D-533).
- PR-92 exit test 3: the 07:07 UTC night of 2026-09-28 on `main` did not run before this entry. The first night after `d5f7e00` runs eight sweep jobs, and its record names each sweep once.

### Traps and gotchas

- A reviewer entry lands above the entry of the author. Add a new entry at the top, and never edit the older one.

### Open questions that block progress

None.

### Next concrete action

After the merge, read the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`. The next PR is M-3 on seeds 1, 2, and 3 over SSH on the Deck (D-606, D-706).

## Session 300: 2026-09-28, Codex

Author: Codex
Session: PR-94, reviewer. Branch `feat/pr-94-seed-flag`. PR #120, pending merge. Base `d566bda`.

### What this session did, and why

- Reviewed PR #120 at effective head `f8d06f5`. The author provider is Claude Code, so the Codex review passes the provider gate.
- The seed flag, its parser rules, boot path, tests, and PR-94 documents match D-703 to D-706. The review record gives `Ready for owner merge` with no findings.
- Five focused tests passed. GitHub code checks passed. `evaluate` and `review-gate` failed because the review record was not on the PR yet.

### State of the build

- Local: five focused tests passed at `f12021b`. GitHub code checks passed at the same head.
- Remote: review record and this entry are on `feat/pr-94-seed-flag` at metadata head `7e06e2e`. The required remote hash comparison passed.

### In flight

- The metadata-tip `evaluate` and `review-gate` checks passed. The heavy code workflows skipped by the documents-only rule, and their required jobs passed at code head `f12021b`.

### Traps and gotchas

- The checkout is detached. Compare its final commit with the head that `gh pr view 120` reports.

### Open questions that block progress

None.

### Next concrete action

The review is complete for effective head `f8d06f5`. The owner can read the review record and the check results before merge.

## Session 299: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-94, author. Branch `feat/pr-94-seed-flag`. PR #120, pending merge. Base `d566bda`.

### What this session did, and why

- The session started as the M-3 author. The M-3 table needs three seeds, and the bot session ran the first seed alone. The owner chose a seed flag in its own PR first (D-703). The session did no work before that answer, so it binds to PR-94.
- Added `--seed <n>` to the bot session: it needs `--bot`, and it takes a whole number from 1 up in digits alone (D-704, D-705). The loop starts on the seed, and a boot failure line carries it.
- Recorded the M-3 seeds: 1, 2, and 3 (D-706). A headless bot session on the Mac reaches floor 2 on seeds 2 and 3.

### State of the build

- Local: the first full run passed 2108 of 2109 tests. The STE test failed on two long sentences, and `f8d06f5` holds the fix. After it, `ste-check` and `det-lint` found zero findings, and the STE and parser tests passed.
- Remote head of the work: `f8d06f5`, plus the metadata and status commit of this entry.

### In flight

- The gitar pass, CI, and `make codex-review` of PR #120.
- PR-92 exit test 3: the first scheduled night on `main` after `d5f7e00` is the 07:07 UTC cron of 2026-09-28. It did not run before this entry.

### Traps and gotchas

- The Mac has no `timeout` command. Run a Godot session under the time limit of the tool.
- The engine tests of the seed flag take about one second, because a headless session at fixed 60 frames per second runs faster than real time.

### Open questions that block progress

None.

### Next concrete action

After the merge of PR #120, start M-3 in a new session: run seeds 1, 2, and 3 on the Deck over SSH (D-606, D-706), and record the table. Check the night of PR-92 exit test 3 on `night-results`.

## Session 298: 2026-09-27, Codex

Author: Codex
Session: PR-93, reviewer. Branch `feat/pr-93-overseer-model`. PR #119, pending merge. Base `b4615c0`.

### What this session did, and why

- Reviewed the PR-93 code head `5faf3b7` as the cross-provider reviewer.
- Checked the model, the hunter model field, the texture mappings, the focused tests, the roadmap, and the PR comments.
- Wrote `docs/reviews/pr-119.md`. The review found no in-scope defect. The owner approval of the contact sheet is recorded in D-700.

### State of the build

- Local focused tests passed: 290 passed, none failed or skipped. Local asset QA found zero findings across four models and three animations.
- Remote CI passed the build and test jobs, bit identity on three platforms, bot checks, asset QA, determinism lint, documents, doc gate, night gate, smoke on three platforms, and STE checks.
- Remote `evaluate` and `review-gate` failed because the review record was not yet on the branch. Remote branch head before this metadata push: `e2eea3e`.

### In flight

- The review record and this handoff entry need one metadata commit and a push to `feat/pr-93-overseer-model`.
- PR #119 is pending owner merge.

### Traps and gotchas

- The review applies to effective head `5faf3b7`. The later code tip `e2eea3e` changes documents only (D-534).
- The contact-sheet image named by D-700 was not in the referenced folder. The owner approval remains recorded in D-700.

### Open questions that block progress

None.

### Next concrete action

- Push the review record and this entry together. Verify the remote head and the review-gate result.

## Session 297: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-93, author. Branch `feat/pr-93-overseer-model`. PR #119, pending merge. Base `b4615c0`.

### What this session did, and why

- Built the Overseer model through the five steps of `asset-texture-creation`. The owner reviewed the Meshy prompt, concept 01, the edit to concept 02, the eleven Meshy views, the build questions, and the contact sheet. The answers are D-686 to D-702.
- The model is 37 units tall over the body box of D-165. A respirator, goggles, and cans hide the face, and the coat rides on wide upper legs (D-686 to D-697).
- The hunter file names its model (D-698). The Game layer draws the Overseer with it, and the contact sheet shows it in cells of 600 pixels (D-700).
- 45 faces trace from the unlit views, with the hand corrections of D-700. The coat and the cowl grain at cell 2, and the rest at cell 8 (D-699).
- Step 1 of `asset-texture-creation` fits the Meshy limits: 800 characters, and no negative prompt field (D-701, D-702).

### State of the build

- Local: build, the full suite with Smoke (2105), `asset-qa`, `det-lint`, `ste-check`, and the Godot build check are green. Remote `main` is `b4615c0`.
- PR #119: each check of the effective head `5faf3b7` is green but the Review gate workflow. The gitar pass approves the head `e2eea3e` with no finding. The review record gives `Ready for owner merge` for `5faf3b7`.

### In flight

- The owner confirmation of the merge of PR #119 (D-524, D-533).
- PR-92 exit test 3: no scheduled night on `main` ran after `d5f7e00` by 03:35 UTC on 2026-09-28. The recent nights started from 12:00 to 13:00 UTC.

### Traps and gotchas

- Meshy text to image takes 800 characters, and it has no negative prompt field (D-702).
- Each grain layer dithers each pixel by up to 1.5 fine steps, whatever its cell (D-599). Only a larger cell softens the patches.
- The reference folder of this PR is `artifacts/reference/overseer-2026-09-27/` in the main checkout, with a copy in the worktree `/Volumes/SSD-1TB/wyc-pr93`.

### Open questions that block progress

None.

### Next concrete action

After the merge, check the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`.

## Session 296: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-77, author. Branch `feat/pr-77-light-and-edges`. PR #118, pending merge. Base `3fdefef`.

### What this session did, and why

- Read the second review of session 295. The record gives `Ready for owner merge` for the effective head `ce02802`, with no finding.
- Moved the title and the rule line of this file back above the newest entry, where session 295 had put its entry.

### State of the build

- Each check of PR #118 is green at `fc85d96`, `review-gate` and `evaluate` included. The effective head stays `ce02802`.

### In flight

- The owner confirmation of the merge of PR #118 (D-524, D-533).
- PR-92 exit test 3 waits for the scheduled night of 07:07 UTC on 2026-09-28.

### Traps and gotchas

- A review entry can land above the title line of this file. Put the title and the rule line back on top, and keep the entry text as it is.

### Open questions that block progress

None.

### Next concrete action

After the merge, check the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`.

## Session 295: 2026-09-27, Codex

Author: Codex
Session: PR-77, reviewer. Branch `feat/pr-77-light-and-edges`. PR #118, ready for owner merge. Effective head `ce02802`.

### What this session did, and why

- Re-reviewed PR #118 after session 294 added the raw Deck frame logs and the response file.
- Verified the exit test 2 frame threshold and transition in all three logs. The measurements pass D-683.
- Read the code diff, tests, project contracts, documents, and PR comments. No code defect remains.
- Updated `docs/reviews/pr-118.md` to approve the effective head.

### State of the build

- Effective head: `ce02802`. The five later commits change only paths in the skip set of D-475.
- The code checks passed on `9acf45d`, including CI, Smoke, Bit identity, Bots, Asset QA, determinism lint, doc gate, night gate, and STE check.
- At `6e00a68`, the metadata checks passed, and Gitar approved the code. `evaluate` and `review-gate` still read the earlier `Blocked` verdict.
- Remote head: the metadata commit of this entry on `feat/pr-77-light-and-edges`.

### In flight

- The review record and this handoff need fresh `evaluate` and `review-gate` results after publication.
- The owner reads the review and confirms the merge summary before the merge.

### Traps and gotchas

- The effective head stays `ce02802`, because later commits changed only review metadata.
- The code, smoke, bot, and bit-identity checks skip a documents-only tip. Their result at `9acf45d` covers the unchanged code.
- Each Deck frame log holds microseconds, one frame per line. The exit test drops frames at 10 seconds or earlier.

### Open questions that block progress

None.

### Next concrete action

The owner reads the review, confirms the merge summary of D-533, and then merges PR #118.

## Session 294: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-77, author. Branch `feat/pr-77-light-and-edges`. PR #118, pending merge. Base `3fdefef`.

### What this session did, and why

- Answered the review of session 293. The review found no defect in the code, and it blocked exit test 2, because the raw Deck frame logs were not in the checkout.
- Committed the raw logs in `docs/reviews/pr-118-deck/`: three runs at `ce02802`, the base, the three MSAA modes, the empty scene of F-190, and two traces with a timing patch.
- Wrote `docs/reviews/pr-118-response.md` with the method of D-683 and the numbers that the logs give.

### State of the build

- Effective head: `ce02802`. The logs and the response are in the metadata set of D-184, so they do not move it.
- CI, smoke, bit identity, bots, and the other checks passed for `ce02802`. `evaluate` and `review-gate` wait for an approving review.

### In flight

- The second round of `make codex-review PR=118`.
- PR-92 exit test 3 waits for the scheduled night of 07:07 UTC on 2026-09-28.

### Traps and gotchas

- A frame log holds one frame per line in microseconds. The stall of F-190 comes every 2.245 s, so a check of exit test 2 reads the grid of step 4 of the response file.

### Open questions that block progress

None.

### Next concrete action

Read the second review. When it approves `ce02802`, check the night of PR-92 exit test 3, and give the owner the merge summary.
