using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>
/// Paints the atlas from the palette and the rules (D-85, D-304, D-307). The result is one palette index per pixel,
/// row by row from the top left corner, so the atlas holds palette colors alone. A tile that no rule paints holds
/// index 0.
/// </summary>
/// <remarks>
/// The noise of a tile comes from a xorshift sequence of 32 bits, from the seed of its rule. The sequence and the
/// comparisons are the ones that the palette preview of OQ-1 ran, so the atlas shows the tiles that the owner chose
/// from (D-304). The order is fixed: one step of the sequence per pixel, rows from the top, pixels from the left.
/// </remarks>
public static class TextureGenerator
{
    /// <summary>The count of values of 32 bits: the divisor that turns a state into a roll from 0 to 1.</summary>
    public const double StateRange = 4294967296.0;

    /// <summary>The palette index of every pixel of the atlas.</summary>
    /// <exception cref="ContextException">Two rules paint one tile.</exception>
    public static byte[] Paint(Palette palette, IReadOnlyList<TextureRule> rules)
    {
        CheckOneRulePerTile(rules);
        byte[] pixels = new byte[AtlasLayout.AtlasPixels * AtlasLayout.AtlasPixels];
        foreach (TextureRule rule in rules)
        {
            PaintTile(pixels, palette, rule);
        }

        return pixels;
    }

    /// <summary>
    /// Paints the tile of one rule. Each pixel starts at the step of the base color on its ramp. A roll below half the
    /// noise amount moves the pixel one step down, and a roll above one minus half the amount moves it one step up. A
    /// pixel on the outer ring then moves down by the edge darkness, and the step stays inside the ramp.
    /// </summary>
    /// <exception cref="ArgumentException">The pixel buffer is not the size of the atlas.</exception>
    public static void PaintTile(byte[] pixels, Palette palette, TextureRule rule)
    {
        int atlasArea = AtlasLayout.AtlasPixels * AtlasLayout.AtlasPixels;
        if (pixels.Length != atlasArea)
        {
            throw new ArgumentException($"The pixel buffer holds {Text(pixels.Length)} pixels, and the atlas needs {Text(atlasArea)}.", nameof(pixels));
        }

        PaletteColor baseColor = palette.Colors[rule.Base];
        PaletteRamp ramp = palette.Ramps[baseColor.Ramp];
        int left = AtlasLayout.Column(rule.Tile) * AtlasLayout.TilePixels;
        int top = AtlasLayout.Row(rule.Tile) * AtlasLayout.TilePixels;
        int last = AtlasLayout.TilePixels - 1;
        double half = rule.Noise / 2.0;
        uint state = rule.Seed;
        for (int y = 0; y < AtlasLayout.TilePixels; y++)
        {
            for (int x = 0; x < AtlasLayout.TilePixels; x++)
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

                if (x == 0 || y == 0 || x == last || y == last)
                {
                    step -= rule.Edge;
                }

                step = Math.Clamp(step, 0, ramp.Count - 1);
                pixels[((top + y) * AtlasLayout.AtlasPixels) + left + x] = (byte)(ramp.First + step);
            }
        }
    }

    /// <summary>One step of the xorshift sequence of 32 bits, with the shifts 13, 17, and 5.</summary>
    public static uint NextState(uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return state;
    }

    /// <summary>No two rules paint one tile. The error names the tile and both files.</summary>
    private static void CheckOneRulePerTile(IReadOnlyList<TextureRule> rules)
    {
        TextureRule?[] owners = new TextureRule?[AtlasLayout.TileCount];
        foreach (TextureRule rule in rules)
        {
            TextureRule? earlier = owners[rule.Tile];
            if (earlier is not null)
            {
                throw new ContextException($"The rules '{earlier.ContentPath}' and '{rule.ContentPath}' both paint the tile {Text(rule.Tile)}, and each tile has one rule.");
            }

            owners[rule.Tile] = rule;
        }
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
