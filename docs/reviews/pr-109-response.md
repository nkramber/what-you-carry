# PR-109 review response

Date: 2026-09-26

Author: Claude Code. Review: `docs/reviews/pr-109.md`, round 1 at `6b327c9`, verdict `Changes required`.

## P2-1: The reader accepts a CRC field before later header fields

Disposition: full merit.

Evidence: at `6b327c9`, `RunRecord.CheckHeaderCrc` found the last `,"headerCrc":` marker and compared the CRC-32 of the bytes before it with the stored value. It never checked that the field ends the line, and `ContentValidator.Check` accepts any field order. A header with `headerCrc` before `seed`, and the CRC of the bytes before that field, read as a valid header with any seed after it (D-637).

Correction: `69c946a`. `WhatYouCarry.Core/Replay/RunRecord.cs` requires the bytes after the marker to be the CRC digits and the closing brace alone. A field after the CRC, or any other byte, is a content error that names the header and the field `headerCrc`.

Regression check: `ReplayTests.ACrcFieldBeforeAnotherFieldIsAnError` builds a header with the CRC field before `seed`, with the CRC of the bytes before it, and a changed seed and an unchanged seed after it. Both rows failed on the reader of `6b327c9` and pass on `69c946a`. The same test rejects a space before the closing brace. `EachFlippedBitOfTheHeaderIsAnError` stays green. `det-lint` gives 0 findings, and the bit-identity answer stays `e202e84e0f5c188a`, because the writer did not change.

## New ids

No new D-# or F-# id. The correction restores D-637 as written.

## PR head

The correction commit is `69c946a`. The handoff commit of session 278 follows it.
