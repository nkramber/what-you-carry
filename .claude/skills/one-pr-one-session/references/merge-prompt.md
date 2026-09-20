# The transitional prompt of the merge

A session ends at the hand-over point (D-375). The next PR starts in a new clean session, and that session holds none of this context. This file holds the prompt that carries the work across the gap.

## The trigger

After the hand-over point, the owner merges the PR and says `Merged PR #x`. The session then writes one transitional prompt, and it does no other work. The message alone starts this step. The owner asks for no prompt.

Write the prompt for the bound PR of the session alone. A merge message for another PR gets the blocked result of the start gate. A merge message for the bound PR is the one exception to step 2 of the start gate.

The prompt names no provider, harness, or model as the source of work (T-6, D-137).

## Procedure: write the prompt

1. Run the session end gate first (D-199).
2. Get the merge commit: `git fetch origin && git log --oneline -1 origin/main`.
3. Read the focused roadmap, and name the next PR. The owner can name a different PR.
4. Read the questions register, and name each open question of the next PR.
5. Read the scope and the exit tests of the next roadmap entry.
6. Name each owner answer that the scope or an exit test requires.
7. Name each exit test of the merged PR that needs a run on `main`.
8. Write the block below in the last message, and stop.

Step 6 finds an owner answer that no OQ-# holds. A sweep of the questions register alone misses it. The roadmap of PR-66 gives an example: the shape of a tier, which D-350 leaves to the PR session.

## The block

The prompt is one fenced block, and the owner pastes it into the next clean session:

```
Start PR-<n>: <the one concern>

PR #<x> merged to `main` as <sha>. Read the newest session handoff entry first.
Repository: what-you-carry. Branch: `<prefix>/pr-<n>-<slug>`. Base: `<sha>`. Role: author.
Load the `one-pr-one-session` skill and the skills of the task before any change.
<Each exit test of the merged PR that needs `main`, and the order: run it before the PR work.>
Open questions for this PR: <each OQ-# with its subject, or `none`>.
Owner answers that the roadmap names and no OQ-# holds: <each one, or `none`>.
First action: <the first concrete action>.
```

## Rules

- The prompt holds one PR of work. A second PR needs a second session, and a second prompt.
- The prompt never asks the new session to record the merge of the old PR. Git holds the merge (D-375).
- An exit test that needs `main` comes before the PR work, because the new PR branch cannot give that result.
- When the merged PR has no exit test for `main`, remove that line.
- The session handoff entry holds the next ids, and the prompt does not copy them.
- The session handoff entry of the ending session holds the same next concrete action. The two agree, or the entry wins.
- The session makes no branch and no change for the next PR (D-375).
