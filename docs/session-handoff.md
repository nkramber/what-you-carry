# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 187: 2026-09-18, Codex

Author: Codex
Session: review GitHub PR #80, the night record ref guard. Branch `feat/night-record-ref`.

### What this session did, and why

- Checked the complete PR diff, the workflow contract, the regression test, the roadmap, and the GitHub comments.
- Verified the author as Claude Code from Session 186. Codex is the eligible reviewer under T-4 and D-101.
- Found no in-scope defect. Added `docs/reviews/pr-80.md` for effective head `63bb4a3`.

### State of the build

- Base and merge base: `3434055`. Effective head: `63bb4a3`. The later `cadcfbb` commit changes only `docs/session-handoff.md` (D-184).
- Local `dotnet test` built the projects, then produced no test result after discovery. The run was stopped, so it is incomplete local evidence.
- GitHub checks on the metadata tip passed CI, Smoke, Bit identity, compare, Bots, Asset QA, det-lint, STE check, doc-gate, and night-gate. `evaluate` failed before publication because no review record existed (D-251).
- Remote head: the review record and this entry are pending commit and push.

### In flight

GitHub PR #80 is ready for owner merge at effective head `63bb4a3`. Exit test 3 of PR-69 needs a hand night on `main` after merge, and the session that runs it states the result in its handoff entry (D-375).

### Traps and gotchas

- The current PR branch is `feat/night-record-ref`. Keep the review head at `63bb4a3`; the handoff-only commit does not change it (D-184).
- The full local test run did not complete. Revision-matched GitHub checks passed on the later metadata tip under D-357.
- The next ids are D-386, OQ-179, F-103, PR-70, and Session 188.

### Open questions that block progress

None blocks PR #80. OQ-177 and OQ-178 bind PR-70 and block nothing.

### Next concrete action

The owner merges GitHub PR #80. A new clean session starts PR-70, the skill port of D-383 to D-385.

## Session 186: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-69, the night record ref guard. Branch `feat/night-record-ref`.

### What this session did, and why

- Gave the two record steps of `night.yml` the condition `github.ref == 'refs/heads/main'` beside `always()` (D-373). A night on another ref now runs in full and writes no record, so it cannot replace the record of `main`.
- Added `RepositoryShapeTests.TheNightRecordStepsRunOnMainAlone`. It reads the text of each record step and the trap that no step holds a bare `always()`.
- Repaired the trunk (F-102). `main` at `3434055` failed two tests. The description of the `gitar-review` skill held a sentence of 27 words, and the handoff held 11 entries.
- Recorded the owner answers of this session for the skill port: D-383 the scope, D-384 the ceiling of a skill file, D-385 the reference files of the session skill.
- Filed OQ-177 on front matter and the STE rules, and OQ-178 on a merge with red checks.

### State of the build

- `main` is at `3434055`, and it is red. GitHub PR #79 merged with `ste-check`, `doc-gate`, and the three build legs red.
- The full suite on the inherited tree: 1150 passed, 2 failed, both from the trunk. After the repair the document tests and the checker pass.
- `dotnet build` is clean. `ste-check` reports 0 findings in 19 files.
- `TheNightRecordStepsRunOnMainAlone` fails on the workflow of `origin/main` and passes on this branch.
- Remote head: `origin/feat/night-record-ref` at the commit that holds this entry, checked with the session end gate.

### In flight

- GitHub PR #80 is pending owner merge on branch `feat/night-record-ref`. It holds the guard, the test, the trunk repair, and the records. The pass of gitar approved effective head `63bb4a3` with no finding and no open thread. The PR waits for the review of the other provider, which comes from Codex.
- Exit test 2 of PR-69 passes. The hand night of this branch ran every sweep to the end in 78 minutes, both record steps skipped, and the record on `night-results` stayed at `24f47be`, the success of `3434055`.
- Exit test 3 of PR-69 needs a hand night on `main`, which no branch can give.

### Traps and gotchas

- Run the full suite at the start of a session. The trunk was red, and two focused runs would hide it.
- A branch night writes no record now. Such a night proves a fix through its run log alone, and a PR body cites that run.
- The night legs and the macOS legs of a PR share the one Mac runner. Dispatch the night after the checks of the PR.
- The deferral phrases of `doc-gate` read the PR body and the newest handoff entry. Name a later PR by its id alone.
- The next ids are D-386, OQ-179, F-103, PR-70, and Session 187.

### Open questions that block progress

None blocks GitHub PR #80. OQ-177 and OQ-178 block nothing, and both bind the work of PR-70.

### Next concrete action

The owner merges GitHub PR #80 after the review. Then a new clean session starts PR-70, the skill port of D-383 to D-385, which holds seven items and its own ceiling change.

## Session 185: 2026-09-16, Codex

Author: Codex
Session: re-review PR #78 at effective head `337d706`. Branch `feat/context-budget`.

### What this session did, and why

- Re-reviewed the P2-1 correction, its response, the documented lookup command, and its regression tests.
- Confirmed that the command finds D-379 to D-382, the relevant revisions, OQ-9, and OQ-44.
- Updated `docs/reviews/pr-78.md`. P2-1 is fixed at `337d706`, and the earlier verdict remains in the record.

### State of the build

- `main` is at `58e4fc8`. The effective head is `337d706`. Later commits change metadata only (D-184).
- The focused `ReviewGateRulesTests`, `RegisterLookupTests`, and `ContextBudgetTests` run passed 27 tests. The documented command and `git diff --check` passed.
- The owner directed this session to exclude CI status from the review verdict.
- The first review publication, `00c29f6852990e0a05a063a45a99f25b1343c41d`, matched the remote head when `gh pr view` checked it.
- Remote head: `origin/feat/context-budget` at the commit that holds this entry, checked with the session end gate before the session ended.

### In flight

PR #78 is ready for owner merge at effective head `337d706`. P2-1 is fixed with regression coverage.

### Traps and gotchas

- The review verdict covers effective head `337d706`. Later commits change metadata only.
- The lookup command needs every relevant D-# and OQ-# in its `d` and `q` values.
- The next ids are D-383, OQ-177, F-102, PR-70, and Session 186.

### Open questions that block progress

None blocks PR #78.

### Next concrete action

The owner merges PR #78.

## Session 184: 2026-09-16, Claude Code

Author: Claude Code
Session: answer review finding P2-1 of PR #78, in the author session of the PR (D-375). Branch `feat/context-budget`.

### What this session did, and why

- Session 183 reviewed PR #78 at `3eb7142` and gave `Changes required` for P2-1: the D-378 lookup command held fixed ids, and no sentence told the reader to replace them. The owner asked for the answer in this author session.
- The finding reproduced, with full merit. A size cut of `CLAUDE.md` had removed the words "with the ids in place of the examples".
- The command now starts with `d='146|375'; q='9|44'`, and its three patterns read `$d` and `$q`. The sentence before it tells the reader to set every D-# and OQ-# number of the task. Two archive sentences are shorter, so the agent files hold 14591 bytes, 2 fewer than on `main`.
- `RegisterLookupTests` reads the command from `AGENTS.md` and applies it to the registers. It finds the rows of D-379 to D-382, OQ-9, and OQ-44, and the revisers D-377 and D-381 of D-187 and D-374. All three tests fail on the `AGENTS.md` of `3eb7142`.
- `docs/reviews/pr-78-response.md` records the disposition and the regression check.

### State of the build

- `main` is at `58e4fc8`. The effective head is `337d706`, the correction commit, and this entry sits in a metadata commit above it (D-184). Pending the repeat review and the owner merge.
- Remote head: `origin/feat/context-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` without the Smoke category: 1146 tests, 0 failures. `ste-check`: 0 findings in 19 files. `det-lint`: 0 findings.
- CI on `337d706` did not complete in this session. The owner told the session not to wait on CI, so no CI result exists here for that head. The push of this entry cancels the runs of `337d706`, and CI on the tip counts for it (D-356, D-357). The reviewer reads that result.
- The automated pass of gitar: automatic reviews stay paused. A `Gitar review` comment at 22:17:56 UTC got "On it" at 22:18:20, and a new dashboard comment approved `337d706` at 22:18:43 with no finding and no thread (D-303, D-374).

### In flight

PR #78: the repeat Codex review of P2-1 per `pr-review` at `337d706`, with the CI result of the tip. The owner then merges.

### Traps and gotchas

- The correction changes `AGENTS.md` and a test, so it moves the effective head past `3eb7142` (D-184). The review of `3eb7142` no longer covers the head.
- The lookup patterns sit in double quotes, so the shell expands `$d` and `$q`. The same lines work in bash and in zsh.
- The next ids are D-383, OQ-177, F-102, PR-70, and Session 185.

### Open questions that block progress

None blocks PR #78.

### Next concrete action

A Codex session runs the repeat review of P2-1 per the `pr-review` skill at the correction head and sets the verdict. The owner then merges PR #78.

## Session 183: 2026-09-16, Codex

Author: Codex
Session: review PR #78 at effective head `3eb7142`. Branch `feat/context-budget`.

### What this session did, and why

- Reviewed the full change and its tests, tools, skills, decisions, design, handoff, PR description, comments, and checks.
- Found P2-1 in `docs/reviews/pr-78.md`: the register command searches fixed ids, so it misses decisions for other tasks.

### State of the build

- `main` is at `58e4fc8`, the base. The effective head is `3eb7142`, and remote tip `77c6f66` changes only handoff metadata (D-184).
- The focused rotation and context budget tests passed: 12 tests, 0 failures. The full non-Smoke test attempt did not complete locally and was cancelled.
- CI on `3eb7142` passed, as recorded in the handoff update at `77c6f66`. Checks after that metadata push passed for the platforms and required workflows. `evaluate` failed before this review record, and `review-gate` skipped.

### In flight

The author must correct P2-1 and request a Codex re-review. The owner merges after the finding closes.

### Traps and gotchas

- The Gitar approval covers effective head `3eb7142`. Later commits changed metadata only.
- The review commit must keep `3eb7142` as the effective head.

### Open questions that block progress

None.

### Next concrete action

The author loads `review-response`, updates the lookup command in both agent files, and adds a regression check.

## Session 182: 2026-09-16, Claude Code

Author: Claude Code
Session: cut the agent context cost of each session, with the owner instructions D-377 to D-382. Branch `feat/context-budget`.

### What this session did, and why

- A token audit of this repository read 10 recent sessions of each harness. Most input came from the model calls after a status poll (17.2% Claude Code, 22.8% Codex) and after handoff and archive work (17.3%, 27.5%). Register text and one large review skill added more. The owner told the session to apply every P0 and P1 fix of the audit.
- D-377: the first action prints the newest handoff entry alone, and the read order adds the newest entry of the branch. The end of a session takes the number from the top heading after a fetch. D-187 gains a revision note for the second full read.
- D-378: `AGENTS.md` holds one lookup command for all D-# and OQ-# ids of a task, with a line that finds each revision (D-186). A check on five superseded decisions printed the superseding id of each.
- D-379: the `handoff-rotate` command moves each entry after the tenth to the archive top with its text intact. Nine tests cover it, and a seed loop of 300 seeds proves the text and the order. A mutation that dropped one moved entry failed three tests.
- D-380: the `one-pr-one-session` skill waits on checks with one `gh pr checks --watch` command, then prints the final state once. The first run on this PR used `--fail-fast` and stopped at once on `evaluate`, which fails until a review record exists (D-251). The skill now waits for every check. `gitar-review` does not change, because other repositories use the same file.
- D-381: `pr-review` keeps the reviewer procedure, and the new `review-response` skill holds the author procedure. D-374 gains a revision note.
- D-382: `ContextBudgetTests` caps the agent files at 15000 bytes, each skill at 12000 (`pr-review` 31000), the handoff at 60000, and its newest entry at 7000.
- The rule paragraph of this file moved from between Session 181 and Session 180 to the file header, so it no longer moves with the entries.
- `CLAUDE.md` holds the new rules in 14586 bytes, 7 fewer than before. The gitar bullets that copied the `gitar-review` skill are gone (D-374), and some build prose is shorter with no rule lost.

### State of the build

- `main` is at `58e4fc8`, the base of this branch. The effective head is `3eb7142`, PR #78, and this entry sits in a metadata commit above it (D-184). Pending owner merge.
- Remote head: `origin/feat/context-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` without the Smoke category: 1143 tests, 0 failures. `ste-check`: 0 findings in 19 files. `det-lint`: 0 findings. No Core, Game, content, or asset path changed, so the simulation version, `bit-identity`, and `asset-qa` stand.
- CI on `3eb7142`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, doc-gate, night-gate, and STE check passed. `evaluate` fails and `review-gate` skips, because no review record exists yet (D-251). Every run completed before the push of this entry, so that push cancels nothing (D-356).
- The automated pass of gitar: the automatic review approved `ca570e2` at 21:30:30 UTC, one push behind the head, with automatic reviews paused. A `Gitar review` comment at 21:31:21 UTC got "On it" at 21:31:40, and the dashboard approved `3eb7142` at 21:32:03 with no finding and no thread (D-303, D-374).
- Start paths, bytes before and after: an implementation author 74615 to 36154, an author who answers findings 109690 to 44642, a reviewer 109690 to 66597.

### In flight

PR #78 changes `WhatYouCarry.Tools` and `WhatYouCarry.Tests`, so it needs the Codex review per `pr-review` at the effective head (T-4). The `review-override` label does not apply. The owner then merges.

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
