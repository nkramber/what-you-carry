using System;
using Godot;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Input;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The test exit of PR-60 (D-311; PR-60 exit test 4) and its press flag. No test here starts the engine. The two
/// engine tests, exit tests 2 and 3, are in <see cref="SmokeSessionTests"/>.
/// </summary>
public sealed class TestExitTests
{
    /// <summary>The Escape key presses the exit, and nothing does before it.</summary>
    [Fact]
    public void EscapePressesTheExit()
    {
        Assert.Equal(Key.Escape, TestExit.ExitKey);

        FakePoll poll = new();
        Assert.False(TestExit.IsPressed(poll));
        poll.Keys.Add(TestExit.ExitKey);
        Assert.True(TestExit.IsPressed(poll));
    }

    /// <summary>The Start button of the first controller presses the exit, and nothing does before it.</summary>
    [Fact]
    public void StartButtonPressesTheExit()
    {
        Assert.Equal(JoyButton.Start, TestExit.ExitButton);

        FakePoll poll = new();
        Assert.False(TestExit.IsPressed(poll));
        poll.JoyButtons.Add((InputReader.FirstController, TestExit.ExitButton));
        Assert.True(TestExit.IsPressed(poll));
    }

    /// <summary>
    /// PR-60 exit test 4. No other key, no mouse button, no other button of the first controller, no axis, and no
    /// button of a second controller ends the session.
    /// </summary>
    [Fact]
    public void NoOtherInputEndsTheSession()
    {
        foreach (Key key in Enum.GetValues<Key>())
        {
            if (key == TestExit.ExitKey)
            {
                continue;
            }

            FakePoll poll = new();
            poll.Keys.Add(key);
            Assert.False(TestExit.IsPressed(poll), $"The key {key} ends the session.");
        }

        foreach (JoyButton button in Enum.GetValues<JoyButton>())
        {
            FakePoll second = new();
            second.JoyButtons.Add((InputReader.FirstController + 1, button));
            Assert.False(TestExit.IsPressed(second), $"The button {button} of a second controller ends the session.");

            if (button == TestExit.ExitButton)
            {
                continue;
            }

            FakePoll first = new();
            first.JoyButtons.Add((InputReader.FirstController, button));
            Assert.False(TestExit.IsPressed(first), $"The button {button} ends the session.");
        }

        foreach (MouseButton button in Enum.GetValues<MouseButton>())
        {
            FakePoll poll = new();
            poll.MouseButtons.Add(button);
            Assert.False(TestExit.IsPressed(poll), $"The mouse button {button} ends the session.");
        }

        foreach (JoyAxis axis in Enum.GetValues<JoyAxis>())
        {
            FakePoll poll = new();
            poll.Axes[(InputReader.FirstController, axis)] = 1.0f;
            Assert.False(TestExit.IsPressed(poll), $"The axis {axis} ends the session.");
        }
    }

    /// <summary>The press flag reads the input name and the tick after it, and nothing else asks for a press.</summary>
    [Fact]
    public void PressOfReadsTheInputAndTheTick()
    {
        Assert.True(TestExit.IsPressRequested([TestExit.PressFlag, TestExit.EscapeName, "0"]));
        Assert.True(TestExit.IsPressRequested(["--smoke", TestExit.PressFlag, TestExit.StartName, "7"]));
        Assert.False(TestExit.IsPressRequested([]));
        Assert.False(TestExit.IsPressRequested(["--smoke"]));

        Assert.Equal(new ScriptedPress(ExitInput.Escape, 0), TestExit.PressOf([TestExit.PressFlag, TestExit.EscapeName, "0"]));
        Assert.Equal(new ScriptedPress(ExitInput.Start, 250), TestExit.PressOf(["--smoke", TestExit.PressFlag, TestExit.StartName, "250"]));
    }

    /// <summary>An absent flag, a short argument list, an unknown name, and a bad tick are each an error that names the cause (T-2).</summary>
    [Fact]
    public void PressOfRejectsBadArguments()
    {
        ContextException absent = Assert.Throws<ContextException>(() => TestExit.PressOf(["--smoke"]));
        Assert.Contains(TestExit.PressFlag, absent.Message, StringComparison.Ordinal);

        Assert.Throws<ContextException>(() => TestExit.PressOf([TestExit.PressFlag]));
        Assert.Throws<ContextException>(() => TestExit.PressOf([TestExit.PressFlag, TestExit.EscapeName]));

        ContextException unknown = Assert.Throws<ContextException>(() => TestExit.PressOf([TestExit.PressFlag, "enter", "1"]));
        Assert.Contains("enter", unknown.Message, StringComparison.Ordinal);

        ContextException negative = Assert.Throws<ContextException>(() => TestExit.PressOf([TestExit.PressFlag, TestExit.StartName, "-1"]));
        Assert.Contains("-1", negative.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => TestExit.PressOf([TestExit.PressFlag, TestExit.StartName, "soon"]));
        Assert.Throws<ContextException>(() => TestExit.PressOf([TestExit.PressFlag, TestExit.StartName, "1.5"]));
    }
}
