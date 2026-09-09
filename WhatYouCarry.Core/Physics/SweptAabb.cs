using System.Globalization;
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
/// The sweep moves the box along Y, then X, then Z. On each axis it finds the nearest solid cell in the path of
/// the leading face and cuts the move there. It reads every cell in the path, so a box never crosses a block at
/// any speed, and a move that a wall cuts on one axis still runs in full on the other two.
/// </para>
/// <para>
/// The sweep stops a face <see cref="ContactSkin"/> before a block face, and never on it (D-235). A float
/// position cannot hold an exact contact: for twelve integer faces below 130, such as x = 16 with the 0.3
/// half-width, (c - h) + h is one ulp off, so a box set on the face would sit one ulp inside the block. The
/// skin is 2^-10 meters, which is exact in float and 128 times the ulp of a position below 128 meters.
/// </para>
/// </remarks>
public static class SweptAabb
{
    /// <summary>The gap that the sweep keeps between a box face and a block face, in meters (D-235). 2^-10, exact in float.</summary>
    public const float ContactSkin = 0.0009765625f;

    /// <summary>
    /// Moves a box by a displacement and gives back the part of it that the grid allows, and the axes that a
    /// solid cell cut.
    /// </summary>
    /// <exception cref="ContextException">A coordinate is not finite, the box has no volume, or the box already overlaps a solid cell.</exception>
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

        float allowedY = SweepAxis(grid, box, Axis.Y, delta.Y, out bool blockedY);
        box = box.Moved(new Vector3(0.0f, allowedY, 0.0f));

        float allowedX = SweepAxis(grid, box, Axis.X, delta.X, out bool blockedX);
        box = box.Moved(new Vector3(allowedX, 0.0f, 0.0f));

        float allowedZ = SweepAxis(grid, box, Axis.Z, delta.Z, out bool blockedZ);

        return new SweepResult(new Vector3(allowedX, allowedY, allowedZ), blockedX, blockedY, blockedZ);
    }

    /// <summary>
    /// Answers whether a box overlaps a solid cell. A box covers the open interval on each axis, so a face that
    /// touches a block face is contact and not overlap. A box that reaches past the grid overlaps the outside,
    /// which is solid (D-237).
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
        return grid.IsAnySolid(lowX, highX, lowY, highY, lowZ, highZ);
    }

    /// <summary>
    /// The part of a move along one axis that the grid allows, for a box that overlaps no solid cell. The
    /// leading face is the max face for a positive move and the min face for a negative one. The candidate
    /// cells run from the cell at the leading face to the cell that holds the end of the move plus the skin, so
    /// a move that would end inside the skin of a block face is cut at the skin too.
    /// </summary>
    private static float SweepAxis(VoxelGrid grid, Aabb box, Axis axis, float delta, out bool blocked)
    {
        blocked = false;
        if (delta == 0.0f)
        {
            return 0.0f;
        }

        // The cells the box covers on each axis. The moving axis takes one candidate cell at a time below.
        int lowX = (int)DetMath.Floor(box.Min.X);
        int highX = -(int)DetMath.Floor(-box.Max.X) - 1;
        int lowY = (int)DetMath.Floor(box.Min.Y);
        int highY = -(int)DetMath.Floor(-box.Max.Y) - 1;
        int lowZ = (int)DetMath.Floor(box.Min.Z);
        int highZ = -(int)DetMath.Floor(-box.Max.Z) - 1;

        float lead;
        int size;
        switch (axis)
        {
            case Axis.X:
                lead = delta > 0.0f ? box.Max.X : box.Min.X;
                size = grid.SizeX;
                break;
            case Axis.Y:
                lead = delta > 0.0f ? box.Max.Y : box.Min.Y;
                size = grid.SizeY;
                break;
            default:
                lead = delta > 0.0f ? box.Max.Z : box.Min.Z;
                size = grid.SizeZ;
                break;
        }

        if (delta > 0.0f)
        {
            // From the cell whose near face is at or past the leading face, to the cell that holds the end of
            // the move plus the skin. The cell at the size is outside the grid and solid, so the loop always
            // ends inside an int, whatever the size of the move.
            int nearCell = -(int)DetMath.Floor(-lead);
            float endFace = lead + delta + ContactSkin;
            int farCell = endFace >= size ? size : (int)DetMath.Floor(endFace);
            for (int cell = nearCell; cell <= farCell; cell++)
            {
                bool solid = axis == Axis.X ? grid.IsAnySolid(cell, cell, lowY, highY, lowZ, highZ)
                    : axis == Axis.Y ? grid.IsAnySolid(lowX, highX, cell, cell, lowZ, highZ)
                    : grid.IsAnySolid(lowX, highX, lowY, highY, cell, cell);
                if (!solid)
                {
                    continue;
                }

                // The nearest solid cell decides. The move ends one skin before its near face, and a face that
                // is already inside the skin does not move at all.
                float limit = (cell - ContactSkin) - lead;
                if (limit < delta)
                {
                    blocked = true;
                    return limit < 0.0f ? 0.0f : limit;
                }

                return delta;
            }

            return delta;
        }

        // The mirror image: from the cell behind the leading face, down to the cell whose far face is at or
        // past the end of the move minus the skin. The cell at -1 is outside the grid and solid.
        int firstCell = (int)DetMath.Floor(lead) - 1;
        float lastFace = lead + delta - ContactSkin;
        int lastCell = lastFace <= 0.0f ? -1 : -(int)DetMath.Floor(-lastFace) - 1;
        for (int cell = firstCell; cell >= lastCell; cell--)
        {
            bool solid = axis == Axis.X ? grid.IsAnySolid(cell, cell, lowY, highY, lowZ, highZ)
                : axis == Axis.Y ? grid.IsAnySolid(lowX, highX, cell, cell, lowZ, highZ)
                : grid.IsAnySolid(lowX, highX, lowY, highY, cell, cell);
            if (!solid)
            {
                continue;
            }

            float limit = (cell + 1 + ContactSkin) - lead;
            if (limit > delta)
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
/// What a sweep gives back: the displacement that the grid allowed, and the axes on which a solid cell cut the
/// move.
/// </summary>
public readonly record struct SweepResult(Vector3 Allowed, bool BlockedX, bool BlockedY, bool BlockedZ);
