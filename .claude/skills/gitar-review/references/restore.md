# Restore the automated pass

The owner suspended the automated pass of gitar on 2026-09-22, because the gitar subscription expired (D-471). The gitar app stays installed. This file holds the original text of each rule that the suspension changed, and the steps to restore the pass.

Load this file when the owner restores the pass.

## Procedure

1. Confirm that the gitar subscription is active again, and that gitar reviews a new PR.
2. Add a decision that restores the pass and supersedes D-471. Do not delete the D-471 row.
3. In the D-250, D-303, and D-374 rows, add the note "Restored by D-N" after the D-471 note.
4. Run `grep -rn 'D-471' --include='*.md' .` to find each place that the suspension changed.
5. Replace each suspended text with its original text from the next section.
6. Remove the section "The suspension" from the `gitar-review` skill.
7. Remove the first paragraph of "Wait for gitar" in `docs/runbooks/session-context.md`.
8. Delete this file, and remove its line from the `gitar-review` skill.
9. Run the full test suite, because it runs the STE checker and the byte ceilings.

Do not revert the suspension PR with git. A revert deletes the D-471 row, and the decision register never deletes a row.

## The original text

The blocks below hold the text before D-471, word for word.

The section of `CLAUDE.md` and `AGENTS.md`, which the two files hold the same:

````markdown
## Automated review pass

An automated reviewer, gitar, comments on every PR after a push (D-250). The author answers every comment before the hand-over to the other provider, or before the override request on a documentation PR.

- Load the `gitar-review` skill after each push. It holds the author procedure, the proof that a review is current, and the commands (D-374).
- When the pass ends, tell the owner that the PR is ready for the other provider, or for the override.
- The reviewing provider reads the PR comments into its review and never addresses gitar (`pr-review`).
- A reply names no provider, harness, or model as the source of work (T-6).
````

The PR gate line of `CLAUDE.md` and `AGENTS.md`:

````markdown
- [ ] The automated pass of gitar approved the head, or every comment of the pass has its answer (D-250).
````

The section of `.claude/skills/review-response/SKILL.md`:

````markdown
## The automated pass

An automated reviewer, gitar, reviews every PR after a push (D-250). The author gets a current review of the head and answers every finding before the hand-over to the other provider. On a documentation PR, the author does this before the override request. This pass comes before the cross-provider review and never replaces it (T-4).

Load `.claude/skills/gitar-review/SKILL.md` after each push, and follow its procedure (D-374). That skill holds the steps, the proof that a review is current, the traps, and the commands. This section gives only the rules of this repo, and each rule wins over that skill:

- The author alone answers gitar. The reviewing provider never replies to gitar (`pr-review`, `references/repeat-review.md`).
- A reply names no provider, harness, or model as the source of the work (T-6, D-176).
- When the pass ends, tell the owner that the PR is ready for the other provider, or for the override. It is not ready to merge yet.
- Record the pass in the handoff entry. Give the count of findings, the count with merit, and the commit that answered each one.
- Record each `Gitar review` comment in the handoff entry too (D-303).
````

The last sentence of section 3.14 of `docs/design.md`:

````markdown
An automated reviewer, gitar, comments on every PR after a push, and the author answers every comment before that review (D-250).
````
