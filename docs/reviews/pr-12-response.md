# PR-12 review response

Date: 2026-09-08

This file answers `docs/reviews/pr-12.md`. The passes answer the review of head `c2ba592`, and the repeat reviews of heads `1bc665b` and `5316033`.

## Summary

All four findings have full merit. Each trigger reproduces on `c2ba592`. Each one now has a correction and a regression test that fails on the old code.

The checkout was not ahead of the remote at the start of this session, so the review commit `2f4f7cc` needed no push (F-59).

## P2-1: Atan2 loses negative zero on the negative x-axis

Disposition: full merit.

The trigger reproduces. `DetMath.Atan2(-0.0f, -1.0f)` gave `3.1415927` at `c2ba592`, and `Math.Atan2` gives `-3.1415927`. The error is two pi. The cause is the line `return y < 0.0f ? -angle : angle;`. Negative zero is not below zero, so the comparison missed it.

Correction: `WhatYouCarry.Core/Determinism/DetMath.cs`. The sign now comes from `float.IsNegative(y)`, which reads the sign bit. The review also names the raw bits, and that part has merit too: `Atan2(-0.0f, 1.0f)` now gives negative zero, which the state hash reads as a different field from positive zero (D-160).

The `x < 0.0f` fold needs no change. A negative zero x is not below zero, and `Atan2` gives pi/2 for a positive y on either sign of a zero x, so the fold must not run there. A first correction added a branch for that case, and a check showed the branch changed no result. The branch is gone.

Regression check: `Atan2ReadsTheSignBitOfANegativeZero` in `WhatYouCarry.Tests/DetMathTests.cs`. It asserts the negative-pi result, the negative-zero result, both zero-x axes, and every sign pair against the double reference. It failed on the old code with `3.1415927` against `-3.1415927`.

The sweep also grew. The `Atan2` grid in `BitIdentitySweep` never makes a negative zero, so the three-platform check could not have caught this defect. The sweep now holds the four sign pairs. That changes the pinned hash from `ef592d4148eb8ba0` to `4d6385bb92454694`, and the CI job proves the new number on the three platforms. This addition is wider than the finding asks. The reason is that the check this PR creates missed the defect this review found.

## P2-2: A default StateHash silently uses the wrong FNV offset

Disposition: full merit.

The trigger reproduces. `StateHash uninitialized = default;` then `uninitialized.Add(1)` gave a stable number at `c2ba592`, and no FNV-1a hash holds it. FNV-1a starts from the offset basis, and a default struct starts from zero.

Correction: `WhatYouCarry.Core/Determinism/StateHash.cs`. The struct holds a `started` field that only the private constructor sets. `AddByte`, `Value`, and `ToString` each call `EnsureStarted`, which throws with the name of the correct factory. An absent value is an error and never a zero (T-2).

The review permits two corrections: a valid default, or a rejected default. This takes the second one. A valid default needs the field to hold the value exclusive-or the offset basis, and a reader must then hold that indirection in mind. A default `StateHash` means a caller that never called `Start`, and T-2 asks the code to name that mistake.

`Equals` compares both fields, so a default hash equals no started hash. `GetHashCode` reads the field and never throws, because a dictionary can hold a default value.

Regression check: `ADefaultStateHashReportsItself` in `WhatYouCarry.Tests/StateHashTests.cs`. It asserts the error on every `Add` overload, on `Value`, and on `ToString`, and it asserts that `Equals` and `GetHashCode` still answer. It failed on the old code, which threw nothing.

## P2-3: The lint check does not detect reflection through System.Type

Disposition: full merit.

The trigger reproduces. A scan of `typeof(string).GetMethods()` reported no finding at `c2ba592`. The namespace rule reads the text `System.Reflection`, and this code never spells it.

Correction: `WhatYouCarry.Tools/DetLint/BannedSymbols.cs` holds a new `ReflectionMembers` list, and `CoreSourceScan` reports one of those names when it stands on the right of a dot. The list holds `GetType`, the `GetMethod` family, `GetCustomAttributes`, `InvokeMember`, `MakeGenericType`, and the rest. The `Names` list also gains `Activator`, `Assembly`, `BindingFlags`, and the four `Info` types.

The review asks to keep legitimate type operations available, and the list follows that. It holds no name that a Core type can hold too. `Type` is absent, because `block.Type` is correct Core code. `typeof` is absent, because it reads nothing at run time on its own. Each dangerous operation needs one of the listed names.

Regression check: `ReflectionWithoutTheNamespaceIsAFinding` asserts an `L-REFLECTION` finding for the review trigger and for three more forms. `AnOrdinaryMemberIsNotAReflectionFinding` asserts that `c.Type`, `c.Value`, and `nameof` stay clean. Both are in `WhatYouCarry.Tests/DetLintTests.cs`.

## P2-3, second pass: the word list was incomplete

Disposition: full merit.

The repeat review at `1bc665b` kept P2-3 open and gave two probes. Both reproduce.

- `Type.GetEvents()` gave no finding. `GetEvents` was absent from the member list, and no list of words can be complete.
- A Core `probe.GetMethods()` gave an `L-REFLECTION` finding. The word matched, and the symbol was a Core member.

The review is correct about the cause. A word list cannot tell two symbols with one name apart. Any list I extend keeps both defects, because `GetType` itself can be a Core member: a class that declares `public new string GetType()` compiles, and this session checked that.

Correction: `WhatYouCarry.Tools/DetLint/CoreSourceScan.cs` now compiles the Core sources with `CSharpCompilation` and asks the semantic model what each name means. The rules read symbols:

- the type that owns the symbol, by its full name, such as `System.Type` or `System.Math`.
- the namespace of that type, for `System.Reflection` and `System.Runtime.Intrinsics`.
- the type that a method or a property gives back, which catches `object.GetType()`. That method belongs to `System.Object`, so the owner rule alone cannot see it.

`BannedSymbols` now holds full type names instead of bare words. The member word list is gone.

This correction is the first option that the review names. The second option, another complete check without text ambiguity, stays possible: a ban on `typeof`, on `GetType`, and on `Type` outside member position covers the two probes. This session did not take it, because it keeps two ambiguities of its own, and because a word rule would also reject the Core `Vector3` that PR-7 declares.

The scan needed one guard for T-2. Without the runtime references every name resolves to nothing, and the scan would report no finding for any file. The compilation now checks that `System.Math` and `System.Reflection.Assembly` resolve, and it throws with context when they do not. The repository scan also reports every compiler error, because a Core file that does not compile resolves no symbol and would pass in silence.

Regression check: `ReflectionWithoutItsNamespaceIsAFinding` covers `typeof(x).GetMethods()`, `o.GetType()`, `t.GetEvents()`, `t.GetProperties()`, and `Activator.CreateInstance`. `ACoreMemberThatSharesAReflectionNameIsNotAFinding` declares a Core type with `GetMethods`, `GetProperties`, and `Type` members and asserts no finding. `ACoreTypeThatSharesABannedNameIsNotAFinding` declares a Core `Vector3` and asserts no finding. `ACoreSourceThatDoesNotCompileIsAFinding` covers the T-2 guard. All are in `WhatYouCarry.Tests/DetLintTests.cs`, and each fails on the old code.

An adversarial run against the real Core tree reported all nine planted uses, and it reported nothing for the Core `Vector3`.

The review also asks for the F-# correction, and that has merit. The reflection comments cited F-62, which is the `Atan2` defect. F-64 is the register entry, and every comment now cites it. F-64 also records this second pass, and D-202 carries a dated note: the package stands, and the tool reads symbols and not words.

## P2-4: Any file named DetMath.cs receives the MathF exemption

Disposition: full merit.

The trigger reproduces. A scan of `WhatYouCarry.Core/Other/DetMath.cs` with a `MathF.Sqrt` call reported no finding at `c2ba592`. The rule read `Path.GetFileName(path)`.

Correction: `BannedSymbols.DetMathFileName` becomes `BannedSymbols.DetMathPath`, which holds `WhatYouCarry.Core/Determinism/DetMath.cs`. `CoreSourceScan.ScanText` compares the whole Core-relative path, and it turns a Windows separator into a forward slash first.

Regression check: `OnlyTheCanonicalDetMathPathIsExempt` asserts an `L-MATHF` finding for the review trigger and for `WhatYouCarry.Core/DetMath.cs`, and no finding for the canonical path in either separator form. `TheCanonicalDetMathPathExists` asserts that the canonical path names a file in this checkout, so the exemption can never point at nothing.

## New ids

- F-62: the `Atan2` negative-zero defect, and the sweep gap that hid it.
- F-63: the default `StateHash` defect.
- F-64: the `DetMath.cs` file-name exemption defect.

No new decision. No new open question.

## P2-3, third pass: the symbol table omitted System.Enum

Disposition: full merit.

The trigger reproduces. `System.Enum.IsDefined(typeof(ReviewProbeValue), 0)` compiled in Core and gave 0 findings at `5316033`. The symbol read was correct, and the table was short.

The review names the strongest evidence for it. Session 28 removed `Enum.IsDefined` from `Rng.ForStream` and named it reflection that G-2 bans, so the tool contradicted the code it guards.

Correction: `BannedSymbols.Types` gains `System.Enum`, `System.Attribute`, `System.AppDomain`, `System.Delegate`, `System.MulticastDelegate`, the three runtime handle types, and `System.Runtime.CompilerServices.RuntimeHelpers`. `CoreSourceScan` also reports the `typeof` keyword. No symbol carries that name, and a `typeof` value can reach a place where no name is banned, so the keyword itself is the rule.

An ordinary enum stays legal. The ban reads `System.Enum`, which owns the metadata methods, and never the Core enum that declares the values. `Rng.ForStream` compares two `RngStream` values, and `det-lint` reports 0 findings on Core.

Regression check: `ReflectionOutsideTheKnownTypesIsAFinding` covers `Enum.IsDefined`, `Enum.GetNames`, `Enum.GetValues`, `Enum.Parse`, `HasFlag`, `Delegate.Method`, `AppDomain.CurrentDomain`, and `RuntimeHelpers.GetHashCode`. `TypeOfIsAFinding` covers the keyword. `AnOrdinaryEnumIsNotAFinding` declares a Core enum and asserts no finding. The two prior probe tests stay green.

## P2-5: Active conditional code escapes the lint compilation

Disposition: full merit.

The trigger reproduces. A `System.Math.Sin` call inside `#if NET10_0` gave 0 findings at `5316033`. The Core project targets net10.0, so the build defines that symbol and compiles the call.

The correction is an owner decision, because one of the two options adds a rule for Core source. This session gave the owner the evidence for both and the owner selected the ban (D-204).

The reason the other option loses: it cannot be complete. `DEBUG` and `RELEASE` are both real builds, so a lint that parses one configuration never reads the other. The bit-identity job builds Debug, so a Release-only divergence would ship unproven by the gate that exists to catch it. A symbol list also has to track the target framework, and a later bump that misses it reopens the hole in silence.

Correction: `CoreSourceScan` reports each `#if` as `L-CONDITIONAL`. The rule reads the directive and not its body, so an inactive branch is a finding too. A `#nullable`, `#region`, or `#pragma` directive stays legal, because none of them selects a branch. Core held no conditional directive, so the rule changed no Core file.

Regression check: `ConditionalCompilationInCoreIsAFinding` covers the review trigger and a `#if DEBUG` pair. `ADirectiveThatSelectsNoBranchIsNotAFinding` covers the three legal directives.

## P2-6: The PR description reports the superseded lint correction

Disposition: full merit.

The description still named the member word list, 144 tests, and head `1bc665b`. The record did not match the change.

Correction: the description now holds a table of all six findings and their state, a section on the symbol read, the reason the sweep grew once, 156 tests, and the current review state. It was updated before this commit, and again after D-204 landed.

## New ids, second and third pass

- D-204: Core holds no conditional compilation. Resolves OQ-77.
- OQ-77: the P2-5 correction. Resolved by D-204.
- F-65: the conditional compilation gap.
- F-64 now records all three P2-3 passes.

## Verification

- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 144 tests, 0 failures. 11 are new.
- The five review triggers, run against `c2ba592` before any correction: all five failed.
- `det-lint --root .`: 0 findings in 4 Core files.
- `ste-check --root .`: 0 findings in 15 files.
- `bit-identity`: `4d6385bb92454694` on macOS arm64.
- The pinned hash changed only because the sweep grew. Core gives the same numbers for every input that the old sweep read.

## Verification, second pass at `1bc665b`

- The two repeat-review probes, run before the correction: both failed.
- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 146 tests, 0 failures.
- `det-lint --root .`: 0 findings in 4 Core files.
- An adversarial Core tree with nine planted uses: 10 findings, and no finding for the Core `Vector3` in the same file.
- `ste-check --root .`: 0 findings in 15 files.
- `bit-identity`: `4d6385bb92454694`, unchanged. The second pass changed no Core number.
