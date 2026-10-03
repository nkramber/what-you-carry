# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

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

## Session 332: 2026-10-03, Codex

Author: Codex
Session: PR-133, reviewer. Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- Reviewed the complete PR-102 diff at effective head `5cb1b0c`, the roadmap tests, the behavior contract, and the Gitar comment.
- The provider gate passed. The review found P2-1: a nearby enemy that the blade cannot hit can hold the clearer in the leave branch without the stall recovery.
- Added `docs/reviews/pr-133.md` with the finding and a `Blocked` verdict. D-774 and D-775 still await owner confirmation under D-644.

### State of the build

- Remote implementation head: `5cb1b0c`. The full local suite passed 2190 tests. The focused regression passed within that run.
- CI, smoke, bit identity, bots, asset QA, deterministic lint, documents, doc gate, STE, Gitar, and the night gate passed. `evaluate` and `review-gate` failed because the review record did not exist yet.
- The review record and this entry are the metadata commit. The PR head after the push is verified with `gh pr view`.

### In flight

- The author must address P2-1. The owner must confirm D-774 and D-775. The review gate must read the pushed record.

### Traps and gotchas

- `NearestLiving` uses feet distance and weapon reach. Blade hits also require vertical overlap. The leave branch strikes before it walks and skips the hunt stall check.
- The seed 9153 regression passes, but it does not cover an enemy inside scalar reach and outside the blade's vertical band.

### Open questions that block progress

None. D-774 and D-775 await the owner under D-644.

### Next concrete action

The author addresses P2-1 and asks the owner to confirm D-774 and D-775. Then a new review checks the fix and the current PR head.

## Session 331: 2026-10-03, Claude Code

Author: Claude Code
Session: PR-102, author, the night fixer of the night 37122879232 (D-643). Branch `fix/night-37122879232`. PR #133, pending merge. Base `04d17e9`.

### What this session did, and why

- The night 37122879232 on `main` failed at the carried seed 9153 of the full clearer, a softlock on floor 4 at the expiry (F-208). No job ended with a runner fault, so no re-run (D-585). The night 37015330351 of 2026-10-02 first read this seed, and D-770 left the fix to this night.
- The trace of floor 4: the policy left at tick 9300 with a scavenger of six health in reach, and the scavenger held the doorway of one cell to the stairwell. The policy never swung while it left. Each swing of the scavenger rolled the body two meters back from the doorway, and the walk back took the ticks until the next swing. The run softlocked at tick 18048.
- The fix: the strike of an enemy in reach comes before the walk out (D-774). Seed 9153 now kills the scavenger, leaves floor 4 near tick 9600, and dies on floor 5. Seed 9153 joins the extra fixed seeds (D-567). The roadmap id is PR-102 (D-775).
- `FullClearerStrikesTheEnemyInReachWhileItLeaves` fails on the old policy: softlock on floor 4 after 18048 ticks. The two `night-seeds` command tests read an extra seeds file of their own in a temporary root, because the seed of D-567 broke their expected lists.
- The fixer session of the night 37015330351 wrote the same fix in its worktree and never committed it (Session 325). This session verified the cause again on `04d17e9` with a trace, and took the fix with new ids.
- No bisect names a commit: the seed first ran on 2026-10-02 as a slice seed, and it softlocks before PR-21 too. The leave rule of D-439 never struck while it left, since PR #85.
- D-774 and D-775 are session answers under D-644, for the owner to confirm.

### State of the build

- Remote head: see the PR. Local: the full suite passed 2190 tests, and `det-lint`, `asset-qa`, and `ste-check` report 0 findings.
- The local sweep of the full clearer over seeds 1 to 5000 and 9001 to 10000: 1703 bottoms, 4297 deaths, 0 softlocks, 0 crashes, in 8 min 54 s.
- The simulation version stays at 19. PR #85 changed the same policy with no raise, and the bit-identity sweep plays the greedy descender and the coward.

### In flight

- The gitar pass, the branch night, and the Codex review, in the order of the night fixer runbook (D-643).

### Traps and gotchas

- A command test of `night-seeds` that reads the extra seeds of the checkout breaks on each seed of D-567. Give such a test a temporary root.
- The strike in reach has no stall check. An enemy inside 1.6 meters that the blade cannot hit holds the policy, in the hunt before this PR and in the walk out after it. The sweep of 6000 seeds read none.
- The worktree of the night 37015330351 still holds its uncommitted draft, with the stale ids D-753, D-754, and PR-101.

### Open questions that block progress

None. D-774 and D-775 wait for the confirmation of the owner under D-644.

### Next concrete action

The owner confirms D-774 and D-775, reads the review record, and merges PR #133. The promoted record of the branch night then makes the night gate green (D-557), and the next night on `main` runs seed 9153 from the extra seeds.

## Session 330: 2026-10-03, Codex

Author: Codex
Session: PR-101, reviewer. Branch `fix/fixer-session-end`. PR #130, pending merge. Base `0034e16`.

### What this session did, and why

- Reviewed the night-fixer change at effective head `c112c23`, its tests, documents, and PR comments.
- Added the review record. The provider gate passed, and the review found no defect in scope.

### State of the build

- The remote head at review start was `ad8c67d`. The review record and this entry are one metadata commit, verified with `gh pr view`. The effective head remains `c112c23`.
- The focused test passed, 23 tests. Hosted CI, smoke, bit identity, bots, asset, lint, documents, doc-gate, STE, and Gitar passed. The night gate failed as D-770 permits.

### In flight

- The owner can merge PR #130 after the metadata checks pass. Exit test 4 runs after merge.

### Traps and gotchas

- The known night-gate failure is the only accepted failed check under D-770. Do not read it as a failed product check.

### Open questions that block progress

None.

### Next concrete action

The owner can merge PR #130 under D-770. After the next failed night on `main`, record the result of exit test 4.

## Session 329: 2026-10-03, Claude Code

Author: Claude Code
Session: PR-101, author. Branch `fix/fixer-session-end`. PR #130, pending merge. Base `0034e16`.

### What this session did, and why

- CI on `main` at `0034e16` passed: CI, smoke, bit identity, bots, `det-lint`, `asset-qa`, `ste-check`, and night promotion.
- The Deck pulled `main` to `0034e16` over SSH (D-606). The command link `~/.local/bin/what-you-carry` now finds `launch/what-you-carry.sh`.
- Merged `main` into this branch. D-753 and D-754 of this branch moved to D-769 and D-770 in each file, because PR-22 holds D-753 to D-767 (D-772).
- D-771 cited D-754 as the same rule for PR #130. That citation now names D-770. D-772 and Session 326 keep the old ids, because they name the move.
- Session 325 went back in its place under Session 326. `handoff-rotate` moved Sessions 318 and 319 to the archive.

### State of the build

- Remote head: see the PR. Locally, the full suite passed, 2,189 tests, after the rotation. `det-lint`, `asset-qa`, and `ste-check` each report 0 findings.
- The night record of `main` reads failure, so `night-gate` stays red, and the owner merges over it (D-770).

### In flight

- At `ad8c67d`, each check passed outside `night-gate`, `evaluate`, and `review-gate`. Gitar approved with no finding, and a PR comment answered its CI note with D-251 and D-770.
- The cross-provider review of Session 330 gives `Ready for owner merge` for the effective head `c112c23`.
- The owner merges PR #130 with the ruleset bypass, over the red `night-gate` check alone (D-770).

### Traps and gotchas

- D-768 to D-773 are now in id order, but D-769 and D-770 carry the date 2026-10-02 under D-768 of 2026-10-03.
- The chore branch `chore/action-pins-after-bumps` holds Session 326 too, and it renumbers its entry (D-772).

### Open questions that block progress

None.

### Next concrete action

After the merge, read the poll log `~/Library/Logs/wyc-night-fixer.log` and the session log of the next failed night. Record PR-101 exit test 4 in the handoff entry.

## Session 328: 2026-10-03, Codex

Author: Codex
Session: PR #131, reviewer. Branch `fix/action-pins-and-launch`. PR #131, pending merge. Base `b58a68b`.

### What this session did, and why

- Re-reviewed PR #131 at effective code head `93dea9b` after the Windows and Deck launch reports arrived.
- Verified the closed shell finding, reviewed the new build path, and updated the existing review record to `Ready for owner merge`.

### State of the build

- The effective code head was `93dea9b`. The review and handoff were pushed in metadata commit `f8d3217`; commit `42cf24c` records the post-push check results. The current remote head was verified as `42cf24c`.
- Launcher tests passed, 15 tests. STE check passed with 0 findings.
- Hosted build and test, bit identity, bots, asset, lint, document, and smoke checks passed at the code head. After publication, Gitar, `review-gate`, `evaluate`, asset, lint, document, and STE checks passed. Metadata-tip code checks skipped. `night-gate` failed as allowed by D-771.

### In flight

- The review record and handoff are published. The final check update is also published.

### Traps and gotchas

- Hosted PowerShell tests use fakes. The owner reports a real Windows launch and a Deck launch at `93dea9b`.
- The known `night-gate` failure remains allowed by D-771.

### Open questions that block progress

None.

### Next concrete action

The review is ready for the owner merge decision; D-771 allows the known `night-gate` failure.
