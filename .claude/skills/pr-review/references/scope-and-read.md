# The scope of a review, and the read of the PR

The `pr-review` skill names this file at steps 2 and 3. It holds what a review reads, and the boundary of what a review judges.

## Establish the review scope

- Follow the read order in `AGENTS.md`.
- Load `.claude/skills/one-pr-one-session/SKILL.md`, and bind the session to this PR in the reviewer role (D-375).
- Load `.claude/skills/ste-writing/SKILL.md` before any review text (D-139).
- Read the PR request, its acceptance criteria, prior review, and applicable focused roadmap.
- Look up each cited D-# and OQ-# with the one lookup command of `AGENTS.md` (D-378). Resolve decision revisions through the `Effect` column in `docs/decisions.md` (D-186). `Superseded by D-N` replaces the whole answer. `Revised in part by D-N` changes only the named part, and the rest of that decision stays current.
- Check `docs/questions.md` for unresolved choices that affect this change (D-124, D-144).
- Read every existing comment on the PR: the automated pass of gitar and the author's replies (D-250). Take each one into the review as a claim to verify, and never as a finding of your own. See `references/repeat-review.md`.
- Export the comments in one command. The runbook `docs/runbooks/session-context.md` holds it under "The comment export". It writes every issue comment and every review thread to one file, with each thread id and each comment id. Read that file one time.
- Record the PR number, target branch, base commit, merge base, and head commit.
- Verify that the local checkout and diff represent those commits.
- Preserve unrelated local edits. Use an isolated checkout when necessary.
- Inspect the complete diff: deleted files, renamed files, configuration, content, schemas, and tests.
- Read a large diff in stages. The runbook holds the commands under "The staged read of a diff": the size, then each changed path, then one path in full. Stop at the stage that answers the question, and read every changed path before the verdict.
- Read each changed file in context. Follow affected callers, consumers, and persistence paths beyond the diff.
- Continue through the scope after the first finding. Record any area that remains uninspected.

The PR description states intent. The diff and verified behavior establish what the PR does.
Label an uncommitted patch review as provisional. It cannot satisfy a review gate for an unidentified PR revision.
If the base or head changes, assess the new diff and affected evidence before a final verdict.

## Stay inside the pull request

A review judges the change in front of it. It does not design the next one.

Read the roadmap entry for this PR and its exit tests before the first finding. Those two texts set the boundary. This section limits the reach of a review. It never lowers the standard for the code that the PR changes.

A concern is in scope when one of these holds:

- The changed code gives a wrong result under a supported condition.
- The change breaks a caller, a saved file, or a build that exists today.
- A stated exit test of this PR does not hold.
- A guardrail that the PR names does not hold for the code that this PR adds.

A concern belongs to a later PR when one of these holds:

- It asks a tool that this PR creates to cover a surface that no exit test names.
- It asks for behavior that the roadmap gives to a later PR.
- It repeats a class of defect that this PR corrected, in a surface that this PR does not touch.
- It needs an owner decision about scope, and not a correction.

Write the second kind under `## Out of scope` in the review record. Name the PR or the roadmap item that holds it. Give it no severity. A line in that section never blocks the merge.

A PR that creates a check must pass that check (G-19). A new check does not cover the whole platform on the first day. A gap in a new tool is a defect of this PR only when a stated exit test names the missing case.
