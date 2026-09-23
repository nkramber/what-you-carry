# The review loop and the merge

The `one-pr-one-session` skill names this file at the end of the gitar pass. It holds the author loop of the cross-provider review, the three-strike stop, and the auto-merge (D-511 to D-517). The `review-response` skill holds the answer to each finding.

## Procedure: the author loop

1. Push the round of changes.
2. Complete the gitar pass with the `gitar-review` skill. Resolve each thread after its reply (D-522).
3. Run `make codex-review PR=<n>` in the background, and wait for the completion notice (D-511).
4. Read the outcome line of the command and its exit code.
5. Do the step that the table below gives for that exit code.

A review round takes longer than the ten-minute limit of a tool call. Do not poll the round. The command starts Codex in a detached worktree at the PR head, so the author checkout does not change. Codex pushes the review record and its own handoff entry as one metadata commit (D-182, D-518).

| Exit | Outcome | Next step |
|---|---|---|
| 0 | The review approves the effective head | Do the auto-merge below |
| 10 | `Changes required` or `Blocked` | Answer the findings with `review-response`, then go to step 1 |
| 11 | The three-strike stop | Do the three-strike stop below |
| 3 | A start condition failed | Correct each condition that the output names, then go to step 3 |
| 1 | A fault: no record, a stale head, no pushed commit, a moved effective head, or a Codex error | Read the transcript, correct the cause, then go to step 3 |

Make exits 2 for each failed target, and it prints the exit code of the command as `Error <code>`. The first line of the command output names the outcome.

## Procedure: the three-strike stop

A P0, P1, or P2 finding that is open in three review rounds stops the fix loop (D-513 to D-515). The `Open at:` line of each finding holds the count.

1. Run `gh pr merge <n> --disable-auto`.
2. Stop the fix loop. Make no more commits for the finding.
3. Ask the owner with `AskUserQuestion`.
4. Give the finding, the evidence of the reviewer, the answers of the author, and the options.
5. Record the answer in `docs/reviews/pr-<n>-response.md`.
6. Record a new D-# when the answer sets a rule.

## Procedure: the auto-merge

1. Write the handoff entry of the session, and commit it as a metadata commit.
2. Push the commit, and run the session end gate (D-199).
3. Prove the gitar pass of the new tip with `gitar-review`.
4. Confirm that the record gives `Ready for owner merge` for the effective head.
5. Run `gh pr merge <n> --auto --squash` (D-516).
6. Wait on the checks with the one command of `docs/runbooks/session-context.md` (D-380).
7. Run `gh pr view <n> --json state,mergedAt,mergeCommit`.
8. When a Mac job ends "not acquired", re-run the failed jobs (D-358), then go to step 6.
9. When the state is `MERGED`, load `merge-prompt.md` and write the prompt.

A documents-only PR with the `review-override` label merges the same way. It needs no review record, and `review-gate` is green by the label (D-517).

The night gate or a Mac leg can hold the merge for hours, and that is normal. When every check is green and the state stays `OPEN`, read `gh pr view <n> --json mergeStateStatus,autoMergeRequest`. An unresolved thread or a stale check blocks the merge.

The ruleset of `main` is the machine gate (D-522). It requires the 20 checks of `.github/rulesets/main.json` and resolved conversations, and it allows squash merges alone. The top-level gitar comments are not review threads, so step 3 proves them.
