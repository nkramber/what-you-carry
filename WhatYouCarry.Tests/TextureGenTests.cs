using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using WhatYouCarry.Tools.TextureGen;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The texture generator of PR-14: the palette, the rules, the atlas, the PNG file, the command, and the body faces
/// (D-85, D-304, D-305, D-307, D-308; PR-14 exit tests 1 to 4).
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class TextureGenTests
{
    private const string BadRule = "textures/rules/bad.json";
    private const float UvTolerance = 0.001f;

    /// <summary>The ramp names of the palette of D-304, in file order.</summary>
    private static readonly string[] OwnerRampNames = ["rock", "slate", "timber", "ochre", "rust", "water", "lichen", "bone"];

    /// <summary>The 32 colors of the palette of D-304, ramp by ramp from dark to light.</summary>
    private static readonly string[] OwnerColors =
    [
        "#14161b", "#2a2e33", "#4e4a43", "#746e65",
        "#10141d", "#222c3a", "#384a5a", "#576f80",
        "#281510", "#462719", "#694328", "#906840",
        "#603714", "#905c16", "#bf8a2a", "#e6c465",
        "#340f11", "#5d1c19", "#8a3122", "#b85734",
        "#04141c", "#082b34", "#144b51", "#317272",
        "#111b10", "#28351e", "#46532e", "#6f7746",
        "#775d50", "#9f836f", "#c0aa92", "#e1d6c2",
    ];

    /// <summary>PR-14 exit test 1. The palette chunk of the atlas is the palette, and every pixel names an index inside it.</summary>
    [Fact]
    public void GeneratorUsesPaletteOnly()
    {
        Palette palette = RepositoryPalette();
        PngImage image = PngReader.Read(TextureGenCommand.AtlasBytes(ContentRoot()));

        byte[] expected = palette.Colors.SelectMany(color => new[] { color.Red, color.Green, color.Blue }).ToArray();
        Assert.Equal(expected, image.PaletteBytes);
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++)
        {
            Assert.True(image.Pixels[pixel] < palette.Colors.Count, $"The pixel {pixel} names the index {image.Pixels[pixel]}, and the palette holds {palette.Colors.Count} colors.");
        }
    }

    /// <summary>PR-14 exit test 2. Two runs of the generator give equal bytes.</summary>
    [Fact]
    public void GeneratorIsDeterministic()
    {
        byte[] first = TextureGenCommand.AtlasBytes(ContentRoot());
        byte[] second = TextureGenCommand.AtlasBytes(ContentRoot());

        Assert.Equal(first, second);
    }

    /// <summary>The committed atlas equals the generator output, so a palette or rule change without a new atlas fails here (D-305).</summary>
    [Fact]
    public void CommittedAtlasMatchesTheGenerator()
    {
        byte[] committed = File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage));
        byte[] generated = TextureGenCommand.AtlasBytes(ContentRoot());

        Assert.True(committed.SequenceEqual(generated), "The committed atlas differs from the generator output. Run the texture-gen command with --root on the checkout, and commit textures/atlas.png (D-305).");
    }

    /// <summary>PR-14 exit test 3. A rule that names a color index past the palette fails, and the error names the index and the file.</summary>
    [Fact]
    public void RuleWithUnknownColorFails()
    {
        ContextException error = Assert.Throws<ContextException>(() => TextureRule.Parse(BadRule, RuleBytes(baseIndex: "32"), RepositoryPalette()));

        Assert.Contains(BadRule, error.Message, StringComparison.Ordinal);
        Assert.Contains("The field 'base' names the color index 32", error.Message, StringComparison.Ordinal);
        Assert.Contains("indices 0 to 31", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-14 exit test 4. The atlas is a square whose side is a power of two, made of whole tiles.</summary>
    [Fact]
    public void AtlasIsPowerOfTwo()
    {
        PngImage image = PngReader.Read(TextureGenCommand.AtlasBytes(ContentRoot()));

        Assert.Equal(AtlasLayout.AtlasPixels, image.Width);
        Assert.Equal(AtlasLayout.AtlasPixels, image.Height);
        Assert.Equal(0, image.Width & (image.Width - 1));
        Assert.Equal(0, image.Width % AtlasLayout.TilePixels);
    }

    /// <summary>The layout has 64 tiles, a block tile is its id, the body tiles start the second row, and a tile outside the atlas is an error (D-85, D-259, D-307).</summary>
    [Fact]
    public void AtlasLayoutPlacesTheBodyTiles()
    {
        Assert.Equal(64, AtlasLayout.TileCount);
        Assert.Equal((7, 0), (AtlasLayout.Column((int)BlockId.Plank), AtlasLayout.Row((int)BlockId.Plank)));
        Assert.Equal((0, 1), (AtlasLayout.Column(AtlasLayout.SkinTile), AtlasLayout.Row(AtlasLayout.SkinTile)));
        Assert.Equal((1, 1), (AtlasLayout.Column(AtlasLayout.ClothTile), AtlasLayout.Row(AtlasLayout.ClothTile)));
        Assert.Equal((2, 1), (AtlasLayout.Column(AtlasLayout.LeatherTile), AtlasLayout.Row(AtlasLayout.LeatherTile)));
        Assert.Throws<ArgumentOutOfRangeException>(() => AtlasLayout.Column(AtlasLayout.TileCount));
        Assert.Throws<ArgumentOutOfRangeException>(() => AtlasLayout.Row(-1));
    }

    /// <summary>The palette file holds the choice of the owner: eight ramps of four colors, with the values of D-304.</summary>
    [Fact]
    public void PaletteIsTheOwnerChoice()
    {
        Palette palette = RepositoryPalette();

        Assert.Equal(OwnerRampNames, palette.Ramps.Select(ramp => ramp.Name).ToArray());
        Assert.All(palette.Ramps, ramp => Assert.Equal(4, ramp.Count));
        string[] colors = palette.Colors.Select(color => $"#{color.Red:x2}{color.Green:x2}{color.Blue:x2}").ToArray();
        Assert.Equal(OwnerColors, colors);
    }

    /// <summary>The rules paint the seven blocks of D-259 and the three body tiles, and no other tile (D-307).</summary>
    [Fact]
    public void RulesPaintEveryBlockAndTheBody()
    {
        IReadOnlyList<TextureRule> rules = TextureGenCommand.ReadRules(ContentRoot(), RepositoryPalette());

        int[] tiles = rules.Select(rule => rule.Tile).OrderBy(tile => tile).ToArray();
        int[] expected =
        [
            (int)BlockId.RawStone, (int)BlockId.HewnStone, (int)BlockId.TimberBeam, (int)BlockId.OreVein,
            (int)BlockId.StillWater, (int)BlockId.Rubble, (int)BlockId.Plank,
            AtlasLayout.SkinTile, AtlasLayout.ClothTile, AtlasLayout.LeatherTile,
        ];
        Assert.Equal(expected, tiles);
    }

    /// <summary>
    /// Four tiles of the atlas equal the tiles of the palette preview that the owner chose from (D-304): the first row
    /// of each, and the count of each palette index over the tile. The preview ran the same sequence and comparisons.
    /// </summary>
    [Theory]
    [InlineData(1, "0,1,1,1,1,0,1,0,1,0,0,0,1,1,1,1", "0:194,1:614,2:216")]
    [InlineData(2, "4,5,5,5,5,5,5,5,5,6,5,5,5,6,5,5", "4:15,5:214,6:707,7:88")]
    [InlineData(4, "12,12,12,13,12,12,12,12,12,12,13,13,12,12,12,13", "12:753,13:271")]
    [InlineData(8, "29,29,29,29,29,29,29,29,30,29,29,29,29,29,30,29", "28:56,29:896,30:72")]
    public void TilesMatchThePalettePreview(int tile, string firstRow, string counts)
    {
        Palette palette = RepositoryPalette();
        byte[] pixels = TextureGenerator.Paint(palette, TextureGenCommand.ReadRules(ContentRoot(), palette));

        int[] row = Enumerable.Range(0, 16).Select(x => (int)TilePixel(pixels, tile, x, 0)).ToArray();
        Assert.Equal(firstRow, string.Join(",", row));

        SortedDictionary<int, int> histogram = new();
        for (int y = 0; y < AtlasLayout.TilePixels; y++)
        {
            for (int x = 0; x < AtlasLayout.TilePixels; x++)
            {
                int value = TilePixel(pixels, tile, x, y);
                histogram[value] = histogram.GetValueOrDefault(value) + 1;
            }
        }

        Assert.Equal(counts, string.Join(",", histogram.Select(pair => $"{pair.Key}:{pair.Value}")));
    }

    /// <summary>Over two hundred seeds, heavy noise and a deep edge never move a pixel off the ramp of its base. A failure names the seed (D-66).</summary>
    [Fact]
    public void NoiseStaysOnTheRampOfItsBase()
    {
        Palette palette = RepositoryPalette();
        PaletteRamp timber = palette.Ramps[2];
        for (uint seed = 1; seed <= 200; seed++)
        {
            byte[] pixels = new byte[AtlasLayout.AtlasPixels * AtlasLayout.AtlasPixels];
            TextureGenerator.PaintTile(pixels, palette, new TextureRule(BadRule, 3, timber.First + 1, 0.9, 3, seed));
            HashSet<int> seen = [];
            for (int y = 0; y < AtlasLayout.TilePixels; y++)
            {
                for (int x = 0; x < AtlasLayout.TilePixels; x++)
                {
                    int value = TilePixel(pixels, 3, x, y);
                    Assert.True(value >= timber.First && value < timber.First + timber.Count, $"Seed {seed}: the pixel ({x}, {y}) holds the index {value}, off the ramp '{timber.Name}' at the indices {timber.First} to {timber.First + timber.Count - 1}.");
                    seen.Add(value);
                }
            }

            Assert.True(seen.Count >= 3, $"Seed {seed}: heavy noise gave {seen.Count} indices, and it moves pixels both down and up the ramp.");
        }
    }

    /// <summary>With no noise, the edge darkness moves the outer ring down the ramp and the inside keeps the base. A deep edge stops at the dark end.</summary>
    [Fact]
    public void EdgeDarkensTheRing()
    {
        Palette palette = RepositoryPalette();
        PaletteRamp timber = palette.Ramps[2];
        byte[] pixels = new byte[AtlasLayout.AtlasPixels * AtlasLayout.AtlasPixels];

        TextureGenerator.PaintTile(pixels, palette, new TextureRule(BadRule, 3, timber.First + 2, 0.0, 1, 7));
        AssertRingAndInside(pixels, 3, timber.First + 1, timber.First + 2);

        TextureGenerator.PaintTile(pixels, palette, new TextureRule(BadRule, 3, timber.First + 2, 0.0, 5, 7));
        AssertRingAndInside(pixels, 3, timber.First, timber.First + 2);
    }

    /// <summary>Two rules for one tile are an error that names the tile and both files.</summary>
    [Fact]
    public void TwoRulesForOneTileFail()
    {
        Palette palette = RepositoryPalette();
        TextureRule first = TextureRule.Parse("textures/rules/a.json", RuleBytes(tile: "3"), palette);
        TextureRule second = TextureRule.Parse("textures/rules/b.json", RuleBytes(tile: "3"), palette);

        ContextException error = Assert.Throws<ContextException>(() => TextureGenerator.Paint(palette, [first, second]));

        Assert.Contains("'textures/rules/a.json'", error.Message, StringComparison.Ordinal);
        Assert.Contains("'textures/rules/b.json'", error.Message, StringComparison.Ordinal);
        Assert.Contains("the tile 3", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Each field of a rule has its bounds, and an unknown field is an error. Each error names the file and the field (D-168, T-2).</summary>
    [Theory]
    [InlineData("64", "1", "0.4", "0", "1001", "", "tile")]
    [InlineData("-1", "1", "0.4", "0", "1001", "", "tile")]
    [InlineData("1", "-1", "0.4", "0", "1001", "", "base")]
    [InlineData("1", "1", "1.5", "0", "1001", "", "noise")]
    [InlineData("1", "1", "-0.1", "0", "1001", "", "noise")]
    [InlineData("1", "1", "\"much\"", "0", "1001", "", "noise")]
    [InlineData("1", "1", "0.4", "-1", "1001", "", "edge")]
    [InlineData("1", "1", "0.4", "0", "0", "", "seed")]
    [InlineData("1", "1", "0.4", "0", "4294967296", "", "seed")]
    [InlineData("1", "1", "0.4", "0", "1001", ", \"glow\": 3", "glow")]
    public void RuleRejectsAFieldOutsideItsBounds(string tile, string baseIndex, string noise, string edge, string seed, string extra, string field)
    {
        byte[] bytes = RuleBytes(tile, baseIndex, noise, edge, seed, extra);

        ContextException error = Assert.Throws<ContextException>(() => TextureRule.Parse(BadRule, bytes, RepositoryPalette()));

        Assert.Contains(BadRule, error.Message, StringComparison.Ordinal);
        Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>An absent field of a rule is an error that names the field (D-92).</summary>
    [Fact]
    public void RuleRejectsAnAbsentField()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("{\"tile\": 1, \"base\": 1, \"noise\": 0.4, \"edge\": 0}");

        ContextException error = Assert.Throws<ContextException>(() => TextureRule.Parse(BadRule, bytes, RepositoryPalette()));

        Assert.Contains("The field 'seed' is absent", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A palette file with a bad color, a repeated color or name, an empty list, an unknown field, or no object is an error that names the file (D-168, T-2).</summary>
    [Theory]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#ABCDEF\"]}]}", "colors", "six lowercase hex digits")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#abc\"]}]}", "colors", "six lowercase hex digits")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [7]}]}", "colors", "six lowercase hex digits")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\", \"#101010\"]}]}", "colors", "each color appears once")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": []}]}", "colors", "at least one color")]
    [InlineData("{\"ramps\": []}", "ramps", "at least one ramp")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\"]}, {\"name\": \"rock\", \"colors\": [\"#202020\"]}]}", "name", "an earlier ramp has that name")]
    [InlineData("{\"ramps\": [{\"name\": \"\", \"colors\": [\"#101010\"]}]}", "name", "is empty")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\"], \"glow\": true}]}", "glow", "not a field of the format")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\"]}], \"count\": 1}", "count", "not a field of the format")]
    [InlineData("[1, 2]", "", "one JSON object")]
    [InlineData("{\"ramps\": ", "", "not valid JSON")]
    public void PaletteRejectsABadFile(string json, string field, string reason)
    {
        ContextException error = Assert.Throws<ContextException>(() => Palette.Parse(AssetPaths.PaletteFile, Encoding.UTF8.GetBytes(json)));

        Assert.Contains(AssetPaths.PaletteFile, error.Message, StringComparison.Ordinal);
        if (field.Length > 0)
        {
            Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
        }

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A palette past 256 colors is an error, because an indexed PNG holds no more.</summary>
    [Fact]
    public void PaletteRejectsMoreColorsThanAPngHolds()
    {
        IEnumerable<string> colors = Enumerable.Range(0, 257).Select(index => $"\"#{index:x6}\"");
        string json = "{\"ramps\": [{\"name\": \"many\", \"colors\": [" + string.Join(", ", colors) + "]}]}";

        ContextException error = Assert.Throws<ContextException>(() => Palette.Parse(AssetPaths.PaletteFile, Encoding.UTF8.GetBytes(json)));

        Assert.Contains("holds 257 colors", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A small image goes through the writer and the strict reader with its size, its palette, and its pixels.</summary>
    [Fact]
    public void PngWriterRoundTrips()
    {
        PaletteColor[] colors = [new PaletteColor(1, 2, 3, 0, 0), new PaletteColor(250, 128, 7, 0, 1)];
        byte[] pixels = [0, 1, 1, 0, 1, 0];

        PngImage image = PngReader.Read(PngWriter.Write(3, 2, colors, pixels));

        Assert.Equal(3, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(new byte[] { 1, 2, 3, 250, 128, 7 }, image.PaletteBytes);
        Assert.Equal(pixels, image.Pixels);
    }

    /// <summary>Image data past one stored block goes into several blocks, and the platform decompressor reads them back.</summary>
    [Fact]
    public void PngWriterSplitsLongDataIntoStoredBlocks()
    {
        PaletteColor[] colors = [new PaletteColor(0, 0, 0, 0, 0), new PaletteColor(255, 255, 255, 0, 1)];
        byte[] pixels = Enumerable.Range(0, 300 * 300).Select(index => (byte)(index % 7 == 0 ? 1 : 0)).ToArray();

        PngImage image = PngReader.Read(PngWriter.Write(300, 300, colors, pixels));

        Assert.True((300 + 1) * 300 > PngWriter.MaxStoredBlock);
        Assert.Equal(pixels, image.Pixels);
    }

    /// <summary>A pixel past the palette, a pixel count that differs from the size, a size of zero, and an empty palette are each an error.</summary>
    [Fact]
    public void PngWriterRejectsABadImage()
    {
        PaletteColor[] colors = [new PaletteColor(1, 2, 3, 0, 0)];

        ArgumentException past = Assert.Throws<ArgumentException>(() => PngWriter.Write(2, 1, colors, [0, 1]));
        Assert.Contains("The pixel 1 names the index 1", past.Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => PngWriter.Write(2, 2, colors, [0, 0, 0]));
        Assert.Throws<ArgumentException>(() => PngWriter.Write(0, 1, colors, []));
        Assert.Throws<ArgumentException>(() => PngWriter.Write(1, 1, [], [0]));
    }

    /// <summary>The command writes the atlas of the palette and the rules under the root, and the file equals the generator output.</summary>
    [Fact]
    public void CommandWritesTheAtlas()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        content.Write(AssetPaths.RuleDirectory + "raw-stone.json", Encoding.UTF8.GetString(RuleBytes()));

        Assert.Equal(0, TextureGenCommand.Run(["--root", content.Root]));

        byte[] written = File.ReadAllBytes(Path.Combine(content.Content, AssetPaths.AtlasImage));
        Assert.Equal(TextureGenCommand.AtlasBytes(content.Content), written);
    }

    /// <summary>A bad rule is exit code 1, and the command writes no atlas (T-2).</summary>
    [Fact]
    public void CommandReportsABadRuleAndWritesNothing()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        content.Write(AssetPaths.RuleDirectory + "raw-stone.json", Encoding.UTF8.GetString(RuleBytes(baseIndex: "99")));

        Assert.Equal(1, TextureGenCommand.Run(["--root", content.Root]));

        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.AtlasImage)));
    }

    /// <summary>A rule file that the user cannot read is exit code 1 with the file name, and never an unhandled exception (T-2, PR #56 review P2-1).</summary>
    [Fact]
    public void CommandReportsAnUnreadableRule()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        content.Write(AssetPaths.RuleDirectory + "raw-stone.json", Encoding.UTF8.GetString(RuleBytes()));
        string rule = Path.Combine(content.Content, AssetPaths.RuleDirectory, "raw-stone.json");

        (int exit, string errors) = RunWithUnreadableFile(content.Root, rule);

        Assert.Equal(1, exit);
        Assert.Contains("raw-stone.json", errors, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.AtlasImage)));
    }

    /// <summary>A palette that the user cannot read is exit code 1 with the file name, and never an unhandled exception (T-2, PR #56 review P2-1).</summary>
    [Fact]
    public void CommandReportsAnUnreadablePalette()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        content.Write(AssetPaths.RuleDirectory + "raw-stone.json", Encoding.UTF8.GetString(RuleBytes()));
        string palette = Path.Combine(content.Content, AssetPaths.PaletteFile);

        (int exit, string errors) = RunWithUnreadableFile(content.Root, palette);

        Assert.Equal(1, exit);
        Assert.Contains("palette.json", errors, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.AtlasImage)));
    }

    /// <summary>
    /// Runs the command while one file cannot be read, and returns the exit code and the error output. Windows reads no
    /// file that another handle holds with no share, and Linux and macOS read no file with no permission. The method
    /// first proves the file unreadable, so a user that permissions do not bind fails the test and never passes it, and
    /// it makes the file readable again before it returns.
    /// </summary>
    private static (int Exit, string Errors) RunWithUnreadableFile(string root, string file)
    {
        TextWriter savedError = Console.Error;
        using StringWriter errors = new();
        FileStream? hold = null;
        if (OperatingSystem.IsWindows())
        {
            hold = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
        }
        else
        {
            File.SetUnixFileMode(file, UnixFileMode.None);
        }

        try
        {
            bool readable = true;
            try
            {
                File.ReadAllBytes(file);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                readable = false;
            }

            Assert.False(readable, $"The test could not make '{file}' unreadable, so it cannot reach the read failure.");
            Console.SetError(errors);
            int exit = TextureGenCommand.Run(["--root", root]);
            return (exit, errors.ToString());
        }
        finally
        {
            Console.SetError(savedError);
            hold?.Dispose();
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
    }

    /// <summary>An absent palette, an absent rule directory, and a rule directory with no rule are each an error that names the place.</summary>
    [Fact]
    public void AnAbsentPaletteOrRuleIsAnError()
    {
        using TemporaryContentDirectory content = new();
        ContextException noPalette = Assert.Throws<ContextException>(() => TextureGenCommand.AtlasBytes(content.Content));
        Assert.Contains("palette.json", noPalette.Message, StringComparison.Ordinal);

        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        ContextException noDirectory = Assert.Throws<ContextException>(() => TextureGenCommand.AtlasBytes(content.Content));
        Assert.Contains("does not exist", noDirectory.Message, StringComparison.Ordinal);

        Directory.CreateDirectory(Path.Combine(content.Content, AssetPaths.RuleDirectory));
        ContextException noRule = Assert.Throws<ContextException>(() => TextureGenCommand.AtlasBytes(content.Content));
        Assert.Contains("holds no rule file", noRule.Message, StringComparison.Ordinal);
    }

    /// <summary>The command needs the root option with a value, and it takes no other argument.</summary>
    [Fact]
    public void CommandNeedsTheRoot()
    {
        Assert.Equal(2, TextureGenCommand.Run([]));
        Assert.Equal(2, TextureGenCommand.Run(["--other"]));
        Assert.Equal(2, TextureGenCommand.Run(["--root"]));
    }

    /// <summary>
    /// Every face of the player model reads one body tile at the world density of 32 texels per meter (D-308). The
    /// head reads skin, the torso and the arms read cloth, and the legs read leather (D-307).
    /// </summary>
    [Fact]
    public void BodyFacesReadOneBodyTileAtWorldDensity()
    {
        string file = Path.Combine(ContentRoot(), AssetPaths.BodyModel);
        BlockbenchModel body = BlockbenchLoader.Parse(AssetPaths.BodyModel, File.ReadAllBytes(file));

        Assert.Equal(10, body.Boxes.Count);
        foreach (ModelBox box in body.Boxes)
        {
            int tile = BodyTileOf(box.Name);
            float tileLeft = AtlasLayout.Column(tile) * AtlasLayout.TilePixels;
            float tileTop = AtlasLayout.Row(tile) * AtlasLayout.TilePixels;
            for (int side = 0; side < box.Faces.Count; side++)
            {
                FaceUv uv = box.Faces[side];
                string face = $"{box.Name}.{(BoxSide)side}";
                float left = uv.LowU * AtlasLayout.AtlasPixels;
                float top = uv.LowV * AtlasLayout.AtlasPixels;
                float right = uv.HighU * AtlasLayout.AtlasPixels;
                float bottom = uv.HighV * AtlasLayout.AtlasPixels;
                Assert.True(left >= tileLeft - UvTolerance && right <= tileLeft + AtlasLayout.TilePixels + UvTolerance, $"The face {face} spans the pixels {left} to {right} across, outside the tile {tile} (D-308).");
                Assert.True(top >= tileTop - UvTolerance && bottom <= tileTop + AtlasLayout.TilePixels + UvTolerance, $"The face {face} spans the pixels {top} to {bottom} down, outside the tile {tile} (D-308).");

                (float width, float height) = FaceMeters((BoxSide)side, box);
                float texelsWide = width * AtlasLayout.TexelsPerMeter;
                float texelsHigh = height * AtlasLayout.TexelsPerMeter;
                Assert.True(Math.Abs((right - left) - texelsWide) < UvTolerance, $"The face {face} is {right - left} texels wide, and {width} meters needs {texelsWide} (D-308).");
                Assert.True(Math.Abs((bottom - top) - texelsHigh) < UvTolerance, $"The face {face} is {bottom - top} texels high, and {height} meters needs {texelsHigh} (D-308).");
            }
        }
    }

    private static string ContentRoot()
    {
        return Path.Combine(RepositoryRoot.Find(), "content");
    }

    private static Palette RepositoryPalette()
    {
        return Palette.Parse(AssetPaths.PaletteFile, File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
    }

    /// <summary>The palette index of one pixel of one tile, from its top left corner.</summary>
    private static byte TilePixel(byte[] pixels, int tile, int x, int y)
    {
        int left = AtlasLayout.Column(tile) * AtlasLayout.TilePixels;
        int top = AtlasLayout.Row(tile) * AtlasLayout.TilePixels;
        return pixels[((top + y) * AtlasLayout.AtlasPixels) + left + x];
    }

    /// <summary>Every pixel of the outer ring of a tile holds one index, and every pixel inside holds another.</summary>
    private static void AssertRingAndInside(byte[] pixels, int tile, int ring, int inside)
    {
        int last = AtlasLayout.TilePixels - 1;
        for (int y = 0; y < AtlasLayout.TilePixels; y++)
        {
            for (int x = 0; x < AtlasLayout.TilePixels; x++)
            {
                bool onRing = x == 0 || y == 0 || x == last || y == last;
                int expected = onRing ? ring : inside;
                int value = TilePixel(pixels, tile, x, y);
                Assert.True(value == expected, $"The pixel ({x}, {y}) of the tile {tile} holds the index {value}, and the rule gives {expected}.");
            }
        }
    }

    /// <summary>The bytes of one rule file with the given field texts.</summary>
    private static byte[] RuleBytes(string tile = "1", string baseIndex = "1", string noise = "0.4", string edge = "0", string seed = "1001", string extra = "")
    {
        string json = "{\"tile\": " + tile + ", \"base\": " + baseIndex + ", \"noise\": " + noise + ", \"edge\": " + edge + ", \"seed\": " + seed + extra + "}";
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>The body material tile of one box of the player model, by the name of the box (D-307).</summary>
    private static int BodyTileOf(string boxName)
    {
        if (boxName.StartsWith("head_", StringComparison.Ordinal))
        {
            return AtlasLayout.SkinTile;
        }

        if (boxName.StartsWith("torso_", StringComparison.Ordinal) || boxName.StartsWith("arm_", StringComparison.Ordinal))
        {
            return AtlasLayout.ClothTile;
        }

        if (boxName.StartsWith("leg_", StringComparison.Ordinal))
        {
            return AtlasLayout.LeatherTile;
        }

        throw new InvalidOperationException($"The box '{boxName}' has no body material in D-307.");
    }

    /// <summary>The width and the height of one face in meters, as the texture shows it (the order of the corners in BoxGeometry).</summary>
    private static (float Width, float Height) FaceMeters(BoxSide side, ModelBox box)
    {
        float x = box.To.X - box.From.X;
        float y = box.To.Y - box.From.Y;
        float z = box.To.Z - box.From.Z;
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
}
