using System;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>The ramp of each pixel of one canvas, and its position on that ramp in parts of a fine step, row by row from the top left corner.</summary>
/// <param name="Ramps">The ramp of each pixel.</param>
/// <param name="Positions">The position of each pixel on its ramp, from 0 to the top of the ramp, in <see cref="Palette.PartsPerFineStep"/> parts of a fine step.</param>
public sealed record CanvasPositions(int[] Ramps, int[] Positions);

/// <summary>
/// Paints one canvas from one recipe (D-505, D-507). The result is one color per pixel, row by row from the top left
/// corner (D-598).
/// </summary>
/// <remarks>
/// <para>
/// Each pixel holds a ramp and a position on it while the layers run, in parts of a fine step (D-528, D-599). The
/// noise of a fill or a rectangle, an edge, and a band move a pixel by whole steps of a color, four fine steps each. A
/// gradient moves it by whole fine steps. A grain moves it by parts, so a pixel can lie between two fine shades. A
/// position can leave the ramp during the layers, and the painter clamps it to the ramp once, after the last layer. A
/// fill with noise and then an edge therefore give the pixels of a rule of PR-14 with the same base, noise, edge, and
/// seed (D-309). The color of a pixel is the color of its position (<see cref="Palette.ColorAt"/>).
/// </para>
/// <para>
/// A grain uses whole numbers alone, so each platform paints the same bytes (D-527). Its lattice holds values from
/// -256 to 256, and a smoothstep in 256ths joins them. Each pixel adds a dither of up to one and a half fine steps,
/// which gives the texel mottle of the 3D reference. The grain moves each pixel by the sum in parts, with no rounding
/// to a fine step (D-599).
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

    /// <summary>The fixed point of a grain: a value of 256 is one whole unit.</summary>
    private const int GrainUnit = 256;

    /// <summary>The dither of a grain, in parts of a fine step: each pixel adds a value from minus this to one less than this.</summary>
    private const int GrainDither = 384;

    private const uint FnvOffset = 2166136261;
    private const uint FnvPrime = 16777619;

    /// <summary>The color of every pixel of one canvas: the color of its position (<see cref="PaintPositions"/>).</summary>
    /// <param name="palette">The palette of the atlas.</param>
    /// <param name="recipe">The recipe that paints the canvas.</param>
    /// <param name="width">The width of the canvas, in pixels, above zero.</param>
    /// <param name="height">The height of the canvas, in pixels, above zero.</param>
    /// <param name="salt">The salt of the canvas: <see cref="BlockSalt"/> for a block, or <see cref="SaltOf"/> of the face name.</param>
    /// <param name="canvasName">The name of the canvas in an error.</param>
    /// <exception cref="ContextException">A rectangle lies wholly outside the canvas, or a seed and the salt give the state zero.</exception>
    public static AtlasColor[] Paint(Palette palette, Recipe recipe, int width, int height, uint salt, string canvasName)
    {
        CanvasPositions canvas = PaintPositions(palette, recipe, width, height, salt, canvasName);
        AtlasColor[] pixels = new AtlasColor[width * height];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            pixels[pixel] = palette.ColorAt(canvas.Ramps[pixel], canvas.Positions[pixel]);
        }

        return pixels;
    }

    /// <summary>The ramp and the position of every pixel of one canvas, after the last layer and the clamp to the ramp.</summary>
    /// <param name="palette">The palette of the atlas.</param>
    /// <param name="recipe">The recipe that paints the canvas.</param>
    /// <param name="width">The width of the canvas, in pixels, above zero.</param>
    /// <param name="height">The height of the canvas, in pixels, above zero.</param>
    /// <param name="salt">The salt of the canvas: <see cref="BlockSalt"/> for a block, or <see cref="SaltOf"/> of the face name.</param>
    /// <param name="canvasName">The name of the canvas in an error.</param>
    /// <exception cref="ContextException">A rectangle lies wholly outside the canvas, or a seed and the salt give the state zero.</exception>
    public static CanvasPositions PaintPositions(Palette palette, Recipe recipe, int width, int height, uint salt, string canvasName)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException($"The canvas {canvasName} is {Text(width)} by {Text(height)} pixels, and a canvas has a positive size.", nameof(width));
        }

        const int Parts = Palette.PartsPerFineStep;
        int[] ramps = new int[width * height];
        int[] positions = new int[width * height];
        foreach (RecipeLayer layer in recipe.Layers)
        {
            switch (layer)
            {
                case FillLayer fill:
                    PaintNoise(palette, fill.Color, fill.Shade, fill.Noise, StartState(fill.Seed, salt, recipe, canvasName), ramps, positions, width, 0, 0, width, height);
                    break;
                case EdgeLayer edge:
                    ShiftRing(positions, width, height, -edge.Steps * Palette.ShadesPerStep * Parts);
                    break;
                case RectLayer rect:
                    PaintRect(palette, rect, StartState(rect.Seed, salt, recipe, canvasName), ramps, positions, width, height, recipe, canvasName);
                    break;
                case BandLayer band:
                    ShiftSide(positions, width, height, band.Side, band.Depth, _ => band.Shift * Palette.ShadesPerStep * Parts);
                    break;
                case GrainLayer grain:
                    AddGrain(grain, StartState(grain.Seed, salt, recipe, canvasName), positions, width, height);
                    break;
                case GradientLayer gradient:
                    ShiftSide(positions, width, height, gradient.Side, gradient.Depth, distance => GradientShift(gradient, distance) * Parts);
                    break;
                default:
                    throw new InvalidOperationException($"The recipe '{recipe.Name}' holds a layer of the type {layer.GetType().Name}, and the painter has no case for it.");
            }
        }

        for (int pixel = 0; pixel < positions.Length; pixel++)
        {
            positions[pixel] = Math.Clamp(positions[pixel], 0, palette.FineTop(ramps[pixel]) * Parts);
        }

        return new CanvasPositions(ramps, positions);
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
    /// Sets the pixels of a rectangle of the canvas to a color at a shade, with noise. A roll below half the noise
    /// amount moves the pixel one step of a color down, and a roll above one minus half the amount moves it one up.
    /// </summary>
    private static void PaintNoise(Palette palette, int color, int shade, double noise, uint state, int[] ramps, int[] positions, int width, int left, int top, int right, int bottom)
    {
        PaletteColor baseColor = palette.Colors[color];
        int baseStep = (baseColor.Step * Palette.ShadesPerStep) + shade;
        double half = noise / 2.0;
        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                state = NextState(state);
                double roll = state / StateRange;
                int step = baseStep;
                if (roll < half)
                {
                    step -= Palette.ShadesPerStep;
                }
                else if (roll > 1.0 - half)
                {
                    step += Palette.ShadesPerStep;
                }

                ramps[(y * width) + x] = baseColor.Ramp;
                positions[(y * width) + x] = step * Palette.PartsPerFineStep;
            }
        }
    }

    /// <summary>The rectangle layer, clipped to the canvas. A rectangle with no pixel inside the canvas is an error, because it paints nothing (T-2).</summary>
    private static void PaintRect(Palette palette, RectLayer rect, uint state, int[] ramps, int[] positions, int width, int height, Recipe recipe, string canvasName)
    {
        int right = Math.Min(rect.X + rect.Width, width);
        int bottom = Math.Min(rect.Y + rect.Height, height);
        if (rect.X >= width || rect.Y >= height)
        {
            throw new ContextException($"The recipe '{recipe.ContentPath}' paints a rectangle at ({Text(rect.X)}, {Text(rect.Y)}), outside the canvas {canvasName} of {Text(width)} by {Text(height)} pixels, so it paints nothing. Bind the face to another recipe.");
        }

        PaintNoise(palette, rect.Color, rect.Shade, rect.Noise, state, ramps, positions, width, rect.X, rect.Y, right, bottom);
    }

    /// <summary>Moves each pixel of the outer ring by a count of parts.</summary>
    private static void ShiftRing(int[] positions, int width, int height, int shift)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    positions[(y * width) + x] += shift;
                }
            }
        }
    }

    /// <summary>
    /// Moves each pixel within the depth of one side by the shift in parts for its distance from that side, zero at the
    /// side. A depth past the canvas covers all of it.
    /// </summary>
    private static void ShiftSide(int[] positions, int width, int height, CanvasSide side, int depth, Func<int, int> shiftAt)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int distance = side switch
                {
                    CanvasSide.Top => y,
                    CanvasSide.Bottom => height - 1 - y,
                    CanvasSide.Left => x,
                    _ => width - 1 - x,
                };
                if (distance < depth)
                {
                    positions[(y * width) + x] += shiftAt(distance);
                }
            }
        }
    }

    /// <summary>The fine steps of a gradient at a distance from its side: the full shift at the side, rounded to the nearest whole step toward the depth.</summary>
    private static int GradientShift(GradientLayer gradient, int distance)
    {
        int share = gradient.Depth - distance;
        return FloorDivide((2 * gradient.Shift * share) + gradient.Depth, 2 * gradient.Depth);
    }

    /// <summary>
    /// Adds the grain to each pixel, in parts of a fine step (D-599). The sequence first fills the lattice, row by row,
    /// and then gives one dither to each pixel, rows from the top and pixels from the left.
    /// </summary>
    private static void AddGrain(GrainLayer grain, uint state, int[] positions, int width, int height)
    {
        int columns = (width / grain.Cell) + 2;
        int rows = (height / grain.Cell) + 2;
        int[] lattice = new int[columns * rows];
        for (int point = 0; point < lattice.Length; point++)
        {
            state = NextState(state);
            lattice[point] = (int)(state % ((2 * GrainUnit) + 1)) - GrainUnit;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                state = NextState(state);
                int dither = (int)(state % (2 * GrainDither)) - GrainDither;
                // The lattice value is in 256ths of one unit, and the amount is in fine steps.
                long smooth = LatticeValue(lattice, columns, grain.Cell, x, y);
                long moved = FloorDivide(grain.Amount * smooth * Palette.PartsPerFineStep, GrainUnit) + dither;
                positions[(y * width) + x] += (int)moved;
            }
        }
    }

    /// <summary>The value of the lattice at one pixel, from -256 to 256: the four points around it, joined by a smoothstep in 256ths.</summary>
    private static int LatticeValue(int[] lattice, int columns, int cell, int x, int y)
    {
        int left = x / cell;
        int top = y / cell;
        int across = Smoothstep(x % cell * GrainUnit / cell);
        int down = Smoothstep(y % cell * GrainUnit / cell);
        int upper = (lattice[(top * columns) + left] * (GrainUnit - across)) + (lattice[(top * columns) + left + 1] * across);
        int lower = (lattice[((top + 1) * columns) + left] * (GrainUnit - across)) + (lattice[((top + 1) * columns) + left + 1] * across);
        return FloorDivide((upper * (GrainUnit - down)) + (lower * down), GrainUnit * GrainUnit);
    }

    /// <summary>The smoothstep of a fraction in 256ths: 3t^2 - 2t^3, in 256ths.</summary>
    private static int Smoothstep(int fraction)
    {
        return fraction * fraction * ((3 * GrainUnit) - (2 * fraction)) / (GrainUnit * GrainUnit);
    }

    /// <summary>The quotient rounded toward minus infinity, for a positive divisor.</summary>
    private static int FloorDivide(int value, int divisor)
    {
        int quotient = value / divisor;
        return value % divisor < 0 ? quotient - 1 : quotient;
    }

    /// <summary>The quotient rounded toward minus infinity, for a positive divisor.</summary>
    private static long FloorDivide(long value, long divisor)
    {
        long quotient = value / divisor;
        return value % divisor < 0 ? quotient - 1 : quotient;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
