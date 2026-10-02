using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Ai;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Items;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Tools.AssetQa;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The equipment, the armor, and the weight of PR-22 (D-753 to D-767; PR-22 exit tests 1 to 7). The player tests step a
/// player on a flat floor, and the loop tests play a bot policy with a loadout.
/// </summary>
public sealed class EquipmentTests
{
    /// <summary>The feet of a test player on the flat floor.</summary>
    private static readonly Vector3 Feet = new(8.5f, 1.0f, 8.5f);

    /// <summary>A tick with no target for the blade.</summary>
    private static readonly EntityBox[] NoTargets = [];

    /// <summary>The ticks that a loop test plays at most on one seed.</summary>
    private const uint TickLimit = 20000;

    private static ContentSet Content => TestWorld.Content;

    private static WeaponDefinition Sword => SimulationLoop.MainWeapon(Content);

    /// <summary>The equipment of one full set, with the tier-0 sword (D-753, D-762).</summary>
    private static Equipment SetOf(string set)
    {
        return Equipment.FromLoadout(Sword, ArmorSets.FullSet(set), Content);
    }

    private static Player NewPlayer(Equipment equipment)
    {
        return new Player(TestWorld.FlatFloor(16, 6), Feet, equipment, Player.MaxHealth);
    }

    private static ItemDefinition Item(string id)
    {
        return Content.Items.Single(item => item.Id == id);
    }

    private static ArmorDefinition Armor(string id)
    {
        return Content.Armors.Single(armor => armor.Id == id);
    }

    private static AffixDefinition Affix(string id)
    {
        return Content.Affixes.Single(affix => affix.Id == id);
    }

    /// <summary>A worn armor piece of the content set, with no affix.</summary>
    private static WornItem Piece(string id)
    {
        return new WornItem(Item(id), Armor(id), []);
    }

    /// <summary>A test-only shield: no shield is content before PR-101 (D-761).</summary>
    private static WornItem Shield()
    {
        ArmorDefinition armor = new("test-shield", 0, ArmorDefinition.ShieldSlot, 0, 0, "models/armor/test-shield.bbmodel");
        return new WornItem(new ItemDefinition("test-shield", ArmorDefinition.ShieldSlot, null, "test-shield"), armor, []);
    }

    /// <summary>A test-only armor piece of one slot and one weight, for a bound of the weight rules.</summary>
    private static WornItem Weighing(string slot, long weight)
    {
        string id = "test-" + slot;
        ArmorDefinition armor = new(id, 0, slot, 0, weight, "models/armor/" + id + ".bbmodel");
        return new WornItem(new ItemDefinition(id, slot, null, id), armor, []);
    }

    private static Intent Idle(uint tick, ushort buttons = 0)
    {
        return new Intent(tick, 0, 0, 0, 0, buttons);
    }

    /// <summary>PR-22 exit test 1. A shield fails with a two-handed weapon and goes on with the one-handed sword (D-26).</summary>
    [Fact]
    public void ShieldNeedsOneHandedMelee()
    {
        WeaponDefinition pick = Content.Weapons.Single(weapon => weapon.Id == "overseer-pick");
        Assert.True(pick.IsTwoHanded);

        Equipment twoHanded = new(pick);
        ContextException refused = Assert.Throws<ContextException>(() => twoHanded.Equip(Shield()));
        Assert.Contains("D-26", refused.Message, StringComparison.Ordinal);
        Assert.Contains("item=test-shield", refused.Message, StringComparison.Ordinal);
        Assert.Empty(twoHanded.Worn);

        Equipment oneHanded = new(Sword);
        Assert.Equal(WeaponDefinition.OneHanded, Sword.Handedness);
        oneHanded.Equip(Shield());
        Assert.Equal("test-shield", Assert.Single(oneHanded.Worn).Item.Id);
    }

    /// <summary>
    /// PR-22 exit test 2. Heavier armor gives a longer dodge cooldown: 45 ticks and 1 more for each point of weight
    /// (D-755). A press of the dodge bit starts the cooldown of the worn weight, and the next roll waits for it.
    /// </summary>
    [Fact]
    public void WeightSlowsDodge()
    {
        Assert.Equal(45, Weight.DodgeCooldownTicks(0));
        Assert.Equal(51, Weight.DodgeCooldownTicks(6));
        Assert.Equal(61, Weight.DodgeCooldownTicks(16));
        Assert.Equal(75, Weight.DodgeCooldownTicks(30));

        int bare = TicksBetweenRolls(new Equipment(Sword));
        int light = TicksBetweenRolls(SetOf("leathers"));
        int medium = TicksBetweenRolls(SetOf("brigandine"));
        int heavy = TicksBetweenRolls(SetOf("blast"));
        Assert.Equal(45, bare);
        Assert.Equal(51, light);
        Assert.Equal(61, medium);
        Assert.Equal(75, heavy);
    }

    /// <summary>The fewest ticks from the press of one roll to a press that starts the next one.</summary>
    private static int TicksBetweenRolls(Equipment equipment)
    {
        for (int gap = 1; gap < 200; gap++)
        {
            Player player = NewPlayer(equipment);
            player.Step(Idle(0, Button.Dodge), 0, 0, NoTargets);
            Assert.True(player.StartedRoll);
            Assert.Equal(Weight.DodgeCooldownTicks(equipment.Weight), player.DodgeCooldown);
            for (int tick = 1; tick < gap; tick++)
            {
                player.Step(Idle((uint)tick), 0, 0, NoTargets);
            }

            player.Step(Idle((uint)gap, Button.Dodge), 0, 0, NoTargets);
            if (player.StartedRoll)
            {
                return gap;
            }
        }

        throw new InvalidOperationException("No second roll started within 200 ticks.");
    }

    /// <summary>
    /// PR-22 exit test 3. A hit takes its damage minus the worn reduction, never below zero (D-754). Plain damage
    /// lands in full (D-759).
    /// </summary>
    [Fact]
    public void ReductionAppliesPerHit()
    {
        Equipment blast = SetOf("blast");
        Assert.Equal(15, blast.Reduction);
        Assert.Equal(19, blast.AfterReduction(34));
        Assert.Equal(0, blast.AfterReduction(10));
        Assert.Equal(0, blast.AfterReduction(15));
        Assert.Throws<ContextException>(() => blast.AfterReduction(-1));

        Player player = NewPlayer(blast);
        Assert.True(player.TakeHit(34));
        Assert.Equal(Player.MaxHealth - 19, player.Health);
        Assert.True(player.TakeHit(10));
        Assert.Equal(Player.MaxHealth - 19, player.Health);
        Assert.True(player.TakePlainDamage(8));
        Assert.Equal(Player.MaxHealth - 27, player.Health);

        Assert.Equal(5, SetOf("leathers").Reduction);
        Assert.Equal(10, SetOf("brigandine").Reduction);
        Player bare = NewPlayer(new Equipment(Sword));
        bare.TakeHit(34);
        Assert.Equal(Player.MaxHealth - 34, bare.Health);
    }

    /// <summary>
    /// PR-22 exit test 4. The affixes of a worn ring act for the wearer in the loop (D-747, D-750). Swift shortens the
    /// windup of the sword. On every tick of a full clear with a ring of burning and a ring of lifesteal, each enemy and
    /// the player end the tick at the health that the hits, the burns, and the heals give, in hit order (D-751).
    /// </summary>
    [Fact]
    public void RingAffixesApply()
    {
        SimulationLoop swift = new(1UL, Content, [new LoadoutEntry("ring-plain", ["swift"])]);
        Assert.Equal(9, swift.Player.Weapon.WindupTicks);
        Assert.Equal(12, swift.Weapon.WindupTicks);

        IReadOnlyList<LoadoutEntry> rings = [new LoadoutEntry("ring-plain", ["burning"]), new LoadoutEntry("ring-plain", ["lifesteal"])];
        int hitTicks = 0;
        int heals = 0;
        int bystanderBurns = 0;
        for (ulong seed = 1; seed <= 12 && (heals == 0 || bystanderBurns == 0 || hitTicks < 20); seed++)
        {
            (int hits, int healed, int burned) = PlayAndCheckAffixes(seed, rings);
            hitTicks += hits;
            heals += healed;
            bystanderBurns += burned;
        }

        Assert.True(hitTicks >= 20, $"The full clears gave {hitTicks} ticks with a hit on an enemy.");
        Assert.True(heals > 0, "No hit healed a hurt player.");
        Assert.True(bystanderBurns > 0, "No burn reached an enemy that no hit met.");
    }

    /// <summary>
    /// Plays one full clear and checks each tick with a hit of the blade against the contract of the hooks. It returns
    /// the ticks with a hit on an enemy, the hits that healed a hurt player, and the burns of an enemy that no hit met.
    /// </summary>
    private static (int Hits, int Healed, int Burned) PlayAndCheckAffixes(ulong seed, IReadOnlyList<LoadoutEntry> rings)
    {
        SimulationLoop loop = new(seed, Content, rings);
        FullClearer policy = new(Content);
        long heal = Sword.Damage * Affix("lifesteal").Parameter(AffixDefinition.PercentName) / 100;
        long burn = Affix("burning").Parameter(AffixDefinition.DamageName);
        float radius = Affix("burning").Parameter(AffixDefinition.RadiusName) / 100.0f;
        int hitTicks = 0;
        int healed = 0;
        int burned = 0;
        while (!loop.Ended && loop.Tick < TickLimit)
        {
            int floor = loop.Floor;
            int playerBefore = loop.Player.Health;
            long[] health = loop.Enemies.Select(enemy => (long)enemy.Health).ToArray();
            Vector3[] feet = loop.Enemies.Select(enemy => enemy.Feet).ToArray();
            loop.Step(policy.Next(loop));
            if (loop.Floor != floor || loop.Player.LastHits.Count == 0)
            {
                continue;
            }

            // The contract, in hit order: a hit lands on a live enemy, heals the player, and burns each live enemy in the
            // radius of the struck feet. A hit on the Overseer runs nothing (D-758).
            long gained = 0;
            bool[] struck = new bool[health.Length];
            bool enemyHit = false;
            foreach (SwordHit hit in loop.Player.LastHits)
            {
                int index = IndexOf(loop, hit.Owner);
                if (index < 0 || health[index] == 0)
                {
                    continue;
                }

                enemyHit = true;
                struck[index] = true;
                health[index] = Math.Max(0, health[index] - hit.Damage);
                gained += heal;
                for (int other = 0; other < health.Length; other++)
                {
                    if (health[other] > 0 && Distance(feet[index], feet[other]) <= radius)
                    {
                        health[other] = Math.Max(0, health[other] - burn);
                        burned += struck[other] ? 0 : 1;
                    }
                }
            }

            if (!enemyHit)
            {
                continue;
            }

            hitTicks++;
            for (int index = 0; index < health.Length; index++)
            {
                Assert.True(health[index] == loop.Enemies[index].Health, $"Seed {seed}, tick {loop.Tick - 1}: enemy {index} has {loop.Enemies[index].Health} health, and the hooks give {health[index]}.");
            }

            long taken = loop.LastActions.Where(action => action.Kind == ActionEventKind.PlayerHit).Sum(action => action.Value);
            long expected = Math.Max(0, Math.Min(Player.MaxHealth, playerBefore + gained) - taken);
            Assert.True(expected == loop.Player.Health, $"Seed {seed}, tick {loop.Tick - 1}: the player has {loop.Player.Health} health, and the heal of {gained} and the hits of {taken} give {expected}.");
            healed += playerBefore < Player.MaxHealth ? 1 : 0;
        }

        return (hitTicks, healed, burned);
    }

    /// <summary>The index of the enemy of an owner id, or -1 for the Overseer.</summary>
    private static int IndexOf(SimulationLoop loop, int owner)
    {
        for (int index = 0; index < loop.Enemies.Count; index++)
        {
            if (loop.Enemies[index].Owner == owner)
            {
                return index;
            }
        }

        Assert.NotNull(loop.Hunter);
        Assert.Equal(loop.Hunter.Owner, owner);
        return -1;
    }

    private static float Distance(Vector3 first, Vector3 second)
    {
        float dx = first.X - second.X;
        float dy = first.Y - second.Y;
        float dz = first.Z - second.Z;
        return MathF.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
    }

    /// <summary>
    /// PR-22 exit test 5. The PR-57 check passes every armor overlay: each box encloses the body box of its name, and no
    /// box clips at the rest pose or at a keyframe of any clip of the body (D-135, D-300, D-767).
    /// </summary>
    [Fact]
    public void OverlayEnclosesLimb()
    {
        string contentRoot = Path.Combine(RepositoryRoot.Find(), "content");
        AssetSet set = AssetSet.Read(contentRoot);

        Assert.Empty(AssetQaCommand.Findings(set, contentRoot).Select(finding => finding.Line()));
        Assert.Equal(Content.Armors.Count, set.Overlays.Count);
        foreach (ArmorDefinition armor in Content.Armors)
        {
            Assert.Contains(set.Overlays, overlay => overlay.Path == armor.Model);
        }
    }

    /// <summary>
    /// PR-22 exit test 6. A record whose header loadout holds an armor set, or a ring with affixes, replays to one state
    /// hash on two replays, and the replays end on the hash of the live run (G-5, D-765).
    /// </summary>
    [Fact]
    public void EquipmentIsDeterministic()
    {
        List<IReadOnlyList<LoadoutEntry>> loadouts =
        [
            [],
            ArmorSets.FullSet("leathers"),
            ArmorSets.FullSet("brigandine"),
            ArmorSets.FullSet("blast"),
            [new LoadoutEntry("ring-plain", ["burning", "lifesteal"]), new LoadoutEntry("ring-plain", ["swift"])],
        ];

        List<StateHash> live = [];
        foreach (IReadOnlyList<LoadoutEntry> loadout in loadouts)
        {
            MemorySink sink = new();
            RunRecorder recorder = new(sink, RunRecord.NewHeader(Content.Hash, 5UL, loadout));
            SimulationLoop loop = new(5UL, Content, loadout);
            FullClearer policy = new(Content);
            while (!loop.Ended && loop.Tick < 3000)
            {
                Intent intent = policy.Next(loop);
                recorder.Record(intent);
                loop.Step(intent);
            }

            ReplayResult first = RunReplayer.Replay(sink.Bytes, Content, new JsonlLogger(new CollectingSink()));
            ReplayResult second = RunReplayer.Replay(sink.Bytes, Content, new JsonlLogger(new CollectingSink()));
            Assert.Equal(loop.Hash(), first.Loop.Hash());
            Assert.Equal(first.Loop.Hash(), second.Loop.Hash());
            Assert.Equal(loadout.Select(entry => entry.Item), first.Header.Loadout.Select(entry => entry.Item));
            live.Add(loop.Hash());
        }

        // The loadout enters the run: the bare run and the heavy run end apart.
        Assert.NotEqual(live[0], live[3]);
    }

    /// <summary>
    /// PR-22 exit test 7. A hit staggers a player below the weight of 24 and does not stagger a player at or above it
    /// (D-314, D-316, D-756). The hit deals its damage in both cases.
    /// </summary>
    [Fact]
    public void HeavyArmorResistsStagger()
    {
        Assert.Equal(24, Weight.StaggerResistWeight);

        Equipment below = new(Sword);
        below.Equip(Weighing(ArmorDefinition.ChestSlot, 23));
        Player light = NewPlayer(below);
        light.TakeHit(10);
        Assert.Equal(Player.StaggerTicks, light.StaggerRemaining);
        Assert.Equal(Player.MaxHealth - 10, light.Health);

        Equipment at = new(Sword);
        at.Equip(Weighing(ArmorDefinition.ChestSlot, 24));
        Player heavy = NewPlayer(at);
        heavy.TakeHit(10);
        Assert.Equal(0, heavy.StaggerRemaining);
        Assert.Equal(0, heavy.GuardRemaining);
        Assert.Equal(Player.MaxHealth - 10, heavy.Health);

        // A full Blast plate set resists, and so do its head, chest, and legs. A full Company brigandine set does not.
        Assert.Equal(0, StaggerAfterHit(SetOf("blast")));
        Equipment threePieces = new(Sword);
        threePieces.Equip(Piece("blast-head"));
        threePieces.Equip(Piece("blast-chest"));
        threePieces.Equip(Piece("blast-legs"));
        Assert.Equal(25, threePieces.Weight);
        Assert.Equal(0, StaggerAfterHit(threePieces));
        Assert.Equal(16, SetOf("brigandine").Weight);
        Assert.Equal(Player.StaggerTicks, StaggerAfterHit(SetOf("brigandine")));
    }

    private static int StaggerAfterHit(Equipment equipment)
    {
        Player player = NewPlayer(equipment);
        player.TakeHit(34);
        return player.StaggerRemaining;
    }

    /// <summary>
    /// Worn weight takes 0.5 percent off the walk and the sprint for each point (D-757). One tick of a walk and of a
    /// sprint moves the heavy set 85 percent of the bare distance.
    /// </summary>
    [Fact]
    public void WeightSlowsTheWalkAndTheSprint()
    {
        Assert.Equal(1.0f, Weight.SpeedFactor(0));
        Assert.Equal(0.97f, Weight.SpeedFactor(6), 6);
        Assert.Equal(0.92f, Weight.SpeedFactor(16), 6);
        Assert.Equal(0.85f, Weight.SpeedFactor(30), 6);
        Assert.Throws<ContextException>(() => Weight.SpeedFactor(-1));
        Assert.Throws<ContextException>(() => Weight.SpeedFactor(Weight.LargestWeight + 1));

        foreach (ushort buttons in new ushort[] { 0, Button.Sprint })
        {
            float bare = OneTickForward(new Equipment(Sword), buttons);
            float heavy = OneTickForward(SetOf("blast"), buttons);
            float speed = buttons == 0 ? PlayerBody.WalkSpeed : PlayerBody.SprintSpeed;
            Assert.Equal(speed * PlayerBody.TickSeconds, bare, 4);
            Assert.Equal(0.85f * bare, heavy, 5);
        }
    }

    /// <summary>The distance of one tick of full forward input on the flat floor.</summary>
    private static float OneTickForward(Equipment equipment, ushort buttons)
    {
        Player player = NewPlayer(equipment);
        Vector3 start = player.Body.Position;
        player.Step(new Intent(0, 0, 0, 0, 127, buttons), 0, 0, NoTargets);
        return Distance(start, player.Body.Position);
    }

    /// <summary>
    /// A hit on the Overseer runs no affix, and burning never reaches it (D-758, F-205). A player with a ring of
    /// lifesteal and a ring of burning in a heavy set swings at the Overseer until a hit of the blade lands on it while
    /// the player is hurt, and that hit heals nothing.
    /// </summary>
    [Fact]
    public void TheOverseerStaysOutsideTheAffixLoop()
    {
        List<LoadoutEntry> loadout = [new LoadoutEntry("ring-plain", ["lifesteal"]), new LoadoutEntry("ring-plain", ["burning"])];
        loadout.AddRange(ArmorSets.FullSet("blast"));
        ContentSet content = WithTimer(TestWorld.PeacefulContent, 2);
        int hurtHits = 0;
        for (ulong seed = 1; seed <= 6 && hurtHits == 0; seed++)
        {
            SimulationLoop loop = new(seed, content, loadout);
            while (!loop.Ended && loop.Tick < TickLimit && hurtHits == 0)
            {
                int before = loop.Player.Health;
                loop.Step(FaceTheOverseerAndSwing(loop));
                if (loop.Hunter is null || !loop.Player.LastHits.Any(hit => hit.Owner == loop.Hunter.Owner))
                {
                    continue;
                }

                long taken = loop.LastActions.Where(action => action.Kind == ActionEventKind.PlayerHit).Sum(action => action.Value);
                Assert.Equal(Math.Max(0, before - taken), loop.Player.Health);
                hurtHits += before < Player.MaxHealth ? 1 : 0;
            }
        }

        Assert.True(hurtHits > 0, "No hit of the blade landed on the Overseer while the player was hurt.");
    }

    /// <summary>One intent that turns the look to the Overseer, or holds it, and presses the attack bit on every other tick.</summary>
    private static Intent FaceTheOverseerAndSwing(SimulationLoop loop)
    {
        ushort buttons = loop.Tick % 2 == 0 ? Button.Attack : (ushort)0;
        if (loop.Hunter is null)
        {
            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }

        int yaw = HumanoidBrain.YawToward(loop.Player.Feet, loop.Hunter.Body.Position);
        return new Intent(loop.Tick, BotIntent.TurnToward(loop.Yaw, yaw), 0, 0, 0, buttons);
    }

    private static ContentSet WithTimer(ContentSet content, long seconds)
    {
        List<FloorTemplate> floors = [];
        foreach (FloorTemplate floor in content.Floors)
        {
            floors.Add(floor with { TimerSeconds = seconds, BossTimerSeconds = 0 });
        }

        return content with { Floors = floors };
    }

    /// <summary>
    /// The equip rules (D-18, D-55, D-750, D-763): each modeled slot holds one item, two rings fit and a third does not,
    /// no worn item is a weapon, an item carries an affix once, and an armor item carries the armor file that it names.
    /// </summary>
    [Fact]
    public void EquipRulesHold()
    {
        Equipment equipment = new(Sword);
        equipment.Equip(Piece("leathers-head"));
        Assert.Contains("holds an item", Assert.Throws<ContextException>(() => equipment.Equip(Piece("blast-head"))).Message, StringComparison.Ordinal);

        WornItem ring = new(Item("ring-plain"), null, [Affix("burning")]);
        equipment.Equip(ring);
        equipment.Equip(ring);
        Assert.Contains("both ring slots", Assert.Throws<ContextException>(() => equipment.Equip(ring)).Message, StringComparison.Ordinal);
        Assert.Equal(2, equipment.Affixes.Count);

        WornItem sword = new(Item("sword-basic"), null, []);
        Assert.Contains("D-422", Assert.Throws<ContextException>(() => equipment.Equip(sword)).Message, StringComparison.Ordinal);

        WornItem twice = new(Item("ring-plain"), null, [Affix("swift"), Affix("swift")]);
        Assert.Contains("D-750", Assert.Throws<ContextException>(() => new Equipment(Sword).Equip(twice)).Message, StringComparison.Ordinal);

        WornItem wrongArmor = new(Item("blast-head"), Armor("blast-chest"), []);
        Assert.Contains("D-763", Assert.Throws<ContextException>(() => new Equipment(Sword).Equip(wrongArmor)).Message, StringComparison.Ordinal);
        WornItem noArmor = new(Item("blast-head"), null, []);
        Assert.Contains("no armor file", Assert.Throws<ContextException>(() => new Equipment(Sword).Equip(noArmor)).Message, StringComparison.Ordinal);
        WornItem ringWithArmor = new(Item("ring-plain"), Armor("blast-head"), []);
        Assert.Contains("names no armor", Assert.Throws<ContextException>(() => new Equipment(Sword).Equip(ringWithArmor)).Message, StringComparison.Ordinal);
    }

    /// <summary>A loadout entry that names an absent item or affix fails, and the error names the entry (D-766, T-2).</summary>
    [Fact]
    public void ALoadoutThatDoesNotResolveFails()
    {
        ContextException noItem = Assert.Throws<ContextException>(() => new SimulationLoop(1UL, Content, ArmorSets.FullSet("velvet")));
        Assert.Contains("velvet-head", noItem.Message, StringComparison.Ordinal);
        Assert.Contains("loadoutEntry=0", noItem.Message, StringComparison.Ordinal);

        ContextException noAffix = Assert.Throws<ContextException>(() => new SimulationLoop(1UL, Content, [new LoadoutEntry("ring-plain", ["sturdy"])]));
        Assert.Contains("affix=sturdy", noAffix.Message, StringComparison.Ordinal);
        Assert.Contains("loadoutItem=ring-plain", noAffix.Message, StringComparison.Ordinal);
    }

    /// <summary>The repository holds the three sets of D-753 with the numbers of D-754, at tier 0 (D-760).</summary>
    [Fact]
    public void RepositoryHoldsTheThreeArmorSets()
    {
        Dictionary<string, (long[] Reduction, long[] Weight)> table = new()
        {
            ["leathers"] = ([1, 2, 1, 1], [1, 2, 2, 1]),
            ["brigandine"] = ([2, 4, 3, 1], [3, 6, 4, 3]),
            ["blast"] = ([3, 6, 4, 2], [6, 12, 7, 5]),
        };

        Assert.Equal(12, Content.Armors.Count);
        foreach ((string set, (long[] reduction, long[] weight)) in table)
        {
            for (int index = 0; index < ArmorSets.SetSlots.Count; index++)
            {
                string slot = ArmorSets.SetSlots[index];
                ArmorDefinition armor = Armor(set + "-" + slot);
                Assert.Equal(0, armor.Tier);
                Assert.Equal(slot, armor.Slot);
                Assert.Equal(reduction[index], armor.Reduction);
                Assert.Equal(weight[index], armor.Weight);
                Assert.Equal("models/armor/" + set + "-" + slot + ".bbmodel", armor.Model);
                Assert.Equal(new ItemDefinition(armor.Id, slot, null, armor.Id), Item(armor.Id));
            }
        }

        Assert.Equal((5L, 6L), (SetOf("leathers").Reduction, SetOf("leathers").Weight));
        Assert.Equal((10L, 16L), (SetOf("brigandine").Reduction, SetOf("brigandine").Weight));
        Assert.Equal((15L, 30L), (SetOf("blast").Reduction, SetOf("blast").Weight));
    }

    /// <summary>An armor file or an armor item that breaks its shape fails with the file and the field (D-763, T-2).</summary>
    [Fact]
    public void ArmorFilesRejectBadInput()
    {
        const string Good = """{ "id": "a", "tier": 0, "slot": "head", "reduction": 1, "weight": 1, "model": "models/armor/a.bbmodel" }""";
        Assert.Equal("a", ReadArmor(Good).Id);
        Assert.Contains("'slot'", ReadArmorError(Good.Replace("\"head\"", "\"amulet\"", StringComparison.Ordinal)), StringComparison.Ordinal);
        Assert.Contains("'weight'", ReadArmorError(Good.Replace("\"weight\": 1", "\"weight\": 31", StringComparison.Ordinal)), StringComparison.Ordinal);
        Assert.Contains("'reduction'", ReadArmorError(Good.Replace("\"reduction\": 1", "\"reduction\": -1", StringComparison.Ordinal)), StringComparison.Ordinal);
        Assert.Contains("'tier'", ReadArmorError(Good.Replace("\"tier\": 0", "\"tier\": -1", StringComparison.Ordinal)), StringComparison.Ordinal);
        Assert.Contains("D-300", ReadArmorError(Good.Replace("models/armor/a", "models/a", StringComparison.Ordinal)), StringComparison.Ordinal);

        Assert.Contains("'armor'", ReadItemError("""{ "id": "h", "slot": "head" }"""), StringComparison.Ordinal);
        Assert.Contains("'armor'", ReadItemError("""{ "id": "s", "slot": "weapon", "weapon": "sword-basic", "armor": "a" }"""), StringComparison.Ordinal);
        Assert.Contains("'armor'", ReadItemError("""{ "id": "r", "slot": "ring", "armor": "a" }"""), StringComparison.Ordinal);
        Assert.Contains("'weapon'", ReadItemError("""{ "id": "h", "slot": "head", "armor": "a", "weapon": "sword-basic" }"""), StringComparison.Ordinal);
    }

    private static ArmorDefinition ReadArmor(string text)
    {
        return ArmorDefinition.FromMembers("armor/a.json", JsonObjectReader.Read("armor/a.json", Encoding.UTF8.GetBytes(text)));
    }

    private static string ReadArmorError(string text)
    {
        return Assert.Throws<ContextException>(() => ReadArmor(text)).Message;
    }

    private static string ReadItemError(string text)
    {
        return Assert.Throws<ContextException>(() => ItemDefinition.FromMembers("items/x.json", JsonObjectReader.Read("items/x.json", Encoding.UTF8.GetBytes(text)))).Message;
    }

    /// <summary>The loader rejects an armor item whose armor file is absent, or of another slot (D-763).</summary>
    [Fact]
    public void TheLoaderChecksTheArmorOfEachItem()
    {
        ReplacedFileSource absent = new("items/blast-head.json", """{ "id": "blast-head", "slot": "head", "armor": "velvet-head" }""");
        ContextException missing = Assert.Throws<ContextException>(() => new ContentLoader(absent).Load());
        Assert.Contains("velvet-head", missing.Message, StringComparison.Ordinal);
        Assert.Contains("items/blast-head.json", missing.Message, StringComparison.Ordinal);

        ReplacedFileSource crossed = new("items/blast-head.json", """{ "id": "blast-head", "slot": "head", "armor": "blast-chest" }""");
        ContextException mismatch = Assert.Throws<ContextException>(() => new ContentLoader(crossed).Load());
        Assert.Contains("'slot'", mismatch.Message, StringComparison.Ordinal);
        Assert.Contains("blast-chest", mismatch.Message, StringComparison.Ordinal);
    }

    /// <summary>An armor item takes the tier of its armor file in the loot roll, so tier 0 armor fits floors 1 to 5 alone (D-745, D-760, D-763).</summary>
    [Fact]
    public void ArmorItemsRollAtTheirTier()
    {
        List<ItemDefinition> armor = Content.Items.Where(item => item.Armor is not null).ToList();
        Rng early = Rng.ForStream(31UL, RngStream.Loot, 1);
        for (int roll = 0; roll < 200; roll++)
        {
            RolledItem item = LootRoller.Roll(early, 1, Content.Items, Content.Weapons, Content.Armors, Content.Affixes);
            if (item.Item.Armor is not null)
            {
                Assert.Equal(0, item.Tier);
            }
        }

        Rng deep = Rng.ForStream(31UL, RngStream.Loot, 12);
        ContextException none = Assert.Throws<ContextException>(() =>
        {
            for (int roll = 0; roll < 100; roll++)
            {
                LootRoller.Roll(deep, 12, armor, Content.Weapons, Content.Armors, Content.Affixes);
            }
        });
        Assert.Contains("tier", none.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A header with a loadout survives a write and a read, entry for entry, and its text holds the entries of D-766.
    /// A loadout id with a letter outside its form fails at the write (T-2).
    /// </summary>
    [Fact]
    public void ALoadoutHeaderRoundTrips()
    {
        string hash = Content.Hash;
        IReadOnlyList<LoadoutEntry> loadout = [new LoadoutEntry("blast-chest", []), new LoadoutEntry("ring-plain", ["burning", "swift"])];
        byte[] line = RunRecord.WriteHeader(RunRecord.NewHeader(hash, 7UL, loadout));
        string text = Encoding.UTF8.GetString(line);
        Assert.Contains("\"loadout\":[{\"item\":\"blast-chest\",\"affixes\":[]},{\"item\":\"ring-plain\",\"affixes\":[\"burning\",\"swift\"]}]", text, StringComparison.Ordinal);

        (RunRecordHeader read, int bodyStart) = RunRecord.ReadHeader(line);
        Assert.Equal(line.Length, bodyStart);
        Assert.Equal(7UL, read.Seed);
        Assert.Equal(2, read.Loadout.Count);
        Assert.Equal("blast-chest", read.Loadout[0].Item);
        Assert.Empty(read.Loadout[0].Affixes);
        Assert.Equal("ring-plain", read.Loadout[1].Item);
        Assert.Equal(new[] { "burning", "swift" }, read.Loadout[1].Affixes);

        Assert.Throws<ContextException>(() => RunRecord.WriteHeader(RunRecord.NewHeader(hash, 7UL, [new LoadoutEntry("Blast\"", [])])));
        Assert.Throws<ContextException>(() => RunRecord.WriteHeader(RunRecord.NewHeader(hash, 7UL, [new LoadoutEntry("ring-plain", [""])])));
    }

    /// <summary>
    /// Each drawn face of an overlay has a place in the atlas, and an undrawn face has none (D-767). The head pieces of
    /// Miner's leathers and of Company brigandine leave the front open, and the closed helm of Blast plate draws it.
    /// </summary>
    [Fact]
    public void OverlayFacesHaveTheirPlaces()
    {
        string contentRoot = Path.Combine(RepositoryRoot.Find(), "content");
        foreach (ArmorDefinition armor in Content.Armors)
        {
            BlockbenchModel model = BlockbenchLoader.Parse(armor.Model, File.ReadAllBytes(Path.Combine(contentRoot, armor.Model)));
            foreach (ModelBox box in model.Boxes)
            {
                for (int side = 0; side < BoxFaces.Names.Count; side++)
                {
                    bool placed = RepositoryTextures.Layout.Faces.Any(place => place.Model == armor.Model && place.Box == box.Name && place.Side == (BoxSide)side);
                    Assert.True(placed == box.IsDrawn((BoxSide)side), $"The face {armor.Model}:{box.Name}:{BoxFaces.Name((BoxSide)side)} is drawn {box.IsDrawn((BoxSide)side)} and placed {placed}.");
                }
            }

            bool openFront = armor.Slot == ArmorDefinition.HeadSlot && armor.Id != "blast-head";
            Assert.Equal(openFront, !model.Boxes[0].IsDrawn(BoxSide.North));
        }
    }

    /// <summary>The repository content with one file replaced.</summary>
    private sealed class ReplacedFileSource(string path, string json) : IContentSource
    {
        public IReadOnlyList<ContentFile> Read()
        {
            List<ContentFile> files = [];
            foreach (ContentFile file in new RepositoryContentSource().Read())
            {
                files.Add(file.Path == path ? new ContentFile(path, Encoding.UTF8.GetBytes(json)) : file);
            }

            return files;
        }
    }

    /// <summary>A sink that keeps the record in memory.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte> Bytes { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Bytes.AddRange(bytes);
        }
    }

    /// <summary>A log sink that keeps the lines.</summary>
    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }
}
