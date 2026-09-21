# PR-84 review response

Date: 2026-09-21

Author: Claude Code

## Identity

- PR: 84
- Review record: `docs/reviews/pr-84.md`
- Verdict of that record: `Changes required` for head `f0005998c23f4f45c832d1ae2a3fed90f09a8313`
- Effective head at this response: the commit that adds `AFloorWithNoPostSkipsEveryWave`. The handoff entry of Session 198 names its hash.

The response commit adds one test to `WhatYouCarry.Tests/TimerTests.cs`. That path lies outside the metadata set, so the effective head moves (D-184). No Core, Game, Tools, or content file changed.

## P1-1: Empty floors crash at the first escalation wave

Disposition: partial merit.

The crash claim has no merit. The trigger does not reproduce:

- `Escalation.TryNextPost` holds both modulo expressions inside `for (int step = 0; step < posts.Count; step++)`. For an empty list the loop body never runs, so no division by zero can happen. The method then gives false.
- `Escalation.Step` then breaks out of its spawn loop and returns `new WaveSpawns(this.Waves, chosen, count - chosen.Count)`. That result holds the wave number, no post, and every spawn of the wave as skipped. This is the contract of D-410 and D-418.
- The record states this itself: "The loop condition does not run for an empty list". The only failure path that it names is a future call, and no such call exists in this PR.
- Exit test 5 already ran the path. `TimerTesterAlwaysDies` plays 1000 seeds of the peaceful set, whose floors hold no post. The first wave is due 1800 ticks after expiry, at tick 12600. A scratch run of 20 seeds on 2026-09-20 had four runs past that tick: 13549, 13184, 13039, and 12763 ticks. Each ended as a death with the cause `overseer`, and none as a crash.

The coverage gap has merit. No test called `Escalation.Step` with an empty post list at a due wave, so the contract had no direct check.

Correction: the new test `AFloorWithNoPostSkipsEveryWave` in `WhatYouCarry.Tests/TimerTests.cs` asserts the contract in two parts:

- A unit part calls `Escalation.Step` with an empty list at three due waves. It asserts the wave numbers 1, 2, and 3, no post, the skipped counts 1, 2, and 3, and a cursor of zero.
- A loop part plays seed 1 of the peaceful set with a timer of 1 second past the first wave. It asserts no enemy, no end of the run, and the two events of the tick: `Wave` with a count of 0 and `WaveSkip` with a count of 1.

No Core change follows. An explicit empty-list return would repeat what the loop condition already does, and the contract names no other behavior (T-1).

Regression check: the new test passed on the unchanged Core code, 1 of 1. That result proves that the trigger does not crash. The focused suite `TimerTests` then passed, 17 of 17.

## The broad suite

The record states that the local broad run gave no result during the review window. The broad suite has these results:

- At `f000599`, the three CI legs of the build-and-test workflow ran `dotnet test` with the filter `Category!=Smoke`, and all three passed: Linux in 23 minutes, macOS in 8 minutes, and Windows in 24 minutes. The `smoke` workflow passed on all three platforms.
- At the response head, the author ran the full local suite with the Smoke category: 1208 passed, 0 failed, in 8 minutes 39 seconds.

## New ids

None. No D-#, F-#, or OQ-# id is new in this response.

## Final PR head

The handoff entry of Session 198 names the remote head after the push (D-199).
