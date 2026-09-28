# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 307: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-95, author. Branch `fix/pr-95-seed-2-transition`. PR #122, pending merge. Base `5ca1e1e`.

### What this session did, and why

- Read the second review round of session 306. The record gives `Ready for owner merge` for the effective head `34c2ddf`, and P2-1 is withdrawn.
- Wrote the merge summary of D-533, and asked the owner to confirm the merge (D-524).

### State of the build

- The remote head before this commit is `742fa6b`. Each check passes there, `review-gate` included. The work head of the automated pass stays `3e01603`.

### In flight

- The owner confirmation of the merge of PR #122.
- PR-92 exit test 3: the 07:07 UTC night of 2026-09-28 on `main` did not start by 14:47 UTC.

### Traps and gotchas

- None new. Session 303 lists the traps of the Deck trace.

### Open questions that block progress

None.

### Next concrete action

After the merge, read the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`.

## Session 306: 2026-09-28, Codex

Author: Codex
Session: PR-95, reviewer. Branch `fix/pr-95-seed-2-transition`. PR #122, ready for owner merge. Effective head `34c2ddf`.

### What this session did, and why

- Re-reviewed P2-1 against the author response, the source before and after the change, and the PR-95 exit tests.
- Withdrew P2-1 because the same-tick tree build existed before this PR, and the roadmap does not include that fallback case.
- Updated `docs/reviews/pr-122.md` with the result and preserved the earlier verdict.

### State of the build

- The remote tip before this commit is `e8f4e9b`; the code head and effective head remain `34c2ddf`.
- Required code checks passed at `3e01603`. The document checks pass. Review gate and evaluate await this review record.

### In flight

- The review record and this handoff entry need one metadata commit and a push to `fix/pr-95-seed-2-transition`.

### Traps and gotchas

- The same-tick fallback builds unfinished chunk meshes and enemy trees on descent. This pre-existing behavior is outside the PR-95 exit tests.
- The Deck logs cover the measured seed 2 transitions. This session did not repeat the Deck run.

### Open questions that block progress

None.

### Next concrete action

After the push, confirm the remote head and report the review result to the owner.

## Session 305: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-95, author. Branch `fix/pr-95-seed-2-transition`. PR #122, pending merge. Base `5ca1e1e`.

### What this session did, and why

- Answered the review of session 304 in `docs/reviews/pr-122-response.md`. P2-1 has no merit: a plan offered on the descent tick also makes `ChunkSwap.AfterTick` build every chunk in that tick, since PR-18. A trees-only fix cannot meet D-635 there, and trees shown late draw enemies with no model.
- Measured the race on the Mac, because the Deck slept: a `--transitions 6` session offered each plan 1 to 5 ticks after its swap, and the floors lasted 344 to 2118 ticks.
- The automated pass approved `3e01603` with no thread. One reply to its review-gate note cites D-251. No `Gitar review` comment.

### State of the build

- The remote head before this commit is `c7cbad4`, the review record. The code head stays `34c2ddf`. Each check but the Review gate workflow is green at `3e01603`.

### In flight

- The second round of `make codex-review PR=122` on the same effective head.
- PR-92 exit test 3: the 07:07 UTC night of 2026-09-28 on `main` did not start by 14:32 UTC.

### Traps and gotchas

- The Deck sleeps after some idle minutes. Ask the owner to wake it before each Deck run.

### Open questions that block progress

None.

### Next concrete action

Read the outcome of the second review round. On approval, write the merge summary, and ask the owner to confirm the merge.

## Session 304: 2026-09-28, Codex

Author: Codex
Session: PR-95, reviewer. Branch `fix/pr-95-seed-2-transition`. PR #122, changes required. Effective head `34c2ddf`.

### What this session did, and why

- Reviewed PR #122 against its tests, roadmap, guardrails, comments, and Deck evidence.
- Found P2-1: a plan that arrives on the descent tick leaves no frame to stage its enemy trees.
- The focused transition and smoke tests passed, and the required CI checks passed.

### State of the build

- The remote head before review publication was `3e01603`. The implementation head is `34c2ddf`.
- The focused tests passed, 2 of 2, on macOS arm64 with .NET 10.0.12. Required CI passed except the expected pre-publication review gate and evaluate check.

### In flight

- PR #122 needs a correction and a regression test for P2-1.

### Traps and gotchas

- When the worker offers a plan on the same physics tick as descent, `BuildSome` has not run before `Rebuild`.
- The Deck logs cover a plan that arrived before descent. They do not cover this timing race.

### Open questions that block progress

None.

### Next concrete action

Load `review-response` and correct P2-1 with a regression test.

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
