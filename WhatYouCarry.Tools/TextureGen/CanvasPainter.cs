using System;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>
/// Paints one canvas from one recipe (D-505, D-507). The result is one palette index per pixel, row by row from the
/// top left corner.
/// </summary>
/// <remarks>
/// <para>
/// Each pixel holds a ramp and a step on it while the layers run. A step can leave the ramp during the layers, and
/// the painter clamps it to the ramp once, after the last layer. A fill with noise and then an edge therefore give
/// the pixels of a rule of PR-14 with the same base, noise, edge, and seed (D-309).
/// </para>
/// <para>
/// The noise of a fill or a rectangle comes from a xorshift sequence of 32 bits, one step per pixel, rows from the
/// top and pixels from the left. The sequence starts at the seed of the layer, combined with the salt of the canvas.
/// A block canvas has the salt zero, so its pixels are the pixels of its tile in the atlas of PR-14. A face canvas
/// has a salt from its name, so two faces of one recipe show two draws of the noise.
/// </para>
/// </remarks>
public static class CanvasPainter
{
    /// <summary>The count of values of 32 bits: the divisor that turns a state into a roll from 0 to 1.</summary>
    public const double StateRange = 4294967296.0;

    /// <summary>The salt of a block canvas.</summary>
    public const uint BlockSalt = 0;

    private const uint FnvOffset = 2166136261;
    private const uint FnvPrime = 16777619;

    /// <summary>The palette index of every pixel of one canvas.</summary>
    /// <param name="palette">The palette of the atlas.</param>
    /// <param name="recipe">The recipe that paints the canvas.</param>
    /// <param name="width">The width of the canvas, in pixels, above zero.</param>
    /// <param name="height">The height of the canvas, in pixels, above zero.</param>
    /// <param name="salt">The salt of the canvas: <see cref="BlockSalt"/> for a block, or <see cref="SaltOf"/> of the face name.</param>
    /// <param name="canvasName">The name of the canvas in an error.</param>
    /// <exception cref="ContextException">A rectangle lies wholly outside the canvas, or a seed and the salt give the state zero.</exception>
    public static byte[] Paint(Palette palette, Recipe recipe, int width, int height, uint salt, string canvasName)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException($"The canvas {canvasName} is {Text(width)} by {Text(height)} pixels, and a canvas has a positive size.", nameof(width));
        }

        int[] ramps = new int[width * height];
        int[] steps = new int[width * height];
        foreach (RecipeLayer layer in recipe.Layers)
        {
            switch (layer)
            {
                case FillLayer fill:
                    PaintNoise(palette, fill.Color, fill.Noise, StartState(fill.Seed, salt, recipe, canvasName), ramps, steps, width, 0, 0, width, height);
                    break;
                case EdgeLayer edge:
                    ShiftRing(steps, width, height, -edge.Steps);
                    break;
                case RectLayer rect:
                    PaintRect(palette, rect, StartState(rect.Seed, salt, recipe, canvasName), ramps, steps, width, height, recipe, canvasName);
                    break;
                case BandLayer band:
                    ShiftBand(steps, width, height, band);
                    break;
                default:
                    throw new InvalidOperationException($"The recipe '{recipe.Name}' holds a layer of the type {layer.GetType().Name}, and the painter has no case for it.");
            }
        }

        byte[] pixels = new byte[width * height];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            PaletteRamp ramp = palette.Ramps[ramps[pixel]];
            int step = Math.Clamp(steps[pixel], 0, ramp.Count - 1);
            pixels[pixel] = (byte)(ramp.First + step);
        }

        return pixels;
    }

    /// <summary>The salt of a face canvas: the FNV-1a hash of 32 bits of the UTF-8 bytes of its name.</summary>
    public static uint SaltOf(string canvasName)
    {
        uint hash = FnvOffset;
        foreach (byte value in Encoding.UTF8.GetBytes(canvasName))
        {
            hash ^= value;
            hash *= FnvPrime;
        }

        return hash;
    }

    /// <summary>One step of the xorshift sequence of 32 bits, with the shifts 13, 17, and 5.</summary>
    public static uint NextState(uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return state;
    }

    /// <summary>The first state of the noise of one layer. A zero state never leaves zero, so it is an error that names the recipe and the canvas.</summary>
    private static uint StartState(uint seed, uint salt, Recipe recipe, string canvasName)
    {
        uint state = seed ^ salt;
        if (state == 0)
        {
            throw new ContextException($"The seed {seed.ToString(CultureInfo.InvariantCulture)} of the recipe '{recipe.ContentPath}' equals the salt of the canvas {canvasName}, and the noise then never leaves zero. Change the seed.");
        }

        return state;
    }

    /// <summary>
    /// Sets the pixels of a rectangle of the canvas to a color with noise. A roll below half the noise amount moves the
    /// pixel one step down, and a roll above one minus half the amount moves it one step up.
    /// </summary>
    private static void PaintNoise(Palette palette, int color, double noise, uint state, int[] ramps, int[] steps, int width, int left, int top, int right, int bottom)
    {
        PaletteColor baseColor = palette.Colors[color];
        double half = noise / 2.0;
        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                state = NextState(state);
                double roll = state / StateRange;
                int step = baseColor.Step;
                if (roll < half)
                {
                    step -= 1;
                }
                else if (roll > 1.0 - half)
                {
                    step += 1;
                }

                ramps[(y * width) + x] = baseColor.Ramp;
                steps[(y * width) + x] = step;
            }
        }
    }

    /// <summary>The rectangle layer, clipped to the canvas. A rectangle with no pixel inside the canvas is an error, because it paints nothing (T-2).</summary>
    private static void PaintRect(Palette palette, RectLayer rect, uint state, int[] ramps, int[] steps, int width, int height, Recipe recipe, string canvasName)
    {
        int right = Math.Min(rect.X + rect.Width, width);
        int bottom = Math.Min(rect.Y + rect.Height, height);
        if (rect.X >= width || rect.Y >= height)
        {
            throw new ContextException($"The recipe '{recipe.ContentPath}' paints a rectangle at ({Text(rect.X)}, {Text(rect.Y)}), outside the canvas {canvasName} of {Text(width)} by {Text(height)} pixels, so it paints nothing. Bind the face to another recipe.");
        }

        PaintNoise(palette, rect.Color, rect.Noise, state, ramps, steps, width, rect.X, rect.Y, right, bottom);
    }

    /// <summary>Moves each pixel of the outer ring by a count of steps.</summary>
    private static void ShiftRing(int[] steps, int width, int height, int shift)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    steps[(y * width) + x] += shift;
                }
            }
        }
    }

    /// <summary>Moves each pixel within the depth of one side by the shift of the band. A band deeper than the canvas covers all of it.</summary>
    private static void ShiftBand(int[] steps, int width, int height, BandLayer band)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = band.Side switch
                {
                    CanvasSide.Top => y < band.Depth,
                    CanvasSide.Bottom => y >= height - band.Depth,
                    CanvasSide.Left => x < band.Depth,
                    _ => x >= width - band.Depth,
                };
                if (inside)
                {
                    steps[(y * width) + x] += band.Shift;
                }
            }
        }
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
