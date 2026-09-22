# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 209: 2026-09-22, Claude Code

Author: Claude Code
Session: gitar suspension, author. Branch `chore/suspend-gitar-pass`. PR #88, pending owner merge.

### What this session did, and why

- The gitar subscription expires. The owner asked for a temporary change that removes the gitar review from the PR gate, and that is easy to reverse.
- A survey found that no branch protection, ruleset, workflow, or tool requires gitar. The requirement lives in the documents alone, so CI does not change.
- Recorded D-471 with three owner answers. Gitar comments that still arrive get an answer. The suspension applies to this PR. The gitar app stays installed.
- Replaced the gitar rules in the agent files, the PR template, the `review-response` skill, and section 3.14 of the design doc. Added a suspension section to the `gitar-review` skill and a skip note to the runbook wait.
- Added `.claude/skills/gitar-review/references/restore.md`. It holds the original text word for word and the restore steps.

### State of the build

- Build: 0 warnings and 0 errors. The full suite passed: 1301 tests, 0 failures, 0 skips. `ste-check` reports 0 findings.
- The effective head is `9996282`. This entry is a metadata commit on top of it.
- The PR changes no code, so it takes the `review-override` label (D-188, D-190).

### In flight

- The CI wait for PR #88, and then the owner merge.

### Traps and gotchas

- The restore is a new decision that supersedes D-471, and not a git revert. A revert deletes the D-471 row.
- `grep -rn 'D-471' --include='*.md' . .github` finds each suspended text.
- The agent files are near the byte ceiling of 15000. The full original section moved to the restore file for that reason.
- The description of the `gitar-review` skill still says to load it after each push. Its suspension section limits the use to the answer of a comment.

### Open questions that block progress

None.

### Next concrete action

Wait for the checks of PR #88, and apply the `review-override` label. When the owner renews gitar, a fresh session follows the restore file.

## Session 208: 2026-09-22, Codex

Author: Codex
Session: PR-20, reviewer. Branch `feat/pr-20-audio-synth-and-effects`. PR #87, review ready; publication CI pending.

### What this session did, and why

- Re-reviewed PR #87 after the author fixed P2-1 and P2-2. The effective head is `996b3c3`; the later commit `de98480` changes only this handoff.
- Both earlier findings are fixed. The path-name check and the overflow-safe WAV bound have six regression cases in total.
- Updated `docs/reviews/pr-87.md` to record the fixes and the `Ready for owner merge` verdict.

### State of the build

- Build: 0 warnings and 0 errors. The full suite passed: 1301 tests, 0 failures, 0 skips. `det-lint`, `asset-qa`, and `ste-check` each report 0 findings.
- Before this review publication, the remote tip was `de98480`; the effective head remains `996b3c3`.
- The CI run for effective head `996b3c3` passed Linux, macOS, Windows, Gitar, bots, `doc-gate`, `night-gate`, `det-lint`, `asset-qa`, `ste-check`, and compare. `evaluate` and `review-gate` failed because the review record still had its earlier verdict.
- The review publication is in flight. Its fresh checks must pass before owner merge.

### In flight

- The push of the review record and this handoff, followed by the CI wait for the updated record.

### Traps and gotchas

- P2-1 rejected `../../../pwn`, directory names, dot segments, and empty names before it built paths.
- P2-2 rejected chunk lengths 2147483640, `int.MaxValue`, and 40 with a contextual WAV error.
- `de98480` is a metadata commit. It does not change the effective head under D-184.
- F-107 and F-108 remain in the later gameplay PR.

### Open questions that block progress

None. OQ-48 and OQ-182 are resolved by D-450 to D-470.

### Next concrete action

Wait for all checks after the review publication to pass, then hand PR #87 to the owner for merge.

## Session 207: 2026-09-22, Codex

Author: Codex
Session: PR-20, reviewer. Branch `feat/pr-20-audio-synth-and-effects`. PR #87, changes required.

### What this session did, and why

- Reviewed the complete PR #87 change at effective head `06cd3b3` for the contracts of D-450 to D-470 and the five exit tests.
- Found two input defects: `audio-analyze` accepts path traversal, and `WavReader` overflows on a malformed chunk length.
- Wrote `docs/reviews/pr-87.md` with the findings and the `Changes required` verdict.

### State of the build

- The full suite passed: 1295 tests, 0 failures, 0 skips, in 7 minutes and 35 seconds. The STE check, determinism lint, and asset check each report 0 findings.
- At the last status read before publication, the remote head was `5853e7b`, and the effective head was `06cd3b3` (D-184). Linux and Windows jobs were in progress. `evaluate` failed because the review record was absent, and `review-gate` was neutral for the same reason.
- The checks wait ran for about 14 minutes without a result, then stopped. It did not establish a pass or a failure for the pending jobs.

### In flight

- The author must correct P2-1 and P2-2 with regression tests, then request a repeat review.

### Traps and gotchas

- `audio-analyze --sound ../../../pwn` wrote `pwn.json` at the checkout root in an isolated run.
- A 20-byte RIFF file with chunk length `2147483640` made `WavReader` throw an unhandled `ArgumentOutOfRangeException`.
- The two pending platform jobs do not count as complete evidence.
- F-107 and F-108 remain in the later gameplay PR.

### Open questions that block progress

None. OQ-48 and OQ-182 are answered. The two findings block owner merge until the author corrects them.

### Next concrete action

The author adds a name check to `audio-analyze`, makes the WAV chunk bounds overflow-safe, adds regression tests, and requests a repeat review.

## Session 206: 2026-09-22, Claude Code

Author: Claude Code
Session: PR-20, author. Branch `feat/pr-20-audio-synth-and-effects`. PR #87, pending owner merge.

### What this session did, and why

- Asked the owner OQ-48 and every open answer of PR-20 before the code, and recorded D-450 to D-470.
- Built the sound tool: `audio-synth` renders each sound file to a WAV file, and `audio-analyze` writes the band levels of a CC0 reference into a spectral layer. The render holds no transcendental of the runtime, so the three platforms give equal bytes.
- The owner rejected the sfxr sounds of the first design, so D-459 and D-462 to D-469 moved the path twice: first to the spectral layer of analysed references, then to CC0 recordings for the pitched sounds.
- Shipped nine sounds: the sword swing, the sword hit, the footstep, the dodge, the player hit, the hunter spawn, the hunter step, the timer alarm, and the timer tick. The owner picked the reference of each one by ear from CC0 candidates.
- Gave Core five action events for the gameplay facts of the sounds (D-454), and the Game layer four buses, one player per sound, the stride rhythm of D-463, and the hunter pitch of D-455.
- Wrote the `Makefile` at the root, under an owner override of G-10 for a second concern in this PR.

### State of the build

- Build: 0 warnings and 0 errors. The remote head is `77f215d`, and the effective head is `06cd3b3` (D-184).
- The full suite passed on the tree of the first commit: 1295 tests, 0 failures, 7 minutes and 55 seconds.
- The STE check, the determinism lint, and the asset check each report 0 findings.
- Every CI check of the effective head passed: the three platforms of CI, of bit identity, and of the smoke session, with `doc-gate`, `night-gate`, `det-lint`, `asset-qa`, `ste-check`, and `bots`.
- The headless smoke session passed with no leaked object and no error line. The play session of the owner ended clean, and the sound bank loaded nine files on the CoreAudio driver.
- Gitar approved the effective head with no finding, and the review has no open thread. The `evaluate` check fails until a review record exists (D-251).

### In flight

- The owner merge of PR #87, after Codex confirms the corrections of the review.

### The review of Codex, and the answer

- Codex reviewed the effective head `06cd3b3` and asked for changes: `docs/reviews/pr-87.md`.
- P2-1: the analysis command joined the sound name into a path, so `--sound ../../../pwn` wrote a file outside the sound directory. Both findings reproduced before the correction.
- P2-2: a chunk length near the top of an int made `body + length` turn negative, so a malformed reference raised an unhandled error with no file and no chunk in it.
- Both have full merit. The corrections and the evidence stand in `docs/reviews/pr-87-response.md`. Six new test cases cover them, and each one fails on the code before the correction.
- The corrections went to the remote as `996b3c3`, which is the effective head now. The full suite passed on that tree: 1301 tests, 0 failures, 7 minutes and 13 seconds.
- The automated pass of gitar ran on `996b3c3` and approved it with no finding and no open thread. The pass of the earlier head `06cd3b3` also had no finding, and no session asked for a manual review.
- Every CI check of `996b3c3` passed but two: `evaluate` and `review-gate` fail while the review record holds the verdict `Changes required` (D-251). The reviewer alone changes that verdict.

### Traps and gotchas

- The dummy audio driver of a headless session mixes nothing, so a playback never ends and the engine reports it leaked. The bank plays nothing on that driver, and the boot line names the driver.
- A level of a spectral layer had a ceiling of 40 decibels, which clamped the loudest bands of three sounds. The ceiling is 80 now, and `ALoudRecordingDoesNotReachTheCeiling` guards it.
- The band noise of a spectral layer drops the pitch of a tone, so a horn or a bell must ship as its recording (D-467).
- `afplay` cannot play OGG, and `afconvert` writes the extensible WAV header. The reader takes that header.
- The Freesound key of the owner lives in `~/.zshrc` as `FREESOUND_API_KEY`, and never in this repository.

### Open questions that block progress

None. OQ-48 and OQ-182 are answered.

### Next concrete action

Codex reviews PR #87 and writes `docs/reviews/pr-87.md` for the effective head `06cd3b3`.

The next PR after this one skips the heavy checks on a head that changes documents alone. The owner asks for it before the bug fixes, and it takes the pattern of PR #54 of the repository `the-thing-below`. The owner wants no document change for it in PR #87, because a change outside the metadata set moves the effective head and starts CI again. The plan:

- A new command of Tools answers one question: does every path of a diff belong to the documents? The rule lives in C# with its own tests, as `doc-gate` and `night-gate` do. A path under `content/`, `.github/workflows/`, or any project directory is never a document.
- The workflows `ci.yml`, `bit-identity.yml`, and `smoke.yml` gain a first job on Linux that runs the command and gives a boolean output. Each platform job takes that job in `needs`, and runs under `if`. A skipped job reports success to the branch rules, so the PR gate stays green with no job that hangs.
- The cheap checks always run: `ste-check`, `doc-gate`, `review-gate`, `night-gate`, `det-lint`, and `asset-qa`. They read the documents, so a document change must not skip them.
- The skip reads the diff from the base of the pull request to the head, so a pull request of documents alone skips from its first push, and a pull request with code runs every check on every push, as today.
- This automates D-357, which a session applies by hand today, and it frees the one Mac runner (D-356, F-99).
- Risks to check first: a skipped required check must count as a pass in the branch ruleset, the skip must not move what D-184 names the effective head, and the list of document paths must be narrow.
- The owner answers three questions in that session: the id of the entry in the roadmap, whether `det-lint` and `asset-qa` skip too, and whether a push to `main` skips as well.

That PR also asks why the checks take so long, which the owner saw on a Windows check of 24 minutes. The one test step of `ci.yml` holds the whole cost: 1055 seconds on the hosted Linux runner, 1206 on the hosted Windows runner, and 451 on the self-hosted Mac mini. The run of `996b3c3` gives a second reading: 872 seconds on Linux, 1518 on Windows, and 489 on the Mac mini, so the hosted runners also vary from run to run. The smoke workflow takes 48 seconds, 103 seconds, and 29 seconds, and the bit identity takes 33 seconds or less, so neither one is the cost. A profile of the suite on 2026-09-22 names ten tests that hold 73 percent of the summed duration of 20.3 minutes, and every one is a seed sweep: `EveryFloorTakesATierWhenOneFits` at 177 seconds, `ReplayReproducesHash` at 132, `EveryPolicyEndsAtTheBottomOrByDeath` at 118, `StairwellReachable` at 98, `CameraNeverInsideSolid` at 97, and `EnemyCountMatchesBudget` at 94. A smaller sweep on a pull request weakens the gate, so the owner decides any change of a seed count.

After that PR: F-107 and F-108, an enemy walks no diagonal, and an enemy does not walk up a ramp. The owner saw both in the play session of PR #87.

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
