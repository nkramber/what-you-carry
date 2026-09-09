using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Projectiles;

/// <summary>
/// The launch direction that lands a projectile of one speed on a target under gravity, or the report that no
/// direction does (G-6). Enemy AI aims with it from PR-16 on.
/// </summary>
/// <remarks>
/// <para>
/// For a horizontal distance d and a height h, the launch angle of the low arc satisfies
/// tan a = (v^2 - sqrt(v^4 - g (g d^2 + 2 h v^2))) / (g d). A negative value under the root means no arc
/// reaches the target, and the solver reports it instead of a direction. A target straight up or down takes a
/// vertical direction, reachable when the apex v^2 / (2 g) reaches the height. Without gravity the direction
/// is the straight line.
/// </para>
/// <para>
/// The closed form is the continuous arc, and the fixed-step integration of the simulation lands a little
/// short of it, as the jump of F-83 does. PR-10 exit test 3 asserts one block of tolerance.
/// </para>
/// </remarks>
public static class ArcSolver
{
    /// <summary>The low-arc direction from one point to another for one speed and one gravity, or the report that the target is out of reach.</summary>
    /// <param name="speed">The launch speed, in meters per second, above zero.</param>
    /// <param name="gravity">The downward acceleration, in meters per second squared, zero or above.</param>
    public static ArcSolution Solve(float speed, float gravity, Vector3 from, Vector3 to)
    {
        Vector3 offset = to - from;
        float horizontal = DetMath.Sqrt((offset.X * offset.X) + (offset.Z * offset.Z));
        float height = offset.Y;

        if (gravity == 0.0f)
        {
            float length = offset.Length();
            return length == 0.0f
                ? new ArcSolution(false, new Vector3(0.0f, 0.0f, 0.0f))
                : new ArcSolution(true, offset * (1.0f / length));
        }

        float speedSquared = speed * speed;
        if (horizontal == 0.0f)
        {
            bool reachable = height <= speedSquared / (2.0f * gravity);
            Vector3 vertical = new(0.0f, height < 0.0f ? -1.0f : 1.0f, 0.0f);
            return new ArcSolution(reachable, vertical);
        }

        float root = (speedSquared * speedSquared) - (gravity * ((gravity * horizontal * horizontal) + (2.0f * height * speedSquared)));
        if (root < 0.0f)
        {
            return new ArcSolution(false, offset * (1.0f / offset.Length()));
        }

        float angle = DetMath.Atan2(speedSquared - DetMath.Sqrt(root), gravity * horizontal);
        float cosAngle = DetMath.Cos(angle);
        float sinAngle = DetMath.Sin(angle);
        Vector3 direction = new(offset.X / horizontal * cosAngle, sinAngle, offset.Z / horizontal * cosAngle);
        return new ArcSolution(true, direction);
    }
}

/// <summary>What the arc solver gives: whether a direction reaches the target, and the unit direction. An unreachable target carries the straight line toward it.</summary>
public readonly record struct ArcSolution(bool Reachable, Vector3 Direction);
