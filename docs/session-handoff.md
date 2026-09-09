# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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
