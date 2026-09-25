# PR-102 response

Date: 2026-09-25

Author: Claude Code. The review record is `docs/reviews/pr-102.md`, round 1 at `69f530818aaecded6c23740880ca1db16a1e4f5d`.

## P2-1: Keep the repository audit prompts out of the night workflow PR

Disposition: full merit.

Evidence: `git diff origin/main...HEAD --name-status` lists `A docs/reviews/repository-review-prompts.md`. Commit `e5e164f` added it. That commit staged with `git add -A` in the shared checkout, and another session had put the untracked file there. The file is in neither `main` nor `fix/pr-88-review-fixes`. PR-85 never had it in scope (G-10).

Correction: the file leaves this PR with `git rm`. The history of the branch keeps its text at `e5e164f:docs/reviews/repository-review-prompts.md`, because the main checkout lost its copy when that checkout left this branch. The owner decides where it goes next.

Regression check: `git diff origin/main...HEAD --name-status` after the correction lists the night workflow, the gather script, the two test files, the design doc, the decision register, the roadmap, the runbook, the `review-and-merge.md` reference, the review record, this file, and the handoff files. Each path serves the night move or its records.

Prevention: this session now stages named paths alone. The handoff entry names the trap.

## New ids

None.

## Final head

The effective head stays `69f530818aaecded6c23740880ca1db16a1e4f5d`, because the correction changes `docs/reviews/` alone, which the skip set of D-475 holds.
