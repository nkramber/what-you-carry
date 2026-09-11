using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Input;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The intent builder of the Game layer (D-77, D-162, D-243, D-289; PR-12 exit tests 1 and 2).</summary>
public sealed class IntentBuilderTests
{
    /// <summary>PR-12 exit test 1. A fractional look delta gives the whole hundredths, and the fraction waits for the next tick.</summary>
    [Fact]
    public void IntentBuilderQuantizes()
    {
        IntentBuilder builder = new();
        RawInput raw = new(MouseX: -1.25f, MouseY: 0.5f, StickLookX: 0.0f, StickLookY: 0.0f, Strafe: 0.0f, Forward: 0.0f, Buttons: 0, ControllerLook: false);

        Intent intent = builder.Build(7, raw);

        // A mouse move of 1.25 pixels to the left turns the look left by 12.5 hundredths, which is a positive yaw (D-234).
        Assert.Equal(7u, intent.Tick);
        Assert.Equal((short)12, intent.YawDelta);
        Assert.Equal(0.5f, builder.YawRemainder);

        // A mouse move of half a pixel down looks down by 5 hundredths, which is a negative pitch (D-248).
        Assert.Equal((short)-5, intent.PitchDelta);
        Assert.Equal(0.0f, builder.PitchRemainder);
    }

    /// <summary>A rate under one hundredth per tick reaches whole hundredths over enough ticks, and no motion is lost.</summary>
    [Fact]
    public void QuantizeCarriesTheFraction()
    {
        IntentBuilder builder = new();
        RawInput raw = new(-0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0, false);

        int sum = 0;
        for (uint tick = 0; tick < 10; tick++)
        {
            sum += builder.Build(tick, raw).YawDelta;
        }

        // Ten ticks of half a hundredth each are five hundredths.
        Assert.Equal(5, sum);
    }

    /// <summary>PR-12 exit test 2. Two builders in one state give equal frames for equal raw input, tick after tick.</summary>
    [Fact]
    public void IntentBuilderIsPure()
    {
        RawInput raw = new(3.7f, -2.2f, 0.6f, -0.4f, 0.5f, 1.0f, Button.Jump | Button.Sprint, false);
        IntentBuilder first = new();
        IntentBuilder second = new();

        for (uint tick = 0; tick < 5; tick++)
        {
            Assert.Equal(first.Build(tick, raw), second.Build(tick, raw));
            Assert.Equal(first.YawRemainder, second.YawRemainder);
            Assert.Equal(first.PitchRemainder, second.PitchRemainder);
        }
    }

    /// <summary>The dead zone gives zero, full deflection gives one, the middle of the live range gives one eighth, and the sign stays (D-289).</summary>
    [Theory]
    [InlineData(0.0f, 0.0f)]
    [InlineData(0.15f, 0.0f)]
    [InlineData(-0.15f, 0.0f)]
    [InlineData(0.1f, 0.0f)]
    [InlineData(1.0f, 1.0f)]
    [InlineData(-1.0f, -1.0f)]
    [InlineData(0.575f, 0.125f)]
    [InlineData(-0.575f, -0.125f)]
    [InlineData(1.5f, 1.0f)]
    public void StickCurveIsCubicPastTheDeadZone(float deflection, float expected)
    {
        Assert.Equal(expected, IntentBuilder.StickCurve(deflection), 5);
    }

    /// <summary>A stick at full deflection turns the look at the full rate, and a push to the right or down is a negative delta (D-234, D-248).</summary>
    [Fact]
    public void StickTurnsAtTheFullRate()
    {
        IntentBuilder builder = new();
        Intent right = builder.Build(0, new RawInput(0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0, true));
        Intent down = builder.Build(1, new RawInput(0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0, true));

        Assert.Equal((short)-IntentBuilder.StickHundredthsPerTick, right.YawDelta);
        Assert.Equal((short)0, right.PitchDelta);
        Assert.Equal((short)0, down.YawDelta);
        Assert.Equal((short)-IntentBuilder.StickHundredthsPerTick, down.PitchDelta);
    }

    /// <summary>The movement bytes clamp to the range of D-233, and a fraction scales by 127.</summary>
    [Theory]
    [InlineData(0.0f, 0)]
    [InlineData(1.0f, 127)]
    [InlineData(-1.0f, -127)]
    [InlineData(2.0f, 127)]
    [InlineData(-3.0f, -127)]
    [InlineData(0.5f, 63)]
    public void MoveByteScalesAndClamps(float fraction, int expected)
    {
        Assert.Equal((sbyte)expected, IntentBuilder.MoveByte(fraction));
    }

    /// <summary>The strafe goes to x and the forward to y (D-233), and the buttons pass through.</summary>
    [Fact]
    public void MovementAndButtonsReachTheFrame()
    {
        IntentBuilder builder = new();
        Intent intent = builder.Build(3, new RawInput(0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.5f, Button.Interact | Button.Dodge, false));

        Assert.Equal((sbyte)-127, intent.MoveX);
        Assert.Equal((sbyte)63, intent.MoveY);
        Assert.Equal((ushort)(Button.Interact | Button.Dodge), intent.Buttons);
    }

    /// <summary>The controller aim bit follows the device of the last look input (D-243).</summary>
    [Fact]
    public void ControllerLookSetsTheAimBit()
    {
        IntentBuilder builder = new();
        Intent controller = builder.Build(0, new RawInput(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, Button.Jump, true));
        Intent mouse = builder.Build(1, new RawInput(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, Button.Jump, false));

        Assert.Equal((ushort)(Button.Jump | Button.ControllerAim), controller.Buttons);
        Assert.Equal(Button.Jump, mouse.Buttons);
    }

    /// <summary>A turn past the range of the frame field saturates, and the carry starts from zero after it.</summary>
    [Fact]
    public void QuantizeSaturates()
    {
        float remainder = 0.7f;
        Assert.Equal(short.MaxValue, IntentBuilder.Quantize(100000.0f, ref remainder));
        Assert.Equal(0.0f, remainder);

        remainder = -0.7f;
        Assert.Equal(short.MinValue, IntentBuilder.Quantize(-100000.0f, ref remainder));
        Assert.Equal(0.0f, remainder);
    }

    /// <summary>A look value that is not a number is an error, and never a look that stands still (T-2).</summary>
    [Fact]
    public void QuantizeRejectsNotANumber()
    {
        IntentBuilder builder = new();
        ContextException error = Assert.Throws<ContextException>(() => builder.Build(0, new RawInput(float.NaN, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0, false)));
        Assert.Contains("not a number", error.Message, System.StringComparison.Ordinal);
    }
}
