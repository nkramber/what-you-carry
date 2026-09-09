# PR-17 review response

Date: 2026-09-08

This file answers `docs/reviews/pr-17.md`, the review of head `614ce49`.

## Summary

All three findings have full merit. Each trigger reproduces on `614ce49`, and each one now has a correction and a regression test that fails on the old code.

The review found no concern for `## Out of scope` and made no description edit, so both sections say none.

## P2-1: The content hash has ambiguous file framing

Disposition: full merit. This is the most serious of the three.

The trigger reproduces. The path `a` with the bytes `bc` and the path `ab` with the byte `c` both gave one hash at `614ce49`, because the input held `abc` in each case. Two content sets shared one hash, and the content hash exists to tell two sets apart (D-163, D-151).

Correction: `WhatYouCarry.Core/Content/ContentHash.cs`. Each file enters the input as a length, its path, a length, and its bytes. Each length takes eight bytes, most significant first. The sort and the repeated-path check stay as they were.

Regression check: `TheHashFramesEachFile` covers the review trigger, a boundary that moves between two files, and an empty file that changes the hash. `ContentHashIsStable` still asserts that the source order does not change the hash and that a rename does.

## P2-2: An out-of-range or fractional JSON number escapes without content context

Disposition: full merit.

The trigger reproduces. `{"minDepth":1.5}` and a number above `Int64.MaxValue` each raised a `FormatException` at `614ce49`. The catch held `JsonException` alone, and `FormatException` is not one, so the error left Core with no file and no field.

The review names the cause exactly. `GetInt64` ran in the reader, and D-220 gives the reader the token text and the validator the number shape.

Correction: the reader keeps the token text through `Utf8JsonReader.ValueSpan`, so it converts nothing. The validator then parses the text, and it already reported the file, the field, and the reason for a number that no whole number holds. The reader now matches the contract that D-220 states.

Regression check: `ANumberThatNoWholeNumberHoldsNamesTheField` covers a fractional number, a number above the range, a number below it, and an exponent form. It asserts that the reader keeps the text and that the validator names the file, the field, and the reason.

## P2-3: The string lint rule exempts every method named `Get`

Disposition: full merit.

The trigger reproduces. `inventory.Get("You died")` gave no finding at `614ce49`. The rule read the last method name of the call, and every type can hold a `Get` method.

Correction: `Get` leaves the engine-name list. A `Get` call takes an id only when its receiver names the string table, and `StringTableReceivers` holds `Strings` and `strings`. Every other `Get` is a finding, and the engine names keep their own exemptions.

This correction stays syntactic, and this response states the limit. The Game project has an engine dependency, so a compilation of it needs the engine assemblies, and the project holds no source file today. The PR that first writes Game code can add a symbol read, and the receiver list holds the boundary until then. The review offers both, and it names the receiver rule first.

Regression check: `OnlyTheStringTableReceiverExemptsAGet` covers the review trigger, a second unrelated `Get`, and three shapes of the string-table call. `TheEngineListHoldsNoGet` asserts that `Get` left the engine list.

## New ids

- F-78: the hash framing and the number that left Core without its context.
- F-79: the `Get` that any type can hold.
- D-223 gains `System.Text.Encoding.GetString/1`, `Utf8JsonReader.ValueSpan`, `System.Array`, and `System.Array.Length`.

`System.Array` and `System.Array.Length` were absent on purpose after PR-4, because Core used no array member then. The P2-1 correction reads the length of a byte array, so this PR adds them, which is the rule of D-207 working as written.

## Verification

- The four review triggers, run against `614ce49` before any correction: all four failed.
- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 280 tests, 0 failures. 10 are new.
- `det-lint --root .`: 0 findings. Core 0 in 21 files, Game 0 in 0 files.
- `ste-check --root .`: 0 findings in 15 files.
- `bit-identity`: `4d6385bb92454694`, unchanged. This pass changed no simulation number.
