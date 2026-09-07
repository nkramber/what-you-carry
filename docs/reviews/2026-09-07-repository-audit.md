# Repository audit

Date: 2026-09-07
Reviewer: Codex
Scope: the local repository before its first commit.
Status: review complete. This file does not approve a PR or change an owner decision.

The repository initially contained eight files: the design, decisions, handoff, archive, two agent files, and two skills.
No code, solution, content, tests, CI workflows, or focused roadmaps exist.
The lack of code agrees with the current phase. These findings concern the plan and its contracts.

P1 means resolve before the affected foundation or feature starts. P2 means resolve before its feature gate. P3 means document correction.
The R-# ids belong only to this review.

## R-1. P1: The first PR cannot meet the merge gate

Evidence: [the agent PR gate](../../AGENTS.md#pr-gate), lines 77 to 82, and [the roadmap](../design.md#7-roadmap), PR-1 to PR-3.

The gate states, "A PR merges only when every line holds".
It requires the bit-identity job, lint tool, and STE checker.
The roadmap creates the STE checker in PR-2 and the other two checks in PR-3.
Section 8 requires PR-1 and PR-2 first. G-14 defers STE enforcement, but the agent gate has no such exception.
The first two PRs therefore cannot satisfy the stated merge rules.

Recommendation: define the checks for each initial PR, or move all required checks into the scaffold.
Keep tests and independent review mandatory. OQ-25 holds the owner choice.

## R-2. P1: Several feature gates precede their prerequisites

Evidence: [the roadmap](../design.md#7-roadmap), PR-11, PR-12, PR-22, PR-27, PR-32, and PR-49.

| Required check | Prerequisite scheduled later | Effect |
|---|---|---|
| PR-11 completes bot runs with a timer-tester policy | Player in PR-15, timer in PR-17, transitions in PR-18 | Full run checks lack the systems they exercise |
| PR-12 smoke session completes one floor and a stairwell | Stairwell choice and transition in PR-18 | The gate requires behavior outside PR-12 |
| PR-22 checks every armor pose | Asset QA tool in PR-49 | The plan requires the pose check before it creates the checker |
| D-128 requires Tier 3 on every economy PR, including PR-27 | Play socket in PR-32 | The first economy PR lacks its required review path |

Section 8 requires strict serial order. No entry defines temporary fixtures or reduced early checks.
An implementation must expand earlier scope, weaken a gate, or change the order.

Recommendation: put each prerequisite before its first consumer. Define early fixtures where the gate tests only a partial system.
OQ-25 holds the sequence choice. D-114, D-115, D-128, and D-135 still apply.

## R-3. P1: The replay contract omits the initial state

Evidence: [persistence](../design.md#39-hub-and-persistence), lines 112 to 114, G-5, and PR-6, lines 295 to 297.

The record contains the seed and intents from the first tick.
D-2 also permits a bank loadout. D-35, D-39, and D-51 permit different skill buffs and amulet abilities before a run.
Two players can use the same seed and inputs with different weapons or buffs. Their damage and end states differ.
The specified record cannot reconstruct that difference on a clean machine.

The contract also omits the content identity and simulation version. A balance change can alter a suspended run despite a valid save schema.
Save schema migration alone does not define replay compatibility.

Recommendation: include an immutable initial state and explicit format, simulation, and content identities.
Test a replay after the live bank and tree change. Test a content mismatch with a contextual report.
OQ-26 holds the compatibility choice. D-69 and D-97 require this contract before replay becomes the save mechanism.

## R-4. P1: Atomic files do not define an atomic save transaction

Evidence: [persistence](../design.md#39-hub-and-persistence), line 112, and PR-31, lines 433 to 436. D-94 requires three JSON files.

Ascension changes the bank, tree, and suspended run. Each file can remain valid while the set becomes inconsistent.
For example, a crash after the bank update but before run completion can leave both the reward and a resumable run.
A repeat ascension could grant the reward again. A different write order could lose the reward.
This is a contract gap, not an observed code defect.

PR-31 tests a mid-floor process kill and a corrupt file. It does not test a crash between each durable update.
The design also omits how a partial intent record recovers after a process kill.

Recommendation: define one committed save generation and an idempotent rule for run completion.
Test process failure at each write boundary for departure, ascension, death, and migration.
OQ-27 holds the save protocol choice. No database dependency is necessary to state this requirement.

## R-5. P2: A new or empty bank has no defined recovery path

Evidence: [the core loop](../design.md#32-core-loop), line 70, and PR-30, lines 428 to 431. D-2 removes carried gear on death.

The player selects a loadout from the bank, but no rule supplies the first loadout or handles the loss of the last weapon.
The permanent amulet does not settle this gap. Its abilities remain open in OQ-7, and the plan does not require a combat ability.
The full-loop gate covers gear loss but does not prove that another playable run can start after repeated deaths.

Recommendation: define the initial loadout and a recovery path after all banked gear is lost.
Test a fresh profile and repeated deaths until the bank is empty. OQ-28 holds the game design choice.

## R-6. P2: The economy gate lacks a reproducible comparison

Evidence: [the economy](../design.md#38-economy), line 108, PR-27, lines 413 to 416, and M-5, lines 446 to 447.

The gate says, "ascension beats death at every depth". OQ-21 asks only for the retained share.
No contract fixes the initial loadout, skill state, seed set, policy pair, time denominator, or pass threshold.
Reduced points alone do not prove a lower rate.
For example, 50 points in 30 seconds exceed 100 points in 120 seconds on a points-per-hour basis.
These numbers illustrate the test gap. They are not measured game results.

Recommendation: define matched trials, the elapsed-time rules, and the comparison statistic before the payout curve passes.
Include early death, full-clear, and repeated shallow ascension policies. OQ-29 holds the comparison choice.

## R-7. P2: Phase 1 conflicts with the milestone decision

Evidence: [D-136](../decisions.md#process), line 162, and [Phase 1](../design.md#7-roadmap), lines 266 to 273.

D-136 states, "Each is a playable build with a written exit test".
The design says the phases are those milestones. Phase 1 has no Game layer, which starts in PR-12.
Its gate checks CI and documents, so it cannot provide the stated playable build.
The owner cannot apply both gate definitions as written.

Recommendation: distinguish a foundation gate from a playable milestone, or revise the phase boundary.
OQ-25 includes this conflict. Neither interpretation became a decision during this review.

## R-8. P3: The verified release header is stale

Evidence: [the design header](../design.md), line 7.

The header names Godot 4.7.1 as stable and 4.7.2 as the .NET build.
The official macOS page lists both editions as 4.7.2, dated 2026-08-18.
The release exists. The defect is the split version claim, not the proposed 4.7.2 pin.
Sources checked on 2026-09-07: [Godot downloads](https://godotengine.org/download/macos/) and [the release archive](https://godotengine.org/download/archive/).

Recommendation: correct the header with a dated note and direct source links.
Recheck the toolchain at scaffold time under D-61 and D-62. This review did not select a .NET version.

## R-9. P3: The blocker summary gives two different scopes

Evidence: [section 8](../design.md#8-sequence-strict-order-single-owner), line 537, and [OQ-1](../design.md#9-open-questions), line 571.

Section 8 requires the palette answer before PR-1. OQ-1 says it blocks PR-14.
The prior handoff called it a first-code blocker while also citing PR-14.
A new session cannot tell whether palette approval intentionally gates the scaffold.

Recommendation: identify the earliest dependent PR for each question in OQ-25.
The new handoff preserves this conflict without selecting an interpretation.

## Verification and limits

- The file inventory and Git status confirm no code or tracked files. All project files remain untracked locally.
- The local repository has no commits. A remote URL exists, but this review did not verify remote history or backups.
- The agent files are byte-identical. The comparison command returned success (D-122).
- The decision register contains D-1 to D-143. This review added no owner decisions.
- No build or test ran because no solution or test project exists.
- The STE checker does not exist. The new text received a manual checklist review under the local skill.
- The review makes no claim about runtime performance, actual save corruption, or measured game balance.
- The accepted five-second rewind, F-17, is not a new defect.

## Document disposition

- `docs/design.md`: added OQ-25 to OQ-29 for the owner choices above. Existing decisions and roadmap entries remain as reviewed.
- `docs/session-handoff.md`: replaced with the audit state and next action (D-118, D-121).
- `docs/decisions.md`: no change needed because no owner answer arrived.
- `AGENTS.md`: no change needed because this review does not revise process rules.
- `CLAUDE.md`: no change needed because this review does not revise process rules.
- `.claude/skills/ste-writing/SKILL.md`: no change needed because this review uses the current text rules.
- `.claude/skills/design-doc-style/SKILL.md`: no change needed because this review uses the current document template.
- `docs/archive/design-v1-2026-09-06.md`: no change needed because the archive preserves the original design.
- `docs/roadmaps/`: no change needed because no focused roadmap exists and this task requested a review.

No commit or PR was created. The local repository lacks a committed base for a review PR.
