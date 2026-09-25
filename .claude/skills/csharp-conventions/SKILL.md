---
name: csharp-conventions
description: The code rules of this repository. Load before you write or review C# in Core, Game, Assets, Tools, or Tests. It holds the language rules, the project boundaries, the determinism rules, the content rules, the test rules, and the allowlist procedure.
---

# C# conventions skill

Load this skill before you write or review code. The tenets win over every rule here (`AGENTS.md`). T-1 asks for readable and simple code, T-2 for zero silent failures, and T-3 for tests that cover everything.

## Language and build

- C# only, tools included. Do not use GDScript (D-64, D-65).
- Nullable reference types on, and warnings as errors (D-68).
- Each project file names its target framework. The Godot editor writes `net8.0` into a project file that has none.
- Every dependency needs a decision entry (G-16).

## The project boundaries

| Project | Rule |
|---|---|
| `WhatYouCarry.Core` | No engine dependency (G-1). It holds the simulation. |
| `WhatYouCarry.Assets` | No engine dependency (D-299). It holds the model reader, the animation reader, and the pose math. Game and Tools read a model through it. |
| `WhatYouCarry.Game` | The engine layer. Godot physics and navigation never feed the simulation (G-3). |
| `WhatYouCarry.Tools` | The command line tools. It needs no engine. |
| `WhatYouCarry.Tests` | xUnit. The Smoke category needs the Godot binary. |

A Core type never reads a Game type. Trace the data flow, and not the import list alone. A value that comes from Godot physics or navigation never enters a tick.

## Determinism

- No `System.Math` transcendentals, no `Vector<T>`, no SIMD, and no reflection in Core. Use `DetMath` (G-2).
- float in Core (D-70).
- The simulation runs on one thread at 60 Hz (D-72, D-73).
- A Core behavior change raises the simulation version (D-151, G-20). The bit-identity sweep then takes a new known answer.
- Iteration order and event order stay stable. A dictionary walk that the result reads needs a sorted key order.
- Each seed has one owner. A new random stream needs a decision.

The `det-lint` command reads Core for the determinism rules and Game for the string rule (D-222).

```
dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -- det-lint --root .
```

## The Core allowlist

Core approves each type and member that it uses outside this project, by name, and a method by the types of its parameters (D-207, D-208, D-582). A new entry needs a decision.

Verify each new entry before the PR opens:

1. Remove the entry from the allowlist.
2. Run `det-lint`.
3. Check that a finding names the member.
4. Put the entry back.

A dead entry widens the boundary in silence, and no check finds it later.

## Errors

- No empty catch blocks (T-2).
- An absent value is an error, and never a zero.
- Every error carries its context: the file, the field, and the reason (D-113).
- Assertions stay on in shipped builds (D-112).
- A fallback that hides a failure is a defect, also when the fallback value looks safe.

## Content and strings

- JSON for all content, and a schema validates it. No `.tres` files (D-91, D-92).
- A load failure names the file, the field, and the reason.
- All strings that the player sees live in the string table (D-98). The `det-lint` command reads Game for this rule.
- An identifier reference and a file name case are part of the content check. The `asset-qa` command reads them (D-135, D-300, D-301, D-302).

## Tests

- No merge without tests (T-3).
- A bug fix ships with a regression test that fails on the old code.
- xUnit. A property test is a seed loop, and each failure names its seed (D-66).
- A test asserts against the contract, and not against a copy of the implementation.
- A test that can pass without the intended behavior is not a test.

## Shape and cost

- Explicit over implicit. A fresh model understands a function from the function and its helper signatures (T-1).
- Helpers go one level deep (D-110).
- Two concrete cases come before any abstraction (D-111).
- No clever one-liners.
- Tune only on measurement (D-109). Every optimization needs a profile before the change and a measurement after it (G-17).

## Before the push

Run the build and the test suite from the checkout root.

```
dotnet build WhatYouCarry.slnx
dotnet test WhatYouCarry.slnx --no-build
```

A local `dotnet test` needs the Godot build for the Smoke category, at the path of `AGENTS.md` or at the path that `WYC_GODOT` names. The `ci.yml` workflow runs the suite outside `Smoke` in five jobs: on Linux and on Windows, one job for the category `Sweep` and one for the rest, and one job on the Mac (D-479). The `smoke` workflow runs the `Smoke` category with the pinned binary on each platform.

A push of documents alone, in the skip set of D-475, runs no full suite (D-491, D-492). It runs `ste-check`, `doc-gate`, and this command:

```
dotnet test WhatYouCarry.slnx --filter Category=Documents
```

A push that holds a path outside the skip set runs the full suite, also when it holds documents too (D-493).
