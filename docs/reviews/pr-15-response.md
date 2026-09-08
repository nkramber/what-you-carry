# PR-15 review response

Date: 2026-09-08

This file answers `docs/reviews/pr-15.md`, the review of head `41206bb`.

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

## Verification

- The four review triggers, run against `41206bb` before any correction: three of three runtime probes failed, and the helper trace matched the finding.
- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 226 tests, 0 failures. 7 are new.
- `det-lint --root .`: 0 findings in 11 Core files.
- `ste-check --root .`: 0 findings in 15 files.
- `bit-identity`: `4d6385bb92454694`, unchanged. This pass changed no simulation number.
