using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.World;

/// <summary>One rectangle of equal values of a face mask: its first cell, its size along the two axes of the mask, and the value.</summary>
public readonly record struct MaskRectangle<TKey>(int U, int V, int Width, int Height, TKey Key);

/// <summary>
/// The greedy sweep of a face mask (D-78): from the first cell with a face, the widest run of equal values along U,
/// then the tallest stack of such runs along V, taken as one rectangle. The block faces and the ramp slopes of the
/// mesher both read their masks through it.
/// </summary>
public static class GreedySweep
{
    private const string LengthField = "length";

    /// <summary>
    /// Every rectangle of the mask, in the order of their first cells. The mask runs U fastest, and the default value
    /// of the key is no face. The sweep clears each cell that it takes, so the mask holds no face when it returns.
    /// </summary>
    /// <exception cref="ContextException">The length of the mask is not the product of the two sizes.</exception>
    public static List<MaskRectangle<TKey>> Rectangles<TKey>(TKey[] mask, int sizeU, int sizeV)
        where TKey : struct, IEquatable<TKey>
    {
        if (mask.Length != sizeU * sizeV)
        {
            ContextException error = new($"A face mask of {sizeU} by {sizeV} cells holds {sizeU * sizeV} values, and this mask holds {mask.Length}.");
            error.AddContext(nameof(sizeU), sizeU.ToString(CultureInfo.InvariantCulture));
            error.AddContext(nameof(sizeV), sizeV.ToString(CultureInfo.InvariantCulture));
            error.AddContext(LengthField, mask.Length.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        List<MaskRectangle<TKey>> rectangles = [];
        for (int v = 0; v < sizeV; v++)
        {
            for (int u = 0; u < sizeU; u++)
            {
                TKey key = mask[u + (v * sizeU)];
                if (key.Equals(default))
                {
                    continue;
                }

                int width = 1;
                while (u + width < sizeU && mask[u + width + (v * sizeU)].Equals(key))
                {
                    width++;
                }

                int height = 1;
                bool nextRowMatches = true;
                while (v + height < sizeV && nextRowMatches)
                {
                    for (int du = 0; du < width; du++)
                    {
                        if (!mask[u + du + ((v + height) * sizeU)].Equals(key))
                        {
                            nextRowMatches = false;
                            break;
                        }
                    }

                    if (nextRowMatches)
                    {
                        height++;
                    }
                }

                for (int dv = 0; dv < height; dv++)
                {
                    for (int du = 0; du < width; du++)
                    {
                        mask[u + du + ((v + dv) * sizeU)] = default;
                    }
                }

                rectangles.Add(new MaskRectangle<TKey>(u, v, width, height, key));
            }
        }

        return rectangles;
    }
}
