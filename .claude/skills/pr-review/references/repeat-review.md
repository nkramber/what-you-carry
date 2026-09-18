# The repeat review

The `pr-review` skill names this file when the author revises the PR. It also holds the rule for the comments of the automated pass.

## Repeat review procedure

Do these steps in order after the author revises the PR.

1. Read the response file when one exists.
2. Check the provider gate again. A reviewer fix changes eligibility.
3. Read the new head, the new base, and the diff since the reviewed head.
4. Verify each claimed fix against its original trigger and its regression check.
5. Set the `Status` line of each prior finding. Keep every id and every piece of evidence.
6. Inspect the new diff for new defects and affected consumers.
7. Add any new finding with the next index in its severity.
8. Update the Identity list to the new effective head.
9. Update the Verification section with the commands that ran on the new head.
10. Write the verdict against the new head. Keep one verdict name in the Verdict section (D-269).
11. Commit the review record and the handoff entry together, then run the session end gate (D-182, D-183, D-199).

Edit the existing `docs/reviews/pr-<number>.md`. Do not create a second file for the same PR.
Do not delete the prior verdict. Replace it, and keep each finding and its history.
Put the earlier verdict in a section above the Verdict section, under a heading that starts with another word, such as `## Earlier verdicts`. The gate reads the section under the exact heading `## Verdict`, and it fails a section that names two verdicts (D-269). The count reads the prose too, so the reason after the verdict names no other verdict: write "the earlier findings are fixed" and not "the changes required are done". `dotnet test` reads every review record the same way, so run it before the push.
Close a finding only when the evidence establishes the fix or an owner decision resolves it.
Record any required check that still waits for a result.

### When a finding closes

A finding closes when the correction makes its stated trigger pass and its regression check pass. Set the status to `fixed in <sha>` then.

A new trigger for the same class of defect is a new finding with a new id. Assess that new finding against the scope rules of `references/scope-and-read.md`. It is not a reason to hold the old id open.

Stop at the third assessment of one id. Write the pattern in the review record, and ask the owner whether this PR carries the whole surface, or a later PR does. A fourth correction of one finding is a scope question, and not a defect.

## Do not address the automated reviewer

The reviewing provider reads the existing PR comments and takes them into its own review context (D-250). It never replies to gitar, never resolves a thread, and never writes a comment on the PR.

- A comment of the automated pass is a claim about the code, like any finding. Verify it against the head, and record the result under `## PR comments` in the review record.
- An author reply is evidence, and the review checks it: the trigger, the contract, and the commit it names.
- An automated comment that the author refuted with evidence is not a finding. An automated comment that the author fixed is a fix to verify. An automated comment that stays open without an answer blocks the verdict, because the author's pass is not complete (D-250).
- The automated pass does not make gitar an author. The provider gate reads the providers of the substantive commits alone.
