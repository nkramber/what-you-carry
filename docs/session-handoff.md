# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 301: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-94, author. Branch `feat/pr-94-seed-flag`. PR #120, pending merge. Base `d566bda`.

### What this session did, and why

- Completed the gitar pass of the work head `f12021b`: an approval with no finding. The CI note on the absent review record got one reply with D-251.
- Ran `make codex-review PR=120`. The record gives `Ready for owner merge` for the effective head `f8d06f5`, with no finding (session 300).

### State of the build

- Each check of PR #120 is green, `evaluate` and `review-gate` included, after the metadata commits of the review. Effective head `f8d06f5`, work head `f12021b`.

### In flight

- The owner confirmation of the merge of PR #120 (D-524, D-533).
- PR-92 exit test 3: the 07:07 UTC night of 2026-09-28 on `main` did not run before this entry. The first night after `d5f7e00` runs eight sweep jobs, and its record names each sweep once.

### Traps and gotchas

- A reviewer entry lands above the entry of the author. Add a new entry at the top, and never edit the older one.

### Open questions that block progress

None.

### Next concrete action

After the merge, read the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`. The next PR is M-3 on seeds 1, 2, and 3 over SSH on the Deck (D-606, D-706).

## Session 300: 2026-09-28, Codex

Author: Codex
Session: PR-94, reviewer. Branch `feat/pr-94-seed-flag`. PR #120, pending merge. Base `d566bda`.

### What this session did, and why

- Reviewed PR #120 at effective head `f8d06f5`. The author provider is Claude Code, so the Codex review passes the provider gate.
- The seed flag, its parser rules, boot path, tests, and PR-94 documents match D-703 to D-706. The review record gives `Ready for owner merge` with no findings.
- Five focused tests passed. GitHub code checks passed. `evaluate` and `review-gate` failed because the review record was not on the PR yet.

### State of the build

- Local: five focused tests passed at `f12021b`. GitHub code checks passed at the same head.
- Remote: review record and this entry are on `feat/pr-94-seed-flag` at metadata head `7e06e2e`. The required remote hash comparison passed.

### In flight

- The metadata-tip `evaluate` and `review-gate` checks passed. The heavy code workflows skipped by the documents-only rule, and their required jobs passed at code head `f12021b`.

### Traps and gotchas

- The checkout is detached. Compare its final commit with the head that `gh pr view 120` reports.

### Open questions that block progress

None.

### Next concrete action

The review is complete for effective head `f8d06f5`. The owner can read the review record and the check results before merge.

## Session 299: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-94, author. Branch `feat/pr-94-seed-flag`. PR #120, pending merge. Base `d566bda`.

### What this session did, and why

- The session started as the M-3 author. The M-3 table needs three seeds, and the bot session ran the first seed alone. The owner chose a seed flag in its own PR first (D-703). The session did no work before that answer, so it binds to PR-94.
- Added `--seed <n>` to the bot session: it needs `--bot`, and it takes a whole number from 1 up in digits alone (D-704, D-705). The loop starts on the seed, and a boot failure line carries it.
- Recorded the M-3 seeds: 1, 2, and 3 (D-706). A headless bot session on the Mac reaches floor 2 on seeds 2 and 3.

### State of the build

- Local: the first full run passed 2108 of 2109 tests. The STE test failed on two long sentences, and `f8d06f5` holds the fix. After it, `ste-check` and `det-lint` found zero findings, and the STE and parser tests passed.
- Remote head of the work: `f8d06f5`, plus the metadata and status commit of this entry.

### In flight

- The gitar pass, CI, and `make codex-review` of PR #120.
- PR-92 exit test 3: the first scheduled night on `main` after `d5f7e00` is the 07:07 UTC cron of 2026-09-28. It did not run before this entry.

### Traps and gotchas

- The Mac has no `timeout` command. Run a Godot session under the time limit of the tool.
- The engine tests of the seed flag take about one second, because a headless session at fixed 60 frames per second runs faster than real time.

### Open questions that block progress

None.

### Next concrete action

After the merge of PR #120, start M-3 in a new session: run seeds 1, 2, and 3 on the Deck over SSH (D-606, D-706), and record the table. Check the night of PR-92 exit test 3 on `night-results`.

## Session 298: 2026-09-27, Codex

Author: Codex
Session: PR-93, reviewer. Branch `feat/pr-93-overseer-model`. PR #119, pending merge. Base `b4615c0`.

### What this session did, and why

- Reviewed the PR-93 code head `5faf3b7` as the cross-provider reviewer.
- Checked the model, the hunter model field, the texture mappings, the focused tests, the roadmap, and the PR comments.
- Wrote `docs/reviews/pr-119.md`. The review found no in-scope defect. The owner approval of the contact sheet is recorded in D-700.

### State of the build

- Local focused tests passed: 290 passed, none failed or skipped. Local asset QA found zero findings across four models and three animations.
- Remote CI passed the build and test jobs, bit identity on three platforms, bot checks, asset QA, determinism lint, documents, doc gate, night gate, smoke on three platforms, and STE checks.
- Remote `evaluate` and `review-gate` failed because the review record was not yet on the branch. Remote branch head before this metadata push: `e2eea3e`.

### In flight

- The review record and this handoff entry need one metadata commit and a push to `feat/pr-93-overseer-model`.
- PR #119 is pending owner merge.

### Traps and gotchas

- The review applies to effective head `5faf3b7`. The later code tip `e2eea3e` changes documents only (D-534).
- The contact-sheet image named by D-700 was not in the referenced folder. The owner approval remains recorded in D-700.

### Open questions that block progress

None.

### Next concrete action

- Push the review record and this entry together. Verify the remote head and the review-gate result.

## Session 297: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-93, author. Branch `feat/pr-93-overseer-model`. PR #119, pending merge. Base `b4615c0`.

### What this session did, and why

- Built the Overseer model through the five steps of `asset-texture-creation`. The owner reviewed the Meshy prompt, concept 01, the edit to concept 02, the eleven Meshy views, the build questions, and the contact sheet. The answers are D-686 to D-702.
- The model is 37 units tall over the body box of D-165. A respirator, goggles, and cans hide the face, and the coat rides on wide upper legs (D-686 to D-697).
- The hunter file names its model (D-698). The Game layer draws the Overseer with it, and the contact sheet shows it in cells of 600 pixels (D-700).
- 45 faces trace from the unlit views, with the hand corrections of D-700. The coat and the cowl grain at cell 2, and the rest at cell 8 (D-699).
- Step 1 of `asset-texture-creation` fits the Meshy limits: 800 characters, and no negative prompt field (D-701, D-702).

### State of the build

- Local: build, the full suite with Smoke (2105), `asset-qa`, `det-lint`, `ste-check`, and the Godot build check are green. Remote `main` is `b4615c0`.
- PR #119: each check of the effective head `5faf3b7` is green but the Review gate workflow. The gitar pass approves the head `e2eea3e` with no finding. The review record gives `Ready for owner merge` for `5faf3b7`.

### In flight

- The owner confirmation of the merge of PR #119 (D-524, D-533).
- PR-92 exit test 3: no scheduled night on `main` ran after `d5f7e00` by 03:35 UTC on 2026-09-28. The recent nights started from 12:00 to 13:00 UTC.

### Traps and gotchas

- Meshy text to image takes 800 characters, and it has no negative prompt field (D-702).
- Each grain layer dithers each pixel by up to 1.5 fine steps, whatever its cell (D-599). Only a larger cell softens the patches.
- The reference folder of this PR is `artifacts/reference/overseer-2026-09-27/` in the main checkout, with a copy in the worktree `/Volumes/SSD-1TB/wyc-pr93`.

### Open questions that block progress

None.

### Next concrete action

After the merge, check the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`.

## Session 296: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-77, author. Branch `feat/pr-77-light-and-edges`. PR #118, pending merge. Base `3fdefef`.

### What this session did, and why

- Read the second review of session 295. The record gives `Ready for owner merge` for the effective head `ce02802`, with no finding.
- Moved the title and the rule line of this file back above the newest entry, where session 295 had put its entry.

### State of the build

- Each check of PR #118 is green at `fc85d96`, `review-gate` and `evaluate` included. The effective head stays `ce02802`.

### In flight

- The owner confirmation of the merge of PR #118 (D-524, D-533).
- PR-92 exit test 3 waits for the scheduled night of 07:07 UTC on 2026-09-28.

### Traps and gotchas

- A review entry can land above the title line of this file. Put the title and the rule line back on top, and keep the entry text as it is.

### Open questions that block progress

None.

### Next concrete action

After the merge, check the night of PR-92 exit test 3 on `night-results`, and write the merge prompt of `one-pr-one-session`.

## Session 295: 2026-09-27, Codex

Author: Codex
Session: PR-77, reviewer. Branch `feat/pr-77-light-and-edges`. PR #118, ready for owner merge. Effective head `ce02802`.

### What this session did, and why

- Re-reviewed PR #118 after session 294 added the raw Deck frame logs and the response file.
- Verified the exit test 2 frame threshold and transition in all three logs. The measurements pass D-683.
- Read the code diff, tests, project contracts, documents, and PR comments. No code defect remains.
- Updated `docs/reviews/pr-118.md` to approve the effective head.

### State of the build

- Effective head: `ce02802`. The five later commits change only paths in the skip set of D-475.
- The code checks passed on `9acf45d`, including CI, Smoke, Bit identity, Bots, Asset QA, determinism lint, doc gate, night gate, and STE check.
- At `6e00a68`, the metadata checks passed, and Gitar approved the code. `evaluate` and `review-gate` still read the earlier `Blocked` verdict.
- Remote head: the metadata commit of this entry on `feat/pr-77-light-and-edges`.

### In flight

- The review record and this handoff need fresh `evaluate` and `review-gate` results after publication.
- The owner reads the review and confirms the merge summary before the merge.

### Traps and gotchas

- The effective head stays `ce02802`, because later commits changed only review metadata.
- The code, smoke, bot, and bit-identity checks skip a documents-only tip. Their result at `9acf45d` covers the unchanged code.
- Each Deck frame log holds microseconds, one frame per line. The exit test drops frames at 10 seconds or earlier.

### Open questions that block progress

None.

### Next concrete action

The owner reads the review, confirms the merge summary of D-533, and then merges PR #118.

## Session 294: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-77, author. Branch `feat/pr-77-light-and-edges`. PR #118, pending merge. Base `3fdefef`.

### What this session did, and why

- Answered the review of session 293. The review found no defect in the code, and it blocked exit test 2, because the raw Deck frame logs were not in the checkout.
- Committed the raw logs in `docs/reviews/pr-118-deck/`: three runs at `ce02802`, the base, the three MSAA modes, the empty scene of F-190, and two traces with a timing patch.
- Wrote `docs/reviews/pr-118-response.md` with the method of D-683 and the numbers that the logs give.

### State of the build

- Effective head: `ce02802`. The logs and the response are in the metadata set of D-184, so they do not move it.
- CI, smoke, bit identity, bots, and the other checks passed for `ce02802`. `evaluate` and `review-gate` wait for an approving review.

### In flight

- The second round of `make codex-review PR=118`.
- PR-92 exit test 3 waits for the scheduled night of 07:07 UTC on 2026-09-28.

### Traps and gotchas

- A frame log holds one frame per line in microseconds. The stall of F-190 comes every 2.245 s, so a check of exit test 2 reads the grid of step 4 of the response file.

### Open questions that block progress

None.

### Next concrete action

Read the second review. When it approves `ce02802`, check the night of PR-92 exit test 3, and give the owner the merge summary.

## Session 293: 2026-09-27, Codex

Author: Codex
Session: PR-77, reviewer. Branch `feat/pr-77-light-and-edges`. PR #118, review blocked. Effective head `ce02802`.

### What this session did, and why

- Reviewed the complete PR-77 diff, its contracts, tests, documents, and comments.
- The code review found no defect. The review record is blocked because exit test 2 lacks the raw Deck frame logs.

### State of the build

- The remote head before publication was `0679715`. The first review publication reached `9449f83`, verified with `gh pr view`. The implementation head is `ce02802`.
- The focused contact sheet, atlas mesher, and game-shape tests passed: 112 of 112 on macOS arm64 with .NET 10.0.400.
- CI, three smoke jobs, bit identity, asset QA, determinism lint, STE, documents, doc gate, night gate, and Gitar passed on `0679715`. On `9449f83`, Gitar and the document checks passed. Code, smoke, and bit-identity jobs skipped because the push changed documents only. `evaluate` and `review-gate` failed because the review verdict is blocked.

### In flight

- The review record needs the Deck frame logs for exit test 2 before it can approve PR #118.

### Traps and gotchas

- The handoff of session 292 reports the Deck measurements, but the logs and their artifact path are not in this checkout.
- The approved contact sheet of D-682 is outside this checkout.

### Open questions that block progress

None. Required evidence is missing for exit test 2.

### Next concrete action

Add the three Deck frame logs to the review evidence, then reassess exit test 2 and update the review record.

## Session 292: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-77, author. Branch `feat/pr-77-light-and-edges`. PR #118, pending merge. Base `3fdefef`.

### What this session did, and why

- Asked OQ-181, and recorded D-677: MSAA at 4x and nearest filtering with mipmaps. The owner then chose the light: a warm lantern that the player carries, a dark ambient light, and no directional light (D-678, D-679).
- The first HUD shot showed the player as a black shape, because the camera stands behind the lantern. The owner moved the lantern above and behind the head (D-680) and took the ambient energy 4 (D-681). The owner approved the contact sheet with a brighter lantern (D-682, exit test 3).
- The block canvases sit on a pitch of 66 pixels, so atlas mipmaps mixed two canvases at block edges. `BlockTiles` and `BlockAtlas` copy each canvas to a slot of 64 pixels, and the world shader measures the level before `fract()`.
- Deck frame logs over SSH (D-606) showed that `main` and PR-77 missed D-295 alike, with MSAA off, 2x, and 4x. The owner chose to fix the frame costs of the game in this PR (OQ-209, D-683, D-684).
- A trace found three causes. An empty scene has a stall of 18 to 19 ms every 2.245 s (F-190). The Godot build compiled no optimization, so a path search took 18 ms (D-685, F-191). The rebuild of the enemy meshes took 16.7 ms at a descent (F-192). Each enemy tree now shares the meshes of a template.

### State of the build

- Remote head: the metadata commit of this entry on `feat/pr-77-light-and-edges`. The code head is `ce02802`.
- Local: the build, 2074 of 2075 tests, `det-lint`, `asset-qa`, the Godot build, and the smoke session pass. The one failure is `RepositoryDocumentsPass`, from the ignored local file `artifacts/reference/scavenger-2026-09-27/resume-prompt.md` of another session. CI has no such file.
- Exit test 2 at `ce02802`, three Deck runs: after the first 10 s, p99 11.7 to 11.9 ms, 15 frames over 16.7 ms on the F-190 cadence, and the transition at 19.9 to 20.3 ms.
- The bit-identity answer `e202e84e0f5c188a` stands with the optimization.

### In flight

- PR #118 waits for CI, the gitar pass, and the cross-provider review through `make codex-review PR=118`.
- PR-92 exit test 3 waits for the first scheduled night on `main` after `d5f7e00`, the cron of 07:07 UTC on 2026-09-28. Its record on `night-results` names each sweep once, and it runs eight sweep jobs.

### Traps and gotchas

- `--build-solutions` builds the Debug configuration. Before D-685, each Deck frame log measured code with no optimization.
- The stall of F-190 comes from the Deck desktop over SSH. A check in Game Mode is open.
- The Deck checkout stays on `feat/pr-77-light-and-edges` at `ce02802`. The scripts `pr77-*.sh`, `diag*.patch`, and `~/frameprobe` on the Deck are throwaway.
- `ModelNodes.Share` needs a template of the same model. `ShareInto` disposes each mesh wrapper at once, as F-177 asks.

### Open questions that block progress

None.

### Next concrete action

Run `make gitar-wait PR=118`, answer each gitar item, wait for green CI, and run `make codex-review PR=118`. Then check the night of PR-92 exit test 3, and give the owner the merge summary.
