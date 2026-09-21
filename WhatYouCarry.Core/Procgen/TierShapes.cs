using System.Collections.Generic;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The four shapes of a tier inside a chamber footprint (D-388). Each builder gives the candidate floors of its
/// shape at a target count of cells, nearest the target first, or an empty list when the footprint takes none.
/// </summary>
/// <remarks>
/// <para>
/// A builder takes the columns that <see cref="ChamberSpace.CanTakeATier"/> gives, and not every column of the
/// footprint. A column that must stay open chamber floor is outside every candidate, so a shape forms in the part
/// of the chamber that no tunnel crosses (D-348).
/// </para>
/// <para>
/// A builder draws nothing. It reads the footprint and gives one candidate, so the tier plan owns every draw of
/// the Procgen stream (D-159). A builder that has more than one candidate of its shape, one per side of the
/// footprint or one per box of the union, gives the one whose count is nearest the target. A tie takes the
/// earlier one, so one footprint and one target give one candidate.
/// </para>
/// <para>
/// A candidate under <see cref="TierPlan.SmallestTierCells"/> cells is no tier, and the tier plan drops it (D-391).
/// </para>
/// </remarks>
public static class TierShapes
{
    /// <summary>The name of one tier shape in a message. The switch is explicit, so no reflection reads the enum (G-2).</summary>
    public static string NameOf(TierShape shape)
    {
        switch (shape)
        {
            case TierShape.Rectangle: return "a rectangle against one side";
            case TierShape.CutLine: return "the far side of a cut line";
            case TierShape.RaisedBox: return "one box of the union";
            default: return "an island";
        }
    }

    /// <summary>
    /// The largest rectangle inside the footprint against one side of it. The builder walks inward from each of the
    /// four sides, and it keeps the deepest rectangle of full columns at each depth.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<Column>> AgainstOneSide(ChamberSpace space, int target)
    {
        List<IReadOnlyList<Column>> candidates = [];
        List<int> counts = [];
        for (int side = 0; side < 4; side++)
        {
            int bestDepth = 0;
            int bestLow = 0;
            int bestHigh = -1;
            int bestCount = 0;
            int low = 0;
            int high = AcrossCount(space, side) - 1;
            for (int depth = 1; depth <= AlongCount(space, side); depth++)
            {
                if (!LongestRun(space, side, depth - 1, low, high, out low, out high))
                {
                    break;
                }

                int count = depth * (high - low + 1);
                if (!Nearer(bestCount, count, target))
                {
                    continue;
                }

                bestDepth = depth;
                bestLow = low;
                bestHigh = high;
                bestCount = count;
            }

            if (bestCount < TierPlan.SmallestTierCells)
            {
                continue;
            }

            List<Column> rectangle = [];
            for (int along = 0; along < bestDepth; along++)
            {
                for (int across = bestLow; across <= bestHigh; across++)
                {
                    rectangle.Add(At(space, side, along, across));
                }
            }

            Add(candidates, counts, InScanOrder(space, rectangle), bestCount, target);
        }

        return candidates;
    }

    /// <summary>Every column of the footprint past a line across it. The builder tries each of the four sides and every line.</summary>
    public static IReadOnlyList<IReadOnlyList<Column>> PastACutLine(ChamberSpace space, int target)
    {
        List<IReadOnlyList<Column>> candidates = [];
        List<int> counts = [];
        for (int side = 0; side < 4; side++)
        {
            int bestDepth = 0;
            int bestCount = 0;
            int count = 0;
            for (int along = 0; along < AlongCount(space, side); along++)
            {
                for (int across = 0; across < AcrossCount(space, side); across++)
                {
                    if (space.CanTakeATier(At(space, side, along, across)))
                    {
                        count++;
                    }
                }

                if (!Nearer(bestCount, count, target))
                {
                    continue;
                }

                bestDepth = along + 1;
                bestCount = count;
            }

            if (bestCount < TierPlan.SmallestTierCells)
            {
                continue;
            }

            List<Column> past = [];
            for (int along = 0; along < bestDepth; along++)
            {
                for (int across = 0; across < AcrossCount(space, side); across++)
                {
                    Column column = At(space, side, along, across);
                    if (space.CanTakeATier(column))
                    {
                        past.Add(column);
                    }
                }
            }

            Add(candidates, counts, InScanOrder(space, past), bestCount, target);
        }

        return candidates;
    }

    /// <summary>The part of the footprint that one box of its union holds. The builder tries every box (D-253).</summary>
    public static IReadOnlyList<IReadOnlyList<Column>> OneBoxOfTheUnion(ChamberSpace space, int target)
    {
        List<IReadOnlyList<Column>> candidates = [];
        List<int> counts = [];
        foreach (ChamberBox box in space.Boxes)
        {
            List<Column> inBox = [];
            foreach (Column column in space.Footprint)
            {
                if (box.Holds(column) && space.CanTakeATier(column))
                {
                    inBox.Add(column);
                }
            }

            if (inBox.Count >= TierPlan.SmallestTierCells)
            {
                Add(candidates, counts, inBox, inBox.Count, target);
            }
        }

        return candidates;
    }

    /// <summary>
    /// A rectangle inside the footprint that touches no wall. The builder takes the columns whose eight neighbors
    /// are footprint, and it grows a rectangle from the one nearest the middle of the footprint, one side at a time.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<Column>> AnIsland(ChamberSpace space, int target)
    {
        bool[] inside = Inside(space);
        if (!NearestToTheMiddle(space, inside, out Column seed))
        {
            return [];
        }

        int minX = seed.X;
        int maxX = seed.X;
        int minZ = seed.Z;
        int maxZ = seed.Z;
        bool grew = true;
        while (grew && (maxX - minX + 1) * (maxZ - minZ + 1) < target)
        {
            grew = false;
            for (int side = 0; side < 4; side++)
            {
                int tryMinX = side == 1 ? minX - 1 : minX;
                int tryMaxX = side == 0 ? maxX + 1 : maxX;
                int tryMinZ = side == 3 ? minZ - 1 : minZ;
                int tryMaxZ = side == 2 ? maxZ + 1 : maxZ;
                if ((tryMaxX - tryMinX + 1) * (tryMaxZ - tryMinZ + 1) > target || !Covers(space, inside, tryMinX, tryMaxX, tryMinZ, tryMaxZ))
                {
                    continue;
                }

                minX = tryMinX;
                maxX = tryMaxX;
                minZ = tryMinZ;
                maxZ = tryMaxZ;
                grew = true;
            }
        }

        List<Column> island = [];
        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                island.Add(new Column(x, z));
            }
        }

        return island.Count < TierPlan.SmallestTierCells ? [] : [island];
    }

    /// <summary>
    /// Adds one candidate to a list, in the order of its nearness to the target. A tie keeps the earlier one. The
    /// candidate goes at the end, and it moves forward one place at a time, because Core holds no list member that
    /// puts an item inside a list (G-21).
    /// </summary>
    private static void Add(List<IReadOnlyList<Column>> candidates, List<int> counts, IReadOnlyList<Column> candidate, int count, int target)
    {
        candidates.Add(candidate);
        counts.Add(count);
        for (int index = candidates.Count - 1; index > 0; index--)
        {
            if (Gap(counts[index - 1], target) <= Gap(counts[index], target))
            {
                return;
            }

            IReadOnlyList<Column> earlier = candidates[index - 1];
            int earlierCount = counts[index - 1];
            candidates[index - 1] = candidates[index];
            counts[index - 1] = counts[index];
            candidates[index] = earlier;
            counts[index] = earlierCount;
        }
    }

    /// <summary>The distance of a count from the target.</summary>
    private static int Gap(int count, int target)
    {
        return count > target ? count - target : target - count;
    }

    /// <summary>
    /// Answers whether a candidate count is nearer the target than the best count so far. A count under the smallest
    /// tier is no candidate, and a tie keeps the count the builder found first.
    /// </summary>
    private static bool Nearer(int best, int candidate, int target)
    {
        if (candidate < TierPlan.SmallestTierCells)
        {
            return false;
        }

        if (best < TierPlan.SmallestTierCells)
        {
            return true;
        }

        return Gap(candidate, target) < Gap(best, target);
    }

    /// <summary>The columns of a list in scan order: X inner, Z outer.</summary>
    private static List<Column> InScanOrder(ChamberSpace space, List<Column> columns)
    {
        List<Column> ordered = [];
        foreach (Column column in space.Columns(space.Mark(columns)))
        {
            ordered.Add(column);
        }

        return ordered;
    }

    /// <summary>
    /// The longest run of footprint columns at one depth from a side, inside the run of the depth before it. Gives
    /// false when the depth holds no such column.
    /// </summary>
    private static bool LongestRun(ChamberSpace space, int side, int along, int lowIn, int highIn, out int low, out int high)
    {
        low = 0;
        high = -1;
        int runLow = 0;
        int runLength = 0;
        int bestLength = 0;
        for (int across = lowIn; across <= highIn + 1; across++)
        {
            bool holds = across <= highIn && space.CanTakeATier(At(space, side, along, across));
            if (holds)
            {
                runLow = runLength == 0 ? across : runLow;
                runLength++;
                continue;
            }

            if (runLength > bestLength)
            {
                bestLength = runLength;
                low = runLow;
                high = runLow + runLength - 1;
            }

            runLength = 0;
        }

        return bestLength > 0;
    }

    /// <summary>The columns of the footprint whose eight neighbors are footprint columns.</summary>
    private static bool[] Inside(ChamberSpace space)
    {
        bool[] inside = space.EmptyMask();
        foreach (Column column in space.Footprint)
        {
            bool ringed = space.CanTakeATier(column);
            for (int z = column.Z - 1; z <= column.Z + 1 && ringed; z++)
            {
                for (int x = column.X - 1; x <= column.X + 1; x++)
                {
                    if (!space.InFootprint(new Column(x, z)))
                    {
                        ringed = false;
                        break;
                    }
                }
            }

            if (ringed)
            {
                space.Set(inside, column);
            }
        }

        return inside;
    }

    /// <summary>The column of a mask nearest the middle of the window, by the sum of the two distances. A tie takes the earlier column in scan order.</summary>
    private static bool NearestToTheMiddle(ChamberSpace space, bool[] mask, out Column nearest)
    {
        int middleX = (space.MinX + space.MaxX) / 2;
        int middleZ = (space.MinZ + space.MaxZ) / 2;
        int bestDistance = -1;
        nearest = default;
        foreach (Column column in space.Columns(mask))
        {
            int alongX = column.X > middleX ? column.X - middleX : middleX - column.X;
            int alongZ = column.Z > middleZ ? column.Z - middleZ : middleZ - column.Z;
            int distance = alongX + alongZ;
            if (bestDistance < 0 || distance < bestDistance)
            {
                bestDistance = distance;
                nearest = column;
            }
        }

        return bestDistance >= 0;
    }

    /// <summary>Answers whether the mask holds every column of a rectangle.</summary>
    private static bool Covers(ChamberSpace space, bool[] mask, int minX, int maxX, int minZ, int maxZ)
    {
        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (!space.Holds(mask, new Column(x, z)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>The count of columns from one side of the window to the other, along the side.</summary>
    private static int AlongCount(ChamberSpace space, int side)
    {
        return side < 2 ? space.Width - 2 : space.Depth - 2;
    }

    /// <summary>The count of columns across one side of the window.</summary>
    private static int AcrossCount(ChamberSpace space, int side)
    {
        return side < 2 ? space.Depth - 2 : space.Width - 2;
    }

    /// <summary>
    /// One column of the window, by a distance from one side and a distance across it. Side 0 starts at the highest
    /// X, side 1 at the lowest X, side 2 at the highest Z, and side 3 at the lowest Z.
    /// </summary>
    private static Column At(ChamberSpace space, int side, int along, int across)
    {
        switch (side)
        {
            case 0:
                return new Column(space.MaxX - 1 - along, space.MinZ + 1 + across);
            case 1:
                return new Column(space.MinX + 1 + along, space.MinZ + 1 + across);
            case 2:
                return new Column(space.MinX + 1 + across, space.MaxZ - 1 - along);
            default:
                return new Column(space.MinX + 1 + across, space.MinZ + 1 + along);
        }
    }
}
