# PR-6 review

Date: 2026-09-08

## Identity

- PR: 6
- Target: `main`
- Base: `546a70a16c0e9a0d24b3b1a7eb2134f2fe6233ba`
- Merge base: `546a70a16c0e9a0d24b3b1a7eb2134f2fe6233ba`
- Head: `39db1f974281203aeb5af8fa274a16a513125d45`
- Branch: `feat/pr-1-scaffold`

## Provider gate

The PR author provider is Claude Code, as stated in session 17 of the session handoff. The reviewer provider is Codex. The providers differ, so the gate passes under T-4 and D-101.

## Intended behavior and scope

The PR creates the four-project .NET scaffold, three-platform CI, and the review-gate workflow. I inspected the complete diff, the review-gate command and tests, the CI workflows, the project files, the roadmap, the decisions, and the questions register. The Core boundary, CI inputs, dependency records, document rules, and review-gate behavior are in scope. Visual behavior is out of scope because the PR adds no scene or player-facing content.

## Findings

### P1-1: The review gate executes evaluator code from the PR head with check-write permission

Status: open.

File: `.github/workflows/review-gate.yml:9-12,53-59` and `WhatYouCarry.Tools/ReviewGate/ReviewGateCommand.cs:84-87`.

Trigger: A same-repository PR changes `WhatYouCarry.Tools` or the workflow, then the `pull_request` job runs that head code with `checks: write`.

Expected: The review gate must enforce T-4 and D-101 through a trusted evaluator. A PR must not control the code that decides whether its own review passes.

Actual: The workflow checks out `github.event.pull_request.head.sha`, builds and runs the PR's `WhatYouCarry.Tools` project, and then gives the result to `gh api` with a token that can write check runs.

Consequence: A PR can change `ReviewGateRules.Evaluate` to return `success`, or change the workflow to post an approved check. The PR can therefore bypass the required cross-provider review. The same token also lets PR-controlled code attempt other check-run writes.

Correction: Run the evaluator from a trusted base or release ref, and keep check publication in trusted workflow code. Add a regression case that changes the head evaluator and proves that the gate result still comes from the trusted evaluator.

Regression check: Run the review-gate workflow against an adversarial PR that changes the evaluator to approve every request. The published check must fail or remain neutral according to the trusted evaluator, and the PR code must not publish a second approved result.

### P1-2: A PR author can replace an approved review record in a metadata-only commit

Status: open.

File: `WhatYouCarry.Tools/ReviewGate/ReviewGateRules.cs:28-29,124-166`, `WhatYouCarry.Tools/ReviewGate/ReviewGateFacts.cs:44-46`, and `WhatYouCarry.Tests/ReviewGateGitTests.cs:35-50`.

Trigger: A reviewer approves an effective head. The PR author then commits a new `docs/reviews/pr-6.md` with an approved verdict and the same effective head.

Expected: A review record must prove that the other provider approved the effective head. D-101 requires one review file per PR. D-184 permits metadata commits without changing the effective head, but it does not permit the PR author to replace the approval.

Actual: The gate reads the review file from the current PR head. It excludes `docs/reviews/` when it computes the effective head. The later metadata commit therefore keeps the old effective head while replacing the text that the gate trusts. The existing metadata test checks only that a review file added in a metadata commit passes.

Consequence: The PR author can self-approve the PR by editing the review record after the real review. The gate then reports success for an unreviewed revision.

Correction: Bind approval to an immutable reviewer-authored record outside the PR-controlled metadata paths, or verify the reviewer-authored commit and its identity before accepting the record. Keep the effective-head rule for metadata updates that do not replace the approval.

Regression check: Add a git-backed test that approves an effective head, changes only `docs/reviews/pr-7.md` in a later commit, and asserts failure unless the change is reviewer-authored and valid.

## Verification

- `dotnet build WhatYouCarry.slnx -m:1`: passed, 0 warnings, 0 errors, revision `39db1f9`.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: passed, 32 tests, 0 failures, revision `39db1f9`.
- GitHub PR #6 status checks: Linux, Windows, and macOS CI passed. The custom `review-gate` check is neutral while the review record is absent.
- `Godot --headless --editor --path WhatYouCarry.Game --build-solutions --quit`: did not run because `Godot` is not on this checkout's command path.
- `git diff --check 546a70a16c0e9a0d24b3b1a7eb2134f2fe6233ba...39db1f974281203aeb5af8fa274a16a513125d45`: passed.
- Manual STE checklist: applied to this review record. Review records and handoffs are dated-record exemptions under the project skill.

## Open questions and accepted risks

OQ-12 remains open for PR-9. No owner decision accepts either finding.

## Verdict

**Changes required.** This verdict applies to head `39db1f974281203aeb5af8fa274a16a513125d45`. The review gate can be bypassed by PR-controlled evaluator code and by replacing the approved review record in a metadata-only commit.
