using Godot;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// Where each block finds its face in the one atlas (D-85): tiles of 32 pixels, eight per row, and the tile
/// index is the block id of D-259. PR-14 writes the atlas image and gives the model faces their own tiles.
/// </summary>
public static class AtlasLayout
{
    /// <summary>The side of one tile, in pixels (D-85).</summary>
    public const int TilePixels = 32;

    /// <summary>The count of tiles along one side of the atlas.</summary>
    public const int TilesPerRow = 8;

    /// <summary>The side of the atlas, in pixels.</summary>
    public const int AtlasPixels = TilePixels * TilesPerRow;

    /// <summary>The side of one tile as a fraction of the atlas. The world shader multiplies the face coordinate by it.</summary>
    public const float TileSize = 1.0f / TilesPerRow;

    /// <summary>The origin of the tile of one block, as a fraction of the atlas.</summary>
    public static Vector2 TileOrigin(BlockId block)
    {
        int index = (int)block;
        return new Vector2((index % TilesPerRow) * TileSize, (index / TilesPerRow) * TileSize);
    }
}
