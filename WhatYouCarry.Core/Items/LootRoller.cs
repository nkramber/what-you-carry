using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Items;

/// <summary>One rolled item: the definition, the rolled tier, the rarity, and the affixes in draw order (D-47, D-748).</summary>
/// <param name="Item">The item definition.</param>
/// <param name="Tier">The rolled tier. A ring keeps it, and it changes nothing on a ring (D-749).</param>
/// <param name="Rarity">The rarity, which sets the count of affixes.</param>
/// <param name="Affixes">The affixes, no one twice (D-750).</param>
public sealed record RolledItem(ItemDefinition Item, long Tier, Rarity Rarity, IReadOnlyList<AffixDefinition> Affixes);

/// <summary>
/// Rolls one item from the loot stream (D-48, D-159, D-745 to D-750): the tier from the depth band of the floor, an
/// item of that tier from a pool, a rarity, and the affixes of the rarity.
/// </summary>
/// <remarks>
/// <para>
/// One roll draws in a fixed order: the tier jump, then the band tier when no jump comes, then the item, then the
/// rarity, then one draw for each affix. Each draw is a whole number under 100 or under a count, so the roll gives
/// one item on every platform (G-9).
/// </para>
/// <para>
/// The 15 floors form three bands of five, the three floor templates of D-210. A band gives its low tier at 70
/// percent and the next tier at 30 percent (D-745). The tier jump comes first: 2 rolls in 100 give one tier over the
/// top tier of the band, and never more than tier 3 (D-746).
/// </para>
/// <para>
/// The caller gives the pool, for example the swords of a weapon slot. An item of the pool fits the rolled tier when
/// its weapon has that tier, and a ring fits every tier (D-749). PR-26 decides the pool of each enemy.
/// </para>
/// </remarks>
public static class LootRoller
{
    /// <summary>The floors of one depth band (D-210, D-745).</summary>
    public const int FloorsPerBand = 5;

    /// <summary>The last floor of a run (D-56).</summary>
    public const int LastFloor = 15;

    /// <summary>The highest tier (D-745).</summary>
    public const long TopTier = 3;

    /// <summary>The rolls in 100 that the tier jump takes (D-746).</summary>
    public const int TierJumpPercent = 2;

    /// <summary>The rolls in 100 without a jump that give the low tier of the band (D-745).</summary>
    public const int LowTierPercent = 70;

    /// <summary>The low tier of the band of a floor: tier 0 on floors 1 to 5, tier 1 on 6 to 10, and tier 2 on 11 to 15 (D-745).</summary>
    /// <exception cref="ContextException">The floor is outside 1 to <see cref="LastFloor"/>.</exception>
    public static long LowTier(int floor)
    {
        if (floor < 1 || floor > LastFloor)
        {
            ContextException error = new($"A loot roll reads a floor from 1 to {LastFloor}, and the floor is {floor}.");
            error.AddContext("floor", ((long)floor).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return (floor - 1) / FloorsPerBand;
    }

    /// <summary>The tier of a tier jump on a floor: one over the top tier of the band, and never over <see cref="TopTier"/> (D-746).</summary>
    /// <exception cref="ContextException">The floor is outside 1 to <see cref="LastFloor"/>.</exception>
    public static long JumpTier(int floor)
    {
        long jump = LowTier(floor) + 2;
        return jump > TopTier ? TopTier : jump;
    }

    /// <summary>Rolls a tier for a floor: the tier jump first, and the band shares otherwise (D-745, D-746).</summary>
    /// <exception cref="ContextException">The floor is outside 1 to <see cref="LastFloor"/>.</exception>
    public static long RollTier(Rng rng, int floor)
    {
        long low = LowTier(floor);
        if (rng.NextInt(100) < TierJumpPercent)
        {
            return JumpTier(floor);
        }

        return rng.NextInt(100) < LowTierPercent ? low : low + 1;
    }

    /// <summary>Rolls a rarity by the shares of D-748, which stay the same at every depth.</summary>
    /// <exception cref="ContextException">The shares of <see cref="Rarities"/> add up to less than 100.</exception>
    public static Rarity RollRarity(Rng rng)
    {
        int draw = rng.NextInt(100);
        int bound = 0;
        foreach (Rarity rarity in Rarities.All)
        {
            bound += Rarities.SharePercent(rarity);
            if (draw < bound)
            {
                return rarity;
            }
        }

        ContextException error = new($"The rarity shares add up to {bound} percent, and they add up to 100 (D-748).");
        error.AddContext("shares", ((long)bound).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>Rolls one item for a floor from a pool (D-745 to D-750).</summary>
    /// <param name="rng">The loot stream (D-159).</param>
    /// <param name="floor">The floor, from 1 to <see cref="LastFloor"/>.</param>
    /// <param name="pool">The items that the roll can give.</param>
    /// <param name="weapons">The weapon files, which hold the tier of each weapon item (D-749).</param>
    /// <param name="armors">The armor files, which hold the tier of each armor item (D-763).</param>
    /// <param name="affixes">The affixes that the roll can give, each with the same weight (D-750).</param>
    /// <exception cref="ContextException">The floor is outside its bounds, no item of the pool fits the rolled tier, an item names an absent weapon or armor, or the rarity needs more affixes than exist.</exception>
    public static RolledItem Roll(Rng rng, int floor, IReadOnlyList<ItemDefinition> pool, IReadOnlyList<WeaponDefinition> weapons, IReadOnlyList<ArmorDefinition> armors, IReadOnlyList<AffixDefinition> affixes)
    {
        long tier = RollTier(rng, floor);
        List<ItemDefinition> fits = [];
        foreach (ItemDefinition item in pool)
        {
            if (item.Slot == ItemDefinition.RingSlot || TierOf(item, weapons, armors) == tier)
            {
                fits.Add(item);
            }
        }

        if (fits.Count == 0)
        {
            ContextException error = new($"No item of the pool of {pool.Count} fits tier {tier} on floor {floor} (D-745, D-749).");
            error.AddContext("floor", ((long)floor).ToString(CultureInfo.InvariantCulture));
            error.AddContext("tier", tier.ToString(CultureInfo.InvariantCulture));
            error.AddContext("pool", ((long)pool.Count).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        ItemDefinition chosen = fits[rng.NextInt(fits.Count)];
        Rarity rarity = RollRarity(rng);
        int count = Rarities.AffixCount(rarity);
        if (count > affixes.Count)
        {
            ContextException error = new($"The rarity {(int)rarity} needs {count} affixes, and the content holds {affixes.Count} (D-748).");
            error.AddContext("rarity", ((long)rarity).ToString(CultureInfo.InvariantCulture));
            error.AddContext("affixes", ((long)affixes.Count).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        // A draw without return, so one item never carries an affix twice (D-750).
        List<AffixDefinition> left = [.. affixes];
        List<AffixDefinition> drawn = [];
        for (int index = 0; index < count; index++)
        {
            int pick = rng.NextInt(left.Count);
            drawn.Add(left[pick]);
            List<AffixDefinition> rest = [];
            for (int other = 0; other < left.Count; other++)
            {
                if (other != pick)
                {
                    rest.Add(left[other]);
                }
            }

            left = rest;
        }

        return new RolledItem(chosen, tier, rarity, drawn);
    }

    /// <summary>The tier of a weapon item or an armor item: the tier of the file that it names (D-749, D-763).</summary>
    /// <exception cref="ContextException">The item is a ring, or no weapon or armor carries the id that it names.</exception>
    private static long TierOf(ItemDefinition item, IReadOnlyList<WeaponDefinition> weapons, IReadOnlyList<ArmorDefinition> armors)
    {
        foreach (WeaponDefinition weapon in weapons)
        {
            if (item.Weapon is not null && weapon.Id == item.Weapon)
            {
                return weapon.Tier;
            }
        }

        foreach (ArmorDefinition armor in armors)
        {
            if (item.Armor is not null && armor.Id == item.Armor)
            {
                return armor.Tier;
            }
        }

        ContextException error = new($"The item '{item.Id}' of the slot '{item.Slot}' names the weapon '{item.Weapon ?? "none"}' and the armor '{item.Armor ?? "none"}', and no file carries that id (D-749, D-763).");
        error.AddContext("item", item.Id);
        error.AddContext("weapon", item.Weapon ?? "none");
        error.AddContext("armor", item.Armor ?? "none");
        throw error;
    }
}
