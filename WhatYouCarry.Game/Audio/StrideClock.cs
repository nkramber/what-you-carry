using System;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Audio;

/// <summary>
/// The rhythm of the steps of one body (D-454): it adds the horizontal distance of each tick on the ground, and a
/// step sounds each time the sum passes one stride. Each call gives the stride, so the player can take the stride of its
/// speed (D-463) and the Overseer a stride of one length. The rhythm is a sound of the Game layer, and never simulation
/// state.
/// </summary>
/// <remarks>
/// Two bodies step: the player and the Overseer. A faster body passes more strides each second, so the rhythm follows
/// the speed (D-455, D-463). A reset forgets the last position, so a descent or a new hunter makes no step.
/// </remarks>
public sealed class StrideClock
{
    private const string BadStride = "A stride is longer than zero meters.";
    private const string StrideField = "strideMeters";

    private bool hasLast;
    private CoreVector3 last;
    private float travelled;

    /// <summary>Adds the move of one tick.</summary>
    /// <param name="feet">The feet center of the body after the tick.</param>
    /// <param name="grounded">Whether the body stands on the ground. A move in the air or in a roll adds nothing.</param>
    /// <param name="strideMeters">The horizontal distance of one step, in meters.</param>
    /// <returns>True when the move passes the end of a stride.</returns>
    /// <exception cref="ContextException">The stride is not longer than zero.</exception>
    public bool Advance(CoreVector3 feet, bool grounded, float strideMeters)
    {
        if (!(strideMeters > 0.0f))
        {
            ContextException error = new(BadStride);
            error.AddContext(StrideField, strideMeters.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        if (!this.hasLast)
        {
            this.last = feet;
            this.hasLast = true;
            return false;
        }

        float stepX = feet.X - this.last.X;
        float stepZ = feet.Z - this.last.Z;
        this.last = feet;
        if (!grounded)
        {
            return false;
        }

        this.travelled += MathF.Sqrt((stepX * stepX) + (stepZ * stepZ));
        if (this.travelled < strideMeters)
        {
            return false;
        }

        this.travelled %= strideMeters;
        return true;
    }

    /// <summary>Forgets the last position and the distance, so the next move makes no step.</summary>
    public void Reset()
    {
        this.hasLast = false;
        this.travelled = 0.0f;
    }
}
