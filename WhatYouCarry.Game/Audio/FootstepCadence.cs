using System;
using WhatYouCarry.Core.Entities;

namespace WhatYouCarry.Game.Audio;

/// <summary>
/// The rhythm of the footsteps of the player (D-463): 2.5 steps each second at the walk speed, and 3 each second at the
/// sprint speed, in a straight line between the two. The stride is the distance of one step at a speed, and
/// <see cref="StrideClock"/> counts the distance.
/// </summary>
/// <remarks>
/// Below the walk speed the stride stays the stride of the walk, and above the sprint speed it stays the stride of the
/// sprint, so a slow walk in water and a run down a slope each keep a rhythm that follows the speed.
/// </remarks>
public static class FootstepCadence
{
    /// <summary>The steps each second at the walk speed of <see cref="PlayerBody.WalkSpeed"/> (D-463).</summary>
    public const float WalkStepsPerSecond = 2.5f;

    /// <summary>The steps each second at the sprint speed of <see cref="PlayerBody.SprintSpeed"/> (D-463).</summary>
    public const float SprintStepsPerSecond = 3.0f;

    /// <summary>The stride at the walk speed, in meters: 1.6 at 4 meters each second.</summary>
    public const float WalkStrideMeters = PlayerBody.WalkSpeed / WalkStepsPerSecond;

    /// <summary>The stride at the sprint speed, in meters: about 2.33 at 7 meters each second.</summary>
    public const float SprintStrideMeters = PlayerBody.SprintSpeed / SprintStepsPerSecond;

    /// <summary>The stride at one horizontal speed, in meters.</summary>
    /// <param name="speed">The horizontal speed of the body, in meters each second.</param>
    public static float StrideMeters(float speed)
    {
        if (speed <= PlayerBody.WalkSpeed)
        {
            return WalkStrideMeters;
        }

        if (speed >= PlayerBody.SprintSpeed)
        {
            return SprintStrideMeters;
        }

        float part = (speed - PlayerBody.WalkSpeed) / (PlayerBody.SprintSpeed - PlayerBody.WalkSpeed);
        float rate = WalkStepsPerSecond + (part * (SprintStepsPerSecond - WalkStepsPerSecond));
        return speed / rate;
    }
}
