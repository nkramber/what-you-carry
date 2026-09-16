# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 182: 2026-09-16, Claude Code

Author: Claude Code
Session: cut the agent context cost of each session, with the owner instructions D-377 to D-382. Branch `feat/context-budget`.

### What this session did, and why

- A token audit of this repository read 10 recent sessions of each harness. Most input came from the model calls after a status poll (17.2% Claude Code, 22.8% Codex) and after handoff and archive work (17.3%, 27.5%). Register text and one large review skill added more. The owner told the session to apply every P0 and P1 fix of the audit.
- D-377: the first action prints the newest handoff entry alone, and the read order adds the newest entry of the branch. The end of a session takes the number from the top heading after a fetch. D-187 gains a revision note for the second full read.
- D-378: `AGENTS.md` holds one lookup command for all D-# and OQ-# ids of a task, with a line that finds each revision (D-186). A check on five superseded decisions printed the superseding id of each.
- D-379: the `handoff-rotate` command moves each entry after the tenth to the archive top with its text intact. Nine tests cover it, and a seed loop of 300 seeds proves the text and the order. A mutation that dropped one moved entry failed three tests.
- D-380: the `one-pr-one-session` skill waits on checks with one `gh pr checks --watch --fail-fast` command. `gitar-review` does not change, because other repositories use the same file.
- D-381: `pr-review` keeps the reviewer procedure, and the new `review-response` skill holds the author procedure. D-374 gains a revision note.
- D-382: `ContextBudgetTests` caps the agent files at 15000 bytes, each skill at 12000 (`pr-review` 31000), the handoff at 60000, and its newest entry at 7000.
- The rule paragraph of this file moved from between Session 181 and Session 180 to the file header, so it no longer moves with the entries.
- `CLAUDE.md` holds the new rules in 14586 bytes, 7 fewer than before. The gitar bullets that copied the `gitar-review` skill are gone (D-374), and some build prose is shorter with no rule lost.

### State of the build

- `main` is at `58e4fc8`, the base of this branch. Pending owner merge.
- Remote head: `origin/feat/context-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` without the Smoke category: 1143 tests, 0 failures. `ste-check`: 0 findings in 19 files. `det-lint`: 0 findings. No Core, Game, content, or asset path changed, so the simulation version, `bit-identity`, and `asset-qa` stand.
- Start paths, bytes before and after: an implementation author 74615 to 35920, an author who answers findings 109690 to 44408, a reviewer 109690 to 66363.

### In flight

This PR changes `WhatYouCarry.Tools` and `WhatYouCarry.Tests`, so it needs the Codex review per `pr-review` at the effective head (T-4). The `review-override` label does not apply. The owner then merges.

### Traps and gotchas

- The start rules changed in this PR. A reviewer of this branch reads the new `AGENTS.md` of the branch, and `main` keeps the old text until the merge.
- `ContextBudgetTests` reads the newest handoff entry. An entry over 7000 bytes fails `dotnet test`, so keep each entry short.
- Run `handoff-rotate` after the new entry, not before. It exits 1 and changes nothing on a duplicate number, a wrong order, or an archive top that is not older.
- G-17 asks for a measurement after the change. The first three substantial sessions on `main` with these rules count the calls after a status poll, the handoff bytes read at the start, and the calls after archive work. Each writes the counts in its own handoff entry.
- The next ids are D-383, OQ-177, F-102, PR-70, and Session 183.

### Open questions that block progress

None blocks this PR. The PR-66 session asks the owner for the shape of a tier first (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill at the effective head and writes the review record on this branch. The owner then merges. A fresh session then opens PR-69.

## Session 181: 2026-09-16, Codex

Author: Codex
Session: re-review PR #77 at effective head `b91bd24` and record the corrected verdict. Branch `feat/one-pr-one-session`.

### What this session did, and why

- Recomputed the effective head. `b91bd24` changes the Gitar review skill, so it moves the head beyond the duplicate-line correction `f0befdd` (D-184).
- Confirmed Claude Code authored the PR from Session 178. Codex passes the provider gate (T-4, D-101).
- Verified P2-1 remains fixed. `DuplicateMatrixLineFails` passes, the live description passes `doc-gate`, and the same description with the conflicting line fails.
- Verified the current Gitar dashboard follows the request and reply for `b91bd24`. It approves and has no open finding.
- Updated `docs/reviews/pr-77.md`. It retains P2-1 and the earlier verdict, and records the new verdict for `b91bd24`.
- Session 171 moved to the archive, because this entry made eleven entries.

### State of the build

- `main` and the merge base are `9b27afc`. The effective PR head is `b91bd24`.
- Remote head: `origin/feat/one-pr-one-session` at the commit that holds this entry and the review record, checked with `gh pr view` before the session ended.
- `DocGateTests`, `ReviewGateRulesTests`, and `ReviewGateGitTests`: 46 passed. The live `doc-gate` description passed, and the duplicate-line variant failed with one problem.
- Local `dotnet test WhatYouCarry.slnx --no-build` produced no completion after four minutes and was cancelled. The CI platform tests on `b91bd24` passed.
- Before the review update, CI on `b91bd24` passed asset QA, bots, compare, det-lint, doc-gate, platform tests, night gate, and STE check. `evaluate` and `review-gate` failed while the review record held the earlier verdict.
- The current Gitar dashboard approves and reports the earlier finding closed with no new finding.

### In flight

PR #77 is ready for owner merge. The owner merges it.

### Traps and gotchas

- The effective head is `b91bd24`, because the Gitar skill changed after `f0befdd` (D-184).
- Two platform jobs from an older run remained pending, while later matching platform results passed.
- The next ids are D-377, OQ-177, F-102, PR-70, and Session 182.

### Open questions that block progress

None blocks PR #77. The PR-66 session asks the owner for the shape of a tier first (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3.

### Next concrete action

The owner merges PR #77. A fresh session then works on PR-66 and asks the owner for the tier shape first (D-350).

## Session 180: 2026-09-16, Claude Code

Author: Claude Code
Session: answer review finding P2-1 of PR #77, in the author session of the PR. Branch `feat/one-pr-one-session`.

### What this session did, and why

- Session 179 reviewed PR #77 at `887f94f` and gave `Changes required` for P2-1: the documents matrix accepts duplicate category lines. The owner asked for the answer in the author session, and D-375 permits that work after the hand-over.
- The trigger reproduced. A second `docs/design.md` line that contradicts the first passed `doc-gate` with 0 problems.
- `DocGateRules.CheckMatrix` now reports more than one line for a category. `DuplicateMatrixLineFails` fails on the old code and passes with the correction.
- D-376, the enforcement table, and the skill now say exactly one line for each category.
- The review also asked to reject an unknown label if the section is an exact list. `docs/reviews/pr-77-response.md` refutes that part: the categories are a floor, and a label with a typo already fails as a missing category.
- A `Gitar review` comment at 20:20:17 UTC ran a review of `f0befdd`. Gitar replied "On it" at 20:20:39, and it replaced the dashboard comment at 20:21:01 with an approval and no open finding. That review was current, and it kept the old summary word for word.
- The `gitar-review` skill read the unchanged summary as a stale review. A second request at 20:21:24 got the reply "You've sent several Gitar comments in a short window", and a wait that watched the dashboard alone ran ten minutes with no result.
- The owner asked for a correction of the skill. The summary is no longer a condition of a current review. After a request, the author reads the Gitar reply first, a refusal waits ten minutes, and each check reads the newest dashboard id. Command B prints the reply.
- Session 170 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `9b27afc`. The correction commit, which holds this entry and the response file, is the new effective head. Pending the repeat review and the owner merge.
- Remote head: `origin/feat/one-pr-one-session` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `DocGateTests`, `ReviewGateRulesTests`, and `ReviewGateGitTests`: 46 passed. `ste-check`: 0 findings in 18 files. `doc-gate` passes on the current PR description and fails on the description with the duplicate line.

### In flight

PR #77: the gitar pass on the head after the skill correction, then the repeat Codex review of P2-1 per the `pr-review` skill. The owner then merges.

### Traps and gotchas

- The correction commit holds code, so it moves the effective head past `887f94f` (D-184). The review of `887f94f` no longer covers the head.
- The `gitar-review` skill is the same file in each repo that uses gitar. Put the same correction in the copy of each other repo.
- A duplicate line stops the other checks of that category, because the gate cannot know which line holds. Each other category keeps its checks.
- The next ids are D-377, OQ-177, F-102, PR-70, and Session 181.

### Open questions that block progress

None blocks PR #77. The PR-66 session asks the owner for the shape of a tier first (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3.

### Next concrete action

A Codex session runs the repeat review of P2-1 per the `pr-review` skill at the correction head and sets the verdict. The owner then merges PR #77.

## Session 179: 2026-09-16, Codex

Author: Codex
Session: review PR #77 at effective head `887f94f` and record the duplicate matrix finding. Branch `feat/one-pr-one-session`.

### What this session did, and why

- Reviewed PR #77, which adds the one-PR session rule and the `doc-gate` command and workflow (D-375, D-376).
- Confirmed the provider gate. Session 178 identifies Claude Code as the author, so Codex is eligible under T-4 and D-101.
- Read the full diff, tests, workflow, design, decisions, questions, description, and current Gitar dashboard.
- Found P2-1: the matrix checker accepts duplicate category lines. A direct fixture with conflicting `docs/design.md` dispositions passed with 0 problems.
- Wrote `docs/reviews/pr-77.md` with the verdict `Changes required` for `887f94f`.

### State of the build

- `main` and the merge base are `9b27afc`. The effective head is `887f94f`. Commit `6800376` changes only `docs/session-handoff.md` after that head (D-184).
- The focused `DocGateTests` suite passed 14 tests. The duplicate-line probe passed the invalid fixture, which confirms P2-1.
- Before publication, CI on PR tip `6800376` passed the three-platform tests, bit identity, compare, bots, asset QA, `det-lint`, `doc-gate`, night gate, and STE check. `evaluate` failed and `review-gate` skipped before this review record.
- After metadata commit `faab1be`, the refreshed checks were pending. The branch head matched `origin/feat/one-pr-one-session` after fetch and `gh pr view`.
- Local fetch first hit a sandbox permission error for `.git/FETCH_HEAD`. A permitted fetch succeeded and confirmed `origin/main` and the PR head.

### In flight

PR #77 needs a correction for P2-1 and a repeat review. The owner then decides when to merge.

### Traps and gotchas

- `DocGateRules.CheckMatrix` calls `List.Find`, so duplicate matching lines after the first do not get checked.
- The metadata tip `6800376` does not replace effective head `887f94f` under D-184.
- `evaluate` and `review-gate` must refresh after the review record reaches the PR.
- The next ids are D-377, OQ-177, F-102, PR-70, and Session 180.

### Open questions that block progress

None blocks PR #77. The PR-66 session asks the owner for the shape of a tier first (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3.

### Next concrete action

The author corrects P2-1, runs the duplicate-line regression test, and requests a repeat review of PR #77.

## Session 178: 2026-09-16, Claude Code

Author: Claude Code
Session: bind each session to one PR, and make each PR carry all of its documents, with the owner instructions D-375 and D-376. Branch `feat/one-pr-one-session`.

### What this session did, and why

- The owner asked for a strict rule: one PR in one clean session, and no PR that only records an earlier PR. The goal is a lower token cost, because 24 of the 76 PRs recorded the merge of the PR before them.
- D-375 binds a session to one repository, one branch, one PR, and one role. It supersedes D-297 and revises in part D-121, the count of PRs only. Before the merge, the documents say `Done in PR #N` and "pending owner merge". Git holds the merge commit and the merge time.
- D-376 adds the `doc-gate` job and the command of the same name in `WhatYouCarry.Tools`. The job fails when the PR does not change the handoff, when the newest entry names another branch, or when the documents matrix is incomplete. It also fails when a matrix line disagrees with the diff, on a phrase that puts documents off to later work, or on a merge record title or branch.
- The skill `.claude/skills/one-pr-one-session/SKILL.md` holds the start gate, the documents matrix, the status marks, and the completion gate. It is 5620 bytes. `AGENTS.md` and `CLAUDE.md` name its path in one line, and they stay identical (D-122).
- The PR template gains the eight matrix lines and two gate lines. `docs/design.md` section 3.14 gains the enforcement table, and G-22 states the rule. The `design-doc-style` and `pr-review` skills gain one line each.
- Five fresh evaluators ran the skill in a dry run, with no hint of the expected result. A merge record request and a second PR request after a compaction both stopped with the blocked line. A clean start and a reviewer session bound to one PR. A draft description with a deferral failed the gate.
- The evaluators found two gaps, and this PR closes both. One deferral phrase of an evaluator passed the patterns, and a new pattern and a test now hold it. An exit test that needs a night on `main` after the merge had no place, and D-375 and the skill now give it one: the next session writes the result in its own handoff entry.
- The owner then noted that an answer to gitar or review findings needs no new session. Start gate step 2 had blocked a session whose PR got to the hand-over, so an author could not answer findings on its own PR. Step 2 now blocks a merged or closed PR alone, and D-375 and the design table state the rule.
- Automatic gitar reviews are paused. A `Gitar review` comment at 19:47 UTC ran a manual review of `00816c8`, which approved with 1 finding, and the finding had merit. The pattern `will update` matched any sentence about a job. `887f94f` ties the pattern to a document, and `RuntimeBehaviorTextIsNotADeferral` fails on the old pattern (D-250, D-303).
- Session 168 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `9b27afc`, the base revision of this branch. The effective head is `887f94f`, the answer to the gitar finding, and this entry is in a metadata commit above it (D-184). Pending owner merge.
- Remote head: `origin/feat/one-pr-one-session` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` without the Smoke category: 1129 tests, 0 failures. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 36 files. `ste-check`: 0 findings in 18 files. `git diff --check` is clean, and the YAML of `doc-gate.yml` parses.
- No Core, Game, content, or asset change, so the simulation version stays 11, `bit-identity` stays `a0b32bad006b3dfe`, and `asset-qa` and the smoke session do not read this change.

### In flight

This PR changes `WhatYouCarry.Tools` and `.github/`, so it needs the Codex review per the `pr-review` skill at the effective head (T-4, D-190). The `review-override` label does not apply. The owner then merges. PR-69 follows in a fresh session.

### Traps and gotchas

- The `doc-gate` job reads the PR description. Edit the description, and the job runs again on the edited event.
- The `doc-gate` job runs on `pull_request` from the PR head, so this PR runs the new rules on itself.
- A roadmap or design mark `Done in PR #N` moves the effective head (D-184). Write it after the PR opens and before the gitar pass. The handoff and the review record do not move it.
- The deferral check reads a fixed list of phrases. A new form of deferral passes it, and the reviewer catches it.
- An author session can answer the findings of its own PR after the hand-over. It never starts another PR.
- The harness exposes no session identity. The start gate of the skill and the owner hold the clean session rule.
- The memory note on merge dates applies to the history alone. A new PR writes no merge date.
- The next ids are D-377, OQ-177, F-102, PR-70, and Session 179.

### Open questions that block progress

None blocks this PR. The PR-66 session asks the owner for the shape of a tier first (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill at the effective head and writes the review record on this branch. The owner then merges. A fresh session then opens PR-69 under D-375 and D-376.

## Session 177: 2026-09-16, Claude Code

Author: Claude Code
Session: add the `gitar-review` skill, and make `pr-review` and `AGENTS.md` link to it. The owner told the session to commit and push to `main` directly, with no PR.

### What this session did, and why

- The owner added the `gitar-review` skill and asked that `pr-review` link to it, with no copy and no wrong text.
- The section "The automated pass" of `pr-review` held a second copy of the procedure. Three parts were wrong. It gave the pause note as the trigger, it gave no proof that a review is current, and it asked for a push after each fix.
- That section now loads `gitar-review` and keeps only the rules of this repo. The author alone answers gitar, a reply names no source of work, a PR is ready for the other provider or the override, and the handoff records the pass.
- `AGENTS.md` and `CLAUDE.md` said that `pr-review` holds both procedures, and they gave the pause note as the trigger. Both files now load `gitar-review` and use the trigger of that skill. The two files stay identical (D-122).
- D-374 records the owner instruction. D-250 and D-303 gain a note of the part that D-374 revises (D-186).
- The skill file `.claude/skills/gitar-review/SKILL.md` enters the repo in this commit.
- Session 167 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` held `0d2a709`, the squash merge of PR #76. This session adds one docs commit on `main`, and that commit holds this entry.
- Remote head: `origin/main` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet test`: 1111 tests, 0 failures, without the Smoke category. `ste-check`: 0 findings in 17 files, after one change in `gitar-review` (see the traps).
- No code changed. `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `ac534d9`.

### In flight

Nothing. The commit went to `main` with no PR, on the owner instruction, so no gitar pass and no `review-override` label apply to it.

### Traps and gotchas

- The memory note of a check run as the proof of a current gitar pass is out of date. A paused gitar attaches a check with the pause note. Apply "Prove that a review is current" in `gitar-review`.
- Line 42 of `gitar-review` failed STE 3.6 with "is not resolved". It now reads "Read each open thread." Put the same change in the copy of each other repo.
- `gitar-review` is the same file in each repo that uses gitar. Put a rule of this repo in `pr-review` or `AGENTS.md`, and not in `gitar-review`.
- The next ids are D-375, OQ-177, F-102, PR-70, and Session 178.

### Open questions that block progress

None blocks this change. The list of Session 176 stands: the PR-66 session asks the owner for the shape of a tier first (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3.

### Next concrete action

A fresh session opens PR-69: it gives the two publish steps of `night.yml` the ref condition of D-373, and it adds the test of exit test 1. After each push, that session loads `gitar-review`. PR-66 follows, and it asks the owner for the shape of a tier first (D-350).

## Session 176: 2026-09-16, Claude Code

Author: Claude Code
Session: record the merge of PR-68 as PR #75, and the owner answer D-373 on the night record, in the same invocation as Session 174 (D-297). Branch `docs/pr-68-merge-record`, PR #76.

### What this session did, and why

- Session 174 opened PR-68 as PR #75. Session 175 approved `cb3c2e2` in `docs/reviews/pr-75.md` with no finding. The owner merged PR #75 as `ac534d9` at 13:47 UTC on 2026-09-16.
- `docs/design.md` marks PR-68 done and closes the F-101 row with the cause and the fix. The Phase 2 roadmap gains the PR-68 status line and the mark in sequence item 17.
- The owner asked why a night ever runs on a branch commit. The answer named a real gap. D-275 guards the read of the night record, and no guard reads the ref on the write.
- The hand night of PR-68 replaced the one record of `night-results` with a branch commit. That cost nothing over the `4bc8cd4` failure. The same write over a fresh success turns the `night-gate` job of every PR red. The publish step also keeps no history, because it starts a new orphan branch each night.
- D-373: the two publish steps of `night.yml` take the condition `github.ref == 'refs/heads/main'`. A night on another ref runs every step and writes no record, and its run log carries the evidence. OQ-176 records the question, and D-274 gains a revision note for the ref alone. PR-69 carries the code, at sequence item 18 before PR-66.
- The scheduled night of 2026-09-16 started at 13:23 UTC on `7345c9c`, the commit before the merge, because the 08:07 cron ran 5 hours 16 minutes late (F-95). That commit holds the defect, so the run would fail and write a failure record. A cancel of it freed the one Mac runner, and the cancel wrote a cancelled record.
- The night of run 35104616127 passed on `main` at `ac534d9` and ended at 15:22 UTC. The two bot sets of 5000 seeds and the sweep of 100000 seeds passed. The record reads `ac534d9` with the status success, and that commit is on `main`, so the `night-gate` job is green again.
- The roadmap kept no resolved line for OQ-174 and OQ-175. This session added them beside OQ-176.
- Session 166 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `ac534d9`, the squash merge of PR #75. This branch holds one docs commit above it, `975f5b6`, and this entry stands in a metadata commit over it (D-184).
- Remote head: `origin/docs/pr-68-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet test`: 1111 tests, 0 failures, without the Smoke category. `ste-check`: 0 findings in 16 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `ac534d9`.
- CI on `975f5b6`: CI and bit identity passed on the three platforms, with the compare job. Asset-qa, det-lint, and STE check passed. The `night-gate` job passed, the first green one since the night of 2026-09-15 (F-101). `evaluate` fails and `review-gate` is grey, because this PR carries the `review-override` label in place of a review record (D-188, D-190, D-251).
- The record on the branch `night-results` holds `ac534d9` with the status success, from 15:22 UTC on 2026-09-16. It goes stale 48 hours after that time (D-177).

### In flight

PR #76: documentation alone. The `review-override` label carries the review (D-188, D-190). The owner merges it. PR-69 follows in a fresh session (D-121), and PR-66 comes after it.

### Traps and gotchas

- The GitHub PR #75 is PR-68, and the GitHub PR #76 is this merge record. The roadmap id PR-69 guards the night record, and PR-66 digs the ramps and the tiers.
- The merge mark of PR-68 uses the UTC date of the merge, 2026-09-16. D-373, OQ-176, and this entry use the local date, also 2026-09-16.
- A night on a branch still overwrites the record of `main` until PR-69 lands. Run a night on `main` after a branch night, or leave the branch night for last.
- Dispatch a hand night only after the macOS legs of CI, smoke, and bit identity finish. The night and those three legs take the one self-hosted Mac runner, and a night holds it for about 80 minutes.
- A queued night cancels with no harm, because the record steps never run before the job starts. A night that already started writes a record on any outcome, a cancel included.
- A squash merge gives the branch commits no place in the history of `main`, so a night record from a branch always fails the ancestry check of D-275.
- The next ids are D-374, OQ-177, F-102, PR-70, and Session 177.

### Open questions that block progress

None blocks PR #76. D-373 resolves OQ-176, and PR-69 carries it. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #76 with the `review-override` label. A fresh session then opens PR-69: it gives the two publish steps of `night.yml` the ref condition of D-373, and it adds the test of exit test 1. PR-66 follows, and it asks the owner for the shape of a tier first (D-350).

## Session 175: 2026-09-16, Codex

Author: Codex
Session: review PR-68 as PR #75, the shaft landing fix, at effective head `cb3c2e2`. Branch `feat/pr-68-shaft-landing`.

### What this session did, and why

- Read the PR description, complete diff, affected Core callers, tests, roadmap, design, decisions, questions, and every PR comment.
- Checked the provider gate. Session 174 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Verified that the pillar pass skips every column in a shaft hole, and that the generator checks each landing for a floor cell and reachability.
- Verified the seed 79146 regression, the simulation version rise, the bit-identity update, and the automated pass correction at `cb3c2e2`.
- Found no in-scope defect. Wrote `docs/reviews/pr-75.md` with the verdict `Ready for owner merge` for `cb3c2e2`.

### State of the build

- `main` and the merge base are `7345c9c`. The effective head of PR #75 is `cb3c2e2`. The later `ec4ce65` commit changes only `docs/session-handoff.md` and `docs/session-handoff-archive.md` under D-184.
- Remote head: the review record and this handoff are pushed to `origin/feat/pr-68-shaft-landing`, and `gh pr view` verifies the remote head.
- `git diff --check` and the byte-identity check of `AGENTS.md` and `CLAUDE.md` passed.
- The local build produced no output or completion result and was interrupted. Revision-matched CI passed CI, Smoke, Bit identity, compare, Bots, Asset QA, det-lint, and STE check. The hand night and local 100000-seed sweep passed. `night-gate` stays red by D-372.

### In flight

PR #75 is ready for owner merge. The owner merges with the red `night-gate` under D-372. The first scheduled night on `main` restores the base-branch night evidence. PR-66 follows in a fresh session.

### Traps and gotchas

- The GitHub PR is #75, and the roadmap item is PR-68.
- The effective head is `cb3c2e2`, not this metadata commit (D-184).
- The local .NET build gave no completion result. Do not report that attempt as a passed gate.
- `CheckShaftLandings` treats a missing solid landing as a contextual construction error. It gives a separate cause from an unreachable floor cell (D-113, T-2).
- The next ids are D-373, OQ-176, F-102, PR-69, and Session 176.

### Open questions that block progress

None blocks PR #75. D-372 resolves OQ-175. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #75 with the red `night-gate` under D-372. A fresh session then opens PR-66 and asks the owner for the shape of a tier first (D-350).

## Session 174: 2026-09-16, Claude Code

Author: Claude Code
Session: finish PR-68, the shaft landing fix of D-370, and open it as PR #75, with the owner answer D-372. Branch `feat/pr-68-shaft-landing`.

### What this session did, and why

- Session 173 left the fix in the working tree with no commit, no push, and no PR. This session ran the gates, corrected one stale test, committed, pushed, and opened the PR.
- The full suite found a failure that Session 173 never ran. `SimulationTests.TheConstantsHold` pins the simulation version, and it still read 10. The assert now reads 11, and its remark names the pillar rule of F-101 in place of the dig restart of PR-67, which PR-64 already left stale.
- The owner answered the night gate of this PR. D-372: PR-68 merges although the `night-gate` job is red, as D-371 merged the PR-65 merge record. OQ-175 records the question.
- The `night-gate` job cannot go green on this PR. D-275 fails a record whose commit is not an ancestor of the base branch, so a hand night on this branch reads as foreign. `main` holds the defect, and seed 79146 is deterministic, so every night on `main` fails until this fix merges. The two rules make a deadlock, and D-372 breaks it.
- The PR-68 gate line of the Phase 2 roadmap names the red `night-gate` and D-372.
- The automated pass of gitar approved `2d4be05` with one finding of the Quality kind (D-250). `CheckShaftLandings` threw on two conditions and named one cause, so a landing that a pillar or a heap of rubble takes away read as an unreachable cell. Each condition now has its own message and a `cause` key (D-113, T-2). A second pass, which the comment `Gitar review` started, approved `cb3c2e2` with that finding closed and no new one (D-303).
- A hand run of `night.yml` on this branch passed at `cb3c2e2` and gives the CI evidence of exit test 2: run 35067529373, 1 hour 18 minutes (D-370). The two bot sets of 5000 seeds and the sweep of 100000 seeds all passed. The record on `night-results` now reads `cb3c2e2` with the status success, and the `night-gate` job stays red, because that commit is not on `main` (D-275). The first dispatch, run 35066777588, stood at `2d4be05`. A cancel of it before its start left the record untouched.
- Session 164 and Session 163 moved to the archive, because the file held twelve entries with this one.

### State of the build

- `main` is at `7345c9c`. The effective head of PR #75 is `cb3c2e2`, the answer to the automated pass. `2d4be05` below it holds the fix and the registers. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-68-shaft-landing` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 1116 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 36 files. `asset-qa`: 0 findings, 2 models, 0 overlays, 3 animations. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `a0b32bad006b3dfe`.
- The night sweep of 100000 seeds passed on this head in a local run of Session 173: 2 tests and 0 failures in 43 minutes. That is the local evidence of exit test 2.
- The bot sweep of seeds 1 to 100 passed on both policies: 0 softlocks and 0 crashes.
- CI on `cb3c2e2`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, and STE check passed, and the automated pass of gitar approved the head. No macOS leg ended "not acquired". `night-gate` fails, and D-372 carries the merge. `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `cb3c2e2` completed before the push of this entry, so that push cancels nothing (D-356).

### In flight

PR #75: the Codex review per the `pr-review` skill at the effective head `cb3c2e2` (T-4). The owner then merges with the red `night-gate` (D-372), and a docs PR records the merge (D-297). PR-66 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #75 is PR-68. The roadmap id PR-66 digs the ramps and the tiers, and it comes next.
- The effective head is `cb3c2e2`, and not the metadata commit of this entry (D-184). The register lines of D-372 and OQ-175 are outside the metadata set, so they sit in the code commit.
- `night-gate` is red on this PR by design, and D-372 carries the merge. A reviewer reads that line as an answer and not as a miss.
- The hand night replaced the one record on `night-results`. It now reads `cb3c2e2` with the status success, in place of the `4bc8cd4` failure. The gate stays red for every PR either way, because the new record commit is not on `main`.
- The simulation version is 11, and the bit-identity known answer is `a0b32bad006b3dfe`. A test that pins the version by a literal breaks on the next rise. `SimulationTests.TheConstantsHold` is the one such test.
- `IsUnderShaft` reads every shaft of the plan and not the shafts over the chamber alone. A column under any shaft loses its pillar, which costs a few pillars and keeps the rule simple (T-1).
- Rubble is solid, so a collapse in a shaft column can fill a landing by the same path as a pillar. The sweep of 100000 seeds found no such floor, and `CheckShaftLandings` now makes any such floor a loud error (T-2).
- The PR sweep reads 5000 seeds and never reads seed 79146, so a PR run passes while a night fails.
- The next ids are D-373, OQ-176, F-102, PR-69, and Session 175.

### Open questions that block progress

None blocks PR #75. D-370 carries the fix, and D-372 carries the merge. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #75 per the `pr-review` skill at the effective head `cb3c2e2` and writes `docs/reviews/pr-75.md`. The owner then merges with the red `night-gate` (D-372), and a docs PR records the merge (D-297). A fresh session then opens PR-66, and it asks the owner for the shape of a tier first (D-350).

## Session 173: 2026-09-16, Claude Code

Author: Claude Code
Session: open PR-68, the shaft landing fix, with the owner answer D-370. Branch `feat/pr-68-shaft-landing`. The owner reset the context in the middle of the work, so this entry hands over a working tree with no commit, no push, and no PR.

### What this session did, and why

- The branch came from `origin/main` at `7345c9c`, the merge of PR #74.
- A scratch dump outside the repository named the cause of F-101. Seed 79146, floor 7 holds chamber 1 with its floor at row 9 and chamber 3 under it with its floor at row 3. `DigPlan.TryDigShaft` proves two air rows over the landing of each of the nine columns of a hole, and it then carves the shaft at the column (25, 48). The detail pass runs after it, and `DetailPass.RaisePillars` raised a pillar of chamber 3 at the floor cell (25, 3, 48), the column of that shaft. The pillar filled the rows 4 to 8, so its top took the landing air row 8.
- The landing became the top of a pillar, one cell wide, five rows over the chamber floor. A body drops onto it and cannot climb back, so the search of PR-9 reaches no path to it. Nothing excluded a shaft column from a pillar.
- The fix: `DetailPass.IsUnderShaft` answers whether a shaft drops through a column, and `RaisePillars` skips such a column, beside the anchor of a chamber. The check comes after the draw, as the anchor check does, so the draw order does not change.
- `FloorGenerator.CheckShaftLandings` confirms that the spawn reaches the landing of every shaft, and the error names the chamber, the column, and the row (D-112, T-2). The class remark names the new confirmation.
- The simulation version rose to 11, and `bit-identity` prints `a0b32bad006b3dfe` (D-260, G-20). `BitIdentityTests.ExpectedHash` holds the new answer, and its remark names the move.
- `ProcgenTests.ShaftOfSeed79146LandsOnAReachableFloor` is the regression test of exit test 1. On `4bc8cd4` with that test, it fails with "The shaft at Column { X = 25, Z = 48 } lands at row 8, and the spawn does not reach it". It passes with the fix.
- `docs/design.md` names the cause in the F-101 row.
- The night sweep of 100000 seeds passed on this head, with the variable `WYC_NIGHT_SWEEP=1`: `EveryChamberReachable` and `DetailKeepsEveryChamberReachable`, 2 tests and 0 failures, in 43 minutes. That is the local evidence of exit test 2.

### State of the build

- `main` is at `7345c9c`. The branch `feat/pr-68-shaft-landing` stands on it with no commit. Nothing is pushed, and no PR exists.
- The working tree holds six modified files, and no untracked file: `WhatYouCarry.Core/Procgen/DetailPass.cs`, `WhatYouCarry.Core/Procgen/FloorGenerator.cs`, `WhatYouCarry.Core/Simulation/SimulationVersion.cs`, `WhatYouCarry.Tests/BitIdentityTests.cs`, `WhatYouCarry.Tests/ProcgenTests.cs`, and `docs/design.md`.
- `dotnet build`: 0 warnings, 0 errors. The focused run of `ShaftOfSeed79146LandsOnAReachableFloor`, `EveryChamberReachable`, and `DetailKeepsEveryChamberReachable` passed 3 tests with the PR sweep of 5000 seeds.
- The bot sweep of seeds 1 to 100 passed: the random walker ended by budget with 0 softlocks and 0 crashes, and the greedy descender reached the bottom on all 100.
- `bit-identity` prints `a0b32bad006b3dfe`. The full suite, `det-lint`, `asset-qa`, `ste-check`, and the Godot build check did not run yet on this head.
- The night sweep of 100000 seeds passed on this head: 2 tests and 0 failures in 43 minutes. No floor of those seeds holds an unreachable shaft landing, and none holds an unreachable chamber floor cell. The generator threw on no floor of the sweep.
- The record on the branch `night-results` holds `4bc8cd4` with the status failure, so the `night-gate` job fails on every PR until a night passes (D-115, D-177).

### In flight

PR-68 is unfinished. No commit exists. The night sweep of exit test 2 passed on this head already. The next session runs the other gates, commits, pushes, opens the PR, dispatches a night on the branch, and hands the PR to a Codex review (T-4).

### Traps and gotchas

- This file holds eleven entries with this one. Move Session 163 to the archive with the commit of the next entry (D-146).
- The scratch files of the old session stand outside the repository, in `/private/tmp/claude-501/-Volumes-SSD-1TB-what-you-carry/dc03dda0-997a-4585-8aa6-7c7379eff1a1/scratchpad/`. `pr68-body.md` is a draft PR description with the placeholders `{NIGHT_SWEEP}`, `{SWEEPS}`, and `{LOCAL_CHECKS}`. `detail-dump.txt` and `shaft-dump.txt` hold the diagnosis. A new session reads them by that absolute path, or writes the description again from this entry.
- A scratch test in `WhatYouCarry.Tests/` never reaches a commit. Delete it, check `git status`, and build again before any commit.
- The PR sweep reads 5000 seeds and never reads seed 79146, so a PR run passes while a night fails. The night sweep needs the variable `WYC_NIGHT_SWEEP=1`, and it takes about 35 minutes.
- `night.yml` takes a manual event, so a night runs on the branch before the merge (D-370).
- The version rise alone moves the bit-identity answer, because the replay header holds the version. The old session did not measure the answer with the version held at 10, so it does not know whether a floor of the sweep also moved.
- The generator now throws on an unreachable shaft landing. A floor with another cause of such a landing becomes a loud error, and the night sweep names its seed.
- The next ids are D-372, OQ-175, F-102, PR-69, and Session 174.

### Open questions that block progress

None blocks PR-68. D-370 carries the fix. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Finish PR-68 in this order.

1. Read this entry and `git status`. The six files above hold the whole change, and the night sweep of exit test 2 passed on them.
2. Run the gates: `dotnet build`, `dotnet test WhatYouCarry.slnx --no-build`, `det-lint`, `asset-qa`, `ste-check`, and the Godot build check. The night sweep needs no second local run.
3. Add the Session 174 entry, move Session 163 to the archive, and commit the six files with the entry.
4. Push, open the PR, and answer the automated pass (D-250).
5. Dispatch a night on the branch with `gh workflow run night.yml --ref feat/pr-68-shaft-landing`, and wait for a success record (D-115, D-370). The `night-gate` job stays red until a night passes.
6. Hand the PR to a Codex review at the effective head (T-4).
