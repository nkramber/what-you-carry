using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>One paint layer of a recipe (D-507). <see cref="CanvasPainter"/> applies the layers in file order.</summary>
public abstract record RecipeLayer;

/// <summary>The first layer of every recipe: each pixel takes the color, and the noise moves it one step on its ramp.</summary>
/// <param name="Color">The flat palette index of the color.</param>
/// <param name="Noise">The chance, from 0 to 1, that a pixel moves one step. Half of the chance moves it down, and the other half moves it up.</param>
/// <param name="Seed">The seed of the noise, not zero.</param>
public sealed record FillLayer(int Color, double Noise, uint Seed) : RecipeLayer;

/// <summary>The outer ring of pixels of the canvas moves down its ramp by a count of steps.</summary>
public sealed record EdgeLayer(int Steps) : RecipeLayer;

/// <summary>A rectangle of the canvas, from its top left corner, takes a color with its own noise. The canvas clips it.</summary>
public sealed record RectLayer(int X, int Y, int Width, int Height, int Color, double Noise, uint Seed) : RecipeLayer;

/// <summary>The pixels within a depth of one side of the canvas move up or down their ramp by a shift in steps.</summary>
public sealed record BandLayer(CanvasSide Side, int Depth, int Shift) : RecipeLayer;

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
/// <param name="Layers">The layers in paint order. The first layer is the one fill.</param>
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

    private static readonly string[] LayerForm = [LayersKey];
    private static readonly string[] ExtendForm = [ExtendsKey, SwapKey];
    private static readonly string[] FillFields = [KindKey, ColorKey, NoiseKey, SeedKey];
    private static readonly string[] EdgeFields = [KindKey, StepsKey];
    private static readonly string[] RectFields = [KindKey, XKey, YKey, WidthKey, HeightKey, ColorKey, NoiseKey, SeedKey];
    private static readonly string[] BandFields = [KindKey, SideKey, DepthKey, ShiftKey];
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

    /// <summary>The layers of a layer list. The first layer is the one fill.</summary>
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
            bool isFill = layer is FillLayer;
            if (isFirst != isFill)
            {
                throw ContentError.Make(path, KindKey, $"on '{owner}' is not valid here: the first layer is a '{FillKind}', and no other layer is one");
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
                return new FillLayer(ReadColor(path, item, owner, palette), ReadFraction(path, item, owner, NoiseKey), ReadSeed(path, item, owner));
            case EdgeKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, EdgeFields);
                return new EdgeLayer(ReadAtLeast(path, item, owner, StepsKey, 1));
            case RectKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, RectFields);
                return new RectLayer(
                    ReadAtLeast(path, item, owner, XKey, 0),
                    ReadAtLeast(path, item, owner, YKey, 0),
                    ReadAtLeast(path, item, owner, WidthKey, 1),
                    ReadAtLeast(path, item, owner, HeightKey, 1),
                    ReadColor(path, item, owner, palette),
                    ReadFraction(path, item, owner, NoiseKey),
                    ReadSeed(path, item, owner));
            case BandKind:
                JsonShape.CheckNoUnknownMember(path, item, owner, BandFields);
                return new BandLayer(ReadSide(path, item, owner), ReadAtLeast(path, item, owner, DepthKey, 1), ReadShift(path, item, owner));
            default:
                throw ContentError.Make(path, KindKey, $"on '{owner}' is '{kind}', and a layer kind is one of {FillKind}, {EdgeKind}, {RectKind}, and {BandKind} (D-507)");
        }
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
            throw ContentError.Make(path, ShiftKey, $"on '{owner}' is 0, and a band of no shift paints nothing");
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
