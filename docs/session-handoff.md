# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 206: 2026-09-22, Claude Code

Author: Claude Code
Session: PR-20, author. Branch `feat/pr-20-audio-synth-and-effects`. PR pending.

### What this session did, and why

- Asked the owner OQ-48 and every open answer of PR-20 before the code, and recorded D-450 to D-470.
- Built the sound tool: `audio-synth` renders each sound file to a WAV file, and `audio-analyze` writes the band levels of a CC0 reference into a spectral layer. The render holds no transcendental of the runtime, so the three platforms give equal bytes.
- The owner rejected the sfxr sounds of the first design, so D-459 and D-462 to D-469 moved the path twice: first to the spectral layer of analysed references, then to CC0 recordings for the pitched sounds.
- Shipped nine sounds: the sword swing, the sword hit, the footstep, the dodge, the player hit, the hunter spawn, the hunter step, the timer alarm, and the timer tick. The owner picked the reference of each one by ear from CC0 candidates.
- Gave Core five action events for the gameplay facts of the sounds (D-454), and the Game layer four buses, one player per sound, the stride rhythm of D-463, and the hunter pitch of D-455.
- Wrote the `Makefile` at the root, under an owner override of G-10 for a second concern in this PR.

### State of the build

- Build: 0 warnings and 0 errors. The base is `170f08c`, and no commit exists on the branch yet.
- The 38 audio tests pass, the STE check, the determinism lint, and the asset check each report 0 findings.
- The last full suite ran while the documents changed, so it needs one more run before the commit.
- The headless smoke session passed earlier in the session, with no leaked object and no error line.

### In flight

- The first commit, the push, the gitar pass, and the hand-over to Codex.
- The owner runs a play session of the nine sounds. The verdict of that session is not in yet.

### Traps and gotchas

- The dummy audio driver of a headless session mixes nothing, so a playback never ends and the engine reports it leaked. The bank plays nothing on that driver, and the boot line names the driver.
- A level of a spectral layer had a ceiling of 40 decibels, which clamped the loudest bands of three sounds. The ceiling is 80 now, and `ALoudRecordingDoesNotReachTheCeiling` guards it.
- The band noise of a spectral layer drops the pitch of a tone, so a horn or a bell must ship as its recording (D-467).
- `afplay` cannot play OGG, and `afconvert` writes the extensible WAV header. The reader takes that header.
- The Freesound key of the owner lives in `~/.zshrc` as `FREESOUND_API_KEY`, and never in this repository.

### Open questions that block progress

None. OQ-48 and OQ-182 are answered.

### Next concrete action

Run the full suite, commit, push, and answer the gitar pass. Then hand the PR to Codex for the cross-provider review. After the merge, a fresh session takes F-107 and F-108: an enemy walks no diagonal, and an enemy does not walk up a ramp. The owner saw both in the play session of this PR.

## Session 205: 2026-09-21, Codex

Author: Codex
Session: PR-86, reviewer. Branch `feat/pr-19-hud-and-navigation`. PR #86, pending owner merge.

### What this session did, and why

- Reviewed the complete PR-86 implementation and test diff for the HUD and controller navigation base.
- Added `docs/reviews/pr-86.md` with the cross-provider verdict for effective head `c64e1e2`.

### State of the build

- Build: 0 warnings and 0 errors at the remote tip `5ea0566`.
- Focused HUD, navigation, input, and stairwell tests passed: 30 tests.
- Det-lint, asset QA, and STE check passed with 0 findings.
- The Godot smoke session passed. The full test suite stalled before it returned a result.

### In flight

- The owner merge of PR #86.

### Traps and gotchas

- The PR tip `5ea0566` is metadata-only. The review effective head is `c64e1e2`.
- The pre-review evaluate failure reported the missing `docs/reviews/pr-86.md` record. It was not a product test failure.

### Open questions that block progress

None.

### Next concrete action

The owner can merge PR #86 after the review record and this handoff commit reach the remote branch.

## Session 204: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-19, author. Branch `feat/pr-19-hud-and-navigation`. PR #86, pending owner merge.

### What this session did, and why

- Dispatched the night on `main` at `32e7909` under D-440. Run 35655210549 passed, and the local `night-gate` command reads the record as a pass.
- Asked the owner the HUD answers that the roadmap named. D-441 to D-448 record them.
- Built the HUD, the layout scale, the damage numbers, the focus map, and the fixture screen. Added the tap and the hold at the stairwell prompt, the prompt device, and the `--hud-shot` fixture.
- The smoke session found that the accept action of Godot 4.7 has no controller input. The owner chose the A button (D-449).

### State of the build

- Build: 0 warnings and 0 errors. The Godot build check passed.
- Local tests: 1246 of 1246 outside the Smoke category, and 7 of 7 in it.
- Det-lint, asset QA, and STE check: 0 findings.
- The HUD shot rendered on the Mac, exit code 0.
- Remote PR head: the commit of this entry. Effective head: `c64e1e2`.

### In flight

- The gitar pass on PR #86, and the CI of the head.
- The cross-provider review of PR #86 after the gitar pass.

### Traps and gotchas

- A Game literal that is not a const, such as an exception message or a context key, is a det-lint finding. Use a const field.
- A static field of an engine type, such as `StringName`, runs engine code when a test first reads the class. Keep const names.
- The string rule of det-lint does not read an interpolated string. `HudReadsStringTable` reads it for the Ui directory.
- `StairwellHold` sets the interact bit on the release of a tap, and not on the press. A bot sends its intents past it.
- The HUD scale goes on the canvas layer alone. A root stretch changes the mouse motion of the look.

### Open questions that block progress

None.

### Next concrete action

Finish the gitar pass on PR #86, then hand the PR to the other provider for the review.

## Session 203: 2026-09-21, Codex

Author: Codex
Session: PR-18, reviewer, after the repeat review of session 202. Branch `feat/pr-18-stairwell-and-transition`. PR #85, pending owner merge.

### What this session did, and why

- Re-reviewed PR #85 at effective head `bab19cc` after the full-clearer timeout fix.
- Verified the original transition review trigger and the new clearer regression tests.
- Updated `docs/reviews/pr-85.md` with the current verdict and evidence.

### State of the build

- Build passed with 0 warnings and 0 errors.
- Focused transition, bot, and measurement tests passed, 11 of 11.
- Det-lint, asset QA, and STE check passed.
- PR head is `e32c6cb` on the remote. The effective head is `bab19cc`.
- The night gate is red on the prior base-branch record under D-440. The first night on `main` after merge must pass.

### In flight

- The owner must merge PR #85.

### Traps and gotchas

- The review gate turns green after the review record reaches the PR branch.
- The broad local test command stalled after the build. The author reported 1229 of 1229 tests, and focused tests passed locally.

### Open questions that block progress

None.

### Next concrete action

The owner merges PR #85. After the merge, start a new session and run the night on `main`.

## Session 202: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-18, author, after the review of session 201. Branch `feat/pr-18-stairwell-and-transition`. PR #85, pending owner merge.

### What this session did, and why

- Exit test 6 failed on the Deck at 52.8 ms. A trace of each transition found 38 to 46 ms of chunk mesh work in one main-thread frame. The worker task now meshes the chunks. The next Deck run passed at 11.1 ms on `c3ca60a`.
- The night of `ad7589a` read a full-clearer softlock on seeds 2100 and 2109. By D-438 this PR fixes it: the clearer leaves when the time runs short (D-439). The owner took the merge past the red night gate (D-440).
- The owner asked for the splash image off in this PR with no decision entry. The PR description records the override.
- Filed OQ-181, the antialiasing of the world, against PR-62.
- Rotated the handoff after session 201 left 11 entries.

### State of the build

- Local: 1229 of 1229 tests, the Smoke category included. A local sweep of 5000 full-clearer seeds read 0 softlocks.
- PR checks at `bab19cc`: every check green except `review-gate`, which waits for the repeat review, and `night-gate`, which D-440 overrides.
- The hand night on the branch, run 35638438679, passed every step.

### In flight

- The repeat cross-provider review of PR #85. The review of session 201 covered `46c4b9d`. The mesh fix, the clearer fix, and D-440 came after it.

### Traps and gotchas

- The Deck needs `git pull` and `dotnet build` before a run. A dig line with no `meshMicros` field means an old build.
- `FullClearerClearsFloor` (PR-16 exit test 5) now allows the enemies that a leaving clearer leaves alive (D-439).
- A descent on floor 15 still throws, because no template covers floor 16. PR-35 holds that stairwell.
- `main` has no branch protection, although D-387 asks for it.

### Open questions that block progress

None.

### Next concrete action

Codex: review PR #85 again at the effective head. After the owner merge, dispatch a night on `main` so that the night gate reads green (D-440).

## Session 201: 2026-09-21, Codex

Author: Codex
Session: PR-18, reviewer. Branch `feat/pr-18-stairwell-and-transition`. PR #85, pending owner merge.

### What this session did, and why

- Reviewed PR #85 at effective head `46c4b9d` under the cross-provider gate.
- Inspected the full diff, the transition state, the chunk swap, the bot and night workflows, the focused roadmap, the applicable decisions, and the Gitar comment.
- Wrote `docs/reviews/pr-85.md`. The record has no finding and a blocked verdict because the Steam Deck transition exit test remains unresolved.

### State of the build

- Remote PR head: `5bac1cd`.
- Build passed with 0 warnings and 0 errors.
- Focused transition, measurement, and smoke tests passed, 31 of 31.
- `det-lint`, `asset-qa`, and `ste-check` passed with 0 findings.
- A local three-transition headless bot session exited 0 and recorded `transitionMicrosMax` of 16667 microseconds.
- The full local test command produced no result after the build and was interrupted. CI reports the test, Smoke, Bit identity, Bots, Night gate, Asset QA, Determinism lint, Doc gate, STE check, and Gitar checks as passed.

### In flight

- The review record and this handoff entry are pushed in metadata commit `5bac1cd`.
- The owner must run the Deck transition command in `CLAUDE.md` and record exit test 6.

### Traps and gotchas

- The review applies to effective head `46c4b9d`, not metadata tip `14f0378`.
- The transition test removes enemy content by design under D-437.

### Open questions that block progress

- None. The Deck result is a required exit test, not an open owner question.

### Next concrete action

Wait for the owner to record the Deck result and rerun the review gate.

## Session 200: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-18, author. Branch `feat/pr-18-stairwell-and-transition`. PR #85, pending owner merge.

### What this session did, and why

- Asked the owner OQ-44, OQ-161, and the five PR-18 answers before any code. Later asked three more: the coward sweeps, the smoke order, and the enemies of the transition test. Recorded D-427 to D-437.
- Core: `NextFloorWorker`, a pure function of the seed and the floor (D-429), and `SimulationLoop.OfferNextFloor`. `StairwellPrompt` opens on the stairwell cell (D-431). The `coward` policy (D-433) and the `ascend` end state (D-430).
- Game: `ChunkSwap` digs on a task, uploads four chunks each frame, and swaps in one frame. Before this PR, the world mesh never changed after a descent. The prompt text, the smoke walk to the stairwell (D-436), and `--transitions` (D-435, D-437).
- Tools and workflows: the ascend count in the bot summary and the night record, and the coward in `bots.yml` and `night.yml` (D-434).

### State of the build

- Local: 1223 of 1223 tests, the Smoke category included. `det-lint`, `asset-qa`, and `ste-check` report 0 findings.
- The code head is `46c4b9d`. The status marks and this entry follow it in one docs commit. CI runs on the push.
- A local headless run with `--transitions 10` exits 0. Three of the ten swaps read `fromWorker: false`, because a headless run goes faster than real time.

### In flight

- PR #85 waits for CI, the automated pass, and the cross-provider review.
- Exit test 6 needs the owner: run the transition command of `CLAUDE.md` on the Deck (D-428). Exit 0 passes. Record `transitionMicrosMax` from the end line.

### Traps and gotchas

- The smoke script alone dies to the scavengers of seed 1 at tick 273. The walk comes first for that reason (D-436).
- The descender with enemies dies on floor 2 of seed 1. The transition test loads no enemy family (D-437).
- `OfferedFloorKeepsTheRunHash` waits on the task when the prompt opens. A test that needs the task to end first fails under the load of the full suite.
- A descent on floor 15 still throws, because no template covers floor 16. The bots ascend there. PR-35, the ending, holds that stairwell.

### Open questions that block progress

None. Exit test 6 waits on the Deck of the owner, and no question blocks it.

### Next concrete action

Wait for CI with the command of `docs/runbooks/session-context.md`, then load `gitar-review` and answer the automated pass. Then hand PR #85 to Codex for the cross-provider review.

## Session 199: 2026-09-21, Codex

Author: Codex
Session: PR-84, repeat cross-provider review. Branch `feat/pr-17-timer-and-hunter`. PR #84, ready for owner merge.

### What this session did, and why

- Reopened the review record after the author added the empty-post regression test.
- Recomputed the effective head as `5242ff6`.
- Verified that P1-1 does not reproduce and marked it withdrawn.
- Set the current verdict to `Ready for owner merge`.

### State of the build

- The focused timer suite passed 17 of 17 tests.
- The author reported 1208 of 1208 tests with the Smoke category.
- Required implementation checks and the automated pass are green at the new head.

### In flight

- The review record and this handoff are pushed at `1a2da98`.

### Traps and gotchas

- The prior finding stays in the review record as withdrawn.
- The effective head is the test commit `5242ff6`. The review commit remains metadata.

### Open questions that block progress

None.

### Next concrete action

The owner can merge PR #84 after the review-gate record turns green.

## Session 198: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-84, answer to the cross-provider review, author. Branch `feat/pr-17-timer-and-hunter`. Pending owner merge.

### What this session did, and why

- Answered the one finding of `docs/reviews/pr-84.md` in `docs/reviews/pr-84-response.md`.
- P1-1 has partial merit. The crash does not reproduce, because both modulo expressions of `Escalation.TryNextPost` sit inside a loop that does not run for an empty list. The missing test was real.
- Added `AFloorWithNoPostSkipsEveryWave` to `TimerTests`. It passed on the unchanged Core code. No Core change follows.

### State of the build

- The test commit moves the effective head, because `WhatYouCarry.Tests/` lies outside the metadata set (D-184). Read its hash from `git log`. It is the commit of this entry.
- The full local suite passed 1208 of 1208 with the Smoke category, in 8 minutes 39 seconds. `TimerTests` passed 17 of 17.
- The CI of the new head and the gitar pass follow the push.

### In flight

- The gitar pass of the new effective head.
- The repeat review by the other provider.

### Traps and gotchas

- The review record keeps P1-1 open until the repeat review sets its status. The author never edits the review record.
- The PR-16 night result stands in the entry of Session 196.

### Open questions that block progress

None.

### Next concrete action

The other provider runs the repeat review of PR #84 at the new effective head.

## Session 197: 2026-09-21, Codex

Author: Codex
Session: PR-17, the floor timer, the hunter, and the escalation, reviewer. Branch `feat/pr-17-timer-and-hunter`. PR #84, changes required.

### What this session did, and why

- Reviewed PR #84 at effective head `f000599` as the opposite provider.
- Found P1-1 in `Escalation`: an empty post list can crash at the first due wave, although D-410 and D-418 require a skipped wave.
- Added the review record at `docs/reviews/pr-84.md`.

### State of the build

- The focused timer suite passed 16 of 16 tests.
- The full suite did not finish during the review window and was interrupted.
- The remote PR head is `a33653a`.

### In flight

- The author must handle P1-1 and add the empty-post wave regression test.

### Traps and gotchas

- `TestWorld.PeacefulContent` has no enemy family and can produce an empty post list.
- The review record uses effective head `f000599`. The metadata commit does not change that head.

### Open questions that block progress

None.

### Next concrete action

The author fixes P1-1 and reruns the focused and full test suites.
