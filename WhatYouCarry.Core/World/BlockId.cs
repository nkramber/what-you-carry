namespace WhatYouCarry.Core.World;

/// <summary>
/// The kind of block in one cell of the grid (D-78, D-164). The value is the one byte that the grid stores.
/// </summary>
/// <remarks>
/// The ids take the order of D-210 (D-259). A number here is part of the grid, so it never changes once a floor
/// holds it. Every block but air and still water is solid (D-239, D-258). PR-7 declared the first two, and
/// PR-59 declared the rest.
/// </remarks>
public enum BlockId : byte
{
    /// <summary>No block. A body moves through it.</summary>
    Air = 0,

    /// <summary>Raw stone, the rock of the mine (D-210). A body stops at it.</summary>
    RawStone = 1,

    /// <summary>Hewn stone, the cut walls of the older workings (D-210). A body stops at it.</summary>
    HewnStone = 2,

    /// <summary>A timber beam, the posts of the working mine (D-210). A body stops at it.</summary>
    TimberBeam = 3,

    /// <summary>An ore vein in the deep (D-210). A body stops at it.</summary>
    OreVein = 4,

    /// <summary>Still water, one block deep in the older workings (D-210, D-258). A body moves through it, and it walks and jumps more slowly there.</summary>
    StillWater = 5,

    /// <summary>Rubble, the fill of a collapsed dead end (D-210, D-253). A body stops at it.</summary>
    Rubble = 6,

    /// <summary>A plank on a wall of the working mine (D-210). A body stops at it.</summary>
    Plank = 7,
}
