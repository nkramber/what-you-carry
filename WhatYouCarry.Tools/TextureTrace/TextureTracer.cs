using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>
/// Traces one box face from a screenshot into a texel map (D-612). The four corners of the face on the screenshot
/// frame it, and a point of the canvas maps to the screenshot by bilinear steps between them. Each texel reads a grid
/// of points across the middle half of its cell, so an edge between two texels of the reference does not blend into
/// it. The trace takes the mean of those pixels in linear light, and then the palette shade of the named ramps at the
/// least distance in OKLab, as D-529 measured the base colors.
/// </summary>
public static class TextureTracer
{
    /// <summary>The characters of a map legend, in the order that the trace gives them out.</summary>
    public const string LegendCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>The share of a texel cell, on each side, that the sample leaves out.</summary>
    private const double Inset = 0.25;

    /// <summary>The points of the sample grid along each side of a texel.</summary>
    private const int SamplesAcross = 4;

    /// <summary>The map of one face.</summary>
    /// <param name="image">The screenshot.</param>
    /// <param name="imagePath">The path of the screenshot, for an error.</param>
    /// <param name="face">The face, its corners on the screenshot, and its ramps.</param>
    /// <param name="width">The width of the face canvas, in texels.</param>
    /// <param name="height">The height of the face canvas, in texels.</param>
    /// <param name="palette">The palette.</param>
    /// <exception cref="ContextException">A corner passes the edge of the screenshot, a sample reads a pixel that is not fully opaque, or the face needs more shades than the legend has characters.</exception>
    public static MapLayer Trace(ScreenshotImage image, string imagePath, TraceFace face, int width, int height, Palette palette)
    {
        string faceName = TextureLayout.FaceName(face.Model, face.Box, face.Side);
        foreach (ImagePoint corner in face.Corners)
        {
            if (corner.X > image.Width || corner.Y > image.Height)
            {
                throw new ContextException($"The corner ({Number(corner.X)}, {Number(corner.Y)}) of the face {faceName} passes the edge of the screenshot '{imagePath}' of {Text(image.Width)} by {Text(image.Height)} pixels.");
            }
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
    /// The mean color of a grid of points across the middle half of the cell of one texel, in linear light. Each point
    /// reads the pixel under it.
    /// </summary>
    private static OkLab Sample(ScreenshotImage image, string imagePath, TraceFace face, string faceName, int x, int y, int width, int height)
    {
        double red = 0.0;
        double green = 0.0;
        double blue = 0.0;
        for (int down = 0; down < SamplesAcross; down++)
        {
            for (int across = 0; across < SamplesAcross; across++)
            {
                double share = (1.0 - (2.0 * Inset)) / SamplesAcross;
                ImagePoint point = PointOf(face.Corners, (x + Inset + ((across + 0.5) * share)) / width, (y + Inset + ((down + 0.5) * share)) / height);
                int column = Math.Min((int)Math.Floor(point.X), image.Width - 1);
                int row = Math.Min((int)Math.Floor(point.Y), image.Height - 1);
                ScreenshotPixel pixel = image.Pixels[(row * image.Width) + column];
                if (pixel.Alpha != byte.MaxValue)
                {
                    throw new ContextException($"The texel ({Text(x)}, {Text(y)}) of the face {faceName} reads the pixel ({Text(column)}, {Text(row)}) of the screenshot '{imagePath}', and that pixel is not fully opaque. Move the corners of the face onto the model.");
                }

                red += Linear(pixel.Red);
                green += Linear(pixel.Green);
                blue += Linear(pixel.Blue);
            }
        }

        const int Count = SamplesAcross * SamplesAcross;
        return OkLab.FromLinear(red / Count, green / Count, blue / Count);
    }

    /// <summary>The point of the screenshot under one point of the canvas, in shares of its width and height: the bilinear mix of the four corners.</summary>
    private static ImagePoint PointOf(IReadOnlyList<ImagePoint> corners, double across, double down)
    {
        double topLeft = (1.0 - across) * (1.0 - down);
        double topRight = across * (1.0 - down);
        double bottomRight = across * down;
        double bottomLeft = (1.0 - across) * down;
        return new ImagePoint(
            (corners[0].X * topLeft) + (corners[1].X * topRight) + (corners[2].X * bottomRight) + (corners[3].X * bottomLeft),
            (corners[0].Y * topLeft) + (corners[1].Y * topRight) + (corners[2].Y * bottomRight) + (corners[3].Y * bottomLeft));
    }

    private static double Linear(byte value)
    {
        return (double)LinearLight.Of(value) / LinearLight.Full;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }

    private static string Number(double number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
