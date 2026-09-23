using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>The name and the size of one canvas that the packer places.</summary>
public sealed record CanvasSize(string Name, int Width, int Height);

/// <summary>
/// Places each canvas in the atlas on shelves (D-505, D-506). Each canvas takes its size and a gutter on each side.
/// The packer sorts the canvases by height, then by width, both from the largest, then by name. It fills a shelf from
/// the left, and it opens the next shelf under the tallest canvas of the last one. The order and the places depend on
/// the canvases alone, so the atlas has one form.
/// </summary>
public static class AtlasPacker
{
    /// <summary>The place of each canvas, inside its gutter, in the order of the input.</summary>
    /// <exception cref="ContextException">A canvas does not fit in the atlas. The error names the canvas (D-506).</exception>
    public static IReadOnlyList<AtlasRect> Pack(IReadOnlyList<CanvasSize> canvases)
    {
        int[] order = new int[canvases.Count];
        for (int index = 0; index < order.Length; index++)
        {
            order[index] = index;
        }

        Array.Sort(order, (left, right) => Compare(canvases[left], canvases[right]));
        AtlasRect[] places = new AtlasRect[canvases.Count];
        int shelfTop = 0;
        int shelfHeight = 0;
        int cursor = 0;
        foreach (int index in order)
        {
            CanvasSize canvas = canvases[index];
            int cellWidth = canvas.Width + (2 * AtlasLayout.Gutter);
            int cellHeight = canvas.Height + (2 * AtlasLayout.Gutter);
            if (cursor + cellWidth > AtlasLayout.AtlasPixels)
            {
                shelfTop += shelfHeight;
                shelfHeight = 0;
                cursor = 0;
            }

            if (cellWidth > AtlasLayout.AtlasPixels || shelfTop + cellHeight > AtlasLayout.AtlasPixels)
            {
                throw new ContextException($"The atlas of {Text(AtlasLayout.AtlasPixels)} pixels has no room for the canvas {canvas.Name} of {Text(canvas.Width)} by {Text(canvas.Height)} pixels. A larger atlas needs a new decision (D-506).");
            }

            places[index] = new AtlasRect(cursor + AtlasLayout.Gutter, shelfTop + AtlasLayout.Gutter, canvas.Width, canvas.Height);
            cursor += cellWidth;
            shelfHeight = Math.Max(shelfHeight, cellHeight);
        }

        return places;
    }

    private static int Compare(CanvasSize left, CanvasSize right)
    {
        if (left.Height != right.Height)
        {
            return right.Height.CompareTo(left.Height);
        }

        if (left.Width != right.Width)
        {
            return right.Width.CompareTo(left.Width);
        }

        return string.CompareOrdinal(left.Name, right.Name);
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
