using System;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// The exponential and the natural logarithm of a double, from the five exact IEEE operations and the exact power-of-two
/// scale (D-453). The runtime versions differ by platform, and a committed sound must equal the render on each one.
/// </summary>
/// <remarks>
/// <see cref="Exp"/> splits the argument into a whole count of the logarithm of 2 and a remainder of at most half of it,
/// sums the Taylor series of the remainder to 20 terms, and scales the sum by the power of two. <see cref="Log"/> splits
/// the argument into a mantissa from the square root of one half to the square root of 2 and a power of two, and sums
/// the series of the inverse hyperbolic tangent of the mantissa. Each result is within a few units in the last place.
/// </remarks>
public static class DetDouble
{
    /// <summary>The natural logarithm of 2.</summary>
    public const double Ln2 = 0.69314718055994530942;

    /// <summary>The natural logarithm of one thousandth: a fall of 60 decibels.</summary>
    public const double LnThousandth = -6.9077552789821370521;

    private const int ExpTerms = 20;
    private const int LogTerms = 30;
    private const double SqrtHalf = 0.70710678118654752440;

    /// <summary>The exponential of a finite value. A value below -700 gives zero, and a value above 700 is an error.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is not finite, or it is above 700.</exception>
    public static double Exp(double value)
    {
        if (!double.IsFinite(value) || value > 700.0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The exponential takes a finite value up to 700.");
        }

        if (value < -700.0)
        {
            return 0.0;
        }

        double whole = Math.Round(value / Ln2, MidpointRounding.ToEven);
        double remainder = value - (whole * Ln2);
        double term = 1.0;
        double sum = 1.0;
        for (int index = 1; index <= ExpTerms; index++)
        {
            term = term * remainder / index;
            sum += term;
        }

        return Math.ScaleB(sum, (int)whole);
    }

    /// <summary>The natural logarithm of a value above zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is not finite, or it is not above zero.</exception>
    public static double Log(double value)
    {
        if (!double.IsFinite(value) || value <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The logarithm takes a finite value above zero.");
        }

        int exponent = Math.ILogB(value);
        // The mantissa of ILogB lies from 1 up to 2. Above the square root of 2 it halves, so the series converges fast.
        double mantissa = Math.ScaleB(value, -exponent);
        if (mantissa > 1.0 / SqrtHalf)
        {
            mantissa *= 0.5;
            exponent++;
        }

        double ratio = (mantissa - 1.0) / (mantissa + 1.0);
        double square = ratio * ratio;
        double power = ratio;
        double sum = 0.0;
        for (int index = 0; index < LogTerms; index++)
        {
            sum += power / ((2 * index) + 1);
            power *= square;
        }

        return (2.0 * sum) + (exponent * Ln2);
    }
}
