using System.Collections.Generic;
using WhatYouCarry.Tools.TextureGen;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The place of each color and fine shade of a palette, found by its color. The atlas stores colors and not indices
/// (D-598), so a test reads a pixel as a ramp, a fine step, and an atlas index through this map. Each color of the
/// palette is unique, so each color has one place.
/// </summary>
internal sealed class PaletteShades
{
    private readonly Palette palette;
    private readonly Dictionary<AtlasColor, (int Ramp, int FineStep)> shadeOfColor = [];

    public PaletteShades(Palette palette)
    {
        this.palette = palette;
        for (int ramp = 0; ramp < palette.Ramps.Count; ramp++)
        {
            for (int fineStep = 0; fineStep <= palette.FineTop(ramp); fineStep++)
            {
                this.shadeOfColor.Add(palette.AtlasColors[palette.AtlasIndex(ramp, fineStep)], (ramp, fineStep));
            }
        }
    }

    /// <summary>Answers whether a color is a color or a fine shade of the palette.</summary>
    public bool Contains(AtlasColor color)
    {
        return this.shadeOfColor.ContainsKey(color);
    }

    /// <summary>The ramp and the fine step of a color of the palette. A color off the palette fails the test.</summary>
    public (int Ramp, int FineStep) Shade(AtlasColor color)
    {
        Assert.True(this.shadeOfColor.TryGetValue(color, out (int Ramp, int FineStep) shade), $"The color {Hex(color)} is no color or fine shade of the palette.");
        return shade;
    }

    /// <summary>The atlas index of a color of the palette: the index of <see cref="Palette.AtlasColors"/>.</summary>
    public int Index(AtlasColor color)
    {
        (int ramp, int fineStep) = this.Shade(color);
        return this.palette.AtlasIndex(ramp, fineStep);
    }

    /// <summary>A color as '#' and six lowercase hex digits.</summary>
    public static string Hex(AtlasColor color)
    {
        return $"#{color.Red:x2}{color.Green:x2}{color.Blue:x2}";
    }
}
