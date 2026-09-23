using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The place of each block canvas in the atlas, as the fractions that the world shader reads (D-85, D-505). The
/// texture layout of the generator gives each place, so the mesher never computes one. The object never changes after
/// the constructor, so the next-floor worker can mesh with it on its own thread (D-72).
/// </summary>
public sealed class BlockTiles
{
    /// <summary>The side of one block canvas as a fraction of the atlas. The world shader multiplies the face coordinate by it.</summary>
    public const float Size = (float)AtlasLayout.BlockPixels / AtlasLayout.AtlasPixels;

    private const string NotOneBlockFace = "The texture layout gives a block a canvas that is not one block face of 32 pixels (D-85).";
    private const string NoCanvas = "The texture layout has no canvas for the block. Bind a recipe to the block, and run texture-gen (D-505).";
    private const string FileField = "file";
    private const string BlockField = "block";
    private const string WidthField = "width";
    private const string HeightField = "height";

    private readonly Dictionary<BlockId, Vector2> origins = [];

    /// <summary>The origins of every block canvas of one layout.</summary>
    /// <exception cref="ContextException">A block canvas is not 32 pixels on a side, or it names an id past a byte.</exception>
    public BlockTiles(TextureLayout layout)
    {
        foreach (BlockPlace place in layout.Blocks)
        {
            if (place.At.Width != AtlasLayout.BlockPixels || place.At.Height != AtlasLayout.BlockPixels || place.Block < 0 || place.Block > byte.MaxValue)
            {
                ContextException error = new(NotOneBlockFace);
                error.AddContext(FileField, AssetPaths.LayoutFile);
                error.AddContext(BlockField, place.Block.ToString(CultureInfo.InvariantCulture));
                error.AddContext(WidthField, place.At.Width.ToString(CultureInfo.InvariantCulture));
                error.AddContext(HeightField, place.At.Height.ToString(CultureInfo.InvariantCulture));
                throw error;
            }

            this.origins.Add((BlockId)place.Block, new Vector2((float)place.At.X / AtlasLayout.AtlasPixels, (float)place.At.Y / AtlasLayout.AtlasPixels));
        }
    }

    /// <summary>The origin of the canvas of one block, as a fraction of the atlas.</summary>
    /// <exception cref="ContextException">The layout has no canvas for the block.</exception>
    public Vector2 Origin(BlockId block)
    {
        if (!this.origins.TryGetValue(block, out Vector2 origin))
        {
            ContextException error = new(NoCanvas);
            error.AddContext(FileField, AssetPaths.LayoutFile);
            error.AddContext(BlockField, ((int)block).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return origin;
    }
}
