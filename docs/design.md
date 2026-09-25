# What You Carry: Design and Roadmap

Status: **design document v2, pre-production.** This file supersedes `docs/archive/design-v1-2026-09-06.md`, the "Descent" document. Its source is the decision register `docs/decisions.md`, entries D-1 onward. The owner recorded those decisions in an interview on 2026-09-06 and 2026-09-07. Nothing in this file is code. Each plan item ships as one pull request.

External facts, verified 2026-09-07:

- Godot 4.7.2 is the current release for both editions, standard and .NET, dated 2026-08-18. Source: [the Godot macOS download page](https://godotengine.org/download/macos/), fetched 2026-09-07. Refuted 2026-09-07 (audit R-8, F-35): the earlier text here said "stable 4.7.1 (2026-07-14), .NET build 4.7.2". That split came from a search summary, not from the source page.
- Metal is the default render driver on Apple Silicon since Godot 4.4. Intel Macs use MoltenVK. Source: godotengine.org, "Dev snapshot: Godot 4.4 dev 1".
- The Steam Direct fee is 100 USD per app. Steam credits it after 1,000 USD adjusted gross revenue. Source: partner.steamgames.com, "Steam Direct Fee".
- The Apple Developer Program fee is 99 USD per year. Source: Apple Developer Program pages, via search summaries.
- GitHub treats a `neutral` or `skipped` check conclusion as a success. A skipped job does not stop a merge, even as a required check. Source: docs.github.com, "About status checks", verified 2026-09-07.

Verified 2026-09-07: the .NET LTS pin is .NET 10 LTS (D-173). Godot 4.7 accepts .NET 8 or later.

2026-09-07 correction pass: v2 refuted or superseded fourteen v1 claims. Each stays in the register (F-1 to F-14) with its correction and date. The v1 file stays in the archive unchanged.

2026-09-07 PR #1 review pass: the cross-provider review in `docs/reviews/pr-1.md` raised three findings. The response is `docs/reviews/pr-1-response.md`. The register records them as F-47 to F-49, and D-176 to D-178 resolve them.

2026-09-07 audit pass: the repository audit in `docs/reviews/2026-09-07-repository-audit.md` found nine defects, R-1 to R-9. All nine have merit. The register records them as F-28 to F-36, and D-148 to D-154 resolve them. The response file `docs/reviews/2026-09-07-repository-audit-response.md` gives each disposition. F-37 records a repository state the audit session left behind.

2026-09-07 settings correction pass: the Claude Code startup dialog rejected `.claude/settings.json`. D-172 set `attribution.commit` and `attribution.pr` to booleans, and the schema requires strings. D-175 revises D-172 and sets empty strings. F-15 records the refuted claim.

2026-09-08 STE pass: the first run of the PR-2 checker found 77 sentences in the documents that broke a rule, most in the passive voice. PR-2 rewrote each one with the same meaning, and the checker now runs on every PR.

Text rules: this file follows ASD-STE100 (D-139). Tables are exempt from sentence-length counts.

## 1. Thesis

What You Carry is a solo third-person dungeon crawler for Steam. The player descends a procedural dungeon one floor at a time. At each stairwell the player decides: descend or ascend. Ascension banks the gear. Death loses the gear that the player carried in and found on the way. The dungeon is content. The decision is the game.

The plan puts foundations first, because every later system depends on them: a deterministic core, a replay format, and the document protocol. A first playable floor comes second, because only a person can judge feel. The economy comes third, because it needs the feel to be right. Content and the Steam release come last. Five gated phases hold that order (D-136 as revised by D-150).

## 2. Lessons learned (carry into every PR)

From the connector-syncer program:

1. **L-1. One concern per PR.** A bundled PR froze behind one review objection (connector-syncer PR #724).
2. **L-2. Commit the evidence.** An uncommitted A/B script hid its gaps from reviewers (connector-syncer PR #724).
3. **L-3. Audit design claims before you trust them.** A first draft carried refutable zero-regression claims (connector-syncer lesson 6). In this project the v1 document carried four silent contradictions (F-1 to F-4).
4. **L-4. Verify a platform claim before you build on it.** Nobody enforced a documented storage cap (connector-syncer lesson 7). In this project the v1 claim "both target machines are overpowered" failed when the Steam Deck became a target (F-3).
5. **L-5. A plan entry is a claim, and it ages.** A fix shape survived three weeks unexamined because it read as one `if` statement (connector-syncer lesson 8).

From the 2026-09-06 interview:

6. **L-6. A new decision can remove the premise of an old one.** Re-check the old one. D-44, D-48, and D-52 removed the premise behind the v1 rejection of free ascension. D-50 reversed it. D-97 superseded D-10 and D-95 in the same way.
7. **L-7. Ask about what the document does not say.** The v1 document never said whether banked gear enters the dungeon (F-5). The answer, D-2, reshaped the economy.
8. **L-8. Two answers can conflict when neither one is wrong.** Quote both and settle it at once. D-74 and D-75 conflicted on the aim input. D-77 settled it. D-47 and D-16 conflicted on visible loot. D-49 settled it.
9. **L-9. An exploit hides in every convenience.** "Resume at floor start" (D-95) made a quit a free heal and a timer reset. D-97 closed it.
10. **L-10. One language, one format, one term.** A Python generator, `.tres` files, and a borrowed skill with another project's names all entered the plan. D-65, D-91, and D-131 removed them.

## 3. System map and system rules

### 3.1 System map

| Component | Project | Reads | Writes | Sensitivity |
|---|---|---|---|---|
| Simulation loop | Core | intent stream, seed | world state, run log | Total. Every replay and bot run depends on it |
| DetMath and RNG | Core | seed | numbers | Total. Cross-platform bit identity |
| Voxel world and collision | Core | procgen output | hits, positions | High |
| Procgen | Core | seed, floor number, floor templates | voxel grid, spawns, stairwell | High |
| Camera | Core | look deltas, world | aim ray | High. Aim depends on it |
| Projectiles | Core | weapon data | hits | High. Pillar 4 |
| Enemy AI and pathfinding | Core | world, player state | enemy actions | High |
| Items, affixes, loot | Core | item data, affix set | drops, enemy loadouts | High. The economy |
| Points, timer, hunter, death payout | Core | run events | skill points | Total. The design risk |
| Amulet and skill tree | Core | tree data, saves | abilities, unlocks | High |
| Persistence | Core | one profile file and a run record (D-152) | one profile file and a run record (D-152) | High. Corruption risk |
| Game layer | Game | Core state | screen, audio, intent | Medium. Cosmetic by design |
| Model loader and mesher | Game | voxel grid, model JSON | meshes | Medium. Performance |
| UI | Game | Core state, string table | screens | Medium. Controller and Deck |
| Audio synthesizer | Tools, Game | parameter files | sounds | Medium |
| Texture generator | Tools | palette, rules | atlas | Low |
| STE checker, lint tool, asset QA | Tools | source, docs, models | pass or fail | Gate |
| Bot harness | Tests | policies, seeds | run logs | High. Tier 2 |
| LLM play socket | Game, Core | state JSON | intents | Medium. Tier 3 |

### 3.2 Core loop

A run starts at floor 1 in a small hub (D-3, D-9). The player selects a loadout from the bank. Gear that enters the dungeon is at risk (D-2). The loadout screen always offers a basic kit: a tier-0 sword with no affixes, at no cost, never lost (D-153). Each floor ends at a stairwell. Floors 5, 10, and 15 end at a boss (D-6). Floor 15 is the end of v1 (D-5).

At each stairwell the player ascends or descends (D-50). The stairwell of floor 15, the deepest floor, offers the ascend alone (D-579). Ascension costs nothing. The run ends, and the next run starts at floor 1. The restart is the cost. The timer pauses at the stairwell (D-140). A full run to floor 15 takes 30 to 45 minutes (D-4).

### 3.3 Player

One character. Gear is identity (D-17). The camera is over-the-shoulder and the player controls it (D-13). Aim is free, with aim assist on a controller (D-14). The intent marks a controller aim on each tick, and the assist pulls the aim ray toward a target inside a cone (D-243, D-244). Core derives the camera on each tick, and its boom sweeps the grid along the line (D-245, D-246). Input targets are keyboard and mouse, controller, and the Steam Deck (D-15). The Deck is the performance floor and the readability floor.

Movement verbs are dodge roll, sprint, and jump (D-27). There is no stamina. Dodge has a cooldown, and armor weight extends it (D-28). A roll moves 3 meters in 18 ticks, and no hit lands during it (D-327, D-328). A roll needs the ground and dry feet, and it cancels a swing (D-329, D-337). Health carries across floors, and zero health ends the run as a death (D-322, D-335). Potions in the satchel are the only heal (D-24).

### 3.4 Equipment

Modeled slots: head, chest, legs, feet, amulet, shield (D-18). The two ring slots have no model (D-18, D-55). The player equips one main weapon at a time (D-20). Spare weapons ride in the satchel. A swap is slow, and the player cannot cancel it (D-21). The satchel is small and visible (D-19). Consumables in the satchel go to a quick slot (D-22).

Armor gives damage reduction plus weight. Weight slows movement and dodge recovery (D-23). A shield needs a one-handed melee weapon (D-26). Only shields block. Nothing interrupts a two-handed melee swing (D-29). A stagger system exists for the player (F-21). Heavy armor resists stagger, and light armor does not (D-314). A stagger lasts 20 ticks, and a guard of 30 ticks after it stops a stunlock (D-326).

### 3.5 Combat

Fights are fast and lethal (D-25). The tier-0 sword sweeps an arc of 90 degrees at a reach of 1.6 meters, and the arc follows the look (D-324, D-325). A swing starts on a press, and the walk stays free during it (D-323). No hitscan exists. Every projectile is a simulated object with travel time, drop, and a lifetime. Enemies fire the same projectiles under the same rules (D-30). The player's own bombs deal full self-damage (D-32).

Ammunition is infinite. Reload and draw time are the only cost (D-40). Guns are muzzle-loaders with single-target burst and a slow reload (D-7, D-42). Bows have small area damage and a faster draw (D-42). Exotic weapons use mana. Mana regenerates slowly, and potions restore it (D-33, D-43). The feel references are Hunt: Showdown gunplay and Risk of Rain 2 movement (D-60).

Damage numbers are always on. Bosses show a health bar with a number (D-36). Numbers must not cover silhouettes (F-24).

### 3.6 Amulet and skill tree

Every player always has an amulet. The player never loses it (D-34). The tree upgrades it and assigns one active ability and one passive (D-39). The tree unlocks the first orb, a basic active, from the start (D-153). Amulet abilities use cooldowns (D-38). Found amulets are satchel items (D-41). On ascension a found amulet unlocks a skill orb in the tree. A death forfeits the unlock (D-37).

The tree has several shallow branches. Found-amulet orbs gate the branch tips (D-54). Proficiencies only buff (D-35). Respec is free at the hub (D-51). No difficulty modifiers exist in v1. Depth is the only dial (D-53).

### 3.7 Loot and items

Humanoid enemies wear and wield what they drop (D-16, D-31). Monsters have their own attacks. Items have random affixes (D-47). An enemy shows rarity color only, and it uses its affixes (D-49). Affixes are a fixed set of behaviors that any wielder gets. Item tiers rise by depth band, with a rare higher-tier chance anywhere (D-48). Rings are pure affix carriers (D-55).

### 3.8 Economy

Skill points come per kill, with a boss bonus (D-44). Each floor has a visible time limit. Deeper floors get more time (D-46). The timer runs in boss fights and pauses at the stairwell (D-140). When the timer expires, an unkillable hunter spawns and spawns escalate. The hunter accelerates until escape is impossible (D-45). The lengths per floor band and for boss floors are in D-407. The hunter is the Overseer (D-409), and its pace is in D-408. Waves of the floor families escalate after expiry (D-410).

Death keeps a share of the run's skill points. The share scales with the depth reached (D-52). Ascension must beat death in points per hour at every depth. This is the sensitive number. M-5 measures it with matched trials in simulated time (D-154). The trials use a fixed seed set, a fixed initial state, paired ascend and die policies at every depth, and a bootstrap confidence interval.

### 3.9 Hub and persistence

The hub is one small scene with a bank, a skill shrine, a loadout screen, and the descent entrance (D-9). One profile file holds the tree, the bank, and the suspended-run pointer, with a schema version, a generation number, and a migration path (D-94, D-152). The game writes it atomically. One profile write commits an ascension or a death. A run id in the profile makes completion idempotent.

A run record is a separate append-only file (D-151, D-152). Its header carries a format version, a simulation version constant, a content hash, the seed, and an immutable initial state. The initial state holds the loadout items with their rolls, the tree state, and the amulet assignment. The fixed 16-byte tick frames of D-162 follow, each with a CRC-32 (D-226). The loader truncates a torn tail. Replay ignores the live bank and tree.

Suspend works anywhere. Resume replays the record to five seconds before the exit tick when the simulation version and the content hash match (D-97, D-151). On a mismatch, or on a repeat crash, resume starts at floor start with a notice and a log line. Steam achievements and cloud saves ship at launch (D-96).

### 3.10 Enemies and bosses

The v1 scope is small (D-56): 15 floors, one biome, 3 bosses, about 12 weapons, about 8 enemy families, about 10 affixes, one hunter. Two enemy archetypes exist: humanoids that use the player gear pool, and monsters with their own attacks (D-31). All AI and pathfinding live in Core on the voxel grid (D-76).

The roster holds eight families, four humanoid and four monster (D-395). The humanoids are the scavenger, the company guard, the deep cultist, and the foreman revenant. The monsters are the cave crawler, the powder tick, the drowned miner, and the stone maw. Each family covers a band of floors, and no family flies, so the ground move rule of D-165 and D-345 serves every one. PR-16 ships the scavenger, and PR-36 to PR-42 add the other seven (D-396).

Each floor fills every chamber past the first with enemies for the weight of that chamber (D-167). The chamber of the player spawn takes none (D-398). An enemy holds its post until the player comes inside 20 meters with a clear line of sight (D-400). It returns to that post after 5 seconds with no sight.

### 3.11 World and art

The world theme is fantasy with black-powder guns (D-7). The v1 biome is a collapsed deep mine, and a blasting charge is a mining tool (D-210). The tone is dark with dry humor (D-8). The look reference is Minecraft Dungeons, pushed darker with torchlight (D-59). The world is a voxel grid of one-meter cubes (D-78). Each floor is a mine dig plan of galleries, drifts, chambers, and shafts (D-253). Props break, and walls are permanent (D-79).

Every floor is 64 by 20 by 64 blocks, with 5 to 9 chambers (D-343, D-344). A gallery is 7 blocks wide and 5 high, and a drift is 5 blocks wide and 4 high (D-341). A chamber is 5 to 8 blocks high. A tunnel changes height by a ramp or a shaft, and never by a one-block step (D-347). A ramp rises one block over two, three, or four blocks, and a body walks it with no jump (D-345, D-346). Some chambers hold a tier 2 blocks over the floor, and a ramp joins the tier to the floor (D-348 to D-350). A tier takes one of four shapes, and it covers 25 to 40 percent of the chamber floor (D-388, D-389). The generator draws the slope of the ramp of a tier from the slopes that fit beside it (D-390). A chamber too small for a tier and its ramp holds none (D-391). Every floor holds one tier when a chamber of it fits one (D-392). The ramp of a tier is 3 or 2 cells wide, the widest that fits (D-393). The dig routes a drift under a chamber, so a shaft of that chamber has a landing (D-394).

A ramp cell is one block id from 8 to 43, so the grid stays one byte per cell (D-164, D-367). On a ramp, the speed along the slope is the flat speed (D-362). A walk and a sprint stay on the slope on the way down, and a roll leaves it (D-363). A body does not slide on a ramp, and it jumps and rolls from a ramp as from flat ground (D-364 to D-366). The mesher draws every face of a ramp with the raw stone tile, so a ramp reads as the floor that it joins (D-368).

Models are cuboid, with a custom proportion set and one shared base body (D-82). Textures are 32 px faces on one atlas from an own palette of 117 colors (D-85, D-304, D-528, D-530). The palette is nine ramps of four colors from dark to light, with three fine shades between each pair of colors (D-304, D-528, D-530). A tool generates the atlas of 512 from the palette and the recipes (D-305, D-505, D-506). A recipe is a list of paint layers, with clustered grain and fine shades (D-507, D-527). A file next to each model names the recipe of each face (D-508). Every face has 32 texels per meter, a body face too (D-308). Each asset starts as a Meshy look reference, and the agent rebuilds it as boxes (D-496, D-509). The lighting budget is ambient plus a few dynamic point lights, no shadow maps, and vertex ambient occlusion (D-81). Animation is JSON keyframes per bone, and locomotion is procedural (D-87). The camera collides in Core and the Game layer fades walls (D-88). Avoid Minecraft tells (D-83).

A C# synthesizer generates all audio from parameter files, music included (D-89, D-93). Music quality is a register risk (F-18).

### 3.12 Architecture

Two projects hold the game: `WhatYouCarry.Core` and `WhatYouCarry.Game` (D-108). Core is a pure C# library with no engine dependency. It owns the simulation, collision, pathfinding, projectiles, camera, items, economy, and saves. Game is a thin Godot layer for render, audio, and input. Tools and Tests are separate projects. A fifth project, `WhatYouCarry.Assets`, holds the model reader, the animation reader, and the pose math, with no engine dependency (D-299). Game and Tools read a model through it. The atlas layout lives there too, because Game and the texture generator of Tools read the same tiles. Core uses the frame of Godot: right-handed, Y up, meters, and forward at yaw zero is minus Z (D-234). A body is a box that Core sweeps against the grid, one axis at a time (D-165). A contact stops one skin before a block face, and the edge of the grid is a wall (D-235, D-237).

Determinism rules (D-69 to D-73, D-77):

- Cross-platform bit identity from day one. CI asserts it on Linux x64, macOS arm64, and Windows x64.
- float in Core. DetMath replaces `System.Math` transcendentals.
- The simulation runs on one thread at 60 Hz. One worker generates the next floor.
- The intent is one fixed-size record per tick: quantized look deltas, movement, buttons. Core derives the aim ray.
- Godot physics and navigation never feed the simulation (D-80).

Stack rules (D-61 to D-68, D-90 to D-92, D-98):

- Pin Godot 4.7.2 .NET and .NET 10 LTS at scaffold time (D-173). Upgrade only by a decision entry.
- C# only. Do not use GDScript. Tools are C#.
- No step needs the editor. C# builds the scenes.
- JSON for all content, validated by a schema per type. An absent field is an error.
- xUnit. Property tests are seed loops. Each failure names its seed.
- Nullable on, warnings as errors. A hand-written JSONL logger.
- All strings that the player sees live in an ID-keyed table.

### 3.13 Test tiers

- Tier 1: property tests over seeds. The full count on `main`, one fifth of it on a pull request (D-480), and one hundred thousand each night (D-116).
- The seeds of each night: the fixed set stays the gate, and the UTC date selects a slice of one tenth past it (D-564, D-566). A slice failure fails the night, and each later night runs the failed seed until a night passes it (D-565, D-567).
- Tier 2: scripted bots. A few hundred runs per PR. Each night runs seeds 1 to 5000 and a slice of 500 for each of five policies (D-115, D-127, D-564, D-566).
- Tier 3: LLM play over a socket. Weekly on main, plus every economy PR (D-128).
- Tier 3b: an LLM reads the outlier run logs.
- Tier 4: vision play from screenshots. Milestone only (D-133).
- Game layer: a headless smoke session per PR, plus contract tests for content (D-114).
- Asset QA: budgets, pivots, UV coverage, box interpenetration at keyframe extremes, and file name case (D-135).
- Humans: the owner from the first playable, friends from the hub loop (D-134).

### 3.14 Process

Two harnesses work the repo: Claude Code and Codex (D-137). One session is one harness invocation, one PR, and one role (D-121, D-375). The PR carries its code, tests, registers, design and roadmap state, review record, and handoff entry. No PR exists only to record an earlier PR (D-375). Each PR has its own handoff entry (D-146). The owner starts every session, and an author session starts the reviewer session with `make codex-review` (D-103, D-511). The owner owns every open question (D-124). A PR with the green light merges itself by GitHub auto-merge after the owner confirms the merge summary of four questions and answers (D-102, D-516, D-524, D-533, D-552). The owner can still merge. A review uses the ChatGPT login alone, and never API pricing (D-523). Scheduled tests can run at night. Scheduled agents cannot (D-117). The other provider reviews every PR, and the review file lives in `docs/reviews/` (D-101). A finding that is open in three review rounds stops the fix loop, and the owner decides (D-513, D-514). A ruleset on `main` requires each gate check and resolved conversations (D-522). An automated reviewer, gitar, reviews every PR after a push, and the author answers every comment before the cross-provider review. Its pass is a gate (D-250, D-574, D-575). The heavy CI jobs skip a PR head that changes documents alone, and a push to `main` runs every job (D-473, D-474). The tests that read a document run on each head (D-476).

The document protocol (D-118, D-120, D-125, D-129, D-132):

- `docs/design.md`: this file. Update it when intent changes.
- `docs/decisions.md`: the decision register. One file until about 300 rows (D-141).
- `docs/questions.md`: the open questions register, OQ-1 onward (D-144).
- `docs/session-handoff.md`: the 10 newest sessions, newest first. A session reads the newest entry first (D-377). Each session adds an entry, and the `handoff-rotate` command moves older entries to `docs/session-handoff-archive.md` (D-146, D-379).
- `docs/reviews/`: one file per PR.
- `docs/roadmaps/`: focused roadmaps, linked from section 7.
- `CLAUDE.md` and `AGENTS.md`: identical pointer files (D-122).
- The PR description: the documents matrix, one line for each category (D-376).

The lifecycle of a PR (D-375, D-376). The `one-pr-one-session` skill holds the procedure. A PR cannot know its merge commit or its merge time, so its documents say "Done in PR #N" and "pending merge", and git holds the merge. The file `.claude/skills/one-pr-one-session/references/enforcement.md` holds the table of every rule and its enforcement: a machine, the agent, the owner, or not observable (D-383).

## 4. Cost model (what we pay, what we do not know)

What we pay:

- Owner time: near full time (D-107).
- Tokens: a generous budget on two harnesses (D-107). The amount per PR is unknown until M-4. The token audit of 2026-09-16 measured 10 sessions of each harness. The median input was 32.3 million tokens for a Claude Code session and 4.3 million for a Codex session, most of it from the cache. D-377 to D-382 act on the largest causes.
- CI: GitHub-hosted Linux x64 and Windows x64 minutes on every PR (D-100). Wall time per PR is unknown until M-1. F-109 measured it again on 2026-09-22: 18 to 21 minutes on the hosted legs of a code head. A push of documents alone skips the heavy jobs after a green head (D-474). A session runs no full suite for such a change either (D-491).
- The night: eight jobs on hosted Linux from PR-85, six of them for the sweeps. Standard hosted runners cost nothing for a public repository (D-572, billing page, read 2026-09-24).
- The Mac Mini as a self-hosted macOS arm64 runner: power, and RAM shared with the editor and the harness (D-100, D-105). It runs the macOS legs of each PR until PR-86 (D-573).
- Purchases that do not exist yet (D-142):
  - an external SSD before PR-1. It arrived 2026-09-07, and the checkout is on it (D-145, D-192).
  - an Apple Developer account at 99 USD per year before PR-51.
  - a Steam Direct fee of 100 USD before PR-52.
- Music and sound: zero license cost (D-93). The quality cost is unknown (F-18).

Measurements that answer the unknowns:

- M-1: CI wall time per PR, per platform.
- M-2: night sweep wall time on the Mac Mini.
- M-3: Steam Deck 99th percentile frame time on the one-floor build.
- M-4: tokens per PR from the harness usage reports.
- M-5: points per hour by bot policy, ascend versus die, at every depth.

## 5. Defect and finding register

Status: ✅ done (code merged, or "doc" for a document-only correction) · 🔧 planned (item listed) · ⚠ constraint (binds a pull request) · ❓ needs owner input · ⏸ out of scope · 🅿 parked.

| # | Finding | Date | Status |
|---|---|---|---|
| F-1 | v1 said content lives in "JSON or .tres". Core has no engine dependency and cannot read `.tres` | 2026-09-06 | ✅ D-91. Done in PR #17 (PR-5) |
| F-2 | v1 assumed greedy meshing but never chose a voxel world | 2026-09-06 | ✅ D-78. Done in PR #21 (PR-7) and PR #52 (PR-13) |
| F-3 | v1 said both target machines are overpowered. The Steam Deck (D-15) is not | 2026-09-06 | ⚠ Binds M-3 and every render PR |
| F-4 | v1 named Deep Rock Galactic as the non-hitscan reference. Most of its guns are hitscan | 2026-09-06 | ✅ doc. D-60 replaced it |
| F-5 | v1 never said whether banked gear enters the dungeon | 2026-09-06 | 🔧 D-2. Binds PR-30 |
| F-6 | v1 never said who simulates player collision, enemy AI, and pathfinding | 2026-09-06 | 🔧 D-76, D-80. Binds PR-7, PR-16 |
| F-7 | v1 had no audio plan | 2026-09-06 | 🔧 D-89, D-93. The effects are done in PR #87. Binds PR-50 for the music |
| F-8 | D-74 put a world-space aim in the intent. D-75 put the camera in Core. Both cannot hold | 2026-09-06 | ✅ D-77. Done in PR #19 (PR-6) and PR #23 (PR-8) |
| F-9 | D-95 "resume at floor start" made a quit a free heal and a timer reset | 2026-09-06 | 🔧 D-97. Binds PR-31 |
| F-10 | D-47 random affixes cannot show on an enemy, against D-16 | 2026-09-06 | 🔧 D-49. Binds PR-21, PR-26 |
| F-11 | v1 rejected free ascension as "no tension". D-44, D-48, D-52 removed that premise | 2026-09-06 | ⚠ D-50 reversed it. Binds M-5 |
| F-12 | v1 used Python for the texture generator, against one language | 2026-09-06 | ✅ D-65. Done in PR #56 (PR-14) |
| F-13 | v1 determinism matrix omitted Windows x64, the largest audience and the second dev machine | 2026-09-06 | ✅ D-71. Done in PR #12 (PR-3) |
| F-14 | v1 had no v1 scope in numbers | 2026-09-06 | ✅ doc. D-56 |
| F-15 | The harness default adds a co-author trailer to commits. D-137 forbids it | 2026-09-06 | 🔧 Refuted 2026-09-07: D-172 set booleans, the schema requires strings, and the file was ignored. D-175 revises D-172 and sets empty strings. Codex unverified. The PR-1 attribution scan binds every PR (D-176) |
| F-16 | The borrowed ste-writing skill carried another project's names and a Python checker | 2026-09-07 | ✅ doc (skills created). ✅ PR-2 holds the C# checker |
| F-17 | D-97 rewinds five seconds on resume. A quit undoes five seconds | 2026-09-07 | ⏸ Accepted by the owner |
| F-18 | All music is generated (D-93). Quality is unproven | 2026-09-07 | ❓ Owner ear at each phase gate. Binds PR-50 |
| F-19 | Night sweeps are scheduled jobs. D-103 forbids scheduled agents | 2026-09-06 | ✅ doc. D-117: tests may run on a schedule, agents may not |
| F-20 | Per-kill points (D-44) reward a full clear of every floor | 2026-09-06 | ⚠ The timer counters it. Binds M-5 |
| F-21 | Hyper-armor (D-29) needs a stagger system. The effect of armor weight on stagger is undecided | 2026-09-06 | ⚠ D-314 and D-316, 2026-09-12. Binds PR-15 and PR-22 |
| F-22 | The v1 build order put procgen before any render layer for weeks. D-57 wants a playable floor early | 2026-09-06 | 🔧 Phase 2 places the Game skeleton right after the Core foundations |
| F-23 | The name "Descent" had a trademark risk. "What You Carry" (D-11) has no trademark search yet | 2026-09-06 | ❓ OQ-17 |
| F-24 | Always-on numbers (D-36) and Deck 800p (D-15) strain Pillar 5 readability | 2026-09-06 | ⚠ Binds PR-19. Tier 4 checks it |
| F-25 | The SSD, the Apple account, and the Steam account do not exist (D-142) | 2026-09-07 | ⚠ The SSD arrived 2026-09-07 (D-192). Accounts bind PR-51, PR-52 |
| F-26 | v1 named `HANDOFF.md`, `DECISIONS.md`, and a five-file set in uppercase | 2026-09-07 | ✅ doc. D-129 lowercase, D-132 one design file |
| F-27 | v1 listed the harness as one unnamed system. Two providers exist | 2026-09-07 | ✅ doc. D-137 names both. Binds the review file format in PR-1 |
| F-28 | Audit R-1: the PR gate required the STE checker, the lint tool, and the bit-identity job before PR-2 and PR-3 created them | 2026-09-07 | ✅ D-148. PR-1 added the CI skeleton, and PR-2 added `ste-check`. The gate names an absent check with the PR that creates it. PR-3 added `det-lint` and the bit-identity job. Done in PR #12 (PR-3) |
| F-29 | Audit R-2: four gates needed systems or tools that arrived later: PR-11 bots, PR-12 smoke, PR-22 pose check, PR-27 Tier 3 | 2026-09-07 | 🔧 D-149. Binds PR-7, PR-9, PR-11, PR-12, PR-16, PR-17, PR-18, PR-57, PR-49, and the PR-32 position |
| F-30 | Audit R-3: seed plus intents did not determine a run. The loadout, tree, and amulet came from outside the seed, and code or content could change under a suspended run | 2026-09-07 | 🔧 D-151. Binds PR-6, PR-31 |
| F-31 | Audit R-4: three atomic files did not make one atomic save. A crash between writes could duplicate or lose a reward | 2026-09-07 | 🔧 D-152 revises D-94. Binds PR-31 |
| F-32 | Audit R-5: no rule gave the first loadout or a run after the bank emptied | 2026-09-07 | 🔧 D-153. Binds PR-28, PR-30 |
| F-33 | Audit R-6: the economy gate had no reproducible comparison. Fewer points in less time could win on rate | 2026-09-07 | 🔧 D-154. Binds PR-27, M-5 |
| F-34 | Audit R-7: D-136 called every milestone playable, and Phase 1 had no Game layer | 2026-09-07 | ✅ doc. D-150: Gate 1 is a foundation gate |
| F-35 | Audit R-8: the header split Godot into stable 4.7.1 and .NET 4.7.2. The source page lists both editions as 4.7.2 | 2026-09-07 | ✅ doc. Header corrected with the refuted text kept |
| F-36 | Audit R-9: section 8 put the palette answer before PR-1, and OQ-1 said it blocks PR-14 | 2026-09-07 | ✅ doc. OQ-1 blocks PR-14 only. Section 8 corrected |
| F-37 | HEAD pointed at the unborn branch `docs/repository-audit`. The first commit would have missed `main` (D-126) | 2026-09-07 | ✅ D-156. HEAD reset to `main` before the initial commit |
| F-38 | PR-10's gate named the full weapon roster, which does not exist until Phase 3 | 2026-09-07 | 🔧 A test-only definitions file under D-149. Binds PR-10, PR-24, PR-43 to PR-46 |
| F-39 | The macOS CI leg needs the Mac Mini registered as a self-hosted runner (D-100). No item listed that action | 2026-09-07 | ✅ D-157. The owner registered the runner `mac-mini-m4` on 2026-09-07 (D-192) |
| F-40 | D-88's effect note put wall fade in the mesher as per-block visibility. A shader test needs no mesher change | 2026-09-07 | ✅ D-292: the shader test, with no mesher change. Done in PR #52 (PR-13) |
| F-41 | No item said whether a Steam Deck unit exists for M-3, and D-15 makes the Deck the floor | 2026-09-07 | ✅ D-296: the owner owns a Steam Deck, and M-3 measures on it. Binds M-3 |
| F-42 | D-49 shows a rarity color on an enemy, but no decision names the rarity tiers | 2026-09-07 | ❓ OQ-52. Binds PR-21, PR-26 |
| F-43 | D-128 sets the Tier 3 cadence but not the model or the budget | 2026-09-07 | ❓ OQ-58. Binds PR-32 |
| F-44 | The Deck verification checklist is an external fact with no source or date in the plan | 2026-09-07 | ❓ OQ-66. Binds PR-54 |
| F-45 | D-152 changed the save files, and no item names which files cloud saves sync | 2026-09-07 | ❓ OQ-71. Binds PR-52 |
| F-46 | D-158 required branch protection, and GitHub returned 403: the feature needs Pro or a public repository, against D-106 | 2026-09-07 | ✅ doc. D-170 defers protection until launch |
| F-47 | PR #1 review P1-1: a commit body implied that an agent wrote the commits, and T-6 had no stated boundary for a tool name | 2026-09-07 | ✅ doc. D-176 fixes the reading. The commit body is rewritten. Binds PR-1 exit test 6 |
| F-48 | PR #1 review P1-2: PR-11 created the `night-gate` job and the night job together, so the gate had no result to read on its first run, against G-19 | 2026-09-07 | ✅ D-177 splits them. PR-11 merged as PR #34, two nights ran by hand, and PR-58 merged 2026-09-10 as PR #40 with the gate green on its own PR (G-19) |
| F-52 | Two providers picked the same session number on the same day, because each read the handoff before the other wrote it | 2026-09-07 | ✅ D-187. Fetch and re-read before the handoff commit. The PR-2 session number check fails on a duplicate |
| F-53 | One `Revised by` marker made every citation of a partly revised decision stale. Partial revisions carried 33 of 48 citations and caused three rounds of churn | 2026-09-07 | ✅ doc. D-186 splits the marker into `Superseded by` and `Revised in part by`. The D-178 check keys on the first only |
| F-51 | A grey `review-gate` would stop blocking at launch. GitHub counts a neutral conclusion as a success for a required check, verified 2026-09-07 | 2026-09-07 | ✅ D-181, D-185. Advisory mode gives neutral. Enforced mode gives failure. The tracked file `.github/review-gate-mode` selects the mode, and the workflow reads it from the base branch. D-521 sets `enforced` in PR-78 |
| F-50 | Nothing on GitHub stops a merge without a cross-provider review. T-4 and D-101 are rules only, and D-170 leaves `main` unprotected | 2026-09-07 | ✅ D-179 and D-181, D-185 add the `review-gate` job in PR-1. D-521 and D-522 make it enforced and a required check of the ruleset of `main` in PR-78 |
| F-49 | PR #1 review P2-1: six lines cited D-172 or OQ-2 as a current answer after D-173 and D-175 revised them | 2026-09-07 | ✅ doc. All six corrected. ✅ D-178. PR-2 holds the reference check |
| F-54 | A launch agent cannot read an external volume. macOS denied `/Volumes/SSD-1TB/actions-runner/runsvc.sh` with `Operation not permitted`, and the agent exited 126. A launchd probe repeated the denial, and a login shell read the same path correctly, verified 2026-09-07 | 2026-09-07 | ✅ D-193. Full Disk Access for `/bin/bash` and the runner `node` binary. Binds PR-1 and every machine rebuild |
| F-55 | The Phase 1 roadmap named `WhatYouCarry.sln`, and the .NET 10 SDK creates a `.slnx` file. `dotnet new sln --format` gives `Default: slnx`, and a smoke job on the runner made `Smoke.slnx`, verified 2026-09-07 | 2026-09-07 | ✅ D-194. `WhatYouCarry.slnx`. Godot 4.7.2 accepts it. Binds PR-1 |
| F-56 | PR #6 review P1-1: the `review-gate` workflow ran the tool from the PR head with `checks: write`, so a PR could change the code that judges it | 2026-09-08 | ✅ D-197. `pull_request_target`, the head as data only. Binds PR-1 and Phase 5 |
| F-57 | PR #6 review P1-2: an author can rewrite the approved review file in a metadata commit, and one shared identity cannot prove the reviewer, verified 2026-09-08 | 2026-09-08 | 🔧 D-198. Accepted risk under D-190. The output names the commit that last changed the review file |
| F-58 | GitHub triggers `pull_request_target` only when the workflow file exists on the default branch. PR #7 against the PR-1 branch produced no run, and the events reference states the rule, verified 2026-09-08 | 2026-09-08 | ✅ D-197 Effect. The `review-gate` check cannot run on PR-1 itself. PR-1 merged as PR #6, and the check runs on each later PR |
| F-59 | The reviewer left its commit unpushed on the shared checkout twice on PR #10, against D-183, and the author pushed it each time. The `pr-review` skill said "push" in three places with no verification step and no evidence trail | 2026-09-08 | ✅ D-199. The session end gate reads the remote. The push line is a required part of the review record. Binds every session |
| F-60 | D-161 named a reduction to [-pi, pi], degree-7 minimax polynomials, and an absolute error of at most 1e-6. The three cannot hold together. A true Remez minimax fit of degree 7 on [-pi, pi] reaches 2.5e-4 for sine, 250 times the target, measured 2026-09-08 | 2026-09-08 | ✅ D-203. A fold to [-pi/4, pi/4] and a quadrant. The degree and the target stand. Binds PR-3 |
| F-61 | The Phase 1 roadmap wrote PR-3 exit test 5 as "two states with equal fields in a different insertion order hash equal". D-160 names "a fixed declared order", and no FNV-1a hash can hold the roadmap form | 2026-09-08 | ✅ Document correction in PR-3. D-160 is the decision, and the exit test now asserts that the order is part of the contract |
| F-62 | PR #12 review P2-1: `DetMath.Atan2` read the sign of y with `y < 0.0f`, and negative zero is not below zero. `Atan2(-0, -1)` gave pi against the reference -pi, an error of two pi. The bit-identity sweep never made a negative zero, so the three-platform check could not see it | 2026-09-08 | ✅ Corrected in PR-3. The sign comes from `float.IsNegative`. The sweep now holds the four sign pairs |
| F-63 | PR #12 review P2-2: a `StateHash` from `default` held zero and not the FNV-1a offset basis. It accepted a field and gave a stable number that no FNV-1a hash holds | 2026-09-08 | ✅ Corrected in PR-3. The struct rejects a hash that `Start` did not make (T-2) |
| F-64 | PR #12 review P2-4: the lint tool compared the file name `DetMath.cs` alone, so a second file with that name in another Core directory took the `MathF` exemption. Review P2-3: the reflection rule read namespace text alone, so `typeof(x).GetMethods()` gave no finding. The repeat review refuted the first correction for P2-3: a word list missed `Type.GetEvents()` and falsely reported a Core `probe.GetMethods()`. The third pass found that the symbol table omitted `System.Enum`, which session 28 already named as reflection. The fourth pass found `System.ComponentModel.TypeDescriptor`, a metadata surface beside reflection | 2026-09-08 | ✅ Corrected in PR-3, in four passes. See F-66 for the structural answer. The path rule reads the whole Core-relative path. The first reflection correction matched member words and was incomplete, so the scan now compiles the sources and reads symbols. A Core type may carry a banned platform name, which PR-7 needs for its own `Vector3`. The reflection set now covers Enum, Attribute, AppDomain, Delegate, the runtime handles, RuntimeHelpers, and `typeof` |
| F-65 | PR #12 review P2-5: a `System.Math.Sin` call inside `#if NET10_0` compiles in the Core build, because the target framework is net10.0, and `det-lint` reported nothing, because its parse defined no symbol | 2026-09-08 | ✅ D-204. Core holds no conditional compilation, and `det-lint` reports each `#if`. The owner selected the ban over the build symbols, which cannot read both the Debug and the Release branch |
| F-66 | The PR #12 review reopened the reflection finding four times, and each pass named one more type that the denylist missed: namespace text, member words, `System.Enum`, then `System.ComponentModel.TypeDescriptor`. A denylist over the class library cannot be completed by audit, and each gap stays green until someone finds it | 2026-09-08 | ✅ D-205, which D-207 supersedes on the same date. Core approves each type it uses, and `det-lint` reports every other one. The owner selected an allowlist over one more denylist entry. An adversarial run reports `System.Text`, `System.Threading`, `System.Text.Json`, and `System.Linq.Expressions`, which no denylist named |
| F-67 | PR #12 review P2-7 and P2-8. `System.Guid.NewGuid()` compiled in Core with no finding, because `System` was approved as a whole namespace. An import of an unapproved namespace also gave no finding, because the import check read the denylist alone | 2026-09-08 | ✅ D-206, which D-207 supersedes on the same date. The tool approves the `System` types one at a time, and the import check reads the allowlist. `Guid` and `HashCode` join the type denylist, and `Object.GetHashCode` and `String.GetHashCode` report `L-IDENTITY`. An adversarial run reports `GC` and `OperatingSystem`, which no denylist named |
| F-68 | PR #12 review P2-9: `EqualityComparer<string>.Default.GetHashCode(v)` compiled in Core with no finding, and three processes gave three hashes for one string. The member belongs to `EqualityComparer`, and D-205 approved `System.Collections.Generic` as a whole namespace. `CultureInfo.CurrentCulture` and `RuntimeFeature.IsDynamicCodeSupported` passed the same way | 2026-09-08 | ✅ D-207. Core approves every external type by full name, and no namespace as a whole. A member of an approved type that reads the machine stays in the member denylist. The whole-namespace approval of D-205 and the `System` type list of D-206 both end here |
| F-69 | PR #12 review P2-9, third pass: the type allowlist approved every member of an approved type, so `new CultureInfo("en-US", useUserOverride: true)`, `string.Intern(v)`, and `string.IsInterned(v)` compiled in Core with no finding. The .NET contract for `UseUserOverride` names the user settings, and the contract for `Intern` names the process intern pool | 2026-09-08 | ✅ D-208. Core approves every member of an outside type by name and overload arity. The arity also separates `UInt64.ToString/2`, which takes a format provider, from `ToString/0`, which reads the current culture |
| F-70 | The PR #12 review reopened one finding four times and another one three times. Each pass found a real defect in the new lint tool, and each pass widened the surface that the tool had to cover: a namespace, then a type, then a member, then an overload. Five owner decisions, D-204 to D-208, came from that one boundary, and PR-3 names a lint tool for the banned symbols of G-2 and G-21 | 2026-09-08 | ✅ D-209. The roadmap entry and the exit tests set the review boundary. A concern outside it goes under `## Out of scope` and blocks no merge. A finding closes on its own trigger, and the third assessment of one id asks the owner |
| F-71 | PR-4 found three ways to reach a member that the D-208 rule never read. `CultureInfo c = new("en-US", true);` gave 0 findings, and `new CultureInfo("en-US", true)` gave one, so the P2-9 case was reachable by another spelling. A `base` initializer and an indexer passed the same way, and a check of the allowlist for dead entries found the gap | 2026-09-08 | ✅ D-216. The rule reads a target-typed `new`, a constructor initializer, and an indexer. The owner chose the fix in the PR-4 branch over a separate PR |
| F-72 | PR #15 review P2-1 and P2-2. A caller field named `level` or `message` gave a line with two properties of one name, so a reader could take either value. A safe assertion also added its call site to the caller field set, so a second safe assertion threw on the repeated name and a later ordinary line carried the assertion fields | 2026-09-08 | ✅ Corrected in PR-4. `LogFields` reserves the two names and rejects them at the add. The assertion report takes a copy, so the caller set goes in and comes out unchanged |
| F-73 | PR #15 review P2-3 and P2-4. A string value with an unpaired surrogate gave a line that `JsonDocument.Parse` rejected, so one accepted message could make the log file invalid at that line. The JSON write path also ran three helper levels below its operation, against D-110 | 2026-09-08 | ✅ Corrected in PR-4. The escape pass reads by index, keeps a matched pair, and escapes every other surrogate. `AppendText`, `AppendRaw`, and `HexDigit` are gone, and the write path is one level |
| F-74 | PR #15 review P2-5: a caller field named `assertFile`, `assertLine`, or `assertMember` made the assertion report throw on the repeated name, so one accepted field stopped the report that D-112 requires, and a safe assertion became an exception | 2026-09-08 | ✅ Corrected in PR-4. The three names join the reserved set, and `CopyWithCallSite` writes them on a path that no caller field reaches |
| F-75 | PR #15 review P2-6: D-214 said 8 types and 16 members, and it then named `String.this[]` as one more, while the code held 9 types and 18 additions. The response file named no final head, and the PR description named a superseded head, an old test count, and the old member count | 2026-09-08 | ✅ Corrected in PR-4. D-214 states the counts in the code and the totals after them. The response names the final head, and the description names the current revision |
| F-76 | The PR #12 review reopened its description finding four times, and the PR #15 review reopened one. Each pass cost a whole round trip for a head, a test count, or a sentence that a later commit replaced | 2026-09-08 | ✅ D-217. A review corrects a stale fact in the PR description directly, and it records each edit under `## Description edits`. A wrong claim stays a finding |
| F-77 | PR #15 review P2-7: the F-73 correction put the replacement character in place of a lone surrogate, and a field name took it too. Two accepted names that differ only in a lone surrogate then reached the object as one property name, which is the ambiguity that F-72 removed | 2026-09-08 | ✅ D-218, which ratifies the correction of PR-4. A field name is an identifier, and a lone surrogate in one is an error at the add. A value and a message carry content from the run, so those keep the replacement and never throw |
| F-78 | PR #17 review P2-1 and P2-2. The content hash appended each path and each byte sequence with no length, so the path `a` with the bytes `bc` and the path `ab` with the byte `c` gave one hash. A fractional or out-of-range JSON number also raised a `FormatException`, which left Core with no file and no field | 2026-09-08 | ✅ Corrected in PR-5. Each file enters the hash with a length, its path, a length, and its bytes. The reader keeps the number token as text, and the validator names the file, the field, and the reason |
| F-79 | PR #17 review P2-3: the Game string rule exempted every method named `Get`, so `inventory.Get("You died")` gave no finding. Every type can hold a `Get` method | 2026-09-08 | ✅ Corrected in PR-5. A `Get` call takes an id only when its receiver names the string table. The rule stays syntactic, because the Game project needs the engine assemblies for a symbol read and holds no source file yet |
| F-80 | PR-6: sections 3.9 and 7 of this file said "length-prefixed, checksummed tick frames", and D-162 fixed the frame at 16 bytes with no length | 2026-09-09 | ✅ D-226. Both lines name the fixed frame now |
| F-81 | PR-6: the validator message of PR-5 wrote an enum value inside an interpolated string, and the runtime formats one through its metadata. `det-lint` bans `System.Enum` and reads no interpolation | 2026-09-09 | ✅ Corrected in PR-6 with an explicit switch. OQ-99 asks for the lint rule |
| F-82 | PR-7: a float position cannot hold an exact contact with a block face. For twelve integer faces below 130, such as x = 16 with the 0.3 half-width and y = 8 with the 1.8 height, `(c - h) + h` is one ulp off, measured 2026-09-09 | 2026-09-09 | ✅ D-235. The sweep stops one skin of 2^-10 meters before a face, and never on it. Binds PR-7, PR-8, PR-10, PR-16 |
| F-83 | PR-7: D-231 said the apex of a jump is 1.23 meters. That is the closed form. The fixed-step integration at 60 Hz gives 1.17 meters, measured 2026-09-09. The one-block clear and the two-block fail hold | 2026-09-09 | ✅ doc. D-231 Effect corrected. `JumpClearsOneBlock` asserts the two outcomes and never the apex |
| F-84 | The review-gate workflow job stayed green on a neutral verdict, because a job cannot be neutral by its exit code, and the workflow carries the name "Review gate". The PR rollup of PR #23 read as a pass with no review record, seen 2026-09-09 | 2026-09-09 | ✅ D-251. The job fails on neutral too. The check run keeps its three conclusions |
| F-85 | PR #23 review P2-1: the bit-identity sweep folded the camera pose and the aim ray from a second live loop and not from the replay, so the three-platform proof of PR-8 exit test 5 did not read the replay traversal | 2026-09-09 | ✅ Corrected in PR-8. The replay takes an `IReplayObserver`, and the sweep folds the camera of every replayed tick through it |
| F-86 | PR #28 automated pass: D-262 as first recorded gave gravity and the jump velocity one factor of one half, with the claim that the apex stays at one block and the rise doubles. The kinematics give half the apex and the same rise time, so a body could not leave a pool | 2026-09-09 | ✅ Corrected in PR #28 before the merge. Time doubles, so the velocity takes one half and gravity one quarter, and D-262 carries the dated correction |
| F-87 | PR #31 review P2-1: the spread drew a yaw offset and a pitch offset, each inside the half angle, so a shot could leave the cone of D-266 by up to 6 degrees at the corner | 2026-09-09 | ✅ Corrected in PR-10 before the merge. The spread draws one angle from the direction and one roll around it, so every shot stays inside the cone |
| F-88 | PR #31 review P2-2: the arc solver took a speed of zero, a negative gravity, or a value that is not finite, and gave a direction that claimed to reach the target | 2026-09-09 | ✅ Corrected in PR-10 before the merge. The solver rejects each with a context error (T-2) |
| F-89 | The review gate read the first verdict name after the first text match of the Verdict heading. The repeat review of PR #31 kept a previous-verdict line inside the section, and a history section named "Verdict history" matched the heading as a prefix, so the gate read "Changes required" twice for an approved head, seen 2026-09-10 | 2026-09-10 | ✅ D-269. The gate reads the section under the exact heading, fails two names, and names both. A repository test and the review skill hold the rule |
| F-90 | PR #34 review P1-1: the bot tests called a command that writes to the process console while the bit-identity command test captured that console, so one full run of the suite failed on the timing of the two classes | 2026-09-10 | ✅ Corrected in PR-11 before the merge. Every test class that touches the console is in one xUnit collection, and a shape test keeps a new one in it |
| F-91 | M-1: the Windows CI job of PR-10 took 601 seconds on the push run of `main`, one second over the ten-minute bound of the M-1 procedure, measured 2026-09-10. The same job of PR-11 took 574 seconds | 2026-09-10 | ✅ D-277 keeps the 5000 PR seeds of D-116. The bound reads again at Gate 1, and a job past eleven minutes files a new question |
| F-92 | The first night, run 34499677095 by hand on 2026-09-10: three of five thousand greedy descender runs crashed, because the dig plan ran its 400 jobs and a chamber stayed in rock, on seed 2170 floor 10, seed 3000 floor 4, and seed 4786 floor 10. With the cap lifted the floors need 2817, 1661, and 615 jobs, and ordinary floors under 300 | 2026-09-10 | ✅ D-279 raises the cap to 10000, with the three floors as a regression test. The PR sweep digs one floor per seed and never met them, and the bots dig all fifteen. D-359 supersedes the cap on 2026-09-14 with a job budget and a restart in PR-67 |
| F-93 | PR #40 review P1-1: the `night-gate` workflow read `night.json` from the PR checkout when the fetch of `night-results` failed, so a PR could carry a fresh success record and pass the gate with no night | 2026-09-10 | ✅ Corrected in PR-58 before the merge. The command fetches the branch itself and reads the record from git, never from the working tree. A git failure other than an absent branch is an error that names the command |
| F-94 | The scheduled night at 03:00 UTC on 2026-09-11 never fired: no run of the schedule event exists, the runner was free from 02:45 UTC, the old cron line stood on `main` until 03:54 UTC, and the workflow reads active on GitHub | 2026-09-11 | ✅ D-285 moves the cron to 08:07 UTC, off the start of the hour, which GitHub names as the load peak that delays or drops a schedule. Refuted 2026-09-11 as the whole cause: the 08:07 UTC night came 4 h 40 min late, and a watch read it as a miss (F-95) |
| F-95 | The scheduled night at 08:07 UTC on 2026-09-11 never fired either: no run of the schedule event existed at 09:00 UTC, the runner was online and idle from 07:15 UTC, the `7 8 * * *` line stood on `main` from 06:27 UTC, the workflow read active, and Actions was on. The repository has no schedule run in two chances on two cron lines, githubstatus.com listed no incident after 2026-09-04, and the cause is unknown | 2026-09-11 | ✅ Refuted 2026-09-11: the night ran at 12:47 UTC as run 34600758086, 4 h 40 min after the cron, and passed. The watch stopped at 09:00 UTC and read a late run as a miss. D-286 moved the cron to 17:21 UTC for one test, and D-288 returns it to 08:07 UTC |
| F-96 | The PR-15 contact sheet showed the art as a first pass: faces of three rule settings, a body of ten boxes, and one light. The owner asked whether these were test materials, and no roadmap item raised the art to finished quality | 2026-09-13 | 🔧 D-338 keeps the PR-15 sheet as a first pass. D-339 adds PR-62 after PR-20. D-504 splits it into PR-62 and PR-74 to PR-77. Binds PR-62 and PR-74 to PR-77 |
| F-97 | The owner played floor 1 of PR-15 and found the tunnels very cramped. The dig plan carves drifts of 3 by 3 blocks, a gallery of 5 by 3, and chambers 3 to 4 high, and every rise in a tunnel is a one-block step that needs a jump (D-165, D-253) | 2026-09-14 | ✅ D-341 to D-351: wider and taller spaces, one floor size and one room count, ramps of three slopes, chamber tiers, and the work before PR-16. Done in PR #65 (PR-63), PR #71 (PR-64), PR #73 (PR-65), PR #82 (PR-66), and PR #83 (PR-16) |
| F-98 | The PR-63 measurement: on the sizes of D-341, D-343, and D-344, the two night sets of 175000 floors pass, and the largest need is 5890 jobs. A sweep of 500000 more floors found 7 that ran the 10000 jobs of D-279 with a chamber still in rock: seed 193207 floor 8, 286534 floor 5, 370718 floor 9, 451689 floor 10, 469501 floor 2, 514012 floor 8, and 573862 floor 8 | 2026-09-14 | ✅ D-353 and D-359 to D-361, merged in GitHub PR #69 on 2026-09-14. A dig that runs 1000 jobs with a chamber in rock starts again with a new chamber draw, up to 4 digs. The PR-67 sweep of the 675000 floors found 0 errors |
| F-99 | The macOS leg of Bit identity on the PR #65 review tip `d66fd24` never started: the self-hosted runner did not acquire the job in 20 minutes, and the compare job skipped. Four pushes in 15 minutes queued about 12 macOS jobs on the one Mac runner | 2026-09-14 | ✅ D-355 to D-358, merged in GitHub PR #67 on 2026-09-14. A newer event on a PR cancels the older run of each PR workflow, and CI on the tip counts for the effective head. `RepositoryShapeTests` holds the groups. The groups do not stop a lost leg, and F-100 holds that cause |
| F-100 | The macOS leg of Bit identity on the PR #67 head `66ebefb` ended "not acquired" after 8 minutes, with three macOS jobs queued and no older run. Inside that window, the runner log shows an `acquirejob` HTTP 409 Conflict with "job assignment is invalid: MissingKey" and a skipped job message at 19:39 and 19:41 UTC, then a message for the cancelled job at 19:42 UTC. Inside the F-99 window, the log shows the same conflict at 17:40, 17:42, and 17:51 UTC. On 2026-09-14 the runner, at v2.337.0, the latest release that day, ran 54 jobs by 19:48 UTC and skipped 18 job messages after a conflict. A re-run of the failed jobs at 19:45 UTC passed | 2026-09-14 | ✅ D-358, merged in GitHub PR #67 on 2026-09-14: the author re-runs the failed jobs of a run with a lost self-hosted leg, and the re-run counts as CI for that head. The runner can still skip a job message after a conflict |
| F-101 | The night of 2026-09-15 at the merge commit `4bc8cd4` failed. `EveryChamberReachable` and `DetailKeepsEveryChamberReachable` report seed 79146, floor 7: the shaft at column (25, 48) lands on an unreachable floor at row 8. The bot sweeps of that night passed, and the dig itself reported no error. The floor comes from the dig sizes of PR-63. At `d2ef347`, before PR-63, that seed and floor give a grid of 72 by 16 by 72 with no shaft. At `e1076ca` and at `d65823c`, after PR-63 and without PR-64 and PR-67, the grid is 64 by 20 by 64 with the hash `ff14f981092fdf5a` and the same unreachable landing, which is the grid of `4bc8cd4`. The PR sweep of 5000 seeds never reads seed 79146, and the last green night, at `f487401` on 2026-09-14, ran before PR-63 merged. The cause is the detail pass: `RaisePillars` raised a pillar of chamber 3 at the floor cell (25, 3, 48), which is the column of the shaft of chamber 1 at (25, 48), so the top of that pillar filled the landing air row 8 that `TryDigShaft` proved free before it carved | 2026-09-15 | ✅ D-370, merged in GitHub PR #75 on 2026-09-16. `RaisePillars` raises no pillar in a column that a shaft drops through, and `FloorGenerator.CheckShaftLandings` makes any other cause a loud error. Seed 79146 is a regression test, and the hand night of 100000 seeds passed at `cb3c2e2`. D-371 merged the PR-65 merge record with the gate red, and D-372 merged this PR with it red |
| F-102 | `main` at `3434055` failed two tests. The front matter description of the `gitar-review` skill held one sentence of 27 words, and rule 6.3 of the STE checker holds a descriptive sentence to 25. `docs/session-handoff.md` held 11 entries, and D-379 keeps 10. GitHub PR #79 merged on 2026-09-16 with `ste-check`, `doc-gate`, and the three build legs red, so the trunk carried both faults into every later branch | 2026-09-17 | ✅ PR-69 splits the description into two sentences and runs `handoff-rotate`. PR-70 holds the checker to rule 6.3 on the front matter (OQ-177, D-386). The owner applies a branch protection rule on `main` (OQ-178, D-387) |
| F-103 | Shafts are nearly absent from the dug floors, and PR-66 makes them rarer. The base `27db615` digs 9 shafts in 20000 floors, about one in 2200. The PR-66 dig digs 4 in 60000, about one in 15000. A shaft needs a five by five ring of chamber floor over dug space with two air rows, and the no-step rule of D-347 takes the one-block rises out of the tunnels, so two spaces stack less often. The shaft of D-253 is the one-way drop of a floor, and the F-101 regression seed 79146 no longer digs one | 2026-09-20 | ✅ D-394. Done in PR #82 (PR-66). The shaft pass of PR-66 raises the share of floors with a shaft from 0.045 percent to 2.5 percent |
| F-104 | Two walkers that each walk to the cell of the other never meet. A body crosses one cell in about the ticks of one path search, so it lies between two cells at every search, and two paths of one length lead from the two cells, each through the other. The bot and the enemy then swing around each other 2 meters apart. A search that starts at the cell ahead keeps the step that runs, and the last meters of a chase are a straight walk at the body and not at a cell | 2026-09-20 | ✅ PR #83. `PathFollower` and `PathWalk.CanWalkStraight` |
| F-105 | A body can come no nearer its goal with a clear path ahead of it. Three causes: a drift check that read the X and the Z of a waypoint and never its row missed a body that a fall or a roll took two rows under its path, and the body then jumped at a step of two blocks for the rest of the floor. A step up jumped in place and gave the body no forward input, so a step that one jump cannot clear repeats. A roll away from every blade ends in a standoff that no duel leaves. The drift check now reads the row, a step up jumps and walks on in the same tick, a roll goes through the enemy, and a walk that gains nothing for four seconds jumps. The measured softlock rate of a bot fell from about 10 percent to zero over 2000 seeds of each policy | 2026-09-20 | ✅ PR #83. `PathFollower`, `BotIntent.Roll`, and the two bot policies |
| F-106 | A handoff entry added at the end of `docs/session-handoff.md`, under an older session, fails `HandoffRotateTests.RepositoryFilesHoldTheRule` and reds the three build legs of its branch. The rule of D-146 keeps the newest entry first. Four sessions did this: Session 191 on the base of PR #83, and Sessions 193 and 194 on its branch. Each one held the review of PR #83 at `Blocked` for CI that the same entry had reddened, and the review commits moved the tip again on each pass. `handoff-rotate` reported the order and changed nothing, so every repair was by hand | 2026-09-20 | ✅ PR #83. D-406: the rotation puts the entry back in place and names it |
| F-107 | An enemy walks no diagonal. `GridMoves.Directions` is 4, with the steps (1,0), (-1,0), (0,1), and (0,-1), so every path of an enemy is a staircase of side steps, and no enemy cuts a corner | 2026-09-22 | ✅ PR #90 (PR-72): the diagonal move of D-486 to D-489 in `GridMoves` and `GridPathfinder`. The owner saw it in the play session of PR-20 |
| F-108 | An enemy does not walk up a ramp, as the owner saw in the play session of PR-20. The cause is open: `GridMoves.RampWalk` models a walk on a ramp, and `Enemy.Step` and `Hunter.Step` each pass the slope rule of the body, as the player does. The path follower, the ramp geometry check, or the approach of the brain holds the fault. Cause found 2026-09-22 in PR-72: the enemy climbed in slow jumps (D-485). `PathWalk.NeedsAJump` read the feet against the face of the next place, and the feet at the middle of a place stand under that face. On seed 1, floor 1, 80 to 87 percent of the ticks of each climb were in the air | 2026-09-22 | ✅ PR #90 (PR-72): the jump rule reads the slope under the feet |
| F-109 | A code head waited 18 to 21 minutes for the hosted CI legs. Run 35771495463 on `06cd3b3` of PR #87 took 1055 seconds for the test step on Linux, 1206 on Windows, and 451 on the Mac mini. The build took 22 to 29 seconds. A local profile of 2026-09-22 read 1294 tests: 23.4 minutes of summed test time, 22.4 minutes of CPU time, and 9 minutes 10 seconds of wall time. Ten seed sweeps held 75 percent of the summed time. `ProcgenTests` summed to 549 seconds in one class, and xUnit runs the tests of one class in sequence, so that class set the wall time of the Mac mini and of a local run. The hosted runners have 4 vCPUs, so CPU time bound them. Each push of documents alone also ran every check again | 2026-09-22 | ✅ PR #89 (PR-71): the CI skip (D-472 to D-477), the class split (D-478), the job split (D-479), and one fifth of each sweep on a pull request (D-480, D-481) |
| F-110 | The CI, Smoke, and Bit identity workflows each named three jobs `linux-x64`, `windows-x64`, and `macos-arm64`, as the check runs of PR #92 at `d96ae19` show. A ruleset requires a check by its name, so one required name matched three jobs, and a pass of one hid a failure of another | 2026-09-23 | ✅ PR #93 (PR-78): each platform job has a check name with the prefix of its workflow, and `RulesetTests` fails on a name that two jobs carry (D-522) |
| F-111 | The night of 2026-09-23 at `e069e16` read a softlock of the greedy descender on 26 of 5000 seeds. The night of `170f08c` passed. The 26 seeds pass at `ea84473` and softlock at `837902b`, so PR-72 made them. Two faults. `PathFollower` read a wedge after four seconds with no gain on the goal, also on a detour of a diagonal path, and the bot then jumped at each step. A jump under a low ceiling or on a ramp held the bot in a loop (24 seeds). `GridMoves.DiagonalMove` took a drop into a side column and a walk under an overhang as one diagonal drop, and the body landed on the overhang (seeds 2669 and 2879) | 2026-09-23 | ✅ PR #97 (PR-81): an arrival at a waypoint starts the wedge count again, and a diagonal drop needs an open fall (D-545, D-546) |
| F-112 | The local night of PR-81 crashed seed 4119 of the greedy descender on floor 3: "The box overlaps a solid cell before the move." An enemy slid away from a wall on X and along it on Z. `SweptAabb.Sweep` moved its box one step at a time, to max x = (7.722285 + 0.3) - 0.022284 = 8.0, a contact, so the Z move left the block at x = 8 out. `PlayerBody` built its box from the feet, (7.722285 - 0.022284) + 0.3 = 8.000001, which overlapped that block. Float addition does not associate. The fault is as old as D-235, and the walk of PR-81 reached it | 2026-09-23 | ✅ PR #97 (PR-81): the sweep builds each box in the form of the caller (D-549) |
| F-113 | A repository review read the loop at `e5e164f`: a hit that took the last health on the tick of a stairwell press did not end the tick. The interact bit then descended with a dead player, and `new Player` threw on health 0, so the Game quit and a bot run read a crash. The ascend bit ended the dead run as an ascend. Seeds 2, 4, and 11 reproduce it at the stairwell of floor 1 | 2026-09-24 | ✅ PR #104 (PR-88): a run that ends in the tick takes no stairwell choice, and a test presses each bit on the lethal tick (D-322, G-20) |
| F-114 | A descend press at the stairwell of floor 15 dug floor 16, which no template covers, and the loop threw after the tick moved on. The HUD showed the descend line on every floor. No bot takes that path, so no night can find it | 2026-09-24 | ✅ PR #104 (PR-88): the press does nothing on the deepest floor, and the HUD hides the line (D-579) |
| F-115 | An exception outside the four guarded blocks of `Main.cs` left the engine callback. The engine glue printed it and called the callbacks again, so a session went on half updated and quit with exit code 0 and no JSONL error line. A throw on each draw gave 3070 engine errors and exit code 0 | 2026-09-24 | ✅ PR #104 (PR-88): each engine callback catches every exception, writes one error line, and quits with exit code 1 (T-2, D-114) |
| F-116 | `review-gate` found a verdict name anywhere in the Verdict section. It approved "Not Ready for owner merge", "Changes Required" with a later approving name, and a fenced example of an approving section above the real one. `codex-review` read the same parse as an approval | 2026-09-24 | ✅ PR #104 (PR-88): the first line of the section starts with the bold name, and the parse skips each fenced block (D-179, D-269) |
| F-117 | The reachability sweep let an exception of the dig leave the sweep. It wrote no failure line, so the night record lost the seed, and the carry of D-567 could not run it again | 2026-09-24 | ✅ PR #104 (PR-88): a dig that throws fails its seed, and the sweep goes on to the next seed (D-565, D-567) |
| F-118 | The asset loaders kept the last value of a repeated JSON key. A repeated bone track crashed `asset-qa` with exit code 134 and no file named, and it failed the pose in the Game | 2026-09-24 | ✅ PR #104 (PR-88): the five parse sites reject a repeated key, and a loader fault of one file is a finding on that file (D-92, T-2) |
| F-119 | `asset-qa` read the top of `content/models/` alone, and the content loader accepts a model in a subdirectory. An animation whose model value had no `.bbmodel` extension, or named no body, loaded and was never posed | 2026-09-24 | ✅ PR #104 (PR-88): the gate reads each depth, the model value ends in `.bbmodel`, and an animation of no body is a finding (D-135) |
| F-120 | The content validators checked lower bounds alone. An enemy health of 4294967296 loaded, and each enemy of floor 1 spawned dead. A `maxDepth` of 4294967297 gave a deepest floor of 1, and a `timerSeconds` past the long range gave a timer of one second | 2026-09-24 | ✅ PR #104 (PR-88): each numeric field fits the type that holds it, and a projectile damage is one or more (D-92, G-7) |
| F-121 | An error inside a tick or a replay named no seed and no floor, a log line held the message alone, and a failed descent left the plan and the floor number in disagreement | 2026-09-24 | ✅ PR #104 (PR-88): each error of a run names the seed, the floor, and the tick, the log line names the type and the inner chain, and a failed descent leaves the old floor whole (T-2, D-113). A dig task that the swap drops stays unobserved, because no test reaches it |
| F-122 | The frame log flag took an empty word, and the session then hung at its end with no end line. A boot failure wrote an empty frame log, and git did not ignore the relative outputs of the Game commands | 2026-09-24 | ✅ PR #104 (PR-88): an empty word stops the boot, the quit always reaches the engine, and a boot failure writes no frame log (T-2) |
| F-123 | The state hash folded the waypoint and the ticks since the last search of each path follower, and not the stored path or the wedge count, so two states that differ there gave one hash | 2026-09-24 | ✅ PR #104 (PR-88): the hash reads the path and the wedge count, and the bit-identity sweep takes a new known answer (D-160, G-20) |
| F-124 | Some tests claimed more than they checked: a determinism test that compared the end hash alone, a worker test with no enemy spawns, a ray test that asserted one condition two times, and F-111 regression tests that accepted a death | 2026-09-24 | ✅ PR #104 (PR-88): each test checks its claim, and three reachability properties have checks (T-3) |
| F-125 | The gate tools had edge cases: a directory with the name of a root document hid a code change from the effective head, an unknown finding status read as closed, a night record could end in the future, a tag could take the place of the night branch, a promotion read a build failure as no promotion, and git could stop on a full error pipe | 2026-09-24 | ✅ PR #104 (PR-88): each case is an error or reads the exact ref (T-2). The label time against the push time (D-539) and a child process timeout stay open, because neither has a source or a value |
| F-126 | The ruleset runbook and the comment export used fixed paths under `/tmp`, so two sessions on one machine could read the file of the other | 2026-09-24 | ✅ PR #104 (PR-88): each procedure writes its files with `mktemp` and sets its own variables |
| F-127 | Core published the boss floors, the timer marks, and the move tables as arrays that any caller could change for each run in the process | 2026-09-24 | ✅ PR #104 (PR-88): each table is a read-only list |
| F-128 | The swing of `overseer-pick` is 30, 6, and 30 ticks, and it names the sword clip of 36 ticks with the phases 12, 6, and 18 (D-87). The Game poses the player alone, so no player sees it yet | 2026-09-25 | 🔧 found in PR-88. A test holds the gap until a clip for the pick exists |
| F-129 | `RampRayTests.TheMarchAgreesWithAFineWalkOverRamps` asserts one condition two times, the fault of F-124 on the ramp march | 2026-09-25 | ✅ PR #104 (PR-88): the test asserts the hit and both bounds of its distance (T-3) |
| F-130 | `ChamberBudget.WindowTop` and the sums of chamber weights are long values that a very large `difficultyBudget` or chamber weight can overflow | 2026-09-25 | 🔧 found in PR-88 |
| F-131 | The loaders passed four schema faults: a floor band that no detail pass knows, a locator rotation that the model loader dropped, an empty string in the string table, and a keyframe pose that overflowed near the float limit | 2026-09-24 | ✅ PR #104 (PR-88): each fault fails at load, and the pose at a keyframe tick is the keyframe (D-92, G-7, T-2) |
| F-132 | `det-lint` did not see the constructs that the compiler lowers: an interpolation, a concatenation of a number, a record `GetHashCode`, and `double` in Core, and an interpolated or const text in Game. The member keys counted parameters, so one entry approved every overload of that count | 2026-09-24 | ✅ PR #104 (PR-88): each construct is a finding outside error text, and each method entry names its parameter types (G-2, G-8, D-582) |
| F-133 | The bit-identity sweep masked the stairwell bits, gave the enemies one damage, dug synthetic floors alone, and ran Debug alone, so the three platforms never compared a descent, a death, an ascend, or a real floor | 2026-09-24 | ✅ PR #104 (PR-88): the sweep folds one real floor and two recorded runs that descend, die, and ascend, and each platform job checks that Release gives the Debug hash (G-9). The known answer now moves with the content numbers too |
| F-134 | `night-gate.yml` put the base branch into the shell text, it did not state that it runs the code of the PR, and no test stopped a second job named `review-gate` or a second workflow with `checks: write` | 2026-09-24 | ✅ PR #104 (PR-88): the base branch comes through the environment, the workflow states the trust, and two tests hold the rules. The wording of D-197 and a daily night gate re-run wait for the owner |
| F-135 | A press and a release inside one long frame never reached a tick, and Tab and Shift+Tab stopped on the last control of a row | 2026-09-24 | ✅ PR #104 (PR-88): the reader latches each press edge for the next intent, and the tab order follows the focus map (D-77, D-446). The text size on the Deck waits for M-3 |
| F-136 | The design doc and the agent guidance lagged the decisions: the tenets, the save files, the night seeds, the merge summary, the gitar gate, the README, the CI job split, job names, a lookup line, a style source path, and code remarks | 2026-09-24 | ✅ PR #104 (PR-88): each line matches its decision, and a test holds the tenet copies equal (D-122). The register split waits for OQ-205 |
| F-137 | Revision markers, status marks, the PR-86 entry, three roadmap lines, the order of the archive, and a reference exemption that read a whole line lagged the registers | 2026-09-24 | ✅ PR #104 (PR-88): each one follows D-186 and the status rules. The markers of D-188 and D-190 wait for an owner answer, because D-517 says that both stand |
| F-138 | No machine check held T-6, and one attribution line reached `main` in the body of `94f0897` | 2026-09-24 | ✅ PR #104 (PR-88): `doc-gate` fails a co-author trailer, a generation line, and the robot line in the title, the description, and each commit message, and a bare provider name passes (D-176) |
| F-139 | Six merges passed a failed gate with no register row: #14, #16, #18, #85, #94, and #95 | 2026-09-24 | ✅ PR #104 (PR-88): F-141 to F-146 record each exception |
| F-140 | `ste-check` matched `.md` in one letter case, never read `.markdown`, read the worktrees under the checkout, and its skill described three rules other than the code | 2026-09-24 | ✅ PR #104 (PR-88): the checker and the skill agree, and a test reads each row |
| F-141 | PR #14 changed documents alone and merged as `51b3de7` with no review record and no `review-override` label (T-4, D-188, D-190). The `review-gate` check of head `e105990` read neutral in advisory mode, because `docs/reviews/pr-14.md` did not exist | 2026-09-25 | ✅ doc. The row records the exception (F-139). No decision covers it |
| F-142 | PR #16 changed documents alone and merged as `a37f0af` with no review record and no `review-override` label (T-4, D-188, D-190). The `review-gate` check of head `5302374` read neutral in advisory mode, because `docs/reviews/pr-16.md` did not exist | 2026-09-25 | ✅ doc. The row records the exception (F-139). No decision covers it |
| F-143 | PR #18 changed documents and the agent files alone and merged as `c11fb41` with no review record and no `review-override` label (T-4, D-188, D-190). Its handoff entry planned the merge with the label, and the PR timeline holds no label event. The `review-gate` check of head `79e3d38` read neutral in advisory mode | 2026-09-25 | ✅ doc. The row records the exception (F-139). No decision covers it |
| F-144 | PR #85 (PR-18) merged as `32e7909` while the `review-gate` check of head `86b0d83` read failure: "A commit outside the metadata set came after the review". The review record named `bab19cc`, and the registers commit `e32c6cb` of D-440 came after it (D-184). D-440 covers the red `night-gate` check alone | 2026-09-25 | ✅ doc. The row records the exception (F-139). No decision covers the red `review-gate` check |
| F-145 | PR #94 (PR-74) merged as `2c4e6d5` with the verdict `Blocked` in `docs/reviews/pr-94.md` line 59. The required `review-gate` and `night-gate` checks of head `2a4b1c8` read failure. The owner merged through the admin bypass of the ruleset of `main` (D-520, D-537). Only the session handoff recorded it | 2026-09-25 | ✅ doc. The row records the exception (F-139). No decision covers it |
| F-146 | PR #95 (PR-79) merged as `25afe34` with the verdict `Blocked` in `docs/reviews/pr-95.md` line 83. The required `review-gate` and `night-gate` checks of head `7efc763` read failure. The owner merged through the admin bypass of the ruleset of `main` (D-520, D-537). Only the session handoff recorded it | 2026-09-25 | ✅ doc. The row records the exception (F-139). No decision covers it |

## 6. Guardrails (the safety contract for every PR)

### 6.1 Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins (D-119): T-5, T-2, T-3, T-4, T-1. T-6 is absolute and never conflicts.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures. Helpers go one level deep (D-110). Two concrete cases before any abstraction (D-111). No clever one-liners. Tune only on measurement (D-109).
- **T-2. Zero silent failures.** No empty catch blocks. An absent value is an error, never a zero. Every error carries its context (D-113). Assertions stay on in shipped builds (D-112).
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it. The review file in `docs/reviews/` records the findings (D-101). A PR that changes no code merges without a review when the owner adds the `review-override` label (D-188, D-190).
- **T-5. Document everything.** Continuity is the first duty. Each session adds its entry at the top of `docs/session-handoff.md` (D-146). The other documents update when intent, a decision, or a plan changes (D-118).
- **T-6. No attribution.** No code, game text, commit, PR description, or GitHub comment names an agent, harness, or model as the source of work (D-137). Two places are exempt: the author field in `docs/session-handoff.md`, and the files in `docs/reviews/`.

### 6.2 Guardrails

1. **G-1.** Core has no engine dependency. A test asserts the reference list.
2. **G-2.** No `System.Math` transcendentals, no `Vector<T>`, no SIMD, no reflection, no dynamic dispatch in Core. The lint tool enforces it (D-67).
3. **G-3.** Godot physics and navigation never feed the simulation (D-80).
4. **G-4.** The simulation runs on one thread. Only the next-floor generator runs on a worker (D-72).
5. **G-5.** Every run records its seed and intent stream from the first tick. That record is the crash report and the resume file (D-97).
6. **G-6.** No hitscan. Every projectile is a simulated object.
7. **G-7.** Every content file validates against its schema at load and in a test. An absent field is an error (D-92).
8. **G-8.** No inline strings that the player sees (D-98).
9. **G-9.** The three-platform bit-identity job is green before merge (D-71).
10. **G-10.** One concern per PR (L-1).
11. **G-11.** No unowned decision. A session that hits an open question files it and stops (D-124).
12. **G-12.** Ids in this file and in the decision register never change (design-doc-style rule).
13. **G-13.** No co-author trailer, generation line, or text that names a model as the source of the work in a commit, PR, or comment (T-6, D-176). A tool name that identifies a file, a schema, or a version is not attribution. The `doc-gate` job checks the trailer and the generation line (F-138).
14. **G-14.** Every document follows ASD-STE100. The `ste-check` job runs the checker in the PR gate (D-139, PR-2).
15. **G-15.** The Steam Deck at 800p is the readability and performance floor for every UI and render change (D-15).
16. **G-16.** Every dependency has a decision entry that justifies it.
17. **G-17.** Every optimization has a profile before it and a measurement after it (D-109).
18. **G-18.** Squash merge from a short branch, with a conventional commit subject (D-126). GitHub auto-merge merges a PR with the green light after the owner confirms the merge summary, and the owner can merge too (D-516, D-524, D-552).
19. **G-19.** A PR that creates a check passes that check. A PR names any check that does not exist yet, with the PR that creates it (D-148).
20. **G-20.** Every Core behavior change bumps the simulation version constant, and the review confirms it (D-151).
21. **G-21.** No `System.Random`, `DateTime`, `Stopwatch`, or `Environment.TickCount` in Core. The seed and the tick are the only sources of randomness and time (D-69, D-73).
22. **G-22.** One session works on one PR. The PR carries all of its documents, and no PR exists only to record an earlier PR. The `doc-gate` job checks the parts that a machine can read (D-375, D-376).

## 7. Roadmap

Phases are the five milestones of D-136 as revised by D-150. Gate 1 is a foundation gate with a written exit test. Gates 2 to 5 are playable builds with a written exit test and the owner's playtest sign-off. Ids: PR-# code changes, M-# measurements. An entry that covers several PRs notes its reserved id range. Focused roadmaps in `docs/roadmaps/` expand the phases with per-PR exit tests. Phase 1: [phase-1-foundations.md](roadmaps/phase-1-foundations.md). Phase 2: [phase-2-first-playable.md](roadmaps/phase-2-first-playable.md). Phase 3: [phase-3-full-loop.md](roadmaps/phase-3-full-loop.md). Phase 4: [phase-4-content-complete.md](roadmaps/phase-4-content-complete.md). Phase 5: [phase-5-early-access.md](roadmaps/phase-5-early-access.md).

### Phase 1: Foundations (foundation gate, D-150: CI green on three platforms with a bit-identical end state, docs and PR gate live, no playtest) ✅ Signed 2026-09-11 (D-288)

**PR-1: Repository scaffold.** ✅ Merged 2026-09-08 as PR #6.
Create the solution with `WhatYouCarry.Core`, `WhatYouCarry.Game`, `WhatYouCarry.Tools`, and `WhatYouCarry.Tests` (D-108). Pin Godot 4.7.2 .NET and .NET 10 LTS (D-61, D-62, D-173). Create `CLAUDE.md` and `AGENTS.md` as identical pointer files with a test that asserts equality (D-122). Add the GitHub Actions workflow that builds and runs `dotnet test` on Linux x64, macOS arm64, and Windows x64 (D-148). Add a PR template with the gate checklist and the "no change needed because" lines (D-118). The template has a line that names each absent check with the PR that creates it (D-148). Add the `review-gate` job (D-179). It publishes a check run with three conclusions (D-181). The mode file `.github/review-gate-mode` selects the mode, and the workflow reads it from the base branch (D-185). Success means an approved review of the effective head. Failure means a review that does not approve, or a stale head. Grey means no review record yet, and only in advisory mode. The workflow job reads red then, because a job cannot be grey by its exit code (D-251). The label `review-override` gives success for a PR that changes no code, and the owner adds that label (D-188, D-190). The job is advisory until launch, because GitHub locks branch protection on a private free repository (D-170, D-180). The attribution option is in place (D-175). Needs the external SSD (D-145) and the runner (D-171). No game code.
Gate: `dotnet build` and `dotnet test` pass on all three platforms, and the `review-gate` job runs on the PR.
> *In plain English:* this makes the empty project with its four parts and the rules files that every future session reads first. It also adds a check that turns red when a change has no approved review. It adds nothing that plays. It is safe because it changes no behavior.

**PR-2: STE checker.** ✅ Merged 2026-09-08 as PR #10.
Port the STE checker to C# as `WhatYouCarry.Tools.SteCheck` (D-130). It flags passive voice, helper verbs, sentence-initial and preposition-led -ing forms, semicolons, contractions, and the 20-word and 25-word limits. Dated records are exempt. Run it in CI on every hand-written `.md` file. The same command runs the reference check (D-178, D-186) and the session number check (D-187).
Gate: the checker passes on itself, on this file, and on the skills. The PR names the absent lint tool and bit-identity job with PR-3 (D-148).
> *In plain English:* this adds a tool that reads every document and reports sentences that break the text rules. Documents are the project's memory, so the tool guards that memory.

**PR-3: Seeded RNG, DetMath, lint, and the bit-identity CI job.** ✅ Merged 2026-09-08 as PR #12.
Implement the seeded RNG as xoshiro128** streams, one per subsystem, seeded by SplitMix64 (D-159). Implement DetMath in float from polynomial methods that use only the five exact IEEE operations (D-70, D-161). `Sin` and `Cos` fold to [-pi/4, pi/4] and a quadrant, and `Atan2` folds to the octant (D-203). `Pow` takes an integer exponent (D-200). Implement the state hash as FNV-1a 64 over the raw field bits, in a fixed declared order (D-160). Implement the `det-lint` command, which parses each Core source file with the C# compiler API (D-67, D-202). Add the `bit-identity` command and the CI job that runs it on Linux x64, macOS arm64, and Windows x64 (D-201). A fourth job compares the three hashes (D-69, D-71).
Gate: this PR passes its own lint tool and bit-identity job (D-148), and the lint tool fails a test file that calls `System.Math.Sin`.
> *In plain English:* different computers give slightly different answers for functions like sine. A Windows machine then cannot repeat a bug from a Mac. This adds our own math that gives the same answer everywhere, and a check that proves it on every change.

**PR-4: Logger, error context, and assertions.** ✅ Merged 2026-09-08 as PR #15.
Implement the JSONL logger with a required field set per context (D-68, D-113). Inside a run the set is seed, floor, tick, subsystem, and entity ids. Outside a run the set is save versions, screen, action, and file paths. The logger throws on an absent required field. Implement the assertion helper that writes a full report and continues where a caller marks it safe (D-112). The report names the call site from the compiler, and it walks no stack (D-215). A field name is an identifier, and invalid text in one is an error. A value and a message carry content from the run, and the writer puts the replacement character in place of invalid text there (D-218, F-73, F-77). Core hands each line to a sink, and it opens no file (D-211). Every line carries a level and a message (D-212). Define the error types that carry context on rethrow.
Gate: a test proves that a log line without its context fails, and that an assertion report contains the seed.
> *In plain English:* every message the game writes about itself now carries enough facts to replay the moment. A message without those facts is itself an error.

**PR-5: Content loader, schemas, and the string table.** ✅ Merged 2026-09-08 as PR #17.
Implement the JSON content loader with one schema per content type (D-91, D-92). A load failure names the file, the field, and the reason. A test loads every content file in the repository. Implement the ID-keyed string table, and add a lint rule against inline strings that the player sees (D-98).
Gate: a content file with an absent field fails the load test with the field name.
> *In plain English:* every weapon, enemy, and screen text lives in data files with a strict shape. A file with a gap fails loudly instead of a silent zero.

**PR-6: Simulation loop, intent record, recorder, and replay.** ✅ Merged 2026-09-09 as PR #19.
Implement the fixed-step loop at 60 Hz (D-73). Define the intent record: quantized yaw and pitch deltas, a movement vector, and button states (D-74, D-77). Define the run record (D-151). Its header holds a format version, a simulation version constant, a content hash, the seed, and an immutable initial state. The fixed 16-byte checksummed tick frames of D-162 follow the header (D-226). Implement the recorder that writes the header and appends frames from the first tick (D-97, G-5). Implement the replay that drives the loop from a record and ignores the live bank and tree. Property tests assert three facts over one thousand seeds. A replay reproduces the end-state hash. A torn tail truncates to the last complete frame. A version or content mismatch produces a contextual report.
Gate: the replay of a recorded run gives the same hash on all three platforms, and a mismatch report names both versions.
> *In plain English:* the game runs in fixed steps and writes down its start state and every input. That record then plays any run again, so every bug becomes repeatable. A record from an older version says so instead of a silent failure.

**PR-7: Voxel world and Core collision.** ✅ Merged 2026-09-09 as PR #21.
Implement the voxel grid of one-meter cubes (D-78, D-234). Implement Core collision for player and enemy boxes against the grid with swept movement, gravity, ledges, and jump (D-27, D-80, D-231, D-235 to D-240). Godot physics has no part in it. Add the player box that reads the intent's movement and jump, so a Core-only run exists before the Game layer (D-149). Property tests assert no tunnel at maximum speed and no fall through a floor block.
Gate: a box that moves at the maximum speed never crosses a solid block.
> *In plain English:* the dungeon is a grid of blocks. The game itself decides how bodies bump into them, so the result is identical on every machine.

**PR-8: Camera as a Core system.** ✅ Merged 2026-09-09 as PR #23.
Implement the over-the-shoulder camera in Core (D-13, D-75, D-241, D-242, D-245 to D-249). It integrates the quantized look deltas, sweeps its boom against the grid, and derives the aim ray (D-77, D-88). Aim assist runs here from enemy positions (D-14, D-243, D-244). Property tests assert the camera never enters a solid block and the aim ray is deterministic.
Gate: a recorded run with camera motion replays to the same hash on all three platforms.
> *In plain English:* the camera is part of the simulation, not decoration, so where you look and where you aim replay exactly.

**PR-9: Procgen v1 and property tests.** ✅ Merged 2026-09-09 as PR #27.
Implement floor generation on the grid for one biome, a collapsed deep mine (D-6, D-13, D-46, D-210). A floor is a mine dig plan: a main gallery with side drifts, chambers, shafts and ramps, a spawn point, and a stairwell (D-253). Every tunnel is at least three by three (D-166). The floor size grows with depth, by band, in the floor template (D-252). D-343 revises the band sizes for PR-63: every band is 64 by 20 by 64. Chamber kinds are a content type with a weight, and the sum of weights lands inside the budget window (D-167, D-255). The spawn is in the first chamber, and the stairwell is in the farthest one (D-256). Implement the stairwell transition in Core (D-50, D-149). Two button bits at the stairwell carry the choice, so the record replays it (D-257). The next floor generates from the run seed and the floor number. PR-9 carves raw stone and air alone, and PR-59 adds the detail (D-254). Property tests run over thousands of seeds per PR and one hundred thousand each night (D-116). They assert four facts: every chamber is reachable, no chambers overlap, the stairwell is reachable, and the difficulty budget is within tolerance.
Gate: the night sweep passes on one hundred thousand seeds.
> *In plain English:* this digs the mine floors from a random seed: galleries, side tunnels, chambers, and shafts, and no two floors look alike. Tests over huge numbers of seeds prove that a player can reach every chamber and the stairs down.

**PR-59: Mine detail pass.** ✅ Merged 2026-09-09 as PR #29.
Add the detail that makes the dig plan read as the mine of D-210: collapses that fill dead ends with rubble, pillars in chambers, and the block ids by band (D-254, D-259). Floors 1 to 5 take timber beams and planks. Floors 6 to 10 take hewn stone and still water. Floors 11 to 15 take ore veins. Still water is not solid: a pool is one block deep, and a body walks and jumps through it at half pace (D-258, D-261 to D-264). The reachability sweep of PR-9 runs again, so no detail closes a chamber or a tunnel. The simulation version rises, because a floor with other blocks is another simulation (D-260). This entry follows PR-9 in the sequence.
Gate: the PR-9 property tests pass with the detail on, and every floor of a band uses the blocks of its band. A body in water walks and jumps at half pace.
> *In plain English:* the bare tunnels gain the look of a mine: fallen rock, timber, cut stone, ore, and water. The tests from before run again, so nothing blocks the way.

**PR-10: Projectile simulation.** ✅ Merged 2026-09-10 as PR #31.
Implement the projectile integrator with fixed-step Euler, swept collision against the grid and entity boxes, gravity scale, lifetime, and spread from weapon data (G-6, D-266). Implement the arc solver with DetMath. Add a `projectile` content schema and a test-only definitions file (F-38, D-149). Until PR-15 gives the loadout a weapon, the attack bit fires the first definition of the content set once per press (D-265, D-267). D-320 supersedes D-265 in PR-15, and the attack bit swings the sword there. A shot starts at the shoulder point and flies toward the aim hit (D-268). The file holds the slowest arc, the fastest flat shot, the longest lifetime, and the widest spread. Property tests assert three facts. No projectile tunnels through the minimum wall at the maximum velocity. Every projectile ends inside its lifetime. The arc solver reaches a reachable target and reports an unreachable one.
Gate: the projectile property tests pass over the test-only definitions. PR-24 and PR-43 to PR-46 rerun them over the real roster.
> *In plain English:* bullets and arrows are real objects that fly, drop, and can miss. Tests prove a fast bullet never passes through a wall.

**PR-11: Bot harness (Tier 2).** ✅ Merged 2026-09-10 as PR #34.
Implement the headless runner at one hundred times speed and the first two policies: random walker and greedy descender (D-127, D-149). Later PRs add a policy with the system it exercises: full-clearer with PR-16, timer-tester with PR-17, coward with PR-18. Each run writes a structured run log with the policy name, seed, and end state. Add a few hundred runs to the PR job and ten thousand to the night job (D-115). The night job publishes a result record. PR-58 adds the gate that reads it (D-177). The runs end as bottom, ascend, budget, softlock, crash, or death, and the night job writes its record to the branch `night-results` (D-270 to D-273, D-403).
Gate: ten thousand night runs of the two policies complete with zero crashes and zero softlocks.
> *In plain English:* simple robots play thousands of runs every night without graphics. They find crashes and dead ends before a person ever sees them.

**PR-58: Night gate.** ✅ Merged 2026-09-10 as PR #40.
Add the `night-gate` job to the PR workflow. It reads the result record that the PR-11 night job publishes (D-177). The gate passes only on a success record from a night in the last 48 hours, whatever event ran it (D-274). An absent, malformed, stale, cancelled, or failed record fails the gate, and so does a record whose commit is not on the base branch (D-115, D-275, T-2). The message names the case, the commit, and the time. This entry follows PR-11 in the sequence, after one night runs, scheduled or by hand (G-19, D-278).
Gate: the job fails each of the six bad records and passes on the real night record.
> *In plain English:* every merge now needs a green night from the robots. A missing or old result stops the merge, so nobody can merge on silence.

**M-1: CI wall time per PR.** ✅ Table complete 2026-09-10 in the Phase 1 roadmap (D-276). One job exceeded ten minutes by one second, and D-277 keeps the seed counts (F-91).
Record the wall time of each CI job per platform for ten PRs. Binds the seed counts in D-116 if a PR job exceeds ten minutes.

**M-2: Night sweep wall time.** ✅ Table complete 2026-09-11 in the Phase 1 roadmap, seven nights (D-283). The longest night took 68 minutes against the six-hour bound.
Record the night sweep duration for seven nights: the six hand runs of 2026-09-10 and the first scheduled night of 2026-09-11 (D-283). Binds the night run counts in D-115 and D-116.

### Phase 2: First playable (gate: the owner plays one floor with the timer, the hunter, and a stairwell, D-57)

**PR-12: Game skeleton and input.** ✅ Merged 2026-09-11 as PR #49.
Create the Godot project with scenes built in C# (D-63). Bind the Core loop at 60 Hz with render interpolation (D-73). Map keyboard, mouse, and controller to the intent, with sensitivity and curves applied before quantization (D-15, D-77, D-289). Add the headless smoke session to CI: boot, start a run, move for one thousand ticks, quit, with no log errors (D-114, D-149). PR-18 extends it to the stairwell.
Gate: the smoke session passes on all three platforms.
> *In plain English:* this is the first thing you can open and move in. It also adds an automatic run of the real game on every change.

**PR-13: Model loader and mesher.** ✅ Merged 2026-09-12 as PR #52.
Implement the Blockbench JSON loader that builds an ArrayMesh from the box list, with armor overlay attachment per slot (D-9, D-18). Implement greedy meshing for the voxel grid, one mesh per chunk, with vertex ambient occlusion (D-78, D-81, D-291). Fade the walls between the camera and the player in the world shader, with no change to the mesher (D-88, D-292). One atlas, one material. A test asserts the mesh count per floor stays under the budget of D-291.
Gate: a full floor renders under the mesh budget, and M-3 records the Deck frame time.
> *In plain English:* this turns the block grid and the box models into pictures, cheaply enough for the smallest target machine. It also hides the wall between the camera and you.

**PR-57: Asset QA gate v1.** ✅ Merged 2026-09-12 as PR #54.
Implement the C# tool for three checks (D-135, D-149). No two boxes interpenetrate at the rest pose or at any animation keyframe, with the pairs of a bone and its parent exempt (D-301). Each armor overlay encloses the body box of its name (D-300). Every file name reference matches the file case (D-302). The tool reads the animation files of D-298 and poses each model through `WhatYouCarry.Assets`, the reader that Game shares (D-299). Run it in CI on every model. PR-49 extends it. This entry follows PR-13 in the sequence.
Gate: the tool fails a model with a clip and a model with a wrong-case reference.
> *In plain English:* from the first model onward, an automatic inspection catches pieces that clip through each other and file names that break on Linux.

**PR-14: Texture generator and palette.** ✅ Merged 2026-09-12 as PR #56.
Implement the C# texture generator that writes the 32 px atlas from the palette and the rule files under `content/textures/` (D-65, D-85, D-305). The palette is candidate A of D-304. Ten rules paint the seven blocks and three body materials (D-307), and each body face reads its tile at 32 texels per meter (D-308). A Game flag renders the contact sheet at game zoom for review (D-306).
Gate: the owner approves the first contact sheet.
> *In plain English:* a tool paints every texture from a fixed set of about thirty colors, so the whole game looks like one thing.

**PR-60: Fullscreen window and the test exit.** ✅ Merged 2026-09-13 as PR #58.
Open the game window in borderless fullscreen at the resolution of the display, on every desktop and on the Deck (D-310). End a session on the Escape key or the controller Start button during tests, until PR-53 brings the escape menu (D-311). One PR carries both changes before PR-15 (D-312).
Gate: a test covers the window mode and each quit input, and a headless session still passes.
> *In plain English:* the game fills the screen that it runs on, so it is no longer a small box on a large display. During testing, Escape or Start closes it.

**PR-61: User argument check.** ✅ Merged 2026-09-13 as PR #60.
Read the user arguments of the Game layer once at boot, with one parser (D-313). The parser holds each flag and the count of words after it. An unknown word, an unknown flag, a repeated flag, and a flag with too few words each stop the boot. A flag that the session ignores stops it too: the contact sheet flag with another flag, and the smoke flag with the bot flag (D-317). The error names the word (T-2).
Gate: a test covers each kind of bad argument, and the user arguments of every session command in `CLAUDE.md` still parse.
> *In plain English:* the game ignores a typo in a test command today, and a test can pass while it runs the wrong command. After this change, the typo stops the game with a message that names it.

**PR-15: Player entity and the first weapon.** ✅ Merged 2026-09-14 as PR #62.
Implement the player in Core: sprint, jump, dodge on a cooldown, health, stagger, and one sword with windup, active, and recovery frames (D-25, D-27, D-28, D-29, D-314, D-315). PR-15 builds the zero-weight case, and PR-22 adds the effects of weight (D-316). The owner answers of D-319 to D-337 set the roll, the stagger and its guard, the arc, the death, the sword model, and the clips. The attack bit swings the sword, and the Phase 1 shot ends (D-320). Play the animation files of D-298 and implement the procedural locomotion in Game (D-87). A test asserts that the animation file agrees with the Core time values. The PR also carries `README.md` with the launch steps (D-318).
Gate: the owner confirms the sword feels committed and readable, and approves the contact sheet with the sword (D-330).
> *In plain English:* you can run, jump, dodge, and swing a sword, and the swing shows its wind-up so you can read an enemy.

**PR-63: Dig sizes in the floor template.** ✅ Merged 2026-09-14 as PR #65.
Move the dig sizes from Core constants to the floor template (D-342), and set the wide sizes of D-341. The validator rejects an even tunnel width and a size that the floor cannot hold (D-352). Every band takes one floor of 64 by 20 by 64 and 5 to 9 chambers with a budget of 100 (D-343, D-344). The chamber kinds take the larger box ranges of D-341. The PR-9 and PR-59 property tests, the bot sweep, and the night sweep run on the new sizes. The simulation version rises (G-20). The dig tail of F-98 waits for PR-67, and PR-63 keeps the job cap of D-279 (D-353). D-359 supersedes that cap in PR-67.
Gate: the property tests and the night sweep pass on the new sizes, and the owner confirms that floor 1 no longer feels cramped.
> *In plain English:* the tunnels and chambers are small today, so a fight feels cramped. This change makes every space wider and taller, and the sizes live in data files.

**PR-67: Dig restart.** ✅ Merged 2026-09-14 as PR #69.
When a dig runs 1000 jobs with a chamber still in rock, dig the floor again (D-353, D-359). The restart draws the chamber kinds again from the next draws of the Procgen stream (D-159, D-361). The floor still comes from the seed and the floor number alone. A floor with no complete dig in 4 digs is an error (D-360). The floors that needed more than 1000 jobs change, so the simulation version rises (G-20). The 7 floors of F-98 become regression tests, and a sweep of at least 675000 floors reports zero dig errors.
Gate: every floor of F-98 digs, and the property tests, the bot sweep, and the night sweep pass.
> *In plain English:* about one floor in 96000 fails to dig on the new sizes. This change digs such a floor again from the same seed, so no run stops on it.

**PR-64: Ramp cells in Core.** ✅ Merged 2026-09-15 as PR #71.
Add the ramp cell to the voxel grid: a sloped floor that rises one block over two, three, or four blocks along one of four directions (D-345, D-346). A body walks up and down a ramp with no jump. The camera boom and projectiles stop at the slope (D-246). The reachability search reads a ramp as a walk (D-165, as D-345 revises it). A ramp cell is a block id from 8 to 43 (D-367). The speed along the slope is the flat speed, a walk stays on the slope on the way down, and a roll leaves it (D-362, D-363). A body does not slide, and it jumps and rolls from a ramp (D-364 to D-366). The simulation version rises (G-20).
Gate: property tests assert no tunnel through a ramp at maximum speed, and the bit-identity job passes with a ramp run.
> *In plain English:* today a height change is a row of whole-block steps, and each step needs a jump. This change adds a sloped block that bodies walk up and down, with the same result on every machine.

**PR-65: Ramp meshes in Game.** ✅ Merged 2026-09-15 as PR #73.
Extend the greedy mesher of PR-13 to ramp cells: a sloped face, and each side and end that shows, with the raw stone tile at 32 texels per meter (D-308, D-368). A face beside a ramp shows over the slope, and the vertex occlusion of D-81 reads the upper half of a ramp as a block. The chunk mesh budget of D-291 holds. The contact sheet shows each slope at game zoom (D-306).
Gate: mesher tests cover each slope and direction, and the owner approves the ramps on a contact sheet.
> *In plain English:* the game can draw only whole blocks today. This change draws the sloped blocks of PR-64 with the same textures and shade as the walls.

**PR-68: The shaft landing fix.** ✅ Merged 2026-09-16 as PR #75.
Make the dig give every shaft a landing that a body reaches from the spawn (D-370, F-101). Seed 79146, floor 7 becomes a regression test that fails on `4bc8cd4`. The simulation version rises when the dug floors move (G-20).
Gate: the seed regression test passes, and a night sweep of 100000 seeds passes.
> *In plain English:* one floor in a hundred thousand drops the player down a shaft into a space with no way back. This change joins every shaft landing to the rest of the floor, and that floor becomes a test.

**PR-69: The night record guards the ref.** ✅ Done in PR #80.
Give the two publish steps of `night.yml` the condition `github.ref == 'refs/heads/main'` (D-373). A night on another ref runs every step and writes no record, so it cannot replace the record that the `night-gate` job reads for `main`. A branch night proves a fix through its run log alone. The read rule of D-275 does not change.
Gate: a test pins the condition on both steps, a branch night leaves the record as it was, and a night on `main` writes it.
> *In plain English:* a night on a side branch can wipe the record that tells every pull request the main line is healthy. This change lets a side branch run the night, and that record stays as it was.

**PR-70: The skill port.** ✅ Done in PR #81.
Split the agent instructions, so a session loads the detail of one step at the step that needs it (D-383). The `gitar-review` skill takes the effective head and the metadata set of D-184, so a commit of metadata keeps a pass current. The `ste-writing` skill takes a glossary of the process terms and a table of the byte ceilings. The `pr-review` skill keeps the procedure and moves each case to a reference file. The `one-pr-one-session` skill keeps the binding, the start gate, the documents matrix, and the completion gate (D-385). A new skill, `csharp-conventions`, holds the code rules, and both agent files point to it. A new runbook, `docs/runbooks/session-context.md`, holds the targeted reads, the commit command, the two waits, the comment export, and the staged read of a diff. Every `.md` file under `.claude/skills/` takes one byte ceiling (D-384). The STE checker holds the front matter of a file to rule 6.3 alone (D-386).
Gate: the context budget test reads every skill file, the checker test pins the front matter rule, and `ste-check` reports no finding.
> *In plain English:* every session reads the same instructions before it starts work, and those files grew too large to read cheaply. This change splits them, so a session reads the detail of one step only when it reaches that step.

**PR-66: Ramps and chamber tiers in the generator.** ✅ Done in PR #82.
Dig ramps in place of one-block steps, so a tunnel changes height by a ramp or a shaft alone (D-345, D-347). Each ramp takes a slope from the list of its floor template (D-346). The tier chance of each chamber kind gives some chambers a tier 2 blocks over the floor, and a ramp joins the two (D-348 to D-350). The detail pass of PR-59 keeps every ramp clear. The PR-9 and PR-59 property tests, the bot sweep, and the night sweep run again. The simulation version rises (G-20).
Gate: no tunnel over the seed sweep holds a one-block step, and every ramp slope comes from its template. The owner confirms the ramps and the tiers in play.
> *In plain English:* the mine joins its levels with smooth ramps of three slopes in place of steps. Some chambers get a raised floor, so a fight can use the high ground.

**PR-16: First enemy family, AI, and pathfinder.** ✅ Done in PR #83.
Implement the A* grid pathfinder that understands jumps, drops, and ramps (D-76, D-345). Implement the scavenger, the first humanoid family, with Core AI: the wake on sight, the approach along a path, the swing, and the retreat on the cooldown (D-30, D-31, D-395 to D-402). Fill the chambers past the first with enemies for their weights (D-167, D-398). Add the full-clearer bot policy, and give every policy the roll and a swing at what stands in reach (D-149, D-403, D-404). Fix the wedge of a body against a ramp slope (D-405, F-104, F-105). Bot runs cover it.
Gate: the bot sweep passes with enemies active, and no run reads a crash or a softlock.
> *In plain English:* the first enemies find their way through the dungeon and fight by the same rules you do.

**PR-17: Floor timer, hunter, and escalation.** ✅ Done in PR #84.
Implement the visible per-floor timer with lengths in data (D-44, D-46, D-407). Implement the hunter as one entity with a speed curve that grows until escape is impossible, and the escalation spawner (D-45, D-408 to D-410). A death carries its cause (D-411). The rules of the hunt and the waves are D-413 to D-419, and a bot that promises progress reads softlock at expiry (D-420). The timer pauses at the stairwell and runs in boss fights (D-140). Add the timer-tester bot policy (D-149).
Gate: a timer-tester bot always dies to the hunter, and a greedy descender rarely meets it.
> *In plain English:* each floor has a clock. When it runs out, an unstoppable hunter arrives and gets faster, so a wait is never the safe choice.

**PR-18: Stairwell and floor transition.** ✅ Done in PR #85.
Implement the stairwell with the untimed descend-or-ascend prompt, open while the body stands on the stairwell cell (D-50, D-140, D-431). Implement the floor transition with the next floor generated on a worker during the current floor (D-72, D-429). A test asserts the worker output equals a synchronous generation for the same seed. Add the coward bot policy and the `ascend` end state (D-149, D-430, D-433), and extend the smoke session to the stairwell (D-436).
Gate: the transition shows no frame over 22 milliseconds on the Deck (D-427, D-435).
> *In plain English:* you reach the stairs, choose to go down or leave, and the next floor already exists, so there is no pause.

**PR-19: HUD and controller navigation.** ✅ Done in PR #86.
Build the HUD in C# Control nodes: health, timer, damage numbers, and a boss bar placeholder (D-36, D-90, D-441 to D-445). Numbers must not cover silhouettes (F-24). Place the stairwell prompt, and let a hold of interact ascend (D-447, D-448). Build the controller navigation base for every later screen on one layout scale at Deck size (G-15, D-446, D-449).
Gate: the HUD reads at 800p and every element works with a controller.
> *In plain English:* the on-screen numbers and bars appear, sized for the smallest screen, and every menu works with a gamepad from the start.

**PR-20: Audio synthesizer and first effects.** ✅ Done in PR #87.
Implement the C# synthesizer that renders each sound from its file to a WAV file (D-93, D-462). A noisy sound comes from a spectral layer: the band levels that the analysis takes from a CC0 reference (D-459, D-461, D-464). A pitched sound ships as its CC0 recording in a recording layer (D-467). Ship the first set: sword, footsteps, dodge, hit, hunter, and timer warnings, with the rendered files in the repository (D-453). Bind them in Game to the action events of Core and to a stride rhythm, on four buses (D-452, D-454, D-456, D-463). The hunter step takes the pitch of its speed (D-455).
Gate: the owner approves the sword and hunter sounds (D-457, D-470).
> *In plain English:* a tool makes every sound from a recipe, and the first sounds give the sword and the hunter their weight.

**PR-71: CI skip and faster tests.** ✅ Done in PR #89.
Skip the heavy jobs of `ci.yml`, `bit-identity.yml`, `smoke.yml`, and `bots.yml` on a PR head that changes documents alone: a PR of documents alone, or a push of documents after a green head (D-473 to D-477). The `ci-skip` command of Tools holds the rules. The tests that read a document run on each head (D-476). Split `ProcgenTests` into nested classes and each hosted leg into two jobs (D-478, D-479). A pull request runs one fifth of each seed sweep (D-480, D-481). F-109 holds the measurements.
Gate: a documents push after a green head skips the heavy jobs. A code head of the PR records the time of each job against run 35771495463.
> *In plain English:* each push waited up to 21 minutes for the full checks, also a push that changed a document alone. Such a push now skips the heavy checks, and a pull request runs fewer seeds on more runners.

**PR-72: Enemy diagonals and ramp climb.** ✅ Done in PR #90.
Fix F-107 and F-108 in one PR (D-483, D-484). The path search takes a diagonal move where two side moves reach the corner column (D-486). A diagonal move costs 14, and a side move costs 10 (D-487). A diagonal move rises one block at most, and it needs the start and the corner columns open. It passes one solid corner, but a step up passes none (D-489). The enemies, the Overseer, and the bots walk it, and `Reachability` keeps the side moves (D-488). The jump rule reads the slope under the feet, so a climb of a ramp takes no jump (D-485). The simulation version rises to 15 (G-20).
Gate: the enemy walk tests, the diagonal walk sweep, the bot tests, and the new bit-identity answer pass.
> *In plain English:* enemies walked in staircases of side steps and hopped up ramps. They now cut corners where a player can, and they walk up a ramp as the player does.

**PR-73: No suite for documents.** ✅ Done in PR #91.
A change of documents alone runs no full test suite for the author, a review, a review response, or a handoff (D-490, D-491). It runs `ste-check`, `doc-gate`, and the `Documents` category of D-476. The skip set of D-475 names the documents (D-492). A change with any other path runs the full suite (D-493). The test line of the PR gate says so (D-494).
Gate: the agent files, the skills, and the registers agree on the rule, and the `Documents` category passes.
> *In plain English:* each session ran every test also for a change that touched only a document. Now such a change runs only the checks that read documents.

**PR-62: Texture recipe system.** ✅ Done in PR #92.
Split from the art pass of D-339 (D-504). A recipe under `content/textures/recipes/` is an ordered list of paint layers: `fill` with noise, `edge`, `rect`, `band`, and a color swap of a recipe that extends another (D-505, D-507). A file next to each model names a recipe for each box and face (D-508). The generator sizes each face at 32 texels per meter, packs it into an atlas of 512 (D-506), and writes `content/textures/layout.json`. Game reads each place from the layout. Every block keeps its pixels, and the body and the sword keep their materials (D-504). The skill `asset-texture-creation` gives the art steps of every asset, from the Meshy prompt to the box model (D-509).
Gate: the atlas and the layout match the generator, each block matches its old tile, and the owner confirms that the look stayed the same.
> *In plain English:* each box face read one plain tile of noise, so no box showed a face, a belt, or a boot. Now each face gets its own painted canvas, and nothing changes on screen yet.

**PR-78: Codex review and auto-merge.** ✅ Done in PR #93.
`make codex-review PR=<n>` starts the cross-provider review through the Codex CLI in a detached worktree, and the `codex-review` command judges the record that the review pushes (D-511, D-512). A finding open in three rounds stops the fix loop for the owner (D-513 to D-515). A ruleset on `main` requires every gate check, and a PR with the green light merges by auto-merge after the owner confirms (D-516, D-517, D-520 to D-522, D-524). The review uses the ChatGPT login alone (D-523).
Gate: the new tests pass, the command reviews this PR, and each required check reports on a documents head and on a code head.
> *In plain English:* the owner started every review by hand and merged every PR by hand. Now one command starts the review, and a PR that passes every gate merges itself.

**PR-74: Body art.** ✅ Done in PR #94.
The body gains a brow, a nose, and a beard on the head bone, and a toe box on each lower leg (D-497, D-498, D-501, D-502). Every other box stays (D-499). The face and the trim follow D-525 and D-526. The recipes gain fine shades, a clustered grain, and a gradient, so the body carries the detail of the 3D reference (D-527, D-528). A ninth ramp, umber, holds the dark browns, and each material takes the measured shade of the reference (D-529 to D-531). The owner approved the contact sheet as finished art (D-532). The Meshy images are a look reference alone (D-496).
Gate: the clip check passes at every keyframe, and the grain paints the same bytes on each platform. The owner approves the contact sheet as finished art.
> *In plain English:* the miner was ten plain boxes with speckled paint. This change adds a brow, a nose, a beard, and boots, and paints the body with the soft mottle of the 3D model.

**PR-79: Review process after approval.** ✅ Done in PR #95.
The effective head that `review-gate` and `make codex-review` read skips each commit whose paths all lie in the skip set of D-475 (D-534). An approving review then stays green after a later documents commit. The gitar pass and the override label keep the metadata set of D-184 (D-539). A PR of documents alone needs the `review-override` label, and the label covers each path of the skip set (D-540, D-541). Before the owner confirms a merge, the session writes the merge summary: What, How, CI, and Codex review (D-533).
Gate: the review gate and the review command tests pass, and the merge confirmation of this PR uses the merge summary.
> *In plain English:* a fix of one word in a document after the approval of a PR asked for a second full review. Now a change of documents alone keeps the approval, and gitar still reads each push.

**PR-80: Gitar pause.** ✅ Done in PR #96.
The owner pauses the gitar requirement until a later PR of the owner (D-542). The author answers each gitar comment that comes in. A gitar review with feedback stops the session, and the session alerts the owner. `make codex-review PR=<n> -- --skip-gitar-review` drops the Gitar start checks and keeps the thread check. The flag stays after the pause (D-543). This PR goes before the night fix of D-538 (D-544). PR-87 ends the pause (D-574).
Gate: the review command tests pass, and the review of this PR runs with the flag.
> *In plain English:* each PR waited for an automated review before the second review. The owner pauses that wait. A review that still comes in gets an answer, and the owner hears about it at once.

**PR-81: Night fix and branch nights.** ✅ Done in PR #97.
The fix of F-111 (D-538). A diagonal drop needs an open fall in the corner column (D-545). An arrival at a waypoint starts the wedge count of `PathFollower` again (D-546). The sweep builds each box in the form of the caller, so a body never ends one ulp inside a block (F-112, D-549). The simulation version rises to 16 (G-20). A night on a branch writes its record to `night-branch/<branch>`, and `main` alone writes `night-results` (D-373, D-538). The `night-gate` job of a PR passes on a success record of its branch night at the effective head or later (D-547). The branch night then re-runs the gate of its PR (D-548). A gitar notice, a comment with no specific item, needs no answer and blocks no verdict (D-550).
Gate: the night of this branch passes the bot sweep and the seed sweep. The `night-gate` job of this PR reads that record green.
> *In plain English:* the enemy walk fix of PR-72 left the test bot stuck on 26 of 5000 floors, and every merge then waited for a manual override. The bot now walks those floors, and a fix PR can prove itself with its own night run.

**PR-82: Template gitar line.** ✅ Done in PR #98.
The gitar line of the PR template takes the rule of D-550, so a gitar notice needs no answer there too (D-551, D-553). A test holds the template line equal to the gitar line of the PR gate in the agent files (D-554).
Gate: the test passes, and it fails on the old template.
> *In plain English:* the agent files let a gitar comment with no specific item go without an answer. The checklist of each new PR still asked for an answer to every gitar comment. Now both say the same thing.

**PR-83: Night record promotion.** ✅ Done in PR #99.
A job on each push to `main` finds the merged PR and reads its branch night (D-557). The night promotes to the record of `main` at the merge commit when three things hold. It passed inside 48 hours of its end (D-556). Its tree differs from the merge commit only in the skip set (D-555). The record of `main` names an older commit (D-558). A night on `main` keeps a record at a later commit (D-562). Each new record re-runs the gate of each open PR (D-559).
Gate: the promotion tests pass, and the `night-gate` job of this PR reads green.
> *In plain English:* a PR that proved itself with its own night run still left the main branch red after the merge. Now that green result carries over, when the merge changes only documents after the night.

**PR-84: Night fixed seeds.** ✅ Done in PR #100.
The fixed seeds of each night stay the gate, and each night also runs a slice of one tenth past them (D-564, D-566). The UTC date of the night start selects the window, and the run log and the record name it. A slice failure fails the night (D-565). Each later night runs a failed seed again until a night passes it (D-567). The fix PR adds the seed to the extra fixed seeds. A promotion needs a branch night that ran each carried seed (D-569).
Gate: the seed tests pass, and a branch night of this PR names the slice of its date.
> *In plain English:* each night tested the same seeds, so a fault past them stayed hidden. Now each night also tests a new batch that the date picks, and a failure blocks merges until a fix.

**PR-85: Night on hosted Linux.** ✅ Done in PR #102.
The night cron moves to 07:07 UTC (D-571). The night moves to hosted Linux as parallel jobs (D-572, D-573). One job takes the date and the record of `main`, one job runs each sweep, and one last job writes the record. The carry rules of D-567 and D-569 stand.
Gate: a branch night on hosted Linux ends and names the slice, and the shape tests read the new cron and jobs.
> *In plain English:* the night ran on the Mac of the owner and blocked the checks of every PR for hours. Now it runs on free cloud machines, in parallel.

**PR-87: Gitar pause ends.** ✅ Done in PR #103.
The pause of D-542 ends, and the automated pass is a gate again (D-574). After each push, `make gitar-wait` waits 60 seconds, then reads the gitar check run of the head every 30 seconds. It ends when that run completes and the dashboard shows the review. With no check run at 6 minutes it posts one `Gitar review` comment, and at 15 minutes it stops (D-575). This PR comes after PR-85 and before PR-86 (D-576). A review round starts only on green CI, the Review gate workflow aside (D-577).
Gate: the wait tests pass, and the gitar pass and the review of this PR run with no flag.
> *In plain English:* the owner paused the automated review of each PR while it did not work. It works again, so each PR waits for it again. A script watches for the review, and it asks for one when none starts.

**PR-88: Repository review fixes.** ✅ Done in PR #104.
Fix the verified findings F-113 to F-140 of the repository review of 2026-09-24 (D-578). A run that ends in a tick takes no stairwell choice, and the deepest floor offers the ascend alone (D-322, D-579). Each engine callback of the Game catches every exception and quits with exit code 1. The review gate reads the verdict from the first line of its section. The seed sweep names a seed whose dig throws. The asset gate rejects a repeated key and reads each depth of the model directory. Content bounds fit their consumers, and each error of a run names the seed and the floor. The state hash reads the path of each follower, and the gate tools close their edge cases. The lint, the bit-identity sweep, the attribution check, and the STE check see more, and the registers match their decisions. This PR holds more than one concern (D-580). The simulation version rises (G-20).
Gate: a regression test for each finding fails on the old code, and the suite, the smoke session, and the bit-identity sweep pass.
> *In plain English:* a review of the whole repository found faults. A death at the stairwell crashed the game, and some checks passed a fault that they must catch. This PR fixes each one, with a test.

**PR-86: Hosted macOS legs.** 🔧
The macOS legs of `ci.yml`, `smoke.yml`, and `bit-identity.yml` move to the hosted macOS arm64 runner, and the self-hosted runner retires (D-572, D-573).
Gate: the three macOS legs pass on the hosted runner, and no workflow names the self-hosted label.
> *In plain English:* the last checks leave the Mac of the owner. No code from a pull request runs on that Mac again.

**PR-75: Sword art.** 🔧
The sword of PR-15 gains the detail that the owner asks for, on the recipes of PR-62 (D-504).
Gate: the clip check passes, and the owner approves a contact sheet of the sword.
> *In plain English:* the sword is three plain boxes. This change gives it the detail of a finished weapon.

**PR-76: Enemy models.** 🔧
The enemy models of PR-16 gain their own boxes and recipes, and the color swap gives each enemy its colors (D-504, D-507).
Gate: the clip check and the smoke session pass, and the owner approves a contact sheet of the enemies.
> *In plain English:* the enemies borrow a first-pass look. This change gives them their own bodies and colors.

**PR-77: Scene light and edge smoothing.** 🔧
The scene light moves toward the torchlight of D-59, inside the budget of D-81. The antialiasing mode and the texture filter of OQ-181 follow a measurement on the Deck against D-295 (D-504). OQ-181 blocks the start.
Gate: a frame log on the Deck meets D-295, and the owner approves a contact sheet with the new light.
> *In plain English:* the light is one flat setting, and block edges look jagged on the Deck. This change adds torchlight and smooth edges inside the frame budget.

**M-3: Steam Deck frame time.** 🔧
Measure the 99th percentile frame time on the Steam Deck OLED of D-296 over one full floor, with the target of D-295. Binds every render PR (F-3).

### Phase 3: Full loop (gate: hub, loadout, death loss, bank, tree, saves, and replay resume work, and friends play)

**PR-21: Items, tiers, and affixes.** 🔧
Implement item definitions in JSON with tiers by depth band and a rare higher-tier chance (D-48, OQ-22). Implement rarity and random affixes as a fixed set of behaviors that any wielder gets (D-47, D-49). Property tests assert the band distribution.
Gate: the affix set has one test per behavior, for the player and for an enemy.
> *In plain English:* items get random extra powers, and an enemy that carries such an item uses the power against you.

**PR-22: Equipment slots, armor overlays, and weight.** 🔧
Implement the modeled slots and the two ring slots (D-18, D-55). Attach armor overlays and the shield to the shared base body (D-82). Implement damage reduction and weight on movement and dodge (D-23, D-26). Set the growth of the dodge cooldown with weight and the weight at which armor resists stagger (D-314, D-316).
Gate: the PR-57 pose check passes for every armor piece on every animation (D-135).
> *In plain English:* what you wear shows on your body. Heavy pieces make you slower and harder to stagger. The pieces never clip through each other.

**PR-23: Satchel, quick slot, throwables, potions, and weapon swap.** 🔧
Implement the satchel with a slot count from OQ-3 (D-19). Implement the quick slot, bombs with full self-damage, health and mana potions, and the slow uncancelable weapon swap (D-21, D-22, D-24, D-32).
Gate: a full satchel forces a drop choice, and the choice works on a controller.
> *In plain English:* you carry a small bag. Bombs, potions, spare weapons, and found amulets all compete for its few slots.

**PR-24: Bow and musket.** 🔧
Implement one bow with a fast draw and small area arrows, and one musket with a slow reload and a flat shot (D-40, D-42, OQ-13). The attack bit fires the ranged weapon of the loadout (D-320). Enemies use both (D-30). Contact sheets prove the two projectiles are distinct in flight.
Gate: the owner confirms the musket reload feels like exposure and the bow feels like pressure.
> *In plain English:* the first ranged weapons arrive. A shot from a musket is a commitment, and an arrow is quick and spreads its damage.

**PR-25: Exotic weapon and mana.** 🔧
Implement mana with slow regeneration and one exotic weapon with a charge time (D-33, D-43). Mana potions restore it.
Gate: a bot with only an exotic weapon can finish a floor.
> *In plain English:* magic is a weapon that spends a bar that refills slowly instead of a reload.

**PR-26: Enemies wear what they drop.** 🔧
Generate humanoid enemy loadouts from the loot tables at spawn, compose their models from the gear, and drop that gear on death (D-16). Show rarity color on the enemy (D-49). Monsters show their drop per OQ-23.
Gate: a bot log confirms every dropped item was on the enemy that dropped it.
> *In plain English:* you can see the good weapon on the dangerous enemy before you fight, which is the whole point of the title.

**PR-27: Skill points and the death payout.** 🔧
Implement points per kill with a boss bonus, the run total, and the depth-scaled retained share on death (D-44, D-52, OQ-21). Instrument points per simulated hour by policy in the run log for M-5. Implement the M-5 trial harness (D-154). It uses one thousand fixed seeds, the basic kit and an empty tree as the initial state, and the policy set. Time is the tick count over 60 plus a sixty-second hub cost per run start. The statistic has a bootstrap 95 percent interval. A Tier 3 session runs on this PR (D-128), which is why PR-32 precedes it.
Gate: the M-5 pass condition holds. At every depth the ascend lower bound exceeds the die upper bound, and deep ascend beats shallow repeat.
> *In plain English:* kills earn points. A death keeps a share that grows with depth, so a quick shallow death never pays better than a real run.

**PR-28: Amulet.** 🔧
Implement the permanent amulet with one active and one passive on cooldowns (D-34, D-38, D-39, OQ-7). Implement found amulets as satchel items that unlock skill orbs on ascension (D-37, D-41). The tree unlocks the first orb, a basic active, from the start (D-153).
Gate: the amulet survives a death in a bot run, and a found amulet does not.
> *In plain English:* you always keep one amulet with your chosen power. Amulets you find unlock new choices, but only if you make it out.

**PR-29: Skill tree and tree screen.** 🔧
Implement the tree from JSON with shallow branches, orb-gated tips, proficiency buffs, and free respec (D-35, D-51, D-54, OQ-8). Build the tree screen for controller and Deck.
Gate: every node is reachable with a controller alone.
> *In plain English:* the between-run upgrade screen arrives, and you can change your choices freely.

**PR-30: Hub, bank, and loadout.** 🔧
Build the hub scene with the bank, the skill shrine, the loadout screen, and the descent entrance (D-9). The loadout screen compares affixed items (D-2, D-47). It always offers the basic kit: a tier-0 sword with no affixes, at no cost, not stored in the bank (D-153).
Gate: a run started from a loadout loses that loadout on death and banks found gear on ascension. A fresh profile starts a run, and a profile that lost every banked item starts another.
> *In plain English:* the camp between runs is where you choose what to risk. You can lose what you carry down.

**PR-31: Persistence and replay resume.** 🔧
Implement the profile file with the tree, the bank, the suspended-run pointer, a schema version, a generation number, migrations, and an atomic write (D-94, D-152). Implement the run record file per PR-6, with a run id that the profile records on completion. Implement suspend anywhere and resume by replay to five seconds before the exit tick (D-97). Resume is exact when the simulation version and content hash match, and floor start with a notice otherwise (D-151). Test a process kill at each write boundary: departure, ascension, death, and migration.
Gate: a kill of the process mid-floor resumes five seconds earlier. A kill at every write boundary leaves one consistent profile with no duplicated reward. The loader refuses a corrupted file with a report.
> *In plain English:* you can stop at any time and come back to almost the same moment. A crash at any instant leaves your bank whole and never pays a reward twice.

**PR-32: LLM play socket and log reader.** 🔧
This PR precedes PR-27, because D-128 requires Tier 3 play on every economy PR (D-149). Expose the compact state JSON over a socket at one to five decisions per second (D-128). Add the log aggregation that hands outlier runs to a reader. Neither runs unattended (D-117).
Gate: one weekly session files its findings in the questions register.
> *In plain English:* a language model can play the game slowly through a text channel to hunt for exploits that a robot does not try.

**M-4: Tokens per PR.** 🔧
Record tokens per PR from the harness usage reports for ten PRs. Informs the budget in D-107.

**M-5: Points per hour by policy.** 🔧
The matched-trial protocol of D-154, run from PR-27 onward. Binds the death payout curve (OQ-21) and F-11, F-20, F-33.

### Phase 4: Content complete (gate: the D-56 roster, three bosses, a Tier 4 pass, and two friend playtests)

**PR-33: Boss framework and boss 1.** 🔧
Implement the boss room, the boss bar with a number (D-36), and boss patterns as JSON. Ship boss 1 on floor 5 (D-6, OQ-11).
Gate: a bot policy can defeat boss 1, and the owner confirms the telegraphs read.
> *In plain English:* the first big fight arrives with a health bar you can read and attacks you can see before they land.

**PR-34: Boss 2.** 🔧
Ship boss 2 on floor 10.
Gate: same as PR-33.
> *In plain English:* the second big fight.

**PR-35: Boss 3 and the ending.** 🔧
Ship boss 3 on floor 15 and the v1 ending sequence (D-5).
Gate: a full 15-floor run ends with the ending, and the run time is inside 30 to 45 minutes (D-4).
> *In plain English:* the last fight and the end of a full descent, at the length the design promised.

**PR-36 to PR-42: Enemy families two to eight.** 🔧
One PR per family, humanoid or monster (D-31, D-56, OQ-9). Each family has a silhouette specification before its model, a contact sheet at game zoom, and bot coverage.
Gate per PR: the family reads as distinct on the contact sheet.
> *In plain English:* seven more kinds of enemy, each added alone, so the owner can judge its shape and its behavior on their own.

**PR-43 to PR-46: Weapons to twelve.** 🔧
One PR per weapon class batch: melee, bows, guns, exotics (D-42, D-56, OQ-10). Every weapon is data. Each projectile is distinct in flight on a contact sheet.
Gate per PR: no new code in Core for a new weapon.
> *In plain English:* the weapon list grows to about twelve, and each one is a data file, not a program change.

**PR-47: Affixes to ten.** 🔧
Grow the affix set to about ten behaviors (D-56). Each has a test for the player and for an enemy.
Gate: Tier 3 play finds no dominant affix.
> *In plain English:* more random item powers, each proven to work for you and against you.

**PR-48: Destructible props.** 🔧
Implement props as entities without AI that break (D-79). Walls stay permanent.
Gate: the pathfinder never re-plans for a prop.
> *In plain English:* barrels and crates break. Walls do not, so no wall can stop the hunter.

**PR-49: Asset QA gate v2.** 🔧
Extend the PR-57 tool with the polygon budget, pivot placement, and UV coverage checks (D-135).
Gate: the tool fails a model over the polygon budget and a model with a bad pivot.
> *In plain English:* the inspection from PR-57 learns three more checks, so every model meets its budget before it enters the game.

**PR-50: Music.** 🔧
Implement the sequencer format and render the first tracks with the synthesizer (D-93). The owner's ear is the gate (F-18).
Gate: the owner approves a hub track and a dungeon track.
> *In plain English:* music made from recipes. If it does not sound good enough, the decision to generate it opens again.

Tier 4 vision pass at the phase gate (D-133).

### Phase 5: Early Access candidate (gate: a Steam build on three platforms, Deck verified checklist met, notarized macOS build)

**PR-51: Export pipeline and notarization.** 🔧
Set up the Windows, Linux, and macOS universal exports. Sign and notarize the macOS build (D-142, OQ-18). Pre-warm shaders at load.
Gate: an exported build runs from a clean install on all three platforms.
> *In plain English:* the game becomes something a player can install, also on a Mac that blocks unsigned software.

**PR-52: Steamworks.** 🔧
Add achievements and cloud saves with an explicit conflict rule (D-96, OQ-19). Record the dependency (G-16).
Gate: a save conflict shows a prompt and never resolves in silence.
> *In plain English:* your bank follows you between machines, and a clash between two copies asks you instead of a guess.

**PR-53: Settings and accessibility.** 🔧
Build the settings screen. Complete the string table (D-98). Add accessibility options per OQ-14. Build the escape menu on the Escape key and the controller Start button, and remove the test exit of PR-60 (D-311).
Gate: every option works with a controller, and Escape opens the escape menu and never ends the game.
> *In plain English:* the options screen, the escape menu, complete text, and any assist options the owner chooses.

**PR-54: Steam Deck verification pass.** 🔧
Controller glyphs, 800p text sizes, default settings that hit the M-3 target, and the Deck checklist (D-15).
Gate: the Deck checklist passes.
> *In plain English:* a test confirms that the game plays well on the handheld.

**PR-55: Crash report flow.** 🔧
Build the UI flow that shows a report after an assertion failure and offers to save the seed and intent record (D-112).
Gate: a forced assertion produces a report a session can replay.
> *In plain English:* when something goes wrong, the game hands the player a file that lets us replay the exact moment.

### Phase 6: Parked

- **PR-56: Endless mode past floor 15.** 🅿 After v1 (D-5).
- **Heat modifiers.** 🅿 After v1, in endless mode (D-53).
- **Mod loader.** ⏸ Unsupported, not prevented (D-99).
- **Co-op.** ⏸ Never (D-12).

## 8. Sequence (strict order, single owner, rewritten 2026-09-07)

One person owns the program. Items run one at a time in this order. The list changed on 2026-09-07 after the repository audit (D-147, D-149, D-150). The audit findings come first. PR-57 enters Phase 2. PR-58 enters Phase 1. PR-32 precedes PR-27. Gate 1 is a foundation gate.

1. Address every audit finding (D-147). ✅ Done 2026-09-07: D-148 to D-154, F-28 to F-37.
2. Owner: receive the external SSD and move the checkout to it (D-145). ✅ The SSD arrived 2026-09-07 (D-192), and the checkout is on it.
3. Owner: register the runner on 2026-09-08 (D-157, D-171). ✅ OQ-2: D-173. ✅ OQ-16: D-175. ✅ OQ-30: D-156. Protection deferred: D-170. ✅ The owner registered the runner on 2026-09-07 (D-192).
4. PR-1, PR-2. ✅ PR-1 merged 2026-09-08 as PR #6. ✅ PR-2 merged 2026-09-08 as PR #10.
5. PR-3, PR-4, PR-5. ✅ PR-3 merged 2026-09-08 as PR #12. ✅ PR-4 merged 2026-09-08 as PR #15. ✅ PR-5 merged 2026-09-08 as PR #17.
6. PR-6, PR-7, PR-8. ✅ PR-6 merged 2026-09-09 as PR #19. ✅ PR-7 merged 2026-09-09 as PR #21. ✅ PR-8 merged 2026-09-09 as PR #23.
7. PR-9, PR-59, PR-10, PR-11. One night runs, scheduled or by hand, then PR-58 (D-177, D-278). ✅ PR-9 merged 2026-09-09 as PR #27. ✅ PR-59 merged 2026-09-09 as PR #29. ✅ PR-10 merged 2026-09-10 as PR #31. ✅ PR-11 merged 2026-09-10 as PR #34. ✅ Two nights ran by hand 2026-09-10. ✅ PR-58 merged 2026-09-10 as PR #40.
8. M-1, M-2. ✅ M-1 table complete 2026-09-10 (D-276, D-277). ✅ M-2 table complete 2026-09-11, seven nights (D-283).
9. ✅ **← GATE 1 (foundation).** Signed 2026-09-11 (D-288). Nothing below starts until the bit-identity job, `dotnet test`, and the night sweep are green. Gate 1 signs after the first scheduled night passes on its own (D-283).
10. PR-12, PR-13, PR-57, PR-14. ✅ PR-12 merged 2026-09-11 as PR #49. ✅ PR-13 merged 2026-09-12 as PR #52. ✅ PR-57 merged 2026-09-12 as PR #54. ✅ PR-14 merged 2026-09-12 as PR #56.
11. PR-60, PR-61, PR-15, PR-63, PR-67, PR-64, PR-65, PR-68, PR-69, PR-70, PR-66, PR-16, PR-17, PR-18. ✅ PR-60 merged 2026-09-13 as PR #58. ✅ PR-61 merged 2026-09-13 as PR #60. ✅ PR-15 merged 2026-09-14 as PR #62. ✅ PR-63 merged 2026-09-14 as PR #65. ✅ PR-67 merged 2026-09-14 as PR #69. ✅ PR-64 merged 2026-09-15 as PR #71. ✅ PR-65 merged 2026-09-15 as PR #73. ✅ PR-68 merged 2026-09-16 as PR #75. ✅ PR-69 done in PR #80. ✅ PR-70 done in PR #81. ✅ PR-66 done in PR #82. ✅ PR-16 done in PR #83. ✅ PR-17 done in PR #84. ✅ PR-18 done in PR #85.
12. PR-19, PR-20, PR-71, PR-72, PR-73, PR-62, PR-78, PR-74, PR-79, PR-80, PR-81. Then PR-82, PR-83, PR-84, PR-85, PR-87, PR-88, PR-86, PR-75, PR-76, PR-77. ✅ PR-19 done in PR #86. ✅ PR-20 done in PR #87. ✅ PR-71 done in PR #89. ✅ PR-72 done in PR #90. ✅ PR-73 done in PR #91. ✅ PR-62 done in PR #92. ✅ PR-78 done in PR #93. ✅ PR-74 done in PR #94. ✅ PR-79 done in PR #95. ✅ PR-80 done in PR #96. ✅ PR-81 done in PR #97. ✅ PR-82 done in PR #98. ✅ PR-83 done in PR #99. ✅ PR-84 done in PR #100. ✅ PR-87 done in PR #103. ✅ PR-88 done in PR #104.
13. M-3.
14. **← GATE 2.** The owner plays one floor and signs off on feel.
15. PR-21, PR-22, PR-23.
16. PR-24, PR-25, PR-26.
17. PR-32.
18. PR-27, PR-28, PR-29.
19. PR-30, PR-31.
20. M-4, M-5.
21. **← GATE 3.** The full loop works. Friends play (D-134).
22. PR-33, PR-34, PR-35.
23. PR-36 to PR-42.
24. PR-43 to PR-46, PR-47.
25. PR-48, PR-49, PR-50.
26. Tier 4 pass.
27. **← GATE 4.** Content complete. Two friend playtests filed.
28. Owner: buy the Apple Developer account and pay the Steam Direct fee (F-25).
29. PR-51, PR-52, PR-53.
30. PR-54, PR-55.
31. **← GATE 5.** Early Access candidate.
32. Phase 6 stays parked.

## 9. Open questions

The open questions register is `docs/questions.md` (D-144). It holds OQ-1 onward with options, recommendations, what each blocks, and the date and decision that resolve each one. File a new question there, not here. Ids never change.
