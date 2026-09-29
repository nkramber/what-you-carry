using System;
using System.Collections.Generic;
using System.Text;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Items;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The items, the affixes, the rarities, and the loot roll of PR-21 (D-745 to D-752; PR-21 exit tests 1 to 6). The
/// affix tests step a player and an enemy on a flat floor, and the roll tests draw from the loot stream.
/// </summary>
public sealed class ItemTests
{
    /// <summary>The rolls of each floor in the band tests (PR-21 exit test 3).</summary>
    private const int RollsPerFloor = 100000;

    private static readonly EntityBox[] NoTargets = [];

    /// <summary>The feet of the body that swings, in the middle of the flat floor of 16 blocks.</summary>
    private static readonly Vector3 Feet = new(8.5f, 1.0f, 8.5f);

    private static ContentSet Content => TestWorld.Content;

    private static WeaponDefinition Sword => SimulationLoop.MainWeapon(Content);

    private static EnemyDefinition Scavenger => Content.Enemies[0];

    /// <summary>A box of the size of the player body, standing at a point.</summary>
    private static Aabb BodyBoxAt(Vector3 feet, float halfWidth = PlayerBody.HalfWidth)
    {
        return new Aabb(feet - new Vector3(halfWidth, 0.0f, halfWidth), feet + new Vector3(halfWidth, PlayerBody.Height, halfWidth));
    }

    /// <summary>A box that covers the whole arc in front, so the blade meets it on the first active tick.</summary>
    private static EntityBox[] WideTargetAhead(int owner)
    {
        return [new EntityBox(owner, BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, -1.0f), 1.5f))];
    }

    private static AffixDefinition Affix(string id)
    {
        foreach (AffixDefinition affix in Content.Affixes)
        {
            if (affix.Id == id)
            {
                return affix;
            }
        }

        throw new InvalidOperationException($"The repository content holds no affix '{id}'.");
    }

    private static Player NewPlayer(WeaponDefinition weapon)
    {
        return new Player(TestWorld.FlatFloor(16, 6), Feet, weapon, Player.MaxHealth);
    }

    private static Enemy NewEnemy(VoxelGrid grid, Vector3 feet, int owner, WeaponDefinition? weapon = null)
    {
        return new Enemy(grid, feet, Scavenger, weapon ?? Sword, owner);
    }

    /// <summary>The tick of the first hit of a player swing that a press starts on tick zero.</summary>
    private static long PlayerHitTick(WeaponDefinition weapon)
    {
        Player player = NewPlayer(weapon);
        EntityBox[] targets = WideTargetAhead(7);
        ushort previous = 0;
        for (int tick = 0; tick < weapon.SwingTicks; tick++)
        {
            ushort buttons = tick == 0 ? Button.Attack : (ushort)0;
            player.Step(new Intent(0U, 0, 0, 0, 0, buttons), previous, 0, targets);
            previous = buttons;
            if (player.LastHits.Count > 0)
            {
                return tick;
            }
        }

        throw new InvalidOperationException($"The swing of '{weapon.Id}' hit nothing.");
    }

    /// <summary>The step of the first hit of an enemy swing that starts before step zero.</summary>
    private static long EnemyHitTick(WeaponDefinition weapon)
    {
        Enemy enemy = NewEnemy(TestWorld.FlatFloor(16, 6), Feet, 1, weapon);
        EntityBox[] targets = WideTargetAhead(0);
        enemy.StartSwing();
        for (int tick = 0; tick < weapon.SwingTicks; tick++)
        {
            enemy.Step(new Vector3(0.0f, 0.0f, 0.0f), false, 0, targets);
            if (enemy.LastHits.Count > 0)
            {
                return tick;
            }
        }

        throw new InvalidOperationException($"The swing of '{weapon.Id}' hit nothing.");
    }

    /// <summary>
    /// PR-21 exit test 1. Each affix acts on the player as its wielder (D-747, D-750, D-751). Lifesteal heals 15 percent
    /// of the hit, rounded down, up to full health. Burning deals 8 to each foe within 1.5 meters of the struck target,
    /// with no stagger. Swift starts the blade 3 ticks sooner, 6 with two, and never sooner than half the windup.
    /// </summary>
    [Fact]
    public void EveryAffixWorksForPlayer()
    {
        AffixDefinition lifesteal = Affix(AffixDefinition.Lifesteal);
        AffixDefinition burning = Affix(AffixDefinition.Burning);
        AffixDefinition swift = Affix(AffixDefinition.Swift);
        VoxelGrid grid = TestWorld.FlatFloor(16, 6);

        // Lifesteal: 34 x 15 / 100 is 5.1, so the heal is 5. It reads the full hit, also on a nearly dead target.
        Player wielder = new(grid, Feet, Sword, 50);
        Enemy struck = NewEnemy(grid, Feet + new Vector3(0.0f, 0.0f, -1.0f), 1);
        AffixBehaviors.OnHit(wielder, [lifesteal], Sword.Damage, struck.Feet, [struck]);
        Assert.Equal(55, wielder.Health);
        Assert.Equal((int)Scavenger.Health, struck.Health);

        Player nearlyFull = new(grid, Feet, Sword, Player.MaxHealth - 2);
        AffixBehaviors.OnHit(nearlyFull, [lifesteal, lifesteal], Sword.Damage, struck.Feet, [struck]);
        Assert.Equal(Player.MaxHealth, nearlyFull.Health);

        // Burning: the struck target and a foe at 1.2 meters take 8, a foe at 2 meters takes none, and no one staggers.
        Enemy target = NewEnemy(grid, new Vector3(4.5f, 1.0f, 4.5f), 1);
        Enemy near = NewEnemy(grid, new Vector3(5.7f, 1.0f, 4.5f), 2);
        Enemy far = NewEnemy(grid, new Vector3(4.5f, 1.0f, 6.5f), 3);
        Player burner = NewPlayer(Sword);
        AffixBehaviors.OnHit(burner, [burning], Sword.Damage, target.Feet, [target, near, far]);
        Assert.Equal((int)Scavenger.Health - 8, target.Health);
        Assert.Equal((int)Scavenger.Health - 8, near.Health);
        Assert.Equal((int)Scavenger.Health, far.Health);
        Assert.Equal(0, target.StaggerRemaining);
        Assert.Equal(0, near.StaggerRemaining);
        Assert.Equal(Player.MaxHealth, burner.Health);

        // Two burning affixes add, and a dead foe takes no burn.
        Enemy dead = NewEnemy(grid, new Vector3(4.5f, 1.0f, 5.0f), 4);
        dead.TakeHit(Scavenger.Health);
        AffixBehaviors.OnHit(burner, [burning, burning], Sword.Damage, target.Feet, [target, dead]);
        Assert.Equal((int)Scavenger.Health - 24, target.Health);
        Assert.True(dead.IsDead);

        // Swift: 12 - 12 x 25 / 100 is 9, two give 6, and three stop at the cap of 50 percent.
        Assert.Equal(Sword.WindupTicks, PlayerHitTick(Sword));
        Assert.Equal(9, PlayerHitTick(AffixBehaviors.Swift(Sword, [swift])));
        Assert.Equal(6, PlayerHitTick(AffixBehaviors.Swift(Sword, [swift, swift])));
        Assert.Equal(6, PlayerHitTick(AffixBehaviors.Swift(Sword, [swift, swift, swift])));
        Assert.Equal(Sword.WindupTicks, AffixBehaviors.Swift(Sword, [lifesteal, burning]).WindupTicks);
    }

    /// <summary>
    /// PR-21 exit test 2. Each affix acts on an enemy as its wielder, with the same effect as on the player (D-49,
    /// D-747). Lifesteal stops at the health of the family, burning reaches the player and no roll, and swift starts
    /// the blade of the enemy 3 ticks sooner.
    /// </summary>
    [Fact]
    public void EveryAffixWorksForEnemy()
    {
        AffixDefinition lifesteal = Affix(AffixDefinition.Lifesteal);
        AffixDefinition burning = Affix(AffixDefinition.Burning);
        AffixDefinition swift = Affix(AffixDefinition.Swift);
        VoxelGrid grid = TestWorld.FlatFloor(16, 6);

        // Lifesteal: an enemy at 6 of 40 heals 5 from a hit of 34, and one at 38 stops at 40.
        Enemy wielder = NewEnemy(grid, Feet, 1);
        wielder.TakeHit(Scavenger.Health - 6);
        Player struck = NewPlayer(Sword);
        AffixBehaviors.OnHit(wielder, [lifesteal], Sword.Damage, struck.Feet, [struck]);
        Assert.Equal(11, wielder.Health);
        Assert.Equal(Player.MaxHealth, struck.Health);

        Enemy nearlyFull = NewEnemy(grid, Feet, 2);
        nearlyFull.TakeHit(2);
        AffixBehaviors.OnHit(nearlyFull, [lifesteal], Sword.Damage, struck.Feet, [struck]);
        Assert.Equal((int)Scavenger.Health, nearlyFull.Health);

        // Burning: the struck player takes 8 with no stagger. A player in a roll takes none (D-328).
        Enemy burner = NewEnemy(grid, new Vector3(4.5f, 1.0f, 4.5f), 3);
        Player target = NewPlayer(Sword);
        AffixBehaviors.OnHit(burner, [burning], Sword.Damage, target.Feet, [target]);
        Assert.Equal(Player.MaxHealth - 8, target.Health);
        Assert.Equal(0, target.StaggerRemaining);

        Player rolling = NewPlayer(Sword);
        rolling.Step(new Intent(0U, 0, 0, 0, 0, Button.Dodge), 0, 0, NoTargets);
        Assert.True(rolling.RollRemaining > 0);
        AffixBehaviors.OnHit(burner, [burning], Sword.Damage, rolling.Feet, [rolling]);
        Assert.Equal(Player.MaxHealth, rolling.Health);

        // Swift: the blade of the enemy starts at step 9 in place of step 12.
        Assert.Equal(Sword.WindupTicks, EnemyHitTick(Sword));
        Assert.Equal(9, EnemyHitTick(AffixBehaviors.Swift(Sword, [swift])));
        Assert.Equal(6, EnemyHitTick(AffixBehaviors.Swift(Sword, [swift, swift])));
    }

    /// <summary>
    /// PR-21 exit test 3. One hundred thousand rolls on each floor give each tier within 1 point of its share: the band
    /// share of D-745 on the 98 rolls in 100 with no tier jump, plus the 2 of the jump of D-746. The test is stricter
    /// than the 2 percent of the roadmap. No roll gives a tier outside the band and its jump.
    /// </summary>
    [Fact]
    public void BandDistributionHolds()
    {
        for (int floor = 1; floor <= LootRoller.LastFloor; floor++)
        {
            ulong seed = 2100UL + (ulong)floor;
            Rng rng = Rng.ForStream(seed, RngStream.Loot, floor);
            long[] counts = new long[LootRoller.TopTier + 1];
            for (int roll = 0; roll < RollsPerFloor; roll++)
            {
                RolledItem item = LootRoller.Roll(rng, floor, Content.Items, Content.Weapons, Content.Affixes);
                counts[item.Tier]++;
            }

            long low = ((long)floor - 1) / 5;
            double[] expected = new double[LootRoller.TopTier + 1];
            expected[low] += 0.98 * 70.0;
            expected[low + 1] += 0.98 * 30.0;
            expected[Math.Min(low + 2, LootRoller.TopTier)] += 2.0;
            for (int tier = 0; tier <= LootRoller.TopTier; tier++)
            {
                double share = counts[tier] * 100.0 / RollsPerFloor;
                Assert.True(Math.Abs(share - expected[tier]) <= 1.0, $"Seed {seed}, floor {floor}: tier {tier} took {share:F2} percent, and D-745 and D-746 give {expected[tier]:F2}.");
            }
        }
    }

    /// <summary>PR-21 exit test 4. On floor 1 the tier jump gives tier 2 in 2 rolls of 100, within 0.5 percent (D-746).</summary>
    [Fact]
    public void RareHigherTierChance()
    {
        const ulong Seed = 2104UL;
        Rng rng = Rng.ForStream(Seed, RngStream.Loot, 1);
        long jumps = 0;
        for (int roll = 0; roll < RollsPerFloor; roll++)
        {
            if (LootRoller.RollTier(rng, 1) == 2)
            {
                jumps++;
            }
        }

        double share = jumps * 100.0 / RollsPerFloor;
        Assert.True(Math.Abs(share - LootRoller.TierJumpPercent) <= 0.5, $"Seed {Seed}: the tier jump took {share:F3} percent of the rolls on floor 1.");
        Assert.Equal(2, LootRoller.JumpTier(1));
        Assert.Equal(3, LootRoller.JumpTier(6));
        Assert.Equal(3, LootRoller.JumpTier(15));
    }

    /// <summary>PR-21 exit test 5. One seed gives the same items twice: the same definition, tier, rarity, and affixes.</summary>
    [Fact]
    public void RollIsDeterministic()
    {
        for (ulong seed = 1; seed <= 20; seed++)
        {
            Rng first = Rng.ForStream(seed, RngStream.Loot, 3);
            Rng second = Rng.ForStream(seed, RngStream.Loot, 3);
            for (int roll = 0; roll < 200; roll++)
            {
                RolledItem one = LootRoller.Roll(first, 3, Content.Items, Content.Weapons, Content.Affixes);
                RolledItem two = LootRoller.Roll(second, 3, Content.Items, Content.Weapons, Content.Affixes);
                Assert.True(one.Item == two.Item && one.Tier == two.Tier && one.Rarity == two.Rarity, $"Seed {seed}, roll {roll}: the two rolls differ.");
                Assert.Equal(AffixIds(one), AffixIds(two));
            }
        }
    }

    /// <summary>
    /// PR-21 exit test 6. An affix file with a behavior that Core does not have fails, and the error names the
    /// behavior. An item file with an absent weapon fails at the load, and the error names the weapon (D-752).
    /// </summary>
    [Fact]
    public void AffixValidatorRejectsUnknownBehavior()
    {
        ContextException behavior = Assert.Throws<ContextException>(() => ReadAffix("""{ "id": "frost", "behavior": "frost", "percent": 10 }"""));
        Assert.Contains("'frost'", behavior.Message, StringComparison.Ordinal);
        Assert.Contains("behavior", behavior.Message, StringComparison.Ordinal);

        ContextException weapon = Assert.Throws<ContextException>(() => new ContentLoader(new ExtraFileSource("items/axe.json", """{ "id": "axe", "slot": "weapon", "weapon": "axe-tier-1" }""")).Load());
        Assert.Contains("items/axe.json", weapon.Message, StringComparison.Ordinal);
        Assert.Contains("'axe-tier-1'", weapon.Message, StringComparison.Ordinal);
    }

    /// <summary>The validators reject a missing or extra parameter, a percent over 100, a ring with a weapon, a weapon item with none, and an unknown slot.</summary>
    [Fact]
    public void ValidatorsRejectBadShapes()
    {
        Assert.Contains("'damage'", Assert.Throws<ContextException>(() => ReadAffix("""{ "id": "b", "behavior": "burning", "radiusCentimetres": 150 }""")).Message, StringComparison.Ordinal);
        Assert.Contains("'damage'", Assert.Throws<ContextException>(() => ReadAffix("""{ "id": "s", "behavior": "swift", "percent": 25, "damage": 3 }""")).Message, StringComparison.Ordinal);
        Assert.Contains("'percent'", Assert.Throws<ContextException>(() => ReadAffix("""{ "id": "l", "behavior": "lifesteal", "percent": 101 }""")).Message, StringComparison.Ordinal);
        Assert.Contains("'radiusCentimetres'", Assert.Throws<ContextException>(() => ReadAffix("""{ "id": "b", "behavior": "burning", "damage": 8, "radiusCentimetres": 0 }""")).Message, StringComparison.Ordinal);

        Assert.Contains("'weapon'", Assert.Throws<ContextException>(() => ReadItem("""{ "id": "r", "slot": "ring", "weapon": "sword-basic" }""")).Message, StringComparison.Ordinal);
        Assert.Contains("'weapon'", Assert.Throws<ContextException>(() => ReadItem("""{ "id": "s", "slot": "weapon" }""")).Message, StringComparison.Ordinal);
        Assert.Contains("'head'", Assert.Throws<ContextException>(() => ReadItem("""{ "id": "h", "slot": "head" }""")).Message, StringComparison.Ordinal);
    }

    /// <summary>The repository holds the five items and the three affixes of D-747 and D-749, and the sword of each tier has its damage.</summary>
    [Fact]
    public void RepositoryHoldsTheFirstItemsAndAffixes()
    {
        Assert.Equal(5, Content.Items.Count);
        Assert.Equal(new[] { "burning", "lifesteal", "swift" }, AffixIds(Content.Affixes));
        Assert.Equal(15, Affix(AffixDefinition.Lifesteal).Parameter(AffixDefinition.PercentName));
        Assert.Equal(8, Affix(AffixDefinition.Burning).Parameter(AffixDefinition.DamageName));
        Assert.Equal(150, Affix(AffixDefinition.Burning).Parameter(AffixDefinition.RadiusName));
        Assert.Equal(25, Affix(AffixDefinition.Swift).Parameter(AffixDefinition.PercentName));

        long[] damage = [34, 39, 45, 52];
        foreach (ItemDefinition item in Content.Items)
        {
            if (item.Slot == ItemDefinition.RingSlot)
            {
                Assert.Equal("ring-plain", item.Id);
                Assert.Null(item.Weapon);
                continue;
            }

            WeaponDefinition weapon = WeaponNamed(item.Weapon);
            Assert.Equal(damage[weapon.Tier], weapon.Damage);
            Assert.Equal(Sword with { Id = weapon.Id, Tier = weapon.Tier, Damage = weapon.Damage }, weapon);
        }
    }

    /// <summary>
    /// The rarities of D-748: the affix counts, the shares over many rolls, the outlines, and no affix twice on one
    /// item. A ring fits every tier, and a pool with no item of the rolled tier fails with the tier.
    /// </summary>
    [Fact]
    public void RaritiesHoldTheirShares()
    {
        Assert.Equal(100, Rarities.SharePercent(Rarity.Common) + Rarities.SharePercent(Rarity.Rare) + Rarities.SharePercent(Rarity.Epic));
        Assert.False(Rarities.HasOutline(Rarity.Common));
        Assert.Equal("#5174c4", Rarities.Outline(Rarity.Rare));
        Assert.Equal("#8b64a8", Rarities.Outline(Rarity.Epic));
        Assert.Throws<ContextException>(() => Rarities.Outline(Rarity.Common));

        const ulong Seed = 2105UL;
        Rng rng = Rng.ForStream(Seed, RngStream.Loot, 12);
        long[] counts = new long[3];
        List<ItemDefinition> rings = [];
        foreach (ItemDefinition item in Content.Items)
        {
            if (item.Slot == ItemDefinition.RingSlot)
            {
                rings.Add(item);
            }
        }

        for (int roll = 0; roll < RollsPerFloor; roll++)
        {
            RolledItem item = LootRoller.Roll(rng, 12, rings, Content.Weapons, Content.Affixes);
            counts[(int)item.Rarity]++;
            Assert.Equal(Rarities.AffixCount(item.Rarity), item.Affixes.Count);
            Assert.Equal(item.Affixes.Count, new HashSet<string>(AffixIds(item.Affixes)).Count);
        }

        foreach (Rarity rarity in Rarities.All)
        {
            double share = counts[(int)rarity] * 100.0 / RollsPerFloor;
            Assert.True(Math.Abs(share - Rarities.SharePercent(rarity)) <= 1.0, $"Seed {Seed}: the rarity {rarity} took {share:F2} percent.");
        }

        List<ItemDefinition> baseSword = [];
        foreach (ItemDefinition item in Content.Items)
        {
            if (item.Id == "sword-basic")
            {
                baseSword.Add(item);
            }
        }

        Assert.Single(baseSword);
        ContextException missing = Assert.Throws<ContextException>(() =>
        {
            Rng draws = Rng.ForStream(Seed, RngStream.Loot, 15);
            for (int roll = 0; roll < 100; roll++)
            {
                LootRoller.Roll(draws, 15, baseSword, Content.Weapons, Content.Affixes);
            }
        });
        Assert.Contains("tier", missing.Message, StringComparison.Ordinal);
    }

    /// <summary>A heal or plain damage below zero, and either one on a dead body, fails (T-2).</summary>
    [Fact]
    public void HealAndPlainDamageRejectBadInput()
    {
        Player player = NewPlayer(Sword);
        Assert.Throws<ContextException>(() => player.Heal(-1));
        Assert.Throws<ContextException>(() => player.TakePlainDamage(-1));
        Assert.True(player.TakePlainDamage(Player.MaxHealth));
        Assert.True(player.IsDead);
        Assert.Throws<ContextException>(() => player.Heal(1));
        Assert.Throws<ContextException>(() => player.TakePlainDamage(1));

        Enemy enemy = NewEnemy(TestWorld.FlatFloor(16, 6), Feet, 1);
        Assert.Throws<ContextException>(() => enemy.Heal(-1));
        Assert.True(enemy.TakePlainDamage(Scavenger.Health + 5));
        Assert.Equal(0, enemy.Health);
        Assert.Throws<ContextException>(() => enemy.Heal(1));
        Assert.Throws<ContextException>(() => AffixBehaviors.OnHit(player, [], -1, Feet, []));
    }

    private static List<string> AffixIds(RolledItem item)
    {
        return AffixIds(item.Affixes);
    }

    private static List<string> AffixIds(IReadOnlyList<AffixDefinition> affixes)
    {
        List<string> ids = [];
        foreach (AffixDefinition affix in affixes)
        {
            ids.Add(affix.Id);
        }

        return ids;
    }

    private static WeaponDefinition WeaponNamed(string? id)
    {
        foreach (WeaponDefinition weapon in Content.Weapons)
        {
            if (weapon.Id == id)
            {
                return weapon;
            }
        }

        throw new InvalidOperationException($"The repository content holds no weapon '{id}'.");
    }

    private static AffixDefinition ReadAffix(string json)
    {
        const string Path = "affixes/test.json";
        return AffixDefinition.FromMembers(Path, JsonObjectReader.Read(Path, Encoding.UTF8.GetBytes(json)));
    }

    private static ItemDefinition ReadItem(string json)
    {
        const string Path = "items/test.json";
        return ItemDefinition.FromMembers(Path, JsonObjectReader.Read(Path, Encoding.UTF8.GetBytes(json)));
    }

    /// <summary>The repository content with one more file.</summary>
    private sealed class ExtraFileSource(string path, string json) : IContentSource
    {
        public IReadOnlyList<ContentFile> Read()
        {
            List<ContentFile> files = [.. new RepositoryContentSource().Read()];
            files.Add(new ContentFile(path, Encoding.UTF8.GetBytes(json)));
            return files;
        }
    }
}
