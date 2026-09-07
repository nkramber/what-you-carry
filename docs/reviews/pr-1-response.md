# PR-1 review response

Date: 2026-09-07
Review: `docs/reviews/pr-1.md`

| Round | Reviewed head | Verdict | Answered in |
|---|---|---|---|
| 1 | `9459534` | Changes required, P1-1, P1-2, P2-1 | `abd2af7` |
| 2 | `60087b0` | Changes required, P2-2 | this round |

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

## Verification

- Roadmap header scope check over all five roadmaps: passed, zero omitted PR, zero omitted measurement, zero revised id.
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

## Open questions

No new owner question. OQ-12 remains open for PR-9.
