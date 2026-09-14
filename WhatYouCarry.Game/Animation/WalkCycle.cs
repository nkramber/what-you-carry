using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Entities;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Animation;

/// <summary>
/// The procedural walk of the player body (D-87, D-333): the upper legs swing 30 degrees each way over one full cycle
/// per 1.2 meters of distance traveled, and the arms swing 20 degrees against the legs. The lower legs and the lower
/// arms stay straight. The walk is a pose of the Game layer and never simulation state.
/// </summary>
/// <remarks>
/// The amount of the swing follows the horizontal speed up to the walk speed, so a body at rest stands straight, and a
/// body that walks or sprints takes the whole swing. A positive turn about X moves a hanging limb forward (D-234, D-298).
/// </remarks>
public static class WalkCycle
{
    /// <summary>The distance traveled over one full cycle of the legs, in meters (D-333).</summary>
    public const float StrideMeters = 1.2f;

    /// <summary>The swing of an upper leg each way, in degrees (D-333).</summary>
    public const float LegDegrees = 30.0f;

    /// <summary>The swing of an arm each way, in degrees (D-333).</summary>
    public const float ArmDegrees = 20.0f;

    /// <summary>The bone of the left upper leg.</summary>
    public const string LegLeft = "leg_left";

    /// <summary>The bone of the right upper leg.</summary>
    public const string LegRight = "leg_right";

    /// <summary>The bone of the left upper arm.</summary>
    public const string ArmLeft = "arm_left";

    /// <summary>The bone of the right upper arm.</summary>
    public const string ArmRight = "arm_right";

    /// <summary>The rotations of the four limbs, in degrees, at a distance traveled and an amount from zero to one.</summary>
    public static Dictionary<string, CoreVector3> Rotations(float walked, float amount)
    {
        float swing = MathF.Sin(walked / StrideMeters * 2.0f * MathF.PI) * amount;
        return new Dictionary<string, CoreVector3>
        {
            [LegLeft] = new CoreVector3(LegDegrees * swing, 0.0f, 0.0f),
            [LegRight] = new CoreVector3(-LegDegrees * swing, 0.0f, 0.0f),
            [ArmLeft] = new CoreVector3(-ArmDegrees * swing, 0.0f, 0.0f),
            [ArmRight] = new CoreVector3(ArmDegrees * swing, 0.0f, 0.0f),
        };
    }

    /// <summary>The amount of the swing at a horizontal speed in meters per second: the speed over the walk speed, up to one.</summary>
    public static float Amount(float horizontalSpeed)
    {
        float amount = horizontalSpeed / PlayerBody.WalkSpeed;
        return amount > 1.0f ? 1.0f : amount;
    }
}
