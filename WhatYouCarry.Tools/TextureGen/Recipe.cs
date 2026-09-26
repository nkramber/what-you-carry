using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>One paint layer of a recipe (D-507). <see cref="CanvasPainter"/> applies the layers in file order.</summary>
public abstract record RecipeLayer;

/// <summary>The first layer of every recipe: each pixel takes the color at its shade, and the noise moves it one step on its ramp.</summary>
/// <param name="Color">The flat palette index of the color.</param>
/// <param name="Shade">The fine steps from the color, from -3 to 3 (D-527, D-528). Zero paints the color itself.</param>
/// <param name="Noise">The chance, from 0 to 1, that a pixel moves one step. Half of the chance moves it down, and the other half moves it up.</param>
/// <param name="Seed">The seed of the noise, not zero.</param>
public sealed record FillLayer(int Color, int Shade, double Noise, uint Seed) : RecipeLayer;

/// <summary>The outer ring of pixels of the canvas moves down its ramp by a count of steps.</summary>
public sealed record EdgeLayer(int Steps) : RecipeLayer;

/// <summary>A rectangle of the canvas, from its top left corner, takes a color at its shade with its own noise. The canvas clips it.</summary>
public sealed record RectLayer(int X, int Y, int Width, int Height, int Color, int Shade, double Noise, uint Seed) : RecipeLayer;

/// <summary>The pixels within a depth of one side of the canvas move up or down their ramp by a shift in steps.</summary>
public sealed record BandLayer(CanvasSide Side, int Depth, int Shift) : RecipeLayer;

/// <summary>
/// Clustered noise (D-527): each pixel moves by a smooth value of a lattice with a spacing of <paramref name="Cell"/>
/// pixels, times <paramref name="Amount"/> fine steps, plus a dither of its own.
/// </summary>
/// <param name="Cell">The spacing of the lattice, in pixels, 1 or more.</param>
/// <param name="Amount">The largest move of the smooth value, in fine steps, 1 or more.</param>
/// <param name="Seed">The seed of the lattice and the dither, not zero.</param>
public sealed record GrainLayer(int Cell, int Amount, uint Seed) : RecipeLayer;

/// <summary>The pixels within a depth of one side move by up to a shift in fine steps, the full shift at the side and less toward the depth (D-527).</summary>
public sealed record GradientLayer(CanvasSide Side, int Depth, int Shift) : RecipeLayer;

/// <summary>
/// A texel map (D-612): each pixel of the canvas takes the fine shade that the legend gives its character. The map
/// covers the whole canvas, so it has the size of the canvas. The <c>texture-trace</c> command writes a map from an
/// unlit view of the look reference, and a hand edit of a row corrects a feature.
/// </summary>
/// <param name="Rows">The rows of the map from the top, each a string of legend characters from the left.</param>
/// <param name="Legend">The color and the shade of each character.</param>
public sealed record MapLayer(IReadOnlyList<string> Rows, IReadOnlyDictionary<char, MapShade> Legend) : RecipeLayer;

/// <summary>One entry of a map legend: a flat palette index, and the fine steps from it, as a fill names them (D-528).</summary>
public readonly record struct MapShade(int Color, int Shade);

/// <summary>The four sides of a canvas, as its texture shows them.</summary>
public enum CanvasSide
{
    /// <summary>The top row.</summary>
    Top,

    /// <summary>The bottom row.</summary>
    Bottom,

    /// <summary>The left column.</summary>
    Left,

    /// <summary>The right column.</summary>
    Right,
}

/// <summary>One recipe with its layers resolved: a recipe that extends another holds the layers of the other, with its colors swapped.</summary>
/// <param name="ContentPath">The path of the recipe file, relative to the content directory.</param>
/// <param name="Name">The recipe name: the file name without its extension.</param>
/// <param name="Layers">The layers in paint order. The first layer is the one fill or the one map.</param>
public sealed record Recipe(string ContentPath, string Name, IReadOnlyList<RecipeLayer> Layers);

/// <summary>
/// The recipe files under <c>textures/recipes/</c> (D-505, D-507). A file holds one of two forms. The first form is a
/// list of layers. The second form extends another recipe and swaps ramps: each color of a swapped ramp takes the
/// same step of the other ramp. A recipe that extends another cannot itself be extended, so one read resolves every
/// recipe.
/// </summary>
/// <remarks>
/// Each layer names its kind, and an unknown kind is an error (D-507). The recipe is a file of this project, so the
/// unknown-field check of D-168 applies, and an absent field is an error (D-92).
/// </remarks>
public static class RecipeFile
{
    /// <summary>The field of the layer list.</summary>
    public const string LayersKey = "layers";

    /// <summary>The field of the recipe that a recipe extends.</summary>
    public const string ExtendsKey = "extends";

    /// <summary>The field of the ramp swap of a recipe that extends another.</summary>
    public const string SwapKey = "swap";

    /// <summary>The field of the kind of a layer.</summary>
    public const string KindKey = "kind";

    private const string FillKind = "fill";
    private const string EdgeKind = "edge";
    private const string RectKind = "rect";
    private const string BandKind = "band";
    private const string GrainKind = "grain";
    private const string GradientKind = "gradient";
    private const string MapKind = "map";
    private const string LegendKey = "legend";
    private const string RowsKey = "rows";
    private const string ColorKey = "color";
    private const string NoiseKey = "noise";
    private const string SeedKey = "seed";
    private const string StepsKey = "steps";
    private const string XKey = "x";
    private const string YKey = "y";
    private const string WidthKey = "width";
    private const string HeightKey = "height";
    private const string SideKey = "side";
    private const string DepthKey = "depth";
    private const string ShiftKey = "shift";
    private const string ShadeKey = "shade";
    private const string CellKey = "cell";
    private const string AmountKey = "amount";

    /// <summary>The most fine steps that a shade names from its color: one short of the next color (D-528).</summary>
    private const int ShadeLimit = Palette.ShadesPerStep - 1;

    private static readonly string[] LayerForm = [LayersKey];
    private static readonly string[] ExtendForm = [ExtendsKey, SwapKey];
    private static readonly string[] FillFields = [KindKey, ColorKey, ShadeKey, NoiseKey, SeedKey];
    private static readonly string[] EdgeFields = [KindKey, StepsKey];
    private static readonly string[] RectFields = [KindKey, XKey, YKey, WidthKey, HeightKey, ColorKey, ShadeKey, NoiseKey, SeedKey];
    private static readonly string[] BandFields = [KindKey, SideKey, DepthKey, ShiftKey];
    private static readonly string[] GrainFields = [KindKey, CellKey, AmountKey, SeedKey];
    private static readonly string[] GradientFields = [KindKey, SideKey, DepthKey, ShiftKey];
    private static readonly string[] MapFields = [KindKey, LegendKey, RowsKey];
    private static readonly string[] LegendFields = [ColorKey, ShadeKey];
    private static readonly string[] SideNames = ["top", "bottom", "left", "right"];

    /// <summary>The recipe name of one file path: the file name without the directory and the extension.</summary>
    public static string NameOf(string contentPath)
    {
        string fileName = contentPath.Substring(contentPath.LastIndexOf('/') + 1);
        int dot = fileName.LastIndexOf('.');
        return dot < 0 ? fileName : fileName.Substring(0, dot);
    }

    /// <summary>Every recipe of the given files, with each extension resolved.</summary>
    /// <param name="files">The content path and the bytes of each recipe file, in ordinal order of the path.</param>
    /// <param name="palette">The palette that each color index and ramp name must name.</param>
    /// <exception cref="ContextException">A file is not valid, it extends a recipe that does not exist or that extends another, or its swap names a ramp that is absent, of another length, or unused.</exception>
    public static IReadOnlyDictionary<string, Recipe> ReadAll(IReadOnlyList<(string Path, byte[] Bytes)> files, Palette palette)
    {
        Dictionary<string, Recipe> layered = [];
        List<(string Path, string Parent, Dictionary<int, int> Swap)> extensions = [];
        foreach ((string path, byte[] bytes) in files)
        {
            using JsonDocument document = TextureJson.Parse(path, bytes);
            JsonElement root = document.RootElement;
            if (root.TryGetProperty(LayersKey, out _))
            {
                JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, LayerForm);
                layered.Add(NameOf(path), new Recipe(path, NameOf(path), ReadLayers(path, root, palette)));
                continue;
            }

            if (!root.TryGetProperty(ExtendsKey, out _))
            {
                throw ContentError.MakeForFile(path, $"the file holds neither '{LayersKey}' nor '{ExtendsKey}', and a recipe holds one of the two (D-507)");
            }

            JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, ExtendForm);
            string parent = JsonShape.Text(path, root, TextureJson.RootName, ExtendsKey);
            extensions.Add((path, parent, ReadSwap(path, root, palette)));
        }

        Dictionary<string, Recipe> recipes = new(layered);
        foreach ((string path, string parent, Dictionary<int, int> swap) in extensions)
        {
            if (!layered.TryGetValue(parent, out Recipe? source))
            {
                string reason = IsExtension(parent, extensions)
                    ? $"names '{parent}', a recipe that extends another, and a recipe extends a recipe of layers alone"
                    : $"names '{parent}', and no recipe file under {AssetPaths.RecipeDirectory} has that name";
                throw ContentError.Make(path, ExtendsKey, reason);
            }

            recipes.Add(NameOf(path), new Recipe(path, NameOf(path), Swapped(path, source, swap, palette)));
        }

        return recipes;
    }

    /// <summary>The layers of a layer list. The first layer is the one fill or the one map, because each paints every pixel.</summary>
    private static List<RecipeLayer> ReadLayers(string path, JsonElement root, Palette palette)
    {
        JsonElement list = JsonShape.Member(path, root, TextureJson.RootName, LayersKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() == 0)
        {
            throw ContentError.Make(path, LayersKey, "is not a list that holds at least one layer");
        }

        List<RecipeLayer> layers = [];
        foreach (JsonElement item in list.EnumerateArray())
        {
            string owner = $"{LayersKey}[{Text(layers.Count)}]";
            RecipeLayer layer = ReadLayer(path, item, owner, palette);
            bool isFirst = layers.Count == 0;
            bool isBase = layer is FillLayer or MapLayer;
            if (isFirst != isBase)
            {
                throw ContentError.Make(path, KindKey, $"on '{owner}' is not valid here: the first layer is a '{FillKind}' or a '{MapKind}', and no other layer is one");
            }

            layers.Add(layer);
        }

        return layers;
    }

    /// <summary>One layer, by its kind.</summary>
    private static RecipeLayer ReadLayer(string path, JsonElement item, string owner, Palette palette)
    {
        string kind = JsonShape.Text(path, item, owner, KindKey);
        switch (kind)
        {
            case FillKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, FillFields);
                int fillColor = ReadColor(path, item, owner, palette);
                return new FillLayer(fillColor, ReadShade(path, item, owner, palette, fillColor), ReadFraction(path, item, owner, NoiseKey), ReadSeed(path, item, owner));
            case EdgeKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, EdgeFields);
                return new EdgeLayer(ReadAtLeast(path, item, owner, StepsKey, 1));
            case RectKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, RectFields);
                int rectColor = ReadColor(path, item, owner, palette);
                return new RectLayer(
                    ReadAtLeast(path, item, owner, XKey, 0),
                    ReadAtLeast(path, item, owner, YKey, 0),
                    ReadAtLeast(path, item, owner, WidthKey, 1),
                    ReadAtLeast(path, item, owner, HeightKey, 1),
                    rectColor,
                    ReadShade(path, item, owner, palette, rectColor),
                    ReadFraction(path, item, owner, NoiseKey),
                    ReadSeed(path, item, owner));
            case BandKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, BandFields);
                return new BandLayer(ReadSide(path, item, owner), ReadAtLeast(path, item, owner, DepthKey, 1), ReadShift(path, item, owner));
            case GrainKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, GrainFields);
                return new GrainLayer(ReadAtLeast(path, item, owner, CellKey, 1), ReadFineSteps(path, item, owner, AmountKey, palette, 1), ReadSeed(path, item, owner));
            case GradientKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, GradientFields);
                int shift = ReadShift(path, item, owner);
                CheckWithinLongestRamp(path, owner, ShiftKey, shift, palette);
                return new GradientLayer(ReadSide(path, item, owner), ReadAtLeast(path, item, owner, DepthKey, 1), shift);
            case MapKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, MapFields);
                return ReadMap(path, item, owner, palette);
            default:
                throw ContentError.Make(path, KindKey, $"on '{owner}' is '{kind}', and a layer kind is one of {FillKind}, {EdgeKind}, {RectKind}, {BandKind}, {GrainKind}, {GradientKind}, and {MapKind} (D-507, D-527, D-612)");
        }
    }

    /// <summary>
    /// A map layer. The legend maps each character of one letter to a color and a shade. Each row has the same length,
    /// each character of a row has a legend entry, and each legend entry appears in a row, because an unused entry
    /// hides a typing error (T-2).
    /// </summary>
    private static MapLayer ReadMap(string path, JsonElement item, string owner, Palette palette)
    {
        JsonElement legendItem = JsonShape.Member(path, item, owner, LegendKey);
        if (legendItem.ValueKind != JsonValueKind.Object || !legendItem.EnumerateObject().MoveNext())
        {
            throw ContentError.Make(path, LegendKey, $"on '{owner}' is not an object that maps at least one character to a color and a shade");
        }

        Dictionary<char, MapShade> legend = [];
        foreach (JsonProperty entry in legendItem.EnumerateObject())
        {
            string entryOwner = $"{owner}.{LegendKey}.{entry.Name}";
            if (entry.Name.Length != 1 || char.IsWhiteSpace(entry.Name[0]))
            {
                throw ContentError.Make(path, LegendKey, $"on '{owner}' names the key '{entry.Name}', and a legend key is one visible character");
            }

            if (legend.ContainsKey(entry.Name[0]))
            {
                throw ContentError.Make(path, LegendKey, $"on '{owner}' names the key '{entry.Name}' twice");
            }

            JsonShape.CheckNoUnknownMember(path, entry.Value, entryOwner, LegendFields);
            int color = ReadColor(path, entry.Value, entryOwner, palette);
            legend.Add(entry.Name[0], new MapShade(color, ReadShade(path, entry.Value, entryOwner, palette, color)));
        }

        JsonElement rowsItem = JsonShape.Member(path, item, owner, RowsKey);
        if (rowsItem.ValueKind != JsonValueKind.Array || rowsItem.GetArrayLength() == 0)
        {
            throw ContentError.Make(path, RowsKey, $"on '{owner}' is not a list that holds at least one row");
        }

        List<string> rows = [];
        HashSet<char> used = [];
        foreach (JsonElement rowItem in rowsItem.EnumerateArray())
        {
            string row = rowItem.ValueKind == JsonValueKind.String ? rowItem.GetString() ?? string.Empty : string.Empty;
            string rowOwner = $"{owner}.{RowsKey}[{Text(rows.Count)}]";
            if (row.Length == 0 || (rows.Count > 0 && row.Length != rows[0].Length))
            {
                throw ContentError.Make(path, RowsKey, $"on '{rowOwner}' is not a string of the length of the first row, {Text(rows.Count > 0 ? rows[0].Length : 0)} characters, and every row is a string of one length above zero");
            }

            for (int column = 0; column < row.Length; column++)
            {
                if (!legend.ContainsKey(row[column]))
                {
                    throw ContentError.Make(path, RowsKey, $"on '{rowOwner}' holds the character '{row[column]}' at the column {Text(column)}, and the legend has no entry for it");
                }

                used.Add(row[column]);
            }

            rows.Add(row);
        }

        foreach (char key in legend.Keys)
        {
            if (!used.Contains(key))
            {
                throw ContentError.Make(path, LegendKey, $"on '{owner}' names the key '{key}', and no row uses it");
            }
        }

        return new MapLayer(rows, legend);
    }

    /// <summary>The swap of a recipe that extends another: from the flat index of each color of a swapped ramp to the index of the same step on the other ramp.</summary>
    private static Dictionary<int, int> ReadSwap(string path, JsonElement root, Palette palette)
    {
        JsonElement swap = JsonShape.Member(path, root, TextureJson.RootName, SwapKey);
        if (swap.ValueKind != JsonValueKind.Object || !swap.EnumerateObject().MoveNext())
        {
            throw ContentError.Make(path, SwapKey, "is not an object that maps at least one ramp name to another");
        }

        Dictionary<int, int> map = [];
        foreach (JsonProperty pair in swap.EnumerateObject())
        {
            PaletteRamp from = RampNamed(path, palette, pair.Name);
            string target = pair.Value.ValueKind == JsonValueKind.String ? pair.Value.GetString() ?? string.Empty : string.Empty;
            PaletteRamp to = RampNamed(path, palette, target);
            if (from.Count != to.Count)
            {
                throw ContentError.Make(path, SwapKey, $"maps the ramp '{from.Name}' of {Text(from.Count)} colors to the ramp '{to.Name}' of {Text(to.Count)}, and a swap keeps the step of each color");
            }

            if (map.ContainsKey(from.First))
            {
                throw ContentError.Make(path, SwapKey, $"names the ramp '{from.Name}' twice, and a swap maps each ramp once");
            }

            for (int step = 0; step < from.Count; step++)
            {
                map.Add(from.First + step, to.First + step);
            }
        }

        return map;
    }

    /// <summary>The layers of the source recipe with each color swapped. A swapped ramp that no layer paints is an error, because the swap then does nothing (T-2).</summary>
    private static List<RecipeLayer> Swapped(string path, Recipe source, Dictionary<int, int> swap, Palette palette)
    {
        HashSet<int> used = [];
        List<RecipeLayer> layers = [];
        foreach (RecipeLayer layer in source.Layers)
        {
            switch (layer)
            {
                case FillLayer fill:
                    used.Add(palette.Colors[fill.Color].Ramp);
                    layers.Add(fill with { Color = swap.GetValueOrDefault(fill.Color, fill.Color) });
                    break;
                case RectLayer rect:
                    used.Add(palette.Colors[rect.Color].Ramp);
                    layers.Add(rect with { Color = swap.GetValueOrDefault(rect.Color, rect.Color) });
                    break;
                case MapLayer map:
                    Dictionary<char, MapShade> legend = [];
                    foreach ((char key, MapShade shade) in map.Legend)
                    {
                        used.Add(palette.Colors[shade.Color].Ramp);
                        legend.Add(key, shade with { Color = swap.GetValueOrDefault(shade.Color, shade.Color) });
                    }

                    layers.Add(map with { Legend = legend });
                    break;
                default:
                    layers.Add(layer);
                    break;
            }
        }

        foreach (int color in swap.Keys)
        {
            int ramp = palette.Colors[color].Ramp;
            if (!used.Contains(ramp))
            {
                throw ContentError.Make(path, SwapKey, $"names the ramp '{palette.Ramps[ramp].Name}', and the recipe '{source.Name}' paints no color of it");
            }
        }

        return layers;
    }

    private static bool IsExtension(string name, List<(string Path, string Parent, Dictionary<int, int> Swap)> extensions)
    {
        foreach ((string path, _, _) in extensions)
        {
            if (NameOf(path) == name)
            {
                return true;
            }
        }

        return false;
    }

    private static PaletteRamp RampNamed(string path, Palette palette, string name)
    {
        foreach (PaletteRamp ramp in palette.Ramps)
        {
            if (ramp.Name == name)
            {
                return ramp;
            }
        }

        throw ContentError.Make(path, SwapKey, $"names the ramp '{name}', and the palette has no ramp of that name");
    }

    private static int ReadColor(string path, JsonElement item, string owner, Palette palette)
    {
        int color = JsonShape.WholeNumber(path, item, owner, ColorKey);
        if (color < 0 || color >= palette.Colors.Count)
        {
            throw ContentError.Make(path, ColorKey, $"on '{owner}' names the color index {Text(color)}, and the palette holds {Text(palette.Colors.Count)} colors at the indices 0 to {Text(palette.Colors.Count - 1)}");
        }

        return color;
    }

    /// <summary>The shade member: fine steps from the color, from -3 to 3, that keep the pixel on the ramp of the color (D-527, D-528).</summary>
    private static int ReadShade(string path, JsonElement item, string owner, Palette palette, int color)
    {
        int shade = JsonShape.WholeNumber(path, item, owner, ShadeKey);
        if (shade < -ShadeLimit || shade > ShadeLimit)
        {
            throw ContentError.Make(path, ShadeKey, $"on '{owner}' is {Text(shade)}, and a shade is from {Text(-ShadeLimit)} to {Text(ShadeLimit)} fine steps (D-528)");
        }

        PaletteColor named = palette.Colors[color];
        int fineStep = (named.Step * Palette.ShadesPerStep) + shade;
        if (fineStep < 0 || fineStep > palette.FineTop(named.Ramp))
        {
            throw ContentError.Make(path, ShadeKey, $"on '{owner}' is {Text(shade)}, and it moves the color {Text(color)} off the end of the ramp '{palette.Ramps[named.Ramp].Name}'");
        }

        return shade;
    }

    /// <summary>A whole count of fine steps from 1 to the fine steps of the longest ramp.</summary>
    private static int ReadFineSteps(string path, JsonElement item, string owner, string name, Palette palette, int least)
    {
        int value = ReadAtLeast(path, item, owner, name, least);
        CheckWithinLongestRamp(path, owner, name, value, palette);
        return value;
    }

    /// <summary>A move of more fine steps than the longest ramp passes the end of every ramp, so it is an error.</summary>
    private static void CheckWithinLongestRamp(string path, string owner, string name, int fineSteps, Palette palette)
    {
        int most = LongestRamp(palette);
        if (fineSteps < -most || fineSteps > most)
        {
            throw ContentError.Make(path, name, $"on '{owner}' is {Text(fineSteps)}, and a move passes no more than {Text(most)} fine steps, the length of the longest ramp");
        }
    }

    /// <summary>The fine steps of the longest ramp of the palette.</summary>
    private static int LongestRamp(Palette palette)
    {
        int longest = 0;
        for (int ramp = 0; ramp < palette.Ramps.Count; ramp++)
        {
            longest = System.Math.Max(longest, palette.FineTop(ramp));
        }

        return longest;
    }

    /// <summary>A number member from 0 to 1, as a double, so the value in the file is the value that the painter compares.</summary>
    private static double ReadFraction(string path, JsonElement item, string owner, string name)
    {
        JsonElement value = JsonShape.Member(path, item, owner, name);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double number) || !double.IsFinite(number) || number < 0.0 || number > 1.0)
        {
            throw ContentError.Make(path, name, $"on '{owner}' is not a number from 0 to 1");
        }

        return number;
    }

    /// <summary>The seed member: a whole number that 32 bits hold, and not zero, because a zero seed never leaves zero.</summary>
    private static uint ReadSeed(string path, JsonElement item, string owner)
    {
        JsonElement value = JsonShape.Member(path, item, owner, SeedKey);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetUInt32(out uint seed) || seed == 0)
        {
            throw ContentError.Make(path, SeedKey, $"on '{owner}' is not a whole number from 1 to 4294967295, and a zero seed never leaves zero");
        }

        return seed;
    }

    private static int ReadAtLeast(string path, JsonElement item, string owner, string name, int least)
    {
        int value = JsonShape.WholeNumber(path, item, owner, name);
        if (value < least)
        {
            throw ContentError.Make(path, name, $"on '{owner}' is {Text(value)}, and it is {Text(least)} or more");
        }

        return value;
    }

    private static int ReadShift(string path, JsonElement item, string owner)
    {
        int shift = JsonShape.WholeNumber(path, item, owner, ShiftKey);
        if (shift == 0)
        {
            throw ContentError.Make(path, ShiftKey, $"on '{owner}' is 0, and a layer of no shift paints nothing");
        }

        return shift;
    }

    private static CanvasSide ReadSide(string path, JsonElement item, string owner)
    {
        string side = JsonShape.Text(path, item, owner, SideKey);
        int index = System.Array.IndexOf(SideNames, side);
        if (index < 0)
        {
            throw ContentError.Make(path, SideKey, $"on '{owner}' is '{side}', and a side is one of {string.Join(", ", SideNames)}");
        }

        return (CanvasSide)index;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
