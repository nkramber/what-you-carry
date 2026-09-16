# PR-78 review response

Date: 2026-09-16

This file answers the review of `3eb7142` in `docs/reviews/pr-78.md`.

## P2-1: The register lookup command searches fixed decision ids

Disposition: full merit.

Evidence: at `3eb7142`, `AGENTS.md` lines 20 to 28 held the ids D-146, D-375, OQ-9, and OQ-44 in the three patterns. No sentence told the reader to replace them. A draft of the sentence had the words "with the ids in place of the examples", and the size cut of the agent file removed them.

Correction:

- The command now starts with `d='146|375'; q='9|44'`, and the three patterns read `$d` and `$q` in double quotes.
- The sentence before the command tells the reader to replace the example numbers with every D-# and OQ-# number of the task (D-378).
- `AGENTS.md` and `CLAUDE.md` stay identical. They hold 14591 bytes, 2 fewer than before PR #78, because two sentences on the archive are shorter. The archive rule stays in the handoff header and in D-379.

Regression check:

- `RegisterLookupTests` reads the command from `AGENTS.md` and applies its patterns to the committed registers.
- `LookupCommandTakesTheIdsOfTheTask`: the first line sets `d` and `q`, and no pattern holds a literal id list.
- `LookupCommandFindsTheRowsOfNewDecisionsAndQuestions`: with D-379 to D-382 and OQ-9 and OQ-44, the command prints each of the six rows.
- `LookupCommandFindsEachRevision`: with D-187 and D-374, the revision line prints D-377 and D-381, the decisions that revise them.
- The three tests pass on the correction. With the `AGENTS.md` of `3eb7142`, all three fail.
- The documented command ran in bash and in zsh with D-379 to D-382 and OQ-9 and OQ-44. It printed the four decision rows, the revision lines of D-374 and D-381, and the two question rows.

## New ids

None. D-378 already requires the lookup of every id of the task, and the correction makes the command agree with it.

## Final head

The correction commit holds `AGENTS.md`, `CLAUDE.md`, `WhatYouCarry.Tests/RegisterLookupTests.cs`, this file, and the Session 184 handoff entry. It changes paths outside the metadata set, so it is the new effective head (D-184). The handoff entry names it.
