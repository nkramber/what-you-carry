# PR-6 response

Date: 2026-09-08

Answers `docs/reviews/pr-6.md`, verdict `Changes required` at head `39db1f9`. The owner chose the corrections on 2026-09-08 (D-197, D-198).

## P1-1: The review gate executes evaluator code from the PR head with check-write permission

Disposition: full merit.

Evidence: the `pull_request` event runs the workflow file and the tool from the PR head. The workflow at `39db1f9` checked out `github.event.pull_request.head.sha` and ran `dotnet run` on it with `checks: write`. D-190 accepts that the agents hold the owner token before launch, but Phase 5 makes `review-gate` a required check, and the hole then matters.

Correction: `.github/workflows/review-gate.yml` moves to the `pull_request_target` event (D-197). GitHub runs the file and the tool from the base branch. The job checks out the base, fetches `refs/pull/<number>/head` into the object store, and never checks out the head. The tool reads the head through git, as before. Commit `9624cfa`.

Regression check:

- `ReviewGateRunsOnPullRequestTarget` asserts the event, that no step checks out the head, and that the head is fetched as a ref. It fails on the old workflow. 35 tests pass locally.
- The live adversarial run that the review asks for cannot happen before the merge. GitHub triggers `pull_request_target` only when the workflow file exists on the default branch (F-58). PR #7, a throwaway PR against `feat/pr-1-scaffold` with an evaluator that approves every PR and a workflow that posts success, produced no run at all. PR #7 is closed and its branch is deleted. After the merge, a throwaway PR against `main` with the same head proves the trusted evaluator, and this file records the run.

## P1-2: A PR author can replace an approved review record in a metadata-only commit

Disposition: partial merit.

Evidence for the mechanism: it reproduces. `ReviewGateNamesARewriteOfTheReviewFile` commits a `Changes required` record, then rewrites it to `Ready for owner merge` in a later commit, and the gate gives success.

Evidence against the correction: the review asks the gate to verify "the reviewer-authored commit and its identity". Every commit in this repository has one author, one committer, and no signature. `git log --format='%an <%ae> | %cn <%ce>' | sort | uniq -c` gives `nkramber` for all 11 commits, and `git log --format=%G?` gives `N` for each. Both providers push with the owner account and token (D-190). Git cannot tell them apart, and the handoff author field is self-declared. The requested test, "fails unless the change is reviewer-authored", has no predicate to evaluate.

Correction: an accepted risk under D-190, recorded as D-198. The check-run output names the commit that last changed the review file, with its subject, on success and on a verdict failure. The owner reads that line before a merge and recognizes a commit that the reviewer did not make. A per-provider identity is a separate owner decision. Commit `9624cfa`.

Regression check: `ReviewGateNamesTheCommitThatChangedTheReviewFile` (pure) and `ReviewGateNamesARewriteOfTheReviewFile` (git-backed) assert the named commit. Both fail on the old code.

## New ids

- D-197: trusted evaluator, `pull_request_target`.
- D-198: review record trust, an accepted risk with a named commit.
- F-56: the head ran the evaluator.
- F-57: one identity cannot prove the reviewer.
- F-58: `pull_request_target` triggers only from the default branch.

## Final PR head

The code corrections are in `9624cfa`. The commit that holds this file also holds the register corrections for F-58, so it is the effective head. Its hash is in the session 19 handoff.
