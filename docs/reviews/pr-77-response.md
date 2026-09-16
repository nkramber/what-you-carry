# PR-77 review response

Date: 2026-09-16

Review: `docs/reviews/pr-77.md`, verdict `Changes required` for `887f94f`.

## Push state at the start

`git fetch origin` and `git status --short --branch` showed the branch level with `origin/feat/one-pr-one-session` at `d781d62`, with no commit ahead (F-59). The review record and Session 179 were already on the remote.

## P2-1: The documents matrix accepts duplicate category lines

Disposition: partial merit. The duplicate part has full merit. The part about unknown labels has no merit.

Reproduction: the PR description, with the line `` - `docs/design.md`: Not applicable: no design changes affect this pull request. `` added at the end, passed `doc-gate` at `d781d62` with 0 problems over 18 changed paths. The diff changes `docs/design.md`, so the added line contradicts the first line.

Correction:

- `DocGateRules.CheckMatrix` reads every line for a category with `FindAll`. More than one line is a problem that names the count and the category. The gate then reads no disposition for that category, because it cannot know which line holds.
- `DuplicateMatrixLineFails` adds the line of the review to a complete fixture and requires the problem "2 lines for `docs/design.md`". The test fails on the old `Find` and passes with the correction.
- D-376, the enforcement table in `docs/design.md` section 3.14, and the `one-pr-one-session` skill now say exactly one line for each category. D-376 is new in this PR, so its text changes in place with no revision marker.

No merit, unknown labels: the review asks to reject an unknown label only if the section is an exact list. The categories of D-376 are a floor. A line for another document, such as `docs/archive/`, records more and makes no disposition ambiguous. A label with a typo does not escape the gate, because the real category then has no line, and the gate reports it.

Regression checks:

- `dotnet test WhatYouCarry.slnx --no-build --filter "FullyQualifiedName~DocGateTests"`: 15 passed, 0 failed.
- With `DocGateRules.cs` at the old code and the new test: `DuplicateMatrixLineFails` failed.
- `doc-gate` over the description with the duplicate line: fail, 1 problem. `doc-gate` over the current PR description: pass, 0 problems.
- `ste-check`: 0 findings in 18 files.

## New ids

None. D-376 changes in place.

## Final head

The commit that holds this file holds the correction, so it is the new effective head. The handoff entry of Session 180 names it after the push.
