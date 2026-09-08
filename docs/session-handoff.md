# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 40: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the sixth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The sixth review closed P2-7 and P2-8, and it kept P2-6 open and opened P2-9. Both reproduce, so both have full merit.
- P2-9. `EqualityComparer<string>.Default.GetHashCode(v)` gave no finding. The member belongs to `EqualityComparer`, and D-205 approved `System.Collections.Generic` as a whole namespace, which D-207 supersedes. The reviewer ran three processes and got three hashes for one string.
- The cause is the same shape as P2-7, one level out. Each approved namespace holds a machine-dependent type beside the one Core needs.
- A run with the four namespaces removed named one type: `CultureInfo`. The owner chose the one allowlist with that number in hand (D-207, OQ-80, F-68).
- D-207 supersedes D-205 and D-206. Core approves every external type by full name, and no namespace passes as a whole. The list holds ten entries, and the tool is smaller: one list serves the type rule and the import rule.
- `CultureInfo` needs both rules. Core formats with `InvariantCulture`, and the same type holds `CurrentCulture`. The member denylist holds the five culture members that read the user, the process, or the machine.
- `System.Array` and the collection types are absent, because Core uses neither today. PR-7 will add `System.Array` with its decision.
- P2-6, third pass. The description listed four owner decisions while the same page named seven. It lists every decision now, with the finding that produced it, and it names no session number.
- 187 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds six review commits and six correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 187 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A whole-namespace approval fails at every level. The namespace list leaked `Guid`, and then the `System` type list leaked `EqualityComparer` through the other namespaces. Approve each type by name.
- A type allowlist does not remove the member denylist. `CultureInfo` holds `InvariantCulture` and `CurrentCulture`, and a type with both kinds of member needs both rules.
- A superseded decision needs the superseding id on every line that cites it (D-178). The reference check found four such lines in one commit, and the checker is the only reason they did not ship.
- A large edit by text slice can delete a neighbor. This session removed five helpers with one block replacement, and the build named each one.
- Measure the friction before the question. Seven types settled D-206, and one type settled D-207.

### Open questions that block progress

No new owner question. OQ-80 is resolved by D-207. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 39: 2026-09-08, Codex

Author: Codex
Session: fifth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `9ad2a4c` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-7 is fixed. `Guid.NewGuid()` now reports `L-RANDOM`.
- Confirmed that P2-8 is fixed. An unused `using System.Text;` now reports `L-NAMESPACE`.
- Kept P2-6 open. The PR description says that four owner decisions were needed and lists D-200 to D-203, but the same description identifies D-200 to D-206 as the decisions for the PR.
- Added P2-9. `EqualityComparer<string>.Default.GetHashCode(value)` compiles in Core and gives no lint finding. Three processes gave three values for the same string.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `9ad2a4c`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 183 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `9ad2a4c`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-6 and P2-9, then another repeat review.

### Traps and gotchas

- A member ban reads the type that declares the member. `EqualityComparer<string>.GetHashCode` reaches the string hash, but the member belongs to the comparer and not to `String`.
- A whole-namespace approval can admit process state through a type that the decision never names.
- Run a process-randomized hash probe in separate processes. Repeated calls inside one process use one seed.
- A session number in the PR description becomes stale when the required review adds the next handoff entry.

### Open questions that block progress

No new owner question was filed in this review. The correction for P2-9 can require a revision to D-205. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-6 and P2-9, add a regression test for the comparer trigger, and request another repeat review.

## Session 38: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the fifth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The fifth review closed P2-3 and P2-6, and it opened P2-7 and P2-8. Both reproduce, so both have full merit.
- P2-7. `System.Guid.NewGuid()` compiles in Core and gave no finding. D-205 approved `System` as a whole namespace, and `System` is the one broad namespace that Core uses, so the rest of the nondeterminism sits inside it.
- Before the question, this session measured the cost. A run with `System` removed from the allowlist named seven `System` types in Core. The owner chose the type allowlist with that number in hand (D-206, OQ-79, F-67).
- D-206 revises D-205 in part. `System` is approved by type now, and the other approved namespaces stand as whole namespaces.
- `Guid` and `HashCode` also join the type denylist, so each reports its own rule. The review asks for a randomness finding on `Guid.NewGuid()`, and it reads `L-RANDOM`.
- One correction goes past the finding. `Object.GetHashCode` and `String.GetHashCode` report `L-IDENTITY`, because a member of an approved type can still read the machine. A hash of an address changes an iteration order between two runs of one seed.
- The first form of the `System` rule was wrong, and the check caught it. The rule also read the type a method gives back, and `det-lint` reported 24 findings on clean Core code. The allowlist binds the names that Core writes, never a return type.
- P2-8. `using System.Text;` gave no finding, because `AddImportFinding` read the denylist alone. It calls the allowlist now.
- 183 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds five review commits and five correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 183 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- An allowlist at the namespace level leaves the one namespace that the project uses. `System` holds `Guid`, `HashCode`, `GC`, `OperatingSystem`, `Console`, and `AppContext`. Approve that namespace by type.
- A type rule must bind the names that the source writes, not the type a method gives back. The first form flagged every method that gives back a bool. Run the tool on clean code before you keep a rule.
- A member of an approved type can still read the machine. `Object.GetHashCode` gives an address, and `String.GetHashCode` takes a new seed in each process.
- An allowlist binds each entry point on its own. The import check and the use check are two entry points, and P2-8 was the one that nobody updated.
- Measure the friction before you ask the owner for a stricter rule. Seven types was the number that settled the question.

### Open questions that block progress

No new owner question. OQ-79 is resolved by D-206. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 37: 2026-09-08, Codex

Author: Codex
Session: fourth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `60d678e` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-3 is fixed. The allowlist blocks the prior reflection and metadata probes.
- Confirmed that P2-6 is fixed at this effective head. The PR description identifies the current correction evidence.
- Added P2-7. `Guid.NewGuid()` compiles in Core and gives no lint finding against the seed-only rule.
- Added P2-8. An unapproved `using System.Text;` directive compiles and gives no `L-NAMESPACE` finding.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `60d678e`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 172 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `60d678e`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-7 and P2-8, then another repeat review.

### Traps and gotchas

- The exact `System` namespace contains `Guid.NewGuid()` and other nondeterministic APIs.
- A namespace allowlist still needs symbol bans inside each approved namespace.
- `AddImportFinding` applies the old namespace denylist, but it does not apply the new allowlist.
- An unused `using` directive compiles without a warning in the Core project.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-7 and P2-8, add their regression tests, and request another repeat review.

## Session 36: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the fourth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The fourth review kept P2-3 and P2-6 open. Both reproduce, so both have full merit.
- P2-3 reopened for the fourth time. `System.ComponentModel.TypeDescriptor.GetProperties` compiles in Core and gave no finding. Each pass named one more type: namespace text, member words, `System.Enum`, then `TypeDescriptor`.
- The denylist was the defect, not its contents. `System.Linq.Expressions`, `System.Text.Json`, `System.Runtime.Serialization`, and `System.Dynamic` all reach type metadata under no name that a rule held. A fifth pass was likely.
- The owner chose the namespace allowlist (D-205, OQ-78, F-66). Core uses `System`, `System.Collections.Generic`, `System.Globalization`, `System.Numerics`, `System.Runtime.CompilerServices`, and any namespace under `WhatYouCarry.`. Each entry matches one namespace and never its children, so `System` does not approve `System.ComponentModel`.
- The type denylist stays for the cases inside an approved namespace. `TypeDescriptor` joined it too, so the review's regression check reads `L-REFLECTION` and not the wider rule.
- Core used two namespaces, so the rule changed no Core file. An adversarial run reported all five planted uses, and three of them name a namespace that no denylist ever held.
- P2-6, second pass. The description named head `5316033` and session 28 after the first correction. It now names the effective head and the current session.
- 172 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds four review commits and four correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 172 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A denylist over a library the size of the class library never ends. Four review passes proved it on this PR. Turn the boundary around and approve what enters.
- An allowlist entry matches one namespace and never its children. `System` must not approve `System.ComponentModel`, so the match is exact.
- Keep the type denylist beside the allowlist. `System.Math` and `System.Type` sit inside an approved namespace, and only the denylist reaches them.
- A specific rule id carries more than a wide one. `TypeDescriptor` sits in both lists, so the finding says reflection and not namespace.
- The PR description is part of the record (D-118), and a review reads it. Update it with each correction, and name the effective head in it.

### Open questions that block progress

No new owner question. OQ-78 is resolved by D-205. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 35: 2026-09-08, Codex

Author: Codex
Session: third repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `549c75c` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-5 is fixed. The lint reports active and inactive `#if` directives under D-204.
- Kept P2-3 open. `TypeDescriptor.GetProperties(object)` compiles in Core and gives no reflection finding.
- Kept P2-6 open. The PR description omits effective head `549c75c` and incorrectly identifies Session 28 as current.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `549c75c`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 160 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `549c75c`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-3 and P2-6, then another repeat review.

### Traps and gotchas

- `TypeDescriptor` accesses type metadata without a `System.Reflection` symbol or a `System.Type` result.
- A finite banned-type table needs probes against all BCL metadata-access surfaces.
- The PR description must identify the effective head, not only the prior review head.
- A session number in the PR description becomes stale after each review response.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3, then update the PR description for P2-6. Request another repeat review.

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
