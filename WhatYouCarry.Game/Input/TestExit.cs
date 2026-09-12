using Godot;

namespace WhatYouCarry.Game.Input;

/// <summary>
/// The test exit (D-311): the Escape key and the Start button of the first controller end the session at once.
/// <c>Main</c> polls it once per tick, before the intent of that tick, in every session that runs the loop.
/// The escape menu of PR-53 replaces this exit and removes the quit on both inputs.
/// </summary>
/// <remarks>
/// The exit reads the poll and never an input event, so a test runs it with no engine, as the reader tests do
/// (D-111). A headless run has no key and no controller down, so the smoke session and the bot session of CI
/// never end here.
/// </remarks>
public static class TestExit
{
    /// <summary>The key that ends the session (D-311).</summary>
    public const Key ExitKey = Key.Escape;

    /// <summary>The controller button that ends the session (D-311).</summary>
    public const JoyButton ExitButton = JoyButton.Start;

    /// <summary>Answers whether the exit key or the exit button of the first controller is down at this moment.</summary>
    public static bool IsPressed(IInputPoll poll)
    {
        return poll.IsKeyPressed(ExitKey) || poll.IsJoyButtonPressed(InputReader.FirstController, ExitButton);
    }
}
