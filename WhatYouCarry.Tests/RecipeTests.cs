using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.TextureGen;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The recipe system of PR-62: the layer kinds, the extension with a swap, the binding files, the packer, and the
/// layout file (D-505 to D-508; PR-62 exit test 3).
/// </summary>
public sealed class RecipeTests
{
    private const string RecipePath = "textures/recipes/test.json";
    private const string PaintPath = "models/rig.paint.json";

    /// <summary>The flat index of the second color of the timber ramp of D-304.</summary>
    private const int TimberTwo = 9;

    /// <summary>Over two hundred seeds, heavy noise and a deep edge never move a pixel off the ramp of its color. A failure names the seed (D-66).</summary>
    [Fact]
    public void NoiseStaysOnTheRampOfItsColor()
    {
        Palette palette = RepositoryPalette();
        PaletteRamp timber = palette.Ramps[2];
        for (uint seed = 1; seed <= 200; seed++)
        {
            Recipe recipe = Layers(new FillLayer(TimberTwo, 0, 0.9, seed), new EdgeLayer(3));
            int[] pixels = PaintIndices(palette, recipe, 20, 12, CanvasPainter.BlockSalt, "test");

            Assert.All(pixels, value => Assert.True(value >= timber.First && value < timber.First + timber.Count, $"Seed {seed}: a pixel holds the index {value}, off the ramp '{timber.Name}'."));
            Assert.True(pixels.Distinct().Count() >= 3, $"Seed {seed}: heavy noise gave {pixels.Distinct().Count()} indices, and it moves pixels both down and up the ramp.");
        }
    }

    /// <summary>With no noise, an edge moves the outer ring down the ramp, and the inside keeps the color. A deep edge stops at the dark end.</summary>
    [Fact]
    public void EdgeDarkensTheRing()
    {
        Palette palette = RepositoryPalette();
        int first = palette.Ramps[2].First;

        int[] one = PaintIndices(palette, Layers(new FillLayer(first + 2, 0, 0.0, 7), new EdgeLayer(1)), 6, 5, CanvasPainter.BlockSalt, "test");
        AssertRingAndInside(one, 6, 5, first + 1, first + 2);

        int[] deep = PaintIndices(palette, Layers(new FillLayer(first + 2, 0, 0.0, 7), new EdgeLayer(5)), 6, 5, CanvasPainter.BlockSalt, "test");
        AssertRingAndInside(deep, 6, 5, first, first + 2);
    }

    /// <summary>
    /// The painter clamps a step to its ramp once, after the last layer, as a rule of PR-14 did (D-309). A light color
    /// with full noise and an edge of one gives a ring of the top step or two steps down, and never one step down.
    /// </summary>
    [Fact]
    public void PainterClampsOnceAfterTheLayers()
    {
        Palette palette = RepositoryPalette();
        int first = palette.Ramps[2].First;
        int[] pixels = PaintIndices(palette, Layers(new FillLayer(first + 3, 0, 1.0, 11), new EdgeLayer(1)), 10, 10, CanvasPainter.BlockSalt, "test");

        for (int x = 0; x < 10; x++)
        {
            Assert.Contains(pixels[x], new[] { first + 1, first + 3 });
        }
    }

    /// <summary>A rectangle takes its color inside the canvas, the canvas clips it, and the pixels outside it keep the fill.</summary>
    [Fact]
    public void RectPaintsItsColorAndTheCanvasClipsIt()
    {
        Palette palette = RepositoryPalette();
        Recipe recipe = Layers(new FillLayer(TimberTwo, 0, 0.0, 3), new RectLayer(6, 2, 10, 2, 29, 0, 0.0, 5));

        int[] pixels = PaintIndices(palette, recipe, 8, 6, CanvasPainter.BlockSalt, "test");

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool inside = x >= 6 && y >= 2 && y < 4;
                Assert.Equal(inside ? 29 : TimberTwo, pixels[(y * 8) + x]);
            }
        }
    }

    /// <summary>A rectangle with no pixel inside the canvas is an error that names the recipe and the canvas, because it paints nothing (T-2).</summary>
    [Fact]
    public void RectOutsideTheCanvasFails()
    {
        Recipe recipe = Layers(new FillLayer(TimberTwo, 0, 0.0, 3), new RectLayer(8, 0, 2, 2, 29, 0, 0.0, 5));

        ContextException error = Assert.Throws<ContextException>(() => CanvasPainter.Paint(RepositoryPalette(), recipe, 8, 6, CanvasPainter.BlockSalt, "models/rig.bbmodel:arm:up"));

        Assert.Contains(RecipePath, error.Message, StringComparison.Ordinal);
        Assert.Contains("models/rig.bbmodel:arm:up", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A band shifts the pixels within its depth of one side, and a band deeper than the canvas covers all of it.</summary>
    [Theory]
    [InlineData(CanvasSide.Top, 2)]
    [InlineData(CanvasSide.Bottom, 1)]
    [InlineData(CanvasSide.Left, 3)]
    [InlineData(CanvasSide.Right, 9)]
    public void BandShiftsOneSide(CanvasSide side, int depth)
    {
        Recipe recipe = Layers(new FillLayer(TimberTwo, 0, 0.0, 3), new BandLayer(side, depth, -1));

        int[] pixels = PaintIndices(RepositoryPalette(), recipe, 7, 5, CanvasPainter.BlockSalt, "test");

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                bool inside = side switch
                {
                    CanvasSide.Top => y < depth,
                    CanvasSide.Bottom => y >= 5 - depth,
                    CanvasSide.Left => x < depth,
                    _ => x >= 7 - depth,
                };
                Assert.Equal(inside ? TimberTwo - 1 : TimberTwo, pixels[(y * 7) + x]);
            }
        }
    }

    /// <summary>Two faces of one recipe show two draws of the noise, and the block salt keeps the draw of the seed alone.</summary>
    [Fact]
    public void FaceSaltsGiveTheirOwnNoise()
    {
        Palette palette = RepositoryPalette();
        Recipe recipe = Layers(new FillLayer(TimberTwo, 0, 0.5, 1009));

        int[] north = PaintIndices(palette, recipe, 16, 16, CanvasPainter.SaltOf("models/player.bbmodel:head_box:north"), "north");
        int[] east = PaintIndices(palette, recipe, 16, 16, CanvasPainter.SaltOf("models/player.bbmodel:head_box:east"), "east");
        int[] block = PaintIndices(palette, recipe, 16, 16, CanvasPainter.BlockSalt, "block");
        int[] again = PaintIndices(palette, recipe, 16, 16, CanvasPainter.BlockSalt, "block");

        Assert.NotEqual(north, east);
        Assert.NotEqual(north, block);
        Assert.Equal(block, again);
        Assert.Equal(0u, CanvasPainter.BlockSalt);
    }

    /// <summary>A seed equal to the salt of a canvas gives the state zero, which never leaves zero, so it is an error that names the recipe and the canvas.</summary>
    [Fact]
    public void SeedEqualToTheSaltFails()
    {
        uint salt = CanvasPainter.SaltOf("models/rig.bbmodel:arm:north");
        Recipe recipe = Layers(new FillLayer(TimberTwo, 0, 0.5, salt));

        ContextException error = Assert.Throws<ContextException>(() => CanvasPainter.Paint(RepositoryPalette(), recipe, 4, 4, salt, "models/rig.bbmodel:arm:north"));

        Assert.Contains(RecipePath, error.Message, StringComparison.Ordinal);
        Assert.Contains("models/rig.bbmodel:arm:north", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-62 exit test 3. A recipe with a bad layer is an error that names the file and the field (D-92, D-168, D-507).</summary>
    [Theory]
    [InlineData("{\"layers\": [{\"kind\": \"glow\"}]}", "kind", "a layer kind is one of")]
    [InlineData("{\"layers\": [{\"kind\": \"edge\", \"steps\": 1}]}", "kind", "the first layer is a 'fill'")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}]}", "kind", "no other layer is one")]
    [InlineData("{\"layers\": []}", "layers", "at least one layer")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 80, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}]}", "color", "names the color index 80")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 1.5, \"seed\": 3}]}", "noise", "from 0 to 1")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 0}]}", "seed", "never leaves zero")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1}]}", "seed", "is absent")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3, \"glow\": 1}]}", "glow", "not a field of the format")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"edge\", \"steps\": 0}]}", "steps", "1 or more")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"rect\", \"x\": -1, \"y\": 0, \"width\": 1, \"height\": 1, \"color\": 1, \"shade\": 0, \"noise\": 0, \"seed\": 3}]}", "x", "0 or more")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"rect\", \"x\": 0, \"y\": 0, \"width\": 0, \"height\": 1, \"color\": 1, \"shade\": 0, \"noise\": 0, \"seed\": 3}]}", "width", "1 or more")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"band\", \"side\": \"middle\", \"depth\": 1, \"shift\": 1}]}", "side", "a side is one of")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"band\", \"side\": \"top\", \"depth\": 1, \"shift\": 0}]}", "shift", "paints nothing")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}], \"extends\": \"stone\"}", "extends", "not a field of the format")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 4, \"noise\": 0.1, \"seed\": 3}]}", "shade", "a shade is from -3 to 3")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 0, \"shade\": -1, \"noise\": 0.1, \"seed\": 3}]}", "shade", "off the end of the ramp")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"noise\": 0.1, \"seed\": 3}]}", "shade", "is absent")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"grain\", \"cell\": 0, \"amount\": 1, \"seed\": 3}]}", "cell", "1 or more")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"grain\", \"cell\": 2, \"amount\": 0, \"seed\": 3}]}", "amount", "1 or more")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"grain\", \"cell\": 2, \"amount\": 13, \"seed\": 3}]}", "amount", "the length of the longest ramp")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"grain\", \"cell\": 2, \"amount\": 1, \"seed\": 3, \"shade\": 1}]}", "shade", "not a field of the format")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"gradient\", \"side\": \"top\", \"depth\": 2, \"shift\": 0}]}", "shift", "paints nothing")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"gradient\", \"side\": \"top\", \"depth\": 2, \"shift\": -13}]}", "shift", "the length of the longest ramp")]
    [InlineData("{\"layers\": [{\"kind\": \"fill\", \"color\": 1, \"shade\": 0, \"noise\": 0.1, \"seed\": 3}, {\"kind\": \"gradient\", \"side\": \"top\", \"depth\": 0, \"shift\": 1}]}", "depth", "1 or more")]
    public void RecipeRejectsABadLayer(string json, string field, string reason)
    {
        ContextException error = Assert.Throws<ContextException>(() => ReadOne(json));

        Assert.Contains(RecipePath, error.Message, StringComparison.Ordinal);
        Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A shade names a fine step between the colors of a ramp (D-527, D-528): the step of the color times four, plus the shade.</summary>
    [Theory]
    [InlineData(-3)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public void ShadeNamesAFineStep(int shade)
    {
        Palette palette = RepositoryPalette();

        int[] pixels = PaintIndices(palette, Layers(new FillLayer(TimberTwo, shade, 0.0, 3)), 4, 3, CanvasPainter.BlockSalt, "test");

        int expected = palette.AtlasIndex(2, Palette.ShadesPerStep + shade);
        Assert.All(pixels, value => Assert.Equal(expected, value));
        Assert.Equal(shade == 0, expected == TimberTwo);
    }

    /// <summary>
    /// Over a hundred seeds, a grain keeps every pixel on its ramp, moves pixels both down and up, and puts pixels
    /// between two fine shades. A failure names the seed (D-66, D-527, D-599).
    /// </summary>
    [Fact]
    public void GrainStaysOnTheRampAndMovesBothWays()
    {
        Palette palette = RepositoryPalette();
        const int Start = 8 * Palette.PartsPerFineStep;
        for (uint seed = 1; seed <= 100; seed++)
        {
            Recipe recipe = Layers(new FillLayer(TimberTwo + 1, 0, 0.0, seed), new GrainLayer(2, 3, seed));
            CanvasPositions canvas = CanvasPainter.PaintPositions(palette, recipe, 12, 10, CanvasPainter.BlockSalt, "test");

            Assert.All(canvas.Ramps, ramp => Assert.True(ramp == 2, $"Seed {seed}: a pixel lies on the ramp {ramp}, off the timber ramp."));
            Assert.True(canvas.Positions.Any(position => position < Start), $"Seed {seed}: the grain moved no pixel down.");
            Assert.True(canvas.Positions.Any(position => position > Start), $"Seed {seed}: the grain moved no pixel up.");
            Assert.True(canvas.Positions.Any(position => position % Palette.PartsPerFineStep != 0), $"Seed {seed}: the grain put no pixel between two fine shades.");
        }
    }

    /// <summary>The color of each painted pixel is the color of its ramp and its position (D-599).</summary>
    [Fact]
    public void PaintGivesTheColorOfEachPosition()
    {
        Palette palette = RepositoryPalette();
        Recipe recipe = Layers(new FillLayer(TimberTwo + 1, 0, 0.4, 5), new GrainLayer(2, 3, 5), new EdgeLayer(1));

        CanvasPositions canvas = CanvasPainter.PaintPositions(palette, recipe, 9, 7, CanvasPainter.BlockSalt, "test");
        AtlasColor[] pixels = CanvasPainter.Paint(palette, recipe, 9, 7, CanvasPainter.BlockSalt, "test");

        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Assert.Equal(palette.ColorAt(canvas.Ramps[pixel], canvas.Positions[pixel]), pixels[pixel]);
        }
    }

    /// <summary>A grain of a larger cell gives larger clusters: neighbor pixels agree more often than with a cell of one pixel (D-527).</summary>
    [Fact]
    public void GrainClustersByItsCell()
    {
        Palette palette = RepositoryPalette();
        double single = 0.0;
        double wide = 0.0;
        for (uint seed = 1; seed <= 40; seed++)
        {
            single += NeighborCorrelation(CanvasPainter.PaintPositions(palette, Layers(new FillLayer(TimberTwo + 1, 0, 0.0, seed), new GrainLayer(1, 3, seed)), 16, 16, CanvasPainter.BlockSalt, "test").Positions, 16);
            wide += NeighborCorrelation(CanvasPainter.PaintPositions(palette, Layers(new FillLayer(TimberTwo + 1, 0, 0.0, seed), new GrainLayer(4, 3, seed)), 16, 16, CanvasPainter.BlockSalt, "test").Positions, 16);
        }

        Assert.True(wide / 40.0 > (single / 40.0) + 0.3, $"The mean neighbor correlation is {wide / 40.0:F2} for a cell of 4 and {single / 40.0:F2} for a cell of 1.");
    }

    /// <summary>
    /// A grain uses whole numbers alone, so each platform paints the same bytes (D-527, D-599). The three CI platforms
    /// run this test on one canvas of the trousers recipe: the first eight colors of its first row, and the SHA-256 hash of the red,
    /// green, and blue bytes of all its pixels.
    /// </summary>
    [Fact]
    public void GrainPaintsTheSameBytesOnEachPlatform()
    {
        Palette palette = RepositoryPalette();
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll(TextureGenCommand.ReadRecipeFiles(Path.Combine(RepositoryRoot.Find(), "content")), palette);

        AtlasColor[] pixels = CanvasPainter.Paint(palette, recipes["trousers"], 16, 20, CanvasPainter.SaltOf("models/player.bbmodel:leg_left_upper_box:east"), "east");

        byte[] bytes = pixels.SelectMany(pixel => new[] { pixel.Red, pixel.Green, pixel.Blue }).ToArray();
        Assert.Equal("#1e1408,#241a0d,#342718,#362919,#362919,#3b2d1d,#2b1f11,#1a1107", string.Join(",", pixels.Take(8).Select(PaletteShades.Hex)));
        Assert.Equal("0943ff6bd77a9165e67a9016a65242b50ae9747c8fc4479e283384988d218ba9", Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    /// <summary>A gradient shifts the full amount at its side and less toward its depth, rounded to whole fine steps, and nothing past the depth (D-527).</summary>
    [Fact]
    public void GradientFadesFromItsSide()
    {
        Palette palette = RepositoryPalette();
        Recipe recipe = Layers(new FillLayer(TimberTwo + 1, 0, 0.0, 3), new GradientLayer(CanvasSide.Bottom, 4, -4));

        int[] pixels = PaintIndices(palette, recipe, 3, 6, CanvasPainter.BlockSalt, "test");

        int[] expectedFine = [8, 8, 7, 6, 5, 4];
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Assert.Equal(palette.AtlasIndex(2, expectedFine[y]), pixels[(y * 3) + x]);
            }
        }
    }

    /// <summary>A recipe with neither form is an error that names the file.</summary>
    [Fact]
    public void RecipeNeedsOneForm()
    {
        ContextException error = Assert.Throws<ContextException>(() => ReadOne("{}"));

        Assert.Contains("neither 'layers' nor 'extends'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A recipe that extends another holds its layers with each color of a swapped ramp on the same step of the other ramp (D-505, D-507).</summary>
    [Fact]
    public void ExtensionSwapsTheRamps()
    {
        Palette palette = RepositoryPalette();
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll(
            [
                ("textures/recipes/skin.json", Bytes("{\"layers\": [{\"kind\": \"fill\", \"color\": 29, \"shade\": 0, \"noise\": 0.3, \"seed\": 8}, {\"kind\": \"rect\", \"x\": 0, \"y\": 0, \"width\": 2, \"height\": 2, \"color\": 9, \"shade\": 0, \"noise\": 0, \"seed\": 4}, {\"kind\": \"edge\", \"steps\": 1}]}")),
                ("textures/recipes/goblin.json", Bytes("{\"extends\": \"skin\", \"swap\": {\"bone\": \"lichen\"}}")),
            ],
            palette);

        Recipe goblin = recipes["goblin"];
        Assert.Equal("textures/recipes/goblin.json", goblin.ContentPath);
        Assert.Equal(new FillLayer(25, 0, 0.3, 8), goblin.Layers[0]);
        Assert.Equal(new RectLayer(0, 0, 2, 2, 9, 0, 0.0, 4), goblin.Layers[1]);
        Assert.Equal(new EdgeLayer(1), goblin.Layers[2]);

        int[] skin = PaintIndices(palette, recipes["skin"], 5, 5, CanvasPainter.BlockSalt, "skin");
        int[] green = PaintIndices(palette, goblin, 5, 5, CanvasPainter.BlockSalt, "goblin");
        for (int pixel = 0; pixel < skin.Length; pixel++)
        {
            int expected = palette.Colors[skin[pixel]].Ramp == 7 ? skin[pixel] - 28 + 24 : skin[pixel];
            Assert.Equal(expected, green[pixel]);
        }
    }

    /// <summary>A bad extension is an error that names the file: an absent parent, a parent that extends another, an unknown ramp, an unused ramp, and a ramp of another length.</summary>
    [Fact]
    public void ExtensionRejectsABadSwap()
    {
        Palette palette = RepositoryPalette();
        (string, byte[]) skin = ("textures/recipes/skin.json", Bytes("{\"layers\": [{\"kind\": \"fill\", \"color\": 29, \"shade\": 0, \"noise\": 0.3, \"seed\": 8}]}"));

        ContextException absent = Assert.Throws<ContextException>(() => RecipeFile.ReadAll([skin, ("textures/recipes/a.json", Bytes("{\"extends\": \"hide\", \"swap\": {\"bone\": \"rust\"}}"))], palette));
        Assert.Contains("no recipe file", absent.Message, StringComparison.Ordinal);

        ContextException chain = Assert.Throws<ContextException>(() => RecipeFile.ReadAll(
            [skin, ("textures/recipes/a.json", Bytes("{\"extends\": \"skin\", \"swap\": {\"bone\": \"rust\"}}")), ("textures/recipes/b.json", Bytes("{\"extends\": \"a\", \"swap\": {\"rust\": \"bone\"}}"))],
            palette));
        Assert.Contains("a recipe that extends another", chain.Message, StringComparison.Ordinal);

        ContextException unknown = Assert.Throws<ContextException>(() => RecipeFile.ReadAll([skin, ("textures/recipes/a.json", Bytes("{\"extends\": \"skin\", \"swap\": {\"bone\": \"gold\"}}"))], palette));
        Assert.Contains("no ramp of that name", unknown.Message, StringComparison.Ordinal);

        ContextException unused = Assert.Throws<ContextException>(() => RecipeFile.ReadAll([skin, ("textures/recipes/a.json", Bytes("{\"extends\": \"skin\", \"swap\": {\"rust\": \"bone\"}}"))], palette));
        Assert.Contains("paints no color of it", unused.Message, StringComparison.Ordinal);

        Palette uneven = Palette.Parse(AssetPaths.PaletteFile, Bytes("{\"ramps\": [{\"name\": \"a\", \"colors\": [\"#101010\", \"#202020\"], \"shades\": [\"#141414\", \"#181818\", \"#1c1c1c\"]}, {\"name\": \"b\", \"colors\": [\"#303030\"], \"shades\": []}]}"));
        ContextException length = Assert.Throws<ContextException>(() => RecipeFile.ReadAll(
            [("textures/recipes/base.json", Bytes("{\"layers\": [{\"kind\": \"fill\", \"color\": 0, \"shade\": 0, \"noise\": 0, \"seed\": 8}]}")), ("textures/recipes/a.json", Bytes("{\"extends\": \"base\", \"swap\": {\"a\": \"b\"}}"))],
            uneven));
        Assert.Contains("keeps the step of each color", length.Message, StringComparison.Ordinal);
    }

    /// <summary>The block file binds every block id other than air once, and each error names the file (D-505).</summary>
    [Theory]
    [InlineData("1,2,3,4,5,6", "", "no entry for the block 7")]
    [InlineData("0,1,2,3,4,5,6,7", "", "other than air")]
    [InlineData("1,2,3,4,5,6,7,8", "", "other than air")]
    [InlineData("1,1,2,3,4,5,6,7", "", "an earlier entry names that block")]
    [InlineData("1,2,3,4,5,6,7", "hide", "no recipe file")]
    public void BlockFileBindsEveryBlockOnce(string blocks, string recipe, string reason)
    {
        IReadOnlyDictionary<string, Recipe> recipes = new Dictionary<string, Recipe> { ["stone"] = Layers(new FillLayer(1, 0, 0.0, 1)) };
        string name = recipe.Length > 0 ? recipe : "stone";
        IEnumerable<string> entries = blocks.Split(',').Select(block => "{\"block\": " + block + ", \"recipe\": \"" + name + "\"}");
        byte[] bytes = Bytes("{\"blocks\": [" + string.Join(", ", entries) + "]}");

        ContextException error = Assert.Throws<ContextException>(() => PaintFile.ReadBlocks(AssetPaths.BlockPaintFile, bytes, recipes));

        Assert.Contains(AssetPaths.BlockPaintFile, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A paint file gives each box its recipe, and a face override replaces the recipe of that face alone (D-508).</summary>
    [Fact]
    public void PaintFileBindsEachBoxAndFace()
    {
        ModelPaint paint = PaintFile.ReadModel(PaintPath, Bytes(RigPaint("\"arm\": {\"recipe\": \"stone\", \"faces\": {\"up\": \"moss\"}}")), RigModel(), TwoRecipes());

        Assert.Equal(new[] { "stone", "stone", "stone", "stone", "stone", "stone" }, paint.Recipes[0]);
        Assert.Equal(new[] { "stone", "stone", "stone", "stone", "moss", "stone" }, paint.Recipes[1]);
    }

    /// <summary>A bad paint file is an error that names the file: another model, a missing box, an unknown box, a bad face, and an unknown recipe (D-508).</summary>
    [Theory]
    [InlineData("{\"model\": \"models/other.bbmodel\", \"boxes\": {}}", "model", "names the model models/rig.bbmodel")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"stone\", \"faces\": {}}}}", "boxes", "no entry for the box 'arm'")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"stone\", \"faces\": {}}, \"arm\": {\"recipe\": \"stone\", \"faces\": {}}, \"tail\": {\"recipe\": \"stone\", \"faces\": {}}}}", "boxes", "no box of that name")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"stone\", \"faces\": {\"top\": \"moss\"}}, \"arm\": {\"recipe\": \"stone\", \"faces\": {}}}}", "faces", "each override names one of")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"hide\", \"faces\": {}}, \"arm\": {\"recipe\": \"stone\", \"faces\": {}}}}", "recipe", "no recipe file")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"stone\"}, \"arm\": {\"recipe\": \"stone\", \"faces\": {}}}}", "faces", "is absent")]
    public void PaintFileRejectsABadBinding(string json, string field, string reason)
    {
        ContextException error = Assert.Throws<ContextException>(() => PaintFile.ReadModel(PaintPath, Bytes(json), RigModel(), TwoRecipes()));

        Assert.Contains(PaintPath, error.Message, StringComparison.Ordinal);
        Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>The packer places every canvas inside the atlas with its gutter, no two cells overlap, and the same input gives the same places.</summary>
    [Fact]
    public void PackerPlacesEveryCanvasApart()
    {
        List<CanvasSize> canvases = [];
        for (int index = 0; index < 120; index++)
        {
            canvases.Add(new CanvasSize($"canvas {index}", 1 + (index * 7 % 31), 1 + (index * 11 % 29)));
        }

        IReadOnlyList<AtlasRect> places = AtlasPacker.Pack(canvases);
        IReadOnlyList<AtlasRect> again = AtlasPacker.Pack(canvases);

        Assert.Equal(places, again);
        for (int index = 0; index < places.Count; index++)
        {
            AtlasRect place = places[index];
            Assert.Equal((canvases[index].Width, canvases[index].Height), (place.Width, place.Height));
            Assert.True(place.X >= 1 && place.Y >= 1 && place.X + place.Width + 1 <= AtlasLayout.AtlasPixels && place.Y + place.Height + 1 <= AtlasLayout.AtlasPixels, $"The canvas {index} at ({place.X}, {place.Y}) and its gutter leave the atlas.");
            for (int other = index + 1; other < places.Count; other++)
            {
                Assert.False(CellsOverlap(place, places[other]), $"The cells of the canvases {index} and {other} overlap.");
            }
        }
    }

    /// <summary>A canvas wider or higher than the atlas is an error that names it, also at a size where the sum with the gutter wraps (D-604, T-2).</summary>
    [Theory]
    [InlineData(int.MaxValue, 4)]
    [InlineData(4, int.MaxValue)]
    [InlineData(1023, 4)]
    public void PackerRejectsACanvasLargerThanTheAtlas(int width, int height)
    {
        ContextException error = Assert.Throws<ContextException>(() => AtlasPacker.Pack([new CanvasSize("models/rig.bbmodel:arm:north", width, height)]));

        Assert.Contains("models/rig.bbmodel:arm:north", error.Message, StringComparison.Ordinal);
        Assert.Contains("has no room for the canvas", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-62 exit test 3. A full atlas is an error that names the canvas that does not fit (D-604).</summary>
    [Fact]
    public void PackerReportsAFullAtlas()
    {
        List<CanvasSize> canvases = [.. Enumerable.Range(0, 300).Select(index => new CanvasSize($"canvas {index}", AtlasLayout.BlockPixels, AtlasLayout.BlockPixels))];

        ContextException error = Assert.Throws<ContextException>(() => AtlasPacker.Pack(canvases));

        Assert.Contains("has no room for the canvas", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-604", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The layout text parses back to the same places, and a face that the layout does not hold is an error that names it (D-505).</summary>
    [Fact]
    public void LayoutRoundTripsAndNamesAnAbsentFace()
    {
        TextureLayout layout = new(
            [new BlockPlace(1, "raw-stone", new AtlasRect(1, 1, 32, 32))],
            [new FacePlace("models/rig.bbmodel", "arm", BoxSide.Up, "moss", new AtlasRect(35, 1, 4, 3))]);

        TextureLayout parsed = TextureLayout.Parse(AssetPaths.LayoutFile, Bytes(layout.ToJson()));

        Assert.Equal(layout.ToJson(), parsed.ToJson());
        Assert.Equal(new AtlasRect(35, 1, 4, 3), parsed.Face("models/rig.bbmodel", "arm", BoxSide.Up));
        ContextException absent = Assert.Throws<ContextException>(() => parsed.Face("models/rig.bbmodel", "arm", BoxSide.Down));
        Assert.Contains("models/rig.bbmodel:arm:down", absent.Message, StringComparison.Ordinal);
        ContextException noBlock = Assert.Throws<ContextException>(() => parsed.Block(2));
        Assert.Contains("no canvas for the block 2", noBlock.Message, StringComparison.Ordinal);
    }

    /// <summary>A bad layout file is an error that names the file and the field.</summary>
    [Theory]
    [InlineData("{\"atlas\": 512, \"blocks\": [], \"faces\": []}", "atlas", "1024 pixels on a side")]
    [InlineData("{\"atlas\": 1024, \"blocks\": [{\"block\": 1, \"recipe\": \"a\", \"at\": [1000, 0, 32, 32]}], \"faces\": []}", "at", "inside the atlas")]
    [InlineData("{\"atlas\": 1024, \"blocks\": [{\"block\": 1, \"recipe\": \"a\", \"at\": [2147483647, 1, 1, 32]}], \"faces\": []}", "at", "inside the atlas")]
    [InlineData("{\"atlas\": 1024, \"blocks\": [], \"faces\": [{\"model\": \"m\", \"box\": \"b\", \"face\": \"up\", \"recipe\": \"a\", \"at\": [1, 2147483647, 4, 1]}]}", "at", "inside the atlas")]
    [InlineData("{\"atlas\": 1024, \"blocks\": [], \"faces\": [{\"model\": \"m\", \"box\": \"b\", \"face\": \"top\", \"recipe\": \"a\", \"at\": [0, 0, 1, 1]}]}", "face", "a face is one of")]
    [InlineData("{\"atlas\": 1024, \"blocks\": [], \"faces\": [], \"size\": 1}", "size", "not a field of the format")]
    public void LayoutRejectsABadFile(string json, string field, string reason)
    {
        ContextException error = Assert.Throws<ContextException>(() => TextureLayout.Parse(AssetPaths.LayoutFile, Bytes(json)));

        Assert.Contains(AssetPaths.LayoutFile, error.Message, StringComparison.Ordinal);
        Assert.Contains($"The field '{field}'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A layout that places one block twice is an error that names the file and the block.</summary>
    [Fact]
    public void LayoutRejectsABlockTwice()
    {
        string json = "{\"atlas\": 1024, \"blocks\": [{\"block\": 1, \"recipe\": \"a\", \"at\": [1, 1, 32, 32]}, {\"block\": 1, \"recipe\": \"a\", \"at\": [35, 1, 32, 32]}], \"faces\": []}";

        ContextException error = Assert.Throws<ContextException>(() => TextureLayout.Parse(AssetPaths.LayoutFile, Bytes(json)));

        Assert.Contains(AssetPaths.LayoutFile, error.Message, StringComparison.Ordinal);
        Assert.Contains("the block 1 twice", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The texel size of a face follows its box at 64 texels per meter, and the canvas rounds a fraction up and a float hair down (D-308, D-603).</summary>
    [Fact]
    public void FaceTexelsFollowTheBox()
    {
        BlockbenchModel player = BlockbenchLoader.Parse(AssetPaths.BodyModel, File.ReadAllBytes(Path.Combine(RepositoryRoot.Find(), "content", AssetPaths.BodyModel)));
        ModelBox head = player.Box("head_box") ?? throw new InvalidOperationException("The player has no head box.");
        ModelBox torso = player.Box("torso_box") ?? throw new InvalidOperationException("The player has no torso box.");

        Assert.Equal((32, 32), BoxFaces.CanvasTexels(head, BoxSide.North));
        Assert.Equal((40, 44), BoxFaces.CanvasTexels(torso, BoxSide.North));
        Assert.Equal((20, 44), BoxFaces.CanvasTexels(torso, BoxSide.East));
        Assert.Equal((40, 20), BoxFaces.CanvasTexels(torso, BoxSide.Up));
        Assert.Equal(43.2f, BoxFaces.Texels(torso, BoxSide.West).Height, 0.001f);
        Assert.True(BoxFaces.TryParse("down", out BoxSide down) && down == BoxSide.Down);
        Assert.False(BoxFaces.TryParse("top", out _));
    }

    /// <summary>The atlas index of each pixel of one painted canvas: each color turned back into its place in <see cref="Palette.AtlasColors"/>.</summary>
    private static int[] PaintIndices(Palette palette, Recipe recipe, int width, int height, uint salt, string canvasName)
    {
        PaletteShades shades = new(palette);
        return CanvasPainter.Paint(palette, recipe, width, height, salt, canvasName).Select(shades.Index).ToArray();
    }

    /// <summary>The correlation of the position of each pixel with the pixel on its right.</summary>
    private static double NeighborCorrelation(int[] positions, int width)
    {
        double[] values = positions.Select(value => (double)value).ToArray();
        double mean = values.Average();
        double variance = values.Select(value => (value - mean) * (value - mean)).Average();
        List<double> products = [];
        for (int pixel = 0; pixel < values.Length; pixel++)
        {
            if ((pixel % width) < width - 1)
            {
                products.Add((values[pixel] - mean) * (values[pixel + 1] - mean));
            }
        }

        return products.Average() / variance;
    }

    private static bool CellsOverlap(AtlasRect a, AtlasRect b)
    {
        return a.X - 1 < b.X + b.Width + 1 && b.X - 1 < a.X + a.Width + 1 && a.Y - 1 < b.Y + b.Height + 1 && b.Y - 1 < a.Y + a.Height + 1;
    }

    private static Recipe Layers(params RecipeLayer[] layers)
    {
        return new Recipe(RecipePath, "test", layers);
    }

    private static Recipe ReadOne(string json)
    {
        return RecipeFile.ReadAll([(RecipePath, Bytes(json))], RepositoryPalette())["test"];
    }

    private static IReadOnlyDictionary<string, Recipe> TwoRecipes()
    {
        return new Dictionary<string, Recipe>
        {
            ["stone"] = Layers(new FillLayer(1, 0, 0.0, 1)),
            ["moss"] = Layers(new FillLayer(25, 0, 0.0, 1)),
        };
    }

    private static BlockbenchModel RigModel()
    {
        return BlockbenchLoader.Parse("models/rig.bbmodel", Bytes(ModelJson.SiblingRig()));
    }

    private static string RigPaint(string armEntry)
    {
        return "{\"model\": \"models/rig.bbmodel\", \"boxes\": {\"torso\": {\"recipe\": \"stone\", \"faces\": {}}, " + armEntry + "}}";
    }

    private static void AssertRingAndInside(int[] pixels, int width, int height, int ring, int inside)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool onRing = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                Assert.True(pixels[(y * width) + x] == (onRing ? ring : inside), $"The pixel ({x}, {y}) holds {pixels[(y * width) + x]}.");
            }
        }
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
