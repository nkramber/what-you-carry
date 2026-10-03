# PR-133 response

Date: 2026-10-03. The author of PR #133 answers the review record `docs/reviews/pr-133.md` of round 1, at the effective head `5cb1b0c`.

## P2-1: A nearby enemy can hold the clearer at the stairwell

Disposition: full merit.

Evidence: the strike in reach had no stall drop. `NearestLiving` reads the feet distance against the reach, and the blade meets a box inside the height band of the weapon alone, 50 to 170 centimeters over the feet for the basic sword (`MeleeWeapon.WedgeHits`). A run with harmless weapons shows the hold with no geometry: before the correction, the full clearer on seed 6 strikes the first enemies of floor 1 until the timer expires, and the run reads a softlock at tick 10800. The hold existed in the hunt before this PR, and D-774 widened it to the walk out.

Correction: `WhatYouCarry.Core/Bots/FullClearer.cs`. A strike that takes no health off an enemy for `StalledTicks` ticks, ten seconds, drops that enemy from the strike and from the hunt for the rest of the floor (D-776, F-209). The count runs across the enemies in reach, and a hit that lands starts it again. A count for each enemy alone never ended on floor 1 of seed 6, because two enemies in reach traded the place of the nearest one from tick to tick. The policy exposes the dropped enemies as `Unhurt`, and `Dropped` holds them too.

Regression check: `FullClearerDropsAnEnemyThatTheStrikesDoNotHurt` plays seed 6 with a content set whose every weapon deals no damage. On the correction it ends at the bottom with 15 floors. On the policy of `5cb1b0c` it fails with "Seed 6 with harmless weapons: the full clearer ended as Softlock on floor 1 after 10800 ticks". The test of the reviewer, an enemy inside the reach and outside the band on a hand-built floor, has no route through the loop: `SimulationLoop` digs its floor from the seed, and no test places an enemy. The harmless weapons reproduce the same hold with no geometry.

Other checks: the full suite passed 2191 tests. `det-lint`, `asset-qa`, and `ste-check` report 0 findings. The local sweep of the full clearer over seeds 1 to 5000 and 9001 to 10000 read 1703 bottoms, 4297 deaths, 0 softlocks, and 0 crashes, the same counts as before the correction. `FullClearerStrikesTheEnemyInReachWhileItLeaves` still passes.

## The verdict note on D-774 and D-775

The record asks the owner to confirm D-774 and D-775 before approval. Under D-644 the night fixer answers each owner question itself, records it with the mark "Session answer under D-644, for the owner to confirm", and lists each such decision in the notice at the end. The owner confirms at the merge. D-776 takes the same mark.

## New ids

- D-776: a strike that hurts nothing drops its enemy.
- F-209: the hold of the strike on an enemy that no swing hurts.

## Final PR head

The commit that carries this file and the correction, on `fix/night-37122879232`. The handoff entry of Session 333 names the push.
