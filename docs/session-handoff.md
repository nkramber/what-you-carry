# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 190: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-66, the ramps and the chamber tiers of the generator. Branch `feat/pr-66-ramps-and-tiers`.

### What this session did, and why

- Made the transitional prompt of the merge an automatic rule. The message `Merged PR #N` of the owner starts it, and the owner asks for no prompt. The trigger and the block sit in `references/merge-prompt.md`, and `SKILL.md` takes two short lines. The block names the merge commit, the next PR, each open question, and each owner answer that the roadmap names and no OQ-# holds. `RepositoryShapeTests.SessionSkillTriggersTheTransitionalPrompt` reads both halves.
- Asked the owner the shape of a tier before any generator code, as D-350 and the roadmap require. D-388 to D-391 hold those four answers. D-392 and D-393 followed, because the measured tier rate fell far under the chances of D-350.
- Dug true ramps in place of the one-block steps of the old code, and added the third carve rule of `DigCanvas`: no unit takes a floor row one row over or under the floor of a walkable space beside it (D-347).
- Built the tiers after the whole dig, with the four shapes, the drawn share, the ramp of a slope that fits, and the shrink of D-388 to D-391.
- Raised F-103, the shafts of a floor. The owner answered D-394, and a pass now digs a drift under a chamber so a shaft of that chamber has a landing.
- Repaired every defect that the full suite and the wide sweeps found. The traps below name each one.

### State of the build

- Base and merge base: `27db615`, the merge of PR #81.
- `dotnet test` locally: 1157 passed, 0 failed, with the filter `Category!=Smoke`. `det-lint` reports 0 findings in Core and 0 in Game. `ste-check` reports 0 findings over 34 files.
- The simulation version is 12, and the bit-identity answer is `15904316a1b4ec07`. The sweep floors are 32 by 12 by 32, and the shaft route needs 4 rows under a chamber, so the sweep reads no shaft route.
- Measurements over 1000 to 60000 floors: 52 percent of floors hold a tier, 2.5 percent hold a shaft against 0.045 percent on the base, zero one-block steps, and no generator error over 40000 floors. One floor costs 29 milliseconds against 21 on the base.
- Remote head: `origin/feat/pr-66-ramps-and-tiers` reached `97efa5c`, and the status showed no `[ahead N]`. This entry follows it as a metadata commit, so the effective head stays `97efa5c` (D-184).
- Every check of PR #82 passes on `97efa5c`: Gitar, the three build legs, the three smoke legs, bit identity, compare, bots, asset-qa, det-lint, ste-check, doc-gate, and night-gate. `evaluate` fails and `review-gate` shows grey, because no review record exists yet (D-251).

### In flight

GitHub PR #82 holds the ramps, the tiers, the shaft pass, the seven decisions, the two questions, F-103, and this entry. It waits for the owner merge.

The automated pass of gitar is complete on the effective head `97efa5c`. The first review of `5b58a2e` approved with one finding: the `<remarks>` block of `DigShaftRoutes` closed a paragraph that no `<para>` opened. The finding has full merit, and `97efa5c` fixes it. A count of the tags over the file balances, because the missing open and the missing close cancel each other, so a check of the nesting inside each documentation block found the block. That check found no other block over Core, Tests, and Tools. The review of `97efa5c` approved with the verdict "No issues remain", and its one thread is resolved with the commit that fixed it. Findings with merit: one.

The cross-provider review comes next. Codex is the eligible reviewer, because Claude Code wrote this PR (T-4, D-101).

### Traps and gotchas

- A tier built mid-dig walls the walkers in. The tier of the first chamber barred the gallery walker, and 5 percent of floors then ran the whole job budget. `BuildTiers` runs after the dig for that reason.
- A down ramp writes its slope into the row of the walker. The first version replaced the floor of a chamber or of an older tunnel, and the body at the spawn stood in a slope. A ramp cell now digs into solid rock alone.
- A chamber takes the cells of a tunnel ramp into its floor, because a ramp cell is solid. A tier over such a column stood on a slope, and the low end of a tier ramp met one. Both read `ChamberSpace.IsSloped` now. Seed 8911, floor 2 held that defect, and the guard of `CheckTiers` named it.
- The hole of a shaft takes the floor of each of its nine columns away. One hole landed on the high end of a ramp and took its landing, so seed 177, floor 13 held a ramp that led nowhere. A shaft now takes no column of a ramp or of an end of one.
- The greedy descender read the row of the next cell to decide a jump. It jumped in place on every ramp, and it never jumped from a ramp onto a block of the same row. The walk now reads the floor height of the next cell against the feet, and the slope where the body enters a ramp cell.
- The camera test of PR-8 read a whole ramp cell as solid. The camera rests over a slope in open air, and the ray of D-246 stops at the slope.
- The seven floors of F-98 dig inside the budget now. The restart test takes new seeds, measured over 120000 floors: about one floor in 3300 runs the budget.
- Seed 79146, floor 7 digs no shaft, so the F-101 test reads the rule over every shaft of the sweep in place of that one floor.
- A tunnel that crosses a chamber keeps its cross-section (D-342), so its swath takes no tier. That swath, and the straight corridor of the ramp, decide how many floors hold a tier.
- The next ids are D-395, OQ-181, F-104, PR-71, and Session 191.

### Open questions that block progress

None. OQ-179 has its answers in D-388 to D-393, and OQ-180 has D-394.

### Next concrete action

Codex reviews GitHub PR #82, per `.claude/skills/pr-review/SKILL.md`, and writes `docs/reviews/pr-82.md`. The review reads the third carve rule of `DigCanvas`, the tier plan and its guards, the shaft pass, and the jump rule of the greedy descender. Exit test 10 needs the owner to play floor 1 and confirm the ramps and the tiers. About half of the floors hold a tier, so a floor with none needs a second floor or a named seed.

## Session 189: 2026-09-19, Codex

Author: Codex
Session: PR-81, the cross-provider review. Branch `feat/skill-port`.

### What this session did, and why

- Reviewed PR #81 at effective head `be07a42`. The PR ports and splits the repository skills, adds the session runbook and code-conventions skill, and updates the STE front-matter rule.
- Inspected the complete diff, the PR description, the roadmap entry, the cited decisions and questions, the automated comments, and the changed checker and test files.
- Found no actionable defect. The review record is `docs/reviews/pr-81.md`.

### State of the build

- The focused context-budget and STE tests passed 19 of 19. `ste-check` found 0 findings in 34 files.
- The broader filtered suite stalled after compilation and was canceled after a bounded wait. Remote CI, smoke, bit identity, compare, bots, asset-qa, det-lint, ste-check, doc-gate, night-gate, and Gitar passed on the tip `64bc3da`.
- The review record reached remote head `d08e8d9` after the lease-protected metadata update.

### In flight

The owner can merge after the review record reaches the PR branch and `review-gate` turns green.

### Traps and gotchas

- The effective head is `be07a42`. The later commits change only the metadata set.
- The local broad suite gave no result. Treat that run as incomplete evidence, not as a pass.

### Open questions that block progress

None.

### Next concrete action

Run the session end gate, then wait for the review-gate check.

## Session 188: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-70, the skill port. Branch `feat/skill-port`.

### What this session did, and why

- Ran the hand night on `main` for exit test 3 of PR-69, which no branch can give. The scheduled night of 2026-09-18 at `ea338f4` had failed after 27 minutes, because the self-hosted runner lost communication with the server. No record came from that run. The hand night ran every sweep to the end and wrote the record.
- Ported the seven items of D-383. The `gitar-review` skill takes the effective head and the metadata set of D-184, so a commit of metadata keeps a pass current. The `ste-writing` skill takes a glossary of the process terms and a table of every byte ceiling. The `pr-review` skill keeps the procedure, and seven reference files hold each case. The `one-pr-one-session` skill keeps the binding, the start gate, the documents matrix, and the completion gate (D-385). The `design-doc-style` skill takes the entry list of a focused roadmap. A new runbook, `docs/runbooks/session-context.md`, holds the commands of a session. A new skill, `csharp-conventions`, holds the code rules, and both agent files point to it.
- Applied D-384. Every `.md` file under `.claude/skills/` takes one ceiling of 12000 bytes, a reference file included, and the separate ceiling of `pr-review` ends.
- Asked the two open questions that bind PR-70, and recorded the answers. D-386 answers OQ-177, and the STE checker now holds the front matter of a file to rule 6.3 alone. D-387 answers OQ-178, and the owner applies a branch protection rule on `main`.
- Sizes: `pr-review/SKILL.md` fell from 30443 bytes to 4313, `ste-writing` from 10443 to 8496, and each agent file grew from 14695 to 13986 after the code rules moved out. No reference file is over 6927 bytes.

### State of the build

- Base and merge base: `ea338f4`, the merge of PR #80. Effective head: `be07a42`. The tip `a5e7f7f` changes the handoff paths alone, so it is metadata under D-184.
- `dotnet test` locally: 1149 passed, 0 failed, with the filter `Category!=Smoke`. `ste-check` reports 0 findings over 34 files. `doc-gate` passes over 34 changed paths.
- GitHub PR #81 is open. Every check passes on the tip: Gitar, CI on three platforms, Smoke on three platforms, Bit identity, compare, bots, asset-qa, det-lint, ste-check, doc-gate, and night-gate. `evaluate` fails because no review record exists (D-251), and `review-gate` shows grey.
- The night of `main` at `ea338f4` ended success at 2026-09-19T00:45:48Z. The record on `night-results` moved from `24f47be` to `40f148d`, and it names `ea338f4`.
- Exit test 3 of PR-69 passes in full. The hand night on `main` wrote the record, and the `night-gate` job of PR #81 read that record and turned green.
- Remote head: `origin/feat/skill-port` reached `1d46c14` with this entry, and the status showed no `[ahead N]`. A final metadata commit follows that head with the correction below. The effective head stays `be07a42` through every one of them (D-184).

### In flight

GitHub PR #81 holds the port, the two decisions, the roadmap entry, and this entry. It is pending owner merge.

The automated pass of gitar is complete on the effective head `be07a42`. Gitar reviewed two heads, `a5e7f7f` and `1d46c14`, and each review approved with the verdict "No issues found". Neither opened a review thread. The pass needed no `Gitar review` comment, because the automatic review started 5 seconds after the PR opened. Gitar replaced its dashboard comment between the two reviews, so the id changed from `5737986894` to `5738085529`. One CI analysis comment named the missing `docs/reviews/pr-81.md`. That comment has its answer on the PR: `review-gate` reads a record that no reviewer wrote yet, which is the state that D-251 states. Findings with merit: zero. No commit answers a finding.

The cross-provider review comes next. Codex is the eligible reviewer, because Claude Code wrote this PR (T-4, D-101).

### Traps and gotchas

- The scheduled night of 2026-09-18 failed with the annotation "The self-hosted runner lost communication with the server". That is not the "not acquired" case of D-358, and a re-run of the failed job is not the fix. A new dispatch on the same ref is. The runner was online and idle after the failure.
- A full night takes 78 to 91 minutes on this runner. The failed run died at 27 minutes, so a short run time is the first sign of a lost runner.
- `docs/design.md` marked PR-69 as planned after PR #80 merged. This PR corrects that mark to `✅ Done in PR #80.` The correction is a stale fact, and it is not a record of an earlier PR (D-375).
- The `review-response` skill read two sections of `pr-review` with a `sed` command over a line range. The split breaks such a command. That skill now names `references/commit-and-push.md`, and D-381 carries a partial revision mark.
- A reference file is under the byte ceiling of D-384 too. The `ste-writing` skill states every ceiling in one table.
- A later commit on this branch put its handoff entry at the end of the file, and CI turned red on all three legs. `HandoffRotateTests.RepositoryFilesHoldTheRule` states the cause: the file keeps the newest entry first (D-146). A repair commit moved that entry to the top, word for word, and ran `handoff-rotate`. Add a new entry at the top, and never at the end.
- The next ids are D-388, OQ-179, F-103, PR-71, and Session 190.

### Open questions that block progress

None blocks PR-70. OQ-177 and OQ-178 have their answers in D-386 and D-387. D-387 needs an owner action in the GitHub settings, and no code of this repository enforces it.

### Next concrete action

Codex reviews GitHub PR #81 at effective head `be07a42`, per `.claude/skills/pr-review/SKILL.md`, and writes `docs/reviews/pr-81.md`. That review reads the split of each skill against the file it replaced, and the front matter rule of the checker. After the merge, a new clean session starts PR-66, the ramps and the chamber tiers. D-387 needs an owner action in the GitHub settings, and it needs no PR.

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
- Remote head: `origin/feat/night-record-ref` reached `fb9177b` with the review record and this entry. Every check passed, including `review-gate`. A final metadata update follows this head.

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
