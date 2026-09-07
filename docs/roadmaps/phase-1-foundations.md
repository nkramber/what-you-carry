# Phase 1 roadmap: Foundations

Status: **focused roadmap, active.** This file expands Phase 1 of `docs/design.md` section 7: PR-1 to PR-11, M-1, and M-2. It applies D-148 to D-152 and D-156. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the tenets (section 6.1). This file adds per-PR scope, exit tests, review focus, and the questions that each PR needs answered before it starts.

External facts: none new. The Godot version is in the design header, verified 2026-09-07.

Correction passes: none yet.

## 1. Thesis

Phase 1 builds the part of the game that no player sees and every later PR stands on. It ends at Gate 1, a foundation gate with no playtest (D-150). The order inside the phase follows dependency. The scaffold and the STE checker come first, because every later PR must pass the gate they create (D-148). DetMath, the RNG, and the bit-identity job come next, because no simulation math may exist before them (D-69). The loop, the record, the world, the camera, procgen, and projectiles follow in the order that each one reads the one before. The bot harness comes last, because it needs a run to run (D-149).

Each PR below lists its exit tests. An exit test is a named test or check that must pass before the PR can merge. The list is the written exit test that D-150 requires for Gate 1.

## 2. Findings that bind this phase

The design register in `docs/design.md` section 5 holds every finding. These rows bind Phase 1.

| # | Finding | Binds |
|---|---|---|
| F-13 | Windows x64 was absent from the determinism matrix | PR-1, PR-3 |
| F-15 | The harness default adds a co-author trailer | Every PR |
| F-25 | The SSD arrives 2026-09-08 | PR-1 |
| F-28 | The gate required checks before they existed | PR-1, PR-2, PR-3 |
| F-29 | Four gates preceded their prerequisites | PR-7, PR-9, PR-11 |
| F-30 | The run record omitted the initial state and versions | PR-6 |
| F-38 | PR-10's gate named a weapon roster that does not exist until Phase 3 | PR-10 |
| F-39 | The macOS CI leg needs a self-hosted runner that nobody has registered | PR-1 |

## 3. Guardrails for this phase

All guardrails in `docs/design.md` section 6.2 apply. These three matter most in Phase 1:

1. **G-1.** Core has no engine dependency. PR-1 adds the test that asserts it.
2. **G-2 and G-21.** No `System.Math`, `MathF` outside DetMath, SIMD, reflection, `System.Random`, or wall-clock reads in Core. PR-3 adds the lint tool that enforces it.
3. **G-19.** A PR that creates a check passes that check. PR-1 to PR-3 name the absent checks (D-148).

## 4. Roadmap

Each entry has: scope, out of scope, exit tests, review focus, the check clause, the gate, and a plain-English paragraph. Review focus lists the rows of the review contract that apply. The review skill is `.claude/skills/pr-review/SKILL.md`.

### PR-1: Repository scaffold

Scope:

- `WhatYouCarry.sln` with four projects (D-108, D-66):
  - Core: a class library with no package or project references.
  - Game: a Godot .NET project that references Core.
  - Tools: a console project.
  - Tests: an xUnit project that references Core and Tools.
- `global.json` that pins the .NET SDK (D-62, OQ-2). `Directory.Build.props` with nullable on, warnings as errors, and one language version (D-68).
- `project.godot` and the Game project file on `Godot.NET.Sdk` at the pinned version (D-61).
- `.github/workflows/ci.yml` with three jobs: build and test on hosted Linux x64, hosted Windows x64, and the self-hosted macOS arm64 runner (D-100, D-148, OQ-31).
- `.github/pull_request_template.md` with the gate checklist and one "no change needed because" line per document (D-118). It has one line that names each absent check with the PR that creates it (D-148).
- A build and test command section in `CLAUDE.md` and `AGENTS.md`, identical (D-122).

Out of scope: any Core type beyond an empty namespace, any scene, any content file.

Exit tests:

1. `dotnet build` succeeds on all three platforms in CI.
2. `AgentFilesAreIdentical` reads both agent files and asserts equal bytes (D-122).
3. `CoreReferencesNoEngine` reads the Core project file and asserts no package reference and no project reference (G-1).
4. The CI workflow has one job per platform, and the macOS job selects the self-hosted runner label.
5. The PR description names the STE checker, the lint tool, and the bit-identity job as absent, with PR-2 and PR-3 (D-148).
6. No commit in the PR carries a co-author trailer or a generation line (T-6).

Review focus: Core boundary, input and CI boundaries, dependencies, documents.

Check clause: the STE checker, the lint tool, and the bit-identity job do not exist. PR-2 and PR-3 create them.

Gate: exit tests 1 to 6 pass.

> *In plain English:* this makes the empty project with its four parts. It adds the automatic build on three kinds of computer and the checklist every change must fill in. It adds nothing that plays.

### PR-2: STE checker

Scope:

- `WhatYouCarry.Tools/SteCheck/`: a console command that reads each hand-written `.md` file and reports each sentence that breaks a rule (D-130, D-139).
- Rules:
  - the 20-word limit in a numbered step, and the 25-word limit elsewhere.
  - semicolons and contractions.
  - passive voice, found by an auxiliary plus a past participle.
  - helper verbs.
  - -ing forms at a sentence start or after a preposition.
- Word counts follow rules 8.5 to 8.7: parentheses, hyphenated words, numbers, and identifiers count as one word. Headings count as one word.
- A numbered item counts as a procedural step, with the 20-word limit, only inside a section whose heading contains "Sequence" or "Procedure". Every other numbered item uses the 25-word limit.
- Exempt by path: `docs/reviews/`, `docs/session-handoff.md`, `docs/session-handoff-archive.md`, `docs/archive/`. Exempt by block: tables and fenced code.
- Output: one line per finding with file, line, rule id, and the sentence. A non-zero exit code on any finding.
- A CI job `ste-check` that runs the command on every non-exempt `.md` file.

Out of scope: the STE dictionary, spell checks, term consistency.

Exit tests:

1. One unit test per rule with a sentence that passes and a sentence that fails.
2. `RepositoryDocumentsPass` runs the checker on the repository and asserts zero findings.
3. `FixtureFindsEveryRule` runs the checker on a fixture file with one violation per rule and asserts one finding per rule.
4. The `ste-check` CI job exists and runs on the PR.
5. The PR description names the lint tool and the bit-identity job as absent, with PR-3 (D-148).

Review focus: errors, input boundaries, documents, test quality.

Check clause: the lint tool and the bit-identity job do not exist. PR-3 creates them. This PR passes its own checker (G-19).

Gate: exit tests 1 to 5 pass.

> *In plain English:* this adds a tool that reads every document and reports each sentence that breaks the text rules. Documents are the project's memory, so the tool guards that memory.

### PR-3: Seeded RNG, DetMath, lint, and the bit-identity CI job

Scope:

- `Core/Determinism/Rng.cs`: the algorithm from OQ-33, seeded from one 64-bit seed, with a documented stream split for subsystems.
- `Core/Determinism/DetMath.cs`: `Sin`, `Cos`, `Atan2`, `Sqrt`, `Pow`, `Abs`, `Floor`, `Clamp`, and `Lerp` in float (D-70). Range reduction and polynomial evaluation use only add, subtract, multiply, divide, and IEEE square root. `Sqrt` wraps the IEEE square root, which is a basic operation. The accuracy target is OQ-35.
- `Core/Determinism/StateHash.cs`: the hash from OQ-34 over the raw bit patterns of a state, in a fixed field order.
- `WhatYouCarry.Tools/DetLint/`: a command that parses each Core source file with the C# compiler API and reports each banned symbol (D-67, G-2, G-21). The banned symbols are `System.Math`, `MathF` outside `DetMath.cs`, `System.Numerics.Vector`, `System.Runtime.Intrinsics`, `System.Reflection`, `dynamic`, `System.Random`, `DateTime`, `Stopwatch`, and `Environment.TickCount`.
- `Tests/BitIdentity/`: a program that runs a fixed RNG stream and a DetMath sweep over a fixed input grid, then prints the state hash.
- A CI job `bit-identity` that runs that program on each platform and a final step that fails if the three hashes differ (D-69, D-71).

Out of scope: any simulation type, any vector or matrix type beyond what DetMath needs.

Exit tests:

1. `RngKnownAnswer` asserts the first sixteen outputs for seed 1 against recorded values.
2. `RngStreamsDiffer` asserts that two subsystem streams from one seed do not overlap in the first ten thousand outputs.
3. `DetMathAccuracy` compares each function to a double reference over the OQ-35 range and asserts the OQ-35 tolerance.
4. `DetMathRangeReduction` asserts that `Sin` and `Cos` at an angle plus many full turns equal the base angle within tolerance.
5. `StateHashOrder` asserts that two states with equal fields in a different insertion order hash equal.
6. `LintFailsSystemMath` runs the lint tool on a fixture that calls `System.Math.Sin` and asserts one finding.
7. `LintPassesCore` runs the lint tool on the Core project and asserts zero findings.
8. The `bit-identity` CI job reports one equal hash on three platforms.

Review focus: determinism, errors, dependencies, test quality.

Check clause: this PR creates the lint tool and the bit-identity job. It passes both (G-19).

Gate: exit tests 1 to 8 pass.

> *In plain English:* different computers give slightly different answers for functions like sine. This adds our own math that gives the same answer everywhere, and a random-number source that any seed replays. A check on every change proves the three kinds of computer agree to the last bit.

### PR-4: Logger, error context, and assertions

Scope:

- `Core/Logging/JsonlLogger.cs`: one JSON object per line, with a required field set per context (D-68, D-113). The run context requires seed, floor, tick, subsystem, and entity ids. The hub context requires save versions, screen, action, and file paths. The logger throws on an absent required field.
- `Core/Logging/Invariant.cs`: `Assert` that writes a full report and continues where the caller marks the call safe, and throws elsewhere (D-112). The report holds the context, the message, the stack, and the run record path when a run exists.
- `Core/Logging/ContextException.cs`: an exception type that carries the context and adds to it on each rethrow.

Out of scope: a report viewer, file rotation, the crash report UI (PR-55).

Exit tests:

1. `LogLineWithoutContextThrows` asserts that a run-context line without a seed throws.
2. `LogLineIsValidJson` parses an emitted line and asserts every required key.
3. `AssertionReportHasSeed` triggers an assertion in a run context and asserts the report contains the seed.
4. `SafeAssertionContinues` asserts that a call marked safe returns after the report.
5. `UnsafeAssertionThrows` asserts that an unmarked call throws a `ContextException`.
6. `RethrowAddsContext` catches, adds a field, rethrows, and asserts both fields on the outer catch.

Review focus: errors, input boundaries, test quality.

Check clause: none. All checks exist.

Gate: exit tests 1 to 6 pass.

> *In plain English:* every message the game writes about itself carries enough facts to replay the moment. A message without those facts is itself an error.

### PR-5: Content loader, schemas, and the string table

Scope:

- `Core/Content/ContentLoader.cs`: reads a content directory, validates each file against the validator for its type, and returns typed records (D-91, D-92). A failure names the file, the field, and the reason. An unknown field is a failure.
- One validator per content type, in the form that OQ-42 selects. Phase 1 types: `floor-template` (PR-9), `projectile` (PR-10), and `strings`.
- `Core/Content/ContentHash.cs`: the hash from OQ-37 over every content file in sorted path order, for the run record header (D-151).
- `Core/Content/Strings.cs`: the ID-keyed string table from `content/strings/en.json` (D-98). An unknown id throws.
- A lint rule in DetLint (D-98, G-8): a string literal in the Game project outside a `Strings.Get` call is a finding. An allow list covers node names and paths.

Out of scope: any content beyond the three Phase 1 types, localization.

Exit tests:

1. `AbsentFieldNamesField` loads a fixture with one absent field and asserts the file, field, and reason in the error.
2. `UnknownFieldFails` loads a fixture with one extra field and asserts a failure.
3. `EveryContentFileLoads` loads every file under `content/` and asserts success.
4. `UnknownStringIdThrows` asserts that an unknown id throws.
5. `ContentHashIsStable` asserts that two loads of the same files give one hash, and a one-byte change gives another.
6. `LintFlagsInlineString` runs the lint tool on a fixture Game file with an inline literal and asserts one finding.

Review focus: content, errors, Core boundary, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* every floor shape, projectile, and screen text lives in a data file with a strict shape. A file with a gap or an extra field fails loudly instead of a silent zero.

### PR-6: Simulation loop, intent record, recorder, and replay

Scope:

- `Core/Simulation/SimulationLoop.cs`: a fixed step at 60 Hz that advances one tick per intent (D-73). The loop reads no clock.
- `Core/Simulation/Intent.cs`: the layout from OQ-36 (D-74, D-77).
- `Core/Simulation/SimulationVersion.cs`: one constant, initial value 1 (D-151, G-20).
- `Core/Replay/RunRecord.cs`: the header and the frame layout from OQ-37. The header holds the format version, the simulation version, the content hash, the seed, and the immutable initial state (D-151). Phase 1 writes an empty loadout, an empty tree, and no amulet assignment in the initial state, because those types do not exist yet. The schema is complete.
- `Core/Replay/RunRecorder.cs`: writes the header, then appends one checksummed frame per tick from the first tick (D-97, G-5).
- `Core/Replay/RunReplayer.cs`: reads a record, checks the versions and the content hash, and drives the loop from the frames. It ignores the live bank and tree. A torn tail truncates to the last complete frame (D-152). A mismatch produces a report that names both versions.

Out of scope: the resume UI, the profile file (PR-31), the five-second rewind (PR-31).

Exit tests:

1. `ReplayReproducesHash` over one thousand seeds with random intents: the live run and the replay end with one state hash.
2. `TornTailTruncates` writes a record, cuts the last frame in half, and asserts the replay stops at the last complete frame with a log line.
3. `FrameChecksumDetectsFlip` flips one bit in a frame and asserts a report that names the frame.
4. `VersionMismatchReports` changes the simulation version in a header and asserts a report that names both versions.
5. `ContentMismatchReports` changes the content hash in a header and asserts a report.
6. `HeaderRoundTrip` writes and reads a header and asserts every field equal.
7. The `bit-identity` job replays one fixed record on three platforms and asserts one hash.

Review focus: determinism, replay, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the game runs in fixed steps and writes down its start state and every input. Any run can then be played again from that record, which is how every bug becomes repeatable. A record from an older version says so instead of a silent failure.

### PR-7: Voxel world and Core collision

Scope:

- `Core/World/VoxelGrid.cs`: a flat array of block ids with the limits from OQ-38 (D-78). Solid or air per id.
- `Core/Physics/SweptAabb.cs`: swept movement of an axis-aligned box against the grid, one axis at a time, with the box size and jump from OQ-39 (D-27, D-80).
- `Core/Entities/PlayerBody.cs`: a box that reads the intent's movement vector and jump button, applies gravity, and moves through `SweptAabb` (D-149). No health, no weapon.
- Constants for gravity, walk speed, sprint speed, and jump velocity in Core, with a decision entry for the initial values.

Out of scope: enemies, the camera, procgen, any render.

Exit tests:

1. `NoTunnelAtMaxSpeed` over ten thousand random directions: a box at the maximum speed never ends inside a solid block, and never crosses a one-block wall.
2. `NoFallThroughFloor` over one thousand seeds: a box that rests on a floor block stays above it after one thousand ticks.
3. `JumpClearsOneBlock` asserts that a jump from flat ground lands on a one-block step, and fails on a two-block step.
4. `NoOverlapAfterAnyTick` over one thousand seeds with random intents: the box never overlaps a solid block.
5. `PlayerBodyIsDeterministic` replays a record and asserts one state hash.

Review focus: determinism, Core boundary, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the dungeon is a grid of blocks. The game itself decides how a body bumps into them, so the result is identical on every machine. A first body can already walk and jump through a test grid.

### PR-8: Camera as a Core system

Scope:

- `Core/Camera/OrbitCamera.cs`: an over-the-shoulder camera with a boom (D-13, D-75). It integrates the quantized yaw and pitch deltas, clamps pitch, sweeps the boom against the grid, and derives the aim ray (D-77, D-88).
- `Core/Camera/AimAssist.cs`: a function from the aim ray and a target list to an assisted aim ray (D-14). Phase 1 tests it with a synthetic target list, because enemies arrive in PR-16.
- Constants for boom length, shoulder offset, pitch limits, and assist strength, with a decision entry for the initial values.

Out of scope: wall fade (Game layer, PR-13), sensitivity curves (Game layer, PR-12).

Exit tests:

1. `CameraNeverInsideSolid` over one thousand seeds with random look deltas in a random grid: the camera position is never inside a solid block.
2. `AimRayIsDeterministic` replays a record with look deltas and asserts one aim-ray hash.
3. `PitchClamps` asserts that a large pitch delta stops at the limit.
4. `AssistPullsTowardTarget` asserts that the assisted ray is closer to a synthetic target than the raw ray, and equal to the raw ray with no target.
5. The `bit-identity` job replays a record with camera motion on three platforms and asserts one hash.

Review focus: determinism, replay, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the camera is part of the simulation, not decoration, so where you look and where you aim replay exactly on every machine.

### PR-9: Procgen v1 and property tests

Scope:

- `content/floors/*.json`: floor templates with size by depth, room count ranges, corridor cross-section from OQ-40, and the difficulty budget from OQ-41 (D-6, D-46, OQ-12).
- `Core/Procgen/FloorGenerator.cs`: rooms and corridors on the grid, a spawn point, and a stairwell, from the run seed and the floor number (D-78).
- `Core/Procgen/Reachability.cs`: a search over walkable cells. A cell is walkable with two air blocks above it. A move is a step of at most one block up, or any drop, per OQ-39.
- `Core/Simulation/StairwellTransition.cs`: on arrival at the stairwell, a policy or the player chooses descend or ascend. Descend generates the next floor. Ascend ends the run (D-50, D-149).

Out of scope: enemies, loot, the timer, the second biome.

Exit tests:

1. `EveryRoomReachable` over five thousand seeds per PR and one hundred thousand each night: every room is reachable from the spawn (D-116).
2. `NoRoomOverlap` asserts no two rooms share a block.
3. `StairwellReachable` asserts a path from the spawn to the stairwell.
4. `BudgetWithinTolerance` asserts the floor's budget within the OQ-41 tolerance.
5. `CorridorCrossSection` asserts every corridor cell has the OQ-40 clearance.
6. `FloorSizeGrowsWithDepth` asserts floor 15 is larger than floor 1 for the same seed.
7. `GenerationIsDeterministic` asserts one grid hash for one seed, and the `bit-identity` job asserts it on three platforms.
8. `DescendAdvancesFloor` asserts that a descend at the stairwell generates floor n+1 from the same run seed, and that an ascend ends the run.

Review focus: determinism, content, test quality.

Check clause: none.

Gate: exit tests 1 to 8 pass, and the night sweep passes on one hundred thousand seeds.

> *In plain English:* this builds the dungeon floors from a random seed. Tests over huge numbers of seeds prove that every floor can be finished and that the stairs lead to the next one.

### PR-10: Projectile simulation

Scope:

- `content/projectiles/test-extremes.json`: a test-only set of projectile definitions (F-38, D-149): the slowest arc, the fastest flat shot, the longest lifetime, and the widest spread. Real weapons arrive in PR-24 and PR-43 to PR-46, and rerun these tests.
- `Core/Projectiles/ProjectileSimulation.cs`: a flat array of projectiles, fixed-step Euler integration, gravity scale, lifetime, spread, and swept collision against the grid and entity boxes (G-6). No spatial partition (D-109).
- `Core/Projectiles/ArcSolver.cs`: the launch angle for a target under gravity, with DetMath only. It reports an unreachable target.

Out of scope: damage, hit effects, enemy use, render trails.

Exit tests:

1. `NoTunnelThroughMinimumWall` fires the fastest definition at one-block walls from ten thousand random positions. It asserts a hit on the wall face and never a position beyond it.
2. `EveryProjectileTerminates` asserts every projectile ends by hit or by lifetime within its lifetime budget.
3. `ArcSolverReachesTarget` over one thousand reachable targets: the solved launch lands within one block of the target.
4. `ArcSolverReportsUnreachable` asserts a clear failure result for a target beyond range.
5. `ProjectilesAreDeterministic` replays a record with shots and asserts one state hash, and the `bit-identity` job asserts it on three platforms.
6. `EntityBoxHit` asserts a shot at a player box registers a hit on the box.

Review focus: determinism, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass over the test-only definitions.

> *In plain English:* bullets and arrows are real objects that fly, drop, and can miss. Tests prove a fast bullet never passes through a wall, even before any real weapon exists.

### PR-11: Bot harness (Tier 2)

Scope:

- `WhatYouCarry.Tools/BotRunner/`: a command that runs N runs for a policy over a seed range, headless, with no sleep between ticks (D-115, D-127). It writes one JSONL run log per run through the PR-4 logger.
- Policies in Core, each a few dozen lines (D-149): `RandomWalker` holds a random movement and jump for a random number of ticks, then picks again. `GreedyDescender` walks the reachability path to the stairwell and always descends.
- Run end states: `bottom` at floor 15, `softlock` after a tick budget with no floor progress, and `crash` on any exception. The log holds the exception.
- CI: the PR job runs one hundred seeds per policy. A scheduled night job on the self-hosted macOS runner runs five thousand seeds per policy (D-115, D-117). A `night-gate` job in the PR workflow reads the latest night result and fails when it is not a success.

Out of scope: the coward, full-clearer, and timer-tester policies (PR-16 to PR-18), Tier 3.

Exit tests:

1. `RunLogHasRequiredFields` asserts policy, seed, end state, floors reached, and ticks in every run log.
2. `SoftlockIsDetected` runs a fixture policy that stands still and asserts the `softlock` end state.
3. `CrashIsLogged` runs a fixture policy that throws and asserts the `crash` end state with the exception.
4. `GreedyDescenderReachesBottom` asserts the `bottom` end state on one hundred seeds.
5. The PR job completes two hundred runs with zero crashes.
6. The night job completes ten thousand runs with zero crashes and zero softlocks.
7. `NightGateFailsOnRedNight` asserts that the gate job fails on a fixture night result of failure.

Review focus: determinism, errors, input and CI boundaries, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* simple robots play thousands of runs every night without graphics. They find crashes and dead ends before a person ever sees them, and a bad night stops the next merge.

### M-1: CI wall time per PR

Procedure: after PR-3 and each later Phase 1 PR, read the duration of each CI job per platform from the workflow run. Record ten PRs in a table in this file. If any PR job exceeds ten minutes, file a question on the seed counts in D-116.

### M-2: Night sweep wall time

Procedure: after PR-11, read the night job duration for seven nights. Record them in a table in this file. If a night exceeds six hours, file a question on the run counts in D-115 and D-116.

## 5. Sequence

One person owns the program. Items run one at a time in this order. Each PR opens only after the one before it merges.

1. Owner: receive the SSD and move the checkout to it (D-145).
2. Owner: answer OQ-2, OQ-16, OQ-31, OQ-32.
3. PR-1.
4. PR-2.
5. Owner: answer OQ-33, OQ-34, OQ-35.
6. PR-3.
7. PR-4.
8. Owner: answer OQ-42.
9. PR-5.
10. Owner: answer OQ-36, OQ-37.
11. PR-6.
12. Owner: answer OQ-38, OQ-39.
13. PR-7.
14. PR-8.
15. Owner: answer OQ-12, OQ-40, OQ-41.
16. PR-9.
17. PR-10.
18. PR-11.
19. M-1 table complete. M-2 table complete.
20. **← GATE 1 (foundation).** Every exit test in this file passes. The bit-identity job, `dotnet test`, and the night sweep are green. The owner signs the gate in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 1. Each names the PR it blocks.

- OQ-2: the .NET version. Blocks PR-1.
- OQ-16: the harness attribution option. Blocks PR-1.
- OQ-31: the self-hosted macOS runner. Blocks PR-1.
- OQ-32: branch protection on `main`. Recommended before PR-1 merges.
- OQ-33: the RNG algorithm. Blocks PR-3.
- OQ-34: the state hash method. Blocks PR-3.
- OQ-35: the DetMath accuracy target. Blocks PR-3.
- OQ-36: the intent record layout. Blocks PR-6.
- OQ-37: the run record file layout and the content hash. Blocks PR-5 and PR-6.
- OQ-38: the voxel grid limits. Blocks PR-7 and PR-9.
- OQ-39: the player box and jump height. Blocks PR-7.
- OQ-40: the corridor cross-section. Blocks PR-8 and PR-9.
- OQ-41: the difficulty budget definition. Blocks PR-9.
- OQ-42: the content validator form. Blocks PR-5.
- OQ-12: the biome. Blocks PR-9.
