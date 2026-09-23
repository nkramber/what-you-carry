using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Pathfinding;

/// <summary>
/// The rules that walk a body along a cell path of <see cref="GridMoves"/>: the cell under the feet, the arrival
/// at a waypoint, and the jump that a step up needs (D-165, D-345).
/// </summary>
/// <remarks>
/// A path holds cells, and a body holds a position in meters. These rules join the two. Two readers walk a path
/// (D-111): the greedy descender of the bots, which turns each answer into an intent, and the humanoid brain of
/// the AI, which turns each answer into a velocity. One copy of the rules keeps the two walks alike.
/// </remarks>
public static class PathWalk
{
    /// <summary>How near the feet center must come to the center of a waypoint cell, in meters.</summary>
    public const float Arrival = 0.02f;

    /// <summary>The height of the eye point over the feet, in meters: near the top of the box of D-165.</summary>
    public const float EyeHeight = 1.6f;

    /// <summary>How near a body walks straight at a target in place of a path, in meters.</summary>
    public const float StraightRange = 4.0f;

    /// <summary>The height difference, in meters, at which a straight walk gives way to a path. One block needs a jump or a ramp (D-165, D-345).</summary>
    public const float StraightRise = 1.0f;

    /// <summary>The floor cell under a feet center: the cell that the ground probe of the body reads (D-235).</summary>
    public static Cell FloorCellOf(Vector3 feet)
    {
        return new Cell(
            (int)DetMath.Floor(feet.X),
            (int)DetMath.Floor(feet.Y - SweptAabb.GroundProbe),
            (int)DetMath.Floor(feet.Z));
    }

    /// <summary>Answers whether a body stands centered on a cell, on the ground, with that cell under its feet.</summary>
    public static bool Arrived(Vector3 feet, bool onGround, Cell cell)
    {
        float deltaX = cell.X + 0.5f - feet.X;
        float deltaZ = cell.Z + 0.5f - feet.Z;
        bool centered = DetMath.Abs(deltaX) < Arrival && DetMath.Abs(deltaZ) < Arrival;
        return centered && onGround && FloorCellOf(feet) == cell;
    }

    /// <summary>
    /// Answers whether a body needs a jump to reach the floor of the next cell (D-165). The rule reads the height
    /// of that floor against the ground under the body, and not the row of the cell, because a body on a ramp
    /// stands inside its own row and the floor of a ramp cell lies between its row and the next (D-345).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The body enters the next cell at the point of that cell nearest the feet: a face for a side move, and a
    /// corner for a diagonal move. The floor of a ramp cell is its slope at that point. A walk onto the side of a
    /// ramp meets the slope higher up, and that rise needs a jump like any other.
    /// </para>
    /// <para>
    /// The ground is the slope under the feet at that same point, because the slope carries the body up to it
    /// with no jump. A body at the middle of a ramp cell stands half a place under the face of the next place, so
    /// a rule that read the feet alone gave a jump on each place of a climb (F-108). Off a ramp, the ground is the
    /// feet.
    /// </para>
    /// </remarks>
    public static bool NeedsAJump(VoxelGrid grid, Cell next, Vector3 feet)
    {
        float entryX = DetMath.Clamp(feet.X, next.X, next.X + 1.0f);
        float entryZ = DetMath.Clamp(feet.Z, next.Z, next.Z + 1.0f);
        float floor = next.Y + 1.0f;
        if (grid.TryGetRamp(next.X, next.Y, next.Z, out Ramp slope))
        {
            float along = DetMath.Clamp(slope.Along(next.X, next.Z, entryX, entryZ), 0.0f, 1.0f);
            floor = slope.SlopeAt(next.Y, along);
        }

        float ground = feet.Y;
        Cell under = FloorCellOf(feet);
        if (grid.TryGetRamp(under.X, under.Y, under.Z, out Ramp slopeUnder))
        {
            float alongUnder = DetMath.Clamp(slopeUnder.Along(under.X, under.Z, entryX, entryZ), 0.0f, 1.0f);
            ground = slopeUnder.SlopeAt(under.Y, alongUnder);
        }

        return floor - ground > Arrival;
    }

    /// <summary>The feet center of the middle of one cell, in meters: the point that a walk to that cell aims at.</summary>
    public static Vector3 CenterOf(Cell cell)
    {
        return new Vector3(cell.X + 0.5f, cell.Y + 1.0f, cell.Z + 0.5f);
    }

    /// <summary>
    /// The share of full speed that moves a body toward a point, in the horizontal plane: a unit direction, or a
    /// shorter one when the point lies nearer than one tick of motion. A body that already stands on the point
    /// gets zero.
    /// </summary>
    /// <param name="feet">The feet center of the body, in meters (D-235).</param>
    /// <param name="point">The point to walk to. The walk reads its X and Z alone.</param>
    /// <param name="metresPerTick">How far the body moves in one tick at its full speed.</param>
    /// <remarks>
    /// A body that always moves its full step passes the point and turns back on the next tick, so it swings
    /// around the point and never comes inside <see cref="Arrival"/> of it. The last step is short, so the body
    /// lands on the point and the walk goes on to the next waypoint.
    /// </remarks>
    public static Vector3 Toward(Vector3 feet, Vector3 point, float metresPerTick)
    {
        float deltaX = point.X - feet.X;
        float deltaZ = point.Z - feet.Z;
        float length = DetMath.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
        if (length == 0.0f)
        {
            return new Vector3(0.0f, 0.0f, 0.0f);
        }

        float share = length < metresPerTick ? length / metresPerTick : 1.0f;
        return new Vector3(deltaX / length * share, 0.0f, deltaZ / length * share);
    }

    /// <summary>Answers whether a clear ray joins the eye points of two bodies (D-400). A ramp stops the ray like any solid part (D-246).</summary>
    public static bool Sees(VoxelGrid grid, Vector3 feet, Vector3 otherFeet)
    {
        Vector3 eye = new(feet.X, feet.Y + EyeHeight, feet.Z);
        Vector3 otherEye = new(otherFeet.X, otherFeet.Y + EyeHeight, otherFeet.Z);
        return !GridRay.FirstSolid(grid, eye, otherEye).Hit;
    }

    /// <summary>The distance between two feet centers, in meters.</summary>
    public static float Distance(Vector3 from, Vector3 to)
    {
        float deltaX = to.X - from.X;
        float deltaY = to.Y - from.Y;
        float deltaZ = to.Z - from.Z;
        return DetMath.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
    }

    /// <summary>
    /// Answers whether a body walks straight at a target in place of a path: the target stands near, at nearly the
    /// height of the body, and a clear ray joins the two eye points.
    /// </summary>
    /// <remarks>
    /// A walk along a path aims at the center of the next cell. Two bodies that each walk to the cell of the other
    /// swap cells faster than either crosses one, and each new path then turns the walk back, so the two swing
    /// around each other and never meet. A straight walk over the last meters aims at the body and not at a cell,
    /// so it closes (F-104).
    /// </remarks>
    public static bool CanWalkStraight(VoxelGrid grid, Vector3 feet, Vector3 target)
    {
        float rise = DetMath.Abs(target.Y - feet.Y);
        if (rise >= StraightRise || Distance(feet, target) > StraightRange)
        {
            return false;
        }

        return Sees(grid, feet, target);
    }
}
