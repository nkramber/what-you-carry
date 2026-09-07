# PR-1 review response

Date: 2026-09-07
Review: `docs/reviews/pr-1.md`

| Round | Reviewed head | Verdict | Answered in |
|---|---|---|---|
| 1 | `9459534` | Changes required, P1-1, P1-2, P2-1 | `abd2af7` |
| 2 | `60087b0` | Changes required, P2-2 | `b1b772a` |
| 3 | `223aae8` | Ready for owner merge | superseded by later commits |
| 4 | `6e45d6f` | Changes required, P1-3 and P1-4 | `3684dae` |
| 5 | `e51e272` | Changes required, P2-3 | this round |

## Summary

The review raised three findings. Two have full merit. One has partial merit, and this response gives the evidence for the correction. The owner approved every disposition below on 2026-09-07. The decisions are D-176, D-177, and D-178.

## P1-1: prohibited attribution in the PR commit

Disposition: partial merit. Corrected under D-176.

The commit body of `9459534` named a harness and said that a co-author trailer stayed on. That second clause implied that an agent wrote the commits, so it named a source of the work. The finding is correct on that point.

The finding is too broad on the rest. T-6 prohibits text that "names an agent, harness, or model as the source of work". D-137 uses the same limit: "No work is attributed to an agent, harness, or model". A tool name that identifies a configured file is not a claim about the source of the work.

The broad reading also condemns text that the review did not raise:

- `docs/decisions.md`, D-172 and D-175, which name the schema, the extension, and the command line tool.
- `docs/questions.md`, OQ-16.
- `docs/design.md` line 18 and the F-15 row.
- The PR description, which names the same three tools.
- The path `.claude/settings.json`.

None of those files hold an exemption. A consistent broad reading removes the reason that each decision exists, so the registers stop explaining themselves.

Correction:

- D-176 states the boundary. Text that names an agent, harness, or model as the source of the work is prohibited. A tool name that identifies a configured file, a schema, or a verified version is not attribution.
- The commit body is rewritten. It states the schema failure and the fix, and it names no source of the work.
- PR-1 exit test 6 now scans every commit subject and body for a source-of-work claim, not only for a trailer and a generation line.
- The register records F-47.

Regression check: a scan of every commit on the branch, over the subject, the body, the author, and the committer, for `claude`, `codex`, `anthropic`, `openai`, `gpt`, `copilot`, `co-authored-by`, `generated with`, `harness`, `assistant`, `model`, `agent`, and `AI`. The scan returns one match, the path `.claude/settings.json`, which D-176 permits. No other match.

## P1-2: `night-gate` has no first-run result

Disposition: full merit. Corrected under D-177.

PR-11 created the scheduled night job and the gate together, so the gate had no result to read on its own run. G-19 requires the PR that creates a check to pass that check. The roadmap defined only the red-night case. The absent, stale, and cancelled cases had no contract.

Correction:

- PR-11 creates the scheduled night job only. The night job publishes a result record with the commit, the end time, and the status.
- A new entry, PR-58, creates the `night-gate` job. It opens after one scheduled night runs, so a real record exists (G-19).
- The gate passes only on a success record from the last 48 hours. An absent, stale, cancelled, or failed record fails the gate. Each failure message names the case (T-2, D-113).
- PR-11 exit test 7 asserts that the night job publishes the record. PR-58 holds seven exit tests, one per case, plus the real run.
- PR-11 now carries a check clause that names `night-gate` as absent, with PR-58 (D-148).
- The register records F-48. `docs/design.md` and `docs/roadmaps/phase-1-foundations.md` both hold the split and the sequence position.

## P2-1: superseded decisions presented as current

Disposition: full merit. The list was incomplete. Corrected under D-178.

All five cited lines were stale. A sweep of the repository found a sixth that the review did not name.

| File and line | Old text | New text |
|---|---|---|
| `docs/design.md:12` | Not verified, OQ-2 | Verified, D-173, .NET 10 LTS |
| `docs/design.md:300` | D-61, D-62, OQ-2 | D-61, D-62, D-173 |
| `docs/design.md:300` | D-172 | D-175 |
| `docs/design.md:572` | OQ-16: D-172 | OQ-16: D-175 |
| `phase-1-foundations.md:68` | T-6, D-172 | T-6, D-175, D-176 |
| `phase-1-foundations.md:384` | OQ-16: D-172 | OQ-16: D-175 |
| `phase-1-foundations.md:412` | resolved by D-172 | resolved by D-175 |

The last row is the line that the review missed.

Two lines keep the old ids on purpose. `docs/design.md` line 18 and the F-15 row are revision history, and they name both decisions.

Correction beyond the text fix: D-178 adds a reference check to the PR-2 checker. The check reads the `Effect` column of `docs/decisions.md` and fails on any file outside that register that cites a revised decision as a current answer. It skips a line that holds `revises`, `revised by`, or `supersedes`. PR-2 gains two exit tests for it. The register records F-49.

## P2-2: the Phase 1 roadmap header omits its new PR and decision scope

Disposition: full merit. Corrected under D-178, second round.

Line 3 of `docs/roadmaps/phase-1-foundations.md` named PR-1 to PR-11, M-1, and M-2. It did not name PR-58. It applied D-148 to D-152 and D-156, and it did not name the decisions of this PR. Line 9 said `Correction passes: none yet`, and the file held a correction pass.

Correction:

- Line 3 now names PR-58 and every governing decision.
- Line 9 records the 2026-09-07 correction pass, with D-176 to D-180 and PR-58.
- `docs/roadmaps/phase-5-early-access.md` gets the same treatment, because this PR added sequence step 11 to it. The review did not name that file.

The regression check that the finding specifies found three more defects of the same class. The review did not name them:

- The new phase-1 range `D-156 to D-168` swallowed D-158, which D-170 revises. The range is now `D-156, D-157, D-159 to D-168`.
- `phase-2-first-playable.md` line 3 had the same defect. Its range `D-157 to D-168` also swallowed D-158. It is now `D-157, and D-159 to D-168`.
- `phase-1-foundations.md` line 465 cited D-158 for OQ-32 with no revision marker. It now states that D-170 revises D-158.

A wider sweep for every revised decision found two more, both older than this PR:

- `docs/design.md:28` cited D-136 alone. D-150 revises it. The line now says `D-136 as revised by D-150`.
- `phase-3-full-loop.md:372` cited D-94 alone. D-152 revises it. The line now says `D-94 as revised by D-152`.

D-178 is refined in the same pass. A line passes the reference check when it holds a revision word **or** when it names the revising decision beside the revised one. Without the second clause the check would fail on F-34, F-46, and four correct `D-94, D-152` pairs, which are all self-consistent.

Regression check: compare each roadmap header with every `### PR-` and `### M-` heading in that file, and reject a header that names a revised decision. Then run the D-178 reference check over the design doc, the agent files, the roadmaps, and the skills. Both return zero findings.

## P1-3: PR-1 has no way to set the review gate mode

Disposition: full merit. Corrected under D-185.

D-181 put the mode in the repository variable `REVIEW_GATE_MODE`. A workflow cannot create a repository variable, and `checks: write` with `contents: read` does not grant it. D-181 also makes an absent value a failure. PR-1 therefore could not pass the check that it creates, against G-19. This is the same class as P1-2.

Correction: D-185 moves the mode to the tracked file `.github/review-gate-mode`. PR-1 creates the file with `advisory`, so PR-1 passes its own check with no owner action. The workflow reads the file from the base branch, never from the PR head, so a PR cannot change the mode that judges it. Phase 5 step 11 changes the file through a reviewed PR instead of an invisible settings edit. An absent, empty, or unknown value still fails and names the file (T-2).

Regression check: run the check with the file absent, empty, `advisory`, `enforced`, and an unknown value. Only the two known values pass. Open a PR that edits the mode file and confirm that the run uses the base value. PR-1 exit tests 15 and 16 cover both.

## P1-4: the required review commit invalidates its own effective head

Disposition: full merit. Corrected under D-184.

D-182 and D-183 require the reviewer to commit and push the review record together with the handoff entry. The effective head excluded only `docs/reviews/`, so that commit changed a path outside the exclusion and became the effective head. The record inside it then named an older commit, and rule 3 rejected it. Every corrective commit repeated the loop.

Reproduced on this branch. Commit `6e45d6f` holds `docs/reviews/pr-1.md` and `docs/session-handoff.md` only. Under the old rule the effective head was `eab18db`, while the record named `6e45d6f`, so the gate rejected a compliant review.

Correction: D-184 defines the metadata set as `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. The effective head is the newest commit outside that set. A commit that changes only those paths is a metadata commit. The archive is in the set because a handoff rollover writes it in the same commit (D-146).

Regression check: with the metadata set excluded, the effective head of this branch resolves to `8efb267`, the last substantive commit, and not to either review commit. PR-1 exit tests 12 and 13 cover the metadata commit and the mixed commit.

## P2-3: the F-51 risk row names the removed mode variable

Disposition: full merit. Corrected in the design register.

The defect came from the D-185 citation pass. That pass added the `D-185` id to every line that cited `D-181`, and it did not read the prose beside the id. The F-51 row gained the correct citation and kept the sentence "`REVIEW_GATE_MODE` selects the mode". A mechanical citation edit does not make the sentence true.

Correction: the row now says that the tracked file `.github/review-gate-mode` selects the mode, and that the workflow reads it from the base branch. The D-181 and D-185 citations stand.

The finding names one line. The regression check that it specifies covers more, so this response ran that sweep across every current document. It returned two further hits, and neither is a defect:

- `docs/decisions.md:204` is the D-181 row. The register records what D-181 said, and the row carries the `Revised in part by D-185 on 2026-09-07, the mode source only` marker. D-186 requires that form.
- `docs/decisions.md:208` is the D-185 row. It named the thing that it replaced, not a current source. The wording now says "Revises the mode source in D-181", because D-186 requires the Effect column to name the changed part. That change also removes the phrase from the sweep.

Regression check: search every non-exempt current document for `REVIEW_GATE_MODE` and `repository variable`. The search returns only the D-181 register row, which carries its revision marker. Reviews, handoffs, and the archive stay exempt.

Lesson recorded in the handoff: a mechanical citation pass must read the sentence that holds the citation. F-53 covers the churn that caused the pass.

## Verification

- Roadmap header scope check over all five roadmaps: passed, zero omitted PR, zero omitted measurement, zero revised id.
- Effective head under D-184, computed on this branch: `8efb267`, the last substantive commit. The old rule returned a review commit.
- D-178 reference check over the design doc, both agent files, all five roadmaps, and both skills: zero findings.
- Attribution scan over every commit on the branch: passed, as stated above.
- Stale reference sweep for `D-172`, `D-169`, `OQ-2`, `OQ-16`, and `.NET 8` outside `docs/decisions.md`, `docs/questions.md`, `docs/reviews/`, and the handoff: no current-status hit.
- `cmp -s AGENTS.md CLAUDE.md`: passed.
- `git diff --check`: passed.
- JSON parse of `.claude/settings.json`: passed.
- Build and test: not run. No solution exists. PR-1 creates it.
- STE checker: not run. PR-2 creates it. The changed documents received a script scan for semicolons, contractions, sentence length, and verb -ing forms.

## New decisions

- D-176: the attribution reading.
- D-177: the night gate bootstrap, and the PR-11 and PR-58 split.
- D-178: the reference check in PR-2, refined in round 2 with the reviser-named exemption.
- D-179: the `review-gate` check.
- D-180: the `review-gate` enforcement path.
- D-181: three conclusions and two modes, revised by D-185 for the mode source.
- D-182: commit the review record with the handoff entry.
- D-183: the reviewer pushes its own review commit.
- D-184: the metadata paths for the effective head.
- D-185: the tracked mode file, read from the base branch.
- D-186: the two revision markers, `Superseded by` and `Revised in part by`.
- D-187: the session number procedure.

## Open questions

No new owner question. OQ-12 remains open for PR-9.
