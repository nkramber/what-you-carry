# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

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

- The CI of the push, then the automated pass, then `make codex-review PR=130`.

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

## Session 327: 2026-10-03, Codex

Author: Codex
Session: PR #131, reviewer. Branch `fix/action-pins-and-launch`. PR #131, pending merge. Base `b58a68b`.

### What this session did, and why

- Reviewed all changed paths, tests, documents, PR comments, and the current CI evidence for PR #131.
- Added the review record. It records no code finding, but blocks on the owner's Windows PC launch test.
- Corrected two action-version counts in the PR description. D-768 lists five actions.

### State of the build

- The remote review metadata commit is `15e83c8`. The reviewed code head is `e8396e2`. The local full suite passed, 2,186 tests. Hosted build, test, bit-identity, bot, asset, lint, document, and smoke checks passed on the code head.
- The `night-gate` check failed on the known night records. D-771 permits this failure. On metadata tip `15e83c8`, Gitar, `asset-qa`, `det-lint`, `doc-gate`, `documents`, and `ste-check` passed. Code-only checks skipped.
- The review-gate and `evaluate` checks failed because the record verdict is `Blocked` pending the owner's Windows PC test.
- The post-push Gitar dashboard approved the head and marked the shell finding as closed.

### In flight

- The owner Windows PC launch test remains in flight.

### Traps and gotchas

- The PowerShell CI tests use fakes. They do not run the installer against WindowsApps or start a real game.
- The Deck shortcut and SSH test runs share `~/what-you-carry`. Each shortcut start checks out `main`.

### Open questions that block progress

The owner must report the Windows PC launch test before the review can approve the Windows launcher.

### Next concrete action

Run the Windows launch procedure on a Windows PC, then update the review record with that evidence.

## Session 326: 2026-10-03, Claude Code

Author: Claude Code
Session: PR #131, author. Branch `fix/action-pins-and-launch`. PR #131, pending merge. Base `b58a68b`.

### What this session did, and why

- PRs #110, #111, #113, #114, and #115 of Dependabot merged with CI red on `main` (F-206). `ActionDecisionTests` held the old pins as text, and `ci-skip` named setup-dotnet v6.0.0 while each workflow named v5.4.0.
- PR #131 brings the setup-dotnet v6.0.0 of the Dependabot PR #112 to each workflow. The test now holds the five new pins. D-768 records the versions and revises in part D-619, D-620, and D-636.
- Checked each new pin against the commit of its release tag, and read the breaking changes of each major. The night downloads by name or by pattern, so the path change of download-artifact v5 does not apply.
- The owner asked for the launch work in the same PR, with no decision entry for the second concern. `launch/what-you-carry.sh` serves the Deck, and `launch/what-you-carry.ps1` serves Windows. Each updates to `main`, builds, imports, and plays. `LauncherTests` covers both with fakes.
- Gitar does not review a PR that a bot opens. The commits moved from the Dependabot PR #112 to PR #131, and PR #112 closes (D-773).
- The README has one procedure for each machine. The Mac keeps `make play`, because the work sessions share its checkout.

### State of the build

- The remote head is the branch tip of this PR. Locally, the full suite, `det-lint`, `asset-qa`, and `ste-check` are green.
- The Deck ran the script over SSH: from the detached `34c2ddf` to `main` at `b58a68b`, the build, the import, and a play session that the test exit ended with code 0.
- The owner ran the Windows test. The install, the Godot hash, and the command on the path passed. The PC had SDK 10.0.401 alone, and the build named the cause. After the winget install of 10.0.400, the Godot step `--build-solutions` failed with no build log (F-207). The game then started with `dotnet build` alone, so both scripts now skip the Godot step.

### In flight

- The owner ran `what-you-carry -NoUpdate` on Windows at `93dea9b` from the home directory: the build, then the game on Vulkan and WASAPI. The test exit ended the session at tick 115 with no error. The Windows test of Session 327 passed.
- The Deck ran the script of `93dea9b` over SSH: the update to `main`, the build, and a play session that the test exit ended with code 0. The untracked test copy is gone, and the checkout is clean.
- The automated pass, then `make codex-review PR=131`. CI was green at `78d02b1` on PR #112 outside `night-gate`, `evaluate`, and `review-gate`. PR #131 merges over the red night gate alone (D-771).
- The merge order of D-772: PR #131, PR #130, the chore branch with the Dependabot group alone, then PR-22.

### Traps and gotchas

- Until the merge and one pull on the Deck, the command link `~/.local/bin/what-you-carry` and the desktop shortcut point to a file that does not exist.
- The play shortcut on the Deck and the SSH test runs share `~/what-you-carry`. Each start of the shortcut checks out `main`.
- The old desktop shortcut "The Thing Below" starts the repository `nkramber/the-thing-below`, not this game.
- The PowerShell tests run on the Windows leg alone. No `pwsh` is on the Mac.
- PR #130 must move D-753 and D-754 to D-769 and D-770 before its merge. The chore branch holds session 326 too, and it renumbers its entry (D-772).

### Open questions that block progress

None.

### Next concrete action

After the merge, run `git -C ~/what-you-carry pull --ff-only origin main` on the Deck over SSH, so that the shortcut finds its script.

## Session 325: 2026-10-02, Claude Code

Author: Claude Code
Session: PR-101, author. Branch `fix/fixer-session-end`. PR #130, pending merge. Base `4043785`.

### What this session did, and why

- The night 37015330351 on `main` failed at the carried seed 9153 of the full clearer. The night notice of D-642 came: the job `notify` passed. This is the first part of PR-91 exit test 24 (D-736).
- The fixer session of that night found the cause and wrote a fix, but it stopped before a commit. It started the sweep and the full suite in the background, and its reply then ended `claude -p` with exit 0. The poll sent no notice. The fixer part of PR-91 exit test 24 failed.
- The fix: no background tasks, a command limit of 6 hours, an end mark, and up to 3 resumes of the same session, then a stop notice (D-769).
- The owner leaves the night fix for the next failed night, as a test of the fixer, and merges this PR over the red night gate (D-770).

### State of the build

- Remote head: see the PR. The 23 `NightFixerTests` pass. The new test of the missing end mark fails on the poll of `main`.
- The night record of `main` reads failure, so `night-gate` stays red (D-770).

### In flight

- The CI of PR #130 waits for a fix of `main`. The five Dependabot merges #110, #111, #113, #114, and #115 at 04:39 to 04:40 UTC on 2026-10-03 left `main` red at `b58a68b`, with six failed Documents tests. The owner chose a new PR that keeps the bumps. After it merges, merge `main` into this branch, wait for green CI, and run `make codex-review PR=130`.
- Gitar approved the work head `b1a56c1` with no finding.
- The fixer draft of the night 37015330351 stays uncommitted in `~/Library/Application Support/wyc-night-fixer/work-37015330351` on the Mac Mini, on the local branch `fix/night-37015330351`. Its handoff text names a PR #130 that never opened.

### Traps and gotchas

- A branch of this kind must not start with `fix/night-`. The poll counts each open PR from such a branch as a fix PR, and it queues the night.
- The auto mode of the harness blocked an edit and a live test of the poll as an unsafe agent. The owner moved the session to the accept-edits mode.
- The live behavior of `CLAUDE_CODE_DISABLE_BACKGROUND_TASKS` and `BASH_MAX_TIMEOUT_MS` comes from the code of the CLI 2.1.283, not from a live run. The next failed night tests it.

### Open questions that block progress

None.

### Next concrete action

After the merge, read the poll log `~/Library/Logs/wyc-night-fixer.log` and the session log of the next failed night. Record PR-101 exit test 4 in the handoff entry.

## Session 324: 2026-09-28, Codex

Author: Codex
Session: PR-21, reviewer. Branch `feat/pr-21-items-tiers-affixes`. PR #129, pending merge. Base `bc41332`.

### What this session did, and why

- Reviewed the complete code, content, tests, documents, comments, and exit tests of PR #129 at effective head `240d987`.
- Found no in-scope defect. Added the review record with `Ready for owner merge` after Gitar replaced its missing-record comment with an approval notice.

### State of the build

- PR head `40cdf97` has passing three-platform CI, smoke, and bit-identity checks, plus passing content, document, night, and Gitar checks. Metadata head `7e84c41` passed Gitar, `evaluate`, and `review-gate`, plus all other required checks.
- The focused item tests passed 10 tests. Local `det-lint`, `ste-check`, and `asset-qa` each report 0 findings.
- The review record and this entry are in one metadata commit. The later metadata amendment records the final checks.

### In flight

- The owner can review the record and merge PR #129.

### Traps and gotchas

- The PR tip `40cdf97` changes documents after effective code head `240d987`.
- PR-22 owns the equipment hooks and swift clip timing. PR-26 owns enemy drops and loadouts.

### Open questions that block progress

None.

### Next concrete action

The owner can review and merge PR #129.

## Session 323: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-21, author. Branch `feat/pr-21-items-tiers-affixes`. PR #129, pending merge. Base `bc41332`.

### What this session did, and why

- Asked the owner OQ-22, OQ-51, and OQ-52, and the first item definitions that the roadmap needs (D-123). The answers are D-745 to D-752.
- Added the `item` and `affix` content types, five items, three swords of tiers 1 to 3, and three affixes: lifesteal, burning, and swift. The owner left out sturdy.
- Added `Core/Items/`: the rarities, the loot roller on the loot stream, and the affix behaviors. The player and the enemy implement `IWielder`.
- Changed exit test 6 to `AffixValidatorRejectsUnknownBehavior`, because no item file names an affix (D-752).

### State of the build

- `main` is `bc41332`. The local build, `det-lint`, `asset-qa`, and `ste-check` are clean. The full local suite passed after the fix of one content count test.
- Each code check of PR #129 is green at the work head `40cdf97`. The gitar pass approved it with no finding. The review record `docs/reviews/pr-129.md` gives `Ready for owner merge` for the effective head `240d987`.
- The loop calls no affix, so the simulation version and the bit-identity answer stay as they are.

### In flight

- The owner confirmation of the merge of PR #129 (D-524, D-533).

### Traps and gotchas

- The item files sort by path, so `ring-plain` comes before `sword-basic` in `ContentSet.Items`.
- The Overseer does not implement `IWielder`. PR-22 decides if burning reaches it, as a foe of the player.
- `AffixBehaviors.Swift` returns the weapon with a shorter `WindupTicks`. The Game layer must play the clip windup at that length (PR-22).

### Open questions that block progress

None.

### Next concrete action

After the owner confirms, run `gh pr merge 129 --auto --squash`, wait on the checks, and write the merge prompt of `one-pr-one-session`.

## Session 322: 2026-09-28, Codex

Author: Codex
Session: PR-100, reviewer. Branch `feat/pr-100-enemy-swing-clip`. PR #128, pending merge. Base `9da1817`.

### What this session did, and why

- Reviewed the complete PR-100 diff at effective head `a03c756` against the four roadmap exit tests and the affected contracts.
- Found no in-scope defect. Added `docs/reviews/pr-128.md` with the provider gate, verified comments, findings, checks, and verdict.
- Inspected the Tier 4 frame for exit test 3. The scavenger windup reads in the supplied frame (D-744).

### State of the build

- `main` is `9da1817`. CI, smoke, bit identity, asset QA, deterministic lint, STE, document gate, documents, night gate, and Gitar passed for PR head `fe46148`. The focused local tests passed 60 of 60. Local `asset-qa` and `det-lint` each report 0 findings.
- The remote PR head after the metadata push is recorded in the review file and verified with `gh pr view`.

### In flight

- Owner review and merge confirmation for PR #128.

### Traps and gotchas

- The PR checks `review-gate` and `evaluate` failed before this review because the review record did not exist. The pushed record should satisfy those checks.
- No supplied frame shows the Overseer swing because the timer-tester does not turn the camera (D-738). Exit test 3 checks the scavenger.

### Open questions that block progress

None.

### Next concrete action

Read the pushed review record and merge summary, then confirm the merge of PR #128.

## Session 321: 2026-09-28, Claude Code

Author: Claude Code
Session: PR-100, author. Branch `feat/pr-100-enemy-swing-clip`. PR #128, pending merge. Base `9da1817`.

### What this session did, and why

- Asked OQ-211. The premise of its option one held for the scavenger alone: the pick swings 30, 6, and 30 ticks, and a clip plays tick for tick. D-741: the scavenger plays `player.sword-swing.json` on its own bones, and the Overseer plays its own clip `overseer.pick-swing.json`, an overhead chop of 66 ticks.
- The check of `asset-qa` paired a clip with the model that its file names, so it never posed the player clip on the scavenger. D-742: `asset-qa` reads the family, hunter, and weapon files, and it poses each enemy model with the swing clip of its weapon.
- The first run of that check found a clip of 0.0417 meters between the right upper arm and the hood of the scavenger at tick 18. D-743: the player clip turns the right arm to [70, 0, 0] at that tick.
- `EnemyNodes.AfterTick` poses each enemy tree and the Overseer from its swing tick through `EnemyPose`. `EnemyClips` checks each clip against its weapon and its model. `Main` logs a failure of the enemy trees and quits (T-2).
- Ran the Tier 4 captures of D-714 at the PR head. Tick 3060 is the one shot with a scavenger windup in front. The owner confirmed that the windup reads (D-744), and the frames are in `docs/reviews/pr-128-frames/`.

### State of the build

- The remote head of `main` is `9da1817`. Locally at the code commit `a03c756`: the full suite passes 2158 of 2158, the Smoke category included. `asset-qa`, `det-lint`, and `ste-check` give 0 findings. The bit-identity sweep gives the known answer `1023ce079eb0af50`, and the Godot build passes.
- On PR #128 at `fe46148`, each check is green but `evaluate` and `review-gate`, which waited for the record (D-577). The automated pass approved `fe46148` with no finding. The review record gives `Ready for owner merge` for the effective head `a03c756`.

### In flight

- The owner confirmation of the merge of PR #128 (D-524, D-533), then the auto-merge.

### Traps and gotchas

- A frame shot draws one tick in 60, and the windup of the scavenger lasts 12 ticks. A scan of the full-clearer run in Core found the shot ticks of a windup in advance. The player body or a wall hid most of them.
- The timer-tester never turns the camera, so no bot frame shows the windup of the Overseer (D-738).
- A test fixture that writes a partial family or hunter file now gets a load finding of its own from `asset-qa` (D-742).

### Open questions that block progress

None.

### Next concrete action

After the merge, write the merge prompt of `one-pr-one-session`.
