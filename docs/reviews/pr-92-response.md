# PR-92 review response

Date: 2026-09-23

Author: Claude Code. Review: `docs/reviews/pr-92.md`, verdict `Changes required` at effective head `665457179a90b47931725292dbd2c960d28a089c`.

## Start

`git fetch` showed the review commit `48b5689` on `origin/feat/pr-62-texture-recipes`. The checkout pulled it with a fast forward. The checkout was not ahead of the remote.

## P2-1: Layout bounds can wrap and accept an invalid canvas

Disposition: full merit.

Reproduction: two new cases of `RecipeTests.LayoutRejectsABadFile` parse a layout with `at` of `[2147483647, 1, 1, 32]` for a block and `[1, 2147483647, 4, 1]` for a face. Both cases failed on the old code: the parse accepted each rectangle, because `x + width` and `y + height` wrap to a negative value.

Correction: `WhatYouCarry.Assets/TextureLayout.cs` compares each size with the room that the start leaves: `width <= 512 - x` and `height <= 512 - y`. No sum of two large values remains in the check (G-7, T-2, D-506).

The same wrap stood in the second check of D-506, the packer. `AtlasPacker.Pack` added the gutter to a canvas size before its bounds check, so a canvas of `int.MaxValue` texels wrapped to a negative cell. `WhatYouCarry.Tools/TextureGen/AtlasPacker.cs` now rejects a size past the atlas room before any sum. The new theory `RecipeTests.PackerRejectsACanvasLargerThanTheAtlas` failed on the old code for both large sizes. The correction stays inside the atlas bounds rule of D-506, which the finding names.

Regression checks:

- The four new failing cases pass after the correction, and the case of a width of 511 passes before and after.
- `RecipeTests` and `TextureGenTests` passed 100 of 100. The committed atlas and layout equal the generator output, so the packer places every canvas as before.
- The full suite passed 1436 of 1436 after the correction, the Smoke category included.

## Exit test 6

The owner confirmed that the contact sheet keeps the look: D-510.

## Other notes of the review

- The review states that the local full suite stalled. The author ran the full suite before the hand-over (1431 of 1431) and after this correction (1436 of 1436). Both runs completed in about six minutes.

## New ids

- D-510: the owner confirms exit test 6 of PR-62.

## Final head

The commit that holds this file, on `origin/feat/pr-62-texture-recipes`. It holds the correction, so it is the new effective head.
