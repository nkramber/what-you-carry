# PR-15 review response

Date: 2026-09-08

This file answers `docs/reviews/pr-15.md`. The first pass answers the review of head `41206bb`. The second pass answers the repeat review of head `e42a0a8`.

## Summary

All four findings have full merit. Each trigger reproduces on `41206bb`, and each one now has a correction and a regression test that fails on the old code.

The review is the first one under D-209. It used the `## Out of scope` section for two concerns, the Game-layer sink of PR-31 and PR-55 and the simulation version constant of PR-7, and it gave neither a severity. Both readings are correct.

## P2-1: Caller fields can replace the required level or message

Disposition: full merit.

The trigger reproduces. A field named `level` gave a line with two `level` properties at `41206bb`. `LogFields` rejects two caller fields of one name, and the logger writes `level` and `message` itself, so those two names sat outside that rule.

Correction: `WhatYouCarry.Core/Logging/LogFields.cs`. `Add` rejects the two reserved names, and `LogFields.LevelName` and `LogFields.MessageName` hold them, so the logger and the rule read one source. The rejection lands at the add and not at the write, so no line with two of one name ever reaches the sink.

Regression check: `AReservedFieldNameIsAnError` covers both names. It asserts the error, and it then parses an ordinary line and counts one property of that name.

## P2-2: A safe assertion changes the caller context and breaks the next assertion

Disposition: full merit. This is the most serious of the four.

The trigger reproduces. Two safe assertions with one `LogFields` object threw on the second call at `41206bb`, because the first call added `assertFile` to the caller object. A safe assertion promises to write its report and continue, and this turned the second one into an exception.

Correction: `WhatYouCarry.Core/Logging/Invariant.cs`. The report takes a copy, and `LogFields.Copy` makes it. The caller set goes in and comes out unchanged.

Regression check: `ASafeAssertionLeavesTheCallerContextAlone` runs two safe failures on one set, asserts two lines and no exception, asserts the field count is unchanged, and then writes an ordinary line and asserts that it carries no assertion field.

## P2-3: The string encoder emits JSON that the standard parser rejects

Disposition: full merit.

The trigger reproduces. A value that holds `\uD800` gave a line that `JsonDocument.Parse` rejected at `41206bb`.

This session also checked the correction that the review offers. `JsonDocument.Parse` accepts `"\ud800"` in the escape form, so the escape keeps the line valid and keeps the value readable. The other path, a rejection of the value, would make a log call throw on its content, and a logger that throws on a message is a poor tool for a crash report.

Correction: `WhatYouCarry.Core/Logging/JsonlLogger.cs`. `AppendQuoted` reads the string by index. A high surrogate with its low partner stays as one character, and every other surrogate takes the `\u` escape with four lowercase digits. The control characters below the space take the same form.

The index loop needs `System.String.this[]` in the D-208 list, and the owner approved that one entry. The removal check confirms it in use.

Regression check: `AnUnpairedSurrogateKeepsTheLineValid` covers a lone high surrogate, a lone low surrogate, a value that is one lone surrogate, and two high surrogates in a row, in both a value and the message. `AMatchedSurrogatePairReadsBackUnchanged` asserts that a real pair survives the round trip.

## P2-4: The logger helper chain exceeds one level

Disposition: full merit.

The trace is correct. `BuildLine` called `AppendText`, which called `AppendQuoted`, which called `HexDigit`. That is three levels below the operation, and D-110 permits one.

Correction: `AppendText`, `AppendRaw`, and `HexDigit` are gone. `BuildLine` writes each separator itself and calls `AppendQuoted` for each JSON string. `AppendQuoted` writes the four hexadecimal digits in a short loop and calls no helper. `Write` calls `ContextName` and `RequiredFields`, and neither one calls a helper.

The whole write path is now one level: `BuildLine` calls `LevelName` and `AppendQuoted`, and both are leaves.

Regression check: this one is structural, so no test asserts it. `TheLineKeepsTheFieldOrder` and `LogLineIsValidJson` cover the output of the rewritten path, and every prior logging test stays green.

## New ids

- F-72: the reserved names and the caller context that a safe assertion changed.
- F-73: the unpaired surrogate and the helper chain.
- D-214 gains `System.String.this[]`, which the owner approved for the P2-3 correction.

No new decision beyond that entry. No new open question.

## P2-3, second pass: the escape form was not enough

Disposition: full merit, and the correction changes.

The repeat review kept P2-3 open on the test, and the check for that found a defect in the correction itself.

The row count is right. The theory held four `InlineData` rows of raw surrogate text, and three ran. xUnit takes the display name from the data, two rows of unpaired surrogate text give one name, and one row never ran. The low-surrogate case was the one that never ran.

The correction of the correction: each row now carries a label and a code unit, and the body builds the text. Four rows run, and each one covers five shapes: the surrogate between letters, alone, twice, at the start, and at the end. Each shape goes in a field value, in a field name, and in the message of one line.

Those rows then failed, and they showed that the first fix was incomplete. `JsonDocument.Parse` accepts the `\u` escape of a lone surrogate, and `GetString` on that value throws `Cannot read invalid UTF-16 JSON text`. The first probe of this session called `Parse` alone, so it passed and hid the rest.

`AppendQuoted` now writes the replacement character in place of a lone surrogate. The line parses, and a reader takes the value back with one visible mark where the invalid text was. That mark is the standard result of invalid UTF-16, and it is the opposite of a silent drop (T-2). The review offers a rejection instead, and this session did not take it: a logger that throws on the content of a message is a poor tool for a crash report (PR-55).

Regression check: `AnUnpairedSurrogateKeepsTheLineValid` runs four rows and twenty shapes, and each one asserts `GetString` and asserts that the lone surrogate is gone and the replacement character is present.

## P2-5: Caller fields can prevent every assertion report

Disposition: full merit.

The trigger reproduces. A caller field named `assertFile` made the report copy hold that name, and `CopyWithCallSite` then met a repeated name and threw before `logger.Write` ran. One accepted caller field stopped the report that D-112 requires, and a safe assertion became an exception.

This is the same shape as P2-1, one level out. That correction reserved the two names that the logger writes, and it left the three that the assertion report writes.

Correction: the three call-site names join the reserved set, so `LogFields.Add` rejects them. `CopyWithCallSite` writes them straight into the copy, on a path that no caller field reaches, so the report can never fail on a repeated name.

Regression check: `ACallerFieldCannotStopTheAssertionReport` covers all three names. It asserts the rejection at the add, and it then fails a safe assertion and asserts the report with its call site. `AReservedFieldNameIsAnError` also covers all five reserved names now.

## P2-6: The correction records contradict the reviewed revision

Disposition: full merit on all three points.

- D-214 said 8 types and 16 members, and it then named `String.this[]` as one more. The code held 9 types and 18 additions. A count of the two lists gave the true numbers.
- The response file named no final head.
- The PR description named `41206bb`, 219 tests, and the old member count.

Correction: D-214 states the counts in the code, 9 types and 18 members, and the totals after them, 18 types and 26 members. It also carries a dated note that names the first text. This response ends with the final head. The description names the current revision.

The owner also gave the reviewer a way to end this class of finding. D-217 lets a review correct a stale fact in the PR description directly, and record each edit under `## Description edits`. The PR #12 review reopened its description finding four times, and this one reopened once (F-76).

## Verification

- The four review triggers, run against `41206bb` before any correction: three of three runtime probes failed, and the helper trace matched the finding.
- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 226 tests, 0 failures. 7 are new.
- `det-lint --root .`: 0 findings in 11 Core files.
- `ste-check --root .`: 0 findings in 15 files.
- `bit-identity`: `4d6385bb92454694`, unchanged. This pass changed no simulation number.

## Verification, second pass at `e42a0a8`

- The P2-3 row count: 3 of 4 rows ran before the correction, and 4 of 4 run now.
- The P2-3 second defect: `GetString` threw on the escape form, and it returns the value now.
- The P2-5 trigger, run before the correction: it threw before the report.
- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 233 tests, 0 failures.
- `det-lint --root .`: 0 findings in 11 Core files.
- `ste-check --root .`: 0 findings in 15 files.
- `bit-identity`: `4d6385bb92454694`, unchanged.

## New ids, second pass

- D-217: a review corrects a stale fact in the PR description. Records F-76.
- F-74: the caller field that stopped the assertion report.
- F-75: the records that contradicted the reviewed revision.
- F-76: the description findings that four passes and one pass could not settle.

## Final head

The final head of this answer is the commit that holds this file.
