# Open questions

Status: active register. Owner: Nate. Started 2026-09-07 (D-144). Written in ASD-STE100.

This file holds every open question for the owner. Each question has an id (OQ-#). The numbers never change. A resolved question stays in this file with its date and the D-# id that resolved it. The design doc (`docs/design.md`) section 9 links here.

How to file a question (D-124, D-138):

- State the question plainly.
- Give the options with their tradeoffs.
- Give a recommendation and its reason.
- Name what the question blocks.
- If the question blocks the current work, stop the session after you file it.

## Register

1. **OQ-1. The palette.** The owner designs or approves the 32-color palette (D-85). Blocks PR-14. Recommendation: start from a dark-weighted ramp of eight hues with four values each. Scope corrected 2026-09-07 (audit R-9, F-36): this question blocks PR-14 only, not PR-1.
2. **OQ-2. The .NET version.** Which .NET LTS does Godot 4.7.2 support? Blocks PR-1. Recommendation: verify on the Godot download page at scaffold time and record the answer in D-62. Resolved 2026-09-07: D-169, .NET 8 LTS. Revised 2026-09-07: D-173, .NET 10 LTS.
3. **OQ-3. Satchel slot count.** Blocks PR-23. Recommendation: six slots.
4. **OQ-4. Timer lengths.** Per floor band and for boss floors (D-46, D-140). Blocks PR-17. Recommendation: three minutes on floors 1 to 5, four on 6 to 10, five on 11 to 15, plus two on boss floors.
5. **OQ-5. Stagger and weight.** Does heavy armor resist stagger (F-21)? Blocks PR-15. Recommendation: yes, heavy armor resists stagger and light armor does not.
6. **OQ-6. The hunter.** Look, sound, first speed, and speed curve. Blocks PR-17.
7. **OQ-7. Amulet abilities.** The first three actives and three passives. Blocks PR-28.
8. **OQ-8. Tree branches.** The branch list. Blocks PR-29. Recommendation: melee, bows, guns, exotics, survival, amulet.
9. **OQ-9. Enemy families.** The eight families and which are humanoid. Blocks PR-16 and PR-36 to PR-42.
10. **OQ-10. Weapon list.** The twelve weapons by class. Blocks PR-24 and PR-43 to PR-46.
11. **OQ-11. Boss concepts.** Three bosses. Blocks PR-33 to PR-35.
12. **OQ-12. The biome.** The one v1 biome concept. Blocks PR-9.
13. **OQ-13. Arrow area damage.** Splinter on impact, or a volley (D-42)? Blocks PR-24. Recommendation: splinter on impact.
14. **OQ-14. Accessibility.** None in v1 (D-53), or reward-neutral assist options? Blocks PR-53.
15. **OQ-15. Deck frame target.** 60 or 40 frames per second at 800p. Blocks M-3. Recommendation: 60, with 40 as the fallback if M-3 fails.
16. **OQ-16. Harness attribution option.** Turn off the co-author trailer in the Claude Code settings file and confirm the Codex equivalent (F-15). Blocks PR-1. Resolved 2026-09-07: D-172, project settings committed. Codex stays unverified. Revised 2026-09-07: D-175, empty strings, because the schema rejects booleans.
17. **OQ-17. Trademark search.** Search Steam and the USPTO for "What You Carry" (F-23). Blocks the store page.
18. **OQ-18. Purchase dates.** The SSD before PR-1, the Apple account before PR-51, the Steam fee before PR-52 (D-142). Resolved in part 2026-09-07: the SSD arrives 2026-09-08 (D-145). The two accounts stay open.
19. **OQ-19. Cloud save conflicts.** Newest wins with a prompt, or local wins? Blocks PR-52. Recommendation: always prompt, never resolve in silence.
20. **OQ-20. Friend playtest protocol.** The written protocol and the findings file format (D-134). Blocks Gate 3.
21. **OQ-21. Death payout curve.** The initial retained share by depth (D-52). Blocks PR-27. Recommendation: 10 percent at floor 1 to 60 percent at floor 14, linear.
22. **OQ-22. Tier bands.** The band table and the rare higher-tier chance (D-48). Blocks PR-21.
23. **OQ-23. Monster drops.** How a non-humanoid shows its drop (D-16, D-31). Blocks PR-26. Recommendation: a visible sack or gem on the model.
24. **OQ-24. The questions register.** D-120 named a questions file. This section now holds the open questions. Keep one register here, or create `docs/questions.md`? Recommendation: keep one register here. Resolved 2026-09-07: D-144 creates this file as the register.
25. **OQ-25. Initial gates and prerequisites.** Raised 2026-09-07. Which checks apply before PR-3 creates the lint tool and bit-identity job? The agent gate requires both on every PR. Several later gates also precede their prerequisites. Resolved 2026-09-07: D-148 (gate bootstrap clause), D-149 (four prerequisite fixes), D-150 (Gate 1 is a foundation gate). The R-9 scope conflict is a document correction (F-36).

    D-136 requires a playable build at each milestone, but Phase 1 has no Game layer. Section 8 places palette approval before PR-1, while OQ-1 names PR-14. This question blocks PR-1 and the final Phase 1 roadmap.

    Options: move prerequisites earlier, or define explicit checks for partial systems. Recommendation: put each prerequisite before its consumer and distinguish the foundation gate from playable milestones. See the [repository audit](reviews/2026-09-07-repository-audit.md).

26. **OQ-26. Replay state and compatibility.** Raised 2026-09-07. What initial state and version identities must a replay retain? D-2, D-35, and D-39 permit loadouts and skills that the seed does not determine. Blocks PR-6 and PR-31. Resolved 2026-09-07: D-151.

    Options: preserve the original simulation and content, or reject incompatible records with an explicit recovery policy. Recommendation: record an immutable initial state and the format, simulation, and content identities. Define the compatibility policy before suspend depends on replay.

27. **OQ-27. Save transaction and crash recovery.** Raised 2026-09-07. How do the three files commit one result after ascension or death? Separate atomic writes do not define recovery between file updates. Blocks PR-31. Resolved 2026-09-07: D-152.

    Options: a journal with repeat-safe recovery, or versioned file sets with one committed-generation marker. Recommendation: define a shared generation and test a process kill at each write boundary. Specify recovery of a partial intent record too.

28. **OQ-28. First loadout and empty bank.** Raised 2026-09-07. What starts the first run, and what permits another run after the player loses all banked gear? Blocks PR-30. Resolved 2026-09-07: D-153.

    Options: a basic replacement loadout, or a combat path that needs no banked gear. Recommendation: a basic replacement loadout with no resale or skill-point value. Confirm its effect on the loss rule in D-2.

29. **OQ-29. Economy comparison contract.** Raised 2026-09-07. Which matched trials prove that ascension beats death in points per hour? Blocks PR-27 and M-5. Resolved 2026-09-07: D-154.

    Specify the initial state, seed set, policy pair, elapsed-time rules, comparison statistic, and pass threshold. Options: compare aggregate rates across matched runs, or require a bound for each seed. Recommendation: compare aggregate rates with a separate check for extreme exploit cases. OQ-21 still owns the payout curve.
30. **OQ-30. The HEAD branch.** Raised 2026-09-07. The repository has no commits, and HEAD points at the unborn branch `docs/repository-audit`, created by the audit session. D-126 names `main` as trunk. The first commit lands where HEAD points. Options: reset HEAD to `main` with `git symbolic-ref HEAD refs/heads/main`, or keep the branch and merge it later. Recommendation: reset HEAD to `main` before PR-1. Blocks PR-1. Resolved 2026-09-07: D-156. HEAD was reset to `main` before the initial commit.
31. **OQ-31. The self-hosted macOS runner.** Raised 2026-09-07 (F-39). D-100 puts the macOS arm64 CI leg on the Mac Mini. Register the Mac Mini as a self-hosted GitHub Actions runner with a label, and keep it on. The runner shares the machine with the harness (D-105). Blocks PR-1. Recommendation: label `macos-arm64-self-hosted`, run the runner as a launch agent, and put its work directory on the external SSD (D-145). Resolved 2026-09-07: D-157. Runbook written 2026-09-07: `docs/runbooks/macos-runner.md`. Registration on 2026-09-08 (D-171).
32. **OQ-32. Branch protection on `main`.** Raised 2026-09-07. D-126 makes `main` the trunk and the owner the only merger. GitHub can enforce that. Options: require the CI checks and restrict pushes to the owner, or leave `main` open. Recommendation: protect `main`, require the `build`, `ste-check`, and `bit-identity` checks once they exist, and restrict pushes to the owner. Blocks nothing. Recommended before PR-1 merges. Resolved 2026-09-07: D-158. Revised 2026-09-07: D-170 defers protection until launch, because GitHub needs Pro or a public repository for it.
33. **OQ-33. The RNG algorithm.** Raised 2026-09-07. Blocks PR-3. Options: xoshiro128** (32-bit state, four words, fast, simple), PCG32 (64-bit multiply, one word of state), or SplitMix64 alone. Recommendation: xoshiro128** for the streams, seeded by SplitMix64 from the 64-bit run seed, with one stream per subsystem. It uses only integer operations, which are identical on every platform, and it is short enough to read in one screen (T-1). Resolved 2026-09-07: D-159.
34. **OQ-34. The state hash method.** Raised 2026-09-07. Blocks PR-3. Options: FNV-1a 64 over the raw bit patterns of every field in a fixed order, or SHA-256 over a serialized state. Recommendation: FNV-1a 64 over the bit patterns. It is a few lines, needs no dependency, and a fixed field order makes the hash a contract. SHA-256 belongs to the content hash (OQ-37), where tamper resistance matters more than speed. Resolved 2026-09-07: D-160.
35. **OQ-35. The DetMath accuracy target.** Raised 2026-09-07. Blocks PR-3. Options: an absolute error of 1e-6 on the reduced range, or 1e-4. Recommendation: range reduction to [-pi, pi], and a minimax polynomial of degree 7 for sine and cosine. The absolute error is at most 1e-6 against a double reference. `Atan2` at most 1e-6 in radians. `Sqrt` wraps the IEEE square root, so it is exact. Tests cover [-4 pi, 4 pi] for the range reduction. Resolved 2026-09-07: D-161.
36. **OQ-36. The intent record layout.** Raised 2026-09-07. Blocks PR-6. Recommendation: a fixed 16-byte frame with these fields: Resolved 2026-09-07: D-162.
    - tick as uint32.
    - yaw and pitch deltas as int16, in hundredths of a degree.
    - movement x and y as int8.
    - buttons as a uint16 bit mask.
    - a CRC32 as uint32.

    Hundredths of a degree give a finer step than any display can show, and int16 allows 327 degrees in one tick, which no input reaches.
37. **OQ-37. The run record file layout and the content hash.** Raised 2026-09-07. Blocks PR-5 and PR-6. Recommendation: one UTF-8 JSON header line, then the fixed frames of OQ-36. The header holds the format version, the simulation version, the content hash, the seed, and the initial state. The content hash is SHA-256 over each content file's relative path and bytes, in sorted path order. A JSON header is readable in a bug report. Fixed frames make a torn tail trivial to detect (D-152). Resolved 2026-09-07: D-163.
38. **OQ-38. The voxel grid limits.** Raised 2026-09-07. Blocks PR-7 and PR-9. Recommendation: block ids as one byte, a flat array, and a maximum floor of 128 by 32 by 128 blocks. That is 512 KB per floor, small enough to generate on the worker and hash in the bit-identity job. Larger floors need a decision. Resolved 2026-09-07: D-164.
39. **OQ-39. The player box and jump height.** Raised 2026-09-07. Blocks PR-7. Recommendation: a box of 0.6 by 1.8 by 0.6 meters, a jump that clears exactly one block, and no automatic step-up. One rule for the player and for the pathfinder (PR-16): a move is one block up or any drop. Resolved 2026-09-07: D-165.
40. **OQ-40. The corridor cross-section.** Raised 2026-09-07. Blocks PR-8 and PR-9. Recommendation: three blocks wide and three blocks high as the minimum for every corridor. The camera boom of PR-8 needs the width, and the jump of OQ-39 needs the height. Resolved 2026-09-07: D-166.
41. **OQ-41. The difficulty budget definition.** Raised 2026-09-07. Blocks PR-9. No enemies exist in Phase 1, so the budget cannot count them. Recommendation: each floor template names a budget number, and each room template names a weight. The generator's sum must be within 10 percent of the budget. PR-16 maps weights to enemy spawns. Resolved 2026-09-07: D-167.
42. **OQ-42. The content validator form.** Raised 2026-09-07. Blocks PR-5. Options: JSON Schema files with a validator package, or one hand-written C# validator per type. Recommendation: hand-written validators with an explicit required-field list per type. The types are few, the code is plain, and no dependency needs a decision entry (G-16). Revisit if the type count passes ten. Resolved 2026-09-07: D-168.
43. **OQ-43. The chunk size and mesh budget.** Raised 2026-09-07. Blocks PR-13. The mesher emits one mesh per chunk. Options: 16 by 32 by 16 chunks, which gives 64 meshes for a maximum floor (D-164), or one mesh per floor. Recommendation: 16 by 32 by 16 chunks and a budget of 64 world meshes plus one per entity. Chunks let the Game layer upload the next floor over several frames (PR-18) and keep one draw per chunk.
44. **OQ-44. The transition hitch budget.** Raised 2026-09-07. Blocks PR-18. Recommendation: no frame over 33 milliseconds on the Steam Deck across a floor transition, which is two frames at 60. The general target is OQ-15.
45. **OQ-45. The animation keyframe format.** Raised 2026-09-07. Blocks PR-15. Recommendation: one JSON file per animation. Each bone has a list of keyframes with a time in ticks at 60 Hz and an euler rotation in degrees. Interpolation is linear. Each tick range has a phase tag: `windup`, `active`, `recovery`, or `idle`. Ticks, not seconds, so the test of D-87 compares integers.
46. **OQ-46. The initial combat numbers.** Raised 2026-09-07. Blocks PR-15. Each tuned number is a decision (D-123). Recommendation:
    - player health 100.
    - sword windup 12 ticks, active 6, recovery 18, damage 34.
    - dodge cooldown 45 ticks at zero weight, dodge distance 3 meters.
    - walk 4 meters per second, sprint 7.

    Three sword hits kill a 100-health enemy, which matches fast and lethal (D-25).
47. **OQ-47. Default bindings and curves.** Raised 2026-09-07. Blocks PR-12. Recommendation for keyboard and mouse:
    - WASD move, mouse look.
    - left button attack, right button block.
    - space jump, left shift dodge, left control sprint.
    - E interact, Q throwable, R reload, Tab satchel, F amulet active.

    Recommendation for a controller:
    - left stick move, right stick look.
    - A jump, B dodge.
    - right trigger attack, left trigger block.
    - left bumper sprint, right bumper throwable.
    - X interact, Y amulet active, D-pad satchel, view button reload.

    Curves: a linear mouse and a cubic stick curve with a 15 percent dead zone.
48. **OQ-48. The sound parameter format.** Raised 2026-09-07. Blocks PR-20. Recommendation: one JSON file per sound. The fields are an oscillator type, a base frequency, a pitch sweep, an envelope, a noise mix, a low-pass cutoff, and a seed. This is the classic small-synthesizer set, and it covers swings, hits, steps, and alarms.
49. **OQ-49. The wall fade approach.** Raised 2026-09-07 (F-40). Blocks PR-13. Option one: a shader test that fades fragments inside a capsule between the camera and the player. Option two: per-block visibility flags in the mesher, as D-88's effect note said. Recommendation: the shader test. The mesher stays unchanged, no CPU work runs per frame, and the fade follows the camera at any speed. It revises the effect note of D-88 only, not the decision.
50. **OQ-50. A Steam Deck unit for M-3.** Raised 2026-09-07 (F-41). Blocks M-3. D-15 makes the Deck the floor, and M-3 measures on one. Do you own a Steam Deck? Options: buy or borrow a Deck before PR-13, or measure on the Windows box with a frame cap as a stand-in until then. Recommendation: a Deck before PR-13, because the stand-in cannot measure the Deck's GPU.
51. **OQ-51. The first affixes.** Raised 2026-09-07. Blocks PR-21. The behavior set is code, so each affix needs a decision. Recommendation for the first four: lifesteal (heal a share of damage dealt), burning (a small area of fire damage on hit, the arrow area of D-42 for arrows), swift (a shorter windup or draw), and sturdy (extra damage reduction). Each works for the player and for an enemy (D-49).
52. **OQ-52. Rarity tiers and colors.** Raised 2026-09-07 (F-42). Blocks PR-21 and PR-26. D-49 shows a rarity color on an enemy, but no decision names the rarities. Recommendation: three rarities separate from the tier. Common has no affix and no color. Rare has one affix and a blue outline. Epic has two affixes and a purple outline. Tier sets the base stats. Rarity sets the affix count.
53. **OQ-53. Satchel and consumable numbers.** Raised 2026-09-07. Blocks PR-23. Recommendation:
    - the satchel count of OQ-3.
    - a weapon swap of 45 ticks.
    - a bomb fuse of 90 ticks, a 2-meter area, and 60 damage.
    - a health potion of 50 over 60 ticks.
    - a mana potion of 50 at once.
54. **OQ-54. Bow and musket numbers.** Raised 2026-09-07. Blocks PR-24. Recommendation for the bow:
    - a 20-tick full draw.
    - 40 meters per second at full draw, gravity scale 1.
    - 25 damage plus 10 in a 1.5-meter area.

    Recommendation for the musket:
    - a 150-tick reload.
    - 120 meters per second, gravity scale 0.2.
    - 80 damage.

    Three sword hits, two arrows, or one musket ball kill a 100-health enemy.
55. **OQ-55. Mana numbers.** Raised 2026-09-07. Blocks PR-25. Recommendation:
    - a pool of 100, and regeneration of 2 per second.
    - an exotic bolt that costs 25, with a 30-tick charge and 40 damage.
    - a mana potion of 50.
56. **OQ-56. Point values.** Raised 2026-09-07. Blocks PR-27. Recommendation: points per kill equal the enemy weight times a floor multiplier of 1 plus 0.25 per floor past the first. The boss bonus is 500 times the band index. M-5 tunes these with the payout curve of OQ-21.
57. **OQ-57. The profile path.** Raised 2026-09-07. Blocks PR-31. Recommendation: `user://profile.json` for the profile and `user://runs/<run-id>.record` for run records, with `user://` as the Godot user directory on every platform. Never a hard-coded path (v1 section 5.3).
58. **OQ-58. The Tier 3 model and budget.** Raised 2026-09-07 (F-43). Blocks PR-32. D-128 sets the cadence but not the model or the budget. Recommendation: the socket is model-agnostic. The owner runs the weekly session with the harness of the day and a one-hour budget, and files findings in this register. The session never runs unattended (D-117).
59. **OQ-59. The hub layout.** Raised 2026-09-07. Blocks PR-30. Recommendation: a 24 by 8 by 24 block camp with no enemies. The bank chest, the skill shrine, and the descent entrance stand in a triangle, each within twelve blocks of the spawn.
60. **OQ-60. The boss pattern format.** Raised 2026-09-07. Blocks PR-33. Recommendation: a boss is a list of phases. Each phase has a health threshold, a list of attacks, and a movement rule. Each attack references a weapon or projectile definition and names its telegraph in ticks, with a minimum of 30 ticks. Boss health is 800 times the band index.
61. **OQ-61. The silhouette specification.** Raised 2026-09-07. Blocks PR-36 to PR-42. Recommendation: one JSON file per family with three fields. The first is the proportion ratios of head, torso, and limbs against the base body. The second is one distinctive box that no other family has. The third is a color role from the palette. A family is distinct when at least two of the three differ from every earlier family.
62. **OQ-62. The Tier 4 protocol.** Raised 2026-09-07. Blocks the Tier 4 pass. Recommendation: three sessions of ten minutes each, on a full floor, a boss fight, and the hub. Screenshots go through the socket at one to two per second. The model answers a fixed checklist. The items are the telegraphs it missed, the projectiles it did not tell apart, what the UI covered, and where the camera clipped. Findings go to this register.
63. **OQ-63. The music direction and track list.** Raised 2026-09-07. Blocks PR-50. Recommendation: six tracks: hub, dungeon bands 1 to 5, 6 to 10, and 11 to 15, boss, and hunter. A dark tempo of 80 to 100 beats per minute and a small synthesizer palette of four voices. The hunter track is a pulse that rises. The owner's ear decides (F-18).
64. **OQ-64. The prop set.** Raised 2026-09-07. Blocks PR-48. Recommendation: three props, barrel, crate, and torch stand, with 20 health each, no drops in v1 except a 5 percent potion chance from a barrel.
65. **OQ-65. The polygon budget, pivot rule, and UV rule.** Raised 2026-09-07. Blocks PR-49. Recommendation:
    - at most 40 boxes per character model and 12 per weapon.
    - the pivot at the center of the feet for a character, and at the grip for a weapon.
    - every face mapped to the atlas.
66. **OQ-66. The Deck checklist source.** Raised 2026-09-07 (F-44). Blocks PR-54. The Steam Deck verification requirements are an external fact. Recommendation: fetch Valve's current requirements at the start of PR-54. Record the date and the source in the Phase 5 roadmap. Copy each item into the exit evidence table.
67. **OQ-67. The notarization tool.** Raised 2026-09-07. Blocks PR-51. Options: Apple's notarization tool on the Mac Mini, or a cross-platform signature tool from Linux as the v1 design suggested. Recommendation: Apple's tool on the Mac Mini, because the Mac exists and is the CI runner (D-157). Record the steps and the date in the Phase 5 roadmap.
68. **OQ-68. The achievement list.** Raised 2026-09-07. Blocks PR-52. Recommendation: ten achievements at launch:
    - first ascension, and first death.
    - floor 5, floor 10, and the ending.
    - a full bank.
    - a run with the basic kit only.
    - a boss without a hit.
    - a floor under half the timer.
    - an ascension from floor 14.
69. **OQ-69. The settings list.** Raised 2026-09-07. Blocks PR-53. Recommendation: video with resolution, window mode, and vertical sync. Audio with master, effects, and music volumes. Input with sensitivity, invert look, and a full rebind. Accessibility per OQ-14.
70. **OQ-70. The crash report location.** Raised 2026-09-07. Blocks PR-55. Recommendation: `user://reports/<date>-<seed>/` with the report JSON and a copy of the run record, plus a copy-to-clipboard summary. V1 has no upload, so reports need no privacy policy.
71. **OQ-71. The cloud save file set.** Raised 2026-09-07 (F-45). Blocks PR-52. D-152 changed the files. Recommendation: the profile file and the suspended run record only. Completed run records and reports stay local.
72. **OQ-72. The review-gate mode file on PR-1.** Raised 2026-09-07. Blocks the PR-1 merge under G-19. D-185 reads `.github/review-gate-mode` from the base branch, and PR-1 creates that file. On PR-1 the base branch has no file, so the `review-gate` check is red with the message "The file '.github/review-gate-mode' is absent on the base branch". The roadmap and the D-185 Effect column say that PR-1 passes its own check, and that is not true. A local run of the tool against `origin/main` proved it. Options:
    - (a) The owner merges PR-1 with that red, and a decision records a one-time exception to G-19 for the bootstrap.
    - (b) The owner commits the one-line mode file to `main` before the merge. This bypasses the review of a `.github/` path (D-190 code set).
    - (c) The tool reads the head when the base has no file. This weakens D-185 for every later PR.

    Recommendation: (a). The red is the correct output of D-185 and T-2, the check is advisory until launch (D-180), and the exception ends when PR-1 merges. Resolved 2026-09-07: D-196, option (b). The owner put the file on `main` in `4ec9708`.
