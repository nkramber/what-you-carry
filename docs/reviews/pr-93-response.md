# PR-93 review response

Date: 2026-09-23

Author: Claude Code. Review: `docs/reviews/pr-93.md`, round 1 at `b7623f4`, verdict `Changes required`.

## P1-1: Approval accepts an open blocking finding

Disposition: full merit.

Evidence: `ReviewOutcomeRules.Judge` returned `Approve` on the verdict name before it read the severity of the open findings. `review-record.md` gives `Ready for owner merge` only when no blocking finding remains, and a P2 with an owner disposition carries the status `accepted risk`, not `open`.

Correction: `WhatYouCarry.Tools/CodexReview/ReviewOutcome.cs`. An approving record with an open P0, P1, or P2 finding is a fault (exit 1) that names each such finding. An open P3 finding still passes, and an `accepted risk` status still passes.

Regression check: `AnApprovalWithAnOpenBlockingFindingIsAFault` for P0-1, P1-1, and P2-1, and `AnApprovalWithAnAcceptedRiskApproves`. On the old `ReviewOutcome.cs`, the three cases of the first test failed (3 of 3). With the correction, `dotnet test --filter FullyQualifiedName~CodexReview` passes 56 of 56.

Scope note: the finding also says that `review-gate` accepts the same record. The correction keeps to the command, the contract that this finding names. `review-gate` reads the three machine-read parts alone (D-179, D-514), and a change of its rules is not in the scope of PR-78.

## Other changes in this round

The owner gave two new rules during round 1. They are in the same push, so round 2 reviews them:

- D-523: no review uses API pricing. The command removes `OPENAI_API_KEY`, `CODEX_API_KEY`, and `CODEX_ACCESS_TOKEN` from each Codex process, passes `forced_login_method="chatgpt"`, and refuses unless `codex login status` gives `Logged in using ChatGPT`.
- D-524: before a merge, the session gives the owner a summary of one paragraph and gets the merge confirmation.

New ids: D-523, D-524. No new F-# id.
