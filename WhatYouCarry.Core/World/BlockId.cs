namespace WhatYouCarry.Core.World;

/// <summary>
/// The kind of block in one cell of the grid (D-78, D-164). The value is the one byte that the grid stores.
/// </summary>
/// <remarks>
/// PR-7 defines the two ids that collision needs, and no more (D-239). PR-9 adds the rest of the mine block set
/// of D-210 with a decision that also says whether still water is solid. A number here is part of the grid, so
/// it never changes once a later PR reads it.
/// </remarks>
public enum BlockId : byte
{
    /// <summary>No block. A body moves through it.</summary>
    Air = 0,

    /// <summary>Raw stone, the rock of the mine (D-210). A body stops at it.</summary>
    RawStone = 1,
}
