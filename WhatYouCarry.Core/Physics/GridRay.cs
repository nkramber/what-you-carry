using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Physics;

/// <summary>
/// A ray march through the grid along a line segment (D-246). It visits every cell that the segment passes,
/// in order, and reports the first solid part.
/// </summary>
/// <remarks>
/// <para>
/// This is the grid traversal of Amanatides and Woo. The march keeps, for each axis, the segment parameter at
/// which the line next crosses a cell boundary on that axis, and it steps the axis with the nearest crossing.
/// A tie goes to X, then Y, then Z, so a line through a corner passes one cell at a time and never skips the
/// cell at the corner.
/// </para>
/// <para>
/// The per-axis sweep of PR-7 follows a staircase and not the line, so a diagonal boom through it would leave
/// the camera beside the line. This march is exact along the line. A cell outside the grid is solid (D-237),
/// so a segment of any length ends at the edge at the latest.
/// </para>
/// <para>
/// A ramp is solid under its slope alone (D-345, D-367). The height of a point over the slope changes linearly
/// along the segment, so the march reads it where the segment enters and leaves the cell, and a change of sign
/// gives the point where the segment meets the slope. The camera boom and every shot then stop at the slope, and
/// not at the face of the cell.
/// </para>
/// </remarks>
public static class GridRay
{
    // A segment parameter past the end. An axis that the line never crosses takes it, so that axis never wins
    // the nearest-crossing choice before the march passes the end of the segment.
    private const float Beyond = 2.0f;

    /// <summary>
    /// The first solid part along the segment from <paramref name="start"/> to <paramref name="end"/>. A hit
    /// carries the distance from the start to the point where the line enters the solid part, in meters.
    /// </summary>
    /// <exception cref="ContextException">A coordinate is not finite, or the start is inside a solid part.</exception>
    public static RayHit FirstSolid(VoxelGrid grid, Vector3 start, Vector3 end)
    {
        CheckFinite("start", start);
        CheckFinite("end", end);

        int x = (int)DetMath.Floor(start.X);
        int y = (int)DetMath.Floor(start.Y);
        int z = (int)DetMath.Floor(start.Z);

        // A march from inside rock has no start, and a hit at zero would hide the caller defect (T-2). A start
        // over the slope of a ramp is in air.
        bool startInRamp = grid.TryGetRamp(x, y, z, out Ramp startRamp);
        if ((startInRamp && startRamp.HeightOver(x, y, z, start.X, start.Y, start.Z) < 0.0f) || (!startInRamp && grid.IsSolid(x, y, z)))
        {
            ContextException inside = new($"The ray starts inside a solid cell at {start}.");
            inside.AddContext("start", start.ToString());
            throw inside;
        }

        Vector3 delta = end - start;
        float length = delta.Length();
        if (length == 0.0f)
        {
            return new RayHit(false, 0.0f);
        }

        int stepX = delta.X > 0.0f ? 1 : delta.X < 0.0f ? -1 : 0;
        int stepY = delta.Y > 0.0f ? 1 : delta.Y < 0.0f ? -1 : 0;
        int stepZ = delta.Z > 0.0f ? 1 : delta.Z < 0.0f ? -1 : 0;

        // The segment parameter runs from 0 at the start to 1 at the end. For each axis: the parameter at the
        // next boundary crossing, and the parameter between two crossings.
        float nextX = stepX == 0 ? Beyond : ((stepX > 0 ? x + 1 : x) - start.X) / delta.X;
        float nextY = stepY == 0 ? Beyond : ((stepY > 0 ? y + 1 : y) - start.Y) / delta.Y;
        float nextZ = stepZ == 0 ? Beyond : ((stepZ > 0 ? z + 1 : z) - start.Z) / delta.Z;
        float stepParameterX = stepX == 0 ? 0.0f : stepX / delta.X;
        float stepParameterY = stepY == 0 ? 0.0f : stepY / delta.Y;
        float stepParameterZ = stepZ == 0 ? 0.0f : stepZ / delta.Z;

        // The outside is solid, so the march ends at the edge at the latest. The cap is a guard against a defect
        // in this method, and it never fires on a correct march (T-2).
        int remaining = grid.SizeX + grid.SizeY + grid.SizeZ + 3;
        float enter = 0.0f;
        while (true)
        {
            float parameter = nextX;
            Axis axis = Axis.X;
            if (nextY < parameter)
            {
                parameter = nextY;
                axis = Axis.Y;
            }

            if (nextZ < parameter)
            {
                parameter = nextZ;
                axis = Axis.Z;
            }

            // The current cell is air or a ramp. In a ramp, the segment meets the slope where the height over the
            // slope reaches zero between the entry and the exit of the cell.
            if (grid.TryGetRamp(x, y, z, out Ramp ramp))
            {
                float leave = parameter < 1.0f ? parameter : 1.0f;
                Vector3 entry = start + (delta * enter);
                Vector3 exit = start + (delta * leave);
                float overAtEntry = ramp.HeightOver(x, y, z, entry.X, entry.Y, entry.Z);
                float overAtExit = ramp.HeightOver(x, y, z, exit.X, exit.Y, exit.Z);
                if (overAtEntry <= 0.0f)
                {
                    return new RayHit(true, enter * length);
                }

                if (overAtExit <= 0.0f)
                {
                    float meet = enter + ((leave - enter) * (overAtEntry / (overAtEntry - overAtExit)));
                    return new RayHit(true, meet * length);
                }
            }

            // The end of the segment lies inside the current cell, and the segment meets no solid part there.
            if (parameter > 1.0f)
            {
                return new RayHit(false, length);
            }

            if (axis == Axis.X)
            {
                x += stepX;
                nextX += stepParameterX;
            }
            else if (axis == Axis.Y)
            {
                y += stepY;
                nextY += stepParameterY;
            }
            else
            {
                z += stepZ;
                nextZ += stepParameterZ;
            }

            enter = parameter;

            // A block stops the line at its face. A ramp waits for the slope test of the next pass.
            if (!grid.TryGetRamp(x, y, z, out _) && grid.IsSolid(x, y, z))
            {
                return new RayHit(true, parameter * length);
            }

            remaining--;
            if (remaining < 0)
            {
                ContextException runaway = new($"The ray march from {start} to {end} visited more cells than the grid holds, which is a defect of the march.");
                runaway.AddContext("start", start.ToString());
                runaway.AddContext("end", end.ToString());
                throw runaway;
            }
        }
    }

    /// <summary>Stops a coordinate that is not a number, because no march can read it (T-2).</summary>
    private static void CheckFinite(string name, Vector3 value)
    {
        if (float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z))
        {
            return;
        }

        ContextException error = new($"The ray march needs finite coordinates, and {name} is {value}.");
        error.AddContext("name", name);
        error.AddContext("value", value.ToString());
        throw error;
    }
}

/// <summary>
/// What a ray march gives back. With a hit, the distance runs from the start to the point where the line enters
/// the solid part. Without one, the distance is the length of the whole segment.
/// </summary>
public readonly record struct RayHit(bool Hit, float Distance);
