# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 30: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Read the four P2 findings in `docs/reviews/pr-12.md`. Each one reproduces, and each one has full merit.
- P2-1. `Atan2` read the sign of y with `y < 0.0f`, and negative zero is not below zero. `Atan2(-0, -1)` gave pi against the reference -pi, an error of two pi. The sign now comes from `float.IsNegative` (F-62).
- P2-2. A `StateHash` from `default` held zero and not the FNV offset basis, and it accepted fields in silence. The struct now rejects a hash that `Start` did not make (T-2, F-63).
- P2-3. The reflection rule read the namespace text alone, so `typeof(x).GetMethods()` gave no finding. A reflection member name is now a finding on its own. The list holds no name that a Core type can hold too, so `block.Type` stays clean.
- P2-4. The `MathF` exemption compared the file name alone. It now compares the whole Core-relative path (F-64).
- The bit-identity sweep never made a negative zero, so the three-platform check could not have caught P2-1. The sweep now holds the four sign pairs. The pinned hash moved from `ef592d4148eb8ba0` to `4d6385bb92454694`. Core gives the same numbers for every input the old sweep read, so this is a wider check and not a behavior change.
- Wrote `docs/reviews/pr-12-response.md` with the disposition and the evidence for each finding.
- 11 new tests. The total is 144, and all pass.

### State of the build

- `main` is at `86078b8`. The branch holds the review commit and this correction commit above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 144 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- Each of the five review triggers failed against `c2ba592` before the corrections.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- Negative zero passes every comparison against zero. Read the sign bit with `float.IsNegative` when the sign matters.
- A struct that `default` makes skips every factory. A hash, a counter, or any accumulator with a non-zero start needs a guard.
- A determinism sweep proves only what it reads. The `Atan2` grid held no negative zero, so it could not see the defect. Widen the sweep with each defect it missed.
- A first correction for P2-1 added a branch for a negative zero x. A check showed the branch changed no result, and it is gone. Test a defensive branch before you keep it.
- The pinned bit-identity hash changes when the sweep grows, not only when Core changes. Say which one it was.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 29: 2026-09-08, Codex

Author: Codex
Session: review PR #12. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Reviewed PR #12 at effective head `c2ba592` against base and merge base `86078b8`.
- Confirmed the provider gate. Claude Code wrote the substantive change, and Codex reviewed it.
- Found four P2 defects in DetMath, StateHash, and the determinism lint.
- Wrote `docs/reviews/pr-12.md` with the verdict `Changes required`.

### State of the build

- The local build passes with 0 warnings and 0 errors.
- The local test suite passes with 133 tests and 0 failures.
- The lint, STE, bit-identity, and Godot checks pass.
- The remote branch contains the review record and this entry.

### In flight

PR #12 needs P2-1 through P2-4 corrected and a repeat review.

### Traps and gotchas

- A numeric grid that contains zero does not necessarily contain negative zero.
- A public struct always permits default initialization.
- A namespace scan does not detect all reflection calls.
- A file-name check does not identify one canonical path.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 through P2-4, add regression tests, and request a repeat Codex review.

## Session 28: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-3, the seeded RNG, DetMath, the lint tool, and the bit-identity job. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Started PR-3 after the owner merged PR #11. Session 27 named it as the next action.
- Four owner questions blocked the start, and the owner answered all four (D-200 to D-203, OQ-73 to OQ-76).
- The largest one refutes D-161. That decision named a reduction to [-pi, pi], degree-7 minimax polynomials, and an absolute error of at most 1e-6. A Remez fit of degree 7 on [-pi, pi] reaches 2.5e-4 for sine, 250 times the target. D-203 folds to [-pi/4, pi/4] and a quadrant instead, and the degree and the target stand (F-60).
- Wrote `Rng.cs` (xoshiro128** with SplitMix64 seeding), `DetMath.cs`, `StateHash.cs`, and `RngStream.cs` in Core.
- Wrote the `det-lint` command, which parses each Core file with the C# compiler API (D-202), and the `bit-identity` command (D-201).
- Wrote `.github/workflows/bit-identity.yml` and `.github/workflows/det-lint.yml`. The bit-identity workflow passes each hash up as a job output, so it needs no artifact action.
- 70 new tests. The total is 133, and all pass. Exit tests 1 to 7 each have a test, and the CI run proved exit test 8.
- Corrected exit test 5. The roadmap asked for the opposite of D-160, and no FNV-1a hash can hold the roadmap form (F-61).
- Updated the PR template. PR-3 creates the lint tool and the bit-identity job, so the two bootstrap notes are gone.

### State of the build

- `main` is at `86078b8`, the squash merge of PR #11. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 133 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The bit-identity job on PR #12 proved exit test 8. Linux x64, Windows x64, and macOS arm64 each answered `ef592d4148eb8ba0`, and the compare job agreed. This is the first live proof of G-9.
- Every check on PR #12 is green: CI on the three platforms, `bit-identity` on the three platforms and its compare job, `det-lint`, and `ste-check`. The `review-gate` check is neutral with the title "No review record", which is the advisory grey of D-181.
- The Godot 4.7.2 headless build check passes with `/Applications/Godot_mono.app/Contents/MacOS/Godot`. The name `Godot` is not on the command path.

### In flight

PR #12 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- Two numbers proved a documented plan wrong this session. Measure a numeric claim before you build on it.
- .NET never fuses a multiply and an add, so every float intermediate is the same on the three platforms. That is the whole basis of DetMath.
- The angle limit is 4096 radians. At 65536 the error reaches 9.6e-07 and the 1e-6 gate has no margin left.
- `Enum.IsDefined` reads the enum through reflection, which G-2 bans in Core. `Rng.ForStream` uses an explicit bound, and `EveryDeclaredStreamIsAccepted` guards it.
- A Roslyn scan of `System.Math.Sin` puts `Math` in the `Name` of an inner member access, not in the `Expression`. `Vector128<float>` is a `GenericNameSyntax` and never an `IdentifierNameSyntax`. Both defects passed the first build and failed the tests.
- An RngStream number is part of the seed. A new subsystem appends a value, and it never renumbers one.
- The bit-identity hash is pinned in `BitIdentityTests`. A deliberate Core change updates it, and G-20 asks the review to confirm the version bump.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #12 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-12.md`. After the merge, PR-4 starts: the logger, the error context, and the assertions.

## Session 27: 2026-09-08, Claude Code

Author: Claude Code
Session: the session end gate for the `pr-review` skill. Branch `docs/pr-review-push-gate`, PR #11.

### What this session did, and why

- The owner asked for a fix after the reviewer left its commit unpushed on the shared checkout twice on PR #10 (F-59). The skill said "push" in three places, with no verification step and no evidence trail.
- Added a "Session end gate" section to the skill. Four commands after the commit, and the evidence comes from the remote: `git status --short --branch` shows no `[ahead N]`, and `gh pr view --json headRefOid` equals `git rev-parse HEAD`.
- Added the push line as a required part of the Verification section in the review skeleton. A record with no push line is incomplete.
- Added the failure path. A denied push does not end the session. The session asks the owner to approve it and says in the handoff that the record is unpushed. A sandbox that blocks the network denies a push in silence, so the status line is the evidence and not the push output.
- Added the start-of-session check for both roles. If the checkout is ahead with the other provider's commit, push it first and record that.
- The owner also chose the rule for every session, not only reviews. D-199 records it, and revises in part D-146: the state of the build names the remote head. `CLAUDE.md` and `AGENTS.md` carry the rule in the session handoff section.
- This is the second PR of one harness invocation, after PR #10, on the owner instruction. D-121 names one PR per session.

### State of the build

- `main` is at `d5eb298`, the squash merge of PR #10. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/docs/pr-review-push-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` and `dotnet test` pass: 63 tests, 0 failures. The checker reports 0 findings.

### In flight

PR #11 changes only `docs/`, `CLAUDE.md`, `AGENTS.md`, and `.claude/skills/`, so the `review-override` label covers it (D-190). The owner asked for the label in the instruction, and this session added it.

### Traps and gotchas

- The evidence for a push is the remote, never the local checkout. A sandbox denial gives no git error.
- A revision of one part of a decision needs the `Revised in part by` marker on the old row, and the reference check does not flag it (D-186).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #11 with the `review-override` label. Then a new session starts PR-3: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 26: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `b9bc6cd` against base and merge base `94aadc5`.
- Confirmed that P2-1 to P2-4 are resolved.
- Updated `docs/reviews/pr-10.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 is ready for owner merge.

### Traps and gotchas

- The mask pass must protect every delimiter inside an earlier masked span.
- The review commit is metadata. The review head remains `b9bc6cd` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #10. Then PR-3 starts with the seeded RNG, DetMath, the lint tool, and the bit-identity job.

## Session 25: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 repeat review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the repeat review in `docs/reviews/pr-10.md`. P2-1 to P2-3 are resolved, and P2-4 is new.
- P2-4 has full merit. The quote pass masked the text inside a quote, but it left a quoted `)` as a plain character, so the parenthesis pass paired the outer `(` with it. `MaskChar` now masks both parentheses, both quote forms, and the backtick inside a span, and `Unmask` restores them. Four new assertions in `NestedParenthesesAreOneOpaqueWord` cover it.
- Updated `docs/reviews/pr-10-response.md` with the P2-4 disposition.
- The review commit `712b2e0` was on the local checkout and not on the remote, as `29b1066` was before it. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code correction too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- A mask pass must mask every delimiter inside its span, not only the sentence punctuation. A later pass reads any character that the earlier pass left plain.
- An unbalanced quote or an unclosed parenthesis stays plain text. Each token then counts on its own.
- The reviewer's commit was unpushed for the second time. Check `git status` before you start, and push it.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 at the new effective head and updates `docs/reviews/pr-10.md`.

## Session 24: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `d55666d` against base and merge base `94aadc5`.
- Confirmed that the three prior P2 findings are resolved.
- Found P2-4. A quoted close parenthesis can end an outer parenthesized span.
- Updated `docs/reviews/pr-10.md` with the new finding and the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot check did not complete because Godot could not write its macOS support file.

### In flight

PR #10 needs P2-4 corrected and a repeat review at the new effective head.

### Traps and gotchas

- The masking passes run in sequence. A later pass can read delimiters that an earlier pass already made opaque.
- `FindSpanEnd` fixes nested parentheses of one type. It does not protect against delimiters from another span type.
- The review commit is metadata. The review head remains `d55666d` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-4, add the regression tests, and request another repeat Codex review.

## Session 23: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the three P2 findings in `docs/reviews/pr-10.md` and assessed each against the evidence.
- P2-1 has partial merit. The splitter requires a space after a colon on purpose: the `ste-writing` skill holds a bare URL in prose, and a split at every colon cuts it, and cuts every time and ratio. The roadmap and the skill over-claimed with "everywhere". Both now state the space condition, and `ColonInsideAWordDoesNotEndASentence` covers it.
- P2-2 has full merit. Nested parentheses ended the span early. `FindSpanEnd` now counts depth, and `NestedParenthesesAreOneOpaqueWord` covers it.
- P2-3 has full merit. A trailing argument after `--root` was silent. The loop now rejects every argument that is not a `--root <value>` pair, with exit 2 and a message that names the argument. Three new assertions cover it.
- Wrote `docs/reviews/pr-10-response.md`.
- The review commit `29b1066` was on the local checkout and not on the remote. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code corrections too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- The reviewer commits on this checkout when both providers share it. Check `git status` for an unpushed commit before you start, and push it.
- `Sentence.Words` holds the masked tokens. Unmask a word before you compare it to text.
- Count the words of a test sentence with the splitter, not in your head. Three expectations in this PR were off by one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 per the repeat review procedure and updates `docs/reviews/pr-10.md` to the new effective head.

## Session 22: 2026-09-07, Codex

Author: Codex
Session: PR-10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Reviewed PR #10 at effective head `3ea5f23` against base and merge base `94aadc5`.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Found three P2 defects in the sentence splitter and command argument parser.
- Wrote `docs/reviews/pr-10.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 61 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 needs the three P2 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The checker only treats a colon as a sentence end when whitespace or the line end follows it. The project rule says that every colon ends a sentence.
- Nested parentheses do not remain one opaque word. The mask pairs the first opening parenthesis with the first closing parenthesis.
- `ste-check` ignores a trailing argument after a valid `--root` pair and returns the repository result.
- The review commit is metadata. The review head remains `3ea5f23` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 to P2-3, add the regression tests, and request a repeat Codex review.

## Session 21: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-2, the STE checker. Branch `feat/pr-2-ste-check`, PR #10.

### What this session did, and why

- Started PR-2 after the owner merged PR #9. Session 19 named it as the next action, and nothing blocked it.
- Wrote the `ste-check` command of `WhatYouCarry.Tools` (D-130, D-139). One reader turns a Markdown file into prose lines, one splitter turns a line into sentences and words, and one rules class holds the seven STE rules. The reference check (D-178, D-186) and the session number check (D-187) are two more classes. `RepositoryCheck` runs all three over one checkout.
- The splitter masks text in backticks, double quotes, and parentheses. Each span is one opaque word (8.5, 8.6), and no grammar rule reads inside it. A colon ends a sentence everywhere (8.4).
- Wrote 25 tests. Exit tests 1 to 9 of the roadmap each have a test, and the command test runs `Program.Main` on a throwaway checkout for the exit codes 0, 1, and 2.
- Wrote `.github/workflows/ste-check.yml` with the one job `ste-check`. It runs on Linux only, because the checks read text.
- The first run found 77 findings in 15 files: 52 passive, 15 helper verbs, 5 over 25 words, and 5 -ing forms. Two were rule gaps. A hyphenated identifier in a skill front matter matched the -ing rule, and the noun "finding" matched it after "per". Both are exclusions now, with tests. The other 75 were real, and each sentence is rewritten with the same meaning.
- Rewrote the checker section of the `ste-writing` skill. It gives the command, a table of the rule ids, the exemptions, and the Markdown conventions the checker needs.
- Marked PR-1 and PR-2 done in the design doc and the roadmap. PR-1 was still marked planned after its merge.

### State of the build

- `main` is at `94aadc5`. The branch holds one code commit, `3ea5f23`, and this entry.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 61 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. CI, `ste-check`, and `review-gate` run on it from the trunk workflows.

### In flight

PR #10 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- The passive rule is a heuristic: an auxiliary, an optional adverb, and a word that ends in "ed" or is on the irregular list. "is required" and "is closed" are findings. Rewrite with the actor as the subject.
- "have" before a participle is a complex tense, so "the docs have dated records" is a finding. Use "hold".
- The checker reads each line alone. A sentence that wraps to a second line counts as two short ones.
- The reference check skips the exempt paths. A dated record cites old decisions as history, and a rewrite to name the reviser falsifies it.
- `perl -p` reads one line at a time. A multi-line replacement needs `-0`, and an argument that starts with a hyphen needs `--` before it. Both failed silently this session before the fix.
- A `## Procedure` or `## Sequence` heading gives every numbered item below it the 20-word limit, until the next heading at any level.
- To add a technical name that ends in -ing, add it to `NotIngForms` in `SteRules.cs` with a test.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #10 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-10.md`. After the merge, PR-3 starts: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.
