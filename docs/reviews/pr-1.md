# PR-1 review

Date: 2026-09-07

## Identity

- PR: 1
- Target: `main`
- Base: `1c16c45`
- Merge base: `1c16c45`
- Head: `9459534`
- Branch: `docs/roadmaps`

## Provider gate

The session handoff records Claude Code as the author of the substantive roadmap and document changes. The active reviewer is Codex. The providers differ, so T-4 and D-101 pass.

## Intended behavior and scope

This PR adds the five focused roadmaps, the macOS runner runbook, repository settings, decision updates, question updates, and session continuity records. The roadmaps define the scope, checks, gates, dependencies, and owner questions for PR-1 through PR-55.

The review inspected the complete diff from `1c16c45` to `9459534`, all changed files in context, the current design and decision registers, the question register, the existing audit records, the project agent files, the required review and STE skills, and the runner procedure.

Affected contracts include T-4, T-5, T-6, D-118, D-137, D-146, D-148, D-150, D-151, D-152, D-157, D-170, D-173, and D-175.

## Findings

### P1-1: The PR commit contains prohibited attribution

Status: open.

Commit: `9459534`

Evidence: the commit subject is `fix: set the attribution fields to empty strings (D-175)`. Its body names the "Claude Code startup dialog" and says that the harness ignored the settings file.

Expected: T-6 and D-137 prohibit agent, harness, and model attribution in commits. The only exemptions are the session-handoff author field and files in `docs/reviews/`.

Actual: the commit body names Claude Code and the harness. The settings fix does not remove this historical commit text from the PR.

Consequence: the PR fails the absolute attribution gate even though `.claude/settings.json` is valid JSON.

Correction: rewrite the commit message without provider or harness names, or recreate the PR history with an impersonal message. Add a check that scans subjects and bodies for prohibited attribution before merge.

Regression check: scan every commit in the PR, including the body, for `Claude Code`, `Codex`, `agent`, `harness`, model names, co-author trailers, and generation lines. The scan must return no finding.

### P1-2: `night-gate` has no first-run result

Status: open.

File: `docs/roadmaps/phase-1-foundations.md:349-367`

Trigger: PR-11 creates a `night-gate` job that reads the latest scheduled night result. PR-11 is the first PR that creates the scheduled night job and the gate.

Expected: D-148 and G-19 require a PR that creates a check to pass that check. D-115 requires a failed night to block the next merge. The initial run needs a defined, reproducible result before the new gate can pass.

Actual: the roadmap defines behavior only when a prior night result exists. It does not define what happens when the result store is empty. The PR-11 gate can therefore fail forever before the first scheduled run, or pass without evidence if the workflow treats an absent result as success.

Consequence: PR-11 cannot demonstrate its own required gate, and the merge decision can depend on an unspecified missing-result fallback.

Correction: add an explicit bootstrap path. For example, run the night command as a required job in PR-11, publish its result, and make `night-gate` require a matching successful result for the reviewed commit. Define the missing, stale, cancelled, and expired-result cases as failures.

Regression check: execute the workflow with no prior night result, with a result for an older commit, with a failed result, and with a successful result for the reviewed commit. Only the last case may pass.

### P2-1: The design source still presents superseded decisions as current

Status: open.

Files: `docs/design.md:12`, `docs/design.md:300`, `docs/design.md:572`, `docs/roadmaps/phase-1-foundations.md:68`, `docs/roadmaps/phase-1-foundations.md:384`

Trigger: the PR revises D-169 to D-173 for the .NET pin and D-172 to D-175 for the attribution settings.

Expected: the design, roadmap, question register, and decision register must state one current answer per contract (D-118 and D-139). D-173 makes .NET 10 LTS the current pin. D-175 makes empty strings the current attribution setting.

Actual:

- `docs/design.md:12` still says that the .NET LTS version is not verified.
- `docs/design.md:300` and `docs/design.md:572` still cite D-172 for the attribution option and OQ-2 resolution.
- `phase-1-foundations.md:384` still marks OQ-16 with D-172.
- `phase-1-foundations.md:68` uses D-172 for the attribution gate.

The decision and question registers contain the later revisions, so readers receive conflicting instructions depending on the file they read.

Consequence: the first implementation PR can select .NET 8, treat the invalid boolean settings as current, or reopen questions that the roadmap marks closed. This also makes the documented source of truth fail its purpose.

Correction: update every current-status reference to D-173 or D-175. Keep old ids only in revision history, and state clearly that the later decision supersedes the earlier one.

Regression check: run a repository reference check that reports current-status text for a superseded decision. Confirm that the design and all focused roadmaps agree with the `Effect` column of the latest decision.

## Verification

- `git diff --check main...docs/roadmaps`: passed.
- `cmp -s AGENTS.md CLAUDE.md`: passed.
- JSON parse of `.claude/settings.json`: passed.
- Commit attribution scan: failed because commit `9459534` names Claude Code and the harness.
- Build and test: not run. The repository has no solution or implementation on either the base or reviewed head, as stated in `AGENTS.md`.
- STE checker: not run. PR-2 creates it, and the project has no checker yet. The changed documents received a manual review against `.claude/skills/ste-writing/SKILL.md`.
- CI results: unavailable for the reviewed head in the local checkout.
- Runner registration and SSD placement: not verified. The runbook records them as owner actions before PR-1.

## Open questions and accepted risks

No new owner question is required for these findings. The existing OQ-12 remains open for PR-9, as the handoff and Phase 1 roadmap state. The first-run behavior for `night-gate` is an implementation contract that the roadmap must define before PR-11.

## Verdict

**Changes required.** This verdict applies to head `9459534`. The provider gate passes, but P1-1 and P1-2 fail required PR contracts. P2-1 must also be corrected so the roadmap and design have one current source of truth. Re-review the new head after the commit history, bootstrap behavior, and superseded references change.
