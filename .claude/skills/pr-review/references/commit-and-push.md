# The commit and the push of a record

The `pr-review` skill names this file at step 8. The `review-response` skill names it too, because the author commits a response file the same way.

## Commit the record

Always commit the review record and the session handoff, then push them to the PR branch (D-182, D-183). Do it in the session that writes them.

| After | Commit these files | Who commits |
|---|---|---|
| A review or a repeat review | `docs/reviews/pr-<number>.md` and `docs/session-handoff.md` | The reviewer |
| Work that answers a review | `docs/reviews/pr-<number>-response.md`, each corrected file, and `docs/session-handoff.md` | The author |

Make one commit that holds the record and its handoff entry. Never leave either file uncommitted or unpushed.
A push is the only way `review-gate` sees the record, because the gate reads the PR head (D-183).
A review is complete only when the remote holds the record. The session end gate below proves it.

An uncommitted review record has three effects:

- The next commit from the other provider absorbs it, and the history no longer shows who wrote what.
- An author can commit an approval that the author never read, and then report the wrong verdict.
- `review-gate` cannot read the record, because the record is not on the PR head (D-179, D-181, D-185).

Write the commit message in an impersonal voice. Name no provider, agent, harness, or model (T-6, D-176).
Fetch the remote and read the handoff again before you write the entry. Take the highest session number and add one (D-187).
Name the remote head in the state of the build (D-199).
Add the handoff entry at the top of the file, as a new entry (D-146).
Another provider can add an entry above yours while you work. Add your own entry. Never append to an older one, and never edit theirs.

## Session end gate

Run these four commands after the commit, in this order. The evidence comes from the remote, not from the local checkout (D-199).

```
git push origin <branch>
git fetch origin
git status --short --branch
gh pr view <number> --json headRefOid --jq .headRefOid
```

The status line must show no `[ahead N]`. The hash from `gh pr view` must equal `git rev-parse HEAD`.
Write the push line in the Verification section of the review record, and name the remote head in the handoff entry.
A record with no push line is incomplete, and the next session treats it as unpushed.

If the remote refuses the push, the review is not complete. Do not end the session.
Ask the owner to approve the push, and say in the handoff that the record has a commit and no push.
A sandbox that blocks the network denies the push without a message from git, so read the status line and not the push output.

At the start of a review or a repeat review, run `git fetch` and `git status --short --branch` too.
If the checkout is ahead of the remote with a commit from the other provider, push it first. Record that in the review file (F-59).

## Scope limits

A review request authorizes inspection, verification, the review record, and the handoff entry.
It requires a commit of those two files, and a push of that commit to the PR branch (D-182, D-183).
It also authorizes a correction of a stale fact in the PR description, under "Correct the PR description" in `references/review-record.md` (D-217).
It does not by itself authorize a code fix, a merge, or another external message.
A reply to the automated reviewer is an external message, and the reviewer never writes one (D-250).
A reviewer never pushes to `main` (D-170).
Honor explicit authorization already present in the session.
If the reviewer writes a substantive fix, reassess provider eligibility. The reviewer cannot approve its own contribution.
Do not disguise a fix as review metadata to bypass the provider gate.
