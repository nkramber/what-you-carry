---
name: one-pr-one-session
description: Bind a session to one PR, and make that PR carry all of its documents. Load it at the start of all PR work. That work includes implementation, a new or continued PR, answers to review findings, a PR review, and the final documents and handoff.
---

# One PR, one session

A session works on one PR (D-375). The PR carries its code, tests, decisions, questions, design and roadmap state, review record, and handoff entry. No PR exists only to record an earlier PR. The `doc-gate` job checks the parts that a machine can read (D-376).

This skill does not copy `AGENTS.md`. The rules and the PR gate stay there.

## Procedure: the start gate

Do these steps before the first edit, commit, or review of the session.

1. Look for substantive work on another PR or another repository in this conversation.
2. Look for a PR in this conversation that the owner already merged or closed.
3. If you find either, stop, and reply `Blocked: start a new clean session for this PR.`
4. Write the binding: the repository, the branch, the PR number or the PR intent, and the role.
5. Confirm that the PR has one concern (G-10).
6. Confirm that you read the handoff and the read order of `AGENTS.md`.
7. Run `git fetch origin`, and record `git rev-parse origin/main` as the base revision.
8. Write the first documents matrix. See "The documents matrix".
9. If you cannot confirm a step, do not start the work.

The role is author, reviewer, or correction author. Substantive work is an edit, a commit, a push, a PR, a review record, or a handoff entry. A summary, a compaction, a fork, or a subagent of such a session is not a clean session. The harness gives no session identity, so the agent alone can do this check.

## Stay bound

- Work on the bound PR alone. More than one clean session can work on one PR, one after the other.
- After the hand-over, the same session can answer the gitar findings and the review findings of the bound PR. That work needs no new session.
- When a request asks for a second PR, reply `Blocked: start a new clean session for this PR.` Do not start that work.
- Put work outside the scope in the next concrete action of the handoff entry, for a fresh session.
- A reviewer session writes the review record and its handoff entry on the PR branch, and nothing more (`pr-review`).
- A PR that records the merge, the documents, or the handoff of an earlier PR breaks D-375. Refuse it, and name D-375.

## The documents matrix

The `## Documents` section of the PR description holds one line for each category, in the order of the PR template. Each line starts with one of three values, then a reason of five words or more:

- `Changed: <reason>`
- `Reviewed; no change needed: <reason>`
- `Not applicable: <reason>`

The reason names the part of the document and the cause, for example "section 3.14 already states the rule that this fix keeps". The categories are `docs/design.md`, `docs/decisions.md`, `docs/questions.md`, `docs/roadmaps/`, `docs/runbooks/`, `docs/session-handoff.md`, `CLAUDE.md` and `AGENTS.md`, and `.claude/skills/`. The `review-gate` job reads `docs/reviews/`.

Reject each of these:

- A document that a later PR or the merge brings up to date.
- A planned documentation PR after this PR.
- A reason with no document or no cause, such as "no documentation impact".
- A handoff entry that describes work that is not in this PR.
- A design doc, a decision, a roadmap, or a question that disagrees with the PR.

Write the description to a file, and run the gate before you open the PR or edit its description:

```
dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -- doc-gate --root . --base origin/main --head HEAD --body <file> --title "<title>" --branch <branch>
```

The `doc-gate` job runs the same rules on each push and on each edit of the description.

## Status before the merge

A PR cannot know its merge commit or its merge time. Git and GitHub hold both, and no document copies them.

- Mark the item in `docs/design.md` and in the focused roadmap as `✅ Done in PR #N.` Write no merge date and no merge commit.
- Write the mark after the PR opens, and before the gitar pass. A design doc or roadmap commit moves the effective head (D-184).
- The handoff entry names the branch and the state "pending owner merge". The handoff and the review record are metadata, so they do not move the effective head.
- A later session reads the merge from git. It does not open a PR to record the merge.
- Some exit tests need a run on `main` after the merge. The PR names each one. The next session reads the result and writes it in its own handoff entry.

## Procedure: the completion gate

Before the hand-over to the other provider, or to the override, confirm items 1 to 5, 7, and 8. Before the owner merge, confirm all eight.

1. The PR holds the code and the regression tests (T-3).
2. `docs/decisions.md` and `docs/questions.md` hold each new decision and question.
3. The design doc and the focused roadmap agree with the PR.
4. The documents matrix is complete, and the `doc-gate` job is green.
5. The handoff entry names this branch and the state "pending owner merge".
6. The review record approves the effective head, or the override label applies (D-188, D-190).
7. Each other line of the PR gate in `AGENTS.md` holds.
8. No required work waits for a second PR.

At the hand-over and at the merge, run the session end gate (D-199). Then write this line, with the PR number in place of N:

`This session is bound to PR #N and is complete. End this session. Start a new clean session before beginning another PR.`

Do not offer to start the next PR. The owner can bring findings on the same PR back to this session.

`docs/design.md` section 3.14 gives the enforcement of each rule: machine, agent, owner, or not observable.
