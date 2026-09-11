# PR-49 response

Date: 2026-09-11

## Identity

- PR: 49
- Reviewed head: `91d1b6f`
- Branch: `feat/pr-12-game-skeleton`

## Push check

The review commits `42bd504`, `307f025`, and `735f7fd` came from the other checkout and were on the remote. `git status --short --branch` showed this checkout behind the remote by those three commits, and a fast-forward pull brought them in. No push came first (F-59).

## P2-1: A held controller stick overrides the last mouse look device

Disposition: full merit.

Evidence: `Read` polled the look stick on each tick and set the controller flag when the deflection was past the dead zone. `AddMouseMotion` set the flag false, and the next `Read` set it true again from the held stick. D-243 names the device that gave the look input, and its effect names a player who changes the device. The mouse event was the latest look input on that tick, and the frame set the controller aim bit.

Correction: the look device follows the latest look event. `Main._UnhandledInput` hands a mouse motion event and a joypad motion event to the reader as plain values. `InputReader.AddMouseMotion` names the mouse, and the new `InputReader.AddLookStickMotion` names the controller when the event moves a look axis of the first controller past the dead zone. `Read` reads the stick deflection and never the device. A held stick sends no new event, so a later mouse motion takes the look, and the stick takes it back when it moves again.

The reader test needs a reader that runs with no engine, so the reader now polls through `IInputPoll`: `EnginePoll` over the engine singleton, and a test poll in `InputReaderTests`. Those are the two callers that D-111 asks for, as for `ILogSink` and `IContentSource` (D-211, D-219). The seam is the smallest change that makes the regression test possible, and it lets the tests pin every D-289 binding with a bit.

Regression check: `HeldStickThenMouseMotionReadsTheMouse` holds the right stick at 0.8 in the poll, sends one stick event and then one mouse event, and asserts that the next two frames name the mouse. With the old poll-based flag put back in `Read` and the solution built, `dotnet test WhatYouCarry.slnx --no-build --filter "FullyQualifiedName~InputReaderTests"` fails that one test with `Assert.False() Failure`, 1 failed and 7 passed. With the correction, the same command passes 8 tests. The seven other reader tests cover the stick event that takes the look back, the events that change nothing, every keyboard and controller binding of D-289 with a bit, the opposed keys, the trigger press point, and the mouse sum per tick.

## New ids

No new decision, no new question, and no new finding. The review record holds P2-1.

## Final head

The correction is the one commit above the review commits. It holds `Input/IInputPoll.cs`, `Input/EnginePoll.cs`, `Input/InputReader.cs`, two lines of `Main.cs`, `InputReaderTests.cs`, this response, the Session 127 handoff entry, and the move of Sessions 117 and 116 to the archive. It is the effective head.
