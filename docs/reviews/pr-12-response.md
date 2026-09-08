# PR-12 review response

Date: 2026-09-08

This file answers `docs/reviews/pr-12.md`. The passes answer the review of head `c2ba592`, and the repeat reviews of heads `1bc665b`, `5316033`, `549c75c`, `60d678e`, `9ad2a4c`, and `e30ddfd`.

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
- D-205: the Core namespace allowlist. Resolves OQ-78.
- OQ-77: the P2-5 correction. Resolved by D-204.
- OQ-78: the reflection boundary of the lint tool. Resolved by D-205.
- F-65: the conditional compilation gap.
- D-206: the Core `System` type allowlist. Resolves OQ-79, and revises D-205 in part.
- OQ-79: the `System` surface of Core. Resolved by D-206.
- F-66: the denylist that four passes could not complete.
- D-207: the Core type allowlist. Resolves OQ-80, and supersedes D-205 and D-206.
- OQ-80: the whole-namespace approvals. Resolved by D-207.
- F-67: the randomness source inside the approved namespace, and the import that escaped the allowlist.
- D-208: the Core member allowlist. Resolves OQ-81, and extends D-207.
- OQ-81: the member surface of an approved type. Resolved by D-208.
- F-68: the machine-dependent member behind an approved namespace.
- F-69: the machine-dependent member of an approved type.
- F-64 now records all four P2-3 passes.

## P2-3, fourth pass: the denylist itself was the defect

Disposition: full merit, and the correction goes wider than the finding asks.

The trigger reproduces. `System.ComponentModel.TypeDescriptor.GetProperties(object)` compiles in the Core project, checked with `dotnet build`, and gave 0 findings at `549c75c`.

The review asks for two things: the `TypeDescriptor` surface, and an audit of the remaining class library surfaces. The first has a plain correction. The second cannot end, and this response says so with the record of this review as the evidence.

P2-3 reopened four times. Each pass named one more thing the denylist missed:

| Pass | Head | What the denylist missed |
|---|---|---|
| 1 | `c2ba592` | reflection that never spells `System.Reflection` |
| 2 | `1bc665b` | `Type.GetEvents`, and a false report on a Core `probe.GetMethods` |
| 3 | `5316033` | `System.Enum` |
| 4 | `549c75c` | `System.ComponentModel.TypeDescriptor` |

The class library holds more of these. `System.Linq.Expressions`, `System.Text.Json`, `System.Runtime.Serialization`, and `System.Dynamic` each reach type metadata without a name that any current rule holds. A fifth pass was likely.

The owner selected the structural answer (D-205). Core may use only an approved namespace, and `det-lint` reports every other one as `L-NAMESPACE`. The approved set is `System`, `System.Collections.Generic`, `System.Globalization`, `System.Numerics`, `System.Runtime.CompilerServices`, and any namespace under `WhatYouCarry.`. Each entry matches one namespace and never its children, so `System` does not approve `System.ComponentModel`.

The type denylist stays, for the cases inside an approved namespace: `System.Math`, `System.Type`, `System.Enum`, `System.Random`, `System.DateTime`, and the rest. `System.ComponentModel.TypeDescriptor` also joins it, so the review's regression check reads `L-REFLECTION` and not the wider rule.

Core used two namespaces on this date, `System` and `System.Globalization`, so the rule changed no Core file.

Regression check: `TypeDescriptorIsAReflectionFinding` covers the review trigger. `ANamespaceOutsideTheAllowlistIsAFinding` covers `System.Text`, `System.Linq.Expressions`, `System.Text.Json`, `System.Collections`, and `System.Threading`. `AnApprovedNamespaceIsNotAFinding` and `AProjectNamespaceIsNotAFinding` guard the other side. Every prior probe test stays green.

An adversarial run against the real Core tree reported all five planted uses, and three of those five name a namespace that no denylist ever held.

## P2-6, second pass: the head and the session number

Disposition: full merit.

The description carried the semantic scan and 160 tests after the first correction, and it still named head `5316033` and session 28. Both were stale.

Correction: the description now names the effective head, the current session, and the current review state. This response also records that the description changes with each correction, because the PR record is part of the documentation set (D-118).

## P2-7: The System allowlist permits a second randomness source

Disposition: full merit.

The trigger reproduces. `System.Guid.NewGuid()` compiles in Core, checked with `dotnet build`, and gave 0 findings at `60d678e`. D-205 approved `System` as a whole namespace, and `Guid` sits inside it and outside the type denylist.

The finding also shows the limit of the D-205 answer. The allowlist closed every namespace that Core does not use, and `System` is the one broad namespace that Core does use. The rest of the nondeterminism lives there: `HashCode` takes a new seed in each process, and `GC`, `OperatingSystem`, `Console`, and `AppContext` each read the machine.

The review offers the narrow correction first, and this session measured the cost of the wide one before it asked the owner. A run with `System` removed from the allowlist named every `System` type that Core uses, and there are seven of them. The list starts at nine with `Object` and `String`.

The owner selected the allowlist (D-206). `System` is approved by type now, and the other approved namespaces stand as whole namespaces, so D-206 revises D-205 in part.

`Guid` and `HashCode` also join the type denylist, so each one reports its own rule. The review asks for a randomness finding on `Guid.NewGuid()`, and it reads `L-RANDOM` and not the wider `L-SYSTEM`.

One correction goes past the finding. A member of an approved type can still read the machine, so `Object.GetHashCode` and `String.GetHashCode` report `L-IDENTITY`. The default hash of a reference type is its address, and the string hash takes a new seed in each process. Either one changes an iteration order between two runs of one seed. This session found that gap while it checked the allowlist claim, and a known determinism hole is worse than a small scope change.

The first form of this correction was wrong, and a check caught it. The `System` type rule also read the type that a method gives back, and `det-lint` then reported 24 findings on clean Core code: every method that gives back a bool, a void, or a uint. The allowlist binds the names that Core writes, and never a return type. The namespace rule still reads a return type, because a namespace has no such noise.

Regression check: `AnotherRandomnessSourceIsAFinding` covers the review trigger. `AnUnapprovedSystemTypeIsAFinding` covers `GC`, `OperatingSystem`, `Console`, `AppContext`, and `Uri`. `AHashThatReadsTheMachineIsAFinding` covers the three hash paths. `AnApprovedSystemTypeIsNotAFinding` guards the other side with a tuple return, an exception, and a primitive member call.

An adversarial run against the real Core tree reported all seven planted uses, and two of them name a `System` type that no denylist held.

## P2-8: Unapproved namespace imports escape the allowlist

Disposition: full merit. This one is a plain defect in the D-205 correction.

The trigger reproduces. `using System.Text;` in a Core file gave 0 findings at `60d678e`. `AddImportFinding` read the namespace denylist and never the allowlist, so D-205 bound a use of a type and not an import.

Correction: `AddImportFinding` now calls `IsAllowedNamespace` after the denylist. An import of `System` stays correct, because D-206 approves its types one at a time.

Regression check: `AnUnapprovedNamespaceImportIsAFinding` asserts one `L-NAMESPACE` finding for `using System.Text;` with no use of a type in it, and no finding for `using System;`.

## P2-9: An approved namespace bypasses the seeded-identity rule

Disposition: full merit.

The trigger reproduces. `System.Collections.Generic.EqualityComparer<string>.Default.GetHashCode(v)` compiles in Core and gave 0 findings at `9ad2a4c`. `CultureInfo.CurrentCulture` and `RuntimeFeature.IsDynamicCodeSupported` gave none either. The review also ran three processes and got three hashes for one string, which is the strongest evidence in this review so far.

The cause is the same shape as P2-7, one level out. D-205 approved four namespaces as wholes, and each of those holds a machine-dependent type beside the one Core needs. `System.Collections.Generic` holds `EqualityComparer`, and `System.Runtime.CompilerServices` holds `RuntimeFeature`.

The review rules out one more member ban, and the evidence supports that. The same process-randomized string hash is reachable through `Dictionary`, `HashSet`, and any comparer that a caller gives.

This session measured the wide correction before it asked. A run with the four namespaces removed from the allowlist named one type: `CultureInfo`. The owner chose the one allowlist with that number in hand (D-207).

D-207 supersedes D-205 and D-206. Core approves every type outside this project by full name, and no namespace passes as a whole. The list holds ten entries. The tool is also smaller: the namespace allowlist and the `System` type list become one list, and the import rule reads that list instead of a second one, so it stays current on its own.

One case needs a member rule beside the type rule. `CultureInfo` is approved, because `StateHash` formats with `InvariantCulture`, and the same type holds `CurrentCulture`, which reads the user. The member denylist now holds the five culture members that read the user, the process, or the machine. A type with both kinds of member needs both rules, and that is the limit of a type allowlist.

`System.Array` and the collection types are absent from the list, because Core uses no array member and no collection today. The PR that first needs one adds it with a decision. PR-7 will need `System.Array`.

Regression check: `AMachineMemberBehindAWrapperIsAFinding` covers the comparer trigger and `RuntimeFeature`. `AMachineMemberOfAnApprovedTypeIsAFinding` covers `CultureInfo.CurrentCulture` and asserts that `InvariantCulture` stays clean. `AnUnapprovedTypeIsAFinding` covers twelve types across eight namespaces. `AnApprovedTypeIsNotAFinding` guards the other side.

An adversarial run against the real Core tree reported all three planted uses and stayed silent on `CultureInfo.InvariantCulture` in the same file.

## P2-6, third pass: the owner-decision section

Disposition: full merit.

The description carried a section titled "Four owner decisions this PR needed" that listed D-200 to D-203, while the gate section of the same description named D-200 to D-206. D-204 to D-207 each came from a review finding, and the section omitted them.

Correction: the section now lists every decision of this PR with the finding that produced it. The review also asks not to bind the description to a session number that the next review makes stale, and the description no longer names one.

## P2-9, third pass: an approved type is not an approved surface

Disposition: full merit.

The three triggers reproduce. `new CultureInfo("en-US", useUserOverride: true)`, `string.Intern(v)`, and `string.IsInterned(v)` each compile in Core and gave 0 findings at `e30ddfd`. The review cites the .NET contract for each one, and both citations hold: `UseUserOverride` reads the user settings, and `Intern` writes the process intern pool.

The review states the shape of the defect exactly. D-207 moved the gap from the namespace to the type, and the type still approved its whole surface.

The review also rules out another member denylist, and the evidence supports that. `CultureInfo` holds a second constructor with the same behavior, `new CultureInfo("en-US")`, which no denylist entry for the two-argument form would reach.

This session measured the wide correction before it asked. Core uses six members and two constructors of an outside type. The owner chose the member allowlist with the overload arity (D-208).

The arity closes a second gap that no trigger named. `UInt64.ToString/2` takes a format provider and gives the same text on every machine, and `ToString/0` reads the current culture. The two share a name, so a name-only list would approve both. The same split binds `Parse`, `TryParse`, and `Compare`.

D-208 extends D-207, and D-207 stands: the type list still binds every type name, and the member list binds each member of an approved type.

Two corrections came from a run and not from the finding. A named argument carries a containing type and is not a member of it, so `useUserOverride:` reported a false member finding until the rule read a method, a property, a field, and an event alone. `Object.GetType` also joined the member denylist, so it keeps the `L-REFLECTION` id instead of the wider `L-MEMBER` one.

Regression check: `AMemberOfAnApprovedTypeNeedsItsOwnEntry` covers the three triggers and the one-argument `CultureInfo` constructor. `TheOverloadArityIsPartOfTheEntry` covers `ToString/0` against `ToString/2`. `AnApprovedMemberIsNotAFinding` guards the other side with every member that Core uses and a member of a Core type.

An adversarial run against the real Core tree reported all six planted uses and stayed silent on `CultureInfo.InvariantCulture` and `float.IsNegative` in the same file.

## P2-6, fourth pass: the count, the row, and the volatile values

Disposition: full merit on all three points.

The review names three defects in the description, and each one holds.

- The decision section said that three decisions came from a review, and D-204 to D-207 are four. One row grouped D-205 and D-206, and the count read the rows and not the ids.
- The P2-6 row said that the description names the current session, and the description names none. The row described a correction that the description no longer contained.
- The summary named the pass count and the newest review head, and this review made both stale again.

Correction: the decision table lists every id with the finding that produced it, and the sentence above it counts four from the plan and four from reviews. The P2-6 row states the correction that the description holds. The summary names the effective head alone, which the next review does not change unless the head changes.

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
