using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Input;

/// <summary>
/// Builds the intent of one tick from the raw input of that tick (D-77, D-162, D-289). The sensitivity and the
/// curves apply here, before the quantization, so Core reads whole hundredths of a degree and never a float.
/// </summary>
/// <remarks>
/// <para>
/// The mouse is linear: each pixel of motion turns the look by <see cref="MouseHundredthsPerPixel"/>. A stick
/// takes the cubic curve of <see cref="StickCurve"/> and turns the look by up to
/// <see cref="StickHundredthsPerTick"/> on one tick at full deflection. The two sum, so a player who uses both
/// on one tick gets both.
/// </para>
/// <para>
/// The quantization keeps the fraction that a tick does not use and adds it to the next tick, so a slow look
/// never rounds to a standstill. That remainder is the whole state of the builder. Two builders with equal
/// remainders give equal frames for equal raw input, and no clock or engine read enters here.
/// </para>
/// <para>
/// The two sensitivity numbers are the recommendation of OQ-157, and the owner sets them by a decision.
/// </para>
/// </remarks>
public sealed class IntentBuilder
{
    /// <summary>The hundredths of a degree that one pixel of mouse motion turns the look, linear (D-289, OQ-157): a tenth of a degree.</summary>
    public const float MouseHundredthsPerPixel = 10.0f;

    /// <summary>The hundredths of a degree that a stick at full deflection turns the look on one tick (OQ-157): 180 degrees per second.</summary>
    public const float StickHundredthsPerTick = 300.0f;

    /// <summary>The dead zone of a stick, as a fraction of the full deflection (D-289).</summary>
    public const float StickDeadZone = 0.15f;

    /// <summary>The largest magnitude of a movement byte (D-233).</summary>
    public const int MoveScale = 127;

    private const string NotANumber = "A look value is not a number, and the builder cannot quantize it.";
    private const string ValueField = "value";
    private const string RemainderField = "remainder";

    private float yawRemainder;
    private float pitchRemainder;

    /// <summary>The fraction of a hundredth of yaw that the last tick did not use.</summary>
    public float YawRemainder => this.yawRemainder;

    /// <summary>The fraction of a hundredth of pitch that the last tick did not use.</summary>
    public float PitchRemainder => this.pitchRemainder;

    /// <summary>
    /// The intent of one tick. A mouse move to the right and a stick push to the right both turn the look
    /// right, which is a negative yaw (D-234). A mouse move down and a stick push down both look down, which
    /// is a negative pitch (D-248).
    /// </summary>
    /// <exception cref="ContextException">A look value is not a number.</exception>
    public Intent Build(uint tick, RawInput raw)
    {
        float yaw = -((raw.MouseX * MouseHundredthsPerPixel) + (StickCurve(raw.StickLookX) * StickHundredthsPerTick));
        float pitch = -((raw.MouseY * MouseHundredthsPerPixel) + (StickCurve(raw.StickLookY) * StickHundredthsPerTick));
        short yawDelta = Quantize(yaw, ref this.yawRemainder);
        short pitchDelta = Quantize(pitch, ref this.pitchRemainder);

        ushort buttons = raw.Buttons;
        if (raw.ControllerLook)
        {
            buttons |= Button.ControllerAim;
        }

        return new Intent(tick, yawDelta, pitchDelta, MoveByte(raw.Strafe), MoveByte(raw.Forward), buttons);
    }

    /// <summary>
    /// The cubic stick curve with the dead zone (D-289). A deflection inside the dead zone gives zero. Past it,
    /// the deflection rescales so the edge of the dead zone is zero and full deflection is one, and the cube of
    /// that keeps a small push slow and a full push fast. The sign of the deflection stays.
    /// </summary>
    public static float StickCurve(float deflection)
    {
        float magnitude = deflection < 0.0f ? -deflection : deflection;
        if (magnitude <= StickDeadZone)
        {
            return 0.0f;
        }

        float scaled = (magnitude - StickDeadZone) / (1.0f - StickDeadZone);
        if (scaled > 1.0f)
        {
            scaled = 1.0f;
        }

        float curved = scaled * scaled * scaled;
        return deflection < 0.0f ? -curved : curved;
    }

    /// <summary>
    /// The whole hundredths of a look value, with the fraction carried in <paramref name="remainder"/> to the
    /// next tick. A value past the range of the frame field saturates, and the carry then starts from zero,
    /// because a turn of more than 327 degrees in one tick has no fraction worth keeping.
    /// </summary>
    /// <exception cref="ContextException">The value is not a number.</exception>
    public static short Quantize(float hundredths, ref float remainder)
    {
        // A cast of a value that is not a number gives zero in silence, and a look that stands still hides the
        // defect (T-2).
        if (float.IsNaN(hundredths))
        {
            ContextException error = new(NotANumber);
            error.AddContext(ValueField, hundredths.ToString(System.Globalization.CultureInfo.InvariantCulture));
            error.AddContext(RemainderField, remainder.ToString(System.Globalization.CultureInfo.InvariantCulture));
            throw error;
        }

        float total = hundredths + remainder;
        if (total >= short.MaxValue)
        {
            remainder = 0.0f;
            return short.MaxValue;
        }

        if (total <= short.MinValue)
        {
            remainder = 0.0f;
            return short.MinValue;
        }

        short whole = (short)total;
        remainder = total - whole;
        return whole;
    }

    /// <summary>The movement byte of a fraction from minus one to one (D-233). A fraction past that range clamps.</summary>
    public static sbyte MoveByte(float fraction)
    {
        if (fraction >= 1.0f)
        {
            return MoveScale;
        }

        if (fraction <= -1.0f)
        {
            return -MoveScale;
        }

        return (sbyte)(fraction * MoveScale);
    }
}
