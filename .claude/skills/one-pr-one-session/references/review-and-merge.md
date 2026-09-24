# The review loop and the merge

The `one-pr-one-session` skill names this file at the end of the gitar pass. It holds the author loop of the cross-provider review, the three-strike stop, the auto-merge, and the merge summary (D-511 to D-517, D-524, D-533). The `review-response` skill holds the answer to each finding.

**Pause (D-542).** During the gitar pause, step 2 of the author loop and step 3 of the auto-merge change. Export the PR comments, answer each gitar comment with an item, and do no push wait. A gitar notice needs no answer (D-550). Step 3 of the author loop runs `make codex-review PR=<n> -- --skip-gitar-review` (D-543). When a gitar review holds feedback, stop at once and alert the owner.

## Procedure: the author loop

1. Push the round of changes.
2. Complete the gitar pass with the `gitar-review` skill. Resolve each thread after its reply (D-522).
3. Run `make codex-review PR=<n>` in the background, and wait for the completion notice (D-511).
4. Read the outcome line of the command and its exit code.
5. Do the step that the table below gives for that exit code.

No round uses API pricing. The command removes each API credential variable from the Codex processes, and it refuses a login that is not ChatGPT (D-523). A review round takes longer than the ten-minute limit of a tool call. Do not poll the round. The command starts Codex in a detached worktree at the PR head, so the author checkout does not change. Codex pushes the review record and its own handoff entry as one metadata commit (D-182, D-518).

| Exit | Outcome | Next step |
|---|---|---|
| 0 | The review approves the effective head (D-534) | Do the auto-merge below |
| 10 | `Changes required` or `Blocked` | Answer the findings with `review-response`, then go to step 1 |
| 11 | The three-strike stop | Do the three-strike stop below |
| 3 | A start condition failed | Correct each condition that the output names, then go to step 3 |
| 1 | A fault: no record, a stale head, no pushed commit, a moved work head, or a Codex error | Read the transcript, correct the cause, then go to step 3 |

Make exits 2 for each failed target, and it prints the exit code of the command as `Error <code>`. The first line of the command output names the outcome.

The record names the effective head, which skips each documents commit (D-534). A documents commit after an approving round keeps `review-gate` green, so no new round is due. The gitar pass still reads each push, because it follows the work head. A round fails when any commit outside the metadata set arrives during the round, a documents commit included.

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
5. Write the merge summary below (D-533).
6. Ask the owner to confirm the merge with `AskUserQuestion` (D-524).
7. Stop when the owner does not confirm. Do the next step that the owner names.
8. Run `gh pr merge <n> --auto --squash` (D-516).
9. Wait on the checks with the one command of `docs/runbooks/session-context.md` (D-380).
10. Run `gh pr view <n> --json state,mergedAt,mergeCommit`.
11. When a Mac job ends "not acquired", re-run the failed jobs (D-358), then go to step 9.
12. When the state is `MERGED`, load `merge-prompt.md` and write the prompt.

A documents-only PR with the `review-override` label merges the same way, the owner confirmation included. It needs no review record, and `review-gate` is green by the label (D-517). For a PR that the owner merges by hand, give the merge summary at the hand-over (D-524).

When the night record of `main` fails, run a night on the PR branch with `gh workflow run night.yml --ref <branch>`. The night runs on hosted Linux, so it does not wait for the PR checks (D-572). The night writes its record to `night-branch/<branch>` and re-runs the `night-gate` check of the PR (D-538, D-547, D-548). After the merge, `night-promote.yml` makes that night the record of `main` when the merge adds only paths of the skip set (D-555 to D-558).

The night gate or a Mac leg can hold the merge for hours, and that is normal. When every check is green and the state stays `OPEN`, read `gh pr view <n> --json mergeStateStatus,autoMergeRequest`. An unresolved thread or a stale check blocks the merge.

The ruleset of `main` is the machine gate (D-522). It requires the 20 checks of `.github/rulesets/main.json` and resolved conversations, and it allows squash merges alone. The top-level gitar comments are not review threads, so step 3 proves them.

## The merge summary

Before the owner confirms a merge, write the summary as four questions, each with its answer of a few sentences (D-533, D-552). Use these questions, in this order:

- **Q: What does this PR change?** A: the change, and the roadmap item that it closes.
- **Q: How does it do it?** A: the method, and the parts of the code or the documents that changed.
- **Q: Is CI green?** A: green or not. Name each red check, and the cause when you know it.
- **Q: What did the Codex review say?** A: the verdict of the record, `Ready for owner merge`, `Blocked`, or `Changes required`, and the effective head that it names.

Put each point that needs the owner as one more question at the end. A PR with the `review-override` label has no review record. Its Codex review answer names the label and the account that added it (D-190).
