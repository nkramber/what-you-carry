# PR-100 response

Date: 2026-09-24

Author: Claude Code. This file answers the review record `docs/reviews/pr-100.md` of round 1, at the effective head `198a0c9`.

## P1-1: The build-failure record drops carried seeds

Disposition: full merit.

Evidence: after a failed build, the plan step does not run, and `night-record` has no binary. The publish step then wrote the record `{"commit","endedAt","status"}` alone. `NightSeeds.ReadRecordSeeds` reads a record without `failedSeeds` as a record that carries no seed. The next night can then pass on a new window, and a promotion finds no seed to require. That breaks D-567 and D-569.

Correction:

- `.github/scripts/night-failure-record.sh` writes the failure record with `jq`. The record keeps the `failedSeeds` of the record of `main`. An unreadable record of `main` fails the script, and the publish step then writes no record (T-2).
- `.github/workflows/night.yml`: the fallback of the publish step fetches the record of `main` and runs the script. It no longer depends on the plan step.
- `docs/roadmaps/phase-2-first-playable.md`: the PR-84 scope names the script, and exit test 9 names the new test.

Regression check:

- `NightSeedsTests.TheFailureRecordOfABrokenBuildKeepsTheCarriedSeeds` runs the script under bash and jq on a record of `main` that names a failed seed. The failure record keeps the seed, the next plan runs it, and a promotion without it reads `carry-missing`. An absent record of `main`, and a wrong argument count, fail the script. The Windows leg reads the script text alone, because it has no bash and jq pair. The Linux and macOS legs run the behavior. On the old code, the test fails, because the script did not exist.
- `RepositoryShapeTests.TheNightPlansTheSeedsOfEachSweep` asserts that the publish step calls the script, and that the old `printf` record is gone.

## New ids

None.

## Final head

The push that holds this file. The effective head moves to that commit, because the fix changes `.github/`.
