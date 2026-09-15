using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Physics;

/// <summary>
/// Swept movement of an axis-aligned box against the grid, one axis at a time (D-80, D-165). Godot physics has
/// no part in it (G-3).
/// </summary>
/// <remarks>
/// <para>
/// The sweep moves the box along Y, then X, then Z. On each axis it finds the nearest solid part in the path of
/// the leading face and cuts the move there. It reads every cell in the path, so a box never crosses a block at
/// any speed, and a move that a wall cuts on one axis still runs in full on the other two.
/// </para>
/// <para>
/// A ramp is solid under its slope alone (D-345, D-367). A box stands on the highest point of the slope under its
/// footprint. Before a move along X or Z, the sweep lifts the box onto a slope that rises under the end of the
/// move, when the slope rises no more than a slope of 1:2 over the move and the box has room above. The sweep never
/// lifts a box onto a block, so a step still needs a jump (D-165). A slope that no lift clears cuts the move where
/// it meets the feet, and the side of a ramp is a wall.
/// </para>
/// <para>
/// The sweep stops a face <see cref="ContactSkin"/> before a block face or a slope, and never on it (D-235). A
/// float position cannot hold an exact contact: for twelve integer faces below 130, such as x = 16 with the 0.3
/// half-width, (c - h) + h is one ulp off, so a box set on the face would sit one ulp inside the block. The skin
/// is 2^-10 meters, which is exact in float and 128 times the ulp of a position below 128 meters.
/// </para>
/// </remarks>
public static class SweptAabb
{
    /// <summary>The gap that the sweep keeps between a box face and a block face, in meters (D-235). 2^-10, exact in float.</summary>
    public const float ContactSkin = 0.0009765625f;

    /// <summary>
    /// Moves a box by a displacement and gives back the part of it that the grid allows, and the axes that a
    /// solid part cut. The vertical part of the result holds the lifts onto a ramp.
    /// </summary>
    /// <exception cref="ContextException">A coordinate is not finite, the box has no volume, or the box already overlaps a solid part.</exception>
    public static SweepResult Sweep(VoxelGrid grid, Aabb box, Vector3 delta)
    {
        CheckFinite("box.Min", box.Min);
        CheckFinite("box.Max", box.Max);
        CheckFinite("delta", delta);

        if (box.Min.X >= box.Max.X || box.Min.Y >= box.Max.Y || box.Min.Z >= box.Max.Z)
        {
            ContextException flat = new($"The box has no volume. Its corners are {box.Min} and {box.Max}.");
            flat.AddContext("min", box.Min.ToString());
            flat.AddContext("max", box.Max.ToString());
            throw flat;
        }

        // A box inside rock has no correct move, and a sweep from there would give a number that looks right.
        // The spawn check and this check together keep the invariant that a body never overlaps a block (T-2).
        if (Overlaps(grid, box))
        {
            ContextException inside = new($"The box overlaps a solid cell before the move. Its corners are {box.Min} and {box.Max}.");
            inside.AddContext("min", box.Min.ToString());
            inside.AddContext("max", box.Max.ToString());
            throw inside;
        }

        float allowedY = SweepVertical(grid, box, delta.Y, out bool blockedY);
        box = box.Moved(new Vector3(0.0f, allowedY, 0.0f));

        // A slope that rises under the end of a move lifts the box first, so the move runs over the slope (D-345).
        float liftX = RampLift(grid, box, Axis.X, delta.X);
        box = box.Moved(new Vector3(0.0f, liftX, 0.0f));
        float allowedX = SweepAcross(grid, box, Axis.X, delta.X, out bool blockedX);
        box = box.Moved(new Vector3(allowedX, 0.0f, 0.0f));

        float liftZ = RampLift(grid, box, Axis.Z, delta.Z);
        box = box.Moved(new Vector3(0.0f, liftZ, 0.0f));
        float allowedZ = SweepAcross(grid, box, Axis.Z, delta.Z, out bool blockedZ);

        return new SweepResult(new Vector3(allowedX, allowedY + liftX + liftZ, allowedZ), blockedX, blockedY, blockedZ);
    }

    /// <summary>
    /// Answers whether a box overlaps a solid part. A box covers the open interval on each axis, so a face that
    /// touches a block face or a slope is contact and not overlap. A box that reaches past the grid overlaps the
    /// outside, which is solid (D-237). A box overlaps a ramp when its bottom lies under the highest point of the
    /// slope under it (D-367).
    /// </summary>
    public static bool Overlaps(VoxelGrid grid, Aabb box)
    {
        // The comparisons are written so that a NaN coordinate counts as outside. This check also keeps every
        // cast below inside the range of an int.
        if (!(box.Min.X >= 0.0f) || !(box.Max.X <= grid.SizeX)
            || !(box.Min.Y >= 0.0f) || !(box.Max.Y <= grid.SizeY)
            || !(box.Min.Z >= 0.0f) || !(box.Max.Z <= grid.SizeZ))
        {
            return true;
        }

        // The cells a box covers on one axis run from floor(min) to ceil(max) - 1. The second form is
        // -floor(-max) - 1, so a max on a block face names the cell below the face and not the one above it.
        int lowX = (int)DetMath.Floor(box.Min.X);
        int highX = -(int)DetMath.Floor(-box.Max.X) - 1;
        int lowY = (int)DetMath.Floor(box.Min.Y);
        int highY = -(int)DetMath.Floor(-box.Max.Y) - 1;
        int lowZ = (int)DetMath.Floor(box.Min.Z);
        int highZ = -(int)DetMath.Floor(-box.Max.Z) - 1;
        for (int y = lowY; y <= highY; y++)
        {
            for (int z = lowZ; z <= highZ; z++)
            {
                for (int x = lowX; x <= highX; x++)
                {
                    // A ramp in a row over the bottom row always meets the box, because its slope lies over its row.
                    if (grid.TryGetRamp(x, y, z, out Ramp ramp))
                    {
                        if (box.Min.Y < ramp.HighestUnder(x, y, z, box.Min.X, box.Max.X, box.Min.Z, box.Max.Z))
                        {
                            return true;
                        }
                    }
                    else if (grid.IsSolid(x, y, z))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// The part of a move along Y that the grid allows, for a box that overlaps no solid part. Up, a ramp stops the
    /// top face at its base like a block, because its slope covers the whole base. Down, the scan starts in the row
    /// of the bottom face, because a ramp in that row can hold the box on its slope. The candidate rows run to the
    /// row that holds the end of the move and the skin, so a move that would end inside the skin stops at the skin.
    /// </summary>
    private static float SweepVertical(VoxelGrid grid, Aabb box, float delta, out bool blocked)
    {
        blocked = false;
        if (delta == 0.0f)
        {
            return 0.0f;
        }

        if (delta > 0.0f)
        {
            int lowX = (int)DetMath.Floor(box.Min.X);
            int highX = -(int)DetMath.Floor(-box.Max.X) - 1;
            int lowZ = (int)DetMath.Floor(box.Min.Z);
            int highZ = -(int)DetMath.Floor(-box.Max.Z) - 1;

            // The row at the size is outside the grid and solid, so the loop always ends inside an int.
            int nearRow = -(int)DetMath.Floor(-box.Max.Y);
            float endFace = box.Max.Y + delta + ContactSkin;
            int farRow = endFace >= grid.SizeY ? grid.SizeY : (int)DetMath.Floor(endFace);
            for (int row = nearRow; row <= farRow; row++)
            {
                if (!grid.IsAnySolid(lowX, highX, row, row, lowZ, highZ))
                {
                    continue;
                }

                // The move ends one skin before the base of the row, and a face already inside the skin does not move.
                float limit = (row - ContactSkin) - box.Max.Y;
                if (limit < delta)
                {
                    blocked = true;
                    return limit < 0.0f ? 0.0f : limit;
                }

                return delta;
            }

            return delta;
        }

        // The row at -1 is outside the grid and solid, so the loop always ends at the base of the grid.
        int firstRow = (int)DetMath.Floor(box.Min.Y);
        float lastFace = box.Min.Y + delta - ContactSkin;
        int lastRow = lastFace <= 0.0f ? -1 : -(int)DetMath.Floor(-lastFace) - 1;
        for (int row = firstRow; row >= lastRow; row--)
        {
            if (!grid.TryTopUnder(row, box.Min.X, box.Max.X, box.Min.Z, box.Max.Z, out float top))
            {
                continue;
            }

            float limit = (top + ContactSkin) - box.Min.Y;
            if (limit > delta)
            {
                blocked = true;
                return limit > 0.0f ? 0.0f : limit;
            }

            return delta;
        }

        return delta;
    }

    /// <summary>
    /// The height that a move along X or Z lifts the box onto a ramp before the move, or zero (D-345). The slope
    /// under the footprint at the end of the move decides, and the lift leaves one skin over it. The lift needs a
    /// ramp at the highest point under that footprint, a rise of no more than a slope of 1:2 over the move, and room
    /// over the box. A block as high as the slope, such as the floor at the top of a ramp, does not stop the lift.
    /// </summary>
    private static float RampLift(VoxelGrid grid, Aabb box, Axis axis, float delta)
    {
        if (delta == 0.0f)
        {
            return 0.0f;
        }

        Aabb end = axis == Axis.X ? box.Moved(new Vector3(delta, 0.0f, 0.0f)) : box.Moved(new Vector3(0.0f, 0.0f, delta));

        // A move that ends past the edge of the grid stops at the edge, which is a wall (D-237).
        if (!(end.Min.X >= 0.0f) || !(end.Max.X <= grid.SizeX) || !(end.Min.Z >= 0.0f) || !(end.Max.Z <= grid.SizeZ))
        {
            return 0.0f;
        }

        float distance = delta > 0.0f ? delta : -delta;
        float reach = (distance / Ramp.SteepestRun) + ContactSkin;
        int lowX = (int)DetMath.Floor(end.Min.X);
        int highX = -(int)DetMath.Floor(-end.Max.X) - 1;
        int lowZ = (int)DetMath.Floor(end.Min.Z);
        int highZ = -(int)DetMath.Floor(-end.Max.Z) - 1;
        int lowRow = (int)DetMath.Floor(box.Min.Y);
        int highRow = (int)DetMath.Floor(box.Min.Y + reach);

        bool slopeFound = false;
        float slopeTop = 0.0f;
        bool blockFound = false;
        float blockTop = 0.0f;
        for (int row = lowRow; row <= highRow; row++)
        {
            for (int z = lowZ; z <= highZ; z++)
            {
                for (int x = lowX; x <= highX; x++)
                {
                    if (grid.TryGetRamp(x, row, z, out Ramp ramp))
                    {
                        float top = ramp.HighestUnder(x, row, z, end.Min.X, end.Max.X, end.Min.Z, end.Max.Z);
                        if (!slopeFound || top > slopeTop)
                        {
                            slopeTop = top;
                            slopeFound = true;
                        }
                    }
                    else if (grid.IsSolid(x, row, z) && (!blockFound || row + 1 > blockTop))
                    {
                        blockTop = row + 1;
                        blockFound = true;
                    }
                }
            }
        }

        if (!slopeFound || (blockFound && blockTop > slopeTop))
        {
            return 0.0f;
        }

        float lift = (slopeTop + ContactSkin) - box.Min.Y;
        if (lift <= 0.0f || lift > reach)
        {
            return 0.0f;
        }

        // The top face needs room for the whole lift. A row past the top of the grid is outside and solid.
        int boxLowX = (int)DetMath.Floor(box.Min.X);
        int boxHighX = -(int)DetMath.Floor(-box.Max.X) - 1;
        int boxLowZ = (int)DetMath.Floor(box.Min.Z);
        int boxHighZ = -(int)DetMath.Floor(-box.Max.Z) - 1;
        int nearRow = -(int)DetMath.Floor(-box.Max.Y);
        int farRow = (int)DetMath.Floor(box.Max.Y + lift + ContactSkin);
        if (grid.IsAnySolid(boxLowX, boxHighX, nearRow, farRow, boxLowZ, boxHighZ))
        {
            return 0.0f;
        }

        return lift;
    }

    /// <summary>
    /// The part of a move along X or Z that the grid allows, for a box that overlaps no solid part. The columns run
    /// in the order of the move, from the column that the leading face stands in to the column that holds the end of
    /// the move and the skin. A block, the base of a ramp over the feet row, the high face of a slope that falls
    /// ahead, and the side of a slope that rises across the move cut the move one skin before the face of their
    /// column. A slope that rises ahead cuts the move one skin before the place where it meets the feet. In the
    /// column that the leading face stands in, only a slope that rises ahead can meet the box.
    /// </summary>
    private static float SweepAcross(VoxelGrid grid, Aabb box, Axis axis, float delta, out bool blocked)
    {
        blocked = false;
        if (delta == 0.0f)
        {
            return 0.0f;
        }

        bool positive = delta > 0.0f;
        float lead;
        int size;
        int lowCross;
        int highCross;
        if (axis == Axis.X)
        {
            lead = positive ? box.Max.X : box.Min.X;
            size = grid.SizeX;
            lowCross = (int)DetMath.Floor(box.Min.Z);
            highCross = -(int)DetMath.Floor(-box.Max.Z) - 1;
        }
        else
        {
            lead = positive ? box.Max.Z : box.Min.Z;
            size = grid.SizeZ;
            lowCross = (int)DetMath.Floor(box.Min.X);
            highCross = -(int)DetMath.Floor(-box.Max.X) - 1;
        }

        int footRow = (int)DetMath.Floor(box.Min.Y);
        int highRow = -(int)DetMath.Floor(-box.Max.Y) - 1;

        // The column past the grid on either side is outside and solid, so the loop always ends inside an int.
        int firstColumn;
        int lastColumn;
        if (positive)
        {
            firstColumn = -(int)DetMath.Floor(-lead) - 1;
            float endFace = lead + delta + ContactSkin;
            lastColumn = endFace >= size ? size : (int)DetMath.Floor(endFace);
        }
        else
        {
            firstColumn = (int)DetMath.Floor(lead);
            float endFace = lead + delta - ContactSkin;
            lastColumn = endFace <= 0.0f ? -1 : -(int)DetMath.Floor(-endFace) - 1;
        }

        int step = positive ? 1 : -1;
        for (int column = firstColumn; positive ? column <= lastColumn : column >= lastColumn; column += step)
        {
            bool entering = column != firstColumn;
            float face = positive ? (column - ContactSkin) - lead : (column + 1 + ContactSkin) - lead;
            bool cut = false;
            float limit = 0.0f;
            for (int row = footRow; row <= highRow; row++)
            {
                for (int cross = lowCross; cross <= highCross; cross++)
                {
                    int x = axis == Axis.X ? column : cross;
                    int z = axis == Axis.X ? cross : column;
                    float cellLimit;
                    if (grid.TryGetRamp(x, row, z, out Ramp ramp))
                    {
                        bool alongMove = ramp.RisesAlongX == (axis == Axis.X);
                        bool risesAhead = alongMove && positive == (ramp.Rise == RampRise.PlusX || ramp.Rise == RampRise.PlusZ);
                        if (row == footRow && risesAhead)
                        {
                            // The place along the rise where the slope meets the feet. At the high face or past it,
                            // the feet clear the slope of the cell.
                            float meet = ramp.MeetAt(row, box.Min.Y);
                            if (meet >= 1.0f)
                            {
                                continue;
                            }

                            meet = meet < 0.0f ? 0.0f : meet;
                            cellLimit = positive ? ((column + meet) - ContactSkin) - lead : (((column + 1) - meet) + ContactSkin) - lead;
                        }
                        else
                        {
                            float top = alongMove ? ramp.SlopeAt(row, 1.0f) : ramp.HighestUnder(x, row, z, box.Min.X, box.Max.X, box.Min.Z, box.Max.Z);
                            if (!entering || (row == footRow && box.Min.Y >= top))
                            {
                                continue;
                            }

                            cellLimit = face;
                        }
                    }
                    else if (entering && grid.IsSolid(x, row, z))
                    {
                        cellLimit = face;
                    }
                    else
                    {
                        continue;
                    }

                    if (!cut || (positive ? cellLimit < limit : cellLimit > limit))
                    {
                        limit = cellLimit;
                        cut = true;
                    }
                }
            }

            if (!cut)
            {
                continue;
            }

            // The nearest cut decides. A face that is already inside the skin does not move at all.
            if (positive && limit < delta)
            {
                blocked = true;
                return limit < 0.0f ? 0.0f : limit;
            }

            if (!positive && limit > delta)
            {
                blocked = true;
                return limit > 0.0f ? 0.0f : limit;
            }

            return delta;
        }

        return delta;
    }

    /// <summary>Stops a coordinate that is not a number, because no sweep can read it (T-2).</summary>
    private static void CheckFinite(string name, Vector3 value)
    {
        if (float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z))
        {
            return;
        }

        ContextException error = new($"The sweep needs finite coordinates, and {name} is {value}.");
        error.AddContext("name", name);
        error.AddContext("value", value.ToString());
        throw error;
    }
}

/// <summary>
/// What a sweep gives back: the displacement that the grid allowed, and the axes on which a solid part cut the
/// move.
/// </summary>
public readonly record struct SweepResult(Vector3 Allowed, bool BlockedX, bool BlockedY, bool BlockedZ);
