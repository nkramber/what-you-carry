# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 313: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-97, author. Branch `feat/pr-97-camera-occlusion`. PR #124, pending merge. Base `91dbaa2`.

### What this session did, and why

- Answered the one gitar finding with d6589d9: the camera reach of the model fade is the straight distance to the body box, as D-721 says, and not the distance on each axis. The test `TheReachIsAStraightDistance` fails on 55a5b99. Gitar approved d6589d9, and the thread is resolved.
- Posted the D-251 note for the review-gate line of the gitar dashboard.
- Captured the frames of D-714 again at d6589d9. The ten camera frames of `docs/reviews/pr-124-frames/` are identical to the pixel. The HUD shot differs in one box of 21 by 40 pixels at the damage numbers alone, so D-723 stands.
- Read the review of session 312: `Ready for owner merge` for the effective head d6589d9, with no finding.

### State of the build

- Each check of PR #124 passed at d6589d9, except `evaluate` and `review-gate`, which waited for the review record. The effective head stays d6589d9.

### In flight

- The owner confirmation of the merge of PR #124 (D-524, D-533).

### Traps and gotchas

- The HUD shot is not identical to the pixel from run to run at the damage numbers. Compare the camera frames, and not that box.

### Open questions that block progress

None.

### Next concrete action

After the merge, start PR-98 (D-716). PR-99 follows it before Gate 2 (D-724).

## Session 312: 2026-09-28, Codex

Author: Codex
Session: PR-97, reviewer. Branch `feat/pr-97-camera-occlusion`. PR #124, pending merge. Reviewed head `d6589d96ca8362d94c69b64d7617c0980fc061ae`.

### What this session did, and why

- Reviewed the camera and model occlusion change, its exit tests, the PR comments, and the supplied frames.
- Verified the straight-distance correction in `ModelFade` and its corner regression test.
- Added the cross-provider review record. The review found no blocking defect and records the owner-accepted F-195 reduction (D-723).

### State of the build

- The PR code head was `d6589d96ca8362d94c69b64d7617c0980fc061ae`, based on `91dbaa25091e9da00f4d4ff5acd0a214c39d0dd7`.
- The build passed with no warnings or errors. The focused camera, grid-ray, model-fade, and render-interpolation tests passed, 45 tests.
- CI passed on the code head for the three-platform build and test, bit identity, smoke, bots, asset QA, det-lint, STE, documents, doc-gate, and night-gate. The review-gate and evaluate checks failed because the review record did not yet exist.
- The review record and this handoff were pushed together as a metadata commit. The effective code head remains `d6589d96ca8362d94c69b64d7617c0980fc061ae`.

### In flight

- GitHub must run the review-gate and evaluate checks on the metadata head.
- The owner can review the merge summary and merge PR #124.

### Traps and gotchas

- The record approves the code head above. The review and handoff commit changes metadata only.
- F-202 belongs to PR-99 under D-724. The damage numbers and stairwell prompt belong to PR-98 under D-716.

### Open questions that block progress

None.

### Next concrete action

Read the checks for the metadata head. Then give the owner the merge summary for PR #124.

## Session 311: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-97, author. Branch `feat/pr-97-camera-occlusion`. PR #124, pending merge. Base `91dbaa2`.

### What this session did, and why

- Asked the owner the three camera questions of D-715. The owner took each recommendation: D-719 (the shoulder 0.9 meters right and 0.7 meters up), D-720 (the drawn camera 2.0 meters behind the shoulder point, also in rock), and D-721 (the model dither).
- Wrote the camera in Core, the rock march in `GridRay`, the model fade in Game, and their tests. The simulation version rose to 19, and the bit-identity known answer moved to `1023ce079eb0af50` (G-20).
- Ran the captures of D-714 on the Mac. A drawn camera in rock showed the black background in 5 of 63 full-clearer frames. The owner took a background of dark stone (D-722).
- The owner told the session to check the frames of exit test 3 and confirm. F-196 and F-197 are not in the cited frames, and F-195 is reduced. The owner accepted the reduction (D-723). The frames found F-202, which PR-99 holds before Gate 2 (D-724).

### State of the build

- `main` is `91dbaa2`. The PR head carries the code, the frames in `docs/reviews/pr-124-frames/`, and the documents.
- Local runs at the code head: the suite outside Smoke passed, 2117 tests. The Smoke category passed with the local Godot, 14 tests. `det-lint` and `ste-check` passed.

### In flight

- CI, the gitar pass, and the cross-provider review through `make codex-review PR=124`.

### Traps and gotchas

- The bot turns up to 180 degrees in one tick, and the Game interpolates the camera positions in a straight line, so a frame shot can draw from the head (F-202). Do not read such a frame as a camera rule defect.
- A scratch test in `WhatYouCarry.Tests` can run a bot policy on a `SimulationLoop` and print `loop.Camera()` at each shot tick. It gives the camera of a frame shot fast. Delete it before a commit.
- Two of 62 full-clearer shots stop the drawn camera at 1.2 meters, at a thin wall with air behind it (D-720, D-723).

### Open questions that block progress

None.

### Next concrete action

Wait for the checks, answer the gitar pass, and run `make codex-review PR=124`. After the merge, start PR-98 (D-716).

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
