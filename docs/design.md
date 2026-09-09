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
| Persistence | Core | three save files | three save files | High. Corruption risk |
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

At each stairwell the player ascends or descends (D-50). Ascension costs nothing. The run ends, and the next run starts at floor 1. The restart is the cost. The timer pauses at the stairwell (D-140). A full run to floor 15 takes 30 to 45 minutes (D-4).

### 3.3 Player

One character. Gear is identity (D-17). The camera is over-the-shoulder and the player controls it (D-13). Aim is free, with aim assist on a controller (D-14). Input targets are keyboard and mouse, controller, and the Steam Deck (D-15). The Deck is the performance floor and the readability floor.

Movement verbs are dodge roll, sprint, and jump (D-27). There is no stamina. Dodge has a cooldown, and armor weight extends it (D-28). Health carries across floors. Potions in the satchel are the only heal (D-24).

### 3.4 Equipment

Modeled slots: head, chest, legs, feet, amulet, shield (D-18). The two ring slots have no model (D-18, D-55). The player equips one main weapon at a time (D-20). Spare weapons ride in the satchel. A swap is slow, and the player cannot cancel it (D-21). The satchel is small and visible (D-19). Consumables in the satchel go to a quick slot (D-22).

Armor gives damage reduction plus weight. Weight slows movement and dodge recovery (D-23). A shield needs a one-handed melee weapon (D-26). Only shields block. Nothing interrupts a two-handed melee swing (D-29). A stagger system exists for the player (F-21).

### 3.5 Combat

Fights are fast and lethal (D-25). No hitscan exists. Every projectile is a simulated object with travel time, drop, and a lifetime. Enemies fire the same projectiles under the same rules (D-30). The player's own bombs deal full self-damage (D-32).

Ammunition is infinite. Reload and draw time are the only cost (D-40). Guns are muzzle-loaders with single-target burst and a slow reload (D-7, D-42). Bows have small area damage and a faster draw (D-42). Exotic weapons use mana. Mana regenerates slowly, and potions restore it (D-33, D-43). The feel references are Hunt: Showdown gunplay and Risk of Rain 2 movement (D-60).

Damage numbers are always on. Bosses show a health bar with a number (D-36). Numbers must not cover silhouettes (F-24).

### 3.6 Amulet and skill tree

Every player always has an amulet. The player never loses it (D-34). The tree upgrades it and assigns one active ability and one passive (D-39). The tree unlocks the first orb, a basic active, from the start (D-153). Amulet abilities use cooldowns (D-38). Found amulets are satchel items (D-41). On ascension a found amulet unlocks a skill orb in the tree. A death forfeits the unlock (D-37).

The tree has several shallow branches. Found-amulet orbs gate the branch tips (D-54). Proficiencies only buff (D-35). Respec is free at the hub (D-51). No difficulty modifiers exist in v1. Depth is the only dial (D-53).

### 3.7 Loot and items

Humanoid enemies wear and wield what they drop (D-16, D-31). Monsters have their own attacks. Items have random affixes (D-47). An enemy shows rarity color only, and it uses its affixes (D-49). Affixes are a fixed set of behaviors that any wielder gets. Item tiers rise by depth band, with a rare higher-tier chance anywhere (D-48). Rings are pure affix carriers (D-55).

### 3.8 Economy

Skill points come per kill, with a boss bonus (D-44). Each floor has a visible time limit. Deeper floors get more time (D-46). The timer runs in boss fights and pauses at the stairwell (D-140). When the timer expires, an unkillable hunter spawns and spawns escalate. The hunter accelerates until escape is impossible (D-45).

Death keeps a share of the run's skill points. The share scales with the depth reached (D-52). Ascension must beat death in points per hour at every depth. This is the sensitive number. M-5 measures it with matched trials in simulated time (D-154). The trials use a fixed seed set, a fixed initial state, paired ascend and die policies at every depth, and a bootstrap confidence interval.

### 3.9 Hub and persistence

The hub is one small scene with a bank, a skill shrine, a loadout screen, and the descent entrance (D-9). One profile file holds the tree, the bank, and the suspended-run pointer, with a schema version, a generation number, and a migration path (D-94, D-152). The game writes it atomically. One profile write commits an ascension or a death. A run id in the profile makes completion idempotent.

A run record is a separate append-only file (D-151, D-152). Its header carries a format version, a simulation version constant, a content hash, the seed, and an immutable initial state. The initial state holds the loadout items with their rolls, the tree state, and the amulet assignment. The fixed 16-byte tick frames of D-162 follow, each with a CRC-32 (D-226). The loader truncates a torn tail. Replay ignores the live bank and tree.

Suspend works anywhere. Resume replays the record to five seconds before the exit tick when the simulation version and the content hash match (D-97, D-151). On a mismatch, or on a repeat crash, resume starts at floor start with a notice and a log line. Steam achievements and cloud saves ship at launch (D-96).

### 3.10 Enemies and bosses

The v1 scope is small (D-56): 15 floors, one biome, 3 bosses, about 12 weapons, about 8 enemy families, about 10 affixes, one hunter. Two enemy archetypes exist: humanoids that use the player gear pool, and monsters with their own attacks (D-31). All AI and pathfinding live in Core on the voxel grid (D-76).

### 3.11 World and art

The world theme is fantasy with black-powder guns (D-7). The v1 biome is a collapsed deep mine, and a blasting charge is a mining tool (D-210). The tone is dark with dry humor (D-8). The look reference is Minecraft Dungeons, pushed darker with torchlight (D-59). The world is a voxel grid of one-meter cubes (D-78). Props break, and walls are permanent (D-79).

Models are cuboid, with a custom proportion set and one shared base body (D-82). Textures are 32 px faces on one atlas from an own palette of about 32 colors (D-85). The lighting budget is ambient plus a few dynamic point lights, no shadow maps, and vertex ambient occlusion (D-81). Animation is JSON keyframes per bone, and locomotion is procedural (D-87). The camera collides in Core and the Game layer fades walls (D-88). Avoid Minecraft tells (D-83).

A C# synthesizer generates all audio from parameter files, music included (D-89, D-93). Music quality is a register risk (F-18).

### 3.12 Architecture

Two projects hold the game: `WhatYouCarry.Core` and `WhatYouCarry.Game` (D-108). Core is a pure C# library with no engine dependency. It owns the simulation, collision, pathfinding, projectiles, camera, items, economy, and saves. Game is a thin Godot layer for render, audio, and input. Tools and Tests are separate projects.

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

- Tier 1: property tests over seeds. Thousands per PR, one hundred thousand each night (D-116).
- Tier 2: scripted bots. A few hundred runs per PR, ten thousand each night (D-115, D-127).
- Tier 3: LLM play over a socket. Weekly on main, plus every economy PR (D-128).
- Tier 3b: an LLM reads the outlier run logs.
- Tier 4: vision play from screenshots. Milestone only (D-133).
- Game layer: a headless smoke session per PR, plus contract tests for content (D-114).
- Asset QA: budgets, pivots, UV coverage, box interpenetration at keyframe extremes, and file name case (D-135).
- Humans: the owner from the first playable, friends from the hub loop (D-134).

### 3.14 Process

Two harnesses work the repo: Claude Code and Codex (D-137). One session is one harness invocation, one PR, and one handoff rewrite (D-121). The owner starts every session, merges every PR, and owns every open question (D-102, D-103, D-124). Scheduled tests can run at night. Scheduled agents cannot (D-117). The other provider reviews every PR, and the review file lives in `docs/reviews/` (D-101).

The document protocol (D-118, D-120, D-125, D-129, D-132):

- `docs/design.md`: this file. Update it when intent changes.
- `docs/decisions.md`: the decision register. One file until about 300 rows (D-141).
- `docs/questions.md`: the open questions register, OQ-1 onward (D-144).
- `docs/session-handoff.md`: the 10 newest sessions, newest first. Each session adds an entry. Older entries move to `docs/session-handoff-archive.md` (D-146).
- `docs/reviews/`: one file per PR.
- `docs/roadmaps/`: focused roadmaps, linked from section 7.
- `CLAUDE.md` and `AGENTS.md`: identical pointer files (D-122).

## 4. Cost model (what we pay, what we do not know)

What we pay:

- Owner time: near full time (D-107).
- Tokens: a generous budget on two harnesses (D-107). The amount per PR is unknown until M-4.
- CI: GitHub-hosted Linux x64 and Windows x64 minutes on every PR (D-100). Wall time per PR is unknown until M-1.
- The Mac Mini as a self-hosted macOS arm64 runner: power, and RAM shared with the editor and the harness (D-100, D-105).
- Purchases that do not exist yet (D-142):
  - an external SSD before PR-1. Ordered, arrives 2026-09-08 (D-145).
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
| F-1 | v1 said content lives in "JSON or .tres". Core has no engine dependency and cannot read `.tres` | 2026-09-06 | 🔧 D-91. Binds PR-5 |
| F-2 | v1 assumed greedy meshing but never chose a voxel world | 2026-09-06 | 🔧 D-78. Binds PR-7, PR-13 |
| F-3 | v1 said both target machines are overpowered. The Steam Deck (D-15) is not | 2026-09-06 | ⚠ Binds M-3 and every render PR |
| F-4 | v1 named Deep Rock Galactic as the non-hitscan reference. Most of its guns are hitscan | 2026-09-06 | ✅ doc. D-60 replaced it |
| F-5 | v1 never said whether banked gear enters the dungeon | 2026-09-06 | 🔧 D-2. Binds PR-30 |
| F-6 | v1 never said who simulates player collision, enemy AI, and pathfinding | 2026-09-06 | 🔧 D-76, D-80. Binds PR-7, PR-16 |
| F-7 | v1 had no audio plan | 2026-09-06 | 🔧 D-89, D-93. Binds PR-20, PR-50 |
| F-8 | D-74 put a world-space aim in the intent. D-75 put the camera in Core. Both cannot hold | 2026-09-06 | 🔧 D-77. Binds PR-6, PR-8 |
| F-9 | D-95 "resume at floor start" made a quit a free heal and a timer reset | 2026-09-06 | 🔧 D-97. Binds PR-31 |
| F-10 | D-47 random affixes cannot show on an enemy, against D-16 | 2026-09-06 | 🔧 D-49. Binds PR-21, PR-26 |
| F-11 | v1 rejected free ascension as "no tension". D-44, D-48, D-52 removed that premise | 2026-09-06 | ⚠ D-50 reversed it. Binds M-5 |
| F-12 | v1 used Python for the texture generator, against one language | 2026-09-06 | 🔧 D-65. Binds PR-14 |
| F-13 | v1 determinism matrix omitted Windows x64, the largest audience and the second dev machine | 2026-09-06 | 🔧 D-71. Binds PR-3 |
| F-14 | v1 had no v1 scope in numbers | 2026-09-06 | ✅ doc. D-56 |
| F-15 | The harness default adds a co-author trailer to commits. D-137 forbids it | 2026-09-06 | 🔧 Refuted 2026-09-07: D-172 set booleans, the schema requires strings, and the file was ignored. D-175 revises D-172 and sets empty strings. Codex unverified. The PR-1 attribution scan binds every PR (D-176) |
| F-16 | The borrowed ste-writing skill carried another project's names and a Python checker | 2026-09-07 | ✅ doc (skills created). ✅ PR-2 holds the C# checker |
| F-17 | D-97 rewinds five seconds on resume. A quit undoes five seconds | 2026-09-07 | ⏸ Accepted by the owner |
| F-18 | All music is generated (D-93). Quality is unproven | 2026-09-07 | ❓ Owner ear at each phase gate. Binds PR-50 |
| F-19 | Night sweeps are scheduled jobs. D-103 forbids scheduled agents | 2026-09-06 | ✅ doc. D-117: tests may run on a schedule, agents may not |
| F-20 | Per-kill points (D-44) reward a full clear of every floor | 2026-09-06 | ⚠ The timer counters it. Binds M-5 |
| F-21 | Hyper-armor (D-29) needs a stagger system. The effect of armor weight on stagger is undecided | 2026-09-06 | ❓ OQ-5. Binds PR-15 |
| F-22 | The v1 build order put procgen before any render layer for weeks. D-57 wants a playable floor early | 2026-09-06 | 🔧 Phase 2 places the Game skeleton right after the Core foundations |
| F-23 | The name "Descent" had a trademark risk. "What You Carry" (D-11) has no trademark search yet | 2026-09-06 | ❓ OQ-17 |
| F-24 | Always-on numbers (D-36) and Deck 800p (D-15) strain Pillar 5 readability | 2026-09-06 | ⚠ Binds PR-19. Tier 4 checks it |
| F-25 | The SSD, the Apple account, and the Steam account do not exist (D-142) | 2026-09-07 | ⚠ SSD ordered, arrives 2026-09-08 (D-145). Accounts bind PR-51, PR-52 |
| F-26 | v1 named `HANDOFF.md`, `DECISIONS.md`, and a five-file set in uppercase | 2026-09-07 | ✅ doc. D-129 lowercase, D-132 one design file |
| F-27 | v1 listed the harness as one unnamed system. Two providers exist | 2026-09-07 | ✅ doc. D-137 names both. Binds the review file format in PR-1 |
| F-28 | Audit R-1: the PR gate required the STE checker, the lint tool, and the bit-identity job before PR-2 and PR-3 created them | 2026-09-07 | 🔧 D-148. PR-1 added the CI skeleton, and PR-2 added `ste-check`. The gate names an absent check with the PR that creates it. Binds PR-3 |
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
| F-39 | The macOS CI leg needs the Mac Mini registered as a self-hosted runner (D-100). No item listed that action | 2026-09-07 | 🔧 D-157. Owner action before PR-1 |
| F-40 | D-88's effect note put wall fade in the mesher as per-block visibility. A shader test needs no mesher change | 2026-09-07 | ❓ OQ-49. Binds PR-13 |
| F-41 | No item said whether a Steam Deck unit exists for M-3, and D-15 makes the Deck the floor | 2026-09-07 | ❓ OQ-50. Binds M-3 |
| F-42 | D-49 shows a rarity color on an enemy, but no decision names the rarity tiers | 2026-09-07 | ❓ OQ-52. Binds PR-21, PR-26 |
| F-43 | D-128 sets the Tier 3 cadence but not the model or the budget | 2026-09-07 | ❓ OQ-58. Binds PR-32 |
| F-44 | The Deck verification checklist is an external fact with no source or date in the plan | 2026-09-07 | ❓ OQ-66. Binds PR-54 |
| F-45 | D-152 changed the save files, and no item names which files cloud saves sync | 2026-09-07 | ❓ OQ-71. Binds PR-52 |
| F-46 | D-158 required branch protection, and GitHub returned 403: the feature needs Pro or a public repository, against D-106 | 2026-09-07 | ✅ doc. D-170 defers protection until launch |
| F-47 | PR #1 review P1-1: a commit body implied that an agent wrote the commits, and T-6 had no stated boundary for a tool name | 2026-09-07 | ✅ doc. D-176 fixes the reading. The commit body is rewritten. Binds PR-1 exit test 6 |
| F-48 | PR #1 review P1-2: PR-11 created the `night-gate` job and the night job together, so the gate had no result to read on its first run, against G-19 | 2026-09-07 | 🔧 D-177 splits them. PR-58 adds the gate after one night runs. Binds PR-11, PR-58 |
| F-52 | Two providers picked the same session number on the same day, because each read the handoff before the other wrote it | 2026-09-07 | ✅ D-187. Fetch and re-read before the handoff commit. The PR-2 session number check fails on a duplicate |
| F-53 | One `Revised by` marker made every citation of a partly revised decision stale. Partial revisions carried 33 of 48 citations and caused three rounds of churn | 2026-09-07 | ✅ doc. D-186 splits the marker into `Superseded by` and `Revised in part by`. The D-178 check keys on the first only |
| F-51 | A grey `review-gate` would stop blocking at launch. GitHub counts a neutral conclusion as a success for a required check, verified 2026-09-07 | 2026-09-07 | 🔧 D-181, D-185. Advisory mode gives neutral. Enforced mode gives failure. The tracked file `.github/review-gate-mode` selects the mode, and the workflow reads it from the base branch |
| F-50 | Nothing on GitHub stops a merge without a cross-provider review. T-4 and D-101 are rules only, and D-170 leaves `main` unprotected | 2026-09-07 | 🔧 D-179 and D-181, D-185 add the `review-gate` job in PR-1. D-180 makes it a required check at launch. Advisory until then |
| F-49 | PR #1 review P2-1: six lines cited D-172 or OQ-2 as a current answer after D-173 and D-175 revised them | 2026-09-07 | ✅ doc. All six corrected. ✅ D-178. PR-2 holds the reference check |
| F-54 | A launch agent cannot read an external volume. macOS denied `/Volumes/SSD-1TB/actions-runner/runsvc.sh` with `Operation not permitted`, and the agent exited 126. A launchd probe repeated the denial, and a login shell read the same path correctly, verified 2026-09-07 | 2026-09-07 | ✅ D-193. Full Disk Access for `/bin/bash` and the runner `node` binary. Binds PR-1 and every machine rebuild |
| F-55 | The Phase 1 roadmap named `WhatYouCarry.sln`, and the .NET 10 SDK creates a `.slnx` file. `dotnet new sln --format` gives `Default: slnx`, and a smoke job on the runner made `Smoke.slnx`, verified 2026-09-07 | 2026-09-07 | ✅ D-194. `WhatYouCarry.slnx`. Godot 4.7.2 accepts it. Binds PR-1 |
| F-56 | PR #6 review P1-1: the `review-gate` workflow ran the tool from the PR head with `checks: write`, so a PR could change the code that judges it | 2026-09-08 | ✅ D-197. `pull_request_target`, the head as data only. Binds PR-1 and Phase 5 |
| F-57 | PR #6 review P1-2: an author can rewrite the approved review file in a metadata commit, and one shared identity cannot prove the reviewer, verified 2026-09-08 | 2026-09-08 | 🔧 D-198. Accepted risk under D-190. The output names the commit that last changed the review file |
| F-58 | GitHub triggers `pull_request_target` only when the workflow file exists on the default branch. PR #7 against the PR-1 branch produced no run, and the events reference states the rule, verified 2026-09-08 | 2026-09-08 | 🔧 D-197 Effect. The `review-gate` check cannot run on PR-1 itself. The proof runs after the merge |
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

## 6. Guardrails (the safety contract for every PR)

### 6.1 Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins (D-119): T-5, T-2, T-3, T-4, T-1. T-6 is absolute and never conflicts.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures (D-110). Two concrete cases before any abstraction (D-111). No clever one-liners. Tune only on measurement (D-109).
- **T-2. Zero silent failures.** No empty catch blocks. An absent value is an error, never a zero. Every error carries the seed, floor, tick, and entity ids inside a run, and the save versions, screen, action, and file paths outside one (D-113). Assertions stay on in shipped builds (D-112).
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it. The review file records the findings (D-101).
- **T-5. Document everything.** Continuity is the first duty. Each session rewrites the handoff. The other documents update when intent, a decision, or a plan changes (D-118).
- **T-6. No attribution.** No code, game text, commit, PR description, or GitHub comment names an agent, harness, or model as the source of work (D-137). Two places are exempt: the author field in the session handoff, and the review files.

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
13. **G-13.** No co-author trailer, generation line, or model name in a commit, PR, or comment (T-6).
14. **G-14.** Every document follows ASD-STE100. The `ste-check` job runs the checker in the PR gate (D-139, PR-2).
15. **G-15.** The Steam Deck at 800p is the readability and performance floor for every UI and render change (D-15).
16. **G-16.** Every dependency has a decision entry that justifies it.
17. **G-17.** Every optimization has a profile before it and a measurement after it (D-109).
18. **G-18.** Squash merge by the owner, from a short branch, with a conventional commit subject (D-126).
19. **G-19.** A PR that creates a check passes that check. A PR names any check that does not exist yet, with the PR that creates it (D-148).
20. **G-20.** Every Core behavior change bumps the simulation version constant, and the review confirms it (D-151).
21. **G-21.** No `System.Random`, `DateTime`, `Stopwatch`, or `Environment.TickCount` in Core. The seed and the tick are the only sources of randomness and time (D-69, D-73).

## 7. Roadmap

Phases are the five milestones of D-136 as revised by D-150. Gate 1 is a foundation gate with a written exit test. Gates 2 to 5 are playable builds with a written exit test and the owner's playtest sign-off. Ids: PR-# code changes, M-# measurements. An entry that covers several PRs notes its reserved id range. Focused roadmaps in `docs/roadmaps/` expand the phases with per-PR exit tests. Phase 1: [phase-1-foundations.md](roadmaps/phase-1-foundations.md). Phase 2: [phase-2-first-playable.md](roadmaps/phase-2-first-playable.md). Phase 3: [phase-3-full-loop.md](roadmaps/phase-3-full-loop.md). Phase 4: [phase-4-content-complete.md](roadmaps/phase-4-content-complete.md). Phase 5: [phase-5-early-access.md](roadmaps/phase-5-early-access.md).

### Phase 1: Foundations (foundation gate, D-150: CI green on three platforms with a bit-identical end state, docs and PR gate live, no playtest)

**PR-1: Repository scaffold.** ✅ Merged 2026-09-08 as PR #6.
Create the solution with `WhatYouCarry.Core`, `WhatYouCarry.Game`, `WhatYouCarry.Tools`, and `WhatYouCarry.Tests` (D-108). Pin Godot 4.7.2 .NET and .NET 10 LTS (D-61, D-62, D-173). Create `CLAUDE.md` and `AGENTS.md` as identical pointer files with a test that asserts equality (D-122). Add the GitHub Actions workflow that builds and runs `dotnet test` on Linux x64, macOS arm64, and Windows x64 (D-148). Add a PR template with the gate checklist and the "no change needed because" lines (D-118). The template has a line that names each absent check with the PR that creates it (D-148). Add the `review-gate` job (D-179). It publishes a check run with three conclusions (D-181). The mode file `.github/review-gate-mode` selects the mode, and the workflow reads it from the base branch (D-185). Success means an approved review of the effective head. Failure means a review that does not approve, or a stale head. Grey means no review record yet, and only in advisory mode. The label `review-override` gives success for a PR that changes no code, and the owner adds that label (D-188, D-190). The job is advisory until launch, because GitHub locks branch protection on a private free repository (D-170, D-180). The attribution option is in place (D-175). Needs the external SSD (D-145) and the runner (D-171). No game code.
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

**PR-6: Simulation loop, intent record, recorder, and replay.** 🔧 Open as PR #19.
Implement the fixed-step loop at 60 Hz (D-73). Define the intent record: quantized yaw and pitch deltas, a movement vector, and button states (D-74, D-77). Define the run record (D-151). Its header holds a format version, a simulation version constant, a content hash, the seed, and an immutable initial state. The fixed 16-byte checksummed tick frames of D-162 follow the header (D-226). Implement the recorder that writes the header and appends frames from the first tick (D-97, G-5). Implement the replay that drives the loop from a record and ignores the live bank and tree. Property tests assert three facts over one thousand seeds. A replay reproduces the end-state hash. A torn tail truncates to the last complete frame. A version or content mismatch produces a contextual report.
Gate: the replay of a recorded run gives the same hash on all three platforms, and a mismatch report names both versions.
> *In plain English:* the game runs in fixed steps and writes down its start state and every input. That record then plays any run again, so every bug becomes repeatable. A record from an older version says so instead of a silent failure.

**PR-7: Voxel world and Core collision.** 🔧
Implement the voxel grid of one-meter cubes (D-78). Implement Core collision for player and enemy boxes against the grid with swept movement, gravity, ledges, and jump (D-27, D-80). Godot physics has no part in it. Add the player box that reads the intent's movement and jump, so a Core-only run exists before the Game layer (D-149). Property tests assert no tunnel at maximum speed and no fall through a floor block.
Gate: a box that moves at the maximum speed never crosses a solid block.
> *In plain English:* the dungeon is a grid of blocks. The game itself decides how bodies bump into them, so the result is identical on every machine.

**PR-8: Camera as a Core system.** 🔧
Implement the over-the-shoulder camera in Core (D-13, D-75). It integrates the quantized look deltas, sweeps its boom against the grid, and derives the aim ray (D-77, D-88). Aim assist runs here from enemy positions (D-14). Property tests assert the camera never enters a solid block and the aim ray is deterministic.
Gate: a recorded run with camera motion replays to the same hash on all three platforms.
> *In plain English:* the camera is part of the simulation, not decoration, so where you look and where you aim replay exactly.

**PR-9: Procgen v1 and property tests.** 🔧
Implement floor generation on the grid for one biome, a collapsed deep mine (D-6, D-13, D-46, D-210). A floor has rooms, corridors with a minimum width for the camera, a spawn point, and a stairwell. Floor size grows with depth. Implement the stairwell transition in Core (D-50, D-149). On arrival, a policy or the player chooses descend or ascend. The next floor generates from the run seed and the floor number. Floor templates are JSON. Property tests run over thousands of seeds per PR and one hundred thousand each night (D-116). They assert four facts: every room is reachable, no rooms overlap, the stairwell is reachable, and the difficulty budget is within tolerance.
Gate: the night sweep passes on one hundred thousand seeds.
> *In plain English:* this builds the dungeon floors from a random seed and proves, over huge numbers of seeds, that a player can finish every floor.

**PR-10: Projectile simulation.** 🔧
Implement the projectile integrator with fixed-step Euler, swept collision against the grid and entity boxes, gravity scale, lifetime, and spread from weapon data (G-6). Implement the arc solver with DetMath. Add a `projectile` content schema and a test-only definitions file (F-38, D-149). The file holds the slowest arc, the fastest flat shot, the longest lifetime, and the widest spread. Property tests assert three facts. No projectile tunnels through the minimum wall at the maximum velocity. Every projectile ends inside its lifetime. The arc solver reaches a reachable target and reports an unreachable one.
Gate: the projectile property tests pass over the test-only definitions. PR-24 and PR-43 to PR-46 rerun them over the real roster.
> *In plain English:* bullets and arrows are real objects that fly, drop, and can miss. Tests prove a fast bullet never passes through a wall.

**PR-11: Bot harness (Tier 2).** 🔧
Implement the headless runner at one hundred times speed and the first two policies: random walker and greedy descender (D-127, D-149). Later PRs add a policy with the system it exercises: full-clearer with PR-16, timer-tester with PR-17, coward with PR-18. Each run writes a structured run log with the policy name, seed, and end state. Add a few hundred runs to the PR job and ten thousand to the night job (D-115). The night job publishes a result record. PR-58 adds the gate that reads it (D-177).
Gate: ten thousand night runs of the two policies complete with zero crashes and zero softlocks.
> *In plain English:* simple robots play thousands of runs every night without graphics. They find crashes and dead ends before a person ever sees them.

**PR-58: Night gate.** 🔧
Add the `night-gate` job to the PR workflow. It reads the result record that the PR-11 night job publishes (D-177). The gate passes only on a success record from a scheduled night in the last 48 hours. An absent, stale, cancelled, or failed record fails the gate, and the message names the case (D-115, T-2). This entry follows PR-11 in the sequence, after one scheduled night runs (G-19).
Gate: the job fails each of the four bad records and passes on the real night record.
> *In plain English:* every merge now needs a green night from the robots. A missing or old result stops the merge, so nobody can merge on silence.

**M-1: CI wall time per PR.** 🔧
Record the wall time of each CI job per platform for ten PRs. Binds the seed counts in D-116 if a PR job exceeds ten minutes.

**M-2: Night sweep wall time.** 🔧
Record the night sweep duration on the Mac Mini for one week. Binds the night run counts in D-115 and D-116.

### Phase 2: First playable (gate: the owner plays one floor with the timer, the hunter, and a stairwell, D-57)

**PR-12: Game skeleton and input.** 🔧
Create the Godot project with scenes built in C# (D-63). Bind the Core loop at 60 Hz with render interpolation (D-73). Map keyboard, mouse, and controller to the intent, with sensitivity and curves applied before quantization (D-15, D-77). Add the headless smoke session to CI: boot, start a run, move for one thousand ticks, quit, with no log errors (D-114, D-149). PR-18 extends it to the stairwell.
Gate: the smoke session passes on all three platforms.
> *In plain English:* this is the first thing you can open and move in. It also adds an automatic run of the real game on every change.

**PR-13: Model loader and mesher.** 🔧
Implement the Blockbench JSON loader that builds an ArrayMesh from the box list, with armor overlay attachment per slot (D-9, D-18). Implement greedy meshing for the voxel grid with vertex ambient occlusion and per-block visibility for wall fade (D-78, D-81, D-88). One atlas, one material. A test asserts the mesh count per floor stays under a budget.
Gate: a full floor renders under the mesh budget, and M-3 records the Deck frame time.
> *In plain English:* this turns the block grid and the box models into pictures, cheaply enough for the smallest target machine.

**PR-57: Asset QA gate v1.** 🔧
Implement the C# tool for three checks (D-135, D-149). No two boxes interpenetrate at any animation keyframe. Each armor overlay encloses its limb box. Every file name reference matches the file case. Run it in CI on every model. PR-49 extends it. This entry follows PR-13 in the sequence.
Gate: the tool fails a model with a clip and a model with a wrong-case reference.
> *In plain English:* from the first model onward, an automatic inspection catches pieces that clip through each other and file names that break on Linux.

**PR-14: Texture generator and palette.** 🔧
Implement the C# texture generator that emits the 32 px atlas from the palette and rule files (D-65, D-85). The palette is an owner input (OQ-1). Contact sheets render at game zoom for review.
Gate: the owner approves the first contact sheet.
> *In plain English:* a tool paints every texture from a fixed set of about thirty colors, so the whole game looks like one thing.

**PR-15: Player entity and the first weapon.** 🔧
Implement the player in Core: sprint, jump, dodge on a cooldown, health, stagger, and one sword with windup, active, and recovery frames (D-25, D-27, D-28, D-29, OQ-5). Implement the animation JSON format and the procedural locomotion in Game (D-87). A test asserts that the animation file agrees with the Core time values.
Gate: the owner confirms the sword feels committed and readable.
> *In plain English:* you can run, jump, dodge, and swing a sword, and the swing shows its wind-up so you can read an enemy.

**PR-16: First enemy family, AI, and pathfinder.** 🔧
Implement the 3D grid pathfinder that understands jumps and drops (D-76). Implement one humanoid family with Core AI: target selection, approach, attack, and reload behavior (D-30, D-31, OQ-9). Add the full-clearer bot policy (D-149). Bot runs cover it.
Gate: the bot sweep passes with enemies active.
> *In plain English:* the first enemies find their way through the dungeon and fight by the same rules you do.

**PR-17: Floor timer, hunter, and escalation.** 🔧
Implement the visible per-floor timer with lengths in data (D-44, D-46, OQ-4). Implement the hunter as one entity with a speed curve that grows until escape is impossible, and the escalation spawner (D-45, OQ-6). The timer pauses at the stairwell and runs in boss fights (D-140). Add the timer-tester bot policy (D-149).
Gate: a timer-tester bot always dies to the hunter, and a greedy descender rarely meets it.
> *In plain English:* each floor has a clock. When it runs out, an unstoppable hunter arrives and gets faster, so a wait is never the safe choice.

**PR-18: Stairwell and floor transition.** 🔧
Implement the stairwell with the untimed descend-or-ascend prompt (D-50, D-140). Implement the floor transition with the next floor generated on a worker during the current floor (D-72). A test asserts the worker output equals a synchronous generation for the same seed. Add the coward bot policy, and extend the smoke session to the stairwell (D-149).
Gate: the transition shows no frame over the hitch budget on the Deck.
> *In plain English:* you reach the stairs, choose to go down or leave, and the next floor already exists, so there is no pause.

**PR-19: HUD and controller navigation.** 🔧
Build the HUD in C# Control nodes: health, timer, damage numbers, and a boss bar placeholder (D-36, D-90). Numbers must not cover silhouettes (F-24). Build the controller navigation base for every later screen at Deck size (G-15).
Gate: the HUD reads at 800p and every element works with a controller.
> *In plain English:* the on-screen numbers and bars appear, sized for the smallest screen, and every menu works with a gamepad from the start.

**PR-20: Audio synthesizer and first effects.** 🔧
Implement the C# synthesizer that renders sound effects from parameter files (D-93). Ship the first set: sword, footsteps, dodge, hit, hunter, and timer warnings. Bind them in Game.
Gate: the owner approves the sword and hunter sounds.
> *In plain English:* a tool makes every sound from a recipe, and the first sounds give the sword and the hunter their weight.

**M-3: Steam Deck frame time.** 🔧
Measure the 99th percentile frame time on a Deck over one full floor, with the target from OQ-15. Binds every render PR (F-3).

### Phase 3: Full loop (gate: hub, loadout, death loss, bank, tree, saves, and replay resume work, and friends play)

**PR-21: Items, tiers, and affixes.** 🔧
Implement item definitions in JSON with tiers by depth band and a rare higher-tier chance (D-48, OQ-22). Implement rarity and random affixes as a fixed set of behaviors that any wielder gets (D-47, D-49). Property tests assert the band distribution.
Gate: the affix set has one test per behavior, for the player and for an enemy.
> *In plain English:* items get random extra powers, and an enemy that carries such an item uses the power against you.

**PR-22: Equipment slots, armor overlays, and weight.** 🔧
Implement the modeled slots and the two ring slots (D-18, D-55). Attach armor overlays and the shield to the shared base body (D-82). Implement damage reduction and weight on movement and dodge (D-23, D-26).
Gate: the PR-57 pose check passes for every armor piece on every animation (D-135).
> *In plain English:* what you wear shows on your body. Heavy pieces make you slower. The pieces never clip through each other.

**PR-23: Satchel, quick slot, throwables, potions, and weapon swap.** 🔧
Implement the satchel with a slot count from OQ-3 (D-19). Implement the quick slot, bombs with full self-damage, health and mana potions, and the slow uncancelable weapon swap (D-21, D-22, D-24, D-32).
Gate: a full satchel forces a drop choice, and the choice works on a controller.
> *In plain English:* you carry a small bag. Bombs, potions, spare weapons, and found amulets all compete for its few slots.

**PR-24: Bow and musket.** 🔧
Implement one bow with a fast draw and small area arrows, and one musket with a slow reload and a flat shot (D-40, D-42, OQ-13). Enemies use both (D-30). Contact sheets prove the two projectiles are distinct in flight.
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
Build the settings screen. Complete the string table (D-98). Add accessibility options per OQ-14.
Gate: every option works with a controller.
> *In plain English:* the options screen, complete text, and any assist options the owner chooses.

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
2. Owner: receive the external SSD and move the checkout to it (D-145).
3. Owner: register the runner on 2026-09-08 (D-157, D-171). ✅ OQ-2: D-173. ✅ OQ-16: D-175. ✅ OQ-30: D-156. Protection deferred: D-170.
4. PR-1, PR-2. ✅ PR-1 merged 2026-09-08 as PR #6. ✅ PR-2 merged 2026-09-08 as PR #10.
5. PR-3, PR-4, PR-5. ✅ PR-3 merged 2026-09-08 as PR #12. ✅ PR-4 merged 2026-09-08 as PR #15. ✅ PR-5 merged 2026-09-08 as PR #17.
6. PR-6, PR-7, PR-8. 🔧 PR-6 open as PR #19.
7. PR-9, PR-10, PR-11. One scheduled night runs, then PR-58 (D-177).
8. M-1, M-2.
9. **← GATE 1 (foundation).** Nothing below starts until the bit-identity job, `dotnet test`, and the night sweep are green.
10. PR-12, PR-13, PR-57, PR-14.
11. PR-15, PR-16, PR-17, PR-18.
12. PR-19, PR-20.
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
