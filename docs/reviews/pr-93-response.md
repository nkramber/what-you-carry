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

## Round 2 at `4e850b9`: P2-1, an unsupported finding severity can pass as nonblocking

Disposition: full merit.

Evidence: the finding heading pattern accepted each digit from 0 to 9, and the outcome rule counts P0 to P2 as blocking. An open `P4-1` under an approving verdict therefore passed. The same class has two more silent paths: a heading such as `### P10-1:` or `### P1: title` did not match the pattern, and the parser skipped it with no error (T-2).

Correction: `WhatYouCarry.Tools/CodexReview/ReviewFindings.cs`. A severity outside P0 to P3 is a fault that names the finding. Each other `###` heading in the Findings section is a fault that names the heading. The severity pattern takes one to three digits, so no parse can overflow. `findings.md` of `pr-review` states the rule. P3 stays the one nonblocking severity.

Regression check: `AFindingHeadingOutsideTheFormatIsAFault` for `P4-1`, `P10-1`, `### P1:`, and `### Notes`. On the old parser, all four failed (4 of 4). With the correction, `dotnet test --filter FullyQualifiedName~CodexReview` passes 61 of 61, and the P3 approval case still passes.

## The automated pass after round 1

Gitar found at `2dca4fe` that `codex login status` writes its status to stderr, and the command read stdout alone, so each round refused. `4e850b9` reads both streams, and `TheLoginStatusReadsTheStderrOfTheCli` starts a real child that writes to stderr. The thread has its reply and is resolved.

## The automated pass after round 2

- At `7159ad2`, Gitar found that a `#### P1-1:` or `###P1-1:` heading still passed the guard with no fault. Full merit, the same class as P2-1. The guard now faults on each line that starts with `###`, and two more cases of `AFindingHeadingOutsideTheFormatIsAFault` cover it. On the parser of `7159ad2`, both cases failed.
- Gitar found that one line of the PR comments section of `pr-93.md` says that round 2 approved, and the verdict of round 2 is `Changes required`. The claim is correct. The review record belongs to the reviewing provider (D-101, D-182), so the author makes no edit, and the next round reads the thread and corrects the section.
- `smoke-linux-x64` failed at `7159ad2` with exit 134 after the session ended, on a Godot shutdown error ("Leaked unsafe reference to object: ArrayMesh"). The PR changes no Game code, and the same job passed at `b7623f4` and `4e850b9`. The author re-ran the failed job.

New ids: D-523, D-524. No new F-# id.
