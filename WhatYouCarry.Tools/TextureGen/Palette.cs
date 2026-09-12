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

/// <summary>One ramp of the palette: its name, the flat index of its darkest color, and its count of colors.</summary>
public sealed record PaletteRamp(string Name, int First, int Count);

/// <summary>
/// The palette of the atlas (D-85, D-304), read from <c>textures/palette.json</c>: a list of ramps, and each ramp
/// has a name and its colors from dark to light. The flat index of a color counts through the ramps in file order.
/// A rule names its base color by that index, and the generator moves a pixel along the ramp of that index alone,
/// so every pixel of the atlas is a palette color (PR-14 exit test 1).
/// </summary>
/// <remarks>
/// The palette is a file of this project, so the unknown-field check of D-168 applies. A color is <c>#</c> and six
/// lowercase hex digits, so each color has one spelling. A color that appears twice is an error, because two
/// indices for one color hide a mistake in the file.
/// </remarks>
public sealed class Palette
{
    /// <summary>The most colors that the palette of one indexed PNG holds.</summary>
    public const int MaxColors = 256;

    /// <summary>The field of the list of ramps.</summary>
    public const string RampsKey = "ramps";

    /// <summary>The field of the name of a ramp.</summary>
    public const string NameKey = "name";

    /// <summary>The field of the colors of a ramp.</summary>
    public const string ColorsKey = "colors";

    private const int ColorLength = 7;

    private static readonly string[] RootFields = [RampsKey];
    private static readonly string[] RampFields = [NameKey, ColorsKey];

    private Palette(IReadOnlyList<PaletteRamp> ramps, IReadOnlyList<PaletteColor> colors)
    {
        this.Ramps = ramps;
        this.Colors = colors;
    }

    /// <summary>Every ramp, in file order.</summary>
    public IReadOnlyList<PaletteRamp> Ramps { get; }

    /// <summary>Every color, by flat index.</summary>
    public IReadOnlyList<PaletteColor> Colors { get; }

    /// <summary>The palette of one file.</summary>
    /// <exception cref="ContextException">The file is not a JSON object, a field is absent or unknown, a list is empty, a color is not lowercase hex, a name or a color appears twice, or the count passes <see cref="MaxColors"/>.</exception>
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

        List<PaletteRamp> ramps = [];
        List<PaletteColor> colors = [];
        Dictionary<string, string> placeOfColor = [];
        foreach (JsonElement ramp in rampList.EnumerateArray())
        {
            string owner = $"{RampsKey}[{Text(ramps.Count)}]";
            string name = JsonShape.Text(path, ramp, owner, NameKey);
            JsonShape.CheckNoUnknownMember(path, ramp, owner, RampFields);
            CheckNewName(path, owner, name, ramps);

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
                string value = item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : string.Empty;
                if (!IsColor(value))
                {
                    throw ContentError.Make(path, ColorsKey, $"holds a value at {place} that is not '#' and six lowercase hex digits");
                }

                if (placeOfColor.TryGetValue(value, out string? earlier))
                {
                    throw ContentError.Make(path, ColorsKey, $"holds '{value}' at {place} and at {earlier}, and each color appears once");
                }

                placeOfColor.Add(value, place);
                colors.Add(new PaletteColor(HexByte(value, 1), HexByte(value, 3), HexByte(value, 5), ramps.Count, step));
            }

            ramps.Add(new PaletteRamp(name, first, colors.Count - first));
        }

        if (colors.Count > MaxColors)
        {
            throw ContentError.Make(path, RampsKey, $"holds {Text(colors.Count)} colors, and an indexed PNG holds {Text(MaxColors)} at most");
        }

        return new Palette(ramps, colors);
    }

    /// <summary>A ramp name is not empty, and no earlier ramp has it.</summary>
    private static void CheckNewName(string path, string owner, string name, List<PaletteRamp> earlier)
    {
        if (name.Length == 0)
        {
            throw ContentError.Make(path, NameKey, $"on '{owner}' is empty");
        }

        foreach (PaletteRamp ramp in earlier)
        {
            if (ramp.Name == name)
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
