using System.IO;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The texture of every chunk (D-677): each block canvas of the atlas image, copied to its slot of <see cref="BlockTiles"/>,
/// with mipmaps. Each slot starts at a multiple of 64 pixels, so each mipmap level down to one pixel for each canvas
/// keeps the canvases apart, and a distant face does not shimmer. The world shader stops at that level.
/// </summary>
public static class BlockAtlas
{
    private const string NoMipmaps = "The engine could not make the mipmaps of the block atlas.";
    private const string FileField = "file";
    private const string ErrorField = "error";

    /// <summary>The block atlas of one atlas image.</summary>
    /// <param name="atlas">The atlas image of <see cref="AtlasFile.Load"/>, with no mipmaps.</param>
    /// <param name="tiles">The canvas and the slot of each block.</param>
    /// <param name="contentDirectory">The content directory of the atlas file, which an error names.</param>
    /// <exception cref="ContextException">The engine could not make the mipmaps.</exception>
    public static ImageTexture Build(Image atlas, BlockTiles tiles, string contentDirectory)
    {
        Image blocks = Image.CreateEmpty(AtlasLayout.AtlasPixels, AtlasLayout.AtlasPixels, false, atlas.GetFormat());
        foreach (BlockCanvas canvas in tiles.Canvases)
        {
            blocks.BlitRect(atlas, canvas.Source, canvas.Slot);
        }

        Error result = blocks.GenerateMipmaps();
        if (result != Error.Ok)
        {
            ContextException failed = new(NoMipmaps);
            failed.AddContext(FileField, Path.Combine(contentDirectory, AssetPaths.AtlasImage));
            failed.AddContext(ErrorField, result.ToString());
            throw failed;
        }

        return ImageTexture.CreateFromImage(blocks);
    }
}
