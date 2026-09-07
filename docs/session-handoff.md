# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 13: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the D-184 and D-185 corrections. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Re-reviewed the diff through effective head `e51e272`.
- Confirmed P1-3 is fixed: the tracked `.github/review-gate-mode` file supplies `advisory`, and the workflow reads it from the base branch (D-185).
- Confirmed P1-4 is fixed: the review, handoff, and archive paths are metadata, so the required review commit does not change the effective head (D-184).
- Added P2-3: the F-51 row in `docs/design.md` still names the removed `REVIEW_GATE_MODE` variable as the mode source.

### State of the build

- No code, solution, or CI workflow exists. PR-1 creates them.
- The effective-head check resolves to `e51e272` after excluding the D-184 metadata set.
- `AGENTS.md` and `CLAUDE.md` remain byte-identical.
- The review verdict is Changes required.

### In flight

PR #1 needs the F-51 mode-source text corrected in `docs/design.md`.

### Traps and gotchas

- The current mode source is `.github/review-gate-mode`, not `REVIEW_GATE_MODE` (D-185).
- The review record must name the effective head, not the metadata commit (D-184).
- The review and handoff commits must be pushed to the PR branch (D-182, D-183).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct the F-51 row, then request another repeat review of PR #1.

## Session 12: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the P1-3 and P1-4 findings, and add the push-back rule to the review skill. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The review at head `6e45d6f` raised P1-3 and P1-4. Both have full merit. Recorded D-184 and D-185 and wrote the dispositions in `docs/reviews/pr-1-response.md`.
- P1-4: D-182 requires the reviewer to commit the review record with the handoff entry, and the effective head excluded only `docs/reviews/`. The review commit therefore became the effective head and rejected the record it published. Reproduced it. The old rule returned `eab18db` while the record named `6e45d6f`.
- D-184 defines the metadata set: `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. With that set excluded the effective head resolves to `8efb267`, the last substantive commit. The archive is in the set because a handoff rollover writes it in the same commit.
- P1-3: a workflow cannot create a repository variable, so PR-1 could not pass the check it creates, against G-19. D-185 moves the mode to the tracked file `.github/review-gate-mode`. PR-1 creates it with `advisory`. The workflow reads the file from the base branch, so a PR cannot change the mode that judges it.
- The owner asked that a request to address review findings load the `pr-review` skill, and that the skill state that a finding is a claim, not a fact. Added an "Address review findings" section with a seven-step procedure and a push-back table. Widened the skill description so the request triggers it.
- D-185 revises D-181, which made fifteen citations stale. The D-178 check found each one. They now name D-185.
- The owner then asked how to stop the two recurring problems. Recorded D-186 and D-187, and F-52 and F-53.
- D-186 splits the revision marker. `Superseded by D-N` replaces the whole answer, and every citation must name D-N. `Revised in part by D-N` changes one named part, and the decision stays citable. The Effect column must name the part that changed and the parts that stand.
- Reclassified the seven revisions. D-94, D-158, D-169, and D-172 are superseded. D-136, D-179, and D-181 are revised in part. The register already used `Superseded by` for D-10 and D-95, so the verb is not new.
- The D-178 check now keys on `Superseded by` only. Partial revisions carried 33 of the 48 citations, and they produced all three rounds of churn.
- D-187 fixes the session number. Fetch the remote and read the handoff again immediately before the handoff commit, then take the highest number and add one. Every session pushes (D-183), so a push serializes the writers. The PR-2 checker fails on a duplicate number.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds the review commits and this session's commit.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review. P1-3 and P1-4 are corrected in documents only, because no workflow exists yet.

### Traps and gotchas

- The effective head excludes three paths now, not one (D-184). A review commit that touches only those paths is metadata.
- The mode file is read from the base branch, never the PR head (D-185). A PR that edits the mode file does not change its own mode.
- A finding is a claim, not a fact. Assess it, and refute it with evidence when it does not hold.
- A partial revision still marks the whole decision as revised, so every citation must name the reviser. D-181 needed fifteen edits.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff since `6e45d6f` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 11: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the neutral review-gate change.

### What this session did, and why

- Rechecked PR #1 at effective head `6e45d6f`, including the D-181, D-182, and D-183 changes.
- Confirmed the prior four findings remain fixed.
- Added P1-3 to `docs/reviews/pr-1.md`: PR-1 does not define how `REVIEW_GATE_MODE=advisory` is created.
- Added P1-4: the required review commit changes the handoff path and invalidates its own effective head.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- The review verdict is Changes required.

### In flight

PR #1 needs an owner setup step or bootstrap mechanism for the required review-gate repository variable, and a metadata-path correction for the effective-head rule.

### Traps and gotchas

- D-181 forbids a default for an absent or unknown `REVIEW_GATE_MODE`.
- The PR-1 roadmap says it sets the variable, but the scope contains no repository-state action.
- D-182 and D-183 require the review record and handoff in one commit, but D-179 excludes only `docs/reviews/`.
- The effective head at review start is `6e45d6f`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Define the advisory-mode variable setup, correct the metadata-path rule, revise PR #1, and request another repeat review.

## Session 10: 2026-09-07, Claude Code

Author: Claude Code
Session: the neutral grey state for `review-gate`. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The owner asked where the red check appears, then asked for a neutral grey state instead of a permanent red one. `review-gate` failed whenever no review record existed, so a PR stayed red for most of its life and a red rollup masked a real build failure.
- Verified first that GitHub counts a neutral or skipped conclusion as a success for a required status check. Source: docs.github.com, "About status checks", verified 2026-09-07. A plain grey-when-absent gate would stop blocking a merge at launch. Recorded F-51.
- D-181 gives the check three conclusions and two modes. In `advisory` mode a missing review file is neutral. In `enforced` mode it is a failure. The repository variable `REVIEW_GATE_MODE` selects the mode. An absent or unknown value fails the job and names the variable (T-2).
- A workflow job cannot set a neutral conclusion by its exit code. The job publishes a check run through the Checks API, so the workflow needs `checks: write`.
- D-181 revises D-179. D-180 is not revised, because D-181 only adds the mode step to its launch procedure.
- Marking D-179 as revised made nine citations stale. The D-178 check found each one. They now name D-181.
- Updated the PR-1 scope and exit tests to sixteen, both agent files, the `pr-review` skill with a color table, Phase 5 step 11, and the design register.
- Recorded D-182 after the handoff and review record of sessions 8 and 9 reached this session uncommitted. A later commit absorbed them, and this session then reported the wrong verdict. The `pr-review` skill now requires a commit of the review record with its handoff entry, and the agent files carry the same rule.
- D-182 narrows the scope limit in the skill, which listed a commit as an unauthorized action. A code fix, a merge, and an external message stay unauthorized.
- D-183 lets the reviewer push its own review commit to the PR branch. A push is the only way `review-gate` reads the record, because the gate reads the PR head. The reviewer never pushes to `main`.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds fifteen commits, all on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

**PR #1 is not ready to merge.** Session 9 approved head `223aae8`. This session pushed `4f7796e` after that approval, and it changes eight files outside `docs/reviews/`, including both agent files, the decision register, and the review skill. The approval does not cover the effective head. Rule 3 of D-179 applies.

### Traps and gotchas

- The verdict in `docs/reviews/pr-1.md` reads `Ready for owner merge`, and it applies to head `223aae8` only. Read the head field, not the verdict alone.
- Grey is correct only in advisory mode. Never use a neutral conclusion for an enforced gate (F-51).
- The `review-gate` job stays green itself. The check run it publishes carries the color, so the Checks list holds two rows.
- Never write a decision range that spans a revised id. D-179 is revised, so a header says `D-176 to D-178, D-180, and D-181`.
- Read the review file before a commit that sweeps it in. This session committed an approval it had not read, and then reported the wrong verdict.
- One session is one handoff entry (D-146). Add a new entry. Do not append to an older one after another provider writes above it.
- A review record that is not committed is invisible to `review-gate`, because the gate reads the PR head (D-182).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff from `223aae8` to `4f7796e` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 9: 2026-09-07, Codex

Author: Codex
Session: final repeat review of PR #1 on `docs/roadmaps`.

### What this session did, and why

- Rechecked PR #1 at effective head `223aae8`.
- Confirmed the P2-2 fix and reviewed the effective-head command clarification.
- Updated `docs/reviews/pr-1.md` with a Ready for owner merge verdict.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- All four prior findings are fixed. No new finding remains.

### In flight

PR #1 is ready for owner merge. Build and CI checks remain deferred because this PR defines the solution and workflows that PR-1 creates.

### Traps and gotchas

- The reviewed effective head is `223aae8`.
- The review file is machine-read. Keep its head field and verdict exact.
- The owner must still register the runner and move the checkout to the SSD before PR-1.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner can merge PR #1. Then complete the SSD and runner actions before starting PR-1.

## Session 8: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 on `docs/roadmaps`.

### What this session did, and why

- Rechecked PR #1 at effective head `60087b0` against the prior review and the author response.
- Confirmed fixes for P1-1, P1-2, and P2-1.
- Added P2-2 to `docs/reviews/pr-1.md` because the Phase 1 header omits PR-58, D-177, and D-178.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff check passes. The agent files remain identical. The settings file parses as JSON.
- The review verdict remains Changes required.

### In flight

PR #1 needs a small roadmap header correction. The review record now names head `60087b0`.

### Traps and gotchas

- The effective head is the newest commit outside `docs/reviews/`.
- The old findings stay in the review record with fixed dispositions.
- The Phase 1 sequence includes PR-58, but the roadmap header still states PR-1 to PR-11 only.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct the Phase 1 roadmap header and request a final repeat review against the new effective head.

## Session 7: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the PR #1 review. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Read `docs/reviews/pr-1.md` and checked each of the three findings against the branch, the registers, and the PR.
- P1-2 and P2-1 have full merit. P1-1 has partial merit. The owner approved every disposition. Recorded D-176 to D-178 and F-47 to F-49. Wrote `docs/reviews/pr-1-response.md`.
- P1-1: T-6 and D-137 prohibit text that names an agent, harness, or model as the source of the work. A tool name that identifies a configured file is not attribution, so the broad reading in the finding would also condemn D-172, D-175, OQ-16, F-15, the PR description, and the settings file path. D-176 states the boundary. The body of the head commit is rewritten, because one clause implied that an agent wrote the commits. PR-1 exit test 6 now scans every subject and body, not only trailers.
- P1-2: PR-11 created the night job and the `night-gate` job together, so the gate had no result to read on its first run, against G-19. D-177 splits them. PR-11 publishes a result record. The new PR-58 adds the gate after one night runs. An absent, stale, cancelled, or failed record fails the gate.
- P2-1: fixed all six stale references. The review named five. A sweep found a sixth at `phase-1-foundations.md:412`. D-178 adds a reference check to the PR-2 checker, so the next revision cannot leak.
- The owner asked for a GitHub merge criterion that blocks a merge without the review files. GitHub returns 403 for branch protection and for rulesets on a private free repository, verified this session. No hard block is possible today.
- Recorded D-179 and D-180 and F-50. PR-1 gains a `review-gate` job. It reads `docs/reviews/pr-<number>.md`, requires the verdict `Ready for owner merge`, and requires the recorded head to be the effective head. The job is advisory until launch. Phase 5 step 11 makes it a required check after the repository becomes public.
- The owner asked for the head to match the PR head. The `pr-review` skill says the opposite: do not require the review file to hold its own hash. The effective head reconciles both. The effective head is the newest commit outside `docs/reviews/`.
- The owner chose not to require the response file. The gate reads the review file only.
- Codex re-reviewed at head `60087b0`. P1-1, P1-2, and P2-1 are marked fixed. One new finding, P2-2, is open: the Phase 1 roadmap header omits PR-58 and the new decisions, and it still says `Correction passes: none yet`.
- P2-2 has full merit. Fixed line 3 and line 9 of the Phase 1 roadmap. Gave `phase-5-early-access.md` the same treatment, because this PR added its sequence step 11. The review did not name that file.
- Ran the regression check that P2-2 specifies. It found three more defects of the same class. The new phase-1 range `D-156 to D-168` swallowed the revised D-158, `phase-2-first-playable.md` had the same defect in `D-157 to D-168`, and phase-1 line 465 cited D-158 with no revision marker. All three are fixed.
- A wider sweep found two older ones: `docs/design.md:28` cited D-136 alone, and `phase-3-full-loop.md:372` cited D-94 alone. Both now name the revising decision.
- Refined D-178. A line passes the reference check when it holds a revision word or when it names the revising decision. Without that clause the check fails on F-34, F-46, and four correct `D-94, D-152` pairs.
- Rewrote `.claude/skills/pr-review/SKILL.md` for the format. It now holds a review file skeleton, the three machine-read fields, a finding format with stable `P<severity>-<n>` ids, the attribution boundary of D-176, the gate rules, a ten-step repeat review procedure, and the response file contract.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds thirteen commits. The head commit of session 5 was amended, and the branch was force pushed.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review by Codex against the new head.

### Traps and gotchas

- The head commit was amended. The review file `docs/reviews/pr-1.md` names head `9459534`, which no longer exists.
- D-176 fixes the attribution reading. Do not strip a tool name that identifies a configured file, a schema, or a version. Strip a claim about the source of the work.
- The owner squash-merges (D-126). GitHub fills the squash body with every commit message. Check that body before the merge.
- PR-58 is new. Phase 1 now ends with PR-11, one scheduled night, PR-58, then the measurements and Gate 1.
- Ids never change. PR-58 sits after PR-11 in the sequence, not after PR-57.
- The PR-2 reference check skips a line that holds `revises`, `revised by`, or `supersedes`. The two F-15 history lines were reworded to hold that word.
- Branch protection and rulesets both return 403 on this repository. Do not plan a hard merge block before launch (D-180).
- `review-gate` reads three exact things: the file name, the `- Head: ` line, and the verdict name. A reworded verdict fails the job.
- The effective head ignores a commit that changes only `docs/reviews/`. A review file commit does not invalidate its own approval.
- `docs/reviews/pr-1.md` now records head `60087b0` and holds four findings. P2-2 is the open one, and this session fixed it.
- A decision range in a header can swallow a revised decision. Write `D-156, D-157, D-159 to D-168`, not `D-156 to D-168`, when D-158 is revised.
- The roadmap header is a scope summary. Update line 3 and line 9 whenever a PR entry, a measurement, or a governing decision changes.
- Never write a decision range that spans a revised id. D-179 is revised, so a header says `D-176 to D-178, D-180, and D-181`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #1 a third time against the new head, per `.claude/skills/pr-review/SKILL.md`. P2-2 is the only finding to confirm. Sessions 8 and 9 did that work.

## Session 6: 2026-09-07, Codex

Author: Codex
Session: review PR #1 on `docs/roadmaps`.

### What this session did, and why

- Read the handoff, agent rules, review skill, STE skill, design, decisions, questions, existing reviews, and focused roadmaps.
- Verified PR #1 at base `1c16c45` and head `9459534`.
- Wrote `docs/reviews/pr-1.md` with three findings and a Changes required verdict.

### State of the build

- No solution or implementation exists on the reviewed head.
- The diff passes `git diff --check`.
- The agent files remain byte-identical. The settings file parses as JSON.

### In flight

PR #1 needs a revision. The review identifies prohibited attribution in a commit body, an undefined first-run path for `night-gate`, and stale references to D-172 and the unresolved OQ-2 state.

### Traps and gotchas

- T-6 applies to commit bodies as well as commit subjects.
- D-175 supersedes D-172. D-173 supersedes D-169.
- PR-11 creates the night result and the gate. The empty-result case needs an explicit contract.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Revise PR #1, then run a repeat review against the new head. Recheck the commit history, the first night-gate run, and every current-status reference to the revised decisions.

## Session 5: 2026-09-07, Claude Code

Author: Claude Code
Session: fix the attribution setting, no new PR. Branch `docs/roadmaps`, pushed, part of PR #1.

### What this session did, and why

- Found why the Claude Code startup dialog reported that `.claude/settings.json` failed to parse. The file is valid JSON, but `attribution.commit` and `attribution.pr` were booleans. The schema requires strings. When one value fails validation, the harness ignores the whole file, so the co-author trailer stayed on, against T-6.
- Set both fields to the empty string, which hides the attribution (D-175). Verified against the settings reference, the schema in the VS Code extension 2.1.263, and the validator in the CLI 2.1.261.
- Marked D-172 as revised. Updated OQ-16 and F-15. Pushed one commit to `docs/roadmaps`, so PR #1 carries the fix.

### State of the build

- `main` has one commit, `1c16c45`, on the remote. The branch `docs/roadmaps` holds the six commits of session 4 and one commit of this session, all on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.

### In flight

PR #1 from `docs/roadmaps` to `main` is open and waits for the other provider's review (T-4). The attribution fix is part of it.

### Traps and gotchas

- `attribution.commit` and `attribution.pr` are strings. A boolean makes the harness ignore the whole settings file, and the startup dialog calls it a parse failure.
- A settings file that fails validation loses every setting in it, not only the bad field. Run `/doctor` to see what the harness dropped.
- The traps in the session 4 entry still apply.

### Open questions that block progress

No new question. The session 4 entry lists the open ones.

### Next concrete action

Unchanged from session 4. A Codex session reviews PR #1 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-1.md`. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

## Session 4: 2026-09-07, Claude Code

Author: Claude Code
Session: initial commit, the five focused roadmaps, and PR #1. Branch `docs/roadmaps`, pushed.

### What this session did, and why

- Reset HEAD to `main`, added `.gitignore` and `.gitattributes`, and pushed every file to `origin/main` as commit `1c16c45`, message "initial commit" (D-156). The owner asked for a direct push to `main`.
- Created the branch `docs/roadmaps` from `main` for the focused roadmaps (D-156).
- Wrote `docs/roadmaps/phase-1-foundations.md`. Each PR entry has scope, out of scope, exit tests, review focus, a check clause, a gate, and a plain-English paragraph. It adds M-1 and M-2 procedures and a Phase 1 sequence.
- Found two gaps of the audit R-2 class and handled them under D-149. PR-10's gate named a weapon roster that does not exist until Phase 3 (F-38). A test-only definitions file fixes it. The macOS CI leg needs a self-hosted runner that nobody had listed (F-39, OQ-31).
- Added G-21 to the design guardrails: no `System.Random` or wall-clock reads in Core.
- Filed OQ-31 to OQ-42 in `docs/questions.md`: the owner actions and the technical choices that Phase 1 PRs need before they start, each with a recommendation.
- Linked the roadmap from `docs/design.md` section 7.
- Asked the owner OQ-31 to OQ-42 in three batches and recorded D-157 to D-168. The Phase 1 roadmap now cites those decisions instead of the questions.
- Wrote the four later roadmaps in the same format: `phase-2-first-playable.md`, `phase-3-full-loop.md`, `phase-4-content-complete.md`, and `phase-5-early-access.md`. Each has per-PR scope, exit tests, review focus, a check clause, a gate, and a sequence with the owner questions placed before the PR that needs them.
- Filed OQ-43 to OQ-71 for those phases, each with a recommendation. Added F-40 to F-45 to the design register for gaps the roadmaps exposed: wall fade in the mesher, no Deck unit named, no rarity tiers named, no Tier 3 model or budget, no source for the Deck checklist, and no cloud save file set after D-152.
- Pushed every commit on `docs/roadmaps` to the remote after the owner pushed the branch.
- Closed OQ-2 (D-169, .NET 8 LTS, revised the same day to .NET 10 LTS as D-173), OQ-16 (D-172, `.claude/settings.json` with attribution off), and the runner timing (D-171, tomorrow on the SSD). Wrote `docs/runbooks/macos-runner.md`.
- Found that branch protection needs GitHub Pro or a public repository. The owner deferred it until launch (D-170, F-46).
- Renamed the GitHub repository to `nkramber/what-you-carry` and moved the checkout to `/Users/nate/Repos/what-you-carry` (D-174). The runbook cites the new name.

### State of the build

- `main` has one commit, `1c16c45`, on the remote. The branch `docs/roadmaps` holds six commits of this session and is on the remote.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The STE checker does not exist until PR-2. This session scanned the changed documents by script.

### In flight

All five roadmaps are on the branch. PR #1 from `docs/roadmaps` to `main` is open and waits for the other provider's review (T-4).

### Traps and gotchas

- The roadmap never restates a decision. It cites D-# ids. Read the `Effect` column before you cite an early decision.
- PR-1 cannot merge until the runner exists (D-157, D-171) and the SSD holds the checkout (D-145). OQ-2 and OQ-16 are closed.
- Nothing on GitHub stops a push to `main` (D-170). The rule in the agent files is the only guard. Never push to `main`.
- The .NET pin is .NET 10 LTS (D-173). D-169 stays in the register as revised.
- The checkout path changed on 2026-09-07 (D-174). Open `/Users/nate/Repos/what-you-carry` in the editor. The session memory for the old path was copied to the new path.
- Each Phase 1 PR has owner questions listed before it in the roadmap sequence. Ask them before the PR starts, not inside it (D-124).
- `Sqrt` in DetMath wraps the IEEE square root. The lint tool must allow `MathF` inside `DetMath.cs` only.
- The night-gate job in PR-11 reads the latest scheduled run. A red night blocks the next merge by design (D-115).

### Open questions that block progress

`docs/questions.md` holds OQ-1 to OQ-71. One stays open for Phase 1: OQ-12 blocks PR-9. One owner action precedes PR-1: the runner registration on 2026-09-08 per the runbook (D-171). The SSD arrives the same day (D-145). Each later roadmap lists its own open questions in its section 6, with the owner's answer placed in the sequence before the PR that needs it.

### Next concrete action

A Codex session reviews PR #1 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-1.md`. The author of this branch is Claude Code, so Claude Code cannot review it. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md` with the volume name. After the owner merges PR #1, PR-1 starts from `phase-1-foundations.md`.

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
