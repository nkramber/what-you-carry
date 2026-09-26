using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.TextureGen;
using WhatYouCarry.Tools.TextureTrace;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The texel maps of D-612: the <c>map</c> layer of a recipe, the screenshot reader, the trace of a face, the trace
/// spec, and the <c>texture-trace</c> command.
/// </summary>
public sealed class TextureTraceTests
{
    private const string RecipePath = "textures/recipes/test.json";
    private const int Rust = 4;
    private const int Bone = 7;
    private const int Umber = 8;
    private const int Steel = 9;

    /// <summary>A map paints each texel with the color and the shade of its legend character, and nothing else.</summary>
    [Fact]
    public void MapPaintsEachTexelFromItsLegend()
    {
        Palette palette = RepositoryPalette();
        Recipe recipe = ReadOne("{\"layers\": [{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}, \"b\": {\"color\": 29, \"shade\": -2}}, \"rows\": [\"aab\", \"bba\"]}]}", palette);

        AtlasColor[] pixels = CanvasPainter.Paint(palette, recipe, 3, 2, CanvasPainter.SaltOf("face"), "face");

        AtlasColor rust = palette.AtlasColors[17];
        AtlasColor skin = palette.AtlasColors[palette.AtlasIndex(Bone, (1 * Palette.ShadesPerStep) - 2)];
        Assert.Equal([rust, rust, skin, skin, skin, rust], pixels);
    }

    /// <summary>A layer after a map changes the map, as it changes a fill, and a swap moves the colors of the legend (D-507).</summary>
    [Fact]
    public void LayersAndSwapsWorkOnAMap()
    {
        Palette palette = RepositoryPalette();
        string map = "{\"layers\": [{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}, \"rows\": [\"aa\", \"aa\"]}, {\"kind\": \"edge\", \"steps\": 1}]}";
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll([("textures/recipes/shirt.json", Bytes(map)), ("textures/recipes/moss-shirt.json", Bytes("{\"extends\": \"shirt\", \"swap\": {\"rust\": \"moss\"}}"))], palette);

        AtlasColor[] shirt = CanvasPainter.Paint(palette, recipes["shirt"], 2, 2, 1, "shirt");
        AtlasColor[] moss = CanvasPainter.Paint(palette, recipes["moss-shirt"], 2, 2, 1, "moss");

        Assert.All(shirt, pixel => Assert.Equal(palette.AtlasColors[16], pixel));
        Assert.All(moss, pixel => Assert.Equal(palette.AtlasColors[64], pixel));
    }

    /// <summary>A map of another size than its canvas is an error that names the recipe and the canvas (T-2).</summary>
    [Fact]
    public void MapOfAnotherSizeIsAnError()
    {
        Palette palette = RepositoryPalette();
        Recipe recipe = ReadOne("{\"layers\": [{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}, \"rows\": [\"aa\", \"aa\"]}]}", palette);

        ContextException error = Assert.Throws<ContextException>(() => CanvasPainter.Paint(palette, recipe, 3, 2, 1, "models/rig.bbmodel:arm:north"));

        Assert.Contains(RecipePath, error.Message, StringComparison.Ordinal);
        Assert.Contains("models/rig.bbmodel:arm:north", error.Message, StringComparison.Ordinal);
        Assert.Contains("a map of 2 by 2 texels", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A bad map is an error that names the file and the field (D-92, D-168, D-612).</summary>
    [Theory]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}, \"rows\": [\"ab\"]}", "rows", "the legend has no entry for it")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}, \"b\": {\"color\": 18, \"shade\": 0}}, \"rows\": [\"aa\"]}", "legend", "no row uses it")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"ab\": {\"color\": 17, \"shade\": 0}}, \"rows\": [\"a\"]}", "legend", "one visible character")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}, \"rows\": [\"aa\", \"a\"]}", "rows", "the length of the first row")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}, \"rows\": []}", "rows", "at least one row")]
    [InlineData("{\"kind\": \"map\", \"legend\": {}, \"rows\": [\"a\"]}", "legend", "at least one character")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17}}, \"rows\": [\"a\"]}", "shade", "is absent")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 16, \"shade\": -1}}, \"rows\": [\"a\"]}", "shade", "off the end of the ramp")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0, \"noise\": 0}}, \"rows\": [\"a\"]}", "noise", "not a field of the format")]
    [InlineData("{\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}}", "rows", "is absent")]
    public void MapRejectsABadLayer(string layer, string field, string reason)
    {
        ContextException error = Assert.Throws<ContextException>(() => ReadOne("{\"layers\": [" + layer + "]}", RepositoryPalette()));

        Assert.Contains(RecipePath, error.Message, StringComparison.Ordinal);
        Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A map paints every pixel, as a fill does, so a map after the first layer is an error.</summary>
    [Fact]
    public void MapAfterAFillIsAnError()
    {
        string json = "{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0, \"seed\": 3}, {\"kind\": \"map\", \"legend\": {\"a\": {\"color\": 17, \"shade\": 0}}, \"rows\": [\"a\"]}]}";

        ContextException error = Assert.Throws<ContextException>(() => ReadOne(json, RepositoryPalette()));

        Assert.Contains("the first layer is a 'fill' or a 'map', and no other layer is one", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The reader undoes each of the five row filters, skips an ancillary chunk, and reads RGB and RGBA.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ScreenshotReaderUndoesEachFilter(bool alpha)
    {
        const int Width = 3;
        const int Height = 5;
        ScreenshotPixel[] expected = new ScreenshotPixel[Width * Height];
        for (int pixel = 0; pixel < expected.Length; pixel++)
        {
            expected[pixel] = new ScreenshotPixel((byte)(pixel * 37), (byte)(255 - (pixel * 11)), (byte)(pixel * pixel * 7), alpha ? (byte)(200 + pixel) : byte.MaxValue);
        }

        byte[] file = Png(Width, Height, expected, alpha, [0, 1, 2, 3, 4], withText: true);

        ScreenshotImage image = ScreenshotPng.Read("shot.png", file);

        Assert.Equal(Width, image.Width);
        Assert.Equal(Height, image.Height);
        Assert.Equal(expected, image.Pixels);
    }

    /// <summary>A screenshot of a form that the trace does not read is an error that names the file and the form.</summary>
    [Theory]
    [InlineData("depth", "the bit depth 16")]
    [InlineData("critical", "the critical chunk 'XBAD'")]
    [InlineData("checksum", "stores the checksum")]
    [InlineData("filter", "the filter type 5")]
    public void ScreenshotReaderRejectsAnotherForm(string fault, string reason)
    {
        ScreenshotPixel[] pixels = [new(1, 2, 3, 255), new(4, 5, 6, 255)];
        byte[] file = fault switch
        {
            "depth" => Png(2, 1, pixels, alpha: false, [0], bitDepth: 16),
            "critical" => Png(2, 1, pixels, alpha: false, [0], extraChunk: "XBAD"),
            "filter" => Png(2, 1, pixels, alpha: false, [5]),
            _ => BreakFirstChecksum(Png(2, 1, pixels, alpha: false, [0])),
        };

        ContextException error = Assert.Throws<ContextException>(() => ScreenshotPng.Read("shot.png", file));

        Assert.Contains("'shot.png'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>Each texel takes the fine shade of the named ramps at the least distance, and the legend gives out characters in the order of first use.</summary>
    [Fact]
    public void TraceSnapsEachTexelToItsShade()
    {
        Palette palette = RepositoryPalette();
        AtlasColor shirt = palette.AtlasColors[palette.AtlasIndex(Rust, 5)];
        AtlasColor skin = palette.AtlasColors[29];
        ScreenshotImage image = Image(8, 4, (x, _) => x < 4 ? shirt : skin);

        MapLayer map = TextureTracer.Trace(image, "shot.png", Face(new ImageArea(0, 0, 8, 4), 0, Rust, Bone), 4, 2, palette);

        Assert.Equal(["0011", "0011"], map.Rows);
        Assert.Equal(new MapShade(17, 1), map.Legend['0']);
        Assert.Equal(new MapShade(29, 0), map.Legend['1']);
    }

    /// <summary>A texel reads the middle half of its cell, so a dark line between the texels of the reference stays out of the map.</summary>
    [Fact]
    public void TraceReadsTheMiddleOfEachCell()
    {
        Palette palette = RepositoryPalette();
        AtlasColor shirt = palette.AtlasColors[17];
        AtlasColor line = palette.AtlasColors[0];
        ScreenshotImage image = Image(16, 8, (x, y) => x % 4 == 0 || x % 4 == 3 || y % 4 == 0 || y % 4 == 3 ? line : shirt);

        MapLayer map = TextureTracer.Trace(image, "shot.png", Face(new ImageArea(0, 0, 16, 8), 0, Rust, 0), 4, 2, palette);

        Assert.Equal(["0000", "0000"], map.Rows);
        Assert.Equal(new MapShade(17, 0), map.Legend['0']);
    }

    /// <summary>A turn of one quarter clockwise brings the lower left corner of the area to the upper left of the canvas.</summary>
    [Fact]
    public void TraceTurnsTheArea()
    {
        Palette palette = RepositoryPalette();
        AtlasColor[] corners = [palette.AtlasColors[17], palette.AtlasColors[29], palette.AtlasColors[33], palette.AtlasColors[37]];
        ScreenshotImage image = Image(4, 4, (x, y) => corners[(y < 2 ? 0 : 2) + (x < 2 ? 0 : 1)]);

        MapLayer map = TextureTracer.Trace(image, "shot.png", Face(new ImageArea(0, 0, 4, 4), 1, Rust, Bone, Umber, Steel), 2, 2, palette);

        MapShade[] texels = [.. map.Rows.SelectMany(row => row).Select(key => map.Legend[key])];
        Assert.Equal([new MapShade(33, 0), new MapShade(17, 0), new MapShade(37, 0), new MapShade(29, 0)], texels);
    }

    /// <summary>A sample of a pixel that is not fully opaque, and an area past the edge, are errors that name the face and the screenshot (T-2).</summary>
    [Fact]
    public void TraceRejectsABadArea()
    {
        Palette palette = RepositoryPalette();
        ScreenshotImage image = new(2, 2, [new(1, 1, 1, 255), new(1, 1, 1, 255), new(1, 1, 1, 0), new(1, 1, 1, 255)]);

        ContextException clear = Assert.Throws<ContextException>(() => TextureTracer.Trace(image, "shot.png", Face(new ImageArea(0, 0, 2, 2), 0, Rust), 2, 2, palette));
        ContextException past = Assert.Throws<ContextException>(() => TextureTracer.Trace(image, "shot.png", Face(new ImageArea(1, 0, 2, 2), 0, Rust), 2, 2, palette));

        Assert.Contains("not fully opaque", clear.Message, StringComparison.Ordinal);
        Assert.Contains("models/player.bbmodel:head_box:north", clear.Message, StringComparison.Ordinal);
        Assert.Contains("passes the edge of the screenshot 'shot.png'", past.Message, StringComparison.Ordinal);
    }

    /// <summary>A face that needs more shades than the legend has characters is an error, and not a map that reuses a character.</summary>
    [Fact]
    public void TraceRejectsTooManyShades()
    {
        Palette palette = RepositoryPalette();
        List<AtlasColor> shades = [];
        for (int ramp = 0; ramp < 6; ramp++)
        {
            for (int fine = 0; fine <= palette.FineTop(ramp); fine++)
            {
                shades.Add(palette.AtlasColors[palette.AtlasIndex(ramp, fine)]);
            }
        }

        ScreenshotImage image = Image(shades.Count, 1, (x, _) => shades[x]);

        ContextException error = Assert.Throws<ContextException>(() => TextureTracer.Trace(image, "shot.png", Face(new ImageArea(0, 0, shades.Count, 1), 0, 0, 1, 2, 3, 4, 5), shades.Count, 1, palette));

        Assert.Contains("more than 62 shades", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The recipe text of a map reads back as the same map.</summary>
    [Fact]
    public void RecipeTextReadsBack()
    {
        Palette palette = RepositoryPalette();
        MapLayer map = new(["01", "10"], new Dictionary<char, MapShade> { ['0'] = new(17, 1), ['1'] = new(29, -2) });

        Recipe recipe = ReadOne(TextureTracer.RecipeText(map), palette);

        MapLayer read = Assert.IsType<MapLayer>(Assert.Single(recipe.Layers));
        Assert.Equal(map.Rows, read.Rows);
        Assert.Equal(map.Legend.OrderBy(entry => entry.Key), read.Legend.OrderBy(entry => entry.Key));
    }

    /// <summary>The OKLab of white and of sRGB red match the values that Ottosson gives.</summary>
    [Fact]
    public void OkLabMatchesTheReference()
    {
        OkLab white = OkLab.FromLinear(1.0, 1.0, 1.0);
        OkLab red = OkLab.FromLinear(1.0, 0.0, 0.0);

        Assert.Equal(1.0, white.L, 3);
        Assert.Equal(0.0, white.A, 3);
        Assert.Equal(0.0, white.B, 3);
        Assert.Equal(0.628, red.L, 3);
        Assert.Equal(0.225, red.A, 3);
        Assert.Equal(0.126, red.B, 3);
    }

    /// <summary>A bad trace spec is an error that names the file and the field (D-612).</summary>
    [Theory]
    [InlineData("\"image\": \"side\"", "image", "no key of that name")]
    [InlineData("\"face\": \"top\"", "face", "a face is one of")]
    [InlineData("\"recipe\": \"Head Face\"", "recipe", "lowercase letters, digits, and hyphens")]
    [InlineData("\"turn\": 4", "turn", "0 to 3 quarter turns")]
    [InlineData("\"ramps\": [\"rust\", \"rust\"]", "ramps", "twice")]
    [InlineData("\"ramps\": [\"glow\"]", "ramps", "no ramp of that name")]
    [InlineData("\"at\": [0, 0, 0, 4]", "at", "a size of 1 or more")]
    [InlineData("\"at\": [0.5, 0, 4, 4]", "at", "whole pixels")]
    public void SpecRejectsABadFace(string change, string field, string reason)
    {
        string name = change.Substring(1, change.IndexOf('"', 1) - 1);
        Dictionary<string, string> fields = new()
        {
            ["model"] = "\"model\": \"models/player.bbmodel\"",
            ["box"] = "\"box\": \"head_box\"",
            ["face"] = "\"face\": \"north\"",
            ["recipe"] = "\"recipe\": \"head-front\"",
            ["image"] = "\"image\": \"front\"",
            ["at"] = "\"at\": [0, 0, 4, 4]",
            ["turn"] = "\"turn\": 0",
            ["ramps"] = "\"ramps\": [\"bone\"]",
        };
        fields[name] = change;
        string spec = "{\"images\": {\"front\": \"shots/front.png\"}, \"faces\": [{" + string.Join(", ", fields.Values) + "}]}";

        ContextException error = Assert.Throws<ContextException>(() => TraceSpecFile.Parse("textures/traces/test.json", Bytes(spec), RepositoryPalette()));

        Assert.Contains("textures/traces/test.json", error.Message, StringComparison.Ordinal);
        Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The command traces each face of the spec at the size of its canvas and writes its recipe. A second run keeps the
    /// recipe that exists, so a hand edit of a map survives it.
    /// </summary>
    [Fact]
    public void CommandWritesEachAbsentRecipeAndKeepsTheRest()
    {
        string root = Path.Combine(Path.GetTempPath(), $"wyc-trace-{Guid.NewGuid():N}");
        try
        {
            string content = Path.Combine(root, "content");
            Directory.CreateDirectory(Path.Combine(content, AssetPaths.RecipeDirectory));
            Directory.CreateDirectory(Path.Combine(content, AssetPaths.TraceDirectory));
            Directory.CreateDirectory(Path.Combine(content, AssetPaths.ModelDirectory));
            Directory.CreateDirectory(Path.Combine(root, "shots"));
            string repository = Path.Combine(RepositoryRoot.Find(), "content");
            File.Copy(Path.Combine(repository, AssetPaths.PaletteFile), Path.Combine(content, AssetPaths.PaletteFile));
            File.Copy(Path.Combine(repository, AssetPaths.BodyModel), Path.Combine(content, AssetPaths.BodyModel));
            AtlasColor skin = RepositoryPalette().AtlasColors[29];
            ScreenshotPixel[] pixels = [.. Enumerable.Repeat(new ScreenshotPixel(skin.Red, skin.Green, skin.Blue, 255), 64 * 64)];
            File.WriteAllBytes(Path.Combine(root, "shots", "front.png"), Png(64, 64, pixels, alpha: false, [.. Enumerable.Repeat((byte)2, 64)]));
            string spec = "{\"images\": {\"front\": \"shots/front.png\"}, \"faces\": [{\"model\": \"models/player.bbmodel\", \"box\": \"head_box\", \"face\": \"north\", \"recipe\": \"head-front\", \"image\": \"front\", \"at\": [0, 0, 64, 64], \"turn\": 0, \"ramps\": [\"bone\"]}]}";
            File.WriteAllText(Path.Combine(content, AssetPaths.TraceDirectory, "test.json"), spec);

            (int written, int kept) = TextureTraceCommand.Trace(root, "test");
            (int writtenAgain, int keptAgain) = TextureTraceCommand.Trace(root, "test");

            Assert.Equal((1, 0), (written, kept));
            Assert.Equal((0, 1), (writtenAgain, keptAgain));
            Recipe recipe = ReadOne(File.ReadAllText(Path.Combine(content, AssetPaths.RecipeDirectory, "head-front.json")), RepositoryPalette());
            MapLayer map = Assert.IsType<MapLayer>(Assert.Single(recipe.Layers));
            Assert.Equal(32, map.Rows.Count);
            Assert.All(map.Rows, row => Assert.Equal(new string('0', 32), row));
            Assert.Equal(new MapShade(29, 0), map.Legend['0']);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>A command line without both options is exit code 2.</summary>
    [Fact]
    public void CommandNeedsBothOptions()
    {
        Assert.Equal(2, TextureTraceCommand.Run(["--root", "."]));
        Assert.Equal(2, TextureTraceCommand.Run(["--spec", "miner", "--glow"]));
    }

    private static TraceFace Face(ImageArea at, int turn, params int[] ramps)
    {
        return new TraceFace(AssetPaths.BodyModel, "head_box", BoxSide.North, "head-front", "front", at, turn, ramps);
    }

    private static ScreenshotImage Image(int width, int height, Func<int, int, AtlasColor> colorAt)
    {
        ScreenshotPixel[] pixels = new ScreenshotPixel[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                AtlasColor color = colorAt(x, y);
                pixels[(y * width) + x] = new ScreenshotPixel(color.Red, color.Green, color.Blue, 255);
            }
        }

        return new ScreenshotImage(width, height, pixels);
    }

    /// <summary>
    /// A PNG with the given filter on each row, as a screenshot tool writes it. The filter bytes follow PNG section 9.2
    /// from the pixels, so the reader must undo them to get the pixels back.
    /// </summary>
    private static byte[] Png(int width, int height, ScreenshotPixel[] pixels, bool alpha, byte[] filters, bool withText = false, string? extraChunk = null, byte bitDepth = 8)
    {
        int bytesPerPixel = alpha ? 4 : 3;
        int rowBytes = width * bytesPerPixel;
        byte[] raw = new byte[rowBytes * height];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            raw[pixel * bytesPerPixel] = pixels[pixel].Red;
            raw[(pixel * bytesPerPixel) + 1] = pixels[pixel].Green;
            raw[(pixel * bytesPerPixel) + 2] = pixels[pixel].Blue;
            if (alpha)
            {
                raw[(pixel * bytesPerPixel) + 3] = pixels[pixel].Alpha;
            }
        }

        using MemoryStream data = new();
        for (int row = 0; row < height; row++)
        {
            byte filter = filters[row % filters.Length];
            data.WriteByte(filter);
            for (int index = 0; index < rowBytes; index++)
            {
                int left = index >= bytesPerPixel ? raw[(row * rowBytes) + index - bytesPerPixel] : 0;
                int up = row > 0 ? raw[((row - 1) * rowBytes) + index] : 0;
                int upLeft = row > 0 && index >= bytesPerPixel ? raw[((row - 1) * rowBytes) + index - bytesPerPixel] : 0;
                int predictor = filter switch
                {
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => PaethOf(left, up, upLeft),
                    _ => 0,
                };
                data.WriteByte((byte)(raw[(row * rowBytes) + index] - predictor));
            }
        }

        using MemoryStream compressed = new();
        using (ZLibStream deflater = new(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflater.Write(data.ToArray());
        }

        using MemoryStream file = new();
        file.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        byte[] header = new byte[13];
        WriteInt(header, 0, width);
        WriteInt(header, 4, height);
        header[8] = bitDepth;
        header[9] = alpha ? (byte)6 : (byte)2;
        Chunk(file, "IHDR", header);
        if (withText)
        {
            Chunk(file, "tEXt", Encoding.ASCII.GetBytes("Software\0Screenshot"));
        }

        if (extraChunk is not null)
        {
            Chunk(file, extraChunk, [1, 2, 3]);
        }

        Chunk(file, "IDAT", compressed.ToArray());
        Chunk(file, "IEND", []);
        return file.ToArray();
    }

    private static int PaethOf(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int toLeft = Math.Abs(estimate - left);
        int toUp = Math.Abs(estimate - up);
        int toUpLeft = Math.Abs(estimate - upLeft);
        if (toLeft <= toUp && toLeft <= toUpLeft)
        {
            return left;
        }

        return toUp <= toUpLeft ? up : upLeft;
    }

    private static void Chunk(MemoryStream file, string type, byte[] data)
    {
        byte[] body = [.. Encoding.ASCII.GetBytes(type), .. data];
        byte[] length = new byte[4];
        WriteInt(length, 0, data.Length);
        byte[] checksum = new byte[4];
        WriteInt(checksum, 0, (int)Crc32.Of(body, 0, body.Length));
        file.Write(length);
        file.Write(body);
        file.Write(checksum);
    }

    private static byte[] BreakFirstChecksum(byte[] file)
    {
        // The header chunk ends at byte 33, and its last four bytes are its checksum.
        file[32] ^= 0xFF;
        return file;
    }

    private static void WriteInt(byte[] bytes, int offset, int value)
    {
        bytes[offset] = (byte)(value >> 24);
        bytes[offset + 1] = (byte)(value >> 16);
        bytes[offset + 2] = (byte)(value >> 8);
        bytes[offset + 3] = (byte)value;
    }

    private static Recipe ReadOne(string json, Palette palette)
    {
        return RecipeFile.ReadAll([(RecipePath, Bytes(json))], palette)["test"];
    }

    private static byte[] Bytes(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    private static Palette RepositoryPalette()
    {
        return Palette.Parse(AssetPaths.PaletteFile, File.ReadAllBytes(Path.Combine(RepositoryRoot.Find(), "content", AssetPaths.PaletteFile)));
    }
}
