# Response to the repository audit

Date: 2026-09-07
Response author: Claude Code
Audit: `2026-09-07-repository-audit.md`, reviewer Codex
Status: every finding has a disposition. The owner made the decisions on 2026-09-07.

This file records the disposition of each audit finding (T-4). The R-# ids belong to the audit. The F-# ids are the design register entries. The D-# ids are the owner decisions.

## Merit

All nine findings have merit. Five are contract conflicts in the v2 design text (R-1, R-2, R-3, R-4, R-7). Two are gaps (R-5, R-6). One is a fact error (R-8). One is a sequence error (R-9). None was rejected.

## Dispositions

| Audit id | Merit | Register | Decision | Change made |
|---|---|---|---|---|
| R-1 | Accepted. The gate required checks that PR-2 and PR-3 create | F-28 | D-148 | PR-1 adds the CI skeleton. The gate names an absent check with the PR that creates it. G-19 added. PR-2 and PR-3 gates updated |
| R-2 | Accepted. Four gates preceded their prerequisites | F-29 | D-149 | PR-7 adds the player box. PR-9 adds the stairwell transition. PR-11 ships two policies, and PR-16, PR-17, PR-18 add theirs. PR-12 smoke reduced, PR-18 extends it. PR-57 asset QA v1 added after PR-13, PR-49 becomes v2. PR-32 precedes PR-27 |
| R-3 | Accepted. The record omitted the initial state and the versions | F-30 | D-151 | Run record header defined. Replay ignores the live bank and tree. Exact resume on a match, floor start on a mismatch. G-20 added. PR-6 and PR-31 updated |
| R-4 | Accepted. Three atomic files did not make one atomic save | F-31 | D-152 | D-94 revised: one profile file plus one run record. One write commits a run result. A run id makes completion idempotent. Write-boundary tests in PR-31 |
| R-5 | Accepted. No first loadout and no empty-bank path | F-32 | D-153 | A basic kit is always available and never lost. The first orb is unlocked. PR-28 and PR-30 updated with a fresh-profile test |
| R-6 | Accepted. The economy gate had no reproducible comparison | F-33 | D-154 | M-5 protocol defined: matched trials in simulated time with a hub cost and a bootstrap interval. PR-27 gate updated |
| R-7 | Accepted. Phase 1 could not be playable | F-34 | D-150 | D-136 revised: Gate 1 is a foundation gate. Phase 1 heading and roadmap intro updated |
| R-8 | Accepted. The header split Godot into 4.7.1 and 4.7.2 | F-35 | none | Header corrected from the source page, fetched 2026-09-07. The refuted text stays with a dated note |
| R-9 | Accepted. Section 8 put the palette before PR-1 | F-36 | none | OQ-1 blocks PR-14 only. Section 8 rewritten with a dated note |

## Findings added by this response

- F-37: HEAD points at the unborn branch `docs/repository-audit`. The first commit would miss `main` (D-126). OQ-30 asks the owner to reset HEAD before PR-1.

## Owner decisions in the same session

- D-144: a separate `docs/questions.md` holds the open questions register.
- D-145: the external SSD is ordered and arrives 2026-09-08.
- D-146: the handoff keeps the 10 newest sessions. Older entries move to `docs/session-handoff-archive.md`.
- D-147: address every audit finding before the focused roadmaps start.

## Documents changed

- `docs/design.md`: header, sections 3.2, 3.6, 3.8, 3.9, 3.14, 4, 5, 6.2, 7, 8, and 9.
- `docs/decisions.md`: D-144 to D-154, and effect notes on D-94, D-114, D-118, D-120, D-121, D-125, D-127, D-135, D-136, D-142.
- `docs/questions.md`: created from the former section 9. OQ-1, OQ-18, OQ-24 to OQ-29 annotated. OQ-30 added.
- `CLAUDE.md` and `AGENTS.md`: read order, T-5, the question rule, a handoff section, and three gate lines.
- `.claude/skills/design-doc-style/SKILL.md` and `.claude/skills/ste-writing/SKILL.md`: the questions file and new terms.
- `docs/session-handoff.md`: converted to the ten-session format. `docs/session-handoff-archive.md` created.
