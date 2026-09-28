# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 318: 2026-09-28, Codex

Author: Codex
Session: PR-99, reviewer. Branch `feat/pr-99-camera-turn-frames`. PR #126, pending merge. Base `ec383bf`.

### What this session did, and why

- Reviewed the camera interpolation change, its tests, its frame evidence, and the PR contracts.
- Added the review record for effective head `552874d`. The provider gate passed. The review found no blocking defect.

### State of the build

- Local focused tests passed, 6 tests. The full suite passed, 2144 tests, on macOS arm64 with .NET 10.
- GitHub run `36492529592` passed the three-platform CI and smoke checks on the same code. The later PR commits changed documents and frame evidence only.
- The remote PR branch head at review start was `1dd704b`. The review record and this handoff are in the metadata commit pushed to the PR branch.

### In flight

- The owner can review and merge PR #126.

### Traps and gotchas

- Gitar's approval summary named no specific item. D-550 classifies it as a notice.
- The frame pairs show the accepted camera position inside rock after tick 2800. D-733 and D-734 record this result.

### Open questions that block progress

None.

### Next concrete action

Review the merge summary, then merge PR #126 when ready.

## Session 317: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-99, author. Branch `feat/pr-99-camera-turn-frames`. PR #126, pending merge. Base `ec383bf`.

### What this session did, and why

- F-202 (D-724): the frame interpolated the two tick poses in a straight line, so a turn of 180 degrees drew the camera from the head. The Game now interpolates the look (`TickLook`), the yaw the short way, and places the pose of each frame with `OrbitCamera.Place`.
- Exit test 1 fails on the pose interpolation of `main`: 1.5 meters in place of 3.0 at the fraction 0.25.
- One placement costs about 0.7 microseconds on the spawn of seed 1, in a Release build on the Mac.
- The shots of D-714 draw each frame at the fraction 0, so they cannot show F-202. The owner chose pairs of frames at a fraction of 0.5 as the evidence, and accepted the close frames of D-720 at the interpolated yaw (D-733). The owner confirmed exit test 3 (D-734), after a correction: the halfway frames with no turn differ in up to 1.1 percent of the pixels.

### State of the build

- Local: the full suite (2144 tests, Smoke included), `det-lint`, `ste-check`, and the Godot build pass. The remote head of `main` is `ec383bf`.

### In flight

- The gitar pass and the cross-provider review. The owner confirmed exit test 3 on the frames in `docs/reviews/pr-126-frames/` (D-734).

### Traps and gotchas

- The Game bot session ends one second after the first descent (PR-18), so the full-clearer capture ends at tick 3821 on floor 2. The death at tick 7140 of D-714 comes from `bot-run`.
- A frame shot never shows a frame between two ticks (D-733). For a turn frame, patch `FrameShots.IsShotTick` in a scratch worktree, and run with `--fixed-fps 120`.

### Open questions that block progress

None.

### Next concrete action

Answer the gitar pass. Then run `make codex-review PR=126` when each check but the Review gate workflow is green.

## Session 316: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-98, author. Branch `feat/pr-98-damage-numbers-prompt`. PR #125, pending merge. Base `db8e8cd`.

### What this session did, and why

- Read the review of session 315: `Ready for owner merge` for the effective head b446cdd, with no finding.
- Gitar approved b446cdd, and its one thread is resolved. The D-251 note for the review-gate line of the dashboard is posted.

### State of the build

- Each check of PR #125 passed at 8e29146, except `evaluate` and `review-gate`, which waited for the review record. The effective head stays b446cdd.

### In flight

- The owner confirmation of the merge of PR #125 (D-524, D-533).

### Traps and gotchas

- macOS has no `timeout` command. A CI wait that starts with it ends at once with exit code 8, and every check reads pending.

### Open questions that block progress

None.

### Next concrete action

After the merge, start PR-99 (D-724), the last PR before Gate 2.

## Session 315: 2026-09-28, Codex

Author: Codex
Session: PR-98, reviewer. Branch `feat/pr-98-damage-numbers-prompt`. PR #125, pending merge. Base `db8e8cd`.

### What this session did, and why

- Reviewed the code, tests, documents, comments, and frames of PR #125. No in-scope finding remains.
- Added the review record for effective head `b446cdd` and this entry in one metadata commit (D-182).

### State of the build

- The local HUD and number-sight tests passed, 28 tests. The remote CI checks for work head `b446cdd` passed through documents-only tip `8e29146` (D-357).
- The `review-gate` and dependent `evaluate` checks failed because the review record did not yet exist. They need a fresh run after this push.

### In flight

- The review record and this entry await the metadata commit and push to the PR branch.

### Traps and gotchas

- `GridRay.FirstSolid` throws when a march starts in rock. The grazing-edge test checks the step past the first open cell.
- The PR tip includes documents after the effective work head. The review applies to `b446cdd`.

### Open questions that block progress

None.

### Next concrete action

Wait for the review-gate checks, then give the owner the merge summary for PR #125.

## Session 314: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-98, author. Branch `feat/pr-98-damage-numbers-prompt`. PR #125, pending merge. Base `db8e8cd`.

### What this session did, and why

- Asked the owner the four questions of PR-98 and two more that the frames raised. The answers are D-725 to D-731: numbers of 28 pixels, a black outline of 3 pixels, a hidden owner rule, a ray that skips the rock of the drawn camera, the prompt at the bottom right, no number for a dead owner, and a hold of 0.5 seconds before the fade.
- The code: `DamageNumbers.cs`, `Hud.cs`, `HudLayout.cs`, and the new `NumberSight.cs`, with tests in `HudTests.cs` and `NumberSightTests.cs`.
- Captured the HUD shot and the two runs of D-714 at the PR head. The owner confirmed that F-199 and F-201 are gone, and F-24 closes (D-732). The frames are in `docs/reviews/pr-125-frames/`.
- Answered the one gitar finding with b446cdd: a step past the rock can land in the next block, and the march from there threw. The number now hides there. The test `AStepPastAnEdgeIntoRockHides` fails on 3248bae.

### State of the build

- The full suite passed locally, 2141 tests, at b446cdd. `det-lint` and `ste-check` have no finding. The effective head is b446cdd.
- The captures end as at PR-96: 63 and 214 frames, and the timer-tester dies at tick 12873.

### In flight

- CI and the gitar pass of PR #125, then `make codex-review PR=125`.

### Traps and gotchas

- The HUD outline of the engine size 4 draws a band of 1.0 pixel, which does not read on the dark stone. An enlarged crop hid it: count the pixels, and do not judge by eye.
- A dead enemy keeps its place and its body box (D-322). A rule that reads the enemy boxes must skip the dead ones.
- `GridRay.FirstSolid` throws on a start in rock, and the drawn camera can stand in rock (D-720). `NumberSight` skips that rock first (D-729), and a start that lands in a block after the step past the face hides the number.
- The prompt test checks the body box. With the model margin of 0.3 meters and a wall on the right, the box reaches the left edge of the prompt box at a pitch up. The prompt text stands about 280 pixels farther right.

### Open questions that block progress

None.

### Next concrete action

Answer the gitar pass of PR #125, then start `make codex-review PR=125` when each check but the Review gate workflow is green. After the merge, PR-99 (D-724).

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
