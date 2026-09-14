# Phase 2 roadmap: First playable

Status: **focused roadmap, active.** This file expands Phase 2 of `docs/design.md` section 7: PR-12 to PR-20, PR-57, PR-60 to PR-67, and M-3. It applies D-149, D-150, D-157, D-159 to D-168, D-288, D-289, D-291 to D-296, D-298 to D-302, and D-304 to D-353. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

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
| F-96 | The art stayed a first pass, and no item raised it to finished quality | PR-62 |
| F-97 | The tunnels felt cramped in play, and every rise in a tunnel needed a jump | PR-63, PR-64, PR-65, PR-66, PR-16 |
| F-98 | On the wide sizes, about one floor in 96000 ran the dig job cap with a chamber still in rock | PR-63, PR-67, PR-66 |

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

Status: merged 2026-09-12 as PR #52, commit `9749581`. Exit tests 1 to 6 passed before the merge, and CI, smoke, bit identity, bots, det-lint, and STE check passed on the merge commit. Exit test 7 waits for the M-3 run on the Steam Deck of D-296 (OQ-161). OQ-159, OQ-160, and OQ-161 stayed open at the merge. The first two bind the constants of the loader, the mesher, and the shader.

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

Status: merged 2026-09-12 as PR #54, commit `811aa84`. Exit tests 1 to 5 passed before the merge, and CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. The five questions that the PR raised, OQ-45 and OQ-162 to OQ-165, had their answers before the merge (D-298 to D-302). D-303 records the manual trigger of the automated pass.

Scope:

- `WhatYouCarry.Assets/`: a fifth project with no engine dependency (D-299). The model reader of PR-13 moves into it from Game, and the animation reader of D-298 and the pose math join it. Game and Tools read a model through it.
- `WhatYouCarry.Tools/AssetQa/`: the command `asset-qa` reads each model, each overlay, and each animation and runs three checks (D-135, D-149). No two boxes of one model, or of a model plus one overlay, penetrate each other at the rest pose or at any keyframe (D-301). A shared face is not a clip, and the pairs of a bone and its parent are exempt. Each armor overlay box encloses the body box of its name (D-300). Every file name reference matches the file case (D-302).
- Every content source skips the `models/` directory, so an animation file next to its model is not a content file of the Core loader (D-298).
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

Status: merged 2026-09-12 as PR #56, commit `3d8060b`. Exit tests 1 to 5 passed before the merge, and CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. The owner approved the first contact sheet as D-309. The review found one defect, P2-1, an unreadable input that crashed `texture-gen`, and its correction merged with the PR.

Scope:

- `content/textures/palette.json`: the palette of D-304, eight ramps of four colors from dark to light (D-85).
- `WhatYouCarry.Tools/TextureGen/`: the command `texture-gen` reads the palette and the rule files and writes the atlas of 256 px as one indexed PNG to `content/textures/atlas.png` (D-65, D-85, D-305). A seed in each rule file makes the output deterministic.
- `content/textures/rules/*.json`: one rule file per material, ten in all (D-307). Each rule holds its tile, the base color index, the noise amount, the edge darkness, and a seed.
- Every content source skips `content/textures/`, as it skips `content/models/`, and Game reads the atlas at boot (D-305).
- `content/models/player.bbmodel`: each face reads its body tile at 32 texels per meter (D-308).
- A Game flag `--contact-sheet <png>` renders every block material and the body at game zoom to one image for review (D-83, D-306).

Out of scope: any hand-painted texture, model textures beyond the three body materials of D-307, armor textures (PR-22).

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

### PR-60: Fullscreen window and the test exit

Status: merged 2026-09-13 as PR #58, commit `94f0897`. Exit tests 1 to 5 passed before the merge, and CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. The review found two defects, and their corrections merged with the PR. P2-1: exit tests 2 and 3 did not run a session. P2-2: a plain word after the press tick passed in silence. D-313 gives the check of the whole argument list to PR-61.

Scope:

- `WhatYouCarry.Game/project.godot`: the window mode setting opens the game in the borderless fullscreen of the engine (D-310). The window takes the resolution of the display, on every desktop and on the Steam Deck. A headless run opens no window, as before.
- `WhatYouCarry.Game/Main.cs`: the Escape key and the Start button of a controller end the session with exit code 0 and an end line (D-311). The quit stays until the escape menu of PR-53 replaces it.
- `WhatYouCarry.Game/Input/TestExit.cs`: the two exit inputs, and the `--press` flag that gives the engine one press of either input at one tick. Exit tests 2 and 3 run a headless smoke session with the flag, so the smoke workflow proves the exit on each platform.
- One PR carries both changes, as an exception to G-10 (D-312).

Out of scope: the escape menu and any display option (PR-53), and the Deck verification pass (PR-54).

Exit tests:

1. `WindowOpensFullscreen` asserts that the project opens the window in borderless fullscreen with no fixed size (D-310).
2. `EscapeEndsTheSession` asserts that the Escape key ends a session with exit code 0 and an end line (D-311).
3. `StartButtonEndsTheSession` asserts the same for the Start button of a controller (D-311).
4. `NoOtherInputEndsTheSession` asserts that no other key or button ends a session.
5. `SmokeSessionPasses` and `ContactSheetFailsHeadless` still pass, because a headless run opens no window.

Review focus: presentation, input and CI boundaries, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the game fills the screen that it runs on, so it is no longer a small box on a large display. During testing, Escape or Start closes it.

### PR-61: User argument check

Status: merged 2026-09-13 as PR #60, commit `4a1048c`. Exit tests 1 to 7 passed before the merge, and CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. The review found no defect. Exit test 7 and the ignored flag rule come from D-317, which the owner chose before the code.

Scope:

- `WhatYouCarry.Game/UserArguments.cs`: one parser that reads the user arguments once at boot (D-313). It holds each flag of the Game layer and the count of words after it. `--smoke` and `--bot` take no word, `--frame-log` and `--contact-sheet` take one, and `--press` takes two.
- The parser rejects an unknown word, an unknown flag, a repeated flag, and a flag with too few words. Each is a `ContextException` that names the word, and `Main` ends the boot with exit code 1 (T-2).
- The parser also rejects a flag that the session ignores (D-317). The contact sheet flag takes no other flag, and the smoke flag and the bot flag exclude each other. The error names both flags.
- `Main`, `SmokeSession`, `BotSession`, `FrameLog`, `ContactSheet`, and `TestExit` read their flags through the parser. The check of a plain word after the press tick moves into the parser.

Out of scope: the engine flags before `--`, which the engine reads, and any new flag.

Exit tests:

1. `UnknownWordStopsTheBoot` asserts that a plain word, such as `unexpected` after `--smoke`, is an error that names the word.
2. `UnknownFlagStopsTheBoot` asserts the same for a misspelled flag, such as `--smok`.
3. `RepeatedFlagStopsTheBoot` asserts the same for a second `--press`.
4. `ShortFlagStopsTheBoot` asserts that a flag with too few words after it is an error that names the flag.
5. `SessionCommandsParse` asserts that the user arguments of the smoke, test exit, bot, and contact sheet commands in `CLAUDE.md` parse with no error.
6. `BadArgumentEndsTheSession` runs the headless game with `--smoke unexpected` and asserts exit code 1 and the word in the error line.
7. `IgnoredFlagStopsTheBoot` asserts that the contact sheet flag with another flag, or the smoke flag with the bot flag, is an error that names both (D-317).

Review focus: input and CI boundaries, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the game ignores a typo in a test command today, and a test can pass while it runs the wrong command. After this change, the typo stops the game with a message that names it.

### PR-15: Player entity and the first weapon

Status: merged 2026-09-14 as PR #62, commit `3f3e8bf`. Exit tests 1 to 6 passed before the merge, and CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. D-338 closed exit test 8 as a first pass before the merge. D-340 closed exit test 7 after it, when the owner played the sword. The automated pass found an arc with a step of no turn, and the review found weapon asset paths that left the model directory, P2-1. Both corrections merged with the PR.

Scope:

- `Core/Entities/Player.cs`: the PR-7 body plus sprint, dodge on a cooldown, health, and stagger (D-27, D-28, D-29). The player has no armor in this PR, so the PR builds the zero-weight case: the stagger rule of light armor (D-314) and the cooldown of D-315. PR-22 adds the effects of weight (D-316). The sprint speed is 7 meters per second (D-319).
- The roll moves 3 meters over 18 ticks in the direction of the movement input, and the cooldown counts from the press (D-327). It starts on the ground and out of still water, and it cancels a swing (D-329, D-337). No hit lands during a roll (D-328).
- A stagger lasts 20 ticks and stops the walk, the swing, the roll, and the jump. A guard of 30 ticks after it stops a stunlock (D-326). The tier-0 sword takes one hand, so a hit interrupts its swing (D-321).
- At zero health the run ends as a death (D-322). Health alone carries across a descent (D-335).
- `Core/Combat/MeleeWeapon.cs`: one sword with windup, active, and recovery frames in ticks, and a hit box swept through the active frames (D-25). The initial numbers are D-315. The blade sweeps 90 degrees at a reach of 1.6 meters from right to left, from the camera yaw of each tick (D-324, D-325). A swing starts on a press, and the walk stays free (D-323, D-324).
- The attack bit swings the first weapon definition of the content set, and the Phase 1 shot ends (D-320).
- `content/weapons/sword-basic.json`: the tier-0 sword of D-153, with the `weapon` content type and validator (D-168, D-334).
- `content/models/sword-basic.bbmodel`: the sword with a blade of the new metal texture rule, at the new `weapon` locator of the body (D-330). The contact sheet shows the sword in the hand (D-336).
- `content/models/player.<animation>.json`: the swing, the roll, and the stagger of the body, in the format of D-298, next to the model (D-87, D-331).
- `WhatYouCarry.Game/Animation/`: the keyframe player, which reads a clip through `WhatYouCarry.Assets` (D-299), and the procedural locomotion from distance traveled (D-87, D-333). The body faces the camera yaw (D-332).
- `README.md`: the steps to launch the game. One PR carries it with PR-15, as an exception to G-10 (D-318).
- `AnimationMatchesCore` asserts that each animation's phase ranges equal the weapon's windup, active, and recovery ticks.

Out of scope: enemies, damage numbers on screen (PR-19), any second weapon, the effects of weight (PR-22).

Exit tests:

1. `DodgeCooldownHolds` asserts that a second dodge inside the cooldown does nothing (D-316).
2. `SwordHitsOnlyInActiveFrames` asserts no hit during windup or recovery, and a hit during active frames.
3. `StaggerInterruptsSwing` asserts that a hit during windup cancels the swing when the stagger rule applies.
4. `HealthNeverBelowZero` asserts the floor at zero and a death event at zero.
5. `AnimationMatchesCore` passes for the sword.
6. `PlayerIsDeterministic` replays a record with attacks and asserts one state hash on three platforms.
7. The owner confirms the sword feels committed and readable, recorded as a decision.
8. The owner approves the contact sheet with the sword and the metal tile, recorded as a decision (D-309, D-330).

Review focus: determinism, replay, gameplay, presentation.

Check clause: none.

Gate: exit tests 1 to 8 pass.

> *In plain English:* you can run, jump, dodge, and swing a sword. The swing shows its wind-up, so an enemy can read it, and a hit can stop it.

### PR-63: Dig sizes in the floor template

Scope:

- `Core/Content/FloorTemplate.cs`: the gallery width and height, the drift width and height, and the chamber height range join the template (D-342). The validator rejects a size under the minimum of D-166, and each error names the field (T-2). It also rejects an even tunnel width, a width past the rock shell, and a height past the rows of the floor (D-352).
- `Core/Procgen/DigPlan.cs`: the dig reads these sizes from the template. The constants `GalleryRadius`, `DriftRadius`, `TunnelHeight`, `ChamberHeightMin`, and `ChamberHeightMax` leave Core.
- `content/floors/*.json`: each template takes a gallery of 7 by 5, a drift of 5 by 4, and chambers 5 to 8 high (D-341). It also takes a floor of 64 by 20 by 64 and 5 to 9 rooms with a budget of 100 (D-343, D-344).
- `content/chambers/*.json`: the box size ranges of D-341.
- Tests: a test of one floor size on every band replaces `FloorSizeGrowsWithDepth` (D-343). `TunnelCrossSection` reads the sizes of the template over each stamp of the gallery and the drifts, and the floor plan lists those stamps.
- If 5 to 9 rooms do not fit a 64 by 64 floor over the seed sweep, the session files a question (D-344).
- The dig tail of F-98 waits for PR-67, and PR-63 keeps the job cap of D-279 (D-353).
- The simulation version rises, and the bit-identity sweep takes a new known answer (G-20).

Out of scope: ramps and tiers (PR-64 to PR-66), the camera numbers of D-242, the enemy spawns (PR-16).

Exit tests:

1. `EveryChamberReachable`, `NoChamberOverlap`, `StairwellReachable`, and `BudgetWithinTolerance` pass on every template with the new sizes.
2. `TunnelCrossSection` asserts over the seed sweep that every gallery and drift has the width and the height of its template.
3. The content tests reject a missing size field and a size under the minimum of D-166. They also reject an even tunnel width and a size that the floor cannot hold (D-352). Each error names the field.
4. The bot sweep and the night sweep report zero crashes and zero softlocks on the new sizes, inside the dig job cap of D-279.
5. The bit-identity job passes on the three platforms with the new known answer.
6. The owner plays floor 1 and confirms that the spaces no longer feel cramped, recorded as a decision.

Review focus: determinism, content, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* the tunnels and chambers are small today, so a fight feels cramped. This change makes every space wider and taller, and the sizes live in data files that a later tune can change.

### PR-67: Dig restart

Scope:

- `Core/Procgen/FloorGenerator.cs` and `Core/Procgen/DigPlan.cs`: when a dig passes a job budget, the generator digs the floor again from the next draws of the Procgen stream (D-159, D-353). The floor still comes from the seed and the floor number alone.
- Before the code, the owner sets the job budget and the count of digs before an error (D-353). A decision records whether the cap of D-279 changes.
- Tests: each of the 7 floors of F-98 digs every chamber of its budget.
- The session decides the simulation version and the bit-identity known answer under D-260 and G-20.

Out of scope: the ramps and the tiers (PR-66), a new rule for the chamber draw.

Exit tests:

1. `EveryTailFloorDigs` asserts that each of the 7 floors of F-98 digs every chamber of its budget.
2. The PR-9 and PR-59 property tests pass.
3. A sweep of at least the 675000 floors of F-98 reports zero dig errors, and the PR records the largest job count.
4. The bot sweep and the night sweep report zero crashes and zero softlocks.
5. The bit-identity job passes on the three platforms.

Review focus: determinism, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* about one floor in 96000 fails to dig on the new sizes. This change digs such a floor again from the same seed, so no run stops on it.

### PR-64: Ramp cells in Core

Scope:

- `Core/World/VoxelGrid.cs` and `Core/World/BlockId.cs`: a ramp cell holds a sloped floor that rises one block over two, three, or four blocks, along one of four directions (D-345, D-346). The session decides the encoding of a ramp, inside D-164 or with a decision that revises it, before the code.
- `Core/Physics/SweptAabb.cs`: a body walks up and down a ramp with no jump. The owner answers the motion on a ramp before the code: the speed on a slope, the jump, the roll, and the stagger.
- `Core/Physics/GridRay.cs`: the camera boom and every projectile stop at the slope of a ramp, and not at the edge of its cell (D-246).
- `Core/Procgen/Reachability.cs`: the search reads a ramp as a walk in both directions, the move rule of D-165 as D-345 revises it.
- Tests build ramps by hand in a test grid. The generator digs no ramp before PR-66.
- The simulation version rises, and the bit-identity sweep adds a run over ramps (G-20).

Out of scope: the ramp mesh (PR-65), the generator (PR-66), the pathfinder (PR-16).

Exit tests:

1. `BodyWalksUpARamp` asserts for each slope and each direction that a body walks from the low floor to the high floor with no jump.
2. `BodyWalksDownARamp` asserts that a body walks from the high floor to the low floor with the motion that the owner sets.
3. A property test over a seed loop of ramp grids asserts no tunnel through a ramp at maximum speed and no fall through a slope.
4. `CameraNeverEntersARamp` asserts that the boom stops at the slope, and `ProjectileHitsARampSlope` asserts that a shot stops on it.
5. `TheSearchFollowsTheBodyRule` passes on ramp grids, and the search joins the two floors of a ramp in both directions.
6. The bit-identity job passes on the three platforms with the ramp run.

Review focus: determinism, physics, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* today a height change is a row of whole-block steps, and each step needs a jump. This change adds a sloped block that bodies walk up and down, with the same result on every machine.

### PR-65: Ramp meshes in Game

Scope:

- `WhatYouCarry.Game/World/GreedyMesher.cs`: a ramp cell gives a sloped face and two side faces (D-345). Each face takes the tile of its block at 32 texels per meter (D-308).
- `WhatYouCarry.Game/World/AmbientOcclusion.cs`: the vertex occlusion of D-81 reads the slope of a ramp.
- The chunk mesh budget of D-291 holds with ramps.
- The contact sheet adds ramps of the three slopes at game zoom (D-306).

Out of scope: the generator (PR-66), new texture rules (PR-62).

Exit tests:

1. `GreedyMesherTests` cover each slope and each direction: a ramp gives its sloped face and its side faces, and a block beside it keeps its open faces.
2. The mesh budget test of D-291 passes on a test floor with ramps.
3. The smoke session passes on the three platforms.
4. The owner approves a contact sheet with the ramps, recorded as a decision.

Review focus: presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* the game can draw only whole blocks today. This change draws the sloped blocks of PR-64 with the same textures and shade as the walls.

### PR-66: Ramps and chamber tiers in the generator

Scope:

- `Core/Procgen/DigPlan.cs`: `TryDigRamp` digs ramp cells in place of one-block steps, so a tunnel changes height by a ramp or a shaft alone (D-345, D-347). The Procgen stream picks the slope of each ramp from the list of the template (D-159, D-346).
- `content/floors/*.json`: every template lists the slopes 1:2, 1:3, and 1:4 (D-346).
- Chamber tiers: a chamber with a tier gets a floor 2 blocks over its chamber floor, and a ramp joins the two (D-348, D-349). The tier chance of the chamber kind decides which chambers get one (D-350).
- `content/chambers/*.json`: each kind names its tier chance. Great stope takes 50, cavern 40, stope, ore bin, and pump chamber 25, and the rest 0 (D-350).
- `Core/Procgen/DetailPass.cs`: rubble, pillars, posts, and pools keep every ramp and its two ends clear.
- The owner answers the shape of a tier before the code: its share of the chamber, and the case of a chamber too small for a tier and its ramp.
- The simulation version rises, and the bit-identity sweep takes a new known answer (G-20).

Out of scope: enemy spawns on a tier (PR-16).

Exit tests:

1. The PR-9 and PR-59 property tests pass with ramps and tiers: every chamber and tier reachable, no overlap, the stairwell reachable, and the budget within tolerance.
2. `TunnelsHaveNoStep` asserts over the seed sweep that no tunnel floor changes height by a one-block step.
3. `RampsUseTheTemplateSlopes` asserts over the seed sweep that every ramp has a slope of its template, and that each slope appears.
4. `TierChanceMatchesTheKind` asserts over the seed sweep that each kind gets tiers near its chance, and none at chance 0.
5. `TierIsTwoBlocksUp` asserts that every tier floor is 2 blocks over its chamber floor, and that a ramp joins the two.
6. The bot sweep and the night sweep report zero crashes and zero softlocks with ramps and tiers.
7. The bit-identity job passes on the three platforms with the new known answer.
8. The owner plays floor 1 and confirms the ramps and the tiers, recorded as a decision.

Review focus: determinism, gameplay, test quality.

Check clause: none.

Gate: exit tests 1 to 8 pass.

> *In plain English:* the mine joins its levels with smooth ramps of three slopes in place of steps. Some chambers get a raised floor, so a fight can use the high ground.

### PR-16: First enemy family, AI, and pathfinder

Scope:

- `Core/Pathfinding/GridPathfinder.cs`: an A* search over walkable cells with the move rule of D-165 as D-345 revises it: one block up, any drop, or a walk along a ramp (D-76).
- `Core/Ai/HumanoidBrain.cs`: target selection, approach along a path, and attack with the same melee rules as the player (D-30, D-31). The brain retreats when the swing is on cooldown.
- `content/enemies/<family>.json`: the first humanoid family from OQ-9, with the `enemy` content type and validator (D-168). Its weight maps to the room weights of D-167.
- Spawn placement: the generator of PR-9 fills room weights with enemies of this family.
- `Core/Bots/FullClearer.cs`: a policy that hunts every enemy on the floor before the stairwell (D-149).

Out of scope: ranged enemy attacks (PR-24), monsters, a second family.

Exit tests:

1. `PathfinderRespectsMoveRule` asserts a path uses one-block steps, drops, and ramps, and never a two-block step.
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

### PR-62: Art quality pass

Scope:

- Texture rules: the generator of PR-14 gains the rule kinds of a finished material (D-305, D-339). A face then shows more than a base color with noise and an edge.
- Models: the body of PR-13 and the sword of PR-15 gain the detail that the owner asks for. The detail stays inside the proportion set of D-82 and the rule of D-83. The enemy models of PR-16 take the same pass.
- Light: the scene light of play and of the contact sheet moves toward the torchlight of D-59, inside the budget of D-81.
- The owner answers the rule kinds and the looks in the PR-62 session, before the code (D-339).

Out of scope: armor overlays (PR-22), the enemy families of PR-36 to PR-42, the polygon, pivot, and UV checks (PR-49).

Exit tests:

1. `CommittedAtlasMatchesTheGenerator` and `GeneratorIsDeterministic` pass with the new rule kinds.
2. `RepositoryModelsPass` passes on the new models and on every clip.
3. `SmokeSessionPasses` passes on the three platforms with the new models and the new light.
4. The owner approves a new contact sheet as finished art, recorded as a decision.

Review focus: presentation, content, test quality.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* the blocks, the body, and the sword look like a first pass today. This change gives them the detail and the light of a finished game, before the owner signs off on the first playable.

### M-3: Steam Deck frame time

Procedure: on the Steam Deck OLED of the owner (D-296), run the PR-13 build and then the PR-18 build over one full floor. The bot policy `GreedyDescender` drives the Game layer. Record the 99th percentile frame time from a frame log. Repeat for three seeds. Record the table in this file. The target comes from D-295. A miss files a question that binds the next render PR (F-3).

The bot session and the frame log come from PR-13 (OQ-161). The command in `CLAUDE.md` starts the game with the two flags `--bot` and `--frame-log <path>`. The session ends at the first descent. The file holds one frame time per line, in microseconds. The end line of the log carries the count of frames and the 99th percentile. The seed of the session is the first seed of `Main` until the hub of PR-30 picks one per run. The three seeds of the table wait for a seed flag or for PR-30.

## 5. Sequence

One person owns the program. Items run one at a time in this order. Gate 1 signed 2026-09-11 (D-288).

1. ✅ OQ-47 answered 2026-09-11: D-289.
2. ✅ PR-12 merged 2026-09-11 as PR #49.
3. ✅ OQ-43 and OQ-49 answered 2026-09-11: D-291 and D-292.
4. ✅ PR-13 merged 2026-09-12 as PR #52. Exit test 7 waits for M-3 (OQ-161).
5. ✅ PR-57 merged 2026-09-12 as PR #54.
6. ✅ OQ-1 answered 2026-09-12: D-304.
7. ✅ PR-14 merged 2026-09-12 as PR #56.
8. ✅ PR-60 merged 2026-09-13 as PR #58.
9. ✅ OQ-5 and OQ-46 answered 2026-09-12: D-314 and D-315, with D-316 for weight. ✅ OQ-45 answered 2026-09-12: D-298.
10. ✅ PR-61 merged 2026-09-13 as PR #60.
11. ✅ PR-15 merged 2026-09-14 as PR #62.
12. ✅ Owner answers on the dig sizes and the ramps, 2026-09-14: D-341 to D-351.
13. PR-63. ✅ OQ-172 answered 2026-09-14: D-353.
14. PR-67.
15. PR-64.
16. PR-65.
17. PR-66.
18. Owner: answer OQ-9, at least the first family.
19. PR-16.
20. Owner: answer OQ-4 and OQ-6.
21. PR-17.
22. Owner: answer OQ-44.
23. PR-18.
24. PR-19.
25. Owner: answer OQ-48.
26. PR-20.
27. PR-62. ✅ OQ-171 answered 2026-09-13: D-339.
28. M-3 table complete. The OQ-15 and OQ-50 answers came early, on 2026-09-11: D-295 and D-296.
29. Tier 4 pass on the screenshot fixture (D-133).
30. **← GATE 2 (first playable).** Every exit test in this file passes. The owner plays one floor and signs off on feel in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 2. Each names the PR it blocks.

Open:

- OQ-4: timer lengths. Blocks PR-17.
- OQ-6: the hunter. Blocks PR-17.
- OQ-9: the enemy families. Blocks PR-16.
- OQ-44: the transition hitch budget. Blocks PR-18.
- OQ-48: the sound parameter format. Blocks PR-20.
- OQ-159: the model file format. Blocks nothing, and it binds the loader of PR-13.
- OQ-160: the occlusion levels and the wall fade numbers. Blocks nothing, and it binds the mesher and the shader of PR-13.
- OQ-161: the M-3 run on the Steam Deck. Blocks exit test 7 of PR-13 and M-3.

Resolved 2026-09-14:

- OQ-172 (D-353): the dig tail. PR-63 and PR-67.

Resolved 2026-09-13:

- OQ-171 (D-339): the art quality pass. PR-62.

Resolved 2026-09-12:

- OQ-170 (D-313): unknown user arguments. PR-61.
- OQ-5 (D-314 and D-316): stagger and weight. PR-15 and PR-22.
- OQ-46 (D-315): the initial combat numbers. PR-15.
- OQ-1 (D-304): the palette. PR-14.
- OQ-166 (D-305): the home of the texture files. PR-14.
- OQ-167 (D-306): the render of the contact sheet. PR-14.
- OQ-168 (D-307): the texture scope. PR-14 and PR-22.
- OQ-169 (D-308): the body texel density. PR-14 and PR-22.
- OQ-45 (D-298): the animation keyframe format. PR-57 and PR-15.
- OQ-162 (D-299): the home of the model reader. PR-57.
- OQ-163 (D-300): the overlay rule. PR-57 and PR-22.
- OQ-164 (D-301): the clip rule. PR-57 and PR-49.
- OQ-165 (D-302): the file name reference rule. PR-57.

Resolved 2026-09-11:

- OQ-15 (D-295): the Deck frame target. M-3.
- OQ-43 (D-291): the chunk size and mesh budget. PR-13.
- OQ-47 (D-289): default bindings and curves. PR-12.
- OQ-49 (D-292): the wall fade approach. PR-13.
- OQ-50 (D-296): a Steam Deck unit for M-3. M-3 and PR-13.
- OQ-157 (D-293): the look sensitivity numbers. PR-12.
- OQ-158 (D-294): the Godot binary in CI. PR-12.
