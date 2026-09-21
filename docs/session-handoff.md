# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

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

## Session 196: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-17, the floor timer, the hunter, and the escalation, author. Branch `feat/pr-17-timer-and-hunter`. PR #84, pending owner merge.

### What this session did, and why

- Asked the owner OQ-4, OQ-6, and every rule of the hunt before code. The answers are D-407 to D-426. OQ-4 and OQ-6 are resolved.
- Revised in part: D-271 by D-420, D-320 by D-422, D-400 by D-419, and D-403 by D-411.
- Added `FloorTimer`, `Hunter` (the Overseer), `Escalation`, `TimerEvent`, `HunterDefinition`, and the `TimerTester` policy.
- Added `content/hunter/overseer.json`, `content/weapons/overseer-pick.json`, and the timer and wave fields of each floor template.
- A death carries its cause. The bot log writes the timer events, and the night record gains `deathCauses` of each policy.
- A policy that promises progress reads softlock at expiry (D-420). The timer tester joins the PR bot job and the night (D-426).
- The Game draws the wave enemies and the Overseer with the body model as a fixture.
- Raised the simulation version to 14. The bit-identity answer moved to `6f7da3d2313688bd`.

### State of the build

- Base `a5461d4`. The code head is `4e85570`, and the effective head is `f000599`, because that commit changes the design doc and the roadmap (D-184).
- The local suite passed 1206 of 1207 before the handoff rotation. The one failure was `RepositoryFilesHoldTheRule`, which this rotation repairs.
- det-lint, asset-qa, ste-check, the Godot build, and the smoke session passed.
- PR-17 exit tests 1 to 7 pass. Exit test 6 measured expiry on 0 of 2293 floors.
- All PR #84 checks passed at `f000599`. `evaluate` reads red until the review record exists (D-251).
- The gitar pass is current and found no issues. Its CI note got a reply with D-251.
- Exit test 7 of PR-16 passed: night run 35561635447 on `main` at `a5461d4` ended in success.
  - Random walker: 0 crashes, 0 softlocks, 4839 deaths, 161 by budget.
  - Greedy descender: 0 crashes, 0 softlocks, 4669 deaths, 331 at the bottom.
  - Full clearer: 0 crashes, 0 softlocks, 3928 deaths, 1072 at the bottom. This is the first night of the full clearer at 5000 seeds.
  - The `night.json` of that night is the first to carry the deaths of each policy (D-403): 4839, 4669, and 3928.

### In flight

- The cross-provider review of PR #84 at effective head `f000599`.

### Traps and gotchas

- `main` at `a5461d4` is red on CI. Session 195 sat at the end of the handoff, and 11 entries stood in the file. The rotation of this session moves 195 back and 185 to the archive.
- The timer and wave fields are required in every floor template. A test content set needs them and one `hunter/` file.
- The Overseer spawn is a fault on a floor with no hidden reachable cell (D-415). The night reports each such floor as a crash.
- Exit tests 5 and 6 run their seeds in parallel. Core holds no mutable static state.
- The pick reuses the sword model and animation until PR-14. Its low and high blade heights copy the sword.
- The night now runs four policies, so it takes more wall time on the Mac runner.

### Open questions that block progress

None.

### Next concrete action

The other provider reviews PR #84 at effective head `f000599`.

## Session 195: 2026-09-20, Codex

Author: Codex
Session: PR-83, repeat cross-provider review. Branch `feat/pr-16-enemies-and-pathfinder`.

### What this session did, and why

- Reopened PR #83 after the D-406 handoff rotation correction.
- Reviewed the new substantive diff at effective head `f698cd9`.
- Verified the handoff tests and all required implementation checks.
- Updated `docs/reviews/pr-83.md` with the verdict `Ready for owner merge`.

### State of the build

- `HandoffRotateTests` passed, 9 of 9.
- The three build legs, three smoke legs, bit identity, compare, bots, asset-qa, det-lint, STE, doc-gate, and night-gate pass for the corrected tip.
- `evaluate` and `review-gate` wait for this review record. The effective head is `f698cd9`.

### In flight

The approving repeat-review record is pushed in metadata commit `a07243a`.

### Traps and gotchas

- D-406 changes substantive tool and test paths, so the effective head moved from `8452dbd` to `f698cd9`.
- Duplicate session numbers remain an error case.
- The owner merges the PR.

### Open questions that block progress

None.

### Next concrete action

Verify the remote review-gate result after the record publishes.

## Session 194: 2026-09-20, Codex

Author: Codex
Session: PR-83, repeat cross-provider review. Branch `feat/pr-16-enemies-and-pathfinder`.

### What this session did, and why

- Reopened PR #83 after the response to Session 193.
- Verified the response, the handoff-order correction, and the focused handoff test.
- Updated `docs/reviews/pr-83.md` and kept the verdict blocked because fresh required CI remains pending.

### State of the build

- `HandoffRotateTests` passed, 9 of 9.
- The response reports the completed broad suite and Smoke results. The corrected tip has completed passing jobs for bots, smoke, asset-qa, det-lint, STE, doc-gate, and night-gate.
- Fresh Linux, macOS, and Windows CI jobs remain pending. The effective code head remains `8452dbd`.

### In flight

The repeat-review record is pushed in metadata commit `8c385ce`.

### Traps and gotchas

- Later commits remain metadata-only, so the effective code head stays `8452dbd`.
- The review gate cannot pass while this record has a blocked verdict.
- Pending CI is incomplete evidence.

### Open questions that block progress

None.

### Next concrete action

Recheck the fresh CI jobs before the owner merge.

## Session 193: 2026-09-20, Codex

Author: Codex
Session: PR-83, cross-provider review. Branch `feat/pr-16-enemies-and-pathfinder`.

### What this session did, and why

- Reviewed PR #83 at effective head `8452dbd`.
- Inspected the pathfinder, movement rules, enemy and combat state, AI, procgen, simulation, bots, workflows, tests, and PR comments.
- Added `docs/reviews/pr-83.md` with no code finding and a blocked verdict because required CI remains pending.

### State of the build

- Focused tests passed, 6 of 6.
- The broad non-smoke test run compiled but stalled without a result.
- GitHub checks pass for completed jobs, but Linux and Windows CI remain pending. The review-gate check waits for this review record.
- Remote code head remains `8452dbd`. Metadata commit `9fb8fda` carries the review record and this entry.

### In flight

The review record and this handoff entry are pushed to the PR branch.

### Traps and gotchas

- The effective head excludes later handoff-only commits.
- A stalled test run is incomplete evidence, not a pass.
- The review stays blocked until required CI completes.

### Open questions that block progress

None.

### Next concrete action

Recheck the pending Linux and Windows CI jobs before the owner merge.

## Session 192: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-16, the first enemy family, the AI, and the pathfinder. Branch `feat/pr-16-enemies-and-pathfinder`.

### What this session did, and why

- Repaired `main`. The entry of Session 191 sat at the end of `docs/session-handoff.md`, so `HandoffRotateTests` failed on every CI leg of any branch (D-146). The entry moved to the top, word for word, and `handoff-rotate` ran.
- Asked the owner OQ-9 before any enemy content, as the roadmap and D-124 require. D-395 to D-405 hold eleven answers over four batches: the eight families, the scavenger as the first one, its gear, its numbers, its cadence, the empty spawn chamber, the wake rule, the drawing, the `death` end state, the descender that fights, and the wedge fix inside this PR.
- Moved the move rule of D-165 and D-345 out of `Reachability` into `Core/Pathfinding/GridMoves.cs`, and wrote the A* search of D-76 over it. The generator and the AI now read one rule.
- Wrote the scavenger, the brain, the spawn pass, the full clearer, and the drawing of each enemy. The player of PR-15 now reads the shared `Swing` and `Stagger` of `Core/Combat`.
- Found and fixed F-104 and F-105, two walk defects that only the new movement reaches. The measured softlock rate of a bot fell from about 10 percent to zero over 2000 seeds of each policy.

### State of the build

- Base and merge base: `e1c20ea`, the merge of PR #82.
- `dotnet test` locally: 1172 passed, 0 failed, with the filter `Category!=Smoke`. The Smoke category passed 5 of 5 against the pinned binary. `det-lint` reports 0 findings in Core and 0 in Game. `ste-check` reports 0 findings over 34 files. `asset-qa` reports 0 findings.
- The simulation version is 13, and the bit-identity answer is `d701dca6d5cee4d8`.
- The bot sweep of seeds 1 to 100 passed for all three policies: 0 crashes and 0 softlocks. Over 2000 seeds, the descender reaches the bottom on 129 and the clearer on 421, and neither reads a crash or a softlock.
- Exit test 8 of PR-66, the night sweep on `main` at `e1c20ea`: pass. Run 35542505776 ended `success`. The random walker ran out its budget on all 5000 seeds, the greedy descender reached the bottom on all 5000, and neither read a softlock or a crash. The reachability sweep of 100000 seeds passed in the same job. So the ramps and the tiers hold over the night.
- That night ran the workflow of `main`, which plays two policies and writes no death count. The third policy and the death count of D-403 are on this branch, so the next night after the merge plays three.
- Remote head: `origin/feat/pr-16-enemies-and-pathfinder` reached `e766b23`, and the status showed no `[ahead N]`. That commit and the one before it change `docs/session-handoff.md` alone, so the effective head is `8452dbd` (D-184). The commit `8452dbd` changes `docs/design.md` and `docs/roadmaps/`, which sit outside the metadata set, so it moves the effective head.
- Every check of PR #83 passes at the tip `cc70827`: Gitar, the three build legs, the three smoke legs, bit identity, compare, bots, asset-qa, det-lint, ste-check, doc-gate, and night-gate. The build legs took 11 minutes 39 seconds on Linux, 8 minutes 4 seconds on macOS, and 16 minutes 1 second on Windows. `evaluate` and `review-gate` fail, because the review record holds the verdict `Blocked` (D-179, D-181, D-185).

### In flight

GitHub PR #83 holds the whole PR-16 scope, the eleven decisions, the two findings, and this entry. It waits for the owner merge.

The review record `docs/reviews/pr-83.md` of Session 193 reports no code finding, and its verdict is `Blocked` for the effective head `8452dbd`, because required CI was pending and a local broad test run stalled. `docs/reviews/pr-83-response.md` answers both limits: the suite completed at 1172 passed and 0 failed at `0075b92`, and the three CI legs failed on the handoff order of D-146 alone. The entry of Session 193 sat under Session 183, and this session moved it to the top, word for word, and ran `handoff-rotate`. The verdict stays as the reviewing provider wrote it, because the author of a PR never sets it (T-4, D-381).

The automated pass of gitar is complete on the effective head `8452dbd`. The `Code Review` block reads "Approved" with no finding. The `CI failed` block of the same comment names the missing `docs/reviews/pr-83.md`, and a PR reply answers it with D-251: the review gate stays red until the reviewing provider writes that file, and the author of a PR does not write it. Findings with merit: none.

The cross-provider review comes next. Codex is the eligible reviewer, because Claude Code wrote this PR (T-4, D-101).

### Traps and gotchas

- A walk to the cell of a body that moves never closes. Two walkers each aimed at the cell of the other and swung around each other 2 meters apart for a whole floor (F-104). A search now starts at the cell that the body walks into, and the last 4 meters of a chase are a straight walk at the body.
- A drift check that reads the X and the Z of a waypoint and not its row misses a body that fell two rows under its path. The body then jumped at a step of two blocks until the floor budget ran out (F-105).
- A roll away from every blade loses a duel: the roll covers 3 meters, and the enemy closes again during the 45-tick cooldown. `BotIntent.Roll` rolls through the enemy, which took the descender from 2 bottoms in 200 seeds to 43.
- A step up onto the side of a ramp is a legal move. A ban on it read as a fix for F-105 and was not one, and `TheSearchFollowsTheBodyRuleOnRamps` names the case.
- `TestWorld.NewLoop` now digs a floor with no enemy family, so a test of the camera, the body, or the replay reads its own rule. A test of the enemies builds its loop from `TestWorld.Content`.
- The smoke session dies at tick 273 of its 600-tick script on seed 1, and it ends clean with exit code 0 and the end kind in its line. The smoke gate covers fewer ticks than it did, and a stronger smoke script belongs to a later PR.
- The sweep enemy of the bit-identity content swings a second weapon of 1 damage, because a run that ends by death takes no more intents and the sweep record holds 600 of them.
- The owner chose the wedge fix inside PR-16 (D-405). This PR therefore carries the walk fixes beside the enemies, which widens it past one concern (G-10).
- A handoff entry added at the end of the file reds the three build legs, and four sessions did it (F-106). The owner asked for the tool fix inside this PR, so `handoff-rotate` now puts such an entry back in its place and names it (D-406). `RepositoryFilesHoldTheRule` asserts that the committed file needs no sort, so the check still names a file that breaks D-146.
- That tool change moves the effective head off `8452dbd`, so the automated pass and the cross-provider review both read the new head.
- The next ids are D-407, OQ-181, F-107, PR-71, and Session 195.

### Open questions that block progress

None. OQ-9 has its answers in D-395 and D-396, and the handoff loop has its answer in D-406.

### Next concrete action

Codex reviews GitHub PR #83 and writes `docs/reviews/pr-83.md`, per `.claude/skills/pr-review/SKILL.md`. The review reads the move rule of `GridMoves`, the A* search, the brain, the spawn pass, and the two walk fixes of F-104 and F-105. Exit test 8 of PR-66 needs the night on `main` at `e1c20ea` to report zero crashes and zero softlocks.
