using System.Collections.Generic;
using Godot;
using WhatYouCarry.Game.Input;
using Xunit;
using CoreButton = WhatYouCarry.Core.Simulation.Button;

namespace WhatYouCarry.Tests;

/// <summary>
/// The input reader of the Game layer over a test poll (D-15, D-243, D-289; PR #49 review P2-1). No test here
/// starts the engine.
/// </summary>
public sealed class InputReaderTests
{
    /// <summary>PR #49 review P2-1. A held stick sends no new event, so a later mouse motion takes the look, and the frame names the mouse.</summary>
    [Fact]
    public void HeldStickThenMouseMotionReadsTheMouse()
    {
        FakePoll poll = new();
        poll.Axes[(InputReader.FirstController, InputReader.LookAxisX)] = 0.8f;
        InputReader reader = new(poll);

        reader.AddLookStickMotion(InputReader.FirstController, InputReader.LookAxisX, 0.8f);
        Assert.True(reader.Read().ControllerLook);

        // The stick stays where it is, so the poll still reads 0.8, and the mouse moves before the next tick.
        reader.AddMouseMotion(3.0f, 0.0f);
        RawInput raw = reader.Read();
        Assert.False(raw.ControllerLook);
        Assert.Equal(0.8f, raw.StickLookX);
        Assert.Equal(3.0f, raw.MouseX);

        // The held stick still sends no event, so the mouse keeps the look on the next tick too.
        Assert.False(reader.Read().ControllerLook);
    }

    /// <summary>A stick event past the dead zone on a look axis takes the look back from the mouse (D-243).</summary>
    [Fact]
    public void StickEventPastTheDeadZoneTakesTheLook()
    {
        InputReader reader = new(new FakePoll());
        reader.AddMouseMotion(1.0f, 1.0f);
        Assert.False(reader.Read().ControllerLook);

        reader.AddLookStickMotion(InputReader.FirstController, InputReader.LookAxisY, -0.5f);
        Assert.True(reader.Read().ControllerLook);

        // The look holds with the controller until the mouse moves again.
        Assert.True(reader.Read().ControllerLook);
        reader.AddMouseMotion(0.0f, -2.0f);
        Assert.False(reader.Read().ControllerLook);
    }

    /// <summary>A stick event inside the dead zone, on a move axis, or from another controller changes nothing.</summary>
    [Fact]
    public void OtherStickEventsChangeNothing()
    {
        InputReader reader = new(new FakePoll());
        reader.AddLookStickMotion(InputReader.FirstController, InputReader.LookAxisX, 0.1f);
        reader.AddLookStickMotion(InputReader.FirstController, InputReader.MoveAxisX, 1.0f);
        reader.AddLookStickMotion(InputReader.FirstController + 1, InputReader.LookAxisX, 1.0f);
        Assert.False(reader.Read().ControllerLook);
    }

    /// <summary>Every keyboard and mouse binding of D-289 with a bit reaches the frame.</summary>
    [Fact]
    public void KeyboardAndMouseBindingsReachTheFrame()
    {
        FakePoll poll = new();
        poll.Keys.Add(InputReader.ForwardKey);
        poll.Keys.Add(InputReader.RightKey);
        poll.Keys.Add(InputReader.JumpKey);
        poll.Keys.Add(InputReader.SprintKey);
        poll.Keys.Add(InputReader.DodgeKey);
        poll.Keys.Add(InputReader.InteractKey);
        poll.MouseButtons.Add(InputReader.AttackButton);

        RawInput raw = new InputReader(poll).Read();

        Assert.Equal(1.0f, raw.Forward);
        Assert.Equal(1.0f, raw.Strafe);
        Assert.Equal(CoreButton.Jump | CoreButton.Sprint | CoreButton.Dodge | CoreButton.Attack | CoreButton.Interact, raw.Buttons);
        Assert.False(raw.ControllerLook);
    }

    /// <summary>Two opposed keys cancel, and the back and left keys give the negative directions (D-233).</summary>
    [Fact]
    public void OpposedKeysCancelAndBackIsNegative()
    {
        FakePoll both = new();
        both.Keys.Add(InputReader.ForwardKey);
        both.Keys.Add(InputReader.BackKey);
        both.Keys.Add(InputReader.LeftKey);
        both.Keys.Add(InputReader.RightKey);
        RawInput cancelled = new InputReader(both).Read();
        Assert.Equal(0.0f, cancelled.Forward);
        Assert.Equal(0.0f, cancelled.Strafe);

        FakePoll back = new();
        back.Keys.Add(InputReader.BackKey);
        back.Keys.Add(InputReader.LeftKey);
        RawInput negative = new InputReader(back).Read();
        Assert.Equal(-1.0f, negative.Forward);
        Assert.Equal(-1.0f, negative.Strafe);
    }

    /// <summary>Every controller binding of D-289 with a bit reaches the frame, and the stick down is forward negative.</summary>
    [Fact]
    public void ControllerBindingsReachTheFrame()
    {
        FakePoll poll = new();
        poll.JoyButtons.Add((InputReader.FirstController, InputReader.JumpButton));
        poll.JoyButtons.Add((InputReader.FirstController, InputReader.DodgeButton));
        poll.JoyButtons.Add((InputReader.FirstController, InputReader.SprintButton));
        poll.JoyButtons.Add((InputReader.FirstController, InputReader.InteractButton));
        poll.Axes[(InputReader.FirstController, InputReader.AttackAxis)] = 0.6f;
        poll.Axes[(InputReader.FirstController, InputReader.MoveAxisX)] = 0.5f;
        poll.Axes[(InputReader.FirstController, InputReader.MoveAxisY)] = -0.5f;
        poll.Axes[(InputReader.FirstController, InputReader.LookAxisX)] = 0.3f;
        poll.Axes[(InputReader.FirstController, InputReader.LookAxisY)] = -0.2f;

        RawInput raw = new InputReader(poll).Read();

        Assert.Equal(CoreButton.Jump | CoreButton.Sprint | CoreButton.Dodge | CoreButton.Attack | CoreButton.Interact, raw.Buttons);
        Assert.Equal(0.5f, raw.Strafe);
        Assert.Equal(0.5f, raw.Forward);
        Assert.Equal(0.3f, raw.StickLookX);
        Assert.Equal(-0.2f, raw.StickLookY);
    }

    /// <summary>A trigger under the press point is not an attack.</summary>
    [Fact]
    public void TriggerUnderThePressPointDoesNotAttack()
    {
        FakePoll poll = new();
        poll.Axes[(InputReader.FirstController, InputReader.AttackAxis)] = 0.4f;
        Assert.Equal(0, new InputReader(poll).Read().Buttons);
    }

    /// <summary>The mouse motion sums between two ticks, and the sum starts again after each read.</summary>
    [Fact]
    public void MouseMotionSumsBetweenTicksAndThenStartsAgain()
    {
        InputReader reader = new(new FakePoll());
        reader.AddMouseMotion(1.0f, 2.0f);
        reader.AddMouseMotion(3.0f, 4.0f);

        RawInput first = reader.Read();
        Assert.Equal(4.0f, first.MouseX);
        Assert.Equal(6.0f, first.MouseY);

        RawInput second = reader.Read();
        Assert.Equal(0.0f, second.MouseX);
        Assert.Equal(0.0f, second.MouseY);
    }

    /// <summary>A poll that holds the keys, the buttons, and the axes that a test sets. Everything else is up, or at zero.</summary>
    private sealed class FakePoll : IInputPoll
    {
        public HashSet<Key> Keys { get; } = [];

        public HashSet<MouseButton> MouseButtons { get; } = [];

        public HashSet<(int Device, JoyButton Button)> JoyButtons { get; } = [];

        public Dictionary<(int Device, JoyAxis Axis), float> Axes { get; } = [];

        public bool IsKeyPressed(Key key)
        {
            return this.Keys.Contains(key);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return this.MouseButtons.Contains(button);
        }

        public bool IsJoyButtonPressed(int device, JoyButton button)
        {
            return this.JoyButtons.Contains((device, button));
        }

        public float GetJoyAxis(int device, JoyAxis axis)
        {
            return this.Axes.TryGetValue((device, axis), out float value) ? value : 0.0f;
        }
    }
}
