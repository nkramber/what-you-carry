# PR-58 review response

Date: 2026-09-12

## Identity

- PR: 58
- Review: `docs/reviews/pr-58.md`, verdict `Changes required` for head `3dfbf03`.
- Response head: the commit that holds this file, on `feat/pr-60-fullscreen-test-exit`.

## P2-1: Exit tests do not verify session termination

Disposition: full merit.

Evidence: at `3dfbf03`, `EscapeEndsTheSession` and `StartButtonEndsTheSession` assert the two constants and the return value of `TestExit.IsPressed`, and nothing drives `Main`. Exit tests 2 and 3 of the Phase 2 roadmap ask for a session that ends with exit code 0 and an end line (D-311). A `Main` that never reads the exit passes both tests.

Correction: an engine-level test through the smoke harness, with one scripted press.

- `WhatYouCarry.Game/Input/TestExit.cs` gains the `--press` flag. The two arguments after it name the input, `escape` or `start`, and the tick. `PressOf` reads them into a `ScriptedPress`, and an absent argument, an unknown name, or a tick that is not a whole number is a `ContextException` that names the cause (T-2). `EventOf` builds the engine event of the press: the Escape key down, or the Start button of the first controller down.
- `WhatYouCarry.Game/Main.cs` reads the flag at boot. At the tick of the press it gives the event to the input singleton of the engine. The engine holds the input down from the next frame, and the poll of the next tick reads it as any real press. The exit path itself did not change.
- `WhatYouCarry.Tests/SmokeSessionTests.cs` gains `EscapeEndsTheSession` and `StartButtonEndsTheSession` in the Smoke category. Each runs the headless smoke session with a press at tick 100 and asserts exit code 0, no error line, no smoke end line, and one test exit line at a tick past the press and before the end of the script. The smoke workflow runs the category on the three platforms.
- `WhatYouCarry.Tests/TestExitTests.cs` keeps the negative coverage as exit test 4. The two unit tests of the predicate are `EscapePressesTheExit` and `StartButtonPressesTheExit` now, so no unit test carries the name of an engine test. Two new tests cover the flag parse and its errors.
- `CLAUDE.md`, `AGENTS.md`, and the PR-60 entry of the Phase 2 roadmap name the flag and the test exit session command.

Regression check: the two engine tests against the Game code of `3dfbf03`.

- With `Main.cs` and `TestExit.cs` from `3dfbf03` and the new tests: 2 failed. The old code ignores the flag, the smoke session runs to its own end line at tick 1000, and the assertion on the absent smoke end line fails.
- With the correction: 2 passed. The Escape press at tick 100 ends the session at tick 101, and the Start press ends it at tick 101.
- A manual run with `--press enter 100` ends at boot with exit code 1 and the error `The press flag names no input of the test exit. The names are escape and start. [argument=enter]`.

## Verification

- `dotnet build WhatYouCarry.slnx`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build`: 769 tests, 0 failures, with the four Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files.
- `ste-check`: 0 findings in 15 files.
- Core and content did not change, so `bit-identity` and `asset-qa` stay as the review recorded them.

## New ids

None. The correction adds no decision, no finding, and no question.
