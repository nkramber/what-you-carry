# Session handoff

Rule (D-146, D-377, D-379): this file keeps the 10 newest sessions, newest first. Read the newest entry, and the newest entry that names your branch. At the end of a session, add a new entry at the top, then run `handoff-rotate`. It moves each entry beyond the tenth to the top of `docs/session-handoff-archive.md`.

## Session 236: 2026-09-23, Codex

Author: Codex
Session: PR-98, reviewer. Branch `chore/pr-82-template-gitar-notice`. PR #98, blocked. Base `2071cb6`.

### What this session did, and why

- Reviewed PR #98 at effective head `bc51dde`. The template line and its equality test meet the D-550 contract.
- Added `docs/reviews/pr-98.md` with the review evidence and verdict.
- The review found no in-scope defect. Required CI evidence blocks approval.

### State of the build

- The focused template test passed, and the Documents tests passed 141 of 141. `ste-check` and local `doc-gate` passed.
- GitHub at `d158af7` had a failed `night-gate`. The base night record failed, and the branch-night record was absent.
- GitHub showed Linux smoke as failed, but its run was still in progress and its failure log was unavailable. Required platform jobs, sweeps, bots, and macOS smoke remained pending.
- The remote head at hand-over is `d158af7`. Later PR commits change documents only, so the effective head remains `bc51dde`.

### In flight

- PR #98 remains blocked until the required night and CI evidence passes.

### Traps and gotchas

- The Gitar comment is a free-plan notice with no claim. D-550 says to ignore it.
- The review record and this entry form one metadata commit. The commit does not change the effective head.

### Open questions that block progress

None.

### Next concrete action

Resolve the night-gate and remaining CI results, then request a fresh review if the effective head changes.

## Session 235: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-82, author. Branch `chore/pr-82-template-gitar-notice`. PR #98, pending merge. Base `2071cb6`.

### What this session did, and why

- Ran a night by hand on `main` at `2071cb6`, run 35944586534. The record of `main` was the red night of `e069e16`. The run was in progress at the hand-over. A green night there turns `night-gate` green for this PR.
- The owner confirmed the roadmap id PR-82 (D-553) and a test of the template line (D-554).
- The gitar line of the PR gate in `.github/pull_request_template.md` now reads as the line in the agent files: "every gitar comment with an item has its answer" (D-550, D-551).
- `PullRequestTemplateGitarLineMatchesThePrGate` holds the two lines equal. It failed on the old template.
- The Phase 2 roadmap and `docs/design.md` hold the PR-82 entry.

### State of the build

- Local: `ste-check` 0, `det-lint` 0. The full suite passed 1586 of 1586.
- Code head: `bc51dde`. Later commits of this PR change documents only.

### In flight

- The PR checks, and `make codex-review PR=98 -- --skip-gitar-review` (D-543).

### Traps and gotchas

- The template lies outside the skip set of D-475, so this PR runs the full suite and needs a Codex review, and not the override label.
- The handoff of Session 232 quoted the new line with "(D-550)" at the end. The agent files have no such citation, and the test asks for equal lines, so the template has none too.

### Open questions that block progress

None.

### Next concrete action

This session: the review round, then the merge request with the summary in questions and answers (D-552).

The session after the merge takes the owner focus of 2026-09-23: the fixed seeds of the night (D-551). Follow the next concrete action of Session 232. File the next OQ-# with the options and a recommendation, and ask the owner for the roadmap id.

## Session 234: 2026-09-23, Codex

Author: Codex
Session: PR-81, reviewer. Branch `fix/pr-81-night-softlocks`. PR #97, Ready for owner merge at effective head `4282206`.

### What this session did, and why

- Re-reviewed PR #97 after the sweep correction and after the branch night finished.
- Found no defect. Updated the existing review record and preserved its earlier blocked verdict.
- The remote code head is `4282206`. Later PR commits change documents only.

### State of the build

- The focused sweep regression test passed locally.
- CI, smoke, bit identity, bots, asset QA, lint, STE, and doc-gate passed at code head `4282206`.
- The branch night passed at `4282206`; the fresh `night-gate` passed at PR tip `87e4712`.
- The prior review-gate run read the old blocked verdict. The metadata push must start a fresh review-gate run.

### In flight

- The fresh review-gate run after this metadata commit.

### Traps and gotchas

- Later commits after `4282206` change documents only, so the effective code head remains `4282206`.
- The only gitar comment says it is working. D-550 says that notice needs no answer.

### Open questions that block progress

None.

### Next concrete action

Check the fresh review-gate result after this metadata push.

## Session 233: 2026-09-23, Codex

Author: Codex
Session: PR-81, reviewer. Branch `fix/pr-81-night-softlocks`. PR #97, Blocked at effective head `ba7f448`.

### What this session did, and why

- Reviewed PR #97 as the opposite provider and found no code defect.
- Recorded that the required branch night and its `night-gate` result remain incomplete.
- The remote PR head after publication is recorded by the review-gate check; the effective code head remains `ba7f448`.

### State of the build

- Build and 12 focused regression and gate tests passed locally.
- CI, smoke, bit identity, bot, lint, asset QA, and document checks passed at effective head `ba7f448`.
- Branch night run 35909827024 was in progress at hand-over. The current `night-gate` failed because its branch record did not yet exist.
- The review record and this handoff entry were pushed together as one metadata commit. The remote PR head was checked with `gh pr view`.

### In flight

- Branch night run 35909827024 and its re-run of `night-gate`.
- The fresh `review-gate` run after publication of the review record.

### Traps and gotchas

- The branch night tests the effective head `ba7f448`. Later document commits do not change the code it tested (D-534, D-547).
- Gitar's only comment says it is working. The exported comments contain no feedback or review thread.

### Open questions that block progress

None. Required night evidence is incomplete.

### Next concrete action

Check run 35909827024 and the new `night-gate` result. Re-review the same PR after the branch night passes, or record any night failure.

## Session 232: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-81, author. Branch `fix/pr-81-night-softlocks`. PR #97, pending merge. Base `582f346`.

### What this session did, and why

- Reproduced the 26 greedy-descender softlocks of the night at `e069e16`. They pass at `170f08c` and `ea84473`, and they softlock at `837902b`, so PR-72 made them (F-111).
- Found two faults with a trace of each seed. The wedge count of `PathFollower` read a wedge on a detour of a diagonal path (24 seeds). `DiagonalMove` took a drop under an overhang as one diagonal drop (seeds 2669 and 2879).
- The owner chose both fixes (D-545, D-546), the effective head for a branch night (D-547), and a re-run of the gate by the night (D-548).
- Built the branch nights of D-538: the record on `night-branch/<branch>`, the gate read of it, and the re-run step.
- The local night then crashed seed 4119 of the greedy descender (F-112). The sweep box and the body box ended one ulp apart, and the body box overlapped a block. The owner chose the exact fix: the sweep builds each box in the form of the caller (D-549).
- The first review round at `ba7f448` found no code issue. It read `Blocked` for the branch night that had not ended.
- The owner asked that a gitar notice, a comment with no specific item, get no answer and block no verdict (D-550). The skills and the agent files take the rule in PR-81. The PR template takes it in the next PR (D-551).

### State of the build

- Local: `det-lint` 0, `ste-check` 0. The suite at `ba7f448` passed 1582 of 1583, and the version pin of `SimulationTests` was the one failure.
- The local night of `ba7f448`: no softlock in any policy, and one crash, seed 4119. The full clearer read no fault over 3863 seeds before the stop.
- The branch night run 35909827024 at `ba7f448` was cancelled for the crash.

### In flight

- The sweep fix of D-549, its full suite, its CI, a new branch night, and review round 2.

### Traps and gotchas

- The self-hosted runner is this Mac. A local night slows the Mac CI legs, and a rebuild of the checkout during `bot-run --no-build` breaks the run. Run a local night from its own worktree.
- `SweptAabb.Sweep` of a plain box and `SweptAabb.SweepFeet` of a body share one frame. A caller that builds its box in another form can meet F-112 again.
- A trace of one seed needs the follower of the policy, which is private. A scratch test with reflection read it, and the scratch test is not in the PR.
- The seeds of the greedy descender: 751 764 940 947 1087 1268 1456 1566 1597 1785 1986 2064 2091 2412 2523 2669 2879 3052 3298 3347 3589 3757 3881 3994 4014 4870.

### Open questions that block progress

None.

### Next concrete action

This session: wait on branch night run 35915259159 and its re-run of `night-gate`, then run review round 2.

The next session, after PR #97 merges, makes one PR alone (D-551). The gitar line of the PR gate in `.github/pull_request_template.md` takes the rule of D-550: "every gitar comment with an item has its answer (D-550)". PR-81 changed the same line in `CLAUDE.md` and `AGENTS.md`. Ask the owner for its roadmap id.

The session after it takes the owner focus of 2026-09-23: the fixed seeds of the night (D-551).

- The night runs seeds 1 to 5000 for each bot policy and seeds 1 to 100000 for the seed sweep, every night (D-115, D-116).
- A fixed set gives a clean before and after, a replay of each failure, and a stable gate. The PR-81 bisect used all three.
- A fixed set also never tests a floor past its range, and a fix can pass the known seeds alone.
- The option to put to the owner: keep the fixed set as the gate, and add a rotating slice each night, such as a window from the date. The run log names the window, so each failure replays.
- The owner decides whether a failure in the slice blocks the merge, or files a finding that adds the seed to the fixed set.

First action of that session: file the next OQ-# in `docs/questions.md` with these options and a recommendation, and ask the owner. Ask the owner for the roadmap id of the PR too. Then record each answer as a D-#.

## Session 231: 2026-09-23, Codex

Author: Codex
Session: PR-80, reviewer. Branch `chore/pr-80-gitar-pause`. PR #96, pending merge. Base `25afe34`.

### What this session did, and why

- Reviewed PR #96 at effective head `f331068` under D-542 to D-544.
- The review found no issue in the Gitar pause, the flag path, the thread check, or the supporting documents.
- Added `docs/reviews/pr-96.md` with the verdict `Ready for owner merge`.

### State of the build

- `dotnet test WhatYouCarry.slnx` passed 1573 of 1573 tests at `f331068`.
- The remote head of PR #96 was `f331068`. The required CI checks passed, except `night-gate`, which failed on `e069e16` under D-544. The review-gate failure came from the missing review record.
- The review record and this handoff entry are published to `origin/chore/pr-80-gitar-pause`.

### In flight

- PR #96 awaits the owner merge process. The owner uses the merge summary of D-533 and the bypass of D-544 if `night-gate` remains red.

### Traps and gotchas

- `make -n codex-review PR=96 -- --skip-gitar-review` confirms that Make passes the flag after the required options.
- Gitar posted a free-plan notice and no review feedback. The review export found no open review thread.

### Open questions that block progress

None.

### Next concrete action

The owner checks the merge conditions for PR #96 and follows D-533.

## Session 230: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-80, author. Branch `chore/pr-80-gitar-pause`. PR #96, pending merge. Base `25afe34`.

### What this session did, and why

- The owner paused the gitar requirement until a later PR of the owner (D-542). The author answers each gitar comment that comes in. A gitar review with feedback stops the session, and the session alerts the owner.
- Added the flag `--skip-gitar-review` to `codex-review`, through `make codex-review PR=<n> -- --skip-gitar-review` (D-543). The flag drops the Gitar check run and the Gitar dashboard checks. The thread check stays (D-522). The flag stays after the pause.
- Each text of the pause outside the registers names D-542, so `grep -rn 'D-542'` finds each one for the PR that ends the pause.
- `AGENTS.md` was 19 bytes under its ceiling. The Game argument rules and the generated file rules moved to `docs/runbooks/commands.md` (D-382).
- The owner put this PR before the night fix of D-538, with a bypass merge when `night-gate` stays red (D-544).

### State of the build

- The full suite passed 1573 of 1573 locally. `ste-check` gave no finding.
- The remote head of `main` is `25afe34`. The newest night failed at `e069e16` (D-538), so `night-gate` stays red.

### In flight

- `make codex-review PR=96 -- --skip-gitar-review` approved the effective head `f331068` with no finding (session 231).
- Exit test 4 of PR-79 and of PR-80: the runbook commit after the approval is a documents commit. `review-gate` must stay green on it.
- The Gitar dashboard gave "Gitar is working" at 18:26 UTC, with no thread. Read the PR comments again before the merge summary.

### Traps and gotchas

- Make reads each word after `--` as a goal. The `--%` rule of the `Makefile` keeps make from a stop, and the target passes each such goal to the command.
- `AGENTS.md` holds 14842 bytes of 15000. Move detail to a runbook before a new rule.

### Open questions that block progress

None.

### Next concrete action

Confirm `review-gate` on the new head, read the PR comments, and give the owner the merge summary of D-533.

## Session 229: 2026-09-23, Codex

Author: Codex
Session: PR-79, reviewer. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Re-reviewed PR #95 at effective head `25ad21e`.
- Verified that P2-1 is fixed: the override label and effective-head logic now use the same document path set. No new code finding remains.
- Updated the existing review record with the finding history and current verdict.

### State of the build

- The focused review tests passed 119 of 119 at `25ad21e`.
- The Documents category passed 140 of 140 after the review record and handoff edits.
- `ste-check` passed with no findings, and `doc-gate` passed with 0 problems over 25 paths.
- Asset QA, lint, document, STE, bot, the Linux and macOS CI platform results, Smoke, and bit-identity passed at `25ad21e`.
- The Windows CI job later passed at 17:42 UTC. The metadata-only run skipped heavy jobs under D-474 rule 2 after the previous code-head CI run passed.
- The night gate failed on the D-538 record. The review gate and evaluator fail on the published `Blocked` verdict.

### In flight

- The review record and this handoff entry are published on `origin/chore/pr-79-review-process`.
- PR #95 remains blocked by the D-538 night-gate failure. The review gate and evaluator fail on the `Blocked` verdict; metadata-only jobs pass or skip under D-474 rule 2.

### Traps and gotchas

- The code head is `25ad21e`; later review and handoff commits are documents-only and do not move the effective head (D-534).
- The current night failure belongs to D-538, which is out of scope for PR-79. D-537 keeps the ruleset bypass for the manual merge of that work.

### Open questions that block progress

None.

### Next concrete action

Resolve the D-538 night-gate failure. Then re-review the same effective head and update the existing review record.

## Session 228: 2026-09-23, Claude Code

Author: Claude Code
Session: PR-79, author. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Answered round 1 of the Codex review (`Changes required` at `d13f73c`). P2-1 has full merit: a PR of the root `README.md` or `LICENSE` alone passed neither by review nor by label.
- Recorded the owner instruction as D-541: the `review-override` label covers each path of the skip set of D-475. `review-gate` now reads the one list of the CI skip for the label. D-190 carries a partial revision mark.
- Wrote `docs/reviews/pr-95-response.md`. It also shows that the Smoke jobs ran and passed at `d13f73c`.

### State of the build

- `dotnet build` passed with no warning. The full suite result and the new code head are in the response file and the PR.
- Four new tests fail on the old `ReviewGateRules.cs` and pass with the correction.

### In flight

- Round 2 of the Codex review on the new code head, then the merge summary and the owner confirmation.

### Traps and gotchas

- `review-gate` runs the tool of `main`, so round 2 must record the new code head. The correction commit holds code, so the old rule and the new rule give one head.
- The night gate stays red on the record of D-538. D-537 keeps the bypass for the merge.

### Open questions that block progress

None.

### Next concrete action

Read round 2 of the Codex review of PR #95. On approval, write the merge summary of D-533 and ask the owner to confirm the merge.

## Session 227: 2026-09-23, Codex

Author: Codex
Session: PR-79, reviewer. Branch `chore/pr-79-review-process`. PR #95, pending merge. Base `2c4e6d5`.

### What this session did, and why

- Reviewed PR #95 at effective head `d13f73c`.
- Found that a PR of only `README.md` or `LICENSE` cannot pass the review gate by review or by label. The review record names the correction and regression checks.

### State of the build

- The focused review tests passed 111 of 111. They built all projects.
- At code head `d13f73c`, the CI platform, bot, content, document, bit-identity, lint, and STE checks passed. At metadata head `7781d3b`, the document checks passed, and the heavy jobs skipped.
- The night gate failed on the known record in D-538. The Smoke jobs were skipped. The review gate failed because P2-1 remains open.

### In flight

- PR #95 needs a correction for P2-1 and a repeat review.
- The night gate remains red until the work in D-538 lands.

### Traps and gotchas

- D-475 skips root `README.md` and `LICENSE` for review. D-190 does not allow the override label for either path.
- The failed night record belongs to D-538, which is outside PR-79.

### Open questions that block progress

None.

### Next concrete action

Correct P2-1 on PR #95 and request a repeat review. Handle the night failure in the next PR under D-538.
