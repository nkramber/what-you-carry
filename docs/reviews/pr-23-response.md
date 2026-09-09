# PR-23 response

Date: 2026-09-09

## Identity

- PR: 23
- Reviewed head: `e6e89aa`
- Response head: `4e98470`
- Branch: `feat/pr-8-camera`

## Push check

`git fetch` and `git status --short --branch` showed the checkout level with the remote at the review commit `c056848`, so no push came first (F-59).

## P2-1: The bit-identity camera fold does not replay the record

Disposition: full merit.

Evidence: `BitIdentitySweep.AddCamera` built a `SimulationLoop` of its own and stepped the intent list, and `AddReplay` folded the end hash of the replay alone. PR-8 exit test 5 names a replay of a record with camera motion, and no camera value came from the replay traversal.

Correction: `WhatYouCarry.Core/Replay/IReplayObserver.cs` declares `AfterTick(SimulationLoop loop)`, and the replay calls it after each complete frame. `RunReplayer.Replay` gains an overload with the observer, and the five-argument overload passes a silent one, so the twelve callers stand. The sweep folds the camera pose and the aim ray of every replayed tick through a `CameraFold` observer, beside the end hash and the CRC-32. `AddCamera` and the second loop are gone. The sweep and a test are the two concrete callers of the interface (D-111).

The sweep hash moved from `92ef27ee175b3e7e` to `afed0063a6cf8a50`, because the fold order changed. The simulation version stays 3, because no simulation number changed (G-20).

Regression checks:

- `TheReplayVisitsEveryCompleteFrame`: the observer sees ticks 1 to 6 for a whole record of six frames, 1 to 5 for the same record cut inside its last frame, and nothing for a header alone. Passed.
- `AimRayIsDeterministic`: over one hundred seeds, the replay folds the camera and the aim ray of every tick through the observer, and the fold equals the live fold. Passed.
- `TheSweepReadsEveryFunctionAndStream`: the sweep names `: IReplayObserver` and holds no `new SimulationLoop(`. The old sweep fails the second assertion. Passed on the new sweep.
- `BitIdentityKnownAnswer`: `afed0063a6cf8a50` on macOS arm64. The three-platform job runs on the push.

Checks on `4e98470`: `dotnet build` 0 warnings, `dotnet test` 426 tests and 0 failures, `det-lint` 0 findings in 43 Core files, `ste-check` 0 findings in 15 files.

## New ids

F-85 records the finding. No new decision and no new question.

## Final head

`4e98470` is the effective head. The commit that holds this file and the Session 73 entry is a metadata commit (D-184).
