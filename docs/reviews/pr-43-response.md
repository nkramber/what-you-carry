# PR-43 response

Date: 2026-09-11

## Identity

- PR: 43
- Reviewed head: `d89338c`
- Branch: `chore/night-logs-artifact`

## Push check

`git fetch` and `git pull --ff-only` brought the review commits `01b3d17`, `8f55419`, and `abdea9a` into the checkout, and `git status --short --branch` showed it level with the remote, so no push came first (F-59).

## P2-1: The shape test does not require the upload step to follow the reachability sweep

Disposition: full merit.

Evidence: the assertion compared the position of the upload step with the greedy-descender step alone. A workflow text with the upload step moved to a place between the descender and the sweep keeps the upload step after the descender, so the old assertion passed it, while a failed sweep then ran with no upload. A script that moved the step in the real workflow text confirmed it: the old comparison read true and the sweep comparison read false.

Correction: the check moves into `UploadStepDefect`, which reads a workflow text and returns the first defect by name or null. It requires the upload step after each of the three bot steps by name: the walker, the descender, and the sweep. `NightWorkflowKeepsTheLogsOfAFailedNight` asserts null on the real workflow. `NightWorkflowUploadStepMustFollowEveryBotStep` is the regression case the review asked for: `MoveStepBefore` cuts the upload step from the real workflow text and puts it before the sweep, and the check names the sweep. It moves the step before the walker too, and it removes the action, and the check names each. The workflow itself did not change, because the review found its behavior right.

Regression check: `dotnet test WhatYouCarry.slnx --no-build --filter FullyQualifiedName~RepositoryShape`: 14 passed, 0 failed. On the old assertion the moved text passed, as the evidence above shows, and the new check fails it by name.

## New ids

No new decision, no new question, and no new finding. The review record holds P2-1.

## Final head

The correction is the one commit above the review commits. It holds the test, this response, and the Session 111 handoff entry, and it is the effective head.
