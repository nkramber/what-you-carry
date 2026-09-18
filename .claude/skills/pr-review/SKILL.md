---
name: pr-review
description: Review a pull request at principal-engineer depth as the reviewing provider. Require the opposite provider, precise evidence, regression checks, and a revision-specific verdict. Use for PR reviews and repeat reviews after fixes. The author answers a review with the review-response skill.
---

# PR review skill

Review the change as the engineer accountable for its effect on the whole system.
Judge correctness, contracts, failure recovery, test quality, and future maintenance.
Apply this standard to code, content, tools, CI, skills, and document PRs.
The author answers a review, and the automated pass of gitar, with the `review-response` skill (D-381).
A green test suite or a persuasive PR description does not establish correctness.

This file holds the procedure. The reference files hold the detail of each step. Load a reference file at the step that needs it, and not before.

| Step | Reference file | What it holds |
|---|---|---|
| 1 | `references/provider-gate.md` | The provider that may review, and the evidence of authorship |
| 2, 3 | `references/scope-and-read.md` | The read of the PR, and the boundary of a review |
| 4 | `references/review-standard.md` | The depth of the review, and the project contracts |
| 5 | `references/verification.md` | The checks that run, and the evidence of each one |
| 6 | `references/findings.md` | The test for a finding, the severity, and the finding format |
| 7 | `references/review-record.md` | The record skeleton, the verdicts, and the review gate check |
| 8 | `references/commit-and-push.md` | The commit, the push, the session end gate, and the scope limits |
| A repeat pass | `references/repeat-review.md` | The second and later passes, and the comments of gitar |

## Procedure

1. Run the provider gate. Stop with `Blocked` when the providers match or the evidence conflicts.
2. Establish the scope: the read order of `AGENTS.md`, the roadmap entry, the exit tests, the register ids, and every PR comment.
3. Read the complete diff in stages, and read each changed file in context.
4. Build an independent account of the behavior, and read each project contract that the change touches.
5. Run the checks that can falsify the changed behavior, and record the evidence of each one.
6. Write each finding with its trigger, its contract, its evidence, and its regression check.
7. Write `docs/reviews/pr-<number>.md` with the effective head and one verdict.
8. Commit the record with the handoff entry, push it, and run the session end gate.

Steps 2 and 3 use the targeted reads, the comment export, and the staged read of a diff. The runbook `docs/runbooks/session-context.md` holds all three commands.

## The three verdicts

A review ends with one of three verdicts: `Blocked`, `Changes required`, or `Ready for owner merge`. The file `references/review-record.md` gives the condition of each one.

Write one verdict name in the `## Verdict` section, exactly as that file spells it. The `review-gate` job reads that section, and it fails a section that names two verdicts (D-269). A line under `## Out of scope` never gives the verdict `Changes required`.

The owner alone merges the PR (D-102, D-126). Approval applies to the recorded revision alone.

## Rules that hold at every step

- A concern is in scope when the changed code breaks a contract that this PR names, or when a stated exit test fails. A concern with neither goes under `## Out of scope`, with no severity.
- Continue through the scope after the first finding. Record any area that remains uninspected.
- Do not invent findings to meet a quota. A thorough review can produce no actionable findings.
- Never reply to gitar, never resolve a thread, and never write a comment on the PR. Take each comment into the review as a claim to verify (D-250).
- Name no provider, agent, harness, or model in the PR description or in a GitHub comment (T-6, D-137). The review record and the handoff author field are the two exempt places.
- Quote both statements when owner decisions conflict. File the question in `docs/questions.md` and stop dependent work (D-124, D-138).
- Load `.claude/skills/one-pr-one-session/SKILL.md` and bind the session in the reviewer role (D-375). Load `.claude/skills/ste-writing/SKILL.md` before any review text (D-139).
