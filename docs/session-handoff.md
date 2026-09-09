# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 56: 2026-09-08, Codex

Author: Codex
Session: review PR #17 for PR-5. Branch `feat/pr-5-content`.

### What the session did, and why

- Reviewed PR #17 at effective head `614ce49` against base and merge base `a37f0af`.
- Confirmed the provider gate. Session 55 identifies Claude Code as the author, and Codex is the eligible reviewer.
- Inspected the content source, JSON reader, validators, typed records, content hash, string table, Game string lint rule, tests, content files, and project documents.
- Found three P2 defects. The content hash has ambiguous file framing. A malformed number can escape without content context. The string lint rule exempts unrelated `Get` methods.
- Wrote `docs/reviews/pr-17.md` with the findings and the verdict `Changes required`.

### State of the build

- `main` is at `a37f0af`. The reviewed effective head is `614ce49`.
- Remote head: `origin/feat/pr-5-content` is `ace29d4`, verified after the review commit.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 270 tests and 0 failures.
- `det-lint` reports 0 findings. `ste-check` reports 0 findings. `bit-identity` gives `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes. All applicable GitHub build, test, lint, STE, comparison, and bit-identity checks pass at `614ce49`.

### In flight

PR #17 needs corrections for P2-1, P2-2, and P2-3, followed by a repeat review.

### Traps and gotchas

- Hash each path and byte sequence with unambiguous boundaries. Concatenation alone can give one input to two content sets.
- `Utf8JsonReader.GetInt64()` can throw `FormatException` for a JSON number that does not fit `Int64`. Catch or avoid that conversion before the contextual error boundary.
- The string rule must distinguish `Strings.Get` from an unrelated method named `Get`.
- The Game directory has no source file yet. The fixture rule must still falsify false exemptions.

### Open questions that block progress

No owner question is needed. The author can correct all three findings within the current scope.

### Next concrete action

The author corrects the three findings and requests a repeat review at the new effective head.

## Session 55: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-5, the content loader, the schemas, the content hash, and the string table. Branch `feat/pr-5-content`.

### What this session did, and why

- Started PR-5 after the owner merged PR #16. Four owner questions came before the code, and D-219 to D-223 record the answers.
- D-219: Core takes the bytes from an `IContentSource` and opens no file. This applies D-211 one PR later, and it keeps a directory enumeration order out of the content hash.
- D-220: `Utf8JsonReader` reads the JSON. It uses no serializer and no reflection, and it gives the position that D-92 needs for an error message. A hand-written parser holds the number, escape, and surrogate rules, and PR-4 took three review passes on the escape rules alone.
- D-221: SHA-256 for the content hash, and FNV-1a stays for the state hash. One needs resistance, and the other needs speed.
- D-222: one command, two rule sets. `det-lint` reads Core with the determinism rules and Game with the string rule of G-8.
- D-223 records the allowlist additions: 9 types and 27 members. The lists hold 27 types and 53 members now.
- Wrote `IContentSource`, `ContentFile`, `JsonObjectReader`, `ContentValidator`, `ContentError`, `ContentHash`, `Strings`, `FloorTemplate`, `ProjectileDefinition`, and `ContentLoader` in `Core/Content/`.
- Wrote the first content: three floor templates for the three bands of D-210, two projectile definitions, and the string table.
- Wrote `GameStringScan` in the lint tool, and the command reports the two counts.
- 36 new tests. The total is 270. Exit tests 1 to 6 each have a test.
- The tests caught one real defect. A malformed file let `JsonReaderException` out of Core, and that error names no file. Every content failure names the file, the field, and the reason now (D-92, T-2).

### State of the build

- `main` is at `a37f0af`. The branch holds the PR-5 work above it.
- Remote head: `origin/feat/pr-5-content` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 270 tests and 0 failures.
- `det-lint` reports 0 findings: Core 0 in 21 files, Game 0 in 0 files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. PR-5 adds no simulation number.

### In flight

PR-5 waits for a Codex review (T-4). It changes code, so no override applies.

### Traps and gotchas

- A platform reader raises its own error type, and that error names no file. Wrap it, or the file name never reaches the owner.
- The Game project holds no source file yet, so the Game scan reads nothing on this checkout. The rule has fixture tests, and `LintPassesGame` guards the real directory.
- The three floor bands must cover floors 1 to 15 with no gap and no overlap. `EveryContentFileLoads` counts each depth.
- An optional content field needs a default that the record states. `areaCentimetres` is zero when the file omits it.
- A `Utf8JsonReader` is a ref struct, and it lives inside the try block that catches its error.

### Open questions that block progress

No new owner question. No open question blocks PR-6 to PR-11.

### Next concrete action

A Codex session reviews PR-5 per `.claude/skills/pr-review/SKILL.md`, under the scope rules of D-209, and writes `docs/reviews/pr-<number>.md`.

## Session 54: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-4 merge across the documents. Branch `docs/pr-4-merge-record`.

### What this session did, and why

- The owner merged PR #15 as `1749463`, after a Codex review approved effective head `94afeea`.
- Audited every document against the merged state. The registers were complete: D-211 to D-217, OQ-82 to OQ-86, and F-71 to F-77 all landed with the PR.
- The owner then answered the one open item of PR-4. D-218 ratifies the log name and value rule, and OQ-87 records the question.
- Three lines were stale, and this session corrects each one. The focused roadmap had no status line for PR-4, and its header cited D-200 to D-216 while the PR added D-217. The design doc marked PR-4 as open, in the marker and in the Phase 1 sequence.
- The correction passes of the roadmap now record the PR-4 outcome: five owner questions before the code, three review passes on the logger, and the description rule of D-217.
- The build on `main` is healthy: 234 tests, 0 lint findings, 0 checker findings.

### State of the build

- `main` is at `1749463`, the squash merge of PR #15. This branch holds one commit above it.
- Remote head: `origin/docs/pr-4-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`.

### In flight

This PR changes only `docs/`, so the `review-override` label covers it (D-190). PR-5 starts after the merge.

### Traps and gotchas

- The owner ratified the PR-4 name and value rule as D-218. A field name is an identifier and must hold valid text. A value and a message take the replacement character, because a throw on content loses a crash report at the moment that the owner needs it most.
- D-218 reaches the registers and the two plan documents in this PR. The code comments cite F-77, and F-77 cites D-218, so this PR changes no code and keeps the override.
- PR-5 needs owner answers before its code. The content loader reads files, and D-211 kept `System.IO` out of Core for the logger. The loader also needs a JSON reader and SHA-256, and the D-207 list holds neither. The lint rule of G-8 also reads the Game project, and the tool reads Core alone today.
- A merged PR leaves a status line in three places: the focused roadmap, the design doc marker, and the Phase 1 sequence.

### Open questions that block progress

No open question blocks PR-5 from starting. Four questions come before its code, and the next session files them.

### Next concrete action

The owner merges this documentation PR with the `review-override` label. It carries the merge record and D-218, and the owner asked for one PR. Then a new session starts PR-5: the content loader, the schemas, the content hash, and the string table (D-91, D-92, D-98, D-163, D-168).

## Session 53: 2026-09-08, Codex

Author: Codex
Session: third repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `94afeea` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the P2-7 correction, and Codex reviewed it.
- Closed P2-7. `LogFields` rejects unmatched surrogates in field names with a position context. Valid paired-surrogate names remain accepted, and values and messages keep the replacement character.
- Corrected the stale effective head in the PR description from `d530f4c` to `94afeea` under D-217.
- Updated the review verdict to `Ready for owner merge` for `94afeea`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `94afeea`.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub CI, lint, STE, and bit-identity checks pass at `94afeea`. The review gate reflects the prior review until this metadata commit runs it again.

### In flight

PR #15 is ready for owner merge after the metadata review commit and its review-gate check pass.

### Traps and gotchas

- Replacement is correct for content and wrong for an identifier. Invalid field-name text must fail before serialization.
- The effective head is the code commit `94afeea`; the review record and handoff commit are metadata commits.
- The PR description had a stale effective-head fact even though its code and test counts were current.

### Open questions that block progress

No open question blocks PR-5 to PR-11.

### Next concrete action

Publish the metadata review commit, then verify the review-gate result and leave the owner to merge.

## Session 52: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the second PR #15 repeat review. Branch `feat/pr-4-logging`.

### What this session did, and why

- The review closed P2-3, P2-5, and P2-6, and it opened P2-7. That one has full merit, and the F-73 correction caused it.
- P2-7. The replacement character stood in place of a lone surrogate everywhere, and a field name took it too. A field named with a lone high surrogate and one named with a lone low surrogate both reached the object as one property name, which is the ambiguity that F-72 removed.
- The split is the fix. A field name is an identifier that the code writes, and it must reach the line unchanged, so invalid text in one is an error at the add. A value and a message carry content from the run, and those keep the replacement and never throw (F-77).
- The same pass flattened the add path. `AddField` called `Has`, which put it two levels below the caller, and the new name scan would have made a third. `AddField` calls no method now (D-110).
- The review used D-217 for the first time. It made four edits to the PR description and recorded each one under `## Description edits`. Every edit is correct, and none changes what the PR says it does.
- 1 new test. The total is 234.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, three review commits, and three correction commits.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head.

### Traps and gotchas

- A fix for content can break an identifier. The replacement character is right for a message and wrong for a field name, because two names can then reach the object as one.
- The response file states the name and value split as a Core contract, and no owner decision holds it. The owner can make it a decision.
- A depth fix and a new check meet. `AddField` was already two levels below its caller, and the name scan would have made a third, so the method calls nothing now.
- D-217 works. The reviewer corrected four stale facts in the description in the same pass that found P2-7.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure, and it updates `docs/reviews/pr-15.md` to the new effective head.

## Session 51: 2026-09-08, Codex

Author: Codex
Session: second repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `d530f4c` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the correction, and Codex reviewed it.
- Closed P2-3. Four labeled surrogate rows run through five text positions, and both parsing and string reading succeed.
- Closed P2-5. All three assertion call-site names are reserved, and the trusted copy path writes a complete report.
- Closed P2-6. D-214, the response, and the primary PR evidence agree on the head, the test total, and the allowlist counts.
- Added P2-7. Distinct accepted field names with unmatched high and low surrogates both serialize as U+FFFD. The resulting JSON object holds two equal property names, against the no-ambiguity contract of `LogFields` and T-2.
- Corrected four stale facts in the PR description under D-217, and recorded each edit in `docs/reviews/pr-15.md`.
- Updated the review verdict to `Changes required` for `d530f4c`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `d530f4c`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 233 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `d530f4c`. The review gate reflects the prior review until this metadata commit runs it again.

### In flight

PR #15 needs a correction for P2-7, then another repeat review.

### Traps and gotchas

- Replacement is not injective. Two invalid UTF-16 field names can become one valid JSON property name.
- A parser accepts duplicate JSON property names. Count the parsed properties or reject the input before serialization.
- The surrogate regression test puts invalid text in a field name, but it does not assert that distinct accepted names stay distinct.
- D-217 permits a reviewer to correct verified stale facts in the PR description. It does not permit changes to an owner-ticked gate line or to the author's substantive claims.

### Open questions that block progress

No open question blocks the P2-7 correction. The correction can reject invalid UTF-16 in field names or preserve a unique emitted name.

### Next concrete action

Correct P2-7 and add a regression test with distinct unmatched high- and low-surrogate field names. Then request another repeat review.

## Session 50: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #15 repeat review, and give the reviewer the PR description. Branch `feat/pr-4-logging`.

### What this session did, and why

- The repeat review closed P2-1, P2-2, and P2-4, and it kept P2-3 open and opened P2-5 and P2-6. All three have full merit.
- P2-3. The review found that three of four theory rows ran. xUnit takes the display name from the data, two rows of raw surrogate text give one name, and the low-surrogate row never ran. Each row carries a label and a code unit now, and the body builds five shapes for each one.
- Those rows then failed, and they showed the first fix was incomplete. `JsonDocument.Parse` accepts the `\u` escape of a lone surrogate, and `GetString` on that value throws. The first probe called `Parse` alone, so it passed and hid the rest.
- `AppendQuoted` writes the replacement character in place of a lone surrogate now. The line parses, and a reader takes the value back with one visible mark (F-73).
- P2-5. A caller field named `assertFile` stopped the assertion report, because the copy already held that name. This is P2-1 one level out: the first fix reserved the two names the logger writes and left the three the report writes. All five are reserved now, and `CopyWithCallSite` writes the call site on a path that no caller field reaches (F-74).
- P2-6. D-214 said 8 types and 16 members while the code held 9 and 18. A count of the lists gave the true numbers, and D-214 states them with the totals after them (F-75).
- The owner asked for a skill change so a reviewer can correct a stale PR description directly. D-217 records it, and the `pr-review` skill gains the rules and a `## Description edits` section in the record (F-76).
- 7 new tests. The total is 233.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, two review commits, and two correction commits.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 233 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head. That review can correct the PR description directly under D-217.

### Traps and gotchas

- Two xUnit theory rows of raw invalid text take one test id, and one row never runs. Give each row a label, and build the text in the body.
- `JsonDocument.Parse` and `GetString` are two gates. A line can parse and still fail when a reader asks for the value. Assert both.
- A reserved-name rule needs every name that the code writes. The first pass covered the logger and missed the assertion report.
- A count in a decision drifts from the code. State the totals, and count the list before you write them.
- A correction can be incomplete and still pass its first probe. Write the probe against the contract, not against the fix.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure, and it updates `docs/reviews/pr-15.md` to the new effective head.

## Session 49: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `e42a0a8` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the corrections, and Codex reviewed them.
- Closed P2-1. `LogFields` rejects the two logger names before a line exists.
- Closed P2-2. The assertion report uses a copy, so two safe failures preserve the caller fields.
- Kept P2-3 open. The encoder correction passes, but xUnit skips one required surrogate row because two rows have one test id.
- Closed P2-4. `BuildLine` calls two leaf helpers, and the prior nested helpers are absent.
- Added P2-5. A caller field with an assertion call-site name prevents every report and changes a safe failure to an exception.
- Added P2-6. D-214, the response, and the PR description disagree about the member count, test count, and final head.
- Updated `docs/reviews/pr-15.md` with the verdict `Changes required` for `e42a0a8`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `e42a0a8`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` reports 226 passes and 0 failures.
- The test run warns that xUnit skips one duplicate-id surrogate row. The P2-3 regression check is incomplete.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `e42a0a8`.

### In flight

PR #15 needs corrections for P2-3, P2-5, and P2-6, then another repeat review.

### Traps and gotchas

- xUnit can omit a theory row before execution when two invalid strings produce one display id.
- The assertion call-site names are logger-owned names, like `level` and `message`.
- A correction that adds an allowlist member must update the count and every current revision record.

### Open questions that block progress

No new owner question. No open question blocks the corrections.

### Next concrete action

Correct P2-3, P2-5, and P2-6, add the regression checks, and request another repeat Codex review.

## Session 48: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #15 review. Branch `feat/pr-4-logging`.

### What this session did, and why

- Read the four P2 findings in `docs/reviews/pr-15.md`. Each one reproduces, so each one has full merit.
- The first draft of this entry took the number 47, which the review session already held. The D-187 check caught it, and the entry is 48.
- This is the first review under D-209. It used the `## Out of scope` section for the Game-layer sink of PR-31 and PR-55 and for the simulation version constant of PR-7, and it gave neither a severity. Both readings are correct.
- P2-1. A caller field named `level` or `message` gave a line with two properties of one name. `LogFields` now reserves both names and rejects them at the add, so no such line reaches the sink (F-72).
- P2-2. A safe assertion added its call site to the caller field set, so a second safe assertion threw on the repeated name. A safe assertion promises to continue, and this turned the second one into an exception. The report takes a copy now (F-72).
- P2-3. A value with an unpaired surrogate gave a line that the parser rejected. The escape pass reads by index, keeps a matched pair as one character, and escapes every other surrogate. A check showed that `JsonDocument.Parse` accepts the escape form, so the fix keeps the value and never throws on a message (F-73).
- P2-4. The JSON write path ran three helper levels below its operation. `AppendText`, `AppendRaw`, and `HexDigit` are gone, and `BuildLine` calls only leaves (D-110, F-73).
- The P2-3 fix needs `System.String.this[]` for the one-character lookahead, and the owner approved that entry. D-214 records it.
- 7 new tests. The total is 226.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, the review commit, and this correction commit.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 226 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head.

### Traps and gotchas

- A helper that takes a caller object and adds to it changes that object for every later call. A report takes a copy.
- The logger writes its own `level` and `message`, so those two names need a rule of their own. A duplicate-name check over the caller fields alone does not reach them.
- An unpaired surrogate is not text that UTF-8 can hold. A parser rejects the raw form and accepts the `\u` escape form.
- D-110 counts every level. `BuildLine` to `AppendText` to `AppendQuoted` to `HexDigit` is three, and one is the limit.
- A patch script that fails part way leaves the file unchanged, and the build then passes on the old code. Read the file after a large scripted edit.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure and updates `docs/reviews/pr-15.md` to the new effective head.

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
