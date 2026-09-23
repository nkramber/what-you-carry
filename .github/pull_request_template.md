## Summary

<!-- What the PR changes, and why. One concern per PR (G-10). -->

## PR gate

Each line holds before the merge, by auto-merge or by the owner (`CLAUDE.md`, PR gate, D-516).

- [ ] Tests written and green (T-3).
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The three-platform build-and-test workflow is green (D-148).
- [ ] The three-platform bit-identity job is green (G-9).
- [ ] The `det-lint` job is green (G-2, G-21).
- [ ] The `ste-check` job is green (G-14). It runs the STE checker, the reference check, and the session number check (D-178, D-187).
- [ ] The automated pass of gitar approved the head, or every comment of the pass has its answer (D-250).
- [ ] The other provider reviewed it, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-101, D-184). A PR that changes no code is exempt when the owner adds the `review-override` label (D-188, D-190).
- [ ] The `review-gate` check is green. Red means no review record, or a review that does not approve this head (D-181, D-185, D-521).
- [ ] No review thread stays open, and the ruleset of `main` holds (D-522).
- [ ] The owner confirmed the merge after a summary of one paragraph (D-524).
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Every check that does not exist yet has a line above with the PR that creates it (D-148, G-19).
- [ ] `docs/session-handoff.md` is current, and its newest entry names the branch of this PR (D-376).
- [ ] The `doc-gate` job is green (D-375, D-376). No part of this PR waits for a later PR.
- [ ] No attribution anywhere (T-6). No commit subject or body names an agent, harness, or model as the source of the work (D-176).

## Documents

One line for each category, in this order (D-375, D-376). Start the line with `Changed:`, `Reviewed; no change needed:`, or `Not applicable:`. Then give a reason of five words or more that names the part of the document and the cause. The `doc-gate` job reads this section.

- `docs/design.md`:
- `docs/decisions.md`:
- `docs/questions.md`:
- `docs/roadmaps/`:
- `docs/runbooks/`:
- `docs/session-handoff.md`:
- `CLAUDE.md` and `AGENTS.md`:
- `.claude/skills/`:
