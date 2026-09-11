using Godot;

namespace WhatYouCarry.Game.Input;

/// <summary>
/// What the reader polls once per tick (D-15): the keys, the mouse buttons, and the controller buttons and
/// axes that are down at that moment. The engine poll and the test poll are the two callers that D-111 asks
/// for, so a reader test runs with no engine.
/// </summary>
public interface IInputPoll
{
    /// <summary>Answers whether one key is down.</summary>
    bool IsKeyPressed(Key key);

    /// <summary>Answers whether one mouse button is down.</summary>
    bool IsMouseButtonPressed(MouseButton button);

    /// <summary>Answers whether one button of one controller is down.</summary>
    bool IsJoyButtonPressed(int device, JoyButton button);

    /// <summary>The deflection of one axis of one controller, from minus one to one.</summary>
    float GetJoyAxis(int device, JoyAxis axis);
}
