# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 226: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-79, author. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Built D-534. `review-gate` and `make codex-review` read the effective head, the newest commit outside the skip set of D-475. A documents commit after an approving review keeps the gate green.
- Kept the metadata set of D-184 for the gitar pass and for the override label, as the work head. The owner answered the two open points in session: D-539 (the label keeps D-190) and D-540 (a PR of documents alone needs the label).
- Wrote the merge summary of D-533 (What, How, CI, Codex review) into `one-pr-one-session`, `review-and-merge.md`, the agent files, and the PR template.
- Added the PR-79 entry to the Phase 2 roadmap and to the design doc, with `✅ Done in PR #95.`

### State of the build

- Code head `d13f73c`, and it is the effective head under the old rule and the new rule. `dotnet build` passed with no warning. `dotnet test` passed 1550 of 1550, Smoke included. `ste-check` gave 0 findings.
- Six new tests fail when `SkipPaths` returns the metadata set, so they prove D-534.

### In flight

- The Codex review of `d13f73c`, then the merge summary and the owner confirmation.

### Traps and gotchas

- `review-gate` runs the tool of `main` (`pull_request_target`, D-197). This PR is judged by the old rule until it merges. Keep a code path in the last commit outside the metadata set before each review round, so the two rules give one head. A documents-only fix needs a new round, or a commit that also holds code.
- Exit test 4 runs on the first PR after the merge: a documents commit after its approval must keep `review-gate` green.
- Sequence item 35 of the Phase 2 roadmap still reads `PR-74.` with no done mark, although PR #94 merged.

### Open questions that block progress

None.

### Next concrete action

Read the Codex review of PR #95, and answer each finding. Then write the merge summary and ask the owner to confirm the merge. The night fix of D-538 is the next PR, in a new session.

## Session 225: 2026-09-23, Codex

Author: Codex
Session: PR-74, reviewer. Branch `feat/pr-74-body-art`. PR #94, blocked. Base `6ee836d`.

### What this session did, and why

- Reviewed PR #94 at effective head `df900d5` under the Codex review request that Session 224 started by hand (D-536).
- Verified the body geometry and paint recipes, fine shades, grain and gradient behavior, palette indexing, tests, documentation, and ruleset fields. No in-scope defect was found.
- Published the review record with this handoff entry in one metadata commit (D-182).

### State of the build

- `dotnet build WhatYouCarry.slnx` passed with no warnings or errors. The focused texture, recipe, model, and ruleset tests passed 152 of 152.
- The live ruleset matches `.github/rulesets/main.json`. `asset-qa`, `det-lint`, `doc-gate`, `documents`, `ste-check`, Linux smoke, and Linux bit identity passed.
- After the metadata push, the three CI jobs, both sweep jobs, all bit-identity jobs, and the document and asset checks passed. The three Smoke jobs and bots skipped on metadata head `3d7f08b`. `night-gate` failed, and `evaluate` and `review-gate` failed because the verdict is `Blocked`. The effective head remains `df900d5`.

### In flight

- PR #94 remains blocked. The owner must resolve the night-gate block of D-538, and exit test 3 still needs fresh Smoke evidence.

### Traps and gotchas

- The owner waived Gitar for PR #94. Its in-progress dashboard has no review threads and makes no code claim.
- The metadata commit does not change the effective head under D-184. The code head stays `df900d5`.
- Local smoke sessions need the full Godot path in `AGENTS.md`. This checkout did not have a configured Godot binary.

### Open questions that block progress

None. OQ-181 blocks PR-77 alone.

### Next concrete action

Reassess PR #94 after the owner resolves the night-gate block of D-538 and a fresh three-platform Smoke run completes.

## Session 224: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-74, author. Branch `feat/pr-74-body-art`. PR #94, pending merge. Base `6ee836d`.

### What this session did, and why

- PR-78 exit test 4, run by the PR-78 session after the merge of PR #93: `allow_auto_merge` is on, and the ruleset `main` (id 23883816) is active. The live ruleset matched `.github/rulesets/main.json` at `6ee836d`, plus two fields of the `pull_request` rule: `require_extra_approval_for_unattributed_changes: false` (owner choice, 2026-09-23) and `required_reviewers: []`.
- Second concern, by owner approval with no decision: the file now declares both fields, and `RulesetTests` asserts each. The comparison of `docs/runbooks/main-ruleset.md` against the live ruleset gives an empty diff.
- The body gains the brow, the nose, the beard, and the two toe boxes at the places of D-497, D-498, D-501, and D-502. The face layout and the trim come from owner answers (D-525, D-526).
- The owner rejected the noise sheets of D-503 (0.12, 0.20, 0.30): "None of them match the level of detail that the 3D model screenshots have." D-527 supersedes D-503. The recipe system gains a `shade` field, a clustered `grain`, and a `gradient`, in whole numbers alone. The palette gains three fine shades between each pair of colors (D-528), and a ninth ramp, umber, for the dark browns (D-530). The body takes the measured shades of the unlit 3D reference (D-529, D-531).
- The owner approved the sheet b2 as finished art (D-532, exit test 4). The owner kept D-525 where the brow hides the eye row from the camera of the sheet.
- The owner asked to remove the bypass checkbox (D-535), and then called it a mistake. D-537 restores the bypass of D-520. The live ruleset (id 23883816) had no bypass from about 16:10 to 16:25 UTC, and it matches the file again. PR-74 carries no bypass change.
- The night of 2026-09-23 on `main` failed: greedy-descender softlocked 26 of 5000 seeds at `e069e16`. `night-gate` is red on every PR. D-538 plans the fix and a night gate that counts a hand night on the PR branch.
- The owner waived the gitar pass for this PR. Gitar never reviewed a head of PR #94, and the Codex review starts by hand with the invocation of `make codex-review` (D-536).
- The owner asked for two process changes, recorded for the next PR: the merge summary of four sections (D-533), and a `review-gate` that stays green after a later documents commit (D-534). The gitar pass still reviews each push.
- No roadmap item improves the player animation beyond the clips and the walk of PR-15 (D-331, D-333). The owner asked, and no item exists.

### State of the build

- The full suite passed 1525 of 1526 at the code commit before the last documents fix, and the one failure was the citation of D-503, now fixed. Smoke and Documents pass 138 of 138. `ste-check`, `asset-qa`, and `det-lint` give 0 findings.
- Every block and the sword keep their exact pixels. The atlas palette grows from 32 to 117 entries.
- The remote head and the PR number go into the next entry after the push.

### In flight

- PR #94 is open, and PR-74 is marked done in `docs/design.md` and the roadmap. Next: the gitar pass, then `make codex-review PR=94`, then the merge summary of D-524.

### Traps and gotchas

- Colors keep their flat indices 0 to 35 in a recipe. The atlas holds the 36 colors first and then the shades, so a block pixel keeps its index. Umber is 32 to 35.
- Every `fill` and `rect` needs `shade`. The `noise` of `fill` and `rect`, `edge`, and `band` still move a whole color step (four fine steps).
- The painter clamps once after the last layer. A texel that the noise moved down and a band moved up stays at the base.
- The `leather` recipe is now the sword grip alone. The body reads `trousers`, `boot`, and `boot-toe`.
- `SendUserFile` cannot deliver in this session type. The sheets and the comparisons are in the git-ignored folder `artifacts/reference/meshy-miner-2026-09-22/`, files 10 to 17.
- A restore with `cp "$bk"/*.json` put `layout.json` into the recipe folder one time. Back up the recipes alone.
- PR #93 dropped the title and the rule line at the top of this file. `doc-gate` finds the newest entry by `\n## Session `, so an entry at byte 0 reads as absent. This PR restores both lines.

### Open questions that block progress

None.

### Next concrete action

Run the gitar pass of `gitar-review` on PR #94, then `make codex-review PR=94` in the background. PR-74 needs the owner merge with the bypass checkbox, because `night-gate` is red. After PR #94 merges, the next session opens one process PR for D-533 and D-534: a new roadmap item, the effective head in `ReviewGate` and `CodexReview` over the skip set of D-475, the tests, and the summary form in `one-pr-one-session`. The PR after it is the night fix of D-538: bisect the softlocks from `170f08c` to `e069e16` (PR-72 first), fix them, and let a hand night on the PR branch count for that PR.

## Session 223: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-78, author, the hand-over. Branch `feat/pr-78-codex-review`. PR #93, pending merge. Base `e069e16`.

### What this session did, and why

- Round 3 of `make codex-review PR=93` approved effective head `e28ecd2` with exit 0 (Session 222). P1-1 and P2-1 are fixed, and no finding is open.
- Exit test 3 holds: each of the 20 required checks reported on the code head `e28ecd2` and on the metadata tip `3c5b7fe`. The heavy jobs gave `skipped` on the tip, which GitHub counts as a pass.
- The owner gets the summary of D-524 and merges this PR by hand (D-519).

### State of the build

- The full suite passed 1506 of 1506 at `e28ecd2`, and `ste-check` finds no issue. Gitar approved `e28ecd2` with every thread resolved.
- The effective head is `e28ecd2`. This entry is a metadata commit.

### In flight

- The owner merge of PR #93. Then, on `Merged PR #93`, this session asks for the approval of the setup, and runs `docs/runbooks/main-ruleset.md`: auto-merge on, the ruleset from `main`, and the comparison of the live ruleset (D-519, exit test 4).

### Traps and gotchas

- The live ruleset does not exist before the setup. Until then, nothing on GitHub enforces the PR gate.
- `smoke-linux-x64` aborted one time at the Godot shutdown (exit 134, a leaked ArrayMesh) with no Game change, and a re-run passed. A required check makes such a flake block auto-merge until a re-run.
- The review record of round 3 lists `e28ecd2` in the `Open at:` line of P2-1, which is fixed. A fixed finding does not count, so the stop reads it correctly.

### Open questions that block progress

None.

### Next concrete action

After the owner merge, get the approval of the setup, apply it with `docs/runbooks/main-ruleset.md`, and write the transitional prompt.

## Session 222: 2026-09-23, Codex

Author: Codex
Session: PR-78, reviewer, round 3. Branch `feat/pr-78-codex-review`. PR #93, Ready for owner merge. Base `e069e16`.

### What this session did, and why

- Re-reviewed PR #93 at effective head `e28ecd2` after the author fixed P2-1 and the malformed-heading cases.
- Confirmed that P1-1 and P2-1 pass their regression checks. The review record now preserves both earlier verdicts and gives the current verdict.
- The latest automated pass approved the code fixes. Its review threads have replies and are resolved.

### State of the build

- The focused Codex review, ruleset, and review-gate tests passed: 70 passed, 0 failed, 0 skipped. The build succeeded as part of the test command.
- Required code checks passed on `e28ecd2`. After metadata commit `a91b075`, `evaluate`, `review-gate`, and the document checks passed. Code-only jobs skipped on the metadata head under the documents-only rule.
- The effective head is `e28ecd2`. The review record and this entry are metadata.

### In flight

- The author gives the owner the merge summary required by D-524 after the fresh results pass.

### Traps and gotchas

- Only P3 is nonblocking. The parser now faults on an unsupported severity or a malformed heading in the Findings section.
- Exit test 4, the live ruleset setup, waits until after merge under D-519.

### Open questions that block progress

None.

### Next concrete action

The author checks the fresh publication results, then gives the owner the required merge summary.

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
Session: PR-78, author, the answers to rounds 1 and 2. Branch `feat/pr-78-codex-review`. PR #93, pending merge. Base `e069e16`.

### What this session did, and why

- `make codex-review PR=93` ran round 1 end to end: exit 10 with P1-1 open, and no fault. The Codex entry is Session 219.
- P1-1 has full merit. An approving record with an open P0 to P2 finding is now a fault. `docs/reviews/pr-93-response.md` holds the answer.
- The gitar pass of `2dca4fe` found one bug with full merit: `codex login status` writes its status to stderr, and the command read stdout alone, so every round refused. The command now reads both streams, and a test starts a real child that writes to stderr.
- Round 2 at `4e850b9` exited 10 with P2-1 open: a severity outside P0 to P3, or a heading that the parser skipped, passed as nonblocking. The parser now faults on both. P1-1 is fixed in `2dca4fe`.
- The gitar pass of `7159ad2` found one more skipped heading form, `####` or `###P`, now a fault. `smoke-linux-x64` aborted at the Godot shutdown there (exit 134, a leaked ArrayMesh) with no Game change, and the author re-ran it.
- The owner added D-523 (no API pricing: the command strips the three credential variables, forces the ChatGPT login, and checks `codex login status`) and D-524 (a summary of one paragraph and the owner confirmation before each merge).

### State of the build

- The build passes. The full suite passes, and `ste-check` finds no issue. The P1-1 tests fail on the old code, 3 of 3.
- The effective head is the commit that holds this entry. Round 1 reviewed `b7623f4`, and round 2 reviewed `4e850b9`.

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
