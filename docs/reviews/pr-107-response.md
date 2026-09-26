# PR-107 review response

Date: 2026-09-26

Author: Claude Code. Review: `docs/reviews/pr-107.md`, round 1 at effective head `e4a097e`, verdict `Changes required`.

## P2-1: Blend accepts a zero part count

Disposition: full merit.

Evidence: at `e4a097e`, `LinearLight.Blend(0, 255, 0, 0)` passed the weight check, because a weight of 0 is not past 0 parts. The division by `parts` then threw `DivideByZeroException`, an error with no context (T-2).

Correction: commit `6afe1db`. `WhatYouCarry.Tools/TextureGen/LinearLight.cs` rejects a count of parts that is not positive with `ArgumentOutOfRangeException` before the weight check and the division. The message names the count.

Regression check: `TextureGenTests.LinearLightFollowsTheTransferFunction` now calls `Blend` with 0 parts and with -1 parts, and it expects `ArgumentOutOfRangeException` with the count in the message. It also checks both endpoints of a blend. On the old `LinearLight.cs`, the test failed with `DivideByZeroException`. With the correction, the full suite passed 1826 of 1826.

## New ids

None.

## Final head

Commit `6afe1db` is the new effective head. The handoff entry of Session 268 names it.
