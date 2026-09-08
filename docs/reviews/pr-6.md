# PR-6 review

Date: 2026-09-08

## Identity

- PR: 6
- Target: `main`
- Base: `546a70a16c0e9a0d24b3b1a7eb2134f2fe6233ba`
- Merge base: `546a70a16c0e9a0d24b3b1a7eb2134f2fe6233ba`
- Head: `03e6a298e8c1f5d982c9e2af5618815de04ef72d`
- Branch: `feat/pr-1-scaffold`

## Provider gate

The PR author provider is Claude Code, as stated in session 17 of the session handoff. The reviewer provider is Codex. The providers differ, so the gate passes under T-4 and D-101.

## Intended behavior and scope

The PR creates the four-project .NET scaffold, three-platform CI, and the review-gate workflow. This repeat review inspected the response, the diff since `39db1f9`, the review-gate command and tests, the CI workflows, the project files, the roadmap, the decisions, and the questions register. The Core boundary, CI inputs, dependency records, document rules, and review-gate behavior are in scope. Visual behavior is out of scope because the PR adds no scene or player-facing content.

## Findings

### P1-1: The review gate executes evaluator code from the PR head with check-write permission

Status: fixed in `9624cfa`.

File: `.github/workflows/review-gate.yml:9-12,53-59` and `WhatYouCarry.Tools/ReviewGate/ReviewGateCommand.cs:84-87`.

Trigger: A same-repository PR changes `WhatYouCarry.Tools` or the workflow, then the `pull_request` job runs that head code with `checks: write`.

Expected: The review gate must enforce T-4 and D-101 through a trusted evaluator. A PR must not control the code that decides whether its own review passes.

Actual: The revised workflow runs on `pull_request_target`, checks out the base branch, fetches the PR head as a git object, and runs the base-branch `WhatYouCarry.Tools` project. It does not check out the PR head.

Consequence: The old workflow allowed a PR to bypass the required cross-provider review. The revised workflow does not execute PR-controlled evaluator code.

Correction: The workflow runs the evaluator from the trusted base branch. The PR head remains data only. `ReviewGateRunsOnPullRequestTarget` checks the event and fetch behavior.

Regression check: `dotnet test WhatYouCarry.slnx --no-build -m:1` passed with 35 tests. The live adversarial proof waits until PR #6 merges because `pull_request_target` runs only from the default branch (F-58).

### P1-2: A PR author can replace an approved review record in a metadata-only commit

Status: accepted risk, D-198.

File: `WhatYouCarry.Tools/ReviewGate/ReviewGateRules.cs:28-29,124-166`, `WhatYouCarry.Tools/ReviewGate/ReviewGateFacts.cs:44-46`, and `WhatYouCarry.Tests/ReviewGateGitTests.cs:35-50`.

Trigger: A reviewer approves an effective head. The PR author then commits a new `docs/reviews/pr-6.md` with an approved verdict and the same effective head.

Expected: A review record must prove that the other provider approved the effective head. D-101 requires one review file per PR. D-184 permits metadata commits without changing the effective head, but it does not permit the PR author to replace the approval.

Actual: The gate reads the review file from the current PR head. It excludes `docs/reviews/` when it computes the effective head. The later metadata commit therefore keeps the old effective head while replacing the text that the gate trusts. The existing metadata test checks only that a review file added in a metadata commit passes.

Consequence: The PR author can self-approve the PR by editing the review record after the real review. The gate then reports success for an unreviewed revision. D-198 accepts this risk because the shared GitHub identity and unsigned commits cannot prove which provider wrote the record.

Correction: D-198 records the accepted risk. The check output names the commit that last changed the review file, including its subject. The owner reads that line before merge. A second identity or signing key remains a separate owner decision.

Regression check: `ReviewGateNamesARewriteOfTheReviewFile` and `ReviewGateNamesTheCommitThatChangedTheReviewFile` passed. They fail on the old code and name the rewrite commit.

## Verification

- `dotnet build WhatYouCarry.slnx -m:1`: passed, 0 warnings, 0 errors, revision `03e6a29`.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: passed, 35 tests, 0 failures, revision `03e6a29`.
- GitHub PR #6 status checks: Linux, Windows, and macOS CI passed at head `03e6a29`. No `review-gate` check runs on PR #6 because D-197 requires the workflow to come from the default branch, and `main` does not hold it before this PR merges.
- `Godot --headless --editor --path WhatYouCarry.Game --build-solutions --quit`: did not run because `Godot` is not on this checkout's command path.
- `git diff --check 546a70a16c0e9a0d24b3b1a7eb2134f2fe6233ba...03e6a298e8c1f5d982c9e2af5618815de04ef72d`: passed.
- Manual STE checklist: applied to this review record. Review records and handoffs are dated-record exemptions under the project skill.

## Open questions and accepted risks

OQ-12 remains open for PR-9. D-198 accepts the review-record identity risk. The STE checker, lint tool, and bit-identity job remain absent until PR-2 and PR-3, as named in the PR description.

## Verdict

**Ready for owner merge.** This verdict applies to head `03e6a298e8c1f5d982c9e2af5618815de04ef72d`. P1-1 is fixed in `9624cfa`. P1-2 remains an accepted risk under D-198, with the commit identity limitation and owner check documented.
