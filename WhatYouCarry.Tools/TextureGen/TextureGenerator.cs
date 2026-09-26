using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>The atlas pixels and the texture layout of one generator run.</summary>
/// <param name="Pixels">The color of every pixel of the atlas, row by row from the top left corner.</param>
/// <param name="Layout">The place of each block canvas and each face canvas.</param>
public sealed record AtlasResult(AtlasColor[] Pixels, TextureLayout Layout);

/// <summary>
/// Paints the atlas from the palette, the recipes, and the bindings (D-305, D-505). Each block id has a canvas of
/// 64 by 64 pixels. Each face of each box of each model has a canvas of its size at 64 texels per meter (D-603). The
/// packer places every canvas, and a gutter around each canvas repeats its edge pixels. A pixel that no canvas and no
/// gutter covers holds the first color of the palette.
/// </summary>
public static class TextureGenerator
{
    /// <summary>The atlas and the layout of the given recipes and bindings.</summary>
    /// <param name="palette">The palette of the atlas.</param>
    /// <param name="recipes">Every recipe, by name.</param>
    /// <param name="blocks">The recipe of each block id, in order of the id.</param>
    /// <param name="models">The paint of each model, in order of the model path.</param>
    /// <exception cref="ContextException">A face has no area, a rectangle paints nothing, a seed gives a zero state, or the atlas has no room.</exception>
    public static AtlasResult Generate(Palette palette, IReadOnlyDictionary<string, Recipe> recipes, IReadOnlyList<BlockPaint> blocks, IReadOnlyList<ModelPaint> models)
    {
        List<CanvasSize> sizes = [];
        List<AtlasColor[]> canvases = [];
        foreach (BlockPaint block in blocks)
        {
            string name = "block " + Text(block.Block);
            sizes.Add(new CanvasSize(name, AtlasLayout.BlockPixels, AtlasLayout.BlockPixels));
            canvases.Add(CanvasPainter.Paint(palette, recipes[block.Recipe], AtlasLayout.BlockPixels, AtlasLayout.BlockPixels, CanvasPainter.BlockSalt, name));
        }

        foreach (ModelPaint paint in models)
        {
            for (int boxIndex = 0; boxIndex < paint.Model.Boxes.Count; boxIndex++)
            {
                ModelBox box = paint.Model.Boxes[boxIndex];
                for (int side = 0; side < BoxFaces.Names.Count; side++)
                {
                    string name = TextureLayout.FaceName(paint.Model.Path, box.Name, (BoxSide)side);
                    (int width, int height) = BoxFaces.CanvasTexels(box, (BoxSide)side);
                    if (width <= 0 || height <= 0)
                    {
                        throw new ContextException($"The face {name} has no area, so no canvas can paint it. Give the box a size on each axis.");
                    }

                    Recipe recipe = recipes[paint.Recipes[boxIndex][side]];
                    sizes.Add(new CanvasSize(name, width, height));
                    canvases.Add(CanvasPainter.Paint(palette, recipe, width, height, CanvasPainter.SaltOf(name), name));
                }
            }
        }

        IReadOnlyList<AtlasRect> places = AtlasPacker.Pack(sizes);
        AtlasColor[] pixels = new AtlasColor[AtlasLayout.AtlasPixels * AtlasLayout.AtlasPixels];
        Array.Fill(pixels, palette.AtlasColors[0]);
        for (int index = 0; index < canvases.Count; index++)
        {
            Place(pixels, canvases[index], places[index]);
        }

        return new AtlasResult(pixels, Layout(blocks, models, places));
    }

    /// <summary>
    /// Copies one canvas into the atlas at its place, and fills its gutter: each gutter pixel takes the nearest pixel
    /// of the canvas, the corners included.
    /// </summary>
    private static void Place(AtlasColor[] atlas, AtlasColor[] canvas, AtlasRect place)
    {
        int gutter = AtlasLayout.Gutter;
        for (int y = -gutter; y < place.Height + gutter; y++)
        {
            int sourceY = System.Math.Clamp(y, 0, place.Height - 1);
            for (int x = -gutter; x < place.Width + gutter; x++)
            {
                int sourceX = System.Math.Clamp(x, 0, place.Width - 1);
                atlas[((place.Y + y) * AtlasLayout.AtlasPixels) + place.X + x] = canvas[(sourceY * place.Width) + sourceX];
            }
        }
    }

    /// <summary>The layout of the places, in the order that the canvases took: the blocks, then the faces by model, box, and side.</summary>
    private static TextureLayout Layout(IReadOnlyList<BlockPaint> blocks, IReadOnlyList<ModelPaint> models, IReadOnlyList<AtlasRect> places)
    {
        List<BlockPlace> blockPlaces = [];
        int index = 0;
        foreach (BlockPaint block in blocks)
        {
            blockPlaces.Add(new BlockPlace(block.Block, block.Recipe, places[index]));
            index++;
        }

        List<FacePlace> facePlaces = [];
        foreach (ModelPaint paint in models)
        {
            for (int boxIndex = 0; boxIndex < paint.Model.Boxes.Count; boxIndex++)
            {
                for (int side = 0; side < BoxFaces.Names.Count; side++)
                {
                    string recipe = paint.Recipes[boxIndex][side];
                    facePlaces.Add(new FacePlace(paint.Model.Path, paint.Model.Boxes[boxIndex].Name, (BoxSide)side, recipe, places[index]));
                    index++;
                }
            }
        }

        return new TextureLayout(blockPlaces, facePlaces);
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
