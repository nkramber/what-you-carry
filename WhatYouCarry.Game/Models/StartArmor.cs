using System.Collections.Generic;
using WhatYouCarry.Core.Items;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// The armor of the run start (D-762): the flag `--armor` with the id of one set starts the run in the four pieces of
/// that set, and the loadout of the run holds them (D-766). With no flag, the run starts with no armor. A set that the
/// content does not hold stops the boot where the loop reads the loadout, and the error names the absent item.
/// </summary>
public static class StartArmor
{
    /// <summary>The flag that names the set of the run start, such as `--armor blast`.</summary>
    public const string Flag = "--armor";

    /// <summary>The loadout of the run start: the four pieces of the set of the flag, or no item with no flag.</summary>
    public static IReadOnlyList<LoadoutEntry> LoadoutOf(UserArguments arguments)
    {
        if (!arguments.Has(Flag))
        {
            return [];
        }

        return ArmorSets.FullSet(arguments.WordsOf(Flag)[0]);
    }
}
