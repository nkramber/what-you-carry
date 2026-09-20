using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The intent that a bot policy builds from a look and a direction in the world frame (D-227, D-233, D-234). Two
/// policies walk a path and turn to face a target (D-111): the greedy descender and the full clearer.
/// </summary>
/// <remarks>
/// The loop turns the look first and then moves the body with the yaw after the turn, so a policy that reads the
/// yaw before its own turn sends the body the wrong way. Every call here takes the yaw after the turn, and the
/// movement bytes come from that frame.
/// </remarks>
public static class BotIntent
{
    // Hundredths of a degree to radians, as the body reads them.
    private const float RadiansPerHundredth = DetMath.Pi / 18000.0f;

    /// <summary>The yaw delta of one tick that turns a look toward a yaw, in hundredths of a degree, by the shorter way.</summary>
    public static short TurnToward(int from, int to)
    {
        int delta = to - from;
        if (delta > SimulationLoop.FullTurn / 2)
        {
            delta -= SimulationLoop.FullTurn;
        }
        else if (delta < -SimulationLoop.FullTurn / 2)
        {
            delta += SimulationLoop.FullTurn;
        }

        return (short)delta;
    }

    /// <summary>
    /// One intent that moves the body along a direction in the world frame. The direction is a share of the walk
    /// speed, from zero to one in length, as <see cref="Pathfinding.PathWalk.Toward"/> gives it.
    /// </summary>
    /// <param name="tick">The tick of the intent.</param>
    /// <param name="direction">The direction and the share of full speed, in the world frame. Its Y part is not read.</param>
    /// <param name="yawDelta">The turn of this tick, in hundredths of a degree.</param>
    /// <param name="yawAfter">The yaw sum after that turn, which the body moves in.</param>
    /// <param name="buttons">The buttons of the tick.</param>
    public static Intent Walk(uint tick, Vector3 direction, short yawDelta, int yawAfter, ushort buttons)
    {
        // Forward at yaw zero is minus Z and right is plus X, and the yaw turns counterclockwise seen from above
        // (D-234). That turn is its own inverse, so the same pair of terms gives the strafe and the forward share
        // of a world direction.
        float radians = yawAfter * RadiansPerHundredth;
        float sin = DetMath.Sin(radians);
        float cos = DetMath.Cos(radians);
        float strafe = (direction.X * cos) - (direction.Z * sin);
        float forward = (-direction.X * sin) - (direction.Z * cos);
        return new Intent(tick, yawDelta, 0, Byte(strafe), Byte(forward), buttons);
    }

    /// <summary>
    /// The intent that rolls through one enemy: the look turns onto it, and the movement input points forward, so
    /// the roll of D-327 carries the body at it and past it.
    /// </summary>
    /// <remarks>
    /// A roll away from a blade loses a duel. The roll covers 3 meters in 18 ticks, and the cooldown of 45 ticks
    /// then holds while the enemy closes again at 5 meters per second, so the body never comes inside the reach of
    /// its own weapon. A roll through the enemy ends beside it or behind it, in reach, with the blade ready
    /// (D-327, D-328, D-402).
    /// </remarks>
    public static Intent Roll(uint tick, int yaw, int yawAfter)
    {
        return new Intent(tick, TurnToward(yaw, yawAfter), 0, 0, PlayerBody.MoveScale, Button.Dodge);
    }

    /// <summary>One share of full speed, from -1 to 1, as a movement byte (D-233).</summary>
    public static sbyte Byte(float share)
    {
        float clamped = DetMath.Clamp(share, -1.0f, 1.0f);
        return (sbyte)DetMath.Floor((clamped * PlayerBody.MoveScale) + 0.5f);
    }
}
