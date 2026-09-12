using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The place of a block face in the atlas, as the fractions that the world shader reads (D-85, D-259). The layout
/// itself lives in the Assets project, because the texture generator of Tools paints the same tiles.
/// </summary>
public static class AtlasTile
{
    /// <summary>The side of one tile as a fraction of the atlas. The world shader multiplies the face coordinate by it.</summary>
    public const float Size = 1.0f / AtlasLayout.TilesPerRow;

    /// <summary>The origin of the tile of one block, as a fraction of the atlas. The tile index is the block id.</summary>
    public static Vector2 Origin(BlockId block)
    {
        int tile = (int)block;
        return new Vector2(AtlasLayout.Column(tile) * Size, AtlasLayout.Row(tile) * Size);
    }
}
