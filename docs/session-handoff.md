# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 217: 2026-09-23, Codex

Author: Codex
Session: PR-62, reviewer. Branch `feat/pr-62-texture-recipes`. PR #92, pending owner merge. Base `97a12ff`.

### What this session did, and why

- Re-reviewed PR #92 at effective head `d96ae19` and updated `docs/reviews/pr-92.md`.
- Verified P2-1. The parser and packer now reject overflowing bounds. The new tests pass on this head and fail on the old code.
- D-510 closes exit test 6. The owner confirmed that the contact sheet keeps the look.

### State of the build

- The focused recipe and texture tests passed 100 of 100. The old-code comparison failed only on the four overflow cases.
- CI, Smoke, bit identity, `ste-check`, `det-lint`, `asset-qa`, `doc-gate`, `documents`, `night-gate`, `bots`, and Gitar passed at effective head `d96ae19`.
- The effective head is `d96ae19`. The review record and this handoff are metadata. The review gate must pass on the published metadata tip.

### In flight

- PR #92: publish the review record and handoff, then verify the checks on the metadata tip.

### Traps and gotchas

- A bounds check compares each size with the room that remains. It does not add two large values.
- OQ-181 blocks PR-77, not PR-62 (D-504).

### Open questions that block progress

None for PR-62.

### Next concrete action

Verify the review gate and session end gate after the push. The owner can merge PR #92. Then a clean author session can start PR-74 from Session 214 and D-496 to D-503.

## Session 216: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-62, correction author. Branch `feat/pr-62-texture-recipes`. PR #92, pending owner merge. Base `97a12ff`.

### What this session did, and why

- Answered the review of Session 215 in `docs/reviews/pr-92-response.md`.
- P2-1 had full merit. The layout parse accepted a canvas whose `x + width` wrapped past the int limit. The check now compares each size with the room that the start leaves. The packer had the same wrap, and it now rejects a size past the atlas before any sum. New cases in `RecipeTests` failed on the old code and pass now.
- The owner confirmed exit test 6: the look stayed the same (D-510).

### State of the build

- The full suite passed 1436 of 1436, Smoke included. `ste-check` gave 0 findings.
- The committed atlas and layout did not change.
- The remote head is the commit that holds this entry. It holds the correction, so it is the new effective head.

### In flight

- PR #92: the automated pass of gitar on the new head, then the repeat review of Codex.

### Traps and gotchas

- A bounds check of two ints adds no two large values. Compare the size with the room that remains.

### Open questions that block progress

None.

### Next concrete action

Codex re-reviews PR #92 at the new effective head. After the merge, PR-74 starts from Session 214 and D-496 to D-503.

## Session 215: 2026-09-23, Codex

Author: Codex
Session: PR-62, reviewer. Branch `feat/pr-62-texture-recipes`. PR #92, changes required. Base `97a12ff`.

### What this session did, and why

- Reviewed the texture recipes, atlas packer, layout parser, Game UV consumers, tests, and the PR documents.
- Found P2-1: an overflowing canvas coordinate can pass the layout bounds check. Added the review record for effective head `6654571`.
- Reviewed the before-and-after contact sheet. It looks consistent at sheet scale, but exit test 6 still needs the owner's confirmation.

### State of the build

- The focused recipe, texture, and model tests passed 113 of 113. `ste-check`, `det-lint`, and `asset-qa` passed with 0 findings.
- The local full suite stalled without output and was interrupted. Its result is incomplete. The remote CI, smoke, bit-identity, and bot checks passed on effective head `6654571`.
- CI passed on Linux, Windows, and macOS, with Linux and Windows sweeps. Bit identity passed on all three platforms and in compare.
- The remote branch tip before this review was `adfe9c7`. This session pushed the review record and this handoff to `origin/feat/pr-62-texture-recipes`.

### In flight

- PR #92 needs a fix and regression test for P2-1, and the owner's confirmation of exit test 6.

### Traps and gotchas

- Later handoff-only commits do not change the effective head (D-184).
- OQ-181 blocks PR-77, not PR-62 (D-504).

### Open questions that block progress

None for PR-62. The owner confirmation and the missing checks are exit evidence, not open questions.

### Next concrete action

The author fixes P2-1 and adds the overflow regression test. The owner confirms the contact sheet. Codex re-reviews PR #92.

## Session 214: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-62, author. Branch `feat/pr-62-texture-recipes`. PR #92, pending owner merge. Base `97a12ff`.

### What this session did, and why

- The owner started PR-62 from a Meshy look reference of the body (`artifacts/reference/meshy-miner-2026-09-22/`, git ignores it). The owner answered in session: D-496 to D-509.
- D-504 splits the art pass. PR-62 is the texture recipe system. PR-74 is the body, PR-75 the sword, PR-76 the enemy models, and PR-77 the light with OQ-181. OQ-181 now blocks PR-77.
- The body answers for PR-74: the brow, the toe, the beard, and the nose boxes (D-497, D-498, D-501, D-502), the kept proportions (D-499), the colors (D-500), and the noise pick on the sheet (D-503).
- The recipe system (D-505 to D-508): recipes under `content/textures/recipes/`, the block file `content/textures/blocks.json`, and a paint file next to each model. `texture-gen` paints one canvas per block and per face, packs them into a 512 atlas with a gutter, and writes `content/textures/layout.json`. Game reads every UV from the layout. The loader reads no face UV.
- Every block canvas keeps its PR-14 pixels, and a hash test holds that. The body and the sword keep their materials, and each face now draws its own noise.
- The owner asked for the skill `asset-texture-creation` in this PR (D-509). It gives the five Meshy steps of every asset.

### State of the build

- The full suite passed 1431 of 1431, Smoke included. `ste-check`, `det-lint`, and `asset-qa` gave 0 findings.
- The contact sheets before and after sit in `artifacts/pr-62/`. `body-before-after.png` shows the body.
- The remote head is the commit that holds this entry, on `origin/feat/pr-62-texture-recipes`. That commit adds the done marks, so it is the effective head.

### In flight

- PR #92: CI is green except the review gate (D-251). Gitar approved the effective head `6654571` with no finding. The PR changes code, so it needs a review record, not the override.
- Exit test 6 needs the owner: confirm that the look stayed the same on the contact sheet.

### Traps and gotchas

- `models/*.paint.json` is not an animation. `AssetSet` skips the suffix, so no animation can take the name `paint`.
- A rectangle layer wholly outside a face canvas is an error. Bind that face to another recipe in the paint file.
- A change to a model box size moves the packer, so run `texture-gen` and commit the atlas and the layout together.
- The agent files have 11 bytes left under D-382.

### Open questions that block progress

None for PR-62. OQ-181 blocks PR-77.

### Next concrete action

Codex reviews PR #92. After the merge, PR-74 starts from D-496 to D-503 and the skill `asset-texture-creation`.

- The owner added two angles for PR-74: `08-3d-top.png` and `09-3d-head-front-close.png` in the reference folder. Step 4 of the skill approved them.
- The close-up measures the brow at y 25.0 to 26.0, the nose band at 23.2 to 24.9, and the beard at 20.8 to 23.2. Each agrees with D-497, D-501, and D-502 within 0.2 units.
- The face paint of the close-up: hair on the top 1.5 to 2 units with a small peak, brown eyes of about 1.5 by 0.5 units under the brow, a lighter nose, and a mouth notch of about 2 by 0.5 units.

## Session 213: 2026-09-22, Claude Code

Author: Claude Code
Session: PR-73, author. Branch `chore/pr-73-docs-only-no-suite`. PR #91, pending owner merge. Base `837902b`.

### What this session did, and why

- The owner asked that a change of documents alone run no full test suite: for the author, a review, a review response, and a handoff. A change of code still runs the full suite.
- The owner answered in session: D-490 to D-495. Such a change runs `ste-check`, `doc-gate`, and `dotnet test WhatYouCarry.slnx --filter Category=Documents` (D-491). The skip set of D-475 names the documents (D-492). A change with any other path runs the full suite (D-493). The test line of the PR gate adds a clause (D-494). The PR template keeps its text (D-495). No decision revises an earlier one.
- The agent files replace the rule "a document edit needs the test suite". `csharp-conventions`, `review-response`, `gitar-review` step 21, `pr-review/references/verification.md`, and `one-pr-one-session/references/enforcement.md` state the rule.
- The design doc and the Phase 2 roadmap add the PR-73 entry, before PR-62.

### State of the build

- `ste-check` found 0 issues in 34 files. The `Documents` category passed 131 of 131 tests. The full suite did not run, by the rule of this PR.
- `CLAUDE.md` and `AGENTS.md` are identical, at 14989 of 15000 bytes.
- The effective head is the commit that holds this entry, on `origin/chore/pr-73-docs-only-no-suite`. Every path of the PR is in the skip set, so the heavy jobs skip by rule 1 of D-474.

### In flight

- PR #91: the automated pass of gitar, then the `review-override` label (D-188, D-190).

### Traps and gotchas

- The agent files have 11 bytes left under D-382. The Smoke detail moved to `csharp-conventions` to make room. A new rule there needs a move of detail to a skill, or a decision.
- The CI filter text in `csharp-conventions` still reads `Category!=Smoke`. `ci.yml` also excludes `Sweep` since PR-71. PR-73 kept that text, because it is outside this concern.
- An edit of `.github/pull_request_template.md` moves a PR into the code set of D-190 (D-495).

### Open questions that block progress

None.

### Next concrete action

The owner merges PR #91. The next session starts PR-62. OQ-181 blocks it, so the owner answers OQ-181 first.

## Session 212: 2026-09-22, Codex

Author: Codex
Session: PR-72, reviewer. Branch `fix/pr-72-enemy-movement`. PR #90, pending owner merge. Base `ea84473`.

### What this session did, and why

- Reviewed PR #90 at effective head `09926b6` against the enemy diagonal and ramp movement contracts.
- Confirmed the fix for the closed gitar finding and added the review record.

### State of the build

- The focused review suite passed 53 tests. The full suite passed 1385 tests with no skips. `ste-check` found 0 issues in 34 files.
- CI, Smoke, bit identity, and bots passed after effective head `09926b6` on metadata commit `751431d`.
- The review record and this handoff reached the PR in `cb103bf`. The fresh `evaluate`, `review-gate`, documents, STE, doc-gate, det-lint, asset-QA, night-gate, and gitar checks passed. The heavy jobs skipped under Rule 2.

### In flight

- PR #90 awaits owner merge. The review approves effective head `09926b6`.

### Traps and gotchas

- The effective head is `09926b6`. Later commits change only metadata.
- The diagonal sweep covers 120 seeds. D-480 keeps the full count on `main` and in the night.

### Open questions that block progress

None.

### Next concrete action

The owner merges PR #90.

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
- Effective head `09926b6`. CI, Smoke, bit identity, bots, det-lint, asset-QA, STE, doc-gate, night-gate, and gitar passed on `751431d`, every heavy job ran. `evaluate` reads red until the review record exists (D-251).
- Remote head before this metadata commit: `751431d`.

### In flight

- The status marks `✅ Done in PR #90.` are in the roadmap and the design doc.
- The review by Codex of the effective head `09926b6`.
- CI passed on `b1e3ab5`, `evaluate` apart (D-251). The gitar pass of `35311d8` gave one finding: a box on the edge of a block over a ramp got a needless jump. The fix takes the higher of the feet and the slope, with the test `ABodyOnTheEdgeOfABlockOverARampNeedsNoJumpOntoIt`. The bit-identity answer did not move.

### Traps and gotchas

- The walk sweep is `EnemyWalkTests.DiagonalSweep`, in the `Sweep` category, at a fixed 120 seeds (about 8 s local). A seed count on floor 1 alone missed every fault. The faults sat on floors 6 to 15.
- An arrival alone proves little. A follower that searches again walks the two side moves, so the sweep also counts the jumps.
- `GridMoves.FloorHeightAt` works in 24ths of a block, so the rule stays in integers (G-9).
- On `main`, the Session 210 entry sat above the `# Session handoff` title. `doc-gate` finds the newest entry by `\n## Session `, so it read Session 209 and failed this branch. This entry puts the title and the rule line back on top, with Session 210 unchanged under Session 211. Add an entry under the rule line.

### Open questions that block progress

None.

### Next concrete action

Codex reviews PR #90 at the effective head `09926b6` and writes `docs/reviews/pr-90.md`. The owner starts it with "Review PR #90" in Codex Desktop.

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
