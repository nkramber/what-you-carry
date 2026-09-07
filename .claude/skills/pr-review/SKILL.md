---
name: pr-review
description: Review a pull request at principal-engineer depth, or answer a review as the author. Require the opposite provider, precise evidence, regression checks, and a revision-specific verdict. A finding is a claim, not a fact, and the author can refute one with evidence. Use for PR reviews, repeat reviews after fixes, and any request to address, answer, or fix review findings or review feedback.
---

# PR review skill

Review the change as the engineer accountable for its effect on the whole system.
Judge correctness, contracts, failure recovery, test quality, and future maintenance.
Apply this standard to code, content, tools, CI, skills, and document PRs.
A green test suite or a persuasive PR description does not establish correctness.

## Mandatory provider gate

**The reviewer MUST NOT come from the provider that wrote the PR.**
This requirement applies before the substantive review starts and before any approval (T-4, D-101).

| Provider that wrote the PR | Required reviewer |
|---|---|
| Claude Code, Anthropic | Codex, OpenAI |
| Codex, OpenAI | Claude Code, Anthropic |

A different model, account, session, or subagent from the same provider does not qualify.
A prompt that assigns the other provider's name does not change the actual provider.
Author self-checks and automated tests do not satisfy this gate.

Substantive changes alter code, data, configuration, requirements, or executable instructions.
Review findings and test reports alone do not make the reviewer a PR author.

1. Identify the actual reviewer provider from the active environment.
2. Identify every provider that contributed substantive changes or fixes to this PR.
3. Verify authorship from the owner's statement or the relevant handoff and review records.
4. Match each source to this PR and its revision.
5. Record the providers, source, and eligibility result in the review file.

The newest handoff entry can describe a review rather than authorship. A Git account alone does not identify the provider.
Do not infer authorship from prose style, commit email, or a branch name.

**Stop with `Blocked` if the providers match, authorship is unknown, or the evidence conflicts.**
State which fact or eligible reviewer is required. Ask the owner to supply that fact or start the opposite-provider session.
Do not perform a substitute review with another model from the same provider.

If both providers wrote substantive changes in the PR, neither qualifies for the whole PR.
Record the conflict and request an owner decision about how to separate the changes.
Do not approve through reciprocal review of selected hunks.

## Establish the review scope

- Follow the read order in `AGENTS.md`.
- Load `.claude/skills/ste-writing/SKILL.md` before any review text (D-139).
- Read the PR request, its acceptance criteria, prior review, and applicable focused roadmap.
- Resolve decision revisions through the `Effect` column in `docs/decisions.md`.
- Check `docs/questions.md` for unresolved choices that affect this change (D-124, D-144).
- Record the PR number, target branch, base commit, merge base, and head commit.
- Verify that the local checkout and diff represent those commits.
- Preserve unrelated local edits. Use an isolated checkout when necessary.
- Inspect the complete diff: deleted files, renamed files, configuration, content, schemas, and tests.
- Read each changed file in context. Follow affected callers, consumers, and persistence paths beyond the diff.
- Continue through the scope after the first finding. Record any area that remains uninspected.

The PR description states intent. The diff and verified behavior establish what the PR does.
Label an uncommitted patch review as provisional. It cannot satisfy a review gate for an unidentified PR revision.
If the base or head changes, assess the new diff and affected evidence before a final verdict.

## Principal-engineer review standard

Build an independent account of the behavior before comparison with the author's explanation.
For each changed behavior, trace the input, state transition, output, side effects, and recovery path.
State the invariant that each boundary must preserve.

### Correctness and system effects

- Check normal use, boundary values, absent data, invalid data, repeated actions, and interrupted actions where applicable.
- Trace state ownership and lifetime across Core, Game, Tools, and Tests.
- Inspect initialization, cancellation, cleanup, restart, and replay when the change affects those paths.
- Check event order, resource disposal, integer bounds, and float edge cases where they affect the result.
- Inspect compatibility with existing callers, content, saves, and exported builds.
- Check whether a local fix creates a defect in another consumer of the same contract.
- Verify each acceptance criterion against implementation and evidence.

Do not expand the review into an unrelated rewrite.
Distinguish defects introduced by the PR, defects it exposes, and independent pre-existing defects.
A pre-existing defect blocks this PR only when it prevents the changed behavior or a required gate.

### Project contracts

Apply each relevant row. Record why an area does not apply when its omission could mislead a reviewer.

| Area | Required examination |
|---|---|
| Core boundary | No engine dependency or gameplay input from Godot physics or navigation. Trace data flow, not only imports (G-1, G-3). |
| Determinism | Seed ownership, stable iteration and event order, DetMath use, one simulation thread, and 60 Hz intents. Inspect the pure worker boundary (D-69 to D-77). |
| Replay | Immutable initial loadout, tree, and amulet state. Format and simulation versions, content hash, and the specified mismatch path. Verify the version bump for Core behavior changes (D-151, G-20). |
| Persistence | Profile generation, atomic replacement, run pointer, checksummed frames, and one-time completion by run id. Trace failure at each write boundary (D-152). |
| Errors | Required context, visible failure, safe recovery, and assertions in shipped builds. An empty catch or silent fallback violates T-2 (D-112, D-113). |
| Content | JSON schemas at load and in tests. Absent fields report the file, field, and reason. Check identifier references and file name case (D-91, D-92). |
| Input and CI boundaries | Check size limits, file paths, and validation at affected external inputs. Inspect CI permissions, secret access, and execution of untrusted content when those boundaries change. |
| Gameplay | Player and enemy rules, gear loss, permanent amulet, basic kit, timer, and floor transitions where affected. Trace repeated runs as well as one run (D-2, D-30, D-34, D-140, D-153). |
| Economy | Matched policies, fixed seeds, initial state, time denominator, and statistical bounds follow D-154. A smaller payout alone does not prove a lower rate. |
| Presentation | Controller use, Deck 800p readability, animation time values, and asset QA. Headless tests do not establish visual quality or game feel (D-15, D-87, D-135). |
| Dependencies and cost | A decision justifies each dependency. Performance claims include a profile before the change and a measurement after it (G-16, G-17). |

Do not reintroduce an earlier contract that a later decision supersedes.
For example, D-152 supersedes the three-file save design in D-94.

### Design, maintainability, and documents

- Confirm one concern per PR and a clear reason for every changed subsystem (G-10).
- Check helper depth against D-110.
- Require two concrete uses before an abstraction (D-111).
- Prefer explicit ownership and visible control flow over hidden coupling.
- Explain the concrete maintenance cost of a design objection.
- Do not report personal style preferences as correctness defects.
- Check that design text, decisions, questions, code, and acceptance criteria agree.
- Check each roadmap prerequisite against the first gate that needs it.
- Distinguish proposed work, implemented work, measured behavior, and owner approval.
- Verify material external claims against dated primary sources.
- Check the document dispositions required by D-118.
- Confirm `AGENTS.md` and `CLAUDE.md` remain identical when either changes (D-122).
- Check attribution restrictions in commits, PR text, comments, and deliverables (D-137).

Documentation and skill PRs require the same provider independence and evidence discipline as code PRs.
For a skill change, examine its trigger, scope, instructions, references, and behavior on a realistic request.
Treat contradictory instructions and gates that cannot pass as defects.

## Verification

Run the focused checks that can falsify the changed behavior. Complete the applicable project gates.
Use the current build commands in `AGENTS.md`. Do not invent a successful command when no solution or tool exists.

- Read the tests as critically as the implementation.
- Verify that each bug fix has a regression test that fails on the old behavior (T-3).
- Use an isolated comparison when execution of the regression test against the base is practical.
- Otherwise, explain the causal reason the old behavior fails the assertion and state the execution limit.
- Check test assertions against the contract, not a copy of the implementation.
- Inspect seed coverage, state diversity, boundary cases, and failure context (D-66).
- Check test discovery, skipped tests, mocks, fixtures, and assertions that can pass without the intended behavior.
- Distinguish a passed check from a skipped, unavailable, failed, or author-reported check.
- Record the command, revision, environment, result, and relevant artifact for each required check.
- Verify CI results against the reviewed revision and configured test target.
- Check the three-platform bit-identity result and required smoke and content tests (D-71, D-114).
- Check the applicable bot, seed, economy, asset, and human gates in the current roadmap.
- Confirm that a known night failure does not bypass the next merge gate (D-115).

Use the initial-check clause only as D-148 and G-19 permit.
Name the absent check and the PR that creates it. A PR that creates a check must pass it.
The clause does not excuse a failed existing check.

Do not repeat broad suites without a new change, failure, or unresolved risk.
Do not weaken a test or threshold to obtain a pass.
Absent required evidence blocks approval. Optional evidence gaps belong in the limitations.

## Precise findings

Investigate each suspected defect before it becomes a finding.
Search for a caller guarantee, validation layer, existing test, or later decision that could refute the concern.
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

| Severity | Meaning |
|---|---|
| P0 | Immediate critical failure, such as broad durable data loss or a release that cannot start. State the demonstrated scope. |
| P1 | Major correctness, recovery, determinism, or required-gate failure. Resolve before merge. |
| P2 | A concrete defect or material contract gap under a supported condition. Resolve before merge or obtain an explicit owner disposition. |
| P3 | An optional improvement with no broken required contract. It does not block merge. |

Severity describes impact and urgency. It does not replace evidence or the project gate.
Do not reduce severity because the patch is small or the author calls the change safe.
Quote both statements when owner decisions conflict. File the question in `docs/questions.md` and stop dependent work (D-124, D-138).

## Review record

Use one file per PR in `docs/reviews/` (D-101). Reuse its existing name and finding ids on repeat reviews.
For a new record, use `docs/reviews/pr-<number>.md` with the actual PR number, not the roadmap id.
Record provider names only in the permitted review record and handoff author fields (D-137).
Omit those names from any PR description or GitHub comment.

The `review-gate` job reads this file (D-179, D-181, D-185). Three parts of it are machine-read. Keep their format exact:

| Part | Exact form | Rule |
|---|---|---|
| The file name | `docs/reviews/pr-<number>.md` | The number is the GitHub PR number, not the roadmap id. |
| The head field | `- Head: ` and the hash in backticks, in the Identity list | The hash is the effective head. A short hash is permitted. |
| The verdict | One of the three verdict names, in the `## Verdict` section | Write the name exactly. Do not reword it. |

The effective head is the newest commit that changes a path outside the metadata set (D-184).
The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`.
A commit that changes only those paths is a metadata commit, and it does not change the effective head.
The required review commit holds the review record and the handoff entry, so it is always a metadata commit (D-182).
Without that rule the review commit would invalidate the review that it publishes.
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

One subsection per finding, in severity order. Use the finding format below.
Write "No finding." when the review found none.

## Verification

One line per command or check, with its result.
Name each check that did not run and the reason.

## Open questions and accepted risks

Name each open OQ-# and each accepted risk with its D-# id.

## Verdict

**<Blocked | Changes required | Ready for owner merge>.** This verdict applies to head `<sha>`.
Give the reason in one or two sentences.
```

## Verdicts

| Verdict | Required condition |
|---|---|
| Blocked | Provider independence, the review target, a necessary owner decision, or required evidence remains unresolved. Record any verified defects too. |
| Changes required | The eligible review found defects or contract violations that need correction. List the required changes. |
| Ready for owner merge | The provider gate passes, the complete scope has review coverage, all required checks pass, and no blocking finding remains. |

No findings does not mean no risk. State material limits without a claim of zero regressions.
Approval applies only to the recorded revision. A new base or head requires assessment of the changed scope and evidence.
The owner alone merges the PR (D-102, D-126).

When the review record enters the PR, retain the assessed implementation head in that file.
Check any later metadata commit before the final verdict.
Do not require the review file to contain its own commit hash.
A metadata commit cannot hide code, content, requirement, or test changes.

## Finding format

Give each finding a stable id: the letter `P`, the severity number, a hyphen, and an index. `P1-1` is the first P1 finding.
Keep the id for the life of the PR. Never renumber a finding on a repeat review.

```markdown
### P<severity>-<n>: <short title that states the defect>

Status: <open | fixed in `<sha>` | accepted risk, D-# | withdrawn>.

File: `<path>:<line range>`, or Commit: `<sha>`.

Trigger: the input or state that produces the defect.

Expected: the required behavior, with the contract, tenet, guardrail, or D-# id.

Actual: the observed behavior.

Consequence: the effect on the player, the data, the build, or the maintainer.

Correction: the smallest change that restores the contract.

Regression check: the command or test that establishes the fix, and the result that must appear.
```

A withdrawn finding stays in the file with the evidence that refuted it. Never delete a finding.

## Do not raise a tool name as attribution

T-6 and D-137 prohibit text that names an agent, harness, or model **as the source of the work** (D-176).
A tool name that identifies a configured file, a schema, or a verified version is not attribution.

| Raise it | Do not raise it |
|---|---|
| A commit body that says an agent wrote the change. | The path `.claude/settings.json`. |
| A co-author trailer or a generation line. | A decision that names the schema it was verified against. |
| A PR description that credits a model. | A document that records which tool rejected a file. |

Apply the same test to every file before a finding. A reading that condemns the decision register is too broad.

## The review gate check

PR-1 adds a `review-gate` check (D-179, D-181, D-185). It applies three rules:

1. `docs/reviews/pr-<number>.md` exists for the PR number.
2. The verdict is `Ready for owner merge`.
3. The head in the Identity list is the effective head.

The check has three states. Read the color before you start:

| Color | Meaning | What to do |
|---|---|---|
| Grey | No review record exists for this PR. | Write one. This is the normal state before a review. |
| Red | A review record exists, and it does not approve this head. | Read the findings. The author corrects them. |
| Green | An approved review covers the effective head. | The owner may merge (D-102, D-126). |

Grey appears only while the check is advisory. At launch the same case turns red (D-181, D-185).
GitHub counts a neutral conclusion as a success for a required check, so enforced mode never uses grey.
The check is advisory until launch, because GitHub locks branch protection on a private free repository (D-170, D-180).

Rule 3 fails when the author pushes code after the approval. That result is correct.
Reassess the new diff, then update the head field and the verdict together.
Rule 3 does not fail when the last commit changes only the metadata paths (D-184).

## Repeat review procedure

Do these steps in order after the author revises the PR.

1. Read the response file when one exists.
2. Check the provider gate again. A reviewer fix changes eligibility.
3. Read the new head, the new base, and the diff since the reviewed head.
4. Verify each claimed fix against its original trigger and its regression check.
5. Set the `Status` line of each prior finding. Keep every id and every piece of evidence.
6. Inspect the new diff for new defects and affected consumers.
7. Add any new finding with the next index in its severity.
8. Update the Identity list to the new effective head.
9. Update the Verification section with the commands that ran on the new head.
10. Write the verdict against the new head.
11. Commit the review record and the handoff entry together, then push to the PR branch (D-182, D-183).

Edit the existing `docs/reviews/pr-<number>.md`. Do not create a second file for the same PR.
Do not delete the prior verdict. Replace it, and keep each finding and its history.
Close a finding only when the evidence establishes the fix or an owner decision resolves it.
Record any required check that still waits for a result.

## Address review findings

Use this section when you answer a review. The author does this work, not the reviewer.

**A finding is a claim, not a fact.** A review can be wrong. Assess each finding against the evidence before you change anything. A finding carries no authority that the evidence does not give it.

1. Read the finding, then read the file and the lines it names.
2. Reproduce the trigger. A finding that does not reproduce has no merit.
3. Read the contract the finding cites. Check the `Effect` column of `docs/decisions.md` for a later revision.
4. Decide the disposition: full merit, partial merit, or no merit.
5. Correct every finding that has merit. Use the smallest change that restores the contract.
6. Record each disposition in `docs/reviews/pr-<number>-response.md`.
7. Commit and push the response, the corrections, and the handoff entry (D-182, D-183).

Push back when the evidence supports it. State the reason and show the proof:

| Reason to push back | What to show |
|---|---|
| The finding reads a rule too broadly. | Quote the rule. Name the other files that the broad reading also condemns. |
| The finding cites a superseded decision. | Quote the `Effect` column and name the current decision. |
| The trigger does not reproduce. | Give the command, the revision, and the result. |
| The correction breaks another contract. | Name the contract and the caller that it breaks. |
| The finding states a style preference. | Name the contract that the code does not break. |
| The finding repeats a risk that a decision already accepted. | Quote the D-# id and its accepted risk. |

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

## Commit the record

Always commit the review record and the session handoff, then push them to the PR branch (D-182, D-183). Do it in the session that writes them.

| After | Commit these files | Who commits |
|---|---|---|
| A review or a repeat review | `docs/reviews/pr-<number>.md` and `docs/session-handoff.md` | The reviewer |
| Work that answers a review | `docs/reviews/pr-<number>-response.md`, each corrected file, and `docs/session-handoff.md` | The author |

Make one commit that holds the record and its handoff entry. Never leave either file uncommitted or unpushed.
A push is the only way `review-gate` sees the record, because the gate reads the PR head (D-183).

An uncommitted review record has three effects:

- The next commit from the other provider absorbs it, and the history no longer shows who wrote what.
- An author can commit an approval that the author never read, and then report the wrong verdict.
- `review-gate` cannot read the record, because the record is not on the PR head (D-179, D-181, D-185).

Write the commit message in an impersonal voice. Name no provider, agent, harness, or model (T-6, D-176).
Add the handoff entry at the top of the file, as a new entry (D-146).
Another provider can add an entry above yours while you work. Add your own entry. Never append to an older one, and never edit theirs.

## Scope limits

A review request authorizes inspection, verification, the review record, and the handoff entry.
It requires a commit of those two files, and a push of that commit to the PR branch (D-182, D-183).
It does not by itself authorize a code fix, a merge, or an external message.
A reviewer never pushes to `main` (D-170).
Honor explicit authorization already present in the session.
If the reviewer writes a substantive fix, reassess provider eligibility. The reviewer cannot approve its own contribution.
Do not disguise a fix as review metadata to bypass the provider gate.
