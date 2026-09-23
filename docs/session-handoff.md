# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 211: 2026-09-22, Claude Code

Author: Claude Code
Session: PR-72, author. Branch `fix/pr-72-enemy-movement`. PR #90, pending owner merge. Base `ea84473`.

### What this session did, and why

- PR-71 exit test 6 passes. The push runs of `ea84473` on `main` ran every job of `ci.yml` (7 jobs), `smoke.yml` (4), `bit-identity.yml` (5), and `bots.yml` (2), and each passed. No job skipped. Each `ci-skip` log reads "The event 'push' runs every job. A push to main never skips (D-473)." Runs 35806594707, 35806594677, 35806594709, and 35806594708.
- Owner answers D-483 to D-489: the id PR-72, one PR for F-107 and F-108, the F-108 case (seed 1, floor 1), the diagonal rule, the costs 10 and 14, the walkers, and no diagonal step up past a corner.
- F-108: six probes on seed 1, floor 1 found no stall. The owner then said the enemy "slowly jumped up the ramp". `PathWalk.NeedsAJump` read the feet against the face of the next place, so 80 to 87 percent of the ticks of a climb were in the air. It now reads the slope under the feet at the entry point.
- F-107: `GridMoves.DiagonalMove` and the search costs of D-487. Three faults came out of the tests and a sweep of 431597 diagonal moves over 120 seeds: a two-block rise from two side steps, a climb cut out of a ramp, and a drop into a landing column with a block at body height (the Overseer of `TimerTesterAlwaysDies` seed 662). The rule now limits the rise in rows and in floor height, and it needs the start and landing columns open. D-489 closed the fourth: a step up past a corner lands short.

### State of the build

- Local: the full suite passed (1377 tests, Smoke apart), then Smoke 7 of 7, the Godot build check, `det-lint` 0, `ste-check` 0.
- Simulation version 15. Bit identity `dc4258105649a548`. With version 14 the new walk gives `5edea237bc4e2fae`, so the walk moved the hash too.
- Remote head before this entry: `b283abe`, not pushed yet.

### In flight

- The status marks `✅ Done in PR #90.` are in the roadmap and the design doc.
- The CI of the code head, then the gitar pass, then the hand-over to Codex.

### Traps and gotchas

- The walk sweep is `EnemyWalkTests.DiagonalSweep`, in the `Sweep` category, at a fixed 120 seeds (about 8 s local). A seed count on floor 1 alone missed every fault. The faults sat on floors 6 to 15.
- An arrival alone proves little. A follower that searches again walks the two side moves, so the sweep also counts the jumps.
- `GridMoves.FloorHeightAt` works in 24ths of a block, so the rule stays in integers (G-9).
- On `main`, the Session 210 entry sat above the `# Session handoff` title. `doc-gate` finds the newest entry by `\n## Session `, so it read Session 209 and failed this branch. This entry puts the title and the rule line back on top, with Session 210 unchanged under Session 211. Add an entry under the rule line.

### Open questions that block progress

None.

### Next concrete action

Finish the gitar pass on PR #90, then hand over to Codex for the review.

## Session 210: 2026-09-22, Codex

Author: Codex
Session: PR-71, reviewer. Branch `chore/ci-skip-for-document-heads`. PR #89, pending owner merge. Base `a2473ec`.

### What this session did, and why

- Reviewed PR #89 at effective head `05aa78a` for its CI skip, document tests, seed share, and test split.
- Found no defect. Added the review record under `docs/reviews/pr-89.md` for the owner and the review gate.

### State of the build

- The focused local suite passed: 216 tests, 0 failed, with `WYC_PR_SWEEP=1`.
- The full local suite passed: 1351 tests, 0 failed, with the full seed counts.
- CI, smoke, bit identity, bots, and the documents job passed on `05aa78a`.
- The documents push at `34109e6` passed the document, STE, doc-gate, det-lint, asset-QA, and night-gate checks. The four heavy workflows skipped by Rule 2. `evaluate` failed because the review record was not on the branch yet.
- The remote head before this metadata commit was `34109e6`.
- After review publication at `1f06e28`, `evaluate`, `review-gate`, and every required check passed. The heavy workflows skipped by Rule 2, and Gitar passed again.

### In flight

- Exit test 6 waits for the merge and the first push to `main`.
- The next session reads those workflow runs and records the result.

### Traps and gotchas

- `05aa78a` is the effective head. `34109e6` changes only `docs/session-handoff.md`.
- `--no-renames` lists both paths of a move. Keep `CodeMovedIntoTheSkipSetRunsEveryJob` as the guard for moves into the skip set.

### Open questions that block progress

None.

### Next concrete action

After the owner merges PR #89, read the first push to `main` for exit test 6.

## Session 209: 2026-09-22, Claude Code

Author: Claude Code
Session: PR-71, author. Branch `chore/ci-skip-for-document-heads`. PR #89, pending owner merge. Base `a2473ec` (PR #87 merged).

### What this session did, and why

- The owner asked for the CI skip of a documents head and a study of the CI duration, in one PR (D-471). The PR description records that exception to G-10.
- The new `ci-skip` command and the composite action `.github/actions/ci-skip` skip the heavy jobs of `ci.yml`, `bit-identity.yml`, `smoke.yml`, and `bots.yml` (D-472 to D-477). Rule 1 covers a PR of documents alone. Rule 2 covers a push of documents after a head whose run of that workflow passed.
- Each test class that reads a document carries the category `Documents`, and the `documents` job runs it on each head (D-476).
- F-109 records the CI duration. The fixes: a class split of `ProcgenTests` (D-478), two jobs on each hosted leg (D-479), and one fifth of each seed sweep on a pull request (D-480, D-481). No NuGet cache (D-482).

### State of the build

- Build: 0 warnings and 0 errors. `ste-check` and `det-lint` report 0 findings.
- Local suite with `WYC_PR_SWEEP=1`: 1343 passed in 1 minute 47 seconds. At the full count: 1343 passed in 5 minutes 9 seconds. Before the change: 1294 tests in 9 minutes 10 seconds.
- The effective head is `05aa78a`, the fix of the one gitar finding. Gitar approved it, and every check passed except `evaluate`, which waits for the review record (D-251).
- CI of `05aa78a` against run 35771495463 of PR #87, the test step alone: Linux 1055 to 384 seconds, with 204 in `linux-x64-sweeps`. Windows 1206 to 437, with 88 in `windows-x64-sweeps`. Mac mini 451 to 113. The `documents` job took 32 seconds.

### In flight

- The hand-over of PR #89 to Codex for the review.
- Exit test 4: the push of this entry is a documents push after the green head `05aa78a`, so the heavy jobs of the four workflows skip by rule 2. The PR comment of exit tests 4 and 5 holds the result.
- On the hosted legs the rest job is slower than the sweeps job: 384 against 204 seconds on Linux. A move of `EnemyTests` into the `Sweep` category can balance them, and the owner decides.

### Traps and gotchas

- A skipped job reports success. The `!cancelled()` condition runs every heavy job when `ci-skip` fails, so a fault never passes in silence.
- xUnit reads no trait of an outer class on a nested class. Each nested class of `ProcgenTests` carries its own `Sweep` trait, and `EveryNestedClassOfASweepClassTakesTheCategory` checks it.
- A class that calls a command joins the console collection, or `EveryConsoleTestIsInTheCollection` fails.
- `git diff --name-only` hides the old path of a move. `ChangedPaths` passes `--no-renames`, so a code file moved into `docs/` still runs every job (gitar finding on PR #89, `CodeMovedIntoTheSkipSetRunsEveryJob`).
- `CLAUDE.md` sits 43 bytes under the ceiling of D-382, so the reviewer rule for a skipped job lives in the pr-review verification reference.
- `main` has no branch protection yet (D-387), so no required check reads the skipped jobs today.

### Open questions that block progress

None. The owner answered each question of this PR: D-471 to D-482.

### Next concrete action

Hand PR #89 to Codex for the review. After the merge, the next session reads the first push to `main` for exit test 6 (D-473).

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
