using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Camera;

/// <summary>
/// The over-the-shoulder camera (D-13, D-75). It reads the yaw and pitch sums of the loop, sweeps its boom
/// against the grid, and gives the pose that the aim ray starts from (D-77, D-88, D-247).
/// </summary>
/// <remarks>
/// <para>
/// The pivot sits <see cref="PivotHeight"/> over the feet. The shoulder point is the pivot moved right and up in
/// camera space, and the boom runs back from the shoulder point along the look direction (D-242). Two ray
/// marches place the camera. The first runs from the pivot to the shoulder point, because a player who hugs a
/// right wall puts the shoulder point inside rock. The second runs the boom from the shoulder point that the
/// first gave (D-246, D-249). Each march pulls its end point back by <see cref="CameraRadius"/> at a wall.
/// </para>
/// <para>
/// The pivot is always in air: it sits inside the body box, 0.3 meters from every face, and the body never
/// overlaps rock. The camera holds no state, and Core smooths nothing (D-245). A positive pitch looks up
/// (D-248).
/// </para>
/// </remarks>
public static class OrbitCamera
{
    /// <summary>The height of the pivot over the feet, in meters (D-242).</summary>
    public const float PivotHeight = 1.5f;

    /// <summary>The shoulder offset to the right, in camera space, in meters (D-242).</summary>
    public const float ShoulderRight = 0.6f;

    /// <summary>The shoulder offset up, in camera space, in meters (D-242).</summary>
    public const float ShoulderUp = 0.3f;

    /// <summary>The length of the boom from the shoulder point back along the look direction, in meters (D-242).</summary>
    public const float BoomLength = 3.0f;

    /// <summary>The camera radius, in meters. A march pulls its end point back by this much at a wall, so the near plane stays out of it (D-246).</summary>
    public const float CameraRadius = 0.25f;

    // Hundredths of a degree to radians. Both sums stay far below the DetMath angle limit.
    private const float RadiansPerHundredth = DetMath.Pi / 18000.0f;

    /// <summary>The pose of the camera for one feet position and one look.</summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="feet">The feet center of the body.</param>
    /// <param name="yaw">The yaw sum of the loop, in hundredths of a degree, from 0 up to but not including a full turn (D-227).</param>
    /// <param name="pitch">The pitch sum of the loop, in hundredths of a degree, inside the limit of D-241. Positive looks up (D-248).</param>
    /// <exception cref="ContextException">The yaw or the pitch is outside its range, or the pivot is inside a solid cell.</exception>
    public static CameraPose Place(VoxelGrid grid, Vector3 feet, int yaw, int pitch)
    {
        // The loop keeps both sums inside these ranges. A value outside them is a caller defect, and a pose from
        // it would look right (T-2).
        if (yaw < 0 || yaw >= SimulationLoop.FullTurn || pitch < -SimulationLoop.PitchLimit || pitch > SimulationLoop.PitchLimit)
        {
            ContextException error = new($"The camera needs a yaw from 0 to {SimulationLoop.FullTurn - 1} and a pitch from {-SimulationLoop.PitchLimit} to {SimulationLoop.PitchLimit}. The yaw is {yaw}, and the pitch is {pitch}.");
            error.AddContext("yaw", ((long)yaw).ToString(System.Globalization.CultureInfo.InvariantCulture));
            error.AddContext("pitch", ((long)pitch).ToString(System.Globalization.CultureInfo.InvariantCulture));
            throw error;
        }

        float sinYaw = DetMath.Sin(yaw * RadiansPerHundredth);
        float cosYaw = DetMath.Cos(yaw * RadiansPerHundredth);
        float sinPitch = DetMath.Sin(pitch * RadiansPerHundredth);
        float cosPitch = DetMath.Cos(pitch * RadiansPerHundredth);

        // Forward at yaw zero and pitch zero is minus Z, and right is plus X. The yaw turns counterclockwise
        // seen from above, and a positive pitch lifts forward toward plus Y (D-234, D-248). Right stays
        // horizontal, and up follows from the two by the right-hand rule.
        Vector3 forward = new(-sinYaw * cosPitch, sinPitch, -cosYaw * cosPitch);
        Vector3 right = new(cosYaw, 0.0f, -sinYaw);
        Vector3 up = Vector3.Cross(right, forward);

        Vector3 pivot = feet + new Vector3(0.0f, PivotHeight, 0.0f);
        Vector3 shoulder = PullIn(grid, pivot, pivot + (right * ShoulderRight) + (up * ShoulderUp));
        Vector3 position = PullIn(grid, shoulder, shoulder - (forward * BoomLength));
        return new CameraPose(position, forward, right, up);
    }

    /// <summary>
    /// The end of a segment, or the point <see cref="CameraRadius"/> before the first solid cell on it. A wall
    /// nearer than the radius gives the start itself.
    /// </summary>
    private static Vector3 PullIn(VoxelGrid grid, Vector3 start, Vector3 target)
    {
        RayHit hit = GridRay.FirstSolid(grid, start, target);
        if (!hit.Hit)
        {
            return target;
        }

        float room = hit.Distance - CameraRadius;
        if (room <= 0.0f)
        {
            return start;
        }

        Vector3 delta = target - start;
        return start + (delta * (room / delta.Length()));
    }
}
