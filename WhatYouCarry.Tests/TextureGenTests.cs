using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Vector2 = Godot.Vector2;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Models;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Tools.TextureGen;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The texture generator over the repository content: the palette, the atlas, the layout, the PNG file, and the
/// command (D-85, D-304, D-305, D-308, D-505, D-506; PR-14 exit tests 1 to 4, PR-62 exit tests 1, 2, and 4).
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class TextureGenTests
{
    private const float UvTolerance = 0.0001f;

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

    /// <summary>
    /// The SHA-256 hash of the palette indices of each block tile of the atlas of PR-14, 32 by 32, row by row. The
    /// hashes come from the committed atlas at the base of PR-62, before the recipe system (D-504).
    /// </summary>
    public static TheoryData<int, string> BlockTilesOfPr14 => new()
    {
        { 1, "981083a1115b20d2690854eae699e4ef77056dd4359505a5d34981138a861556" },
        { 2, "357660711b5e8669096250b54911cd42feaec44302b5da156ae34c3dbb8cd48f" },
        { 3, "2bc18cd11e78cb3ada7d1c5738886e9f463f86612291d12792d1190e32274467" },
        { 4, "f29ccbe11f118f8f421b46f38bf3960aeb1bc51604627b3e8990a517419a4b32" },
        { 5, "30b6c15ff74b8532a3200c760fd2e5843db0d01b9cbff5ca8ca27f405e804833" },
        { 6, "611fc1c60bfdba59157f3fc5938c428e8c74a06872d6b31bf28a105defffbb57" },
        { 7, "749e352201daa86a731cc2899f47f66fe65c1f15e27a8f6b0b9328fe1c997b90" },
    };

    /// <summary>PR-14 exit test 1. The palette chunk of the atlas is the palette, and every pixel names an index inside it.</summary>
    [Fact]
    public void GeneratorUsesPaletteOnly()
    {
        Palette palette = RepositoryPalette();
        PngImage image = PngReader.Read(TextureGenCommand.Generate(ContentRoot()).Atlas);

        byte[] expected = palette.Colors.SelectMany(color => new[] { color.Red, color.Green, color.Blue }).ToArray();
        Assert.Equal(expected, image.PaletteBytes);
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++)
        {
            Assert.True(image.Pixels[pixel] < palette.Colors.Count, $"The pixel {pixel} names the index {image.Pixels[pixel]}, and the palette holds {palette.Colors.Count} colors.");
        }
    }

    /// <summary>PR-14 exit test 2 and PR-62 exit test 1. Two runs of the generator give equal atlas bytes and equal layout text.</summary>
    [Fact]
    public void GeneratorIsDeterministic()
    {
        GeneratorOutput first = TextureGenCommand.Generate(ContentRoot());
        GeneratorOutput second = TextureGenCommand.Generate(ContentRoot());

        Assert.Equal(first.Atlas, second.Atlas);
        Assert.Equal(first.Layout, second.Layout);
    }

    /// <summary>PR-62 exit test 1. The committed atlas equals the generator output, so an input change without a new atlas fails here (D-305).</summary>
    [Fact]
    public void CommittedAtlasMatchesTheGenerator()
    {
        byte[] committed = File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage));
        byte[] generated = TextureGenCommand.Generate(ContentRoot()).Atlas;

        Assert.True(committed.SequenceEqual(generated), "The committed atlas differs from the generator output. Run the texture-gen command with --root on the checkout, and commit textures/atlas.png and textures/layout.json (D-305, D-505).");
    }

    /// <summary>PR-62 exit test 1. The committed layout equals the generator output byte for byte (D-505).</summary>
    [Fact]
    public void CommittedLayoutMatchesTheGenerator()
    {
        string committed = File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.LayoutFile));
        string generated = TextureGenCommand.Generate(ContentRoot()).Layout;

        Assert.True(committed == generated, "The committed layout differs from the generator output. Run the texture-gen command with --root on the checkout, and commit textures/atlas.png and textures/layout.json (D-505).");
    }

    /// <summary>PR-14 exit test 4. The atlas is a square of 512 pixels, a power of two, that holds whole block canvases (D-506).</summary>
    [Fact]
    public void AtlasIsPowerOfTwo()
    {
        PngImage image = PngReader.Read(TextureGenCommand.Generate(ContentRoot()).Atlas);

        Assert.Equal(512, AtlasLayout.AtlasPixels);
        Assert.Equal(AtlasLayout.AtlasPixels, image.Width);
        Assert.Equal(AtlasLayout.AtlasPixels, image.Height);
        Assert.Equal(0, image.Width & (image.Width - 1));
        Assert.Equal(0, image.Width % AtlasLayout.BlockPixels);
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

    /// <summary>
    /// PR-62 exit test 2. The canvas of each block in the committed atlas holds the pixels of its tile in the atlas of
    /// PR-14, so the world keeps its look (D-504, D-309).
    /// </summary>
    [Theory]
    [MemberData(nameof(BlockTilesOfPr14))]
    public void BlockCanvasEqualsItsTileOfPr14(int block, string hash)
    {
        PngImage image = PngReader.Read(File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage)));
        AtlasRect place = RepositoryTextures.Layout.Block(block);

        byte[] pixels = new byte[AtlasLayout.BlockPixels * AtlasLayout.BlockPixels];
        for (int y = 0; y < AtlasLayout.BlockPixels; y++)
        {
            for (int x = 0; x < AtlasLayout.BlockPixels; x++)
            {
                pixels[(y * AtlasLayout.BlockPixels) + x] = image.Pixels[((place.Y + y) * image.Width) + place.X + x];
            }
        }

        Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant());
    }

    /// <summary>
    /// A block canvas of each of four recipes equals the tile of the palette preview that the owner chose from (D-304):
    /// the first row, and the count of each palette index. The skin recipe at the block salt gives the old skin tile.
    /// </summary>
    [Theory]
    [InlineData("raw-stone", "0,1,1,1,1,0,1,0,1,0,0,0,1,1,1,1", "0:194,1:614,2:216")]
    [InlineData("hewn-stone", "4,5,5,5,5,5,5,5,5,6,5,5,5,6,5,5", "4:15,5:214,6:707,7:88")]
    [InlineData("ore-vein", "12,12,12,13,12,12,12,12,12,12,13,13,12,12,12,13", "12:753,13:271")]
    [InlineData("skin", "29,29,29,29,29,29,29,29,30,29,29,29,29,29,30,29", "28:56,29:896,30:72")]
    public void BlockCanvasesMatchThePalettePreview(string recipe, string firstRow, string counts)
    {
        Palette palette = RepositoryPalette();
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll(TextureGenCommand.ReadRecipeFiles(ContentRoot()), palette);
        byte[] pixels = CanvasPainter.Paint(palette, recipes[recipe], AtlasLayout.BlockPixels, AtlasLayout.BlockPixels, CanvasPainter.BlockSalt, recipe);

        Assert.Equal(firstRow, string.Join(",", pixels.Take(16)));
        string histogram = string.Join(",", pixels.GroupBy(value => value).OrderBy(group => group.Key).Select(group => $"{group.Key}:{group.Count()}"));
        Assert.Equal(counts, histogram);
    }

    /// <summary>
    /// PR-62 exit test 4. Each face of the player and the sword has a canvas in the committed layout, of its size at 32
    /// texels per meter, and the box mesh reads that many texels of it (D-308, D-505).
    /// </summary>
    [Theory]
    [InlineData("models/player.bbmodel", 10)]
    [InlineData("models/sword-basic.bbmodel", 4)]
    public void EveryFaceHasACanvasAtWorldDensity(string modelPath, int boxes)
    {
        BlockbenchModel model = BlockbenchLoader.Parse(modelPath, File.ReadAllBytes(Path.Combine(ContentRoot(), modelPath)));

        Assert.Equal(boxes, model.Boxes.Count);
        foreach (ModelBox box in model.Boxes)
        {
            MeshData mesh = BoxGeometry.Build(model.Path, box, RepositoryTextures.Layout);
            for (int side = 0; side < BoxFaces.Names.Count; side++)
            {
                string face = TextureLayout.FaceName(model.Path, box.Name, (BoxSide)side);
                AtlasRect canvas = RepositoryTextures.Layout.Face(model.Path, box.Name, (BoxSide)side);
                (int canvasWidth, int canvasHeight) = BoxFaces.CanvasTexels(box, (BoxSide)side);
                Assert.True(canvas.Width == canvasWidth && canvas.Height == canvasHeight, $"The canvas of {face} is {canvas.Width} by {canvas.Height}, and the face needs {canvasWidth} by {canvasHeight} (D-308).");

                (float width, float height) = BoxFaces.Texels(box, (BoxSide)side);
                Vector2 topLeft = mesh.Uvs[side * MeshData.QuadVertices];
                Vector2 bottomRight = mesh.Uvs[(side * MeshData.QuadVertices) + 2];
                Assert.Equal((float)canvas.X / AtlasLayout.AtlasPixels, topLeft.X, UvTolerance);
                Assert.Equal((float)canvas.Y / AtlasLayout.AtlasPixels, topLeft.Y, UvTolerance);
                Assert.Equal(width, (bottomRight.X - topLeft.X) * AtlasLayout.AtlasPixels, 0.01f);
                Assert.Equal(height, (bottomRight.Y - topLeft.Y) * AtlasLayout.AtlasPixels, 0.01f);
            }
        }
    }

    /// <summary>
    /// The paint files keep the materials of D-308 and D-330: the head reads skin, the torso and the arms cloth, the legs
    /// leather, and the sword a leather grip, a plank guard, and a metal blade and tip. No face overrides its box yet.
    /// </summary>
    [Fact]
    public void PaintFilesKeepTheMaterials()
    {
        Dictionary<string, string> expected = new()
        {
            ["models/player.bbmodel:head_box"] = "skin",
            ["models/player.bbmodel:torso_box"] = "cloth",
            ["models/player.bbmodel:arm_left_upper_box"] = "cloth",
            ["models/player.bbmodel:arm_left_lower_box"] = "cloth",
            ["models/player.bbmodel:arm_right_upper_box"] = "cloth",
            ["models/player.bbmodel:arm_right_lower_box"] = "cloth",
            ["models/player.bbmodel:leg_left_upper_box"] = "leather",
            ["models/player.bbmodel:leg_left_lower_box"] = "leather",
            ["models/player.bbmodel:leg_right_upper_box"] = "leather",
            ["models/player.bbmodel:leg_right_lower_box"] = "leather",
            ["models/sword-basic.bbmodel:grip_box"] = "leather",
            ["models/sword-basic.bbmodel:guard_box"] = "plank",
            ["models/sword-basic.bbmodel:blade_box"] = "metal",
            ["models/sword-basic.bbmodel:tip_box"] = "metal",
        };

        Assert.Equal(expected.Count * BoxFaces.Names.Count, RepositoryTextures.Layout.Faces.Count);
        foreach (FacePlace place in RepositoryTextures.Layout.Faces)
        {
            Assert.Equal(expected[place.Model + ":" + place.Box], place.Recipe);
        }
    }

    /// <summary>Every block id other than air has a canvas of 32 pixels in the committed layout, bound to the recipe of its material (D-259, D-505).</summary>
    [Fact]
    public void EveryBlockHasACanvas()
    {
        string[] recipes = ["raw-stone", "hewn-stone", "timber-beam", "ore-vein", "still-water", "rubble", "plank"];
        Assert.Equal(recipes.Length, RepositoryTextures.Layout.Blocks.Count);
        for (int index = 0; index < recipes.Length; index++)
        {
            BlockPlace place = RepositoryTextures.Layout.Blocks[index];
            Assert.Equal(index + 1, place.Block);
            Assert.Equal(recipes[index], place.Recipe);
            Assert.Equal(AtlasLayout.BlockPixels, place.At.Width);
            Assert.Equal(AtlasLayout.BlockPixels, place.At.Height);
        }
    }

    /// <summary>The gutter of every canvas of the committed atlas repeats the nearest pixel of the canvas, so a sample at a face edge never reads a neighbor.</summary>
    [Fact]
    public void GutterRepeatsTheCanvasEdge()
    {
        PngImage image = PngReader.Read(File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage)));
        List<AtlasRect> places = [.. RepositoryTextures.Layout.Blocks.Select(place => place.At), .. RepositoryTextures.Layout.Faces.Select(place => place.At)];
        foreach (AtlasRect place in places)
        {
            for (int y = -1; y <= place.Height; y++)
            {
                for (int x = -1; x <= place.Width; x++)
                {
                    int sourceX = Math.Clamp(x, 0, place.Width - 1);
                    int sourceY = Math.Clamp(y, 0, place.Height - 1);
                    byte gutter = image.Pixels[((place.Y + y) * image.Width) + place.X + x];
                    byte source = image.Pixels[((place.Y + sourceY) * image.Width) + place.X + sourceX];
                    Assert.True(gutter == source, $"The pixel ({x}, {y}) of the canvas at ({place.X}, {place.Y}) holds {gutter}, and the nearest canvas pixel holds {source}.");
                }
            }
        }
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

    /// <summary>The command writes the atlas and the layout of a small content directory, and both files equal the generator output.</summary>
    [Fact]
    public void CommandWritesTheAtlasAndTheLayout()
    {
        using TemporaryContentDirectory content = MinimalContent();

        Assert.Equal(0, TextureGenCommand.Run(["--root", content.Root]));

        GeneratorOutput expected = TextureGenCommand.Generate(content.Content);
        Assert.Equal(expected.Atlas, File.ReadAllBytes(Path.Combine(content.Content, AssetPaths.AtlasImage)));
        Assert.Equal(expected.Layout, File.ReadAllText(Path.Combine(content.Content, AssetPaths.LayoutFile)));
    }

    /// <summary>A bad recipe is exit code 1, and the command writes neither file (T-2).</summary>
    [Fact]
    public void CommandReportsABadRecipeAndWritesNothing()
    {
        using TemporaryContentDirectory content = MinimalContent();
        content.Write(AssetPaths.RecipeDirectory + "stone.json", "{\"layers\": [{\"kind\": \"fill\", \"color\": 99, \"noise\": 0.4, \"seed\": 1001}]}");

        Assert.Equal(1, TextureGenCommand.Run(["--root", content.Root]));

        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.AtlasImage)));
        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.LayoutFile)));
    }

    /// <summary>A recipe file that the user cannot read is exit code 1 with the file name, and never an unhandled exception (T-2, PR #56 review P2-1).</summary>
    [Fact]
    public void CommandReportsAnUnreadableRecipe()
    {
        using TemporaryContentDirectory content = MinimalContent();
        string recipe = Path.Combine(content.Content, AssetPaths.RecipeDirectory, "stone.json");

        (int exit, string errors) = RunWithUnreadableFile(content.Root, recipe);

        Assert.Equal(1, exit);
        Assert.Contains("recipes", errors, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.AtlasImage)));
    }

    /// <summary>A palette that the user cannot read is exit code 1 with the file name, and never an unhandled exception (T-2, PR #56 review P2-1).</summary>
    [Fact]
    public void CommandReportsAnUnreadablePalette()
    {
        using TemporaryContentDirectory content = MinimalContent();
        string palette = Path.Combine(content.Content, AssetPaths.PaletteFile);

        (int exit, string errors) = RunWithUnreadableFile(content.Root, palette);

        Assert.Equal(1, exit);
        Assert.Contains("palette.json", errors, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(content.Content, AssetPaths.AtlasImage)));
    }

    /// <summary>An absent palette, an absent recipe directory, an empty recipe directory, and an absent block file are each an error that names the place.</summary>
    [Fact]
    public void AnAbsentInputIsAnError()
    {
        using TemporaryContentDirectory content = new();
        ContextException noPalette = Assert.Throws<ContextException>(() => TextureGenCommand.Generate(content.Content));
        Assert.Contains("palette.json", noPalette.Message, StringComparison.Ordinal);

        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        ContextException noDirectory = Assert.Throws<ContextException>(() => TextureGenCommand.Generate(content.Content));
        Assert.Contains("does not exist", noDirectory.Message, StringComparison.Ordinal);

        Directory.CreateDirectory(Path.Combine(content.Content, AssetPaths.RecipeDirectory));
        ContextException noRecipe = Assert.Throws<ContextException>(() => TextureGenCommand.Generate(content.Content));
        Assert.Contains("holds no recipe file", noRecipe.Message, StringComparison.Ordinal);

        content.Write(AssetPaths.RecipeDirectory + "stone.json", StoneRecipe);
        ContextException noBlocks = Assert.Throws<ContextException>(() => TextureGenCommand.Generate(content.Content));
        Assert.Contains("blocks.json", noBlocks.Message, StringComparison.Ordinal);
    }

    /// <summary>A model with no paint file, and a paint file with no model, are each an error that names the file (D-508).</summary>
    [Fact]
    public void EachModelHasOnePaintFile()
    {
        using TemporaryContentDirectory content = MinimalContent();
        content.Write("models/rig.bbmodel", ModelJson.SiblingRig());
        ContextException noPaint = Assert.Throws<ContextException>(() => TextureGenCommand.Generate(content.Content));
        Assert.Contains("'models/rig.bbmodel' has no paint file 'models/rig.paint.json'", noPaint.Message, StringComparison.Ordinal);

        content.Write("models/rig.paint.json", "{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"stone\", \"faces\": {}}, \"arm\": {\"recipe\": \"stone\", \"faces\": {}}}}");
        GeneratorOutput output = TextureGenCommand.Generate(content.Content);
        TextureLayout layout = TextureLayout.Parse(AssetPaths.LayoutFile, Encoding.UTF8.GetBytes(output.Layout));
        Assert.Equal(2 * BoxFaces.Names.Count, layout.Faces.Count);

        content.Write("models/ghost.paint.json", "{}");
        ContextException orphan = Assert.Throws<ContextException>(() => TextureGenCommand.Generate(content.Content));
        Assert.Contains("'models/ghost.paint.json' names no model file", orphan.Message, StringComparison.Ordinal);
    }

    /// <summary>The command needs the root option with a value, and it takes no other argument.</summary>
    [Fact]
    public void CommandNeedsTheRoot()
    {
        Assert.Equal(2, TextureGenCommand.Run([]));
        Assert.Equal(2, TextureGenCommand.Run(["--other"]));
        Assert.Equal(2, TextureGenCommand.Run(["--root"]));
    }

    /// <summary>A recipe of one fill, which the small content directories of these tests bind to every block.</summary>
    private const string StoneRecipe = "{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"noise\": 0.4, \"seed\": 1001}]}";

    /// <summary>A content directory of the repository palette, one recipe, and a block file that binds the recipe to every block.</summary>
    private static TemporaryContentDirectory MinimalContent()
    {
        TemporaryContentDirectory content = new();
        content.Write(AssetPaths.PaletteFile, File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
        content.Write(AssetPaths.RecipeDirectory + "stone.json", StoneRecipe);
        IEnumerable<string> entries = Enumerable.Range(1, 7).Select(block => "{\"block\": " + block + ", \"recipe\": \"stone\"}");
        content.Write(AssetPaths.BlockPaintFile, "{\"blocks\": [" + string.Join(", ", entries) + "]}");
        return content;
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

    private static string ContentRoot()
    {
        return Path.Combine(RepositoryRoot.Find(), "content");
    }

    private static Palette RepositoryPalette()
    {
        return Palette.Parse(AssetPaths.PaletteFile, File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
    }
}
