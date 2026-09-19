# The transitional prompt of the merge

A session ends at the hand-over or at the merge (D-375). The next PR starts in a new clean session, and that session has none of this context. This file holds the prompt that carries the work across the gap.

Write the prompt at the end of the session, after the session end gate (D-199). Give it to the owner in the last message. The owner starts a new session and gives it the prompt.

## What the prompt holds

- The PR that the owner merges or merged, and its branch.
- The next PR id, with the D-# ids that bind it.
- Each exit test of the merged PR that needs a run on `main` after the merge.
- Each open question that blocks the next PR.
- The next ids: the next D-#, OQ-#, F-#, PR-#, and session number.

The prompt names no provider, harness, or model as the source of work (T-6, D-137). The handoff entry holds the same facts, and the prompt is a short form of them.

## The form

```
Merge PR #<number> (<branch>), then start a new clean session with this prompt:

Resume work. The newest handoff entry gives the state. This session takes PR-<id>,
<the one concern of that PR>, which D-<ids> bind. Before the PR work, run
<the exit test of the merged PR that needs main>, and state the result in the
handoff entry. <The open question that blocks it, or "No open question blocks it.">
The next ids are D-<n>, OQ-<n>, F-<n>, PR-<n>, and Session <n>.
```

## Rules

- The prompt holds one PR of work. A second PR needs a second session, and a second prompt.
- The prompt never asks the new session to record the merge of the old PR. Git holds the merge (D-375).
- An exit test that needs `main` comes before the PR work, because the new PR branch cannot give that result.
- When the merged PR has no exit test for `main`, remove that sentence.
- The handoff entry of the ending session holds the same next concrete action. The two agree, or the entry wins.
