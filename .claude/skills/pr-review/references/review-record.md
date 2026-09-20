# The review record

The `pr-review` skill names this file at step 7. It holds the record skeleton, the verdicts, the corrections that a reviewer makes to the PR description, and the review gate check.

## Review record

Use one file per PR in `docs/reviews/` (D-101). Reuse its existing name and finding ids on repeat reviews.
For a new record, use `docs/reviews/pr-<number>.md` with the actual PR number, not the roadmap id.
Record provider names only in the permitted review record and handoff author fields (D-137).
Omit those names from any PR description or GitHub comment.

The `review-gate` job reads the review record (D-179, D-181, D-185). Three parts of it are machine-read. Keep their format exact:

| Part | Exact form | Rule |
|---|---|---|
| The file name | `docs/reviews/pr-<number>.md` | The number is the GitHub PR number, not the roadmap id. |
| The head field | `- Head: ` and the hash in backticks, in the Identity list | The hash is the effective head. A short hash is permitted. |
| The verdict | One of the three verdict names, in the `## Verdict` section | Write the name exactly. Do not reword it. |

The effective head is the newest commit that changes a path outside the metadata set (D-184).
The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`.
A commit that changes only those paths is a metadata commit, and it does not change the effective head.
The required review commit holds the review record and the handoff entry, so it is always a metadata commit (D-182).
Without that rule the review commit invalidates the review that it publishes.
Record the effective head, not the tip, when the review commit is the last commit.

Use this skeleton. Keep the heading text and the order.

```markdown
# PR-<number> review

Date: <YYYY-MM-DD>

## Identity

- PR: <number>
- Target: `main`
- Base: `<sha>`
- Merge base: `<sha>`
- Head: `<effective head sha>`
- Branch: `<branch>`

## Provider gate

State the author provider, the source of that fact, and the reviewer provider.
State the gate result against T-4 and D-101.

## Intended behavior and scope

State the intent, what the review inspected, and every affected contract.
Name any area that remains uninspected.

## Findings

One subsection per finding, in severity order. Use the finding format of `references/findings.md`.
Write "No finding." when the review found none.

## Out of scope

One line per concern that a later PR holds. Name that PR or roadmap item.
Give no severity here. Write "None." when the review found none.

## PR comments

One line per existing comment thread on the PR: the claim, the author's answer, and what the review verified (D-250).
Write "None." when the PR holds no comment.

## Description edits

One line per correction that this review made to the PR description.
Give the old value and the new one. Write "None." when the review changed nothing.

## Verification

One line per command or check, with its result.
Name each check that did not run and the reason.
End with the push line: `- Push: <sha> is the head of origin/<branch>, verified with gh pr view.`

## Open questions and accepted risks

Name each open OQ-# and each accepted risk with its D-# id.

## Verdict

**<Blocked | Changes required | Ready for owner merge>.** This verdict applies to head `<sha>`.
Give the reason in one or two sentences.
```

## Correct the PR description

A PR description is part of the documentation set (D-118). A description that names a stale head, an old count, or a superseded correction misleads the owner at the merge.

The reviewer corrects such a description directly (D-217). It needs no finding, and the author needs no extra pass for it.

The reviewer changes only a fact that the review verified:

- the effective head, the base, or the merge base.
- a count that the review ran, such as the test total or the finding total.
- a check result that the review read.
- a sentence that names a correction that a later commit replaced.

The reviewer never changes:

- what the author says the PR does, or why.
- a decision, a tradeoff, or a recommendation.
- a gate line that the owner ticks.

Name no provider, agent, harness, or model in the description (T-6, D-137).

Write one line for each edit in the review record, under `## Description edits`. Give the old value and the new one. The owner then reads every change in one place.

A claim that is wrong in substance stays a finding. The reviewer corrects a stale fact, and the author corrects a wrong statement.

## Verdicts

| Verdict | Required condition |
|---|---|
| Blocked | Provider independence, the review target, a necessary owner decision, or required evidence remains unresolved. Record any verified defects too. |
| Changes required | The eligible review found in-scope defects or contract violations that need correction. List the required changes. |
| Ready for owner merge | The provider gate passes, the complete scope has review coverage, all required checks pass, and no blocking finding remains. |

A line under `## Out of scope` never gives the verdict `Changes required`.
No findings does not mean no risk. State material limits without a claim of zero regressions.
Approval applies only to the recorded revision. A new base or head requires assessment of the changed scope and evidence.
The owner alone merges the PR (D-102, D-126).

When the review record enters the PR, retain the assessed implementation head in that file.
Check any later metadata commit before the final verdict.
Do not require the review file to contain its own commit hash.
A metadata commit cannot hide code, content, requirement, or test changes.

## The review gate check

PR-1 adds a `review-gate` check (D-179, D-181, D-185). It applies three rules:

1. `docs/reviews/pr-<number>.md` exists for the PR number.
2. The verdict is `Ready for owner merge`.
3. The head in the Identity list is the effective head.

The check has three states. Read the color before you start:

| Color | Meaning | What to do |
|---|---|---|
| Grey | No review record exists for this PR. The job line reads red (D-251). | Write one. This is the normal state before a review. |
| Red | A review record exists, and it does not approve this head. | Read the findings. The author corrects them. |
| Green | An approved review covers the effective head. | The owner may merge (D-102, D-126). |

Grey appears only while the check is advisory. At launch the same case turns red (D-181, D-185).
GitHub counts a neutral conclusion as a success for a required check, so enforced mode never uses grey.
The check is advisory until launch, because GitHub locks branch protection on a private free repository (D-170, D-180).

Rule 3 fails when the author pushes code after the approval. That result is correct.
Reassess the new diff, then update the head field and the verdict together.
Rule 3 does not fail when the last commit changes only the metadata paths (D-184).
