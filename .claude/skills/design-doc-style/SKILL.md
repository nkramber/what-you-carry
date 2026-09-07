---
name: design-doc-style
description: Section template and rules for docs/design.md, modeled on the connector-syncer document-summary-roadmap.md. Load before you edit the design doc or a focused roadmap.
---

# Design-doc style skill

The owner wants the design doc in the style of `/Users/nate/Repos/connector-syncer-docs/docs/document-summary-roadmap.md` (D-132). This skill gives the template. Write in ASD-STE100. Load `ste-writing` first.

The design doc is one file: `docs/design.md`. Its roadmap section is the high-level roadmap. Focused roadmaps are separate files in `docs/roadmaps/`. The roadmap section links to them.

## Section template

1. **Status header.** State the doc status, what it supersedes, and the date you verified each external fact. Add a dated line for each correction pass. Never delete a refuted claim. Mark it refuted and keep it.
2. **Thesis.** One paragraph. What the game is for and why the plan has this order.
3. **Lessons learned.** Numbered. Each lesson names the event that taught it. Carry lessons from connector-syncer when they apply.
4. **System map.** A table of components, what each reads, and its sensitivity.
5. **Cost model.** What we pay, what we do not know, and which measurement will answer it.
6. **Defect and finding register.** A numbered table. Findings carry evidence and dates. Findings bind to plan items ("binds PR-3"). The status legend:
   - ✅ done (code merged)
   - 🔧 planned (item listed)
   - ⚠ constraint (binds a pull request)
   - ❓ needs owner input
   - ⏸ out of scope (a decision parked it)
   - 🅿 parked
7. **Guardrails.** Numbered invariants that every PR must keep.
8. **Roadmap.** Phases. Each entry has an id (PR-#, M-#, I-#), a technical paragraph, and a gate. It ends with a plain-English paragraph in a block quote that starts with "*In plain English:*".
9. **Sequence.** A strict ordered list with a single owner. Mark the gate.
10. **Open questions.** A link to `docs/questions.md` (D-144). The register there is numbered. Record the date and the answer there when one arrives.

## Rules

- Every roadmap entry ends with a plain-English paragraph. The paragraph explains the item to a reader who does not know the code.
- Every external fact has a source and a date.
- The numbers continue across revisions. Never renumber.
- "One concern per pull request" applies to the plan items.
- A refuted premise stays in the doc with a dated correction.
- Ids: F-# findings, PR-# code changes, M-# measurements, I-# integrations, D-# owner decisions (in `docs/decisions.md`), OQ-# open questions (in `docs/questions.md`), G-# guardrails, L-# lessons. A review file or an audit file uses its own local ids, and they never enter the design doc register.
- The tenets live in the design doc and in the agent files. The agent files quote them in full (D-122).

## Plain-English paragraph rules

- Max 25 words per sentence (STE rule 6.3).
- No code identifiers unless the reader needs them.
- Say what is missing today, what the change does, and why it is safe.

## Focused roadmaps

A focused roadmap covers one area, for example procgen or the projectile simulation. It uses the same sections 1, 2, 6, 7, 8, 9, and 10. It links to the design doc for the system map and the cost model. Its PR-# ids continue the global sequence. It never restates a decision. It cites the D-# id.
