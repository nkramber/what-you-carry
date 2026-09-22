using System;
using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Audio;

/// <summary>
/// The pitch of the step of the Overseer (D-455): its speed over its speed at expiry, which stops at 2. The step is one
/// octave higher at the sprint speed, 70 seconds after expiry (D-408), and it stays there.
/// </summary>
public static class HunterPitch
{
    /// <summary>The highest pitch ratio: one octave (D-455).</summary>
    public const float MaxRatio = 2.0f;

    private const string BadStart = "The hunter speed at expiry is above zero.";
    private const string StartField = "startSpeed";

    /// <summary>The pitch ratio at one speed.</summary>
    /// <param name="speed">The speed of the Overseer, in meters per second.</param>
    /// <param name="startSpeed">The speed of the Overseer at expiry, in meters per second: 3.5 in the content of D-408.</param>
    /// <exception cref="ContextException">The speed at expiry is not above zero (T-2).</exception>
    public static float Ratio(float speed, float startSpeed)
    {
        if (!(startSpeed > 0.0f))
        {
            ContextException error = new(BadStart);
            error.AddContext(StartField, startSpeed.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return MathF.Min(speed / startSpeed, MaxRatio);
    }
}
