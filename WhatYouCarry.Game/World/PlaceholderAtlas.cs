using Godot;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The atlas of PR-13: one flat color per block id, in the tiles of <see cref="AtlasLayout"/>, built at boot.
/// PR-14 replaces it with the generated image from the palette of OQ-1 (D-85).
/// </summary>
public static class PlaceholderAtlas
{
    /// <summary>The color of every tile that no block owns, which the model faces read until PR-14 assigns their tiles.</summary>
    public static readonly Color BaseColor = new(0.50f, 0.46f, 0.42f);

    /// <summary>The color of each tile, by block id (D-259). The air tile is black, and no face reads it.</summary>
    public static readonly Color[] TileColors =
    [
        new(0.0f, 0.0f, 0.0f),
        new(0.42f, 0.40f, 0.38f),
        new(0.55f, 0.52f, 0.48f),
        new(0.45f, 0.30f, 0.16f),
        new(0.60f, 0.50f, 0.25f),
        new(0.20f, 0.35f, 0.55f),
        new(0.33f, 0.30f, 0.28f),
        new(0.58f, 0.42f, 0.24f),
    ];

    /// <summary>The atlas texture, with nearest filtering left to the shader.</summary>
    public static ImageTexture Create()
    {
        Image image = Image.CreateEmpty(AtlasLayout.AtlasPixels, AtlasLayout.AtlasPixels, false, Image.Format.Rgb8);
        image.Fill(BaseColor);
        for (int block = 0; block < TileColors.Length; block++)
        {
            Vector2 origin = AtlasLayout.TileOrigin((BlockId)block) * AtlasLayout.AtlasPixels;
            Rect2I tile = new((int)origin.X, (int)origin.Y, AtlasLayout.TilePixels, AtlasLayout.TilePixels);
            image.FillRect(tile, TileColors[block]);
        }

        return ImageTexture.CreateFromImage(image);
    }
}
