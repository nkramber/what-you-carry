using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>One color of the palette: its three sRGB bytes, the index of its ramp, and its step on that ramp from zero at the dark end.</summary>
public readonly record struct PaletteColor(byte Red, byte Green, byte Blue, int Ramp, int Step);

/// <summary>One ramp of the palette: its name, the flat index of its darkest color, its count of colors, and the atlas index of its first fine shade (D-528).</summary>
public sealed record PaletteRamp(string Name, int First, int Count, int ShadeFirst);

/// <summary>One color of the atlas: three sRGB bytes.</summary>
public readonly record struct AtlasColor(byte Red, byte Green, byte Blue);

/// <summary>
/// The palette of the atlas (D-85, D-304), read from <c>textures/palette.json</c>: a list of ramps, and each ramp
/// has a name and its colors from dark to light. The flat index of a color counts through the ramps in file order.
/// A rule names its base color by that index, and the generator moves a pixel along the ramp of that index alone,
/// so every pixel of the atlas is a palette color (PR-14 exit test 1). The atlas stores the color of each pixel and
/// not an index, so the palette has no count limit (D-598).
/// </summary>
/// <remarks>
/// <para>
/// Each ramp also lists its fine shades, three between each pair of its colors (D-528). A pixel moves along a ramp in
/// fine steps: <see cref="ShadesPerStep"/> fine steps make one step of a color, so the fine step of a color is its
/// step times four. The list of atlas colors holds every color at its flat index, and then the shades of each ramp
/// in ramp order.
/// </para>
/// <para>
/// The palette is a file of this project, so the unknown-field check of D-168 applies. A color is <c>#</c> and six
/// lowercase hex digits, so each color has one spelling. A color that appears twice is an error, because two
/// indices for one color hide a mistake in the file.
/// </para>
/// </remarks>
public sealed class Palette
{
    /// <summary>The field of the list of ramps.</summary>
    public const string RampsKey = "ramps";

    /// <summary>The field of the name of a ramp.</summary>
    public const string NameKey = "name";

    /// <summary>The field of the colors of a ramp.</summary>
    public const string ColorsKey = "colors";

    /// <summary>The field of the fine shades of a ramp.</summary>
    public const string ShadesKey = "shades";

    /// <summary>The fine steps from one color of a ramp to the next (D-528).</summary>
    public const int ShadesPerStep = 4;

    private const int ColorLength = 7;

    private static readonly string[] RootFields = [RampsKey];
    private static readonly string[] RampFields = [NameKey, ColorsKey, ShadesKey];

    private Palette(IReadOnlyList<PaletteRamp> ramps, IReadOnlyList<PaletteColor> colors, IReadOnlyList<AtlasColor> atlasColors)
    {
        this.Ramps = ramps;
        this.Colors = colors;
        this.AtlasColors = atlasColors;
    }

    /// <summary>Every ramp, in file order.</summary>
    public IReadOnlyList<PaletteRamp> Ramps { get; }

    /// <summary>Every color, by flat index.</summary>
    public IReadOnlyList<PaletteColor> Colors { get; }

    /// <summary>Every color and every fine shade: every color at its flat index, then the fine shades of each ramp.</summary>
    public IReadOnlyList<AtlasColor> AtlasColors { get; }

    /// <summary>The palette of one file.</summary>
    /// <exception cref="ContextException">The file is not a JSON object, a field is absent or unknown, a list is empty, a color is not lowercase hex, a ramp has the wrong count of shades, or a name or a color appears twice.</exception>
    public static Palette Parse(string path, byte[] bytes)
    {
        using JsonDocument document = TextureJson.Parse(path, bytes);
        JsonElement root = document.RootElement;
        JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, RootFields);
        JsonElement rampList = JsonShape.Member(path, root, TextureJson.RootName, RampsKey);
        if (rampList.ValueKind != JsonValueKind.Array || rampList.GetArrayLength() == 0)
        {
            throw ContentError.Make(path, RampsKey, "is not a list that holds at least one ramp");
        }

        List<(string Name, int First, int Count)> rampColors = [];
        List<PaletteColor> colors = [];
        List<List<AtlasColor>> shadesOfRamp = [];
        Dictionary<string, string> placeOfColor = [];
        foreach (JsonElement ramp in rampList.EnumerateArray())
        {
            string owner = $"{RampsKey}[{Text(rampColors.Count)}]";
            string name = JsonShape.Text(path, ramp, owner, NameKey);
            JsonShape.CheckNoUnknownMember(path, ramp, owner, RampFields);
            CheckNewName(path, owner, name, rampColors);

            JsonElement colorList = JsonShape.Member(path, ramp, name, ColorsKey);
            if (colorList.ValueKind != JsonValueKind.Array || colorList.GetArrayLength() == 0)
            {
                throw ContentError.Make(path, ColorsKey, $"on the ramp '{name}' is not a list that holds at least one color");
            }

            int first = colors.Count;
            foreach (JsonElement item in colorList.EnumerateArray())
            {
                int step = colors.Count - first;
                string place = $"step {Text(step)} of the ramp '{name}'";
                string value = CheckedColor(path, ColorsKey, item, place, placeOfColor);
                colors.Add(new PaletteColor(HexByte(value, 1), HexByte(value, 3), HexByte(value, 5), rampColors.Count, step));
            }

            int count = colors.Count - first;
            shadesOfRamp.Add(ReadShades(path, ramp, name, count, placeOfColor));
            rampColors.Add((name, first, count));
        }

        List<PaletteRamp> ramps = [];
        List<AtlasColor> atlasColors = [];
        foreach (PaletteColor color in colors)
        {
            atlasColors.Add(new AtlasColor(color.Red, color.Green, color.Blue));
        }

        for (int index = 0; index < rampColors.Count; index++)
        {
            ramps.Add(new PaletteRamp(rampColors[index].Name, rampColors[index].First, rampColors[index].Count, atlasColors.Count));
            atlasColors.AddRange(shadesOfRamp[index]);
        }

        return new Palette(ramps, colors, atlasColors);
    }

    /// <summary>The highest fine step of one ramp: its last color, at <see cref="ShadesPerStep"/> fine steps for each step.</summary>
    public int FineTop(int ramp)
    {
        return (this.Ramps[ramp].Count - 1) * ShadesPerStep;
    }

    /// <summary>The atlas index of one fine step of one ramp: a color at a whole step, and otherwise one of its shades.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The fine step lies outside the ramp.</exception>
    public int AtlasIndex(int ramp, int fineStep)
    {
        PaletteRamp named = this.Ramps[ramp];
        if (fineStep < 0 || fineStep > this.FineTop(ramp))
        {
            throw new ArgumentOutOfRangeException(nameof(fineStep), $"The fine step {Text(fineStep)} lies outside the ramp '{named.Name}', which runs from 0 to {Text(this.FineTop(ramp))}.");
        }

        int step = fineStep / ShadesPerStep;
        int between = fineStep % ShadesPerStep;
        return between == 0 ? named.First + step : named.ShadeFirst + (step * (ShadesPerStep - 1)) + between - 1;
    }

    /// <summary>The shades of one ramp: three between each pair of its colors, dark to light (D-528).</summary>
    private static List<AtlasColor> ReadShades(string path, JsonElement ramp, string name, int colorCount, Dictionary<string, string> placeOfColor)
    {
        int expected = (colorCount - 1) * (ShadesPerStep - 1);
        JsonElement shadeList = JsonShape.Member(path, ramp, name, ShadesKey);
        if (shadeList.ValueKind != JsonValueKind.Array || shadeList.GetArrayLength() != expected)
        {
            throw ContentError.Make(path, ShadesKey, $"on the ramp '{name}' is not a list of {Text(expected)} shades, three between each pair of its {Text(colorCount)} colors (D-528)");
        }

        List<AtlasColor> shades = [];
        foreach (JsonElement item in shadeList.EnumerateArray())
        {
            string place = $"shade {Text(shades.Count)} of the ramp '{name}'";
            string value = CheckedColor(path, ShadesKey, item, place, placeOfColor);
            shades.Add(new AtlasColor(HexByte(value, 1), HexByte(value, 3), HexByte(value, 5)));
        }

        return shades;
    }

    /// <summary>One color of the file: '#' and six lowercase hex digits, at no earlier place. The place joins the map.</summary>
    private static string CheckedColor(string path, string field, JsonElement item, string place, Dictionary<string, string> placeOfColor)
    {
        string value = item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : string.Empty;
        if (!IsColor(value))
        {
            throw ContentError.Make(path, field, $"holds a value at {place} that is not '#' and six lowercase hex digits");
        }

        if (placeOfColor.TryGetValue(value, out string? earlier))
        {
            throw ContentError.Make(path, field, $"holds '{value}' at {place} and at {earlier}, and each color appears once");
        }

        placeOfColor.Add(value, place);
        return value;
    }

    /// <summary>A ramp name is not empty, and no earlier ramp has it.</summary>
    private static void CheckNewName(string path, string owner, string name, List<(string Name, int First, int Count)> earlier)
    {
        if (name.Length == 0)
        {
            throw ContentError.Make(path, NameKey, $"on '{owner}' is empty");
        }

        foreach ((string rampName, _, _) in earlier)
        {
            if (rampName == name)
            {
                throw ContentError.Make(path, NameKey, $"on '{owner}' is '{name}', and an earlier ramp has that name");
            }
        }
    }

    /// <summary>Answers whether a text is '#' and six lowercase hex digits.</summary>
    private static bool IsColor(string value)
    {
        if (value.Length != ColorLength || value[0] != '#')
        {
            return false;
        }

        for (int index = 1; index < ColorLength; index++)
        {
            char digit = value[index];
            bool isDigit = digit >= '0' && digit <= '9';
            bool isLowerHex = digit >= 'a' && digit <= 'f';
            if (!isDigit && !isLowerHex)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>The byte of the two hex digits at one offset of a checked color.</summary>
    private static byte HexByte(string color, int offset)
    {
        return byte.Parse(color.AsSpan(offset, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
