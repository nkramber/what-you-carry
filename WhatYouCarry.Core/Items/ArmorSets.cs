using System.Collections.Generic;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Core.Items;

/// <summary>
/// The loadout of one full armor set (D-753, D-762). The items of a set carry the id of the set and the slot, as
/// `blast-chest`, so a set needs no list of its own. A set that the content does not hold fails where the loadout
/// resolves, and the error names the absent item.
/// </summary>
public static class ArmorSets
{
    /// <summary>The four slots of a full set, in the order of D-18.</summary>
    public static readonly IReadOnlyList<string> SetSlots =
    [
        ArmorDefinition.HeadSlot,
        ArmorDefinition.ChestSlot,
        ArmorDefinition.LegsSlot,
        ArmorDefinition.FeetSlot,
    ];

    /// <summary>The loadout entries of the four pieces of one set, with no affix (D-762, D-766).</summary>
    public static IReadOnlyList<LoadoutEntry> FullSet(string set)
    {
        List<LoadoutEntry> loadout = [];
        foreach (string slot in SetSlots)
        {
            loadout.Add(new LoadoutEntry(set + "-" + slot, []));
        }

        return loadout;
    }
}
