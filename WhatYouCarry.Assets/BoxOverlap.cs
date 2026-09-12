using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Assets;

/// <summary>
/// The overlap of two posed boxes, by the separating axis test (D-135, D-301). A box at a pose is a rotated
/// box, so an axis-aligned test is not enough.
/// </summary>
/// <remarks>
/// The test projects both boxes on fifteen axes: the three axes of each box, and the nine cross products of
/// one axis of each. The overlap on one axis is the sum of the two projected radii minus the distance of the
/// two centers along it. The depth is the least overlap over the axes. A negative depth is a gap on that axis,
/// so the boxes are apart. A depth of zero is a shared face, an edge, or a corner, and a positive depth is a
/// penetration by that much.
/// </remarks>
public static class BoxOverlap
{
    /// <summary>
    /// The depth under which an overlap is float noise and not a clip, in meters: one hundredth of a
    /// millimeter (D-301). Two faces that touch at a pose can differ by the last bits of a sine.
    /// </summary>
    public const float TouchEpsilon = 0.00001f;

    /// <summary>The squared length under which a cross product of two axes is the zero vector, so the axes are parallel.</summary>
    private const float ParallelEpsilon = 0.000001f;

    /// <summary>Answers whether the two boxes penetrate each other by more than float noise (D-301).</summary>
    public static bool Clips(PosedBox first, PosedBox second)
    {
        return Depth(first, second) > TouchEpsilon;
    }

    /// <summary>The penetration depth of two boxes, in meters. Negative when the boxes are apart, zero when they touch.</summary>
    public static float Depth(PosedBox first, PosedBox second)
    {
        Vector3[] firstAxes = [first.Axes.AxisX(), first.Axes.AxisY(), first.Axes.AxisZ()];
        Vector3[] secondAxes = [second.Axes.AxisX(), second.Axes.AxisY(), second.Axes.AxisZ()];
        Vector3 offset = second.Center - first.Center;

        float depth = float.MaxValue;
        foreach (Vector3 axis in firstAxes)
        {
            depth = Least(depth, OverlapAlong(axis, first, firstAxes, second, secondAxes, offset));
        }

        foreach (Vector3 axis in secondAxes)
        {
            depth = Least(depth, OverlapAlong(axis, first, firstAxes, second, secondAxes, offset));
        }

        foreach (Vector3 firstAxis in firstAxes)
        {
            foreach (Vector3 secondAxis in secondAxes)
            {
                Vector3 cross = Vector3.Cross(firstAxis, secondAxis);
                if (Vector3.Dot(cross, cross) < ParallelEpsilon)
                {
                    continue;
                }

                Vector3 unit = cross * (1.0f / cross.Length());
                depth = Least(depth, OverlapAlong(unit, first, firstAxes, second, secondAxes, offset));
            }
        }

        return depth;
    }

    /// <summary>The overlap of the two projections on one unit axis.</summary>
    private static float OverlapAlong(Vector3 axis, PosedBox first, Vector3[] firstAxes, PosedBox second, Vector3[] secondAxes, Vector3 offset)
    {
        float firstRadius = Radius(axis, first.HalfExtents, firstAxes);
        float secondRadius = Radius(axis, second.HalfExtents, secondAxes);
        float distance = Absolute(Vector3.Dot(offset, axis));
        return firstRadius + secondRadius - distance;
    }

    /// <summary>The half width of one box projected on one unit axis.</summary>
    private static float Radius(Vector3 axis, Vector3 halfExtents, Vector3[] boxAxes)
    {
        return (halfExtents.X * Absolute(Vector3.Dot(boxAxes[0], axis)))
            + (halfExtents.Y * Absolute(Vector3.Dot(boxAxes[1], axis)))
            + (halfExtents.Z * Absolute(Vector3.Dot(boxAxes[2], axis)));
    }

    private static float Absolute(float value)
    {
        return value < 0.0f ? -value : value;
    }

    private static float Least(float first, float second)
    {
        return second < first ? second : first;
    }
}
