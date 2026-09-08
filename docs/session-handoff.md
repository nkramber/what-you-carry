# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

A Codex session reviews the diff since `e51e272` and updates `docs/reviews/pr-1.md` to the new effective head. On 2026-09-08 the owner mounts the SSD, and a session runs `docs/runbooks/macos-runner.md`.

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
