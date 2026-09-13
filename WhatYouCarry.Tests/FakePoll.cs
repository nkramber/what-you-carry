using System.Collections.Generic;
using Godot;
using WhatYouCarry.Game.Input;

namespace WhatYouCarry.Tests;

/// <summary>
/// A poll that holds the keys, the buttons, and the axes that a test sets. Everything else is up, or at zero.
/// The reader tests and the test exit tests share it, so no test starts the engine (D-111).
/// </summary>
internal sealed class FakePoll : IInputPoll
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
