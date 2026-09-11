# PR-46 response

Date: 2026-09-11

## Identity

- PR: 46
- Reviewed head: `862fd4c`
- Branch: `chore/night-schedule-reset`

## Push check

The review commit `c76d54a` came from this checkout. `git status --short --branch` showed the checkout level with `origin/chore/night-schedule-reset` at `c76d54a`, so no push came first (F-59).

## P2-1: The schedule shape test does not assert the documented return time

Disposition: full merit.

Evidence: the test asserted `02:07 Central Standard Time` alone for the return. A script changed the return text of the workflow comment to `09:07 UTC, which is 02:07 Central Standard Time`, and `NightWorkflowRunsAtTheScheduleTestTime` passed on it. The PR description and the Session 116 entry said that the test asserts the return time, so the claim and the test disagreed.

Correction: the assertion reads `08:07 UTC, which is 02:07 Central Standard Time`, the phrase of the return on one line of the comment. The review asked for an assertion on `08:07 UTC` alone. That text passes on the trigger too: after the change, line 3 of the workflow still holds `08:07 UTC runs of 2026-09-11 never came (F-94, F-95)`, and the script found it there. The phrase binds the return time to its Central Standard Time value. The workflow did not change.

Regression check: on the correction, trigger 1, the return time changed to `09:07 UTC`, fails with `Not found: "08:07 UTC, which is 02:07 Central Standar"···`. Trigger 2, the return time removed, fails the same way. With the branch workflow restored, `dotnet test WhatYouCarry.slnx --no-build --filter FullyQualifiedName~RepositoryShape` passes 14 tests, 0 failures. Before the correction, trigger 1 passed the test.

## New ids

No new decision, no new question, and no new finding. The review record holds P2-1.

## Final head

The correction is the one commit above the review commit. It holds the test, this response, the Session 118 handoff entry, and the move of Sessions 108 and 107 to the archive, and it is the effective head.
