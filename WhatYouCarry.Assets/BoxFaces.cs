using System;
using System.Collections.Generic;

namespace WhatYouCarry.Assets;

/// <summary>
/// The names and the texel sizes of the six faces of a box (D-308, D-505). The width and the height of a face are
/// the sizes that its texture shows: a vertical face stands upright, and the up and the down faces have their top
/// edge at north. The box mesh of Game and the texture generator of Tools both read them here.
/// </summary>
public static class BoxFaces
{
    /// <summary>
    /// The largest amount that a texel size can stand above a whole number and still count as that number. A box of
    /// 8 file units is 0.5 meters, and the float product with 64 texels per meter can land a hair above 32.
    /// </summary>
    public const float WholeTolerance = 0.001f;

    /// <summary>The name of each side as Blockbench writes it, in the order of <see cref="BoxSide"/>.</summary>
    public static readonly IReadOnlyList<string> Names = ["north", "east", "south", "west", "up", "down"];

    /// <summary>The Blockbench name of one side.</summary>
    public static string Name(BoxSide side)
    {
        return Names[(int)side];
    }

    /// <summary>The side of one Blockbench face name. A name that is not one of the six gives false.</summary>
    public static bool TryParse(string name, out BoxSide side)
    {
        for (int index = 0; index < Names.Count; index++)
        {
            if (Names[index] == name)
            {
                side = (BoxSide)index;
                return true;
            }
        }

        side = BoxSide.North;
        return false;
    }

    /// <summary>The width and the height of one face, in texels at 64 texels per meter (D-308, D-603). The values can hold a fraction.</summary>
    public static (float Width, float Height) Texels(ModelBox box, BoxSide side)
    {
        float x = (box.To.X - box.From.X) * AtlasLayout.TexelsPerMeter;
        float y = (box.To.Y - box.From.Y) * AtlasLayout.TexelsPerMeter;
        float z = (box.To.Z - box.From.Z) * AtlasLayout.TexelsPerMeter;
        switch (side)
        {
            case BoxSide.North:
            case BoxSide.South:
                return (x, y);
            case BoxSide.East:
            case BoxSide.West:
                return (z, y);
            default:
                return (x, z);
        }
    }

    /// <summary>
    /// The whole texels of the canvas of one face: each size of <see cref="Texels"/>, rounded up. A face of 21.6
    /// texels reads 21.6 rows of a canvas of 22. A face with no area gives a zero.
    /// </summary>
    public static (int Width, int Height) CanvasTexels(ModelBox box, BoxSide side)
    {
        (float width, float height) = Texels(box, side);
        return (RoundUp(width), RoundUp(height));
    }

    private static int RoundUp(float texels)
    {
        return (int)MathF.Ceiling(texels - WholeTolerance);
    }
}
