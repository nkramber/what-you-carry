---
name: review-response
description: Answer a review of your own pull request as the author. Verify each finding as a claim, correct or refute it with evidence, write the response file, and answer the automated pass of gitar. Use for any request to address, answer, or fix review findings or review feedback.
---

# Review response skill

Use this skill when you answer a review of your own PR. The reviewing provider uses the `pr-review` skill, and never this one (T-4, D-381).

## Start

- Load `.claude/skills/one-pr-one-session/SKILL.md`, and bind the session to the PR as the author or the correction author (D-375).
- Load `.claude/skills/ste-writing/SKILL.md` before the response file or a reply (D-139).
- Look up each D-# and OQ-# that the findings cite with the one lookup command of `AGENTS.md` (D-378).
- Read `.claude/skills/pr-review/references/commit-and-push.md`. It holds the commit of a record, the session end gate, and the scope limits, and it applies to the author too.
- The runbook `docs/runbooks/session-context.md` holds the targeted reads, the commit command, the check wait, and the comment export.

## Address review findings

**A finding is a claim, not a fact.** A review can be wrong. Assess each finding against the evidence before you change anything. A finding carries no authority that the evidence does not give it.

1. Run `git fetch` and `git status --short --branch`. If the checkout is ahead of the remote with the reviewer's commit, push it first. Record that in the response file (F-59).
2. Read the finding, then read the file and the lines it names.
3. Reproduce the trigger. A finding that does not reproduce has no merit.
4. Read the contract the finding cites. Find each later revision with the lookup command of `AGENTS.md` (D-186, D-378).
5. Decide the disposition: full merit, partial merit, or no merit.
6. Correct every finding that has merit. Use the smallest change that restores the contract.
7. Record each disposition in `docs/reviews/pr-<number>-response.md`.
8. Commit the response, the corrections, and the handoff entry, then run the session end gate (D-182, D-183, D-199).

Push back when the evidence supports it. State the reason and show the proof:

| Reason to push back | What to show |
|---|---|
| The finding reads a rule too broadly. | Quote the rule. Name the other files that the broad reading also condemns. |
| The finding cites a superseded decision. | Quote the `Effect` column and name the current decision. |
| The finding calls a partial revision a supersession. | Quote the `Revised in part by` marker and the part that still stands (D-186). |
| The trigger does not reproduce. | Give the command, the revision, and the result. |
| The correction breaks another contract. | Name the contract and the caller that it breaks. |
| The finding states a style preference. | Name the contract that the code does not break. |
| The finding repeats a risk that a decision already accepted. | Quote the D-# id and its accepted risk. |
| The finding asks for work outside the PR scope. | Quote the roadmap entry and the exit tests. Name the PR that holds the work. |
| The finding reopens one id for the third time. | Name the three triggers and ask the owner to settle the scope (D-124). |

A disagreement belongs in the response file, with the evidence. Never delete a finding from the review record.
The reviewer sets a refuted finding to `withdrawn` and keeps the evidence that refuted it.

Never accept a finding only to close the review faster. A wrong correction costs more than a written disagreement.
Never widen a correction past the contract that the finding names.
Ask the owner when a finding and an owner decision conflict. Quote both (D-124, D-138).

Partial merit is common. Correct the part that has merit, and refute the rest in the same entry.

## The response file

The author answers a review in `docs/reviews/pr-<number>-response.md`.
This file is a convention, not a gate. `review-gate` does not read it (D-179, D-181, D-185).
Write one when the verdict is `Changes required` or `Blocked`. A clean first pass needs none.

The response file states, for each finding:

- The disposition: full merit, partial merit, or no merit.
- The evidence, when the disposition is partial merit or no merit.
- The correction that landed, with the file and the decision id.
- The regression check that ran, and its result.

The response also lists each new D-# and F-# id, and the final PR head.
A disagreement with a finding belongs here, with the evidence. Do not remove the finding from the review file.

## The automated pass

The owner suspended the automated pass of gitar (D-471). The author asks for no gitar review and waits for none. The pass never replaces the cross-provider review (T-4).

When gitar posts a comment on a PR, answer it before the hand-over to the other provider, or before the override request. Use the answer steps of `.claude/skills/gitar-review/SKILL.md` (D-374). These rules of this repo win over that skill:

- The author alone answers gitar. The reviewing provider never replies to gitar (`pr-review`, `references/repeat-review.md`).
- A reply names no provider, harness, or model as the source of the work (T-6, D-176).
- Record each comment of gitar in the handoff entry, with the commit that answered it.

The file `.claude/skills/gitar-review/references/restore.md` holds the original section and the restore steps.
