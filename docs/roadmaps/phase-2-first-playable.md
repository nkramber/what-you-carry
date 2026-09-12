# Phase 2 roadmap: First playable

Status: **focused roadmap, active.** This file expands Phase 2 of `docs/design.md` section 7: PR-12 to PR-20, PR-57, and M-3. It applies D-149, D-150, D-157, D-159 to D-168, D-288, D-289, and D-291 to D-296. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the tenets (section 6.1). Phase 1 is `phase-1-foundations.md`. Gate 1 must pass before PR-12 starts.

External facts, verified 2026-09-11:

- The GitHub release `4.7.2-stable` of Godot holds one .NET zip per platform: `Godot_v4.7.2-stable_mono_linux_x86_64.zip`, `Godot_v4.7.2-stable_mono_win64.zip`, and `Godot_v4.7.2-stable_mono_macos.universal.zip`. Source: the GitHub release API, fetched 2026-09-11.
- The Linux zip holds the executable `Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64`. The Windows zip holds `Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe` next to the window executable. The macOS zip holds `Godot_mono.app`. Source: a download and a listing of each zip, 2026-09-11.
- The option `--fixed-fps 60` of Godot 4.7.2 gives every frame the delta of one sixtieth of a second and waits for no clock. The headless smoke session of one thousand ticks then ends in under one second. Source: the help text of the local build and a local run, 2026-09-11.

Correction passes: none yet.

## 1. Thesis

Phase 2 puts the first floor in the owner's hands (D-57). It ends at Gate 2, the first playable milestone (D-150). That build has one generated floor, one sword, one enemy family, the timer, the hunter, and a stairwell. The owner signs off on feel. Every system in this phase renders or drives what Phase 1 built. The order follows dependency. The Game skeleton comes first, because nothing is visible without it. The mesher and the model loader come next, because the player and the enemies need bodies. The asset QA tool follows the loader, before any armor exists (D-149). The player, the first enemy, the timer, and the stairwell then arrive in play order. The HUD and the first sounds close the phase, because feel needs both.

This phase holds the first balance numbers of the project. Each number that a fresh session can set differently is a decision (D-123). The questions in section 6 collect them before the PR that needs them.

## 2. Findings that bind this phase

| # | Finding | Binds |
|---|---|---|
| F-3 | The Steam Deck is the performance floor | PR-13, PR-18, M-3 |
| F-22 | The v1 order put procgen before any render layer for weeks | PR-12 |
| F-24 | Always-on numbers and Deck 800p strain readability | PR-19 |
| F-29 | Four gates preceded their prerequisites | PR-12, PR-16, PR-17, PR-18, PR-57 |
| F-40 | D-88's effect note put wall fade in the mesher. A shader test needs no mesher change | PR-13 |
| F-41 | No item said whether a Steam Deck unit exists for M-3 | M-3 |

## 3. Guardrails for this phase

All guardrails in `docs/design.md` section 6.2 apply. These four matter most in Phase 2:

1. **G-3.** Godot physics and navigation never feed the simulation. The Game layer reads Core state and writes intents, nothing else.
2. **G-8.** No inline strings that the player sees. The HUD reads the string table from the first label.
3. **G-15.** The Steam Deck at 800p is the readability and performance floor for every UI and render change.
4. **G-17.** Every optimization has a profile before it and a measurement after it. M-3 is the first profile.

## 4. Roadmap

Each entry has: scope, out of scope, exit tests, review focus, the check clause, the gate, and a plain-English paragraph. The review skill is `.claude/skills/pr-review/SKILL.md`.

### PR-12: Game skeleton and input

Status: merged 2026-09-11 as PR #49, commit `9313358`. Exit tests 1 to 6 passed before the merge, and the smoke workflow passed on the three platforms on its first run. OQ-157 and OQ-158 stayed open at the merge. The owner answered both on 2026-09-11 with the merged values: D-293 for the two constants, and D-294 for the cache action.

Scope:

- `WhatYouCarry.Game/Main.cs`: the root node built in C# (D-63). It owns one Core simulation, steps it at 60 Hz from the engine's fixed process, and interpolates render positions between the last two ticks (D-73).
- `WhatYouCarry.Game/Input/IntentBuilder.cs`: reads keyboard, mouse, and controller each tick, applies sensitivity and curves, and writes one intent frame (D-15, D-77, D-162). The bindings and curve defaults come from D-289.
- `WhatYouCarry.Game/Smoke/SmokeSession.cs`: a command-line flag `--smoke` (D-114, D-149). It boots, starts a run from seed 1, plays a fixed intent script of one thousand ticks, and quits. The exit code is 0 only when the log has no error.
- CI: a job `smoke` on each platform that downloads the pinned Godot .NET binary, caches it, and runs the smoke session headless.
- A placeholder box for the player and a flat colored floor, so movement is visible before PR-13.

Out of scope: any model, the mesher, the HUD, the stairwell (PR-18).

Exit tests:

1. `IntentBuilderQuantizes` feeds a fractional look delta and asserts the frame holds the quantized hundredths of a degree (D-162).
2. `IntentBuilderIsPure` asserts that two calls with equal raw input give equal frames.
3. `RenderInterpolates` asserts a render position halfway between two tick positions at half a tick.
4. `SmokeSessionPasses` runs the smoke session headless and asserts exit code 0 and zero error lines.
5. The `smoke` CI job passes on all three platforms.
6. `GameReadsNoEnginePhysics` asserts that no Game file references a physics body or a navigation node (G-3).

Review focus: Core boundary, determinism, input and CI boundaries, presentation.

Check clause: none. All Phase 1 checks exist.

Gate: exit tests 1 to 6 pass.

> *In plain English:* this is the first thing you can open and move in. It also adds an automatic run of the real game on every change, on all three kinds of computer.

### PR-13: Model loader and mesher

Status: open 2026-09-11 as PR #52. Exit tests 1 to 6 pass. Exit test 7 waits for the M-3 run on the Steam Deck of D-296 (OQ-161). OQ-159 and OQ-160 bind the constants of the loader, the mesher, and the shader.

Scope:

- `WhatYouCarry.Game/Models/BlockbenchLoader.cs`: reads a Blockbench JSON file and builds an ArrayMesh from the box list (D-9, D-18). The box hierarchy becomes the bones for animation. Each equipment slot has one attachment point.
- `WhatYouCarry.Game/World/GreedyMesher.cs`: one mesh per chunk of the voxel grid, with faces merged across equal blocks, and vertex ambient occlusion (D-78, D-81). The chunk size and the mesh budget come from D-291.
- Wall fade in the world shader: a fragment between the camera and the player, inside a capsule around that segment, fades (D-88). The mesher does not change for it. The approach comes from D-292.
- One atlas texture and one material for every world chunk and every model (D-85).
- `MeshBudgetTest` asserts the mesh instance count per floor under the D-291 budget.

Out of scope: textures (PR-14), armor overlays (PR-22), props (PR-48).

Exit tests:

1. `LoaderBuildsEveryBox` loads a fixture model with ten boxes and asserts ten box meshes with the declared pivots.
2. `LoaderRejectsMalformed` loads a fixture with an absent size and asserts a failure that names the box.
3. `MesherMergesFaces` meshes a 4 by 4 by 1 slab and asserts six faces, not ninety-six.
4. `MesherIsDeterministic` meshes one grid twice and asserts equal vertex buffers.
5. `MeshBudgetTest` meshes a maximum floor (D-164) and asserts the instance count under budget.
6. `AmbientOcclusionDarkensCorners` asserts a corner vertex darker than a flat vertex.
7. M-3 records the Deck frame time on a full floor of world meshes.

Review focus: presentation, dependencies and cost, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* this turns the block grid and the box models into pictures, cheaply enough for the smallest target machine. It also hides the wall between the camera and you.

### PR-57: Asset QA gate v1

Scope:

- `WhatYouCarry.Tools/AssetQa/`: a command that reads each model and its animations and runs three checks (D-135, D-149). No two boxes of one model, or of a model plus its overlays, interpenetrate at any keyframe beyond a tolerance. Each armor overlay encloses its limb box. Every file name reference matches the file case.
- A CI job `asset-qa` that runs on every model under `content/models/`.

Out of scope: the polygon budget, pivots, and UV coverage (PR-49).

Exit tests:

1. `ClipIsDetected` runs the tool on a fixture with two boxes that overlap at one keyframe and asserts one finding that names the keyframe.
2. `OverlayMustEnclose` runs the tool on a fixture overlay smaller than its limb and asserts one finding.
3. `WrongCaseIsDetected` runs the tool on a fixture that references `Sword.json` where the file is `sword.json` and asserts one finding.
4. `RepositoryModelsPass` runs the tool on every model in the repository and asserts zero findings.
5. The `asset-qa` CI job exists and runs on the PR.

Review focus: content, errors, test quality.

Check clause: this PR creates the `asset-qa` check and passes it (G-19).

Gate: exit tests 1 to 5 pass.

> *In plain English:* from the first model onward, an automatic inspection catches pieces that clip through each other and file names that break on Linux.

### PR-14: Texture generator and palette

Scope:

- `content/palette.json`: the palette of about 32 colors, an owner input (D-85, OQ-1).
- `WhatYouCarry.Tools/TextureGen/`: a command that reads the palette and rule files and emits the 32 px atlas as one PNG (D-65, D-85). A seed in the rule file makes the output deterministic.
- `content/textures/rules/*.json`: one rule file per material: base color index, noise amount, edge darkness, and a seed.
- A contact sheet command that renders every material and every model at game zoom to one image for review (D-83).

Out of scope: any hand-painted texture, model textures beyond the first three materials.

Exit tests:

1. `GeneratorUsesPaletteOnly` asserts every pixel in the atlas is one of the palette colors.
2. `GeneratorIsDeterministic` runs the generator twice and asserts equal bytes.
3. `RuleWithUnknownColorFails` asserts a rule that names a color index past the palette fails with the index.
4. `AtlasIsPowerOfTwo` asserts the atlas dimensions.
5. The owner approves the first contact sheet, recorded as a decision.

Review focus: content, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* a tool paints every texture from a fixed set of about thirty colors, so the whole game looks like one thing.

### PR-15: Player entity and the first weapon

Scope:

- `Core/Entities/Player.cs`: the PR-7 body plus sprint, dodge on a cooldown that armor weight extends, health, and stagger (D-27, D-28, D-29). The stagger rule against weight is OQ-5.
- `Core/Combat/MeleeWeapon.cs`: one sword with windup, active, and recovery frames in ticks, and a hit box swept through the active frames (D-25). The initial numbers are OQ-46.
- `content/weapons/sword-basic.json`: the tier-0 sword of D-153, with the `weapon` content type and validator (D-168).
- `content/animations/*.json`: the keyframe format of OQ-45, with per-bone euler rotations in ticks and a phase tag per range (D-87).
- `WhatYouCarry.Game/Animation/`: the keyframe player and the procedural locomotion from distance traveled (D-87).
- `AnimationMatchesCore` asserts that each animation's phase ranges equal the weapon's windup, active, and recovery ticks.

Out of scope: enemies, damage numbers on screen (PR-19), any second weapon.

Exit tests:

1. `DodgeCooldownHolds` asserts a second dodge inside the cooldown does nothing, and the cooldown grows with weight.
2. `SwordHitsOnlyInActiveFrames` asserts no hit during windup or recovery, and a hit during active frames.
3. `StaggerInterruptsSwing` asserts that a hit during windup cancels the swing when the stagger rule applies.
4. `HealthNeverBelowZero` asserts the floor at zero and a death event at zero.
5. `AnimationMatchesCore` passes for the sword.
6. `PlayerIsDeterministic` replays a record with attacks and asserts one state hash on three platforms.
7. The owner confirms the sword feels committed and readable, recorded as a decision.

Review focus: determinism, replay, gameplay, presentation.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* you can run, jump, dodge, and swing a sword. The swing shows its wind-up, so an enemy can read it, and a hit can stop it.

### PR-16: First enemy family, AI, and pathfinder

Scope:

- `Core/Pathfinding/GridPathfinder.cs`: an A* search over walkable cells with the move rule of D-165, one block up or any drop (D-76).
- `Core/Ai/HumanoidBrain.cs`: target selection, approach along a path, and attack with the same melee rules as the player (D-30, D-31). The brain retreats when the swing is on cooldown.
- `content/enemies/<family>.json`: the first humanoid family from OQ-9, with the `enemy` content type and validator (D-168). Its weight maps to the room weights of D-167.
- Spawn placement: the generator of PR-9 fills room weights with enemies of this family.
- `Core/Bots/FullClearer.cs`: a policy that hunts every enemy on the floor before the stairwell (D-149).

Out of scope: ranged enemy attacks (PR-24), monsters, a second family.

Exit tests:

1. `PathfinderRespectsMoveRule` asserts a path uses one-block steps and drops, and never a two-block step.
2. `PathfinderFindsStairwell` over one thousand seeds asserts a path from every spawn to the stairwell.
3. `EnemyUsesPlayerRules` asserts an enemy swing has the same windup, active, and recovery as the player's sword.
4. `EnemyCountMatchesBudget` asserts the spawned weight within 10 percent of the floor budget (D-167).
5. `FullClearerClearsFloor` asserts zero enemies alive when the policy reaches the stairwell.
6. `AiIsDeterministic` replays a record with enemies and asserts one state hash on three platforms.
7. The bot sweep with enemies active reports zero crashes and zero softlocks.

Review focus: determinism, gameplay, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the first enemies find their way through the dungeon and fight by the same rules you do.

### PR-17: Floor timer, hunter, and escalation

Scope:

- `Core/Simulation/FloorTimer.cs`: a tick countdown per floor, with lengths per floor band and boss floors in the floor template (D-44, D-46, OQ-4). It pauses at the stairwell and runs in boss fights (D-140).
- `Core/Entities/Hunter.cs`: one unkillable entity that spawns at expiry, paths to the player, and follows a speed curve that grows until escape is impossible (D-45, OQ-6).
- `Core/Simulation/Escalation.cs`: extra spawns after expiry on a schedule in data (D-45).
- `Core/Bots/TimerTester.cs`: a policy that waits until expiry, then runs for the stairwell (D-149).
- Timer events in the run log for M-5.

Out of scope: the timer display (PR-19), the hunter model and sound (PR-14, PR-20 fixtures until then).

Exit tests:

1. `TimerPausesAtStairwell` asserts no countdown while the stairwell prompt is open.
2. `HunterSpawnsAtExpiry` asserts one hunter on the tick after expiry.
3. `HunterCannotDie` asserts the hunter's health ignores damage.
4. `HunterSpeedGrows` asserts speed at expiry plus one minute exceeds speed at expiry.
5. `TimerTesterAlwaysDies` over one thousand seeds asserts the `death` end state with the hunter as the cause.
6. `GreedyDescenderRarelyMeetsHunter` over one thousand seeds asserts the hunter spawns in under 5 percent of runs.
7. `TimerIsDeterministic` replays a record past expiry and asserts one state hash on three platforms.

Review focus: determinism, gameplay, economy, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* each floor has a clock. When it runs out, an unstoppable hunter arrives and gets faster, so a wait is never the safe choice.

### PR-18: Stairwell and floor transition

Scope:

- `Core/Simulation/StairwellPrompt.cs`: the untimed descend-or-ascend choice on arrival, driven by an intent button or a bot policy (D-50, D-140).
- `Core/Simulation/NextFloorWorker.cs`: generates floor n+1 on one worker thread during floor n, from the run seed and the floor number (D-72). It hands the grid to the simulation at the transition. The worker reads no simulation state.
- `WhatYouCarry.Game/World/ChunkSwap.cs`: uploads the next floor's meshes over several frames before the transition, so the swap is one frame.
- `Core/Bots/Coward.cs`: a policy that ascends at the first stairwell (D-149).
- The smoke session of PR-12 extends to the stairwell and one descend.

Out of scope: the hub (PR-30), the ending (PR-35).

Exit tests:

1. `WorkerEqualsSynchronous` asserts the worker's grid hash equals a synchronous generation for the same seed and floor.
2. `WorkerReadsNoSimulationState` asserts the worker's inputs are the seed and the floor number only, by a type test on its signature.
3. `PromptIsUntimed` asserts the timer holds while the prompt is open (D-140).
4. `AscendEndsRun` asserts the `ascend` end state and no next floor.
5. `CowardAscendsFirst` asserts the `ascend` end state at floor 1 for the coward policy.
6. `TransitionUnderHitchBudget` on the Deck asserts no frame over the OQ-44 budget across ten transitions.
7. `SmokeReachesStairwell` runs the extended smoke session and asserts exit code 0.

Review focus: determinism, Core boundary, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* you reach the stairs, choose to go down or leave, and the next floor already exists, so there is no pause.

### PR-19: HUD and controller navigation

Scope:

- `WhatYouCarry.Game/Ui/Hud.cs`: health, the timer, damage numbers, and a boss bar placeholder, as Control nodes built in C# (D-36, D-90). Every label reads the string table (G-8).
- Damage numbers: small, short-lived, placed above the hit and never over the hit entity's silhouette (F-24).
- `WhatYouCarry.Game/Ui/Navigation.cs`: focus movement and activation for every later screen with a controller, and one layout scale for 800p (G-15).
- A screenshot fixture at 1280 by 800 for the Tier 4 pass at the phase gate (D-133).

Out of scope: the satchel, the tree, the hub, settings.

Exit tests:

1. `HudReadsStringTable` asserts no string literal in the HUD source outside `Strings.Get` (G-8).
2. `DamageNumberAvoidsSilhouette` asserts the number's rectangle does not overlap the entity's screen box.
3. `HudFitsDeck` asserts every HUD element inside 1280 by 800 at the Deck scale.
4. `NavigationReachesEveryControl` asserts that focus can reach every control on a fixture screen with the controller model alone.
5. `HudConstructsHeadless` constructs the HUD in the smoke session without error (D-114).

Review focus: presentation, content, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the on-screen numbers and bars appear, sized for the smallest screen, and every menu works with a gamepad from the start.

### PR-20: Audio synthesizer and first effects

Scope:

- `WhatYouCarry.Tools/AudioSynth/`: a command that renders a sound from a parameter file to a WAV, deterministically (D-93). The parameter format is OQ-48.
- `content/audio/sfx/*.json`: the first set: sword swing, sword hit, footstep, dodge, player hit, hunter spawn, hunter step, timer alarm at one minute, and timer expiry.
- `WhatYouCarry.Game/Audio/`: playback bound to Core events, with the hunter's sound pitched by its speed.
- The synthesizer runs at build time. The game ships the rendered WAV files.

Out of scope: music (PR-50), a mix beyond one bus per group.

Exit tests:

1. `SynthIsDeterministic` renders one parameter file twice and asserts equal bytes.
2. `SynthRejectsUnknownField` asserts a parameter file with an extra field fails with the field (D-92).
3. `EveryEventHasSound` asserts each Core event in the fixture list maps to a rendered file.
4. `HunterPitchFollowsSpeed` asserts a higher pitch at a higher hunter speed.
5. The owner approves the sword and hunter sounds, recorded as a decision.

Review focus: content, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* a tool makes every sound from a recipe, and the first sounds give the sword and the hunter their weight.

### M-3: Steam Deck frame time

Procedure: on the Steam Deck OLED of the owner (D-296), run the PR-13 build and then the PR-18 build over one full floor. The bot policy `GreedyDescender` drives the Game layer. Record the 99th percentile frame time from a frame log. Repeat for three seeds. Record the table in this file. The target comes from D-295. A miss files a question that binds the next render PR (F-3).

The bot session and the frame log come from PR-13 (OQ-161). The command in `CLAUDE.md` starts the game with the two flags `--bot` and `--frame-log <path>`. The session ends at the first descent. The file holds one frame time per line, in microseconds. The end line of the log carries the count of frames and the 99th percentile. The seed of the session is the first seed of `Main` until the hub of PR-30 picks one per run. The three seeds of the table wait for a seed flag or for PR-30.

## 5. Sequence

One person owns the program. Items run one at a time in this order. Gate 1 signed 2026-09-11 (D-288).

1. ✅ OQ-47 answered 2026-09-11: D-289.
2. ✅ PR-12 merged 2026-09-11 as PR #49.
3. ✅ OQ-43 and OQ-49 answered 2026-09-11: D-291 and D-292.
4. PR-13.
5. PR-57.
6. Owner: answer OQ-1.
7. PR-14.
8. Owner: answer OQ-5, OQ-45, OQ-46.
9. PR-15.
10. Owner: answer OQ-9, at least the first family.
11. PR-16.
12. Owner: answer OQ-4 and OQ-6.
13. PR-17.
14. Owner: answer OQ-44.
15. PR-18.
16. PR-19.
17. Owner: answer OQ-48.
18. PR-20.
19. M-3 table complete. The OQ-15 and OQ-50 answers came early, on 2026-09-11: D-295 and D-296.
20. Tier 4 pass on the screenshot fixture (D-133).
21. **← GATE 2 (first playable).** Every exit test in this file passes. The owner plays one floor and signs off on feel in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 2. Each names the PR it blocks.

Open:

- OQ-1: the palette. Blocks PR-14.
- OQ-4: timer lengths. Blocks PR-17.
- OQ-5: stagger and weight. Blocks PR-15.
- OQ-6: the hunter. Blocks PR-17.
- OQ-9: the enemy families. Blocks PR-16.
- OQ-44: the transition hitch budget. Blocks PR-18.
- OQ-45: the animation keyframe format. Blocks PR-15.
- OQ-46: the initial combat numbers. Blocks PR-15.
- OQ-48: the sound parameter format. Blocks PR-20.
- OQ-159: the model file format. Blocks nothing, and it binds the loader of PR-13.
- OQ-160: the occlusion levels and the wall fade numbers. Blocks nothing, and it binds the mesher and the shader of PR-13.
- OQ-161: the M-3 run on the Steam Deck. Blocks exit test 7 of PR-13 and M-3.

Resolved 2026-09-11:

- OQ-15 (D-295): the Deck frame target. M-3.
- OQ-43 (D-291): the chunk size and mesh budget. PR-13.
- OQ-47 (D-289): default bindings and curves. PR-12.
- OQ-49 (D-292): the wall fade approach. PR-13.
- OQ-50 (D-296): a Steam Deck unit for M-3. M-3 and PR-13.
- OQ-157 (D-293): the look sensitivity numbers. PR-12.
- OQ-158 (D-294): the Godot binary in CI. PR-12.
