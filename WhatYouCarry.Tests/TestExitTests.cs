using System;
using Godot;
using WhatYouCarry.Game.Input;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The test exit of PR-60 (D-311; PR-60 exit tests 2 to 4). No test here starts the engine.</summary>
public sealed class TestExitTests
{
    /// <summary>PR-60 exit test 2. The Escape key ends the session.</summary>
    [Fact]
    public void EscapeEndsTheSession()
    {
        Assert.Equal(Key.Escape, TestExit.ExitKey);

        FakePoll poll = new();
        Assert.False(TestExit.IsPressed(poll));
        poll.Keys.Add(TestExit.ExitKey);
        Assert.True(TestExit.IsPressed(poll));
    }

    /// <summary>PR-60 exit test 3. The Start button of the first controller ends the session.</summary>
    [Fact]
    public void StartButtonEndsTheSession()
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
}
