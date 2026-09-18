# Technical names of the project areas

The STE rules permit a technical name as written (rule 1.5). This file lists the names of each project area. The `ste-writing` skill holds the process terms, which a session meets in every document.

Load this file when you write about the game, the art, the save files, or the code.

## The tools and the platforms

Godot, C#, .NET, xUnit, dotnet format, GitHub Actions, Steam, Steamworks, Steam Deck, Blockbench, JSON, JSONL, Metal, Vulkan, MoltenVK.

## The two harnesses

Claude Code, Codex.

## The project names

WhatYouCarry.Core, WhatYouCarry.Game, WhatYouCarry.Assets, WhatYouCarry.Tools, WhatYouCarry.Tests, DetMath.

## The game title

What You Carry.

## Game terms, the run

run, floor, stairwell, hunter, timer, boss, hub, bank, loadout, tick, seed, replay.

## Game terms, the gear

satchel, quick slot, amulet, skill orb, skill tree, skill point, affix, rarity, tier, band, potion, mana.

## Game terms, combat

cooldown, reload, hyper-armor, stagger, dodge, block, shield, intent.

## Game terms, the world

voxel, greedy meshing, pathfinding, lighting, ambient occlusion.

## Art terms

atlas, tile, palette, ramp, texture rule, texel, contact sheet, game zoom.

## Save terms

profile file, run record, basic kit, simulation version, content hash.

## The standard

ASD-STE100, STE.

## Code identifiers

A code identifier in backticks is one word (rule 8.6). The grammar rules do not read inside it.

## One term per concept

Rules 1.11 and 9.4 permit one name for one thing. Use the left term, and never the right ones.

| Use | Never |
|---|---|
| ascend | extract, cash out |
| descend | go deeper |
| bank | stash, vault |
| satchel | backpack, bag |
| stairwell | exit, stairs |
| hunter | ghost, chaser |

## Add a name

An `-ing` word that is a noun or a technical name passes the checker. The list lives in `SteRules.cs`. To add a name, add it to that list with a test.
