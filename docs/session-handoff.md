# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

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

## Session 291: 2026-09-27, Codex

Author: Codex
Session: PR-76, reviewer. Branch `feat/pr-76-enemy-models`. PR #117, pending merge. Base `d5f7e00`.

### What this session did, and why

- Reviewed PR #117 at effective head `9a0f932`, as the cross-provider review required by T-4 and D-101.
- Checked the family model field, scavenger model and recipes, model rendering, contact sheet, content errors, and PR-76 exit tests. No in-scope finding remains.
- Added `docs/reviews/pr-117.md` with the verdict and verification record.

### State of the build

- The PR head `fdfee53` passed `asset-qa`, `det-lint`, `doc-gate`, `documents`, `evaluate`, Gitar, `night-gate`, `review-gate`, and `ste-check`. The documents-only rule skipped the bit-identity, bot, CI, and smoke jobs on this metadata head.
- The earlier implementation tip `e1316ca` passed CI on all three platforms, bit identity on all three platforms, smoke on all three platforms, asset QA, determinism lint, STE, doc gate, documents, night gate, bots, and Gitar.
- The focused local test filter passed 304 tests. Local asset QA reported zero findings. The Documents category passed 254 tests, and local STE check reported zero findings.
- `evaluate` and `review-gate` first failed because the review record was absent. Both passed after the review metadata was published.
- Local STE check passed with zero findings. The Documents category passed 254 tests.

### In flight

- The review record and Session 291 handoff are published to `feat/pr-76-enemy-models`.

### Traps and gotchas

- The PR tip `e1316ca` is metadata after effective head `9a0f932`. The review applies to `9a0f932`.
- The local full suite did not run. The focused suite passed, and the full CI suite passed on the PR tip.

### Open questions that block progress

None.

### Next concrete action

Give the owner the merge summary and wait for the owner to decide whether to merge.

## Session 290: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-76, author. Branch `feat/pr-76-enemy-models`. PR #117, pending merge. Base `d5f7e00`.

### What this session did, and why

- The session continued the PR-76 author session in a new context, from the resume prompt in the reference folder.
- The Step 1 template of `asset-texture-creation` names no texel grid and no texel count. In concept 03, a count gave texels 4 times too big. An edit of the earlier image is the fix path (D-664).
- `content/models/scavenger.bbmodel`: the base body without the brow, the nose, and the beard. Four hood plates of 1 unit (D-666, D-674), the scarf (D-670), and the sack of 9.5 by 2.5 by 3.5 (D-675). The owner chose the sack size because the D-667 sack shared its back and side planes with the hood back plate.
- `traces/scavenger.json` traces 52 faces. The hand corrections: soot up one step (D-666), clay down one shade (D-669), the grey hood band and the background texels removed, the eyes (D-671), and the belt, knot, and ends (D-672). The torso front names umber for the trousers in the coat opening.
- Hidden faces extend miner recipes with the swap of D-507. The family file names its model (D-673). `EnemyNodes` draws each enemy with its family model, and the contact sheet shows each model.
- The owner approved the contact sheet (D-676, exit test 3).

### State of the build

- Remote `main` is `d5f7e00`. The PR head carries the code and the done marks. Locally: the build, the full suite (2066 of 2067, and the one failure was the STE check of the ignored resume prompt, clean on a worktree), `asset-qa`, `det-lint`, and the Godot build check pass.

### In flight

- CI, the gitar pass, and the cross-provider review of PR #117.
- PR-92 exit test 3: the first night on `main` after `d5f7e00` is the 07:07 UTC cron of 2026-09-28, and it can lag hours. Check that it runs eight sweep jobs and that the `night-results` record names each sweep once.

### Traps and gotchas

- The hand corrections are not idempotent. To trace a scavenger face again, delete its recipe, run `texture-trace`, and apply each correction of this entry once.
- The top view `10` has the model front at the image right, so the up face corners turn by a quarter.
- The STE check reads the ignored reference folder on a local run. The resume prompt there has findings, and CI never sees them.

### Open questions that block progress

None for PR-76.

### Next concrete action

Answer the gitar pass of PR #117, then run `make codex-review PR=117` when every check but the Review gate workflow is green.

## Session 289: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-92, author, merge. Branch `fix/pr-92-review-fixes`. PR #116, pending merge. Base `e9a89aa`.

### What this session did, and why

- The gitar pass of `0f1242e` found one edge case with full merit: the provider gate did not read the handoff archive. `37fca06` fixes it, and the thread is resolved.
- `ci-windows-x64` failed at `37fca06` on a test that split the command output on a bare newline. `a09b9f9` splits it on the newline of the platform.
- Gitar approved `a09b9f9` with no open thread. A PR comment answers its CI note with D-251.
- Review round 1 (session 288) gave `Ready for owner merge` at the effective head `a09b9f9`, with no finding.
- The session asks the owner to confirm the merge with the merge summary (D-533).

### State of the build

- Remote `main` is `e9a89aa`. The effective head is `a09b9f9`. Each check but `review-gate` and `evaluate` passed at `a09b9f9`, and those two read the record of this commit.

### In flight

- The owner confirmation, then `gh pr merge 116 --auto --squash` (D-516).

### Traps and gotchas

- A test that reads console output splits on `Environment.NewLine`, because the Windows leg writes CR LF.

### Open questions that block progress

None for PR-92.

### Next concrete action

After the merge, the next session records exit test 3: the first night on `main` runs eight sweep jobs. The frame costs of F-184 wait for M-3 and PR-77.

## Session 288: 2026-09-27, Codex

Author: Codex
Session: PR-116, reviewer. Branch `fix/pr-92-review-fixes`. PR #116, pending merge. Base `e9a89aa`.

### What this session did, and why

- Reviewed all 60 changed paths of PR #116 against its roadmap, decisions, questions, and PR comments.
- Found no blocking defect. The review record approves effective head `a09b9f9`.
- The author entries name Claude Code. The Codex review passes the provider gate (T-4, D-101).

### State of the build

- Remote `main` is `e9a89aa`. The remote PR head before this metadata commit is `a09b9f9`.
- The local build passed with zero warnings. The focused review tests passed, 426 of 426.
- CI passed the build and test jobs on all three platforms, Smoke, bit identity, bots, the night gate, and the document and asset checks. `evaluate` and `review-gate` await this metadata commit.

### In flight

- The review record and this handoff entry need one metadata commit on `fix/pr-92-review-fixes`.
- The first night on `main` after merge must run eight sweep jobs. Exit test 3 assigns its record check to a later session.

### Traps and gotchas

- The PR comment says the missing review record caused the expected pre-review gate failure. The Gitar provider-gate finding is fixed and resolved.
- The owner accepted the review trust-boundary risk (D-647) and the night-gate window risk (D-654).
- The measured frame-cost fixes remain with M-3 and PR-77 (G-17).

### Open questions that block progress

None for PR-116.

### Next concrete action

Push the review record and this entry together. Verify the remote head and wait for the metadata checks. The owner can then review the merge summary.

## Session 287: 2026-09-27, Claude Code

Author: Claude Code
Session: PR-92, author. Branch `fix/pr-92-review-fixes`. PR #116, pending merge. Base `e9a89aa`.

### What this session did, and why

- The night fixer log printed "the night 36320653722 passed" at the first poll after the merge of PR #109. No night on `main` failed yet, so exit test 23 of PR-91 still waits for the Pushover notice and a fixer session.
- The owner report `/Volumes/SSD-1TB/what-you-carry-repository-review.md` was gone from the disk. The session rebuilt it from the full read of session 285 and the marks of PR #109, and restored it at the same path.
- The owner answered each open question of D-596: D-646 to D-659. PR-92 holds more than one concern (D-646).
- Code: the night keeps each failed seed and runs two shards of the two long sweeps (D-648, D-655). The override label reads the push time (D-653). The replay cuts a torn tail (D-656). `make claude-review` reviews a PR that Codex writes (D-649). The bot session takes `--policy`.
- The Deck found F-178: the move stick had no dead zone, and an idle player died at tick 310. D-658 fixes it. The owner read the HUD on the Deck (D-659).
- The Deck measured, two runs each with the enemies: a transition at 33.1 milliseconds, and the timer expiry at 56.5 and 56.7 (F-184).
- The session turned on Dependabot alerts, Dependabot security updates, secret scanning, and push protection (D-657).

### State of the build

- Remote `main` is `e9a89aa`. The local suite outside `Smoke` passed 2035 of 2036 tests, and the one STE failure has its fix. `ste-check` is clean.

### In flight

- The CI of PR #116, the gitar pass, then `make codex-review PR=116`.
- The first night on `main` after the merge runs eight sweep jobs (exit test 3). A later session records it.

### Traps and gotchas

- `pkill -f` over SSH matches its own command line when the pattern is in it. Run a Deck script from a file.
- GitHub makes a check suite for the last commit of each push alone, so D-653 reads the suites from the work head to the PR head.
- Each review command now refuses a PR whose author entries do not name the other provider.

### Open questions that block progress

None for PR-92.

### Next concrete action

Finish the review loop of PR #116. The frame costs of F-184 wait for M-3 and PR-77.
