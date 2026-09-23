# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 221: 2026-09-23, Codex

Author: Codex
Session: PR-78, reviewer, round 2. Branch `feat/pr-78-codex-review`. PR #93, changes required. Base `e069e16`.

### What this session did, and why

- Re-reviewed PR #93 at effective head `4e850b9` after the author fixed P1-1 and the login check.
- P1-1 is fixed in `2dca4fe`. The approving record now rejects an open P0 to P2 finding.
- Added P2-1: the parser accepts unsupported severities P4 to P9, and the outcome rules treat them as nonblocking.
- Updated `docs/reviews/pr-93.md` with the prior verdict, the fixed finding, P2-1, and this round’s evidence.

### State of the build

- The focused Codex review, ruleset, and review gate tests passed: 64 passed, 0 failed, 0 skipped. The build succeeded as part of the test command.
- Required code checks passed on `4e850b9`. `evaluate` and `review-gate` failed because the published review still required changes. Fresh results are pending this record.
- The effective head is `4e850b9`. This review record and this entry are metadata.

### In flight

- Fresh `evaluate` and `review-gate` results after the metadata commit.
- The author must fix P2-1 and start another review round after the Gitar pass.

### Traps and gotchas

- The finding format defines P0 to P3. Only P3 is nonblocking.
- The code and workflow checks pass at the effective head, but the review gate is not green until a review approves it.
- Exit test 4, the live ruleset setup, waits until after merge under D-519.

### Open questions that block progress

None.

### Next concrete action

The author rejects unsupported finding severities, then starts the next round after the Gitar pass.

## Session 220: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-78, author, the answer to round 1. Branch `feat/pr-78-codex-review`. PR #93, pending merge. Base `e069e16`.

### What this session did, and why

- `make codex-review PR=93` ran round 1 end to end: exit 10 with P1-1 open, and no fault. The Codex entry is Session 219.
- P1-1 has full merit. An approving record with an open P0 to P2 finding is now a fault. `docs/reviews/pr-93-response.md` holds the answer.
- The gitar pass of `2dca4fe` found one bug with full merit: `codex login status` writes its status to stderr, and the command read stdout alone, so every round refused. The command now reads both streams, and a test starts a real child that writes to stderr.
- The owner added D-523 (no API pricing: the command strips the three credential variables, forces the ChatGPT login, and checks `codex login status`) and D-524 (a summary of one paragraph and the owner confirmation before each merge).

### State of the build

- The build passes. The full suite passes, and `ste-check` finds no issue. The P1-1 tests fail on the old code, 3 of 3.
- The effective head is the commit that holds this entry. Round 1 reviewed `b7623f4`.

### In flight

- PR #93: the gitar pass of the new head, then round 2 with `make codex-review PR=93`.
- After the owner merge: the setup of D-519 on approval.

### Traps and gotchas

- `codex login status` writes to stderr, and a terminal hides that. It also reads `auth.json` alone. It gave `Logged in using ChatGPT` with a fake `OPENAI_API_KEY` set, so the command also strips the variables from each Codex process.
- The API keys in the `.env` of decktome serve its paid deck gate. D-523 changes the Codex child processes alone, and it revokes no key.
- A `git stash pop` refuses a file that a later edit touched. Save the new hunk as a patch first.

### Open questions that block progress

None.

### Next concrete action

Complete the gitar pass of the new head, then run `make codex-review PR=93` in the background.

## Session 219: 2026-09-23, Codex

Author: Codex
Session: PR-78, reviewer. Branch `feat/pr-78-codex-review`. PR #93, changes required. Base `e069e16`.

### What this session did, and why

- Reviewed PR #93 at effective head `b7623f4` for the Codex review command, the three-strike count, and the main ruleset.
- Added review record `docs/reviews/pr-93.md`. Finding P1-1 shows that the command approves a record with an open P0, P1, or P2 finding.
- Pushed the review record and this handoff together as one metadata commit, as D-182 and D-518 require.

### State of the build

- `dotnet build WhatYouCarry.slnx` passed with 0 warnings and 0 errors. The full test suite passed: 1,491 passed, 0 failed, 0 skipped.
- The metadata checks passed: `ste-check` reported 0 findings, and the Documents category passed 131 tests.
- The effective head is `b7623f4`. The remote review branch holds the metadata commit with this entry and the review record.
- Before publication, all code-head checks passed except `evaluate`, which failed because the review record was absent. `review-gate` was skipping. Gitar passed.
- Checks for metadata commit `c06f6ce` finished. Gitar, `documents`, `doc-gate`, `ste-check`, and all other reported applicable checks passed. Heavy jobs skipped. `evaluate` and `review-gate` failed for the changes-required verdict.

### In flight

- The author must correct P1-1 and run the next review round.
- Exit test 4 checks the live ruleset after the owner approves the post-merge setup.

### Traps and gotchas

- An approved verdict skips the strike result in `ReviewOutcomeRules.Judge`. The P3 approval test does not cover an open P0 to P2 finding.
- The review record applies to effective head `b7623f4`; the metadata commit does not change that head (D-184).
- `evaluate` and `review-gate` fail until a later review approves the effective head.

### Open questions that block progress

None.

### Next concrete action

The author fixes P1-1, then starts the next review round after the Gitar pass.

## Session 218: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-78, author. Branch `feat/pr-78-codex-review`. PR #93, pending merge. Base `e069e16`.

### What this session did, and why

- The owner asked for an automated Codex review, a three-strike stop, and a gated auto-merge. D-511 to D-522 record the direction and the answers of 2026-09-23.
- `make codex-review PR=<n>` updates the npm CLI and runs the new `codex-review` command of Tools (D-511, D-512). The command checks the start conditions, probes `gpt-6-luna` at effort `medium`, runs Codex in a detached worktree, and judges the pushed record.
- The `Open at:` line of each finding counts the review rounds. A P0 to P2 finding open in three rounds exits 11 (D-513 to D-515).
- `.github/rulesets/main.json` holds the ruleset of `main`, and `.github/review-gate-mode` turns `enforced` (D-520 to D-522). The platform jobs got unique check names (F-110).
- The owner merges this PR by hand. After the merge, the session applies the settings on approval (D-519).

### State of the build

- The build passes with no warnings. The new tests pass: `CodexReviewTests`, `CodexReviewGitTests`, and `RulesetTests`. `ste-check` finds no issue.
- The effective head is the commit that holds this entry. Origin holds it after the push.
- The CLI facts of 2026-09-23: npm `@openai/codex` 0.156.1 answered the probe. Homebrew holds 0.39.0, and the app bundles 0.155.0-alpha.9.2.

### In flight

- PR #93: the gitar pass, then `make codex-review PR=93`, then the answers to the findings.
- After the owner merge: the setup of D-519, and the live ruleset check with `docs/runbooks/main-ruleset.md`.

### Traps and gotchas

- GNU make exits 2 for each failed target. The exit code of the command shows as `Error <code>`, and the first output line names the outcome.
- `codex exec` reads a piped stdin into the prompt, so the command closes stdin.
- A skipped job reports success to a required check. A job that never reports blocks every merge, so `RulesetTests` binds each required name to one job.
- The agent files stand at 14987 of 15000 bytes. The Tools commands now use the `tools <command>` form.

### Open questions that block progress

None. The owner answered each question of this PR in session.

### Next concrete action

Complete the gitar pass of PR #93, then run `make codex-review PR=93` in the background.

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
- The effective head is `d96ae19`. The review record and this handoff are metadata. The review gate and `evaluate` passed on metadata tip `c8f73cc`.

### In flight

- PR #92: publish the check results in this record, then verify the session end gate.

### Traps and gotchas

- A bounds check compares each size with the room that remains. It does not add two large values.
- OQ-181 blocks PR-77, not PR-62 (D-504).

### Open questions that block progress

None for PR-62.

### Next concrete action

Verify the session end gate after the push. The owner can merge PR #92. Then a clean author session can start PR-74 from Session 214 and D-496 to D-503.

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
