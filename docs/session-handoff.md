# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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
Session: record the PR-10 merge, and answer the PR-11 questions before its code. Branch `docs/pr-10-merge-record`.

### What this session did, and why

- The owner merged PR #31 as `1ce78c6` after the Codex repeat review, and PR #32 as `a3091c9` after a Codex review with no finding (Session 87). PR #32 hardened the review gate under D-269.
- The design doc PR-10 entry reads merged, the roadmap PR-10 entry has its status line, and the sequence marks item 18. The roadmap correction note records F-87 and F-88.
- Asked four owner questions in one batch, and D-270 to D-273 record the answers. OQ-138 to OQ-141 hold the questions.
- D-270: four run end states: `bottom`, `budget`, `softlock`, and `crash`. The random walker ends by budget, and the zero-softlock test of the night reads the greedy descender. D-149 is revised in part.
- D-271: 18000 ticks per floor for the descender, and 36000 ticks per wander for the walker, as Core constants.
- D-272: `RngStream.Bot` is the fifth value of D-159, at the end.
- D-273: the night job commits `night.json` to the orphan branch `night-results`, and PR-58 reads it with one fetch.
- The PR-11 roadmap entry holds the four rules and the night sweep of PR-9, and exit test 6 reads the descender.

### State of the build

- `main` is at `a3091c9`, the squash merge of PR #32. This branch holds the document commit above it, and this entry above that.
- Remote head: `origin/docs/pr-10-merge-record` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 509 tests, 0 failures, as PR #32 left them. No code changed.
- `det-lint`: 0 findings. `ste-check`: 0 findings in 15 files.
- `bit-identity`: `d8943df12fefcbee`. The simulation version is 6.

### In flight

PR #33 is open and it holds this branch. It changes `docs/` alone, so the `review-override` label covers it (D-190). The session applies the label after the automated pass, when every comment has its answer. No other PR is open.

### Where Phase 1 stands

PR-1 to PR-10 and PR-59 are merged. PR-11 remains, and then one scheduled night, PR-58, M-1, and M-2 reach Gate 1. No open question blocks any of them.

### Traps and gotchas

- PR-11 adds `RngStream.Bot`: the bound in `Rng.ForStream` names `Projectile` as the last value, so the bound moves with the enum, and `EveryDeclaredStreamIsAccepted` fails until it does. The bit-identity sweep lists its streams by name, so the new value moves no hash.
- The greedy descender walks the reachability path with a step-up jump in place, as `StairwellTests.WalkTo` does. That walker moves from the tests into a Core policy, and the test then reads the policy.
- At floor 15 the descender ascends, because floor 16 has no template (D-3, D-252). The `bottom` state is that ascent.
- The night job runs on the self-hosted macOS runner and needs write permission on contents to push `night.json` (D-273). The runner label is `macos-arm64-self-hosted` (D-157).
- The run log of a bot run goes through the JSONL logger of PR-4 into a file that the runner opens. Core opens no file, so the sink lives in the Tools project (D-211).
- The override label goes stale on any push outside the metadata set (D-190). The session applies it after the last push and the automated pass, and adds it again after a later push.

### Open questions that block progress

None. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges PR #33. Then a new session starts PR-11 on a short branch: the Bot stream, the two policies in Core, the runner command in Tools with the run log and the four end states, the PR job at one hundred seeds per policy, the scheduled night job with the record on `night-results`, and the seven exit tests, under D-115, D-117, D-127, D-149, D-157, D-177, and D-270 to D-273.


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
