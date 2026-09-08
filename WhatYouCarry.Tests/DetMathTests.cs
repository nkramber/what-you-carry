using System;
using WhatYouCarry.Core.Determinism;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// DetMath against a double reference (D-161, revised in part by D-203; PR-3 exit tests 3 and 4).
/// </summary>
/// <remarks>
/// The reference is <c>System.Math</c> in double. Core must never call it, and a test may, because the test
/// asks whether the float result is near the true value. The lint tool reads Core alone.
/// </remarks>
public sealed class DetMathTests
{
    /// <summary>The absolute error that D-161 allows.</summary>
    private const double Tolerance = 1e-6;

    /// <summary>PR-3 exit test 3. Sine and cosine hold the tolerance across [-4 pi, 4 pi].</summary>
    [Fact]
    public void DetMathAccuracy()
    {
        const int samples = 200001;
        double start = -4.0 * Math.PI;
        double span = 8.0 * Math.PI;

        double worstSin = 0.0;
        double worstCos = 0.0;
        double worstAngle = 0.0;
        for (int index = 0; index < samples; index++)
        {
            float angle = (float)(start + (span * index / (samples - 1)));
            double sinError = Math.Abs(DetMath.Sin(angle) - Math.Sin(angle));
            double cosError = Math.Abs(DetMath.Cos(angle) - Math.Cos(angle));
            if (sinError > worstSin || cosError > worstCos)
            {
                worstAngle = angle;
            }

            worstSin = Math.Max(worstSin, sinError);
            worstCos = Math.Max(worstCos, cosError);
        }

        Assert.True(worstSin <= Tolerance, $"Sin reached {worstSin:E3} near the angle {worstAngle}, and the limit is {Tolerance:E0}.");
        Assert.True(worstCos <= Tolerance, $"Cos reached {worstCos:E3} near the angle {worstAngle}, and the limit is {Tolerance:E0}.");
    }

    /// <summary>Atan2 holds the tolerance across a grid, and it gives the correct quadrant on each axis.</summary>
    [Fact]
    public void Atan2Accuracy()
    {
        const int side = 400;
        double span = 4.0 * Math.PI;

        double worst = 0.0;
        for (int row = 0; row <= side; row++)
        {
            for (int column = 0; column <= side; column++)
            {
                float y = (float)(-span + (2.0 * span * row / side));
                float x = (float)(-span + (2.0 * span * column / side));
                if (y == 0.0f && x == 0.0f)
                {
                    continue;
                }

                worst = Math.Max(worst, Math.Abs(DetMath.Atan2(y, x) - Math.Atan2(y, x)));
            }
        }

        Assert.True(worst <= Tolerance, $"Atan2 reached {worst:E3}, and the limit is {Tolerance:E0}.");

        Assert.Equal(0.0f, DetMath.Atan2(0.0f, 1.0f), 6);
        Assert.Equal(DetMath.HalfPi, DetMath.Atan2(1.0f, 0.0f), 6);
        Assert.Equal(DetMath.Pi, DetMath.Atan2(0.0f, -1.0f), 6);
        Assert.Equal(-DetMath.HalfPi, DetMath.Atan2(-1.0f, 0.0f), 6);
        Assert.Equal(DetMath.QuarterPi, DetMath.Atan2(1.0f, 1.0f), 6);
        Assert.Equal(-3.0f * DetMath.QuarterPi, DetMath.Atan2(-1.0f, -1.0f), 6);
    }

    /// <summary>
    /// PR-3 exit test 4. An angle plus many full turns gives the same result as the base angle. The reduction,
    /// and not the polynomial, carries this.
    /// </summary>
    [Fact]
    public void DetMathRangeReduction()
    {
        float[] baseAngles = [0.0f, 0.3f, 1.0f, -1.0f, DetMath.QuarterPi, DetMath.HalfPi, 2.5f, -3.0f, DetMath.Pi];
        int[] turnCounts = [1, 2, 7, 20, 100, 350, 600, -1, -50, -400];
        float fullTurn = 2.0f * DetMath.Pi;

        foreach (float baseAngle in baseAngles)
        {
            foreach (int turns in turnCounts)
            {
                float turned = baseAngle + (turns * fullTurn);
                if (DetMath.Abs(turned) > DetMath.MaxAngle)
                {
                    continue;
                }

                // The comparison is against the true sine of the turned angle, not against the base angle. The
                // float value of `turned` is not the exact sum, so the two true sines differ a little too.
                Assert.True(
                    Math.Abs(DetMath.Sin(turned) - Math.Sin(turned)) <= Tolerance,
                    $"Sin missed at the base angle {baseAngle} plus {turns} turns.");
                Assert.True(
                    Math.Abs(DetMath.Cos(turned) - Math.Cos(turned)) <= Tolerance,
                    $"Cos missed at the base angle {baseAngle} plus {turns} turns.");
            }
        }
    }

    /// <summary>An angle above the limit is an error, and it names the limit and the angle (T-2).</summary>
    [Fact]
    public void AnAngleAboveTheLimitIsAnError()
    {
        float above = DetMath.MaxAngle * 1.5f;
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Sin(above));
        Assert.Contains("4096", error.Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Cos(-above));

        // The limit itself is inside the domain.
        Assert.True(DetMath.Abs(DetMath.Sin(DetMath.MaxAngle)) <= 1.0f);
        Assert.True(DetMath.Abs(DetMath.Sin(-DetMath.MaxAngle)) <= 1.0f);
    }

    /// <summary>An angle that is not finite is an error and never a silent NaN (T-2).</summary>
    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void AnAngleThatIsNotFiniteIsAnError(float angle)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Sin(angle));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Cos(angle));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Atan2(angle, 1.0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Atan2(1.0f, angle));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Sqrt(angle));
    }

    /// <summary>The zero vector has no angle, so Atan2 reports it (T-2).</summary>
    [Fact]
    public void TheZeroVectorHasNoAngle()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Atan2(0.0f, 0.0f));
        Assert.Contains("zero vector", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Sqrt is the IEEE square root, so it is exact on a perfect square and correct elsewhere.</summary>
    [Fact]
    public void SqrtIsExact()
    {
        Assert.Equal(0.0f, DetMath.Sqrt(0.0f));
        Assert.Equal(2.0f, DetMath.Sqrt(4.0f));
        Assert.Equal(11.0f, DetMath.Sqrt(121.0f));
        for (int index = 0; index <= 2000; index++)
        {
            float value = index * 0.25f;
            Assert.True(Math.Abs(DetMath.Sqrt(value) - Math.Sqrt(value)) <= Tolerance);
        }
    }

    /// <summary>A negative value has no real square root, so Sqrt reports it (T-2).</summary>
    [Fact]
    public void ANegativeSquareRootIsAnError()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Sqrt(-1.0f));
        Assert.Contains("-1", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Pow with an integer exponent is exact for every value that a float holds (D-200).</summary>
    [Fact]
    public void PowIsExactForAnIntegerExponent()
    {
        Assert.Equal(1.0f, DetMath.Pow(7.5f, 0));
        Assert.Equal(7.5f, DetMath.Pow(7.5f, 1));
        Assert.Equal(56.25f, DetMath.Pow(7.5f, 2));
        Assert.Equal(1024.0f, DetMath.Pow(2.0f, 10));
        Assert.Equal(0.25f, DetMath.Pow(2.0f, -2));
        Assert.Equal(-8.0f, DetMath.Pow(-2.0f, 3));
        Assert.Equal(16.0f, DetMath.Pow(-2.0f, 4));
        Assert.Equal(0.0f, DetMath.Pow(0.0f, 3));

        // A power of two is exact in float, so the squaring order cannot change the answer.
        for (int exponent = -20; exponent <= 20; exponent++)
        {
            Assert.Equal((float)Math.Pow(2.0, exponent), DetMath.Pow(2.0f, exponent));
        }
    }

    /// <summary>Pow reports the three inputs that have no answer (T-2).</summary>
    [Fact]
    public void PowReportsAnInputWithNoAnswer()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Pow(float.NaN, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Pow(2.0f, int.MinValue));

        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Pow(0.0f, -3));
        Assert.Contains("-3", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Abs, Floor, Clamp, and Lerp give the plain answers.</summary>
    [Fact]
    public void TheSimpleFunctionsGiveThePlainAnswers()
    {
        Assert.Equal(2.5f, DetMath.Abs(-2.5f));
        Assert.Equal(2.5f, DetMath.Abs(2.5f));

        Assert.Equal(-3.0f, DetMath.Floor(-2.5f));
        Assert.Equal(2.0f, DetMath.Floor(2.5f));
        Assert.Equal(2.0f, DetMath.Floor(2.0f));

        Assert.Equal(0.0f, DetMath.Clamp(-1.0f, 0.0f, 10.0f));
        Assert.Equal(10.0f, DetMath.Clamp(11.0f, 0.0f, 10.0f));
        Assert.Equal(5.0f, DetMath.Clamp(5.0f, 0.0f, 10.0f));
        Assert.Equal(4.0f, DetMath.Clamp(9.0f, 4.0f, 4.0f));

        // Lerp gives the start exactly at zero. It is not required to give the end exactly at one.
        Assert.Equal(-2.0f, DetMath.Lerp(-2.0f, 6.0f, 0.0f));
        Assert.Equal(2.0f, DetMath.Lerp(-2.0f, 6.0f, 0.5f));
        Assert.Equal(10.0f, DetMath.Lerp(-2.0f, 6.0f, 1.5f));
    }

    /// <summary>An inverted clamp range is a caller defect, and it must not give a value that looks correct (T-2).</summary>
    [Fact]
    public void AnInvertedClampRangeIsAnError()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => DetMath.Clamp(5.0f, 10.0f, 0.0f));
        Assert.Contains("10", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The three named constants are the nearest float to their true values.</summary>
    [Fact]
    public void TheConstantsAreTheNearestFloats()
    {
        Assert.Equal((float)Math.PI, DetMath.Pi);
        Assert.Equal((float)(Math.PI / 2.0), DetMath.HalfPi);
        Assert.Equal((float)(Math.PI / 4.0), DetMath.QuarterPi);
    }
}
