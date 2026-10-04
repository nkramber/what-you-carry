# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 342: 2026-10-04, Codex

Author: Codex
Session: PR-132, reviewer. Branch `chore/action-pins-after-bumps`. PR #132, pending merge. Base `f6f3df1`.

### What this session did, and why

- Re-reviewed PR #132 at effective head `fb0bd1c` after the merge of `main` and the decision id changes.
- Verified the Dependabot groups and the fixed P2-1 regression test. Updated the existing review record.

### State of the build

- Remote head before this review commit: `fb0bd1c`.
- The focused action tests passed, 11 tests. The Documents category passed, 262 tests. `doc-gate` and `ste-check` passed.
- Hosted CI, sweeps, smoke, bit identity, bots, asset QA, deterministic lint, documents, STE, Dependabot validation, night gate, and Gitar passed at `fb0bd1c`.
- `review-gate` and `evaluate` failed before this record approved the current effective head.
- The review record and this handoff entry will be pushed together in one metadata commit.

### In flight

- Verify the post-push checks and confirm the remote head.

### Traps and gotchas

- The Gitar comment reports the old review verdict. It found no issue and ran no functional validation.
- The PR has no focused roadmap entry.

### Open questions that block progress

None.

### Next concrete action

Wait for the post-push checks with the session command. Confirm the remote head and report any new failure.

## Session 341: 2026-10-04, Claude Code

Author: Claude Code
Session: PR-132, correction author. Branch `chore/action-pins-after-bumps`. PR #132, pending merge. Base `f6f3df1`.

### What this session did, and why

- PR #132 had a merge conflict with `main` after PR #133 merged. Merged `main` at `f6f3df1` into this branch.
- PR #133 took D-774 to D-776 on `main`. This branch moved its ids to the next free ids, as D-772 orders: D-774 to D-777, D-775 to D-778, and D-776 to D-779. The text of each decision stays the same. D-636 now names D-777.
- `.github/dependabot.yml` and `ActionDecisionTests` cite D-777 and D-779 in their remarks. `docs/reviews/pr-132-response.md` records the move.
- The branch entries of Sessions 331 to 333 collided with the entries of PR #133. They are now Sessions 338 to 340, with the new ids.

### State of the build

- Locally after the merge: the build, the full suite, `ste-check`, and `det-lint` are green.
- The remark change moves the effective head, so the approval of `6da30fe` does not cover it. `review-gate` reads the new head.
- The night runs of PR #133 passed at seed 9153. The `night-gate` check of this PR passed at `02cafd1`.

### In flight

- A new cross-provider review of the merge head, after the gitar pass and green CI.

### Traps and gotchas

- Review records keep the ids of their round. `docs/reviews/pr-132.md` cites D-774 to D-776 for the decisions that are now D-777 to D-779.
- Each parallel branch takes D-# ids from the same free range. Look at `main` for the next free id before each merge.

### Open questions that block progress

None.

### Next concrete action

Run `make gitar-wait PR=132`. When each check but the Review gate workflow is green, run `make codex-review PR=132`.

## Session 340: 2026-10-03, Codex

Author: Codex
Session: PR-132, reviewer. Branch `chore/action-pins-after-bumps`. PR #132, pending merge. Base `04d17e9`.

### What this session did, and why

- Re-reviewed the correction at `6da30fe` and verified that the security group meets D-779.
- Closed P2-1 in the existing review record. The new verdict is `Ready for owner merge` for `6da30fe`.

### State of the build

- Remote head before this review commit: `6da30fe`.
- The focused security grouping test passed, 1 test. The full suite passed, 2,191 tests, with no skips.
- The Documents category passed, 262 tests, with no skips after the review record format fix.
- `ste-check` and `doc-gate` passed with no findings.
- Hosted CI, smoke, bit identity, sweeps, bots, asset QA, deterministic lint, documents, STE, Dependabot validation, and Gitar passed at `6da30fe`.
- `night-gate` failed on the carried seed 9153, as D-778 permits. `review-gate` and `evaluate` failed before this record approved the current head.
- This review record and handoff entry will be pushed in one metadata commit.

### In flight

- Verify the post-push checks and Gitar review. The owner can merge over the known night failure under D-778 when every other gate passes.

### Traps and gotchas

- The security group with pattern `"*"` can group more than one action in a PR. D-779 accepts this behavior.
- The PR has no focused roadmap entry.

### Open questions that block progress

None.

### Next concrete action

Read the post-push checks once. Report any failure beyond the night gate that D-778 allows.

## Session 339: 2026-10-03, Codex

Author: Codex
Session: PR-132, reviewer. Branch `chore/action-pins-after-bumps`. PR #132, pending merge. Base `04d17e9`.

### What this session did, and why

- Reviewed all five changed paths and the PR comments.
- Found that `group-by: dependency-name` does not group security updates across directories. D-657 enables Dependabot security updates, and D-777 states the broader one-action-per-PR rule.
- Added the review record with finding P2-1. The verdict is `Changes required` for effective head `33f81fe`.

### State of the build

- Remote head before this review commit: `15fe03a`.
- The focused grouping test passed. The local full suite aborted after 1,902 tests when the test host crashed.
- The document tests passed (261 tests). `ste-check` and `doc-gate` passed with no findings.
- Hosted CI at `33f81fe` passed outside the known `night-gate` failure and the review-dependent checks. D-778 allows that night failure for this PR.
- The post-push Gitar review approved the work head and noted that security updates remain ungrouped. The review record includes the verified concern as P2-1.
- The review record and this handoff entry will be pushed together as one metadata commit.

### In flight

- The author must answer finding P2-1 before the review can approve this head.

### Traps and gotchas

- `group-by: dependency-name` covers version updates only. The repository also enables Dependabot security updates (D-657).
- The PR has no focused roadmap entry.

### Open questions that block progress

None. The review records the scope gap as P2-1 for the author to answer.

### Next concrete action

Answer P2-1, then start the next author review round for PR #132.

## Session 338: 2026-10-03, Claude Code

Author: Claude Code
Session: Dependabot group, author. Branch `chore/action-pins-after-bumps`. PR #132, pending merge. Base `04d17e9`.

### What this session did, and why

- Merged `main` at `04d17e9` into this branch, as D-772 orders. The workflow pins and `ActionDecisionTests` of `main` stand (D-768).
- The branch dropped its pin fix and its D-755 and D-757. Its D-756 is now D-777, and its Session 326 entry is now this entry.
- The branch keeps the group `each-action` of `.github/dependabot.yml`: one PR bumps one action in every directory (D-777). `DependabotBumpsOneActionInEveryDirectoryInOnePr` reads the group.
- Round 1 of the cross-provider review (Session 339) gave `Changes required` at `33f81fe`: P2-1, security updates do not use the group. The finding holds, because `group-by` applies to version updates alone. The owner chose the group `security` with `applies-to: security-updates` (D-779). `DependabotGroupsSecurityUpdatesInEveryDirectory` reads it, and it fails on the old file. `docs/reviews/pr-132-response.md` answers the round.
- Round 2 gives `Ready for owner merge` for the effective head `6da30fe`, with P2-1 fixed.
- PR-101 exit test 4 fails. The night 37122879232 on `main` at `04d17e9` failed at 12:36 UTC. The poll started its session at 12:50 UTC as PID 20293, Claude Code 2.1.288. After 3 hours the process had 0.19 seconds of CPU time, an empty session log, and no transcript. Its main thread waited in one `openat` call. The poll runs `claude -p` with no time limit, so no end mark, no resume, and no notice came (T-2).

### State of the build

- Remote head: see the PR. Locally, the full suite, `det-lint`, `asset-qa`, and `ste-check` are green after the merge.
- The night record of `main` still reads failure at the carried seed 9153, so `night-gate` stays red. The owner chose a review now and a merge over that check alone (D-778).
- At `33f81fe`, each check passed outside `night-gate`, `evaluate`, and `review-gate`. Gitar approved `33f81fe` and `15fe03a` with no finding, and a PR comment answered its CI note with D-251. No `Gitar review` comment was necessary.
- Locally, after the P2-1 correction, the full suite passed, 2,191 tests.
- At `6da30fe`, each check passed outside `night-gate`, `evaluate`, and `review-gate`, and Gitar approved with no finding.

### In flight

- At `1602b56`, `review-gate`, `evaluate`, and Gitar passed. The owner holds the merge of PR #132 until the cause of the fixer hang is known. D-778 still permits the merge over the red `night-gate` check.
- PID 20293 still waits on the prompt. An answer of Allow lets the session of the night 37122879232 continue.

### Traps and gotchas

- The `group-by: dependency-name` key works for version updates alone, and each directory must use one ecosystem.
- `main` has no D-753 to D-767, because PR-22 holds them (D-772).
- The night 37015330351 ran on Claude Code 2.1.283. The update to 2.1.288 came at 06:47 UTC on 2026-10-03. The worktree of the fixer keeps its git directory on `/Volumes/SSD-1TB`. The owner found the cause on the Mac Mini: two macOS prompts, "2.1.288" and "claude" "would like to access files on a removable volume". macOS gives each new version of the binary a new identity, so each update asks again, and the open waits for the answer.

### Open questions that block progress

None.

### Next concrete action

A fresh session fixes the cause in its own PR: the fixer checkout moves off the removable volume, and the session start gets a time limit with a notice. After that, the owner decides the merge of PR #132.

## Session 337: 2026-10-03, Claude Code

Author: Claude Code
Session: PR-102, author, the night fixer of the night 37122879232 (D-643). Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- The hand-over of PR #133, the night fix of seed 9153. Review round 3 (Session 336) reads `Ready for owner merge` for the effective head `db623c8`. No finding is open.
- The PR holds three changes of the full clearer: the strike before the walk out (D-774), the drop of an enemy after ten seconds of strikes that hurt nothing (D-776), and the count that reads the sum of the health of the enemies. Seed 9153 joins the extra fixed seeds (D-567). The roadmap id is PR-102 (D-775).
- Sessions 331, 333, and 335 hold the rounds of this session. The response file `docs/reviews/pr-133-response.md` answers P2-1 and P2-2.
- The notice "ready to merge" goes to the owner after this entry, with the PR link and the session answers D-774, D-775, and D-776 (D-644).

### State of the build

- Remote head: see the PR. The effective head is `db623c8`, and this entry is a metadata commit after the review record `c46901f`.
- The branch nights 37140330448, 37143136198, 37144818723, and 37147689444 passed at `b384bae`, `5e26c7f`, `86d9b99`, and `db623c8`, each with seed 9153 in the list. The record `night-branch/fix/night-37122879232` reads success at `db623c8`.
- Every check of `db623c8` passed but `evaluate` and `review-gate`, which waited for the approving record. The local suite passed 2192 tests, and the three local sweeps of 6000 full-clearer seeds read 0 softlocks and 0 crashes.

### In flight

- The owner merge of PR #133 (D-524). The fixer never merges.

### Traps and gotchas

- The merge needs the ruleset bypass for no check: the night gate of this PR is green by its branch night (D-538). After the merge, the promotion of D-557 makes that night the record of `main`, because each commit after `db623c8` is in the skip set.
- The three decisions of this PR are session answers under D-644. The owner confirms them at the merge, or changes them with a new D-#.

### Open questions that block progress

None. D-774, D-775, and D-776 wait for the confirmation of the owner under D-644.

### Next concrete action

The owner reads the merge summary in the PR description, confirms D-774 to D-776, and merges PR #133. The next night on `main` then runs seed 9153 from the extra seeds.

## Session 336: 2026-10-03, Codex

Author: Codex
Session: PR-133, reviewer, round 3. Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- Reviewed the correction of P2-2. `FullClearerCountsAHitOnAnotherEnemy` passes on `db623c8`, and P2-2 is fixed.
- Rechecked the resolved Gitar thread. The health sum answers the target-swap claim, and the floor-wide count matches D-776.
- The isolated pre-fix test run did not compile because the historical worktree lacked Godot references. The author reports that the new test fails on the old count before tick 1000.
- Updated the review record to approve the effective head `db623c8`. D-774, D-775, and D-776 remain session answers for the owner to confirm at merge.

### State of the build

- Remote head at close: the metadata commit that carries this entry and the review record. Effective head: `db623c8561eb2a9648fb33b6fa47cb810809099e`.
- The focused regression passed 1 test. Current-head CI passed Linux, Windows, macOS, the sweeps, smoke, bit identity, bots, asset QA, lint, documents, doc gate, STE, Gitar, and night gate.
- `evaluate` and `review-gate` passed after this approving review record reached the PR branch. Heavy jobs skipped the metadata commit.

### In flight

- PR #133 awaits owner merge. The review record and this entry are published in one metadata commit.
- The owner confirms D-774, D-775, and D-776 at merge under D-644.

### Traps and gotchas

- The old isolated worktree cannot compile the test project without Godot references. Do not report that attempt as a test failure.
- The review applies to effective head `db623c8`; the metadata commit does not change it.

### Open questions that block progress

None. The owner confirms D-774, D-775, and D-776 at merge under D-644.

### Next concrete action

The owner reads the review record, confirms the session answers, and decides whether to merge PR #133.

## Session 335: 2026-10-03, Claude Code

Author: Claude Code
Session: PR-102, correction author, the night fixer of the night 37122879232 (D-643). Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- Answered review round 2 of PR #133 (Session 334). P2-2 has full merit: the change of the stall count to the health sum had no test that failed on the count that read the selected enemy alone (T-3).
- `FullClearerCountsAHitOnAnotherEnemy` plays floor 1 of seed 6 with harmless weapons, and every 120 ticks it takes one point of health off the living enemy farthest from the body. The policy drops no enemy. On the count of `5e26c7f` it dropped enemy 3 at tick 744.
- A probe of 80 seeds with weapons of one damage point read no drop at either count, so natural play tells the two counts apart on no seed. The hit through `Enemy.TakeHit` stands in for the hit on the other enemy of a pair.
- The response file holds P2-2. The same session wrote rounds 0 and 1 (Sessions 331 and 333).
- The gitar pass of `86d9b99` approved, and gitar confirmed the health sum in its thread, which is resolved. The branch night 37144818723 passed at `86d9b99` with seed 9153 in the list.

### State of the build

- Remote head: see the PR. Local: the full suite passed 2192 tests, and `det-lint`, `asset-qa`, and `ste-check` report 0 findings. Core did not change in this round, so the sweep of Session 333 stands.
- The test commit moves the effective head, so the branch night runs again at the new head (D-547).

### In flight

- The gitar pass of the test commit, the fourth branch night, and review round 3 (D-643).

### Traps and gotchas

- A regression test of the count needs a hit that the strike does not select. `Enemy.TakeHit` is public, and the test calls it on the farthest living enemy between two ticks of the loop.
- The policy drops an enemy at tick 744 of seed 6 with harmless weapons when no hit lands, so a test of the count needs a hit at least every 600 strike ticks.

### Open questions that block progress

None. D-774, D-775, and D-776 wait for the confirmation of the owner under D-644.

### Next concrete action

Review round 3 reads the test. Then the owner confirms D-774 to D-776, reads the review record, and merges PR #133.

## Session 334: 2026-10-03, Codex

Author: Codex
Session: PR-133, reviewer, round 2. Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- Reviewed the correction at effective head `86d9b99` and verified the claim on the resolved Gitar thread.
- Confirmed that the health sum detects a hit on either enemy when two enemies trade the nearest place.
- Found P2-2: no regression test proves that a hit on another target resets the strike count.
- Updated `docs/reviews/pr-133.md`. P2-1 is fixed in `5e26c7f`. The current verdict is `Changes required` for P2-2.

### State of the build

- Remote code head before this metadata push: `86d9b9978ccf204b728f672447ae4153cbef8c32`.
- Current-head CI passes for build, tests, sweeps, smoke, bit identity, bots, asset QA, lint, documents, doc gate, STE, Gitar, and night gate. `evaluate` and `review-gate` fail because no approving record covers the open finding.
- The focused full-clearer tests pass 6 tests at the current head and at `5e26c7f`. The suite therefore does not prove the health-sum correction.
- The review record and this entry are one metadata commit, pushed to `origin/fix/night-37122879232` and verified with `gh pr view`.

### In flight

- The author adds a test for a hit on an enemy other than the nearest target, then requests a new review round.
- The owner confirms D-774, D-775, and D-776 at the merge under D-644.

### Traps and gotchas

- The harmless-weapon test cannot cover a hit on another enemy. A regression test must fail at `5e26c7f`.
- The floor-wide count across encounters follows D-776. The Gitar comment's target-swap concern is fixed by the health sum.

### Open questions that block progress

P2-2 blocks approval until the author adds the regression test. No open OQ applies.

### Next concrete action

The author adds the test and requests a new review round on the new effective head.

## Session 333: 2026-10-03, Claude Code

Author: Claude Code
Session: PR-102, correction author, the night fixer of the night 37122879232 (D-643). Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- Answered review round 1 of PR #133 (Session 332). P2-1 has full merit: an enemy inside the reach that no swing hurts held the strike with no stall drop, in the hunt before this PR and in the walk out after it.
- The correction: a strike that takes no health off an enemy for ten seconds drops that enemy from the strike and the hunt (D-776, F-209). The count runs across the enemies in reach, because the nearest one changes from tick to tick when two stand near, and a count for each enemy alone never ended.
- `FullClearerDropsAnEnemyThatTheStrikesDoNotHurt` plays seed 6 with harmless weapons to the bottom. On the policy with no stall drop it softlocks on floor 1 at tick 10800.
- The response file is `docs/reviews/pr-133-response.md`. The same session wrote the fix of round 0 (Session 331).
- The gitar pass of the first head `b384bae` approved with no finding and no thread. The branch night 37140330448 passed at that head with seed 9153 in the list.
- The gitar pass of the correction `5e26c7f` approved with one finding, which has merit: a hit on one enemy while the count read the other one never ended the count. The count now reads the sum of the health of the enemies, which any hit that lands lowers, and the commit that carries this entry answers the thread. The branch night 37143136198 passed at `5e26c7f` with seed 9153 in the list.

### State of the build

- Remote head: see the PR. Local: the full suite passed 2191 tests, and `det-lint`, `asset-qa`, and `ste-check` report 0 findings.
- The local sweep of the full clearer over seeds 1 to 5000 and 9001 to 10000 on the health sum rule: 1703 bottoms, 4297 deaths, 0 softlocks, 0 crashes, the same counts as before the correction, in 8 min 48 s.
- The correction moves the effective head, so the branch night runs again at the new head (D-547).

### In flight

- The gitar pass of the health sum commit, the third branch night, and review round 2 (D-643).

### Traps and gotchas

- A content set with harmless weapons makes every enemy one that no swing hurts, and the enemies hurt nobody. Floor 1 of seed 1 then holds more such enemies than ten seconds each fit into the timer, so seed 1 softlocks also with the drop. Seed 6 reaches the bottom.
- The band case of P2-1 needs an enemy 1.2 meters or more above or below the feet inside 1.6 meters, so a hand-built floor is hard. Harmless weapons reproduce the hold with no geometry.

### Open questions that block progress

None. D-774, D-775, and D-776 wait for the confirmation of the owner under D-644.

### Next concrete action

Review round 2 reads the correction. Then the owner confirms D-774 to D-776, reads the review record, and merges PR #133.
