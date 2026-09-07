# Decisions

Status: active register. Owner: Nate. Started 2026-09-06. Written in ASD-STE100.

This file records each owner decision. Each decision has an id (D-#). The numbers never change. A reversed decision stays in this file with a dated note in the "Effect" column. The design doc (`docs/design.md`) is built from these decisions.

How to read this file:

- The "Decision" column gives the answer.
- The "Effect" column gives what the decision changes, binds, or supersedes.
- A date in the "Effect" column marks a later revision.

## Game design

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-1 | 2026-09-06 | Project purpose | Commercial Steam release. | Steam features, audio, UI polish, controller support, and notarization are in scope. |
| D-2 | 2026-09-06 | Banked gear in runs | The player selects a loadout from the bank. Gear that enters the dungeon is lost on death. | Needs a hub and a loadout screen. D-34 gives the amulet exception. |
| D-3 | 2026-09-06 | Ascension result | The run ends. The next run starts at floor 1. | Early floors must be short. |
| D-4 | 2026-09-06 | Run length | 30 to 45 minutes to floor 15. | Two to three minutes per floor. |
| D-5 | 2026-09-06 | Dungeon bottom | Floor 15 is the end of v1. An endless mode comes after v1. | The floor-15 boss is the final boss. |
| D-6 | 2026-09-06 | Floor bosses | Bosses only on floors 5, 10, and 15. Other floors end at a stairwell. | Replaces the v1 cadence table. v1 has three bosses. |
| D-7 | 2026-09-06 | World theme | Fantasy with black-powder guns. | Guns are muzzle-loaders. Reload time is a weapon stat. |
| D-8 | 2026-09-06 | Tone | Dark with dry humor. | A consistent voice for text. |
| D-9 | 2026-09-06 | Hub | A small physical hub. Equipment is modeled on the player body. | The model loader needs armor overlays. |
| D-10 | 2026-09-06 | Suspend | Suspend at stairwells only. | Superseded by D-97 on 2026-09-06. |
| D-11 | 2026-09-06 | Name | "What You Carry". | Replaces "Descent" in every file. |
| D-12 | 2026-09-06 | Co-op | Solo forever. No network code. | Pillar 3 stands. |
| D-13 | 2026-09-06 | Camera | Over-the-shoulder. The player controls the camera. | Procgen must guarantee a minimum corridor width. |
| D-14 | 2026-09-06 | Aim | Free aim. Controller aim assist. | Aim assist is a Core function (D-77). |
| D-15 | 2026-09-06 | Input | Keyboard and mouse, controller, and a Steam Deck verified target. | The Deck is the performance floor. Each UI screen must work without a mouse. |
| D-16 | 2026-09-06 | Visible loot | Enemies wear and wield what they drop. | Loot tables are enemy loadouts. |
| D-17 | 2026-09-06 | Character | One character. Gear is identity. | No classes. |
| D-18 | 2026-09-06 | Equipment slots | Head, chest, legs, feet, amulet, and shield are modeled. Two ring slots are not modeled. | Shield added by D-26. Rings are the one exception to the model rule. |
| D-19 | 2026-09-06 | Inventory | Equipped gear plus a small visible satchel. | Satchel slots compete: loot, potions, spare weapons, found amulets. |
| D-20 | 2026-09-06 | Weapon slots | One main weapon at a time. | Reload is exposure. Dodge is the counter. |
| D-21 | 2026-09-06 | Weapon swap | Spare weapons ride in the satchel. A swap is slow and cannot be canceled. | Swap time is tuned against reload time. |
| D-22 | 2026-09-06 | Throwables | Consumables in the satchel. A quick slot uses them with the main weapon equipped. | The thrown weapon class shrinks to wielded exotics. |
| D-23 | 2026-09-06 | Armor effect | Damage reduction plus weight. Weight slows movement and dodge recovery. | Two stats per armor piece. |
| D-24 | 2026-09-06 | Health | Potions in the satchel. No free heal. Health carries across floors. | Low health is an ascend signal. |
| D-25 | 2026-09-06 | Lethality | Fast and lethal. A few hits kill. | Telegraph readability moves up in priority. |
| D-26 | 2026-09-06 | Defense | Dodge, plus block with a shield. A shield needs a one-handed melee weapon. | Weapons have a handedness property. |
| D-27 | 2026-09-06 | Movement | Dodge roll, sprint, and jump. | Procgen, pathfinding, and Core collision are three-dimensional. |
| D-28 | 2026-09-06 | Stamina | None. Dodge has a cooldown. Armor weight extends the cooldown. | Sprint is free. |
| D-29 | 2026-09-06 | Block rules | Only shields block. Two-handed melee swings cannot be interrupted (hyper-armor). | A stagger system exists for the player. |
| D-30 | 2026-09-06 | Enemy shots | The same projectile simulation and rules as the player. | Enemy AI leads targets and reloads. |
| D-31 | 2026-09-06 | Enemy roster | Humanoid enemies use the player gear pool. Monsters have their own attacks. | Two enemy archetypes. |
| D-32 | 2026-09-06 | Self-damage | Full self-damage from the player's own bombs. | The first bomb needs a clear tell. |
| D-33 | 2026-09-06 | Magic | Mana is a resource for exotic weapons only. | Three resource models: reload, mana, cooldown. |
| D-34 | 2026-09-06 | Amulet | Every player always has an amulet. It is never lost. The tree upgrades it and assigns its ability. | Key feature. Exception to D-2. |
| D-35 | 2026-09-06 | Proficiency | Tree proficiencies only buff. No penalty for an unskilled class. | Found loot is good news for any build. |
| D-36 | 2026-09-06 | Numbers | Damage numbers always on. Bosses show a health bar with a number. | Style must not hide silhouettes. |
| D-37 | 2026-09-06 | Found amulets | Found amulets unlock skill orbs in the tree. A death forfeits the unlock. | A second reason to descend. |
| D-38 | 2026-09-06 | Amulet cost | Amulet abilities use cooldowns. | Mana stays with exotic weapons. |
| D-39 | 2026-09-06 | Amulet shape | One active ability plus one passive, assigned in the tree. | Two assignment screens. |
| D-40 | 2026-09-06 | Ammunition | Infinite ammunition. Reload and draw time are the only cost. | Replaces design doc v1 section 2.5. |
| D-41 | 2026-09-06 | Amulet carry | Found amulets are satchel items. | An unlock competes for satchel space. |
| D-42 | 2026-09-06 | Bows and guns | Bows have small area damage and a faster draw. Guns have single-target burst and a slow reload. | Arrows need an area mechanic. |
| D-43 | 2026-09-06 | Mana regeneration | Slow regeneration over time. Potions restore mana. | Exotic rhythm is "wait". |
| D-44 | 2026-09-06 | Skill points | Earned per kill with a boss bonus. Each floor has a time limit. | The timer is a new system. |
| D-45 | 2026-09-06 | Timer expiry | An unkillable hunter spawns and spawns escalate. The hunter accelerates until escape is impossible. | One hunter entity plus an escalation spawner. |
| D-46 | 2026-09-06 | Timer shape | A visible countdown. Deeper floors get more time. | Per-floor time in data. |
| D-47 | 2026-09-06 | Item rolls | Random affixes on drops. | Binds D-49. |
| D-48 | 2026-09-06 | Depth reward | Item tiers by depth band, plus a rare chance of a higher tier anywhere. | A property test checks the band distribution. |
| D-49 | 2026-09-06 | Affix visibility | Enemies show rarity color only. Enemies use their affixes. | Affixes are behaviors for any wielder. |
| D-50 | 2026-09-06 | Ascension | Ascend at every stairwell at no cost. The restart at floor 1 is the cost. | Reverses design doc v1 section 4.2. |
| D-51 | 2026-09-06 | Respec | Free at the hub. | Build identity comes from gear. |
| D-52 | 2026-09-06 | Death payout | The retained share of run skill points scales with the depth reached. | The curve is the sensitive number. |
| D-53 | 2026-09-06 | Modifiers | None in v1. Depth is the only difficulty dial. | Heat modifiers belong to the endless mode. |
| D-54 | 2026-09-06 | Tree shape | Several shallow branches. Found-amulet orbs gate the branch tips. | The tree UI must work on a controller. |
| D-55 | 2026-09-06 | Rings | Pure affix carriers. Not modeled. Lost on death. | Enemies use ring affixes. |
| D-56 | 2026-09-06 | v1 scope | Small: 15 floors, one biome, 3 bosses, about 12 weapons, about 8 enemy families, about 10 affixes, one hunter. | Early Access size. |
| D-57 | 2026-09-06 | First playable | One floor, one sword, one enemy family, the timer, the hunter, and a stairwell. No economy. | Feel before economy. |
| D-58 | 2026-09-06 | Timeline | About 12 months to the first public release. | Revised 2026-09-06 from "no deadline". |
| D-59 | 2026-09-06 | Look reference | Minecraft Dungeons light and material discipline, pushed darker, with torchlight. | Beat the clone comparison on silhouette and mood. |
| D-60 | 2026-09-06 | Feel reference | Hunt: Showdown gunplay. Risk of Rain 2 movement and camera. | Only the gunplay transfers from Hunt. |

## Technology

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-61 | 2026-09-06 | Godot version | Pin the latest stable 4.x at scaffold time. Upgrade only by a decision entry with the determinism CI green. | A fixed API surface. |
| D-62 | 2026-09-06 | .NET version | The newest LTS release that the pinned Godot supports. | |
| D-63 | 2026-09-06 | Editor use | The Godot editor is never required. C# builds the scenes. A few small text scenes exist for the hub and UI roots. | Every workflow runs from the command line. |
| D-64 | 2026-09-06 | GDScript | Banned. C# only, tools included. | |
| D-65 | 2026-09-06 | Tool language | C# only for tools. | Replaces the Python texture generator. See D-130. |
| D-66 | 2026-09-06 | Test stack | xUnit. Property tests are seed loops with explicit assertions. Each failure names its seed. | No shrink library. |
| D-67 | 2026-09-06 | Lint | .NET analyzers, dotnet format, and a C# lint tool that scans Core syntax trees for banned symbols. | Runs in the PR gate. |
| D-68 | 2026-09-06 | Null and log | Nullable reference types on, warnings as errors. A hand-written JSONL logger with the mandatory fields. | |
| D-69 | 2026-09-06 | Determinism | Cross-platform bit-identity from day one. | DetMath, lint, and CI before gameplay. |
| D-70 | 2026-09-06 | Precision | float in Core. | Matches Godot vector types. |
| D-71 | 2026-09-06 | CI matrix | Linux x64, macOS arm64, and Windows x64 on every PR. | Needs a macOS arm64 runner (D-100). |
| D-72 | 2026-09-06 | Threads | The simulation is single-threaded. One worker generates the next floor. | The worker is pure and seeded. |
| D-73 | 2026-09-06 | Tick rate | 60 Hz fixed. The Game layer interpolates. | |
| D-74 | 2026-09-06 | Intent | One fixed-size record per tick. | The aim field is replaced by D-77. |
| D-75 | 2026-09-06 | Camera home | The camera is a Core system. | Camera collision is deterministic. |
| D-76 | 2026-09-06 | AI home | All AI and pathfinding in Core, on the voxel grid. Godot navigation is never used. | The pathfinder understands jumps and drops. |
| D-77 | 2026-09-06 | Look input | The intent carries quantized yaw and pitch deltas, movement, and buttons. Core derives the aim ray and applies aim assist. | Sensitivity applies in the Game layer before quantization. |
| D-78 | 2026-09-06 | World | A voxel grid of one-meter cubes. | Procgen writes blocks. Greedy meshing renders them. |
| D-79 | 2026-09-06 | Destruction | Props only. Walls are permanent. | No re-mesh mid-floor. |
| D-80 | 2026-09-06 | Godot physics | Cosmetic only. Core owns all gameplay collision. | |
| D-81 | 2026-09-06 | Lighting | Ambient plus a small budget of dynamic point lights. No shadow maps. Vertex ambient occlusion. | Deck safe. |
| D-82 | 2026-09-06 | Proportions | A custom proportion set with one shared base body. | Armor overlays are shared. |
| D-83 | 2026-09-06 | Mojang | Avoid the tells. Keep the cuboid style. | A style rule in the art spec. |
| D-84 | 2026-09-06 | Textures | 32 or 64 px faces. | Pinned by D-85. |
| D-85 | 2026-09-06 | Atlas pin | 32 px faces. An own palette of about 32 colors, weighted dark. | The palette needs the owner's eye. |
| D-86 | 2026-09-06 | Blockbench | Review only. Every fix goes through the generator or the model JSON. | The QA gate is the source of truth. |
| D-87 | 2026-09-06 | Animation | JSON keyframes per bone, next to the model. Locomotion is procedural. Gameplay time values live in Core weapon data. | A test checks that both files agree. |
| D-88 | 2026-09-06 | Camera walls | The camera collides in Core. The Game layer fades walls between the camera and the player. | The mesher needs per-block visibility. |
| D-89 | 2026-09-06 | Audio | All generated, music included. | Music quality is a register risk. |
| D-90 | 2026-09-06 | UI | Godot Control nodes built in C#. Controller navigation and Deck screen sizes from the first screen. | |
| D-91 | 2026-09-06 | Data format | JSON for all content. No .tres files. | One loader, one schema set. |
| D-92 | 2026-09-06 | Schemas | One schema per content type. Validate at load and in tests. An absent field is an error. | |
| D-93 | 2026-09-06 | Audio source | Procedural C# synthesis from parameter files and a sequencer format. | |
| D-94 | 2026-09-06 | Save format | Three JSON files with schema versions and migrations from v1. Atomic writes. | Revised by D-152 on 2026-09-07: one profile file plus one run record. |
| D-95 | 2026-09-06 | Mid-floor exit | Resume at floor start. | Superseded by D-97 on 2026-09-06. |
| D-96 | 2026-09-06 | Steamworks | Achievements and cloud saves at launch. | A dependency entry. Cloud conflicts are explicit. |
| D-97 | 2026-09-06 | Resume | Resume by replay of the recorded intent stream, to five seconds before the exit tick. A repeat crash falls back to floor start. | Supersedes D-10 and D-95. Suspend works anywhere. |
| D-98 | 2026-09-06 | Localization | English only. All strings in an ID-keyed table from day one. | The lint tool forbids inline strings. |
| D-99 | 2026-09-06 | Mods | Unsupported, not prevented. | |
| D-100 | 2026-09-06 | CI runners | GitHub-hosted Linux and Windows. The Mac Mini is a self-hosted macOS arm64 runner. | The Mac Mini stays on. |

## Process

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-101 | 2026-09-06 | Review record | A reviews directory with one file per PR. | Exempt from D-137. |
| D-102 | 2026-09-06 | Merge | The owner merges every PR. | |
| D-103 | 2026-09-06 | Autonomy | The owner starts every session. No scheduled agents. | See D-117. |
| D-104 | 2026-09-06 | Owner role | Owner decisions, a playtest at each milestone, and spot checks of code. | |
| D-105 | 2026-09-06 | Harness host | Locally on the Mac Mini. | An external SSD is mandatory. |
| D-106 | 2026-09-06 | Repository | Private until launch. | |
| D-107 | 2026-09-06 | Budget | Near full time. Generous tokens. | Twelve months is conservative. |
| D-108 | 2026-09-06 | Namespace | WhatYouCarry.Core, WhatYouCarry.Game, WhatYouCarry.Tools, WhatYouCarry.Tests. | |
| D-109 | 2026-09-06 | Optimized | Not wasteful. Tune only on measurement. | Concrete rules in the standards. |
| D-110 | 2026-09-06 | Helpers | Allowed, one level deep. | The reviewer checks depth. |
| D-111 | 2026-09-06 | Abstraction | Two concrete cases before any abstraction. | Single-instance systems stay concrete. |
| D-112 | 2026-09-06 | Assertions | On in shipped builds. A failure logs a full report and continues where safe. | A report flow in the UI. |
| D-113 | 2026-09-06 | Hub errors | Outside a run, errors carry save versions, screen, action, and file paths. | Two context schemas. |
| D-114 | 2026-09-06 | Game tests | A headless scripted smoke session per PR, plus contract tests for content. | Headless Godot in CI. D-149: the smoke grows with the systems. |
| D-115 | 2026-09-06 | Bot cadence | A few hundred bot runs per PR. Ten thousand each night. A night failure blocks the next merge. | |
| D-116 | 2026-09-06 | Seed sweeps | Thousands of seeds per PR. One hundred thousand each night. | |
| D-117 | 2026-09-06 | Night jobs | Scheduled tests are allowed. Scheduled agents are not. | Consistent with D-103. |
| D-118 | 2026-09-06 | Doc cadence | The handoff every session. Other docs when relevant. The PR gate needs an explicit "no change needed" line per doc. | D-146 changes the handoff from a rewrite to an added entry. |
| D-119 | 2026-09-06 | Tenet order | Documentation, then no silent failures, then tests, then cross-provider review, then simplicity. The attribution tenet is absolute. | |
| D-120 | 2026-09-06 | Doc layout | All artifacts in docs. The v1 design doc moves to docs/archive. | Case revised by D-129. Shape revised by D-132. D-144 adds `docs/questions.md`. |
| D-121 | 2026-09-06 | Session | One harness invocation, one PR, one handoff rewrite. | D-146: one handoff entry, not a rewrite. |
| D-122 | 2026-09-06 | Agent files | CLAUDE.md and AGENTS.md are identical pointer files. A test asserts equality. | |
| D-123 | 2026-09-06 | Decision granularity | Record anything a fresh session could plausibly redo differently. | |
| D-124 | 2026-09-06 | Self-resolve | Never. Every open question belongs to the owner. A session stops and files the question. | Drain the questions file before each session. |
| D-125 | 2026-09-06 | Doc growth | The handoff is replaced each session. Decisions split per area with an index of current state. | D-146 keeps 10 sessions in the handoff. D-141 sets the split threshold. |
| D-126 | 2026-09-06 | Git | Trunk. Short branches. Squash merge by the owner. Conventional commit prefixes. No trailers. | |
| D-127 | 2026-09-06 | Bot policy | A small family of simple deterministic policies. | Each policy is named in the run log. D-149: each policy lands with the system it exercises. |
| D-128 | 2026-09-06 | LLM play | Weekly on main, plus every economy PR. | Needs the socket and a compact state format. |
| D-129 | 2026-09-07 | File case | Lowercase file names in docs. | Revises D-120. |
| D-130 | 2026-09-07 | STE checker | Port the checker to C#. | A tool project. |
| D-131 | 2026-09-07 | Skills | Create ste-writing and design-doc-style in .claude/skills, adapted to this project. | |
| D-132 | 2026-09-07 | Doc shape | One combined docs/design.md per the template. Focused roadmaps are separate linked files. | Revises D-120. |
| D-133 | 2026-09-07 | Vision tier | Screenshots plus the intent socket. Milestone only. | A screenshot hook in the Game layer. |
| D-134 | 2026-09-07 | Playtests | The owner from the first playable. Friends from the hub loop. | |
| D-135 | 2026-09-07 | Pose check | No box interpenetration at animation extremes. Armor overlays enclose their limb. | D-149: the check lands as PR-57 before any armor PR. |
| D-136 | 2026-09-07 | Milestones | Five gated milestones. Each is a playable build with a written exit test. | Revised by D-150 on 2026-09-07: Gate 1 is a foundation gate. |
| D-140 | 2026-09-07 | Timer pauses | The timer pauses at the stairwell choice. The timer runs in boss fights. | Boss floors need a larger time budget in data. |
| D-141 | 2026-09-07 | Decisions file | One file until it exceeds about 300 rows. Then split per area and make this file the index. | The D-125 mechanism. |
| D-142 | 2026-09-07 | Prerequisites | A GitHub repository exists. An Apple Developer account, an external SSD, and a Steam partner account do not exist yet. | Three purchases before Phase 5. The SSD comes before Phase 1. D-145: the SSD arrives 2026-09-08. |
| D-143 | 2026-09-07 | Interview close | No more topics before v2. The design doc v2 is written from D-1 to D-143. | The gaps that remain become open questions in the design doc. |
| D-144 | 2026-09-07 | Questions file | A separate `docs/questions.md` holds the open questions register. The design doc section 9 links to it. | Resolves OQ-24. Revises the D-120 file list. |
| D-145 | 2026-09-07 | External SSD | Ordered. Arrives 2026-09-08. | Updates D-142 and F-25. |
| D-146 | 2026-09-07 | Handoff history | `docs/session-handoff.md` keeps the last 10 sessions, newest first. A new session adds an entry. The oldest entry beyond 10 moves to `docs/session-handoff-archive.md`. | Revises D-118, D-121, and D-125. |
| D-147 | 2026-09-07 | Audit first | Address every finding of the 2026-09-07 repository audit before the focused roadmaps start. | Sequence step 0. |
| D-148 | 2026-09-07 | Gate bootstrap | PR-1 adds the three-platform build-and-test workflow. A check that does not exist yet is named in the PR with the PR that creates it. The PR that creates a check must pass it. | Resolves audit R-1. Only PR-2 and PR-3 use the clause. |
| D-149 | 2026-09-07 | Gate prerequisites | Four fixes: a Core player box in PR-7 and the stairwell transition in PR-9, with PR-11 limited to two policies, and later PRs add theirs. The PR-12 smoke is boot, run, move, quit, extended in PR-18. A new PR-57, asset QA v1, follows PR-13. PR-32 moves before PR-27. | Resolves audit R-2. |
| D-150 | 2026-09-07 | Gate 1 | Gate 1 is a foundation gate with a written exit test and no playtest. Gates 2 to 5 are playable builds with the owner's sign-off. | Resolves audit R-7. Revises D-136. |
| D-151 | 2026-09-07 | Run record | The record header carries a format version, a simulation version constant, a content hash, the seed, and an immutable initial state: loadout items with rolls, tree state, amulet assignment. Replay ignores the live bank and tree. Resume is exact on a match, and floor start with a notice on a mismatch. | Resolves audit R-3 and OQ-26. Binds PR-6, PR-31. |
| D-152 | 2026-09-07 | Save protocol | One profile file holds the tree, the bank, and the suspended-run pointer, with a schema version and a generation number, written atomically. The run record is a separate append-only file with checksummed tick frames. One profile write commits ascension or death. A run id makes completion idempotent. | Resolves audit R-4 and OQ-27. Revises D-94. Binds PR-31. |
| D-153 | 2026-09-07 | Basic kit | The loadout screen always offers a tier-0 sword with no affixes at no cost. It is not a bank item. The first run uses it by default. The tree unlocks the amulet's first orb, a basic active, from the start. | Resolves audit R-5 and OQ-28. Binds PR-28, PR-30. |
| D-154 | 2026-09-07 | Economy test | M-5 uses matched trials in simulated time: one thousand fixed seeds, the basic kit and an empty tree, ascend and die policies at every depth plus shallow-repeat, full-clear, and early-death, simulated ticks over 60 plus a sixty-second hub cost per run start, and a bootstrap 95 percent interval. Pass: at every depth the ascend lower bound exceeds the die upper bound, and deep ascend beats shallow repeat. | Resolves audit R-6 and OQ-29. Binds PR-27, M-5. |
| D-155 | 2026-09-07 | Project skill location | All project skills live in `.claude/skills/`. Every new project skill must use that directory. Both agent files state the path and require direct access to each required `SKILL.md`. | Extends D-131 to all project skills. Applies even when a skill is absent from the automatic skill list. |
| D-156 | 2026-09-07 | Initial commit | Reset HEAD to `main`. Commit every file as "initial commit" and push to `origin/main`. Roadmap work happens on the branch `docs/roadmaps`. | Resolves OQ-30 and F-37. Adds `.gitignore` and `.gitattributes`. |

## Rules set during the interview

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-137 | 2026-09-06 | Attribution tenet | No work is attributed to an agent, harness, or model in code, the game, commits, PR descriptions, or GitHub comments. Exemptions: the session-handoff author field ("Claude Code" or "Codex") and the reviews directory. | Absolute. Not ranked. |
| D-138 | 2026-09-07 | Push-back rule | Ask the moment a question appears. Push back with evidence. Quote both statements on a conflict. Record each answer in this file. | |
| D-139 | 2026-09-07 | Text rules | ASD-STE100 for every doc, skill, and agent file. The design doc follows the design-doc-style template. | Load the ste-writing skill first. |
