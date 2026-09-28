# PR-122 response

Date: 2026-09-28

Review record: `docs/reviews/pr-122.md`, verdict `Changes required` at the effective head `34c2ddf8c72702056b827dc552cd1ca9ebd262fa`.

## P2-1: A same-tick descent can build every enemy tree

Disposition: no merit.

The trigger is real in the code. `ChunkSwap.BeforeTick` can offer the plan on the tick that `loop.Step` descends. `EnemyNodes.Rebuild` then finds no staged tree, and `Grow` builds every tree in that tick. The correction that the finding asks for cannot meet its own conditions, and the race is a part of an older path that this PR does not change.

Evidence:

1. The same race makes the chunk swap build every chunk of the floor in the same tick. `ChunkSwap.AfterTick` builds each mesh that `UploadSome` did not reach (`WhatYouCarry.Game/World/ChunkSwap.cs`, the loop over `stagedMeshes` in `AfterTick`). A plan offered on the descent tick has `stagedChunk` at 0, so the tick builds up to 64 chunk meshes (D-291). The remark of `ChunkSwap` since PR-18 states this path: "A descent that comes before the task or the upload ends builds the rest of the chunks in that frame. The frame is then slow, and the frame log shows it." The tree cost is the small part of that tick. No change to the trees alone can put that tick under the budget of D-635.
2. The only way to build no tree in that tick is to show the enemies later. The enemies of the new floor then draw with no model for some frames. The finding asks to "keep the descent behavior and tree visibility correct", so that correction breaks its own condition. The Game also cannot hold the descent for the worker, because the task changes no tick (D-429).
3. The race needs the task to end on the descent tick itself. A `--transitions 6` session on the Mac at `3e01603` offered each plan 1 to 5 ticks after its swap: dig plus meshes of 25 to 90 ms. The six floors lasted 344 to 2118 ticks. The Deck was asleep at the time of this answer, so the Deck scale of the dig is not measured here. A dig three to five times slower still ends well before the shortest floor.
4. The remark of `EnemyNodes` already states the path, the same way as `ChunkSwap`: "A descent before the upload ends builds the rest of the trees in that tick."

Correction: none. The code stays at `34c2ddf`.

Regression check: none new. `SmokeReachesStairwell` covers the normal path, where the plan arrives before the descent: `digging:false`, `builtTrees:0`, and a nonzero `stagedTrees`. The Deck rows in `docs/reviews/pr-122-deck/swap-lines.txt` show seven staged trees and no built tree on each run of seed 2.

## New ids

None. No new D-# and no new F-#.

## Final head

The code head stays `34c2ddf8c72702056b827dc552cd1ca9ebd262fa`. This response is a metadata commit.
