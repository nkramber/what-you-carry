using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>
/// One rule file under <c>textures/rules/</c> (D-85, D-307): the atlas tile that the rule paints, the flat palette
/// index of its base color, the noise amount, the edge darkness, and the seed of the noise.
/// </summary>
/// <remarks>
/// <para>
/// The noise amount is the chance, from 0 to 1, that a pixel moves one step off the base on its ramp. Half of the
/// chance moves it down, and the other half moves it up. The edge darkness is the count of steps that the outer
/// ring of the tile moves down. <see cref="TextureGenerator"/> applies both.
/// </para>
/// <para>
/// The seed starts a xorshift sequence, and a zero seed stays zero forever, so a zero seed is an error. The rule is
/// a file of this project, so the unknown-field check of D-168 applies.
/// </para>
/// </remarks>
public sealed record TextureRule(string ContentPath, int Tile, int Base, double Noise, int Edge, uint Seed)
{
    /// <summary>The field of the atlas tile that the rule paints.</summary>
    public const string TileKey = "tile";

    /// <summary>The field of the flat palette index of the base color.</summary>
    public const string BaseKey = "base";

    /// <summary>The field of the noise amount, from 0 to 1.</summary>
    public const string NoiseKey = "noise";

    /// <summary>The field of the edge darkness, in steps.</summary>
    public const string EdgeKey = "edge";

    /// <summary>The field of the seed of the noise.</summary>
    public const string SeedKey = "seed";

    private static readonly string[] Fields = [TileKey, BaseKey, NoiseKey, EdgeKey, SeedKey];

    /// <summary>The rule of one file, checked against the atlas and the palette.</summary>
    /// <exception cref="ContextException">The file is not a JSON object, a field is absent, unknown, or of another kind, the tile is outside the atlas, the base names an index past the palette, the noise is outside 0 to 1, the edge is below zero, or the seed is zero.</exception>
    public static TextureRule Parse(string path, byte[] bytes, Palette palette)
    {
        using JsonDocument document = TextureJson.Parse(path, bytes);
        JsonElement root = document.RootElement;
        JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, Fields);

        int tile = JsonShape.WholeNumber(path, root, TextureJson.RootName, TileKey);
        if (tile < 0 || tile >= AtlasLayout.TileCount)
        {
            throw ContentError.Make(path, TileKey, $"is {Text(tile)}, and the atlas holds the tiles 0 to {Text(AtlasLayout.TileCount - 1)}");
        }

        int baseIndex = JsonShape.WholeNumber(path, root, TextureJson.RootName, BaseKey);
        if (baseIndex < 0 || baseIndex >= palette.Colors.Count)
        {
            throw ContentError.Make(path, BaseKey, $"names the color index {Text(baseIndex)}, and the palette holds {Text(palette.Colors.Count)} colors at the indices 0 to {Text(palette.Colors.Count - 1)}");
        }

        double noise = ReadFraction(path, root, NoiseKey);
        int edge = JsonShape.WholeNumber(path, root, TextureJson.RootName, EdgeKey);
        if (edge < 0)
        {
            throw ContentError.Make(path, EdgeKey, $"is {Text(edge)}, and the edge darkness is zero or more steps");
        }

        uint seed = ReadSeed(path, root);
        return new TextureRule(path, tile, baseIndex, noise, edge, seed);
    }

    /// <summary>A number member from 0 to 1, as a double, so the value in the file is the value that the generator compares.</summary>
    private static double ReadFraction(string path, JsonElement root, string name)
    {
        JsonElement value = JsonShape.Member(path, root, TextureJson.RootName, name);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double number) || !double.IsFinite(number) || number < 0.0 || number > 1.0)
        {
            throw ContentError.Make(path, name, "is not a number from 0 to 1");
        }

        return number;
    }

    /// <summary>The seed member: a whole number that 32 bits hold, and not zero.</summary>
    private static uint ReadSeed(string path, JsonElement root)
    {
        JsonElement value = JsonShape.Member(path, root, TextureJson.RootName, SeedKey);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetUInt32(out uint seed) || seed == 0)
        {
            throw ContentError.Make(path, SeedKey, "is not a whole number from 1 to 4294967295, and a zero seed never leaves zero");
        }

        return seed;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
