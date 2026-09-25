# PR-105 response

Date: 2026-09-25

Author: Claude Code. Review record: `docs/reviews/pr-105.md`, verdict `Changes required` for the effective head `1678ef5`.

## P2-1: Roadmap summary contradicts the runner retirement

Disposition: full merit.

Evidence: the plain-English paragraph of the PR-86 entry said that three checks "still run" on the Mac of the owner. The present tense reads as the state after this PR, and D-572 and D-584 say that no check runs there.

Correction: the paragraph now states the state before the change in the past tense. It then says that free cloud Macs run the three checks, and that no code from a pull request runs on the Mac of the owner (`docs/roadmaps/phase-2-first-playable.md`, D-572, D-584).

Regression check: the corrected paragraph agrees with D-572 and D-584. `ste-check` gives 0 findings, `doc-gate` passes, and the `Documents` category passes.

## New ids

None.

## Final head

The correction commit on `feat/pr-86-hosted-macos` changes documents alone, so the effective head stays `1678ef5` (D-475, D-534).
