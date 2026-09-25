# Full repository review prompts

Use Prompt A for a read-only review. Use Prompt B in a later implementation session with the review report from Prompt A.

## Prompt A: Read-only repository review

```text
Conduct a full-scale, intense, senior-principal-level review of the complete What You Carry repository.

Treat this as an independent technical audit of the current repository state. Review the whole system, not only recent changes. Read the current branch, all tracked source and test files, project instructions and skills, design and decision records, roadmaps, review records, build and release workflows, tools, assets, content, and generated-file rules. Trace important contracts across their callers, consumers, tests, CI jobs, and documentation.

The review is strictly read-only. Do not edit, create, delete, format, stage, commit, or stash repository files. Do not change branches, tags, remotes, GitHub state, issues, pull requests, comments, labels, secrets, or settings. Do not run commands that write into the repository. If a needed check can write files, use a temporary directory outside the repository or report that check as not run. Do not fix findings. Save the final report outside the repository, such as `/tmp/what-you-carry-repository-review.md`. Report the exact output path.

Start by reading the newest `docs/session-handoff.md` entry alone, then read the applicable instructions in `AGENTS.md` and `CLAUDE.md`. Follow the repository read order. Load the relevant project skills before reviewing their areas. In particular, load `pr-review`, `ste-writing`, and `csharp-conventions`. Read design section 6 in full. Search the decision and question registers by relevant ids. Do not read either register in full.

Record the date, branch, commit, base commit if known, dirty-state summary, review scope, and tools or checks used. Do not infer a clean state from a short status display. Separate verified defects, contract gaps, unresolved owner questions, risks, and optional improvements. Cite exact paths and line numbers. Confirm each potential defect against call sites, validation, tests, and later decisions before you report it.

Review these areas at minimum:

1. Architecture, dependency direction, project boundaries, and public interfaces.
2. Simulation determinism, seeds, ticks, concurrency, replay, and cross-platform bit identity.
3. Gameplay rules, state transitions, combat, enemy behavior, world generation, and failure recovery.
4. Persistence, save compatibility, atomicity, migration, corruption handling, and durable data loss.
5. Content schemas, validation, identifiers, balancing, assets, texture layout, audio, and localization rules.
6. Error handling, observability, diagnostics, logging, and silent failure paths.
7. Security and trust boundaries in tools, workflows, build inputs, artifacts, permissions, and supply chain.
8. Performance and memory risks on the Steam Deck floor. Recommend optimization only when evidence supports it.
9. Test quality, missing boundary cases, flaky tests, test isolation, and the link between each gate and the behavior it claims to prove.
10. Build, CI, release, night-gate, smoke, documentation, review, and repository-maintenance workflows.
11. Design, roadmap, decision, question, handoff, review, and agent-instruction consistency.
12. Usability, accessibility, and player-facing failures that the repository contracts can substantiate.

Run focused checks that can safely preserve the read-only requirement. Do not treat a green broad suite as proof that a contract is correct. Use targeted reproductions or trace the full causal path for high-impact claims. Record exact commands, outcomes, limits, and unavailable checks. A timeout, missing tool, permission limit, or skipped check is incomplete evidence, not a pass or a product defect.

Write the report in Markdown with this structure:

# Repository review

Date, revision, branch, scope, and read-only statement.

## Executive summary

State the overall risk, the most important verified issues, and material evidence gaps. Do not claim a clean bill of health when checks remain incomplete.

## P0 findings
## P1 findings
## P2 findings
## P3 findings

Order findings by severity, then by practical impact within each severity. Omit empty severity sections or state `None found.` Give every finding a stable id such as `RR-P1-1`. Do not reuse ids.

Each finding must include:

- Severity and concise defect title.
- Status: verified, unresolved question, or optional improvement.
- Exact file and line range, plus the reviewed revision.
- Trigger or supported condition.
- Expected behavior and its contract, test, design rule, or decision id.
- Observed behavior and concrete consequence.
- Reproduction, causal trace, or other evidence.
- Smallest correction that restores the contract.
- Regression test or verification needed to prove the correction.
- Related findings, when they share one cause.

Use these severity definitions:

- P0: immediate critical failure with demonstrated broad durable data loss, a release that cannot start, or an equivalent catastrophic outcome.
- P1: major correctness, security, recovery, determinism, or required-gate failure.
- P2: concrete defect or material contract gap under a supported condition.
- P3: useful improvement that does not break a required contract.

Do not lower severity because a change seems small. Do not assign severity to an out-of-scope concern. Do not invent findings to fill a section. Group repeated symptoms under one cause. Mark uncertain claims as risks or questions, not verified defects. Quote conflicting owner instructions and decisions, cite their ids, and do not choose between them.

Finish with:

## Coverage and evidence gaps

List reviewed areas, skipped areas, unavailable checks, and why each limit matters.

## Cross-cutting themes

Group systemic causes without duplicating findings.

## Suggested implementation order

Order work by dependency and risk. Keep independent fixes separate. Identify owner decisions that block work.

## Review conclusion

State what the evidence supports and what it does not support. Confirm that no repository or remote state changed. Give the report path.

Do not implement any recommendation in this session.
```

## Prompt B: Implement findings from a repository review

```text
Implement the approved fixes and improvements from this repository review report: <absolute path to the report from Prompt A>.

Treat the report as input evidence, not as authority to bypass current repository rules. First read the report, then read the newest session handoff and current `AGENTS.md` and `CLAUDE.md`. Check the current branch and working tree. Read the matching review findings against the current code before changing anything. The repository may have changed since the audit. Reproduce each claimed defect or confirm its causal path at the current revision. Do not implement a finding that no longer applies without recording why.

This is an implementation session. Follow the current one-PR, one-session, one-role rules. Load `.claude/skills/one-pr-one-session/SKILL.md` before PR work, `.claude/skills/review-response/SKILL.md` when answering review findings, `.claude/skills/ste-writing/SKILL.md` before writing Markdown, and `.claude/skills/csharp-conventions/SKILL.md` before changing or reviewing C#. Load any other skill that covers the affected area.

Use the owner-approved disposition for each finding. The audit itself did not grant permission to change owner decisions. If a finding requires a choice that the decision register or questions register does not settle, add the exact question to `docs/questions.md` and stop the dependent work. Do not select a default. Record each owner answer in `docs/decisions.md` with the next id and date. Do not silently revise an existing decision.

Create a small implementation plan from the report. Group findings only when they share one cause and can pass one coherent gate. Respect the one-concern-per-PR rule. Do not create a PR that only records earlier PR work. Keep each correction narrow. Preserve existing behavior unless a cited contract requires a change. Do not optimize without a profile before the change and a measurement after it.

For each finding:

1. Reproduce it at the current revision or give the complete causal trace.
2. Identify the governing contract and all affected callers and consumers.
3. Make the smallest correction that restores the contract.
4. Add a regression test that fails on the old behavior.
5. Test the boundary beside the failing case.
6. Update design, decisions, questions, roadmaps, runbooks, skills, or agent instructions only when intent or a contract changes. Keep `AGENTS.md` and `CLAUDE.md` identical.
7. Run the required local checks for the changed paths. For a documents-only change, follow D-491 and D-492. For any other path, follow D-493. Do not report a stalled, interrupted, skipped, or unavailable check as passed.
8. Re-read the final diff for scope, attribution, stale claims, generated files, and missing tests.

Preserve every report finding id in the implementation notes. For each one, record one disposition: fixed with commit and verification evidence, no longer applicable with current evidence, unresolved and blocked by an owner question, accepted risk with the required owner decision, or deferred with a clear reason. Do not mark a finding fixed based only on a passing broad suite.

Do not expand the work into unrelated cleanup. If the report identifies a broad theme, fix only the concrete findings approved for this session. If a new defect appears, add it to the appropriate review record or ask the owner to scope it. Do not quietly enlarge this implementation.

Before any push or merge action, follow the current repository instructions and obtain the owner confirmation those instructions require. Do not send messages, publish comments, change remote state, or merge unless the current owner instruction explicitly authorizes that action.

At completion, report the changed files, findings addressed, tests and checks with exact outcomes, remaining findings, owner questions, and any evidence limits. Keep the report tied to the exact resulting commit. Do not claim completion while required work or a blocking owner answer remains.
```
