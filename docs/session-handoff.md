# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 143: 2026-09-12, Codex

Author: Codex
Session: review PR #58 at effective head `3dfbf03`.

### What this session did, and why

- Verified the provider gate. The handoff identifies Claude Code as the author of the code commit, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, full effective diff, roadmap, decisions, questions, review records, and all PR comments.
- Found P2-1. The Escape and Start tests check only `TestExit.IsPressed`. They do not prove that `Main` writes the end line or quits with exit code 0, although D-311 and the roadmap require that behavior.
- Added `docs/reviews/pr-58.md` with the verdict `Changes required` for effective head `3dfbf03`.

### State of the build

- `main` and the merge base are `f3f0bc0`. The effective head is `3dfbf03`. The later handoff commit remains outside the effective diff under D-184.
- The serial local build passed with 0 warnings and 0 errors. Focused tests passed, 12 tests with 0 failures. Det-lint, STE check, bit identity, and the Godot build check passed.
- The local full test stalled after discovery and was cancelled. Remote CI reported in the previous handoff passed on the code head. The metadata-tip CI rerun passes the product jobs. `evaluate` fails and `review-gate` is neutral until the review record reaches the PR.
- `git fetch origin` could not open `.git/FETCH_HEAD` before the elevated retry. The local branch is at the remote PR metadata tip `5e4dfe1` after the review push.

### In flight

PR #58 needs end-to-end Escape and Start exit tests. The owner must merge only after the finding is corrected and the review gate passes for the effective head.

### Traps and gotchas

- The effective head is `3dfbf03`, not the metadata tip `74ab9d8`, under D-184.
- The focused tests pass because they call the helper directly. They do not run the game loop.
- The next ids are D-313, OQ-170, F-96, and Session 144.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-44 and OQ-99 do not block this PR.

### Next concrete action

The author adds integration coverage for both test exits, pushes the correction, and requests the repeat cross-provider review.

## Session 142: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-60, the fullscreen window and the test exit, as PR #58. Branch `feat/pr-60-fullscreen-test-exit`.

### What this session did, and why

- PR #57 merged as `f3f0bc0`, and the Session 141 handoff named PR-60 as the next action. This session opened it from `main` per D-310 to D-312.
- `WhatYouCarry.Game/project.godot` gains a `[display]` section with `window/size/mode=3`, the borderless fullscreen of the engine, and no window size, so the viewport takes the resolution of the display (D-310).
- `WhatYouCarry.Game/Input/TestExit.cs` holds the two exit inputs, the Escape key and the Start button of the first controller (D-311). `Main` polls it once per tick before the intent, in every session that runs the loop, and quits with the end line `The test exit ends the session.` and exit code 0 when the log holds no error line. `Main` now holds one poll for the reader and the exit.
- The fake poll of the reader tests moved to `WhatYouCarry.Tests/FakePoll.cs`, because the test exit tests share it. `TestExitTests.cs` holds exit tests 2 to 4, and `GameShapeTests.WindowOpensFullscreen` is exit test 1. The window test fails on the old project file with `The project has no display section.`
- `CLAUDE.md` and `AGENTS.md` update the play session line: the window opens fullscreen, Escape or Start ends the session, and the engine flag `--windowed` gives a window.
- The automated pass of gitar ran on `3dfbf03` after the push and approved it at 19:52 UTC with no comment, and its dashboard comment still shows the trial pause note (D-250). The comment `Gitar review` per D-303 got the reply `On it` at 19:53 UTC and no further output in fifty minutes. The approved check run on the head is the pass of D-250.
- Session 132 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `f3f0bc0`. The effective head of PR #58 is `3dfbf03`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-60-fullscreen-test-exit` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 765 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. The Godot build check passed.
- On `3dfbf03`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, and the night gate passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

PR #58: the Codex review per the `pr-review` skill at the effective head `3dfbf03`, then the owner merge. A docs PR then records the merge (D-297).

### Traps and gotchas

- Every session with a window now opens in fullscreen: the play session, the windowed contact sheet run, and the M-3 bot session. The engine flag `--windowed` overrides it. A headless run opens no window, and the smoke workflow proves it.
- The test exit polls the input once per tick in every session that runs the loop, the smoke and bot sessions too. A headless run has no key down, so CI never ends there. A windowed bot session ends on Escape, and the frame log still writes.
- No visual check of the fullscreen ran in this session, because a test cannot see the window. The play session of the owner is that check.
- The `evaluate` check fails until the review record exists, and the `review-gate` check stays grey (D-251).
- The pause note of gitar can show on a PR whose automatic pass ran and approved the head. Read the `Gitar` check run on the head before you post `Gitar review`, and count an `On it` reply with no later output as no new pass.
- The next ids are D-313, OQ-170, F-96, and Session 143.

### Open questions that block progress

None blocks PR #58. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #58 per the `pr-review` skill at the effective head `3dfbf03` and writes `docs/reviews/pr-58.md`. The owner then merges, and a docs PR records the merge. The owner answers OQ-5 and OQ-46 before PR-15.

## Session 141: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-14 as PR #56 and two owner instructions for PR-60, in the same invocation as Sessions 137 and 139 (D-297). Branch `docs/pr-14-merge-record`.

### What this session did, and why

- The owner merged PR #56 as `3d8060b` at 19:21 UTC, with the verdict `Ready for owner merge` for `c83360e` in `docs/reviews/pr-56.md`. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-14 merged in the roadmap entry and in sequence item 10. The Phase 2 roadmap gains the status line of PR-14 and the mark in sequence item 7.
- The owner asked how to run the game from the command line. `CLAUDE.md` and `AGENTS.md` gain the play session command and its quit keys.
- The owner gave two instructions for an immediate follow-up. D-310 opens the window in borderless fullscreen at the resolution of the display, on every desktop and on the Deck, because the default window of 1152 by 648 was tiny on a 4K screen. D-311 ends the game on Escape and on the controller Start button, for testing, until the escape menu of PR-53 replaces the exit.
- D-312 puts both changes in one PR, PR-60, before PR-15, as an exception to G-10. The owner chose borderless fullscreen, one PR, PR-53, and the Start button, against each recommendation.
- `docs/design.md` and the Phase 2 roadmap gain the PR-60 entry, and the Phase 2 sequence puts PR-60 at item 8. The later items move down by one. `docs/design.md` and the Phase 5 roadmap give PR-53 the escape menu and a sixth exit test.
- `.claude/skills/ste-writing/SKILL.md` gains the art terms of PR-14. Session 131 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `3d8060b`, the squash merge of PR #56. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-14-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed. `bit-identity`: `6ec00e90c1c85cdb` on PR #56.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. Then a fresh session opens PR-60 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- PR-60 changes every session with a window. The windowed contact sheet run and the M-3 bot session then open in fullscreen, and the engine flag `--windowed` overrides that. A headless run opens no window.
- Until PR-60 merges, the play session has no quit key: press Cmd+Q, or Ctrl+C in the terminal.
- The Phase 2 sequence moved down by one after item 7. An older handoff that names item 8 or a later item means the item one number higher now.
- The trial quota of gitar pauses the automatic pass for the whole period, so each push needs the comment `Gitar review` (D-303). The result arrives as an edit of a dashboard comment, and the summary text can repeat an earlier pass. The `Gitar` check run on the head proves a fresh pass.
- A Codex session can add an entry and skip the archive move. Count the entries before you add one, and move every entry past the tenth.
- The next ids are D-313, OQ-170, F-96, and Session 142.

### Open questions that block progress

None blocks PR-60. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-60 from `main` per the Phase 2 roadmap and D-310 to D-312. The owner answers OQ-5 and OQ-46 before PR-15.

## Session 140: 2026-09-12, Codex

Author: Codex
Session: re-review PR #56 at effective head `c83360e`.

### What this session did, and why

- Checked the provider gate again. Session 139 identifies Claude Code as the author of the correction, so Codex remains the eligible reviewer under T-4 and D-101.
- Read the response file, the correction diff, the new regression tests, the full review history, and all current PR comments.
- Verified that P2-1 is fixed. The palette and rule reads now convert read failures into contextual `ContextException` values, and the two regression tests pass.
- Updated `docs/reviews/pr-56.md` with the fixed finding, the effective head `c83360e`, the earlier verdict, and the current verdict `Ready for owner merge`.
- Session 130 moved to the archive because this file held eleven sessions with this entry.

### State of the build

- `main` and the merge base are `163742e`. The effective head is `c83360e`. The review and handoff metadata commits remain outside the effective diff.
- The focused texture, contact-sheet, and console tests pass, 57 tests with 0 failures. STE check and det-lint pass locally.
- The local full test and build commands produced no result and were cancelled. Session 139 reports 761 tests with 0 failures and a clean build on the effective head.
- Remote CI, bit identity, smoke, bots, det-lint, asset QA, STE check, night-gate, and the automated pass pass for `c83360e`. The review-gate and evaluate checks failed before this updated review record. Fresh checks for the metadata tip were pending at the review.

### In flight

The updated review record and this handoff entry need a commit and push. The owner can merge after the fresh review-gate and required checks pass.

### Traps and gotchas

- P2-1 keeps its id and its original trigger. Its status is `fixed in c83360e`.
- The effective head is `c83360e`, not the metadata tip, under D-184.
- The local full build can stop without output in this checkout. Remote CI and the author report provide separate evidence.
- The next ids are D-310, OQ-170, F-96, and Session 141.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-44 blocks PR-18. OQ-99 remains open but blocks no work.

### Next concrete action

Commit and push this repeat review and handoff. Verify the fresh review-gate result and the synchronized remote head.

## Session 139: 2026-09-12, Claude Code

Author: Claude Code
Session: answer the PR #56 review, P2-1. Branch `feat/pr-14-texture-generator`.

### What this session did, and why

- Read `docs/reviews/pr-56.md` at the reviewed head `9236744`. P2-1 has full merit: an unreadable palette, rule, or rule directory made `texture-gen` end with an unhandled `UnauthorizedAccessException` and exit code 134, and not with exit code 1 and the path (T-2).
- `ReadFile` and `ReadRules` of `TextureGenCommand` now turn a read failure into a `ContextException` that names the path. The rule directory listing and each rule read share one boundary.
- Two regression tests make the rule or the palette unreadable on every platform, and they assert exit code 1, the file name, and no atlas. Both failed on `9236744`, and both pass on the correction.
- `docs/reviews/pr-56-response.md` records the disposition, the correction, and the checks.
- Sessions 129 and 128 moved to the archive, because the file held twelve entries with this one.
- The trial quota kept the automatic pass of gitar paused. The comment `Gitar review` ran it on demand, and its check run on `c83360e` passed at 19:00 UTC with an approval and no comment (D-250, D-303). The dashboard summary repeats the text of the first pass.

### State of the build

- `main` is at `163742e`. The effective head is `c83360e`, the correction commit above the review commit `cdad3f4`. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-14-texture-generator` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 761 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 26 files. `ste-check`: 0 findings in 15 files. Core, Game, and content did not change.
- On `c83360e`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and the gitar check passed. The `review-gate` check fails, because `docs/reviews/pr-56.md` still gives `Changes required` for `9236744`, and `evaluate` fails with it (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #56: the repeat Codex review of P2-1 at `c83360e`, then the owner merge. The automated pass approved the correction.

### Traps and gotchas

- A test that needs an unreadable file removes every permission on Linux and macOS and holds the file with no share on Windows. It proves the file unreadable first, so a user that permissions do not bind fails the test and never passes it.
- The rule directory has no automated unreadable test, because Windows gives no plain way to make a directory unreadable. The listing shares the catch of the rule test.
- The effective head is the correction commit `c83360e`, not a later metadata commit (D-184).
- Session 138 left eleven entries in the file. Count the entries before you add one, and move every entry past the tenth.
- The next ids are D-310, OQ-170, F-96, and Session 140.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews P2-1 per the repeat review procedure of the `pr-review` skill at the effective head `c83360e` and sets the verdict. The owner then merges.

## Session 138: 2026-09-12, Codex

Author: Codex
Session: review PR #56 at effective head `9236744`.

### What this session did, and why

- Verified that the Session 137 handoff identifies Claude Code as the author of the substantive PR commit. Codex is the eligible reviewer under T-4 and D-101.
- Read the complete PR diff, the texture contracts, the content loader, the atlas consumers, the contact sheet, the tests, the workflow, the roadmap, the decisions, the questions, and every PR comment.
- Found P2-1: an unreadable palette or rule file escapes the command error boundary. A direct unreadable-rule probe produced an unhandled `UnauthorizedAccessException` and exit code 134 instead of exit code 1 with file context.
- Added `docs/reviews/pr-56.md` with the verdict `Changes required` for effective head `9236744`.

### State of the build

- `main` and the merge base are `163742e`. The effective head is `9236744`. The two later handoff commits and the automated-pass note change metadata paths only.
- Focused texture and contact-sheet tests pass, 54 tests with 0 failures. STE check, det-lint, asset QA, texture generation, and bit identity pass locally.
- The local build produced no output and was cancelled. The local full suite did not complete after the smoke portion started. The handoff reports the completed build and 759 passing tests on this head.
- Remote asset QA, bots, compare, det-lint, bit identity, smoke, macOS, Windows, STE check, and night-gate checks pass. Linux CI was pending when checked. Evaluate failed and review-gate skipped before the review record existed.

### In flight

PR #56 needs the unreadable-input correction and its regression tests. The review record and this handoff entry need a commit and push after the owner correction.

### Traps and gotchas

- `texture-gen` catches `ContextException` around atlas construction, but direct file reads in `ReadFile` and `ReadRules` can throw `IOException` or `UnauthorizedAccessException`.
- The effective review head is the feature commit `9236744`, not the metadata tip, under D-184.
- The local dotnet build and the smoke portion of the full suite can stop without output in this checkout. Remote results remain separate evidence.
- The next ids are D-310, OQ-170, F-96, and Session 139.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-44 blocks PR-18. OQ-99 remains open but blocks no work.

### Next concrete action

Correct P2-1 with unreadable palette and rule regression tests, push the correction, and run the automated pass before the repeat Codex review.

## Session 137: 2026-09-12, Claude Code

Author: Claude Code
Session: answer OQ-1 and open PR-14, the texture generator and the palette, as PR #56. Branch `feat/pr-14-texture-generator`.

### What this session did, and why

- PR #55 merged as `163742e` at 14:18 UTC with the `review-override` label. The night of 2026-09-12 ran at `811aa84` and ended with success at 13:04 UTC.
- Sequence item 6 of the Phase 2 roadmap is the owner answer to OQ-1. The session built a preview page of three palettes, each painted on the seven blocks and a miner at game zoom, and asked the owner. The owner chose candidate A, "Lamp and Rock" (D-304).
- Four more questions blocked PR-14, and the owner took each recommendation on the day. OQ-166 asked where the palette and the rules live, because the Core loader stops on unclaimed JSON (D-305). OQ-167 asked where the contact sheet renders (D-306). OQ-168 asked which materials PR-14 ships (D-307). OQ-169 asked how a body face reads its tile, because the model used the 64 px net of a Minecraft skin (D-308).
- `WhatYouCarry.Tools/TextureGen/` is the command `texture-gen`. It reads the palette and ten rules, paints each tile from a xorshift sequence of its seed, and writes an indexed PNG with stored deflate blocks. The tiles equal the preview pixel for pixel, the file has one byte form on every platform, and a test holds the committed atlas equal to the output.
- `WhatYouCarry.Assets/AtlasLayout.cs` holds the tile layout for Game and Tools. `ContentLoader.IsAssetPath` names the `models/` and `textures/` directories, and the three content sources skip both. `Game/World/AtlasFile.cs` loads the atlas at boot, and the placeholder atlas of PR-13 is gone.
- `content/models/player.bbmodel` has the resolution 256, and each face reads its body tile at 32 texels per meter. A script rewrote the face rectangles and changed no other line.
- The Game flag `--contact-sheet <png>` renders the seven blocks and the body from two sides at game zoom. The owner approved the first sheet as drawn, and D-309 records the ten rule values, which closes exit test 5.
- Session 127 moved to the archive, because the file held eleven entries with this one.
- The trial quota paused the automatic pass of gitar on the first push. The comment `Gitar review` ran it on demand, and it approved `2b52074` with no finding (D-250, D-303).

### State of the build

- `main` is at `163742e`, the squash merge of PR #55. This branch holds the feat commit `9236744` and two docs commits of this entry above it. The effective head is `9236744`, because the handoff and the archive are metadata paths (D-184).
- Remote head: `origin/feat/pr-14-texture-generator` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 759 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 26 files. `asset-qa`: 0 findings, 1 model. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`, unchanged.
- The Godot editor build and the windowed contact sheet run end with exit code 0.
- On `2b52074`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and the gitar check passed. The review gate `evaluate` job fails, and `review-gate` skips, until the review record exists (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #56: the Codex review. The automated pass approved the head, and no open question binds the PR.

### Traps and gotchas

- A change to the palette or a rule needs `texture-gen --root .` and a commit of `content/textures/atlas.png`, or `CommittedAtlasMatchesTheGenerator` fails. A rule change also needs a new contact sheet for the owner (D-309).
- The contact sheet needs a window. A run with `--headless` ends with exit code 1 by design, and `ContactSheetFailsHeadless` holds that.
- In zsh, `status` is a read-only variable, and a variable that holds a command with its arguments does not split into words. Name the exit code `rc`, and write each tool command in full.
- Godot has a class `AtlasTexture`, so a Game class of that name is ambiguous under `using Godot`. The loader is `AtlasFile`.
- The body tiles are 8, 9, and 10 (D-307). A new block takes the tile of its id, and a new body material takes the next free tile of row 1.
- The next ids are D-310, OQ-170, F-96, and Session 138.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass on PR #56, then a Codex session reviews it per the `pr-review` skill at the effective head `9236744`. After the merge, the owner answers OQ-5 and OQ-46, and a fresh session opens PR-15.

## Session 136: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-57 as PR #54, in the same invocation as Session 134 (D-297). Branch `docs/pr-57-merge-record`.

### What this session did, and why

- The owner merged PR #54 as `811aa84` at 07:17 UTC, with the verdict `Ready for owner merge` for `ecdc95a` in `docs/reviews/pr-54.md`. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-57 merged in the roadmap entry and in sequence item 10. The Phase 2 roadmap gains the status line of PR-57 and the mark in sequence item 5.
- `docs/questions.md` needs no addendum, because every question that PR-57 raised had its answer before the merge.
- Session 126 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `811aa84`, the squash merge of PR #54. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-57-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed. `bit-identity`: `6ec00e90c1c85cdb` on PR #54.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. Exit test 7 of PR-13 waits for the M-3 run on the Deck (OQ-161).

### Traps and gotchas

- The automatic pass of gitar pauses when the trial quota of the period is used, and the comment `Gitar review` runs one on demand (D-303).
- The handoff held eleven entries with this one. Count the entries before you add one, and move every entry past the tenth.
- The next ids are D-304, OQ-166, F-96, and Session 137.

### Open questions that block progress

OQ-1 blocks PR-14 and sequence item 6. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. The owner then answers OQ-1, the palette, and a fresh session opens PR-14 from `main` per the Phase 2 roadmap.

## Session 135: 2026-09-12, Codex

Author: Codex
Session: review PR #54 at effective head `ecdc95a`. Branch `feat/pr-57-asset-qa-gate`.

### What this session did, and why

- Verified that Claude Code authored the substantive PR commits, so Codex is the eligible reviewer under T-4 and D-101.
- Read the complete diff, the animation and model content contracts, affected callers, tests, workflow, roadmap, decisions, questions, and every PR comment.
- Found no defect. The automated pass finding on duplicate clip and unknown-bone reports is corrected in `0cb62c7` and covered by two regression tests.
- Added `docs/reviews/pr-54.md` with the verdict `Ready for owner merge` for the effective head. The effective head includes the D-303 process commit `ecdc95a`.

### State of the build

- `main` and the merge base are `5848bda`. The effective head is `ecdc95a`. The review metadata tip is `632e281` before this correction commit.
- Focused asset, animation, pose, and overlap tests pass, 80 tests with 0 failures. The local build attempt hung without output and was cancelled.
- Remote build and test, asset QA, bots, bit identity, det-lint, STE check, night gate, and smoke pass. The pre-review evaluate check failed because the review file did not exist, and review-gate skipped for the same reason.

### In flight

The review record and this handoff entry need a commit and push. The fresh evaluate and review-gate checks must pass against the published review record.

### Traps and gotchas

- The effective head is `ecdc95a`, because `.claude/skills/pr-review/SKILL.md` changed in that commit. The review and handoff paths alone are metadata under D-184.
- The local full build did not produce output after several minutes. Remote CI is the build evidence for this review.
- The automated pass was paused before the owner requested the on-demand Gitar review. The on-demand review approved `0cb62c7` after the correction.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind no work. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the review record and handoff entry. Fetch the remote. Confirm that the remote has no ahead count and that the fresh review-gate check passes.

## Session 134: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-57, the asset QA gate v1, as PR #54. Branch `feat/pr-57-asset-qa-gate`.

### What this session did, and why

- PR #53 merged as `5848bda`. The Phase 2 sequence puts PR-57 next, so the session opened it from `main` per the roadmap and D-149.
- Five questions blocked the exit tests, and the owner answered all five on the day. OQ-45 blocked exit test 1, because the tool checks every keyframe and no format existed. OQ-162 asked where the one model reader lives, because the PR-13 reader used engine vectors in Game and Tools cannot reference Game. OQ-163 asked how an overlay box pairs with its limb box. OQ-164 asked what a clip is. OQ-165 asked what a file name reference is. D-298 to D-302 record the answers.
- The first answer on the clip rule took every pair with zero tolerance, and the second answer, on keyframes in v1, made every bent elbow a clip. The session quoted both, and the owner exempted the pairs of a bone and its parent (D-301).
- `WhatYouCarry.Assets` is the fifth project (D-299). The Blockbench reader moved into it from Game with the Core vector, and `AnimationLoader`, `RotationMatrix`, `ModelPose`, and `BoxOverlap` joined it. Game converts each vector where it builds a node.
- `WhatYouCarry.Tools/AssetQa/` holds the command `asset-qa` and the three checks: `ClipCheck`, `OverlayCheck`, and `FileCaseCheck`. `AssetSet` reads every model, overlay, and animation, and a file that does not load is a finding and not a stop.
- `ContentLoader.ModelDirectory` names the directory that every content source skips (D-298), in Game, in the bot runner, and in the tests.
- `.github/workflows/asset-qa.yml` is the new job, and `CLAUDE.md` and `AGENTS.md` carry the command, the gate line, and the Assets rule.
- Exit tests 1 to 5 pass, with 82 new tests. The command on the checkout reports 0 findings over 1 model.
- The automated pass on `73ba35b` had one comment, and it has merit: a pair of two body boxes counted once per overlay, and an unknown bone in an animation gave one finding per overlay. `0cb62c7` counts a body pair on the pass with no overlay alone and checks the tracks of an animation once, with two regression tests that fail on `73ba35b`. The reply on the thread names the commit. The automatic pass was paused by the trial quota after the push, and the comment `Gitar review` ran one on demand, which approved `0cb62c7` with the one finding resolved (D-250).

### State of the build

- `main` is at `5848bda`, the squash merge of PR #53. This branch holds the feat commit `73ba35b`, the fix commit `0cb62c7`, and the docs commit that records D-303 above it. The effective head is the docs commit, because a new decision moves it.
- Remote head: `origin/feat/pr-57-asset-qa-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 704 tests, 0 failures, with the smoke test on the local Godot build. Core gained one constant and no behavior.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 24 files. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. `asset-qa`: 0 findings, 1 model, 0 overlays, 0 animations.
- The Godot editor build, the headless smoke session, and the headless bot session end with exit code 0. The bot reaches floor 2 of seed 1 at tick 421.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The run list held no later night at 06:22 UTC on 2026-09-12. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

PR #54: the Codex review. The automated pass approved the effective head. No open question binds it.

### Traps and gotchas

- A `.json` file under `content/models/` is an animation, and the Core loader never sees it. A `.json` file in any other unclaimed directory still errors in the Core loader.
- A file that is not JSON gives two findings: one from the loader and one from the file case check. Each check reads the file on its own.
- The euler order is the order of Blockbench: the matrix is Rz times Ry times Rx. The test `RotationOrderIsBlockbenchOrder` pins it, and the pose tests read meters and not file units.
- The clip check poses the body with each overlay alone, never two overlays together, because two pieces for one slot enclose the same limb.
- A texture `path` in a model file is a machine path that Blockbench writes, and the file case check flags a rooted reference. The player model has no texture, and PR-14 assigns the atlas.
- The frame log of a `--fixed-fps` run reads the fixed frame time. M-3 runs without that flag.
- The automatic pass of gitar pauses when the trial quota of the period is used. The comment `Gitar review` on the PR runs one on demand (D-303).
- The next ids are D-304, OQ-166, F-96, and Session 135.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #54 per the `pr-review` skill, at the effective head, which is the docs commit above `0cb62c7`. After the merge, the owner answers OQ-1, and a fresh session opens PR-14.

## Session 133: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-13 as PR #52 and the owner answer on documentation PRs (D-297), in the same invocation as Session 131. Branch `docs/pr-13-merge-record`.

### What this session did, and why

- The owner merged PR #52 as `9749581` at 04:48 UTC, with the verdict `Ready for owner merge` for `9085a95` in `docs/reviews/pr-52.md`. CI, smoke, bit identity, bots, det-lint, and STE check passed on the merge commit.
- The session asked where the merge record runs, because D-121 gives one PR per session and Session 131 opened PR #52. The owner answered that a documentation PR needs no new session, and only a code PR does. D-297 records it and revises in part D-121, the count of PRs only.
- `docs/design.md` marks PR-13 merged in the roadmap entry and in sequence item 10, and section 3.14 cites D-297. The Phase 2 roadmap gains the status line of PR-13 and the mark in sequence item 4.
- `docs/questions.md` gains a dated addendum on OQ-159, OQ-160, and OQ-161: the merge came with the three open.
- `CLAUDE.md` and `AGENTS.md` state the session rule with D-297. Sessions 123 and 122 moved to the archive, because the file held eleven entries.

### State of the build

- `main` is at `9749581`, the squash merge of PR #52. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-13-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. `dotnet test` without the Smoke category: 0 failures. No code changed. `bit-identity`: `6ec00e90c1c85cdb` on PR #52.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. The owner answers OQ-159, OQ-160, and OQ-161 in `docs/decisions.md` as the next ids. Exit test 7 of PR-13 waits for the M-3 run on the Deck.

### Traps and gotchas

- D-297 lets a documentation PR follow the code PR in one invocation. A second code PR still needs a fresh session (D-121).
- The handoff held eleven entries after the Codex review session. Count the entries before you add one, and move every entry past the tenth.
- The M-3 command in `CLAUDE.md` runs without `--headless`, `--write-movie`, and `--fixed-fps`, or the frame log reads a fixed frame time.
- The next ids are D-298, OQ-162, F-96, and Session 134.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-57, the asset QA gate v1, from `main` per the Phase 2 roadmap. The owner runs the M-3 command on the Deck when OQ-161 has its answer.
