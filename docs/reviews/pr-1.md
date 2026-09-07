# PR-1 review

Date: 2026-09-07

## Identity

- PR: 1
- Target: `main`
- Base: `1c16c45`
- Merge base: `1c16c45`
- Head: `6e45d6f`
- Branch: `docs/roadmaps`

## Provider gate

The session handoff records Claude Code as the author of the substantive roadmap and document changes. The active reviewer is Codex. The providers differ, so T-4 and D-101 pass.

## Intended behavior and scope

This PR adds the five focused roadmaps, the macOS runner runbook, repository settings, decision updates, question updates, review records, the review skill format, and session continuity records. The roadmaps define the scope, checks, gates, dependencies, and owner questions for PR-1 through PR-58.

The review inspected the complete diff from `1c16c45` to `6e45d6f`, the response and new diffs since `9459534`, all changed files in context, the current design and decision registers, the question register, the existing audit records, the project agent files, the required review and STE skills, and the runner procedure.

Affected contracts include T-4, T-5, T-6, D-118, D-137, D-146, D-148, D-150, D-151, D-152, D-157, D-170, D-173, D-175, D-181, D-182, and D-183.

## Findings

### P1-1: The PR commit contains prohibited attribution

Status: fixed in `e3e2b6a`.

Commit: `9459534`

Evidence: the commit subject is `fix: set the attribution fields to empty strings (D-175)`. Its body names the "Claude Code startup dialog" and says that the harness ignored the settings file.

Expected: T-6 and D-137 prohibit agent, harness, and model attribution in commits. The only exemptions are the session-handoff author field and files in `docs/reviews/`.

Actual: the commit body names Claude Code and the harness. The settings fix does not remove this historical commit text from the PR.

Disposition: partial merit under D-176. The body of `9459534` implied a source of the work. The broader interpretation does not apply to tool names that identify a configured file, schema, or verified version.

Consequence on the reviewed revision: none. The amended commit body does not make that attribution claim.

Correction applied: amend the commit body and add the source-of-work attribution rule to the PR-1 exit test and review skill.

Regression check: scan every commit in the PR for a source-of-work claim, co-author trailer, or generation line. The author reports one permitted configured-file path and no source-of-work claim. The result was not independently reproduced by a structured parser because no implementation exists yet.

### P1-2: `night-gate` has no first-run result

Status: fixed in `abd2af7`.

File: `docs/roadmaps/phase-1-foundations.md:349-367`

Trigger: PR-11 creates a `night-gate` job that reads the latest scheduled night result. PR-11 is the first PR that creates the scheduled night job and the gate.

Expected: D-148 and G-19 require a PR that creates a check to pass that check. D-115 requires a failed night to block the next merge. The initial run needs a defined, reproducible result before the new gate can pass.

Actual: the roadmap defines behavior only when a prior night result exists. It does not define what happens when the result store is empty. The PR-11 gate can therefore fail forever before the first scheduled run, or pass without evidence if the workflow treats an absent result as success.

Disposition: full merit under D-177. PR-11 now publishes the result, and PR-58 creates the gate after one scheduled night exists.

Consequence on the reviewed revision: the first-run case has an explicit sequence and missing, stale, cancelled, and failed result cases.

Correction applied: split the night job and `night-gate` into PR-11 and PR-58. The gate rejects absent, stale, cancelled, and failed records.

Regression check: PR-58 specifies fixture tests for the four failed cases and a real night record. No workflow exists yet, so execution is deferred to PR-11 and PR-58.

### P1-3: PR-1 has no way to set the required review-gate mode

Status: open.

File: `docs/roadmaps/phase-1-foundations.md:57-76`

Trigger: PR-1 creates the `review-gate` workflow. The workflow reads the repository variable `REVIEW_GATE_MODE`, and the roadmap says PR-1 sets that variable to `advisory`.

Expected: D-181 requires an explicit `advisory` or `enforced` value. An absent or unknown value must fail. PR-1 must therefore provide a valid variable before its review-gate check can pass.

Actual: the PR-1 scope only adds repository files and workflow permissions. It does not add an owner setup step, a runbook command, or an API permission that can create the repository variable. GitHub repository variables are external repository state, and `checks: write` plus `contents: read` cannot create one. The roadmap also does not state how a fresh repository gets `REVIEW_GATE_MODE=advisory` before the workflow runs.

Consequence: a fresh PR-1 checkout reaches the explicit absent-variable failure path. The new review-gate check cannot enter its documented grey advisory state or pass its own exit tests until an undocumented external action occurs.

Correction: add the owner setup action before PR-1, with a documented command that creates `REVIEW_GATE_MODE=advisory`, or add a separately scoped bootstrap mechanism. Keep the absent and unknown values as failures.

Regression check: run the workflow with the variable absent, set to `advisory`, set to `enforced`, and set to an unknown value. The first and last cases must fail. PR-1 must document and perform the advisory setup before its real check run.

### P1-4: The required review commit invalidates its own effective head

Status: open.

Files: `.claude/skills/pr-review/SKILL.md:200-202,333-365`, `docs/decisions.md:202,205`, and Commit: `6e45d6f`.

Trigger: D-182 and D-183 require the reviewer to commit and push `docs/reviews/pr-1.md` together with `docs/session-handoff.md`. The effective-head rule excludes only paths under `docs/reviews/`.

Expected: the review record commit and its required handoff commit must be metadata and must not invalidate the review that they publish (D-179, D-182, D-183).

Actual: commit `6e45d6f` changes both the review record and `docs/session-handoff.md`. Under the current effective-head definition, the handoff path is outside `docs/reviews/`, so `6e45d6f` becomes the effective head. The review record in that commit records `8efb267`, and the gate therefore rejects the record. Updating the head and committing the required handoff repeats the same problem with a new commit hash.

Consequence: the review protocol cannot produce a green `review-gate` result after a compliant reviewer commit. The required commit and push path makes the gate self-invalidating.

Correction: define metadata paths consistently. Exclude both `docs/reviews/` and `docs/session-handoff.md` from the effective-head computation, or define a metadata commit by its exact required paths. Update D-179, D-182, D-183, the skill, and the PR-1 exit tests together.

Regression check: create a fixture commit that changes the review record and the handoff only. The effective-head command must return the last substantive commit. Create a later commit that changes any other documentation path. The command must return that later commit.

### P2-1: The design source still presents superseded decisions as current

Status: fixed in `abd2af7`.

Files: `docs/design.md:12`, `docs/design.md:300`, `docs/design.md:572`, `docs/roadmaps/phase-1-foundations.md:68`, `docs/roadmaps/phase-1-foundations.md:384`

Trigger: the PR revises D-169 to D-173 for the .NET pin and D-172 to D-175 for the attribution settings.

Expected: the design, roadmap, question register, and decision register must state one current answer per contract (D-118 and D-139). D-173 makes .NET 10 LTS the current pin. D-175 makes empty strings the current attribution setting.

Actual:

- `docs/design.md:12` still says that the .NET LTS version is not verified.
- `docs/design.md:300` and `docs/design.md:572` still cite D-172 for the attribution option and OQ-2 resolution.
- `phase-1-foundations.md:384` still marks OQ-16 with D-172.
- `phase-1-foundations.md:68` uses D-172 for the attribution gate.

The decision and question registers contain the later revisions, so readers receive conflicting instructions depending on the file they read.

Disposition: full merit under D-178. The six current-status references were corrected, and PR-2 now gains a reference check.

Consequence on the reviewed revision: the cited design and roadmap references agree with D-173 and D-175. Historical references remain in review and history records.

Correction applied: update the six references and add the PR-2 reference check.

Regression check: the author reports a clean stale-reference sweep. The reference checker does not exist yet, so the repository check was not independently executed.

### P2-2: The Phase 1 roadmap header omits its new PR and decision scope

Status: fixed in `b1b772a`.

Files: `docs/roadmaps/phase-1-foundations.md:3,9`

Trigger: the roadmap adds PR-58 and the D-177 and D-178 corrections, but its status line and correction-pass line retain the earlier scope.

Expected: the roadmap header must identify every PR and decision that the roadmap governs (D-118, D-146). Its status must describe the current correction state.

Actual: line 3 says the file expands "PR-1 to PR-11, M-1, and M-2" and applies only D-148 to D-152 and D-156. The roadmap now also contains PR-58, D-177, and D-178. Line 9 says `Correction passes: none yet`, although the file contains the D-176 to D-178 correction pass.

Disposition: full merit under D-178. The correction added the missing scope and status. The follow-up sweep also corrected related revised-decision ranges in the other roadmaps.

Consequence on the reviewed revision: none. The five roadmap headers now state their current scope, and the affected revised-decision references name their revising decisions.

Correction applied: update the Phase 1 and Phase 5 headers, correct the related decision ranges and references, and refine D-178 to accept a line that names its revising decision.

Regression check: the author reports zero omitted PRs, measurements, and revised ids across all five roadmaps. The current manual sweep found no remaining header omission or unmarked revised-decision citation.

## Verification

- `git diff --check main...docs/roadmaps`: passed.
- `cmp -s AGENTS.md CLAUDE.md`: passed.
- JSON parse of `.claude/settings.json`: passed.
- Roadmap header and revised-decision sweep: passed on `6e45d6f`.
- Effective-head specification review: passed. The follow-up names a single `git log` command with a pathspec and avoids an early-terminating pipeline.
- Commit attribution scan: the old finding was reproduced against `9459534` and is fixed in amended commit `e3e2b6a`. A semantic source-of-work scan was not independently automated because the PR creates the scanner only as a future exit test.
- Build and test: not run. The repository has no solution or implementation on either the base or reviewed head, as stated in `AGENTS.md`.
- STE checker: not run. PR-2 creates it, and the project has no checker yet. The changed documents received a manual review against `.claude/skills/ste-writing/SKILL.md`.
- CI results: unavailable for the reviewed head in the local checkout. The workflow files described by the roadmap do not exist yet. The review-gate mode variable is also not present in repository files or the runner runbook.
- Review-gate metadata-path behavior: failed by causal trace. The current definition excludes `docs/reviews/` but not the required handoff path.
- Runner registration and SSD placement: not verified. The runbook records them as owner actions before PR-1.

## Open questions and accepted risks

No new owner question is required for these findings. The existing OQ-12 remains open for PR-9, as the handoff and Phase 1 roadmap state. The first-run behavior for `night-gate` is an implementation contract that the roadmap must define before PR-11.

## Verdict

**Changes required.** This verdict applies to head `6e45d6f`. P1-1, P1-2, P2-1, and P2-2 are fixed. P1-3 and P1-4 remain open because the review-gate mode has no setup path and the compliant review commit invalidates its own effective head.
