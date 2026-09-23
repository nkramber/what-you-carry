# Findings

The `pr-review` skill names this file at step 6. It holds the test for a finding, the severity table, the finding format, and the attribution test.

## Precise findings

Investigate each suspected defect before it becomes a finding.
Search for a caller guarantee, validation layer, existing test, or later decision that can refute the concern.
Use a reproduction, failed assertion, or complete causal trace as evidence.
Separate a verified defect from an unresolved question or an optional suggestion.

Each finding contains:

- A stable local id, severity, and short title that states the defect.
- The reviewed commit and the smallest useful file and line range.
- The input or state that triggers the defect.
- Expected behavior, with the relevant contract or D-# id.
- Actual behavior and its consequence for the player, data, build, or maintainer.
- Evidence, with the seed, command, trace, or artifact when applicable.
- A correction direction and the regression check that will establish the fix.

Group repeated symptoms under one cause. Identify other affected locations without duplicate findings.
Do not prescribe a broad rewrite when a smaller correction restores the contract.
Do not invent findings to meet a quota. A thorough review can produce no actionable findings.

Answer two questions before a finding enters the record:

1. Does the changed code break a contract that this PR names?
2. Does a stated exit test of this PR fail?

A finding needs one yes. A concern with two answers of no goes under `## Out of scope`.

| Severity | Meaning |
|---|---|
| P0 | Immediate critical failure, such as broad durable data loss or a release that cannot start. State the demonstrated scope. |
| P1 | Major correctness, recovery, determinism, or required-gate failure. Resolve before merge. |
| P2 | A concrete defect or material contract gap under a supported condition. Resolve before merge or obtain an explicit owner disposition. |
| P3 | An optional improvement with no broken required contract. It does not block merge. |

Scope decides whether a concern enters the table at all. Severity decides how much it blocks. A concern outside the scope of this PR takes no severity.
Severity describes impact and urgency. It does not replace evidence or the project gate.
Do not reduce severity because the patch is small or the author calls the change safe.
Quote both statements when owner decisions conflict. File the question in `docs/questions.md` and stop dependent work (D-124, D-138).

## Finding format

Give each finding a stable id: the letter `P`, the severity number, a hyphen, and an index. `P1-1` is the first P1 finding.
Keep the id for the life of the PR. Never renumber a finding on a repeat review.

```markdown
### P<severity>-<n>: <short title that states the defect>

Status: <open | fixed in `<sha>` | accepted risk, D-# | withdrawn>.

Open at: `<sha>`, `<sha>`.

File: `<path>:<line range>`, or Commit: `<sha>`.

Trigger: the input or state that produces the defect.

Expected: the required behavior, with the contract, tenet, guardrail, or D-# id.

Actual: the observed behavior.

Consequence: the effect on the player, the data, the build, or the maintainer.

Correction: the smallest change that restores the contract.

Regression check: the command or test that establishes the fix, and the result that must appear.
```

A withdrawn finding stays in the file with the evidence that refuted it. Never delete a finding.

## The Open at line

The `Open at:` line lists the effective head of each review round in which the finding is open, in backticks (D-514). Write each head one time, in the order of the rounds. A round that opens the finding writes its head. A repeat review adds its head to each finding that stays open or opens again. It never removes a head.

`make codex-review` counts the heads. A P0 to P2 finding with three heads in a round that does not approve stops the fix loop, and the owner decides (D-513, D-515). An open finding whose line does not name the head of the round fails the round (T-2). Each `###` heading of the Findings section is a finding heading with a severity from P0 to P3. Any other line that starts with `###` fails the round.

## Do not raise a tool name as attribution

T-6 and D-137 prohibit text that names an agent, harness, or model **as the source of the work** (D-176).
A tool name that identifies a configured file, a schema, or a verified version is not attribution.

| Raise it | Do not raise it |
|---|---|
| A commit body that says an agent wrote the change. | The path `.claude/settings.json`. |
| A co-author trailer or a generation line. | A decision that names the schema it was verified against. |
| A PR description that credits a model. | A document that records which tool rejected a file. |

Apply the same test to every file before a finding. A reading that condemns the decision register is too broad.
