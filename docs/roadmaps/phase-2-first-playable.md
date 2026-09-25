# Phase 2 roadmap: First playable

Status: **focused roadmap, active.** This file expands Phase 2 of `docs/design.md` section 7: PR-12 to PR-20, PR-57, PR-60 to PR-84, PR-87, and M-3. It applies D-149, D-150, D-157, D-159 to D-168, D-288, D-289, D-291 to D-296, D-298 to D-302, D-304 to D-354, and D-359 to D-371. PR-72 applies D-483 to D-489. PR-73 applies D-490 to D-495. PR-62 and PR-74 to PR-77 apply D-496 to D-509. PR-74 also applies D-525 to D-532. PR-78 applies D-511 to D-524. PR-79 applies D-533, D-534, and D-539 to D-541. PR-80 applies D-542 to D-544, and D-574 supersedes D-542. PR-81 applies D-538 and D-545 to D-552. PR-82 applies D-550, D-551, D-553, and D-554. PR-83 applies D-555 to D-563. PR-84 applies D-564 to D-569. PR-87 applies D-574 to D-576. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

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
| F-96 | The art stayed a first pass, and no item raised it to finished quality | PR-62, PR-74 to PR-77 |
| F-97 | The tunnels felt cramped in play, and every rise in a tunnel needed a jump | PR-63, PR-64, PR-65, PR-66, PR-16 |
| F-98 | On the wide sizes, about one floor in 96000 ran the dig job cap with a chamber still in rock | PR-63, PR-67, PR-66 |
| F-101 | The night of 2026-09-15 found a shaft that lands on an unreachable floor on the wide sizes: seed 79146, floor 7 | PR-68, PR-66 |
| F-107 | An enemy walks no diagonal, so every path is a staircase of side steps | PR-72 |
| F-108 | An enemy climbs a ramp in slow jumps | PR-72 |
| F-109 | A code head waited 18 to 21 minutes for the hosted CI legs, and a push of documents alone ran every check again | PR-71 |
| F-110 | Three workflows gave three jobs the same check names, so a required check by name matched three jobs | PR-78 |

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

Status: merged 2026-09-14 as PR #65, commit `002054a`. Exit tests 1, 2, 3, and 5 passed before the merge. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. Exit test 4 passed for the bot sweep of the PR, and a measurement dug the floors of the night with no error. The first night on the new sizes runs after the merge. D-354 closed exit test 6 after the merge, when the owner played floor 1. The review found no defect. The measurement found the dig tail of F-98, and D-353 gives it to PR-67.

Scope:

- `Core/Content/FloorTemplate.cs`: the gallery width and height, the drift width and height, and the chamber height range join the template (D-342). The validator rejects a size under the minimum of D-166, and each error names the field (T-2). It also rejects an even tunnel width, a width past the rock shell, and a height past the rows of the floor (D-352).
- `Core/Procgen/DigPlan.cs`: the dig reads these sizes from the template. The constants `GalleryRadius`, `DriftRadius`, `TunnelHeight`, `ChamberHeightMin`, and `ChamberHeightMax` leave Core.
- `content/floors/*.json`: each template takes a gallery of 7 by 5, a drift of 5 by 4, and chambers 5 to 8 high (D-341). It also takes a floor of 64 by 20 by 64 and 5 to 9 rooms with a budget of 100 (D-343, D-344).
- `content/chambers/*.json`: the box size ranges of D-341.
- Tests: a test of one floor size on every band replaces `FloorSizeGrowsWithDepth` (D-343). `TunnelCrossSection` reads the sizes of the template over each stamp of the gallery and the drifts, and the floor plan lists those stamps.
- If 5 to 9 rooms do not fit a 64 by 64 floor over the seed sweep, the session files a question (D-344).
- The dig tail of F-98 waits for PR-67, and PR-63 keeps the job cap of D-279 (D-353). D-359 supersedes that cap in PR-67.
- The simulation version rises, and the bit-identity sweep takes a new known answer (G-20).

Out of scope: ramps and tiers (PR-64 to PR-66), the camera numbers of D-242, the enemy spawns (PR-16).

Exit tests:

1. `EveryChamberReachable`, `NoChamberOverlap`, `StairwellReachable`, and `BudgetWithinTolerance` pass on every template with the new sizes.
2. `TunnelCrossSection` asserts over the seed sweep that every gallery and drift has the width and the height of its template.
3. The content tests reject a missing size field and a size under the minimum of D-166. They also reject an even tunnel width and a size that the floor cannot hold (D-352). Each error names the field.
4. The bot sweep and the night sweep report zero crashes and zero softlocks on the new sizes, inside the dig job cap of D-279. D-359 supersedes that cap in PR-67.
5. The bit-identity job passes on the three platforms with the new known answer.
6. The owner plays floor 1 and confirms that the spaces no longer feel cramped, recorded as a decision.

Review focus: determinism, content, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* the tunnels and chambers are small today, so a fight feels cramped. This change makes every space wider and taller, and the sizes live in data files that a later tune can change.

### PR-67: Dig restart

Status: merged 2026-09-14 as PR #69, commit `f2a04e6`. Exit tests 1, 2, 3, and 5 passed before the merge. CI, smoke, bit identity, bots, det-lint, asset-qa, and STE check passed on the merge commit. Exit test 4 passed for the bot sweep of the PR. The sweep of exit test 3 dug the floors of the night with no error. The first night on the restart runs after the merge. The review found no defect.

Scope:

- `Core/Procgen/FloorGenerator.cs` and `Core/Procgen/DigPlan.cs`: when a dig runs 1000 jobs with a chamber still in rock, the generator digs the floor again (D-353, D-359). The restart draws the chamber kinds again from the next draws of the Procgen stream (D-159, D-361). The floor still comes from the seed and the floor number alone.
- The owner set the job budget to 1000 jobs, the count of digs to 4, and a new chamber draw on each restart (D-359 to D-361). D-359 supersedes the job cap of D-279.
- Tests: each of the 7 floors of F-98 digs every chamber of its budget. A floor with no complete dig in 4 digs is an error (D-360).
- The floors that needed 1001 to 10000 jobs change, so the simulation version rises to 9 (D-260, G-20). The three floors of the bit-identity sweep dig inside the budget and stay the same. The replay header holds the version, so the known answer moves (G-20).

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

Status: merged 2026-09-15 as PR #71, commit `43f14eb`. Exit tests 1 to 6 passed before the merge, on the effective head `b5bf2de`. The review found no defect. The first night on the ramp code runs after the merge, and no dug floor holds a ramp before PR-66.

Scope:

- `Core/World/Ramp.cs`, `Core/World/VoxelGrid.cs`, and `Core/World/BlockId.cs`: a ramp cell holds a sloped floor that rises one block over two, three, or four blocks, along one of four directions (D-345, D-346). A ramp cell is a block id from 8 to 43, inside D-164 (D-367).
- `Core/Physics/SweptAabb.cs` and `Core/Entities/PlayerBody.cs`: a body walks up and down a ramp with no jump. The speed along the slope is the flat speed (D-362). A walk and a sprint stay on the slope on the way down, and a roll leaves it (D-363). A body does not slide, and it jumps and rolls from a ramp (D-364 to D-366).
- `Core/Physics/GridRay.cs`: the camera boom and every projectile stop at the slope of a ramp, and not at the edge of its cell (D-246).
- `Core/Procgen/Reachability.cs`: the search reads a ramp as a walk in both directions, the move rule of D-165 as D-345 revises it.
- Tests build ramps by hand in a test grid. The generator digs no ramp before PR-66.
- The simulation version rises to 10, and the bit-identity sweep adds a run over ramps (G-20).

Out of scope: the ramp mesh (PR-65), the generator (PR-66), the pathfinder (PR-16).

Exit tests:

1. `BodyWalksUpARamp` asserts for each slope and each direction that a body walks from the low floor to the high floor with no jump.
2. `BodyWalksDownARamp` asserts that a walk and a sprint from the high floor reach the low floor and stay on the slope (D-363). `ARollLeavesARampOnTheWayDown` asserts that a roll leaves it.
3. `NoFallThroughASlope` and `NoTunnelThroughARamp` assert over seed loops of ramp grids no tunnel through a ramp at maximum speed and no fall through a slope.
4. `CameraNeverEntersARamp` asserts that the boom stops at the slope, and `ProjectileHitsARampSlope` asserts that a shot stops on it.
5. `TheSearchWalksARamp` asserts that the search joins the two floors of a ramp in both directions. `TheSearchFollowsTheBodyRuleOnRamps` asserts the body rule beside a ramp.
6. The bit-identity job passes on the three platforms with the ramp run.

Review focus: determinism, physics, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* today a height change is a row of whole-block steps, and each step needs a jump. This change adds a sloped block that bodies walk up and down, with the same result on every machine.

### PR-65: Ramp meshes in Game

Status: merged 2026-09-15 as PR #73, commit `4bc8cd4`. Exit tests 1 to 4 passed before the merge, on the effective head `720c7a9`. The review found no defect. The generator digs no ramp before PR-66, so no floor of play draws one yet.

Scope:

- `WhatYouCarry.Game/World/GreedyMesher.cs`, `RampFaces.cs`, `FaceShape.cs`, and `GreedySweep.cs`: a ramp cell gives a sloped face and each side and end that shows (D-345). A face beside a ramp shows unless the ramp covers it. Each face of a ramp takes the raw stone tile at 32 texels per meter (D-308, D-368), and the slopes of one plane merge.
- `WhatYouCarry.Game/World/AmbientOcclusion.cs`: the vertex occlusion of D-81 reads the upper half of a ramp as a block and the lower half as air.
- `WhatYouCarry.Game/Render/MeshData.cs`: a triangle face for the side of a ramp that comes to a point at the low end.
- The chunk mesh budget of D-291 holds with ramps.
- The contact sheet adds a ramp of each slope at game zoom, in the whole render (D-306).

Out of scope: the generator (PR-66), new texture rules (PR-62).

Exit tests:

1. `GreedyMesherTests.RampGivesItsSlopeAndSideFaces` covers each slope and each direction: a ramp gives its sloped face and its side faces, and a block beside it keeps its open faces. `SlopeVerticesLieOnTheSlope`, `EveryFaceKeepsTheTexelDensity`, `RampTrianglesRunClockwiseWithArea`, and `SlopesMergeAcrossTheWidth` cover the geometry.
2. `MeshBudgetTest` passes on a test floor with a ramp in each chamber (D-291).
3. The smoke session passes on the three platforms.
4. The owner approves a contact sheet with the ramps, recorded as a decision (D-369).

Review focus: presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* the game can draw only whole blocks today. This change draws the sloped blocks of PR-64 with the same textures and shade as the walls.

### PR-68: The shaft landing fix

Status: merged 2026-09-16 as PR #75, commit `ac534d9`. Exit tests 1, 2, and 3 passed before the merge. CI, smoke, and bit identity passed on the three platforms, with the compare job, and asset-qa, bots, det-lint, and STE check passed. The hand night of run 35067529373 passed at `cb3c2e2`, with the two bot sets of 5000 seeds and the sweep of 100000 seeds. The `night-gate` job stayed red, because that commit is not on `main`, and D-372 carried the merge. The review found no defect.

Scope:

- `Core/Procgen`: the dig gives every shaft a landing that a body reaches from the spawn (D-370). The fix names the cause, and it does not weaken the sweep of PR-9.
- Seed 79146, floor 7 becomes a regression test. The floor comes from the dig sizes of D-341, D-343, and D-344 (F-101).
- The simulation version rises when the dug floors move (D-260, G-20).

Out of scope: the ramps and the tiers of PR-66, and the mesh of PR-65.

Exit tests:

1. A regression test digs seed 79146, floor 7, and asserts a reachable landing for every shaft. It fails on `4bc8cd4`.
2. `EveryChamberReachable` and `DetailKeepsEveryChamberReachable` pass over the night sweep of 100000 seeds. `night.yml` takes a manual event, so the run comes before the merge.
3. The PR sweep, the bot sweep, and the property tests pass.

Review focus: procgen, determinism, test quality.

Check clause: none.

Gate: exit tests 1 to 3 pass. The `night-gate` job stays red, because a hand night on the branch writes a record that the base branch does not hold (D-275). D-372 merges PR-68 with that gate red.

> *In plain English:* one floor in a hundred thousand drops the player down a shaft into a space with no way back. This change joins every shaft landing to the rest of the floor, and that floor becomes a test.

### PR-69: The night record guards the ref

✅ Done in PR #80. Exit test 1 passes, and it fails on the workflow of the base. Exit test 2 passes. The hand night of this branch ran every sweep to the end, and both record steps skipped. The record on `night-results` stayed at `24f47be`, the success of `3434055`. Exit test 3 needs a hand night on `main`, which no branch can give. The session that runs it states the result in its own handoff entry.

Scope:

- `.github/workflows/night.yml`: the two publish steps take the condition `github.ref == 'refs/heads/main'` beside `always()` (D-373). A night on another ref runs every step and writes no record.
- The remark of the file names the guard and the reason: the record of `main` is the state of the `night-gate` job, and a branch night must not replace it.
- `docs/design.md` and this roadmap state that a branch night proves a fix through its run log alone.

Out of scope: one record for each commit, a second record file for a branch, and the history of the orphan branch (OQ-176 lists them). The read rule of D-275 does not change.

Exit tests:

1. `RepositoryShapeTests` asserts that both publish steps of `night.yml` hold the ref condition.
2. A hand night on a branch runs to the end and leaves the record of `night-results` as it was. The run log names the sweep result.
3. A hand night on `main` writes the record, and the `night-gate` job of an open PR turns green.

Review focus: the workflow condition, and the test that pins it.

Check clause: none.

Gate: exit tests 1 to 3 pass.

> *In plain English:* a night on a side branch can wipe the record that tells every pull request the main line is healthy. This change lets a side branch run the night, and that record stays as it was.

### PR-70: The skill port

Scope:

- `.claude/skills/gitar-review/`: the skill takes the effective head and the metadata set of D-184, so a commit of metadata keeps a pass current. The traps move to `references/traps.md`. The push wait and the comment export point at the new runbook.
- `.claude/skills/ste-writing/`: the skill takes a glossary table of the process terms and a section that states each byte ceiling. The names of the project areas move to `references/technical-names.md`, and the 53 rules to `references/the-53-rules.md`.
- `.claude/skills/pr-review/`: the skill file holds the procedure alone, and seven reference files hold the detail of each case. The read of the PR takes the comment export command and the staged read of the diff.
- `.claude/skills/one-pr-one-session/`: the skill file keeps the binding, the start gate, the documents matrix, and the completion gate (D-385). The transitional prompt of the merge and the enforcement table move to reference files. Section 3.14 of `docs/design.md` names the enforcement file.
- `.claude/skills/design-doc-style/`: the skill takes the entry list of a focused roadmap, with the form and the rule of each part.
- `.claude/skills/csharp-conventions/`: a new skill holds the code rules of the agent files, and both agent files keep a pointer to it (D-383).
- `docs/runbooks/session-context.md`: a new runbook holds the targeted reads, the commit command, the two waits, the comment export, and the staged read of a diff.
- `WhatYouCarry.Tests/ContextBudgetTests.cs`: every `.md` file under `.claude/skills/` takes the ceiling of 12000 bytes, a reference file included, and the ceiling of 31000 bytes for `pr-review` ends (D-384).
- `WhatYouCarry.Tools/SteCheck/`: the checker reads the front matter of a file and holds it to rule 6.3 alone (D-386). An open `---` with no close is a thematic break, and the file then has no front matter.

Out of scope: the branch protection rule of D-387, which the owner applies in the GitHub settings. No code of this repository enforces it.

Exit tests:

1. `ContextBudgetTests` asserts the ceiling over every `.md` file under `.claude/skills/`, reference files included, and it names D-384 on a failure.
2. `SteCheckTests` asserts that a long front matter description gives one finding of rule 6.3. A passive form and a contraction in the front matter give none, and the body of the same file keeps every rule.
3. `SteCheckTests` asserts that a file that opens with an unclosed `---` line has no front matter, so the grammar rules read its body.
4. `RepositoryShapeTests` passes: each new skill has valid front matter, and `one-pr-one-session` stays under 7000 characters.
5. `ste-check` reports no finding over every document, the new runbook and every reference file included.
6. No live document names a section of `pr-review` that the split moved, and no command reads a moved section by a line range.

Review focus: the faithfulness of each moved section, the front matter rule of the checker, and the pointers between the skills, the runbook, and the design doc.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* every session reads the same instructions before it starts work, and those files grew too large to read cheaply. This change splits them, so a session loads the detail of one step at the step that needs it.

### PR-66: Ramps and chamber tiers in the generator

Status: ✅ Done in PR #82.

Scope:

- `Core/Procgen/DigPlan.cs`: `TryDigRamp` digs ramp cells in place of one-block steps, so a tunnel changes height by a ramp or a shaft alone (D-345, D-347). The Procgen stream picks the slope of each ramp from the list of the template (D-159, D-346).
- `content/floors/*.json`: every template lists the slopes 1:2, 1:3, and 1:4, as the runs 2, 3, and 4 (D-346).
- Chamber tiers: a chamber with a tier gets a floor 2 blocks over its chamber floor, and a ramp joins the two (D-348, D-349). The tier chance of the chamber kind decides which chambers get one (D-350).
- `content/chambers/*.json`: each kind names its tier chance. Great stope takes 50, cavern 40, stope, ore bin, and pump chamber 25, and the rest 0 (D-350).
- `Core/Procgen/TierPlan.cs` and `Core/Procgen/TierShapes.cs`: a tier takes one of four shapes, at a drawn share of 25 to 40 percent (D-388, D-389). Its ramp takes a slope that fits, and the tier shrinks before it skips (D-390, D-391).
- A floor whose draws build no tier takes one in the first chamber of a tiered kind that fits (D-392). The ramp of a tier takes the widest cross-section that fits, 3 cells or 2 (D-393).
- `Core/Procgen/DigCanvas.cs`: `CanCarveWithoutStep` refuses a unit that makes a one-block step with the floor of a neighboring space (D-347). The ramp dig takes the two older rules, because the step at each end of a slope is the ramp.
- `DigPlan.BuildTiers` runs after the whole dig. A tier fills two rows over the chamber floor, and a walker that met one mid-dig found its way barred. The pass reads the true entries of each chamber, and it keeps them open.
- `Core/Content/JsonObjectReader.cs`: the reader takes a list of numbers, which the floor template reads for its slopes (D-346).
- `Core/Procgen/DetailPass.cs`: rubble, pillars, posts, and pools keep every ramp and its two ends clear.
- `Core/Bots/GreedyDescender.cs`: the walk reads the floor height of the next cell against the feet, and not the row. A walk along a ramp needs no jump, and a walk onto the side of one needs a jump (D-165, D-345).
- The camera test of PR-8 reads the slope of a ramp cell, like the ray of D-246. The camera rests over a slope, in open air.
- The restart test of PR-67 takes new seeds, measured under this dig. The seven floors of F-98 dig inside the budget now, and about one floor in 3300 runs the budget.
- `DigPlan.DigShaftRoutes` digs a drift under a chamber, so a shaft of that chamber has a landing (F-103, D-394). The drift starts at a cell of the trail, and it takes a ramp down when no cell lies low enough.
- The hole of a shaft takes no column of a ramp, and no column of an end of one. The hole takes the floor of each of its columns away, and a ramp that ends there loses the floor that its slope meets (D-345).
- The owner answers the shape of a tier before the code: its share of the chamber, and the case of a chamber too small for a tier and its ramp. OQ-179 holds the question, and D-388 to D-391 hold the answers.
- The simulation version rises, and the bit-identity sweep takes a new known answer (G-20).

Out of scope: enemy spawns on a tier (PR-16).

Exit tests:

1. The PR-9 and PR-59 property tests pass with ramps and tiers: every chamber and tier reachable, no overlap, the stairwell reachable, and the budget within tolerance.
2. `TunnelsHaveNoStep` asserts over the seed sweep that no tunnel floor changes height by a one-block step.
3. `RampsUseTheTemplateSlopes` asserts over the seed sweep that every ramp has a slope of its template, and that each slope appears.
4. `TierChanceMatchesTheKind` asserts over the seed sweep that each kind draws tiers near its chance, and that no kind of chance 0 draws or holds one.
5. `TierIsTwoBlocksUp` asserts that every tier floor is 2 blocks over its chamber floor, and that a ramp joins the two.
6. `EveryFloorTakesATierWhenOneFits` asserts that over 40 percent of floors hold a tier, and that every tier ramp is 3 or 2 cells across (D-392, D-393).
7. `EveryFloorTakesAShaftWhenOneFits` asserts that over 2 percent of floors hold a shaft, and that no pillar stands in the hole of one (F-103, D-394).
8. The bot sweep and the night sweep report zero crashes and zero softlocks with ramps and tiers.
9. The bit-identity job passes on the three platforms with the new known answer.
10. The owner plays floor 1 and confirms the ramps and the tiers, recorded as a decision.

Review focus: determinism, gameplay, test quality.

Check clause: none.

Gate: exit tests 1 to 10 pass.

> *In plain English:* the mine joins its levels with smooth ramps of three slopes in place of steps. Some chambers get a raised floor, so a fight can use the high ground.

### PR-16: First enemy family, AI, and pathfinder

✅ Done in PR #83.

Scope:

- `Core/Pathfinding/GridMoves.cs`: the move rule of D-165 as D-345 revises it, moved out of `Reachability`: one block up, any drop, or a walk along a ramp (D-76). The generator and the AI read one rule, so a floor that the generator calls reachable holds no path that an enemy cannot walk (D-111).
- `Core/Pathfinding/GridPathfinder.cs`: an A* search over that rule. Every move costs one, and the estimate is the steps along X and Z. Two cells of one estimate pop in the order that they entered (G-9). One pathfinder holds the arrays of one grid, and every search reuses them (D-109).
- `Core/Pathfinding/PathWalk.cs` and `PathFollower.cs`: the rules that walk a body along a cell path, and the state of one walker. The last meters of a chase are a straight walk at the body of the target (F-104). A walk that gains nothing for four seconds jumps (F-105).
- `Core/Ai/HumanoidBrain.cs`: the wake on sight inside 20 meters with a clear ray (D-400). It gives up after 5 seconds with no ray. It walks a path, swings inside the attack range, and retreats while the cooldown runs (D-402).
- `Core/Entities/Enemy.cs`, `Core/Combat/Swing.cs`, and `Core/Combat/Stagger.cs`: the enemy carries the box of D-165, the swing state machine, and the stagger of D-326. The player of PR-15 now reads the same two types (D-31, D-111).
- `content/enemies/scavenger.json` with the `enemy` content type and its validator (D-168, D-397, D-399, D-402). The loader rejects a family that names no weapon of the set.
- `Core/Procgen/EnemyPlacement.cs`: every chamber past the first takes enemies for its weight, from a running total, and the chamber of the player spawn takes none (D-167, D-398). The pass draws nothing, so every floor digs as it dug before (D-159).
- `Core/Simulation/SimulationLoop.cs`: the enemies and their brains run after the player, in spawn order. The blade of the player takes every living enemy as a target, and the blade of an enemy takes the player (D-325).
- `Core/Bots/FullClearer.cs`: a policy that hunts every enemy of the floor before the stairwell (D-149). `BotIntent` and `BotReflex` hold the intent shape and the roll that both bot policies read (D-111).
- `Core/Bots/BotRun.cs`: the `death` end state of D-403. `Core/Bots/GreedyDescender.cs` swings at what stands in reach and rolls from a blade (D-404).
- `Game/Render/EnemyNodes.cs`: every living enemy draws with the body model of PR-13 and the sword of PR-15, at the position of the simulation, with no clip (D-401).
- The simulation version rises to 13, and the bit-identity sweep takes a new known answer and one enemy family (G-20).
- `Tools/HandoffRotate`: the rotation puts a handoff entry that sits under an older one back in its place, and it names the entry (D-406, F-106). Four sessions added an entry at the end of the file, and each one reddened the three build legs of its branch. The owner asked for this fix inside this PR.

Out of scope: ranged enemy attacks (PR-24), monsters, and a second family with the rule that mixes two families on one floor (PR-36 to PR-42). Also out of scope: an enemy model and its clips (PR-62), and the stagger and roll numbers of armor weight (PR-22).

Exit tests:

1. `PathfinderRespectsMoveRule` asserts a path uses one-block steps, drops, and ramps, and never a two-block step.
2. `PathfinderFindsStairwell` over one thousand seeds asserts a path from every spawn to the stairwell, of the count of moves that the reachability search gives.
3. `EnemyUsesPlayerRules` asserts an enemy swing has the same windup, active, and recovery as the player's sword, and the same box.
4. `EnemyCountMatchesBudget` asserts the spawned weight within 10 percent of the floor budget less the weight of the chamber that holds the player spawn (D-167, D-398).
5. `FullClearerClearsFloor` asserts zero enemies alive when the policy takes the stairwell choice at the stairwell. 2026-09-21: a policy that the timer sends to the stairwell leaves the rest alive (D-439, PR #85).
6. `AiIsDeterministic` replays a record with enemies and asserts one state hash, and the bit-identity sweep folds that replay on three platforms.
7. The bot sweep with enemies active reports zero crashes and zero softlocks. A death is its own end state and fails no gate (D-403).

Review focus: determinism, gameplay, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the first enemies find their way through the dungeon and fight by the same rules you do. They wait at their posts until they see you, and they back off between swings. The handoff tool also repairs an entry that lands in the wrong place, which broke the build four times.

### PR-17: Floor timer, hunter, and escalation

✅ Done in PR #84.

Scope:

- `Core/Simulation/FloorTimer.cs`: a tick countdown per floor, with lengths per floor band and boss floors in the floor template (D-44, D-46, D-407). It pauses at the stairwell and runs in boss fights (D-140).
- `Core/Entities/Hunter.cs`: one unkillable entity that spawns at expiry, paths to the player, and follows a speed curve that grows until escape is impossible (D-45, D-408). Its id is `overseer` (D-409). It swings `overseer-pick` (D-413), takes no damage (D-414), spawns out of sight (D-415), and always knows where the player is (D-416).
- `Core/Simulation/Escalation.cs`: waves of the floor families after expiry, on a schedule in the floor template data (D-45, D-410). The waves spawn at the posts of the plan and hunt at once (D-418, D-419).
- `Core/Bots/BotRun.cs`: the `death` end state carries its cause, and the night record counts the deaths of each cause for each policy (D-411). A policy that promises progress reads softlock at expiry (D-420).
- `Core/Bots/TimerTester.cs`: a policy that stands at the floor entry through the expiry and never fights (D-149, D-421).
- Timer events in the run log for M-5.

Out of scope: the timer display (PR-19), the hunter model and sound (PR-14, PR-20 fixtures until then).

Exit tests:

1. `TimerPausesAtStairwell` asserts no countdown while the stairwell prompt is open.
2. `HunterSpawnsAtExpiry` asserts one hunter on the tick after expiry.
3. `HunterCannotDie` asserts the hunter's health ignores damage.
4. `HunterSpeedGrows` asserts speed at expiry plus one minute exceeds speed at expiry.
5. `TimerTesterAlwaysDies` over one thousand seeds, with the enemy spawns and the waves off, asserts the `death` end state with the cause `overseer` (D-411).
6. `GreedyDescenderRarelyMeetsHunter` runs one thousand seeds. A floor counts when the policy completes it or its timer expires. The test asserts expiry on under 5 percent of at least 500 such floors (D-412).
7. `TimerIsDeterministic` replays a record past expiry and asserts one state hash on three platforms.

Review focus: determinism, gameplay, economy, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* each floor has a clock. When it runs out, an unstoppable hunter arrives and gets faster, so a wait is never the safe choice.

### PR-18: Stairwell and floor transition

✅ Done in PR #85.

Scope:

- `Core/Simulation/StairwellPrompt.cs`: the untimed descend-or-ascend choice (D-50, D-140). The prompt is open while the body stands on the stairwell cell, and it closes when the body leaves (D-431). It does not hold the player, and the Overseer and the waves still attack (D-417). An intent button or a bot policy answers it.
- `Core/Simulation/NextFloorWorker.cs`: a pure function of the run seed and the floor number over the content set (D-72, D-429). Core approves no threading type. `SimulationLoop.OfferNextFloor` takes the plan before the descent, and a headless run digs at the descent.
- `WhatYouCarry.Game/World/ChunkSwap.cs`: runs the worker on one task during each floor and offers the plan to the loop. It uploads the chunks of the next floor over several frames into hidden nodes. The swap at the descent is one frame.
- `Core/Bots/Coward.cs`: a policy that ascends at the first stairwell (D-149). It promises progress (D-433). The PR bot job and the night run it (D-434).
- `Core/Bots/BotRun.cs`: an ascend on floors 1 to 14 ends as `ascend`, the sixth end state, and the night record counts the ascends of each policy (D-430).
- The smoke session of PR-12 walks floor 1 with the greedy descender, opens the prompt, and descends. The script then plays on floor 2 (D-436).
- `Core/Bots/FullClearer.cs`: the policy leaves when the time runs short, so a long floor ends at the stairwell and not as a softlock (D-438, D-439).
- `WhatYouCarry.Game/World/ChunkSwap.cs` meshes the chunks on the worker task. The first Deck run traced 38 to 46 milliseconds of mesh work in one frame on the main thread.
- The bot session takes the flag `--transitions <count>`. The frame log marks each transition, and the session fails on a frame over the hitch budget (D-427, D-435). The session loads no enemy family (D-437).

Out of scope: the hub (PR-30), the ending (PR-35), the button names and the placement of the prompt text (PR-19).

Exit tests:

1. `WorkerEqualsSynchronous` asserts the worker's grid hash equals a synchronous generation for the same seed and floor.
2. `WorkerReadsNoSimulationState` asserts the worker's inputs are the seed and the floor number only, by a type test on its signature.
3. `PromptIsUntimed` asserts that the prompt is open and the timer holds on each tick that the prompt is open (D-140, D-432).
4. `AscendEndsRun` asserts the `ascend` end state and no next floor.
5. `CowardAscendsFirst` asserts the `ascend` end state at floor 1 for the coward policy (D-430).
6. `TransitionUnderHitchBudget` on the Deck asserts no frame over 22 milliseconds across ten transitions (D-427). The owner runs the command in `CLAUDE.md` on the Deck (D-428, D-435), and the session exits 0. The result goes in the handoff of the session that reads it. 2026-09-21: passed on the Deck at `c3ca60a`, with the slowest frame near a transition at 11.1 milliseconds.
7. `SmokeReachesStairwell` runs the extended smoke session and asserts exit code 0.

Review focus: determinism, Core boundary, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 5 and 7 pass. Exit test 6 needs the Deck of the owner.

> *In plain English:* you reach the stairs, choose to go down or leave, and the next floor already exists, so there is no pause.

### PR-19: HUD and controller navigation

✅ Done in PR #86.

Scope:

- `WhatYouCarry.Game/Ui/Hud.cs`: health, the timer, damage numbers, a boss bar placeholder, and the stairwell prompt, as Control nodes built in C# (D-36, D-90). Every label reads the string table (G-8). The default font of the engine (D-441). The health stands at the bottom left (D-442), and the timer stands at the top center with a paused mark (D-443). The boss bar stands under the timer and hides until a boss exists (D-445).
- Damage numbers: small, short-lived, placed above the hit and never over the hit entity's silhouette (F-24). The colors, the lifetime, and the size come from D-444.
- The stairwell prompt at the lower center, with the buttons of the last device (D-447). A tap of interact descends, and a hold of one second ascends (D-448).
- `WhatYouCarry.Game/Ui/Navigation.cs`: focus movement and activation for every later screen with a controller, and one layout scale for 800p (G-15, D-446). The A button activates a control (D-449). The fixture screen holds six buttons and one slider (D-446).
- A screenshot fixture at 1280 by 800 for the Tier 4 pass at the phase gate (D-133): the flag `--hud-shot <png>`.

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

✅ Done in PR #87.

Scope:

- `WhatYouCarry.Tools/AudioSynth/`: the `audio-synth` command renders each sound file to a WAV file, deterministically on the three platforms (D-93, D-462). A sound holds 1 to 4 layers. The `audio-analyze` command writes the band levels of a CC0 reference into a spectral layer (D-459, D-461, D-464). Its four controls trim, stretch, pitch, and tilt the sound (D-465). A pitched sound ships as its CC0 recording in a recording layer (D-467).
- `content/audio/sfx/*.json`: the first set: sword swing, sword hit, footstep, dodge, player hit, hunter spawn, hunter step, timer alarm, and timer tick. The expiry has no sound of its own (D-469). The repository holds each rendered WAV file next to its sound file, and a test compares them, as for the atlas (D-453). The recordings live under `content/audio/recordings/`, with their sources.
- `WhatYouCarry.Core/Simulation/ActionEvent.cs`: an action event for the swing start, the swing hit, the dodge, a hit that lands on the player, and each timer mark (D-454, D-456). The events are not state, so the simulation version stands.
- `WhatYouCarry.Game/Audio/`: four buses (D-452), one player per file, and a cue for each Core event (D-454). The footstep and the hunter step follow a stride rhythm. The hunter step takes the pitch of its speed (D-455).
- The owner approves the sword and hunter sounds after an `afplay` review and a play session (D-457, D-470).

Out of scope: music (PR-50), a mix beyond one bus per group.

Exit tests:

1. `SynthIsDeterministic` renders one parameter file twice and asserts equal bytes.
2. `SynthRejectsUnknownField` asserts a parameter file with an extra field fails with the field (D-92).
3. `EveryEventHasSound` asserts each Core event in the fixture list maps to a rendered file. The list holds the five action kinds, a timer mark at each mark, and the hunter spawn. The expiry has no cue (D-454, D-469).
4. `HunterPitchFollowsSpeed` asserts a higher pitch at a higher hunter speed, up to the ratio 2 (D-455).
5. The owner approves the sword and hunter sounds, recorded as a decision: D-470. `ApprovedSoundsAreTheOwnerChoice` holds each approved file to its SHA-256.

Review focus: content, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* a tool makes every sound from a recipe, and the first sounds give the sword and the hunter their weight.

### PR-71: CI skip and faster tests

✅ Done in PR #89.

Scope:

- `WhatYouCarry.Tools/CiSkip/`: the `ci-skip` command decides whether the heavy jobs of one workflow skip a PR head. Rule 1: every path of the PR is a document. Rule 2: every path of the push after the previous head is a document, and the newest run of the workflow on that head passed (D-474). The skip set is `docs/`, `.claude/skills/`, `CLAUDE.md`, `AGENTS.md`, `README.md`, and `LICENSE` (D-475). A push to `main` never skips (D-473).
- `.github/actions/ci-skip/`: a composite action writes the runs of the previous head from the GitHub API and runs the command. The workflows `ci.yml`, `bit-identity.yml`, `smoke.yml`, and `bots.yml` take a first job `ci-skip`, and each heavy job skips on its output (D-477). The six other workflows run on each head (D-472).
- `.github/workflows/ci.yml`: the `documents` job runs the test category `Documents` on each head (D-476). The hosted Linux and Windows legs each split into two jobs (D-479). The variable `WYC_PR_SWEEP` is `1` on a pull request (D-481).
- `WhatYouCarry.Tests/`: each class that reads a document carries the category `Documents`, and a guard test finds a class without it (D-476). `SweepScope` gives one fifth of each seed sweep on a pull request (D-480). `ProcgenTests` splits into nested classes, so its sweeps run in parallel (D-478).
- `docs/design.md`: F-109 records the measurements of the CI duration.

Out of scope: a cache of the NuGet packages (D-482), and the fixes of F-107 and F-108 (a later PR).

Exit tests:

1. `CiSkipTests` pass: the two rules, the push to `main`, the skip set, the facts from git, the runs file, and the shape of the ten workflows.
2. `DocumentsCategoryTests` pass: every class that reads a document carries the category, and the `documents` job runs it on each head with no condition.
3. `SweepScopeTests` pass, and the suite passes with `WYC_PR_SWEEP=1` and with no variable.
4. On this PR, a push of documents alone after a green head skips the heavy jobs of the four workflows. The log of each `ci-skip` job names rule 2.
5. The CI run of a code head of this PR gives the time of each job, beside run 35771495463 of PR #87 (G-17). The PR description records both.
6. After the merge, the push to `main` runs every job of the four workflows (D-473). The next session reads that run and records the result in its handoff entry.

Review focus: the skip rules and their fallback to a full run (T-2), the conditions of the workflow jobs, and the test split with the seed share.

Check clause: none.

Gate: exit tests 1 to 5 pass. Exit test 6 runs after the merge.

> *In plain English:* each push waited up to 21 minutes for the full checks, also a push that changed a document alone. This change skips the heavy checks for such a push after a green one. It also runs fewer seeds on a pull request and splits the slow tests, so they run side by side.

### PR-72: Enemy diagonals and ramp climb

✅ Done in PR #90.

Scope:

- The PR holds the fixes of F-107 and F-108 by owner instruction, an exception to G-10 (D-483, D-484).
- `WhatYouCarry.Core/Pathfinding/PathWalk.cs`: the jump rule reads the floor of the next cell against the slope under the feet. It reads both at the point where the body enters the next cell. A climb of a ramp then takes no jump (F-108, D-485).
- `WhatYouCarry.Core/Pathfinding/GridMoves.cs`: the diagonal move. Two side moves reach the corner column in either order (D-486). The move rises one block at most, in rows and over the floor under the middle of the start (D-165). The start and the corner columns hold two open cells over the higher floor. A level move and a drop pass one solid corner, and a step up passes none (D-489). `Reachability` keeps the side moves, so the floors do not change (D-488).
- `WhatYouCarry.Core/Pathfinding/GridPathfinder.cs`: a side move costs 10, a diagonal move costs 14, and the estimate is the octile distance (D-487). The enemies, the Overseer, and the bots walk the diagonal moves through `PathFollower` (D-488).
- `WhatYouCarry.Core/Simulation/SimulationVersion.cs`: the version rises to 15, and the bit-identity answer moves (G-20).
- `WhatYouCarry.Tests/`: `EnemyWalkTests` holds the case of D-485, the jump rule, the diagonal rule, the search, and the diagonal walk sweep. `EnemyTests` reads the diagonal move.
- `docs/design.md`: F-107 and F-108.

Out of scope: a jump across a gap, which D-486 did not take, and the straight walk of a brain near its target (F-104).

Exit tests:

1. `EnemiesClimbTheRampsOfTheOwnerFloorWithNoJump` passes: on seed 1, floor 1, a scavenger and the Overseer climb each ramp with no tick in the air (D-485). It fails on the old jump rule.
2. `AWalkAlongARampNeedsNoJump` passes for every rise and run, and `AStepOntoABlockOrTheSideOfARampNeedsAJump` passes.
3. `ABrainClimbsARampWithNoJump` passes for every rise and run.
4. The diagonal rule tests pass: open floor, one solid corner and not two, a step up and a drop (D-486). A diagonal move cuts no climb out of a ramp, and a step up passes no corner (D-489).
5. `TheSearchTakesDiagonalMovesAcrossOpenFloor` passes, and `PathfinderRespectsMoveRule` finds diagonal moves in the paths of 60 seeds (D-487).
6. `DiagonalSweep.ABodyWalksEveryDiagonalMove` passes: on 120 seeds over every floor, a body crosses each diagonal move in 120 ticks with one jump at most.
7. The simulation version is 15, and `BitIdentityKnownAnswer` passes with `dc4258105649a548` on the three platforms (G-9, G-20).
8. The bot tests and `TimerTesterAlwaysDies` pass with the new walk.

Review focus: the diagonal rule against the body physics (D-486, D-489), the jump rule on a ramp, and the cost and the estimate of the search.

Check clause: none.

Gate: exit tests 1 to 8 pass.

> *In plain English:* enemies walked in staircases of side steps and hopped up ramps in slow jumps. They now cut corners where a player can, and they walk up a ramp as the player does.

### PR-73: No suite for documents

✅ Done in PR #91.

Scope:

- `CLAUDE.md` and `AGENTS.md`: a change of documents alone runs `ste-check`, `doc-gate`, and the `Documents` category, and no full suite (D-491, D-492). A change with any other path runs the full suite (D-493). The test line of the PR gate adds the clause of D-494. The Smoke detail moves to `csharp-conventions`, so the agent files stay under the ceiling of D-382.
- `.claude/skills/`: `csharp-conventions`, `review-response`, `gitar-review`, and the `verification.md` reference of `pr-review` state the same rule. The `enforcement.md` reference of `one-pr-one-session` names its enforcement.
- `docs/design.md`: the cost model and this entry.

Out of scope: the PR template, which keeps its text (D-495), and a change of the CI jobs, which PR-71 made (D-474 to D-477).

Exit tests:

1. `ste-check` finds no issue, and the `Documents` category passes.
2. `CLAUDE.md` and `AGENTS.md` are identical and under 15000 bytes (D-122, D-382).
3. `grep -rn "needs the test suite" CLAUDE.md AGENTS.md .claude/skills docs/runbooks` finds nothing.
4. The heavy jobs of the four workflows of D-477 skip this PR by rule 1 of D-474.

Review focus: each place that asks for a test run on a change, and the byte ceilings of the changed files.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* each session ran every test also for a change that touched only a document. Now such a change runs only the checks that read documents.

### PR-62: Texture recipe system

✅ Done in PR #92.

Scope:

- Recipes: `content/textures/recipes/` replaces `content/textures/rules/`. A recipe is an ordered list of paint layers of the five kinds of D-507. A recipe can extend another with a color swap (D-505).
- Paint files: `content/models/player.paint.json` and `content/models/sword-basic.paint.json` name a recipe for each box, and for a single face where that face differs (D-508).
- Generator: `texture-gen` sizes each box face at 32 texels per meter from its box (D-308). It paints the face and packs it into an atlas of 512 by 512 (D-506). It writes `content/textures/layout.json` (D-505). A block is a recipe of 32 by 32 texels.
- Game: the mesher and the model mesh read the place of each block and each face from the layout (D-505). They ignore the UVs of the model file.
- Look: every block keeps its pixels. The body and the sword keep their material, base color, and noise (D-504).
- The owner answers of the art pass for the later PRs: D-496 to D-503. D-527 supersedes D-503, the noise pick.
- Skill: `.claude/skills/asset-texture-creation/` gives the five steps of the art of every asset, from the Meshy prompt to the box model (D-509).

Out of scope: the new body boxes and the face (PR-74), the sword (PR-75), the enemy models (PR-76), the light (PR-77), the armor overlays (PR-22).

Exit tests:

1. `CommittedAtlasMatchesTheGenerator`, a layout test of the same kind, and `GeneratorIsDeterministic` pass.
2. The canvas of each block equals its tile of the atlas of PR-14, pixel for pixel.
3. A test for each layer kind asserts its paint. An unknown kind, a missing recipe, a missing face, and a full atlas each stop the generator with an error that names the file.
4. `RepositoryModelsPass` passes, and each face of each model has a place in the layout at 32 texels per meter.
5. `SmokeSessionPasses` passes on the three platforms.
6. The owner compares a new contact sheet with the sheet of PR-15 and confirms that the look stayed the same.

Review focus: the packer and the layout contract, the error paths of the recipe reader, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* each box face read one plain tile of noise, so no box showed a face, a belt, or a boot. Now each face gets its own painted canvas, and nothing changes on screen yet.

### PR-78: Codex review and auto-merge

✅ Done in PR #93.

Scope:

- `WhatYouCarry.Tools/CodexReview/`: the `codex-review` command. It checks the start conditions and probes the model. It runs one review round in a detached worktree at the PR head, and judges the pushed record (D-511). The Codex call names the model, the effort, the approval policy, and the sandbox (D-511, D-512).
- The login: the command removes the three API credential variables from each Codex process. It forces the ChatGPT login, and refuses any other login (D-523).
- The three-strike count: the `Open at:` line of each finding gives the heads of the rounds in which it is open (D-513 to D-515).
- `Makefile`: the `codex-review` target updates the npm CLI and runs the command (D-512).
- `.github/rulesets/main.json`: the ruleset of `main` (D-520, D-522). `.github/review-gate-mode` holds `enforced` (D-521). The platform jobs of CI, Smoke, and Bit identity take check names with the prefix of the workflow (F-110).
- `WhatYouCarry.Tests/`: `CodexReviewTests`, `CodexReviewGitTests`, and `RulesetTests`.
- `CLAUDE.md`, `AGENTS.md`, and the skills `one-pr-one-session`, `pr-review`, `review-response`, and `gitar-review`: the author loop, the three-strike stop, the auto-merge, and the merge confirmation (D-513, D-516, D-517, D-524). `docs/runbooks/main-ruleset.md` holds the ruleset commands. The agent files name the Tools prefix one time and the play session by its make target, to stay under the ceiling of D-382.

Out of scope: the live repository settings, which the PR-78 session applies after the owner merge (D-519).

Exit tests:

1. `CodexReviewTests`, `CodexReviewGitTests`, and `RulesetTests` pass. A finding open in rounds 1 and 2 passes, and a finding open in round 3 stops. A fixed and reopened finding counts each open round (D-514).
2. `make codex-review PR=<this PR>` reviews this PR, and the author answers each finding until the verdict approves or the three-strike stop fires.
3. Each of the 20 required checks of the ruleset reports on a code head and on a documents head of this PR. The PR description names both heads.
4. After the merge, the owner approves the setup, and the live ruleset matches `.github/rulesets/main.json` (D-519). The PR-78 session runs this test.

Review focus: the start conditions and the outcome rules against T-2, the three-strike count, and the ruleset against the jobs that report on each head.

Check clause: none.

Gate: exit tests 1 to 3 pass. Exit test 4 runs after the merge.

> *In plain English:* the owner started every review by hand and merged every PR by hand. Now one command starts the review, and a PR that passes every gate merges itself.

### PR-74: Body art

✅ Done in PR #94.

Scope:

- `content/models/player.bbmodel`: the brow, the nose, and the beard on the head bone, and a toe box on each lower leg (D-497, D-498, D-501, D-502). Every other box stays (D-499).
- `content/textures/palette.json`: three fine shades between each pair of colors of a ramp, and a ninth ramp, umber, for the dark browns (D-528, D-530).
- `WhatYouCarry.Tools/TextureGen/`: a `shade` field on `fill` and `rect`, and the kinds `grain` and `gradient`, in whole numbers alone (D-527).
- Recipes: the face, the hair, the beard, the sleeve, the torso, the trousers, and the boot, with the trim of D-526 (D-525, D-529, D-531). The face has no eye whites and no flat Minecraft layout (D-83). The blocks and the sword keep their pixels.
- `.github/rulesets/main.json`: the two fields of the `pull_request` rule that the live ruleset holds, by owner approval of 2026-09-23.

Out of scope: the head and feet overlays that enclose the new boxes (PR-22), the scene light (PR-77).

Exit tests:

1. `RepositoryModelsPass` passes. The new boxes clip nothing at the rest pose or at any keyframe of the dodge, the stagger, and the sword swing (D-301).
2. `CommittedAtlasMatchesTheGenerator` and the layout test pass.
3. `SmokeSessionPasses` passes on the three platforms.
4. The owner approves the new contact sheet as finished art, recorded as a decision (D-504). D-532 records it.
5. `GrainPaintsTheSameBytesOnEachPlatform` passes on the three CI platforms (D-527).
6. `RulesetTests` passes, and the comparison of `docs/runbooks/main-ruleset.md` shows an empty diff.

Review focus: the clip check of the new boxes, the whole-number grain and the fine shades, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* the miner was ten plain boxes with speckled paint. This change adds a brow, a nose, a beard, and boots, and paints the body with the soft mottle of the 3D model.

### PR-79: Review process after approval

✅ Done in PR #95.

Scope:

- `WhatYouCarry.Tools/ReviewGate/`: the effective head skips each commit whose paths all lie in the skip set of D-475, and the review path reads it (D-534). The override label reads the work head, the newest commit outside the metadata set (D-539). A PR of documents alone fails the review path, and the failure names the label (D-540). The label covers each path of the skip set, the root `README.md` and `LICENSE` included (D-541).
- `WhatYouCarry.Tools/CodexReview/`: the record names the effective head (D-534). The Gitar start checks and the guard of the round read the work head (D-182, D-184). A PR of documents alone is a refusal with exit 3 (D-540).
- `WhatYouCarry.Tests/`: `ReviewGateRulesTests`, `ReviewGateGitTests`, `CodexReviewTests`, and `CodexReviewGitTests`.
- The skill `one-pr-one-session` and its reference `review-and-merge.md`: the merge summary of four sections, What, How, CI, and Codex review (D-533). `CLAUDE.md` and `AGENTS.md` name it.
- The skills `pr-review`, `gitar-review`, and `ste-writing`: the effective head of the review and the work head of the gitar pass (D-534).

Out of scope: the night fix and the branch nights of D-538, which the next PR holds. The ruleset bypass stays (D-537).

Exit tests:

1. The four test classes pass. A documents commit after an approving review keeps `review-gate` green, and a commit outside the skip set turns it red.
2. `make codex-review PR=<this PR>` reviews this PR, and the author answers each finding until the verdict approves or the three-strike stop fires.
3. The merge confirmation of this PR uses the merge summary of D-533.
4. After the merge, a documents commit after the approving review of a later PR keeps its `review-gate` green. The workflow runs the tool of `main`, so the next PR runs this test, and its session states the result in its handoff entry.

Review focus: the two heads in `review-gate` and in `codex-review`, the override path against D-190, D-539, and D-541, and test quality.

Check clause: none.

Gate: exit tests 1 to 3 pass. Exit test 4 runs after the merge.

> *In plain English:* a fix of one word in a document after the approval of a PR asked for a second full review. Now a change of documents alone keeps the approval, and gitar still reads each push.

### PR-80: Gitar pause

✅ Done in PR #96.

Scope:

- `WhatYouCarry.Tools/CodexReview/`: the flag `--skip-gitar-review` drops the Gitar check run and the Gitar dashboard from the start checks. The thread check stays (D-543).
- `Makefile`: the `codex-review` target passes each word after `--` that starts with `--` to the command (D-543).
- `WhatYouCarry.Tests/`: `CodexReviewTests` and `RulesetTests`.
- `CLAUDE.md` and `AGENTS.md`, the PR template, the skills `gitar-review`, `review-response`, and `one-pr-one-session`, and `docs/runbooks/session-context.md`: a pause note that names D-542. PR-87 removes each note (D-574).
- `docs/runbooks/commands.md`: the rules of the Game arguments and of the generated files leave `AGENTS.md`, which stays under its byte ceiling (D-382).

Out of scope: the end of the pause, which a later PR of the owner holds. The night fix of D-538 comes after this PR (D-544).

Exit tests:

1. `CodexReviewTests` and `RulesetTests` pass. The flag drops each Gitar problem, and an open thread still refuses the round.
2. `make codex-review PR=<this PR> -- --skip-gitar-review` reviews this PR with no Gitar check. The author answers each finding until the verdict approves or the three-strike stop fires.
3. `grep -rn 'D-542'` outside `docs/decisions.md` lists each text of the pause. PR-87 removes each text (D-574).
4. A documents commit after the approving review keeps `review-gate` green. This test is exit test 4 of PR-79.

Review focus: the option parse and the Makefile pass of the flag, the start checks with and without the flag, and the pause texts against D-542. D-574 supersedes D-542.

Check clause: none.

Gate: exit tests 1 to 4 pass. When `night-gate` stays red, the owner merges with the bypass (D-544).

> *In plain English:* each PR waited for an automated review before the second review. The owner pauses that wait. A review that still comes in gets an answer, and the owner hears about it at once.

### PR-81: Night fix and branch nights

✅ Done in PR #97.

Scope:

- By owner instruction, the PR holds the fixes of F-111 and F-112, the branch nights, and the gitar notice rule (G-10 exception, D-538, D-549, D-550).
- `WhatYouCarry.Core/Pathfinding/GridMoves.cs`: a diagonal drop needs an open fall in the corner column, from the landing up to the start (D-545).
- `WhatYouCarry.Core/Pathfinding/PathFollower.cs`: an arrival at a waypoint starts the wedge count again, so a detour away from the goal reads no wedge (D-546).
- `WhatYouCarry.Core/Physics/SweptAabb.cs` and `WhatYouCarry.Core/Entities/PlayerBody.cs`: the sweep builds each box again from the start and the whole displacement so far, in the form of the caller. `SweepFeet` builds the box of a body as `PlayerBody.Box` does (F-112, D-549).
- `WhatYouCarry.Core/Simulation/SimulationVersion.cs`: the version rises to 16, and the bit-identity answer moves (G-20).
- `.github/workflows/night.yml`: a night on a branch writes its record to `night-branch/<branch>`, and `main` alone writes `night-results` (D-373, D-538). A branch night then re-runs the newest `night-gate` run of its branch (D-548).
- `WhatYouCarry.Tools/NightGate/` and `.github/workflows/night-gate.yml`: the gate reads the record of the head branch when the record of `main` fails. That record passes at the effective head of the PR, or at a later commit of the PR (D-547).
- `WhatYouCarry.Tests/`: `EnemyWalkTests`, `PlayerBodyTests`, `BotTests`, `NightGateTests`, `RepositoryShapeTests`, `SimulationTests`, and `BitIdentityTests`.
- `docs/design.md`: F-111 and F-112.
- `CLAUDE.md` and `AGENTS.md`, and the skills `pr-review`, `review-response`, `gitar-review`, `one-pr-one-session`, and `ste-writing`: a gitar notice needs no answer, and it never blocks a verdict (D-550).

Out of scope: the removal of old `night-branch/` branches, and a change of the bot policies. The rule of `Reachability` stays (D-488). The gitar line of the PR template takes D-550 in the next PR (D-551).

Exit tests:

1. `ADiagonalDropNeedsAnOpenFallInTheCornerColumn` passes. It fails on the old rule (D-545).
2. `ADetourAwayFromTheGoalReadsNoWedge` passes. It fails on the old follower (D-546).
3. `GreedyDescenderLeavesTheFloorsOfTheNight` passes on seeds 940, 947, 1268, 2669, 2879, and 4119.
4. `AMoveLeavesTheBoxThatTheSweepRead` passes over four thousand start points. It fails on the old sweep at the start x of seed 4119 (D-549).
5. The simulation version is 16, and `BitIdentityKnownAnswer` passes with `a2e1c2c6f72bc19e` on the three platforms (G-9, G-20).
6. `NightGateTests` pass: a branch night passes at the effective head and at a later documents commit. It fails at an earlier code commit, and it leaves the record of `main` as it was (D-547).
7. `ABranchNightWritesARecordOfItsOwnAndReRunsTheGate` and `NightGateWorkflowFetchesTheRecordWithFullHistory` pass (D-373, D-538, D-548).
8. `grep -rn 'D-550' .claude CLAUDE.md AGENTS.md` lists the rule in each text of the scope.
9. A night by hand on this branch passes, writes `night-branch/fix/pr-81-night-softlocks`, and re-runs the `night-gate` run of this PR, which then passes (D-538, D-548).

Review focus: the open fall, the wedge count against F-105, the sweep frame against D-235, and the branch record read at the effective head.

Check clause: none.

Gate: exit tests 1 to 9 pass.

> *In plain English:* the enemy walk fix of PR-72 left the test bot stuck on 26 of 5000 floors. The bot now walks those floors. A fix PR can also prove itself with a night run of its own, and that run never replaces the result of the main branch.

### PR-82: Template gitar line

✅ Done in PR #98.

Scope:

- `.github/pull_request_template.md`: the gitar line of the PR gate takes the rule of D-550. It reads as the gitar line of the PR gate in the agent files (D-551, D-553).
- `WhatYouCarry.Tests/RepositoryShapeTests.cs`: `PullRequestTemplateGitarLineMatchesThePrGate` (D-554).

Out of scope: the fixed seeds of the night, which the PR after PR-83 holds (D-551, D-560). The end of the gitar pause stays with a later PR of the owner (D-542). PR-87 ends it (D-574).

Exit tests:

1. `PullRequestTemplateGitarLineMatchesThePrGate` passes. It fails on the old template (D-554).
2. `make codex-review PR=<this PR> -- --skip-gitar-review` reviews this PR. The author answers each finding until the verdict approves or the three-strike stop fires (D-543).
3. The merge request of this PR gives the merge summary as questions and answers (D-552).

Review focus: the template line against the PR gate of the agent files, and the test against D-550.

Check clause: none.

Gate: exit tests 1 to 3 pass.

> *In plain English:* the agent files let a gitar comment with no specific item go without an answer. The checklist of each new PR still asked for an answer to every gitar comment. Now both say the same thing, and a test keeps them equal.

### PR-83: Night record promotion

✅ Done in PR #99.

Scope:

- `WhatYouCarry.Tools/NightGate/`: the command `night-promote` reads the record of `main` and the branch record of the merged PR. It compares the trees of the night commit and the merge commit, and it writes the promoted record (D-555, D-556, D-558, D-563). The command `night-publish-check` keeps a record of `main` at a later commit (D-562).
- `.github/workflows/night-promote.yml`: a job on each push to `main` finds the merged PR and runs `night-promote`. It writes `night-results` with a lease on the record that it read (D-557).
- `.github/workflows/night.yml`: a night on `main` runs `night-publish-check` before it writes, and pushes with a lease (D-562). It then re-runs the gate of each open PR (D-559).
- `.github/actions/rerun-night-gates/`: the re-run of the newest `night-gate` run of each open PR on `main` (D-559).
- `WhatYouCarry.Tests/`: `NightGateTests` and `RepositoryShapeTests`.

Out of scope: a promotion of the PR-81 night (D-561), and the fixed seeds of the night, which the next PR holds (D-560).

Exit tests:

1. `BranchNightOfTheMergedPrPromotesAndTheNextPrReadsGreen` passes. It holds the PR-81 case, and the gate of the next PR then reads `main` green (D-555, D-556, D-558).
2. `BranchNightDoesNotPromoteOverACodeChangeOfAnotherPr`, `BranchNightOlderThanTheWindowDoesNotPromote`, and `BranchNightNeverReplacesANewerNightOfMain` pass.
3. `NightOnMainKeepsARecordAtALaterCommit` and `NightPublishDecisionNamesEachCase` pass (D-562).
4. `APushToMainPromotesTheBranchNightOfItsPr`, `ANightOnMainKeepsALaterRecordAndReRunsEveryGate`, and `TheGateReRunActionReRunsTheNewestGateOfEachOpenPr` pass (D-557, D-559).
5. The `night-gate` job of this PR reads green, from a green record of `main` inside 48 hours or from a night on this branch (D-275, D-538, D-547).
6. After the merge, the run of `night-promote.yml` at the merge commit of this PR ends green and names its case. The first merge with a green branch night then promotes it and re-runs the gate of each open PR. The session after each merge reads the run and states the result in its handoff entry (D-375).
7. `make codex-review PR=<this PR> -- --skip-gitar-review` reviews this PR (D-543). The merge request gives the merge summary as questions and answers (D-552).

Review focus: the tree test against the skip set, the ancestry order in both directions, and the lease of each write of `night-results`.

Check clause: none.

Gate: exit tests 1 to 5 and 7 pass. Exit test 6 runs on `main` after the merge.

> *In plain English:* a PR that proved itself with its own night run still left the main branch red after the merge. The next PR then waited a day. Now a merge that changes only documents after that night carries the green result to the main branch.

### PR-84: Night fixed seeds

✅ Done in PR #100.

Scope:

- `WhatYouCarry.Tools/NightGate/NightSeeds.cs`: the seed rules of each night. The fixed set stays the gate, and the UTC date selects a slice of one tenth past it (D-564, D-566). The list also holds the extra fixed seeds and the carried seeds (D-567).
- `WhatYouCarry.Tools/NightGate/extra-seeds.json`: the extra fixed seeds of each sweep. A fix PR of a slice failure adds its seed here (D-567).
- `WhatYouCarry.Tools/NightGate/NightSeedsCommand.cs`: the command `night-seeds` prints the seed list of one sweep, and it names the window in the run log (D-564).
- `WhatYouCarry.Tools/BotRunner/`: `bot-run` takes a seed list and writes the failure line of its policy. `night-record` writes the slice, the carried seeds that ran, and the failed seeds (D-567, D-569).
- `WhatYouCarry.Tools/NightGate/NightPromotionRules.cs`: the case `carry-missing` (D-569).
- `.github/workflows/night.yml`: a step plans the seeds, and each sweep step runs its list. A slice failure fails the night (D-565).
- `.github/scripts/night-failure-record.sh`: after a broken build, the failure record keeps the failed seeds of the record of `main` (D-567).
- `WhatYouCarry.Tests/`: `NightSeedsTests`, `NightGateTests`, `RepositoryShapeTests`, `BotTests`, and the seed list of the reachability sweep in `ProcgenTests`.

Out of scope: the seed counts of a pull request (D-480), and the fix of a seed that a slice finds. A PR of its own holds that fix.

Exit tests:

1. `EachSliceFollowsTheLastPastTheFixedRange` passes (D-566).
2. `ThePlanHoldsEachSeedOnce` and `ASeedListReadsRangesAndSingleSeeds` pass (D-564, D-567).
3. `TheExtraSeedFileHoldsEachSweepPastItsFixedRange` passes (D-567).
4. `TheRecordCarriesEachFailedSeedUntilANightPassesIt`, `ARecordNamesTheSeedsThatItsNightRan`, and `AnOlderRecordCarriesNoSeed` pass (D-567, D-569).
5. `NightSeedsCommandPrintsTheListAndNamesTheWindow` passes (D-564).
6. `NightRecordCommandWritesTheSeedFields` and `NightResultIsPublished` pass.
7. `BotRunTakesASeedListAndWritesItsFailureLine` passes.
8. `TheReachabilitySweepReadsTheSeedListOfTheNight` and `TheNightPlansTheSeedsOfEachSweep` pass.
9. `ABranchNightPromotesOnlyWhenItRanEachFailedSeedOfMain` and `TheFailureRecordOfABrokenBuildKeepsTheCarriedSeeds` pass (D-567, D-569).
10. A branch night of this PR ends, and its record names the slice of its date and the six failure lines (D-538, D-547).
11. After the merge, the first night on `main` runs the slice of its date, and its record names it. The session after the merge reads the run and states the result in its handoff entry (D-375).
12. `make codex-review PR=<this PR> -- --skip-gitar-review` reviews this PR (D-543). The merge request gives the merge summary as questions and answers (D-552).

Review focus: the carry rule when a sweep does not end, the promotion check of D-569, and the seed list of each night step.

Check clause: none.

Gate: exit tests 1 to 10 and 12 pass. Exit test 11 runs on `main` after the merge.

> *In plain English:* each night tested the same seeds, so a fault past them stayed hidden. Now each night also tests a new batch that the date picks. A failure stops merges until a fix, and the failed seed joins the fixed set.

### PR-87: Gitar pause ends

Scope:

- `CLAUDE.md` and `AGENTS.md`, the PR template, the skills `gitar-review`, `review-response`, and `one-pr-one-session`, and `docs/runbooks/session-context.md`: each pause note of D-542 leaves, and the automated pass is a gate again (D-574).
- `.github/scripts/gitar-wait.sh`: the wait after a push. It reads the Gitar check runs of the head every 30 seconds after a wait of 60 seconds (D-575).
- `.github/scripts/gitar-wait.sh`: with no check run at 6 minutes, it posts one `Gitar review` comment. At 15 minutes it stops with exit 1 (D-575).
- `Makefile`: the target `gitar-wait` runs the script (D-575).
- `WhatYouCarry.Tests/`: `GitarWaitTests`, and `NoInstructionTextHoldsTheGitarPause` in `RepositoryShapeTests`.

Out of scope: the flag `--skip-gitar-review`, which stays in `codex-review` (D-543). The night move of PR-85 and the macOS move of PR-86.

Exit tests:

1. `GitarWaitTests` pass on Linux and macOS. A completed check run ends the wait. A missing check run gets one request and a stop at the limit.
2. `NoInstructionTextHoldsTheGitarPause` passes, and it fails on the agent files of `main` before this PR.
3. `PullRequestTemplateGitarLineMatchesThePrGate` passes.
4. `make gitar-wait PR=<this PR>` after each push of this PR ends with exit 0, and the gitar pass of this PR is complete (D-574, D-575).
5. `make codex-review PR=<this PR>` reviews this PR with no flag. The merge request gives the merge summary as questions and answers (D-552).

Review focus: the wait script against D-575, its exit codes and its request rule, and each removed pause text against D-574.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the owner paused the automated review of each PR while it did not work. It works again, so each PR waits for it again. A script watches for the review, and it asks for one when none starts.

### PR-75: Sword art

Scope: the sword of PR-15 gains the detail that the owner asks for, on the recipe system of PR-62 (D-339, D-504).

Out of scope: new weapons.

Exit tests:

1. `RepositoryModelsPass` passes on the sword and every clip.
2. The owner approves a contact sheet of the sword, recorded as a decision.

Review focus: presentation, test quality.

Check clause: none.

Gate: exit tests 1 and 2 pass.

> *In plain English:* the sword is three plain boxes. This change gives it the detail of a finished weapon.

### PR-76: Enemy models

Scope: the enemy models of PR-16 gain their own boxes and recipes, and the color swap of D-507 gives each enemy its colors (D-339, D-504).

Out of scope: the families of PR-36 to PR-42.

Exit tests:

1. `RepositoryModelsPass` passes on each enemy model and every clip.
2. `SmokeSessionPasses` passes on the three platforms.
3. The owner approves a contact sheet of the enemies, recorded as a decision.

Review focus: presentation, the swap, test quality.

Check clause: none.

Gate: exit tests 1 to 3 pass.

> *In plain English:* the enemies borrow a first-pass look. This change gives them their own bodies and colors.

### PR-77: Scene light and edge smoothing

Scope:

- The scene light of play and of the contact sheet moves toward the torchlight of D-59, inside the budget of D-81.
- The antialiasing mode and the texture filter of OQ-181, measured on the Deck against D-295 (D-504).

Out of scope: new light sources in the world.

Exit tests:

1. `SmokeSessionPasses` passes on the three platforms with the new light and the new edge mode.
2. A frame log on the Deck meets D-295 with the chosen mode.
3. The owner approves a contact sheet with the new light, recorded as a decision.

Review focus: the frame time, presentation.

Check clause: none.

Gate: exit tests 1 to 3 pass. OQ-181 blocks the start.

> *In plain English:* the light is one flat setting, and block edges look jagged on the Deck. This change adds torchlight and smooth edges inside the frame budget.

### M-3: Steam Deck frame time

Procedure: on the Steam Deck OLED of the owner (D-296), run the PR-13 build and then the PR-18 build over one full floor. The bot policy `GreedyDescender` drives the Game layer. Record the 99th percentile frame time from a frame log. Repeat for three seeds. Record the table in this file. The target comes from D-295. A miss files a question that binds the next render PR (F-3).

The bot session and the frame log come from PR-13 (OQ-161). The command in `CLAUDE.md` starts the game with the two flags `--bot` and `--frame-log <path>`. The session ends one second after the first descent (PR-18). The build reaches the Deck as the checkout in desktop mode (D-428). The file holds one frame time per line, in microseconds. The end line of the log carries the count of frames and the 99th percentile. The seed of the session is the first seed of `Main` until the hub of PR-30 picks one per run. The three seeds of the table wait for a seed flag or for PR-30.

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
13. ✅ PR-63 merged 2026-09-14 as PR #65. ✅ OQ-172 answered 2026-09-14: D-353.
14. ✅ PR-67 merged 2026-09-14 as PR #69.
15. ✅ PR-64 merged 2026-09-15 as PR #71.
16. ✅ PR-65 merged 2026-09-15 as PR #73.
17. ✅ PR-68 merged 2026-09-16 as PR #75. ✅ OQ-174 answered 2026-09-15: D-370.
18. PR-69. ✅ Done in PR #80. ✅ OQ-176 answered 2026-09-16: D-373.
19. PR-70. ✅ OQ-177 and OQ-178 answered 2026-09-18: D-386 and D-387.
20. PR-66. ✅ Done in PR #82. ✅ OQ-179 and OQ-180 answered 2026-09-19 and 2026-09-20: D-388 to D-394.
21. ✅ OQ-9 answered 2026-09-20: D-395 and D-396.
22. PR-16.
23. ✅ OQ-4 and OQ-6 answered 2026-09-20: D-407 to D-409. The escalation, the exit tests, and the rules of the hunt: D-410 to D-421.
24. PR-17. ✅ Done in PR #84.
25. Owner: answer OQ-44. ✅ Answered 2026-09-21: D-427. The PR-18 answers: D-428 to D-437.
26. PR-18. ✅ Done in PR #85.
27. PR-19.
28. Owner: answer OQ-48 and OQ-182. ✅ Answered 2026-09-21 and 2026-09-22. The answers of PR-20 run from D-450, which D-462 supersedes, to D-470.
29. PR-20. ✅ Done in PR #87.
30. PR-71. ✅ Done in PR #89. ✅ The owner answers of 2026-09-22: D-471 to D-482.
31. PR-72. ✅ Done in PR #90. ✅ The owner answers of 2026-09-22: D-483 to D-489.
32. PR-73. ✅ Done in PR #91. ✅ The owner answers of 2026-09-22: D-490 to D-495.
33. PR-62. ✅ Done in PR #92. ✅ OQ-171 answered 2026-09-13: D-339. ✅ The owner answers of 2026-09-22: D-496 to D-509.
34. PR-78. ✅ Done in PR #93. ✅ The owner answers of 2026-09-23: D-511 to D-524.
35. PR-74.
36. PR-79. ✅ Done in PR #95. ✅ The owner answers of 2026-09-23: D-533, D-534, and D-539 to D-541.
37. PR-80. ✅ Done in PR #96. ✅ The owner answers of 2026-09-23: D-542 to D-544. D-574 supersedes D-542.
38. PR-81. ✅ Done in PR #97. ✅ The owner answers of 2026-09-23: D-538 and D-545 to D-552.
39. PR-82. ✅ Done in PR #98. ✅ The owner answers of 2026-09-23: D-553 and D-554.
40. PR-83. ✅ Done in PR #99. ✅ The owner answers of 2026-09-23: D-555 to D-563.
41. PR-84. ✅ Done in PR #100. ✅ The owner answers of 2026-09-24: D-564 to D-569.
42. PR-85. The night on hosted Linux at 07:07 UTC (D-571 to D-573).
43. PR-87. The end of the gitar pause, and the gitar wait (D-574 to D-576).
44. PR-86. The macOS legs on hosted runners (D-572, D-573).
45. PR-75.
46. PR-76.
47. Owner: answer OQ-181.
48. PR-77.
49. M-3 table complete. The OQ-15 and OQ-50 answers came early, on 2026-09-11: D-295 and D-296.
50. Tier 4 pass on the screenshot fixture (D-133).
51. **← GATE 2 (first playable).** Every exit test in this file passes. The owner plays one floor and signs off on feel in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 2. Each names the PR it blocks.

Open:

- OQ-159: the model file format. Blocks nothing, and it binds the loader of PR-13.
- OQ-160: the occlusion levels and the wall fade numbers. Blocks nothing, and it binds the mesher and the shader of PR-13.
- OQ-181: the antialiasing of the world. Blocks PR-77 (D-504).

Resolved 2026-09-21:

- OQ-48 (D-450, D-462): the sound parameter format. PR-20.
- OQ-182 (D-459, D-464): the sound path after the review of the first sounds. PR-20.
- OQ-44 (D-427): the transition hitch budget. PR-18.
- OQ-161 (D-428): the M-3 run on the Steam Deck. PR-13, PR-18, and M-3.

Resolved 2026-09-20:

- OQ-4 (D-407): the timer lengths. PR-17.
- OQ-6 (D-408, D-409): the hunter pace, look, and sound. PR-17.
- OQ-9 (D-395, D-396): the eight enemy families, and the scavenger as the first one. PR-16.
- OQ-179 (D-388 to D-393): the shape of a tier. PR-66.
- OQ-180 (D-394): the shafts of a floor. PR-66.

Resolved 2026-09-18:

- OQ-177 (D-386): the front matter and the STE rules. PR-70.
- OQ-178 (D-387): a merge with red checks. It needs no code of this repository.

Resolved 2026-09-16:

- OQ-175 (D-372): the night gate of PR-68. PR-68.
- OQ-176 (D-373): the night record of a branch run. PR-69.

Resolved 2026-09-15:

- OQ-174 (D-370): the unreachable shaft landing on the wide sizes. PR-68.

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
