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
9. Do the gitar pass of the new head, then start the next round with `make codex-review PR=<n>` (D-511).

A correction of documents alone runs `ste-check`, `doc-gate`, and the `Documents` category, and no full suite (D-491, D-492). A correction that changes code runs the full suite (D-493).

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
| `make codex-review` exits 11 for an id. | Do the three-strike stop of `one-pr-one-session`, `references/review-and-merge.md` (D-513). |

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

An automated reviewer, gitar, reviews every PR after a push (D-250). The author gets a current review of the head and answers every finding before the hand-over to the other provider. On a documentation PR, the author does this before the override request. This pass comes before the cross-provider review and never replaces it (T-4).

Load `.claude/skills/gitar-review/SKILL.md` after each push, and follow its procedure (D-374). That skill holds the steps, the proof that a review is current, the traps, and the commands. This section gives only the rules of this repo, and each rule wins over that skill:

- The author alone answers gitar. The reviewing provider never replies to gitar (`pr-review`, `references/repeat-review.md`).
- A reply names no provider, harness, or model as the source of the work (T-6, D-176).
- Resolve each thread after its reply, also after a fix. The ruleset of `main` needs each thread resolved (D-522).
- When the pass ends, run `make codex-review PR=<n>`, or apply the override label (D-511, D-517). The file `references/review-and-merge.md` of `one-pr-one-session` holds the loop and the auto-merge.
- Record the pass in the handoff entry. Give the count of findings, the count with merit, and the commit that answered each one.
- Record each `Gitar review` comment in the handoff entry too (D-303).
