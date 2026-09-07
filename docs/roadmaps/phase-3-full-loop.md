# Phase 3 roadmap: Full loop

Status: **focused roadmap, active.** This file expands Phase 3 of `docs/design.md` section 7: PR-21 to PR-32, M-4, and M-5. It applies D-149, D-151 to D-154, and D-165. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the tenets (section 6.1). Phase 2 is `phase-2-first-playable.md`. Gate 2 must pass before PR-21 starts.

External facts: none new.

Correction passes: none yet.

## 1. Thesis

Phase 3 turns one floor into the game. It ends at Gate 3 (D-134, D-150). At that gate the hub, a loadout, death that loses gear, a bank that keeps it, the tree, the saves, and replay resume all work. Friends play. The economy lives here, and the design calls the economy the real risk. The order follows dependency and risk. Items and affixes come first, because every later system carries them. Equipment, the satchel, and the ranged weapons follow, because enemies must wear and drop them (D-16). The Tier 3 socket lands before the first economy PR, because D-128 requires it there (D-149). Points, the amulet, the tree, the hub, and the saves close the loop. M-5 runs from the first economy build and never stops.

This phase carries the sensitive number of the design: the death payout curve (D-52). No PR in this phase changes it without the M-5 trials.

## 2. Findings that bind this phase

| # | Finding | Binds |
|---|---|---|
| F-5 | The v1 doc never said whether banked gear enters the dungeon | PR-30 |
| F-9 | Resume at floor start made a quit a free heal | PR-31 |
| F-10 | Random affixes cannot show on an enemy | PR-21, PR-26 |
| F-11 | Free ascension reversed a v1 position on a changed premise | M-5 |
| F-20 | Per-kill points reward a full clear | M-5 |
| F-24 | Always-on numbers strain readability | PR-23, PR-29, PR-30 |
| F-29 | Four gates preceded their prerequisites | PR-22, PR-27, PR-32 |
| F-30 | The run record omitted the initial state | PR-31 |
| F-31 | Three atomic files did not make one atomic save | PR-31 |
| F-32 | No first loadout and no empty-bank path | PR-28, PR-30 |
| F-33 | The economy gate had no reproducible comparison | PR-27, M-5 |
| F-42 | No decision names the rarity tiers that D-49's colors need | PR-21, PR-26 |
| F-43 | D-128 needs a model and a budget for Tier 3 that nobody recorded | PR-32 |

## 3. Guardrails for this phase

All guardrails in `docs/design.md` section 6.2 apply. These five matter most in Phase 3:

1. **G-5.** Every run records its seed and intent stream from the first tick. The profile and the record are the only saves.
2. **G-7.** Every content file validates against its validator. Items, affixes, enemies, and tree nodes are content.
3. **G-11.** No unowned decision. The balance numbers in this phase are decisions (D-123).
4. **G-19.** The economy gate is the M-5 pass condition, not a smaller payout (D-154).
5. **G-20.** Every Core behavior change bumps the simulation version constant. Every PR in this phase changes Core behavior.

## 4. Roadmap

Each entry has: scope, out of scope, exit tests, review focus, the check clause, the gate, and a plain-English paragraph. The review skill is `.claude/skills/pr-review/SKILL.md`.

### PR-21: Items, tiers, and affixes

Scope:

- `content/items/*.json`: item definitions with a tier, a slot, base stats, and a model reference, with the `item` validator (D-47, D-168).
- `content/affixes/*.json`: the first affixes from OQ-51, each a reference to one coded behavior with parameters (D-49).
- `Core/Items/AffixBehaviors.cs`: the fixed set of behaviors. Each takes a wielder, an event, and parameters, and works for the player and for an enemy (D-49).
- `Core/Items/LootRoller.cs`: rolls a tier from the depth band table of OQ-22, a rarity from OQ-52, and affixes from the RNG stream for loot (D-48, D-159).
- `Core/Items/Rarity.cs`: the rarity tiers and their colors from OQ-52.

Out of scope: the models for items (PR-22), drops from enemies (PR-26), rings beyond the definition (PR-22).

Exit tests:

1. `EveryAffixWorksForPlayer` triggers each behavior on the player and asserts its effect.
2. `EveryAffixWorksForEnemy` triggers each behavior on an enemy and asserts the same effect (D-49).
3. `BandDistributionHolds` rolls one hundred thousand items per floor and asserts each band's tier shares within 2 percent of the OQ-22 table.
4. `RareHigherTierChance` asserts the higher-tier rate within 0.5 percent of the OQ-22 value at floor 1.
5. `RollIsDeterministic` rolls with one seed twice and asserts equal items.
6. `ItemValidatorRejectsUnknownAffix` asserts a definition that names an absent affix fails with the name.

Review focus: determinism, content, gameplay, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* items get random extra powers from a short list. Each power works the same for you and for an enemy that carries the item.

### PR-22: Equipment slots, armor overlays, and weight

Scope:

- `Core/Entities/Equipment.cs`: the six modeled slots and the two ring slots, with equip and unequip rules (D-18, D-55). A shield needs a one-handed melee weapon (D-26).
- `Core/Entities/Weight.cs`: the sum of armor weight, and its effect on walk speed, sprint speed, and dodge cooldown (D-23, D-28). The stagger rule against weight follows OQ-5.
- `content/models/armor/*.json`: overlay boxes per slot on the shared base body (D-82), with the first three armor sets from OQ-10.
- `WhatYouCarry.Game/Models/OverlayAttach.cs`: attaches an overlay model to its slot bone.

Out of scope: item stats beyond reduction and weight, the bank (PR-30).

Exit tests:

1. `ShieldNeedsOneHandedMelee` asserts a shield equip fails with a two-handed weapon and succeeds with a one-handed sword.
2. `WeightSlowsDodge` asserts a longer dodge cooldown with heavier armor.
3. `ReductionAppliesPerHit` asserts damage taken equals the hit minus the reduction, never below zero.
4. `RingAffixesApply` asserts a ring's affix triggers for the wearer.
5. `OverlayEnclosesLimb` runs the PR-57 tool on every armor model and asserts zero findings (D-135).
6. `EquipmentIsDeterministic` replays a record with equip changes and asserts one state hash.

Review focus: gameplay, presentation, content, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* what you wear shows on your body. Heavy pieces make you slower. The pieces never clip through each other.

### PR-23: Satchel, quick slot, throwables, potions, and weapon swap

Scope:

- `Core/Entities/Satchel.cs`: a fixed number of slots from OQ-3, with pick up, drop, and a full-satchel choice (D-19, D-41).
- `Core/Entities/QuickSlot.cs`: one consumable ready to use with the main weapon equipped (D-22).
- `Core/Combat/Bomb.cs`: a thrown projectile with a fuse, an area, and full self-damage (D-32).
- `Core/Entities/Potions.cs`: a health potion and a mana potion with the numbers from OQ-53.
- `Core/Entities/WeaponSwap.cs`: a swap from the satchel that takes the OQ-53 time and cannot be canceled (D-21).
- `WhatYouCarry.Game/Ui/SatchelScreen.cs`: the satchel screen for a controller at Deck size (D-90, G-15).

Out of scope: found amulets as items (PR-28), the bank (PR-30).

Exit tests:

1. `FullSatchelForcesChoice` asserts a pick up with a full satchel opens the drop choice and takes nothing until a choice.
2. `BombHurtsThrower` asserts full damage to the thrower inside the area (D-32).
3. `SwapCannotCancel` asserts a dodge during a swap does nothing until the swap ends.
4. `PotionRestores` asserts the health and mana amounts from OQ-53.
5. `QuickSlotKeepsWeapon` asserts the main weapon stays equipped through a throw.
6. `SatchelScreenNavigates` asserts every slot reachable with the controller model.
7. `SatchelIsDeterministic` replays a record with throws and swaps and asserts one state hash.

Review focus: gameplay, presentation, replay, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* you carry a small bag. Bombs, potions, spare weapons, and found amulets all compete for its few slots, and your own bomb can hurt you.

### PR-24: Bow and musket

Scope:

- `content/weapons/bow-short.json` and `content/weapons/musket.json`: the first ranged weapons with the numbers from OQ-54 (D-40, D-42).
- `Core/Combat/RangedWeapon.cs`: draw and release for the bow, and load and fire for the musket, with the reload as exposure (D-20).
- `Core/Combat/ArrowSplinter.cs` or the volley from OQ-13: the small area damage of arrows (D-42).
- `Core/Ai/HumanoidBrain.cs`: aim with lead from the arc solver, fire, and reload behavior for enemies (D-30).
- Contact sheets of both projectiles in flight at game zoom (D-83).
- The PR-10 projectile tests rerun over the real definitions (F-38).

Out of scope: exotics (PR-25), guns beyond the musket (PR-43 to PR-46).

Exit tests:

1. `MusketReloadIsExposure` asserts no attack during the reload and a dodge that still works.
2. `BowDrawScalesRange` asserts a longer flight at a longer draw.
3. `ArrowAreaDamages` asserts damage to a second target inside the OQ-13 area and none outside.
4. `EnemyLeadsTarget` asserts an enemy shot at a target in motion lands within one block of it.
5. `ProjectileTestsPassOnRoster` reruns the PR-10 property tests over the real definitions (F-38).
6. `ProjectilesDistinctInFlight` asserts the two projectiles differ in speed, drop, and trail parameters.
7. The owner confirms the musket reload feels like exposure and the bow feels like pressure, recorded as a decision.

Review focus: determinism, gameplay, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the first ranged weapons arrive. A shot from a musket is a commitment, and an arrow is quick and spreads its damage.

### PR-25: Exotic weapon and mana

Scope:

- `Core/Entities/Mana.cs`: a pool with slow regeneration and the numbers from OQ-55 (D-33, D-43).
- `content/weapons/exotic-bolt.json`: one exotic weapon with a charge time and a mana cost (D-33).
- `Core/Combat/ExoticWeapon.cs`: charge and release, with no attack at zero mana.
- The mana potion of PR-23 restores the pool.

Out of scope: a second exotic, the amulet (PR-28).

Exit tests:

1. `ManaRegeneratesSlowly` asserts the OQ-55 rate over one minute.
2. `ZeroManaBlocksCharge` asserts no shot at zero mana and a log line, not a silent nothing.
3. `ManaPotionRestores` asserts the OQ-55 amount.
4. `ExoticBotFinishesFloor` runs the greedy descender with only the exotic weapon and asserts the `bottom` end state on one hundred seeds.
5. `ManaIsDeterministic` replays a record with charges and asserts one state hash.

Review focus: gameplay, replay, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* magic is a weapon that spends a bar that refills slowly instead of a reload.

### PR-26: Enemies wear what they drop

Scope:

- `content/loot/*.json`: loot tables per enemy family and depth band, with the `loot-table` validator (D-16).
- `Core/Enemies/LoadoutGenerator.cs`: rolls an enemy's gear at spawn from its table through `LootRoller`, equips it, and marks it as the drop (D-16, D-49).
- `WhatYouCarry.Game/Models/EnemyCompose.cs`: composes the enemy model from the base body, its armor overlays, and its weapon, with the rarity color as an outline (D-49).
- Monster drops per OQ-23.
- Drop on death: the enemy's gear appears at its position with its rolls intact.

Out of scope: monsters as a family (Phase 4), the second humanoid family.

Exit tests:

1. `DropEqualsWorn` over ten thousand seeds asserts every dropped item was on the enemy that dropped it, by item id and rolls.
2. `EnemyUsesItsAffix` asserts an enemy with a lifesteal affix heals on a hit (D-49).
3. `RarityColorShows` asserts the outline color matches the rarity of the best item worn.
4. `LoadoutIsDeterministic` spawns one enemy with one seed twice and asserts equal gear.
5. `LoadoutIsInSatchelOnPickup` asserts the picked-up drop lands in the satchel with the same rolls.

Review focus: determinism, gameplay, content, presentation.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* you can see the good weapon on the dangerous enemy before you fight, which is the whole point of the title. What it drops is exactly what it carried.

### PR-32: LLM play socket and log reader

This PR precedes PR-27, because D-128 requires Tier 3 play on every economy PR (D-149).

Scope:

- `WhatYouCarry.Game/Socket/StateSocket.cs`: a local socket that sends the compact state JSON at one to five decisions per second and accepts one intent-level command per message (D-128). The state holds the floor, the timer, health, mana, the satchel, nearby entities with rarity, and the stairwell direction.
- `WhatYouCarry.Tools/LogReader/`: aggregates the run logs of a sweep, selects the outliers by end state and duration, and writes one summary file for a reader.
- A written procedure for the weekly session and the economy-PR session, with the model and budget from OQ-58 (D-117).

Out of scope: any unattended run (D-117), any vision input (Tier 4).

Exit tests:

1. `StateJsonValidates` asserts the state message validates against its schema (D-92).
2. `CommandBecomesIntent` asserts a command message produces the expected intent frame.
3. `SocketRateLimits` asserts no more than five decisions per second.
4. `LogReaderSelectsOutliers` runs the reader on a fixture sweep and asserts the shortest and longest runs in the summary.
5. One session, run by the owner, files its findings in `docs/questions.md`.

Review focus: input and CI boundaries, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* a language model can play the game slowly through a text channel to hunt for exploits a robot would not try. A summary tool hands it the strangest runs.

### PR-27: Skill points and the death payout

Scope:

- `Core/Economy/SkillPoints.cs`: points per kill from the enemy weight and a floor multiplier, plus a boss bonus, with the numbers from OQ-56 (D-44).
- `Core/Economy/DeathPayout.cs`: the retained share by depth from the curve in OQ-21 (D-52).
- `Core/Economy/RunTotal.cs`: the live run total, paid in full on ascend and at the retained share on death.
- `WhatYouCarry.Tools/EconomyTrials/`: the M-5 harness (D-154). It uses one thousand fixed seeds, the basic kit and an empty tree, and the policy set. Time is simulated ticks over 60 plus sixty seconds of hub cost per run start. The statistic has a bootstrap 95 percent interval. Policies: `AscendAtDepth(n)` and `DieAtDepth(n)` for n in 1 to 14, `ShallowRepeat`, `FullClear`, `EarlyDeath`.
- Points events in the run log.
- A Tier 3 session on this PR (D-128).

Out of scope: the tree (PR-29), the payout UI (PR-30).

Exit tests:

1. `PointsNeverNegative` asserts the run total never drops below zero.
2. `PayoutGrowsWithDepth` asserts a higher retained share at a deeper death.
3. `AscendPaysInFull` asserts the full total on ascend.
4. `TrialsAreDeterministic` runs the harness twice and asserts equal results.
5. `M5PassCondition` asserts at every depth the ascend lower bound exceeds the die upper bound, and deep ascend beats shallow repeat (D-154).
6. The Tier 3 session's findings are filed.

Review focus: economy, determinism, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* kills earn points. A death keeps a share that grows with depth, so a quick shallow death never pays better than a real run. A thousand simulated runs prove it before the change merges.

### PR-28: Amulet

Scope:

- `Core/Amulet/Amulet.cs`: the permanent amulet with one active and one passive slot, both on cooldowns, from the abilities in OQ-7 (D-34, D-38, D-39).
- `content/abilities/*.json`: the first three actives and three passives, with the `ability` validator.
- `Core/Amulet/FoundAmulet.cs`: a satchel item that unlocks one skill orb on ascension and forfeits it on death (D-37, D-41).
- The first orb, a basic active, unlocked from the start (D-153).
- The amulet model on the chest attachment (D-18).

Out of scope: the tree screen (PR-29), a fourth ability.

Exit tests:

1. `AmuletSurvivesDeath` asserts the amulet and its assignments after a death in a bot run.
2. `FoundAmuletForfeitsOnDeath` asserts no orb unlock after a death with a found amulet in the satchel.
3. `FoundAmuletUnlocksOnAscend` asserts the orb unlock after an ascension.
4. `ActiveRespectsCooldown` asserts a second use inside the cooldown does nothing.
5. `FirstOrbIsUnlocked` asserts a fresh profile has the basic active (D-153).
6. `AmuletIsDeterministic` replays a record with ability uses and asserts one state hash.

Review focus: gameplay, replay, content, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* you always keep one amulet with your chosen power. Amulets you find unlock new choices, but only if you make it out.

### PR-29: Skill tree and tree screen

Scope:

- `content/tree/tree.json`: the branches from OQ-8, with nodes, costs, orb gates at the tips, and proficiency buffs, with the `tree` validator (D-35, D-54).
- `Core/Tree/SkillTree.cs`: spend, refund, and the free respec (D-51). A gated node needs its orb.
- `WhatYouCarry.Game/Ui/TreeScreen.cs`: the tree screen for a controller at Deck size (D-90, G-15).

Out of scope: the hub scene (PR-30).

Exit tests:

1. `GatedNodeNeedsOrb` asserts a spend on a gated node fails without the orb and succeeds with it.
2. `RespecRefundsAll` asserts the full point total returns on respec.
3. `ProficiencyOnlyBuffs` asserts no stat below baseline for an unskilled class (D-35).
4. `TreeValidatorRejectsCycle` asserts a tree file with a cycle fails with the node names.
5. `EveryNodeReachableByController` asserts focus reaches every node with the controller model.
6. `TreeIsDeterministic` asserts equal stats from equal spends.

Review focus: gameplay, content, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* the between-run upgrade screen arrives, and you can change your choices freely.

### PR-30: Hub, bank, and loadout

Scope:

- `content/hub/hub.json`: the hub grid from OQ-59, with the bank, the skill shrine, and the descent entrance (D-9).
- `Core/Hub/Bank.cs`: banked items, deposit on ascension, and withdraw to a loadout (D-2).
- `Core/Hub/Loadout.cs`: the selected loadout, the basic kit always offered and never banked (D-153), and the immutable initial state written to the run record header (D-151).
- `WhatYouCarry.Game/Ui/LoadoutScreen.cs` and `BankScreen.cs`: item comparison with affixes, for a controller at Deck size (D-47, D-90).
- The run ends: death loses the loadout and the satchel, ascension banks both (D-2).

Out of scope: the profile file (PR-31), cloud sync (PR-52).

Exit tests:

1. `DeathLosesLoadout` asserts the loadout and satchel gone from the bank after a death.
2. `AscendBanksAll` asserts the loadout and every satchel item in the bank after an ascension.
3. `BasicKitAlwaysOffered` asserts the basic kit on the loadout screen with an empty bank and with a full bank.
4. `FreshProfileStartsRun` asserts a run starts from a new profile.
5. `EmptyBankStartsRun` asserts a run starts after deaths that empty the bank (D-153).
6. `InitialStateInHeader` asserts the run record header holds the loadout items with rolls, the tree state, and the amulet assignment (D-151).
7. `LoadoutScreenNavigates` asserts every item reachable with the controller model.

Review focus: gameplay, replay, presentation, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the camp between runs is where you choose what to risk. What you carry down can be lost, and a basic sword is always there so you can always go again.

### PR-31: Persistence and replay resume

Scope:

- `Core/Persistence/Profile.cs`: one file with the tree, the bank, the suspended-run pointer, a schema version, a generation number, and the last completed run id (D-94, D-152). Write to a temporary file, flush, then rename. The path is OQ-57.
- `Core/Persistence/Migrations.cs`: one migration per schema version, from version 1 (D-94).
- `Core/Persistence/RunRecordFile.cs`: the run record of PR-6 as a file, with a run id, and the torn-tail truncation (D-152).
- `Core/Persistence/Resume.cs`: replay to five seconds before the exit tick when the simulation version and content hash match (D-97, D-151). Otherwise, resume at floor start with a notice and a log line. A repeat crash on resume falls back to floor start.
- Idempotent completion: a run id already in the profile as completed cannot resume or pay again (D-152).

Out of scope: cloud saves (PR-52), the crash report UI (PR-55).

Exit tests:

1. `ProfileWriteIsAtomic` kills the process during a write in a fixture and asserts the old profile intact.
2. `KillAtEveryBoundary` kills at departure, ascension, death, and migration in turn, and asserts one consistent profile with no duplicated reward (D-152).
3. `CompletedRunCannotPayTwice` resumes a record whose run id is completed and asserts a refusal with a report.
4. `ResumeRewindsFiveSeconds` kills mid-floor and asserts the resume tick equals the exit tick minus three hundred.
5. `MismatchResumesAtFloorStart` bumps the simulation version and asserts a floor-start resume with a notice (D-151).
6. `RepeatCrashFallsBack` throws on the first resume and asserts a floor-start resume on the second.
7. `CorruptProfileRefused` flips one byte and asserts a refusal with a report that names the file.
8. `MigrationFromVersionOne` loads a version-1 fixture and asserts the current version with the same bank.

Review focus: persistence, replay, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 8 pass.

> *In plain English:* you can stop at any time and come back to almost the same moment. A crash at any instant leaves your bank whole and never pays a reward twice.

### M-4: Tokens per PR

Procedure: after each of the next ten PRs, read the token count from the harness usage report. Record it in a table in this file against the PR id only. D-137 permits a provider name in the handoff author field and the review files, not here.

### M-5: Points per hour by policy

Procedure: the PR-27 harness runs on every PR that touches points, loot, the timer, or the payout curve, and each night. Record the pass condition result and the interval bounds per depth in a table in this file, dated. A fail blocks the PR (G-19).

## 5. Sequence

One person owns the program. Items run one at a time in this order. Gate 2 must pass first.

1. Owner: answer OQ-22, OQ-51, OQ-52.
2. PR-21.
3. Owner: answer OQ-5 if still open, and the first three armor sets from OQ-10.
4. PR-22.
5. Owner: answer OQ-3 and OQ-53.
6. PR-23.
7. Owner: answer OQ-13 and OQ-54.
8. PR-24.
9. Owner: answer OQ-55.
10. PR-25.
11. Owner: answer OQ-23.
12. PR-26.
13. Owner: answer OQ-58.
14. PR-32.
15. Owner: answer OQ-21 and OQ-56.
16. PR-27.
17. Owner: answer OQ-7.
18. PR-28.
19. Owner: answer OQ-8.
20. PR-29.
21. Owner: answer OQ-59.
22. PR-30.
23. Owner: answer OQ-57.
24. PR-31.
25. M-4 table complete. M-5 table dated.
26. Owner: answer OQ-20. Friends play.
27. **← GATE 3 (full loop).** Every exit test in this file passes. The M-5 pass condition holds. Two friend sessions are filed. The owner signs the gate in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 3. Each names the PR it blocks.

Open:

- OQ-3: satchel slot count. Blocks PR-23.
- OQ-5: stagger and weight. Blocks PR-22 if still open after Phase 2.
- OQ-7: amulet abilities. Blocks PR-28.
- OQ-8: tree branches. Blocks PR-29.
- OQ-10: the weapon list, at least the first armor sets. Blocks PR-22.
- OQ-13: arrow area damage. Blocks PR-24.
- OQ-20: the friend playtest protocol. Blocks Gate 3.
- OQ-21: the death payout curve. Blocks PR-27.
- OQ-22: tier bands. Blocks PR-21.
- OQ-23: monster drops. Blocks PR-26.
- OQ-51: the first affixes. Blocks PR-21.
- OQ-52: rarity tiers and colors. Blocks PR-21 and PR-26.
- OQ-53: satchel and consumable numbers. Blocks PR-23.
- OQ-54: bow and musket numbers. Blocks PR-24.
- OQ-55: mana numbers. Blocks PR-25.
- OQ-56: point values. Blocks PR-27.
- OQ-57: the profile path. Blocks PR-31.
- OQ-58: the Tier 3 model and budget. Blocks PR-32.
- OQ-59: the hub layout. Blocks PR-30.
