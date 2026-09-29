# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 321: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-100, author. Branch `feat/pr-100-enemy-swing-clip`. PR #128, pending merge. Base `9da1817`.

### What this session did, and why

- Asked OQ-211. The premise of its option one held for the scavenger alone: the pick swings 30, 6, and 30 ticks, and a clip plays tick for tick. D-741: the scavenger plays `player.sword-swing.json` on its own bones, and the Overseer plays its own clip `overseer.pick-swing.json`, an overhead chop of 66 ticks.
- The check of `asset-qa` paired a clip with the model that its file names, so it never posed the player clip on the scavenger. D-742: `asset-qa` reads the family, hunter, and weapon files, and it poses each enemy model with the swing clip of its weapon.
- The first run of that check found a clip of 0.0417 meters between the right upper arm and the hood of the scavenger at tick 18. D-743: the player clip turns the right arm to [70, 0, 0] at that tick.
- `EnemyNodes.AfterTick` poses each enemy tree and the Overseer from its swing tick through `EnemyPose`. `EnemyClips` checks each clip against its weapon and its model. `Main` logs a failure of the enemy trees and quits (T-2).
- Ran the Tier 4 captures of D-714 at the PR head. Tick 3060 is the one shot with a scavenger windup in front. The owner confirmed that the windup reads (D-744), and the frames are in `docs/reviews/pr-128-frames/`.

### State of the build

- The remote head of `main` is `9da1817`. Locally at the code commit `a03c756`: the full suite passes 2158 of 2158, the Smoke category included. `asset-qa`, `det-lint`, and `ste-check` give 0 findings. The bit-identity sweep gives the known answer `1023ce079eb0af50`, and the Godot build passes.

### In flight

- CI of PR #128, the automated pass, the cross-provider review through `make codex-review`, and the owner confirmation of the merge.

### Traps and gotchas

- A frame shot draws one tick in 60, and the windup of the scavenger lasts 12 ticks. A scan of the full-clearer run in Core found the shot ticks of a windup in advance. The player body or a wall hid most of them.
- The timer-tester never turns the camera, so no bot frame shows the windup of the Overseer (D-738).
- A test fixture that writes a partial family or hunter file now gets a load finding of its own from `asset-qa` (D-742).

### Open questions that block progress

None.

### Next concrete action

Wait for CI of PR #128, answer the automated pass, then run `make codex-review PR=128`.

## Session 320: 2026-09-28, Claude Code

Author: Claude Code
Session: Gate 2, author. Branch `docs/gate-2-first-playable`. PR #127, pending merge. Base `ee04af7`.

### What this session did, and why

- Read sequence item 57 of the Phase 2 roadmap, and each exit test of that file against the records. Three tests had no pass on record: PR-66 exit test 10, PR-79 exit test 4, and the failed-night proof of PR-91 exit test 24.
- PR-79 exit test 4 passes on PR #126: `review-gate` gave success on the documents commit `9909436` after the approving review `1c846f9`.
- The owner folded PR-66 exit test 10 into the Gate 2 play (D-735), and carried the failed-night proof past the gate (D-736). No night on `main` failed after PR #109.
- The session built `main` at `ee04af7`, and the smoke session passed. The owner played one floor and signed off on feel on two terms (D-740): the scavenger has no swing clip yet, and the world generation gets an overhaul later.
- Neither term was in the plan. D-739 adds PR-100, the enemy swing clip, before PR-21, with OQ-211 for the clip source. The windup check of D-723 moves to PR-100, and F-203 records the rest pose. D-737 closes PR-66 exit test 10, and OQ-210 holds the overhaul.
- The steps of the Overseer warn of an attack from outside the view, so F-198 closes (D-738).
- Added the missing done marks: sequence item 45 (PR #105) and the PR-76 entry (PR #117).

### State of the build

- The remote head of `main` is `ee04af7`. This PR changes documents alone. `ste-check` gives 0 findings, and the Documents category passes 257 of 257.

### In flight

- The automated pass, the `review-override` label, and the owner confirmation of the merge (D-188, D-524, D-533).

### Traps and gotchas

- An enemy draws in the rest pose (D-401), so no play or frame can read the windup of a scavenger before PR-100.
- The handoff of session 297 names the failed-night proof exit test 23 of PR-91. The roadmap puts it in exit test 24.

### Open questions that block progress

- OQ-211 blocks PR-100. OQ-210 blocks no PR.

### Next concrete action

After the merge, Phase 3 starts. Ask the owner OQ-211, then start PR-100 (D-739). The session after the first failed night on `main` states the result of the proof of D-736.

## Session 319: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-99, author. Branch `feat/pr-99-camera-turn-frames`. PR #126, pending merge. Base `ec383bf`.

### What this session did, and why

- Read the review of session 318: `Ready for owner merge` for the effective head 552874d, with no finding.
- Gitar approved 1dd704b with no finding and no thread.

### State of the build

- The full CI, smoke, bit identity, and bots passed at 4cd4f67, which holds the code of 552874d. The later commits change documents alone, so their heavy jobs skip (D-357, D-474). Before the review record, only `evaluate` and `review-gate` were red. The remote head of `main` is `ec383bf`.

### In flight

- The owner confirmation of the merge of PR #126 (D-524, D-533).

### Traps and gotchas

- The shots of D-714 match main to the pixel for any change between two ticks, because each frame of that capture has the fraction 0 (D-733).

### Open questions that block progress

None.

### Next concrete action

After the merge, Gate 2 is next: PR-99 was the last PR before it (D-724).

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
