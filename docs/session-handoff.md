# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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
- The night gate reads the success of run 34600758086 at `095ce5e`, ended 13:45 UTC on 2026-09-11. It turns red at 13:45 UTC on 2026-09-13 unless a night refreshes it. The run list held no run of the 17:21 UTC cron of 2026-09-11 at the check of this session. The next scheduled night is 08:07 UTC on 2026-09-12, and it can start hours late (F-95).

### In flight

PR #51: docs alone. The automated pass approved the head with no code finding. Its one comment reads the missing review record, and the reply on the PR names the `review-override` label (D-188, D-190). No commit answered it. The label is on, and the owner merge is next. Then PR-13 opens from `main` per the Phase 2 roadmap: the Blockbench loader, the greedy mesher, and the wall fade shader.

### Traps and gotchas

- A maximum floor (D-164) fills the world budget of D-291 exactly: 64 chunks and 64 world meshes. A second mesh instance per chunk breaks `MeshBudgetTest`.
- D-295 sets 90 frames per second, which is 11.1 milliseconds per frame. The M-3 row of PR-13 reads the 99th percentile frame time against that bound, and a miss files a question (F-3).
- The recommendation of OQ-44 reads "two frames at 60", and D-295 changed the target. The owner answers OQ-44 before PR-18.
- Three comment lines in `IntentBuilder.cs` cite OQ-157, and one says that the owner sets the numbers by a decision. The reference check reads superseded decisions alone, so the citation passes. The next PR that edits that file can cite D-293.
- `dotnet test` with no filter runs `SmokeSessionPasses` on the local Godot build. The full local suite takes about three and a half minutes.
- The next ids are D-297, OQ-159, F-96, and Session 131.

### Open questions that block progress

None blocks PR-13. OQ-1 blocks PR-14. OQ-44 blocks PR-18. OQ-99 is open, and it blocks nothing.

### Next concrete action

The owner merges this PR, which carries the `review-override` label. A session then opens PR-13 from `main` per the Phase 2 roadmap, D-291, D-292, D-295, and D-296. Exit test 7 records the M-3 frame time on the Steam Deck OLED of the owner.

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
