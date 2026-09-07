# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 3: 2026-09-07, Claude Code

Author: Claude Code
Session: audit remediation, no PR. The repository has no commits.

### What this session did, and why

- Assessed the nine findings of `docs/reviews/2026-09-07-repository-audit.md`. All nine have merit. Asked the owner seven questions and recorded D-148 to D-154.
- Wrote `docs/reviews/2026-09-07-repository-audit-response.md` with each disposition.
- Edited `docs/design.md`: header correction (R-8), F-28 to F-37, G-19 and G-20, roadmap entries PR-1, PR-2, PR-3, PR-6, PR-7, PR-9, PR-11, PR-12, PR-16, PR-17, PR-18, PR-22, PR-27, PR-28, PR-30, PR-31, PR-32, PR-49, M-5, a new PR-57, and section 8 rewritten.
- Created `docs/questions.md` as the open questions register (D-144). Section 9 of the design doc now links to it. Added OQ-30.
- Recorded D-144 to D-147 from the owner's message: the questions file, the SSD order, the ten-session handoff rule, and audit-first order. A Codex session added D-155 in parallel.
- Converted this file to the ten-session format (D-146) and created `docs/session-handoff-archive.md`.
- Updated `CLAUDE.md`, `AGENTS.md`, and both skills for the questions file, the handoff rule, and the gate clause.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists. No commits.
- HEAD points at the unborn branch `docs/repository-audit`. The first commit lands there unless the owner resets HEAD (OQ-30, F-37).
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The STE checker does not exist until PR-2. This session scanned the changed documents by script for semicolons, contractions, verb -ing forms, sentences over 25 words, and passive markers.

### In flight

Nothing is half done. Every audit finding has a decision and a document change.

### Traps and gotchas

- D-152 revises D-94: one profile file plus one run record, not three files. Cite D-152.
- D-150 revises D-136: Gate 1 is a foundation gate with no playtest.
- D-149 moves PR-32 before PR-27 and inserts PR-57 after PR-13. Section 8 is the order. Ids never change.
- The design header keeps the refuted Godot 4.7.1 text with a dated note. Do not delete it.
- The handoff now keeps 10 sessions. Add an entry at the top. Do not rewrite the file.
- The attribution rule stands (T-6). OQ-16 remains open.

### Open questions that block progress

`docs/questions.md` holds OQ-1 to OQ-30. PR-1 depends on OQ-2, OQ-16, OQ-30, and the SSD (D-145, arrives 2026-09-08).

### Next concrete action

The owner said the focused roadmaps begin once the audit findings are addressed (D-147). They are addressed. The next action is `docs/roadmaps/phase-1-foundations.md`: expand PR-1 to PR-11 and M-1 to M-2 with per-PR exit tests, under D-148 and D-149. Load `ste-writing` and `design-doc-style` first. Confirm with the owner that Phase 1 is the first roadmap.

## Session 2: 2026-09-07, Codex

Author: Codex
Session: repository audit, no PR. The local repository has no commits.

### What this session did, and why

- Examined the design, all 143 decisions, the archive, both agent files, and both local skills.
- Wrote `docs/reviews/2026-09-07-repository-audit.md` with nine findings, evidence, recommendations, and verification limits.
- Added OQ-25 to OQ-29 in `docs/design.md` section 9. These cover gates, replay state, save transactions, empty-bank recovery, and economy tests.
- Created the local branch `docs/repository-audit` under D-126. All project files remain untracked.
- Recorded the project skill location as D-155. Both agent files now require `.claude/skills/` for current and new project skills.
- Added direct access instructions for a required skill that is absent from the skill list.
- Added no code. No commit, PR, or merge occurred.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists.
- No build or test ran. The repository has nothing executable to check.
- `AGENTS.md` and `CLAUDE.md` are byte-identical. The comparison returned success (D-122).
- The STE checker does not exist until PR-2. The new text received a manual checklist review.
- The previous design interview produced `docs/design.md` v2 and decisions D-1 to D-143.
- The unchanged v1 archive remains at `docs/archive/design-v1-2026-09-06.md`.
- A Git remote URL exists. This session did not verify remote history or backups.

### In flight

The audit and skill location update are complete. New decisions D-144 to D-154 address several audit findings.
The design and agent rules still need the corresponding audit corrections. D-147 requires those corrections before focused roadmaps.

The audit identified four highest-priority findings:

1. The initial PRs require merge checks that PR-2 and PR-3 create later.
2. Several feature gates precede their test tools or game systems.
3. The replay contract omits the initial loadout, skill state, and simulation and content identities.
4. The save contract lacks a shared commit and recovery rule across the three files.

### Traps and gotchas

- The review file records design gaps, not observed runtime defects. No game code exists.
- The five-second rewind in D-97 is an accepted tradeoff, F-17. This audit does not reopen it.
- The Godot 4.7.2 release exists. The design header incorrectly separates stable 4.7.1 from .NET 4.7.2.
- Official links and the verification date appear in the audit. OQ-2 remains open for the .NET version.
- The agent merge gate and the roadmap disagree about the initial checks. OQ-25 records the conflict.
- Section 8 puts palette approval before PR-1. OQ-1 says it blocks PR-14. The owner must settle that scope.
- Keep the attribution rule in every future commit and PR (D-137). OQ-16 remains open.
- The owner owns every open question (D-124). Ask before a choice, and record each answer (D-138).
- Project skills live in `.claude/skills/`. Read their `SKILL.md` files directly when required (D-155).
- Decisions after D-143 revise the audit state. Consult the current decision register before an audit correction.
- D-97 supersedes D-10 and D-95. D-129 and D-132 revise D-120. D-141 keeps decisions in one file until about 300 rows.
- Review ids stay local to the review file. They do not enter the design finding register.

### Open questions that block progress

`docs/design.md` section 9 still contains the audit questions. D-144 moves the register to `docs/questions.md`.
D-148 to D-154 answer several audit issues. Reconcile the register with those decisions before the next owner question.

PR-1 still depends on OQ-2, OQ-16, and the SSD prerequisite in OQ-18. D-145 records the SSD order.
The palette prerequisite has conflicting scopes, as noted above.
D-151 to D-154 resolve OQ-26 to OQ-29. The affected roadmap entries need those contracts before implementation.

### Next concrete action

Continue the audit corrections under D-147. Apply the current decisions before new owner questions.
Keep the two agent files identical (D-122). Check the current register before each new decision id.

The prior sequence remains design v2, then focused roadmaps, then code.
The next planned artifact is `docs/roadmaps/phase-1-foundations.md`.
It needs per-PR exit tests for PR-1 to PR-11 and measurements M-1 to M-2.
Load the local `ste-writing` and `design-doc-style` skills before that work.

## Session 1: 2026-09-07, Claude Code

Author: Claude Code
Session: design interview, no PR. The repository had no commits.

### What this session did, and why

- Read the v1 design document and asked the owner 143 questions in batches. The owner wanted every assumption confirmed before any code.
- Recorded every answer in `docs/decisions.md` as D-1 to D-143.
- Wrote `docs/design.md` v2 from those decisions, in the design-doc-style template and in ASD-STE100.
- Moved the v1 document to `docs/archive/design-v1-2026-09-06.md` unchanged.
- Created `.claude/skills/ste-writing/SKILL.md` and `.claude/skills/design-doc-style/SKILL.md`, adapted to this project (D-131).
- Created `CLAUDE.md` and the identical `AGENTS.md` (D-122).
- Created the empty folders `docs/reviews/` and `docs/roadmaps/`.

### State of the build

- No code. No solution. No commits. The working tree held only documents, skills, and agent files.
- The STE checker did not exist. The session checked the documents by script.

### Traps and gotchas

- The harness default adds a co-author trailer to commits. T-6 forbids it (OQ-16).
- Every open question belongs to the owner (D-124).
- D-97 supersedes D-10 and D-95. D-129 and D-132 supersede the file layout in D-120.
- The owner named the game "What You Carry" (D-11). The v1 name "Descent" appears only in the archive.
- The design doc uses `M-#` for measurements. The five milestones are the roadmap phases, not `M-#` ids.

### Next concrete action at the time

A focused roadmap for Phase 1. Superseded by session 2, the audit.
