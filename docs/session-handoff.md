# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 150: 2026-09-13, Codex

Author: Codex
Session: review PR #60 at effective head `1e3dba8`.

### What this session did, and why

- Checked the provider gate. Session 149 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, affected callers and tests, roadmap, design, decisions, questions, review records, and all PR comments.
- Found no in-scope defect. The parser rejects unknown words, unknown flags, repeated flags, short flags, and ignored combinations with contextual errors.
- Added `docs/reviews/pr-60.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `98e3c47`. The effective head is `1e3dba8`. The later handoff commit is metadata under D-184.
- The focused tests passed, 37 tests with 0 failures. The local build, full test, and det-lint commands produced no output and did not complete. Revision-matched remote CI passed the required code, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar checks.
- `evaluate` failed and `review-gate` was neutral before this review record existed. Fresh results are needed after the review commit.

### In flight

The review record and this handoff entry need a commit and push. Then verify the fresh review-gate result and the synchronized remote head.

### Traps and gotchas

- The effective head is `1e3dba8`, not the later metadata tip, under D-184.
- A new flag needs an entry in `UserArguments` and an ignored-flag rule when a session ignores it (D-313, D-317).
- The local dotnet commands can stop without output in this checkout. Remote CI provides separate evidence.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 151.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-99 remains open but blocks no work.

### Next concrete action

Commit and push this review and handoff. Verify the fresh review-gate result and the synchronized remote head.

## Session 149: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-61, the user argument check, as PR #60. Branch `feat/pr-61-user-argument-check`.

### What this session did, and why

- PR #59 merged as `98e3c47` at 02:21 UTC on 2026-09-13, and the Session 148 handoff named PR-61 as the next action. This session opened it from `main` per D-313.
- Before the code, the owner answered one scope question. A known flag that the session ignores passed in silence: `--contact-sheet sheet.png --smoke` rendered the sheet and ignored `--smoke`, and `--smoke --bot` never used the bot. D-317 puts that check in PR-61, with the recommendation.
- `WhatYouCarry.Game/UserArguments.cs` reads the user arguments once at boot. Its table holds each flag and its count of words. The parse stops on an unknown word, an unknown flag, a repeated flag, a short flag, and an ignored flag. Each error names the word or the flags.
- `Main` parses first in the boot, so a bad argument is a boot failure with exit code 1. `SmokeSession`, `BotSession`, `FrameLog`, `ContactSheet`, and `TestExit` read their flags through the parser. The trailing word check of `TestExit.PressOf` moved into the parser.
- `UserArgumentsTests.cs` holds exit tests 1 to 5 and 7, and `SmokeSessionTests.BadArgumentEndsTheSession` is exit test 6. The flag tests of four older test files read through the parser, and none passes the unknown flag `--other` now.
- `docs/decisions.md` gains D-317. `docs/design.md` and the Phase 2 roadmap name D-317 in the PR-61 entry, and the roadmap gains exit test 7. `CLAUDE.md` and `AGENTS.md` describe the check.
- The automated pass of gitar ran on `1e3dba8` after the push. Its check run passed, and it approved with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and no `Gitar review` comment was necessary.
- Session 139 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `98e3c47`. The effective head of PR #60 is `1e3dba8`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-61-user-argument-check` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 778 tests, 0 failures, with the five Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 28 files. `ste-check`: 0 findings in 15 files. The Godot build check passed.
- On `1e3dba8`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and gitar passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

PR #60: the Codex review per the `pr-review` skill at the effective head `1e3dba8`, then the owner merge. A docs PR then records the merge (D-297).

### Traps and gotchas

- Every user argument after `--` passes the parser now. An engine flag after the separator, such as `--windowed`, stops the boot, so it must stand before `--`.
- A word that starts with `--` is always a flag. A file path that starts with `--` needs a prefix, such as `./`.
- The contact sheet takes no other flag, and `--smoke` and `--bot` exclude each other (D-317). A PR that adds a flag adds it to the table of `UserArguments`, and to the ignored flag rule when a session ignores it.
- `SessionCommandsParse` reads the Godot commands of `CLAUDE.md`. A new command there with the separator must parse.
- A Godot command with no `--path` opens the project manager window and never quits. This session started one by mistake and stopped the process.
- The jobs of CI, bit identity, smoke, bots, and asset-qa on `1e3dba8` waited about 28 minutes for runners before they started. A long pending state there is the queue, and not a failure.
- `gh pr checks` exits with code 1 as soon as `evaluate` fails, while other checks still run. A wait loop must read the word `pending` in the output, and not the exit code.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 150.

### Open questions that block progress

None blocks PR #60. PR-15 has no open blocker, and its exit test 7 needs the owner to play the sword. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #60 per the `pr-review` skill at the effective head `1e3dba8` and writes `docs/reviews/pr-60.md`. The owner then merges, and a docs PR records the merge (D-297). PR-15 follows.

## Session 148: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-60 as PR #58 and four owner answers, D-313 to D-316 (D-297). Branch `docs/pr-60-merge-record`.

### What this session did, and why

- The owner asked the session to address the PR #58 re-review feedback. The PR had no open feedback at `9803a8b`: Session 146 fixed P2-2, and gitar approved the head. The trigger `--smoke --press escape 100 unexpected` ended at boot with exit code 1 and named `unexpected`. The review commit `e5a3846` raised P2-2, so the cross-provider repeat review of the fix was still necessary.
- Session 147 approved `9803a8b` in `e35d5e9`, and `review-gate` passed on that head. The owner merged PR #58 as `94f0897` at 00:16 UTC on 2026-09-13. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-60 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-60 and the mark in sequence item 8.
- The owner answered three questions, each with the recommendation. D-313 resolves OQ-170: one parser checks the whole user argument list, in PR-61 before PR-15. D-314 resolves OQ-5: heavy armor resists stagger, and light armor does not. D-315 resolves OQ-46: the initial combat numbers.
- PR-15 has no armor, so its exit test 1 and D-314 needed weight numbers that no decision held. The owner chose D-316: PR-15 builds the zero-weight case, and PR-22 sets the growth of the dodge cooldown with weight and the weight at which armor resists stagger, with the first armor sets.
- `docs/design.md` and the Phase 2 roadmap gain the PR-61 entry, and the Phase 2 sequence puts PR-61 at item 10. The later items move down by one. The PR-15 scope and exit test 1 follow D-316. The Phase 3 roadmap gives PR-22 the two weight numbers and exit test 7, `HeavyArmorResistsStagger`.
- Session 138 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `94f0897`, the squash merge of PR #58. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-60-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `94f0897`.
- `dotnet test`: 770 tests, 0 failures, with the four Smoke tests on the local Godot build. That run started before the last Phase 2 list fix and this entry. The run without the Smoke category passed on the final documents, 766 tests and 0 failures.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass (D-188, D-190). Then a fresh session opens PR-61 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- The Phase 2 sequence moved down by one after item 9. An older handoff that names item 10 or a later item means the item one number higher now.
- PR-61 moves the trailing word check of `TestExit.PressOf` into the shared parser. `SmokeSessionPasses`, `EscapeEndsTheSession`, and `StartButtonEndsTheSession` pass user arguments, so each must still pass.
- PR-15 builds the zero-weight case alone (D-316). Its exit test 1 has no weight clause, and PR-22 holds `WeightSlowsDodge` and `HeavyArmorResistsStagger`.
- The merge marks use the UTC date of the merge, so PR-60 reads 2026-09-13. The decisions and this entry use the local date 2026-09-12, as D-295 and Session 147 did.
- No decision names the author of the first animation files of PR-15. The PR-15 scope lists them as work of the PR, in the format of D-298.
- The next ids are D-317, OQ-171, F-96, PR-62, and Session 149.

### Open questions that block progress

None blocks this PR or PR-61. PR-15 has no open blocker, and its exit test 7 needs the owner to play the sword. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-61 from `main` per the Phase 2 roadmap and D-313. PR-15 follows it.

## Session 147: 2026-09-12, Codex

Author: Codex
Session: re-review PR #58 at effective head `9803a8b`.

### What this session did, and why

- Checked the provider gate again. The handoff identifies Claude Code as the author of the correction, so Codex remains the eligible reviewer under T-4 and D-101.
- Read the response file, the correction diff, the affected tests, the roadmap, the decisions, the questions, and all current PR comments.
- Verified that P2-2 is fixed. `PressOf` rejects a plain word after the press tick, names the word, and preserves flag ordering for the other parser.
- Verified the correction with the unit tests and the headless command. The command with `unexpected` now fails at boot with exit code 1 and no successful test-exit line.
- Updated `docs/reviews/pr-58.md` with P2-1 and P2-2 fixed, OQ-170 out of scope, and the verdict `Ready for owner merge` for effective head `9803a8b`.

### State of the build

- `main` and the merge base are `f3f0bc0`. The effective head is `9803a8b`. The later review metadata commits remain outside the effective diff under D-184.
- The focused tests passed, 15 tests with 0 failures. The full suite passed, 770 tests with 0 failures, as reported in the prior handoff. The build, det-lint, STE check, and revision-matched remote gates passed.
- `evaluate` and `review-gate` still read the earlier review record and fail until this update reaches the PR.
- Remote head: the review update is not pushed yet.

### In flight

The owner can merge PR #58 after the fresh review-gate result passes. OQ-170 remains open for whole-list validation in a later change.

### Traps and gotchas

- P2-1 and P2-2 are fixed at `9803a8b`. Keep both finding ids and their evidence in later records.
- OQ-170 accepts the remaining unknown-flag behavior across the other Game parsers. It does not block PR #58.
- The effective head is `9803a8b`, not a later metadata commit, under D-184.
- The next ids are D-313, OQ-171, F-96, and Session 148.

### Open questions that block progress

None blocks PR #58. OQ-170 blocks no work. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159, OQ-160, OQ-44, and OQ-99 do not block this PR.

### Next concrete action

Commit and push the updated review and handoff. Refresh `review-gate` and `evaluate`, then the owner can merge PR #58.

## Session 146: 2026-09-12, Claude Code

Author: Claude Code
Session: answer the PR #58 repeat review, P2-2. Branch `feat/pr-60-fullscreen-test-exit`.

### What this session did, and why

- Read `docs/reviews/pr-58.md` at the reviewed head `39a0c02`. P2-1 is fixed there. P2-2 has partial merit: a plain word after the press tick passed in silence, and T-2 binds the user arguments. The whole argument grammar is outside PR-60, because every older flag has the same property, so that part is OQ-170 for the owner.
- `TestExit.PressOf` reads the word after the tick when one exists. A word that does not start with `--` is a `ContextException` that names the word. A flag after the tick belongs to its own parser, so the two orders of `--smoke` and `--press` both work.
- `PressOfRejectsATrailingWord` asserts the error and the word for the trigger of the review and for a second number after the tick. It fails on the parser of `39a0c02`. `PressOfReadsTheInputAndTheTick` gains the two orders with a flag after the tick.
- The command `--smoke --press escape 100 unexpected` ends at boot with exit code 1 and names `unexpected`.
- `docs/reviews/pr-58-response.md` gains the P2-2 section and the checks of this head. `docs/questions.md` gains OQ-170.
- Session 136 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `f3f0bc0`. The effective head is the correction commit above the repeat review commit `e5a3846`, and it holds this entry, the response file, and the corrected files in one commit (D-182).
- Remote head: `origin/feat/pr-60-fullscreen-test-exit` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 770 tests, 0 failures, with the four Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. Core and content did not change.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #58: the second repeat Codex review of P2-2 at the correction head, then the owner merge. The automated pass on the correction head runs after the push, and the PR carries its result (D-250).

### Traps and gotchas

- The press parser rejects a plain word after the tick and accepts a flag there. It reads nothing else, so an unknown flag anywhere still passes in silence until OQ-170 has its answer.
- The automatic pass of gitar did not run on `39a0c02`, and the head had no `Gitar` check run at all. The comment `Gitar review` ran it, and the pass approved the head three minutes later (D-303). Read the check runs of the head before you post the comment.
- The effective head is the correction commit and not a later metadata commit (D-184).
- The next ids are D-313, OQ-171, F-96, and Session 147.

### Open questions that block progress

None blocks PR #58. OQ-170 blocks nothing. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews P2-2 per the repeat review procedure of the `pr-review` skill at the correction head and sets the verdict. The owner then merges, answers OQ-170, and a docs PR records the merge (D-297).

## Session 145: 2026-09-12, Codex

Author: Codex
Session: re-review PR #58 at effective head `39a0c02`.

### What this session did, and why

- Checked the provider gate again. The handoff identifies Claude Code as the author of the correction, so Codex remains the eligible reviewer under T-4 and D-101.
- Read `docs/reviews/pr-58-response.md`, the correction diff, the affected files, the roadmap, the decisions, the questions, and all current PR comments.
- Verified that P2-1 is fixed. The two Smoke tests drive Escape and Start through the headless Game process, and both assert exit code 0, the test exit line, and no smoke end line.
- Found P2-2. `TestExit.PressOf` accepts trailing arguments after the press tick. A command with `unexpected` exits successfully and ignores that argument, which violates T-2 input validation.
- Updated `docs/reviews/pr-58.md` with P2-1 fixed, P2-2 open, and the verdict `Changes required` for effective head `39a0c02`.

### State of the build

- `main` and the merge base are `f3f0bc0`. The effective head is `39a0c02`. The review and handoff metadata commits remain outside the effective diff under D-184.
- The serial build passed with 0 warnings and 0 errors. Focused tests passed, 7 tests with 0 failures. The Escape and Start command probes passed at tick 101. Det-lint and STE check passed.
- The full local test stalled after discovery and was cancelled. Revision-matched CI passed build, test, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar. `evaluate` and `review-gate` fail because P2-2 remains open.
- Remote head: `08e3797` holds the review update, verified with the session end gate.

### In flight

PR #58 needs trailing-argument validation and its regression test. The owner must request another repeat review after the correction.

### Traps and gotchas

- P2-1 is fixed at `39a0c02`. Keep its finding id and evidence in later reviews.
- P2-2 reproduces with `--smoke --press escape 100 unexpected`. The command must reject `unexpected` after the correction.
- The effective head is `39a0c02`, not a later metadata commit, under D-184.
- The next ids are D-313, OQ-170, F-96, and Session 146.

### Open questions that block progress

None blocks PR #58. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159, OQ-160, OQ-44, and OQ-99 do not block this PR.

### Next concrete action

The author rejects trailing arguments, adds the regression test, pushes the correction, and requests the repeat cross-provider review.

## Session 144: 2026-09-12, Claude Code

Author: Claude Code
Session: answer the PR #58 review, P2-1. Branch `feat/pr-60-fullscreen-test-exit`.

### What this session did, and why

- Read `docs/reviews/pr-58.md` at the reviewed head `3dfbf03`. P2-1 has full merit: exit tests 2 and 3 of the Phase 2 roadmap ask for a session that ends with exit code 0 and an end line, and the two tests proved only the predicate of `TestExit`.
- `TestExit` gains the `--press` flag. The two arguments after it name the input, `escape` or `start`, and the tick. A bad argument is a `ContextException` that names the cause (T-2). `Main` reads the flag at boot and gives the engine event of the press to the input singleton at that tick. The engine holds the input down from the next frame, and the poll of the next tick reads it as a real press.
- `SmokeSessionTests` gains `EscapeEndsTheSession` and `StartButtonEndsTheSession` in the Smoke category. Each runs the headless smoke session with a press at tick 100 and asserts exit code 0, the test exit line at a later tick, no error line, and no smoke end line. Both fail on the Game code of `3dfbf03`, where the session runs to its own end line.
- The unit tests of the predicate are `EscapePressesTheExit` and `StartButtonPressesTheExit` now. Two tests cover the flag parse and its errors. `NoOtherInputEndsTheSession` stays as exit test 4.
- `CLAUDE.md` and `AGENTS.md` gain the test exit session command. The PR-60 scope of the Phase 2 roadmap names the flag and the engine tests.
- `docs/reviews/pr-58-response.md` records the disposition, the correction, and the checks.
- Sessions 133 and 134 moved to the archive, because the file held twelve entries with this one.

### State of the build

- `main` is at `f3f0bc0`. The effective head is the correction commit above the review commit `0f344e6`, and it holds this entry, the response file, and the corrected files in one commit (D-182).
- Remote head: `origin/feat/pr-60-fullscreen-test-exit` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 769 tests, 0 failures, with the four Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. Core and content did not change.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #58: the repeat Codex review of P2-1 at the correction head, then the owner merge. The automated pass on the correction head runs after the push, and the PR carries its result (D-250).

### Traps and gotchas

- The press flag hands the engine one event through the input singleton. The event takes effect on the next frame, so the exit fires one tick after the press tick in a headless run at 60 Hz. The engine tests assert a range, not the exact tick.
- A press flag with a bad name or a bad tick is a boot failure with exit code 1, and never a session that runs.
- The Codex review session left eleven entries in the file. Count the entries before you add one, and move every entry past the tenth.
- The effective head is the correction commit and not a later metadata commit (D-184).
- The next ids are D-313, OQ-170, F-96, and Session 145.

### Open questions that block progress

None blocks PR #58. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews P2-1 per the repeat review procedure of the `pr-review` skill at the correction head and sets the verdict. The owner then merges, and a docs PR records the merge (D-297).

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
