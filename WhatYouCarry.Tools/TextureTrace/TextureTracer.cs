using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>
/// Traces one box face from an area of a screenshot into a texel map (D-612). Each texel of the canvas reads the
/// middle half of its cell of the area, so an edge between two texels of the reference does not blend into it. The
/// trace takes the mean of those pixels in linear light, and then the palette shade of the named ramps at the least
/// distance in OKLab, as D-529 measured the base colors.
/// </summary>
public static class TextureTracer
{
    /// <summary>The characters of a map legend, in the order that the trace gives them out.</summary>
    public const string LegendCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>The share of a texel cell, on each side, that the sample leaves out.</summary>
    private const double Inset = 0.25;

    /// <summary>The map of one face.</summary>
    /// <param name="image">The screenshot.</param>
    /// <param name="imagePath">The path of the screenshot, for an error.</param>
    /// <param name="face">The face, its area of the screenshot, its turn, and its ramps.</param>
    /// <param name="width">The width of the face canvas, in texels.</param>
    /// <param name="height">The height of the face canvas, in texels.</param>
    /// <param name="palette">The palette.</param>
    /// <exception cref="ContextException">The area passes the edge of the screenshot, a sample reads a pixel that is not fully opaque, or the face needs more shades than the legend has characters.</exception>
    public static MapLayer Trace(ScreenshotImage image, string imagePath, TraceFace face, int width, int height, Palette palette)
    {
        string faceName = TextureLayout.FaceName(face.Model, face.Box, face.Side);
        ImageArea at = face.At;
        if (at.X + at.Width > image.Width || at.Y + at.Height > image.Height)
        {
            throw new ContextException($"The area [{Text(at.X)}, {Text(at.Y)}, {Text(at.Width)}, {Text(at.Height)}] of the face {faceName} passes the edge of the screenshot '{imagePath}' of {Text(image.Width)} by {Text(image.Height)} pixels.");
        }

        List<(int Ramp, int Fine, OkLab Color)> shades = Shades(palette, face.Ramps);
        Dictionary<(int Ramp, int Fine), char> keys = [];
        Dictionary<char, MapShade> legend = [];
        List<string> rows = [];
        for (int y = 0; y < height; y++)
        {
            StringBuilder row = new(width);
            for (int x = 0; x < width; x++)
            {
                OkLab color = Sample(image, imagePath, face, faceName, x, y, width, height);
                (int ramp, int fine) = Nearest(shades, color);
                if (!keys.TryGetValue((ramp, fine), out char key))
                {
                    if (keys.Count == LegendCharacters.Length)
                    {
                        throw new ContextException($"The face {faceName} needs more than {Text(LegendCharacters.Length)} shades, the characters of a legend. Name fewer ramps for it in the trace spec.");
                    }

                    key = LegendCharacters[keys.Count];
                    keys.Add((ramp, fine), key);
                    int step = fine / Palette.ShadesPerStep;
                    legend.Add(key, new MapShade(palette.Ramps[ramp].First + step, fine - (step * Palette.ShadesPerStep)));
                }

                row.Append(key);
            }

            rows.Add(row.ToString());
        }

        return new MapLayer(rows, legend);
    }

    /// <summary>The text of a recipe file that holds one map, with its legend in key order and one row on each line.</summary>
    public static string RecipeText(MapLayer map)
    {
        StringBuilder text = new();
        text.Append("{\n  \"layers\": [\n    {\n      \"kind\": \"map\",\n      \"legend\": {\n");
        int entry = 0;
        foreach (char key in LegendCharacters)
        {
            if (!map.Legend.TryGetValue(key, out MapShade shade))
            {
                continue;
            }

            entry++;
            string comma = entry < map.Legend.Count ? "," : string.Empty;
            text.Append($"        \"{key}\": {{\"color\": {Text(shade.Color)}, \"shade\": {Text(shade.Shade)}}}{comma}\n");
        }

        text.Append("      },\n      \"rows\": [\n");
        for (int row = 0; row < map.Rows.Count; row++)
        {
            string comma = row < map.Rows.Count - 1 ? "," : string.Empty;
            text.Append($"        \"{map.Rows[row]}\"{comma}\n");
        }

        text.Append("      ]\n    }\n  ]\n}\n");
        return text.ToString();
    }

    /// <summary>Every fine shade of the named ramps, in the order of the ramps and then from dark to light.</summary>
    private static List<(int Ramp, int Fine, OkLab Color)> Shades(Palette palette, IReadOnlyList<int> ramps)
    {
        List<(int Ramp, int Fine, OkLab Color)> shades = [];
        foreach (int ramp in ramps)
        {
            for (int fine = 0; fine <= palette.FineTop(ramp); fine++)
            {
                AtlasColor color = palette.AtlasColors[palette.AtlasIndex(ramp, fine)];
                shades.Add((ramp, fine, OkLab.FromLinear(Linear(color.Red), Linear(color.Green), Linear(color.Blue))));
            }
        }

        return shades;
    }

    /// <summary>The shade at the least distance. On a tie, the first in the order of <see cref="Shades"/> wins, so the trace gives one answer.</summary>
    private static (int Ramp, int Fine) Nearest(List<(int Ramp, int Fine, OkLab Color)> shades, OkLab color)
    {
        (int Ramp, int Fine) best = (shades[0].Ramp, shades[0].Fine);
        double least = double.MaxValue;
        foreach ((int ramp, int fine, OkLab shade) in shades)
        {
            double distance = shade.DistanceSquared(color);
            if (distance < least)
            {
                least = distance;
                best = (ramp, fine);
            }
        }

        return best;
    }

    /// <summary>
    /// The mean color of the middle half of the cell of one texel, in linear light. A cell smaller than two pixels
    /// holds no pixel center in its middle half, so the sample then reads the pixel under the middle of the cell.
    /// </summary>
    private static OkLab Sample(ScreenshotImage image, string imagePath, TraceFace face, string faceName, int x, int y, int width, int height)
    {
        (double left, double top) = AreaPoint(face.Turn, (x + Inset) / width, (y + Inset) / height);
        (double right, double bottom) = AreaPoint(face.Turn, (x + 1 - Inset) / width, (y + 1 - Inset) / height);
        double lowX = face.At.X + (Math.Min(left, right) * face.At.Width);
        double highX = face.At.X + (Math.Max(left, right) * face.At.Width);
        double lowY = face.At.Y + (Math.Min(top, bottom) * face.At.Height);
        double highY = face.At.Y + (Math.Max(top, bottom) * face.At.Height);
        int firstColumn = (int)Math.Ceiling(lowX - 0.5);
        int lastColumn = (int)Math.Ceiling(highX - 0.5) - 1;
        int firstRow = (int)Math.Ceiling(lowY - 0.5);
        int lastRow = (int)Math.Ceiling(highY - 0.5) - 1;
        if (lastColumn < firstColumn)
        {
            firstColumn = (int)Math.Floor((lowX + highX) / 2.0);
            lastColumn = firstColumn;
        }

        if (lastRow < firstRow)
        {
            firstRow = (int)Math.Floor((lowY + highY) / 2.0);
            lastRow = firstRow;
        }

        double red = 0.0;
        double green = 0.0;
        double blue = 0.0;
        for (int row = firstRow; row <= lastRow; row++)
        {
            for (int column = firstColumn; column <= lastColumn; column++)
            {
                ScreenshotPixel pixel = image.Pixels[(row * image.Width) + column];
                if (pixel.Alpha != byte.MaxValue)
                {
                    throw new ContextException($"The texel ({Text(x)}, {Text(y)}) of the face {faceName} reads the pixel ({Text(column)}, {Text(row)}) of the screenshot '{imagePath}', and that pixel is not fully opaque. Move the area of the face onto the model.");
                }

                red += Linear(pixel.Red);
                green += Linear(pixel.Green);
                blue += Linear(pixel.Blue);
            }
        }

        int count = (lastRow - firstRow + 1) * (lastColumn - firstColumn + 1);
        return OkLab.FromLinear(red / count, green / count, blue / count);
    }

    /// <summary>
    /// The point of the area, in shares of its width and height, under one point of the canvas. The canvas is the area
    /// turned clockwise by the quarter turns of the face, so the inverse turn gives the point of the area.
    /// </summary>
    private static (double X, double Y) AreaPoint(int turn, double across, double down)
    {
        return turn switch
        {
            0 => (across, down),
            1 => (down, 1.0 - across),
            2 => (1.0 - across, 1.0 - down),
            _ => (1.0 - down, across),
        };
    }

    private static double Linear(byte value)
    {
        return (double)LinearLight.Of(value) / LinearLight.Full;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
