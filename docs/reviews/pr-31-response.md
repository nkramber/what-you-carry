# PR-31 response

Date: 2026-09-09

## Identity

- PR: 31
- Reviewed head: `c214d03`
- Response head: `1b58d3a`
- Branch: `feat/pr-10-projectiles`

## Push check

`git fetch` and `git pull --ff-only` brought the review commits `b15c784`, `28b60db`, and `1fee986` into the checkout, and `git status --short --branch` showed it level with the remote, so no push came first (F-59).

## P2-1: Spread offsets can leave the declared cone

Disposition: full merit.

Evidence: `ProjectileSimulation.Fire` drew a yaw offset and a pitch offset, each uniform inside the half angle, and applied both. Two offsets of 15 degrees give an angle of about 21 degrees from the axis, and D-266 names a cone whose half angle is `spreadHundredths`. The old test allowed 22 degrees, so it did not read the contract.

Correction: `Fire` draws one angle from the direction, uniform from zero to the half angle, and one roll around the direction, uniform over a full turn. `Turn` builds two sides of the cone from the cross products with the world up, or with the world right when the direction is vertical, and turns the direction by the angle toward the roll. The angle from the axis is the drawn angle, so no shot leaves the cone. The draw count per shot stays two, so a record replays to the same values.

The sweep hash moved from `3220e92dcbca55a2` to `d8943df12fefcbee`, because the sweep definition has a spread and every shot of the sweep turns another way. The simulation version stays 6: the version rose in this PR already, and the PR is one change to the simulation (G-20).

Regression check: `SpreadStaysInsideTheCone` fires one thousand shots along each of five axes, among them the vertical ones and a slanted one, and asserts that no shot leaves the half angle of 15 degrees by more than a hundredth of a degree, and that the widest comes within one degree of it. The old code fails the first assertion. Passed on the new code.

## P2-2: ArcSolver accepts invalid speed and gravity

Disposition: full merit.

Evidence: `ArcSolver.Solve` read a speed of zero and a negative gravity into the formulas, and `Solve(0, 10, origin, origin)` gave a vertical direction with `Reachable` true. The parameter contract says a speed above zero and a gravity of zero or above, and T-2 asks for a context error on a value outside it.

Correction: `Solve` rejects a speed that is not finite or not above zero, a gravity that is not finite or below zero, and a point that is not finite, each with a `ContextException` that names the value.

Regression check: `ArcSolverRejectsABadSpeedOrGravity` covers a speed of zero, a negative speed, a speed that is not a number, an infinite speed, a negative gravity, a gravity that is not a number, and a point that is not a number. Each asserts the error and the field in its context. The old code throws on none of them. Passed on the new code.

Checks on `1b58d3a`: `dotnet build` 0 warnings, `dotnet test` 506 tests and 0 failures, `det-lint` 0 findings in 57 Core files, `ste-check` 0 findings in 15 files, `bit-identity` `d8943df12fefcbee`.

## New ids

F-87 and F-88 record the findings. No new decision and no new question.

## Final head

The effective head is `1b58d3a`. The commit after it holds this response and the Session 84 handoff entry.
