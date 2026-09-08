# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 47: 2026-09-08, Codex

Author: Codex
Session: review PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Reviewed PR #15 at effective head `41206bb` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the substantive change, and Codex reviewed it.
- Inspected the complete diff, the six exit tests, all changed documents, and the current callers.
- Added P2-1. Caller fields can add a second `level` or `message` property to the JSON object.
- Added P2-2. One safe assertion changes the caller fields, so a second safe assertion throws on `assertFile`.
- Added P2-3. An unpaired surrogate passes the logger and produces JSON that `JsonDocument` rejects.
- Added P2-4. The JSON write path has nested helper calls beyond the one level that D-110 permits.
- Wrote `docs/reviews/pr-15.md` with the verdict `Changes required`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `41206bb`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 219 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `41206bb`.
- Three focused review probes fail and reproduce P2-1, P2-2, and P2-3.

### In flight

PR #15 needs corrections for P2-1 to P2-4, then a repeat Codex review.

### Traps and gotchas

- `LogFields` checks duplicates only inside the caller set. It does not reserve the logger keys.
- `Invariant.Assert` adds its call-site fields to the object that the caller owns.
- A JSON control-character check does not cover invalid UTF-16 surrogate sequences.
- A green broad suite did not cover repeated safe failures or the complete string domain.

### Open questions that block progress

No new owner question. No open question blocks the corrections.

### Next concrete action

Correct P2-1 to P2-4, add regression tests, and request a repeat Codex review.

## Session 46: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-4, the logger, the error context, and the assertions. Branch `feat/pr-4-logging`.

### What this session did, and why

- Started PR-4 after the owner merged PR #14. No open question blocked it, and PR-4 is the first PR with no absent check, so the D-148 clause does not apply.
- Five owner questions came before the code, and D-211 to D-216 record the answers.
- D-211: Core builds a line and hands it to an `ILogSink`. Core opens no file, so a disk failure never reaches the simulation thread, and `System.IO` stays out of the allowlist.
- D-212: every line carries a level and a message beside its context fields. No line carries a wall clock, because the tick is the only time in Core.
- D-213: `StringBuilder` builds the line, because the escape pass appends one character at a time.
- D-215: the assertion report names the call site from the compiler, and it walks no stack. A stack trace changes with the build and the platform. This revises the PR-4 scope line, which said "the stack".
- Wrote `LogLevel`, `LogContextKind`, `ILogSink`, `LogFields`, `ContextException`, `JsonlLogger`, and `Invariant` in `Core/Logging/`.
- 26 new tests. The total is 219. Exit tests 1 to 6 each have a test, and the JSON tests parse each line with a real parser.
- D-214 records the allowlist additions: 8 types and 16 members. A removal check proved each one in use. It also found two dead entries in the PR-3 list, `System.Object` and `List.new/0`, and both are gone.
- The removal check found a live gap. `CultureInfo c = new("en-US", true);` gave no finding, and `new CultureInfo("en-US", true)` gave one, so the P2-9 case was reachable by another spelling. A `base` initializer and an indexer passed the same way (F-71). D-216 closes all three, and the owner chose the fix in this branch over a separate PR.
- `det-lint` caught one real defect in the new Core code. `context.ToString()` on an enum reads the enum metadata, which G-2 bans. An explicit switch replaces it.

### State of the build

- `main` is at `51b3de7`. This branch holds the PR-4 work above it.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 219 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. PR-4 adds no simulation number.

### In flight

PR-4 waits for a Codex review (T-4). It changes code, so no override applies.

### Traps and gotchas

- The allowlist needs a removal check, and not a reading. Two entries of the PR-3 list were dead, and one gap hid behind a spelling that the rule never read.
- A dead allowlist entry widens the boundary in silence. Check each new entry by removal before the PR opens.
- `det-lint` reads the new Core code as it lands. It caught an enum `ToString` in this session, which is reflection under G-2.
- A collection expression, `[]`, calls no constructor that the source names, so `List.new/0` stayed dead.
- An xUnit lambda with every path throwing binds to `Func<Task>` and not to `Action`. Name the delegate type.
- A test that asserts a lint finding breaks when a later PR approves that type. Two such tests moved to a type that stays unapproved.

### Open questions that block progress

No new owner question. OQ-82 to OQ-86 are resolved by D-211 to D-216. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session reviews PR-4 per `.claude/skills/pr-review/SKILL.md`, under the scope rules of D-209, and writes `docs/reviews/pr-<number>.md`.

## Session 45: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-3 merge across the documents. Branch `docs/pr-3-merge-record`.

### What this session did, and why

- The owner merged PR #12 as `9e6fd5f`, after a Codex review gave the verdict `Ready for owner merge` at effective head `4d7cb3e`.
- Audited every document against the merged state. The registers were complete: D-200 to D-209, OQ-73 to OQ-81, and F-60 to F-70 all landed with the PR.
- Four documents held a stale line, and this session corrects each one.
- `docs/roadmaps/phase-1-foundations.md`: the PR-3 status line said "opened", and it names the merge commit now. The header cited D-200 to D-203, and it cites D-200 to D-209 now. The correction passes record the PR-3 outcome.
- `docs/design.md`: the PR-2 and PR-3 markers now name the merge, as the PR-1 marker does. The Phase 1 sequence marks PR-2 and PR-3 as merged.
- `dotnet test` caught the one defect in this session. `RepositoryDocumentsPass` runs the STE checker over the repository, and a new sentence of 26 words failed it. The sentence is three sentences now. A second sentence failed the same limit later, for the biome entry.
- The owner answered OQ-12, the last open question of Phase 1 before PR-9. The v1 biome is a collapsed deep mine (D-210). The block set holds seven blocks, and the three depth bands are floors 1 to 5, 6 to 10, and 11 to 15.
- The biome answer reaches four documents: the decision register, the questions register, the design doc world section and PR-9 entry, and the PR-9 scope in the focused roadmap.
- D-210 feeds four open questions that a later phase needs: OQ-1 the palette, OQ-61 the silhouettes, OQ-64 the props, and PR-14 the texture generator. The three props of OQ-64 belong in a mine with no change.

### State of the build

- `main` is at `9e6fd5f`, the squash merge of PR #12. This branch holds one commit above it.
- Remote head: `origin/docs/pr-3-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs on `main` at `9e6fd5f`.

### In flight

This PR changes only `docs/`, so the `review-override` label covers it (D-190). It carries two concerns, the merge record and the biome decision, and the owner asked for one PR. PR-4 starts after the merge.

### Traps and gotchas

- `RepositoryDocumentsPass` fails the test suite on any STE finding. A document edit needs `dotnet test`, and not the checker alone.
- A merged PR leaves a status line in two places: the focused roadmap and the design doc. The design doc also holds the Phase 1 sequence, which is a third place.
- The roadmap header lists the decisions that the file applies. A PR that adds a decision extends that list.
- Phase 1 has no gate before PR-11. Gate 1 needs the bit-identity job, `dotnet test`, and the night sweep of PR-58.

### Open questions that block progress

No new owner question. D-210 resolves OQ-12, so no open question blocks any PR of Phase 1. PR-4 to PR-11 each have every answer they need.

### Next concrete action

The owner merges this documentation PR with the `review-override` label. Then a new session starts PR-4: the JSONL logger, the error context sets, and the assertion helper (D-68, D-112, D-113). PR-4 is the first PR with no absent check, so the check clause of D-148 does not apply to it.

## Session 44: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #12 at the changed effective head. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Reloaded the `pr-review` skill after the owner approved its change as a second concern on this PR.
- Re-reviewed PR #12 at effective head `4d7cb3e` against base and merge base `86078b8`.
- Inspected the D-208 member allowlist correction and the D-209 review-scope rules. The provider gate passes because Claude Code wrote the substantive changes and Codex reviewed them.
- Closed P2-6. The PR description now identifies the effective head alone, its decision counts agree with its D-200 to D-209 table, and the P2-6 row describes the correction that the page contains.
- Closed P2-9. A focused Core probe reported the original comparer, runtime-feature, and current-culture paths, and it reported the later culture-constructor and string-intern paths.
- Added the required `## Out of scope` section to the review record. This review found no concern to put there.
- Set the verdict to `Ready for owner merge` for effective head `4d7cb3e`.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `4d7cb3e`.
- Remote head: `origin/feat/pr-3-determinism` will hold the metadata commit for this entry after the session-end push. The effective head stays `4d7cb3e` under D-184.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `4d7cb3e`. The review-gate results still reflect the prior verdict until this metadata commit runs them again.

### In flight

PR #12 is ready for the owner to merge after the review-gate checks turn green for this review record.

### Traps and gotchas

- The effective head includes the skill change because `.claude/skills/pr-review/SKILL.md` is outside the metadata set.
- D-209 closes an existing finding when its stated trigger and regression check pass. A different trigger takes a new id and must pass the PR-scope test before it becomes a finding.
- The focused probe is temporary. Remove it before the clean lint and build gates.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9 and does not block this review.

### Next concrete action

Wait for the review-gate checks on the metadata commit. The owner can then squash-merge PR #12.

## Session 43: 2026-09-08, Claude Code

Author: Claude Code
Session: the scope rules for the `pr-review` skill. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- The owner asked for a skill change after seven review passes on PR #12. That review reopened one finding four times and another one three times (F-70).
- Each pass found a real defect, and each correction was right. The sum went past the PR-3 scope, which names a lint tool for the banned symbols of G-2 and G-21. Five owner decisions, D-204 to D-208, came from that one boundary.
- Added a "Stay inside the pull request" section. The roadmap entry for the PR and its exit tests set the boundary. Four tests say a concern is in scope, and four say a later PR holds it.
- Added the `## Out of scope` heading to the review record skeleton. A line there names the PR that holds the concern, takes no severity, and never gives the verdict `Changes required`.
- Added the rule for a new check. A PR that creates a check must pass that check (G-19), and the check does not cover the whole platform on the first day.
- Added a "When a finding closes" section. A finding closes when its stated trigger and its regression check pass. A new trigger of the same class takes a new id. The third assessment of one id stops, and the owner settles the scope.
- Added two rows to the author push-back table, for a finding outside the scope and for a third reopen.
- The section states in two places that it never lowers the standard for the code that a PR changes. Every PR #12 finding stays a defect under these rules.
- This session first opened PR #13 for the skill change, on the G-10 reading that a skill change and the determinism work are two concerns. The owner asked for it on PR #12 instead. PR #13 is closed, and its branch is deleted.

### State of the build

- `main` is at `86078b8`. This branch holds the PR-3 work and this skill change above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. This commit changes no code.

### In flight

PR #12 needs a repeat review at the new effective head, under the new scope rules.

### Traps and gotchas

- `git checkout <file>` restores from HEAD and drops every uncommitted edit in that file. This session lost the whole skill change that way and wrote it again. Commit first, or copy the file.
- A scope rule can hide a real defect. Each rule here names the code that the PR changes as the part that keeps the full standard.
- The skill file was identical on `main` and on this branch, so the change moved between branches as a whole file. Check that before a copy.
- A second PR for a second concern is the G-10 reading, and the owner decides when one PR carries both.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 under the new scope rules, and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 42: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the seventh PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The seventh review kept P2-6 and P2-9 open. Both reproduce, so both have full merit.
- P2-9, third pass. The type allowlist approved every member of an approved type. `new CultureInfo("en-US", useUserOverride: true)`, `string.Intern(v)`, and `string.IsInterned(v)` each gave no finding, and the .NET contract names the user settings and the process intern pool.
- The gap moved from the namespace to the type, and the type still approved its whole surface. A member denylist would miss `new CultureInfo("en-US")`, which has the same behavior as the two-argument form.
- A measurement found six members and two constructors of an outside type in Core. The owner chose the member allowlist with the overload arity (D-208, OQ-81, F-69).
- The arity closes a gap that no trigger named. `UInt64.ToString/2` takes a format provider, and `ToString/0` reads the current culture. The same split binds `Parse`, `TryParse`, and `Compare`.
- Two corrections came from a run and not from the finding. A named argument carries a containing type and is not a member of it, so `useUserOverride:` gave a false finding until the rule read a method, a property, a field, and an event alone. `Object.GetType` joined the member denylist, so it keeps the `L-REFLECTION` id.
- P2-6, fourth pass. The decision section counted rows and not ids, the P2-6 row named a session that the description no longer holds, and the summary named the pass count and the newest review head. All three are corrected.
- 193 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds seven review commits and seven correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- The boundary moved four times: namespace, then `System` type, then every type, then every member. Each level approved the whole of the level below it. A member entry with an overload arity is the first form with nothing below it to leak.
- A named argument and a local both carry a containing type. Read a method, a property, a field, and an event alone.
- Keep a specific rule id beside the wide one. `Object.GetType` sits in the member denylist, so the finding says reflection and not member.
- An overload is not a behavior. `ToString/0` and `ToString/2` differ, and so do the `Parse` and `Compare` families.
- A description that names a pass count or a review head goes stale at the next review. Name the effective head alone.

### Open questions that block progress

No new owner question. OQ-81 is resolved by D-208. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 41: 2026-09-08, Codex

Author: Codex
Session: sixth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `e30ddfd` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that the original P2-9 triggers now report `L-TYPE`, `L-TYPE`, and `L-CLOCK`.
- Kept P2-9 open. `CultureInfo` and `String` are approved types, and their other machine-dependent members still pass.
- A probe used the `CultureInfo` constructor with user overrides. It also used `String.Intern` and `String.IsInterned`.
- The probe compiled, and `det-lint` reported 0 findings. The .NET contracts confirm that these APIs read user or process state.
- Kept P2-6 open. The PR description gives the wrong decision count and contains two claims that contradict its current content.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `e30ddfd`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 187 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `e30ddfd`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-6 and P2-9, then another repeat review.

### Traps and gotchas

- A type allowlist approves every member of an approved type unless another rule limits the members.
- `CultureInfo` has the required `InvariantCulture` member and constructors that read user settings.
- `String.Intern` changes the process intern pool. `String.IsInterned` reads that process state.
- Volatile pass counts and review heads become stale when the required next review completes.

### Open questions that block progress

No new owner question was filed in this review. The P2-9 correction can require a revision to D-207. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-6 and P2-9. Add regression tests for the three new triggers, then request another repeat review.

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
