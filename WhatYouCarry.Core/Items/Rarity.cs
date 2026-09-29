using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Items;

/// <summary>
/// The rarity of a rolled item (D-748). The tier sets the base stats, and the rarity sets the count of affixes. The
/// order of <see cref="Rarities.All"/> is part of the roll, so it must never change.
/// </summary>
public enum Rarity
{
    /// <summary>No affix and no outline.</summary>
    Common = 0,

    /// <summary>One affix and a blue outline.</summary>
    Rare = 1,

    /// <summary>Two affixes and a purple outline.</summary>
    Epic = 2,
}

/// <summary>
/// The numbers of each rarity (D-748): the count of affixes, the share of the loot rolls in percent, and the outline
/// color that an enemy shows (D-49). The shares stay the same at every depth.
/// </summary>
/// <remarks>
/// The outline colors are the brightest step of the cobalt ramp and of the violet ramp of the palette (D-592). Core
/// holds them as text, and the Game layer draws them.
/// </remarks>
public static class Rarities
{
    /// <summary>The outline of a rare item: cobalt 4 (D-748).</summary>
    public const string RareOutline = "#5174c4";

    /// <summary>The outline of an epic item: violet 4 (D-748).</summary>
    public const string EpicOutline = "#8b64a8";

    /// <summary>Every rarity, in the order of the roll: the shares add up from the first.</summary>
    public static readonly IReadOnlyList<Rarity> All =
    [
        Rarity.Common,
        Rarity.Rare,
        Rarity.Epic,
    ];

    /// <summary>The count of affixes of an item of a rarity (D-748).</summary>
    /// <exception cref="ContextException">The value is not a declared rarity.</exception>
    public static int AffixCount(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 0,
            Rarity.Rare => 1,
            Rarity.Epic => 2,
            _ => throw Undeclared(rarity),
        };
    }

    /// <summary>The share of the loot rolls that give a rarity, in percent (D-748). The three shares add up to 100.</summary>
    /// <exception cref="ContextException">The value is not a declared rarity.</exception>
    public static int SharePercent(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 70,
            Rarity.Rare => 25,
            Rarity.Epic => 5,
            _ => throw Undeclared(rarity),
        };
    }

    /// <summary>Answers whether an enemy that carries an item of a rarity shows an outline (D-49, D-748).</summary>
    /// <exception cref="ContextException">The value is not a declared rarity.</exception>
    public static bool HasOutline(Rarity rarity)
    {
        return AffixCount(rarity) > 0;
    }

    /// <summary>The outline color of a rarity, as a hex text of the palette (D-748).</summary>
    /// <exception cref="ContextException">The rarity is common, which has no outline, or the value is not a declared rarity.</exception>
    public static string Outline(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => RareOutline,
            Rarity.Epic => EpicOutline,
            Rarity.Common => throw new ContextException("A common item has no outline (D-748). Read HasOutline first."),
            _ => throw Undeclared(rarity),
        };
    }

    /// <summary>The error of a value that is not a declared rarity.</summary>
    private static ContextException Undeclared(Rarity rarity)
    {
        ContextException error = new($"The rarity must be a value of Rarity. The value is {(int)rarity}.");
        error.AddContext("rarity", ((long)rarity).ToString(CultureInfo.InvariantCulture));
        return error;
    }
}
