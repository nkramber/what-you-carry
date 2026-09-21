# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 192: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-16, the first enemy family, the AI, and the pathfinder. Branch `feat/pr-16-enemies-and-pathfinder`.

### What this session did, and why

- Repaired `main`. The entry of Session 191 sat at the end of `docs/session-handoff.md`, so `HandoffRotateTests` failed on every CI leg of any branch (D-146). The entry moved to the top, word for word, and `handoff-rotate` ran.
- Asked the owner OQ-9 before any enemy content, as the roadmap and D-124 require. D-395 to D-405 hold eleven answers over four batches: the eight families, the scavenger as the first one, its gear, its numbers, its cadence, the empty spawn chamber, the wake rule, the drawing, the `death` end state, the descender that fights, and the wedge fix inside this PR.
- Moved the move rule of D-165 and D-345 out of `Reachability` into `Core/Pathfinding/GridMoves.cs`, and wrote the A* search of D-76 over it. The generator and the AI now read one rule.
- Wrote the scavenger, the brain, the spawn pass, the full clearer, and the drawing of each enemy. The player of PR-15 now reads the shared `Swing` and `Stagger` of `Core/Combat`.
- Found and fixed F-104 and F-105, two walk defects that only the new movement reaches. The measured softlock rate of a bot fell from about 10 percent to zero over 2000 seeds of each policy.

### State of the build

- Base and merge base: `e1c20ea`, the merge of PR #82.
- `dotnet test` locally: 1172 passed, 0 failed, with the filter `Category!=Smoke`. The Smoke category passed 5 of 5 against the pinned binary. `det-lint` reports 0 findings in Core and 0 in Game. `ste-check` reports 0 findings over 34 files. `asset-qa` reports 0 findings.
- The simulation version is 13, and the bit-identity answer is `d701dca6d5cee4d8`.
- The bot sweep of seeds 1 to 100 passed for all three policies: 0 crashes and 0 softlocks. Over 2000 seeds, the descender reaches the bottom on 129 and the clearer on 421, and neither reads a crash or a softlock.
- Exit test 8 of PR-66, the night sweep on `main` at `e1c20ea`: pass. Run 35542505776 ended `success`. The random walker ran out its budget on all 5000 seeds, the greedy descender reached the bottom on all 5000, and neither read a softlock or a crash. The reachability sweep of 100000 seeds passed in the same job. So the ramps and the tiers hold over the night.
- That night ran the workflow of `main`, which plays two policies and writes no death count. The third policy and the death count of D-403 are on this branch, so the next night after the merge plays three.
- Remote head: `origin/feat/pr-16-enemies-and-pathfinder` at `1648ed4`, and GitHub PR #83 holds it.

### In flight

GitHub PR #83 holds the whole PR-16 scope, the eleven decisions, the two findings, and this entry. The checks run next, then the automated pass of gitar, then the cross-provider review. Codex is the eligible reviewer, because Claude Code wrote this PR (T-4, D-101).

### Traps and gotchas

- A walk to the cell of a body that moves never closes. Two walkers each aimed at the cell of the other and swung around each other 2 meters apart for a whole floor (F-104). A search now starts at the cell that the body walks into, and the last 4 meters of a chase are a straight walk at the body.
- A drift check that reads the X and the Z of a waypoint and not its row misses a body that fell two rows under its path. The body then jumped at a step of two blocks until the floor budget ran out (F-105).
- A roll away from every blade loses a duel: the roll covers 3 meters, and the enemy closes again during the 45-tick cooldown. `BotIntent.Roll` rolls through the enemy, which took the descender from 2 bottoms in 200 seeds to 43.
- A step up onto the side of a ramp is a legal move. A ban on it read as a fix for F-105 and was not one, and `TheSearchFollowsTheBodyRuleOnRamps` names the case.
- `TestWorld.NewLoop` now digs a floor with no enemy family, so a test of the camera, the body, or the replay reads its own rule. A test of the enemies builds its loop from `TestWorld.Content`.
- The smoke session dies at tick 273 of its 600-tick script on seed 1, and it ends clean with exit code 0 and the end kind in its line. The smoke gate covers fewer ticks than it did, and a stronger smoke script belongs to a later PR.
- The sweep enemy of the bit-identity content swings a second weapon of 1 damage, because a run that ends by death takes no more intents and the sweep record holds 600 of them.
- The owner chose the wedge fix inside PR-16 (D-405). This PR therefore carries the walk fixes beside the enemies, which widens it past one concern (G-10).
- The next ids are D-406, OQ-181, F-106, PR-71, and Session 193.

### Open questions that block progress

None. OQ-9 has its answers in D-395 and D-396.

### Next concrete action

Codex reviews GitHub PR #83 and writes `docs/reviews/pr-83.md`, per `.claude/skills/pr-review/SKILL.md`. The review reads the move rule of `GridMoves`, the A* search, the brain, the spawn pass, and the two walk fixes of F-104 and F-105. Exit test 8 of PR-66 needs the night on `main` at `e1c20ea` to report zero crashes and zero softlocks.

## Session 191: 2026-09-20, Codex

Author: Codex
Session: PR-82, the ramps, chamber tiers, and shaft routes. Branch `feat/pr-66-ramps-and-tiers`.

### What this session did, and why

- Reviewed PR #82 as the cross-provider reviewer.
- Inspected the complete diff, the generator contracts, the focused roadmap, the decisions, the questions, and all PR comments.
- Found no actionable issue at effective head `97efa5c`.

### State of the build

- Base and merge base: `27db615`.
- The focused procgen test process compiled, then stalled in the shared large sweep and was canceled.
- Revision-matched CI and the automated pass are recorded as passed at `97efa5c`. The review-gate and evaluate checks were expectedly neutral or failed before the review record existed.
- The review record is `docs/reviews/pr-82.md`.

### In flight

The review record and this handoff entry need one commit and a push. The owner play test remains exit test 10.

### Traps and gotchas

- The effective head excludes the later handoff and review-record commits under D-184.
- The local large sweep did not finish in this execution context. Do not report it as a local pass.

### Open questions that block progress

None. OQ-179 and OQ-180 are resolved by D-388 through D-394.

### Next concrete action

Run the review session end gate after the commit and push.

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
