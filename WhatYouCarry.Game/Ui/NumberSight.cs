using System;
using Godot;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The rules that hide the damage number of an enemy that the camera cannot see (D-727, D-729). The HUD reads them on
/// each frame, so a hidden number hides for that frame alone.
/// </summary>
/// <remarks>
/// <para>
/// A wall hides the owner when the segment from the drawn camera to the center of the owner box meets a solid part.
/// The drawn camera can stand in rock (D-720), and the wall fade clears that rock (D-292). So the march skips the rock
/// that holds the drawn camera, up to the first open cell on the segment, and reads the first solid part after it.
/// </para>
/// <para>
/// The body hides the owner when the center of the owner box falls inside the screen box of the body and stands
/// farther from the camera than the center of the body box.
/// </para>
/// </remarks>
public static class NumberSight
{
    /// <summary>
    /// The step past the entry point of the first open cell, in meters. The entry point lies on a cell face, and the
    /// march that starts there must start in the open cell and not in the rock before it.
    /// </summary>
    public const float PastFace = 0.001f;

    /// <summary>Answers whether a solid part stands between the drawn camera and a target (D-727, D-729).</summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="camera">The drawn camera, which can stand in rock (D-720).</param>
    /// <param name="target">The center of the owner box.</param>
    public static bool WallHides(VoxelGrid grid, CoreVector3 camera, CoreVector3 target)
    {
        // The frame between two ticks can put the drawn camera under the slope of a ramp, which is a solid part. No
        // open cell holds the camera then, so the frame shows no number for the owner.
        if (UnderSlope(grid, camera))
        {
            return true;
        }

        CoreVector3 start = camera;
        if (IsBlock(grid, camera))
        {
            // A segment that never leaves the rock ends in rock, so no open cell shows the target.
            RayHit exit = GridRay.FirstOpenPastRock(grid, camera, target);
            if (!exit.Hit)
            {
                return true;
            }

            CoreVector3 toward = target - camera;
            start = camera + (toward * ((exit.Distance + PastFace) / toward.Length()));

            // An open cell of a ramp that the segment enters under the slope holds the first solid part after the rock.
            if (UnderSlope(grid, start))
            {
                return true;
            }
        }

        return GridRay.FirstSolid(grid, start, target).Hit;
    }

    /// <summary>Answers whether the body hides an owner on the screen (D-727).</summary>
    /// <param name="center">The center of the owner box on the screen.</param>
    /// <param name="body">The screen box of the body.</param>
    /// <param name="ownerDistance">The distance from the drawn camera to the center of the owner box.</param>
    /// <param name="bodyDistance">The distance from the drawn camera to the center of the body box.</param>
    public static bool BodyHides(Vector2 center, Rect2 body, float ownerDistance, float bodyDistance)
    {
        return body.HasPoint(center) && ownerDistance > bodyDistance;
    }

    /// <summary>Answers whether a point stands in a cell of a block: a solid cell that holds no ramp.</summary>
    private static bool IsBlock(VoxelGrid grid, CoreVector3 point)
    {
        int x = (int)MathF.Floor(point.X);
        int y = (int)MathF.Floor(point.Y);
        int z = (int)MathF.Floor(point.Z);
        return !grid.TryGetRamp(x, y, z, out _) && grid.IsSolid(x, y, z);
    }

    /// <summary>Answers whether a point stands in a cell of a ramp, under its slope.</summary>
    private static bool UnderSlope(VoxelGrid grid, CoreVector3 point)
    {
        int x = (int)MathF.Floor(point.X);
        int y = (int)MathF.Floor(point.Y);
        int z = (int)MathF.Floor(point.Z);
        return grid.TryGetRamp(x, y, z, out Ramp ramp) && ramp.HeightOver(x, y, z, point.X, point.Y, point.Z) < 0.0f;
    }
}
