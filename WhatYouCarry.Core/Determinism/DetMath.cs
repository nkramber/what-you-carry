using System;

namespace WhatYouCarry.Core.Determinism;

/// <summary>
/// The math that the simulation uses. Every function gives the same bits on every platform (D-69, G-2).
/// </summary>
/// <remarks>
/// <para>
/// A platform math library computes a sine to its own accuracy, so two platforms disagree in the last bits.
/// This class replaces those functions with add, subtract, multiply, divide, and the IEEE square root.
/// Each one of those five is exact under IEEE 754, and .NET never fuses a multiply and an add into one
/// rounded step, so every intermediate value is the same on Linux x64, Windows x64, and macOS arm64.
/// </para>
/// <para>
/// The polynomials are Remez minimax fits, and the comment above each set names its measured error.
/// <c>Sin</c>, <c>Cos</c>, and <c>Atan2</c> hold an absolute error of at most 1e-6 (D-161, revised by D-203).
/// The reduction folds an angle to [-pi/4, pi/4] and a quadrant, which D-203 selected after measurement:
/// degree 7 on [-pi, pi] reaches only 2.5e-4.
/// </para>
/// <para>
/// This is the one file in Core that may name <c>MathF</c>, and only for <c>Sqrt</c>, <c>Abs</c>, and
/// <c>Floor</c>. Those three are exact IEEE operations. The <c>det-lint</c> tool enforces both halves of
/// that rule (D-202).
/// </para>
/// </remarks>
public static class DetMath
{
    /// <summary>The nearest float to pi.</summary>
    public const float Pi = 3.14159274f;

    /// <summary>The nearest float to pi/2.</summary>
    public const float HalfPi = 1.57079637f;

    /// <summary>The nearest float to pi/4.</summary>
    public const float QuarterPi = 0.785398185f;

    /// <summary>
    /// The largest angle magnitude that <see cref="Sin"/> and <see cref="Cos"/> accept, in radians.
    /// Above it the range reduction loses accuracy, so a larger angle is an error and never a wrong number (T-2).
    /// The measured error is 8.5e-08 at this limit and 9.6e-07 at 65536, so the limit keeps a margin of 12
    /// against the 1e-6 target. 4096 radians is 652 full turns, and the simulation wraps every angle it stores.
    /// </summary>
    public const float MaxAngle = 4096.0f;

    // Two over pi, for the quadrant count.
    private const float TwoOverPi = 0.636619747f;

    // pi/2 in three parts. Each part is exact in float, and the sum is pi/2 to the last bit of a double.
    // The reduction subtracts the parts one at a time, so k * part stays exact and the remainder keeps
    // its accuracy for a large k. This is the Cody-Waite method.
    private const float HalfPiHigh = 1.5703125f;
    private const float HalfPiMid = 0.000483751297f;
    private const float HalfPiLow = 7.54979013e-08f;

    // The tangent of pi/8. Above it Atan folds once more, which keeps the polynomial argument small.
    private const float TanEighthPi = 0.414213568f;

    // sin(r) on [-pi/4, pi/4], odd terms r to r^7. Remez minimax error 1.2e-09.
    private const float SinC1 = 1.0f;
    private const float SinC3 = -0.166666374f;
    private const float SinC5 = 0.00833158474f;
    private const float SinC7 = -0.000194621171f;

    // cos(r) on [-pi/4, pi/4], even terms 1 to r^8. Remez minimax error 4.7e-11.
    private const float CosC0 = 1.0f;
    private const float CosC2 = -0.5f;
    private const float CosC4 = 0.0416666158f;
    private const float CosC6 = -0.00138866191f;
    private const float CosC8 = 2.43799295e-05f;

    // atan(x) on [-tan(pi/8), tan(pi/8)], odd terms x to x^7. Remez minimax error 1.1e-07.
    private const float AtanC1 = 0.999997616f;
    private const float AtanC3 = -0.333141685f;
    private const float AtanC5 = 0.195809737f;
    private const float AtanC7 = -0.107797116f;

    /// <summary>The sine of an angle in radians. The measured absolute error is 8.5e-08.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle is not finite, or its magnitude is above <see cref="MaxAngle"/>.</exception>
    public static float Sin(float radians)
    {
        (float remainder, int quadrant) = ReduceToQuadrant(radians, nameof(radians));
        switch (quadrant)
        {
            case 0: return SinPolynomial(remainder);
            case 1: return CosPolynomial(remainder);
            case 2: return -SinPolynomial(remainder);
            default: return -CosPolynomial(remainder);
        }
    }

    /// <summary>The cosine of an angle in radians. The measured absolute error is 8.4e-08.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle is not finite, or its magnitude is above <see cref="MaxAngle"/>.</exception>
    public static float Cos(float radians)
    {
        (float remainder, int quadrant) = ReduceToQuadrant(radians, nameof(radians));
        switch (quadrant)
        {
            case 0: return CosPolynomial(remainder);
            case 1: return -SinPolynomial(remainder);
            case 2: return -CosPolynomial(remainder);
            default: return SinPolynomial(remainder);
        }
    }

    /// <summary>
    /// The angle of the vector (x, y) in radians, in the range [-pi, pi]. The measured absolute error is 3.7e-07.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A component is not finite, or both components are zero.</exception>
    public static float Atan2(float y, float x)
    {
        if (!float.IsFinite(y) || !float.IsFinite(x))
        {
            throw new ArgumentOutOfRangeException(nameof(y), $"Atan2 needs two finite components. y is {y}, and x is {x}.");
        }

        // The zero vector has no angle. An absent value is an error, never a zero (T-2).
        if (y == 0.0f && x == 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Atan2 has no angle for the zero vector. y and x are both zero.");
        }

        float absoluteY = MathF.Abs(y);
        float absoluteX = MathF.Abs(x);

        // Divide the smaller component by the larger one, so the quotient stays in [0, 1].
        // Only one of the two components can be zero here, so the divisor is above zero in both branches.
        float angle = absoluteY <= absoluteX
            ? AtanUnitInterval(absoluteY / absoluteX)
            : HalfPi - AtanUnitInterval(absoluteX / absoluteY);

        // A negative zero x needs no fold. Atan2 gives pi/2 for a positive y on either sign of a zero x,
        // and `x < 0.0f` is already false for negative zero.
        if (x < 0.0f)
        {
            angle = Pi - angle;
        }

        // The sign of y comes from its sign bit and not from a comparison, because negative zero is not below
        // zero. Atan2(-0, -1) is -pi and not pi, and Atan2(-0, 1) is negative zero. The state hash reads the
        // raw bits, so the sign of a zero result is part of the contract too (D-160, F-62).
        return float.IsNegative(y) ? -angle : angle;
    }

    /// <summary>The square root. The IEEE square root is correctly rounded, so it is exact on every platform (D-161).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative or not finite.</exception>
    public static float Sqrt(float value)
    {
        if (!float.IsFinite(value) || value < 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Sqrt needs a finite value that is zero or above. The value is {value}.");
        }

        return MathF.Sqrt(value);
    }

    /// <summary>
    /// The value raised to an integer exponent, by squaring. Every step is one multiply, so the result is exact
    /// on every platform and needs no accuracy target (D-200). A fractional exponent has no caller yet.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is not finite, the exponent is <see cref="int.MinValue"/>, or the value is zero and the exponent is negative.</exception>
    public static float Pow(float value, int exponent)
    {
        if (!float.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Pow needs a finite value. The value is {value}.");
        }

        // int.MinValue has no positive counterpart, so the magnitude below would overflow in silence.
        if (exponent == int.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(exponent), "Pow cannot negate the exponent int.MinValue.");
        }

        if (value == 0.0f && exponent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Pow cannot raise zero to the negative exponent {exponent}.");
        }

        float result = 1.0f;
        float factor = value;
        int remaining = exponent < 0 ? -exponent : exponent;
        while (remaining > 0)
        {
            if ((remaining & 1) != 0)
            {
                result *= factor;
            }

            remaining >>= 1;

            // Square only for a step that follows. The last square would overflow for no reason.
            if (remaining > 0)
            {
                factor *= factor;
            }
        }

        return exponent < 0 ? 1.0f / result : result;
    }

    /// <summary>The magnitude of a value. The IEEE operation clears the sign bit, so it is exact.</summary>
    public static float Abs(float value)
    {
        return MathF.Abs(value);
    }

    /// <summary>The largest integer that is not above the value. The IEEE operation is exact.</summary>
    public static float Floor(float value)
    {
        return MathF.Floor(value);
    }

    /// <summary>The value held inside the range. Both ends are included.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The minimum is above the maximum.</exception>
    public static float Clamp(float value, float minimum, float maximum)
    {
        // An inverted range is a caller defect. It must not give a value that looks correct (T-2).
        if (minimum > maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), $"Clamp needs a minimum that is not above the maximum. The minimum is {minimum}, and the maximum is {maximum}.");
        }

        if (value < minimum)
        {
            return minimum;
        }

        return value > maximum ? maximum : value;
    }

    /// <summary>
    /// The value at <paramref name="amount"/> of the way from <paramref name="start"/> to <paramref name="end"/>.
    /// The form start + (end - start) * amount gives exactly <paramref name="start"/> at 0. It does not give
    /// exactly <paramref name="end"/> at 1, so a caller that needs the end point must test for it.
    /// </summary>
    public static float Lerp(float start, float end, float amount)
    {
        return start + ((end - start) * amount);
    }

    /// <summary>
    /// Folds an angle to a remainder in [-pi/4, pi/4] and the quadrant of the angle it came from.
    /// The angle is quadrant * (pi/2) + remainder.
    /// </summary>
    private static (float Remainder, int Quadrant) ReduceToQuadrant(float radians, string parameterName)
    {
        if (!float.IsFinite(radians))
        {
            throw new ArgumentOutOfRangeException(parameterName, $"The angle must be finite. The angle is {radians}.");
        }

        if (MathF.Abs(radians) > MaxAngle)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"The angle magnitude must be at most {MaxAngle} radians, because the range reduction loses accuracy above it. The angle is {radians}.");
        }

        // The count of quarter turns, rounded to the nearest.
        float turns = MathF.Floor((radians * TwoOverPi) + 0.5f);

        // Subtract the three parts one at a time. Each product is exact, so the remainder keeps its accuracy.
        float remainder = radians - (turns * HalfPiHigh);
        remainder -= turns * HalfPiMid;
        remainder -= turns * HalfPiLow;

        // Two's complement makes the mask correct for a negative count too: -1 gives 3, and -2 gives 2.
        return (remainder, (int)turns & 3);
    }

    /// <summary>The sine of a remainder in [-pi/4, pi/4], by Horner evaluation of the odd minimax polynomial.</summary>
    private static float SinPolynomial(float remainder)
    {
        float square = remainder * remainder;
        float sum = SinC7;
        sum = (sum * square) + SinC5;
        sum = (sum * square) + SinC3;
        sum = (sum * square) + SinC1;
        return sum * remainder;
    }

    /// <summary>The cosine of a remainder in [-pi/4, pi/4], by Horner evaluation of the even minimax polynomial.</summary>
    private static float CosPolynomial(float remainder)
    {
        float square = remainder * remainder;
        float sum = CosC8;
        sum = (sum * square) + CosC6;
        sum = (sum * square) + CosC4;
        sum = (sum * square) + CosC2;
        return (sum * square) + CosC0;
    }

    /// <summary>
    /// The arc tangent of a quotient in [0, 1]. Above tan(pi/8) the identity
    /// atan(x) = pi/4 + atan((x - 1) / (x + 1)) folds the argument back inside [-tan(pi/8), 0].
    /// </summary>
    private static float AtanUnitInterval(float quotient)
    {
        if (quotient > TanEighthPi)
        {
            return QuarterPi + AtanPolynomial((quotient - 1.0f) / (quotient + 1.0f));
        }

        return AtanPolynomial(quotient);
    }

    /// <summary>The arc tangent of a value in [-tan(pi/8), tan(pi/8)], by Horner evaluation of the odd minimax polynomial.</summary>
    private static float AtanPolynomial(float value)
    {
        float square = value * value;
        float sum = AtanC7;
        sum = (sum * square) + AtanC5;
        sum = (sum * square) + AtanC3;
        sum = (sum * square) + AtanC1;
        return sum * value;
    }
}
