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
using WhatYouCarry.Tools.TextureTrace;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The texture generator over the repository content: the palette, the atlas, the layout, the PNG file, and the
/// command (D-85, D-304, D-305, D-308, D-505, D-598, D-603, D-604; PR-14 exit tests 1 to 4, PR-62 exit tests 1, 2, and 4,
/// PR-89 exit tests 1 and 2, D-599).
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class TextureGenTests
{
    private const float UvTolerance = 0.0001f;

    /// <summary>The faces of the trace specs: 40 of the body, 28 of the sword, 52 of the scavenger, and 45 of the Overseer (D-612).</summary>
    private const int TracedFaceCount = 165;

    /// <summary>The trace specs of the repository (D-612).</summary>
    private static readonly string[] TraceSpecs = ["miner", "sword", "scavenger", "overseer"];

    /// <summary>The side of a block tile in the atlas of PR-14 and in the palette preview, at 32 texels per meter (D-85).</summary>
    private const int TilePixelsOfPr14 = 32;

    /// <summary>The indexed atlas at the base of PR-89, before the truecolor atlas of D-598.</summary>

    /// <summary>The ramp names of the palette of D-304, the umber ramp of D-530, the ten ramps of D-592, and the soot ramp of D-600, in file order.</summary>
    private static readonly string[] OwnerRampNames = ["rock", "slate", "timber", "ochre", "rust", "water", "lichen", "bone", "umber", "steel", "brass", "clay", "crimson", "cobalt", "violet", "linen", "moss", "ember", "ice", "soot"];

    /// <summary>The 32 colors of the palette of D-304, the 4 of D-530, the 40 of D-592, and the 4 of D-600, ramp by ramp from dark to light.</summary>
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
        "#160e05", "#2b1f11", "#413221", "#5d4c39",
        "#2b2e32", "#51565b", "#7b8187", "#acb2b7",
        "#372c11", "#5f4e22", "#8c7438", "#bda15c",
        "#472b1b", "#6e442c", "#946043", "#b88569",
        "#320007", "#5d0014", "#8b0e28", "#b83646",
        "#081431", "#182c60", "#2f4b91", "#5174c4",
        "#1d0f27", "#3d234e", "#613e78", "#8b64a8",
        "#4c473d", "#797161", "#a89d88", "#d7cdb8",
        "#09200b", "#18421c", "#2c6a31", "#4e9a52",
        "#6a2500", "#a74b00", "#df8700", "#ffca70",
        "#2d4e54", "#497b83", "#6daab5", "#a4d8e2",
        "#08070b", "#15121a", "#25222c", "#3a3642",
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

    /// <summary>
    /// PR-14 exit test 1. Every pixel of the atlas lies on one ramp of the palette: at a color, at a fine shade, or at
    /// a blend of two fine shades next to each other (D-599).
    /// </summary>
    [Fact]
    public void GeneratorUsesPaletteOnly()
    {
        PaletteShades shades = new(RepositoryPalette());
        PngImage image = PngReader.Read(TextureGenCommand.Generate(ContentRoot()).Atlas);

        for (int pixel = 0; pixel < image.Pixels.Length; pixel++)
        {
            Assert.True(shades.Contains(image.Pixels[pixel]), $"The pixel {pixel} holds {PaletteShades.Hex(image.Pixels[pixel])}, which lies on no single ramp of the palette.");
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

    /// <summary>PR-14 exit test 4. The atlas is a square of 1024 pixels, a power of two, that holds whole block canvases (D-604).</summary>
    [Fact]
    public void AtlasIsPowerOfTwo()
    {
        PngImage image = PngReader.Read(TextureGenCommand.Generate(ContentRoot()).Atlas);

        Assert.Equal(1024, AtlasLayout.AtlasPixels);
        Assert.Equal(AtlasLayout.AtlasPixels, image.Width);
        Assert.Equal(AtlasLayout.AtlasPixels, image.Height);
        Assert.Equal(0, image.Width & (image.Width - 1));
        Assert.Equal(0, image.Width % AtlasLayout.BlockPixels);
    }

    /// <summary>The palette file holds the choice of the owner: twenty ramps of four colors, with the values of D-304, D-530, D-592, and D-600.</summary>
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
    /// Each fine shade lies a quarter, a half, or three quarters of the way between its two colors in linear light, to
    /// within one byte for each channel (D-528). The list of atlas colors holds the colors first, and the soot ramp takes
    /// the palette past the 256 colors of an indexed PNG (D-598, D-600).
    /// </summary>
    [Fact]
    public void EachShadeLiesBetweenItsColorsInLinearLight()
    {
        Palette palette = RepositoryPalette();

        Assert.Equal(260, palette.AtlasColors.Count);
        for (int ramp = 0; ramp < palette.Ramps.Count; ramp++)
        {
            PaletteRamp named = palette.Ramps[ramp];
            for (int fineStep = 0; fineStep <= palette.FineTop(ramp); fineStep++)
            {
                AtlasColor shade = palette.AtlasColors[palette.AtlasIndex(ramp, fineStep)];
                PaletteColor dark = palette.Colors[named.First + (fineStep / Palette.ShadesPerStep)];
                PaletteColor light = palette.Colors[named.First + System.Math.Min((fineStep / Palette.ShadesPerStep) + 1, named.Count - 1)];
                double share = (fineStep % Palette.ShadesPerStep) / (double)Palette.ShadesPerStep;
                AssertChannel(dark.Red, light.Red, share, shade.Red, named.Name, fineStep);
                AssertChannel(dark.Green, light.Green, share, shade.Green, named.Name, fineStep);
                AssertChannel(dark.Blue, light.Blue, share, shade.Blue, named.Name, fineStep);
            }
        }

        Assert.Equal(0, palette.AtlasIndex(0, 0));
        Assert.Equal(palette.Colors.Count, palette.Ramps[0].ShadeFirst);
    }

    /// <summary>
    /// The table of linear light holds the sRGB transfer function of each byte in 65535ths, rounded to the nearest whole
    /// number, and each byte comes back from its own value (D-599). A value outside the table is an error.
    /// </summary>
    [Fact]
    public void LinearLightFollowsTheTransferFunction()
    {
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            double expected = Linear((byte)value) * LinearLight.Full;
            int actual = LinearLight.Of((byte)value);
            Assert.True(System.Math.Abs(expected - actual) <= 0.5, $"The byte {value} holds the linear light {actual}, and the transfer function gives {expected:F2}.");
            Assert.Equal((byte)value, LinearLight.ToByte(actual));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => LinearLight.ToByte(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => LinearLight.ToByte(LinearLight.Full + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => LinearLight.Blend(0, 255, Palette.PartsPerFineStep + 1, Palette.PartsPerFineStep));

        // A count of zero parts fails with its own context, and never divides by zero (PR #107 review P2-1).
        ArgumentOutOfRangeException noParts = Assert.Throws<ArgumentOutOfRangeException>(() => LinearLight.Blend(0, 255, 0, 0));
        Assert.Contains("The count of parts is 0", noParts.Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => LinearLight.Blend(0, 255, 0, -1));
        Assert.Equal((byte)0, LinearLight.Blend(0, 255, 0, Palette.PartsPerFineStep));
        Assert.Equal((byte)255, LinearLight.Blend(0, 255, Palette.PartsPerFineStep, Palette.PartsPerFineStep));
    }

    /// <summary>
    /// A position between two fine shades takes the blend of the two in linear light, to within one byte for each
    /// channel. A whole fine step takes its exact shade, and a position off the ramp is an error (D-599).
    /// </summary>
    [Fact]
    public void PositionBlendsItsShadesInLinearLight()
    {
        Palette palette = RepositoryPalette();
        int[] weights = [0, 1, 64, 128, 200, 255];
        for (int ramp = 0; ramp < palette.Ramps.Count; ramp++)
        {
            string name = palette.Ramps[ramp].Name;
            for (int fineStep = 0; fineStep < palette.FineTop(ramp); fineStep++)
            {
                AtlasColor dark = palette.AtlasColors[palette.AtlasIndex(ramp, fineStep)];
                AtlasColor light = palette.AtlasColors[palette.AtlasIndex(ramp, fineStep + 1)];
                Assert.Equal(dark, palette.ColorAt(ramp, fineStep * Palette.PartsPerFineStep));
                foreach (int weight in weights)
                {
                    AtlasColor color = palette.ColorAt(ramp, (fineStep * Palette.PartsPerFineStep) + weight);
                    double share = weight / (double)Palette.PartsPerFineStep;
                    AssertChannel(dark.Red, light.Red, share, color.Red, name, fineStep);
                    AssertChannel(dark.Green, light.Green, share, color.Green, name, fineStep);
                    AssertChannel(dark.Blue, light.Blue, share, color.Blue, name, fineStep);
                }
            }

            int top = palette.FineTop(ramp) * Palette.PartsPerFineStep;
            Assert.Equal(palette.AtlasColors[palette.AtlasIndex(ramp, palette.FineTop(ramp))], palette.ColorAt(ramp, top));
            Assert.Throws<ArgumentOutOfRangeException>(() => palette.ColorAt(ramp, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => palette.ColorAt(ramp, top + 1));
        }
    }

    /// <summary>
    /// PR-62 exit test 2. The recipe of each block, painted at the tile size of PR-14, gives the pixels of its tile in the
    /// atlas of PR-14 (D-504, D-309). The block recipes stay as they are at 64 texels per meter (D-608), so a change of a
    /// block recipe fails here. The test turns each color back into its atlas index.
    /// </summary>
    [Theory]
    [MemberData(nameof(BlockTilesOfPr14))]
    public void BlockCanvasEqualsItsTileOfPr14(int block, string hash)
    {
        Palette palette = RepositoryPalette();
        PaletteShades shades = new(palette);
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll(TextureGenCommand.ReadRecipeFiles(ContentRoot()), palette);
        BlockPlace place = RepositoryTextures.Layout.Blocks.Single(candidate => candidate.Block == block);

        AtlasColor[] tile = CanvasPainter.Paint(palette, recipes[place.Recipe], TilePixelsOfPr14, TilePixelsOfPr14, CanvasPainter.BlockSalt, place.Recipe);

        byte[] pixels = tile.Select(color => (byte)shades.Index(color)).ToArray();
        Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant());
    }

    /// <summary>
    /// A canvas of 32 by 32 texels of each of three block recipes equals the tile of the palette preview that the owner
    /// chose from (D-304): the first row, and the count of each palette index. The skin of D-531 is no longer the skin
    /// tile of the preview.
    /// </summary>
    [Theory]
    [InlineData("raw-stone", "0,1,1,1,1,0,1,0,1,0,0,0,1,1,1,1", "0:194,1:614,2:216")]
    [InlineData("hewn-stone", "4,5,5,5,5,5,5,5,5,6,5,5,5,6,5,5", "4:15,5:214,6:707,7:88")]
    [InlineData("ore-vein", "12,12,12,13,12,12,12,12,12,12,13,13,12,12,12,13", "12:753,13:271")]
    public void BlockCanvasesMatchThePalettePreview(string recipe, string firstRow, string counts)
    {
        Palette palette = RepositoryPalette();
        PaletteShades shades = new(palette);
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll(TextureGenCommand.ReadRecipeFiles(ContentRoot()), palette);
        int[] pixels = CanvasPainter.Paint(palette, recipes[recipe], TilePixelsOfPr14, TilePixelsOfPr14, CanvasPainter.BlockSalt, recipe).Select(shades.Index).ToArray();

        Assert.Equal(firstRow, string.Join(",", pixels.Take(16)));
        string histogram = string.Join(",", pixels.GroupBy(value => value).OrderBy(group => group.Key).Select(group => $"{group.Key}:{group.Count()}"));
        Assert.Equal(counts, histogram);
    }

    /// <summary>
    /// PR-62 exit test 4. Each face of the player and the sword has a canvas in the committed layout, of its size at 64
    /// texels per meter, and the box mesh reads that many texels of it (D-308, D-505, D-603).
    /// </summary>
    [Theory]
    [InlineData("models/player.bbmodel", 15)]
    [InlineData("models/sword-basic.bbmodel", 8)]
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
    /// The paint files bind each face to its recipe. Each face of a trace spec reads the recipe that its trace wrote (D-612).
    /// A face that no view shows keeps its procedural recipe: the cloth, the skin, and the boot toe on the body. Every other
    /// face reads the recipe of its box, such as the leather at the ends of the grip.
    /// </summary>
    [Fact]
    public void PaintFilesBindTheBodyArt()
    {
        const string Player = "models/player.bbmodel:";
        const string Sword = "models/sword-basic.bbmodel:";
        const string Scavenger = "models/scavenger.bbmodel:";
        const string Overseer = "models/overseer.bbmodel:";
        Dictionary<string, string> boxRecipe = new()
        {
            [Player + "head_box"] = "hair",
            [Player + "brow_box"] = "hair",
            [Player + "nose_box"] = "skin",
            [Player + "beard_box"] = "hair",
            [Player + "torso_box"] = "torso",
            [Player + "arm_left_upper_box"] = "cloth",
            [Player + "arm_left_lower_box"] = "sleeve",
            [Player + "arm_right_upper_box"] = "cloth",
            [Player + "arm_right_lower_box"] = "sleeve",
            [Player + "leg_left_upper_box"] = "trousers",
            [Player + "leg_left_lower_box"] = "boot",
            [Player + "toe_left_box"] = "boot-toe",
            [Player + "leg_right_upper_box"] = "trousers",
            [Player + "leg_right_lower_box"] = "boot",
            [Player + "toe_right_box"] = "boot-toe",
            [Sword + "pommel_box"] = "iron",
            [Sword + "grip_box"] = "leather",
            [Sword + "guard_box"] = "iron",
            [Sword + "guard_left_box"] = "iron",
            [Sword + "guard_right_box"] = "iron",
            [Sword + "blade_box"] = "metal",
            [Sword + "tip_step_box"] = "metal",
            [Sword + "tip_box"] = "metal",
            [Scavenger + "head_box"] = "scavenger-hood",
            [Scavenger + "hood_top_box"] = "scavenger-hood",
            [Scavenger + "hood_left_box"] = "scavenger-hood",
            [Scavenger + "hood_right_box"] = "scavenger-hood",
            [Scavenger + "hood_back_box"] = "scavenger-hood",
            [Scavenger + "scarf_box"] = "scavenger-hood",
            [Scavenger + "sack_box"] = "scavenger-sack",
            [Scavenger + "torso_box"] = "scavenger-coat",
            [Scavenger + "arm_left_upper_box"] = "scavenger-coat",
            [Scavenger + "arm_left_lower_box"] = "scavenger-sleeve",
            [Scavenger + "arm_right_upper_box"] = "scavenger-coat",
            [Scavenger + "arm_right_lower_box"] = "scavenger-sleeve",
            [Scavenger + "leg_left_upper_box"] = "trousers",
            [Scavenger + "leg_left_lower_box"] = "boot",
            [Scavenger + "toe_left_box"] = "boot-toe",
            [Scavenger + "leg_right_upper_box"] = "trousers",
            [Scavenger + "leg_right_lower_box"] = "boot",
            [Scavenger + "toe_right_box"] = "boot-toe",
            [Overseer + "head_box"] = "overseer-coat",
            [Overseer + "brim_box"] = "overseer-helmet",
            [Overseer + "crown_box"] = "overseer-helmet",
            [Overseer + "lamp_box"] = "overseer-helmet",
            [Overseer + "goggle_left_box"] = "overseer-steel",
            [Overseer + "goggle_right_box"] = "overseer-steel",
            [Overseer + "mask_box"] = "overseer-linen",
            [Overseer + "can_left_box"] = "overseer-steel",
            [Overseer + "can_right_box"] = "overseer-steel",
            [Overseer + "torso_box"] = "overseer-coat",
            [Overseer + "arm_left_upper_box"] = "overseer-coat",
            [Overseer + "arm_left_lower_box"] = "overseer-coat",
            [Overseer + "arm_right_upper_box"] = "overseer-coat",
            [Overseer + "arm_right_lower_box"] = "overseer-coat",
            [Overseer + "leg_left_upper_box"] = "overseer-coat",
            [Overseer + "leg_left_lower_box"] = "overseer-leather",
            [Overseer + "toe_left_box"] = "overseer-leather",
            [Overseer + "leg_right_upper_box"] = "overseer-coat",
            [Overseer + "leg_right_lower_box"] = "overseer-leather",
            [Overseer + "toe_right_box"] = "overseer-leather",
        };
        Dictionary<string, string> faceRecipe = new()
        {
            [Player + "torso_box:up"] = "cloth",
            [Player + "torso_box:down"] = "cloth",
            [Player + "arm_left_lower_box:up"] = "cloth",
            [Player + "arm_left_lower_box:down"] = "skin",
            [Player + "arm_right_lower_box:up"] = "cloth",
            [Player + "arm_right_lower_box:down"] = "skin",
            [Player + "leg_left_lower_box:up"] = "boot-toe",
            [Player + "leg_left_lower_box:down"] = "boot-toe",
            [Player + "leg_right_lower_box:up"] = "boot-toe",
            [Player + "leg_right_lower_box:down"] = "boot-toe",
            [Scavenger + "arm_left_lower_box:up"] = "scavenger-coat",
            [Scavenger + "arm_left_lower_box:down"] = "skin",
            [Scavenger + "arm_right_lower_box:up"] = "scavenger-coat",
            [Scavenger + "arm_right_lower_box:down"] = "skin",
            [Scavenger + "leg_left_lower_box:up"] = "boot-toe",
            [Scavenger + "leg_left_lower_box:down"] = "boot-toe",
            [Scavenger + "leg_right_lower_box:up"] = "boot-toe",
            [Scavenger + "leg_right_lower_box:down"] = "boot-toe",

            // The torso sides paint the belt of D-697. An inner face of a limb reads the traced outer face of the other limb.
            [Overseer + "torso_box:east"] = "overseer-torso-side",
            [Overseer + "torso_box:west"] = "overseer-torso-side",
            [Overseer + "arm_left_upper_box:east"] = "overseer-arm-right-upper-east",
            [Overseer + "arm_left_lower_box:east"] = "overseer-arm-right-lower-east",
            [Overseer + "arm_right_upper_box:west"] = "overseer-arm-left-upper-west",
            [Overseer + "arm_right_lower_box:west"] = "overseer-arm-left-lower-west",
            [Overseer + "arm_left_lower_box:down"] = "overseer-leather",
            [Overseer + "arm_right_lower_box:down"] = "overseer-leather",
            [Overseer + "leg_left_lower_box:east"] = "overseer-leg-right-lower-east",
            [Overseer + "leg_right_lower_box:west"] = "overseer-leg-left-lower-west",
            [Overseer + "leg_left_lower_box:down"] = "overseer-sole",
            [Overseer + "leg_right_lower_box:down"] = "overseer-sole",
            [Overseer + "toe_left_box:east"] = "overseer-toe-right-east",
            [Overseer + "toe_right_box:west"] = "overseer-toe-left-west",
            [Overseer + "toe_left_box:down"] = "overseer-sole",
            [Overseer + "toe_right_box:down"] = "overseer-sole",
        };

        IReadOnlyList<TraceFace> traced = RepositoryTraceFaces();
        Assert.Equal(TracedFaceCount, traced.Count);
        foreach (TraceFace face in traced)
        {
            faceRecipe.Add(face.Model + ":" + face.Box + ":" + BoxFaces.Name(face.Side), face.Recipe);
        }

        // The armor overlays of PR-22 have a test of their own, because an undrawn face takes no place (D-767).
        List<FacePlace> places = [];
        foreach (FacePlace place in RepositoryTextures.Layout.Faces)
        {
            if (!place.Model.StartsWith(AssetPaths.ArmorDirectory, StringComparison.Ordinal))
            {
                places.Add(place);
            }
        }

        Assert.Equal(boxRecipe.Count * BoxFaces.Names.Count, places.Count);
        foreach (FacePlace place in places)
        {
            string box = place.Model + ":" + place.Box;
            string face = box + ":" + BoxFaces.Name(place.Side);
            string expected = faceRecipe.TryGetValue(face, out string? own) ? own : boxRecipe[box];
            Assert.True(expected == place.Recipe, $"The face {face} reads the recipe '{place.Recipe}', and the traces of D-612 and the paint of D-525 to D-531 give '{expected}'.");
        }
    }

    /// <summary>
    /// Each traced face of the committed atlas holds only the ramps that its trace spec names (D-612). The head front
    /// names umber and clay alone, so it holds no eye white (D-83). The blade names steel alone, and the grip umber
    /// alone (D-615).
    /// </summary>
    [Fact]
    public void TracedFacesHoldTheirRamps()
    {
        PngImage atlas = PngReader.Read(File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage)));
        Palette palette = RepositoryPalette();
        foreach (TraceFace face in RepositoryTraceFaces())
        {
            new ModelCanvas(atlas, palette, face.Model, face.Box, face.Side).AssertOnRamps(face.Ramps);
        }
    }

    /// <summary>
    /// The skin of the faces that no view shows is clay 2 plus 3 shades, the skin of the traced faces (D-616). The grain of
    /// amount 1 moves a texel by 2 fine steps at most, and clay ends at fine step 12.
    /// </summary>
    [Fact]
    public void UntracedSkinIsClay()
    {
        PngImage atlas = PngReader.Read(File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage)));
        Palette palette = RepositoryPalette();
        new ModelCanvas(atlas, palette, AssetPaths.BodyModel, "arm_left_lower_box", BoxSide.Down).AssertShades("clay", 9, 12, 0, 0, 12, 12);
        new ModelCanvas(atlas, palette, AssetPaths.BodyModel, "nose_box", BoxSide.East).AssertShades("clay", 9, 12, 0, 0, 3, 6);
    }

    /// <summary>
    /// The paint of the scavenger (PR-76). The rope belt is a linen band 2 texels tall on the four torso faces, with a
    /// knot of 3 by 3 and two ends 2 texels wide on the front (D-672). The eye band is skin, and each eye is dark skin of
    /// 2 by 3 texels with no white (D-83, D-669, D-671). The hood reads soot 1 or lighter (D-666).
    /// </summary>
    [Fact]
    public void ScavengerPaintFollowsItsDecisions()
    {
        const string Scavenger = "models/scavenger.bbmodel";
        PngImage atlas = PngReader.Read(File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage)));
        Palette palette = RepositoryPalette();
        foreach ((BoxSide side, int width) in new[] { (BoxSide.North, 40), (BoxSide.South, 40), (BoxSide.East, 20), (BoxSide.West, 20) })
        {
            new ModelCanvas(atlas, palette, Scavenger, "torso_box", side).AssertRamp("linen", 0, 29, width, 2);
        }

        ModelCanvas front = new(atlas, palette, Scavenger, "torso_box", BoxSide.North);
        front.AssertRamp("linen", 22, 31, 3, 3);
        front.AssertRamp("linen", 21, 34, 2, 5);
        front.AssertRamp("linen", 24, 34, 2, 5);

        ModelCanvas face = new(atlas, palette, Scavenger, "head_box", BoxSide.North);
        face.AssertRamp("clay", 4, 11, 24, 7);
        face.AssertShades("clay", 0, 0, 9, 15, 2, 3);
        face.AssertShades("clay", 0, 0, 21, 15, 2, 3);

        new ModelCanvas(atlas, palette, Scavenger, "hood_back_box", BoxSide.South).AssertShades("soot", 4, 12, 0, 0, 40, 32);
        new ModelCanvas(atlas, palette, Scavenger, "hood_top_box", BoxSide.Up).AssertShades("soot", 4, 12, 0, 0, 40, 40);
    }

    /// <summary>
    /// The flat of the blade reads as mid steel with lighter edges (D-615). The dark center of the Meshy blade takes the
    /// light half of the steel ramp: no center texel is under fine step 5, and the center averages 6 or more. The edge
    /// columns average 2 fine steps more than the center. The edge faces of the blade are on the same light half.
    /// </summary>
    [Fact]
    public void BladeFlatIsMidSteel()
    {
        PngImage atlas = PngReader.Read(File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.AtlasImage)));
        Palette palette = RepositoryPalette();
        const string Sword = "models/sword-basic.bbmodel";

        foreach (BoxSide side in new[] { BoxSide.North, BoxSide.South })
        {
            ModelCanvas flat = new(atlas, palette, Sword, "blade_box", side);
            flat.AssertShades("steel", 5, 12, 1, 0, 4, 34);
            double center = flat.MeanFine("steel", 1, 0, 4, 34);
            double edges = (flat.MeanFine("steel", 0, 0, 1, 34) + flat.MeanFine("steel", 5, 0, 1, 34)) / 2.0;
            Assert.True(center >= 6.0, $"The center of the blade {side} face averages {center:F2} fine steps of steel, and mid steel is 6 or more (D-615).");
            Assert.True(edges >= center + 2.0, $"The edges of the blade {side} face average {edges:F2} fine steps of steel, and the center {center:F2}. The edges are 2 or more lighter (D-615).");
        }

        foreach (BoxSide side in new[] { BoxSide.East, BoxSide.West })
        {
            new ModelCanvas(atlas, palette, Sword, "blade_box", side).AssertShades("steel", 5, 12, 0, 0, 2, 34);
        }
    }

    /// <summary>Every block id other than air has a canvas of 64 pixels in the committed layout, bound to the recipe of its material (D-259, D-505, D-603).</summary>
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
                    AtlasColor gutter = image.Pixels[((place.Y + y) * image.Width) + place.X + x];
                    AtlasColor source = image.Pixels[((place.Y + sourceY) * image.Width) + place.X + sourceX];
                    Assert.True(gutter == source, $"The pixel ({x}, {y}) of the canvas at ({place.X}, {place.Y}) holds {PaletteShades.Hex(gutter)}, and the nearest canvas pixel holds {PaletteShades.Hex(source)}.");
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
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\"], \"shades\": []}, {\"name\": \"rock\", \"colors\": [\"#202020\"], \"shades\": []}]}", "name", "an earlier ramp has that name")]
    [InlineData("{\"ramps\": [{\"name\": \"\", \"colors\": [\"#101010\"], \"shades\": []}]}", "name", "is empty")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\"], \"glow\": true}]}", "glow", "not a field of the format")]
    [InlineData("{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\"], \"shades\": []}], \"count\": 1}", "count", "not a field of the format")]
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

    /// <summary>
    /// PR-89 exit test 2. The palette has no count limit (D-598): a palette of 1024 colors loads, and a canvas paints
    /// its last color. An indexed PNG held 256 at most.
    /// </summary>
    [Fact]
    public void PaletteHoldsMoreColorsThanAnIndexedPng()
    {
        IEnumerable<string> ramps = Enumerable.Range(0, 1024).Select(index => $"{{\"name\": \"r{index}\", \"colors\": [\"#{index:x6}\"], \"shades\": []}}");
        string json = "{\"ramps\": [" + string.Join(", ", ramps) + "]}";

        Palette palette = Palette.Parse(AssetPaths.PaletteFile, Encoding.UTF8.GetBytes(json));
        Recipe recipe = RecipeFile.ReadAll([(AssetPaths.RecipeDirectory + "last.json", Encoding.UTF8.GetBytes("{\"layers\": [{\"kind\": \"fill\", \"color\": 1023, \"shade\": 0, \"noise\": 0.0, \"seed\": 1}]}"))], palette)["last"];
        AtlasColor[] pixels = CanvasPainter.Paint(palette, recipe, 2, 2, CanvasPainter.BlockSalt, "test");

        Assert.Equal(1024, palette.AtlasColors.Count);
        Assert.All(pixels, pixel => Assert.Equal(new AtlasColor(0, 3, 255), pixel));
    }

    /// <summary>A ramp with the wrong count of shades is an error that names the ramp and the count (D-528).</summary>
    [Fact]
    public void PaletteRejectsAWrongCountOfShades()
    {
        string json = "{\"ramps\": [{\"name\": \"rock\", \"colors\": [\"#101010\", \"#202020\"], \"shades\": [\"#151515\"]}]}";

        ContextException error = Assert.Throws<ContextException>(() => Palette.Parse(AssetPaths.PaletteFile, Encoding.UTF8.GetBytes(json)));

        Assert.Contains("The field 'shades'", error.Message, StringComparison.Ordinal);
        Assert.Contains("on the ramp 'rock' is not a list of 3 shades", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A small image goes through the writer and the strict reader with its size and the color of each pixel.</summary>
    [Fact]
    public void PngWriterRoundTrips()
    {
        AtlasColor[] pixels = [new AtlasColor(1, 2, 3), new AtlasColor(250, 128, 7), new AtlasColor(0, 0, 0), new AtlasColor(255, 255, 255), new AtlasColor(9, 8, 7), new AtlasColor(1, 2, 3)];

        PngImage image = PngReader.Read(PngWriter.Write(3, 2, pixels));

        Assert.Equal(3, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(pixels, image.Pixels);
    }

    /// <summary>Image data past one stored block goes into several blocks, and the platform decompressor reads them back.</summary>
    [Fact]
    public void PngWriterSplitsLongDataIntoStoredBlocks()
    {
        AtlasColor[] pixels = Enumerable.Range(0, 300 * 300).Select(index => new AtlasColor((byte)index, (byte)(index >> 8), (byte)(index % 7))).ToArray();

        PngImage image = PngReader.Read(PngWriter.Write(300, 300, pixels));

        Assert.True(((300 * 3) + 1) * 300 > PngWriter.MaxStoredBlock);
        Assert.Equal(pixels, image.Pixels);
    }

    /// <summary>A pixel count that differs from the size, and a size of zero, are each an error.</summary>
    [Fact]
    public void PngWriterRejectsABadImage()
    {
        AtlasColor color = new(1, 2, 3);

        ArgumentException count = Assert.Throws<ArgumentException>(() => PngWriter.Write(2, 2, [color, color, color]));
        Assert.Contains("The image holds 3 pixels", count.Message, StringComparison.Ordinal);
        ArgumentException size = Assert.Throws<ArgumentException>(() => PngWriter.Write(0, 1, []));
        Assert.Contains("must be positive", size.Message, StringComparison.Ordinal);
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
        content.Write(AssetPaths.RecipeDirectory + "stone.json", "{\"layers\": [{\"kind\": \"fill\", \"color\": 99, \"shade\": 0, \"noise\": 0.4, \"seed\": 1001}]}");

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
    private const string StoneRecipe = "{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.4, \"seed\": 1001}]}";

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

    /// <summary>One face canvas of a model in the committed atlas, read by texel from its top left as a ramp and a fine step.</summary>
    private sealed class ModelCanvas
    {
        private readonly PngImage atlas;
        private readonly Palette palette;
        private readonly AtlasRect at;
        private readonly string name;
        private readonly PaletteShades shades;

        public ModelCanvas(PngImage atlas, Palette palette, string model, string box, BoxSide side)
        {
            this.atlas = atlas;
            this.palette = palette;
            this.at = RepositoryTextures.Layout.Face(model, box, side);
            this.name = TextureLayout.FaceName(model, box, side);
            this.shades = new PaletteShades(palette);
        }

        /// <summary>Asserts that every texel of a rectangle lies on the named ramp.</summary>
        public void AssertRamp(string ramp, int x, int y, int width, int height)
        {
            this.AssertShades(ramp, 0, int.MaxValue, x, y, width, height);
        }

        /// <summary>
        /// Asserts that every texel of a rectangle lies on the named ramp. One fine step asks for that exact shade. A
        /// range of fine steps, from the lowest to the highest, allows half a fine step more at each end.
        /// </summary>
        public void AssertShades(string ramp, int lowest, int highest, int x, int y, int width, int height)
        {
            const long Half = Palette.PartsPerFineStep / 2;
            for (int row = y; row < y + height; row++)
            {
                for (int column = x; column < x + width; column++)
                {
                    AtlasColor color = this.Color(column, row);
                    int rampIndex = this.shades.RampOf(ramp);
                    bool onRamp = this.shades.TryPlace(color, rampIndex, out RampPlace place);
                    bool inside = lowest == highest
                        ? this.palette.AtlasColors[this.palette.AtlasIndex(rampIndex, lowest)] == color
                        : place.High >= (lowest * (long)Palette.PartsPerFineStep) - Half && place.Low <= (highest * (long)Palette.PartsPerFineStep) + Half;
                    Assert.True(onRamp && inside, $"The texel ({column}, {row}) of {this.name} holds {PaletteShades.Hex(color)}, at {place.Low} to {place.High} parts of a fine step when on '{ramp}' ({onRamp}), and the owner layout gives '{ramp}' at the fine steps {lowest} to {highest}.");
                }
            }
        }

        /// <summary>Asserts that every texel of the canvas lies on one of the ramps, by palette ramp index.</summary>
        public void AssertOnRamps(IReadOnlyList<int> ramps)
        {
            string names = string.Join(", ", ramps.Select(ramp => this.palette.Ramps[ramp].Name));
            for (int row = 0; row < this.at.Height; row++)
            {
                for (int column = 0; column < this.at.Width; column++)
                {
                    AtlasColor color = this.Color(column, row);
                    bool onRamp = false;
                    foreach (int ramp in ramps)
                    {
                        onRamp |= this.shades.TryPlace(color, ramp, out RampPlace _);
                    }

                    Assert.True(onRamp, $"The texel ({column}, {row}) of {this.name} holds {PaletteShades.Hex(color)}, and its trace spec gives the ramps {names}.");
                }
            }
        }

        /// <summary>The mean position of the texels of a rectangle on one ramp, in fine steps. A texel off the ramp fails the test.</summary>
        public double MeanFine(string ramp, int x, int y, int width, int height)
        {
            int rampIndex = this.shades.RampOf(ramp);
            double sum = 0.0;
            for (int row = y; row < y + height; row++)
            {
                for (int column = x; column < x + width; column++)
                {
                    AtlasColor color = this.Color(column, row);
                    Assert.True(this.shades.TryPlace(color, rampIndex, out RampPlace place), $"The texel ({column}, {row}) of {this.name} holds {PaletteShades.Hex(color)}, which is not on '{ramp}'.");
                    sum += (place.Low + place.High) / 2.0 / Palette.PartsPerFineStep;
                }
            }

            return sum / (width * height);
        }

        private AtlasColor Color(int column, int row)
        {
            return this.atlas.Pixels[((this.at.Y + row) * this.atlas.Width) + this.at.X + column];
        }
    }

    /// <summary>Asserts that one channel of a shade lies within one byte of the interpolation in linear light.</summary>
    private static void AssertChannel(byte dark, byte light, double share, byte actual, string ramp, int fineStep)
    {
        double linear = (Linear(dark) * (1.0 - share)) + (Linear(light) * share);
        double expected = 255.0 * (linear <= 0.0031308 ? linear * 12.92 : (1.055 * System.Math.Pow(linear, 1.0 / 2.4)) - 0.055);
        Assert.True(System.Math.Abs(expected - actual) <= 1.0, $"The fine step {fineStep} of the ramp '{ramp}' holds {actual} in a channel, and linear light gives {expected:F2} (D-528).");
    }

    private static double Linear(byte channel)
    {
        double value = channel / 255.0;
        return value <= 0.04045 ? value / 12.92 : System.Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    private static string ContentRoot()
    {
        return Path.Combine(RepositoryRoot.Find(), "content");
    }

    private static Palette RepositoryPalette()
    {
        return Palette.Parse(AssetPaths.PaletteFile, File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.PaletteFile)));
    }

    /// <summary>Every face of the trace specs of the repository, spec by spec in the order of <see cref="TraceSpecs"/>.</summary>
    private static List<TraceFace> RepositoryTraceFaces()
    {
        Palette palette = RepositoryPalette();
        List<TraceFace> faces = [];
        foreach (string spec in TraceSpecs)
        {
            string path = AssetPaths.TraceDirectory + spec + ".json";
            faces.AddRange(TraceSpecFile.Parse(path, File.ReadAllBytes(Path.Combine(ContentRoot(), path)), palette).Faces);
        }

        return faces;
    }
}
