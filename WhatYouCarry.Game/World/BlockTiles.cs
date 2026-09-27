using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>One block canvas: its place in the atlas file, and its slot in the block atlas.</summary>
/// <param name="Block">The block.</param>
/// <param name="Source">The place of the canvas in the atlas file, from the texture layout (D-505).</param>
/// <param name="Slot">The top left pixel of the canvas in the block atlas, a multiple of 64 on each axis.</param>
public readonly record struct BlockCanvas(BlockId Block, Rect2I Source, Vector2I Slot);

/// <summary>
/// The place of each block canvas in the block atlas, as the fractions that the world shader reads (D-85, D-505,
/// D-677). The texture layout of the generator gives the canvas of each block. The block atlas puts the canvases in
/// layout order on a grid of 64 pixels, so each mipmap level down to one pixel for each canvas keeps the canvases
/// apart. The atlas file has a gutter of one pixel, and it keeps them apart on the first level alone. The object never
/// changes after the constructor, so the next-floor worker can mesh with it on its own thread (D-72).
/// </summary>
public sealed class BlockTiles
{
    /// <summary>The side of one block canvas as a fraction of the block atlas. The world shader multiplies the face coordinate by it.</summary>
    public const float Size = (float)AtlasLayout.BlockPixels / AtlasLayout.AtlasPixels;

    /// <summary>The count of slots along one side of the block atlas: 16, so the atlas holds 256 slots, one for each block id.</summary>
    public const int SlotsPerSide = AtlasLayout.AtlasPixels / AtlasLayout.BlockPixels;

    private const string NotOneBlockFace = "The texture layout gives a block a canvas that is not one block face of 64 pixels (D-603).";
    private const string NoCanvas = "The texture layout has no canvas for the block. Bind a recipe to the block, and run texture-gen (D-505).";
    private const string FileField = "file";
    private const string BlockField = "block";
    private const string WidthField = "width";
    private const string HeightField = "height";

    private readonly Dictionary<BlockId, Vector2> origins = [];
    private readonly List<BlockCanvas> canvases = [];

    /// <summary>The slot and the origin of every block canvas of one layout.</summary>
    /// <remarks>
    /// Each id fits a byte, and the layout gives each id one canvas, so the 256 slots hold every canvas.
    /// </remarks>
    /// <exception cref="ContextException">A block canvas is not 64 pixels on a side, or it names an id past a byte.</exception>
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

            int index = this.canvases.Count;
            Vector2I slot = new(index % SlotsPerSide * AtlasLayout.BlockPixels, index / SlotsPerSide * AtlasLayout.BlockPixels);
            Rect2I source = new(place.At.X, place.At.Y, place.At.Width, place.At.Height);
            this.canvases.Add(new BlockCanvas((BlockId)place.Block, source, slot));
            this.origins.Add((BlockId)place.Block, new Vector2((float)slot.X / AtlasLayout.AtlasPixels, (float)slot.Y / AtlasLayout.AtlasPixels));
        }
    }

    /// <summary>Every block canvas, in layout order. <see cref="BlockAtlas"/> copies each one to its slot.</summary>
    public IReadOnlyList<BlockCanvas> Canvases => this.canvases;

    /// <summary>The origin of the canvas of one block, as a fraction of the block atlas.</summary>
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
