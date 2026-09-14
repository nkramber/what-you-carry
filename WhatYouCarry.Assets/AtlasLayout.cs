using System;
using System.Globalization;

namespace WhatYouCarry.Assets;

/// <summary>
/// The layout of the one atlas (D-85): square tiles of 32 pixels, eight to a row, so the atlas is 256 pixels on a
/// side. The tile of a block is its id (D-259), and the three body materials take the first three tiles of the
/// second row (D-307). Game places each face on its tile, and the texture generator of Tools paints each tile, so
/// both read the layout here.
/// </summary>
public static class AtlasLayout
{
    /// <summary>The side of one tile, in pixels (D-85).</summary>
    public const int TilePixels = 32;

    /// <summary>The count of tiles along one side of the atlas.</summary>
    public const int TilesPerRow = 8;

    /// <summary>The side of the atlas, in pixels. It is a power of two (PR-14 exit test 4).</summary>
    public const int AtlasPixels = TilePixels * TilesPerRow;

    /// <summary>The count of tiles in the atlas.</summary>
    public const int TileCount = TilesPerRow * TilesPerRow;

    /// <summary>The texels along one meter of a face: one tile across one block (D-85). A body face has the same density (D-308).</summary>
    public const int TexelsPerMeter = TilePixels;

    /// <summary>The tile of the skin of the body, the first tile of the second row (D-307).</summary>
    public const int SkinTile = TilesPerRow;

    /// <summary>The tile of the cloth of the body (D-307).</summary>
    public const int ClothTile = TilesPerRow + 1;

    /// <summary>The tile of the leather of the body (D-307).</summary>
    public const int LeatherTile = TilesPerRow + 2;

    /// <summary>The tile of the metal of the sword blade, the tile after the body tiles (D-330).</summary>
    public const int MetalTile = TilesPerRow + 3;

    /// <summary>The column of a tile, from zero at the left edge of the atlas.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tile is outside the atlas.</exception>
    public static int Column(int tile)
    {
        CheckTile(tile);
        return tile % TilesPerRow;
    }

    /// <summary>The row of a tile, from zero at the top edge of the atlas.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tile is outside the atlas.</exception>
    public static int Row(int tile)
    {
        CheckTile(tile);
        return tile / TilesPerRow;
    }

    /// <summary>A tile index must name a tile of the atlas.</summary>
    private static void CheckTile(int tile)
    {
        if (tile < 0 || tile >= TileCount)
        {
            throw new ArgumentOutOfRangeException(nameof(tile), tile, $"The atlas holds the tiles 0 to {(TileCount - 1).ToString(CultureInfo.InvariantCulture)}.");
        }
    }
}
