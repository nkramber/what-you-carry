using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;

namespace WhatYouCarry.Core.Combat;

/// <summary>One hit of a swing on one tick: the owner id of the box that the blade met, and the damage of the weapon.</summary>
public readonly record struct SwordHit(int Owner, long Damage);

/// <summary>
/// The geometry of a melee swing (D-25, D-315, D-325): the yaw offset of the blade after each active step, and the
/// test of the wedge that one step sweeps. The player holds the state of a swing, and this class holds no state.
/// </summary>
/// <remarks>
/// <para>
/// The blade is a line in the horizontal plane from the body center out to the reach, inside a height band over the
/// feet. Across the active ticks it turns the arc of the weapon from the right of the camera yaw to the left. Each
/// active tick sweeps one step of the arc from the yaw of that tick, so the arc follows the look (D-324).
/// </para>
/// <para>
/// A box takes the hit when it meets the wedge between the two blade lines of the step inside the band. The test is
/// exact: the box holds the center, a box corner lies in the wedge, a blade line crosses the box, or the rim of the
/// wedge crosses an edge of the box. The yaw turns counterclockwise seen from above (D-234), so a point between the
/// two blade lines has a cross product of zero or less with each of them, in the order of the turn.
/// </para>
/// </remarks>
public static class MeleeWeapon
{
    // Hundredths of a degree to radians.
    private const float RadiansPerHundredth = DetMath.Pi / 18000.0f;

    // A half turn, in hundredths of a degree. The test of the blade lines holds for a wedge up to this turn.
    private const int HalfTurn = 18000;

    /// <summary>
    /// The yaw offset of the blade after a count of active steps, in hundredths of a degree: minus half the arc, rounded
    /// toward zero, at step zero, and the rest of the arc after the last step. A negative offset is to the right.
    /// </summary>
    public static int BladeOffset(WeaponDefinition weapon, long step)
    {
        int half = weapon.ArcHundredths / 2;
        return -half + (int)(weapon.ArcHundredths * step / weapon.ActiveTicks);
    }

    /// <summary>The unit direction of a blade line at a yaw in hundredths of a degree, in the horizontal plane (D-234).</summary>
    public static Vector3 BladeDirection(int yawHundredths)
    {
        float radians = yawHundredths * RadiansPerHundredth;
        return new Vector3(-DetMath.Sin(radians), 0.0f, -DetMath.Cos(radians));
    }

    /// <summary>
    /// Answers whether a box meets the wedge that the blade sweeps from one yaw to the next, from the feet of the body
    /// that swings. The second yaw is the first one turned counterclockwise by more than zero and at most a half turn.
    /// </summary>
    /// <exception cref="ContextException">The wedge turns by zero, clockwise, or past a half turn (T-2).</exception>
    public static bool WedgeHits(WeaponDefinition weapon, Vector3 feet, int fromYaw, int toYaw, Aabb box)
    {
        // A wedge of no turn would accept a point on its line behind the body too, so the test takes a real turn alone.
        if (toYaw <= fromYaw || toYaw - fromYaw > HalfTurn)
        {
            ContextException error = new($"A wedge turns counterclockwise by more than zero and at most a half turn, and this one turns from yaw {fromYaw} to yaw {toYaw} (D-325).");
            error.AddContext("fromYaw", ((long)fromYaw).ToString(CultureInfo.InvariantCulture));
            error.AddContext("toYaw", ((long)toYaw).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        float low = feet.Y + (weapon.LowCentimetres / 100.0f);
        float high = feet.Y + (weapon.HighCentimetres / 100.0f);
        if (box.Max.Y <= low || box.Min.Y >= high)
        {
            return false;
        }

        if (feet.X >= box.Min.X && feet.X <= box.Max.X && feet.Z >= box.Min.Z && feet.Z <= box.Max.Z)
        {
            return true;
        }

        float reach = weapon.ReachCentimetres / 100.0f;
        float reachSquared = reach * reach;
        Vector3 first = BladeDirection(fromYaw);
        Vector3 second = BladeDirection(toYaw);
        float[] xs = [box.Min.X, box.Max.X];
        float[] zs = [box.Min.Z, box.Max.Z];

        foreach (float x in xs)
        {
            foreach (float z in zs)
            {
                float dx = x - feet.X;
                float dz = z - feet.Z;
                if ((dx * dx) + (dz * dz) <= reachSquared && BetweenBladeLines(first, second, dx, dz))
                {
                    return true;
                }
            }
        }

        // A blade line is a flat segment. The segment runs at the middle height of the box, so the slab test of the
        // projectiles reads the two horizontal axes alone.
        Vector3 start = new(feet.X, (box.Min.Y + box.Max.Y) * 0.5f, feet.Z);
        if (ProjectileSimulation.SegmentEntry(start, first * reach, box) >= 0.0f || ProjectileSimulation.SegmentEntry(start, second * reach, box) >= 0.0f)
        {
            return true;
        }

        // The rim crosses an edge of the box: a point of the circle of the reach on the line of an edge, inside the
        // edge and between the blade lines. A point on the circle needs no distance test.
        foreach (float x in xs)
        {
            float dx = x - feet.X;
            float rest = reachSquared - (dx * dx);
            if (rest < 0.0f)
            {
                continue;
            }

            float along = DetMath.Sqrt(rest);
            float[] dzs = [-along, along];
            foreach (float dz in dzs)
            {
                float z = feet.Z + dz;
                if (z >= box.Min.Z && z <= box.Max.Z && BetweenBladeLines(first, second, dx, dz))
                {
                    return true;
                }
            }
        }

        foreach (float z in zs)
        {
            float dz = z - feet.Z;
            float rest = reachSquared - (dz * dz);
            if (rest < 0.0f)
            {
                continue;
            }

            float along = DetMath.Sqrt(rest);
            float[] dxs = [-along, along];
            foreach (float dx in dxs)
            {
                float x = feet.X + dx;
                if (x >= box.Min.X && x <= box.Max.X && BetweenBladeLines(first, second, dx, dz))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Answers whether an offset from the feet lies between the first blade line and the second, in the order of the
    /// turn. The cross products read the horizontal plane alone, and a yaw that grows makes them negative (D-234).
    /// </summary>
    private static bool BetweenBladeLines(Vector3 first, Vector3 second, float dx, float dz)
    {
        return (first.X * dz) - (first.Z * dx) <= 0.0f && (dx * second.Z) - (dz * second.X) <= 0.0f;
    }
}
