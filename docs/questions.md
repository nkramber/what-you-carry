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
2. **OQ-2. The .NET version.** Which .NET LTS does Godot 4.7.2 support? Blocks PR-1. Recommendation: verify on the Godot download page at scaffold time and record the answer in D-62.
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
16. **OQ-16. Harness attribution option.** Turn off the co-author trailer in the Claude Code settings file and confirm the Codex equivalent (F-15). Blocks PR-1.
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
