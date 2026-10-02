using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Items;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Entities;

/// <summary>One worn item: the item, its armor file for an armor piece or null, and its affixes (D-763, D-766).</summary>
/// <param name="Item">The item.</param>
/// <param name="Armor">The armor file that the item names, or null for an item with no armor file.</param>
/// <param name="Affixes">The affixes of the item, in roll order.</param>
public sealed record WornItem(ItemDefinition Item, ArmorDefinition? Armor, IReadOnlyList<AffixDefinition> Affixes);

/// <summary>
/// What the player wears and holds (D-18, D-20, D-55): the main weapon, the six modeled slots, and the two ring slots.
/// The sums of the worn armor give the reduction and the weight (D-754), and the affixes of every worn item act in the
/// loop (D-747, D-750).
/// </summary>
/// <remarks>
/// <para>
/// The modeled slots are head, chest, legs, feet, amulet, and shield, and each holds one item. Two rings fit, and a
/// ring has no model (D-18, D-55). A shield needs a one-handed main weapon (D-26). The main weapon stays the weapon of
/// the loop until the loadout of PR-30, so no worn item is a weapon (D-422).
/// </para>
/// <para>
/// Nothing changes the equipment during a run before PR-23 and PR-30. A run takes it from the loadout of its record
/// header (D-762, D-765, D-766).
/// </para>
/// </remarks>
public sealed class Equipment
{
    /// <summary>The slot of the amulet, a modeled slot that PR-28 fills (D-18).</summary>
    public const string AmuletSlot = "amulet";

    /// <summary>The count of ring slots (D-18, D-55).</summary>
    public const int RingSlots = 2;

    /// <summary>The six modeled slots, in the order of D-18.</summary>
    public static readonly IReadOnlyList<string> ModeledSlots =
    [
        ArmorDefinition.HeadSlot,
        ArmorDefinition.ChestSlot,
        ArmorDefinition.LegsSlot,
        ArmorDefinition.FeetSlot,
        AmuletSlot,
        ArmorDefinition.ShieldSlot,
    ];

    private readonly List<WornItem> worn = [];

    /// <summary>Equipment with a main weapon and no worn item.</summary>
    public Equipment(WeaponDefinition weapon)
    {
        this.Weapon = weapon;
    }

    /// <summary>The main weapon (D-20).</summary>
    public WeaponDefinition Weapon { get; }

    /// <summary>The worn items, in equip order.</summary>
    public IReadOnlyList<WornItem> Worn => this.worn;

    /// <summary>The sum of the reduction of each worn armor piece (D-754).</summary>
    public long Reduction
    {
        get
        {
            long sum = 0;
            foreach (WornItem item in this.worn)
            {
                sum += item.Armor is null ? 0 : item.Armor.Reduction;
            }

            return sum;
        }
    }

    /// <summary>The sum of the weight of each worn armor piece (D-754).</summary>
    public long Weight
    {
        get
        {
            long sum = 0;
            foreach (WornItem item in this.worn)
            {
                sum += item.Armor is null ? 0 : item.Armor.Weight;
            }

            return sum;
        }
    }

    /// <summary>The affixes of every worn item, in equip order. One affix stands once for each item that carries it, so the amounts add (D-750).</summary>
    public IReadOnlyList<AffixDefinition> Affixes
    {
        get
        {
            List<AffixDefinition> carried = [];
            foreach (WornItem item in this.worn)
            {
                foreach (AffixDefinition affix in item.Affixes)
                {
                    carried.Add(affix);
                }
            }

            return carried;
        }
    }

    /// <summary>
    /// The equipment of a loadout (D-762, D-766): each entry resolves to its item, its armor file, and its affixes in
    /// the content set, and goes on in entry order.
    /// </summary>
    /// <exception cref="ContextException">An entry names an absent item, armor, or affix, or it breaks a rule of <see cref="Equip"/>. The error names the entry.</exception>
    public static Equipment FromLoadout(WeaponDefinition weapon, IReadOnlyList<LoadoutEntry> loadout, ContentSet content)
    {
        Equipment equipment = new(weapon);
        for (int index = 0; index < loadout.Count; index++)
        {
            LoadoutEntry entry = loadout[index];
            try
            {
                equipment.Equip(Resolve(entry, content));
            }
            catch (ContextException error)
            {
                error.AddContext("loadoutEntry", ((long)index).ToString(CultureInfo.InvariantCulture));
                error.AddContext("loadoutItem", entry.Item);
                throw;
            }
        }

        return equipment;
    }

    /// <summary>
    /// Puts one item on (D-18, D-26, D-55). The slot must be free, a third ring has no slot, a shield needs a
    /// one-handed main weapon, and an item carries an affix once (D-750).
    /// </summary>
    /// <exception cref="ContextException">The item breaks one of the rules, or its armor file does not match it.</exception>
    public void Equip(WornItem item)
    {
        string slot = item.Item.Slot;
        CheckArmorFile(item);
        CheckAffixes(item);

        if (slot == ItemDefinition.RingSlot)
        {
            if (this.CountOf(slot) >= RingSlots)
            {
                throw Refusal(item, $"both ring slots hold a ring, and a player wears {RingSlots} rings (D-18, D-55)");
            }

            this.worn.Add(item);
            return;
        }

        if (!IsModeledSlot(slot))
        {
            throw Refusal(item, $"the slot '{slot}' is no worn slot. The main weapon stays the weapon of the loop until the loadout of PR-30 (D-20, D-422)");
        }

        if (this.CountOf(slot) > 0)
        {
            throw Refusal(item, $"the slot '{slot}' holds an item, and each modeled slot holds one (D-18)");
        }

        if (slot == ArmorDefinition.ShieldSlot && this.Weapon.Handedness != WeaponDefinition.OneHanded)
        {
            throw Refusal(item, $"the main weapon '{this.Weapon.Id}' is two-handed, and a shield needs a one-handed melee weapon (D-26)");
        }

        this.worn.Add(item);
    }

    /// <summary>The damage of a hit after the reduction of the worn armor, never below zero (D-754, D-759).</summary>
    /// <exception cref="ContextException">The damage is below zero.</exception>
    public long AfterReduction(long damage)
    {
        if (damage < 0)
        {
            ContextException error = new($"A hit deals a damage of zero or more, and the damage is {damage}.");
            error.AddContext("damage", damage.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        long reduced = damage - this.Reduction;
        return reduced < 0 ? 0 : reduced;
    }

    /// <summary>The count of worn items of one slot.</summary>
    private int CountOf(string slot)
    {
        int count = 0;
        foreach (WornItem item in this.worn)
        {
            if (item.Item.Slot == slot)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Answers whether a slot is one of the six modeled slots.</summary>
    private static bool IsModeledSlot(string slot)
    {
        foreach (string modeled in ModeledSlots)
        {
            if (modeled == slot)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>An armor item carries the armor file that it names, of its own slot, and another item carries none (D-763).</summary>
    private static void CheckArmorFile(WornItem item)
    {
        if (item.Item.Armor is null)
        {
            if (item.Armor is not null)
            {
                throw Refusal(item, $"the item names no armor, and it carries the armor '{item.Armor.Id}' (D-763)");
            }

            return;
        }

        if (item.Armor is null)
        {
            throw Refusal(item, $"the item names the armor '{item.Item.Armor}', and it carries no armor file (D-763)");
        }

        if (item.Armor.Id != item.Item.Armor || item.Armor.Slot != item.Item.Slot)
        {
            throw Refusal(item, $"the item names the armor '{item.Item.Armor}' of the slot '{item.Item.Slot}', and it carries the armor '{item.Armor.Id}' of the slot '{item.Armor.Slot}' (D-763)");
        }
    }

    /// <summary>One item never carries an affix twice (D-750).</summary>
    private static void CheckAffixes(WornItem item)
    {
        for (int index = 0; index < item.Affixes.Count; index++)
        {
            for (int other = index + 1; other < item.Affixes.Count; other++)
            {
                if (item.Affixes[index].Id == item.Affixes[other].Id)
                {
                    throw Refusal(item, $"the item carries the affix '{item.Affixes[index].Id}' twice, and one item carries an affix once (D-750)");
                }
            }
        }
    }

    /// <summary>The worn item of one loadout entry, from the content set.</summary>
    /// <exception cref="ContextException">The entry names an item, an armor, or an affix that the content set does not hold.</exception>
    private static WornItem Resolve(LoadoutEntry entry, ContentSet content)
    {
        ItemDefinition? item = null;
        foreach (ItemDefinition candidate in content.Items)
        {
            if (candidate.Id == entry.Item)
            {
                item = candidate;
                break;
            }
        }

        if (item is null)
        {
            ContextException error = new($"The loadout names the item '{entry.Item}', and the content set holds no item of that id (D-766).");
            error.AddContext("item", entry.Item);
            throw error;
        }

        ArmorDefinition? armor = null;
        foreach (ArmorDefinition candidate in content.Armors)
        {
            if (item.Armor is not null && candidate.Id == item.Armor)
            {
                armor = candidate;
                break;
            }
        }

        List<AffixDefinition> affixes = [];
        foreach (string affixId in entry.Affixes)
        {
            affixes.Add(AffixOf(affixId, content));
        }

        return new WornItem(item, armor, affixes);
    }

    /// <summary>The affix of one id in the content set.</summary>
    /// <exception cref="ContextException">The content set holds no affix of the id.</exception>
    private static AffixDefinition AffixOf(string affixId, ContentSet content)
    {
        foreach (AffixDefinition affix in content.Affixes)
        {
            if (affix.Id == affixId)
            {
                return affix;
            }
        }

        ContextException error = new($"The loadout names the affix '{affixId}', and the content set holds no affix of that id (D-766).");
        error.AddContext("affix", affixId);
        throw error;
    }

    /// <summary>The error of an equip that a rule refuses. It names the item and the slot.</summary>
    private static ContextException Refusal(WornItem item, string reason)
    {
        ContextException error = new($"The item '{item.Item.Id}' cannot go on: {reason}.");
        error.AddContext("item", item.Item.Id);
        error.AddContext("slot", item.Item.Slot);
        return error;
    }
}
