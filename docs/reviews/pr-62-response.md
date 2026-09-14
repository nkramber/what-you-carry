# PR-62 review response

Date: 2026-09-13

This file answers `docs/reviews/pr-62.md` for PR #62, roadmap item PR-15. The review read the effective head `6e35bc5` and gave the verdict `Changes required` with one finding, P2-1.

## P2-1: Weapon asset paths accept invalid locations and file types

Disposition: full merit.

Evidence: the trigger reproduced before the correction. Nine regression cases went into `ContentTests.AWeaponOutsideItsBoundsIsAnError` first, and all nine failed against the validator of `6e35bc5`, with 16 other cases passed:

- `models/../floors/a.json` as the model, and `models/../player.w.json` as the animation: a '..' segment.
- `models/player.bbmodel` as the animation, and `models/player.w.json` as the model: the extension of the other kind.
- `models/./w.bbmodel` and `models//w.bbmodel`: a '.' segment and an empty segment.
- `models/a\..\w.bbmodel`: a backslash between segments.
- `models/.bbmodel`: a file name with no name before the extension.
- `models/w.BBMODEL`: the extension in another case.

`WeaponDefinition.AssetPath` read the `models/` prefix alone, so each value passed.

Correction:

- `WhatYouCarry.Core/Content/WeaponDefinition.cs`: `AssetPath` takes the extension of its kind. It rejects a backslash, an empty, '.', or '..' segment, and a file name that does not end in the extension after a name. Each error names the field (D-219, D-298, D-302, D-334, T-2). The check reads the string with `StartsWith`, `Length`, and the indexer alone, so Core needs no new allowlist entry (D-207, D-208).
- `WhatYouCarry.Core/Content/ContentLoader.cs`: `ModelExtension` and `AnimationExtension` join `ModelDirectory` in Core. `WhatYouCarry.Assets/AssetPaths.cs` reads them, so Core and Assets share one value for each extension.
- `WhatYouCarry.Tests/ContentTests.cs`: the nine cases above, and `AWeaponAssetPathUnderTheModelDirectoryLoads`, which keeps a path in a subdirectory of `models/` valid for both fields.

Regression check: after the correction, the 25 weapon cases of `ContentTests` passed, and the full suite passed 855 tests with the five Smoke tests on the local Godot build, with 0 failures. `dotnet build` gives 0 warnings and 0 errors. `det-lint` gives 0 findings, Core 0 in 65 files and Game 0 in 32 files. `asset-qa` gives 0 findings, and `ste-check` gives 0 findings in 16 files.

## New ids

None. No decision, question, or finding register entry changed.

## Final head

The correction commit holds this file, the corrected files, and the Session 154 handoff entry (D-182). That commit is the effective head for the repeat review.
