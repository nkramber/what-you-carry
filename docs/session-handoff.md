# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 231: 2026-09-23, Codex

Author: Codex
Session: PR-80, reviewer. Branch `chore/pr-80-gitar-pause`. PR #96, pending merge. Base `25afe34`.

### What this session did, and why

- Reviewed PR #96 at effective head `f331068` under D-542 to D-544.
- The review found no issue in the Gitar pause, the flag path, the thread check, or the supporting documents.
- Added `docs/reviews/pr-96.md` with the verdict `Ready for owner merge`.

### State of the build

- `dotnet test WhatYouCarry.slnx` passed 1573 of 1573 tests at `f331068`.
- The remote head of PR #96 was `f331068`. The required CI checks passed, except `night-gate`, which failed on `e069e16` under D-544. The review-gate failure came from the missing review record.
- The review record and this handoff entry are published to `origin/chore/pr-80-gitar-pause`.

### In flight

- PR #96 awaits the owner merge process. The owner uses the merge summary of D-533 and the bypass of D-544 if `night-gate` remains red.

### Traps and gotchas

- `make -n codex-review PR=96 -- --skip-gitar-review` confirms that Make passes the flag after the required options.
- Gitar posted a free-plan notice and no review feedback. The review export found no open review thread.

### Open questions that block progress

None.

### Next concrete action

The owner checks the merge conditions for PR #96 and follows D-533.

## Session 230: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-80, author. Branch `chore/pr-80-gitar-pause`. PR #96, pending merge. Base `25afe34`.

### What this session did, and why

- The owner paused the gitar requirement until a later PR of the owner (D-542). The author answers each gitar comment that comes in. A gitar review with feedback stops the session, and the session alerts the owner.
- Added the flag `--skip-gitar-review` to `codex-review`, through `make codex-review PR=<n> -- --skip-gitar-review` (D-543). The flag drops the Gitar check run and the Gitar dashboard checks. The thread check stays (D-522). The flag stays after the pause.
- Each text of the pause outside the registers names D-542, so `grep -rn 'D-542'` finds each one for the PR that ends the pause.
- `AGENTS.md` was 19 bytes under its ceiling. The Game argument rules and the generated file rules moved to `docs/runbooks/commands.md` (D-382).
- The owner put this PR before the night fix of D-538, with a bypass merge when `night-gate` stays red (D-544).

### State of the build

- The full suite passed 1573 of 1573 locally. `ste-check` gave no finding.
- The remote head of `main` is `25afe34`. The newest night failed at `e069e16` (D-538), so `night-gate` stays red.

### In flight

- `make codex-review PR=96 -- --skip-gitar-review` approved the effective head `f331068` with no finding (session 231).
- Exit test 4 of PR-79 and of PR-80: the runbook commit after the approval is a documents commit. `review-gate` must stay green on it.
- The Gitar dashboard gave "Gitar is working" at 18:26 UTC, with no thread. Read the PR comments again before the merge summary.

### Traps and gotchas

- Make reads each word after `--` as a goal. The `--%` rule of the `Makefile` keeps make from a stop, and the target passes each such goal to the command.
- `AGENTS.md` holds 14842 bytes of 15000. Move detail to a runbook before a new rule.

### Open questions that block progress

None.

### Next concrete action

Confirm `review-gate` on the new head, read the PR comments, and give the owner the merge summary of D-533.

## Session 229: 2026-09-23, Codex

Author: Codex
Session: PR-79, reviewer. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Re-reviewed PR #95 at effective head `25ad21e`.
- Verified that P2-1 is fixed: the override label and effective-head logic now use the same document path set. No new code finding remains.
- Updated the existing review record with the finding history and current verdict.

### State of the build

- The focused review tests passed 119 of 119 at `25ad21e`.
- The Documents category passed 140 of 140 after the review record and handoff edits.
- `ste-check` passed with no findings, and `doc-gate` passed with 0 problems over 25 paths.
- Asset QA, lint, document, STE, bot, the Linux and macOS CI platform results, Smoke, and bit-identity passed at `25ad21e`.
- The Windows CI job later passed at 17:42 UTC. The metadata-only run skipped heavy jobs under D-474 rule 2 after the previous code-head CI run passed.
- The night gate failed on the D-538 record. The review gate and evaluator fail on the published `Blocked` verdict.

### In flight

- The review record and this handoff entry are published on `origin/chore/pr-79-review-process`.
- PR #95 remains blocked by the D-538 night-gate failure. The review gate and evaluator fail on the `Blocked` verdict; metadata-only jobs pass or skip under D-474 rule 2.

### Traps and gotchas

- The code head is `25ad21e`; later review and handoff commits are documents-only and do not move the effective head (D-534).
- The current night failure belongs to D-538, which is out of scope for PR-79. D-537 keeps the ruleset bypass for the manual merge of that work.

### Open questions that block progress

None.

### Next concrete action

Resolve the D-538 night-gate failure. Then re-review the same effective head and update the existing review record.

## Session 228: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-79, author. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Answered round 1 of the Codex review (`Changes required` at `d13f73c`). P2-1 has full merit: a PR of the root `README.md` or `LICENSE` alone passed neither by review nor by label.
- Recorded the owner instruction as D-541: the `review-override` label covers each path of the skip set of D-475. `review-gate` now reads the one list of the CI skip for the label. D-190 carries a partial revision mark.
- Wrote `docs/reviews/pr-95-response.md`. It also shows that the Smoke jobs ran and passed at `d13f73c`.

### State of the build

- `dotnet build` passed with no warning. The full suite result and the new code head are in the response file and the PR.
- Four new tests fail on the old `ReviewGateRules.cs` and pass with the correction.

### In flight

- Round 2 of the Codex review on the new code head, then the merge summary and the owner confirmation.

### Traps and gotchas

- `review-gate` runs the tool of `main`, so round 2 must record the new code head. The correction commit holds code, so the old rule and the new rule give one head.
- The night gate stays red on the record of D-538. D-537 keeps the bypass for the merge.

### Open questions that block progress

None.

### Next concrete action

Read round 2 of the Codex review of PR #95. On approval, write the merge summary of D-533 and ask the owner to confirm the merge.

## Session 227: 2026-09-23, Codex

Author: Codex
Session: PR-79, reviewer. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Reviewed PR #95 at effective head `d13f73c`.
- Found that a PR of only `README.md` or `LICENSE` cannot pass the review gate by review or by label. The review record names the correction and regression checks.

### State of the build

- The focused review tests passed 111 of 111. They built all projects.
- At code head `d13f73c`, the CI platform, bot, content, document, bit-identity, lint, and STE checks passed. At metadata head `7781d3b`, the document checks passed, and the heavy jobs skipped.
- The night gate failed on the known record in D-538. The Smoke jobs were skipped. The review gate failed because P2-1 remains open.

### In flight

- PR #95 needs a correction for P2-1 and a repeat review.
- The night gate remains red until the work in D-538 lands.

### Traps and gotchas

- D-475 skips root `README.md` and `LICENSE` for review. D-190 does not allow the override label for either path.
- The failed night record belongs to D-538, which is outside PR-79.

### Open questions that block progress

None.

### Next concrete action

Correct P2-1 on PR #95 and request a repeat review. Handle the night failure in the next PR under D-538.

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
