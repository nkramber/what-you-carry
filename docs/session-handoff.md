# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 310: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-96, author. Branch `feat/tier-4-pass`. PR #123, pending merge. Base `7bab7cf`.

### What this session did, and why

- Read the review of session 309. The record gives `Ready for owner merge` for the effective head `4236f2d`, with no finding.
- Answered the review-gate note of the gitar dashboard with D-251. The gitar pass approved the work head `77ab1d1` with no thread.
- Wrote the merge summary of D-533, and asked the owner to confirm the merge (D-524).

### State of the build

- Each check of PR #123 passed at `77ab1d1`, except `evaluate` and `review-gate`, which waited for the review record. The effective head stays `4236f2d`.

### In flight

- The owner confirmation of the merge of PR #123.

### Traps and gotchas

- None new. Session 308 lists the traps of the frame capture.

### Open questions that block progress

None.

### Next concrete action

After the merge, start PR-97 (D-715).

## Session 309: 2026-09-28, Codex

Author: Codex
Session: PR-96, reviewer. Branch `feat/tier-4-pass`. PR #123, pending merge. Base `7bab7cf`.

### What this session did, and why

- Reviewed effective head `4236f2d` of PR #123 against the PR-96 exit tests and the Tier 4 evidence.
- The provider gate passed. The review found no in-scope defect.
- Added `docs/reviews/pr-123.md` and this handoff entry in one metadata commit.

### State of the build

- The remote code head before the metadata push was `77ab1d1`. Build and test jobs, bit identity, bots, asset QA, determinism lint, smoke, night gate, documents, doc gate, and STE passed.
- The PR checks `evaluate` and `review-gate` failed because the review record did not yet exist. The review record and this entry are now on the PR branch.

### In flight

- PR #123 awaits the owner merge.

### Traps and gotchas

- The review inspected supplied capture artifacts but did not rerun the interactive Mac capture sessions.
- The frame set follows the Gate 2 scope of D-714. The Gate 4 Tier 4 protocol stays open under OQ-62.

### Open questions that block progress

None.

### Next concrete action

The owner can review the record and merge PR #123 when its checks pass.

## Session 308: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-96, author. Branch `feat/tier-4-pass`. PR #123, pending merge. Base `7bab7cf`.

### What this session did, and why

- PR-92 exit test 3 passes. The scheduled night of 15:18 UTC on 2026-09-28 on `main` at `5ca1e1e` (run 36442573875) succeeded with eight sweep jobs. Full-clearer and reachability ran as two shards each. The record on `night-results` names each of the six sweeps once in each section. No night failed, so the Pushover check of PR-91 exit test 23 did not apply. The sweeps took 2 to 9 minutes each, where the night of 2026-09-27 took up to 2.5 hours.
- The owner answered OQ-62 in part, for Gate 2: D-711 to D-714. Sequence item 56 got its roadmap entry, PR-96.
- `--frame-shots <directory>` writes one Deck frame with the HUD each second of a bot session. The flag needs `--bot`. A headless run, a failed write, and a shot with no new frame are each an error line and exit code 1.
- The Tier 4 pass ran on the full-clearer and the timer-tester on seed 1, and on the HUD shot. It found F-195 to F-201. The owner gave each its disposition: D-715 (PR-97, the camera), D-716 (PR-98, the numbers and the prompt, F-24), D-717 (F-198 in the Gate 2 play), and D-718 (F-200 to OQ-61).

### State of the build

- The remote head of `main` is `7bab7cf`. The local suite, `det-lint`, `asset-qa`, and `ste-check` pass on the branch.

### In flight

- PR #123: the automated pass, then `make codex-review`.

### Traps and gotchas

- A Godot window that another window covers draws no frame, and the ticks go on. The first timer-tester capture wrote 204 copies of one frame, because a HUD shot window opened over it. Keep the capture window clear. The capture now stops with an error line.
- Readers of 30 or more frames can end on a connection reset. Split the frames into smaller sets.
- The timer-tester stands still, so 212 of its frames differ only in the timer. A pixel comparison proves it faster than a read of each frame.

### Open questions that block progress

None. The protocol of Gate 4 stays open under OQ-62, and it blocks nothing before Phase 4.

### Next concrete action

After the merge of PR #123, start PR-97 (D-715).

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
