using System.Collections.Generic;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Camera;

/// <summary>
/// Aim assist for a controller (D-14, D-244). It pulls the aim ray toward the target with the smallest angle to
/// the ray, when that angle is inside the cone, by a fraction of the angle. It runs on a tick whose intent sets
/// the controller aim bit, and on no other (D-243).
/// </summary>
/// <remarks>
/// <para>
/// The assist changes the ray and never the yaw and pitch sums, so the state and the hash stay as they are. A
/// target is a point in Phase 1, and PR-16 gives the enemy positions.
/// </para>
/// <para>
/// The pull is a spherical interpolation between the two unit directions, so the new direction sits at exactly
/// the fraction of the angle, whatever the strength. The angle comes from <see cref="DetMath.Atan2"/> of the
/// cross product length and the dot product, which is exact near zero where a dot product alone is not.
/// </para>
/// </remarks>
public static class AimAssist
{
    /// <summary>The half-angle of the cone, in degrees (D-244). A target outside it gets no pull.</summary>
    public const float ConeDegrees = 5.0f;

    /// <summary>The fraction of the angle that the pull covers (D-244).</summary>
    public const float Strength = 0.5f;

    private const float ConeRadians = ConeDegrees * (DetMath.Pi / 180.0f);

    /// <summary>The ray after the assist. The same ray comes back when the flag is clear, when no target is inside the cone, or when the ray already points at the target.</summary>
    /// <param name="ray">The raw aim ray, with a unit direction.</param>
    /// <param name="targets">The target points. A target at the origin of the ray has no direction, and it gets no pull.</param>
    /// <param name="controllerAim">The controller aim bit of the intent (D-243).</param>
    public static AimRay Apply(AimRay ray, IReadOnlyList<Vector3> targets, bool controllerAim)
    {
        if (!controllerAim)
        {
            return ray;
        }

        bool found = false;
        float nearestAngle = 0.0f;
        Vector3 nearestDirection = new(0.0f, 0.0f, 0.0f);
        foreach (Vector3 target in targets)
        {
            Vector3 offset = target - ray.Origin;
            float distance = offset.Length();
            if (distance == 0.0f)
            {
                continue;
            }

            Vector3 direction = offset * (1.0f / distance);
            float angle = AngleBetween(ray.Direction, direction);

            // The first target of a tie keeps its place, so the list order decides and the result replays.
            if (!found || angle < nearestAngle)
            {
                found = true;
                nearestAngle = angle;
                nearestDirection = direction;
            }
        }

        if (!found || nearestAngle > ConeRadians || nearestAngle == 0.0f)
        {
            return ray;
        }

        float sinAngle = DetMath.Sin(nearestAngle);
        float rayWeight = DetMath.Sin((1.0f - Strength) * nearestAngle) / sinAngle;
        float targetWeight = DetMath.Sin(Strength * nearestAngle) / sinAngle;
        Vector3 pulled = (ray.Direction * rayWeight) + (nearestDirection * targetWeight);
        return new AimRay(ray.Origin, pulled);
    }

    /// <summary>The angle between two unit vectors, in radians, from 0 to pi.</summary>
    private static float AngleBetween(Vector3 first, Vector3 second)
    {
        float sine = Vector3.Cross(first, second).Length();
        float cosine = Vector3.Dot(first, second);
        return DetMath.Atan2(sine, cosine);
    }
}
