# Phase 4 roadmap: Content complete

Status: **focused roadmap, active.** This file expands Phase 4 of `docs/design.md` section 7: PR-33 to PR-50 and the Tier 4 pass. It applies D-56, D-133, D-135, and D-149. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the tenets (section 6.1). Phase 3 is `phase-3-full-loop.md`. Gate 3 must pass before PR-33 starts.

External facts: none new.

Correction passes: none yet.

## 1. Thesis

Phase 4 fills the small scope of D-56. That scope is three bosses, eight enemy families, about twelve weapons, about ten affixes, props, music, and the full asset gate. It ends at Gate 4: content complete, a Tier 4 pass, and two friend playtests filed (D-150). Almost every PR in this phase is data, not code. The order puts the bosses first, because the run has no end without them (D-5). Families and weapons follow, one PR at a time, so each silhouette and each projectile gets a judgment alone (L-1). Music comes last, because the owner's ear is its gate (F-18).

The risk in this phase is repetition: fifty brown humanoids. Each family PR writes its silhouette specification before its model, and the contact sheet at game zoom is the gate.

## 2. Findings that bind this phase

| # | Finding | Binds |
|---|---|---|
| F-18 | All music is generated. Quality is unproven | PR-50 |
| F-24 | Always-on numbers strain readability | PR-33, Tier 4 pass |
| F-38 | PR-10's gate named a roster that arrives here | PR-43 to PR-46 |

## 3. Guardrails for this phase

All guardrails in `docs/design.md` section 6.2 apply. These three matter most in Phase 4:

1. **G-7.** Every content file validates. A family, a weapon, an affix, and a boss are content.
2. **G-10.** One concern per PR. One family, one weapon batch, one boss per PR.
3. **G-12.** Ids never change. PR-36 to PR-42 and PR-43 to PR-46 are reserved ranges, one id per PR.

## 4. Roadmap

Each entry has: scope, out of scope, exit tests, review focus, the check clause, the gate, and a plain-English paragraph. The review skill is `.claude/skills/pr-review/SKILL.md`.

### PR-33: Boss framework and boss 1

Scope:

- `Core/Bosses/BossPattern.cs`: a boss as a list of phases, in the format of OQ-60 (D-6). Each phase has attacks that reference weapon or projectile definitions, telegraph ticks, and a health threshold to the next phase.
- `content/bosses/boss-1.json`: the floor-5 boss from OQ-11, with the `boss` validator.
- `Core/Procgen/BossRoom.cs`: a boss room template at the end of floors 5, 10, and 15, with the timer live (D-140).
- `WhatYouCarry.Game/Ui/BossBar.cs`: the boss bar with a number (D-36), placed where it never covers the boss silhouette (F-24).
- `Core/Bots/BossFighter.cs`: a policy that dodges on a telegraph and attacks in recovery.

Out of scope: boss 2 and boss 3, the ending.

Exit tests:

1. `TelegraphPrecedesEveryAttack` asserts each attack has a telegraph of at least the OQ-60 minimum ticks.
2. `PhaseChangesAtThreshold` asserts the next phase at the health threshold.
3. `BossFighterWins` asserts the policy defeats boss 1 on at least 80 percent of one hundred seeds.
4. `BossBarAvoidsSilhouette` asserts the bar's rectangle does not overlap the boss screen box.
5. `BossIsDeterministic` replays a boss fight and asserts one state hash on three platforms.
6. The owner confirms the telegraphs read, recorded as a decision.

Review focus: determinism, gameplay, presentation, content.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* the first big fight arrives with a health bar you can read and attacks you can see before they land.

### PR-34: Boss 2

Scope: `content/bosses/boss-2.json`, the floor-10 boss from OQ-11, on the PR-33 framework. New attacks reference projectile definitions that already exist. No new Core code unless a pattern needs a new attack type, which then gets its own decision.

Out of scope: boss 3.

Exit tests: 1 to 6 of PR-33, for boss 2.

Review focus: gameplay, content.

Check clause: none.

Gate: the PR-33 exit tests pass for boss 2.

> *In plain English:* the second big fight, built from the same parts as the first.

### PR-35: Boss 3 and the ending

Scope:

- `content/bosses/boss-3.json`: the floor-15 boss from OQ-11.
- `Core/Simulation/Ending.cs`: the v1 ending after boss 3, then ascension with everything carried (D-5, D-50).
- `WhatYouCarry.Game/Ui/EndingScreen.cs`: the ending screen with the run summary.

Out of scope: endless mode (Phase 6).

Exit tests:

1. The PR-33 exit tests pass for boss 3.
2. `EndingBanksEverything` asserts the loadout and satchel in the bank after the ending.
3. `FullRunInsideTarget` runs the greedy descender with the boss fighter through fifteen floors on one hundred seeds. It asserts a median run time between 30 and 45 simulated minutes (D-4).
4. `EndingScreenNavigates` asserts the screen works with the controller model.

Review focus: gameplay, economy, presentation.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* the last fight and the end of a full descent, at the length the design promised.

### PR-36 to PR-42: Enemy families two to eight

One PR per family from OQ-9, humanoid or monster (D-31, D-56). Each PR has this scope:

- `content/silhouettes/<family>.json`: the silhouette specification from OQ-61, written before the model.
- `content/models/enemies/<family>.json` and its animations, through the PR-57 gate.
- `content/enemies/<family>.json` with its weight, its attacks, and its loot table or monster drop (D-167, OQ-23).
- A contact sheet at game zoom with every family so far (D-83).
- Bot coverage: the night sweep spawns the family.

Out of scope in each PR: any second family, any new weapon.

Exit tests per PR:

1. `SilhouetteSpecExists` asserts the specification file validates and predates the model in the PR.
2. `FamilyValidates` asserts the enemy, model, and loot files validate.
3. `FamilyIsDistinct` asserts the family's silhouette proportions differ from every earlier family by the OQ-61 threshold.
4. `NightSweepSpawnsFamily` asserts the family appears in the night run logs with zero crashes.
5. The owner confirms the family reads as distinct on the contact sheet, recorded as a decision.

Review focus: content, presentation, test quality.

Check clause: none.

Gate per PR: exit tests 1 to 5 pass.

> *In plain English:* seven more kinds of enemy, each added alone so its shape and its behavior can be judged on their own.

### PR-43 to PR-46: Weapons to twelve

One PR per weapon class batch from OQ-10: melee, bows, guns, exotics (D-42, D-56). Each PR has this scope:

- `content/weapons/*.json` for the batch, with models through the PR-57 gate.
- `content/projectiles/*.json` for each new projectile, distinct in speed, drop, size, and trail.
- The PR-10 projectile tests rerun over the roster so far (F-38).
- A contact sheet of every projectile in flight at game zoom.

Out of scope in each PR: any Core code for a weapon (D-56 gate).

Exit tests per PR:

1. `NoCoreChangeForWeapon` asserts the PR diff touches no file under `Core/` (D-56).
2. `WeaponsValidate` asserts every weapon and projectile file validates.
3. `ProjectileTestsPassOnRoster` reruns the PR-10 property tests (F-38).
4. `ProjectilesDistinct` asserts every pair of projectiles differs in at least two of speed, drop, size, and trail.
5. The owner confirms the batch reads on the contact sheet, recorded as a decision.

Review focus: content, gameplay, test quality.

Check clause: none.

Gate per PR: exit tests 1 to 5 pass.

> *In plain English:* the weapon list grows to about twelve, and each one is a data file, not a program change.

### PR-47: Affixes to ten

Scope: about six more affix behaviors in `Core/Items/AffixBehaviors.cs` and their content files, from the list the owner adds to OQ-51 (D-56). Each has a test for the player and for an enemy (D-49). A Tier 3 session hunts for a dominant affix (D-128).

Out of scope: any affix that needs a new resource.

Exit tests:

1. `EveryAffixWorksForPlayer` and `EveryAffixWorksForEnemy` pass for the full set.
2. `AffixRollsAreUniform` asserts each affix's roll rate within 1 percent of its weight over one hundred thousand rolls.
3. The Tier 3 session finds no affix that wins more than 60 percent of matched trials, and files its findings.

Review focus: gameplay, economy, test quality.

Check clause: none.

Gate: exit tests 1 to 3 pass.

> *In plain English:* more random item powers, each proven to work for you and against you, and none that everyone would always pick.

### PR-48: Destructible props

Scope:

- `Core/Entities/Prop.cs`: an entity with health and no AI that breaks into debris the Game layer shows (D-79).
- `content/props/*.json`: the props from OQ-64 with a model each, through the PR-57 gate.
- Prop placement in room templates by weight.

Out of scope: wall destruction (D-79).

Exit tests:

1. `PropBreaksAtZero` asserts removal at zero health and a debris event.
2. `PathfinderIgnoresProps` asserts no path change after a prop breaks (D-79).
3. `PropsValidate` asserts every prop file validates.
4. `PropsAreDeterministic` replays a record with breaks and asserts one state hash.

Review focus: determinism, gameplay, presentation.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* barrels and crates break. Walls do not, so the hunter can never be walled off.

### PR-49: Asset QA gate v2

Scope: extend `WhatYouCarry.Tools/AssetQa/` with the polygon budget, the pivot placement rule, and the UV coverage check from OQ-65 (D-135).

Out of scope: any model change.

Exit tests:

1. `OverBudgetIsDetected` asserts one finding on a fixture over the box budget.
2. `BadPivotIsDetected` asserts one finding on a fixture with a pivot away from the OQ-65 rule.
3. `UnmappedFaceIsDetected` asserts one finding on a fixture with one unmapped face.
4. `RepositoryModelsPass` asserts zero findings on every model.

Review focus: content, test quality.

Check clause: this PR extends the `asset-qa` check and passes it (G-19).

Gate: exit tests 1 to 4 pass.

> *In plain English:* the inspection from PR-57 learns three more checks, so every model meets its budget before it enters the game.

### PR-50: Music

Scope:

- `WhatYouCarry.Tools/AudioSynth/Sequencer/`: a sequencer format with a tempo, tracks, patterns, and the synthesizer voices of OQ-48, rendered to OGG at build time (D-93).
- `content/audio/music/*.json`: the tracks from OQ-63: hub, three dungeon bands, boss, and hunter.
- Playback in Game bound to the floor band, the boss, and the hunter.

Out of scope: licensed music (D-89).

Exit tests:

1. `SequencerIsDeterministic` renders one track twice and asserts equal bytes.
2. `EveryTrackRenders` asserts a file for every track in the list.
3. `MusicFollowsState` asserts the boss track on a boss floor and the hunter track after expiry.
4. The owner approves a hub track and a dungeon track, recorded as a decision. A refusal reopens D-89 as a question.

Review focus: content, presentation.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* music made from recipes. If it does not sound good enough, the decision to generate it is reopened.

### Tier 4 pass

Procedure: the protocol of OQ-62 (D-133). The owner runs a vision-capable model through the socket with screenshots at one to two frames per second. The sessions cover a full floor, a boss fight, and the hub. The model reports unreadable telegraphs, projectiles it cannot tell apart, UI over the fight, and camera clips. Findings go to `docs/questions.md`. Each finding either changes content in a PR or gets an owner decision.

## 5. Sequence

One person owns the program. Items run one at a time in this order. Gate 3 must pass first.

1. Owner: answer OQ-11 and OQ-60.
2. PR-33.
3. PR-34.
4. PR-35.
5. Owner: answer OQ-9 in full and OQ-61.
6. PR-36 to PR-42, one at a time.
7. Owner: answer OQ-10 in full.
8. PR-43 to PR-46, one at a time.
9. Owner: extend OQ-51 to ten affixes.
10. PR-47.
11. Owner: answer OQ-64.
12. PR-48.
13. Owner: answer OQ-65.
14. PR-49.
15. Owner: answer OQ-63.
16. PR-50.
17. Owner: answer OQ-62. Tier 4 pass.
18. Two friend playtests filed under OQ-20.
19. **← GATE 4 (content complete).** Every exit test in this file passes. The Tier 4 findings have dispositions. The owner signs the gate in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 4. Each names the PR it blocks.

Open:

- OQ-9: the enemy families in full. Blocks PR-36 to PR-42.
- OQ-10: the weapon list in full. Blocks PR-43 to PR-46.
- OQ-11: the boss concepts. Blocks PR-33 to PR-35.
- OQ-20: the friend playtest protocol. Blocks Gate 4.
- OQ-51: the affix list to ten. Blocks PR-47.
- OQ-60: the boss pattern format. Blocks PR-33.
- OQ-61: the silhouette specification. Blocks PR-36 to PR-42.
- OQ-62: the Tier 4 protocol. Blocks the Tier 4 pass.
- OQ-63: the music direction and track list. Blocks PR-50.
- OQ-64: the prop set. Blocks PR-48.
- OQ-65: the polygon budget, pivot rule, and UV rule. Blocks PR-49.
