using Godot;

namespace WhatYouCarry.Game.Input;

/// <summary>The poll of the engine. Each method reads the input singleton of the engine at the moment of the call.</summary>
public sealed class EnginePoll : IInputPoll
{
    /// <inheritdoc/>
    public bool IsKeyPressed(Key key)
    {
        return Godot.Input.IsKeyPressed(key);
    }

    /// <inheritdoc/>
    public bool IsMouseButtonPressed(MouseButton button)
    {
        return Godot.Input.IsMouseButtonPressed(button);
    }

    /// <inheritdoc/>
    public bool IsJoyButtonPressed(int device, JoyButton button)
    {
        return Godot.Input.IsJoyButtonPressed(device, button);
    }

    /// <inheritdoc/>
    public float GetJoyAxis(int device, JoyAxis axis)
    {
        return Godot.Input.GetJoyAxis(device, axis);
    }
}
