using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The floor plan of one chamber: a union of boxes that overlap, smoothed by a cellular pass (D-253, D-255).
/// </summary>
/// <remarks>
/// <para>
/// The first box is centered on the anchor, and each later box is centered on a cell of the union so far, so
/// the union is one connected shape. The cellular pass then rounds each convex corner and fills each notch. A
/// corner goes only when its two neighbors meet through the diagonal cell between them, so the shape stays
/// connected. A notch fills when three of its four neighbors are in the shape. The anchor never goes.
/// </para>
/// <para>
/// The pass runs cell by cell in scan order and reads the shape as it changes, so two corners that share a
/// diagonal never go together. A last pass gives every cell a run of three along X or along Z, because a chamber
/// cell needs the cross-section of D-166 like a tunnel cell: a cell that has no run takes its two X neighbors,
/// and the three then share one run. The draws come from the Procgen stream alone, in one fixed order, so one
/// seed gives one footprint (D-159).
/// </para>
/// </remarks>
public static class ChamberFootprint
{
    /// <summary>The count of cellular passes over the union.</summary>
    public const int SmoothingPasses = 2;

    /// <summary>The footprint of one chamber of a kind, around an anchor, in scan order: Z outer, X inner.</summary>
    public static IReadOnlyList<Column> Make(Rng rng, ChamberKind kind, Column anchor)
    {
        int boxCount = kind.BoxCountMin + rng.NextInt(kind.BoxCountMax - kind.BoxCountMin + 1);

        // Each box is centered on a cell of the union, so the union reaches at most one box side per box from
        // the anchor. The margin of one leaves every cell of the window a full ring of neighbors.
        int radius = (kind.BoxSizeMax * boxCount) + 1;
        int side = (2 * radius) + 1;
        bool[] shape = new bool[side * side];
        List<int> cells = [];

        for (int box = 0; box < boxCount; box++)
        {
            int center = box == 0 ? LocalIndex(radius, radius, side) : cells[rng.NextInt(cells.Count)];
            int centerX = center % side;
            int centerZ = center / side;
            int sizeX = kind.BoxSizeMin + rng.NextInt(kind.BoxSizeMax - kind.BoxSizeMin + 1);
            int sizeZ = kind.BoxSizeMin + rng.NextInt(kind.BoxSizeMax - kind.BoxSizeMin + 1);
            int startX = centerX - (sizeX / 2);
            int startZ = centerZ - (sizeZ / 2);
            for (int z = startZ; z < startZ + sizeZ; z++)
            {
                for (int x = startX; x < startX + sizeX; x++)
                {
                    int index = LocalIndex(x, z, side);
                    if (!shape[index])
                    {
                        shape[index] = true;
                        cells.Add(index);
                    }
                }
            }
        }

        int anchorIndex = LocalIndex(radius, radius, side);
        for (int pass = 0; pass < SmoothingPasses; pass++)
        {
            RoundCorners(shape, side, anchorIndex);
            FillNotches(shape, side);
        }

        RepairRuns(shape, side);

        List<Column> footprint = [];
        for (int z = 1; z < side - 1; z++)
        {
            for (int x = 1; x < side - 1; x++)
            {
                if (shape[LocalIndex(x, z, side)])
                {
                    footprint.Add(new Column(anchor.X + x - radius, anchor.Z + z - radius));
                }
            }
        }

        return footprint;
    }

    /// <summary>The index of one cell of the local window.</summary>
    private static int LocalIndex(int x, int z, int side)
    {
        return x + (z * side);
    }

    /// <summary>
    /// Removes each convex corner: a cell with exactly two neighbors at a right angle, where the diagonal cell
    /// between the two is in the shape. The two neighbors stay connected through that diagonal cell.
    /// </summary>
    private static void RoundCorners(bool[] shape, int side, int anchorIndex)
    {
        for (int z = 1; z < side - 1; z++)
        {
            for (int x = 1; x < side - 1; x++)
            {
                int index = LocalIndex(x, z, side);
                if (!shape[index] || index == anchorIndex)
                {
                    continue;
                }

                bool west = shape[index - 1];
                bool east = shape[index + 1];
                bool north = shape[index - side];
                bool south = shape[index + side];
                int neighbors = (west ? 1 : 0) + (east ? 1 : 0) + (north ? 1 : 0) + (south ? 1 : 0);
                if (neighbors != 2 || west == east)
                {
                    continue;
                }

                int diagonal = index + (east ? 1 : -1) + (south ? side : -side);
                if (shape[diagonal])
                {
                    shape[index] = false;
                }
            }
        }
    }

    /// <summary>
    /// Gives each cell without a run of three along X or along Z its two X neighbors. The two new cells share
    /// the run, and an added cell takes no run from any other cell, so one pass is enough.
    /// </summary>
    private static void RepairRuns(bool[] shape, int side)
    {
        for (int z = 1; z < side - 1; z++)
        {
            for (int x = 1; x < side - 1; x++)
            {
                int index = LocalIndex(x, z, side);
                if (!shape[index] || HasRun(shape, side, index))
                {
                    continue;
                }

                shape[index - 1] = true;
                shape[index + 1] = true;
            }
        }
    }

    /// <summary>Answers whether three cells in a row along X or along Z hold the cell.</summary>
    private static bool HasRun(bool[] shape, int side, int index)
    {
        for (int offset = -2; offset <= 0; offset++)
        {
            bool runX = At(shape, index + offset) && At(shape, index + offset + 1) && At(shape, index + offset + 2);
            bool runZ = At(shape, index + (offset * side)) && At(shape, index + ((offset + 1) * side)) && At(shape, index + ((offset + 2) * side));
            if (runX || runZ)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The cell at an index, or false outside the window.</summary>
    private static bool At(bool[] shape, int index)
    {
        return index >= 0 && index < shape.Length && shape[index];
    }

    /// <summary>Fills each notch: a cell outside the shape with at least three of its four neighbors in it.</summary>
    private static void FillNotches(bool[] shape, int side)
    {
        for (int z = 1; z < side - 1; z++)
        {
            for (int x = 1; x < side - 1; x++)
            {
                int index = LocalIndex(x, z, side);
                if (shape[index])
                {
                    continue;
                }

                int neighbors = (shape[index - 1] ? 1 : 0) + (shape[index + 1] ? 1 : 0) + (shape[index - side] ? 1 : 0) + (shape[index + side] ? 1 : 0);
                if (neighbors >= 3)
                {
                    shape[index] = true;
                }
            }
        }
    }
}
