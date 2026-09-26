using System.Collections.Generic;
using WhatYouCarry.Tools.TextureGen;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The place of a color on one ramp: the ramp, and the lowest and the highest position that give the color, in parts of a fine step.</summary>
internal readonly record struct RampPlace(int Ramp, int Low, int High);

/// <summary>
/// The places of each color that a palette can paint, found by its color. The atlas stores colors and not indices
/// (D-598), and a grain puts a pixel between two fine shades (D-599), so a test reads a pixel through this map. A
/// color of the palette is exact: it is a color or a fine shade. Each position of each ramp gives one color, and
/// several positions next to each other can give the same color. A blend of one ramp can equal a blend of another
/// ramp near it, so a test names the ramp that it reads.
/// </summary>
internal sealed class PaletteShades
{
    private readonly Palette palette;
    private readonly Dictionary<AtlasColor, (int Ramp, int FineStep)> shadeOfColor = [];
    private readonly Dictionary<AtlasColor, List<RampPlace>> placesOfColor = [];

    public PaletteShades(Palette palette)
    {
        this.palette = palette;
        for (int ramp = 0; ramp < palette.Ramps.Count; ramp++)
        {
            for (int fineStep = 0; fineStep <= palette.FineTop(ramp); fineStep++)
            {
                this.shadeOfColor.Add(palette.AtlasColors[palette.AtlasIndex(ramp, fineStep)], (ramp, fineStep));
            }

            for (int position = 0; position <= palette.FineTop(ramp) * Palette.PartsPerFineStep; position++)
            {
                this.AddPlace(palette.ColorAt(ramp, position), ramp, position);
            }
        }
    }

    /// <summary>Answers whether a color lies on a ramp of the palette, at a fine shade or between two.</summary>
    public bool Contains(AtlasColor color)
    {
        return this.placesOfColor.ContainsKey(color);
    }

    /// <summary>The ramp and the fine step of an exact color of the palette. A color off the palette, or between two fine shades, fails the test.</summary>
    public (int Ramp, int FineStep) Shade(AtlasColor color)
    {
        Assert.True(this.shadeOfColor.TryGetValue(color, out (int Ramp, int FineStep) shade), $"The color {Hex(color)} is no color or fine shade of the palette.");
        return shade;
    }

    /// <summary>Answers whether a color lies on one ramp, and gives its place there.</summary>
    public bool TryPlace(AtlasColor color, int ramp, out RampPlace place)
    {
        place = default;
        if (!this.placesOfColor.TryGetValue(color, out List<RampPlace>? places))
        {
            return false;
        }

        foreach (RampPlace candidate in places)
        {
            if (candidate.Ramp == ramp)
            {
                place = candidate;
                return true;
            }
        }

        return false;
    }

    /// <summary>The ramp index of a ramp name. A name that the palette does not hold fails the test.</summary>
    public int RampOf(string name)
    {
        for (int ramp = 0; ramp < this.palette.Ramps.Count; ramp++)
        {
            if (this.palette.Ramps[ramp].Name == name)
            {
                return ramp;
            }
        }

        Assert.Fail($"The palette holds no ramp '{name}'.");
        return -1;
    }

    /// <summary>The atlas index of an exact color of the palette: the index of <see cref="Palette.AtlasColors"/>.</summary>
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

    /// <summary>Joins one position of one ramp to the places of its color. The positions of a ramp come in rising order.</summary>
    private void AddPlace(AtlasColor color, int ramp, int position)
    {
        if (!this.placesOfColor.TryGetValue(color, out List<RampPlace>? places))
        {
            places = [];
            this.placesOfColor.Add(color, places);
        }

        int last = places.Count - 1;
        if (last >= 0 && places[last].Ramp == ramp)
        {
            places[last] = places[last] with { High = position };
            return;
        }

        places.Add(new RampPlace(ramp, position, position));
    }
}
