namespace WhatYouCarry.Assets;

/// <summary>
/// The fixed sizes of the one atlas (D-85, D-506). The texture generator of Tools packs a canvas for each block and
/// each model face into it, and it writes the place of each canvas to the texture layout (D-505). Game and Tools both
/// read the sizes here.
/// </summary>
public static class AtlasLayout
{
    /// <summary>The side of the atlas, in pixels (D-506). It is a power of two (PR-14 exit test 4).</summary>
    public const int AtlasPixels = 512;

    /// <summary>The side of the canvas of one block, in pixels: one block face is 32 texels on a side (D-85).</summary>
    public const int BlockPixels = 32;

    /// <summary>The texels along one meter of a face: one block canvas across one block (D-85). A model face has the same density (D-308).</summary>
    public const int TexelsPerMeter = BlockPixels;

    /// <summary>
    /// The width of the border around each canvas, in pixels. The border repeats the edge pixels of the canvas, so a
    /// sample at the edge of a face never reads a neighbor canvas.
    /// </summary>
    public const int Gutter = 1;
}
