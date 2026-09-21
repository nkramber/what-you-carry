# Session handoff archive

## Session 193: 2026-09-20, Codex

Author: Codex
Session: PR-83, cross-provider review. Branch `feat/pr-16-enemies-and-pathfinder`.

### What this session did, and why

- Reviewed PR #83 at effective head `8452dbd`.
- Inspected the pathfinder, movement rules, enemy and combat state, AI, procgen, simulation, bots, workflows, tests, and PR comments.
- Added `docs/reviews/pr-83.md` with no code finding and a blocked verdict because required CI remains pending.

### State of the build

- Focused tests passed, 6 of 6.
- The broad non-smoke test run compiled but stalled without a result.
- GitHub checks pass for completed jobs, but Linux and Windows CI remain pending. The review-gate check waits for this review record.
- Remote code head remains `8452dbd`. Metadata commit `9fb8fda` carries the review record and this entry.

### In flight

The review record and this handoff entry are pushed to the PR branch.

### Traps and gotchas

- The effective head excludes later handoff-only commits.
- A stalled test run is incomplete evidence, not a pass.
- The review stays blocked until required CI completes.

### Open questions that block progress

None.

### Next concrete action

Recheck the pending Linux and Windows CI jobs before the owner merge.

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
- Remote head: `origin/feat/pr-16-enemies-and-pathfinder` reached `e766b23`, and the status showed no `[ahead N]`. That commit and the one before it change `docs/session-handoff.md` alone, so the effective head is `8452dbd` (D-184). The commit `8452dbd` changes `docs/design.md` and `docs/roadmaps/`, which sit outside the metadata set, so it moves the effective head.
- Every check of PR #83 passes at the tip `cc70827`: Gitar, the three build legs, the three smoke legs, bit identity, compare, bots, asset-qa, det-lint, ste-check, doc-gate, and night-gate. The build legs took 11 minutes 39 seconds on Linux, 8 minutes 4 seconds on macOS, and 16 minutes 1 second on Windows. `evaluate` and `review-gate` fail, because the review record holds the verdict `Blocked` (D-179, D-181, D-185).

### In flight

GitHub PR #83 holds the whole PR-16 scope, the eleven decisions, the two findings, and this entry. It waits for the owner merge.

The review record `docs/reviews/pr-83.md` of Session 193 reports no code finding, and its verdict is `Blocked` for the effective head `8452dbd`, because required CI was pending and a local broad test run stalled. `docs/reviews/pr-83-response.md` answers both limits: the suite completed at 1172 passed and 0 failed at `0075b92`, and the three CI legs failed on the handoff order of D-146 alone. The entry of Session 193 sat under Session 183, and this session moved it to the top, word for word, and ran `handoff-rotate`. The verdict stays as the reviewing provider wrote it, because the author of a PR never sets it (T-4, D-381).

The automated pass of gitar is complete on the effective head `8452dbd`. The `Code Review` block reads "Approved" with no finding. The `CI failed` block of the same comment names the missing `docs/reviews/pr-83.md`, and a PR reply answers it with D-251: the review gate stays red until the reviewing provider writes that file, and the author of a PR does not write it. Findings with merit: none.

The cross-provider review comes next. Codex is the eligible reviewer, because Claude Code wrote this PR (T-4, D-101).

### Traps and gotchas

- A walk to the cell of a body that moves never closes. Two walkers each aimed at the cell of the other and swung around each other 2 meters apart for a whole floor (F-104). A search now starts at the cell that the body walks into, and the last 4 meters of a chase are a straight walk at the body.
- A drift check that reads the X and the Z of a waypoint and not its row misses a body that fell two rows under its path. The body then jumped at a step of two blocks until the floor budget ran out (F-105).
- A roll away from every blade loses a duel: the roll covers 3 meters, and the enemy closes again during the 45-tick cooldown. `BotIntent.Roll` rolls through the enemy, which took the descender from 2 bottoms in 200 seeds to 43.
- A step up onto the side of a ramp is a legal move. A ban on it read as a fix for F-105 and was not one, and `TheSearchFollowsTheBodyRuleOnRamps` names the case.
- `TestWorld.NewLoop` now digs a floor with no enemy family, so a test of the camera, the body, or the replay reads its own rule. A test of the enemies builds its loop from `TestWorld.Content`.
- The smoke session dies at tick 273 of its 600-tick script on seed 1, and it ends clean with exit code 0 and the end kind in its line. The smoke gate covers fewer ticks than it did, and a stronger smoke script belongs to a later PR.
- The sweep enemy of the bit-identity content swings a second weapon of 1 damage, because a run that ends by death takes no more intents and the sweep record holds 600 of them.
- The owner chose the wedge fix inside PR-16 (D-405). This PR therefore carries the walk fixes beside the enemies, which widens it past one concern (G-10).
- A handoff entry added at the end of the file reds the three build legs, and four sessions did it (F-106). The owner asked for the tool fix inside this PR, so `handoff-rotate` now puts such an entry back in its place and names it (D-406). `RepositoryFilesHoldTheRule` asserts that the committed file needs no sort, so the check still names a file that breaks D-146.
- That tool change moves the effective head off `8452dbd`, so the automated pass and the cross-provider review both read the new head.
- The next ids are D-407, OQ-181, F-107, PR-71, and Session 195.

### Open questions that block progress

None. OQ-9 has its answers in D-395 and D-396, and the handoff loop has its answer in D-406.

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

## Session 172: 2026-09-15, Claude Code

Author: Claude Code
Session: record the merge of PR-65 as PR #73, the night failure of F-101, and the owner answers D-370 and D-371, in the same invocation as Session 170 (D-297). Branch `docs/pr-65-merge-record`.

### What this session did, and why

- Session 171 approved `720c7a9` in `docs/reviews/pr-73.md` with no finding. The owner merged PR #73 as `4bc8cd4` at 05:45 UTC on 2026-09-15, and the tree of `4bc8cd4` equals the tip `5062ea2`. The three commits after `720c7a9` change only metadata paths (D-184).
- `main` now holds the ramp meshes of D-368 and the contact sheet of D-369. `docs/design.md` marks PR-65 done, and sequence item 11 names the merge. The Phase 2 roadmap gains the status line of PR-65 and the mark in sequence item 16.
- The night of 2026-09-15 at `4bc8cd4` failed. `EveryChamberReachable` and `DetailKeepsEveryChamberReachable` report seed 79146, floor 7: the shaft at column (25, 48) lands on an unreachable floor at row 8. The bot sweeps of that night passed, and the dig reported no error.
- A scratch test outside the repository dug that seed and floor at four revisions. At `4bc8cd4`, at `e1076ca`, and at `d65823c` the grid hash is `ff14f981092fdf5a`, and the landing is unreachable. At `d2ef347`, before PR-63, the grid is 72 by 16 by 72 with no shaft. The dig sizes of PR-63 make this floor, and PR-64, PR-65, and PR-67 did not.
- The last green night, at `f487401` on 2026-09-14, ran before PR-63 merged, so the night of 2026-09-15 is the first night on the wide sizes. The PR sweep of 5000 seeds never reads seed 79146.
- The owner answered two questions. D-370: PR-68, a fix PR of its own, comes before PR-66, and seed 79146 becomes a regression test. D-371: this merge record merges at once, although the `night-gate` job is red.
- F-101 records the night failure, and OQ-174 records the question. The Phase 2 roadmap gains the PR-68 entry, the F-101 row, and the sequence item 17, and `docs/design.md` gains the PR-68 entry.
- Session 162 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `4bc8cd4`, the squash merge of PR #73. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-65-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `4bc8cd4`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, and STE check passed, the last at 05:57 UTC. The night of 2026-09-15 failed at 14:40 UTC (F-101).
- `dotnet test`: 1115 tests, 0 failures, with the five Smoke tests on the local Godot build. `ste-check`: 0 findings in 16 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `4bc8cd4`.
- The record on the branch `night-results` holds `4bc8cd4` with the status failure, so the `night-gate` job fails on every PR until a night passes (D-115, D-177).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). The owner merges it with the red `night-gate` (D-371). PR-68 follows in a fresh session (D-121, D-370), and PR-66 comes after it.

### Traps and gotchas

- The GitHub PR #73 is PR-65. The roadmap id PR-68 is the shaft landing fix, and PR-66 digs the ramps and the tiers.
- The merge marks use the UTC date of the merge, 2026-09-15. D-370, D-371, and this entry use the local date, also 2026-09-15.
- The `night-gate` job fails on every PR until a night passes. `night.yml` takes a manual event, so PR-68 can run one before it merges.
- The reachability failure is no regression of PR-64, PR-65, or PR-67. The grid hash of seed 79146, floor 7 is the same at three revisions, and the floor first appears with the dig sizes of PR-63.
- The PR sweep of 5000 seeds never reads seed 79146, so a PR run passes while the night fails.
- The simulation version stays 10, and the bit-identity known answer stays `24c37100cd99edf4`.
- The next ids are D-372, OQ-175, F-102, PR-69, and Session 173.

### Open questions that block progress

None blocks this PR. D-370 resolves OQ-174. The PR-68 session diagnoses the shaft landing of seed 79146 and runs a night before the merge. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label and the red `night-gate` (D-371). A fresh session then opens PR-68: it digs seed 79146, floor 7, finds why the shaft lands on an unreachable floor, corrects the dig, adds the regression test, and runs a night before the merge (D-370).

## Session 171: 2026-09-15, Codex

Author: Codex
Session: review PR #73, the ramp meshes in Game, at effective head `720c7a9`. Branch `feat/pr-65-ramp-meshes`.

### What this session did, and why

- Reviewed the complete code and test diff for the ramp mesh, face coverage, ambient occlusion, mesh triangle, greedy sweep, and contact-sheet changes.
- Verified the provider gate. Session 170 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Checked the ramp slope planes, side and end coverage, chunk borders, triangle winding, texture density, occlusion, mesh budget path, contact-sheet layout, and no-ramp mesh preservation against D-368, D-369, and the PR-65 exit tests.
- The focused mesher and contact-sheet suite passed 86 tests. Found no in-scope defect.
- Wrote `docs/reviews/pr-73.md` with the verdict `Ready for owner merge` for `720c7a9`.
- Read the existing automated-review comment. It approved the head and raised no issue.

### State of the build

- `main` is at `a4bf6d6`. The effective head of PR #73 is `720c7a9`. The later `f4747ac` commit changes only `docs/session-handoff.md` and `docs/session-handoff-archive.md` under D-184.
- Remote head: `origin/feat/pr-65-ramp-meshes` is `5e62041` after the review push, verified with `git fetch`, clean status, and `gh pr view`.
- The focused suite passed 86 tests. Local full build and gate commands produced no completion result because the .NET process hung without output. Session 170 reports the full gates and revision-matched CI as passed on `720c7a9`.
- GitHub checks after the review push are in progress, including `evaluate`; no completed post-review verdict is available yet.

### In flight

PR #73 is ready for owner merge after the review commit reaches the PR and the review-gate refreshes. PR-66 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #73 is roadmap PR-65. The roadmap id PR-66 digs the ramps and tiers.
- The effective head is `720c7a9`, not the metadata tip `f4747ac` (D-184).
- A mesh with the side of a ramp holds triangle faces. Read `TriangleCount` for triangles, because `QuadCount` counts quads alone.
- The local .NET hang is an execution-context limitation, not a passed check. Use the revision-matched CI evidence from Session 170.
- The next ids are D-370, OQ-174, F-101, PR-68, and Session 172.

### Open questions that block progress

None blocks PR #73. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the post-review checks, including `evaluate` and `review-gate`, to complete. The owner then merges PR #73. A fresh session opens PR-66 and asks the owner for the shape of a tier first (D-350).


## Session 170: 2026-09-15, Claude Code

Author: Claude Code
Session: open PR-65, the ramp meshes in Game, as PR #73, with the owner answers D-368 and D-369. Branch `feat/pr-65-ramp-meshes`.

### What this session did, and why

- The owner merged PR #72, the merge record of PR-64, as `a4bf6d6` at 03:30 UTC on 2026-09-15, and asked for the next item. The branch came from `origin/main` at `a4bf6d6`.
- Before the code, the owner answered the tile of a ramp face (D-367). D-368: every face of a ramp takes the raw stone tile, because the floor of each tunnel and chamber is raw stone in all three bands. The recommendation stood.
- `FaceShape` reads the part of each side of a cell that its solid fills, in twelfths of a block. `GreedyMesher.FaceVisible` hides a face only when the side of the neighbor covers it, so a wall beside a ramp shows over the slope.
- `RampFaces` gives the slopes, merged in each row by plane and occlusion through `GreedySweep`, and each end, side, and bottom of a ramp cell that shows. The block faces now read their masks through `GreedySweep` too. The side of a run comes to a point at the low end, so `MeshData` gains `AddTriangle` and `TriangleCount`, and `QuadCount` counts the calls of `AddQuad`.
- `AmbientOcclusion.Occludes` reads the upper half of a ramp run as a block and the lower half as air. `CornerLevel` moved from the mesher to `AmbientOcclusion`, so the slopes and the block faces share it.
- The contact sheet adds a ramp of each slope in the whole render, with the camera at the foot. The owner approved the sheet as drawn, and D-369 closes exit test 4.
- A scratch test outside the repository hashed every chunk mesh of floors 1, 6, and 11 of seeds 1 to 8. `main` at `a4bf6d6` and this head both give `364F7B57EACE4F4F2D3034FD1C5A2A85339839351A0341BF8C838EFA157BCD89` over 210966 indices, so a grid with no ramp keeps every bit of its mesh.
- The first `det-lint` run found the plain string `"length"` in the error context of `GreedySweep`, and a named constant replaced it before the commit.
- The automated pass of gitar approved `720c7a9` at 04:55 UTC with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 160 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `a4bf6d6`. The effective head of PR #73 is `720c7a9`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-65-ramp-meshes` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 1115 tests, 0 failures, with the five Smoke tests on the local Godot build. After the last edits of the decisions and the roadmap, a run without the Smoke category passed 1110 tests. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 36 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `24c37100cd99edf4`, because Core does not change.
- CI on `720c7a9`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last at 05:07 UTC. No macOS leg ended "not acquired". `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `720c7a9` completed before the push of this entry, so that push cancels nothing (D-356).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the ramp code and the dig restart, and it can start hours late (F-95).

### In flight

PR #73: the Codex review per the `pr-review` skill at the effective head `720c7a9` (T-4). The owner then merges, and a docs PR records the merge (D-297). PR-66 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #73 is PR-65. The roadmap id PR-66 digs the ramps and the tiers.
- D-368 and D-369 carry 2026-09-14, the local date of the owner answers. This entry carries 2026-09-15, the local date when it was written.
- The effective head is `720c7a9`, and not the metadata commit of this entry (D-184).
- A mesh with the side of a ramp holds triangle faces. Read `TriangleCount` for the triangles, because `QuadCount` counts quads alone, and a test that reads faces by a stride of four vertices breaks on such a mesh.
- A slope reads the occlusion of the cell over the ramp. An end or a side of a ramp cell reads the occlusion of the whole side of its cell, also where the face is lower than the cell.
- The contact sheet is 2400 by 2000 pixels. The area to the right of the block cells is empty and renders black.
- The Godot build check writes a `.uid` file for each new script in Game. Commit the file with the script.
- `det-lint` reads a plain string literal in Game as a string that a player sees, also in an error context. Put a context key in a named constant.
- The simulation version stays 10, and the bit-identity known answer stays `24c37100cd99edf4`.
- The next ids are D-370, OQ-174, F-101, PR-68, and Session 171.

### Open questions that block progress

None blocks PR #73. The PR-66 session asks the owner for the shape of a tier before the code (D-350), and the gate of PR-66 needs the owner to confirm the ramps and the tiers in play. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #73 per the `pr-review` skill at the effective head `720c7a9` and writes `docs/reviews/pr-73.md`. The owner then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-66, and it asks the owner for the shape of a tier first (D-350).

## Session 169: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-64 as PR #71, in the same invocation as Session 167 (D-297). Branch `docs/pr-64-merge-record`.

### What this session did, and why

- Session 168 approved `b5bf2de` in `docs/reviews/pr-71.md` with no finding.
- The owner merged PR #71 as `43f14eb` at 03:05 UTC on 2026-09-15, and the tree of `43f14eb` equals the review tip `78470d0`. The two commits after `b5bf2de`, `d489060` and `78470d0`, change only metadata paths (D-184).
- `main` now holds the ramp cells of D-367 and the motion on a ramp of D-362 to D-366. `docs/design.md` marks PR-64 done, and sequence item 11 names the merge. The Phase 2 roadmap gains the status line of PR-64 and the mark in sequence item 15. F-97 stays open for PR-65 and PR-66.
- The file held twelve entries with this one, so Sessions 159 and 158 moved to the archive.

### State of the build

- `main` is at `43f14eb`, the squash merge of PR #71. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-64-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `43f14eb`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 03:19 UTC. No macOS leg ended "not acquired".
- `ste-check`: 0 findings in 16 files. `dotnet test`: 1049 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `43f14eb`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the ramp code and the dig restart, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-65, the ramp meshes in Game, follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #71 is PR-64. The roadmap id PR-65 is the ramp meshes in Game, and PR-66 digs the ramps and the tiers.
- The merge marks use the UTC date of the merge, 2026-09-15. This entry uses the local date, 2026-09-14.
- The Codex review of Session 168 ran the focused ramp suite of 173 tests, and its local full suite and build gave no completion result there. Session 167 ran the full gates on the effective head.
- The simulation version is 10, so a run record of version 9 fails with the notice of D-151 (D-260).
- The bit-identity known answer is `24c37100cd99edf4`. A version rise alone moves it, because the replay header holds the version.
- The mesher of PR-13 draws a ramp as a cube with the tile of its id. PR-65 draws the slope and its two sides.
- The next ids are D-368, OQ-174, F-101, PR-68, and Session 170.

### Open questions that block progress

None blocks this PR. The PR-65 session asks the owner for the tile of a ramp face before the code (D-367), and exit test 4 of PR-65 needs the owner approval of a contact sheet with the ramps. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-65, and it asks the owner for the tile of a ramp face first (D-367).

## Session 168: 2026-09-14, Codex

Author: Codex
Session: review PR-64 as PR #71 at effective head `b5bf2de`. Branch `feat/pr-64-ramp-cells`.

### What this session did, and why

- Reviewed the complete code and test diff for the ramp cells in Core.
- Verified the provider gate. Session 167 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Checked the ramp ids, slope math, collision sweep, player motion, ray march, reachability search, simulation version, and bit-identity course against D-362 to D-367 and the PR-64 roadmap exit tests.
- Found no in-scope defect. Wrote `docs/reviews/pr-71.md` with the verdict `Ready for owner merge` for `b5bf2de`.
- Read the existing Gitar comment. It approved the head and raised no issue.

### State of the build

- `main` and the merge base are `e1076ca`. The effective head is `b5bf2de`. The later `d489060` commit changes only `docs/session-handoff.md` under D-184.
- The focused ramp suite passed 173 tests with 0 failures. A local full-suite and build attempt produced no completion result after more than one minute. Session 167 reports the full local gates as passed.
- Revision-matched CI on `b5bf2de` passed CI, smoke, bit identity on all three platforms, compare, bots, det-lint, asset QA, night gate, and STE check. Gitar approved with no issue comment.
- `evaluate` failed and `review-gate` skipped before this review record existed, as D-251 predicts. They must refresh after the review commit reaches the PR.

### In flight

PR #71 is ready for owner merge after the fresh review-gate check passes. The owner then merges, and a docs PR records the merge (D-297). PR-65 follows in a fresh session (D-121).

### Traps and gotchas

- The effective head is `b5bf2de`, not this metadata commit (D-184).
- The PR is GitHub #71 and roadmap PR-64. PR-65 is the ramp mesh work, and PR-66 is the ramp generator work.
- The local full-suite and build attempts produced no completion result. Do not report those local attempts as passed.
- The next ids are D-368, OQ-174, F-101, PR-68, and Session 169.

### Open questions that block progress

None blocks PR #71. The PR-65 session asks the owner for the tile of a ramp face before the code (D-367). Exit test 4 of PR-65 needs owner approval of a contact sheet with ramps. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the fresh review-gate check. The owner merges PR #71 after the check passes. A fresh session then opens PR-65 and asks the owner for the tile of a ramp face first (D-367).

## Session 167: 2026-09-14, Claude Code

Author: Claude Code
Session: open PR-64, the ramp cells in Core, as PR #71, with the owner answers D-362 to D-367. Branch `feat/pr-64-ramp-cells`.

### What this session did, and why

- The owner merged PR #70, the merge record of PR-67, as `e1076ca` at 00:19 UTC on 2026-09-15, and asked for the next item. The branch came from `origin/main` at `e1076ca`.
- Before the code, the owner answered six questions in two batches (D-345). D-362: the speed along the slope is the flat speed, and the recommendation was the flat horizontal speed. D-363: a walk and a sprint stay on the slope on the way down, and a roll leaves it, in the words of the owner. D-364: no slide. D-365: a jump from a ramp as from flat ground, which revises in part D-235, the ground probe only. D-366: a roll starts on a ramp and climbs it. D-367: the ramp ids 8 to 43 in the block byte.
- `Ramp` reads a ramp id. `VoxelGrid` takes the ramp ids, and `TryGetRamp` and `TryTopUnder` read them. The solid rule reads a ramp as solid.
- `SweptAabb` stands a box on the highest point of the slope under its footprint. It lifts a box onto a slope that rises under the end of a move along X or Z, by at most the rise of a slope of 1:2 over the move and with room above, and never onto a block (D-165). `PlayerBody` takes the slope factor of D-362, and `Move` takes `followSlope` for the walk down of D-363.
- `GridRay` meets the slope inside a ramp cell by the sign of the height over the slope at the entry and the exit of the cell. `Reachability.RampWalk` joins cells whose slopes meet on the shared face, a chain of ramps included, and `Landing` limits the drop and the step from a ramp.
- The simulation version is 10. The bit-identity sweep gains a ramp course along each rise with each run, and the known answer moves from `efcce6816cec980e` to `24c37100cd99edf4`. Before the ramp run and the version rise, the new code passed the suite with `efcce6816cec980e`, so no grid without a ramp moved.
- Tests: `RampTests`, `RampMotionTests`, `RampRayTests`, and `RampSearchTests`, on the course of `RampCourse`. The first form of `CameraNeverEntersARamp` read a boom hit on the side of a ramp as a hit on the slope and failed. The test now asserts the camera contract, and it counts the hits on a slope.
- The automated pass of gitar approved `b5bf2de` at 01:45 UTC with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 157 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `e1076ca`. The effective head of PR #71 is `b5bf2de`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-64-ramp-cells` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 1049 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 66 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `24c37100cd99edf4`.
- The PR bot sweep ran locally: the random walker and the greedy descender over seeds 1 to 100, 0 softlocks and 0 crashes, and the descender reached the bottom on every seed.
- CI on `b5bf2de`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last at 01:50 UTC. No macOS leg ended "not acquired". `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `b5bf2de` completed before the push of this entry, so that push cancels nothing (D-356).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC runs on `main` with the dig restart, and it can start hours late (F-95).

### In flight

PR #71: the Codex review per the `pr-review` skill at the effective head `b5bf2de` (T-4). The owner then merges, and a docs PR records the merge (D-297). PR-65 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #71 is PR-64. The roadmap id PR-65 is the ramp meshes in Game, and PR-66 digs the ramps.
- The effective head is `b5bf2de`, and not the metadata commit of this entry (D-184).
- `VoxelGrid.IsSolid` reads a ramp as solid. The mesher of PR-13 draws a ramp as a cube with the tile of its id until PR-65, and `DigCanvas.IsAir` reads a ramp as rock until PR-66.
- `GreedyDescender` jumps at each rise of one row on its path, and the walk onto the low end of a ramp is such a rise. The jump lands on the slope. PR-66 runs the bots on ramps.
- The lift onto a slope reads the footprint at the end of the whole move. A move that a block cuts short can leave the body up to half the move over the slope, and the next tick lands it.
- A stagger passes `followSlope` true and a roll passes false (D-363, D-364).
- D-110: the private helpers of `SweptAabb` call only public methods of `VoxelGrid` and `Ramp`. A helper that calls another helper breaks the rule.
- zsh reserves the variable name `status`, so a command chain that sets it stops with "read-only variable".
- The next ids are D-368, OQ-174, F-101, PR-68, and Session 168.

### Open questions that block progress

None blocks PR #71. The PR-65 session asks the owner for the tile of a ramp face before the code, because a ramp has no block kind of its own (D-367). Exit test 4 of PR-65 needs the owner approval of a contact sheet with the ramps. The PR-66 session asks the owner for the shape of a tier before the code (D-350). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #71 per the `pr-review` skill at the effective head `b5bf2de` and writes `docs/reviews/pr-71.md`. The owner then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-65, and it asks the owner for the tile of a ramp face first (D-367).

## Session 166: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-67 as PR #69, in the same invocation as Session 164 (D-297). Branch `docs/pr-67-dig-restart-merge-record`.

### What this session did, and why

- Session 165 approved `80ee5d9` in `docs/reviews/pr-69.md` with no finding. `review-gate` and `evaluate` passed on the tip `28a6705` at 22:55 UTC.
- The owner merged PR #69 as `f2a04e6` at 23:25 UTC on 2026-09-14, and the tree of `f2a04e6` equals the tip `28a6705`. The three commits after `80ee5d9`, `0e69c58`, `bc991cc`, and `28a6705`, change only metadata paths (D-184).
- `main` now holds the job budget of D-359, the four digs of D-360, and the new chamber draw of D-361. `docs/design.md` marks PR-67 and F-98 done, and sequence item 11 names the merge. The Phase 2 roadmap gains the status line of PR-67 and the mark in sequence item 14.
- Session 165 left ten entries in the file, so Session 156 moved to the archive with this one.

### State of the build

- `main` is at `f2a04e6`, the squash merge of PR #69. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-67-dig-restart-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `f2a04e6`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 23:39 UTC. No macOS leg ended "not acquired".
- `dotnet build`: 0 warnings, 0 errors on `f2a04e6`. `dotnet test`: 877 tests, 0 failures, with the five Smoke tests on the local Godot build. `ste-check`: 0 findings in 16 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `f2a04e6`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the dig restart, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-64, the ramp cells in Core, follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #69 is PR-67. The GitHub PR #67 was the concurrency PR, and its merge record used the branch `docs/pr-67-merge-record`.
- The merge marks use the UTC date of the merge, 2026-09-14. This entry uses the local date, also 2026-09-14.
- The Codex review of Session 165 ran in this checkout, and its local build, full test, and STE check gave no completion result there. This session rebuilt `f2a04e6` before its checks.
- The simulation version is 9, so a run record of version 8 fails with the notice of D-151 (D-260).
- The bit-identity known answer is `efcce6816cec980e`. A version rise alone moves it, because the replay header holds the version.
- The next ids are D-362, OQ-174, F-101, PR-68, and Session 167.

### Open questions that block progress

None blocks this PR. The PR-64 session asks the owner for the motion on a ramp before the code: the speed on a slope, the jump, the roll, and the stagger. It also decides the encoding of a ramp cell (D-345). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-64, and it asks the owner for the motion on a ramp first (D-345).

## Session 165: 2026-09-14, Codex

Author: Codex
Session: review PR #69, the dig restart, at effective head `80ee5d9`. Branch `feat/pr-67-dig-restart`.

### What this session did, and why

- Read the PR description, complete diff, affected Core callers, tests, roadmap, design, decisions, questions, prior records, and every PR comment.
- Checked the provider gate. Session 164 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Verified the restart state machine. Each failed dig uses the next draws of the same Procgen stream and a new empty grid, and the first complete plan supplies the shafts and detail.
- The focused restart suite passed 4 tests. The tests cover a complete first dig, budget exhaustion, retry output, and the four-dig contextual error.
- Found no in-scope defect. Wrote `docs/reviews/pr-69.md` with the verdict `Ready for owner merge` for `80ee5d9`.

### State of the build

- `main` and the merge base are `d65823c`. The effective head is `80ee5d9`. The later tips `0e69c58` and `bc991cc` change only metadata paths under D-184.
- Remote head: `origin/feat/pr-67-dig-restart` at `bc991cc`, verified with `gh pr view`. The branch has no ahead count.
- The focused restart tests passed 4 tests. A local full-suite, build, and STE-check attempt produced no completion result. The review records those attempts as unverified.
- Revision-matched CI on `80ee5d9` passed CI, smoke, bit identity on all three platforms, compare, bots, det-lint, asset QA, night gate, and STE check. Gitar approved with no issue comment.
- `evaluate` failed and `review-gate` was neutral before this review record existed, as D-251 predicts. They must refresh after the review commit reaches the PR.

### In flight

PR #69 is ready for owner merge after the fresh required checks pass. The owner then merges, and a docs PR records the merge (D-297). PR-64 follows in a fresh session.

### Traps and gotchas

- The GitHub PR is #69, but the roadmap item is PR-67.
- The effective head is `80ee5d9`, not the metadata tips `0e69c58` and `bc991cc` (D-184).
- The first `git fetch origin` after the review failed because the checkout could not open `.git/FETCH_HEAD`. The initial fetch and PR read succeeded before the failure.
- The local test host first failed with a socket permission error. The elevated focused run passed. The elevated full suite produced no completion result.
- The next ids are D-362, OQ-174, F-101, PR-68, and Session 166.

### Open questions that block progress

None blocks PR #69. PR-64 asks the owner for the motion on a ramp before the code. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the fresh required checks. Verify that `review-gate` passes for effective head `80ee5d9`.

## Session 164: 2026-09-14, Claude Code

Author: Claude Code
Session: open PR-67, the dig restart, as PR #69, with the owner answers D-359 to D-361. Branch `feat/pr-67-dig-restart`.

### What this session did, and why

- The owner merged PR #68, the merge record of the concurrency PR, as `d65823c` at 21:27 UTC on 2026-09-14, and asked for the next item. The branch came from `origin/main` at `d65823c`.
- Before the questions, a scratch program outside the repository dug the 675000 floors of F-98 with a copy of the Core of `main`. The same 7 floors ran the 10000 jobs. The median need is 1 job, one floor in 5500 needs more than 1000, and the largest need that passed is 9952. A replay of each floor over 300 jobs, with a restart at budgets from 500 to 10000, dug every floor on its second dig.
- The owner chose three answers on the recommendation. D-359 sets a job budget of 1000 and supersedes the cap of D-279. D-360 gives a floor 4 digs before the error. D-361 draws the chamber kinds again on each restart.
- `DigPlan.TryDigUntilComplete` replaces `DigUntilComplete` and `MaxJobs`. `FloorGenerator.DigChambers` digs again on an empty grid from the next draws, up to `FloorGenerator.MaxDigs`, and the error names the digs, the budget, and the chambers. The simulation version is 9 (D-260).
- The bit-identity known answer moved from `b00814dbf25e61e8` to `efcce6816cec980e`. The replay header holds the version, and this head with the version set back to 8 prints `b00814dbf25e61e8`, so the version alone moved it.
- `DigRestartTests` replaces `DigPlanJobCapTests`. On a scratch worktree of `main`, the generator fails on all 7 floors of F-98 with "The dig plan ran 10000 jobs" (T-3).
- D-279 carries `Superseded by D-359`, so each other line that cites D-279 names D-359: F-92, F-98, the PR-63 entry, OQ-147, OQ-172, D-353, and the two roadmaps (D-178).
- The automated pass of gitar approved `80ee5d9` at 22:17 UTC with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 154 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `d65823c`. The effective head of PR #69 is `80ee5d9`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-67-dig-restart` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 877 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed, and `bit-identity` prints `efcce6816cec980e`.
- The scratch sweep of this head over the 675000 floors of F-98: 0 generator errors, 674877 floors on the first dig and 123 on the second, and the largest total job count is 1019, on seed 517241 floor 12.
- The PR bot sweep ran locally: the random walker and the greedy descender over seeds 1 to 100, 0 softlocks and 0 crashes, and the descender reached the bottom on every seed.
- CI on `80ee5d9`: CI, smoke, and bit identity passed on the three platforms, with the compare job. Asset-qa, bots, det-lint, the night gate, STE check, and gitar passed, the last at 22:28 UTC. No macOS leg ended "not acquired". `evaluate` fails and `review-gate` is grey, because no review record exists yet (D-251). Every run on `80ee5d9` completed before the push of this entry, so that push cancels nothing (D-356).
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC runs on `main` with the cap of D-279, which D-359 supersedes after the merge, and it can start hours late (F-95).

### In flight

PR #69: the Codex review per the `pr-review` skill at the effective head `80ee5d9` (T-4). The owner then merges, and a docs PR records the merge (D-297). PR-64 follows in a fresh session (D-121).

### Traps and gotchas

- The GitHub PR #69 is PR-67. The GitHub PR #67 was the concurrency PR.
- The effective head is `80ee5d9`, and not the metadata commit of this entry (D-184).
- The bit-identity known answer moves with the simulation version alone, because the replay header holds the version. A first reading in this session said that the answer stays, and the tests refuted it.
- A floor that needed 1001 to 10000 jobs digs another floor now, so a run record of version 8 fails with the notice of D-151.
- `EveryTailFloorDigs` and `TheBudgetEndsTheFirstDigOfAHeavyFloor` pin dig 2 on ten floors. A change to the dig can move a floor to another dig, and the assert names the seed and the floor.
- The scratch programs are not in the repository. D-359 to D-361 and the PR description hold the measurement.
- The next ids are D-362, OQ-174, F-101, PR-68, and Session 165.

### Open questions that block progress

None blocks PR #69. The PR-64 session asks the owner for the motion on a ramp before the code: the speed on a slope, the jump, the roll, and the stagger. It also decides the encoding of a ramp cell (D-345). PR-64 has no measurement or hardware exit test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #69 per the `pr-review` skill at the effective head `80ee5d9` and writes `docs/reviews/pr-69.md`. The owner then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-64, and it asks the owner for the motion on a ramp first (D-345).

## Session 163: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of the concurrency PR as PR #67, in the same invocation as Session 161 (D-297). Branch `docs/pr-67-merge-record`.

### What this session did, and why

- Session 162 approved `d774ab9` in `docs/reviews/pr-67.md` with no finding. The owner merged PR #67 as `10ba70c` at 20:58 UTC on 2026-09-14, and the tree of `10ba70c` equals the review tip `d8bc858`.
- `main` now holds the concurrency groups of D-356, the evidence rule of D-357, and the re-run rule of D-358. `docs/design.md` marks F-99 and F-100 done, and F-99 keeps the note that the groups do not stop a lost leg.
- The file held twelve entries with this one, so Sessions 153 and 152 moved to the archive.

### State of the build

- `main` is at `10ba70c`, the squash merge of PR #67. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-67-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `10ba70c`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 21:11 UTC. No macOS leg ended "not acquired".
- `ste-check`: 0 findings in 16 files. `dotnet test`: 875 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `10ba70c`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the new sizes, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-67, the dig restart, follows in a fresh session (D-121, D-353).

### Traps and gotchas

- The GitHub PR #67 is the concurrency PR. The roadmap id PR-67 is the dig restart.
- The merge marks use the UTC date of the merge, 2026-09-14. This entry uses the local date, also 2026-09-14.
- The review gate runs its workflow from `main` (D-197), so its concurrency group acts from this PR on. It runs on a push and on a label event, and a newer one cancels an older review gate run of that PR that is still in progress (D-356).
- A self-hosted leg can end "not acquired" (F-100). Re-run the failed jobs of that run (D-358). The runner log is under `/Volumes/SSD-1TB/actions-runner/_diag` on the Mac mini.
- The next ids are D-359, OQ-174, F-101, PR-68, and Session 164.

### Open questions that block progress

None blocks this PR. PR-67 asks the owner for the job budget and the count of digs before the code (D-353). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-67, and it asks the owner for the job budget and the count of digs first (D-353).

## Session 162: 2026-09-14, Codex

Author: Codex
Session: review PR #67, the workflow concurrency groups, at effective head `d774ab9`. Branch `chore/pr-run-concurrency`.

### What this session did, and why

- Checked the provider gate. Session 161 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, all ten workflows, affected tests and documents, roadmap, design, decisions, questions, and every PR comment.
- Verified the nine pull request workflows use the D-356 group, and `night.yml` has no group.
- Verified the group uses the PR number for pull request events and the commit for push events. The cancellation expression acts only on pull request events.
- Found no in-scope defect. The focused regression passes on the head and fails on `main` for `asset-qa.yml`, so it rejects the old workflow shape.
- Added `docs/reviews/pr-67.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `5b85b70`. The effective head is `d774ab9`. The later tip commit `65940c7` changes only metadata paths under D-184.
- `dotnet build` passed with 0 warnings and 0 errors. The focused workflow regression passed 2 tests. STE check, det-lint, asset QA, and bit identity passed.
- The local full test command did not emit a completion result because the test host did not complete in this execution context. Revision-matched CI on the effective code head passed CI, smoke, bit identity on all three platforms, asset QA, bots, det-lint, the night gate, and STE check.
- Gitar approved the head with no issue comment. The review gate was neutral or skipped before the review record existed, as D-251 expects.
- Remote head: `origin/chore/pr-run-concurrency`, verified with `gh pr view` after the review commit.

### In flight

PR #67 is ready for owner merge. The next session opens the dig restart item in the Phase 2 roadmap and asks for the job budget and the count of digs before code, as D-353 requires.

### Traps and gotchas

- The GitHub PR #67 is the concurrency PR. The roadmap item PR-67 is the dig restart.
- The effective head is `d774ab9`, not the later metadata tip `65940c7`.
- A local full test without a completion result is an execution-context limit. Do not report it as a passed local gate.
- The review gate becomes green after this review record reaches the PR head.

### Open questions that block progress

None blocks PR #67. OQ-173 is resolved by D-358. The dig restart still needs the owner choices recorded by D-353.

### Next concrete action

The owner merges PR #67. A fresh session starts the roadmap dig restart item and asks for its job budget and dig count before code.

## Session 161: 2026-09-14, Claude Code

Author: Claude Code
Session: open the concurrency groups of D-356 and the evidence rule of D-357 as PR #67, in the same invocation as Sessions 158 and 160 (D-355), and record the owner answer on a lost self-hosted leg, D-358. Branch `chore/pr-run-concurrency`.

### What this session did, and why

- The owner merged PR #66, the PR-63 merge record, as `5b85b70` at 19:21 UTC on 2026-09-14, and asked for the concurrency PR (D-355).
- The branch rebased onto `origin/main` with no conflict. The code commit `704ce9f` became `5381e3f`, and `git range-diff` shows the same change.
- A scratch worktree of `main` at `5b85b70` showed that the new workflow test failed on the old workflows (T-3).
- PR #67 opened at 19:33 UTC. The automated pass approved it with no comment. A macOS self-hosted leg ended "not acquired"; a re-run of the failed jobs passed, including compare.
- D-358 records that the author re-runs failed jobs after a lost self-hosted leg, and the re-run counts as CI for that head. D-358, F-100, OQ-173, and the related records landed in `d774ab9`.
- The manual automated pass approved `d774ab9` with no comment (D-250, D-303).

### State of the build

- `main` was at `5b85b70`. The effective head of PR #67 was `d774ab9`, with this entry in a metadata commit above it (D-184).
- CI, smoke, bit identity, asset QA, bots, determinism lint, the night gate, STE check, and the automated pass passed on `d774ab9`.

### In flight

PR #67 awaited the Codex review at effective head `d774ab9`.

### Traps and gotchas

- GitHub PR #67 was the concurrency PR. Roadmap PR-67 was the dig restart.
- A macOS leg can end "not acquired" while the runner is online. Re-run the failed jobs of that run (D-358).

### Open questions that block progress

None blocks PR #67. D-358 resolves OQ-173.

### Next concrete action

Review PR #67 and write `docs/reviews/pr-67.md`.

## Session 160: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-63 as PR #65, the play test of the owner, D-354, and the owner answers on the concurrency groups of the workflows, D-355 to D-357, in the same invocation as Session 158 (D-297). Branch `docs/pr-63-merge-record`.

### What this session did, and why

- Session 159 approved `6f0d6f2` in `docs/reviews/pr-65.md` with no finding. The owner merged PR #65 as `002054a` at 18:10 UTC on 2026-09-14.
- The macOS leg of Bit identity on the review tip `d66fd24` never started. The self-hosted runner did not acquire the job in 20 minutes, and the compare job skipped. Four pushes in 15 minutes queued about 12 macOS jobs on the one Mac runner. The same code passed that leg on `6f0d6f2`, `e1d58db`, `1e85f94`, and `7028406`.
- The owner asked for a concurrency group in the workflows, so that a newer push cancels the older runs of a PR. The owner answered three questions. D-355 lets this run open that code PR as a one-time exception to D-121, after this PR merges. D-356 cancels older runs on PR events alone, keyed on the PR number, and the night never cancels. D-357 lets CI on the tip count for the effective head when every later commit is a metadata commit.
- The concurrency change is ready on the branch `chore/pr-run-concurrency`, commit `704ce9f` on `002054a`, pushed to origin with no PR. It holds the block in the nine PR workflows, the D-357 line in `CLAUDE.md`, `AGENTS.md`, and the pr-review skill, and two tests in `RepositoryShapeTests`. Locally it passed 875 tests with the five Smoke tests, `ste-check`, and a YAML parse of every workflow, and the new test fails on the workflows of `main`.
- The owner then asked that this PR carry every document that a fresh context needs, so it records D-355 to D-357 and F-99 ahead of the concurrency PR.
- The owner played floor 1 and confirms that the spaces no longer feel cramped. D-354 closes exit test 6 of PR-63.
- `docs/design.md` marks PR-63 merged in its entry and in sequence item 11, and it gains F-99. The Phase 2 roadmap gains the status line of PR-63 and the mark in sequence item 13.
- Session 150 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `002054a`, the squash merge of PR #65, and its tree equals the review tip `d66fd24`. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-63-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- CI on `002054a`: CI, smoke, and bit identity passed on the three platforms, and asset-qa, bots, det-lint, and STE check passed, the last at 18:25 UTC.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 873 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `002054a`.
- PR #66 on its first head `0b31f4e`: the automated pass approved with no comment, and every check passed, the review gate on the `review-override` label included. The register commit above it is newer than the label event and lies outside the metadata set, so the label came off and goes on again after the next pass (D-190).
- The concurrency branch: `origin/chore/pr-run-concurrency` at `704ce9f`, with no PR. No workflow runs on a push to a branch other than `main`.
- The night gate reads run 34858986484 at `f487401` until 15:58 UTC on 2026-09-16. The scheduled night of 2026-09-15 at 08:07 UTC is the first on the new sizes, and it can start hours late (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on again after the last push and the automated pass (D-188, D-190). After this PR merges, the concurrency PR opens from `chore/pr-run-concurrency`, rebased onto `main`, with its own handoff entry, and it takes a Codex review (D-188, D-355). PR-67 follows in a fresh session (D-121, D-353).

### Traps and gotchas

- The GitHub PR #65 is PR-63. The roadmap id PR-65 is the ramp meshes in Game.
- About one floor in 96000 fails to dig on the new sizes (F-98). The floors of the first night on them dug with no error in the measurement of Session 158. A change that moves the floors of the night can fail a night before PR-67 lands (D-353).
- The merge marks use the UTC date of the merge, 2026-09-14. D-354 and this entry use the local date, also 2026-09-14.
- The concurrency commit `704ce9f` sits on `002054a`, and `main` gains this PR first. Rebase the branch onto `origin/main` before the PR opens. The rebase has no conflict, because this PR touches no file of that commit, and the push after it needs `--force-with-lease`.
- D-355 to D-357 and F-99 reach `main` with this PR, so the concurrency PR adds no decision row. It marks F-99 corrected in `docs/design.md`, its handoff entry is Session 161, and its description names the three decisions.
- Under D-356, the handoff push of the concurrency PR cancels the runs of its code commit. That is the change at work and not a failure, and D-357 makes CI on the tip the evidence.
- The review gate runs from the base branch (D-197), so its concurrency group starts to act only after the concurrency PR merges.
- The exact block in each of the nine PR workflows, before `jobs:`:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.event.pull_request.number || github.sha }}
  cancel-in-progress: ${{ github.event_name == 'pull_request' || github.event_name == 'pull_request_target' }}
```

- The next ids are D-358, OQ-173, F-100, PR-68, and Session 161.

### Open questions that block progress

None blocks this PR or the concurrency PR. PR-67 asks the owner for the job budget and the count of digs before the code (D-353). OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. This run, or a fresh session, then opens the concurrency PR: check out `chore/pr-run-concurrency`, rebase it onto `origin/main`, run the build, `dotnet test`, and `ste-check`, push with `--force-with-lease`, open the PR, and add Session 161. The PR takes the automated pass and a Codex review. A fresh session then opens PR-67, and it asks the owner for the job budget and the count of digs first (D-353).

## Session 159: 2026-09-14, Codex

Author: Codex
Session: review PR #65, the dig sizes in the floor template, at effective head `6f0d6f2`. Branch `feat/pr-63-dig-sizes`.

### What this session did, and why

- Checked the provider gate. Session 158 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, affected callers and tests, roadmap, design, decisions, questions, and all PR comments.
- Found no in-scope defect. The template validates the six dig sizes, the plan uses the template sizes, the detail pass uses each tunnel height, and the tests check the changed contract.
- Added `docs/reviews/pr-65.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `d2ef347`. The effective head is `6f0d6f2`. The later handoff commit is metadata under D-184.
- Revision-matched CI passed on all three platforms for build-and-test, bit identity, and smoke. Asset QA, bots, det-lint, STE check, and the night gate passed. The compare job passed with `b00814dbf25e61e8`.
- The local test host could not bind its socket. This execution-context failure does not provide local test evidence. Remote CI provides revision-matched test evidence.
- The automated pass approved the head with no issue comment. The review gate was neutral before the review record existed, and `evaluate` failed for that expected reason.

### In flight

PR #65 is ready for owner merge after this review record reaches the branch. Exit test 6 still needs the owner play test of floor 1. PR-67 follows in a fresh session (D-121).

### Traps and gotchas

- The effective head is `6f0d6f2`, not the later metadata tip, under D-184.
- About one floor in 96000 reaches the dig job cap on these sizes (F-98). D-353 assigns the restart to PR-67.
- The owner play test remains open even though the automated checks pass.
- The next ids are D-354, OQ-173, F-99, PR-68, and Session 160.

### Open questions that block progress

None blocks PR #65. Exit test 6 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner plays floor 1 for exit test 6, then merges PR #65. A docs PR records the merge (D-297). A fresh session then opens PR-67.

## Session 158: 2026-09-14, Claude Code

Author: Claude Code
Session: open PR-63, the dig sizes in the floor template, as PR #65. Branch `feat/pr-63-dig-sizes`.

### What this session did, and why

- The owner merged PR #64, the record of D-341 to D-351, as `d2ef347`, and asked for PR-63. The branch came from `origin/main` at `d2ef347`.
- Before the code, the owner chose D-352 on the recommendation: the validator rejects an even tunnel width, a width past the rock shell, and a dig height that leaves fewer than three rows of the floor. The dig plan also stops on an even width in a template that no validator read.
- `FloorTemplate` gains the six dig sizes, and `DigPlan` reads them in place of five Core constants. Each walker carries the radius and the height of its tunnel, a collapse heap reaches the height of its own tunnel, and `FloorPlan` lists the tunnel stamps. The content takes D-341, D-343, and D-344. The simulation version is 8, and the bit-identity known answer moved from `2258ba8b9cc94b3f` to `b00814dbf25e61e8`.
- `TunnelCrossSection` checks each stamp against the width and the height of its template, and it keeps the D-166 window check on a mask in place of a hash set. `EveryBandHasOneFloorSize` replaces `FloorSizeGrowsWithDepth`. `ADigSizeOutsideItsBoundsIsAnError` covers each bound with its reason, and `AnEvenTunnelWidthStopsThePlan` covers the guard in the plan.
- A scratch program outside the repository dug the floors of the night: 175000 floors with 0 errors, so 5 to 9 rooms fit (D-344). The median need is 1 job, and the largest is 5890. 500000 more floors found 7 that ran the 10000 jobs of D-279 with a chamber still in rock (F-98). The session filed OQ-172, and the owner chose D-353 on the recommendation: PR-63 keeps the cap, and PR-67, a dig restart, comes right after it.
- `DigPlanJobCapTests` pins the three floors of the largest need, 5890, 5662, and 5568 jobs, in place of the F-92 floors of the old sizes.
- The design doc and the Phase 2 roadmap gain F-98 and the PR-67 entry, and the Phase 2 sequence puts PR-67 at item 14.
- The automated pass of gitar ran on `6f0d6f2` after the push. Its check run passed at 17:05 UTC, and it approved with no comment, so 0 comments needed an answer (D-250). Its comment shows the trial pause note, and the completed check run on the head made a `Gitar review` comment unnecessary.
- Session 148 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `d2ef347`. The effective head of PR #65 is `6f0d6f2`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-63-dig-sizes` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 873 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files. The Godot build check passed.
- The PR bot sweep ran locally on the code: the random walker and the greedy descender over seeds 1 to 100, 0 softlocks and 0 crashes, and the descender reached the bottom on every seed.
- CI on `6f0d6f2`: build-and-test passed on the three platforms, the last at 17:15 UTC. Bit identity passed on the three platforms with the compare job, so `b00814dbf25e61e8` holds on each. Smoke passed on the three platforms, and asset-qa, bots, det-lint, the night gate, STE check, and gitar passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251).
- The scheduled night of 2026-09-14, run 34858986484 at `f487401`, passed at 15:58 UTC. The night gate reads it until 15:58 UTC on 2026-09-16.

### In flight

PR #65: the Codex review per the `pr-review` skill at the effective head `6f0d6f2`. Exit test 6 needs the owner to play floor 1 and confirm that the spaces no longer feel cramped, recorded as a decision. The owner then merges, and a docs PR records the merge (D-297). PR-67 follows in a fresh session (D-121).

### Traps and gotchas

- About one floor in 96000 fails to dig on these sizes (F-98). The night passes because its floors are fixed. A change that moves the floors of the night, such as PR-66, can fail a night before PR-67 lands (D-353).
- `DigPlanJobCapTests` digs the three heaviest floors twice each, so the class is the slowest of the procgen tests. `TheCapHoldsTheMeasuredTail` pins the three counts, and a change to the dig moves them.
- A tunnel width in a floor template is odd (D-352). Job 0 is the gallery, and `TunnelStamp.Gallery` reads it.
- The scratch measurement program is not in the repository. F-98 names the seven failed floors, and `FloorGenerator.Generate` on one of them gives the error again.
- zsh does not split a variable that holds a command and its arguments into words. Two command chains of this session failed on it, so write each command in full.
- PR-67 asks the owner for the job budget and the count of digs before the code (D-353).
- The next ids are D-354, OQ-173, F-99, PR-68, and Session 159.

### Open questions that block progress

None blocks PR #65, and exit test 6 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #65 per the `pr-review` skill at the effective head `6f0d6f2` and writes `docs/reviews/pr-65.md`. The owner plays floor 1 for exit test 6, then merges, and a docs PR records the merge (D-297). A fresh session then opens PR-67.

## Session 157: 2026-09-14, Claude Code

Author: Claude Code
Session: record the owner answers on the dig sizes, the ramps, and the chamber tiers, D-341 to D-351, in the same invocation as Sessions 152, 154, and 156 (D-297). Branch `docs/dig-sizes-and-ramps`.

### What this session did, and why

- The owner merged PR #63, the PR-15 merge record, as `f487401` at 14:07 UTC on 2026-09-14.
- The owner then asked whether the tunnels of the demo have the real size of the first playable, because they feel very cramped, and whether ramps with different slopes can replace steps. The demo runs the real generator on floor 1 of seed 1: drifts of 3 by 3, a gallery of 5 by 3, chambers 3 to 4 high, and one-block steps that need a jump (D-165, D-253). F-97 records it.
- The owner answered eleven questions in three batches, D-341 to D-351: the wide sizes, the sizes in the floor template, one floor size and one room count on every band, ramps with true slopes of 1:2, 1:3, and 1:4, no one-block steps in tunnels, chamber tiers 2 blocks high by a chance per kind, and the dig work before PR-16.
- The preset named chamber boxes of 5 to 15 in all. D-341 gives each kind its old range times one and a half, rounded up. The owner can correct a range in the PR-63 session.
- D-351 splits the work into four roadmap items by concern (G-10): PR-63 the dig sizes, PR-64 the ramp cells in Core, PR-65 the ramp meshes in Game, and PR-66 the ramps and the tiers in the generator. The design doc and the Phase 2 roadmap carry the entries, and PR-16 reads ramps in its move rule and its first exit test.
- The Effect columns of D-78, D-165, D-252, D-253, and D-255 carry the marks of the parts that changed (D-186).
- Session 147 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `f487401`, the squash merge of PR #63. This branch holds one docs commit above it.
- Remote head: `origin/docs/dig-sizes-and-ramps` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 855 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `f487401`.
- The scheduled night of 2026-09-14 started at 14:57 UTC as run 34858986484 at `f487401`, 6 h 50 min after the cron, and ran still at 15:06 UTC. Until a night passes, the night gate reads run 34759337453 at `4a1048c`, which turns red at 14:13 UTC on 2026-09-15 (F-95).

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-63 is next, and a fresh session opens it (D-121). OQ-9 can take its answer at any time before PR-16.

### Traps and gotchas

- The demo floor is the output of the real generator, so a change to a dig size moves the simulation version and the bit-identity known answer (G-20).
- `FloorSizeGrowsWithDepth` and `TunnelCrossSection` in `ProcgenTests.cs` hold D-252 and the old constants. PR-63 replaces the first and updates the second.
- No measurement shows that 5 to 9 rooms fit a floor of 64 by 64 with the chambers of D-341. PR-63 files a question when the seed sweep fails (D-344).
- A ramp needs a direction, a slope, and a place along the slope, and the block id of D-164 is one byte. The PR-64 session decides the encoding before the code.
- The owner answers the motion on a ramp in the PR-64 session, and the shape of a tier in the PR-66 session, before the code (D-345, D-350).
- The next ids are D-352, OQ-172, F-98, PR-67, and Session 158.

### Open questions that block progress

None blocks this PR or PR-63. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. A fresh session then opens PR-63 per the Phase 2 roadmap.

## Session 156: 2026-09-14, Claude Code

Author: Claude Code
Session: record the merge of PR-15 as PR #62 and the play test of the owner, D-340, in the same invocation as Sessions 152 and 154 (D-297). Branch `docs/pr-15-merge-record`.

### What this session did, and why

- Session 155 approved `38d1fac` in `docs/reviews/pr-62.md` with P2-1 fixed. The owner merged PR #62 as `3f3e8bf` at 03:16 UTC on 2026-09-14. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- No decision recorded exit test 7 before the merge. The owner played the sword, first asked whether the roll was in the game, found it on Left Control, and confirmed that the sword feels committed and readable. D-340 closes exit test 7.
- `docs/design.md` marks PR-15 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-15 and the mark in sequence item 11.
- Session 146 moved to the archive, because the file held eleven entries with this one. Session 155 moved Session 145 before this session.

### State of the build

- `main` is at `3f3e8bf`, the squash merge of PR #62. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-15-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 16 files. `dotnet test`: 855 tests, 0 failures, with the five Smoke tests on the local Godot build. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `3f3e8bf`.
- The night gate reads the success of run 34759337453 at `4a1048c`, ended 14:13 UTC on 2026-09-13. It turns red at 14:13 UTC on 2026-09-15 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-14, and it can start hours late (F-95). At 13:51 UTC on 2026-09-14, no run of it had started.

### In flight

This PR: docs alone. The `review-override` label goes on after the last push and the automated pass (D-188, D-190). PR-16 is next, OQ-9 blocks it, and a fresh session opens it (D-121).

### Traps and gotchas

- The merge marks use the UTC date of the merge, 2026-09-14. D-340 and this entry use the local date, also 2026-09-14.
- The roll needs the ground, dry feet, and a ready cooldown, and its key is Left Control (D-289, D-329, D-337). A player who misses the key thinks the roll is absent, as the owner did first.
- The GitHub PR #62 is PR-15. The roadmap id PR-62 is the art quality pass after PR-20 (D-339).
- The next ids are D-341, OQ-172, F-97, PR-63, and Session 157.

### Open questions that block progress

None blocks this PR. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. The owner then answers OQ-9, the enemy families, and a fresh session opens PR-16 per the Phase 2 roadmap.

## Session 155: 2026-09-14, Codex

Author: Codex
Session: re-review PR #62 at effective head `38d1fac`.

### What this session did, and why

- Recomputed the effective head. The correction commit is `38d1fac`. Later review and handoff commits are metadata under D-184.
- Read the response file, the correction diff, the original trigger, the new regression tests, affected callers, and current PR comments.
- Verified that P2-1 is fixed. The focused Player, Animation, and Content suite passed 149 tests, including the traversal, separator, extension, and valid subdirectory cases.
- Updated `docs/reviews/pr-62.md` in place. P2-1 is `fixed in 38d1fac`, and the current verdict is `Ready for owner merge`.

### State of the build

- The revision-matched remote build, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar checks pass on `38d1fac`.
- Fresh `review-gate` and `evaluate` checks pass after the review update reaches the PR.
- Duplicate platform CI jobs remain in progress or queued after the metadata push. No pending job is reported as passed.
- The local full-suite run did not produce a final result in the execution window. Session 154 reports 855 tests passed with five Smoke tests, and remote CI provides revision-matched evidence.

### In flight

PR #62 is ready for owner merge after the remaining duplicate CI jobs finish. Exit test 7 remains the owner play test. A docs PR records the merge (D-297).

### Traps and gotchas

- The effective head is `38d1fac`, not the later metadata tip.
- Keep P2-1 and its original trigger in later review history.
- The art quality roadmap item also uses PR-62. It is not GitHub PR #62.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 156.

### Open questions that block progress

None blocks PR #62. Exit test 7 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the remaining duplicate CI jobs. The owner then plays the sword, merges PR #62, and records the merge in a docs PR.

## Session 154: 2026-09-13, Claude Code

Author: Claude Code
Session: answer the PR #62 review, P2-1. Branch `feat/pr-15-player-first-weapon`.

### What this session did, and why

- Read `docs/reviews/pr-62.md` at the reviewed head `6e35bc5`. P2-1 has full merit: `WeaponDefinition.AssetPath` checked the `models/` prefix alone, so a '..' segment and the extension of the other kind passed Core validation.
- Nine regression cases in `ContentTests.AWeaponOutsideItsBoundsIsAnError` failed against the validator of `6e35bc5` before the correction, with 16 other cases passed.
- `AssetPath` takes the extension of its kind, and it rejects a backslash, an empty, '.', or '..' segment, and a file name without the extension after a name. The extensions of a model and an animation moved into `ContentLoader`, and `AssetPaths` reads them. `AWeaponAssetPathUnderTheModelDirectoryLoads` keeps a path in a subdirectory of `models/` valid.
- `docs/reviews/pr-62-response.md` records the disposition, the evidence, and the checks.
- The review session left eleven entries in the file. Sessions 144 and 143 moved to the archive, so the file holds ten with this one.

### State of the build

- `main` is at `aeb91df`. The effective head is the correction commit above the review commits `37f519d` and `5b8f03a`, and it holds this entry, the response file, and the corrected files in one commit (D-182).
- Remote head: `origin/feat/pr-15-player-first-weapon` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 855 tests, 0 failures, with the five Smoke tests on the local Godot build. `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `asset-qa`: 0 findings. `ste-check`: 0 findings in 16 files.
- The night gate reads the success of run 34759337453 at `4a1048c`, ended 14:13 UTC on 2026-09-13. It turns red at 14:13 UTC on 2026-09-15 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-14, and it can start hours late (F-95).

### In flight

PR #62: the automated pass of gitar on the correction head, then the repeat Codex review of P2-1 per the `pr-review` skill. The owner then plays the sword for exit test 7 and merges. A docs PR records the merge (D-297). The automated pass runs after the push, and the PR carries its result (D-250).

### Traps and gotchas

- The effective head is the correction commit, and not a later metadata commit (D-184).
- A weapon asset path names its kind by its extension: a model ends in `.bbmodel`, and a swing clip ends in `.json`. A path in a subdirectory of `models/` still loads.
- `review-gate` reads red until the repeat review approves the correction head (D-181).
- Exit test 7 of PR-15 is still open: the owner plays the sword and records the result as a decision before the merge.
- Session 153 reported a local test host that could not bind its socket. The full local suite ran in this session, and the state of the build gives its result.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 155.

### Open questions that block progress

None blocks PR #62. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews the correction of P2-1 per the repeat review procedure of the `pr-review` skill at the correction head and sets the verdict. The owner plays the sword for exit test 7, then merges, and a docs PR records the merge (D-297).

## Session 153: 2026-09-13, Codex

Author: Codex
Session: review PR #62, the player and the first sword, at effective head `6e35bc5`.

### What this session did, and why

- Checked the provider gate. Session 152 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the full effective diff, the design contract, decisions D-318 to D-339, the questions register, the Phase 2 roadmap, affected callers, tests, and all PR comments.
- Found P2-1. `WeaponDefinition.AssetPath` accepts dot-segment paths and the wrong file extension because it checks only the `models/` prefix. The record gives the correction and regression check.
- Added `docs/reviews/pr-62.md` with the effective head and the verdict `Changes required`.

### State of the build

- `main` and the merge base are `aeb91df`. The effective head is `6e35bc5`. The review and handoff are pushed in metadata commit `37f519d`.
- The focused Player, Animation, and Content tests passed 138 tests. The serial build passed with 0 warnings and 0 errors.
- The full local test host failed to bind its socket. The non-Smoke full suite did not produce a result in the local execution window. These are execution-context limits.
- Revision-matched remote build-and-test, bit identity, smoke, bots, det-lint, asset QA, STE check, night gate, and Gitar checks passed on `6e35bc5`. `evaluate` and `review-gate` do not approve the head until this record reaches the PR.

### In flight

The author must correct P2-1, run the regression check, and request a re-review at the new effective head. Exit test 7 remains the owner play test.

### Traps and gotchas

- Under D-184, the review head is `6e35bc5`, not a later metadata tip.
- The art quality roadmap item also uses PR-62. It is not GitHub PR #62 and remains after PR-20 under D-339.
- The local full suite has a test-host socket restriction. Do not report it as passed.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 154.

### Open questions that block progress

None blocks the review record. P2-1 blocks merge. Exit test 7 needs the owner play test. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The author fixes P2-1, runs the regression check, and requests the repeat review. The owner also records exit test 7.

## Session 152: 2026-09-13, Claude Code

Author: Claude Code
Session: write `README.md` with the launch steps, and open PR-15, the player and the first sword, as PR #62. Branch `feat/pr-15-player-first-weapon`.

### What this session did, and why

- PR #61 merged as `aeb91df` at 18:50 UTC on 2026-09-13, after Session 151. The owner asked for a root `README.md` with the steps to launch the game, and then for the next work, PR-15.
- A fresh clone of `aeb91df` with no `.godot` directory built in 4 seconds, ran the headless smoke session to tick 1000, and ended the test exit at tick 101. `README.md` holds those steps, the controls, and the notes for a bad argument. `README.md` is outside the override set of D-190, so the owner chose to carry it in PR-15 (D-318).
- Before the code, the owner answered the gaps of the PR-15 scope in six batches, D-319 to D-337. The sprint conflict of D-231 and D-315 went to 7 m/s (D-319). The attack bit swings the first weapon, and the Phase 1 shot ends, so D-320 supersedes D-265. Against the recommendation, the owner chose free move and live aim for the swing (D-324) and a roll that no hit lands on (D-328). The owner added a guard against a stunlock to the stagger (D-326), chose a sword model with a new metal tile (D-330), and replaced a half roll in water with no roll in water (D-337).
- Core: `Player` holds health, the roll, the stagger and its guard, and the swing. `MeleeWeapon` holds the exact wedge test of the arc. The loop hashes the run end kind as one byte and the player after the projectiles, and the simulation version is 7. `WeaponDefinition` is the `weapon` content type. `Int32.MaxValue` gave three det-lint findings, so the weapon type stores its ticks as `long` with the `UInt32.MaxValue` bound, as the projectile type does, and needs no allowlist entry.
- Content and Game: `sword-basic.json`, `sword-basic.bbmodel`, the metal rule and the atlas, the `weapon` locator, and the swing, roll, and stagger clips. Asset QA found the right forearm in the torso at tick 26 of the first swing clip, and the keyframe moved out to the right. The Game layer plays the clips, walks the limbs from the distance traveled, holds the sword in the right hand, and stands the lowest box corner of the pose on the feet. The smoke script swings and rolls.
- The bit-identity sweep folds a projectile run and the arc test of its own, because no intent fires a shot now. The known answer moved from `6ec00e90c1c85cdb` to `2258ba8b9cc94b3f`.
- The owner read the contact sheet and asked whether these were test materials. The art is the first output of the approved pipeline, and no roadmap item raised it. D-338 keeps the PR-15 sheet as a first pass and closes exit test 8. D-339 adds PR-62, an art quality pass after PR-20 (OQ-171, F-96).
- The owner parked exit test 7, the play test, and runs the Codex review and the merge after the automated pass.
- The automated pass of gitar approved `932cf0a` with one suggestion: an arc with fewer hundredths than active ticks. It had partial merit. The coverage claim did not hold, and a wedge of no turn accepted a point on its line behind the body. `6e35bc5` rejects that arc and that wedge with regression tests, the reply on the thread names the evidence, and the thread is resolved. The pass approved `6e35bc5` at 22:31 UTC with the finding resolved, and its reply on the thread accepted the reasoning. The watcher posted `Gitar review` at 22:35 UTC, because the check run of the head was not listed yet, and gitar answered `On it`. The approved check run on the head is the pass of D-250.
- Session 142 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `aeb91df`. The effective head of PR #62 is `6e35bc5`, the fix commit above the code commit `932cf0a`. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-15-player-first-weapon` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test` on the code of `932cf0a`: 841 tests with the five Smoke tests on the local Godot build, 0 failures after the correction of four sentences that `SteCheckTests` found before the commit. The fix commit passed the 250 tests of the classes it touches.
- `det-lint`: 0 findings, Core 0 in 65 files, Game 0 in 32 files. `ste-check`: 0 findings in 16 files. `asset-qa`: 0 findings in 2 models and 3 animations. The Godot build check passed, and the contact sheet rendered in a window.
- On `932cf0a`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and gitar passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251). On `6e35bc5`, CI, bit identity, and smoke passed on the three platforms again, and bots, det-lint, asset-qa, STE check, the night gate, and gitar passed.
- The night gate reads the success of run 34759337453 at `4a1048c`, ended 14:13 UTC on 2026-09-13. It turns red at 14:13 UTC on 2026-09-15 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-14, and it can start hours late (F-95).

### In flight

PR #62: the Codex review per the `pr-review` skill at the effective head `6e35bc5`, the play test of the owner for exit test 7, and then the owner merge. A docs PR then records the merge (D-297).

### Traps and gotchas

- Exit test 7 of PR-15 is open: the owner plays the sword and records the result as a decision before the merge. Exit test 8 closed as a first pass (D-338).
- The bit-identity known answer is `2258ba8b9cc94b3f`. The hash reads the run end as one byte of its kind and the player after the projectiles, and `StairwellTests.HashWith` holds that order.
- Nothing in the loop deals damage before PR-16. A test deals damage with `loop.Player.TakeHit`, which the record does not hold, so a replay cannot see it. PR-16 deals damage inside `Step`.
- The swing clip keeps the right arm in front of its own shoulder, and the torso twist gives the sweep. A straight arm across the chest clips the torso or the head at a keyframe, and asset QA reports it.
- The walk amount follows the horizontal speed up to the walk speed, so a body at rest stands straight. D-333 names the stride and the angles alone, and the play test judges that rule.
- The model root stands the lowest box corner of the pose on the feet, so the roll lifts the model about 0.55 m at its middle.
- The GitHub PR #62 is PR-15. The roadmap id PR-62 is the art quality pass.
- The branch came from `origin/main` and tracked it at first. The first push set the upstream to the feature branch, and a push names the branch.
- The next ids are D-340, OQ-172, F-97, PR-63, and Session 153.

### Open questions that block progress

None blocks PR #62. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #62 per the `pr-review` skill at the effective head `6e35bc5` and writes `docs/reviews/pr-62.md`. The owner plays the sword for exit test 7, then merges, and a docs PR records the merge (D-297).

## Session 151: 2026-09-13, Claude Code

Author: Claude Code
Session: record the merge of PR-61 as PR #60, in the same invocation as Session 149 (D-297). Branch `docs/pr-61-merge-record`.

### What this session did, and why

- Session 150 approved `1e3dba8` in `docs/reviews/pr-60.md` with no finding, and `review-gate` passed. The owner merged PR #60 as `4a1048c` at 07:46 UTC on 2026-09-13.
- `docs/design.md` marks PR-61 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-61 and the mark in sequence item 10.
- No decision and no question changed in this PR. D-317 entered the register with PR #60.
- Session 141 moved to the archive, because the file held eleven entries with this one. Session 150 moved Session 140 before this session.

### State of the build

- `main` is at `4a1048c`, the squash merge of PR #60. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-61-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- On `4a1048c`, CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed.
- `ste-check`: 0 findings in 15 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `4a1048c`.
- `dotnet test`: 778 tests, 0 failures, with the five Smoke tests on the local Godot build, on the final documents of this PR.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass (D-188, D-190). Then a fresh session opens PR-15 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- PR-15 builds the zero-weight case alone (D-316). Its exit test 7 needs the owner to play the sword, and PR-22 holds `WeightSlowsDodge` and `HeavyArmorResistsStagger`.
- No decision names the author of the first animation files of PR-15. The PR-15 scope lists them as work of the PR, in the format of D-298.
- A PR that adds a Game flag adds it to the table of `UserArguments`, and to the ignored flag rule when a session ignores it (D-313, D-317). `SessionCommandsParse` reads every Godot command of `CLAUDE.md`.
- Session 150 reports that the local build, the full test, and `det-lint` stopped with no output in its checkout. The same commands completed in Session 149, so remote CI and the author checks carry that evidence.
- The merge marks use the UTC date of the merge, 2026-09-13, and so does this entry.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 152.

### Open questions that block progress

None blocks this PR or PR-15. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-15 from `main` per the Phase 2 roadmap and D-314 to D-316.

## Session 150: 2026-09-13, Codex

Author: Codex
Session: review PR #60 at effective head `1e3dba8`.

### What this session did, and why

- Checked the provider gate. Session 149 identifies Claude Code as the author, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, complete diff, affected callers and tests, roadmap, design, decisions, questions, review records, and all PR comments.
- Found no in-scope defect. The parser rejects unknown words, unknown flags, repeated flags, short flags, and ignored combinations with contextual errors.
- Added `docs/reviews/pr-60.md` with the effective head and the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `98e3c47`. The effective head is `1e3dba8`. The later handoff commit is metadata under D-184.
- The focused tests passed, 37 tests with 0 failures. The local build, full test, and det-lint commands produced no output and did not complete. Revision-matched remote CI passed the required code, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar checks.
- The fresh `evaluate` and `review-gate` checks pass after the review record reached the PR. Duplicate post-metadata platform jobs remain pending, while their prior revision-matched checks pass.

### In flight

The review record and this handoff entry are pushed in `8d9477a`. The owner can merge after the duplicate platform jobs finish, if branch protection requires them.

### Traps and gotchas

- The effective head is `1e3dba8`, not the later metadata tip, under D-184.
- A new flag needs an entry in `UserArguments` and an ignored-flag rule when a session ignores it (D-313, D-317).
- The local dotnet commands can stop without output in this checkout. Remote CI provides separate evidence.
- The next ids are D-318, OQ-171, F-96, PR-62, and Session 151.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-99 remains open but blocks no work.

### Next concrete action

Commit and push this review and handoff. Verify the fresh review-gate result and the synchronized remote head.

## Session 149: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-61, the user argument check, as PR #60. Branch `feat/pr-61-user-argument-check`.

### What this session did, and why

- PR #59 merged as `98e3c47`, and Session 148 named PR-61 as the next action. This session opened it from `main` per D-313.
- The owner answered the scope question for ignored and conflicting flags. D-317 puts whole-list argument parsing in PR-61.
- `UserArguments` validates unknown words, unknown flags, repeated flags, short flags, and ignored flags. `Main` parses before the Game sessions start.
- The tests cover the parser, boot failure, and the affected Game sessions. The documents name D-317 and the new roadmap exit test.
- The automated pass approved `1e3dba8` with no issue comment.

### State of the build

- `main` and the merge base were `98e3c47`. The effective head of PR #60 was `1e3dba8`. The later handoff commit was metadata under D-184.
- The local build, full test, and det-lint commands did not produce a final result. Revision-matched remote CI passed the required code, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and automated checks.

### In flight

The Codex review of PR #60 and the owner merge remained in flight. A docs PR then recorded the merge (D-297).

### Traps and gotchas

- Engine flags stand before the `--` separator. Game flags stand after it.
- A long pending CI state can mean a runner queue. The `evaluate` failure precedes the review record.
- The next ids were D-318, OQ-171, F-96, PR-62, and Session 150.

### Open questions that block progress

None blocked PR #60. OQ-9 blocked PR-16. OQ-4 and OQ-6 blocked PR-17. OQ-44 blocked PR-18. OQ-48 blocked PR-20. OQ-161 blocked exit test 7 of PR-13 and M-3.

### Next concrete action

A Codex session reviews PR #60 at effective head `1e3dba8` and writes `docs/reviews/pr-60.md`.

## Session 148: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-60 as PR #58 and four owner answers, D-313 to D-316 (D-297). Branch `docs/pr-60-merge-record`.

### What this session did, and why

- The owner asked the session to address the PR #58 re-review feedback. The PR had no open feedback at `9803a8b`: Session 146 fixed P2-2, and gitar approved the head. The trigger `--smoke --press escape 100 unexpected` ended at boot with exit code 1 and named `unexpected`. The review commit `e5a3846` raised P2-2, so the cross-provider repeat review of the fix was still necessary.
- Session 147 approved `9803a8b` in `e35d5e9`, and `review-gate` passed on that head. The owner merged PR #58 as `94f0897` at 00:16 UTC on 2026-09-13. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-60 merged in the roadmap entry and in sequence item 11. The Phase 2 roadmap gains the status line of PR-60 and the mark in sequence item 8.
- The owner answered three questions, each with the recommendation. D-313 resolves OQ-170: one parser checks the whole user argument list, in PR-61 before PR-15. D-314 resolves OQ-5: heavy armor resists stagger, and light armor does not. D-315 resolves OQ-46: the initial combat numbers.
- PR-15 has no armor, so its exit test 1 and D-314 needed weight numbers that no decision held. The owner chose D-316: PR-15 builds the zero-weight case, and PR-22 sets the growth of the dodge cooldown with weight and the weight at which armor resists stagger, with the first armor sets.
- `docs/design.md` and the Phase 2 roadmap gain the PR-61 entry, and the Phase 2 sequence puts PR-61 at item 10. The later items move down by one. The PR-15 scope and exit test 1 follow D-316. The Phase 3 roadmap gives PR-22 the two weight numbers and exit test 7, `HeavyArmorResistsStagger`.
- Session 138 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `94f0897`, the squash merge of PR #58. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-60-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed, so `det-lint`, `asset-qa`, and `bit-identity` stand as CI recorded them on `94f0897`.
- `dotnet test`: 770 tests, 0 failures, with the four Smoke tests on the local Godot build. That run started before the last Phase 2 list fix and this entry. The run without the Smoke category passed on the final documents, 766 tests and 0 failures.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass (D-188, D-190). Then a fresh session opens PR-61 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- The Phase 2 sequence moved down by one after item 9. An older handoff that names item 10 or a later item means the item one number higher now.
- PR-61 moves the trailing word check of `TestExit.PressOf` into the shared parser. `SmokeSessionPasses`, `EscapeEndsTheSession`, and `StartButtonEndsTheSession` pass user arguments, so each must still pass.
- PR-15 builds the zero-weight case alone (D-316). Its exit test 1 has no weight clause, and PR-22 holds `WeightSlowsDodge` and `HeavyArmorResistsStagger`.
- The merge marks use the UTC date of the merge, so PR-60 reads 2026-09-13. The decisions and this entry use the local date 2026-09-12, as D-295 and Session 147 did.
- No decision names the author of the first animation files of PR-15. The PR-15 scope lists them as work of the PR, in the format of D-298.
- The next ids are D-317, OQ-171, F-96, PR-62, and Session 149.

### Open questions that block progress

None blocks this PR or PR-61. PR-15 has no open blocker, and its exit test 7 needs the owner to play the sword. OQ-9 blocks PR-16. OQ-4 and OQ-6 block PR-17. OQ-44 blocks PR-18. OQ-48 blocks PR-20. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-61 from `main` per the Phase 2 roadmap and D-313. PR-15 follows it.

## Session 147: 2026-09-12, Codex

Author: Codex
Session: re-review PR #58 at effective head `9803a8b`.

### What this session did, and why

- Checked the provider gate again. The handoff identifies Claude Code as the author of the correction, so Codex remains the eligible reviewer under T-4 and D-101.
- Read the response file, the correction diff, the affected tests, the roadmap, the decisions, the questions, and all current PR comments.
- Verified that P2-2 is fixed. `PressOf` rejects a plain word after the press tick, names the word, and preserves flag ordering for the other parser.
- Verified the correction with the unit tests and the headless command. The command with `unexpected` now fails at boot with exit code 1 and no successful test-exit line.
- Updated `docs/reviews/pr-58.md` with P2-1 and P2-2 fixed, OQ-170 out of scope, and the verdict `Ready for owner merge` for effective head `9803a8b`.

### State of the build

- `main` and the merge base are `f3f0bc0`. The effective head is `9803a8b`. The later review metadata commits remain outside the effective diff under D-184.
- The focused tests passed, 15 tests with 0 failures. The full suite passed, 770 tests with 0 failures, as reported in the prior handoff. The build, det-lint, STE check, and revision-matched remote gates passed.
- `evaluate` and `review-gate` still read the earlier review record and fail until this update reaches the PR.
- Remote head: the review update is not pushed yet.

### In flight

The owner can merge PR #58 after the fresh review-gate result passes. OQ-170 remains open for whole-list validation in a later change.

### Traps and gotchas

- P2-1 and P2-2 are fixed at `9803a8b`. Keep both finding ids and their evidence in later records.
- OQ-170 accepts the remaining unknown-flag behavior across the other Game parsers. It does not block PR #58.
- The effective head is `9803a8b`, not a later metadata commit, under D-184.
- The next ids are D-313, OQ-171, F-96, and Session 148.

### Open questions that block progress

None blocks PR #58. OQ-170 blocks no work. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159, OQ-160, OQ-44, and OQ-99 do not block this PR.

### Next concrete action

Commit and push the updated review and handoff. Refresh `review-gate` and `evaluate`, then the owner can merge PR #58.

## Session 146: 2026-09-12, Claude Code

Author: Claude Code
Session: answer the PR #58 repeat review, P2-2. Branch `feat/pr-60-fullscreen-test-exit`.

### What this session did, and why

- Read `docs/reviews/pr-58.md` at the reviewed head `39a0c02`. P2-1 is fixed there. P2-2 has partial merit: a plain word after the press tick passed in silence, and T-2 binds the user arguments. The whole argument grammar is outside PR-60, because every older flag has the same property, so that part is OQ-170 for the owner.
- `TestExit.PressOf` reads the word after the tick when one exists. A word that does not start with `--` is a `ContextException` that names the word. A flag after the tick belongs to its own parser, so the two orders of `--smoke` and `--press` both work.
- `PressOfRejectsATrailingWord` asserts the error and the word for the trigger of the review and for a second number after the tick. It fails on the parser of `39a0c02`. `PressOfReadsTheInputAndTheTick` gains the two orders with a flag after the tick.
- The command `--smoke --press escape 100 unexpected` ends at boot with exit code 1 and names `unexpected`.
- `docs/reviews/pr-58-response.md` gains the P2-2 section and the checks of this head. `docs/questions.md` gains OQ-170.
- Session 136 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `f3f0bc0`. The effective head is the correction commit above the repeat review commit `e5a3846`, and it holds this entry, the response file, and the corrected files in one commit (D-182).
- Remote head: `origin/feat/pr-60-fullscreen-test-exit` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 770 tests, 0 failures, with the four Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. Core and content did not change.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #58: the second repeat Codex review of P2-2 at the correction head, then the owner merge. The automated pass on the correction head runs after the push, and the PR carries its result (D-250).

### Traps and gotchas

- The press parser rejects a plain word after the tick and accepts a flag there. It reads nothing else, so an unknown flag anywhere still passes in silence until OQ-170 has its answer.
- The automatic pass of gitar did not run on `39a0c02`, and the head had no `Gitar` check run at all. The comment `Gitar review` ran it, and the pass approved the head three minutes later (D-303). Read the check runs of the head before you post the comment.
- The effective head is the correction commit and not a later metadata commit (D-184).
- The next ids are D-313, OQ-171, F-96, and Session 147.

### Open questions that block progress

None blocks PR #58. OQ-170 blocks nothing. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews P2-2 per the repeat review procedure of the `pr-review` skill at the correction head and sets the verdict. The owner then merges, answers OQ-170, and a docs PR records the merge (D-297).

## Session 145: 2026-09-12, Codex

Author: Codex
Session: re-review PR #58 at effective head `39a0c02`.

### What this session did, and why

- Checked the provider gate again. The handoff identifies Claude Code as the author of the correction, so Codex remains the eligible reviewer under T-4 and D-101.
- Read `docs/reviews/pr-58-response.md`, the correction diff, the affected files, the roadmap, the decisions, the questions, and all current PR comments.
- Verified that P2-1 is fixed. The two Smoke tests drive Escape and Start through the headless Game process, and both assert exit code 0, the test exit line, and no smoke end line.
- Found P2-2. `TestExit.PressOf` accepts trailing arguments after the press tick. A command with `unexpected` exits successfully and ignores that argument, which violates T-2 input validation.
- Updated `docs/reviews/pr-58.md` with P2-1 fixed, P2-2 open, and the verdict `Changes required` for effective head `39a0c02`.

### State of the build

- `main` and the merge base are `f3f0bc0`. The effective head is `39a0c02`. The review and handoff metadata commits remain outside the effective diff under D-184.
- The serial build passed with 0 warnings and 0 errors. Focused tests passed, 7 tests with 0 failures. The Escape and Start command probes passed at tick 101. Det-lint and STE check passed.
- The full local test stalled after discovery and was cancelled. Revision-matched CI passed build, test, smoke, bit identity, bots, det-lint, asset QA, STE check, night gate, and Gitar. `evaluate` and `review-gate` fail because P2-2 remains open.
- Remote head: `08e3797` holds the review update, verified with the session end gate.

### In flight

PR #58 needs trailing-argument validation and its regression test. The owner must request another repeat review after the correction.

### Traps and gotchas

- P2-1 is fixed at `39a0c02`. Keep its finding id and evidence in later reviews.
- P2-2 reproduces with `--smoke --press escape 100 unexpected`. The command must reject `unexpected` after the correction.
- The effective head is `39a0c02`, not a later metadata commit, under D-184.
- The next ids are D-313, OQ-170, F-96, and Session 146.

### Open questions that block progress

None blocks PR #58. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159, OQ-160, OQ-44, and OQ-99 do not block this PR.

### Next concrete action

The author rejects trailing arguments, adds the regression test, pushes the correction, and requests the repeat cross-provider review.

## Session 144: 2026-09-12, Claude Code

Author: Claude Code
Session: answer the PR #58 review, P2-1. Branch `feat/pr-60-fullscreen-test-exit`.

### What this session did, and why

- Read `docs/reviews/pr-58.md` at the reviewed head `3dfbf03`. P2-1 has full merit: exit tests 2 and 3 of the Phase 2 roadmap ask for a session that ends with exit code 0 and an end line, and the two tests proved only the predicate of `TestExit`.
- `TestExit` gains the `--press` flag. The two arguments after it name the input, `escape` or `start`, and the tick. A bad argument is a `ContextException` that names the cause (T-2). `Main` reads the flag at boot and gives the engine event of the press to the input singleton at that tick. The engine holds the input down from the next frame, and the poll of the next tick reads it as a real press.
- `SmokeSessionTests` gains `EscapeEndsTheSession` and `StartButtonEndsTheSession` in the Smoke category. Each runs the headless smoke session with a press at tick 100 and asserts exit code 0, the test exit line at a later tick, no error line, and no smoke end line. Both fail on the Game code of `3dfbf03`, where the session runs to its own end line.
- The unit tests of the predicate are `EscapePressesTheExit` and `StartButtonPressesTheExit` now. Two tests cover the flag parse and its errors. `NoOtherInputEndsTheSession` stays as exit test 4.
- `CLAUDE.md` and `AGENTS.md` gain the test exit session command. The PR-60 scope of the Phase 2 roadmap names the flag and the engine tests.
- `docs/reviews/pr-58-response.md` records the disposition, the correction, and the checks.
- Sessions 133 and 134 moved to the archive, because the file held twelve entries with this one.

### State of the build

- `main` is at `f3f0bc0`. The effective head is the correction commit above the review commit `0f344e6`, and it holds this entry, the response file, and the corrected files in one commit (D-182).
- Remote head: `origin/feat/pr-60-fullscreen-test-exit` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 769 tests, 0 failures, with the four Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. Core and content did not change.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #58: the repeat Codex review of P2-1 at the correction head, then the owner merge. The automated pass on the correction head runs after the push, and the PR carries its result (D-250).

### Traps and gotchas

- The press flag hands the engine one event through the input singleton. The event takes effect on the next frame, so the exit fires one tick after the press tick in a headless run at 60 Hz. The engine tests assert a range, not the exact tick.
- A press flag with a bad name or a bad tick is a boot failure with exit code 1, and never a session that runs.
- The Codex review session left eleven entries in the file. Count the entries before you add one, and move every entry past the tenth.
- The effective head is the correction commit and not a later metadata commit (D-184).
- The next ids are D-313, OQ-170, F-96, and Session 145.

### Open questions that block progress

None blocks PR #58. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews P2-1 per the repeat review procedure of the `pr-review` skill at the correction head and sets the verdict. The owner then merges, and a docs PR records the merge (D-297).

## Session 143: 2026-09-12, Codex

Author: Codex
Session: review PR #58 at effective head `3dfbf03`.

### What this session did, and why

- Verified the provider gate. The handoff identifies Claude Code as the author of the code commit, so Codex is the eligible reviewer under T-4 and D-101.
- Read the PR description, full effective diff, roadmap, decisions, questions, review records, and all PR comments.
- Found P2-1. The Escape and Start tests check only `TestExit.IsPressed`. They do not prove that `Main` writes the end line or quits with exit code 0, although D-311 and the roadmap require that behavior.
- Added `docs/reviews/pr-58.md` with the verdict `Changes required` for effective head `3dfbf03`.

### State of the build

- `main` and the merge base are `f3f0bc0`. The effective head is `3dfbf03`. The later handoff commit remains outside the effective diff under D-184.
- The serial local build passed with 0 warnings and 0 errors. Focused tests passed, 12 tests with 0 failures. Det-lint, STE check, bit identity, and the Godot build check passed.
- The local full test stalled after discovery and was cancelled. Remote CI reported in the previous handoff passed on the code head. The metadata-tip CI rerun passes the product jobs. `evaluate` fails and `review-gate` is neutral until the review record reaches the PR.
- `git fetch origin` could not open `.git/FETCH_HEAD` before the elevated retry. The local branch is at the remote PR metadata tip `5e4dfe1` after the review push.

### In flight

PR #58 needs end-to-end Escape and Start exit tests. The owner must merge only after the finding is corrected and the review gate passes for the effective head.

### Traps and gotchas

- The effective head is `3dfbf03`, not the metadata tip `74ab9d8`, under D-184.
- The focused tests pass because they call the helper directly. They do not run the game loop.
- The next ids are D-313, OQ-170, F-96, and Session 144.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-44 and OQ-99 do not block this PR.

### Next concrete action

The author adds integration coverage for both test exits, pushes the correction, and requests the repeat cross-provider review.

## Session 142: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-60, the fullscreen window and the test exit, as PR #58. Branch `feat/pr-60-fullscreen-test-exit`.

### What this session did, and why

- PR #57 merged as `f3f0bc0`, and the Session 141 handoff named PR-60 as the next action. This session opened it from `main` per D-310 to D-312.
- `WhatYouCarry.Game/project.godot` gains a `[display]` section with `window/size/mode=3`, the borderless fullscreen of the engine, and no window size, so the viewport takes the resolution of the display (D-310).
- `WhatYouCarry.Game/Input/TestExit.cs` holds the two exit inputs, the Escape key and the Start button of the first controller (D-311). `Main` polls it once per tick before the intent, in every session that runs the loop, and quits with the end line `The test exit ends the session.` and exit code 0 when the log holds no error line. `Main` now holds one poll for the reader and the exit.
- The fake poll of the reader tests moved to `WhatYouCarry.Tests/FakePoll.cs`, because the test exit tests share it. `TestExitTests.cs` holds exit tests 2 to 4, and `GameShapeTests.WindowOpensFullscreen` is exit test 1. The window test fails on the old project file with `The project has no display section.`
- `CLAUDE.md` and `AGENTS.md` update the play session line: the window opens fullscreen, Escape or Start ends the session, and the engine flag `--windowed` gives a window.
- The automated pass of gitar ran on `3dfbf03` after the push and approved it at 19:52 UTC with no comment, and its dashboard comment still shows the trial pause note (D-250). The comment `Gitar review` per D-303 got the reply `On it` at 19:53 UTC and no further output in fifty minutes. The approved check run on the head is the pass of D-250.
- Session 132 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `f3f0bc0`. The effective head of PR #58 is `3dfbf03`, the one code commit. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-60-fullscreen-test-exit` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 765 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. The Godot build check passed.
- On `3dfbf03`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, and the night gate passed. The `evaluate` check fails and `review-gate` is grey, because no review record exists yet (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

PR #58: the Codex review per the `pr-review` skill at the effective head `3dfbf03`, then the owner merge. A docs PR then records the merge (D-297).

### Traps and gotchas

- Every session with a window now opens in fullscreen: the play session, the windowed contact sheet run, and the M-3 bot session. The engine flag `--windowed` overrides it. A headless run opens no window, and the smoke workflow proves it.
- The test exit polls the input once per tick in every session that runs the loop, the smoke and bot sessions too. A headless run has no key down, so CI never ends there. A windowed bot session ends on Escape, and the frame log still writes.
- No visual check of the fullscreen ran in this session, because a test cannot see the window. The play session of the owner is that check.
- The `evaluate` check fails until the review record exists, and the `review-gate` check stays grey (D-251).
- The pause note of gitar can show on a PR whose automatic pass ran and approved the head. Read the `Gitar` check run on the head before you post `Gitar review`, and count an `On it` reply with no later output as no new pass.
- The next ids are D-313, OQ-170, F-96, and Session 143.

### Open questions that block progress

None blocks PR #58. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #58 per the `pr-review` skill at the effective head `3dfbf03` and writes `docs/reviews/pr-58.md`. The owner then merges, and a docs PR records the merge. The owner answers OQ-5 and OQ-46 before PR-15.

## Session 141: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-14 as PR #56 and two owner instructions for PR-60, in the same invocation as Sessions 137 and 139 (D-297). Branch `docs/pr-14-merge-record`.

### What this session did, and why

- The owner merged PR #56 as `3d8060b` at 19:21 UTC, with the verdict `Ready for owner merge` for `c83360e` in `docs/reviews/pr-56.md`. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-14 merged in the roadmap entry and in sequence item 10. The Phase 2 roadmap gains the status line of PR-14 and the mark in sequence item 7.
- The owner asked how to run the game from the command line. `CLAUDE.md` and `AGENTS.md` gain the play session command and its quit keys.
- The owner gave two instructions for an immediate follow-up. D-310 opens the window in borderless fullscreen at the resolution of the display, on every desktop and on the Deck, because the default window of 1152 by 648 was tiny on a 4K screen. D-311 ends the game on Escape and on the controller Start button, for testing, until the escape menu of PR-53 replaces the exit.
- D-312 puts both changes in one PR, PR-60, before PR-15, as an exception to G-10. The owner chose borderless fullscreen, one PR, PR-53, and the Start button, against each recommendation.
- `docs/design.md` and the Phase 2 roadmap gain the PR-60 entry, and the Phase 2 sequence puts PR-60 at item 8. The later items move down by one. `docs/design.md` and the Phase 5 roadmap give PR-53 the escape menu and a sixth exit test.
- `.claude/skills/ste-writing/SKILL.md` gains the art terms of PR-14. Session 131 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `3d8060b`, the squash merge of PR #56. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-14-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed. `bit-identity`: `6ec00e90c1c85cdb` on PR #56.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-13, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. Then a fresh session opens PR-60 from `main`, because D-121 gives one code PR to each session.

### Traps and gotchas

- PR-60 changes every session with a window. The windowed contact sheet run and the M-3 bot session then open in fullscreen, and the engine flag `--windowed` overrides that. A headless run opens no window.
- Until PR-60 merges, the play session has no quit key: press Cmd+Q, or Ctrl+C in the terminal.
- The Phase 2 sequence moved down by one after item 7. An older handoff that names item 8 or a later item means the item one number higher now.
- The trial quota of gitar pauses the automatic pass for the whole period, so each push needs the comment `Gitar review` (D-303). The result arrives as an edit of a dashboard comment, and the summary text can repeat an earlier pass. The `Gitar` check run on the head proves a fresh pass.
- A Codex session can add an entry and skip the archive move. Count the entries before you add one, and move every entry past the tenth.
- The next ids are D-313, OQ-170, F-96, and Session 142.

### Open questions that block progress

None blocks PR-60. OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-60 from `main` per the Phase 2 roadmap and D-310 to D-312. The owner answers OQ-5 and OQ-46 before PR-15.

## Session 140: 2026-09-12, Codex

Author: Codex
Session: re-review PR #56 at effective head `c83360e`.

### What this session did, and why

- Checked the provider gate again. Session 139 identifies Claude Code as the author of the correction, so Codex remains the eligible reviewer under T-4 and D-101.
- Read the response file, the correction diff, the new regression tests, the full review history, and all current PR comments.
- Verified that P2-1 is fixed. The palette and rule reads now convert read failures into contextual `ContextException` values, and the two regression tests pass.
- Updated `docs/reviews/pr-56.md` with the fixed finding, the effective head `c83360e`, the earlier verdict, and the current verdict `Ready for owner merge`.
- Session 130 moved to the archive because this file held eleven sessions with this entry.

### State of the build

- `main` and the merge base are `163742e`. The effective head is `c83360e`. The review and handoff metadata commits remain outside the effective diff.
- The focused texture, contact-sheet, and console tests pass, 57 tests with 0 failures. STE check and det-lint pass locally.
- The local full test and build commands produced no result and were cancelled. Session 139 reports 761 tests with 0 failures and a clean build on the effective head.
- Remote CI, bit identity, smoke, bots, det-lint, asset QA, STE check, night-gate, and the automated pass pass for `c83360e`. The review-gate and evaluate checks failed before this updated review record. Fresh checks for the metadata tip were pending at the review.

### In flight

The updated review record and this handoff entry need a commit and push. The owner can merge after the fresh review-gate and required checks pass.

### Traps and gotchas

- P2-1 keeps its id and its original trigger. Its status is `fixed in c83360e`.
- The effective head is `c83360e`, not the metadata tip, under D-184.
- The local full build can stop without output in this checkout. Remote CI and the author report provide separate evidence.
- The next ids are D-310, OQ-170, F-96, and Session 141.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-44 blocks PR-18. OQ-99 remains open but blocks no work.

### Next concrete action

Commit and push this repeat review and handoff. Verify the fresh review-gate result and the synchronized remote head.

## Session 139: 2026-09-12, Claude Code

Author: Claude Code
Session: answer the PR #56 review, P2-1. Branch `feat/pr-14-texture-generator`.

### What this session did, and why

- Read `docs/reviews/pr-56.md` at the reviewed head `9236744`. P2-1 has full merit: an unreadable palette, rule, or rule directory made `texture-gen` end with an unhandled `UnauthorizedAccessException` and exit code 134, and not with exit code 1 and the path (T-2).
- `ReadFile` and `ReadRules` of `TextureGenCommand` now turn a read failure into a `ContextException` that names the path. The rule directory listing and each rule read share one boundary.
- Two regression tests make the rule or the palette unreadable on every platform, and they assert exit code 1, the file name, and no atlas. Both failed on `9236744`, and both pass on the correction.
- `docs/reviews/pr-56-response.md` records the disposition, the correction, and the checks.
- Sessions 129 and 128 moved to the archive, because the file held twelve entries with this one.
- The trial quota kept the automatic pass of gitar paused. The comment `Gitar review` ran it on demand, and its check run on `c83360e` passed at 19:00 UTC with an approval and no comment (D-250, D-303). The dashboard summary repeats the text of the first pass.

### State of the build

- `main` is at `163742e`. The effective head is `c83360e`, the correction commit above the review commit `cdad3f4`. This entry is in a metadata commit above it (D-184).
- Remote head: `origin/feat/pr-14-texture-generator` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 761 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 26 files. `ste-check`: 0 findings in 15 files. Core, Game, and content did not change.
- On `c83360e`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and the gitar check passed. The `review-gate` check fails, because `docs/reviews/pr-56.md` still gives `Changes required` for `9236744`, and `evaluate` fails with it (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #56: the repeat Codex review of P2-1 at `c83360e`, then the owner merge. The automated pass approved the correction.

### Traps and gotchas

- A test that needs an unreadable file removes every permission on Linux and macOS and holds the file with no share on Windows. It proves the file unreadable first, so a user that permissions do not bind fails the test and never passes it.
- The rule directory has no automated unreadable test, because Windows gives no plain way to make a directory unreadable. The listing shares the catch of the rule test.
- The effective head is the correction commit `c83360e`, not a later metadata commit (D-184).
- Session 138 left eleven entries in the file. Count the entries before you add one, and move every entry past the tenth.
- The next ids are D-310, OQ-170, F-96, and Session 140.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews P2-1 per the repeat review procedure of the `pr-review` skill at the effective head `c83360e` and sets the verdict. The owner then merges.

## Session 138: 2026-09-12, Codex

Author: Codex
Session: review PR #56 at effective head `9236744`.

### What this session did, and why

- Verified that the Session 137 handoff identifies Claude Code as the author of the substantive PR commit. Codex is the eligible reviewer under T-4 and D-101.
- Read the complete PR diff, the texture contracts, the content loader, the atlas consumers, the contact sheet, the tests, the workflow, the roadmap, the decisions, the questions, and every PR comment.
- Found P2-1: an unreadable palette or rule file escapes the command error boundary. A direct unreadable-rule probe produced an unhandled `UnauthorizedAccessException` and exit code 134 instead of exit code 1 with file context.
- Added `docs/reviews/pr-56.md` with the verdict `Changes required` for effective head `9236744`.

### State of the build

- `main` and the merge base are `163742e`. The effective head is `9236744`. The two later handoff commits and the automated-pass note change metadata paths only.
- Focused texture and contact-sheet tests pass, 54 tests with 0 failures. STE check, det-lint, asset QA, texture generation, and bit identity pass locally.
- The local build produced no output and was cancelled. The local full suite did not complete after the smoke portion started. The handoff reports the completed build and 759 passing tests on this head.
- Remote asset QA, bots, compare, det-lint, bit identity, smoke, macOS, Windows, STE check, and night-gate checks pass. Linux CI was pending when checked. Evaluate failed and review-gate skipped before the review record existed.

### In flight

PR #56 needs the unreadable-input correction and its regression tests. The review record and this handoff entry need a commit and push after the owner correction.

### Traps and gotchas

- `texture-gen` catches `ContextException` around atlas construction, but direct file reads in `ReadFile` and `ReadRules` can throw `IOException` or `UnauthorizedAccessException`.
- The effective review head is the feature commit `9236744`, not the metadata tip, under D-184.
- The local dotnet build and the smoke portion of the full suite can stop without output in this checkout. Remote results remain separate evidence.
- The next ids are D-310, OQ-170, F-96, and Session 139.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants but block no work. OQ-44 blocks PR-18. OQ-99 remains open but blocks no work.

### Next concrete action

Correct P2-1 with unreadable palette and rule regression tests, push the correction, and run the automated pass before the repeat Codex review.

## Session 137: 2026-09-12, Claude Code

Author: Claude Code
Session: answer OQ-1 and open PR-14, the texture generator and the palette, as PR #56. Branch `feat/pr-14-texture-generator`.

### What this session did, and why

- PR #55 merged as `163742e` at 14:18 UTC with the `review-override` label. The night of 2026-09-12 ran at `811aa84` and ended with success at 13:04 UTC.
- Sequence item 6 of the Phase 2 roadmap is the owner answer to OQ-1. The session built a preview page of three palettes, each painted on the seven blocks and a miner at game zoom, and asked the owner. The owner chose candidate A, "Lamp and Rock" (D-304).
- Four more questions blocked PR-14, and the owner took each recommendation on the day. OQ-166 asked where the palette and the rules live, because the Core loader stops on unclaimed JSON (D-305). OQ-167 asked where the contact sheet renders (D-306). OQ-168 asked which materials PR-14 ships (D-307). OQ-169 asked how a body face reads its tile, because the model used the 64 px net of a Minecraft skin (D-308).
- `WhatYouCarry.Tools/TextureGen/` is the command `texture-gen`. It reads the palette and ten rules, paints each tile from a xorshift sequence of its seed, and writes an indexed PNG with stored deflate blocks. The tiles equal the preview pixel for pixel, the file has one byte form on every platform, and a test holds the committed atlas equal to the output.
- `WhatYouCarry.Assets/AtlasLayout.cs` holds the tile layout for Game and Tools. `ContentLoader.IsAssetPath` names the `models/` and `textures/` directories, and the three content sources skip both. `Game/World/AtlasFile.cs` loads the atlas at boot, and the placeholder atlas of PR-13 is gone.
- `content/models/player.bbmodel` has the resolution 256, and each face reads its body tile at 32 texels per meter. A script rewrote the face rectangles and changed no other line.
- The Game flag `--contact-sheet <png>` renders the seven blocks and the body from two sides at game zoom. The owner approved the first sheet as drawn, and D-309 records the ten rule values, which closes exit test 5.
- Session 127 moved to the archive, because the file held eleven entries with this one.
- The trial quota paused the automatic pass of gitar on the first push. The comment `Gitar review` ran it on demand, and it approved `2b52074` with no finding (D-250, D-303).

### State of the build

- `main` is at `163742e`, the squash merge of PR #55. This branch holds the feat commit `9236744` and two docs commits of this entry above it. The effective head is `9236744`, because the handoff and the archive are metadata paths (D-184).
- Remote head: `origin/feat/pr-14-texture-generator` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 759 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 26 files. `asset-qa`: 0 findings, 1 model. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`, unchanged.
- The Godot editor build and the windowed contact sheet run end with exit code 0.
- On `2b52074`, CI, bit identity, and smoke passed on the three platforms, and bots, det-lint, asset-qa, STE check, the night gate, and the gitar check passed. The review gate `evaluate` job fails, and `review-gate` skips, until the review record exists (D-251).
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12. It turns red at 13:04 UTC on 2026-09-14 unless a night refreshes it.

### In flight

PR #56: the Codex review. The automated pass approved the head, and no open question binds the PR.

### Traps and gotchas

- A change to the palette or a rule needs `texture-gen --root .` and a commit of `content/textures/atlas.png`, or `CommittedAtlasMatchesTheGenerator` fails. A rule change also needs a new contact sheet for the owner (D-309).
- The contact sheet needs a window. A run with `--headless` ends with exit code 1 by design, and `ContactSheetFailsHeadless` holds that.
- In zsh, `status` is a read-only variable, and a variable that holds a command with its arguments does not split into words. Name the exit code `rc`, and write each tool command in full.
- Godot has a class `AtlasTexture`, so a Game class of that name is ambiguous under `using Godot`. The loader is `AtlasFile`.
- The body tiles are 8, 9, and 10 (D-307). A new block takes the tile of its id, and a new body material takes the next free tile of row 1.
- The next ids are D-310, OQ-170, F-96, and Session 138.

### Open questions that block progress

OQ-5 and OQ-46 block PR-15. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass on PR #56, then a Codex session reviews it per the `pr-review` skill at the effective head `9236744`. After the merge, the owner answers OQ-5 and OQ-46, and a fresh session opens PR-15.

## Session 136: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-57 as PR #54, in the same invocation as Session 134 (D-297). Branch `docs/pr-57-merge-record`.

### What this session did, and why

- The owner merged PR #54 as `811aa84` at 07:17 UTC, with the verdict `Ready for owner merge` for `ecdc95a` in `docs/reviews/pr-54.md`. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit.
- `docs/design.md` marks PR-57 merged in the roadmap entry and in sequence item 10. The Phase 2 roadmap gains the status line of PR-57 and the mark in sequence item 5.
- `docs/questions.md` needs no addendum, because every question that PR-57 raised had its answer before the merge.
- Session 126 moved to the archive, because the file held eleven entries with this one.

### State of the build

- `main` is at `811aa84`, the squash merge of PR #54. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-57-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed. `bit-identity`: `6ec00e90c1c85cdb` on PR #54.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. Exit test 7 of PR-13 waits for the M-3 run on the Deck (OQ-161).

### Traps and gotchas

- The automatic pass of gitar pauses when the trial quota of the period is used, and the comment `Gitar review` runs one on demand (D-303).
- The handoff held eleven entries with this one. Count the entries before you add one, and move every entry past the tenth.
- The next ids are D-304, OQ-166, F-96, and Session 137.

### Open questions that block progress

OQ-1 blocks PR-14 and sequence item 6. OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. The owner then answers OQ-1, the palette, and a fresh session opens PR-14 from `main` per the Phase 2 roadmap.

## Session 135: 2026-09-12, Codex

Author: Codex
Session: review PR #54 at effective head `ecdc95a`. Branch `feat/pr-57-asset-qa-gate`.

### What this session did, and why

- Verified that Claude Code authored the substantive PR commits, so Codex is the eligible reviewer under T-4 and D-101.
- Read the complete diff, the animation and model content contracts, affected callers, tests, workflow, roadmap, decisions, questions, and every PR comment.
- Found no defect. The automated pass finding on duplicate clip and unknown-bone reports is corrected in `0cb62c7` and covered by two regression tests.
- Added `docs/reviews/pr-54.md` with the verdict `Ready for owner merge` for the effective head. The effective head includes the D-303 process commit `ecdc95a`.

### State of the build

- `main` and the merge base are `5848bda`. The effective head is `ecdc95a`. The review metadata tip is `632e281` before this correction commit.
- Focused asset, animation, pose, and overlap tests pass, 80 tests with 0 failures. The local build attempt hung without output and was cancelled.
- Remote build and test, asset QA, bots, bit identity, det-lint, STE check, night gate, and smoke pass. The pre-review evaluate check failed because the review file did not exist, and review-gate skipped for the same reason.

### In flight

The review record and this handoff entry need a commit and push. The fresh evaluate and review-gate checks must pass against the published review record.

### Traps and gotchas

- The effective head is `ecdc95a`, because `.claude/skills/pr-review/SKILL.md` changed in that commit. The review and handoff paths alone are metadata under D-184.
- The local full build did not produce output after several minutes. Remote CI is the build evidence for this review.
- The automated pass was paused before the owner requested the on-demand Gitar review. The on-demand review approved `0cb62c7` after the correction.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind no work. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the review record and handoff entry. Fetch the remote. Confirm that the remote has no ahead count and that the fresh review-gate check passes.

## Session 134: 2026-09-12, Claude Code

Author: Claude Code
Session: open PR-57, the asset QA gate v1, as PR #54. Branch `feat/pr-57-asset-qa-gate`.

### What this session did, and why

- PR #53 merged as `5848bda`. The Phase 2 sequence puts PR-57 next, so the session opened it from `main` per the roadmap and D-149.
- Five questions blocked the exit tests, and the owner answered all five on the day. OQ-45 blocked exit test 1, because the tool checks every keyframe and no format existed. OQ-162 asked where the one model reader lives, because the PR-13 reader used engine vectors in Game and Tools cannot reference Game. OQ-163 asked how an overlay box pairs with its limb box. OQ-164 asked what a clip is. OQ-165 asked what a file name reference is. D-298 to D-302 record the answers.
- The first answer on the clip rule took every pair with zero tolerance, and the second answer, on keyframes in v1, made every bent elbow a clip. The session quoted both, and the owner exempted the pairs of a bone and its parent (D-301).
- `WhatYouCarry.Assets` is the fifth project (D-299). The Blockbench reader moved into it from Game with the Core vector, and `AnimationLoader`, `RotationMatrix`, `ModelPose`, and `BoxOverlap` joined it. Game converts each vector where it builds a node.
- `WhatYouCarry.Tools/AssetQa/` holds the command `asset-qa` and the three checks: `ClipCheck`, `OverlayCheck`, and `FileCaseCheck`. `AssetSet` reads every model, overlay, and animation, and a file that does not load is a finding and not a stop.
- `ContentLoader.ModelDirectory` names the directory that every content source skips (D-298), in Game, in the bot runner, and in the tests.
- `.github/workflows/asset-qa.yml` is the new job, and `CLAUDE.md` and `AGENTS.md` carry the command, the gate line, and the Assets rule.
- Exit tests 1 to 5 pass, with 82 new tests. The command on the checkout reports 0 findings over 1 model.
- The automated pass on `73ba35b` had one comment, and it has merit: a pair of two body boxes counted once per overlay, and an unknown bone in an animation gave one finding per overlay. `0cb62c7` counts a body pair on the pass with no overlay alone and checks the tracks of an animation once, with two regression tests that fail on `73ba35b`. The reply on the thread names the commit. The automatic pass was paused by the trial quota after the push, and the comment `Gitar review` ran one on demand, which approved `0cb62c7` with the one finding resolved (D-250).

### State of the build

- `main` is at `5848bda`, the squash merge of PR #53. This branch holds the feat commit `73ba35b`, the fix commit `0cb62c7`, and the docs commit that records D-303 above it. The effective head is the docs commit, because a new decision moves it.
- Remote head: `origin/feat/pr-57-asset-qa-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 704 tests, 0 failures, with the smoke test on the local Godot build. Core gained one constant and no behavior.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 24 files. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. `asset-qa`: 0 findings, 1 model, 0 overlays, 0 animations.
- The Godot editor build, the headless smoke session, and the headless bot session end with exit code 0. The bot reaches floor 2 of seed 1 at tick 421.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The run list held no later night at 06:22 UTC on 2026-09-12. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

PR #54: the Codex review. The automated pass approved the effective head. No open question binds it.

### Traps and gotchas

- A `.json` file under `content/models/` is an animation, and the Core loader never sees it. A `.json` file in any other unclaimed directory still errors in the Core loader.
- A file that is not JSON gives two findings: one from the loader and one from the file case check. Each check reads the file on its own.
- The euler order is the order of Blockbench: the matrix is Rz times Ry times Rx. The test `RotationOrderIsBlockbenchOrder` pins it, and the pose tests read meters and not file units.
- The clip check poses the body with each overlay alone, never two overlays together, because two pieces for one slot enclose the same limb.
- A texture `path` in a model file is a machine path that Blockbench writes, and the file case check flags a rooted reference. The player model has no texture, and PR-14 assigns the atlas.
- The frame log of a `--fixed-fps` run reads the fixed frame time. M-3 runs without that flag.
- The automatic pass of gitar pauses when the trial quota of the period is used. The comment `Gitar review` on the PR runs one on demand (D-303).
- The next ids are D-304, OQ-166, F-96, and Session 135.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #54 per the `pr-review` skill, at the effective head, which is the docs commit above `0cb62c7`. After the merge, the owner answers OQ-1, and a fresh session opens PR-14.

## Session 133: 2026-09-12, Claude Code

Author: Claude Code
Session: record the merge of PR-13 as PR #52 and the owner answer on documentation PRs (D-297), in the same invocation as Session 131. Branch `docs/pr-13-merge-record`.

### What this session did, and why

- The owner merged PR #52 as `9749581` at 04:48 UTC, with the verdict `Ready for owner merge` for `9085a95` in `docs/reviews/pr-52.md`. CI, smoke, bit identity, bots, det-lint, and STE check passed on the merge commit.
- The session asked where the merge record runs, because D-121 gives one PR per session and Session 131 opened PR #52. The owner answered that a documentation PR needs no new session, and only a code PR does. D-297 records it and revises in part D-121, the count of PRs only.
- `docs/design.md` marks PR-13 merged in the roadmap entry and in sequence item 10, and section 3.14 cites D-297. The Phase 2 roadmap gains the status line of PR-13 and the mark in sequence item 4.
- `docs/questions.md` gains a dated addendum on OQ-159, OQ-160, and OQ-161: the merge came with the three open.
- `CLAUDE.md` and `AGENTS.md` state the session rule with D-297. Sessions 123 and 122 moved to the archive, because the file held eleven entries.

### State of the build

- `main` is at `9749581`, the squash merge of PR #52. This branch holds one docs commit above it.
- Remote head: `origin/docs/pr-13-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. `dotnet test` without the Smoke category: 0 failures. No code changed. `bit-identity`: `6ec00e90c1c85cdb` on PR #52.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. The owner answers OQ-159, OQ-160, and OQ-161 in `docs/decisions.md` as the next ids. Exit test 7 of PR-13 waits for the M-3 run on the Deck.

### Traps and gotchas

- D-297 lets a documentation PR follow the code PR in one invocation. A second code PR still needs a fresh session (D-121).
- The handoff held eleven entries after the Codex review session. Count the entries before you add one, and move every entry past the tenth.
- The M-3 command in `CLAUDE.md` runs without `--headless`, `--write-movie`, and `--fixed-fps`, or the frame log reads a fixed frame time.
- The next ids are D-298, OQ-162, F-96, and Session 134.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. A fresh session then opens PR-57, the asset QA gate v1, from `main` per the Phase 2 roadmap. The owner runs the M-3 command on the Deck when OQ-161 has its answer.

## Session 132: 2026-09-11, Codex

Author: Codex
Session: review PR #52 at effective head `9085a95`.

### What this session did, and why

- Verified the provider gate. Claude Code authored the substantive PR commits, so Codex is the eligible reviewer under T-4 and D-101.
- Read the complete diff, the model file, affected callers, Core bot and simulation contracts, roadmap, decisions, questions, tests, and every PR comment.
- Found no defect. The automated pass finding on frame-log permission errors is corrected in `9085a95`.
- Added `docs/reviews/pr-52.md` with the verdict `Ready for owner merge`.

### State of the build

- The effective head is `9085a95`. The review and handoff commit are metadata only.
- Local build, full test, focused tests, det-lint, STE check, bit identity, Godot editor build, and smoke pass.
- The bot session passes on seed 1 at floor 2 and tick 421. Its local frame log has 1,001 frames and a 99th percentile of 7,402 microseconds.
- Remote build and test, bots, bit identity, det-lint, night gate, STE check, and smoke pass on the reviewed effective head. The review-gate passes after the metadata push. Duplicate platform checks from that push remain pending.

### In flight

PR #52 needs the owner merge after the pending duplicate checks settle. Exit test 7 needs the Steam Deck M-3 run under OQ-161.

### Traps and gotchas

- The review-gate check passes after `docs/reviews/pr-52.md` reaches the PR head.
- The effective head is `9085a95`, not the later metadata commit.
- Visual feel and Deck readability remain for M-3 and Gate 2.

### Open questions that block progress

OQ-161 blocks exit test 7 and M-3. OQ-159 and OQ-160 block nothing.

### Next concrete action

The review and handoff are pushed at `42e5a46`. Verify the pending duplicate checks, then the owner can merge.

## Session 131: 2026-09-11, Claude Code

Author: Claude Code
Session: open PR-13, the model loader, the greedy mesher, and the wall fade shader, as PR #52. Branch `feat/pr-13-model-loader-and-mesher`.

### What this session did, and why

- PR #51 merged as `52ab6ca`. The Phase 2 sequence puts PR-13 next, and no open question blocks it, so the session opened it from `main` per the roadmap, D-291, D-292, D-295, and D-296.
- `Models/BlockbenchLoader.cs` reads the project file that Blockbench 5 writes: the flat `elements` and `groups` lists, and the `outliner` tree of ids. A group is a bone, a cube is a box under its bone, and a locator named after a slot of D-18 is an attachment point. The session read the Blockbench source of the codec on GitHub to confirm the keys. `BoxGeometry` gives each box its six quads relative to its pivot, and `ModelNodes` builds the bone tree for the engine.
- `World/GreedyMesher.cs` emits one mesh per chunk of D-291, with the faces merged over equal block and equal occlusion, and the vertex occlusion of D-81 in the vertex colors. The outside of the grid is rock (D-237), so the edge of the world shows no face and a chunk reads its neighbor through the grid, so no seam shows.
- `World/world.gdshader` fades each fragment inside the capsule from the camera to the player with a screen-door dither, so every chunk stays in the opaque pass (D-292). `WorldMaterial` sets the two ends on every frame, and `PlaceholderAtlas` gives one flat color per block until PR-14.
- `Measure/BotSession.cs` and `Measure/FrameLog.cs` give M-3 its two flags: the greedy descender drives one floor, and the frame log writes one microsecond count per frame with the 99th percentile in the end line (D-295, D-296).
- `content/models/player.bbmodel` is the first body: ten boxes, ten bones, and six locators, at sixteen units per meter. `Main` loads it in place of the box of PR-12, and the chunk meshes replace the flat floor.
- Exit tests 1 to 6 pass, with 38 new tests. Exit test 7 waits for the M-3 run on the Deck (OQ-161). OQ-159 and OQ-160 hold the constants of the loader, the mesher, and the shader, with the recommendation in the code.
- The session checked the render with the movie writer of the engine, because `screencapture` reaches no display from the shell. The walls, the floor, and the ceiling show from inside with the merged faces, and a wall between the camera and the player dissolves in the dither when the radius is large.
- `CLAUDE.md` and `AGENTS.md` gain the bot session command. Session 121 moved to the archive.
- The automated pass approved the head with one comment, and it has merit: a path that the user cannot write raises `UnauthorizedAccessException`, which is not an `IOException`, so the frame log write failure escaped the catch with no error line (T-2). `9085a95` widens the catch, and the reply on the thread names it (D-250).

### State of the build

- `main` is at `52ab6ca`, the squash merge of PR #51. This branch holds the feat commit `9a7fb96` and the fix commit `9085a95` above it, and the effective head is `9085a95`.
- Remote head: `origin/feat/pr-13-model-loader-and-mesher` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 622 tests, 0 failures, with the smoke test on the local Godot build. Core did not change.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 27 files. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`.
- The headless smoke session and the headless bot session with a frame log both end with exit code 0. The bot reaches floor 2 of seed 1 at tick 421.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The run list held no later night at 03:26 UTC on 2026-09-12. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

PR #52: the automated pass, then the Codex review. Exit test 7 is the M-3 run on the Steam Deck, and OQ-161 asks the owner how the build reaches the Deck. The owner answers OQ-159, OQ-160, and OQ-161 in `docs/decisions.md` as the next ids.

### Traps and gotchas

- `screencapture` fails in this shell with "could not create image from display", and a windowed run never quits. The engine flag `--write-movie <dir>/frame.png` with `--quit-after N` writes N frames offscreen and quits, and `Read` shows a frame.
- The frame log of a movie run reads the fixed frame time and not the real one. M-3 runs the bot session with no `--write-movie` and no `--fixed-fps`.
- The Game string rule flags a literal in `AddContext`, so every context field name in Game is a `const`.
- The Godot editor build writes a `.uid` file next to every new script, and it wrote one for `EnginePoll.cs` and `IInputPoll.cs` of PR-12 too. Commit them, or every session sees them as new.
- The outside of the grid is rock for the occlusion too, so the corners of a flat test floor darken at the grid edge. A test reads an interior vertex, and the darkest level needs two solid edge cells.
- The model faces read the atlas rows that no block owns, so the placeholder atlas fills its base with one gray. PR-14 assigns the model tiles.
- The next ids are D-297, OQ-162, F-96, and Session 132.

### Open questions that block progress

OQ-161 blocks exit test 7 of PR-13 and M-3. OQ-159 and OQ-160 bind constants and block nothing. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass on PR #52, then a Codex session reviews it per the `pr-review` skill. The owner answers OQ-161 and runs the M-3 command of `CLAUDE.md` on the Deck for exit test 7.

## Session 130: 2026-09-11, Claude Code

Author: Claude Code
Session: record the owner answers to OQ-15, OQ-43, OQ-49, OQ-50, OQ-157, and OQ-158 (D-291 to D-296). Branch `docs/oq-43-49-157-158-record`.

### What this session did, and why

- PR #50 merged as `cfafad3` at 23:04 UTC. CI, bit identity, det-lint, STE check, smoke, and bots passed on that commit.
- Phase 2 sequence item 3 is an owner answer, so the session put OQ-43, OQ-49, OQ-157, and OQ-158 to the owner in one batch. The owner chose each recommendation.
- D-291 resolves OQ-43: one mesh per chunk of 16 by 32 by 16 blocks, and a budget of 64 world meshes plus one per entity. D-292 resolves OQ-49: the wall fade in the world shader. It revises in part D-88, the effect note only.
- D-293 resolves OQ-157, and D-294 resolves OQ-158. Both keep the values that PR #49 merged, so no code and no workflow change follows. D-294 is the dependency entry of `actions/cache` (G-16).
- Exit test 7 of PR-13 needs M-3 on a Deck. The sequence put the OQ-50 answer at item 19, after PR-13. The session put OQ-50 and OQ-15 to the owner too.
- D-296 resolves OQ-50: the owner owns a Steam Deck OLED, and M-3 measures on it. D-295 resolves OQ-15 against the recommendation: 90 frames per second, the top refresh rate of the OLED panel, with 60 as the fallback.
- OQ-44 gains a dated note, because its recommendation reads "two frames at 60". The design doc, the Phase 2 roadmap, and the Phase 5 roadmap cite the six decisions. Session 120 moved to the archive.

### State of the build

- `main` is at `cfafad3`, the squash merge of PR #50. This branch holds one docs commit above it.
- Remote head: `origin/docs/oq-43-49-157-158-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 584 tests, 0 failures, with the smoke test on the local Godot build. No code changed.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`.
- The night gate reads the success of run 34692777858 at `811aa84`, ended 13:04 UTC on 2026-09-12.

### In flight

PR #51: docs alone. The automated pass approved the head with no code finding. The owner merge is next. Then PR-13 opens from `main` per the Phase 2 roadmap.

### Traps and gotchas

- A maximum floor fills the world budget of D-291 exactly: 64 chunks and 64 world meshes.
- D-295 sets 90 frames per second. The M-3 row of PR-13 reads the 99th percentile frame time against that bound.
- The next ids are D-297, OQ-159, F-96, and Session 131.

### Open questions that block progress

None blocks PR-13. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. A session then opens PR-13 from `main` per the Phase 2 roadmap.

## Session 129: 2026-09-11, Claude Code

Author: Claude Code
Session: record the merge of PR-12 as PR #49 and bring every document up to date, in the same run as Sessions 125 and 127. Branch `docs/pr-12-merge-record`.

### What this session did, and why

- The owner merged PR #49 as `9313358` at 20:41 UTC, with the verdict `Ready for owner merge` for `066ce0e`. OQ-157 and OQ-158 stayed open at the merge, so `main` holds the two sensitivity constants of the recommendation and the `actions/cache` step with no decision entry yet.
- `docs/design.md` marks PR-12 merged in the roadmap entry and in sequence item 10. The Phase 2 roadmap gains the status line of PR-12, the mark in sequence item 2, and the new state of OQ-157 and OQ-158 in section 6.
- `docs/questions.md` gains a dated addendum on OQ-157 and on OQ-158: the merge came with both open.
- `CLAUDE.md` and `AGENTS.md` gain the `smoke` job in the PR gate.
- Session 119 moved to the archive.

### State of the build

- `main` is at `9313358`, the squash merge of PR #49. On that commit the smoke workflow, bit identity, and CI on the three platforms passed.
- Remote head: `origin/docs/pr-12-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. `dotnet test`: 584 tests, 0 failures. No code changed. `bit-identity`: `6ec00e90c1c85cdb`.
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. The owner answers OQ-157 and OQ-158 in `docs/decisions.md` as the next two ids. If the OQ-157 answer differs from the recommendation, a PR changes `MouseHundredthsPerPixel` and `StickHundredthsPerTick` in `IntentBuilder.cs` and the tests that read them.

### Traps and gotchas

- `dotnet test` with no filter runs `SmokeSessionPasses`, which starts the Godot build at the path that `CLAUDE.md` names, or the one that `WYC_GODOT` names. The three CI jobs filter the Smoke category out, and the smoke workflow runs it.
- A `dotnet test --no-build` after a build of the Game project alone reads a stale copy of the Game assembly in the test output. Build the solution before a test of a Game change.
- The full local suite takes about three and a half minutes on this Mac. A run with a two-minute timeout reads as a stall.
- The Windows CI suite took 9 min 27 s on PR #49, near the ten-minute bound of the M-1 procedure (OQ-145).
- The engine reports the two shift keys as one key and the two control keys as one key. Block, throwable, reload, satchel, and amulet from D-289 have no button bit yet.
- The next ids are D-291, OQ-159, F-96, and Session 130.

### Open questions that block progress

OQ-43 and OQ-49 block PR-13. OQ-157 and OQ-158 are open on `main` and block nothing, and OQ-158 needs its decision entry under G-16. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR with the `review-override` label. The owner answers OQ-157 and OQ-158, then OQ-43 and OQ-49. A session then opens PR-13 from `main` per the Phase 2 roadmap: the Blockbench loader and the greedy mesher.

## Session 128: 2026-09-11, Codex

Author: Codex
Session: re-review PR #49 at effective head `066ce0e`.

### What this session did, and why

- Verified the provider gate. The substantive PR work and the P2-1 correction came from Claude Code, so Codex is the eligible reviewer under T-4 and D-101.
- Read the prior review, the response, the complete correction diff, the roadmap, the affected input callers, the tests, and every PR comment.
- P2-1 has full merit at `91d1b6f`. The correction at `066ce0e` tracks the latest look event instead of the held stick state.
- The regression test fails on the old behavior and passes on the correction. No finding remains at the new effective head.
- Updated `docs/reviews/pr-49.md` with the earlier verdict and the current `Ready for owner merge` verdict.

### State of the build

- `main` and the merge base are `e1cf847`. The effective implementation head is `066ce0e`. Review and handoff metadata commits remain outside the effective diff.
- The focused reader suite passes 8 tests, and the focused smoke test passes. The author reports 584 full-suite tests with 0 failures, and remote Linux, Windows, and macOS CI pass.
- Local det-lint, STE check, bit identity, Godot editor build, and the smoke session pass. The local full build and full test commands did not complete in this execution context after more than 60 seconds without output.

### In flight

The repeat review record needs a commit and push. The review-gate and evaluate checks must refresh against the new `Ready for owner merge` verdict. OQ-158 still blocks the merge.

### Traps and gotchas

- The effective head is `066ce0e`, not a later metadata commit.
- A held stick keeps controller look state until a new mouse or stick event names a device.
- A full local test command may stall after the Godot project build. Use the focused tests and remote full-suite evidence.

### Open questions that block progress

OQ-158 blocks PR #49 under G-16. OQ-157 remains open and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open and blocks nothing.

### Next concrete action

Commit and push this repeat review and handoff. Then verify the remote head and the refreshed review-gate result.

## Session 127: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #49 review, P2-1 (D-243), in the same run as Session 125. Branch `feat/pr-12-game-skeleton`.

### What this session did, and why

- Read `docs/reviews/pr-49.md` at the reviewed head `91d1b6f`. P2-1 has full merit: `Read` set the controller flag from the held stick on each tick, so a later mouse event lost the look device.
- The look device now follows the latest look event (D-243). `Main` hands the mouse motion and the joypad motion events to the reader as plain values, `AddLookStickMotion` names the controller on a look axis event past the dead zone, and `Read` reads the deflection alone.
- The reader polls through `IInputPoll`: `EnginePoll` over the engine, and a test poll in `InputReaderTests`, the two callers of D-111. Eight reader tests pin the device transitions and every D-289 binding with a bit. The regression test fails on the old line, 1 failed and 7 passed, and passes on the correction.
- `docs/reviews/pr-49-response.md` records the disposition, the correction, and the regression check.
- Sessions 117 and 116 moved to the archive, because the file held eleven entries.

### State of the build

- `main` is at `e1cf847`. The effective head is the correction commit, the one commit above the review commits `42bd504`, `307f025`, and `735f7fd`.
- Remote head: `origin/feat/pr-12-game-skeleton` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 584 tests, 0 failures, with the smoke test on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 11 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. Core did not change.

### In flight

PR #49 needs a repeat Codex review of the correction, the owner answers to OQ-157 and OQ-158, and the owner merge. The automated pass runs again on the push.

### Traps and gotchas

- A `dotnet test --no-build` after a build of the Game project alone reads the stale copy of the Game assembly in the test output. Build the solution before a test of a Game change.
- A stick moved past the dead zone and released keeps the look with the controller until the mouse moves, because the release event is inside the dead zone.
- The effective head is the correction commit, not a later metadata commit.
- The next ids are D-291, OQ-159, F-96, and Session 128.

### Open questions that block progress

OQ-158 blocks the merge of this PR (G-16). OQ-157 binds the two constants and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass, then a Codex session reviews the correction per the repeat review procedure of the `pr-review` skill and sets the verdict for the new effective head. The owner answers OQ-157 and OQ-158, and merges.

## Session 126: 2026-09-11, Codex

Author: Codex
Session: review PR #49 at effective head `91d1b6f`.

### What this session did, and why

- Verified the provider gate. The substantive PR work came from Claude Code, so Codex is the eligible reviewer under T-4 and D-101.
- Read the handoff, the project rules, the design, the relevant decisions and questions, the Phase 2 roadmap, the complete PR diff, the affected Core callers, and every PR comment.
- Found P2-1. A held controller stick overrides a later mouse look event, so the input frame can set the controller aim bit for the wrong device.
- Added `docs/reviews/pr-49.md` with a `Changes required` verdict for the effective head.

### State of the build

- `main` and the merge base are `e1cf847`. The effective implementation head is `91d1b6f`. The current branch tip before this session is `dfdc68e`, which holds metadata only.
- The focused Game, input, render, smoke, and shape tests pass with 41 tests. The remote three-platform CI and smoke checks pass on the effective head. Gitar passes, while `evaluate` and `review-gate` fail for the recorded `Changes required` verdict.
- Local det-lint, STE check, bit identity, Godot editor build, and the headless smoke session pass. Local `dotnet build` did not complete after more than 80 seconds without output.

### In flight

PR #49 needs the P2-1 correction and a regression test. OQ-158 also needs an owner decision before merge. The review-gate check must refresh after the review record reaches the PR head.

### Traps and gotchas

- The effective head is `91d1b6f`, not the metadata tip.
- `InputReader.Read` checks the current stick after mouse input has set the flag false. A held stick can therefore override the last mouse event.
- The first `dotnet build` attempt produced no output for more than 80 seconds. Remote CI gives the build evidence for this head.

### Open questions that block progress

OQ-158 blocks PR #49 under G-16. OQ-157 remains open and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open and blocks nothing.

### Next concrete action

The author corrects the input-device state and adds the regression test. Then the author pushes the fix, and a later review checks the new effective head.

## Session 125: 2026-09-11, Claude Code

Author: Claude Code
Session: PR-12, the Game skeleton and input (D-63, D-73, D-77, D-114, D-149, D-289). Branch `feat/pr-12-game-skeleton`.

### What this session did, and why

- PR #48 merged as `e1cf847` at 17:38 UTC, and Gate 1 is signed (D-288), so PR-12 opened from `main` per the Phase 2 roadmap.
- `WhatYouCarry.Game/Main.cs` is the root node, and `Main.tscn` is the one text scene (D-63). It steps one `SimulationLoop` per physics frame at 60 Hz, and `project.godot` pins the physics tick at 60. It draws the player box and the camera between the last two ticks with the interpolation fraction of the engine (D-73, D-245).
- `Input/IntentBuilder.cs` holds the pure logic: the linear mouse, the cubic stick curve with the 15 percent dead zone (D-289), a carry of the fraction between ticks, and the controller aim bit from the device of the look (D-243). `Input/InputReader.cs` reads the engine once per tick with the bindings of D-289 for the five actions that have a bit: jump, sprint, dodge, attack, and interact.
- `Smoke/SmokeSession.cs` is the script of one thousand ticks in four parts. `Main` runs it on `--smoke` and quits with exit code 0 only when the print sink counted no error line (D-114). The engine ends the session in under one second with `--fixed-fps 60`. A reserved bit at tick 500, set by hand one time, gave exit code 1 and an error line with the tick.
- `Content/DirectoryContentSource.cs` reads the content directory of the checkout, next to the project directory (D-219). `Logging/PrintLogSink.cs` prints each line and counts the error lines (D-211).
- `.github/workflows/smoke.yml` runs the one test of the Smoke category on the three platforms with the pinned Godot binary from `actions/cache`. `ci.yml` leaves that category out. The test project references the Game project for the pure logic.
- Filed OQ-157, the two sensitivity numbers, and OQ-158, the cache action as a dependency (G-16). The code holds the recommendation of OQ-157 as two named constants.
- PR #49 opened at the effective head `91d1b6f`. The automated pass approved the head with no code finding. Its one comment reads the red review-gate before a review record exists, which D-251 designs, and the reply on the PR names that. No commit answered it.
- Session 115 moved to the archive.

### State of the build

- `main` is at `e1cf847`. This branch holds the work commit `91d1b6f` above it, and the handoff commits above that (D-184).
- PR #49: CI green on the three platforms with 575 tests, the suite minus the smoke test. Bit identity, det-lint, ste-check, night-gate, bots, and smoke are green. The smoke workflow passed on its first run, with a cache miss and a download on each platform. The Windows suite took 9 min 27 s, near the ten-minute bound of the M-1 procedure (OQ-145).
- Remote head: `origin/feat/pr-12-game-skeleton` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 576 tests, 0 failures, with the smoke test on the local Godot build. The full suite takes about four minutes on this Mac.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 9 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- The Godot editor build check passes. It wrote `Main.cs.uid`, and the checkout keeps that file.

### In flight

This PR needs a Codex review, the owner answers to OQ-157 and OQ-158, and the owner merge. The automated pass is complete. The first run of the engine in CI passed on the three hosted and self-hosted runners.

### Traps and gotchas

- `dotnet test` with no filter runs `SmokeSessionPasses`, which starts the Godot build at the path that `CLAUDE.md` names, or the one that `WYC_GODOT` names. The three CI jobs filter the Smoke category out.
- The engine reports the two shift keys as one key and the two control keys as one key, so the right keys sprint and dodge too.
- Block, throwable, reload, satchel, and amulet from D-289 have no button bit yet. The PR that assigns each bit adds the binding.
- The namespace `WhatYouCarry.Game.Input` hides the engine class `Input`, so the reader writes `Godot.Input`. `Button` needs the alias `CoreButton` next to the engine type of that name.
- The Windows smoke job names the console executable of Godot, because the window executable writes nothing to standard output.
- The next ids are D-291, OQ-159, F-96, and Session 126.

### Open questions that block progress

OQ-158 blocks the merge of this PR (G-16). OQ-157 binds the two constants and blocks nothing. OQ-43 and OQ-49 block PR-13. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #49 at the effective head `91d1b6f` per the `pr-review` skill: the Core boundary, the determinism of the builder, the input and CI boundaries, and the presentation. The owner answers OQ-157 and OQ-158 in `docs/decisions.md`, and a commit sets the two constants if the answer differs from the recommendation. After the merge, the owner answers OQ-43 and OQ-49, and a session opens PR-13.

## Session 124: 2026-09-11, Codex

Author: Codex
Session: re-review PR #48 at effective head `4786cfa`. Branch `chore/night-back-to-0807`.

### What this session did, and why

- Verified that the effective head remains `4786cfa`. The later commits change only review and handoff metadata under D-184.
- Read the prior review, the complete implementation diff, all PR comments, D-286, D-288, D-290, F-94, F-95, the Phase 1 roadmap, and the changed workflow and shape test.
- No finding remains. The required Linux, Windows, and macOS CI jobs, three-platform bit identity, bots, det-lint, STE check, night-gate, and Gitar pass.
- Updated `docs/reviews/pr-48.md` with the earlier `Blocked` verdict and the current `Ready for owner merge` verdict.

### State of the build

- `main` and the merge base are `b700296`. The effective implementation head is `4786cfa`. The current metadata tip is `ccabf74` before this re-review commit.
- The prior local build and focused suite passed. The local full test run did not complete after 120 seconds with no output. Remote platform CI passed the full suite.
- Evaluate and review-gate failed only because the prior review record held `Blocked`. They must refresh after this re-review record reaches the PR head.

### In flight

The re-review record and this handoff entry need a commit and push. The fresh review-gate result must pass against `Ready for owner merge`.

### Traps and gotchas

- The effective head is `4786cfa`, not the metadata tip.
- The prior blocked result was correct while Windows CI was pending. The current verdict can approve only after all required platform checks pass.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the re-review record and handoff entry. Fetch the remote. Confirm that the remote has no ahead count and that the fresh review-gate result passes.

## Session 123: 2026-09-11, Codex

Author: Codex
Session: review PR #48 at effective head `4786cfa`. Branch `chore/night-back-to-0807`.

### What this session did, and why

- Verified that the substantive PR change came from Claude Code in Session 122. Codex is the eligible reviewer under T-4 and D-101.
- Verified the base, merge base, effective head, complete diff, D-286, D-288, D-290, F-94, F-95, the Phase 1 roadmap, the workflow, the shape test, the handoff files, and every PR comment.
- Found no defect. The workflow returns to `7 8 * * *`, keeps `workflow_dispatch`, and records the late scheduled run. The shape test asserts the cron, the hand trigger, the return phrase, and both finding ids.
- The local focused suite, build, det-lint, STE check, and bit identity pass. The local full suite did not complete after 120 seconds with no output. Remote Linux and macOS CI pass, and Windows CI remains pending.
- Added `docs/reviews/pr-48.md` with a `Blocked` verdict until the pending required check and the fresh review-gate result complete.

### State of the build

- `main` and the merge base are `b700296`. The effective implementation head is `4786cfa`. The handoff commit `80eca7a` and the review commit are metadata under D-184.
- The remote branch matched the local branch at `80eca7a` before this review record was added.
- The review record names the pending Windows CI job and the expected pre-review evaluate and review-gate results.

### In flight

The review record and this handoff entry need a commit and push. The Windows CI job must pass. The review-gate job must refresh against this record.

### Traps and gotchas

- The effective head is `4786cfa`, not the metadata tip.
- The review remains blocked until the required Windows CI result passes and the fresh review-gate result reads this record.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the review record and handoff entry. Fetch the remote. Confirm that the remote has no ahead count and that the fresh review-gate result passes.

## Session 122: 2026-09-11, Claude Code

Author: Claude Code
Session: return the night to 08:07 UTC (D-286, D-288, D-290), in the same run as Sessions 116, 118, 120, and 121. Branch `chore/night-back-to-0807`.

### What this session did, and why

- The owner merged PR #47 as `b700296` at 17:02 UTC. Gate 1 is signed (D-288), and Phase 2 can start.
- The cron of `night.yml` reads `7 8 * * *` again, which is 02:07 Central Standard Time. The comment names D-288 and corrects the text of PR #46: the 08:07 UTC run of 2026-09-11 started 4 h 40 min late, at 12:47 UTC (F-95).
- `NightWorkflowRunsAtTwoCentralStandardTime` replaces `NightWorkflowRunsAtTheScheduleTestTime`. It asserts the line, the hand trigger, the phrase `08:07 UTC, which is 02:07 Central Standard Time`, and both finding ids. It fails on the `21 17 * * *` workflow of `main`.
- No new decision: D-286 bound the return to the first scheduled pass, D-288 holds the words of the owner, and D-290 sends the return through this PR with a Codex review.
- Session 112 moved to the archive.

### State of the build

- `main` is at `b700296`, the squash merge of PR #47. The cron on `main` reads `21 17 * * *` until this PR merges. This branch holds the work commit above it, and this entry above that.
- Remote head: `origin/chore/night-back-to-0807` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR needs the automated pass, a Codex review, and the owner merge before 08:07 UTC on 2026-09-12. The 17:21 UTC scheduled run of 2026-09-11 stays (D-290). It can start hours late, and it holds the Mac runner for about an hour.

### Traps and gotchas

- A merge after 08:07 UTC on 2026-09-12 leaves the night of that day at 17:21 UTC.
- A schedule run here can start hours after its cron. Do not read a miss from one hour of silence (F-95).
- D-286 turned the workflow off and on for the test slot alone. This return has no such step.
- The next ids are D-291, OQ-157, F-96, and Session 123.

### Open questions that block progress

None blocks PR-12. OQ-43 and OQ-49 block PR-13. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, and the shape test. The owner merges it before 08:07 UTC on 2026-09-12. After that, a session opens PR-12 from `main` per the Phase 2 roadmap and D-289.

## Session 121: 2026-09-11, Claude Code

Author: Claude Code
Session: record the Gate 1 sign-off, the M-2 table, and the OQ-47 bindings (D-288, D-289, D-290), in the same run as Sessions 116, 118, and 120. Branch `docs/gate-1-record`.

### What this session did, and why

- The owner merged PR #46 as `b1399fc` at 16:43 UTC. The reset turned Night off and on at 16:44 UTC, and the workflow record read active with a new `updated_at`.
- A watch then found the first scheduled night: run 34600758086, the 08:07 UTC cron on `095ce5e`, created at 12:47:32 UTC, 4 h 40 min after the cron. It passed in 58 minutes, and the record on `night-results` reads success at 13:45 UTC. The 09:00 UTC deadline of Session 116 read that late run as a miss, and no check ran again until 16:44 UTC.
- The owner signed Gate 1 (D-288) and asked to move all scheduled runs back to 08:07 UTC. The M-2 table holds seven rows and reads complete. F-94, F-95, and OQ-155 gain dated refutations.
- D-289 resolves OQ-47: the recommendation, with sprint on Left Shift and dodge on Left Control. The owner first wrote "slide" for Control and corrected it to dodge, so D-27 stands.
- D-290 resolves OQ-156: this run opens three PRs, PR #46, this record, and the return to 08:07 UTC with a Codex review. The 17:21 UTC run of today stays.
- Session 111 moved to the archive.

### State of the build

- `main` is at `b1399fc`, the squash merge of PR #46. The cron on `main` reads `21 17 * * *`. This branch holds one docs commit above it.
- Remote head: `origin/docs/gate-1-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `ste-check`: 0 findings in 15 files. No code changed. `bit-identity`: `6ec00e90c1c85cdb`.
- The record on `night-results` is the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. The night gate turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it.

### In flight

This PR: docs alone, with the `review-override` label after the automated pass. Then the return PR: the cron back to `7 8 * * *`, the shape test, and the workflow comment, from `main` after this PR merges, with a Codex review, merged before 08:07 UTC on 2026-09-12. The 17:21 UTC scheduled run of today stays, and it can start hours late.

### Traps and gotchas

- A schedule run here can start hours after its cron. Check `gh run list --workflow=night.yml --event schedule` again for several hours before a miss, and filter a watch by `createdAt`.
- The 17:21 UTC run holds the Mac runner for about an hour when it starts, and macOS PR jobs wait behind it.
- The return PR must merge before 08:07 UTC on 2026-09-12, or the night of that day runs at 17:21 UTC again.
- The next ids are D-291, OQ-157, F-96, and Session 122.

### Open questions that block progress

None blocks PR-12 once this PR merges. OQ-43 and OQ-49 block PR-13, the item after it. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR. This run then opens the return PR from `main`: `7 8 * * *` in `night.yml`, the shape test back on the 08:07 UTC line, the workflow comment, F-95 at ✅, and a handoff entry, with a Codex review. After both merge, a session opens PR-12 from `main` per the Phase 2 roadmap and D-289.

## Session 120: 2026-09-11, Claude Code

Author: Claude Code
Session: move the PR #46 test slot to 17:21 UTC with no repeat review (D-286, D-287), in the same run as Sessions 116 and 118. Branch `chore/night-schedule-reset`.

### What this session did, and why

- The 15:21 UTC slot passed before a merge. The owner moved the test to 17:21 UTC, which is 12:21 Central Daylight Time, and waived the Codex repeat review of the swap.
- The swap commit changes the cron to `21 17 * * *`, the matching strings of `NightWorkflowRunsAtTheScheduleTestTime`, and the times in D-286, OQ-154, OQ-155, F-95, and the Phase 1 roadmap. The shape test fails on the 15:21 UTC workflow.
- The `review-override` label cannot turn the gate green here, because the gate fails the label on a PR that changes a workflow or a test (D-190). D-287 records the owner waiver: the author sets the head field of `docs/reviews/pr-46.md` to the swap commit, with a dated note, one time, as D-282 did for PR #39.
- Session 110 moved to the archive.

### State of the build

- `main` is at `095ce5e`. The effective head is the swap commit. The commit above it holds the head correction, this entry, and the archive move, all metadata (D-184).
- Remote head: `origin/chore/night-schedule-reset` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. The repository shape suite: 14 tests, 0 failures. `ste-check`: 0 findings in 15 files. The full suite ran 535 tests with 0 failures on `862fd4c`, and CI runs it on this head.

### In flight

This PR waits for the checks and the automated pass on the new head, then the owner merge before 17:21 UTC. After the merge, `gh workflow disable night.yml` and then `gh workflow enable night.yml` run, and a watch reads the 17:21 UTC night.

### Traps and gotchas

- A merge or an enable after 17:21 UTC moves the first test to 17:21 UTC on 2026-09-12 (D-286).
- If no run of the schedule event exists by 18:15 UTC, that is a third miss. Stop and ask the owner.
- The handoff entries of Sessions 116 to 119 and the review record name 15:21 UTC. They are dated records, and D-286 names the move.
- The next ids are D-288, OQ-156, F-96, and Session 121. The Gate 1 sign-off is D-288 now.

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #46 before 17:21 UTC. Then the disable and the enable run, and a watch reads the 17:21 UTC night. When it passes, the Gate 1 record follows the edit list of Session 115, with D-288 for the sign-off, the row of the 17:21 UTC night, and F-95 at ✅. A PR returns the cron to 08:07 UTC.

## Session 119: 2026-09-11, Codex

Author: Codex
Session: repeat review PR #46 at effective head `eebb605`. Branch `chore/night-schedule-reset`.

### What this session did, and why

- Read `docs/reviews/pr-46-response.md` and checked the provider gate again. The substantive correction is from Claude Code, so Codex remains eligible.
- Verified the new effective head `eebb605`, the diff since `862fd4c`, the correction trigger, the handoff archive rotation, the workflow, and every PR comment.
- P2-1 is fixed. The shape test now asserts `08:07 UTC, which is 02:07 Central Standard Time`. The prior `09:07 UTC` trigger and a removed return phrase fail the test, and the branch suite passes.
- The automated pass approved the correction and reported the handoff rotation issue. Session 118 moved Sessions 108 and 107 to the archive, and the review verified ten current handoff entries.
- Updated `docs/reviews/pr-46.md` with the fixed finding and the verdict `Ready for owner merge` for `eebb605`.

### State of the build

- `main` and the merge base are `095ce5e`. The effective implementation head is `eebb605`. The review metadata tip is this commit after publication.
- `dotnet build`: 0 warnings, 0 errors. The repository-shape suite passes 14 tests, 0 failures, and 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. The Godot 4.7.2 headless build passes.
- Remote CI, bots, bit identity on all platforms, night-gate, STE check, det-lint, and Gitar pass at `eebb605`. The evaluate and review-gate checks failed because the prior review record still held `862fd4c` and `Changes required`. They must refresh after this review record is pushed.

### In flight

The repeat-review record and this handoff entry need a commit and push. After the fresh review-gate result passes, the owner can merge before the schedule-test deadline or wait for the next slot.

### Traps and gotchas

- The effective head is `eebb605`, not this metadata tip. It is the test correction commit under D-184.
- The review-gate result that reads the old record is stale by design. Do not treat it as a product failure.
- OQ-154 still blocks Phase 2 until the first scheduled night passes and Gate 1 is signed.

### Open questions that block progress

OQ-154 blocks Phase 2. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the updated review record and this entry. Then verify the fresh review-gate result and the remote head.

## Session 118: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #46 review, in the same run as Session 116. Branch `chore/night-schedule-reset`.

### What this session did, and why

- Read P2-1 in `docs/reviews/pr-46.md`. Full merit: the shape test asserted `02:07 Central Standard Time` and not the 08:07 UTC return time, while the PR description and Session 116 said that it did. A workflow comment with the return time changed to 09:07 UTC passed the test.
- The review asked for an assertion on `08:07 UTC`. That text alone passes on the same trigger, because the F-95 sentence of the comment holds `08:07 UTC` too. The test asserts `08:07 UTC, which is 02:07 Central Standard Time` instead. The trigger fails it, and so does a comment with the return time removed.
- `docs/reviews/pr-46-response.md` records the disposition. No new id.
- The automated pass on the review commit left one comment, with merit: Session 117 entered the handoff without an archive move, so the file held 11 entries (D-146). Sessions 108 and 107 moved to the archive with this entry, and the file holds the 10 newest entries. The reply on the thread names this commit.
- This run wrote Session 116 before the review. Session 117 came above it while the run continued, so this entry is a new one at the top, and Session 116 stays as the review read it.

### State of the build

- `main` is at `095ce5e`. This branch holds the work commit `862fd4c`, the Session 116 entry `d8ee1dd`, the review commit `c76d54a`, and the correction that holds this entry above them.
- Remote head: `origin/chore/night-schedule-reset` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. The repository shape suite: 14 tests, 0 failures. `ste-check`: 0 findings in 15 files. The full suite ran 535 tests with 0 failures on `862fd4c`, and CI runs it on this head.
- `det-lint`: 0 findings. `bit-identity`: `6ec00e90c1c85cdb`. Core did not change.

### In flight

This PR waits for the automated pass on the correction, then a Codex repeat review at the effective head, which is the correction commit. The owner merge must land before 15:21 UTC for the test to run today, and the disable and the enable follow the merge.

### Traps and gotchas

- The effective head is the correction commit, because it changes a test. The review record names `862fd4c`, and the repeat review updates the head and the verdict together (D-269).
- A merge or an enable after 15:21 UTC moves the first test to 15:21 UTC on 2026-09-12 (D-286).
- The traps of Session 116 stand: the third-miss deadline at 16:15 UTC, the two PRs after the pass, and the next ids D-287, OQ-156, and F-96. The next session number is 119.

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure at the effective head, verifies P2-1 against its trigger and the regression case, and updates `docs/reviews/pr-46.md` with the status of P2-1 and a new verdict.

## Session 117: 2026-09-11, Codex

Author: Codex
Session: review PR #46 at effective head `862fd4c`. Branch `chore/night-schedule-reset`.

### What this session did, and why

- Verified the provider gate. Session 116 identifies Claude Code as the author of the substantive PR-46 change. Codex is the eligible reviewer.
- Verified the base, merge base, effective head, complete diff, D-285, D-286, F-94, F-95, the Phase 1 roadmap, the workflow, the shape test, the registers, the handoff files, and every PR comment.
- The cron reads `21 15 * * *`, and the workflow comment records the temporary test and the return to 08:07 UTC.
- Found P2-1: the shape test checks `02:07 Central Standard Time` but does not assert the required `08:07 UTC` return text. The focused suite passes 14 tests, but the missing assertion leaves the return-time contract unguarded.
- Wrote `docs/reviews/pr-46.md` with the verdict `Changes required` for `862fd4c`.

### State of the build

- `main` and the merge base are `095ce5e`. The effective implementation head is `862fd4c`. The current metadata tip is `d8ee1dd` before this review commit.
- `dotnet build`: 0 warnings, 0 errors. The focused shape suite passes 14 tests, 0 failures, and 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. The Godot 4.7.2 headless build passes.
- The local full test run did not complete after about 90 seconds with no output. Remote Linux and Windows CI, macOS bit identity, and several other checks were still pending when observed. The review-gate failure is the expected missing-record state until this review is pushed.

### In flight

The review record and this handoff entry need a commit and push. The author must add the direct `08:07 UTC` assertion, rerun the focused suite, and request a repeat review at the new effective head.

### Traps and gotchas

- The effective head is `862fd4c`, not the metadata tip. The first commit changes the workflow, test, and registers. The second commit changes only metadata under D-184.
- `git fetch origin` could not update `.git/FETCH_HEAD` because the execution context denied access. The local branch matched `origin/chore/night-schedule-reset` at `d8ee1dd` before this review commit.
- The owner must merge only after the P2-1 correction, a repeat review, the fresh review-gate result, and all required platform checks pass.

### Open questions that block progress

OQ-154 blocks Phase 2 until the first scheduled night passes. OQ-47 blocks PR-12. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit and push the review record and this entry. Then wait for the author correction and perform the repeat review.

## Session 116: 2026-09-11, Claude Code

Author: Claude Code
Session: listen for the first scheduled night, then move the night to one schedule test at 15:21 UTC (D-286). Branch `chore/night-schedule-reset`.

### What this session did, and why

- A background watch read the 08:07 UTC night of 2026-09-11. No run of the schedule event existed at 09:00 UTC. The runner was online and idle from 07:15 UTC, the `7 8 * * *` line stood on `main` from 06:27 UTC, the workflow read active, and Actions was on. F-95 records it, and F-94 gains a dated note: the minute was not the whole cause.
- Asked the owner, and D-286 records the answer (OQ-155). The cron moves to one test slot, and after the merge `gh workflow disable` and `gh workflow enable` turn the workflow off and on. Gate 1 signs after the first scheduled night passes, and D-283 stands. After that pass, a PR returns the cron to 08:07 UTC.
- The owner first named 11:21 UTC. At 11:16 UTC that slot was out of reach before a merge, and the owner moved the test to 15:21 UTC, which is 10:21 Central Daylight Time.
- The cron reads `21 15 * * *`. `NightWorkflowRunsAtTheScheduleTestTime` replaces `NightWorkflowRunsAtTwoCentralStandardTime`. It asserts the line, the comment, both finding ids, and the 08:07 UTC return time, and it fails on the workflow of `main` at the cron line.
- D-285 carries a `Revised in part by D-286` marker, and OQ-154 gains a dated note. Session 106 moved to the archive.

### State of the build

- `main` is at `095ce5e`, the squash merge of PR #45. This branch holds the work commit above it, and this entry above that.
- Remote head: `origin/chore/night-schedule-reset` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- The record on `night-results` is still the success of hand run 6 at `5455e5d`, ended 02:21 UTC on 2026-09-11. The night gate turns red on every PR at 02:21 UTC on 2026-09-13 unless a night refreshes it.

### In flight

This PR. It needs the automated pass, a Codex review, and the owner merge before 15:21 UTC on 2026-09-11. After the merge, `gh workflow disable night.yml` and then `gh workflow enable night.yml` run, with a read of the workflow state after each. A watch then reads the 15:21 UTC night with the three commands of Session 115.

### Traps and gotchas

- A merge or an enable after 15:21 UTC moves the first test to 15:21 UTC on 2026-09-12 (D-286). The night gate stays green until 02:21 UTC on 2026-09-13.
- The night holds the Mac runner from 15:21 UTC for about 65 minutes. A macOS PR job waits in that window.
- If no run of the schedule event exists by 16:15 UTC, that is a third miss. Stop and ask the owner. The `launchd` timer on the Mac Mini was an option in OQ-155.
- After the pass, two PRs follow: the Gate 1 record, docs alone with the `review-override` label, and the return of the cron to 08:07 UTC, code with a Codex review. Ask the owner the order then.
- The ids moved: the Gate 1 sign-off is D-287 now, and the answer to OQ-47 is the decision after it. The next ids are D-287, OQ-156, F-96, and Session 117.
- D-284 and D-278 carry no marker for the partial revisions of D-285 and D-283. This PR adds the marker for D-286 alone.

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12, and the register holds a full recommendation. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, the shape test, and the registers. The owner merges it before 15:21 UTC. Then the disable and the enable run, and a watch reads the 15:21 UTC night. When it passes, the Gate 1 record follows the edit list of Session 115, with D-287 for the sign-off, the row of the 15:21 UTC night, and F-95 at ✅.

## Session 115: 2026-09-11, Claude Code

Author: Claude Code
Session: put the state of the nights in the documents before a fresh session, which listens for the first scheduled night. Branch `docs/night-state-for-gate-1`.

### What this session did, and why

- The owner merged PR #43 as `557568a` and PR #44 as `57bc164`. The night keeps the logs of a failed run (D-280), and the cron reads `7 8 * * *`, which is 08:07 UTC and 02:07 Central Standard Time (D-284, D-285).
- The M-2 table holds the rows of hand runs 3 to 6: 59, 65, 63, and 68 minutes, all green. Only the row of the first scheduled night is missing.
- OQ-154 is open: the Gate 1 sign-off, after the first scheduled night passes on its own (D-283). The next session resolves it with the owner and records the sign-off as the next decision.
- No scheduled night has ever fired. The 03:00 UTC run of 2026-09-11 never came (F-94), and the 08:07 UTC line has not had its first chance yet.
- Session 105 moved to the archive.

### State of the build

- `main` is at `57bc164`, the squash merge of PR #44. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/night-state-for-gate-1` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The record on `night-results` is the success of hand run 6 at `5455e5d`, ended 02:21 UTC on 2026-09-11. The night gate turns red on every PR at 02:21 UTC on 2026-09-13 unless a night refreshes it.
- No PR is open.

### In flight

The first scheduled night, at 08:07 UTC on 2026-09-11. It takes about 65 minutes. Read it with these commands:

- `gh run list --workflow=night.yml --event schedule --limit 1 --json databaseId,status,conclusion,createdAt`: an empty list means the schedule has not fired.
- `gh api repos/nkramber/what-you-carry/actions/runs/<id> --jq '.run_started_at, .updated_at'`: the minutes of the row come from these two times, because the jobs API gives null times for the self-hosted job.
- `git fetch origin night-results && git show FETCH_HEAD:night.json`: the record, with the commit, the end time, and the status.

### Traps and gotchas

- GitHub delays or drops a schedule at the start of an hour under load, and this repository has never seen a schedule run. If no run exists by 09:00 UTC, that is a new question for the owner, and not a repeat of F-94: name what the runner and the workflow page show.
- The Mac runner takes a queued PR job only when no night is queued. A PR opened while the night runs waits about an hour for its macOS jobs.
- The record of `night-record` on `main` writes no byte-order mark since PR-58, and the gate reads the older records with one through git.
- The next ids are D-286, OQ-155, F-95, and Session 116. The scratchpad of the last session is gone: the M-2 rows above are the only copy of the hand run times, and the Gate 1 edit list below is the only copy of the plan.
- A docs-only PR takes the `review-override` label after its last push and the automated pass, and the review gate reads green then (D-188, D-190).

### Open questions that block progress

OQ-154 blocks Phase 2 until the owner signs Gate 1. OQ-47 blocks PR-12, and the register holds a full recommendation. OQ-99 is open, and it blocks nothing.

### Next concrete action

When the scheduled night passes, ask the owner two things in one batch: the Gate 1 sign-off, and the answer to OQ-47. Then one docs PR, the Gate 1 record, with these edits:

1. The M-2 table: the row `Scheduled night 1, 2026-09-11`, the run id, the commit, the minutes, and the status. Below it: `Status: table complete 2026-09-11, seven nights (D-283).` The design doc M-2 entry reads ✅ with the same words.
2. `docs/decisions.md`: D-286, the Gate 1 sign-off in the words of the owner. Resolves OQ-154. Applies D-150 and D-283. The next decision holds the answer to OQ-47.
3. The Phase 1 roadmap: the status header reads `focused roadmap, complete`, sequence item 22 reads ✅ for M-2, item 23 reads ✅ signed with the D-# id, the correction note gains a dated sentence, and the open questions list moves OQ-154 to a resolved line.
4. The design doc: sequence item 8 reads ✅ for M-2, item 9 reads ✅ signed with the D-# id, and the Phase 1 heading gains the sign-off.
5. Session 116 handoff entry.

After the merge, Phase 2 starts: a session opens PR-12 from `main` per the Phase 2 roadmap entry and the answer to OQ-47.

## Session 114: 2026-09-11, Codex

Author: Codex
Session: review PR #44 at effective head `0bfafbd`. Branch `chore/night-minute`.

### What this session did, and why

- Verified the provider gate. Session 113 identifies Claude Code as the author of the substantive PR-44 change. Codex is the eligible reviewer.
- Verified the base, merge base, effective head, complete diff, D-284, D-285, F-94, the Phase 1 roadmap, and every PR comment.
- The workflow runs at `7 8 * * *`, the shape test checks the line and F-94, and the changed registers and handoff agree with the decision.
- The focused repository-shape suite passes 14 tests. The review found no defect and wrote `docs/reviews/pr-44.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` and the merge base are `557568a`. The effective implementation head is `0bfafbd`. The metadata tip is `838c544`.
- Local det-lint, STE check, bit identity, and the Godot headless build pass. The focused suite passes 14 tests with 0 failures and 0 skips.
- The local full build and full test run did not complete in the execution context. Remote platform builds and tests, bit identity, bots, det-lint, STE check, night-gate, and Gitar pass on the PR tip.

### In flight

The review record and this handoff entry are pushed. The fresh review-gate result must pass at effective head `0bfafbd` before the owner merges.

### Traps and gotchas

- The effective head is `0bfafbd`, not the handoff-only tip `838c544` (D-184).
- The current review-gate failure is the expected missing-record state. It is not a product failure. The gate must rerun after `docs/reviews/pr-44.md` reaches the PR.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the fresh checks. Verify that the remote has no ahead count, and confirm that the fresh review-gate result passes.

## Session 113: 2026-09-11, Claude Code

Author: Claude Code
Session: the night minute, off the start of the hour (D-285). Branch `chore/night-minute`.

### What this session did, and why

- The 03:00 UTC scheduled night of 2026-09-11 never fired. No run of the schedule event exists, the runner was free from 02:45 UTC, the old cron line stood on `main` until PR #42 merged at 03:54 UTC, and the workflow reads active on GitHub. F-94 records it. GitHub documents a delay or a drop at the start of an hour under load.
- Asked the owner, and D-285 records the answer: the night runs at 08:07 UTC, which is 02:07 Central Standard Time, in a PR of its own. OQ-153 holds the question. D-284 is revised in part, the minute only.
- The cron reads `7 8 * * *`, the comment names the minute and F-94, and `NightWorkflowRunsAtTwoCentralStandardTime` asserts the new line in place, so this PR adds no test at the anchor that PR #43 also touches.
- PR #43, the night logs of D-280, is open from Session 109 with a Codex review pending. This PR is the second open PR, because the owner wants the minute on `main` before 08:00 UTC.
- Session 103 moved to the archive at the rebase onto the PR #43 merge. This entry was Session 110 on the branch, and the rebase renumbered it to 113, because `main` holds a Session 110 from the review of PR #43 and D-187 allows one heading per number.

### State of the build

- `main` is at `557568a`, the squash merge of PR #43. This branch holds the workflow commit above it, and this entry above that.
- Remote head: `origin/chore/night-minute` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 533 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR, rebased onto the PR #43 merge. A watch reads the next scheduled run, at 08:00 UTC if this PR merges after that hour and at 08:07 UTC otherwise. After it passes, a session adds the five rows to the M-2 table and the owner signs Gate 1 (D-283).

### Traps and gotchas

- No scheduled run has ever fired for this repository. The first one is the proof D-283 asks for, and a second miss at 08:07 UTC is a new question, not a repeat of F-94.
- Two PRs touched `night.yml` in different hunks, and both added a handoff entry. This one merged second and needed a rebase for the handoff alone, with a renumber.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, and the shape test. The owner merges it when the review allows.

## Session 112: 2026-09-11, Codex

Author: Codex
Session: repeat review PR #43 at effective head `e63c35b`. Branch `chore/night-logs-artifact`.

### What this session did, and why

- Read `docs/reviews/pr-43-response.md` and checked the provider gate again.
- Verified the new effective head `e63c35b` and the diff since `d89338c`. The workflow did not change. The test now checks the upload step after the walker, descender, and reachability sweep.
- Reproduced the original trigger with the new mutation test. The focused repository-shape suite passes 14 tests, with no failures or skips.
- Updated `docs/reviews/pr-43.md`. P2-1 is fixed in `e63c35b`, and the verdict is `Ready for owner merge` after the fresh review gate passes.

### State of the build

- `main` and the merge base are `f19fe2e`. The effective implementation head is `e63c35b`.
- Local det-lint, STE check, bit identity, focused tests, and Godot build pass. The local full suite did not complete after 30 seconds with no output. The author reports 535 tests, 0 failures, and remote platform jobs pass.
- The evaluate and review-gate jobs still read the prior review verdict. They must run again after this review record is pushed.

### In flight

The repeat-review record and this handoff entry need a push. Then the fresh review-gate result must pass before merge.

### Traps and gotchas

- The workflow commit remains `8ea990f`. The new effective head is the test correction `e63c35b`, not a metadata tip.
- The review gate failed before this update because the record still held the prior verdict. That is a stale metadata result, not a product failure.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Push the repeat-review record and handoff. Refresh the review-gate result and confirm the owner can merge when all required checks pass.

## Session 111: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #43 review. Branch `chore/night-logs-artifact`.

### What this session did, and why

- Read the one P2 finding in `docs/reviews/pr-43.md`. Full merit: the shape test compared the upload step with the greedy-descender step alone, so a step moved to a place before the sweep passed the test while a failed sweep ran with no upload.
- `UploadStepDefect` reads a workflow text and names the first defect: no action, a wrong condition, a wrong path, a wrong missing-files rule, or a place before any of the three bot steps. `NightWorkflowUploadStepMustFollowEveryBotStep` moves the step before the sweep and before the walker in the real workflow text with `MoveStepBefore`, and asserts the name of each defect. The workflow did not change.
- `docs/reviews/pr-43-response.md` records the disposition. No new id.
- Session 101 moved to the archive.
- PR #44, the night minute of D-285, is open beside this PR with its own Session 110 entry. The review entry of this branch is Session 110 too, so the second PR to merge renumbers at its rebase, and the entry that renumbers says so (D-187).

### State of the build

- `main` is at `f19fe2e`, the squash merge of PR #42. This branch holds the workflow commit `8ea990f`, the Session 109 entry, the slice fix `d89338c`, the three review commits, and the correction that holds this entry above them.
- Remote head: `origin/chore/night-logs-artifact` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 535 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- No scheduled night has fired yet. The next chance is 08:00 UTC on the line of `main`, or 08:07 UTC once PR #44 merges.

### In flight

PR #43 waits for the automated pass on the correction, then a Codex repeat review at the effective head, which is the correction commit. PR #44 waits for its Codex review. The night watch reads the first scheduled run.

### Traps and gotchas

- The effective head is the correction commit, because it changes a test. The review record still names `d89338c`, and the repeat review updates the head and the verdict together, with one verdict name in the Verdict section (D-269).
- Two open branches each carry a Session 110 heading, one a review entry and one an author entry. The session-number check of the STE checker fails `main` on two headings with one number, so the rebase of the second PR renumbers before the push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure at the effective head, verifies P2-1 against its trigger and the regression case, and updates `docs/reviews/pr-43.md` with the status of P2-1 and a new verdict.

## Session 110: 2026-09-11, Codex

Author: Codex
Session: review PR #43 at effective head `d89338c`. Branch `chore/night-logs-artifact`.

### What this session did, and why

- Verified the base, merge base, effective head, provider gate, complete diff, D-280, the PR-11 night contract, the workflow, the shape test, the handoff files, and every PR comment.
- Gitar's step-slice comment is answered at `d89338c`. The slice now ends at the next step name.
- Found P2-1. The shape test checks that the upload follows the greedy descender, but it does not check that it follows the reachability sweep.
- Added `docs/reviews/pr-43.md`. The verdict is `Changes required` for `d89338c`.

### State of the build

- `main` and the merge base are `f19fe2e`. The effective implementation head is `d89338c`.
- Remote Linux, Windows, macOS, bots, det-lint, STE check, night-gate, and Gitar pass. The evaluate and review-gate results were unavailable because the review file did not exist before this session.
- Local det-lint, STE check, bit identity, and Godot build pass. The local build did not complete, and the local test runner stopped on a VSTest socket permission error. These are execution-context results.

### In flight

PR #43 needs the shape-test order correction, a regression check, a fresh push, and a repeat review.

### Traps and gotchas

- The upload step must follow the reachability sweep as well as the two bot runs. Checking only the greedy-descender step does not prove the full order.
- The review record names `d89338c`, not a later metadata tip, under D-184.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The author corrects P2-1 and pushes the shape test, review response, and handoff. A Codex session repeats the review at the new effective head.

## Session 109: 2026-09-11, Claude Code

Author: Claude Code
Session: the night logs of D-280. Branch `chore/night-logs-artifact`.

### What this session did, and why

- The owner merged PR #42, so the cron of the night reads 08:00 UTC on `main` now (D-284). This PR adds the one step of D-280 to the night workflow: on a failed step, `actions/upload-artifact@v4` keeps the `bot-logs` directory as a run artifact, and a night without logs says so with a warning instead of a silent pass.
- `NightWorkflowKeepsTheLogsOfAFailedNight` reads the workflow and asserts the step, its `failure()` condition, its path, its missing-files rule, and its place after the bot steps.
- D-280 is the dependency entry of the action (G-16).
- Hand runs 3 to 6 passed in 59, 65, 63, and 68 minutes, so six hand runs ran on 2026-09-10 and five passed. The record on `night-results` is the success of hand run 6 at `5455e5d`.
- Session 99 moved to the archive.

### State of the build

- `main` is at `f19fe2e`, the squash merge of PR #42. This branch holds the workflow commit above it, and this entry above that.
- Remote head: `origin/chore/night-logs-artifact` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 534 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR. The 03:00 UTC cron of 2026-09-11 did not fire by 04:00 UTC, while the runner was free and the old line still stood on `main`. The next chance is 08:00 UTC on the new cron, and a watch reads its result. After it passes, a session adds the five rows to the M-2 table and the owner signs Gate 1 (D-283).

### Traps and gotchas

- The upload step runs on `failure()` alone. A cancelled night keeps no logs, and a green night keeps none, by design.
- A change to the cron on `main` takes effect when GitHub reads the new file. GitHub delays or drops a schedule at the top of the hour under load, and the 03:00 UTC run of 2026-09-11 did not come. A cron off the top of the hour is the documented cure.
- The Mac runner takes a queued PR job only when no night is queued, as the six hand runs showed: five macOS jobs waited through two nights. Expect a PR's macOS job to wait for a whole night when one is queued.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the step, its condition, and the shape test. After the first scheduled night passes, a session records M-2 and the Gate 1 sign-off.

## Session 108: 2026-09-11, Codex

Author: Codex
Session: repeat review PR #42 at effective head `b616357`.

### What this session did, and why

- Read the response file and checked the provider gate again.
- The new commits change review and handoff metadata only. The effective head stays `b616357` under D-184.
- The macOS CI job now passes. The response file records 533 passed tests on the effective head. The focused shape test passes again.
- The earlier `Blocked` verdict is replaced with `Ready for owner merge`. No finding is open.
- Session 98 moved to the archive.

### State of the build

- `main` is at `5455e5d`. The effective implementation head is `b616357`. The PR tip before this review commit is `bc7c457`.
- The focused shape test passes. The response file records 533 passed tests, 0 failures, and 0 skips on the effective head.
- Remote Linux, Windows, and macOS CI, bit identity, bots, det-lint, night-gate, STE check, and Gitar pass. The review-gate check waits for this updated record.

### In flight

The repeat review record and this handoff entry are pushed. The review-gate check passes on this record. Duplicate platform jobs from the metadata push remain pending.

### Traps and gotchas

- The effective head is `b616357`, not the metadata tip. The new diff contains only paths in the D-184 metadata set.
- The review-gate failure at the prior tip named the old `Blocked` verdict. It did not report a product failure.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge after the pending duplicate platform jobs complete.

## Session 107: 2026-09-11, Claude Code

Author: Claude Code
Session: answer the PR #42 review. Branch `chore/night-time`.

### What this session did, and why

- Read `docs/reviews/pr-42.md`. No finding. The verdict is `Blocked` on two pieces of evidence: the macOS CI job, and the full suite, which the reviewer's sandbox could not run.
- `docs/reviews/pr-42-response.md` states where each stands. The macOS jobs of this branch are queued behind hand run 5 of D-283, which holds the Mac runner until about 01:15 UTC, and they run before hand run 6. The Linux and Windows CI jobs ran the full suite on this branch and passed, and the author ran it on the effective head: 533 passed.
- No code changed. The effective head stays `b616357`.
- Sessions 96 and 97 moved to the archive. The file held eleven entries, because the review entry above came without a rotation, and D-146 keeps ten.

### State of the build

- `main` is at `5455e5d`, the squash merge of PR #41. This branch holds the workflow commit `b616357`, the Session 105 entry, the three review commits, and this metadata commit above them.
- Remote head: `origin/chore/night-time` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 533 tests, 0 failures, on the effective head before the push of Session 105.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- Hand runs 3 and 4 passed in 59 and 64 minutes. Hand run 5 is on its sweep step, and run 6 follows it.

### In flight

PR #42 waits for its macOS jobs, then for a Codex repeat review that reads them and sets the verdict at the effective head `b616357`. Then the night logs PR of D-280 opens from the local branch `chore/night-logs-artifact` at `0e71413`. No other PR is open.

### Traps and gotchas

- A `Blocked` verdict on pending evidence needs a repeat review after the evidence lands, and the author never sets the verdict. The response file names the evidence so the repeat review finds it in one place.
- Every push to a PR during the hand runs queues a macOS job behind the current night. The queue runs in creation order, so a job pushed before the next dispatch runs before that night.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

When the macOS jobs of PR #42 pass, a Codex session repeats the review per the repeat procedure, reads the macOS result and the response file, and updates `docs/reviews/pr-42.md` with a new verdict at `b616357`.

## Session 106: 2026-09-11, Codex

Author: Codex
Session: review PR #42 at effective head `b616357`.

### What this session did, and why

- Verified the base, merge base, effective head, complete diff, D-284, the Phase 1 roadmap, the workflow, the shape test, the handoff files, the agent files, and every PR comment.
- The cron reads `0 8 * * *`. The shape test checks the cron, the hand trigger, and the 02:00 Central Standard Time comment. No finding remains.
- The metadata commit adds `docs/reviews/pr-42.md` and this entry. The effective head stays `b616357` under D-184.

### State of the build

- `main` is at `5455e5d`. The effective implementation head is `b616357`. The PR metadata tip is `b9a5d5d` after this review commit.
- `dotnet build`: 0 warnings, 0 errors. The focused shape test passes. The full local suite did not complete after the test runner socket error and an approved retry that produced no output.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`. The Godot 4.7.2 headless build passes.
- Remote Linux and Windows checks, bit identity, bots, det-lint, night-gate, STE check, and Gitar pass. The macOS CI job remains pending. The review-gate check waits for this record.

### In flight

The review record and this handoff entry are pushed. The owner must wait for the pending macOS CI result and the review-gate result.

### Traps and gotchas

- The effective head is `b616357`, not the metadata tip. Only the review and handoff paths are metadata under D-184.
- The first local test run failed on a socket permission error. The approved full-suite retry did not complete. Treat both as execution-context results.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Wait for the macOS CI and review-gate results before the owner merges.

## Session 105: 2026-09-11, Claude Code

Author: Claude Code
Session: the night time of D-284. Branch `chore/night-time`.

### What this session did, and why

- The owner merged PR #41 as `5455e5d`. This PR moves the cron of the night workflow to `0 8 * * *`, which is 08:00 UTC and 02:00 Central Standard Time (D-284). The comment in the workflow names the time and the summer shift to 03:00 Central Daylight Time.
- `NightWorkflowRunsAtTwoCentralStandardTime` reads the workflow and asserts the cron, the hand trigger, and the comment.
- Hand run 3 passed in 59 minutes at `5bdef87`, and its record is on `night-results`. Hand run 4 started at 23:05 UTC. The dispatcher starts runs 5 and 6 in turn.
- Session 95 moved to the archive.

### State of the build

- `main` is at `5455e5d`, the squash merge of PR #41. This branch holds the workflow commit above it, and this entry above that.
- Remote head: `origin/chore/night-time` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 533 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

This PR, then the night logs PR of D-280 from the local branch `chore/night-logs-artifact` at `0e71413`. Hand runs 4 to 6 hold the Mac runner in turns until about 02:15 UTC. The old cron fires at 03:00 UTC if this PR merges after that time, and the new one at 08:00 UTC otherwise. Either is the first scheduled night of D-283.

### Traps and gotchas

- The cron of GitHub reads UTC alone. The 08:00 UTC line is 02:00 Central Standard Time and 03:00 Central Daylight Time. A line that follows the local clock all year needs two cron lines and a guard step.
- The effective head of this PR is the workflow commit. This entry is a metadata commit.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cron line, the comment, and the shape test. After the merge, the night logs PR opens. On 2026-09-11, after the first scheduled night, a session adds the five rows to the M-2 table and the owner signs Gate 1.

## Session 104: 2026-09-10, Claude Code

Author: Claude Code
Session: record the PR-58 merge, the Gate 1 timing, and the night time. Branch `docs/pr-58-merge-record`.

### What this session did, and why

- The owner merged PR #40 as `5bdef87`. The design doc PR-58 entry reads merged, F-48 reads done, the roadmap entry has its status line, and the sequences mark items 20 to 22 and the gate condition.
- Asked one owner question on the Gate 1 timing, and the owner answered with two of their own: why seven scheduled nights, and what is playable today. Answered both. The runs are deterministic on one machine, so a scheduled night adds no measurement, and nothing renders before PR-12.
- D-283: four more nights run by hand today, the M-2 table counts the six hand runs and the first scheduled night, and Gate 1 signs on 2026-09-11 after that night passes on its own. OQ-151 holds the question. D-278 is revised in part, the M-2 count only.
- D-284: the night runs at 08:00 UTC, which is 02:00 Central Standard Time. OQ-152 holds the question. D-278 is revised in part, the time only. The workflow change comes in a PR of its own.
- Started the four hand runs at 22:06 UTC, one after another, from a script that dispatches the next when the previous ends, so the PR jobs queued between them still reach the runner. The M-2 section holds the two rows.

### State of the build

- `main` is at `5bdef87`. This session holds the document commit above it.
- The night gate is live on every PR. The record on `night-results` is the success of hand run 2 at `a2799f2` until the next night overwrites it.

### In flight

The four hand runs and the two workflow PRs were in flight. The cron stayed at 03:00 UTC until the night-time PR merged.

### Traps and gotchas

- A macOS CI job queued during a hand run waits for that run, up to about an hour.
- Gate 1 signs after the first scheduled night passes on its own.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Open the night-time PR from `main`, then open the night-logs PR.

## Session 103: 2026-09-10, Codex

Author: Codex
Session: repeat review PR #40 at effective head `d943cb3`. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- Recomputed the PR identity. The base and merge base are `a2799f2`. The effective head is `d943cb3`. The tip `bcb934e` changes review and handoff metadata only.
- Verified the cross-provider gate. Claude Code authored the substantive PR change and its correction. Codex is the eligible reviewer.
- Reproduced P1-1 at the correction boundary. A planted checkout record fails when the remote has no branch. A branch without `night.json` is absent. A remote failure record wins over the planted success record. An unreachable remote reports an error.
- Verified the adjacent byte-order-mark parser case, command exit cases, workflow shape test, full-history checkout, and the remote-read path. P1-1 has full merit at the prior head and is corrected at `d943cb3`.
- Updated `docs/reviews/pr-40.md` with the finding disposition and the verdict `Ready for owner merge`.

### State of the build

- `main` is at `a2799f2`. The PR branch is at `bcb934e` before this review commit. The effective implementation head is `d943cb3`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 532 tests, 0 failures, 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files. `bit-identity`: `6ec00e90c1c85cdb`.
- The Godot 4.7.2 headless build passes. The real `night-gate` probe passes against `origin/night-results` at `2026-09-10T22:00:00Z`.
- The review commit is `0d10eba`. Its Gitar, det-lint, night-gate, STE check, and review-gate checks passed. The metadata tip is now `48ede21`, and its fresh checks are pending after the metadata push.
- Remote head: `origin/feat/pr-58-night-gate` at `48ede21`, checked after the push. The checkout has no ahead count.

### In flight

PR #40 waits for the review-gate check to read this record at the effective head. No other PR is open.

### Traps and gotchas

- The review record must name `d943cb3`, not the metadata tip. The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` (D-184).
- The real-remote probe first failed because the sandbox could not resolve `github.com`. The approved retry passed. Treat the first result as an execution-context failure.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge after the pending CI, bit identity, and bots checks pass. The review-gate check reads `Ready for owner merge` at `d943cb3`.

## Session 102: 2026-09-10, Claude Code

Author: Claude Code
Session: answer the PR #40 review. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- Read the one P1 finding in `docs/reviews/pr-40.md`. Full merit: the fetch step of the workflow left a `night.json` from the PR checkout in place when the fetch failed, and the command read it, so a PR could carry a fresh success record and pass the gate with no night.
- The fetch is in the tool now. `NightGateFacts.Gather` takes the checkout, the remote, the base ref, and the time. It asks the remote for the branch with `ls-remote --exit-code`, fetches it, and reads `night.json` from `FETCH_HEAD` through git, never from the working tree. An absent branch or an absent file is the absent case with the reason, and any other git failure throws with the command (T-2). The workflow has no shell step.
- The parser accepts a leading byte-order mark, because the two records on `night-results` carry one and git does not strip it.
- `NightGateReadsTheRecordFromTheRemoteAndNeverFromTheCheckout` plants a fresh success record in the checkout and asserts the absent case, the reason of a branch without the file, the win of the branch record over the planted file, and the error of an unreachable remote. The exit code test plants the file too. The tests use one temporary repository as the remote of another.
- F-93 records the finding, and `docs/reviews/pr-40-response.md` records the disposition. The tool ran against the real remote from this checkout: pass, at `a2799f2`.
- Session 92 moved to the archive.

### State of the build

- `main` is at `a2799f2`, the squash merge of PR #39. This branch holds the PR-58 commit, the byte-order-mark fix, the documents commit `2253e53`, the review commit `3e964a2`, and the correction that holds this entry above them.
- Remote head: `origin/feat/pr-58-night-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 532 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.

### In flight

PR #40 waits for the automated pass on the correction, then a Codex repeat review at the effective head, which is the correction commit. No other PR is open.

### Traps and gotchas

- The effective head is the correction commit, because it changes code, the register, and the roadmap. The review record still names `2253e53`, and the repeat review updates the head and the verdict together, with one verdict name in the Verdict section (D-269).
- The night gate reads the record through git. A test of it needs a remote with the orphan branch, and `PublishNight` in the tests makes one from a temporary repository.
- `git ls-remote --exit-code` exits 2 for no matching ref. Every other nonzero exit is an error, and the tool throws. The job then fails with the git message and not with a gate case.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure at the effective head, verifies P1-1 against its trigger and the regression test, and updates `docs/reviews/pr-40.md` with the status of P1-1 and a new verdict.

## Session 101: 2026-09-10, Codex

Author: Codex
Session: review PR #40, the night gate. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- Verified the provider gate. Session 100 identifies Claude Code as the provider of the substantive PR-40 change. Codex is the eligible reviewer.
- Verified the base, merge base, effective head `2253e53`, complete diff, PR-58 scope, exit tests, decisions, and all PR comments and replies.
- Found P1-1. The workflow catches every fetch failure as an absent record but leaves a `night.json` from the pull request checkout in place. A pull request can then supply a fresh success record and bypass the absent-record gate.
- Wrote `docs/reviews/pr-40.md` with the verdict `Changes required` for effective head `2253e53`.

### State of the build

- `main` is at `a2799f2`, the squash merge of PR #39. The effective PR-40 head is `2253e53`.
- Remote checks at the PR head passed for CI, bit identity, bots, det-lint, STE check, night-gate, and Gitar. The review-gate check was neutral before the review record existed.
- `git diff --check` passed. A local serial build and test retry hit a stuck execution-context process after an earlier parallel file-copy contention and was cancelled. The remote checks are the revision-specific build and test evidence.

### In flight

PR #40 needs the workflow correction for P1-1 and a repeat review at the new effective head. No other PR is open.

### Traps and gotchas

- The fetch step must not read a file from the pull request checkout when `night-results` is absent or the fetch fails. Use a temporary path or remove the checkout file before the fetch.
- The review record names the effective head `2253e53`, not the metadata tip rule in a future review commit (D-184).

### Open questions that block progress

P1-1 blocks PR #40. OQ-99 is open, and it blocks no other work.

### Next concrete action

Correct the `night-gate` workflow so only a successfully fetched `night-results` record can reach the command. Add the missing workflow regression test, push, and request a repeat review.

## Session 100: 2026-09-10, Claude Code

Author: Claude Code
Session: the second night by hand, and PR-58, the night gate. Branch `feat/pr-58-night-gate`.

### What this session did, and why

- The owner merged PR #39 as `a2799f2`. Started the night by hand on `main` at 19:00 UTC under D-281: run 34517749543, 63 minutes, success. The record on `night-results` names `a2799f2` and ended at 2026-09-10T20:03:22Z.
- PR-58 from `main` at `a2799f2`. `Tools/NightGate/` holds the parser, the facts, the rules, and the command `night-gate`. The rules read the cases in order: absent, malformed, stale, foreign, cancelled, failed, pass. The command exits 0, 1, or 2, and every failure line names the case, the commit, and the time (D-274, D-275).
- The workflow `night-gate.yml` runs one job on Linux with the full history, fetches `night-results`, and passes the record and the base ref to the command. An absent branch reads as an absent record.
- `GitRepository` gains `HasCommit` and `IsAncestor`, each with the exit codes that git documents for a no. `night-record` writes no byte-order mark now, and PR-11 exit test 7 asserts the first byte.
- Exit tests 1 to 6 and 8 pass among 530 tests, with the malformed case, the exit codes, and a workflow shape test beside them. Exit test 7 is the job on this PR. The agent files gain the `night-gate` line of the PR gate.
- The roadmap PR-58 entry holds the scope notes, and the design doc names the malformed case. The M-2 section holds the note of the second hand run.
- Session 90 moved to the archive.

### State of the build

- `main` is at `a2799f2`, the squash merge of PR #39. This branch holds the PR-58 commit, the byte-order-mark fix, and the documents commit above them, and this entry is in the documents commit.
- Remote head: `origin/feat/pr-58-night-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 530 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6. Core did not change.
- The branch `night-results` holds the record of the second hand run.

### In flight

PR-58 is open as the PR that holds this branch. A Codex session reviews it at the effective head, which is the documents commit, because it changes the roadmap and the design doc. After the merge, the night logs PR of D-280 opens from the local branch `chore/night-logs-artifact`, then M-2 collects seven scheduled nights, then Gate 1.

### Traps and gotchas

- The `night-gate` job fails every PR within 48 hours of a red or missing night, a documentation PR too. A hand run on `main` restores it (D-274, D-278).
- The record commit must be on the base branch (D-275). A hand run on a feature branch writes a record that fails every PR to `main`.
- The effective head of this PR is the documents commit, not the code commit, because the roadmap and the design doc lie outside the metadata set (D-184).
- The night holds the one Mac runner for its whole run, so the macOS CI jobs of every open PR wait for it.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR-58 per the `pr-review` skill at the effective head, with the focus on errors, CI boundaries, and test quality. It confirms the job passed on the PR against the real record (exit test 7, G-19).

## Session 99: 2026-09-10, Claude Code

Author: Claude Code
Session: correct the head field of the PR #39 review record, one time (D-282). Branch `fix/dig-plan-job-budget`.

### What this session did, and why

- The review record of Session 98 named the fix commit `0685c4e` as the effective head, and the gate refused it. The effective head was `90ab13e`, because that commit adds entries to the decisions, design, questions, and roadmap files, which lie outside the metadata set (D-184). The hand-over of Session 97 gave the wrong head, and the review followed it.
- The owner chose an author correction over a repeat review, one time. OQ-150 holds the question, and D-282 records the answer.
- The register commit `34cbc39768611d795fb656b6fa6d65a17432931e` holds D-282 and OQ-150, and this commit sets the head field of the record to it and adds a dated note above the Verdict section. This commit changes the record and the two handoff files alone, so the effective head stays `34cbc39768611d795fb656b6fa6d65a17432931e`.
- Ran the review-gate command locally against the head of this commit before the push: conclusion success.
- Session 89 moved to the archive.

### State of the build

- `main` is at `736e466`, the squash merge of PR #38. This branch holds the fix `0685c4e`, the registers commit `90ab13e`, the review commit `ac5f8d9`, the register commit `34cbc39768611d795fb656b6fa6d65a17432931e`, and this entry above them.
- Remote head: `origin/fix/dig-plan-job-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 520 tests, 0 failures. No code changed in this session.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The branch `night-results` holds the failure record of the first night at `fb080ca`.

### In flight

PR #39 waits for the gate to read this head, then for the owner merge. After the merge, the second night by hand on `main` (D-281), then PR-58 from the local branch `feat/pr-58-night-gate` at `32dcbcb`, then the night logs PR of D-280 from the local branch `chore/night-logs-artifact` at `5bef2c8`.

### Traps and gotchas

- The metadata set of D-184 is `docs/reviews/` and the two handoff files alone. A commit that adds a D-#, an F-#, an OQ-#, or a roadmap line moves the effective head. Name the hash that the gate rule gives at every hand-over: the newest commit in the range outside those three paths.
- A decision that a PR itself needs goes in a commit before the review record commit, so the record can name it. The record commit then changes the metadata paths alone.
- D-282 is one time. On every other PR the reviewer owns the review record, and a wrong head field takes a repeat review.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #39 when the review-gate check reads green on this head. Then a session starts the night by hand on `main` (D-281), reads `night.json` on `night-results`, and opens PR-58 on a success record.

## Session 98: 2026-09-10, Codex

Author: Codex
Session: review PR #39 at effective head `0685c4e`. Branch `fix/dig-plan-job-budget`.

### What this session did, and why

- Reviewed the measured dig-plan cap fix for F-92 under D-253 and D-279.
- Verified the exact PR base, merge base, substantive head, provider gate, complete diff, caller, regression tests, decisions, roadmap, and automated pass.
- Added `docs/reviews/pr-39.md`. The review found no issue and records `Ready for owner merge` for effective head `0685c4e`.

### State of the build

- `main` is at `736e466`. The PR branch is at metadata tip `90ab13e`, with effective head `0685c4e`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 520 tests, 0 failures, 0 skips.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #39 waits for the owner to confirm the pending remote checks and merge it. After the merge, run the second night by hand on `main` under D-281.

### Traps and gotchas

- The first local test attempt failed before test execution because VSTest could not bind its local socket. The elevated retry passed.
- The PR review-gate result was neutral before this review record existed. GitHub API access failed during the final status refresh, so the complete remote status remains unverified.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner confirms the remote checks and merges PR #39. Then run the second night by hand on `main` and read its `night.json` record.

## Session 97: 2026-09-10, Claude Code

Author: Claude Code
Session: the first night failed on the dig job cap, and this fix raises the cap on the measurement (F-92, D-279). Branch `fix/dig-plan-job-budget`.

### What this session did, and why

- The hand-run night 34499677095 on `fb080ca` failed at the greedy descender step at 16:34 UTC: bottom 4997, crash 3 of 5000, and the record on `night-results` reads failure. The reachability sweep did not run.
- Reproduced the three crashes locally in ten parallel chunks of 500 seeds. Each is the same fault: the dig plan ran its 400 jobs and a chamber stayed in rock, on seed 2170 floor 10, seed 3000 floor 4, and seed 4786 floor 10. The PR sweep digs floor 11, 1, and 2 for those seeds, so it never met them.
- Measured with the cap lifted: the three floors need 2817, 1661, and 615 jobs. Of 5000 sweep floors, 4969 finish inside 25 jobs, and the largest other need is 208. The three floors generate, play to bottom, and hold 0.033 to 0.036 air against a normal mean of 0.040 to 0.052, so the extra jobs were walkers with no room.
- Asked three owner questions in one batch. D-279 sets `DigPlan.MaxJobs` to 10000, D-280 uploads the bot logs of a failed night as a run artifact, and D-281 runs a second night by hand after this fix merges. OQ-147 to OQ-149 hold them.
- `DigUntilComplete` gives the job count now, and `DigPlanJobCapTests` digs the three floors and fails on the old cap. F-92 is in the register, and the M-2 section holds the note of the first hand run.
- The bit-identity hash stays `6ec00e90c1c85cdb`, because no floor that dug inside 400 jobs changes. The simulation version stays 6 for the same reason.
- Session 87 moved to the archive.

### State of the build

- `main` is at `736e466`, the squash merge of PR #38. This branch holds the fix commit above it, and this entry above that.
- Remote head: `origin/fix/dig-plan-job-budget` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 520 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The branch `night-results` holds one record: `fb080ca`, 16:34:28 UTC, failure. PR-58 is written on the local branch `feat/pr-58-night-gate` at `d6f7c6b` above `fb080ca`, with 528 tests green, and it waits for a success record.

### In flight

This PR, the fix of F-92. A Codex review comes next. After the merge, a second hand run of the night (D-281), then PR-58 opens from `main` with the local branch rebased, then the night logs PR of D-280. No other PR is open.

### Traps and gotchas

- The night runs on the one Mac runner, so the macOS CI jobs of every open PR wait for it. A hand run at daytime holds them for about half an hour, and a green run with the sweep takes longer.
- The runner checkout cleans the bot logs at the next job. Until D-280 lands, reproduce a failed night locally: ten parallel `bot-run` chunks of 500 seeds take about three minutes.
- The sweep digs one floor per seed, `1 + seed % 15`, and the bots dig all fifteen floors of a seed, so the bots cover floors the sweep never digs.
- The record of `night-record` starts with a byte-order mark, because `File.WriteAllText` with `Encoding.UTF8` writes one. `File.ReadAllText` strips it, so the gate of PR-58 reads it, and the PR-58 branch corrects the writer.
- `DigUntilComplete` returns the job count. A caller that ignores it compiles, so the regression test is the only reader.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews this PR per the `pr-review` skill: the cap, the measurement, the regression test, and the hash claim. After the merge, start the night by hand on `main` (D-281), and when its record reads success, open PR-58 from the rebased local branch.

## Session 96: 2026-09-10, Claude Code

Author: Claude Code
Session: record the first night by hand (D-278), and start that night. Branch `docs/night-by-hand`.

### What this session did, and why

- The owner merged PR #37 as `fb080ca`, then asked for a one-time hand run of the night, so that PR-58 opens before the first scheduled night. OQ-146 holds the question, and D-278 records the answer. D-274 is revised in part, the sequence wait only, and its gate rule stands.
- Started the night workflow on `main` at `fb080ca` by `workflow_dispatch` at 16:03 UTC. The run is 34499677095. It writes `night.json` to `night-results` whatever its outcome.
- Sequence item 20 reads "scheduled or by hand" in the roadmap and the design doc, and so does the PR-58 entry. The M-2 procedure counts the seven scheduled nights, with a note beside the table for the hand run.
- Session 86 moved to the archive.

### State of the build

- `main` is at `fb080ca`, the squash merge of PR #37. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/night-by-hand` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The night run 34499677095 was in progress on the runner `mac-mini-m4` when this entry was written. The branch `night-results` appears when its publish step runs.

### In flight

The night run on `main`. This PR holds D-278 and changes no code, so the `review-override` label covers it (D-188, D-190). No other PR is open. The scheduled night still runs at 03:00 UTC on 2026-09-11 and overwrites the record.

### Traps and gotchas

- The night record names the commit it ran on, `fb080ca`. The PR-58 gate checks that the record commit is on the base branch (D-275), and a later merge to `main` keeps it there.
- A failed hand run leaves a failure record. No gate reads it yet, so it blocks nothing, and the scheduled night overwrites it. Read the run log before a second hand run.
- The night runs on this Mac, and the reachability sweep of one hundred thousand seeds is the long step. The M-2 bound is six hours.
- The publish step makes a worktree and an orphan branch in the runner checkout. The scheduled night tomorrow is the second run on that checkout, and its publish step is the one to watch.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

When the night run ends, read `night.json` on `night-results`. On a success record, open PR-58 from `main` per the roadmap entry, D-274, and D-275, and note the hand run duration beside the M-2 table. On a failure record, read the run log and correct the cause in a PR before the next hand run.

## Session 95: 2026-09-10, Claude Code

Author: Claude Code
Session: the M-1 table, and the seed count decision. Branch `docs/m-1-table`.

### What this session did, and why

- The owner merged PR #36 as `0ba348c`. D-276 lets the M-1 table fill before PR-58, and the ten Phase 1 PRs since PR-3 merged, so the table is complete in the Phase 1 roadmap.
- The rows read the push run on `main` of the squash-merge commit of each PR, first attempt, in seconds per job. The CI job grew at PR-9 with the reachability sweep, and again at PR-10 and PR-11.
- The Windows CI job of PR-10 took 601 seconds, one second over the ten-minute bound of the M-1 procedure. F-91 records the measurement, OQ-145 holds the question, and D-277 keeps the 5000 PR seeds of D-116. The bound reads again at Gate 1.
- The design doc M-1 entry reads complete. Sequence item 22 marks M-1 in the roadmap, and item 8 marks it in the design doc.
- Session 85 moved to the archive.

### State of the build

- `main` is at `0ba348c`, the squash merge of PR #36. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/m-1-table` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- No night has run. The branch `night-results` does not exist yet.

### In flight

This PR holds the M-1 table and D-277. It changes no code, so the `review-override` label covers it (D-188, D-190). The first scheduled night runs at 03:00 UTC on 2026-09-11. No other PR is open. After the night, PR-58 opens, and M-2 starts with the first night duration. Then Gate 1.

### Traps and gotchas

- The M-1 rows come from the push runs on `main`, not the pull request runs. A PR runs its jobs on every push, so a PR has many runs, and the push run on `main` is one per PR.
- The M-1 numbers are job durations from the start of the job to its end, as the workflow API reports them. The queue time before the job is not in the number.
- A job past eleven minutes files a new question on D-116 (D-277). The Windows CI job is the one to watch, at 574 to 601 seconds on the last two PRs.
- The M-2 table has no rows until the first night. The night duration comes from the same API, on the night workflow run.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

After the first scheduled night writes `night.json` to `night-results`, a session opens PR-58 from `main` per the roadmap entry, D-274, and D-275. The same session starts the M-2 table with the duration of that night.

## Session 94: 2026-09-10, Claude Code

Author: Claude Code
Session: record the PR-58 decisions and the M-1 timing. Branch `docs/pr-58-decisions`.

### What this session did, and why

- The owner merged PR #35 as `1dc7e42`. Asked the two PR-58 questions and one on the M-1 timing in one batch, and D-274 to D-276 record the answers. OQ-144 holds the third question.
- D-274: any night record counts for the gate of PR-58, whatever event ran the night. The gate rule in the design doc and the roadmap reads "night" now, and the sequence still waits for one scheduled night before PR-58 opens (G-19).
- D-275: the gate checks the time and the commit. A record whose commit is not on the base branch of the PR fails, and the roadmap PR-58 entry gains exit test 8 for it. Exit test 6 names the commit in every failure message.
- D-276: the M-1 table fills before PR-58. The next PR holds the table.
- Session 84 moved to the archive.
- The automated pass asked for the resolved Phase 1 questions grouped under the heading of their date, and `2ce994f` does. The list under the 2026-09-08 heading held later resolutions too.

### State of the build

- `main` is at `1dc7e42`, the squash merge of PR #35. This branch holds the document commit above it, this entry above that, and the correction `2ce994f` above the entry.
- Remote head: `origin/docs/pr-58-decisions` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- No night has run. The branch `night-results` does not exist yet.

### In flight

This PR holds the three decisions. It changes no code, so the `review-override` label covers it (D-188, D-190). The M-1 table comes in the next PR (D-276). The first scheduled night runs at 03:00 UTC on 2026-09-11. No other PR is open.

### Traps and gotchas

- The PR-58 job needs the base branch for the ancestry check of D-275. A checkout of depth one does not hold it, so the job fetches the base ref before the check.
- A hand run of the night counts for the gate now (D-274), at the commit of the branch it runs on. The commit check of D-275 fails a record from a feature branch, so a hand run belongs on `main`.
- The PR-58 exit tests are eight now, and the design doc gate names five bad records.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A session fills the M-1 table in the Phase 1 roadmap from the push runs on `main` for the ten merged PRs since PR-3, one row per PR with the CI job durations per platform (D-276). After the first scheduled night, a session opens PR-58 from `main`.

## Session 93: 2026-09-10, Claude Code

Author: Claude Code
Session: record the PR-11 merge, and file the PR-58 questions before its code. Branch `docs/pr-11-merge-record`.

### What this session did, and why

- The owner merged PR #34 as `7487473` at 12:43 UTC, after the Codex repeat review of Session 92. The design doc PR-11 entry reads merged, the roadmap PR-11 entry has its status line, and the sequence marks item 19. The correction note records F-90.
- Exit tests 1 to 5 and 7 of PR-11 passed before the merge. Exit test 6 reads the first scheduled night, and the roadmap status line says so.
- Read the PR-58 entry for the questions that its code needs. The record of D-273 holds no event, and a hand run of the night workflow writes the same record, so the gate cannot tell a scheduled night from a hand run. The roadmap says nothing about the commit that a record names. OQ-142 and OQ-143 hold the two questions, each with a recommendation, and the session picked no default (D-124).
- No owner answer arrived, so `docs/decisions.md` did not change.
- Session 83 moved to the archive.

### State of the build

- `main` is at `7487473`, the squash merge of PR #34. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-11-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- The push checks on `7487473` passed: CI on the three platforms, bit identity, bots, determinism lint, and STE check.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The runner `mac-mini-m4` is online, from `/Volumes/SSD-1TB/actions-runner` on this machine. The branch `night-results` does not exist yet, and no night has run.

### In flight

This PR holds the merge record and the two questions. It changes no code, so the `review-override` label covers it (D-188, D-190). The first scheduled night runs at 03:00 UTC on 2026-09-11 on the runner, and it writes `night.json` to `night-results`. No other PR is open. PR-1 to PR-11 and PR-59 are merged. One scheduled night runs, then PR-58 opens, and M-1 and M-2 reach Gate 1. The M-1 table has no rows yet, and M-2 starts with the first night.

### Traps and gotchas

- A hand run of the night workflow writes the same record as a scheduled night, at the commit of the branch it runs on. Wait for the scheduled night, so that the first record is one that the PR-58 gate reads, whatever the answers to OQ-142 and OQ-143.
- The publish step of the night adds a worktree and an orphan branch inside the checkout of the runner, and the runner keeps that checkout between jobs. No second night has run yet. If a later night fails at the publish step, read `git worktree list` and `git branch` in the runner checkout first.
- The night workflow has `contents: write` and force-pushes `night-results` (D-273). Nothing else pushes there.
- The `review-override` label goes on after the last push and after the automated pass.

### Open questions that block progress

OQ-142 and OQ-143 block PR-58. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner answers OQ-142 and OQ-143, and a session records the answers as the next D-# ids. After the first scheduled night writes `night.json` to `night-results`, a session opens PR-58 from `main`, and its exit test 7 reads the real record. The same session starts the M-2 table with the duration of the first night.

## Session 92: 2026-09-10, Codex

Author: Codex
Session: repeat review PR-34 at effective head `4e1ea9e`. Branch `feat/pr-11-bots`.

### What this session did, and why

- Verified the provider gate. Sessions 89 and 91 identify Claude Code as the author of the PR-34 change and its correction. Codex is the eligible reviewer.
- Recomputed the effective head. `4e1ea9e` is the newest substantive commit. Later commits change only review and handoff metadata.
- Read the correction diff, the response record, the PR comments and replies, the PR-11 roadmap, the console collection, and the affected tests.
- Closed P1-1. Three independent full test runs pass 518 tests with no failure or skip. The collection serializes all four console-touching test classes, and the shape test scans every test directory.
- Updated `docs/reviews/pr-34.md` with the fixed finding and the verdict `Ready for owner merge` at effective head `4e1ea9e`.

### State of the build

- `main` is at `d3093cf`, the squash merge of PR #33. The effective PR-34 head is `4e1ea9e`.
- Remote head: `origin/feat/pr-11-bots` is `945004a`, pending the final metadata verification push.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures, 0 skips, three times in a row.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #34 needs this repeat-review record pushed. The substantive CI checks pass. The evaluate and review-gate checks must rerun after this metadata commit.

### Traps and gotchas

- The effective head is `4e1ea9e`, not the current metadata tip. D-184 excludes the review and session handoff paths.
- P1-1 stays fixed only while new tests that touch the process console carry the `Console` collection. The shape test enforces this for files under the test directories.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit the repeat-review record and this handoff entry. Push the branch. Fetch and verify the remote head and the review-gate result.

## Session 91: 2026-09-10, Claude Code

Author: Claude Code
Session: answer the PR #34 review. Branch `feat/pr-11-bots`.

### What this session did, and why

- Read the one P1 finding in `docs/reviews/pr-34.md`. Full merit: the bot tests call a command that writes its summary to the process console, and the bit-identity command test captures that console, so a full run could read the wrong line.
- Every test class that calls a command or redirects the console carries `[Collection(ConsoleCollection.Name)]` now, so xUnit runs the four one after another. `ConsoleCollectionTests.EveryConsoleTestIsInTheCollection` reads every test source and fails on a class that touches the console outside the collection.
- Three full runs with `-m:1` on the correction: 518 passed, 0 failed, 0 skipped, each time.
- F-90 records the finding, and `docs/reviews/pr-34-response.md` records the disposition. The correction is `f6ca5f6`, and `4e1ea9e` answers the automated pass on it: the shape test scans every test directory.

### State of the build

- `main` is at `d3093cf`, the squash merge of PR #33. This branch holds the PR-11 commit `8a7367f`, the correction `0536f7e`, the review commits, the correction `f6ca5f6`, and this entry above them.
- Remote head: `origin/feat/pr-11-bots` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 518 tests, 0 failures, three times in a row.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.

### In flight

PR #34 is open and it holds this branch. The automated pass approved `f6ca5f6` with one suggestion, and `4e1ea9e` answers it, so the effective head is `4e1ea9e`. A Codex repeat review at that head updates the same review record. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-10 and PR-59 are merged. PR-11 is open as PR #34. After its merge, one scheduled night runs on its own, then PR-58 opens, and M-1 and M-2 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `4e1ea9e`. The review record still names `0536f7e`, and the repeat review updates the head and the verdict together, with one verdict name in the Verdict section (D-269).
- A test that calls a command of the Tools project, or redirects the console, goes in the console collection, or the shape test fails.
- xUnit runs the classes of one collection one after another, so the console collection takes a little longer than the parallel run of four classes. The full suite still takes under three and a half minutes.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session repeats the review per the repeat procedure, at the effective head `4e1ea9e`, and updates `docs/reviews/pr-34.md` with the status of P1-1 and a new verdict.

## Session 90: 2026-09-10, Codex

Author: Codex
Session: review PR-34 at effective head `0536f7e`. Branch `feat/pr-11-bots`.

### What this session did, and why

- Verified the provider gate. Session 89 identifies Claude Code as the author of the substantive PR-34 change. Codex is the eligible reviewer.
- Recomputed the effective head. `8a7367f` holds the implementation, and `0536f7e` is the newest substantive commit. Later commits change only handoff metadata.
- Read the complete diff, the PR-11 roadmap entry and exit tests, the affected Core callers, the runner and workflows, the bit-identity sweep, the applicable decisions and questions, and every PR comment and author reply.
- Found P1-1. The new bot tests call commands that write to the process-global console while the existing bit-identity test captures that console. The full 517-test run failed 1 test because it captured bot output instead of the hash.
- Wrote `docs/reviews/pr-34.md` with the verdict `Changes required` at effective head `0536f7e`.

### State of the build

- `main` is at `d3093cf`, the squash merge of PR #33. The effective PR-34 head is `0536f7e`.
- Remote head: `origin/feat/pr-11-bots` is `00e4c623700cefa035e94a5aed1672bd4300b7e3`, with no ahead count after the review commit.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 517 tests, 0 failures, 0 skipped.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #34 needs a correction for P1-1. No other PR is open.

### Traps and gotchas

- The effective head is `0536f7e`, not the remote metadata tip. D-184 excludes the review and session handoff paths.
- The review-gate check is neutral until this review record reaches the PR head. The check must rerun after the push.
- The scheduled night workflow has `contents: write` so it can force-push the single `night.json` record to `night-results` under D-273.
- The focused bot and bit-identity tests passed once, but the full suite reproduced the console race. A focused pass does not close P1-1.

### Open questions that block progress

P1-1 blocks PR #34. OQ-99 is open, and it blocks no other work.

### Next concrete action

Commit the review record and this handoff entry. Push the branch. The author must serialize or remove the console race, then request a repeat review at the new effective head.

## Session 89: 2026-09-10, Claude Code

Author: Claude Code
Session: PR-11, the bot harness, the two policies, the runner, and the night workflow. Branch `feat/pr-11-bots`.

### What this session did, and why

- Started PR-11 from `main` at `d3093cf`, the squash merge of PR #33, as Session 88 planned. The commit `8a7367f` holds the code, the workflows, the tests, and the roadmap note. PR #34 holds the branch.
- `RngStream.Bot` is the fifth value of D-159 (D-272). The bound of `Rng.ForStream` names it, and the sweep draws from it, so the known answer moves from `d8943df12fefcbee` to `6ec00e90c1c85cdb`. No simulation number changed, so the version stays 6.
- `Core/Bots/`: `IBotPolicy` with a name, a promise of progress, and one intent per tick. `RandomWalker` holds one movement, one yaw rate, and one jump choice for up to sixty ticks, then draws again. `GreedyDescender` walks the reachability path to the stairwell with the jump in place of PR-9 exit test 8, descends at every stairwell, and ascends at the deepest floor of the content. `BotRun.Play` plays one run to one of the four end states of D-270 with the budgets of D-271, and a crash carries the exception text.
- `Tools/BotRunner/`: `bot-run` loads the content of the checkout, plays a seed range with a policy, and writes two log lines per run through the PR-4 logger into one file per run. It exits nonzero on a crash, or a softlock on a policy that promises progress. `night-record` writes the record of D-273.
- `.github/workflows/bots.yml` plays one hundred seeds per policy on every PR. `.github/workflows/night.yml` runs the scheduled night on the macOS runner at 03:00 UTC: five thousand seeds per policy, the reachability sweeps at one hundred thousand seeds, and the record on the orphan branch `night-results`, whatever the outcome.
- The stairwell test drives the greedy descender until it asks for the stairwell choice, instead of a walker of its own.
- Exit tests 1 to 4 and 7 pass among 517 tests. A local run of the PR job gave one hundred budget ends for the walker and one hundred bottom ends for the descender, with zero crashes. The bots job passed on the push too.
- The automated pass on `8a7367f` found two edges with merit. A broken build left no binary for `night-record`, so the branch would keep a stale record: the publish step writes the failure record in shell when the file is absent now. The seed loop could wrap at the largest seed: the loop runs by the count of seeds now, with a bound of one million. Commit `0536f7e` holds both, with a test for the ranges.

### State of the build

- `main` is at `d3093cf`, the squash merge of PR #33. This branch holds the PR-11 commit `8a7367f` above it, and this entry above that.
- Remote head: `origin/feat/pr-11-bots` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 517 tests, 0 failures, in under five minutes. The hundred-seed descent takes about thirty seconds of it.
- `det-lint`: 0 findings. Core 0 in 61 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `6ec00e90c1c85cdb`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #34 is open and it holds this branch. The automated pass approved `8a7367f` with two edges, and `0536f7e` answers both, so the effective head is `0536f7e`. A Codex session reviews the PR at it. The review focus is determinism, errors, input and CI boundaries, and test quality (roadmap PR-11). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-11 and PR-59 are merged or open. After the PR-11 merge, one scheduled night runs on its own, then PR-58 opens, and M-1 and M-2 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The night workflow pushes to the branch `night-results` with `contents: write`. The first scheduled night at 03:00 UTC after the merge makes the first record, and `workflow_dispatch` runs one by hand. The runner needs the `night-results` branch to accept a force push.
- A bot policy holds state that is not simulation state. The record holds the intents, so a replay needs no policy.
- A descender run takes about a quarter of a second, and a walker run about a tenth. Five thousand of each is about thirty minutes of the night, before the sweep.
- The random walker never sets the stairwell bits, so it never descends and its run ends by budget at 36000 ticks.
- `BotRun.Play` catches every exception, because the harness must run the next seed and the log must hold the fault (D-270). No other Core code catches without a rethrow.
- `RngTests.FloorStreamsAreDistinct` and `RngStreamsDiffer` list the four simulation streams by name. The Bot stream joins the sweep, and the bit-identity test walks the enum, so a sixth stream fails that test until the sweep names it.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #34 per the `pr-review` skill at the effective head `0536f7e`, reads the PR comments and the author replies into the review, and writes `docs/reviews/pr-34.md`. The review confirms the bit-identity change under G-20 and the workflow permissions under the input and CI boundaries.

## Session 88: 2026-09-10, Claude Code

Author: Claude Code
Session: the PR-11 merge record and the PR-58 questions. Branch `docs/pr-11-merge-record`.

### What this session did, and why

- Recorded the PR-11 merge and filed the two PR-58 questions.
- The owner had not answered the questions when this session ended.

### State of the build

- `main` was at `7487473`. The document branch held the merge record and the handoff entry.
- The build, tests, determinism lint, and STE check passed.

### In flight

The first scheduled night and the answers to OQ-142 and OQ-143 were in flight.

### Traps and gotchas

The night publish step uses an orphan branch and a worktree in the runner checkout.

### Open questions that block progress

OQ-142 and OQ-143 blocked PR-58.

### Next concrete action

Record the owner answers, then wait for the first night record before opening PR-58.

## Session 87: 2026-09-10, Codex

Author: Codex
Session: review PR #32 at effective head `60bdb17`. Branch `fix/review-gate-one-verdict`.

### What this session did, and why

- Verified the provider gate. Session 86 identifies Claude Code as the author of the substantive PR-32 change. Codex is the eligible reviewer.
- Recomputed the effective head. `7a388f6` holds the implementation, and `60bdb17` is the newest substantive commit because the skill file lies outside the metadata set of D-184. Later commits change only review and handoff metadata.
- Read the complete diff, the review-gate callers and tests, D-269, OQ-137, the roadmap, the PR comments and author replies, and the current remote checks.
- Found no actionable defect. Wrote `docs/reviews/pr-32.md` with the verdict `Ready for owner merge` at effective head `60bdb17`.

### State of the build

- `main` is at `1ce78c6`, the squash merge of PR #31. The effective PR-32 head is `60bdb17`.
- Local build and test pass. `dotnet build` reports 0 warnings and 0 errors. `dotnet test` reports 509 tests and 0 failures.
- `det-lint` reports 0 findings in 57 Core files and 0 Game files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` returns `d8943df12fefcbee`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.
- The code, lint, STE, Gitar, and Windows checks pass. The review-gate check waits for the review record, and the macOS checks were pending before the metadata push.

### In flight

The review record and this handoff entry are pushed in `5adec71`. The owner can merge after all platform checks pass; the remote review-gate check already reads `Ready for owner merge` at effective head `60bdb17`.

### Traps and gotchas

- The effective head is `60bdb17`, not the remote tip after the review metadata commit. D-184 excludes only the review and handoff paths.
- The parser counts verdict names in the full Verdict section. The review skill now requires the reason to name no other verdict (D-269).
- The review-gate check failed once because the record used a literal marker before its heading. Commit `5adec71` removes that marker, and the new evaluate and review-gate checks pass.

### Open questions that block progress

None. OQ-99 remains open, and it blocks nothing.

### Next concrete action

Wait for the pending platform checks. Then the owner can merge PR #32.

## Session 86: 2026-09-10, Claude Code

Author: Claude Code
Session: harden the review gate against a second verdict name (D-269). Branch `fix/review-gate-one-verdict`.

### What this session did, and why

- The owner merged PR #31 as `1ce78c6`, after a Codex repeat review with the verdict `Ready for owner merge` at the effective head `1b58d3a` (Session 85).
- The gate read that record as "Changes required" twice: the record kept a previous-verdict line inside the Verdict section, and the gate took the first verdict name after the first text match of the heading, so a history section named "Verdict history" matched as a prefix. Two format edits of the record by the author, with the owner's choice and a PR comment, let the gate read the approval.
- The owner asked how to stop that and chose three layers. The commit `7a388f6` holds them. PR #32 holds the branch.
- `ReviewRecord.FindVerdict` reads the section under the line that is exactly `## Verdict`, up to the next heading, and it fails a section with two verdict names with a message that names both in document order (D-269, F-89).
- `EveryRepositoryReviewRecordHoldsOneVerdict` parses every review record of the checkout, so a reviewer sees a second name in `dotnet test` before the push. Two more tests cover the prefix match and the two names.
- The `pr-review` skill repeat procedure keeps one verdict name in the Verdict section and puts an earlier verdict under a heading that starts with another word.
- D-269 records the rule, OQ-137 the question, and F-89 the finding.

### State of the build

- `main` is at `1ce78c6`, the squash merge of PR #31. This branch holds the fix commit `7a388f6` above it, and this entry above that.
- Remote head: `origin/fix/review-gate-one-verdict` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 509 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 57 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`, as PR #31 left it. No Core change. The simulation version is 6.

### In flight

PR #32 is open and it holds this branch. It changes the tools, so it needs a Codex review at the effective head `60bdb17`, the skill edit that answered the automated pass. The pass approved `7a388f6` with one suggestion, and every comment has its answer. The merge record of PR-10 and the PR-11 questions come in the next session, after this fix or beside it. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-10 and PR-59 are merged. PR-11 remains, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The gate now fails a review record whose Verdict section names two verdicts, and it names both. A repeat review keeps one name there and puts the earlier verdict in a section such as `## Earlier verdicts`, above it. The count reads the prose too, so the reason after the verdict names no other verdict. The automated pass raised that edge, and the skill says it now.
- The gate parses `docs/reviews/pr-31.md` with the heading `## Earlier verdicts` that the format edits gave it, and the repository test reads every record, so a record that breaks the rule fails `dotnet test` on every branch.
- The effective head is `60bdb17`, because the skill file lies outside the metadata set of D-190. This PR changes no Core file, so the bit-identity hash stands.
- The PR-10 merge record and the PR-11 questions are still to do: the design doc PR-10 entry, the roadmap status line, and the sequence item 18.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #32 per the `pr-review` skill at the effective head `60bdb17`, reads the PR comments and the author reply into the review, and writes `docs/reviews/pr-32.md`. In parallel or after, a session records the PR-10 merge and asks the PR-11 questions before its code.

## Session 85: 2026-09-09, Codex

Author: Codex
Session: repeat review PR #31 at effective head `1b58d3a`. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Recomputed the effective head. The fix commit `1b58d3a` changes Core code, tests, and the design record. Later commits change only review, handoff, and archive metadata.
- Read the author response, the new diff, the full changed source and tests, the roadmap, the decisions, the questions, the PR comments, and the current checks.
- Verified P2-1 fixed in `1b58d3a`. The spread now samples one angle and one roll, and the test checks five axes against the exact half angle.
- Verified P2-2 fixed in `1b58d3a`. The arc solver rejects invalid and non-finite speed, gravity, and point inputs with context.
- Updated `docs/reviews/pr-31.md` with both findings marked fixed and the verdict `Ready for owner merge` at effective head `1b58d3a`.

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. The PR effective head is `1b58d3a`. The remote tip before this review record update was `33dfb2a`. The review record was published in `e5cb108`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 506 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes after the sandbox permission failure is rerun in the required execution context.
- Current GitHub checks pass for bit identity, compare, CI, determinism lint, STE, and Gitar. The review-gate evaluate job fails on the old verdict and must rerun after this record update.

### In flight

PR #31 is ready for owner merge after the updated review record reaches the remote branch and the review-gate check reads the new verdict. No other PR is open.

### Traps and gotchas

- The effective head is `1b58d3a`, not the remote metadata tip. D-184 excludes the review, session handoff, and archive paths only.
- The spread is uniform in the angle, not in the solid angle. D-266 names the half angle, so this is valid.
- The sweep hash is `d8943df12fefcbee`. Do not restore the prior hash `3220e92dcbca55a2`.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The review record and this handoff entry are published in `e5cb108`. Fetch, verify that the branch has no ahead count, and check that the review-gate result approves effective head `1b58d3a`.

## Session 84: 2026-09-09, Claude Code

Author: Claude Code
Session: answer the PR #31 review. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Read the two P2 findings in `docs/reviews/pr-31.md`. Both have full merit.
- P2-1: the spread drew a yaw offset and a pitch offset, so a shot could leave the cone of D-266 by up to 6 degrees at the corner. The draw is one angle from the direction and one roll around it now, so every shot stays inside the half angle. The draw count per shot stays two.
- P2-2: the arc solver took a speed of zero, a negative gravity, or a value that is not finite. It rejects each with a context error now (T-2).
- The sweep hash moved from `3220e92dcbca55a2` to `d8943df12fefcbee`, because every shot of the sweep turns another way. The simulation version stays 6, because it rose in this PR already (G-20).
- `SpreadStaysInsideTheCone` reads the half angle from the definition over five axes, and `ArcSolverRejectsABadSpeedOrGravity` covers seven bad inputs. 506 tests in total.
- F-87 and F-88 record the findings, and `docs/reviews/pr-31-response.md` records the dispositions.
- The repeat review approved `1b58d3a`, and the gate still read "Changes required": the record held a previous-verdict line above the new verdict, and the gate takes the first verdict name after the heading. With the owner's choice, this session moved that line into an "Earlier verdicts" section above the Verdict section. A first try named that section "Verdict history", and the gate read it as the Verdict section, because the parser matches the heading text as a prefix. The verdict text is as the reviewer wrote it, and the gate output names the editing commit (D-198).

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. This branch holds the PR-10 commit `c214d03`, the review commits, the correction `1b58d3a`, and this entry above them.
- Remote head: `origin/feat/pr-10-projectiles` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 506 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 57 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`. The simulation version is 6.

### In flight

PR #31 is open and it holds this branch. The repeat review approved the effective head `1b58d3a`, and the record reads that verdict after the format edit. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 is open as PR #31. PR-11 remains, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `1b58d3a`, and the review record names it with the verdict `Ready for owner merge`.
- The review gate reads the first verdict name after the first heading that starts with "## Verdict", so a section named "Verdict history" counts as the Verdict section. A note that names an earlier verdict belongs in a section whose heading starts with another word, above the Verdict section. A follow-up PR makes the parser match the exact heading and refuse a section with two verdict names.
- The sweep hash moved without a version change, because the version rose in this PR already. A review that sees `d8943df12fefcbee` must confirm it and not restore `3220e92dcbca55a2`.
- The spread is uniform in the angle from the axis and not in the solid angle of the cone, so shots gather near the axis less than a uniform disc would. D-266 names the half angle alone.
- The cone sides come from the world up, or from the world right for a vertical direction, so a direction within 2.6 degrees of vertical takes the second axis. The angle from the axis is exact either way.
- The reviewing provider reads the existing PR comments and author replies into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge PR #31 after the remote review-gate check reads `Ready for owner merge` at the effective head `1b58d3a`.

## Session 83: 2026-09-09, Codex

Author: Codex
Session: review PR #31 at effective head `c214d03`. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Verified the provider gate. Session 82 identifies Claude Code as the author of PR-10, and Codex is the eligible reviewer.
- Read the complete PR diff, the PR-10 roadmap entry and exit tests, the affected Core callers, the content validator, the replay hash, the bit-identity sweep, the decisions, the questions, and the PR comments and author reply.
- Found two P2 findings. The spread code samples a square of yaw and pitch offsets, so a shot can leave the declared cone. `ArcSolver` accepts zero or negative physics inputs without a contextual error.
- Wrote `docs/reviews/pr-31.md` with the verdict `Changes required` for effective head `c214d03`.

### State of the build

- `main` is at `4687081`, the squash merge of PR #30. The PR tip is `b48cc71`, with metadata commits `ec76f0e` and `b48cc71` after effective head `c214d03`.
- `dotnet build`: pass. `dotnet test`: 500 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `3220e92dcbca55a2`. The simulation version is 6.
- The Godot 4.7.2 headless build check passes.
- GitHub CI has passing bit-identity, compare, CI, determinism lint, STE, and Gitar checks. The review-gate evaluate job fails because the review file was absent before this session. The review record now names the two required corrections.

### In flight

PR #31 needs the two findings corrected and a new cross-provider review at the new effective head. The review record and this handoff entry are pushed at `b15c784`.

### Traps and gotchas

- The effective head is `c214d03`, not the PR tip, because the two later commits change only the metadata paths allowed by D-184.
- The spread test permits 22 degrees for a 15-degree definition. That threshold hides the square-sampling defect.
- The handoff records the one-tick lifetime grace described by the implementation. This review did not raise it because the prior session treats it as intentional and the projectile age remains within the lifetime.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The author corrects P2-1 and P2-2, runs the full gates, and asks for a repeat review. The repeat review keeps the finding ids and updates the same review record to the new effective head.

## Session 82: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-10, the projectile simulation, the arc solver, and the shot of the attack bit. Branch `feat/pr-10-projectiles`.

### What this session did, and why

- Started PR-10 from `main` at `4687081` and added the projectile simulation, arc solver, attack-bit shot, tests, content, and roadmap note.
- The projectile spread, shot origin, fire timing, replay state, and simulation version 6 follow D-265 to D-268 and G-20.
- The six PR-10 exit tests pass, with 500 tests in total.

### State of the build

- `main` is at `4687081`. The PR-10 implementation commit is `c214d03`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 500 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings. The Godot 4.7.2 headless build check passes.
- `bit-identity`: `3220e92dcbca55a2`. The simulation version is 6.

### In flight

PR #31 is open. A Codex session reviews the PR at the effective head.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 is open. PR-11 remains, followed by M-1, M-2, and PR-58.

### Traps and gotchas

- The Projectile stream is not part of the loop state hash. A replay comparison must include a tick with a projectile in flight.
- The shot starts at the marched shoulder point. A shot from inside rock reports an error.
- The fixed-step projectile path lands a little short of the continuous arc.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #31 per the `pr-review` skill and writes `docs/reviews/pr-31.md`.

## Session 81: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-59 merge, and answer the PR-10 questions before its code. Branch `docs/pr-59-merge-record`.

### What this session did, and why

- The owner merged PR #29 as `5ae0a24`, after a Codex review with no finding at the effective head `923e2a4` (Session 80).
- The design doc PR-59 entry reads merged, the roadmap PR-59 entry has its status line, and the sequence marks item 17.
- Asked four owner questions in one batch, and D-265 to D-268 record the answers. OQ-133 to OQ-136 hold the questions.
- D-265: until PR-15 gives the loadout a weapon, the attack bit fires the first projectile definition of the content set, in ordinal path order.
- D-266: the projectile schema gains the required field `spreadHundredths`, the half angle of the spread cone in hundredths of a degree. The Projectile stream draws the spread of each shot.
- D-267: a shot fires once per press of the attack bit: a tick where the bit is set and was clear on the tick before.
- D-268: a shot starts at the shoulder point of D-242 and flies toward the first solid cell the aim ray meets within 100 meters, or the point of the ray at 100 meters.
- The PR-10 roadmap entry holds the four files of the test-only set, the spread field, the shot rules, and the projectile state in the hash with the simulation version 6. The design doc paragraph says the same.

### State of the build

- `main` is at `5ae0a24`, the squash merge of PR #29. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-59-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures, as PR #29 left them. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.

### In flight

PR #30 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The session applies the label after the automated pass, when every comment has its answer. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 and PR-59 are merged. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- PR-10 changes the loop state: the live projectiles join the hash after the run end, in flight order, so the simulation version rises to 6 and the bit-identity hash moves (G-20). The sweep intents set the attack bit on some ticks already, so the sweep fires shots on its own once the loop reads the bit.
- PR-10 changes the projectile schema: `spreadHundredths` joins the required list, the two existing files gain a value, and the content hash of the test set moves.
- The first projectile definition of the content set is the first in ordinal path order, so the file names of the test-only set decide which one the attack bit fires (D-265).
- The aim ray march for the shot target reads the grid with `GridRay`, as the boom sweep does. A ray that meets no solid cell within 100 meters ends at the point of the ray at 100 meters (D-268).
- A shot that starts at the shoulder point can start inside rock when the player hugs a right wall (OQ-118). D-249 marches the shoulder offset first, and the shot origin takes that marched point.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #30. Then a new session starts PR-10 on a short branch: the spread field, the four test-only definitions, the projectile simulation, the arc solver, the shot from the attack bit, the projectiles in the hash with the simulation version 6, and the six exit tests, under D-159, D-231, D-242, D-247, and D-265 to D-268.

## Session 80: 2026-09-09, Codex

Author: Codex
Session: review PR-59 as PR #29 at effective head `923e2a4`. Branch `feat/pr-59-detail`.

### What this session did, and why

- Verified the provider gate. Session 79 identifies Claude Code as the author of the substantive PR-59 commits, and Codex is the eligible reviewer.
- Read the complete diff, the PR-59 roadmap entry and exit tests, the affected Core callers, content and validators, reachability, replay version use, bit-identity sweep, decisions, questions, and the PR comments and replies.
- Found no actionable defect. The detail pass preserves tunnel reachability, the water probe follows D-264, the simulation version rises to 5, and the block set follows D-259.
- Wrote `docs/reviews/pr-29.md` with the verdict `Ready for owner merge` at effective head `923e2a4`.

### State of the build

- `main` is at `45dbaf5`, the squash merge of PR #28. The reviewed effective head is `923e2a4`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 54 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.
- The Godot 4.7.2 headless build check passes.
- GitHub CI, both bit-identity platform jobs and the compare job, determinism lint, STE check, evaluate, and Gitar pass at the PR head. The review-gate result is neutral until the review record exists.

### In flight

PR #29 holds the review record and needs the metadata commit and the session handoff pushed. The owner can merge after the remote review-gate check reads `Ready for owner merge` at effective head `923e2a4`.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59 is open as PR #29. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `923e2a4`, because its design and roadmap changes are substantive under D-184. The handoff and review commits remain metadata commits.
- The review-gate result is neutral before this review record exists. The metadata commit should trigger the gate again.
- D-264 revises only the water probe wording of D-262. The half jump velocity, quarter gravity, and one-block apex still apply.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit the review record and handoff entry. Push the branch. Fetch and verify that the remote head has no ahead count and that the review-gate check reads the approved effective head.

## Session 79: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-59, the mine detail pass, the block ids, and the water rule. Branch `feat/pr-59-detail`.

### What this session did, and why

- Started PR-59 from `main` at `45dbaf5`, the squash merge of PR #28, as Session 78 planned. The commit `787c851` holds the code, the tests, and the roadmap note. PR #29 holds the branch.
- `BlockId` declares 2 hewn stone, 3 timber beam, 4 ore vein, 5 still water, 6 rubble, and 7 plank (D-259). The grid bound accepts 0 to 7, and `IsSolid` lets air and still water through (D-258).
- `Core/Procgen/DetailPass.cs` runs after the shafts and before the stairwell, in this order: collapses, pools, walls, pillars. A collapse fills the last stamp of a walker with no dependent, in cells that walker alone dug, with a rubble heap one to three rows high. A pool is a rectangle of two or three cells a side in a chamber of at least twelve cells, over rock, with a dry floor cell beside every pool cell. A wall block replaces rock that borders air with rock over it, by band. A pillar is one column with dry chamber floor on all eight sides, one try per twenty cells.
- `DigPlan` records which walker dug each air cell, which walkers have a dependent (a chamber, a drift, or a later walker on their trail), and where each walker ended.
- `PlayerBody` scales the speeds and the jump velocity by one half and gravity by one quarter while the column under the feet is water: the feet in a water cell, or the body in the air over water (D-261, D-262).
- The simulation version is 5 (D-260). The sweep content holds one template per band on floors 1 to 3, and the known answer moves from `036df5c08e2682e3` to `62c5e1d152fe94fe`.
- The six exit tests of the roadmap entry pass. The reachability sweeps of PR-9 exit test 1 and PR-59 exit test 1 read one dig per seed through a shared report.
- `TunnelCrossSection` steps over chamber cells, water cells, the cells around a pillar, and the cells around a collapse, because those are not tunnel cells (D-166).
- The automated pass on `787c851` approved the code with two suggestions and one CI notice. The first suggestion asked for a decision on the water probe, and the owner gave it: D-264 revises D-262 in part, and OQ-132 holds the question. The second asked for a direct assertion of the quarter gravity in the air over water, and the test has it now. The CI notice on the absent review record has no merit (D-251). The correction commit holds the decision, the test, and this note.

### State of the build

- `main` is at `45dbaf5`, the squash merge of PR #28. This branch holds the PR-59 commit `787c851` above it, and this entry above that.
- Remote head: `origin/feat/pr-59-detail` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 476 tests, 0 failures. The suite takes about three minutes, and the shared reachability sweep takes most of it.
- `det-lint`: 0 findings. Core 0 in 54 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `62c5e1d152fe94fe`. The simulation version is 5.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #29 is open and it holds this branch. The automated pass approved `787c851`, and the correction commit answers its two suggestions. The effective head is that commit, and a Codex session reviews the PR at it. The review focus is determinism, content, test quality, and replay (roadmap PR-59). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59 is open as PR #29. PR-10 and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The water probe reads the column under the feet (D-264). A probe of the feet cell alone gives an apex of 1.05 blocks at 1.6 times the ticks, because the quarter gravity goes as soon as the feet rise out of the cell.
- A collapse must never fill a cell of a walker with a dependent. The first version filled cells of any walker that were dug by that walker alone, and a walker that looped back cut its own path to a chamber it dug earlier. Seed 418 of floor 14 found it.
- The pillars come last in the detail pass, so no wall block lands on a pillar and no pool opens under one. A pool cell needs air over it and rock under it.
- The pillars and the pools are cells with their floor row, not columns, because two chambers can stack on one column at two rows.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #29 per the `pr-review` skill at the effective head, reads the PR comments and the author replies into the review, and writes `docs/reviews/pr-29.md`. The review confirms the simulation version 5 and the bit-identity change under G-20.

## Session 78: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-9 merge, and answer the PR-59 questions before its code. Branch `docs/pr-9-merge-record`.

### What this session did, and why

- The owner merged PR #27 as `336fe4e`, after a Codex review with no finding at the effective head `bd1366e` (Session 77).
- The design doc PR-9 entry reads merged, the roadmap PR-9 entry has its status line, and the sequence marks item 16.
- Asked six owner questions in two batches, and D-258 to D-263 record the answers. OQ-126 to OQ-131 hold the questions.
- D-258: still water is not solid. A pool is a one-block depression, and a body walks and jumps through it more slowly. The search reads water as air.
- D-259: the block ids take the order of D-210: 2 hewn stone, 3 timber beam, 4 ore vein, 5 still water, 6 rubble, 7 plank. D-239 is revised in part.
- D-260: PR-59 raises the simulation version to 5, because a floor with other blocks is another simulation.
- D-261 to D-263: the walk and sprint speeds take the factor one half in water. The jump velocity takes one half and gravity one quarter, so the apex stays at one block and the rise takes twice as long. The three factors are Core constants in `PlayerBody`.
- The automated pass on the first push found that the first text of D-262 gave both numbers one factor of one half, which halves the apex. The owner took the correction, one half for the velocity and one quarter for gravity, and F-86 records it. The correction commit answers the pass.
- The PR-59 roadmap entry holds the ids, the water rules, the version rise, and two new exit tests. The design doc paragraph and gate say the same.

### State of the build

- `main` is at `336fe4e`, the squash merge of PR #27. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-9-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 469 tests, 0 failures, as PR #27 left them. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `036df5c08e2682e3`. The simulation version is 4.

### In flight

PR #28 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The automated pass approved `930b659`, every comment has its answer, and the label is on. The owner asked that the session apply the label itself from now on, after the last push and the pass. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-9 are merged. PR-59, PR-10, and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Water changes the body: `PlayerBody` reads the block of the feet cell on each tick, and a water cell scales the speeds and the jump velocity by one half and gravity by one quarter (D-261, D-262). The apex stays at one block, so the reachability search of PR-9 reads water as air and needs no pit rule. One factor on both numbers halves the apex (F-86).
- A wall block of the detail pass replaces rock that borders air and removes no air, so D-166 holds as PR-9 left it. Collapses and pillars remove air, and the PR-9 sweep runs again over the result.
- PR-59 raises the simulation version to 5 and moves the bit-identity known answer (D-260, G-20). The sweep folds three floors of its own content set, so the detail pass moves the hash on its own.
- The block ids are part of the grid (D-259). `VoxelGrid.Set` holds the explicit bound of declared ids, and `EveryDeclaredBlockIsAccepted` walks the enum, so a new value fails the test until the bound names it.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #28. Then a new session starts PR-59 on a short branch: the block ids of D-259 in `BlockId` and the grid bound, the water rule in `PlayerBody`, the detail pass with collapses, pillars, and the blocks by band, the simulation version 5, and the six exit tests, under D-210, D-239, D-253, D-254, and D-258 to D-263.

## Session 76: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-9, the mine dig plan, the reachability search, and the stairwell transition. Branch `feat/pr-9-dig-plan`.

### What this session did, and why

- Started PR-9 from `main` at `3e5cbd3`, the squash merge of PR #26, as Session 75 planned. The commit `1c75fc5` holds the code, the content, the tests, and the roadmap note. PR #27 holds the branch.
- The three floor templates gain `sizeX`, `sizeY`, and `sizeZ` by band (D-252). The validator rejects a size past D-164, and a size below 24 by 7 by 24, because the dig plan keeps a shell of rock and needs six rows for one floor.
- `content/chambers/` holds eight chamber kinds with weights from 8 to 40 (D-255). The fields are `id`, `weight`, `boxCountMin`, `boxCountMax`, `boxSizeMin`, and `boxSizeMax`. A box side is at least three (D-166).
- `Core/Procgen/`: `FloorGenerator` digs floor n from the seed, the floor number, and the content set. `ChamberBudget` draws kinds with two feasibility bounds, so the sum lands in the window and the count in the range (D-167). `ChamberFootprint` unions boxes, rounds corners, fills notches, and repairs every cell to a run of three. `DigCanvas` holds the two carve rules. `DigPlan` runs the walkers: a gallery of radius 2, drifts of radius 1, ramps of two to five one-block steps, chambers at the walker, and three by three shafts. `Reachability` is the breadth-first search under D-165.
- The stairwell: the interact bit descends and bit 9 ascends, at the stairwell alone (D-257). `Button.Ascend` is 0x0200, the assigned mask is 0x03FF, and the reserved mask is 0xFC00.
- The loop takes the seed and the content set, and the replay takes the content set (D-236). The state gains the floor number and the run end, the hash reads both after the body, and the simulation version is 4 (G-20). The torn-tail line reads the floor from the state (D-228).
- `Rng.ForStream` gains a floor argument. Floor zero is the run stream of D-159, so every earlier stream and the RNG known answer stand.
- The bit-identity sweep digs three floors of its own content set and replays on a dug floor. The known answer moves from `afed0063a6cf8a50` to `036df5c08e2682e3`.
- The body tests step `PlayerBody` on the flat floor with an explicit yaw, and the loop tests run on dug floors from the repository content. `RepositoryContentSource` is a shared test file now.
- The PR template gains the gitar gate line (D-250).
- The automated pass on `1c75fc5` gave one code finding and two CI notices. The code finding had merit: the loader kept the order of the source, so the chamber list and the budget draw took the order of the file system, and the Windows CI leg found it through `AFloorWithoutOneTemplateIsAnError`. Commit `bd1366e` sorts the content files by ordinal path in the loader, adds the regression test `TheSourceOrderDoesNotReachTheLists`, and names the template that covers floor 1 in the test. The CI notice on the Windows failure was the same defect. The CI notice on the absent review record had no merit (D-251), and it got its reply on the PR. The second pass on `bd1366e` approved the code and repeated the review-record notice, which got the same reply.

### State of the build

- `main` is at `3e5cbd3`, the squash merge of PR #26. This branch holds the PR-9 commit `1c75fc5`, the correction `bd1366e`, and the handoff entries above them.
- Remote head: `origin/feat/pr-9-dig-plan` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 469 tests, 0 failures. The suite takes about ninety seconds, and `EveryChamberReachable` takes about forty-five of them at five thousand seeds.
- `det-lint`: 0 findings. Core 0 in 53 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `036df5c08e2682e3`. The simulation version is 4.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #27 is open and it holds this branch. The automated pass approved `bd1366e`, and every comment has its answer. The PR is ready for a Codex review at the effective head `bd1366e`. The review focus is determinism, content, replay, and test quality (roadmap PR-9). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-8 are merged. PR-9 is open as PR #27. PR-59, PR-10, and PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Three choices sit inside the roadmap scope and no decision names them: the run end is a hash field beside the floor number (D-160), an intent that sets both stairwell bits ascends, and the floor stream is a third argument of `Rng.ForStream`. The review or the owner can ask for a decision on any of them.
- The two carve rules are the proof of reachability. A ramp starts past the stamp around the walker, and its landing reaches one brush radius past the new position, so a second ramp right after the first leaves no gap. A job digs no ramp before its first flat stamp, because the job starts on a cell of another walker. The first version lacked both, and seed 2 of floor 3 found the gap.
- A chamber cell needs a run of three along X or along Z, or the cross-section test fails on it. The corner pass can leave a cell without one, and the repair pass adds the two X neighbors.
- `Reachability.Landing` gives minus one for no move. A step up needs a third air cell over the start, for the jump.
- The content hash of a record must match the hash of the content set that the replay takes. The replay tests use `TestWorld.Content.Hash` in the header, and the header tests keep the fixed hash.
- The typed lists of a content set are in ordinal path order, so `Floors[0]` is the deep band and not the working mine. A test that needs the template of a floor asks `FloorGenerator.TemplateFor`.
- The night count of exit test 1 runs when `WYC_NIGHT_SWEEP` is `1`. PR-11 gives the night job its own switch.
- The loop constructor digs floor 1, so a test that builds one thousand loops digs one thousand floors. `CameraNeverInsideSolid` takes seven seconds for that reason.
- The override label goes stale on any push outside the metadata set (D-190). The effective head is the newest commit outside that set.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #27 per the `pr-review` skill, at the effective head `bd1366e`, reads the PR comments and the author replies into the review, and writes `docs/reviews/pr-27.md`. The review confirms the simulation version 4 and the bit-identity change under G-20.

## Session 75: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-8 merge, and answer the PR-9 questions before its code. Branch `docs/pr-8-merge-record`.

### What this session did, and why

- The owner merged PR #23 as `a3bdf20`, after a Codex review with one P2 finding, the correction `4e98470`, and a repeat review with the verdict `Ready for owner merge` (Sessions 72 to 74).
- Audited every document against the merged state. The registers were complete: D-248 to D-251, OQ-117 to OQ-119, F-82 to F-85, and the review records `pr-23.md`, `pr-23-response.md`, and `pr-25.md` all sit on `main`.
- Four stale places are corrected. The design doc PR-8 entry and its sequence line said open, and the roadmap said open. The roadmap sequence had no mark for PR-3 to PR-8, and its open questions still listed OQ-12, which D-210 resolved on 2026-09-08.
- Asked six owner questions in three batches, and D-252 to D-257 record the answers. OQ-120 to OQ-125 hold the questions.
- D-252: the floor size per band in the floor template: 48 by 12 by 48, 72 by 16 by 72, and 96 by 20 by 96.
- D-253: a floor is a mine dig plan: a main gallery, side drifts, chambers as smoothed box unions with pillars, shafts and ramps, and collapses. Every tunnel takes a brush of at least three by three. The owner asked for a floor far more random, varied, and detailed than rectangles, whatever the implications.
- D-254: PR-9 carves raw stone and air with the exit tests, and a new PR-59 adds the detail and the D-210 block ids. D-239 is revised in part.
- D-255: chamber kinds are a content type, `content/chambers/*.json`, with an id, a weight, a box count range, and a box size range.
- D-256: the spawn is in the first chamber, and the stairwell is in the chamber with the longest walkable path.
- D-257: at the stairwell, the interact bit descends and bit 9 ascends, so the record carries the choice. D-232 is revised in part.
- The design doc and the roadmap hold the rewritten PR-9 entry and the new PR-59 entry, with chambers and tunnels in place of rooms and corridors. The roadmap sequence places PR-59 after PR-9.

### State of the build

- `main` is at `a3bdf20`, the squash merge of PR #23. This branch holds the document commit above it.
- Remote head: `origin/docs/pr-8-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 426 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 43 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `afed0063a6cf8a50`. The simulation version is 3.

### In flight

PR #26 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The label goes on after the automated pass, because a later push makes it stale. No other PR is open.

### Traps and gotchas

- The process since PR #23: gitar comments on every PR after a push (D-250). Answer every comment before the hand-over or the override request. A comment with no merit gets a reply and a resolve. A comment with merit gets the change, a push, and a reply. Tell the owner when the PR is ready for the other provider or for the override.
- The "Review gate / evaluate" job line reads red until an approved review record covers the effective head (D-251). That is the design and not a failure to fix.
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.
- Two open PRs that both add a handoff entry conflict at the top of the file, at the end of the register, at the end of the questions, and in the findings table. The second one to merge takes a merge from `main` first. Rebuild the handoff and the archive from the union of the entries, by number, with ten in the handoff and each entry once.
- The effective head is the newest commit outside the metadata set, and a merge from `main` moves it. The review record names that commit.
- PR-9 changes the loop state: the floor number joins the hash after the fields of PR-7, so the simulation version rises to 4 and the bit-identity hash moves (G-20). The replayer then takes the content set in place of the grid and the spawn, which closes D-236.
- PR-9 adds bit 9 to `Button` and the masks: `AssignedMask` becomes 0x03FF and `ReservedMask` becomes 0xFC00. The random intent helper of the tests and the sweep mask follow.
- PR-9 changes the floor template schema. `sizeX`, `sizeY`, and `sizeZ` join the required list, the three content files gain them, and the content hash of the test set moves.
- The generator and the reachability search step in integers over the Procgen stream (D-159), and no DetMath call is needed for the carving.
- The PR template still lacks the gitar gate line of D-250. PR-9 adds it as the next code PR, in `.github/pull_request_template.md`.
- `StateHash` has no `==` operator. Compare `.Value`.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner adds the `review-override` label to PR #26 after the automated pass and merges it. Then a new session starts PR-9 on a short branch: the floor sizes and the chamber templates as content, the dig plan generator, the reachability search, the stairwell transition with the two bits, the loop floor number with the simulation version 4, and the eight exit tests, under D-159, D-164 to D-167, D-210, D-236, and D-252 to D-257. The session settles the field names of the chamber template in its validator, and it files a question only when a choice changes a contract.

## Session 74: 2026-09-09, Codex

Author: Codex
Session: re-review PR #23 after the P2-1 correction. Branch `feat/pr-8-camera`.

### What this session did, and why

- Verified the provider gate again. Claude Code supplied the substantive PR-23 changes, and Codex is the eligible reviewer.
- Compared the new effective head `4e98470` with the prior reviewed head `e6e89aa`. Read the response file, the replay observer, the bit-identity sweep, the related contracts, and all current PR comments and replies.
- Closed P2-1 as fixed in `4e98470`. `RunReplayer` now calls the observer after each complete frame, and the bit-identity sweep folds camera and aim values from that replay traversal. The second live loop is gone.
- The later automated suggestion for a null guard has no merit. Nullable references are enabled and warnings are errors, so the observer parameter is non-nullable at the call boundary.
- Updated `docs/reviews/pr-23.md` with the new effective head and verdict `Ready for owner merge`.

### State of the build

- `main` is at `1d8f8bd`. The effective PR-23 head is `4e98470`, and the current metadata tip is `4d37187`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 426 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings. `bit-identity`: `afed0063a6cf8a50`.
- The Godot 4.7.2 headless build check passes.
- The latest GitHub checks pass for CI, bit identity, determinism lint, STE, and Gitar. The review-gate and evaluate results still refer to the prior unapproved review record until this update reaches the PR head.
- The first test run hit `SocketException (13): Permission denied` in the restricted context. The rerun in the permitted execution context passed.

### In flight

PR #23 is ready for owner merge after this review record reaches the remote PR head. The effective head remains `4e98470` because the commits after it change only review and handoff metadata.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The review record must name `4e98470`, not the metadata tip `f1af5db`.
- The bit-identity value changed to `afed0063a6cf8a50` because the camera fold now reads the replay observer. The simulation version stays 3 because no simulation behavior contract changed.
- The review-gate and evaluate jobs must run again after this review record reaches the PR head.
- The reviewing provider reads automated comments and author replies into the record and does not reply to or resolve the automated comment (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge PR #23 after the remote review-gate check reads `Ready for owner merge` at effective head `4e98470`.

## Session 73: 2026-09-09, Claude Code

Author: Claude Code
Session: answer the PR #23 review. Branch `feat/pr-8-camera`.

### What this session did, and why

- Read the one P2 finding in `docs/reviews/pr-23.md`. Full merit: the sweep folded the camera and the aim ray from a second live loop, and PR-8 exit test 5 names the replay.
- The replay takes an `IReplayObserver` now, and it calls `AfterTick` after each complete frame. The five-argument `Replay` passes a silent observer, so the twelve callers stand.
- The sweep folds the camera pose and the aim ray of every replayed tick through a `CameraFold` observer, beside the end hash and the CRC-32. The second live loop is gone.
- The sweep hash moved from `92ef27ee175b3e7e` to `afed0063a6cf8a50`, because the fold order changed. The simulation version stays 3, because no simulation number changed (G-20).
- Three tests establish the fix, and one of them is new: the observer sees every complete frame and no torn tail, the replay fold equals the live fold over one hundred seeds, and the sweep holds no loop of its own. 426 tests in total.
- F-85 records the finding, and `docs/reviews/pr-23-response.md` records the disposition.
- The automated pass on the push gave one comment with two items, and neither had merit. The CI notice asked for an edit of the reviewer's verdict, which the author never makes (T-4, D-251). The null guard on the observer asked for a case that the compiler rejects, because nullable reference types are on with warnings as errors (D-68). Both got a reply on the PR, and no thread existed to resolve.

### State of the build

- `main` is at `1d8f8bd`, the squash merge of PR #25. This branch holds the correction `4e98470` above the review commit, and this entry above it.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 426 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 43 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `afed0063a6cf8a50`. The PR #23 review moved it from `92ef27ee175b3e7e`, and `BitIdentityKnownAnswer` pins the new value.

### In flight

PR #23 is open and it holds this branch. The automated pass runs on this push, and then a Codex repeat review at the effective head `4e98470` updates the same review record. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The effective head is `4e98470`. The review record still names `e6e89aa`, and the repeat review updates the head and the verdict together.
- The sweep hash moved without a version change. G-20 ties the version to a simulation number, and the fold order of the sweep is not one.
- The observer runs after each step and inside the frame loop, so a frame that fails its checksum stops the replay before the observer sees it.
- A test observer that folds a hash holds a `StateHash` field and gives it back through a property, because a `ref` cannot cross an interface call.
- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass runs on this push. Then a Codex session repeats the review per the repeat procedure, at the effective head `4e98470`, and updates `docs/reviews/pr-23.md` with the status of P2-1 and a new verdict.

## Session 72: 2026-09-09, Codex

Author: Codex
Session: review PR #23 at effective head `e6e89aa`. Branch `feat/pr-8-camera`.

### What this session did, and why

- Verified the provider gate. Session 71 identifies Claude Code as the author of the substantive PR-23 change, and Codex is the eligible reviewer.
- Read the complete PR diff, the PR description, the Phase 1 PR-8 exit tests, decisions D-241 to D-249, the replay path, the bit-identity sweep, and the existing gitar comment.
- Found one P2 defect. The bit-identity sweep replays the record for the final state hash, but it folds the camera and aim values from a separate live loop. This does not prove the PR-8 replay exit test.
- Wrote `docs/reviews/pr-23.md` with the verdict `Changes required` at effective head `e6e89aa`.

### State of the build

- `main` is at `1d8f8bd`. The effective PR-23 head is `e6e89aa`, and the current metadata tip is `2f365cc`.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 425 tests, 0 failures.
- `det-lint`: 0 findings. `ste-check`: 0 findings. `bit-identity`: `92ef27ee175b3e7e`.
- The Godot 4.7.2 headless build check passes.
- GitHub CI, bit identity, determinism lint, STE, and Gitar pass at `2f365cc`. The review-gate check is neutral and evaluate fails until an approved review record exists, as D-251 requires.
- `git fetch origin` could not update `.git/FETCH_HEAD` because the checkout denied access. `gh pr view` verified the remote head before this review record.

### In flight

PR #23 is open with one P2 finding. The author must make the camera and aim fold read from the replay traversal, then push a substantive revision. A Codex re-review must compare the new effective head and update this same review record.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks them.

### Traps and gotchas

- The effective head is `e6e89aa`, not the metadata tip `2f365cc`.
- The review-gate check is neutral until the review record reaches the PR head. The evaluate job fails while the record does not approve the effective head.
- The camera fold at `WhatYouCarry.Tools/BitIdentity/BitIdentitySweep.cs:219-235` uses a new live loop. The record replay at lines 208-210 does not supply those camera values.
- The existing gitar comment is recorded in `## PR comments`. The reviewing provider does not reply to or resolve that comment (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Correct P2-1 in PR #23. Then run the automated pass, fetch the new effective head, and complete a Codex re-review with the same finding id.

## Session 71: 2026-09-09, Claude Code

Author: Claude Code
Session: bring `main` into the branch after PR #25, and hand PR #23 to the Codex review. Branch `feat/pr-8-camera`.

### What this session did, and why

- The owner merged PR #25 as `1d8f8bd`, after a Codex review with no finding (Session 70). PR #23 is the second open PR, so it takes `main` first, as Session 69 planned.
- Merged `main` into this branch in `e6e89aa`. Four files conflicted at their append points: the questions, the roadmap header, the handoff, and the archive. Each one holds both sides in id order now: OQ-117 to OQ-119, D-200 to D-251 in the header, and every session entry once.
- The findings table merged on its own but out of order. F-82, F-83, and F-84 read in order now.
- The union of the two handoffs held twelve entries, so Sessions 61, 60, and 59 moved to the archive beside 58 and 57, and this entry keeps the file at ten (D-146).
- The effective head of PR #23 is `e6e89aa`, because the merge brings the workflow, the skill, and the agent files. The code of PR-8 is unchanged since `e6d40e0`, and the bit-identity value stands.
- The review-gate workflow on this push reads the D-251 step from `main`, so the job line of PR #23 reads red until its review record exists. That run is the first proof of D-251 (G-19).

### State of the build

- `main` is at `1d8f8bd`, the squash merge of PR #25. This branch holds the PR-8 commit, two merges from `main`, and the entries above them.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 425 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 42 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `92ef27ee175b3e7e`, as PR-8 set it (G-20).

### In flight

PR #23 is open and it holds this branch. gitar approved the PR-8 head and the first merge, and the pass runs again on this push. The PR changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head `e6e89aa`. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The review-gate job line of PR #23 reads red by design until the review record exists (D-251). The check run is neutral. The review record at the effective head turns both green.
- A merge from `main` can put one session entry on both sides of the handoff and the archive. Rebuild both files from the union of the entries, by number, and keep each entry once.
- The effective head is the merge commit `e6e89aa` and not `e6d40e0`. The review record names the merge commit.
- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250). The `## PR comments` part of the record lists them.
- The hash moves in PR-8, and that is the point of G-20. A review that sees `92ef27ee175b3e7e` must confirm it and not restore `e8ef2b1fad938845`.
- The loop hash reads the buttons, so two loops that differ in the controller aim bit alone give two hashes and one camera pose.
- The march tie order is X, then Y, then Z, so a line through a corner visits the cell at the corner. The fine-walk test allows one millimeter for that.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #23 per the `pr-review` skill, at the effective head `e6e89aa`, reads the existing PR comments into the review, and writes `docs/reviews/pr-23.md`. The review focus is determinism, replay, and test quality, and it confirms the simulation version 3 and the bit-identity change under G-20.

## Session 70: 2026-09-09, Codex

Author: Codex
Session: review PR #25 for the review-gate job result. Branch `fix/review-gate-red-on-neutral`.

### What this session did, and why

- Verified PR #25 at effective head `3e15249` against `main` at `a78e759`.
- Confirmed the provider gate. Session 69 identifies Claude Code as the author, and Codex is the eligible reviewer.
- Read the complete diff, the workflow boundary, the test, the related documents, and the automated review comment.
- Found no in-scope defect. The success-only guard fails on neutral, null, and unexpected conclusions, and passes on success.
- Wrote `docs/reviews/pr-25.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` is at `a78e759`, and the reviewed effective head is `3e15249`.
- `dotnet build` passes with 0 warnings and 0 errors. `dotnet test` passes with 399 tests and 0 failures.
- `det-lint` passes with 0 findings. `ste-check` passes with 0 findings. `bit-identity` gives `e8ef2b1fad938845`.
- The Godot 4.7.2 headless build check passes.
- GitHub CI, bit identity, determinism lint, STE, evaluate, and Gitar pass at `3e15249`. The `review-gate` check is neutral because the base branch still holds the old workflow (D-197).
- Remote head: `3451c24` is the review commit, checked after push.

### In flight

PR #25 is open with the verdict `Ready for owner merge` at effective head `3e15249`. The first PR after this merge must prove that the changed workflow makes a neutral check run red (D-251).

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The workflow runs from the base branch. PR #25 cannot exercise its changed workflow on itself (D-197).
- The review record commit changes only metadata, so the effective head stays `3e15249` (D-184).
- The handoff now holds ten entries. Session 58 moved to the archive (D-146).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner can merge PR #25. After the merge, observe the first PR with no review record and confirm that the review-gate job reads red.

## Session 69: 2026-09-09, Claude Code

Author: Claude Code
Session: make the review-gate job red on a neutral verdict (D-251). Branch `fix/review-gate-red-on-neutral`.

### What this session did, and why

- The owner saw the "Review gate / evaluate" job green on PR #23 with no review record. The check run was neutral, and the job stayed green because a job cannot be neutral by its exit code (F-84).
- Asked one owner question with three options, and D-251 records the answer: the job fails on a neutral conclusion too. OQ-119 holds the question. D-181 is revised in part, the job result only.
- The last step of the workflow exits 1 on every conclusion but `success`, and it prints the summary then. `ReviewGateJobFailsOnNeutral` reads the file and fails on the old step.
- The automated pass on this PR gave one comment with one finding, with merit: the first guard read the two strings `failure` and `neutral`, so a missing conclusion passed the job green (T-2). The guard reads `success` alone now, in the second commit, and the reply on the thread names it.
- The design doc, the roadmap, the skill, and the agent files say that the job line reads red on grey. The roadmap PR-1 exit tests gain number 26.
- The workflow runs from the base branch (D-197), so this PR cannot exercise its own change. The proof runs on the first PR after the merge.

### State of the build

- `main` is at `a78e759`, the squash merge of PR #24. This branch holds two commits above it.
- Remote head: `origin/fix/review-gate-red-on-neutral` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 399 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `e8ef2b1fad938845`, the value of `main`. PR #23 moves it to `92ef27ee175b3e7e`.

### In flight

- PR #25 is open and it holds this branch. It changes `.github/`, so it needs the automated pass and then a Codex review with the verdict `Ready for owner merge` at the effective head.
- PR #23 is open on `feat/pr-8-camera`, ready for the Codex review at the effective head `32cad0d`.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Two open PRs again: this one and PR #23. Both append at the handoff top, the register end, the questions end, and the findings table. The second one to merge needs a merge from `main` first, and the handoff then holds more than ten entries, so the oldest move to the archive until ten stay.
- The workflow file on a PR comes from the base branch, so a PR that changes the workflow sees the old behavior on itself. Read the first run after the merge.
- A documentation PR with a valid override label stays green, because the override gives `success` and not `neutral`. A stale label after a push turns red, as on PR #24.
- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250).

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The automated pass runs on PR #25. Then a Codex session reviews PR #25 per the `pr-review` skill and writes `docs/reviews/pr-25.md`, with the review focus on the CI boundary and test quality. PR #23 waits for its own Codex review. The second PR to merge takes a merge from `main` first.

## Session 68: 2026-09-09, Claude Code

Author: Claude Code
Session: run the automated pass on PR #23, and bring `main` into the branch before the Codex review. Branch `feat/pr-8-camera`.

### What this session did, and why

- The owner set the automated review pass of gitar (D-250), and PR #24 recorded it. The first pass on PR #23 gave one comment: approved, no issue. No change followed.
- Merged `main` at `a78e759` into this branch, so the reviewer's checkout holds the `pr-review` skill with the gitar rule. The handoff and the register hold both sides in order: Session 67 above Session 66, and D-248, D-249, D-250.
- The merge conflicted in two files, at the places both branches added to: the top of the handoff and the rows after D-247. The archive merged on its own, because both sides moved Session 56 with one text.
- This entry makes twelve, so Sessions 58 and 57 move to the archive (D-146).
- The effective head of PR #23 is the merge commit `32cad0d`, because the merge brings the skill and the agent files. The code of PR-8 is unchanged since `e6d40e0`, and the bit-identity value stands.

### State of the build

- `main` is at `a78e759`, the squash merge of PR #24. This branch holds the PR-8 commit, the merge, and this entry above it.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 424 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 42 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `92ef27ee175b3e7e`, as PR-8 set it (G-20).

### In flight

PR #23 is open and it holds this branch. gitar approved the PR-8 head, and the pass runs again on this push. The PR changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The reviewing provider reads the existing PR comments into the review and never addresses gitar (D-250). The review record gains a `## PR comments` part, and the skill on this branch holds the rule.
- The effective head is the merge commit `32cad0d` and not `e6d40e0`. The review record names the merge commit.
- The hash moves in PR-8, and that is the point of G-20. A review that sees `92ef27ee175b3e7e` must confirm it and not restore `e8ef2b1fad938845`.
- The loop hash reads the buttons, so two loops that differ in the controller aim bit alone give two hashes and one camera pose.
- The march tie order is X, then Y, then Z, so a line through a corner visits the cell at the corner. The fine-walk test allows one millimeter for that.
- Two open PRs that both add a handoff entry conflict at the top of the file, and the second one to merge needs a merge from `main` first. Merge before the review, so the review head stands.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #23 per the `pr-review` skill, at the effective head `32cad0d`, reads the existing PR comments into the review, and writes `docs/reviews/pr-23.md`. The review focus is determinism, replay, and test quality, and it confirms the simulation version 3 and the bit-identity change under G-20.

## Session 67: 2026-09-09, Claude Code

Author: Claude Code
Session: record the automated review pass of gitar (D-250), and run its first cycles on PR #23 and on this PR. Branch `docs/gitar-review-pass`.

### What this session did, and why

- The owner added an automated reviewer, gitar, that comments on every PR after a push, and gave the rule for it. D-250 records the rule as an owner instruction.
- The `pr-review` skill gained two procedures: "The automated pass" for the author, and "Do not address the automated reviewer" for the reviewing provider. The review record skeleton gained a `## PR comments` part, and the scope limits name a reply to gitar as an external message.
- The agent files gained the "Automated review pass" section and a gate line. Design section 3.14 names the pass.
- The first pass on PR #23 gave one comment: approved, no issue. PR #23 is ready for the Codex review with no change.
- The pass on this PR gave one comment with one finding: the register jumps from D-247 to D-250. No merit. D-248 and D-249 live in PR #23, and ids never change, so D-250 stays. The reply on the thread says so, and the thread is resolved. The owner asked for the reply alone and no note in the register.
- This branch comes from `main`, so it holds neither D-248, nor D-249, nor Session 66. Each of those lands with PR #23.

### State of the build

- `main` is at `9209fb5`, the squash merge of PR #22. This branch holds three document commits above it.
- Remote head: `origin/docs/gitar-review-pass` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 398 tests, 0 failures, on the code of `main`.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `e8ef2b1fad938845`, the value of `main`. PR #23 moves it to `92ef27ee175b3e7e`.

### In flight

- PR #24 is open and it holds this branch. It changes documents, skills, and the agent files alone, so the `review-override` label covers it (D-190). The owner added the label before the second commit, so the label needs a new add after the last push.
- PR #23 is open on `feat/pr-8-camera`, with the effective head `e6d40e0`. It is ready for the Codex review.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open as PR #23. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- Two open PRs both add a handoff entry at the top of the file and a decision row after D-247, and the second one to merge needs a merge from `main` first. After this PR merges, merge `main` into `feat/pr-8-camera` and order the entries and the rows by number: Session 68, 67, 66, and D-248, D-249, D-250.
- The override label is stale after any push outside the metadata set (D-190). Add it after the last push, and not before.
- gitar posts one comment on the PR with its verdict inside a details block, and one review thread on the line of each finding. The thread has a GraphQL node id, and `addPullRequestReviewThreadReply` answers it. The REST list of pull comments was empty while the pass still ran.
- A reply to gitar names no provider (T-6). It states the evidence and the commit. A gap or a fact that an open PR explains gets the reply alone, and no note in a register.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner adds the `review-override` label to PR #24 again and merges it. Then merge `main` into `feat/pr-8-camera`, order the entries and the rows by number, and add the Session 68 entry that records the pass on PR #23. A Codex session then reviews PR #23 per the `pr-review` skill, reads the existing PR comments into the review, and never addresses gitar (D-250).

## Session 66: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-8, the orbit camera, the ray march, the aim ray, and aim assist. Branch `feat/pr-8-camera`.

### What this session did, and why

- The owner merged PR #22 as `9209fb5`. The handoff named PR-8 as the next action, under D-241 to D-247.
- Asked two owner questions in one batch, and D-248 and D-249 record the answers. OQ-117 and OQ-118 hold the questions.
- D-248: a positive pitch looks up.
- D-249: the camera runs two marches. The first pulls the shoulder point in at a wall, and the second runs the boom. The shoulder point of D-242 sits inside rock when the player hugs a right wall, and the pivot never does.
- Wrote `GridRay`, the grid traversal of Amanatides and Woo, with a tie order of X, then Y, then Z. Wrote `CameraPose`, `OrbitCamera`, `AimRay`, and `AimAssist`. `Vector3` gained subtraction, a scale, the dot and cross products, and the length.
- The loop gives `Camera()` and `Aim(targets)` on demand, and neither is state (D-245). The pitch clamp is 8000 (D-241), and bit 8 is the controller aim flag (D-243).
- The assist is a spherical interpolation by the strength, with the angle from `Atan2` of the cross length and the dot product, so a small angle stays exact.
- The simulation version is 3, and the bit-identity hash moved from `e8ef2b1fad938845` to `92ef27ee175b3e7e` on purpose (G-20). The sweep folds in the camera pose and the aim ray of every tick.
- No allowlist entry. 26 new tests, 424 in total. Opened PR #23.

### State of the build

- `main` is at `9209fb5`, the squash merge of PR #22. This branch holds one commit above it.
- Remote head: `origin/feat/pr-8-camera` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 424 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 42 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files.
- `bit-identity`: `92ef27ee175b3e7e`. PR-8 moved it from `e8ef2b1fad938845` on purpose, and `BitIdentityKnownAnswer` pins the new value.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #23 is open and it holds this branch. It changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 is open. PR-9 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The hash moves in this PR, and that is the point of G-20. A review that sees the new value must confirm it and not restore the old one.
- The loop hash reads the buttons, so two loops that differ in the controller aim bit alone give two hashes and one camera pose. A test that expects one hash there fails.
- The march tie order is X, then Y, then Z. A line through a corner visits the cell at the corner, so the march can hit a block that a point sample of the line misses. The fine-walk test allows one millimeter for that.
- A miss carries the whole segment length as its distance, and a zero-length segment carries zero.
- The camera range check reads the loop ranges. A yaw of 36000 or a pitch of 8001 is an error, and a test that builds a pose by hand stays inside them.
- The sweep grid holds one-block steps and two-block pillars, and the boom meets them on many ticks. A change to `ReplayGrid` or to the sweep targets moves the hash.
- `StateHash` has no `==` operator. The camera tests compare `.Value`.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #23 per the `pr-review` skill, at the effective head, and writes `docs/reviews/pr-23.md`. The review focus is determinism, replay, and test quality, and it confirms the simulation version 3 and the bit-identity change under G-20.

## Session 65: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-7 merge, and answer the PR-8 questions before its code. Branch `docs/pr-7-merge-record`.

### What this session did, and why

- The owner merged PR #21 as `d5f20ce`, after a Codex review with no finding.
- Audited every document against the merged state. The registers were complete: D-235 to D-240, OQ-104 to OQ-109, and F-82 and F-83 all landed with the PR.
- Three status lines were stale. The design doc PR-7 entry and its sequence line said open, and the focused roadmap said open. All three name the merge now.
- The handoff held ten entries, because Session 64 archived one. This entry makes eleven, so Session 55 moves to the archive (D-146).
- Asked seven owner questions in two batches, and D-241 to D-247 record the answers. OQ-110 to OQ-116 hold the questions.
- D-241: the pitch limit is plus or minus 80 degrees for the loop and the camera. D-227 is revised in part, the pitch limit only.
- D-242: the pivot sits 1.5 meters over the feet, the shoulder point is 0.6 right and 0.3 up, and the boom is 3 meters.
- D-243: bit 8 of the buttons marks a controller aim on that tick. D-232 is revised in part, bit 8 only.
- D-244: aim assist pulls the ray toward the nearest target inside a 5 degree cone, by half the angle.
- D-245: Core derives the camera on each tick, and the camera holds no state.
- D-246: the boom sweep is a ray march along the line, with a camera radius of 0.25 meters.
- D-247: the aim ray starts at the camera, along the look direction.
- The roadmap PR-8 scope cites the seven decisions, and it names the simulation version rise to 3 (G-20).

### State of the build

- `main` is at `d5f20ce`, the squash merge of PR #21. This branch holds the document commit above it.
- Remote head: `origin/docs/pr-7-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 398 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `e8ef2b1fad938845`.

### In flight

PR #22 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-7 are merged. PR-8 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- PR-8 changes the loop pitch clamp from 9000 to 8000 (D-241). `PitchClamps` in `SimulationTests`, the `PitchLimit` constant, and the D-227 remark in `SimulationLoop` change with it, and the simulation version rises to 3.
- Bit 8 joins the assigned set (D-243). `Button.AssignedMask` becomes 0x01FF and `Button.ReservedMask` becomes 0xFE00. The random intent helper of the tests and the button mask of the bit-identity sweep follow, or the reserved-bit check throws.
- The boom sweep is a ray march and not the PR-7 sweep (D-246). The PR-7 sweep runs one axis at a time and follows a staircase, so a diagonal boom through it ends beside the line.
- The camera holds no state (D-245). The exit test hashes the aim ray and the camera position after a replay, and the loop hash gains no field.
- The bit-identity hash moves again in PR-8, on purpose, and `BitIdentityKnownAnswer` pins the new value (G-20).
- The pitch sign is not in any decision. PR-8 states which sign looks up, in the camera remark and in a test.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #22 with the `review-override` label. Then a new session starts PR-8 on a short branch: the orbit camera, the ray march of the boom, the aim ray, aim assist, and the five exit tests, under D-13, D-14, D-75, D-77, D-88, and D-241 to D-247.

## Session 64: 2026-09-09, Codex

Author: Codex
Session: review PR-7 for the voxel grid, swept box, and player body. Branch `feat/pr-7-world-collision`.

### What this session did, and why

- Verified PR #21 against `main` at `01e68c3` and reviewed effective head `4207548`.
- Confirmed that Session 63 identifies Claude Code as the implementation provider. Codex is the eligible reviewer.
- Inspected the complete implementation, test, tool, and document diff, affected callers, replay paths, Core boundary, and PR-7 contracts.
- Confirmed the simulation version 2 change and the intentional bit-identity change from `283aa4b8cd1281be` to `e8ef2b1fad938845` under G-20.
- Found no blocking defect. Wrote `docs/reviews/pr-21.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` is at `01e68c3`. The reviewed effective head is `4207548`.
- `dotnet build` passes with 0 warnings and 0 errors. `dotnet test` passes with 398 tests and 0 failures.
- `det-lint` passes with 0 findings. `ste-check` passes with 0 findings.
- `bit-identity` gives `e8ef2b1fad938845`.
- The Godot 4.7.2 headless build check passes.
- Remote head: `origin/feat/pr-7-world-collision` at `bd57f34`, which holds the review record and this handoff entry.

### In flight

PR #21 is open with the verdict `Ready for owner merge` at effective head `4207548`. The owner can merge it.

### Traps and gotchas

- PR-7 raises the simulation version and changes the bit-identity value on purpose. Keep both values when the owner merges the PR.
- The sweep uses a contact skin and runs Y, then X, then Z. A face on a block is contact, not overlap.
- The loop and replay take the grid and spawn from the caller until PR-9, as D-236 requires.

### Open questions that block progress

None. OQ-99 remains open, and it blocks nothing.

### Next concrete action

The owner can merge PR #21. A new session starts after the owner merges it.

## Session 63: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-7, the voxel grid, the swept box, and the player body. Branch `feat/pr-7-world-collision`.

### What this session did, and why

- The owner merged PR #20 as `01e68c3`. The handoff named PR-7 as the next action, under D-164, D-165, and D-231 to D-234.
- A float check before the code refuted an exact contact on a block face. For twelve integer faces below 130, `(c - h) + h` is one ulp off (F-82). That made the position representation an owner question.
- Asked six owner questions in two batches, and D-235 to D-240 record the answers. OQ-104 to OQ-109 hold the questions.
- D-235: float meters, feet center, and a contact skin of 2^-10 meters before every face. The ground is a probe two skins below the feet.
- D-236: the loop and the replayer take the grid and the spawn point from the caller until PR-9.
- D-237: a cell outside the grid is solid, and a direct read there is an error.
- D-238: the intent sets the horizontal velocity on every tick, in the air too.
- D-239: air and raw stone are the two block ids of PR-7.
- D-240: no terminal velocity.
- Wrote `VoxelGrid`, `BlockId`, `Vector3`, `Aabb`, `Axis`, `SweptAabb`, `Button`, and `PlayerBody`. The loop holds the grid and the body, and the hash adds the position and the vertical velocity after the five fields of D-227.
- The sweep runs Y, then X, then Z, and a test pins the order. It reads every cell in the path, so a move of 20 meters in one tick still stops at the first block.
- The simulation version is 2, and the bit-identity hash moved from `283aa4b8cd1281be` to `e8ef2b1fad938845` on purpose (G-20).
- The fixed-step apex of a jump is 1.17 meters, and D-231 said 1.23. D-231 carries the dated correction, and F-83 records it.
- No allowlist entry. 62 new tests, 398 in total. Opened PR #21.

### State of the build

- `main` is at `01e68c3`, the squash merge of PR #20. This branch holds two commits above it.
- Remote head: `origin/feat/pr-7-world-collision` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 398 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 37 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files.
- `bit-identity`: `e8ef2b1fad938845`. PR-7 moved it from `283aa4b8cd1281be` on purpose, and `BitIdentityKnownAnswer` pins the new value.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #21 is open and it holds this branch. It changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-6 are merged. PR-7 is open. PR-8 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The hash moves in this PR, and that is the point of G-20. A review that sees the new value must confirm it and not restore the old one.
- A float contact cannot sit on a block face. Twelve integer faces below 130 are one ulp off after `(c - h) + h`, at binade edges such as 16 and 128 (F-82). The sweep stops one skin before a face, and a test that asserts a face on the block fails.
- A box on a face is contact and not overlap. `Overlaps` reads the open interval, and `-Floor(-max) - 1` names the cell below a face that sits on a block boundary.
- The sweep runs Y first. A diagonal move toward a one-block step falls beside the step and stops at its wall, and the other order lands on top. `TheOrderIsYThenXThenZ` pins it.
- The fixed-step apex is 1.17 meters and not 1.23. Assert the one-block clear and the two-block fail, and never the apex.
- The random intent helper masks the buttons to the eight assigned bits. A test that passes a random `ushort` throws on a reserved bit (D-232).
- The loop constructor takes a grid and a spawn. A test that wrote `new SimulationLoop(seed)` uses `TestWorld.NewLoop(seed)`.
- The record does not carry the grid. A replay with another grid gives another hash, and the gap closes in PR-9 (D-236).
- `StateHash` has no `==` operator. Compare `.Value`, or use `Assert.Equal`.
- The Godot check prints `[ DONE ] dotnet_build_project` on a pass. The shell variable `PIPESTATUS` is a bash name, and zsh reads `pipestatus`, so read the line and not the exit code through a pipe.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

A Codex session reviews PR #21 per the `pr-review` skill, at the effective head, and writes `docs/reviews/pr-21.md`. The review focus is determinism, the Core boundary, and test quality, and it confirms the simulation version 2 and the bit-identity change under G-20.

## Session 62: 2026-09-09, Claude Code

Author: Claude Code
Session: record the PR-6 merge, and answer the PR-7 questions before its code. Branch `docs/pr-6-merge-record`.

### What this session did, and why

- The owner merged PR #19 as `3f7e3c2`, after a Codex review with no finding.
- Audited every document against the merged state. The registers were complete: D-224 to D-230, OQ-92 to OQ-99, and F-80 and F-81 all landed with the PR.
- Two status lines were stale. The design doc PR-6 entry and its sequence line said open, and the focused roadmap said open. All three name the merge now.
- The handoff held eleven entries. Session 61 added its entry and moved none to the archive. This session moved sessions 51 and 52 to the archive, so the file holds ten again (D-146).
- Asked four owner questions that block the PR-7 code, and D-231 to D-234 record the answers. OQ-100 to OQ-103 hold the questions.
- D-231: gravity 20, walk 4, sprint 6.5, jump 7. The apex is 1.23 meters, so a jump clears one block and never two (D-165).
- D-232: the button bits. Bit 0 is jump, bit 1 is sprint, and bits 2 to 7 hold dodge, attack, use, interact, and the two quick slot moves. Bits 8 to 15 are reserved, and a set one is an error.
- D-233: the movement is strafe and forward, each over 127, rotated by the yaw sum, with the length clamped to 1.
- D-234: Core uses the frame of Godot. Right-handed, Y up, meters, and forward at yaw zero is minus Z.
- The roadmap PR-7 scope cites the four decisions, and it names the simulation version rise to 2 (G-20).

### State of the build

- `main` is at `3f7e3c2`, the squash merge of PR #19. This branch holds the document commits above it.
- Remote head: `origin/docs/pr-6-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 336 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 29 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `283aa4b8cd1281be`.

### In flight

PR #20 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-6 are merged. PR-7 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- A review session adds an entry and can leave eleven in the handoff. Count the entries at the start of a session, and archive down to ten.
- PR-7 raises the simulation version to 2 and moves the bit-identity hash, because the state gains a position. `BitIdentityKnownAnswer` pins the number, and the PR updates it on purpose (G-20).
- The apex of a jump under fixed-step Euler differs from the closed form by a fraction of a tick. Assert the one-block clear and the two-block fail in a test, and never the apex value alone.
- The movement fraction of -128 clamps to -127, so the two directions have one magnitude.
- The state hash order of D-160 grows in PR-7. Add the new fields after the five of D-227, so the order test of PR-6 still reads.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #20 with the `review-override` label. Then a new session starts PR-7 on a short branch: the voxel grid, the swept box, the player body, and the five exit tests, under D-164, D-165, and D-231 to D-234.

## Session 61: 2026-09-09, Codex

Author: Codex
Session: review PR #19 for the simulation loop, the intent frame, the run record, and the replay. Branch `feat/pr-6-loop-record`.

### What this session did, and why

- Verified PR #19 against `main` at `c11fb41` and reviewed effective head `0850d32`.
- Confirmed that Session 60 identifies Claude Code as the implementation provider. Codex is the eligible reviewer.
- Inspected the complete diff, callers, tests, replay contracts, error paths, Core boundary, and Phase 1 documents.
- Confirmed the intentional bit-identity change from `4d6385bb92454694` to `283aa4b8cd1281be`.
- Found no blocking defect. Wrote `docs/reviews/pr-19.md` with the verdict `Ready for owner merge`.

### State of the build

- `main` is at `c11fb41`. The reviewed effective head is `0850d32`.
- The branch held metadata commit `0850d32` before this review. Review commit `1247769` is on the remote branch.
- `dotnet test` passes with 336 tests and 0 failures.
- `det-lint` passes with 0 findings. `ste-check` passes with 0 findings.
- `bit-identity` gives `283aa4b8cd1281be`.
- The Godot 4.7.2 headless build check passes.
- The local build command stalled without compiler output. The Linux, macOS, and Windows CI build and test jobs pass.

### In flight

PR #19 is open with the verdict `Ready for owner merge` at effective head `0850d32`. The owner can merge it after the review record reaches the remote branch.

### Traps and gotchas

- The effective head is `0850d32` because that commit changes design and roadmap files outside the D-184 metadata set. The later review commits do not change the effective head.
- The bit-identity value changes on purpose because the sweep replays one fixed record.
- The simulation version stays at 1 because PR-6 sets its first value.

### Open questions that block progress

None. OQ-99 remains open, and it blocks nothing.

### Next concrete action

The owner merges PR #19, or requests a review of a new effective head.

## Session 60: 2026-09-09, Claude Code

Author: Claude Code
Session: PR-6, the simulation loop, the intent frame, the run record, the recorder, and the replay. Branch `feat/pr-6-loop-record`.

### What this session did, and why

- The owner merged PR #18 as `c11fb41`. The handoff named PR-6 as the next action, with two questions before the code.
- Asked seven owner questions in three batches, and D-224 to D-230 record the answers. OQ-92 to OQ-98 hold the questions.
- D-224: Core holds a table-driven CRC-32 in `Crc32.cs`. No dependency and no allowlist entry.
- D-225: the recorder writes through an `IRunRecordSink`, as the logger and the content loader do. Core opens no file.
- D-226: the design doc said "length-prefixed, checksummed tick frames" in sections 3.9 and 7, and D-162 fixes the frame at 16 bytes. Both lines name the fixed frame now (F-80).
- D-227: the Phase 1 loop state is the seed, the tick, the yaw and pitch sums in hundredths of a degree, and the buttons. The loop reads no movement byte until PR-7 has collision.
- D-228: the torn-tail log line names floor 1, because every run starts there and a Phase 1 run never leaves it. PR-31 reads the floor from the state.
- D-229: the JSON reader gives an empty list and a null as kinds, so the header carries `loadout`, `tree`, and `amulet` from the first record. A list with an item is an error until Phase 3.
- D-230: 1 type and 6 members enter the allowlist. A removal check proved each one in use.
- Found one defect outside the new files. The validator message wrote an enum value inside an interpolated string, and the runtime formats one through its metadata. An explicit switch replaces it (F-81), and OQ-99 asks whether `det-lint` gains a rule in its own PR.
- The bit-identity sweep records and replays one fixed run now, so the three platforms compare the replay (exit test 7). The known answer moved on purpose (G-20).
- 56 new tests. The total is 336. Opened PR #19.

### State of the build

- `main` is at `c11fb41`, the squash merge of PR #18. This branch holds two commits above it.
- Remote head: `origin/feat/pr-6-loop-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 336 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 29 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files.
- `bit-identity`: `283aa4b8cd1281be`. PR-6 moved it from `4d6385bb92454694` on purpose, and `BitIdentityKnownAnswer` pins the new value.
- The Godot 4.7.2 headless build check passes.

### In flight

PR #19 is open and it holds this branch. It changes code, so it needs a Codex review with the verdict `Ready for owner merge` at the effective head. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-5 are merged. PR-6 is open. PR-7 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- The bit-identity hash moves in this PR, and that is the point of G-20. A review that sees the new value must confirm it and not restore the old one.
- The simulation version stays at 1. It is the first value, so there is no bump to confirm. The next Core PR that moves a simulation number raises it to 2.
- An intent delta is an `int16`, so a yaw near 360 degrees takes two intents in a test. The first draft of `YawWraps` passed 35990 as one delta, and the compiler stopped it.
- The content error text says "file" now, not "content file", because the run record header goes through the same reader. A test that asserts the old words fails.
- The validator names a kind with an explicit switch now. A test that asserts the enum name `Number` fails, and one that asserts `number` passes.
- A Python patch script cannot hold a C# raw string literal inside a Python triple-quoted string. Write the script to a file, or escape the quotation marks.
- The replayer adds the frame index to an error from the decode or the loop and rethrows the same object. The message then holds the tick, both checksums or both ticks, and the frame.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing: the lint rule for an enum inside an interpolation belongs to its own PR.

### Next concrete action

A Codex session reviews PR #19 per the `pr-review` skill, at the effective head, and writes `docs/reviews/pr-19.md`. The review focus is determinism, replay, errors, and test quality, and it confirms the bit-identity change under G-20.

## Session 59: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-5 merge, and prepare the documents for a fresh session. Branch `docs/pr-5-merge-record`.

### What this session did, and why

- The owner merged PR #17 as `e0deb94`, after a Codex review approved the corrected head.
- Audited every document against the merged state. The registers were complete: D-219 to D-223, OQ-88 to OQ-91, and F-78 and F-79 all landed with the PR.
- Three status lines were stale. The focused roadmap said that PR-5 was open, the design doc marked it open, and the Phase 1 sequence did not mark it. All three name the merge now, and the correction passes record the PR-5 outcome.
- The owner then asked for a full document check before a fresh session. That check found three defects in the agent files, which are the first files that a session reads.
- The Godot command in the agent files could not run. The name `Godot` is not on the command path of this machine, and the command needs `/Applications/Godot_mono.app/Contents/MacOS/Godot`.
- The agent files named no `det-lint` command and no `bit-identity` command, and both are required gates. Both are in the command list now.
- The PR gate said "the lint tool and the STE checker pass" as one line. It holds one line for each check now, and the `det-lint` line names G-8 and G-21 beside G-2.
- Added a code rule for the allowlist. A new entry needs a decision, and a session verifies it by removing the entry and running `det-lint`. That check found two dead entries and one live gap in PR-4, and a reading of the list found neither.
- Ran every command in the agent files word for word. All six pass.

### State of the build

- `main` is at `e0deb94`, the squash merge of PR #17. This branch holds two commits above it.
- Remote head: `origin/docs/pr-5-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 280 tests, 0 failures.
- `det-lint`: 0 findings. Core 0 in 21 files, Game 0 in 0 files.
- `ste-check`: 0 findings in 15 files. `bit-identity`: `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes with the full path above.

### In flight

PR #18 is open and it holds this branch. It changes `docs/`, `CLAUDE.md`, and `AGENTS.md`, and every one of those paths is in the eligible set, so the `review-override` label covers it (D-190). No other PR is open.

### Where Phase 1 stands

PR-1 to PR-5 are merged. PR-6 to PR-11 remain, and then M-1, M-2, and PR-58 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- `dotnet test` runs the STE checker over every document, through `RepositoryDocumentsPass`. A document edit needs the test suite, and the checker alone passes a sentence that the suite rejects.
- The session number check of D-187 compares the numbers in one file. Fetch the remote and read the handoff again before the entry, because the other provider adds an entry while a session works.
- `det-lint` reports two counts now. The Game count is 0 files today, because the Game project holds no source file. The string rule has fixture tests until a PR writes Game code.
- The Game string rule is syntactic. A `Get` call takes an id only when its receiver names the string table, and the PR #17 response states that limit.
- An allowlist entry needs a removal check. A reading of the list finds neither a dead entry nor a gap.
- PR-6 is the first PR that makes a simulation number, so it moves the bit-identity hash. `BitIdentityKnownAnswer` pins that number, and the PR updates it on purpose (G-20).

### Open questions that block progress

No open question blocks PR-6 to PR-11. OQ-1, OQ-14, and the later ids belong to Phase 2 and beyond.

### Next concrete action

The owner merges PR #18 with the `review-override` label. Then a new session starts PR-6: the fixed-step loop at 60 Hz, the 16-byte intent frame, the run record, the recorder, and the replay (D-73, D-151, D-162, D-163, G-5). Two questions come before that code. The first is the checksum of D-162, which names CRC32 and no implementation, and the allowlist holds none. The second is whether the recorder writes through a sink, as the logger does under D-211 and the content loader does under D-219.

## Session 58: 2026-09-08, Codex

Author: Codex
Session: repeat review PR #17 for PR-5. Branch `feat/pr-5-content`.

### What this session did, and why

- Re-reviewed PR #17 at effective head `7143fd3` against base and merge base `a37f0af`.
- Confirmed the provider gate. Sessions 55 and 57 identify Claude Code as the author and correction author. Codex is the eligible reviewer.
- Verified the three fixes. The hash frames each file. The JSON reader keeps number text until validation. The string rule checks the receiver of `Get`.
- Verified the new regression tests and found no new in-scope defect.
- Updated `docs/reviews/pr-17.md` with fixed statuses and the verdict `Ready for owner merge`.

### State of the build

- `main` is at `a37f0af`. The reviewed effective head is `7143fd3`.
- Remote head: `origin/feat/pr-5-content` is `9d99dd0`, verified after the review metadata commit.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 280 tests and 0 failures.
- `det-lint` reports 0 findings. `ste-check` reports 0 findings. `bit-identity` gives `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes. The build, test, comparison, lint, and STE checks pass at `7143fd3`.

### In flight

The repeat-review record is published. The review-gate and evaluate checks pass.

### Traps and gotchas

- The effective head is the correction commit `7143fd3`. Review metadata commits do not change it.
- The prior review-gate failure named the old effective head. The updated record must name `7143fd3`.
- The response keeps the syntactic limit of the Game string rule because this PR has no Game source file or engine compilation path.

### Open questions that block progress

No owner question is needed. No open question blocks PR-6 to PR-11.

### Next concrete action

The owner can merge PR #17.

## Session 57: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #17 review. Branch `feat/pr-5-content`.

### What this session did, and why

- Read the three P2 findings in `docs/reviews/pr-17.md`. Each one reproduces, so each one has full merit.
- P2-1 is the most serious. The content hash appended each path and each byte sequence with no length, so the path `a` with the bytes `bc` and the path `ab` with the byte `c` gave one hash. Two content sets shared one hash, and this hash exists to tell two sets apart. Each file enters the input with a length, its path, a length, and its bytes now (F-78).
- P2-2. A fractional or out-of-range number raised a `FormatException`, and the catch held `JsonException` alone, so the error left Core with no file and no field. The reader keeps the token text through `ValueSpan` now, and the validator names the file, the field, and the reason. That is the contract that D-220 states (F-78).
- P2-3. The string rule exempted every method named `Get`, so `inventory.Get("You died")` gave no finding. A `Get` call takes an id only when its receiver names the string table now (F-79).
- The P2-3 correction stays syntactic, and the response states the limit. The Game project needs the engine assemblies for a symbol read, and it holds no source file yet.
- 10 new tests. The total is 280.

### State of the build

- `main` is at `a37f0af`. The branch holds the PR-5 work, two review commits, and this correction commit.
- Remote head: `origin/feat/pr-5-content` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 280 tests and 0 failures.
- `det-lint` reports 0 findings: Core 0 in 21 files, Game 0 in 0 files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A hash over a concatenation needs a boundary for each part. Without a length the path and the bytes run together, and two sets share one input.
- A platform conversion raises its own error type, and a catch of one type misses another. `GetInt64` raises `FormatException`, and the catch held `JsonException`.
- A method name is not a method. Every type can hold a `Get`, and the rule needs the receiver.
- `System.Array` returned to the allowlist. PR-4 left it out because Core used no array member, and the P2-1 correction reads one. That is D-207 working as written.

### Open questions that block progress

No new owner question. No open question blocks PR-6 to PR-11.

### Next concrete action

A Codex session re-reviews PR #17 per the repeat review procedure and updates `docs/reviews/pr-17.md` to the new effective head.

## Session 56: 2026-09-08, Codex

Author: Codex
Session: review PR #17 for PR-5. Branch `feat/pr-5-content`.

### What the session did, and why

- Reviewed PR #17 at effective head `614ce49` against base and merge base `a37f0af`.
- Confirmed the provider gate. Session 55 identifies Claude Code as the author, and Codex is the eligible reviewer.
- Inspected the content source, JSON reader, validators, typed records, content hash, string table, Game string lint rule, tests, content files, and project documents.
- Found three P2 defects. The content hash has ambiguous file framing. A malformed number can escape without content context. The string lint rule exempts unrelated `Get` methods.
- Wrote `docs/reviews/pr-17.md` with the findings and the verdict `Changes required`.

### State of the build

- `main` is at `a37f0af`. The reviewed effective head is `614ce49`.
- Remote head: `origin/feat/pr-5-content` is `ace29d4`, verified after the review commit.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 270 tests and 0 failures.
- `det-lint` reports 0 findings. `ste-check` reports 0 findings. `bit-identity` gives `4d6385bb92454694`.
- The Godot 4.7.2 headless build check passes. All applicable GitHub build, test, lint, STE, comparison, and bit-identity checks pass at `614ce49`.

### In flight

PR #17 needs corrections for P2-1, P2-2, and P2-3, followed by a repeat review.

### Traps and gotchas

- Hash each path and byte sequence with unambiguous boundaries. Concatenation alone can give one input to two content sets.
- `Utf8JsonReader.GetInt64()` can throw `FormatException` for a JSON number that does not fit `Int64`. Catch or avoid that conversion before the contextual error boundary.
- The string rule must distinguish `Strings.Get` from an unrelated method named `Get`.
- The Game directory has no source file yet. The fixture rule must still falsify false exemptions.

### Open questions that block progress

No owner question is needed. The author can correct all three findings within the current scope.

### Next concrete action

The author corrects the three findings and requests a repeat review at the new effective head.

## Session 55: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-5, the content loader, the schemas, the content hash, and the string table. Branch `feat/pr-5-content`.

### What this session did, and why

- Started PR-5 after the owner merged PR #16. Four owner questions came before the code, and D-219 to D-223 record the answers.
- D-219: Core takes the bytes from an `IContentSource` and opens no file. This applies D-211 one PR later, and it keeps a directory enumeration order out of the content hash.
- D-220: `Utf8JsonReader` reads the JSON. It uses no serializer and no reflection, and it gives the position that D-92 needs for an error message. A hand-written parser holds the number, escape, and surrogate rules, and PR-4 took three review passes on the escape rules alone.
- D-221: SHA-256 for the content hash, and FNV-1a stays for the state hash. One needs resistance, and the other needs speed.
- D-222: one command, two rule sets. `det-lint` reads Core with the determinism rules and Game with the string rule of G-8.
- D-223 records the allowlist additions: 9 types and 27 members. The lists hold 27 types and 53 members now.
- Wrote `IContentSource`, `ContentFile`, `JsonObjectReader`, `ContentValidator`, `ContentError`, `ContentHash`, `Strings`, `FloorTemplate`, `ProjectileDefinition`, and `ContentLoader` in `Core/Content/`.
- Wrote the first content: three floor templates for the three bands of D-210, two projectile definitions, and the string table.
- Wrote `GameStringScan` in the lint tool, and the command reports the two counts.
- 36 new tests. The total is 270. Exit tests 1 to 6 each have a test.
- The tests caught one real defect. A malformed file let `JsonReaderException` out of Core, and that error names no file. Every content failure names the file, the field, and the reason now (D-92, T-2).

### State of the build

- `main` is at `a37f0af`. The branch holds the PR-5 work above it.
- Remote head: `origin/feat/pr-5-content` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 270 tests and 0 failures.
- `det-lint` reports 0 findings: Core 0 in 21 files, Game 0 in 0 files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. PR-5 adds no simulation number.

### In flight

PR-5 waits for a Codex review (T-4). It changes code, so no override applies.

### Traps and gotchas

- A platform reader raises its own error type, and that error names no file. Wrap it, or the file name never reaches the owner.
- The Game project holds no source file yet, so the Game scan reads nothing on this checkout. The rule has fixture tests, and `LintPassesGame` guards the real directory.
- The three floor bands must cover floors 1 to 15 with no gap and no overlap. `EveryContentFileLoads` counts each depth.
- An optional content field needs a default that the record states. `areaCentimetres` is zero when the file omits it.
- A `Utf8JsonReader` is a ref struct, and it lives inside the try block that catches its error.

### Open questions that block progress

No new owner question. No open question blocks PR-6 to PR-11.

### Next concrete action

A Codex session reviews PR-5 per `.claude/skills/pr-review/SKILL.md`, under the scope rules of D-209, and writes `docs/reviews/pr-<number>.md`.

## Session 54: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-4 merge across the documents. Branch `docs/pr-4-merge-record`.

### What this session did, and why

- The owner merged PR #15 as `1749463`, after a Codex review approved effective head `94afeea`.
- Audited every document against the merged state. The registers were complete: D-211 to D-217, OQ-82 to OQ-86, and F-71 to F-77 all landed with the PR.
- The owner then answered the one open item of PR-4. D-218 ratifies the log name and value rule, and OQ-87 records the question.
- Three lines were stale, and this session corrects each one. The focused roadmap had no status line for PR-4, and its header cited D-200 to D-216 while the PR added D-217. The design doc marked PR-4 as open, in the marker and in the Phase 1 sequence.
- The correction passes of the roadmap now record the PR-4 outcome: five owner questions before the code, three review passes on the logger, and the description rule of D-217.
- The build on `main` is healthy: 234 tests, 0 lint findings, 0 checker findings.

### State of the build

- `main` is at `1749463`, the squash merge of PR #15. This branch holds one commit above it.
- Remote head: `origin/docs/pr-4-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`.

### In flight

This PR changes only `docs/`, so the `review-override` label covers it (D-190). PR-5 starts after the merge.

### Traps and gotchas

- The owner ratified the PR-4 name and value rule as D-218. A field name is an identifier and must hold valid text. A value and a message take the replacement character, because a throw on content loses a crash report at the moment that the owner needs it most.
- D-218 reaches the registers and the two plan documents in this PR. The code comments cite F-77, and F-77 cites D-218, so this PR changes no code and keeps the override.
- PR-5 needs owner answers before its code. The content loader reads files, and D-211 kept `System.IO` out of Core for the logger. The loader also needs a JSON reader and SHA-256, and the D-207 list holds neither. The lint rule of G-8 also reads the Game project, and the tool reads Core alone today.
- A merged PR leaves a status line in three places: the focused roadmap, the design doc marker, and the Phase 1 sequence.

### Open questions that block progress

No open question blocks PR-5 from starting. Four questions come before its code, and the next session files them.

### Next concrete action

The owner merges this documentation PR with the `review-override` label. It carries the merge record and D-218, and the owner asked for one PR. Then a new session starts PR-5: the content loader, the schemas, the content hash, and the string table (D-91, D-92, D-98, D-163, D-168).

## Session 53: 2026-09-08, Codex

Author: Codex
Session: third repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `94afeea` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the P2-7 correction, and Codex reviewed it.
- Closed P2-7. `LogFields` rejects unmatched surrogates in field names with a position context. Valid paired-surrogate names remain accepted, and values and messages keep the replacement character.
- Corrected the stale effective head in the PR description from `d530f4c` to `94afeea` under D-217.
- Updated the review verdict to `Ready for owner merge` for `94afeea`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `94afeea`.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub CI, lint, STE, and bit-identity checks pass at `94afeea`. The review gate reflects the prior review until this metadata commit runs it again.

### In flight

PR #15 is ready for owner merge after the metadata review commit and its review-gate check pass.

### Traps and gotchas

- Replacement is correct for content and wrong for an identifier. Invalid field-name text must fail before serialization.
- The effective head is the code commit `94afeea`; the review record and handoff commit are metadata commits.
- The PR description had a stale effective-head fact even though its code and test counts were current.

### Open questions that block progress

No open question blocks PR-5 to PR-11.

### Next concrete action

Publish the metadata review commit, then verify the review-gate result and leave the owner to merge.

## Session 52: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the second PR #15 repeat review. Branch `feat/pr-4-logging`.

### What this session did, and why

- The review closed P2-3, P2-5, and P2-6, and it opened P2-7. That one has full merit, and the F-73 correction caused it.
- P2-7. The replacement character stood in place of a lone surrogate everywhere, and a field name took it too. A field named with a lone high surrogate and one named with a lone low surrogate both reached the object as one property name, which is the ambiguity that F-72 removed.
- The split is the fix. A field name is an identifier that the code writes, and it must reach the line unchanged, so invalid text in one is an error at the add. A value and a message carry content from the run, and those keep the replacement and never throw (F-77).
- The same pass flattened the add path. `AddField` called `Has`, which put it two levels below the caller, and the new name scan would have made a third. `AddField` calls no method now (D-110).
- The review used D-217 for the first time. It made four edits to the PR description and recorded each one under `## Description edits`. Every edit is correct, and none changes what the PR says it does.
- 1 new test. The total is 234.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, three review commits, and three correction commits.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 234 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head.

### Traps and gotchas

- A fix for content can break an identifier. The replacement character is right for a message and wrong for a field name, because two names can then reach the object as one.
- The response file states the name and value split as a Core contract, and no owner decision holds it. The owner can make it a decision.
- A depth fix and a new check meet. `AddField` was already two levels below its caller, and the name scan would have made a third, so the method calls nothing now.
- D-217 works. The reviewer corrected four stale facts in the description in the same pass that found P2-7.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure, and it updates `docs/reviews/pr-15.md` to the new effective head.

## Session 51: 2026-09-08, Codex

Author: Codex
Session: second repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `d530f4c` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the correction, and Codex reviewed it.
- Closed P2-3. Four labeled surrogate rows run through five text positions, and both parsing and string reading succeed.
- Closed P2-5. All three assertion call-site names are reserved, and the trusted copy path writes a complete report.
- Closed P2-6. D-214, the response, and the primary PR evidence agree on the head, the test total, and the allowlist counts.
- Added P2-7. Distinct accepted field names with unmatched high and low surrogates both serialize as U+FFFD. The resulting JSON object holds two equal property names, against the no-ambiguity contract of `LogFields` and T-2.
- Corrected four stale facts in the PR description under D-217, and recorded each edit in `docs/reviews/pr-15.md`.
- Updated the review verdict to `Changes required` for `d530f4c`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `d530f4c`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 233 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `d530f4c`. The review gate reflects the prior review until this metadata commit runs it again.

### In flight

PR #15 needs a correction for P2-7, then another repeat review.

### Traps and gotchas

- Replacement is not injective. Two invalid UTF-16 field names can become one valid JSON property name.
- A parser accepts duplicate JSON property names. Count the parsed properties or reject the input before serialization.
- The surrogate regression test puts invalid text in a field name, but it does not assert that distinct accepted names stay distinct.
- D-217 permits a reviewer to correct verified stale facts in the PR description. It does not permit changes to an owner-ticked gate line or to the author's substantive claims.

### Open questions that block progress

No open question blocks the P2-7 correction. The correction can reject invalid UTF-16 in field names or preserve a unique emitted name.

### Next concrete action

Correct P2-7 and add a regression test with distinct unmatched high- and low-surrogate field names. Then request another repeat review.

## Session 50: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #15 repeat review, and give the reviewer the PR description. Branch `feat/pr-4-logging`.

### What this session did, and why

- The repeat review closed P2-1, P2-2, and P2-4, and it kept P2-3 open and opened P2-5 and P2-6. All three have full merit.
- P2-3. The review found that three of four theory rows ran. xUnit takes the display name from the data, two rows of raw surrogate text give one name, and the low-surrogate row never ran. Each row carries a label and a code unit now, and the body builds five shapes for each one.
- Those rows then failed, and they showed the first fix was incomplete. `JsonDocument.Parse` accepts the `\u` escape of a lone surrogate, and `GetString` on that value throws. The first probe called `Parse` alone, so it passed and hid the rest.
- `AppendQuoted` writes the replacement character in place of a lone surrogate now. The line parses, and a reader takes the value back with one visible mark (F-73).
- P2-5. A caller field named `assertFile` stopped the assertion report, because the copy already held that name. This is P2-1 one level out: the first fix reserved the two names the logger writes and left the three the report writes. All five are reserved now, and `CopyWithCallSite` writes the call site on a path that no caller field reaches (F-74).
- P2-6. D-214 said 8 types and 16 members while the code held 9 and 18. A count of the lists gave the true numbers, and D-214 states them with the totals after them (F-75).
- The owner asked for a skill change so a reviewer can correct a stale PR description directly. D-217 records it, and the `pr-review` skill gains the rules and a `## Description edits` section in the record (F-76).
- 7 new tests. The total is 233.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, two review commits, and two correction commits.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 233 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head. That review can correct the PR description directly under D-217.

### Traps and gotchas

- Two xUnit theory rows of raw invalid text take one test id, and one row never runs. Give each row a label, and build the text in the body.
- `JsonDocument.Parse` and `GetString` are two gates. A line can parse and still fail when a reader asks for the value. Assert both.
- A reserved-name rule needs every name that the code writes. The first pass covered the logger and missed the assertion report.
- A count in a decision drifts from the code. State the totals, and count the list before you write them.
- A correction can be incomplete and still pass its first probe. Write the probe against the contract, not against the fix.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure, and it updates `docs/reviews/pr-15.md` to the new effective head.

## Session 49: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Re-reviewed PR #15 at effective head `e42a0a8` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the corrections, and Codex reviewed them.
- Closed P2-1. `LogFields` rejects the two logger names before a line exists.
- Closed P2-2. The assertion report uses a copy, so two safe failures preserve the caller fields.
- Kept P2-3 open. The encoder correction passes, but xUnit skips one required surrogate row because two rows have one test id.
- Closed P2-4. `BuildLine` calls two leaf helpers, and the prior nested helpers are absent.
- Added P2-5. A caller field with an assertion call-site name prevents every report and changes a safe failure to an exception.
- Added P2-6. D-214, the response, and the PR description disagree about the member count, test count, and final head.
- Updated `docs/reviews/pr-15.md` with the verdict `Changes required` for `e42a0a8`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `e42a0a8`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` reports 226 passes and 0 failures.
- The test run warns that xUnit skips one duplicate-id surrogate row. The P2-3 regression check is incomplete.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `e42a0a8`.

### In flight

PR #15 needs corrections for P2-3, P2-5, and P2-6, then another repeat review.

### Traps and gotchas

- xUnit can omit a theory row before execution when two invalid strings produce one display id.
- The assertion call-site names are logger-owned names, like `level` and `message`.
- A correction that adds an allowlist member must update the count and every current revision record.

### Open questions that block progress

No new owner question. No open question blocks the corrections.

### Next concrete action

Correct P2-3, P2-5, and P2-6, add the regression checks, and request another repeat Codex review.

## Session 48: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #15 review. Branch `feat/pr-4-logging`.

### What this session did, and why

- Read the four P2 findings in `docs/reviews/pr-15.md`. Each one reproduces, so each one has full merit.
- The first draft of this entry took the number 47, which the review session already held. The D-187 check caught it, and the entry is 48.
- This is the first review under D-209. It used the `## Out of scope` section for the Game-layer sink of PR-31 and PR-55 and for the simulation version constant of PR-7, and it gave neither a severity. Both readings are correct.
- P2-1. A caller field named `level` or `message` gave a line with two properties of one name. `LogFields` now reserves both names and rejects them at the add, so no such line reaches the sink (F-72).
- P2-2. A safe assertion added its call site to the caller field set, so a second safe assertion threw on the repeated name. A safe assertion promises to continue, and this turned the second one into an exception. The report takes a copy now (F-72).
- P2-3. A value with an unpaired surrogate gave a line that the parser rejected. The escape pass reads by index, keeps a matched pair as one character, and escapes every other surrogate. A check showed that `JsonDocument.Parse` accepts the escape form, so the fix keeps the value and never throws on a message (F-73).
- P2-4. The JSON write path ran three helper levels below its operation. `AppendText`, `AppendRaw`, and `HexDigit` are gone, and `BuildLine` calls only leaves (D-110, F-73).
- The P2-3 fix needs `System.String.this[]` for the one-character lookahead, and the owner approved that entry. D-214 records it.
- 7 new tests. The total is 226.

### State of the build

- `main` is at `51b3de7`. The branch holds the PR-4 work, the review commit, and this correction commit.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 226 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #15 needs a repeat review at the new effective head.

### Traps and gotchas

- A helper that takes a caller object and adds to it changes that object for every later call. A report takes a copy.
- The logger writes its own `level` and `message`, so those two names need a rule of their own. A duplicate-name check over the caller fields alone does not reach them.
- An unpaired surrogate is not text that UTF-8 can hold. A parser rejects the raw form and accepts the `\u` escape form.
- D-110 counts every level. `BuildLine` to `AppendText` to `AppendQuoted` to `HexDigit` is three, and one is the limit.
- A patch script that fails part way leaves the file unchanged, and the build then passes on the old code. Read the file after a large scripted edit.

### Open questions that block progress

No new owner question. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session re-reviews PR #15 per the repeat review procedure and updates `docs/reviews/pr-15.md` to the new effective head.

## Session 47: 2026-09-08, Codex

Author: Codex
Session: review PR #15 for PR-4. Branch `feat/pr-4-logging`.

### What this session did, and why

- Reviewed PR #15 at effective head `41206bb` against base and merge base `51b3de7`.
- Confirmed the provider gate. Claude Code wrote the substantive change, and Codex reviewed it.
- Inspected the complete diff, the six exit tests, all changed documents, and the current callers.
- Added P2-1. Caller fields can add a second `level` or `message` property to the JSON object.
- Added P2-2. One safe assertion changes the caller fields, so a second safe assertion throws on `assertFile`.
- Added P2-3. An unpaired surrogate passes the logger and produces JSON that `JsonDocument` rejects.
- Added P2-4. The JSON write path has nested helper calls beyond the one level that D-110 permits.
- Wrote `docs/reviews/pr-15.md` with the verdict `Changes required`.

### State of the build

- `main` and the merge base are at `51b3de7`. The reviewed effective head is `41206bb`.
- Remote head: `origin/feat/pr-4-logging` holds the metadata commit for this entry, verified with the session-end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 219 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `41206bb`.
- Three focused review probes fail and reproduce P2-1, P2-2, and P2-3.

### In flight

PR #15 needs corrections for P2-1 to P2-4, then a repeat Codex review.

### Traps and gotchas

- `LogFields` checks duplicates only inside the caller set. It does not reserve the logger keys.
- `Invariant.Assert` adds its call-site fields to the object that the caller owns.
- A JSON control-character check does not cover invalid UTF-16 surrogate sequences.
- A green broad suite did not cover repeated safe failures or the complete string domain.

### Open questions that block progress

No new owner question. No open question blocks the corrections.

### Next concrete action

Correct P2-1 to P2-4, add regression tests, and request a repeat Codex review.

## Session 46: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-4, the logger, the error context, and the assertions. Branch `feat/pr-4-logging`.

### What this session did, and why

- Started PR-4 after the owner merged PR #14. No open question blocked it, and PR-4 is the first PR with no absent check, so the D-148 clause does not apply.
- Five owner questions came before the code, and D-211 to D-216 record the answers.
- D-211: Core builds a line and hands it to an `ILogSink`. Core opens no file, so a disk failure never reaches the simulation thread, and `System.IO` stays out of the allowlist.
- D-212: every line carries a level and a message beside its context fields. No line carries a wall clock, because the tick is the only time in Core.
- D-213: `StringBuilder` builds the line, because the escape pass appends one character at a time.
- D-215: the assertion report names the call site from the compiler, and it walks no stack. A stack trace changes with the build and the platform. This revises the PR-4 scope line, which said "the stack".
- Wrote `LogLevel`, `LogContextKind`, `ILogSink`, `LogFields`, `ContextException`, `JsonlLogger`, and `Invariant` in `Core/Logging/`.
- 26 new tests. The total is 219. Exit tests 1 to 6 each have a test, and the JSON tests parse each line with a real parser.
- D-214 records the allowlist additions: 8 types and 16 members. A removal check proved each one in use. It also found two dead entries in the PR-3 list, `System.Object` and `List.new/0`, and both are gone.
- The removal check found a live gap. `CultureInfo c = new("en-US", true);` gave no finding, and `new CultureInfo("en-US", true)` gave one, so the P2-9 case was reachable by another spelling. A `base` initializer and an indexer passed the same way (F-71). D-216 closes all three, and the owner chose the fix in this branch over a separate PR.
- `det-lint` caught one real defect in the new Core code. `context.ToString()` on an enum reads the enum metadata, which G-2 bans. An explicit switch replaces it.

### State of the build

- `main` is at `51b3de7`. This branch holds the PR-4 work above it.
- Remote head: `origin/feat/pr-4-logging` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 219 tests and 0 failures.
- `det-lint` reports 0 findings in 11 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. PR-4 adds no simulation number.

### In flight

PR-4 waits for a Codex review (T-4). It changes code, so no override applies.

### Traps and gotchas

- The allowlist needs a removal check, and not a reading. Two entries of the PR-3 list were dead, and one gap hid behind a spelling that the rule never read.
- A dead allowlist entry widens the boundary in silence. Check each new entry by removal before the PR opens.
- `det-lint` reads the new Core code as it lands. It caught an enum `ToString` in this session, which is reflection under G-2.
- A collection expression, `[]`, calls no constructor that the source names, so `List.new/0` stayed dead.
- An xUnit lambda with every path throwing binds to `Func<Task>` and not to `Action`. Name the delegate type.
- A test that asserts a lint finding breaks when a later PR approves that type. Two such tests moved to a type that stays unapproved.

### Open questions that block progress

No new owner question. OQ-82 to OQ-86 are resolved by D-211 to D-216. No open question blocks PR-5 to PR-11.

### Next concrete action

A Codex session reviews PR-4 per `.claude/skills/pr-review/SKILL.md`, under the scope rules of D-209, and writes `docs/reviews/pr-<number>.md`.

## Session 45: 2026-09-08, Claude Code

Author: Claude Code
Session: record the PR-3 merge across the documents. Branch `docs/pr-3-merge-record`.

### What this session did, and why

- The owner merged PR #12 as `9e6fd5f`, after a Codex review gave the verdict `Ready for owner merge` at effective head `4d7cb3e`.
- Audited every document against the merged state. The registers were complete: D-200 to D-209, OQ-73 to OQ-81, and F-60 to F-70 all landed with the PR.
- Four documents held a stale line, and this session corrects each one.
- `docs/roadmaps/phase-1-foundations.md`: the PR-3 status line said "opened", and it names the merge commit now. The header cited D-200 to D-203, and it cites D-200 to D-209 now. The correction passes record the PR-3 outcome.
- `docs/design.md`: the PR-2 and PR-3 markers now name the merge, as the PR-1 marker does. The Phase 1 sequence marks PR-2 and PR-3 as merged.
- `dotnet test` caught the one defect in this session. `RepositoryDocumentsPass` runs the STE checker over the repository, and a new sentence of 26 words failed it. The sentence is three sentences now. A second sentence failed the same limit later, for the biome entry.
- The owner answered OQ-12, the last open question of Phase 1 before PR-9. The v1 biome is a collapsed deep mine (D-210). The block set holds seven blocks, and the three depth bands are floors 1 to 5, 6 to 10, and 11 to 15.
- The biome answer reaches four documents: the decision register, the questions register, the design doc world section and PR-9 entry, and the PR-9 scope in the focused roadmap.
- D-210 feeds four open questions that a later phase needs: OQ-1 the palette, OQ-61 the silhouettes, OQ-64 the props, and PR-14 the texture generator. The three props of OQ-64 belong in a mine with no change.

### State of the build

- `main` is at `9e6fd5f`, the squash merge of PR #12. This branch holds one commit above it.
- Remote head: `origin/docs/pr-3-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs on `main` at `9e6fd5f`.

### In flight

This PR changes only `docs/`, so the `review-override` label covers it (D-190). It carries two concerns, the merge record and the biome decision, and the owner asked for one PR. PR-4 starts after the merge.

### Traps and gotchas

- `RepositoryDocumentsPass` fails the test suite on any STE finding. A document edit needs `dotnet test`, and not the checker alone.
- A merged PR leaves a status line in two places: the focused roadmap and the design doc. The design doc also holds the Phase 1 sequence, which is a third place.
- The roadmap header lists the decisions that the file applies. A PR that adds a decision extends that list.
- Phase 1 has no gate before PR-11. Gate 1 needs the bit-identity job, `dotnet test`, and the night sweep of PR-58.

### Open questions that block progress

No new owner question. D-210 resolves OQ-12, so no open question blocks any PR of Phase 1. PR-4 to PR-11 each have every answer they need.

### Next concrete action

The owner merges this documentation PR with the `review-override` label. Then a new session starts PR-4: the JSONL logger, the error context sets, and the assertion helper (D-68, D-112, D-113). PR-4 is the first PR with no absent check, so the check clause of D-148 does not apply to it.

## Session 44: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #12 at the changed effective head. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Reloaded the `pr-review` skill after the owner approved its change as a second concern on this PR.
- Re-reviewed PR #12 at effective head `4d7cb3e` against base and merge base `86078b8`.
- Inspected the D-208 member allowlist correction and the D-209 review-scope rules. The provider gate passes because Claude Code wrote the substantive changes and Codex reviewed them.
- Closed P2-6. The PR description now identifies the effective head alone, its decision counts agree with its D-200 to D-209 table, and the P2-6 row describes the correction that the page contains.
- Closed P2-9. A focused Core probe reported the original comparer, runtime-feature, and current-culture paths, and it reported the later culture-constructor and string-intern paths.
- Added the required `## Out of scope` section to the review record. This review found no concern to put there.
- Set the verdict to `Ready for owner merge` for effective head `4d7cb3e`.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `4d7cb3e`.
- Remote head: `origin/feat/pr-3-determinism` will hold the metadata commit for this entry after the session-end push. The effective head stays `4d7cb3e` under D-184.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `4d7cb3e`. The review-gate results still reflect the prior verdict until this metadata commit runs them again.

### In flight

PR #12 is ready for the owner to merge after the review-gate checks turn green for this review record.

### Traps and gotchas

- The effective head includes the skill change because `.claude/skills/pr-review/SKILL.md` is outside the metadata set.
- D-209 closes an existing finding when its stated trigger and regression check pass. A different trigger takes a new id and must pass the PR-scope test before it becomes a finding.
- The focused probe is temporary. Remove it before the clean lint and build gates.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9 and does not block this review.

### Next concrete action

Wait for the review-gate checks on the metadata commit. The owner can then squash-merge PR #12.

## Session 43: 2026-09-08, Claude Code

Author: Claude Code
Session: the scope rules for the `pr-review` skill. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- The owner asked for a skill change after seven review passes on PR #12. That review reopened one finding four times and another one three times (F-70).
- Each pass found a real defect, and each correction was right. The sum went past the PR-3 scope, which names a lint tool for the banned symbols of G-2 and G-21. Five owner decisions, D-204 to D-208, came from that one boundary.
- Added a "Stay inside the pull request" section. The roadmap entry for the PR and its exit tests set the boundary. Four tests say a concern is in scope, and four say a later PR holds it.
- Added the `## Out of scope` heading to the review record skeleton. A line there names the PR that holds it, takes no severity, and never gives the verdict `Changes required`.
- Added the rule for a new check. A PR that creates a check must pass that check (G-19), and the check does not cover the whole platform on the first day.
- Added a "When a finding closes" section. A finding closes when its stated trigger and its regression check pass. A new trigger of the same class takes a new id. The third assessment of one id stops, and the owner settles the scope.
- Added two rows to the author push-back table, for a finding outside the scope and for a third reopen.
- The section states in two places that it never lowers the standard for the code that a PR changes. Every PR #12 finding stays a defect under these rules.
- This session first opened PR #13 for the skill change, on the G-10 reading that a skill change and the determinism work are two concerns. The owner asked for it on PR #12 instead. PR #13 is closed, and its branch is deleted.

### State of the build

- `main` is at `86078b8`. This branch holds the PR-3 work and this skill change above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged. This commit changes no code.

### In flight

PR #12 needs a repeat review at the new effective head, under the new scope rules.

### Traps and gotchas

- `git checkout <file>` restores from HEAD and drops every uncommitted edit in that file. This session lost the whole skill change that way and wrote it again. Commit first, or copy the file.
- A scope rule can hide a real defect. Each rule here names the code that the PR changes as the part that keeps the full standard.
- The skill file was identical on `main` and on this branch, so the change moved between branches as a whole file. Check that before a copy.
- A second PR for a second concern is the G-10 reading, and the owner decides when one PR carries both.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 under the new scope rules, and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 42: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the seventh PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The seventh review kept P2-6 and P2-9 open. Both reproduce, so both have full merit.
- P2-9, third pass. The type allowlist approved every member of an approved type. `new CultureInfo("en-US", useUserOverride: true)`, `string.Intern(v)`, and `string.IsInterned(v)` each gave no finding, and the .NET contract names the user settings and the process intern pool.
- The gap moved from the namespace to the type, and the type still approved its whole surface. A member denylist would miss `new CultureInfo("en-US")`, which has the same behavior as the two-argument form.
- A measurement found six members and two constructors of an outside type in Core. The owner chose the member allowlist with the overload arity (D-208, OQ-81, F-69).
- The arity closes a gap that no trigger named. `UInt64.ToString/2` takes a format provider, and `ToString/0` reads the current culture. The same split binds `Parse`, `TryParse`, and `Compare`.
- Two corrections came from a run and not from the finding. A named argument carries a containing type and is not a member of it, so `useUserOverride:` gave a false finding until the rule read a method, a property, a field, and an event alone. `Object.GetType` joined the member denylist, so it keeps the `L-REFLECTION` id.
- P2-6, fourth pass. The decision section counted rows and not ids, the P2-6 row named a session that the description no longer holds, and the summary named the pass count and the newest review head. All three are corrected.
- 193 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds seven review commits and seven correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 193 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- The boundary moved four times: namespace, then `System` type, then every type, then every member. Each level approved the whole of the level below it. A member entry with an overload arity is the first form with nothing below it to leak.
- A named argument and a local both carry a containing type. Read a method, a property, a field, and an event alone.
- Keep a specific rule id beside the wide one. `Object.GetType` sits in the member denylist, so the finding says reflection and not member.
- An overload is not a behavior. `ToString/0` and `ToString/2` differ, and so do the `Parse` and `Compare` families.
- A description that names a pass count or a review head goes stale at the next review. Name the effective head alone.

### Open questions that block progress

No new owner question. OQ-81 is resolved by D-208. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 41: 2026-09-08, Codex

Author: Codex
Session: sixth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `e30ddfd` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that the original P2-9 triggers now report `L-TYPE`, `L-TYPE`, and `L-CLOCK`.
- Kept P2-9 open. `CultureInfo` and `String` are approved types, and their other machine-dependent members still pass.
- A probe used the `CultureInfo` constructor with user overrides. It also used `String.Intern` and `String.IsInterned`.
- The probe compiled, and `det-lint` reported 0 findings. The .NET contracts confirm that these APIs read user or process state.
- Kept P2-6 open. The PR description gives the wrong decision count and contains two claims that contradict its current content.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `e30ddfd`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 187 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `e30ddfd`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-6 and P2-9, then another repeat review.

### Traps and gotchas

- A type allowlist approves every member of an approved type unless another rule limits the members.
- `CultureInfo` has the required `InvariantCulture` member and constructors that read user settings.
- `String.Intern` changes the process intern pool. `String.IsInterned` reads that process state.
- Volatile pass counts and review heads become stale when the required next review completes.

### Open questions that block progress

No new owner question was filed in this review. The P2-9 correction can require a revision to D-207. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-6 and P2-9. Add regression tests for the three new triggers, then request another repeat review.

## Session 40: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the sixth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The sixth review closed P2-7 and P2-8, and it kept P2-6 open and opened P2-9. Both reproduce, so both have full merit.
- P2-9. `EqualityComparer<string>.Default.GetHashCode(v)` gave no finding. The member belongs to `EqualityComparer`, and D-205 approved `System.Collections.Generic` as a whole namespace, which D-207 supersedes. The reviewer ran three processes and got three hashes for one string.
- The cause is the same shape as P2-7, one level out. Each approved namespace holds a machine-dependent type beside the one Core needs.
- A run with the four namespaces removed named one type: `CultureInfo`. The owner chose the one allowlist with that number in hand (D-207, OQ-80, F-68).
- D-207 supersedes D-205 and D-206. Core approves every external type by full name, and no namespace passes as a whole. The list holds ten entries, and the tool is smaller: one list serves the type rule and the import rule.
- `CultureInfo` needs both rules. Core formats with `InvariantCulture`, and the same type holds `CurrentCulture`. The member denylist holds the five culture members that read the user, the process, or the machine.
- `System.Array` and the collection types are absent, because Core uses neither today. PR-7 will add `System.Array` with its decision.
- P2-6, third pass. The description listed four owner decisions while the same page named seven. It lists every decision now, with the finding that produced it, and it names no session number.
- 187 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds six review commits and six correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 187 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A whole-namespace approval fails at every level. The namespace list leaked `Guid`, and then the `System` type list leaked `EqualityComparer` through the other namespaces. Approve each type by name.
- A type allowlist does not remove the member denylist. `CultureInfo` holds `InvariantCulture` and `CurrentCulture`, and a type with both kinds of member needs both rules.
- A superseded decision needs the superseding id on every line that cites it (D-178). The reference check found four such lines in one commit, and the checker is the only reason they did not ship.
- A large edit by text slice can delete a neighbor. This session removed five helpers with one block replacement, and the build named each one.
- Measure the friction before the question. Seven types settled D-206, and one type settled D-207.

### Open questions that block progress

No new owner question. OQ-80 is resolved by D-207. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 39: 2026-09-08, Codex

Author: Codex
Session: fifth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `9ad2a4c` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-7 is fixed. `Guid.NewGuid()` now reports `L-RANDOM`.
- Confirmed that P2-8 is fixed. An unused `using System.Text;` now reports `L-NAMESPACE`.
- Kept P2-6 open. The PR description says that four owner decisions were needed and lists D-200 to D-203, but the same description identifies D-200 to D-206 as the decisions for the PR.
- Added P2-9. `EqualityComparer<string>.Default.GetHashCode(value)` compiles in Core and gives no lint finding. Three processes gave three values for the same string.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `9ad2a4c`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 183 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `9ad2a4c`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-6 and P2-9, then another repeat review.

### Traps and gotchas

- A member ban reads the type that declares the member. `EqualityComparer<string>.GetHashCode` reaches the string hash, but the member belongs to the comparer and not to `String`.
- A whole-namespace approval can admit process state through a type that the decision never names.
- Run a process-randomized hash probe in separate processes. Repeated calls inside one process use one seed.
- A session number in the PR description becomes stale when the required review adds the next handoff entry.

### Open questions that block progress

No new owner question was filed in this review. The correction for P2-9 can require a revision to D-205. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-6 and P2-9, add a regression test for the comparer trigger, and request another repeat review.

## Session 38: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the fifth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The fifth review closed P2-3 and P2-6, and it opened P2-7 and P2-8. Both reproduce, so both have full merit.
- P2-7. `System.Guid.NewGuid()` compiles in Core and gave no finding. D-205 approved `System` as a whole namespace, and `System` is the one broad namespace that Core uses, so the rest of the nondeterminism sits inside it.
- Before the question, this session measured the cost. A run with `System` removed from the allowlist named seven `System` types in Core. The owner chose the type allowlist with that number in hand (D-206, OQ-79, F-67).
- D-206 revises D-205 in part. `System` is approved by type now, and the other approved namespaces stand as whole namespaces.
- `Guid` and `HashCode` also join the type denylist, so each reports its own rule. The review asks for a randomness finding on `Guid.NewGuid()`, and it reads `L-RANDOM`.
- One correction goes past the finding. `Object.GetHashCode` and `String.GetHashCode` report `L-IDENTITY`, because a member of an approved type can still read the machine. A hash of an address changes an iteration order between two runs of one seed.
- The first form of the `System` rule was wrong, and the check caught it. The rule also read the type a method gives back, and `det-lint` reported 24 findings on clean Core code. The allowlist binds the names that Core writes, never a return type.
- P2-8. `using System.Text;` gave no finding, because `AddImportFinding` read the denylist alone. It calls the allowlist now.
- 183 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds five review commits and five correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 183 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- An allowlist at the namespace level leaves the one namespace that the project uses. `System` holds `Guid`, `HashCode`, `GC`, `OperatingSystem`, `Console`, and `AppContext`. Approve that namespace by type.
- A type rule must bind the names that the source writes, not the type a method gives back. The first form flagged every method that gives back a bool. Run the tool on clean code before you keep a rule.
- A member of an approved type can still read the machine. `Object.GetHashCode` gives an address, and `String.GetHashCode` takes a new seed in each process.
- An allowlist binds each entry point on its own. The import check and the use check are two entry points, and P2-8 was the one that nobody updated.
- Measure the friction before you ask the owner for a stricter rule. Seven types was the number that settled the question.

### Open questions that block progress

No new owner question. OQ-79 is resolved by D-206. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 37: 2026-09-08, Codex

Author: Codex
Session: fourth repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `60d678e` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-3 is fixed. The allowlist blocks the prior reflection and metadata probes.
- Confirmed that P2-6 is fixed at this effective head. The PR description identifies the current correction evidence.
- Added P2-7. `Guid.NewGuid()` compiles in Core and gives no lint finding against the seed-only rule.
- Added P2-8. An unapproved `using System.Text;` directive compiles and gives no `L-NAMESPACE` finding.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `60d678e`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 172 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `60d678e`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-7 and P2-8, then another repeat review.

### Traps and gotchas

- The exact `System` namespace contains `Guid.NewGuid()` and other nondeterministic APIs.
- A namespace allowlist still needs symbol bans inside each approved namespace.
- `AddImportFinding` applies the old namespace denylist, but it does not apply the new allowlist.
- An unused `using` directive compiles without a warning in the Core project.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-7 and P2-8, add their regression tests, and request another repeat review.

## Session 36: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the fourth PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The fourth review kept P2-3 and P2-6 open. Both reproduce, so both have full merit.
- P2-3 reopened for the fourth time. `System.ComponentModel.TypeDescriptor.GetProperties` compiles in Core and gave no finding. Each pass named one more type: namespace text, member words, `System.Enum`, then `TypeDescriptor`.
- The denylist was the defect, not its contents. `System.Linq.Expressions`, `System.Text.Json`, `System.Runtime.Serialization`, and `System.Dynamic` all reach type metadata under no name that a rule held. A fifth pass was likely.
- The owner chose the namespace allowlist (D-205, OQ-78, F-66). Core uses `System`, `System.Collections.Generic`, `System.Globalization`, `System.Numerics`, `System.Runtime.CompilerServices`, and any namespace under `WhatYouCarry.`. Each entry matches one namespace and never its children, so `System` does not approve `System.ComponentModel`.
- The type denylist stays for the cases inside an approved namespace. `TypeDescriptor` joined it too, so the review's regression check reads `L-REFLECTION` and not the wider rule.
- Core used two namespaces, so the rule changed no Core file. An adversarial run reported all five planted uses, and three of them name a namespace that no denylist ever held.
- P2-6, second pass. The description named head `5316033` and session 28 after the first correction. It now names the effective head and the current session.
- 172 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds four review commits and four correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 172 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### Traps and gotchas

- A denylist over a library the size of the class library never ends. Four review passes proved it on this PR. Turn the boundary around and approve what enters.
- An allowlist entry matches one namespace and never its children. `System` must not approve `System.ComponentModel`, so the match is exact.
- Keep the type denylist beside the allowlist. `System.Math` and `System.Type` sit inside an approved namespace, and only the denylist reaches them.
- A specific rule id carries more than a wide one. `TypeDescriptor` sits in both lists, so the finding says reflection and not namespace.
- The PR description is part of the record (D-118), and a review reads it. Update it with each correction, and name the effective head in it.

### Open questions that block progress

No new owner question. OQ-78 is resolved by D-205. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 35: 2026-09-08, Codex

Author: Codex
Session: third repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `549c75c` against base and merge base `86078b8`.
- Confirmed that the provider gate passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that P2-5 is fixed. The lint reports active and inactive `#if` directives under D-204.
- Kept P2-3 open. `TypeDescriptor.GetProperties(object)` compiles in Core and gives no reflection finding.
- Kept P2-6 open. The PR description omits effective head `549c75c` and incorrectly identifies Session 28 as current.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `549c75c`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 160 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `549c75c`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-3 and P2-6, then another repeat review.

### Traps and gotchas

- `TypeDescriptor` accesses type metadata without a `System.Reflection` symbol or a `System.Type` result.
- A finite banned-type table needs probes against all BCL metadata-access surfaces.
- The PR description must identify the effective head, not only the prior review head.
- A session number in the PR description becomes stale after each review response.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3, then update the PR description for P2-6. Request another repeat review.

## Session 34: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the third PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The third review kept P2-3 open and added P2-5 and P2-6. All three reproduce, so all three have full merit.
- P2-3, third pass. The symbol read was correct, and the table was short. `System.Enum.IsDefined` gave no finding, and session 28 had already named `Enum.IsDefined` as reflection when it removed the call from `Rng.ForStream`. The tool contradicted the code it guards.
- The table gains `System.Enum`, `System.Attribute`, `System.AppDomain`, `System.Delegate`, `System.MulticastDelegate`, the three runtime handle types, and `RuntimeHelpers`. The scan also reports the `typeof` keyword, because no symbol carries that name and a type value can reach a place where no name is banned.
- An ordinary enum stays legal. The ban reads `System.Enum`, never the Core enum that declares the values, and `AnOrdinaryEnumIsNotAFinding` holds that.
- P2-5. A `System.Math.Sin` call inside `#if NET10_0` compiles in the Core build and gave no finding. The owner chose the ban on conditional compilation over the build symbols (D-204, OQ-77, F-65). The other option cannot be complete, because Debug and Release are both real builds and a lint parses one.
- `det-lint` reports each `#if` as `L-CONDITIONAL`. A `#nullable`, `#region`, or `#pragma` directive stays legal. Core held no conditional directive, so no Core file changed.
- P2-6. The PR description named the superseded word list, 144 tests, and an old head. It now holds a table of all six findings, the symbol read, and the current evidence.
- 160 tests pass. The bit-identity hash is unchanged, because no Core number changed.

### State of the build

- `main` is at `86078b8`. The branch holds three review commits and three correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 160 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` gives `4d6385bb92454694`, unchanged.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- A symbol table is a list, and a list is never complete on its own. Check the table against the code the project already rejected. Session 28 removed `Enum.IsDefined`, and the tool did not know it.
- A ban on a platform type is not a ban on the language feature. `System.Enum` owns the metadata methods, and a Core enum owns its values. Test the ordinary use.
- `#if` is trivia. A node walk does not reach it, and `DescendantNodes(descendIntoTrivia: true)` does.
- The lint parse defines no preprocessor symbol, so an active branch reads as empty. That is why D-204 bans the directive and does not read the branch.
- The PR description is part of the record (D-118). Update it with each correction, not at the end.

### Open questions that block progress

No new owner question. OQ-77 is resolved by D-204. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 33: 2026-09-08, Codex

Author: Codex
Session: second repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `5316033` against base and merge base `86078b8`.
- Confirmed that the provider gate still passes. Claude Code wrote the correction, and Codex reviewed it.
- Confirmed that the semantic scan fixes the two prior P2-3 probes and the F-# citations.
- Kept P2-3 open. `Enum.IsDefined` compiles in Core and gives no reflection finding, against G-2 and the Session 28 record.
- Added P2-5. A `System.Math.Sin` call under active `#if NET10_0` code builds, but the lint reports no finding.
- Added P2-6. The PR description still reports the removed member word list, 144 tests, and the old effective head.
- Updated `docs/reviews/pr-12.md` with the changed hash and the new evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `5316033`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 146 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `5316033`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs corrections for P2-3, P2-5, and P2-6, then another repeat review.

### Traps and gotchas

- Reflection APIs also exist outside `System.Type`, `System.Activator`, and the `System.Reflection` namespace.
- A semantic scan only reads the branch that its parse symbols select.
- The Core build defines target-framework symbols that the lint compilation does not define.
- The PR description is part of the evidence record. Update it after a correction changes the implementation.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3 and P2-5, then update the PR description for P2-6. Request another repeat review.

## Session 32: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #12 repeat review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- The repeat review closed P2-1, P2-2, and P2-4, and it kept P2-3 open with two probes. Both reproduce, so the finding has full merit.
- `Type.GetEvents()` gave no finding, because `GetEvents` was absent from the member word list. A Core `probe.GetMethods()` gave a false `L-REFLECTION`, because the word matched.
- No word list can fix both. A Core class that declares `public new string GetType()` compiles, and this session checked that. The list itself was the defect.
- `CoreSourceScan` now compiles the Core sources with `CSharpCompilation` and reads the semantic model. The rules match the type that owns a symbol, the namespace of that type, and the type a method or property gives back. That last rule catches `object.GetType()`, which belongs to `System.Object` and not to `System.Type`.
- `BannedSymbols` holds full type names now. The member word list is gone.
- Two T-2 guards. The compilation checks that `System.Math` and `System.Reflection.Assembly` resolve, because without references every name resolves to nothing and the scan would pass every file. The repository scan reports every compiler error, because a Core file that does not compile resolves no symbol.
- Corrected the F-# citations. The reflection comments cited F-62, the `Atan2` defect. F-64 is the register entry, and it now records both correction passes. D-202 carries a dated note.
- Rewrote the lint tests. Each fragment compiles on its own now, so the tests assert real symbol behavior and not an unresolved name.
- 146 tests pass. The bit-identity hash is unchanged, because this pass changed no Core number.

### State of the build

- `main` is at `86078b8`. The branch holds the two review commits and two correction commits above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 146 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- An adversarial Core tree with nine planted uses gave 10 findings, and no finding for a Core `Vector3` in the same file.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- A lint that matches words has two failure modes at once, and each fix makes the other worse. Ask the compiler.
- A semantic scan with no metadata reference resolves nothing and reports nothing. That is a silent pass, so the canary check is not optional.
- `var` resolves to the type the compiler inferred, so it reports the same use a second time. Skip it.
- A test fragment that names an undefined type resolves no symbol, so it gives no finding. A lint test must compile, or it passes for the wrong reason.
- `object.GetType()` belongs to `System.Object`. The owner rule cannot see it, and the rule for the type it gives back can.
- The scan reads the last name of a dotted chain. `System.Math.Sin` holds three names for one call.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 31: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #12. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Re-reviewed PR #12 at effective head `1bc665b` against base and merge base `86078b8`.
- Confirmed that the provider gate still passes. Claude Code wrote the corrections, and Codex reviewed them.
- Confirmed that P2-1, P2-2, and P2-4 are fixed, with regression coverage.
- Kept P2-3 open. The new name list gives both a false negative and a false positive.
- A probe with `Type.GetEvents()` gave no finding. A user-defined `probe.GetMethods()` call gave `L-REFLECTION`.
- Updated `docs/reviews/pr-12.md` with the changed hash and the repeat-review evidence.

### State of the build

- `main` and the merge base are at `86078b8`. The reviewed effective head is `1bc665b`.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 144 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The local bit-identity hash is `4d6385bb92454694`. The Godot 4.7.2 headless build check passes.
- GitHub reports green CI, lint, STE, and bit-identity jobs at `1bc665b`.
- The review-gate results stay red because the review verdict stays `Changes required`.

### In flight

PR #12 needs a complete P2-3 correction and another repeat review.

### Traps and gotchas

- Member text does not identify the member symbol. A Core type can declare a method with a reflection-like name.
- A finite reflection member list can miss a supported `System.Type` API.
- The P2-3 code comments cite F-62. F-64 is the register entry for the reflection defect.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-3 with complete reflection detection and tests for both probe cases. Then request another repeat review.

## Session 30: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #12 review. Branch `feat/pr-3-determinism`.

### What this session did, and why

- Read the four P2 findings in `docs/reviews/pr-12.md`. Each one reproduces, and each one has full merit.
- P2-1. `Atan2` read the sign of y with `y < 0.0f`, and negative zero is not below zero. `Atan2(-0, -1)` gave pi against the reference -pi, an error of two pi. The sign now comes from `float.IsNegative` (F-62).
- P2-2. A `StateHash` from `default` held zero and not the FNV offset basis, and it accepted fields in silence. The struct now rejects a hash that `Start` did not make (T-2, F-63).
- P2-3. The reflection rule read the namespace text alone, so `typeof(x).GetMethods()` gave no finding. A reflection member name is now a finding on its own. The list holds no name that a Core type can hold too, so `block.Type` stays clean.
- P2-4. The `MathF` exemption compared the file name alone. It now compares the whole Core-relative path (F-64).
- The bit-identity sweep never made a negative zero, so the three-platform check could not have caught P2-1. The sweep now holds the four sign pairs. The pinned hash moved from `ef592d4148eb8ba0` to `4d6385bb92454694`. Core gives the same numbers for every input the old sweep read, so this is a wider check and not a behavior change.
- Wrote `docs/reviews/pr-12-response.md` with the disposition and the evidence for each finding.
- 11 new tests. The total is 144, and all pass.

### State of the build

- `main` is at `86078b8`. The branch holds the review commit and this correction commit above it.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 144 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- Each of the five review triggers failed against `c2ba592` before the corrections.

### In flight

PR #12 needs a repeat review at the new effective head.

### Traps and gotchas

- Negative zero passes every comparison against zero. Read the sign bit with `float.IsNegative` when the sign matters.
- A struct that `default` makes skips every factory. A hash, a counter, or any accumulator with a non-zero start needs a guard.
- A determinism sweep proves only what it reads. The `Atan2` grid held no negative zero, so it could not see the defect. Widen the sweep with each defect it missed.
- A first correction for P2-1 added a branch for a negative zero x. A check showed the branch changed no result, and it is gone. Test a defensive branch before you keep it.
- The pinned bit-identity hash changes when the sweep grows, not only when Core changes. Say which one it was.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #12 per the repeat review procedure and updates `docs/reviews/pr-12.md` to the new effective head.

## Session 29: 2026-09-08, Codex

Author: Codex
Session: review PR #12. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Reviewed PR #12 at effective head `c2ba592` against base and merge base `86078b8`.
- Confirmed the provider gate. Claude Code wrote the substantive change, and Codex reviewed it.
- Found four P2 defects in DetMath, StateHash, and the determinism lint.
- Wrote `docs/reviews/pr-12.md` with the verdict `Changes required`.

### State of the build

- The local build passes with 0 warnings and 0 errors.
- The local test suite passes with 133 tests and 0 failures.
- The lint, STE, bit-identity, and Godot checks pass.
- The remote branch contains the review record and this entry.

### In flight

PR #12 needs P2-1 through P2-4 corrected and a repeat review.

### Traps and gotchas

- A numeric grid that contains zero does not necessarily contain negative zero.
- A public struct always permits default initialization.
- A namespace scan does not detect all reflection calls.
- A file-name check does not identify one canonical path.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 through P2-4, add regression tests, and request a repeat Codex review.

## Session 28: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-3, the seeded RNG, DetMath, the lint tool, and the bit-identity job. Branch `feat/pr-3-determinism`, PR #12.

### What this session did, and why

- Started PR-3 after the owner merged PR #11. Session 27 named it as the next action.
- Four owner questions blocked the start, and the owner answered all four (D-200 to D-203, OQ-73 to OQ-76).
- The largest one refutes D-161. That decision named a reduction to [-pi, pi], degree-7 minimax polynomials, and an absolute error of at most 1e-6. A Remez fit of degree 7 on [-pi, pi] reaches 2.5e-4 for sine, 250 times the target. D-203 folds to [-pi/4, pi/4] and a quadrant instead, and the degree and the target stand (F-60).
- Wrote `Rng.cs` (xoshiro128** with SplitMix64 seeding), `DetMath.cs`, `StateHash.cs`, and `RngStream.cs` in Core.
- Wrote the `det-lint` command, which parses each Core file with the C# compiler API (D-202), and the `bit-identity` command (D-201).
- Wrote `.github/workflows/bit-identity.yml` and `.github/workflows/det-lint.yml`. The bit-identity workflow passes each hash up as a job output, so it needs no artifact action.
- 70 new tests. The total is 133, and all pass. Exit tests 1 to 7 each have a test, and the CI run proved exit test 8.
- Corrected exit test 5. The roadmap asked for the opposite of D-160, and no FNV-1a hash can hold the roadmap form (F-61).
- Updated the PR template. PR-3 creates the lint tool and the bit-identity job, so the two bootstrap notes are gone.

### State of the build

- `main` is at `86078b8`, the squash merge of PR #11. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/feat/pr-3-determinism` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` passes with 0 warnings. `dotnet test` passes with 133 tests and 0 failures.
- `det-lint` reports 0 findings in 4 Core files. `ste-check` reports 0 findings in 15 files.
- The bit-identity job on PR #12 proved exit test 8. Linux x64, Windows x64, and macOS arm64 each answered `ef592d4148eb8ba0`, and the compare job agreed. This is the first live proof of G-9.
- Every check on PR #12 is green: CI on the three platforms, `bit-identity` on the three platforms and its compare job, `det-lint`, and `ste-check`. The `review-gate` check is neutral with the title "No review record", which is the advisory grey of D-181.
- The Godot 4.7.2 headless build check passes with `/Applications/Godot_mono.app/Contents/MacOS/Godot`. The name `Godot` is not on the command path.

### In flight

PR #12 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- Two numbers proved a documented plan wrong this session. Measure a numeric claim before you build on it.
- .NET never fuses a multiply and an add, so every float intermediate is the same on the three platforms. That is the whole basis of DetMath.
- The angle limit is 4096 radians. At 65536 the error reaches 9.6e-07 and the 1e-6 gate has no margin left.
- `Enum.IsDefined` reads the enum through reflection, which G-2 bans in Core. `Rng.ForStream` uses an explicit bound, and `EveryDeclaredStreamIsAccepted` guards it.
- A Roslyn scan of `System.Math.Sin` puts `Math` in the `Name` of an inner member access, not in the `Expression`. `Vector128<float>` is a `GenericNameSyntax` and never an `IdentifierNameSyntax`. Both defects passed the first build and failed the tests.
- An RngStream number is part of the seed. A new subsystem appends a value, and it never renumbers one.
- The bit-identity hash is pinned in `BitIdentityTests`. A deliberate Core change updates it, and G-20 asks the review to confirm the version bump.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #12 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-12.md`. After the merge, PR-4 starts: the logger, the error context, and the assertions.

## Session 27: 2026-09-08, Claude Code

Author: Claude Code
Session: the session end gate for the `pr-review` skill. Branch `docs/pr-review-push-gate`, PR #11.

### What this session did, and why

- The owner asked for a fix after the reviewer left its commit unpushed on the shared checkout twice on PR #10 (F-59). The skill said "push" in three places, with no verification step and no evidence trail.
- Added a "Session end gate" section to the skill. Four commands after the commit, and the evidence comes from the remote: `git status --short --branch` shows no `[ahead N]`, and `gh pr view --json headRefOid` equals `git rev-parse HEAD`.
- Added the push line as a required part of the Verification section in the review skeleton. A record with no push line is incomplete.
- Added the failure path. A denied push does not end the session. The session asks the owner to approve it and says in the handoff that the record is unpushed. A sandbox that blocks the network denies a push in silence, so the status line is the evidence and not the push output.
- Added the start-of-session check for both roles. If the checkout is ahead with the other provider's commit, push it first and record that.
- The owner also chose the rule for every session, not only reviews. D-199 records it, and revises in part D-146: the state of the build names the remote head. `CLAUDE.md` and `AGENTS.md` carry the rule in the session handoff section.
- This is the second PR of one harness invocation, after PR #10, on the owner instruction. D-121 names one PR per session.

### State of the build

- `main` is at `d5eb298`, the squash merge of PR #10. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/docs/pr-review-push-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` and `dotnet test` pass: 63 tests, 0 failures. The checker reports 0 findings.

### In flight

PR #11 changes only `docs/`, `CLAUDE.md`, `AGENTS.md`, and `.claude/skills/`, so the `review-override` label covers it (D-190). The owner asked for the label in the instruction, and this session added it.

### Traps and gotchas

- The evidence for a push is the remote, never the local checkout. A sandbox denial gives no git error.
- A revision of one part of a decision needs the `Revised in part by` marker on the old row, and the reference check does not flag it (D-186).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #11 with the `review-override` label. Then a new session starts PR-3: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 26: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `b9bc6cd` against base and merge base `94aadc5`.
- Confirmed that P2-1 to P2-4 are resolved.
- Updated `docs/reviews/pr-10.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 is ready for owner merge.

### Traps and gotchas

- The mask pass must protect every delimiter inside an earlier masked span.
- The review commit is metadata. The review head remains `b9bc6cd` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #10. Then PR-3 starts with the seeded RNG, DetMath, the lint tool, and the bit-identity job.

## Session 25: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 repeat review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the repeat review in `docs/reviews/pr-10.md`. P2-1 to P2-3 are resolved, and P2-4 is new.
- P2-4 has full merit. The quote pass masked the text inside a quote, but it left a quoted `)` as a plain character, so the parenthesis pass paired the outer `(` with it. `MaskChar` now masks both parentheses, both quote forms, and the backtick inside a span, and `Unmask` restores them. Four new assertions in `NestedParenthesesAreOneOpaqueWord` cover it.
- Updated `docs/reviews/pr-10-response.md` with the P2-4 disposition.
- The review commit `712b2e0` was on the local checkout and not on the remote, as `29b1066` was before it. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code correction too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- A mask pass must mask every delimiter inside its span, not only the sentence punctuation. A later pass reads any character that the earlier pass left plain.
- An unbalanced quote or an unclosed parenthesis stays plain text. Each token then counts on its own.
- The reviewer's commit was unpushed for the second time. Check `git status` before you start, and push it.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 at the new effective head and updates `docs/reviews/pr-10.md`.

## Session 24: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `d55666d` against base and merge base `94aadc5`.
- Confirmed that the three prior P2 findings are resolved.
- Found P2-4. A quoted close parenthesis can end an outer parenthesized span.
- Updated `docs/reviews/pr-10.md` with the new finding and the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot check did not complete because Godot could not write its macOS support file.

### In flight

PR #10 needs P2-4 corrected and a repeat review at the new effective head.

### Traps and gotchas

- The masking passes run in sequence. A later pass can read delimiters that an earlier pass already made opaque.
- `FindSpanEnd` fixes nested parentheses of one type. It does not protect against delimiters from another span type.
- The review commit is metadata. The review head remains `d55666d` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-4, add the regression tests, and request another repeat Codex review.

## Session 23: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the three P2 findings in `docs/reviews/pr-10.md` and assessed each against the evidence.
- P2-1 has partial merit. The splitter requires a space after a colon on purpose: the `ste-writing` skill holds a bare URL in prose, and a split at every colon cuts it, and cuts every time and ratio. The roadmap and the skill over-claimed with "everywhere". Both now state the space condition, and `ColonInsideAWordDoesNotEndASentence` covers it.
- P2-2 has full merit. Nested parentheses ended the span early. `FindSpanEnd` now counts depth, and `NestedParenthesesAreOneOpaqueWord` covers it.
- P2-3 has full merit. A trailing argument after `--root` was silent. The loop now rejects every argument that is not a `--root <value>` pair, with exit 2 and a message that names the argument. Three new assertions cover it.
- Wrote `docs/reviews/pr-10-response.md`.
- The review commit `29b1066` was on the local checkout and not on the remote. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code corrections too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- The reviewer commits on this checkout when both providers share it. Check `git status` for an unpushed commit before you start, and push it.
- `Sentence.Words` holds the masked tokens. Unmask a word before you compare it to text.
- Count the words of a test sentence with the splitter, not in your head. Three expectations in this PR were off by one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 per the repeat review procedure and updates `docs/reviews/pr-10.md` to the new effective head.

## Session 22: 2026-09-07, Codex

Author: Codex
Session: PR-10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Reviewed PR #10 at effective head `3ea5f23` against base and merge base `94aadc5`.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Found three P2 defects in the sentence splitter and command argument parser.
- Wrote `docs/reviews/pr-10.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 61 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 needs the three P2 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The checker only treats a colon as a sentence end when whitespace or the line end follows it. The project rule says that every colon ends a sentence.
- Nested parentheses do not remain one opaque word. The mask pairs the first opening parenthesis with the first closing parenthesis.
- `ste-check` ignores a trailing argument after a valid `--root` pair and returns the repository result.
- The review commit is metadata. The review head remains `3ea5f23` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 to P2-3, add the regression tests, and request a repeat Codex review.

## Session 21: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-2, the STE checker. Branch `feat/pr-2-ste-check`, PR #10.

### What this session did, and why

- Started PR-2 after the owner merged PR #9. Session 19 named it as the next action, and nothing blocked it.
- Wrote the `ste-check` command of `WhatYouCarry.Tools` (D-130, D-139). One reader turns a Markdown file into prose lines, one splitter turns a line into sentences and words, and one rules class holds the seven STE rules. The reference check (D-178, D-186) and the session number check (D-187) are two more classes. `RepositoryCheck` runs all three over one checkout.
- The splitter masks text in backticks, double quotes, and parentheses. Each span is one opaque word (8.5, 8.6), and no grammar rule reads inside it. A colon ends a sentence everywhere (8.4).
- Wrote 25 tests. Exit tests 1 to 9 of the roadmap each have a test, and the command test runs `Program.Main` on a throwaway checkout for the exit codes 0, 1, and 2.
- Wrote `.github/workflows/ste-check.yml` with the one job `ste-check`. It runs on Linux only, because the checks read text.
- The first run found 77 findings in 15 files: 52 passive, 15 helper verbs, 5 over 25 words, and 5 -ing forms. Two were rule gaps. A hyphenated identifier in a skill front matter matched the -ing rule, and the noun "finding" matched it after "per". Both are exclusions now, with tests. The other 75 were real, and each sentence is rewritten with the same meaning.
- Rewrote the checker section of the `ste-writing` skill. It gives the command, a table of the rule ids, the exemptions, and the Markdown conventions the checker needs.
- Marked PR-1 and PR-2 done in the design doc and the roadmap. PR-1 was still marked planned after its merge.

### State of the build

- `main` is at `94aadc5`. The branch holds one code commit, `3ea5f23`, and this entry.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 61 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. CI, `ste-check`, and `review-gate` run on it from the trunk workflows.

### In flight

PR #10 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- The passive rule is a heuristic: an auxiliary, an optional adverb, and a word that ends in "ed" or is on the irregular list. "is required" and "is closed" are findings. Rewrite with the actor as the subject.
- "have" before a participle is a complex tense, so "the docs have dated records" is a finding. Use "hold".
- The checker reads each line alone. A sentence that wraps to a second line counts as two short ones.
- The reference check skips the exempt paths. A dated record cites old decisions as history, and a rewrite to name the reviser falsifies it.
- `perl -p` reads one line at a time. A multi-line replacement needs `-0`, and an argument that starts with a hyphen needs `--` before it. Both failed silently this session before the fix.
- A `## Procedure` or `## Sequence` heading gives every numbered item below it the 20-word limit, until the next heading at any level.
- To add a technical name that ends in -ing, add it to `NotIngForms` in `SteRules.cs` with a test.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #10 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-10.md`. After the merge, PR-3 starts: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 20: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Re-reviewed PR #6 at effective head `03e6a29` against base and merge base `546a70a`.
- Confirmed P1-1 is fixed in `9624cfa`. The `pull_request_target` workflow runs the base-branch evaluator and fetches the PR head as data only.
- Confirmed P1-2 remains an accepted risk under D-198. The output names the commit that last changed the review file, and the repository cannot prove provider identity with its shared unsigned commits.
- Found no new finding. Updated `docs/reviews/pr-6.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 35 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI at head `03e6a29`.
- The review-gate check does not run on PR #6 before the workflow reaches `main`, as recorded in D-197 and F-58.

### In flight

The owner can merge PR #6. After the merge, run the adversarial proof against `main` that D-197 requires.

### Traps and gotchas

- The `pull_request_target` workflow is available only after its file reaches the default branch.
- The review-record identity risk remains accepted under D-198.
- The effective head is `03e6a29`, because the latest register changes are outside the metadata paths.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #6. Then run the adversarial proof against `main` and record its result.

## Session 19: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #6 review. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Read the two P1 findings in `docs/reviews/pr-6.md` and assessed each against the evidence.
- P1-1 has full merit. The `pull_request` event ran the tool from the PR head with `checks: write`. The owner chose `pull_request_target` (D-197). The workflow now checks out the base, fetches the PR head as data, and runs the base-branch tool. `ReviewGateRunsOnPullRequestTarget` covers it.
- P1-2 has partial merit. The rewrite of a review file reproduces, and the requested identity check cannot be built, because every commit has one identity and no signature. The owner accepted the risk under D-190 (D-198). The output now names the commit that last changed the review file. Two tests cover it.
- Opened PR #7, a throwaway adversarial PR against the PR-1 branch, to prove the trusted evaluator live. GitHub produced no run. The events reference says `pull_request_target` triggers only when the workflow file exists on the default branch (F-58). Closed PR #7 and deleted the branch.
- Wrote `docs/reviews/pr-6-response.md`, D-197, D-198, F-56 to F-58, and the roadmap corrections.
- After the owner merged PR #6 as `a3b20e2`, opened PR #8, a throwaway adversarial PR against `main`. The trusted tool from `main` answered `neutral` on the adversarial head, and the head's approval never posted. Run 34180347093. Closed PR #8 and deleted the branch. The response file records the proof.

### State of the build

- `main` is at `a3b20e2`, the squash merge of PR #6. The CI workflow and the review-gate workflow are on the trunk.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 35 tests, 0 failures. CI passed on the three platforms at `9624cfa`.
- The `review-gate` check now runs on every PR against `main` from the trunk workflow (D-197). PR #8 proved it.

### In flight

PR #6 is merged. The branch `docs/d-197-proof` records the proof in the response file and this entry. It changes documents only, so the owner adds the `review-override` label (D-190). That label run is the first live use of the override path.

### Traps and gotchas

- `pull_request_target` reads the trigger from the default branch. A workflow on a feature branch never runs for that event, and a PR against that branch gives no error, only silence (F-58).
- A `pull_request_target` run lists the base branch as its head branch in `gh run list`. Do not filter by the PR branch.
- The gate cannot see PR #6 until the merge. The owner merges PR #6 on the review record alone, as for PR #1 to PR #5.
- The identity of a commit proves nothing about the provider (D-198). Read the "Review file last changed by" line in the check output.
- The adversarial proof must never merge. Close the PR and delete the branch after the run.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner adds the `review-override` label to the proof PR and merges it. Then a new session starts PR-2, the STE checker (D-121: one session, one PR).

Entries older than the 10 newest sessions move here from `docs/session-handoff.md` (D-146). Newest first.

## Session 18: 2026-09-08, Codex

Author: Codex
Session: review PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Reviewed PR #6 at head `39db1f9` against base and merge base `546a70a`.
- Confirmed the opposite-provider gate. The PR author is Claude Code, and the reviewer is Codex.
- Found two P1 defects in the review gate. The workflow runs PR-controlled evaluator code with `checks: write`. A PR author can replace an approved review record in a metadata-only commit.
- Wrote `docs/reviews/pr-6.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 32 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI for PR #6. The custom review-gate check is neutral until the review record approves the head.
- The Godot command did not run because `Godot` is not on this checkout's command path.

### In flight

PR #6 needs both P1 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The review-gate workflow checks out the PR head and runs `WhatYouCarry.Tools` from that head.
- The effective-head rule excludes `docs/reviews/`, so it does not detect a review-file replacement.
- PR #6 has GitHub base `546a70a`, while `main` now points to `4ec9708`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P1-1 and P1-2, then request a repeat review of PR #6.

## Session 17: 2026-09-07, Claude Code

Author: Claude Code
Session: PR-1, the repository scaffold. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Started PR-1 from `/Volumes/SSD-1TB/what-you-carry` after the owner merged PR #5. Session 16 named it as the next action, and nothing blocked it.
- Created `WhatYouCarry.slnx` with the four projects (D-108, D-194), `global.json` on SDK 10.0.400 (D-173), and `Directory.Build.props` with nullable on, warnings as errors, and one language version (D-68).
- Put one directory per project at the root. `project.godot` lives in `WhatYouCarry.Game/`, next to its project file, so the Godot editor scans the Game directory and not the whole checkout. The solution stays at the root. This is a layout judgment, and not a decision.
- Wrote the `review-gate` tool as the `review-gate` command of `WhatYouCarry.Tools` (D-65). The rules are one pure function. Git reads are one class. The workflow only gathers the inputs and posts the check run.
- Wrote 32 tests. Every exit test with a name in the roadmap has a test with that name. The git-backed tests build throwaway repositories and run the real `git log` pathspec command.
- Wrote `ci.yml` with one job per platform (D-71, D-100, D-157, D-189) and `review-gate.yml` on the five event types (D-190).
- Wrote the PR template with the gate checklist, the absent checks, and the document lines (D-118, D-148).
- Ran the Godot 4.7.2 editor headless with `--build-solutions` on the Game project. It built, made no solution file, and left the project file unchanged.
- Ran the tool against this checkout with `origin/main` as the base. It found that the base has no mode file, and it gave a failure that names the file. That was OQ-72. The owner chose option (b), and this session put the one-line file on `main` in `4ec9708` on that instruction (D-196). The owner asked first whether a Codex approval plus the override label gives green. It does not: the mode rule runs first, and D-190 keeps code paths out of the override.
- Added the build and test commands to `CLAUDE.md` and `AGENTS.md` (D-122).

### State of the build

- `main` is at `4ec9708`, which holds only the mode file above `546a70a`. The branch `feat/pr-1-scaffold` holds the scaffold commit, the D-196 records, and this entry.
- `dotnet build WhatYouCarry.slnx` and `dotnet test WhatYouCarry.slnx --no-build` pass on the Mac Mini: 32 tests, 0 failures.
- PR #6 is open. The first CI run passed on all three platforms, with 32 tests on each. The `review-gate` workflow posted its check run on the head commit, with the OQ-72 failure.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #6 is open and waits for a Codex review (T-4). After D-196 the `review-gate` check on PR #6 reads `advisory` from `main` and shows grey until the review record lands. The CI workflow passed its first run on this PR on all three platforms.

### Traps and gotchas

- The Godot editor writes `TargetFramework` `net8.0` into a project file that has none, and it keeps a `.csproj.old` copy. Each project file names `net10.0` for that reason. Do not move the target framework into `Directory.Build.props`.
- The Godot SDK knows the configurations `Debug`, `ExportDebug`, and `ExportRelease`. CI builds the default `Debug`. A `--configuration Release` build of the solution is untested.
- `git cat-file -e <rev>:<path>` exits 128 for an absent path, and not 1. The tool uses `git ls-tree`, which prints nothing and exits 0 for an absent path.
- The tool reads the mode file from the base branch, so the base must hold it. D-196 put it there before PR-1 merged. A repository rebuild must keep that file on the trunk.
- One commit went to `main` without a PR on the owner instruction (D-196). That is the exception, not the rule (D-126, D-170).
- A merge of `main` into a PR branch is a commit outside the metadata set, so it moves the effective head and needs a repeat review. A rebase does the same.
- Git marks its object files read-only. The temp-repository helper clears the attribute before delete, or Windows refuses the delete.
- `dotnet run` prints the build output to stdout, so the tool writes its result to a file and never to stdout.
- The workflow uses `jq --slurp` to make one array from the paginated timeline. An empty timeline gives `[]`.
- `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are in the test project as the xUnit stack under D-66. They are the packages that `dotnet test` needs to run xUnit. No separate decision entry exists for them.
- `timeout` does not exist on macOS. `perl -e 'alarm N; exec @ARGV'` does the same job.

### Open questions that block progress

No open question blocks PR-1. OQ-72 is resolved by D-196. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #6 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-<number>.md`. After the merge, PR-2 starts: the STE checker.

## Session 16: 2026-09-07, Claude Code

Author: Claude Code
Session: the toolchain install, the runner registration, and the documentation PR override. Branches `docs/runner-path-and-doc-override` (PR #2, merged) and `docs/runner-registration` (PR #3).

### What this session did, and why

- PR #1 merged as `5c6d7b5`. The next action of session 15 is complete.
- Installed the .NET 10 SDK, version 10.0.400, and the Godot 4.7.2 .NET editor. The runbook names both tools, and this machine had neither.
- The Homebrew cask for the SDK installs a package file that needs an administrator password. This session cannot enter a password, so the Microsoft script `dotnet-install.sh` installed the SDK to `~/.dotnet`.
- Made a test solution with a class library and an xUnit project. `dotnet build` and `dotnet test` were successful, and the default target framework is `net10.0`.
- Found a risk. A launch agent starts with a minimal path, so a CI job on the self-hosted runner cannot find `dotnet`. The owner chose `actions/setup-dotnet` with the `global-json-file` input (D-189).
- The owner gave an override for a documentation-only PR (D-188). This session recorded it in the decision register, `CLAUDE.md`, and `AGENTS.md`.
- Found that D-188 could not work at launch. In enforced mode the `review-gate` job fails a PR with no review record (D-181, D-185). The owner chose the label `review-override` and a wider eligible path set, and D-190 records the mechanism. PR-1 implements it, and the roadmap now holds four more exit tests.
- The SSD came one day early. The owner formatted it as case-sensitive APFS with no encryption, and named the volume `SSD-1TB` (D-191). A probe confirmed the case sensitivity, the write access, and that the volume keeps a file mode.
- Registered the runner on 2026-09-07, one day before the date in D-171 (D-192). The name is `mac-mini-m4` and the version is 2.337.0.
- The launch agent failed at once with `Operation not permitted`, and it exited 126. macOS denies a launch agent every path on an external volume. A launchd probe repeated the denial, and a login shell read the same path correctly (F-54).
- The owner chose the Full Disk Access grant over a move to the internal drive, and added `/bin/bash` and the runner `node` binary (D-193). The runner is online and listens for jobs.
- Ran a smoke job on the runner from a throwaway branch, and then deleted the branch. The job proved five things: the runner accepts a job, the launch agent reads the external volume in a real job, `actions/checkout` works, `actions/setup-dotnet` resolves 10.0.400 from `global.json`, and a build and test cycle passes. The macOS leg of PR-1 is no longer a guess.
- The smoke job found that the .NET 10 SDK creates a `.slnx` file, and the roadmap named `WhatYouCarry.sln` (F-55). A probe proved that Godot 4.7.2 builds from a `.slnx` and creates no `.sln`. The owner chose `.slnx` (D-194).
- Moved the checkout to `/Volumes/SSD-1TB/what-you-carry` (D-195). `git fsck` reported no corruption. This completes the SSD step in the Phase 1 sequence.
- Deleted every merged branch, and pruned the stale remote-tracking refs.

### State of the build

- `main` is at `4ddf2d5`. No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.
- The .NET SDK and the Godot editor are ready on the Mac Mini.
- The runner `mac-mini-m4` is online, and it is not busy. The work directory is `/Volumes/SSD-1TB/actions-work`.
- One smoke job passed on the runner on 2026-09-07. No workflow is in the repository, because the smoke branch is deleted. PR-1 adds the first tracked workflow.

### In flight

PR #2, PR #3, and PR #4 are merged. PR #5 holds the checkout path. This session made four PRs, against the one PR rule of D-121. A direct push to `main` needed a rewind, the smoke job produced a decision, and the checkout move produced another.

This PR changes documentation only. The owner gives the D-188 override and merges it without a cross-provider review. The PR is also eligible under the D-190 path set, so the rule covers its own PR.

### Traps and gotchas

- A launch agent does not read `~/.zshrc`. Do not expect a login shell path on the runner.
- A launch agent reads no external volume without Full Disk Access (F-54, D-193). A machine rebuild repeats the grant, or every job fails.
- Every bash process on the Mac Mini now reads every file. That is the cost of the work directory on the SSD (D-193).
- `actions/setup-dotnet` installs to `~/.dotnet` on this runner, and it reported `already installed` for 10.0.400. No job downloads the SDK again.
- `dotnet new sln` gives a `.slnx` file on .NET 10. A command that names a `.sln` file fails with MSB1009 (F-55).
- The checkout is on the SSD. A session needs the volume mounted, or no file opens (D-195).
- D-190 closes the launch hole in D-188. PR-1 must build the override path, or an overridden PR turns red at launch.
- The `review-override` label does not survive a new commit. A push outside the metadata set after the label needs the label again (D-190).
- The agents hold the owner GitHub token. The label stops an accident, and it does not stop an attack (D-190).
- The `dotnet-sdk` cask needs an administrator password. The Microsoft script needs none.
- This PR holds four concerns, which is against G-10. The session named the conflict, and the owner chose one PR.
- The file held 11 entries before this session. This entry restores the limit of 10 (D-146).
- A commit went to `main` directly, against D-126. The owner merged PR #2 and moved the local checkout to `main`. The uncommitted work moved with the checkout, and the next commit and push landed on the trunk. Check the branch name before each commit.
- The owner chose a rewind. `main` returned to `2d69270`, and the work came back as PR #3. A force push to a trunk is safe only while no other clone holds the old commit.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Start PR-1 from `/Volumes/SSD-1TB/what-you-carry`. The runner, both tools, the CI chain, and the checkout move are complete, so no owner purchase or setup blocks it. PR-1 has 23 exit tests, and the roadmap holds the full scope.

## Session 15: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #1 after the P2-3 correction. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- Re-reviewed the diff through effective head `c255a17`.
- Confirmed P2-3 is fixed. The F-51 row now names `.github/review-gate-mode` and the base-branch read.
- Confirmed the sweep leaves only the historical D-181 decision row for `REVIEW_GATE_MODE`, with its revision marker.
- Updated `docs/reviews/pr-1.md` to approve the effective head.

### State of the build

- No code, solution, or CI workflow exists. PR-1 creates them.
- The effective-head check resolves to `c255a17` after excluding the D-184 metadata set.
- `AGENTS.md` and `CLAUDE.md` remain byte-identical.
- The review verdict is Ready for owner merge.

### In flight

PR #1 is ready for the owner to merge. The owner must complete the runner and SSD actions before implementation work starts (D-145, D-171).

### Traps and gotchas

- The current mode source is `.github/review-gate-mode`, not `REVIEW_GATE_MODE` (D-185).
- The review record must name the effective head, not the metadata commit (D-184).
- The review and handoff commits must be pushed to the PR branch (D-182, D-183).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #1 after confirming the review-gate and other PR gate conditions. Then run `docs/runbooks/macos-runner.md` on 2026-09-08.

## Session 14: 2026-09-07, Claude Code

Author: Claude Code
Session: answer the P2-3 finding. Branch `docs/roadmaps`, part of PR #1.

### What this session did, and why

- The review at head `e51e272` raised P2-3. It has full merit. The F-51 row in the design register still said that `REVIEW_GATE_MODE` selects the mode, and D-185 replaced that variable with the tracked file `.github/review-gate-mode`.
- The defect came from the D-185 citation pass in session 12. That pass added the `D-185` id to every line that cited `D-181` by script, and it did not read the prose beside the id. A mechanical citation edit does not make the sentence true.
- Corrected the F-51 row. It now names the tracked file and the base-branch read.
- Ran the sweep that the finding specifies across every current document. It returned two further hits in `docs/decisions.md`, and neither is a defect. The D-181 row records what D-181 said and carries its revision marker. The D-185 row named the thing it replaced, and its Effect now says "Revises the mode source in D-181", which D-186 requires.
- Used the D-187 procedure for the first time. A fetch showed session 13 on the remote, so this entry is session 14. No collision.

### State of the build

- `main` has one commit, `1c16c45`. The branch `docs/roadmaps` holds the review commits and this session's commit.
- No code, solution, or CI workflow exists. PR-1 creates them.
- `CLAUDE.md` and `AGENTS.md` are byte-identical.

### In flight

PR #1 needs a repeat review. P2-3 is corrected in documents only, because no workflow exists yet.

### Traps and gotchas

- A mechanical citation pass must read the sentence that holds the citation. The script that added `D-185` beside `D-181` left one sentence false, and the review caught it.
- The sweep for a removed name belongs with the citation pass, not after the next review.
- The D-187 procedure works. Fetch, read the highest number, then add one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews the diff through `e51e272` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

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

## Session 2: 2026-09-07, Codex

Author: Codex
Session: repository audit, no PR. The local repository has no commits.

### What this session did, and why

- Examined the design, all 143 decisions, the archive, both agent files, and both local skills.
- Wrote `docs/reviews/2026-09-07-repository-audit.md` with nine findings, evidence, recommendations, and verification limits.
- Added OQ-25 to OQ-29 in `docs/design.md` section 9. These cover gates, replay state, save transactions, empty-bank recovery, and economy tests.
- Created the local branch `docs/repository-audit` under D-126. All project files remain untracked.
- Recorded the project skill location as D-155. Both agent files now require `.claude/skills/` for current and new project skills.
- Added direct access instructions for a required skill that is absent from the skill list.
- Added no code. No commit, PR, or merge occurred.

### State of the build

- No code, solution, test project, content, CI workflow, or focused roadmap exists.
- No build or test ran. The repository has nothing executable to check.
- `AGENTS.md` and `CLAUDE.md` are byte-identical. The comparison returned success (D-122).
- The STE checker does not exist until PR-2. The new text received a manual checklist review.
- The previous design interview produced `docs/design.md` v2 and decisions D-1 to D-143.
- The unchanged v1 archive remains at `docs/archive/design-v1-2026-09-06.md`.
- A Git remote URL exists. This session did not verify remote history or backups.

### In flight

The audit and skill location update are complete. New decisions D-144 to D-154 address several audit findings.
The design and agent rules still need the corresponding audit corrections. D-147 requires those corrections before focused roadmaps.

The audit identified four highest-priority findings:

1. The initial PRs require merge checks that PR-2 and PR-3 create later.
2. Several feature gates precede their test tools or game systems.
3. The replay contract omits the initial loadout, skill state, and simulation and content identities.
4. The save contract lacks a shared commit and recovery rule across the three files.

### Traps and gotchas

- The review file records design gaps, not observed runtime defects. No game code exists.
- The five-second rewind in D-97 is an accepted tradeoff, F-17. This audit does not reopen it.
- The Godot 4.7.2 release exists. The design header incorrectly separates stable 4.7.1 from .NET 4.7.2.
- Official links and the verification date appear in the audit. OQ-2 remains open for the .NET version.
- The agent merge gate and the roadmap disagree about the initial checks. OQ-25 records the conflict.
- Section 8 puts palette approval before PR-1. OQ-1 says it blocks PR-14. The owner must settle that scope.
- Keep the attribution rule in every future commit and PR (D-137). OQ-16 remains open.
- The owner owns every open question (D-124). Ask before a choice, and record each answer (D-138).
- Project skills live in `.claude/skills/`. Read their `SKILL.md` files directly when required (D-155).
- Decisions after D-143 revise the audit state. Consult the current decision register before an audit correction.
- D-97 supersedes D-10 and D-95. D-129 and D-132 revise D-120. D-141 keeps decisions in one file until about 300 rows.
- Review ids stay local to the review file. They do not enter the design finding register.

### Open questions that block progress

`docs/design.md` section 9 still contains the audit questions. D-144 moves the register to `docs/questions.md`.
D-148 to D-154 answer several audit issues. Reconcile the register with those decisions before the next owner question.

PR-1 still depends on OQ-2, OQ-16, and the SSD prerequisite in OQ-18. D-145 records the SSD order.
The palette prerequisite has conflicting scopes, as noted above.
D-151 to D-154 resolve OQ-26 to OQ-29. The affected roadmap entries need those contracts before implementation.

### Next concrete action

Continue the audit corrections under D-147. Apply the current decisions before new owner questions.
Keep the two agent files identical (D-122). Check the current register before each new decision id.

The prior sequence remains design v2, then focused roadmaps, then code.
The next planned artifact is `docs/roadmaps/phase-1-foundations.md`.
It needs per-PR exit tests for PR-1 to PR-11 and measurements M-1 to M-2.
Load the local `ste-writing` and `design-doc-style` skills before that work.

## Session 1: 2026-09-07, Claude Code

Author: Claude Code
Session: design interview, no PR. The repository had no commits.

### What this session did, and why

- Read the v1 design document and asked the owner 143 questions in batches. The owner wanted every assumption confirmed before any code.
- Recorded every answer in `docs/decisions.md` as D-1 to D-143.
- Wrote `docs/design.md` v2 from those decisions, in the design-doc-style template and in ASD-STE100.
- Moved the v1 document to `docs/archive/design-v1-2026-09-06.md` unchanged.
- Created `.claude/skills/ste-writing/SKILL.md` and `.claude/skills/design-doc-style/SKILL.md`, adapted to this project (D-131).
- Created `CLAUDE.md` and the identical `AGENTS.md` (D-122).
- Created the empty folders `docs/reviews/` and `docs/roadmaps/`.

### State of the build

- No code. No solution. No commits. The working tree held only documents, skills, and agent files.
- The STE checker did not exist. The session checked the documents by script.

### Traps and gotchas

- The harness default adds a co-author trailer to commits. T-6 forbids it (OQ-16).
- Every open question belongs to the owner (D-124).
- D-97 supersedes D-10 and D-95. D-129 and D-132 supersede the file layout in D-120.
- The owner named the game "What You Carry" (D-11). The v1 name "Descent" appears only in the archive.
- The design doc uses `M-#` for measurements. The five milestones are the roadmap phases, not `M-#` ids.

### Next concrete action at the time

A focused roadmap for Phase 1. Superseded by session 2, the audit.

## Session 77: 2026-09-09, Codex

Author: Codex
Session: review PR-9 as PR #27 at effective head `bd1366e`. Branch `feat/pr-9-dig-plan`.

### What this session did, and why

- Verified the cross-provider gate. Session 76 identifies Claude Code as the author of the substantive PR-9 change and its correction. Codex is the eligible reviewer.
- Read the complete diff, the PR-9 roadmap entry and exit tests, the affected Core callers, content files and validators, replay and stairwell paths, D-159, D-164 to D-167, D-210, D-228, D-236, D-252 to D-257, and G-20.
- Read the automated pass claims and the author replies from the Session 76 handoff. The source-order finding has a correction and a regression test. The CI notices have answers.
- Found no actionable defect. Wrote `docs/reviews/pr-27.md` with the verdict `Ready for owner merge` at effective head `bd1366e`.

### State of the build

- `main` is at `3e5cbd3`, the squash merge of PR #26. The effective PR-27 head is `bd1366e`.
- Local build and test pass. `dotnet build` reports 0 warnings and 0 errors. `dotnet test` reports 469 tests and 0 failures.
- `det-lint` reports 0 findings in 53 Core files and 0 Game files. `ste-check` reports 0 findings in 15 files.
- `bit-identity` returns `036df5c08e2682e3`. The simulation version is 4.
- The Godot 4.7.2 headless build check passes, as recorded in Session 76.

### In flight

PR #27 needs the review record and this handoff entry committed and pushed. The owner can merge after the remote review-gate check reads `Ready for owner merge` at effective head `bd1366e`.

### Traps and gotchas

- The review record must name `bd1366e`, not this metadata commit.
- The bit-identity value changed to `036df5c08e2682e3` because the simulation now includes procgen and floor transition state. G-20 requires simulation version 4.
- The effective head stays `bd1366e` while later commits change only `docs/reviews/`, `docs/session-handoff.md`, or `docs/session-handoff-archive.md`.
- GitHub API access failed in this review context. The next session must verify the published head and review-gate result after push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

Commit the review record and handoff entry. Push the branch. Fetch and verify that the remote head has no ahead count and that the review-gate check reads the approved effective head.
