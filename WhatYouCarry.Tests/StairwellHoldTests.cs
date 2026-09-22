using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Input;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The tap and the hold of interact at the stairwell prompt (D-448).</summary>
public sealed class StairwellHoldTests
{
    /// <summary>A tap at the open prompt sets the interact bit on the tick of the release alone, and never while it is down.</summary>
    [Fact]
    public void TapDescendsOnRelease()
    {
        StairwellHold hold = new();
        for (int tick = 0; tick < 10; tick++)
        {
            Assert.Equal((ushort)0, hold.Apply(Button.Interact, true));
        }

        Assert.Equal(Button.Interact, hold.Apply(0, true));
        Assert.Equal((ushort)0, hold.Apply(0, true));
    }

    /// <summary>A hold of one second sets the ascend bit on the tick it reaches one second, and the release after it sets nothing.</summary>
    [Fact]
    public void HoldAscendsAtOneSecond()
    {
        StairwellHold hold = new();
        for (int tick = 1; tick < StairwellHold.HoldTicks; tick++)
        {
            Assert.Equal((ushort)0, hold.Apply(Button.Interact, true));
            Assert.Equal(tick, hold.HeldTicks);
        }

        Assert.Equal(Button.Ascend, hold.Apply(Button.Interact, true));
        Assert.Equal((ushort)0, hold.Apply(Button.Interact, true));
        Assert.Equal((ushort)0, hold.Apply(0, true));
    }

    /// <summary>A press that starts before the prompt opens neither descends nor ascends, and the next press counts again.</summary>
    [Fact]
    public void PressBeforeThePromptDoesNotCount()
    {
        StairwellHold hold = new();
        Assert.Equal(Button.Interact, hold.Apply(Button.Interact, false));
        for (int tick = 0; tick < 2 * StairwellHold.HoldTicks; tick++)
        {
            Assert.Equal((ushort)0, hold.Apply(Button.Interact, true));
        }

        Assert.Equal((ushort)0, hold.Apply(0, true));
        Assert.Equal((ushort)0, hold.Apply(Button.Interact, true));
        Assert.Equal(Button.Interact, hold.Apply(0, true));
    }

    /// <summary>The closed prompt passes every button as it is, and a prompt that closes during a hold disarms it.</summary>
    [Fact]
    public void ClosedPromptPassesAndDisarms()
    {
        StairwellHold hold = new();
        ushort buttons = (ushort)(Button.Interact | Button.Sprint | Button.Attack);
        Assert.Equal(buttons, hold.Apply(buttons, false));

        StairwellHold interrupted = new();
        Assert.Equal((ushort)0, interrupted.Apply(0, true));
        Assert.Equal(Button.Sprint, interrupted.Apply((ushort)(Button.Interact | Button.Sprint), true));
        Assert.Equal(Button.Interact, interrupted.Apply(Button.Interact, false));
        Assert.Equal((ushort)0, interrupted.Apply(0, true));
        Assert.Equal(0, interrupted.HeldTicks);
    }
}
