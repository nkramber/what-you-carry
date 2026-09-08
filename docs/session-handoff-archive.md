# Session handoff archive

## Session 32: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #12 repeat review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The repeat review closed P2-1, P2-2, and P2-4, and it kept P2-3 open with two probes. Both reproduce, so the finding has full merit.
- `Type.GetEvents()` gave no finding, because `GetEvents` was absent from the member word list. A Core `probe.GetMethods()` gave a false `L-REFLECTION`, because the word matched.
- No word list can fix both. A Core class that declares `public new string GetType()` compiles, and this session checked that. The list itself was the defect.
- `CoreSourceScan` now compiles the Core sources with `CSharpCompilation` and reads the semantic model. The rules match the type that owns a symbol, the namespace of that type, and the type a method or property gives back. That last rule catches `object.GetType()`, which belongs to `System.Object` and not to `System.Type`.
- `BannedSymbols` holds full type names now. The member word list is gone.
- Two T-2 guards. The compilation checks that `System.Math` and `System.Reflection.Assembly` resolve, because without references every name resolves to nothing and the scan would pass every file. The repository scan reports every compiler error, because a Core file that does not compile resolves no symbol.
- Corrected the F-# citations. The reflection comments cited F-62, the `Atan2` defect. F-64 is the register entry, and it now records both correction passes. D-202 carries a dated note.
- Rewrote the lint tests. Each fragment compiles on its own now, so the tests assert real symbol behavior and not an unresolved name.
- 146 tests pass. The bit-identity hash is unchanged, because this pass changed no Core number.

### State of the build

- `main` is at `86078b8`. The branch holds the two review commits and two correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 146 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- An adversarial Core tree with nine planted uses gave 10 findings, and no finding for a Core `Vector3` in the same file.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- A lint that matches words has two failure modes at once, and each fix makes the other worse. Ask the compiler.
- A semantic scan with no metadata reference resolves nothing and reports nothing. That is a silent pass, so the canary check is not optional.
- `var` resolves to the type the compiler inferred, so it reports the same use a second time. Skip it.
- A test fragment that names an undefined type resolves no symbol, so it gives no finding. A lint test must compile, or it passes for the wrong reason.
- `object.GetType()` belongs to `System.Object`. The owner rule cannot see it, and the rule for the type it gives back can.
- The scan reads the last name of a dotted chain. `System.Math.Sin` holds three names for one call.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 31: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `1bc665b` against base and merge base `86078b8`.
- Confirmed that the provider gate still passes. Claude Code wrote the corrections, and Codex reviewed them.
- Confirmed that P2-1, P2-2, and P2-4 are fixed, with regression coverage.
- Kept P2-3 open. The new name list gives both a false negative and a false positive.
- A probe with `Type.GetEvents()` gave no finding. A user-defined `probe.GetMethods()` call gave `L-REFLECTION`.
- Updated `docs/reviews/pr-12.md` with the changed hash and the repeat-review evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `1bc665b`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 144 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `1bc665b`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs a complete P2-3 correction and another repeat review.

### Traps and gotchas

- Member text does not identify the member symbol. A Core type can declare a method with a reflection-like name.
- A finite reflection member list can miss a supported `System.Type` API.
- The P2-3 code comments cite F-62. F-64 is the register entry for the reflection defect.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3 with complete reflection detection and tests for both probe cases. Then request another repeat review.

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

## Session 20: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Re-reviewed PR #6 at effective head `03e6a29` against base and merge base `546a70a`.
- Confirmed P1-1 is fixed in `9624cfa`. The `pull_request_target` workflow runs the base-branch evaluator and fetches the PR head as data only.
- Confirmed P1-2 remains an accepted risk under D-198. The output names the commit that last changed the review file, and the repository cannot prove provider identity with its shared unsigned commits.
- Found no new finding. Updated `docs/reviews/pr-6.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 35 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI at head `03e6a29`.
- The review-gate check does not run on PR #6 before the workflow reaches `main`, as recorded in D-197 and F-58.

### In flight

The owner can merge PR #6. After the merge, run the adversarial proof against `main` that D-197 requires.

### Traps and gotchas

- The `pull_request_target` workflow is available only after its file reaches the default branch.
- The review-record identity risk remains accepted under D-198.
- The effective head is `03e6a29`, because the latest register changes are outside the metadata paths.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #6. Then run the adversarial proof against `main` and record its result.

## Session 19: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #6 review. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Read the two P1 findings in `docs/reviews/pr-6.md` and assessed each against the evidence.
- P1-1 has full merit. The `pull_request` event ran the tool from the PR head with `checks: write`. The owner chose `pull_request_target` (D-197). The workflow now checks out the base, fetches the PR head as data, and runs the base-branch tool. `ReviewGateRunsOnPullRequestTarget` covers it.
- P1-2 has partial merit. The rewrite of a review file reproduces, and the requested identity check cannot be built, because every commit has one identity and no signature. The owner accepted the risk under D-190 (D-198). The output now names the commit that last changed the review file. Two tests cover it.
- Opened PR #7, a throwaway adversarial PR against the PR-1 branch, to prove the trusted evaluator live. GitHub produced no run. The events reference says `pull_request_target` triggers only when the workflow file exists on the default branch (F-58). Closed PR #7 and deleted the branch.
- Wrote `docs/reviews/pr-6-response.md`, D-197, D-198, F-56 to F-58, and the roadmap corrections.
- After the owner merged PR #6 as `a3b20e2`, opened PR #8, a throwaway adversarial PR against `main`. The trusted tool from `main` answered `neutral` on the adversarial head, and the head's approval never posted. Run 34180347093. Closed PR #8 and deleted the branch. The response file records the proof.

### State of the build

- `main` is at `a3b20e2`, the squash merge of PR #6. The CI workflow and the review-gate workflow are on the trunk.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 35 tests, 0 failures. CI passed on the three platforms at `9624cfa`.
- The `review-gate` check now runs on every PR against `main` from the trunk workflow (D-197). PR #8 proved it.

### In flight

PR #6 is merged. The branch `docs/d-197-proof` records the proof in the response file and this entry. It changes documents only, so the owner adds the `review-override` label (D-190). That label run is the first live use of the override path.

### Traps and gotchas

- `pull_request_target` reads the trigger from the default branch. A workflow on a feature branch never runs for that event, and a PR against that branch gives no error, only silence (F-58).
- A `pull_request_target` run lists the base branch as its head branch in `gh run list`. Do not filter by the PR branch.
- The gate cannot see PR #6 until the merge. The owner merges PR #6 on the review record alone, as for PR #1 to PR #5.
- The identity of a commit proves nothing about the provider (D-198). Read the "Review file last changed by" line in the check output.
- The adversarial proof must never merge. Close the PR and delete the branch after the run.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner adds the `review-override` label to the proof PR and merges it. Then a new session starts PR-2, the STE checker (D-121: one session, one PR).

Entries older than the 10 newest sessions move here from `docs/session-handoff.md` (D-146). Newest first.

## Session 18: 2026-09-08, Codex

Author: Codex
Session: review PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Reviewed PR #6 at head `39db1f9` against base and merge base `546a70a`.
- Confirmed the opposite-provider gate. The PR author is Claude Code, and the reviewer is Codex.
- Found two P1 defects in the review gate. The workflow runs PR-controlled evaluator code with `checks: write`. A PR author can replace an approved review record in a metadata-only commit.
- Wrote `docs/reviews/pr-6.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 32 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI for PR #6. The custom review-gate check is neutral until the review record approves the head.
- The Godot command did not run because `Godot` is not on this checkout's command path.

### In flight

PR #6 needs both P1 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The review-gate workflow checks out the PR head and runs `WhatYouCarry.Tools` from that head.
- The effective-head rule excludes `docs/reviews/`, so it does not detect a review-file replacement.
- PR #6 has GitHub base `546a70a`, while `main` now points to `4ec9708`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P1-1 and P1-2, then request a repeat review of PR #6.

## Session 17: 2026-09-07, Claude Code

Author: Claude Code
Session: PR-1, the repository scaffold. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Started PR-1 from `/Volumes/SSD-1TB/what-you-carry` after the owner merged PR #5. Session 16 named it as the next action, and nothing blocked it.
- Created `WhatYouCarry.slnx` with the four projects (D-108, D-194), `global.json` on SDK 10.0.400 (D-173), and `Directory.Build.props` with nullable on, warnings as errors, and one language version (D-68).
- Put one directory per project at the root. `project.godot` lives in `WhatYouCarry.Game/`, next to its project file, so the Godot editor scans the Game directory and not the whole checkout. The solution stays at the root. This is a layout judgment, and not a decision.
- Wrote the `review-gate` tool as the `review-gate` command of `WhatYouCarry.Tools` (D-65). The rules are one pure function. Git reads are one class. The workflow only gathers the inputs and posts the check run.
- Wrote 32 tests. Every exit test with a name in the roadmap has a test with that name. The git-backed tests build throwaway repositories and run the real `git log` pathspec command.
- Wrote `ci.yml` with one job per platform (D-71, D-100, D-157, D-189) and `review-gate.yml` on the five event types (D-190).
- Wrote the PR template with the gate checklist, the absent checks, and the document lines (D-118, D-148).
- Ran the Godot 4.7.2 editor headless with `--build-solutions` on the Game project. It built, made no solution file, and left the project file unchanged.
- Ran the tool against this checkout with `origin/main` as the base. It found that the base has no mode file, and it gave a failure that names the file. That was OQ-72. The owner chose option (b), and this session put the one-line file on `main` in `4ec9708` on that instruction (D-196). The owner asked first whether a Codex approval plus the override label gives green. It does not: the mode rule runs first, and D-190 keeps code paths out of the override.
- Added the build and test commands to `CLAUDE.md` and `AGENTS.md` (D-122).

### State of the build

- `main` is at `4ec9708`, which holds only the mode file above `546a70a`. The branch `feat/pr-1-scaffold` holds the scaffold commit, the D-196 records, and this entry.
- `dotnet build WhatYouCarry.slnx` and `dotnet test WhatYouCarry.slnx --no-build` pass on the Mac Mini: 32 tests, 0 failures.
- PR #6 is open. The first CI run passed on all three platforms, with 32 tests on each. The `review-gate` workflow posted its check run on the head commit, with the OQ-72 failure.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #6 is open and waits for a Codex review (T-4). After D-196 the `review-gate` check on PR #6 reads `advisory` from `main` and shows grey until the review record lands. The CI workflow passed its first run on this PR on all three platforms.

### Traps and gotchas

- The Godot editor writes `TargetFramework` `net8.0` into a project file that has none, and it keeps a `.csproj.old` copy. Each project file names `net10.0` for that reason. Do not move the target framework into `Directory.Build.props`.
- The Godot SDK knows the configurations `Debug`, `ExportDebug`, and `ExportRelease`. CI builds the default `Debug`. A `--configuration Release` build of the solution is untested.
- `git cat-file -e <rev>:<path>` exits 128 for an absent path, and not 1. The tool uses `git ls-tree`, which prints nothing and exits 0 for an absent path.
- The tool reads the mode file from the base branch, so the base must hold it. D-196 put it there before PR-1 merged. A repository rebuild must keep that file on the trunk.
- One commit went to `main` without a PR on the owner instruction (D-196). That is the exception, not the rule (D-126, D-170).
- A merge of `main` into a PR branch is a commit outside the metadata set, so it moves the effective head and needs a repeat review. A rebase does the same.
- Git marks its object files read-only. The temp-repository helper clears the attribute before delete, or Windows refuses the delete.
- `dotnet run` prints the build output to stdout, so the tool writes its result to a file and never to stdout.
- The workflow uses `jq --slurp` to make one array from the paginated timeline. An empty timeline gives `[]`.
- `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are in the test project as the xUnit stack under D-66. They are the packages that `dotnet test` needs to run xUnit. No separate decision entry exists for them.
- `timeout` does not exist on macOS. `perl -e 'alarm N; exec @ARGV'` does the same job.

### Open questions that block progress

No open question blocks PR-1. OQ-72 is resolved by D-196. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #6 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-<number>.md`. After the merge, PR-2 starts: the STE checker.

## Session 16: 2026-09-07, Claude Code

Author: Claude Code
Session: the toolchain install, the runner registration, and the documentation PR override. Branches `docs/runner-path-and-doc-override` (PR #2, merged) and `docs/runner-registration` (PR #3).

### What this session did, and why

- PR #1 merged as `5c6d7b5`. The next action of session 15 is complete.
- Installed the .NET 10 SDK, version 10.0.400, and the Godot 4.7.2 .NET editor. The runbook names both tools, and this machine had neither.
- The Homebrew cask for the SDK installs a package file that needs an administrator password. This session cannot enter a password, so the Microsoft script `dotnet-install.sh` installed the SDK to `~/.dotnet`.
- Made a test solution with a class library and an xUnit project. `dotnet build` and `dotnet test` were successful, and the default target framework is `net10.0`.
- Found a risk. A launch agent starts with a minimal path, so a CI job on the self-hosted runner cannot find `dotnet`. The owner chose `actions/setup-dotnet` with the `global-json-file` input (D-189).
- The owner gave an override for a documentation-only PR (D-188). This session recorded it in the decision register, `CLAUDE.md`, and `AGENTS.md`.
- Found that D-188 could not work at launch. In enforced mode the `review-gate` job fails a PR with no review record (D-181, D-185). The owner chose the label `review-override` and a wider eligible path set, and D-190 records the mechanism. PR-1 implements it, and the roadmap now holds four more exit tests.
- The SSD came one day early. The owner formatted it as case-sensitive APFS with no encryption, and named the volume `SSD-1TB` (D-191). A probe confirmed the case sensitivity, the write access, and that the volume keeps a file mode.
- Registered the runner on 2026-09-07, one day before the date in D-171 (D-192). The name is `mac-mini-m4` and the version is 2.337.0.
- The launch agent failed at once with `Operation not permitted`, and it exited 126. macOS denies a launch agent every path on an external volume. A launchd probe repeated the denial, and a login shell read the same path correctly (F-54).
- The owner chose the Full Disk Access grant over a move to the internal drive, and added `/bin/bash` and the runner `node` binary (D-193). The runner is online and listens for jobs.
- Ran a smoke job on the runner from a throwaway branch, and then deleted the branch. The job proved five things: the runner accepts a job, the launch agent reads the external volume in a real job, `actions/checkout` works, `actions/setup-dotnet` resolves 10.0.400 from `global.json`, and a build and test cycle passes. The macOS leg of PR-1 is no longer a guess.
- The smoke job found that the .NET 10 SDK creates a `.slnx` file, and the roadmap named `WhatYouCarry.sln` (F-55). A probe proved that Godot 4.7.2 builds from a `.slnx` and creates no `.sln`. The owner chose `.slnx` (D-194).
- Moved the checkout to `/Volumes/SSD-1TB/what-you-carry` (D-195). `git fsck` reported no corruption. This completes the SSD step in the Phase 1 sequence.
- Deleted every merged branch, and pruned the stale remote-tracking refs.

### State of the build

- `main` is at `4ddf2d5`. No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The .NET SDK and the Godot editor are ready on the Mac Mini.
- The runner `mac-mini-m4` is online, and it is not busy. The work directory is `/Volumes/SSD-1TB/actions-work`.
- One smoke job passed on the runner on 2026-09-07. No workflow is in the repository, because the smoke branch is deleted. PR-1 adds the first tracked workflow.

### In flight

PR #2, PR #3, and PR #4 are merged. PR #5 holds the checkout path. This session made four PRs, against the one PR rule of D-121. A direct push to `main` needed a rewind, the smoke job produced a decision, and the checkout move produced another.

This PR changes documentation only. The owner gives the D-188 override and merges it without a cross-provider review. The PR is also eligible under the D-190 path set, so the rule covers its own PR.

### Traps and gotchas

- A launch agent does not read `~/.zshrc`. Do not expect a login shell path on the runner.
- A launch agent reads no external volume without Full Disk Access (F-54, D-193). A machine rebuild repeats the grant, or every job fails.
- Every bash process on the Mac Mini now reads every file. That is the cost of the work directory on the SSD (D-193).
- `actions/setup-dotnet` installs to `~/.dotnet` on this runner, and it reported `already installed` for 10.0.400. No job downloads the SDK again.
- `dotnet new sln` gives a `.slnx` file on .NET 10. A command that names a `.sln` file fails with MSB1009 (F-55).
- The checkout is on the SSD. A session needs the volume mounted, or no file opens (D-195).
- D-190 closes the launch hole in D-188. PR-1 must build the override path, or an overridden PR turns red at launch.
- The `review-override` label does not survive a new commit. A push outside the metadata set after the label needs the label again (D-190).
- The agents hold the owner GitHub token. The label stops an accident, and it does not stop an attack (D-190).
- The `dotnet-sdk` cask needs an administrator password. The Microsoft script needs none.
- This PR holds four concerns, which is against G-10. The session named the conflict, and the owner chose one PR.
- The file held 11 entries before this session. This entry restores the limit of 10 (D-146).
- A commit went to `main` directly, against D-126. The owner merged PR #2 and moved the local checkout to `main`. The uncommitted work moved with the checkout, and the next commit and push landed on the trunk. Check the branch name before each commit.
- The owner chose a rewind. `main` returned to `2d69270`, and the work came back as PR #3. A force push to a trunk is safe only while no other clone holds the old commit.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Start PR-1 from `/Volumes/SSD-1TB/what-you-carry`. The runner, both tools, the CI chain, and the checkout move are complete, so no owner purchase or setup blocks it. PR-1 has 23 exit tests, and the roadmap holds the full scope.

## Session 15: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the P2-3 correction. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Re-reviewed the diff through effective head `c255a17`.
- Confirmed P2-3 is fixed. The F-51 row now names `.github/review-gate-mode` and the base-branch read.
- Confirmed the sweep leaves only the historical D-181 decision row for `REVIEW_GATE_MODE`, with its revision marker.
- Updated `docs/reviews/pr-1.md` to approve the effective head.

### State of the build

- No code, solution, or CI workflow exists. PR-1 creates them.
- The effective-head check resolves to `c255a17` after excluding the D-184 metadata set.
- `AGENTS.md` and `CLAUDE.md` remain byte-identical.
- The review verdict is Ready for owner merge.

### In flight

PR #1 is ready for the owner to merge. The owner must complete the runner and SSD actions before implementation work starts (D-145, D-171).

### Traps and gotchas

- The current mode source is `.github/review-gate-mode`, not `REVIEW_GATE_MODE` (D-185).
- The review record must name the effective head, not the metadata commit (D-184).
- The review and handoff commits must be pushed to the PR branch (D-182, D-183).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #1 after confirming the review-gate and other PR gate conditions. Then run `docs/runbooks/macos-runner.md` on 2026-09-08.

## Session 14: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the P2-3 finding. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The review at head `e51e272` raised P2-3. It has full merit. The F-51 row in the design register still said that `REVIEW_GATE_MODE` selects the mode, and D-185 replaced that variable with the tracked file `.github/review-gate-mode`.
- The defect came from the D-185 citation pass in session 12. That pass added the `D-185` id to every line that cited `D-181` by script, and it did not read the prose beside the id. A mechanical citation edit does not make the sentence true.
- Corrected the F-51 row. It now names the tracked file and the base-branch read.
- Ran the sweep that the finding specifies across every current document. It returned two further hits in `docs/decisions.md`, and neither is a defect. The D-181 row records what D-181 said and carries its revision marker. The D-185 row named the thing it replaced, and its Effect now says "Revises the mode source in D-181", which D-186 requires.
- Used the D-187 procedure for the first time. A fetch showed session 13 on the remote, so this entry is session 14. No collision.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds the review commits and this session's commit.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review. P2-3 is corrected in documents only, because no workflow exists yet.

### Traps and gotchas

- A mechanical citation pass must read the sentence that holds the citation. The script that added `D-185` beside `D-181` left one sentence false, and the review caught it.
- The sweep for a removed name belongs with the citation pass, not after the next review.
- The D-187 procedure works. Fetch, read the highest number, then add one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff through `e51e272` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 13: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the D-184 and D-185 corrections. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Re-reviewed the diff through effective head `e51e272`.
- Confirmed P1-3 is fixed: the tracked `.github/review-gate-mode` file supplies `advisory`, and the workflow reads it from the base branch (D-185).
- Confirmed P1-4 is fixed: the review, handoff, and archive paths are metadata, so the required review commit does not change the effective head (D-184).
- Added P2-3: the F-51 row in `docs/design.md` still names the removed `REVIEW_GATE_MODE` variable as the mode source.

### State of the build

- No code, solution, or CI workflow exists. PR-1 creates them.
- The effective-head check resolves to `e51e272` after excluding the D-184 metadata set.
- `AGENTS.md` and `CLAUDE.md` remain byte-identical.
- The review verdict is Changes required.

### In flight

PR #1 needs the F-51 mode-source text corrected in `docs/design.md`.

### Traps and gotchas

- The current mode source is `.github/review-gate-mode`, not `REVIEW_GATE_MODE` (D-185).
- The review record must name the effective head, not the metadata commit (D-184).
- The review and handoff commits must be pushed to the PR branch (D-182, D-183).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct the F-51 row, then request another repeat review of PR #1.

## Session 12: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the P1-3 and P1-4 findings, and add the push-back rule to the review skill. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The review at head `6e45d6f` raised P1-3 and P1-4. Both have full merit. Recorded D-184 and D-185 and wrote the dispositions in `docs/reviews/pr-1-response.md`.
- P1-4: D-182 requires the reviewer to commit the review record with the handoff entry, and the effective head excluded only `docs/reviews/`. The review commit therefore became the effective head and rejected the record it published. Reproduced it. The old rule returned `eab18db` while the record named `6e45d6f`.
- D-184 defines the metadata set: `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. With that set excluded the effective head resolves to `8efb267`, the last substantive commit. The archive is in the set because a handoff rollover writes it in the same commit.
- P1-3: a workflow cannot create a repository variable, so PR-1 could not pass the check it creates, against G-19. D-185 moves the mode to the tracked file `.github/review-gate-mode`. PR-1 creates it with `advisory`. The workflow reads the file from the base branch, so a PR cannot change the mode that judges it.
- The owner asked that a request to address review findings load the `pr-review` skill, and that the skill state that a finding is a claim, not a fact. Added an "Address review findings" section with a seven-step procedure and a push-back table. Widened the skill description so the request triggers it.
- D-185 revises D-181, which made fifteen citations stale. The D-178 check found each one. They now name D-185.
- The owner then asked how to stop the two recurring problems. Recorded D-186 and D-187, and F-52 and F-53.
- D-186 splits the revision marker. `Superseded by D-N` replaces the whole answer, and every citation must name D-N. `Revised in part by D-N` changes one named part, and the decision stays citable. The Effect column must name the part that changed and the parts that stand.
- Reclassified the seven revisions. D-94, D-158, D-169, and D-172 are superseded. D-136, D-179, and D-181 are revised in part. The register already used `Superseded by` for D-10 and D-95, so the verb is not new.
- The D-178 check now keys on `Superseded by` only. Partial revisions carried 33 of the 48 citations, and they produced all three rounds of churn.
- D-187 fixes the session number. Fetch the remote and read the handoff again immediately before the handoff commit, then take the highest number and add one. Every session pushes (D-183), so a push serializes the writers. The PR-2 checker fails on a duplicate number.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds the review commits and this session's commit.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review. P1-3 and P1-4 are corrected in documents only, because no workflow exists yet.

### Traps and gotchas

- The effective head excludes three paths now, not one (D-184). A review commit that touches only those paths is metadata.
- The mode file is read from the base branch, never the PR head (D-185). A PR that edits the mode file does not change its own mode.
- A finding is a claim, not a fact. Assess it, and refute it with evidence when it does not hold.
- A partial revision still marks the whole decision as revised, so every citation must name the reviser. D-181 needed fifteen edits.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff since `6e45d6f` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 11: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the neutral review-gate change.

### What this session did, and why

- Rechecked PR #1 at effective head `6e45d6f`, including the D-181, D-182, and D-183 changes.
- Confirmed the prior four findings remain fixed.
- Added P1-3 to `docs/reviews/pr-1.md`: PR-1 does not define how `REVIEW_GATE_MODE=advisory` is created.
- Added P1-4: the required review commit changes the handoff path and invalidates its own effective head.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- The review verdict is Changes required.

### In flight

PR #1 needs an owner setup step or bootstrap mechanism for the required review-gate repository variable, and a metadata-path correction for the effective-head rule.

### Traps and gotchas

- D-181 forbids a default for an absent or unknown `REVIEW_GATE_MODE`.
- The PR-1 roadmap says it sets the variable, but the scope contains no repository-state action.
- D-182 and D-183 require the review record and handoff in one commit, but D-179 excludes only `docs/reviews/`.
- The effective head at review start is `6e45d6f`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Define the advisory-mode variable setup, correct the metadata-path rule, revise PR #1, and request another repeat review.

## Session 10: 2026-09-07, Claude Code

Author: Claude Code
Session: the neutral grey state for `review-gate`. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The owner asked where the red check appears, then asked for a neutral grey state instead of a permanent red one. `review-gate` failed whenever no review record existed, so a PR stayed red for most of its life and a red rollup masked a real build failure.
- Verified first that GitHub counts a neutral or skipped conclusion as a success for a required status check. Source: docs.github.com, "About status checks", verified 2026-09-07. A plain grey-when-absent gate would stop blocking a merge at launch. Recorded F-51.
- D-181 gives the check three conclusions and two modes. In `advisory` mode a missing review file is neutral. In `enforced` mode it is a failure. The repository variable `REVIEW_GATE_MODE` selects the mode. An absent or unknown value fails the job and names the variable (T-2).
- A workflow job cannot set a neutral conclusion by its exit code. The job publishes a check run through the Checks API, so the workflow needs `checks: write`.
- D-181 revises D-179. D-180 is not revised, because D-181 only adds the mode step to its launch procedure.
- Marking D-179 as revised made nine citations stale. The D-178 check found each one. They now name D-181.
- Updated the PR-1 scope and exit tests to sixteen, both agent files, the `pr-review` skill with a color table, Phase 5 step 11, and the design register.
- Recorded D-182 after the handoff and review record of sessions 8 and 9 reached this session uncommitted. A later commit absorbed them, and this session then reported the wrong verdict. The `pr-review` skill now requires a commit of the review record with its handoff entry, and the agent files carry the same rule.
- D-182 narrows the scope limit in the skill, which listed a commit as an unauthorized action. A code fix, a merge, and an external message stay unauthorized.
- D-183 lets the reviewer push its own review commit to the PR branch. A push is the only way `review-gate` reads the record, because the gate reads the PR head. The reviewer never pushes to `main`.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds fifteen commits, all on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

**PR #1 is not ready to merge.** Session 9 approved head `223aae8`. This session pushed `4f7796e` after that approval, and it changes eight files outside `docs/reviews/`, including both agent files, the decision register, and the review skill. The approval does not cover the effective head. Rule 3 of D-179 applies.

### Traps and gotchas

- The verdict in `docs/reviews/pr-1.md` reads `Ready for owner merge`, and it applies to head `223aae8` only. Read the head field, not the verdict alone.
- Grey is correct only in advisory mode. Never use a neutral conclusion for an enforced gate (F-51).
- The `review-gate` job stays green itself. The check run it publishes carries the color, so the Checks list holds two rows.
- Never write a decision range that spans a revised id. D-179 is revised, so a header says `D-176 to D-178, D-180, and D-181`.
- Read the review file before a commit that sweeps it in. This session committed an approval it had not read, and then reported the wrong verdict.
- One session is one handoff entry (D-146). Add a new entry. Do not append to an older one after another provider writes above it.
- A review record that is not committed is invisible to `review-gate`, because the gate reads the PR head (D-182).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff from `223aae8` to `4f7796e` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 9: 2026-09-07, Codex

Author: Codex
Session: final repeat review of PR #1 on `docs/roadmaps`.

### What this session did, and why

- Rechecked PR #1 at effective head `223aae8`.
- Confirmed the P2-2 fix and reviewed the effective-head command clarification.
- Updated `docs/reviews/pr-1.md` with a Ready for owner merge verdict.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- All four prior findings are fixed. No new finding remains.

### In flight

PR #1 is ready for owner merge. Build and CI checks remain deferred because this PR defines the solution and workflows that PR-1 creates.

### Traps and gotchas

- The reviewed effective head is `223aae8`.
- The review file is machine-read. Keep its head field and verdict exact.
- The owner must still register the runner and move the checkout to the SSD before PR-1.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner can merge PR #1. Then complete the SSD and runner actions before starting PR-1.

## Session 8: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 on `docs/roadmaps`.

### What this session did, and why

- Rechecked PR #1 at effective head `60087b0` against the prior review and the author response.
- Confirmed fixes for P1-1, P1-2, and P2-1.
- Added P2-2 to `docs/reviews/pr-1.md` because the Phase 1 header omits PR-58, D-177, and D-178.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- The review verdict remains Changes required.

### In flight

PR #1 needs a small roadmap header correction. The review record now names head `60087b0`.

### Traps and gotchas

- The effective head is the newest commit outside `docs/reviews/`.
- The old findings stay in the review record with fixed dispositions.
- The Phase 1 sequence includes PR-58, but the roadmap header still states PR-1 to PR-11 only.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct the Phase 1 roadmap header and request a final repeat review against the new effective head.

## Session 7: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the PR #1 review. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Read `docs/reviews/pr-1.md` and checked each of the three findings against the branch, the registers, and the PR.
- P1-2 and P2-1 have full merit. P1-1 has partial merit. The owner approved every disposition. Recorded D-176 to D-178 and F-47 to F-49. Wrote `docs/reviews/pr-1-response.md`.
- P1-1: T-6 and D-137 prohibit text that names an agent, harness, or model as the source of the work. A tool name that identifies a configured file is not attribution, so the broad reading in the finding would also condemn D-172, D-175, OQ-16, F-15, the PR description, and the settings file path. D-176 states the boundary. The body of the head commit is rewritten, because one clause implied that an agent wrote the commits. PR-1 exit test 6 now scans every subject and body, not only trailers.
- P1-2: PR-11 created the night job and the `night-gate` job together, so the gate had no result to read on its first run, against G-19. D-177 splits them. PR-11 publishes a result record. The new PR-58 adds the gate after one night runs. An absent, stale, cancelled, or failed record fails the gate.
- P2-1: fixed all six stale references. The review named five. A sweep found a sixth at `phase-1-foundations.md:412`. D-178 adds a reference check to the PR-2 checker, so the next revision cannot leak.
- The owner asked for a GitHub merge criterion that blocks a merge without the review files. GitHub returns 403 for branch protection and for rulesets on a private free repository, verified this session. No hard block is possible today.
- Recorded D-179 and D-180 and F-50. PR-1 gains a `review-gate` job. It reads `docs/reviews/pr-<number>.md`, requires the verdict `Ready for owner merge`, and requires the recorded head to be the effective head. The job is advisory until launch. Phase 5 step 11 makes it a required check after the repository becomes public.
- The owner asked for the head to match the PR head. The `pr-review` skill says the opposite: do not require the review file to hold its own hash. The effective head reconciles both. The effective head is the newest commit outside `docs/reviews/`.
- The owner chose not to require the response file. The gate reads the review file only.
- Codex re-reviewed at head `60087b0`. P1-1, P1-2, and P2-1 are marked fixed. One new finding, P2-2, is open: the Phase 1 roadmap header omits PR-58 and the new decisions, and it still says `Correction passes: none yet`.
- P2-2 has full merit. Fixed line 3 and line 9 of the Phase 1 roadmap. Gave `phase-5-early-access.md` the same treatment, because this PR added its sequence step 11. The review did not name that file.
- Ran the regression check that P2-2 specifies. It found three more defects of the same class. The new phase-1 range `D-156 to D-168` swallowed the revised D-158, `phase-2-first-playable.md` had the same defect in `D-157 to D-168`, and phase-1 line 465 cited D-158 with no revision marker. All three are fixed.
- A wider sweep found two older ones: `docs/design.md:28` cited D-136 alone, and `phase-3-full-loop.md:372` cited D-94 alone. Both now name the revising decision.
- Refined D-178. A line passes the reference check when it holds a revision word or when it names the revising decision. Without that clause the check fails on F-34, F-46, and four correct `D-94, D-152` pairs.
- Rewrote `.claude/skills/pr-review/SKILL.md` for the format. It now holds a review file skeleton, the three machine-read fields, a finding format with stable `P<severity>-<n>` ids, the attribution boundary of D-176, the gate rules, a ten-step repeat review procedure, and the response file contract.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds thirteen commits. The head commit of session 5 was amended, and the branch was force pushed.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review by Codex against the new head.

### Traps and gotchas

- The head commit was amended. The review file `docs/reviews/pr-1.md` names head `9459534`, which no longer exists.
- D-176 fixes the attribution reading. Do not strip a tool name that identifies a configured file, a schema, or a version. Strip a claim about the source of the work.
- The owner squash-merges (D-126). GitHub fills the squash body with every commit message. Check that body before the merge.
- PR-58 is new. Phase 1 now ends with PR-11, one scheduled night, PR-58, then the measurements and Gate 1.
- Ids never change. PR-58 sits after PR-11 in the sequence, not after PR-57.
- The PR-2 reference check skips a line that holds `revises`, `revised by`, or `supersedes`. The two F-15 history lines were reworded to hold that word.
- Branch protection and rulesets both return 403 on this repository. Do not plan a hard merge block before launch (D-180).
- `review-gate` reads three exact things: the file name, the `- Head: ` line, and the verdict name. A reworded verdict fails the job.
- The effective head ignores a commit that changes only `docs/reviews/`. A review file commit does not invalidate its own approval.
- `docs/reviews/pr-1.md` now records head `60087b0` and holds four findings. P2-2 is the open one, and this session fixed it.
- A decision range in a header can swallow a revised decision. Write `D-156, D-157, D-159 to D-168`, not `D-156 to D-168`, when D-158 is revised.
- The roadmap header is a scope summary. Update line 3 and line 9 whenever a PR entry, a measurement, or a governing decision changes.
- Never write a decision range that spans a revised id. D-179 is revised, so a header says `D-176 to D-178, D-180, and D-181`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #1 a third time against the new head, per `.claude/skills/pr-review/SKILL.md`. P2-2 is the only finding to confirm. Sessions 8 and 9 did that work.

## Session 6: 2026-09-07, Codex

Author: Codex
Session: review PR #1 on `docs/roadmaps`.

### What this session did, and why

- Read the handoff, agent rules, review skill, STE skill, design, decisions, questions, existing reviews, and focused roadmaps.
- Verified PR #1 at base `1c16c45` and head `9459534`.
- Wrote `docs/reviews/pr-1.md` with three findings and a Changes required verdict.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff passes `git diff --check`.
- The agent files remain byte-identical. The settings file parses as JSON.

### In flight

PR #1 needs a revision. The review identifies prohibited attribution in a commit body, an undefined first-run path for `night-gate`, and stale references to D-172 and the unresolved OQ-2 state.

### Traps and gotchas

- T-6 applies to commit bodies as well as commit subjects.
- D-175 supersedes D-172. D-173 supersedes D-169.
- PR-11 creates the night result and the gate. The empty-result case needs an explicit contract.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Revise PR #1, then run a repeat review against the new head. Recheck the commit history, the first night-gate run, and every current-status reference to the revised decisions.

## Session 5: 2026-09-07, Claude Code

Author: Claude Code
Session: fix the attribution setting, no new PR. Branch `docs/roadmaps`, pushed, part of PR #1.

### What this session did, and why

- Found why the Claude Code startup dialog reported that `.claude/settings.json` failed to parse. The file is valid JSON, but `attribution.commit` and `attribution.pr` were booleans. The schema requires strings. When one value fails validation, the harness ignores the whole file, so the co-author trailer stayed on, against T-6.
- Set both fields to the empty string, which hides the attribution (D-175). Verified against the settings reference, the schema in the VS Code extension 2.1.263, and the validator in the CLI 2.1.261.
- Marked D-172 as revised. Updated OQ-16 and F-15. Pushed one commit to `docs/roadmaps`, so PR #1 carries the fix.

### State of the build

- `main` has one commit, `1c16c45`, on the remote. The branch `docs/roadmaps` holds the six commits of session 4 and one commit of this session, all on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.

### In flight

PR #1 from `docs/roadmaps` to `main` is open and waits for the other provider's review (T-4). The attribution fix is part of it.

### Traps and gotchas

- `attribution.commit` and `attribution.pr` are strings. A boolean makes the harness ignore the whole settings file, and the startup dialog calls it a parse failure.
- A settings file that fails validation loses every setting in it, not only the bad field. Run `/doctor` to see what the harness dropped.
- The traps in the session 4 entry still apply.

### Open questions that block progress

No new question. The session 4 entry lists the open ones.

### Next concrete action

Unchanged from session 4. A Codex session reviews PR #1 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-1.md`. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.
## Session 4: 2026-09-07, Claude Code

Author: Claude Code
Session: initial commit, the five focused roadmaps, and PR #1. Branch `docs/roadmaps`, pushed.

### What this session did, and why

- Reset HEAD to `main`, added `.gitignore` and `.gitattributes`, and pushed every file to `origin/main` as commit `1c16c45`, message "initial commit" (D-156). The owner asked for a direct push to `main`.
- Created the branch `docs/roadmaps` from `main` for the focused roadmaps (D-156).
- Wrote `docs/roadmaps/phase-1-foundations.md`. Each PR entry has scope, out of scope, exit tests, review focus, a check clause, a gate, and a plain-English paragraph. It adds M-1 and M-2 procedures and a Phase 1 sequence.
- Found two gaps of the audit R-2 class and handled them under D-149. PR-10's gate named a weapon roster that does not exist until Phase 3 (F-38). A test-only definitions file fixes it. The macOS CI leg needs a self-hosted runner that nobody had listed (F-39, OQ-31).
- Added G-21 to the design guardrails: no `System.Random` or wall-clock reads in Core.
- Filed OQ-31 to OQ-42 in `docs/questions.md`: the owner actions and the technical choices that Phase 1 PRs need before they start, each with a recommendation.
- Linked the roadmap from `docs/design.md` section 7.
- Asked the owner OQ-31 to OQ-42 in three batches and recorded D-157 to D-168. The Phase 1 roadmap now cites those decisions instead of the questions.
- Wrote the four later roadmaps in the same format: `phase-2-first-playable.md`, `phase-3-full-loop.md`, `phase-4-content-complete.md`, and `phase-5-early-access.md`. Each has per-PR scope, exit tests, review focus, a check clause, a gate, and a sequence with the owner questions placed before the PR that needs them.
- Filed OQ-43 to OQ-71 for those phases, each with a recommendation. Added F-40 to F-45 to the design register for gaps the roadmaps exposed: wall fade in the mesher, no Deck unit named, no rarity tiers named, no Tier 3 model or budget, no source for the Deck checklist, and no cloud save file set after D-152.
- Pushed every commit on `docs/roadmaps` to the remote after the owner pushed the branch.
- Closed OQ-2 (D-169, .NET 8 LTS, revised the same day to .NET 10 LTS as D-173), OQ-16 (D-172, `.claude/settings.json` with attribution off), and the runner timing (D-171, tomorrow on the SSD). Wrote `docs/runbooks/macos-runner.md`.
- Found that branch protection needs GitHub Pro or a public repository. The owner deferred it until launch (D-170, F-46).
- Renamed the GitHub repository to `nkramber/what-you-carry` and moved the checkout to `/Users/nate/Repos/what-you-carry` (D-174). The runbook cites the new name.

### State of the build

- `main` has one commit, `1c16c45`, on the remote. The branch `docs/roadmaps` holds six commits of this session and is on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The STE checker does not exist until PR-2. This session scanned the changed documents by script.

### In flight

All five roadmaps are on the branch. PR #1 from `docs/roadmaps` to `main` is open and waits for the other provider's review (T-4).

### Traps and gotchas

- The roadmap never restates a decision. It cites D-# ids. Read the `Effect` column before you cite an early decision.
- PR-1 cannot merge until the runner exists (D-157, D-171) and the SSD holds the checkout (D-145). OQ-2 and OQ-16 are closed.
- Nothing on GitHub stops a push to `main` (D-170). The rule in the agent files is the only guard. Never push to `main`.
- The .NET pin is .NET 10 LTS (D-173). D-169 stays in the register as revised.
- The checkout path changed on 2026-09-07 (D-174). Open `/Users/nate/Repos/what-you-carry` in the editor. The session memory for the old path was copied to the new path.
- Each Phase 1 PR has owner questions listed before it in the roadmap sequence. Ask them before the PR starts, not inside it (D-124).
- `Sqrt` in DetMath wraps the IEEE square root. The lint tool must allow `MathF` inside `DetMath.cs` only.
- The night-gate job in PR-11 reads the latest scheduled run. A red night blocks the next merge by design (D-115).

### Open questions that block progress

`docs/questions.md` holds OQ-1 to OQ-71. One stays open for Phase 1: OQ-12 blocks PR-9. One owner action precedes PR-1: the runner registration on 2026-09-08 per the runbook (D-171). The SSD arrives the same day (D-145). Each later roadmap lists its own open questions in its section 6, with the owner's answer placed in the sequence before the PR that needs it.

### Next concrete action

A Codex session reviews PR #1 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-1.md`. The author of this branch is Claude Code, so Claude Code cannot review it. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md` with the volume name. After the owner merges PR #1, PR-1 starts from `phase-1-foundations.md`.

## Session 3: 2026-09-07, Claude Code

Author: Claude Code
Session: audit remediation, no PR. The repository has no commits.

### What this session did, and why

- Assessed the nine findings of `docs/reviews/2026-09-07-repository-audit.md`. All nine have merit. Asked the owner seven questions and recorded D-148 to D-154.
- Wrote `docs/reviews/2026-09-07-repository-audit-response.md` with each disposition.
- Edited `docs/design.md`: header correction (R-8), F-28 to F-37, G-19 and G-20, roadmap entries PR-1, PR-2, PR-3, PR-6, PR-7, PR-9, PR-11, PR-12, PR-16, PR-17, PR-18, PR-22, PR-27, PR-28, PR-30, PR-31, PR-32, PR-49, M-5, a new PR-57, and section 8 rewritten.
- Created `docs/questions.md` as the open questions register (D-144). Section 9 of the design doc now links to it. Added OQ-30.
- Recorded D-144 to D-147 from the owner's message: the questions file, the SSD order, the ten-session handoff rule, and audit-first order. A Codex session added D-155 in parallel.
- Converted this file to the ten-session format (D-146) and created `docs/session-handoff-archive.md`.
- Updated `CLAUDE.md`, `AGENTS.md`, and both skills for the questions file, the handoff rule, and the gate clause.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists. No commits.
- HEAD points at the unborn branch `docs/repository-audit`. The first commit lands there unless the owner resets HEAD (OQ-30, F-37).
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The STE checker does not exist until PR-2. This session scanned the changed documents by script for semicolons, contractions, verb -ing forms, sentences over 25 words, and passive markers.

### In flight

Nothing is half done. Every audit finding has a decision and a document change.

### Traps and gotchas

- D-152 revises D-94: one profile file plus one run record, not three files. Cite D-152.
- D-150 revises D-136: Gate 1 is a foundation gate with no playtest.
- D-149 moves PR-32 before PR-27 and inserts PR-57 after PR-13. Section 8 is the order. Ids never change.
- The design header keeps the refuted Godot 4.7.1 text with a dated note. Do not delete it.
- The handoff now keeps 10 sessions. Add an entry at the top. Do not rewrite the file.
- The attribution rule stands (T-6). OQ-16 remains open.

### Open questions that block progress

`docs/questions.md` holds OQ-1 to OQ-30. PR-1 depends on OQ-2, OQ-16, OQ-30, and the SSD (D-145, arrives 2026-09-08).

### Next concrete action

The owner said the focused roadmaps begin once the audit findings are addressed (D-147). They are addressed. The next action is `docs/roadmaps/phase-1-foundations.md`: expand PR-1 to PR-11 and M-1 to M-2 with per-PR exit tests, under D-148 and D-149. Load `ste-writing` and `design-doc-style` first. Confirm with the owner that Phase 1 is the first roadmap.
## Session 2: 2026-09-07, Codex

Author: Codex
Session: repository audit, no PR. The local repository has no commits.

### What this session did, and why

- Examined the design, all 143 decisions, the archive, both agent files, and both local skills.
- Wrote `docs/reviews/2026-09-07-repository-audit.md` with nine findings, evidence, recommendations, and verification limits.
- Added OQ-25 to OQ-29 in `docs/design.md` section 9. These cover gates, replay state, save transactions, empty-bank recovery, and economy tests.
- Created the local branch `docs/repository-audit` under D-126. All project files remain untracked.
- Recorded the project skill location as D-155. Both agent files now require `.claude/skills/` for current and new project skills.
- Added direct access instructions for a required skill that is absent from the skill list.
- Added no code. No commit, PR, or merge occurred.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists.
- No build or test ran. The repository has nothing executable to check.
- `AGENTS.md` and `CLAUDE.md` are byte-identical. The comparison returned success (D-122).
- The STE checker does not exist until PR-2. The new text received a manual checklist review.
- The previous design interview produced `docs/design.md` v2 and decisions D-1 to D-143.
- The unchanged v1 archive remains at `docs/archive/design-v1-2026-09-06.md`.
- A Git remote URL exists. This session did not verify remote history or backups.

### In flight

The audit and skill location update are complete. New decisions D-144 to D-154 address several audit findings.
The design and agent rules still need the corresponding audit corrections. D-147 requires those corrections before focused roadmaps.

The audit identified four highest-priority findings:

1. The initial PRs require merge checks that PR-2 and PR-3 create later.
2. Several feature gates precede their test tools or game systems.
3. The replay contract omits the initial loadout, skill state, and simulation and content identities.
4. The save contract lacks a shared commit and recovery rule across the three files.

### Traps and gotchas

- The review file records design gaps, not observed runtime defects. No game code exists.
- The five-second rewind in D-97 is an accepted tradeoff, F-17. This audit does not reopen it.
- The Godot 4.7.2 release exists. The design header incorrectly separates stable 4.7.1 from .NET 4.7.2.
- Official links and the verification date appear in the audit. OQ-2 remains open for the .NET version.
- The agent merge gate and the roadmap disagree about the initial checks. OQ-25 records the conflict.
- Section 8 puts palette approval before PR-1. OQ-1 says it blocks PR-14. The owner must settle that scope.
- Keep the attribution rule in every future commit and PR (D-137). OQ-16 remains open.
- The owner owns every open question (D-124). Ask before a choice, and record each answer (D-138).
- Project skills live in `.claude/skills/`. Read their `SKILL.md` files directly when required (D-155).
- Decisions after D-143 revise the audit state. Consult the current decision register before an audit correction.
- D-97 supersedes D-10 and D-95. D-129 and D-132 revise D-120. D-141 keeps decisions in one file until about 300 rows.
- Review ids stay local to the review file. They do not enter the design finding register.

### Open questions that block progress

`docs/design.md` section 9 still contains the audit questions. D-144 moves the register to `docs/questions.md`.
D-148 to D-154 answer several audit issues. Reconcile the register with those decisions before the next owner question.

PR-1 still depends on OQ-2, OQ-16, and the SSD prerequisite in OQ-18. D-145 records the SSD order.
The palette prerequisite has conflicting scopes, as noted above.
D-151 to D-154 resolve OQ-26 to OQ-29. The affected roadmap entries need those contracts before implementation.

### Next concrete action

Continue the audit corrections under D-147. Apply the current decisions before new owner questions.
Keep the two agent files identical (D-122). Check the current register before each new decision id.

The prior sequence remains design v2, then focused roadmaps, then code.
The next planned artifact is `docs/roadmaps/phase-1-foundations.md`.
It needs per-PR exit tests for PR-1 to PR-11 and measurements M-1 to M-2.
Load the local `ste-writing` and `design-doc-style` skills before that work.

## Session 1: 2026-09-07, Claude Code

Author: Claude Code
Session: design interview, no PR. The repository had no commits.

### What this session did, and why

- Read the v1 design document and asked the owner 143 questions in batches. The owner wanted every assumption confirmed before any code.
- Recorded every answer in `docs/decisions.md` as D-1 to D-143.
- Wrote `docs/design.md` v2 from those decisions, in the design-doc-style template and in ASD-STE100.
- Moved the v1 document to `docs/archive/design-v1-2026-09-06.md` unchanged.
- Created `.claude/skills/ste-writing/SKILL.md` and `.claude/skills/design-doc-style/SKILL.md`, adapted to this project (D-131).
- Created `CLAUDE.md` and the identical `AGENTS.md` (D-122).
- Created the empty folders `docs/reviews/` and `docs/roadmaps/`.

### State of the build

- No code. No solution. No commits. The working tree held only documents, skills, and agent files.
- The STE checker did not exist. The session checked the documents by script.

### Traps and gotchas

- The harness default adds a co-author trailer to commits. T-6 forbids it (OQ-16).
- Every open question belongs to the owner (D-124).
- D-97 supersedes D-10 and D-95. D-129 and D-132 supersede the file layout in D-120.
- The owner named the game "What You Carry" (D-11). The v1 name "Descent" appears only in the archive.
- The design doc uses `M-#` for measurements. The five milestones are the roadmap phases, not `M-#` ids.

### Next concrete action at the time

A focused roadmap for Phase 1. Superseded by session 2, the audit.
