# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 34: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the third PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The third review kept P2-3 open and added P2-5 and P2-6. All three reproduce, so all three have full merit.
- P2-3, third pass. The symbol read was correct, and the table was short. `System.Enum.IsDefined` gave no finding, and session 28 had already named `Enum.IsDefined` as reflection when it removed the call from `Rng.ForStream`. The tool contradicted the code it guards.
- The table gains `System.Enum`, `System.Attribute`, `System.AppDomain`, `System.Delegate`, `System.MulticastDelegate`, the three runtime handle types, and `RuntimeHelpers`. The scan also reports the `typeof` keyword, because no symbol carries that name and a type value can reach a place where no name is banned.
- An ordinary enum stays legal. The ban reads `System.Enum`, never the Core enum that declares the values, and `AnOrdinaryEnumIsNotAFinding` holds that.
- P2-5. A `System.Math.Sin` call inside `#if NET10_0` compiles in the Core build and gave no finding. The owner chose the ban on conditional compilation over the build symbols (D-204, OQ-77, F-65). The other option cannot be complete, because Debug and Release are both real builds and a lint parses one.
- `det-lint` reports each `#if` as `L-CONDITIONAL`. A `#nullable`, `#region`, or `#pragma` directive stays legal. Core held no conditional directive, so no Core file changed.
- P2-6. The PR description named the superseded word list, 144 tests, and an old head. It now holds a table of all six findings, the symbol read, and the current evidence.
- 160 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds three review commits and three correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 160 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- A symbol table is a list, and a list is never complete on its own. Check the table against the code the project already rejected. Session 28 removed `Enum.IsDefined`, and the tool did not know it.
- A ban on a platform type is not a ban on the language feature. `System.Enum` owns the metadata methods, and a Core enum owns its values. Test the ordinary use.
- `#if` is trivia. A node walk does not reach it, and `DescendantNodes(descendIntoTrivia: true)` does.
- The lint parse defines no preprocessor symbol, so an active branch reads as empty. That is why D-204 bans the directive and does not read the branch.
- The PR description is part of the record (D-118). Update it with each correction, not at the end.

### Open questions that block progress

No new owner question. OQ-77 is resolved by D-204. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 33: 2026-09-08, Codex

Author: Codex
Session: second repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `5316033` against base and merge base `86078b8`.
- Confirmed that the provider gate still passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that the semantic scan fixes the two prior P2-3 probes and the F-# citations.
- Kept P2-3 open. `Enum.IsDefined` compiles in Core and gives no reflection finding, against G-2 and the Session 28 record.
- Added P2-5. A `System.Math.Sin` call under active `#if NET10_0` code builds, but the lint reports no finding.
- Added P2-6. The PR description still reports the removed member word list, 144 tests, and the old effective head.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `5316033`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 146 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `5316033`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-3, P2-5, and P2-6, then another repeat review.

### Traps and gotchas

- Reflection APIs also exist outside `System.Type`, `System.Activator`, and the `System.Reflection` namespace.
- A semantic scan only reads the branch that its parse symbols select.
- The Core build defines target-framework symbols that the lint compilation does not define.
- The PR description is part of the evidence record. Update it after a correction changes the implementation.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3 and P2-5, then update the PR description for P2-6. Request another repeat review.

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
