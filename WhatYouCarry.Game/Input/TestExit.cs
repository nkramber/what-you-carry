using System.Globalization;
using Godot;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Input;

/// <summary>
/// The test exit (D-311): the Escape key and the Start button of the first controller end the session at once.
/// <c>Main</c> polls it once per tick, before the intent of that tick, in every session that runs the loop.
/// The escape menu of PR-53 replaces this exit and removes the quit on both inputs.
/// </summary>
/// <remarks>
/// <para>
/// The exit reads the poll and never an input event, so a test runs it with no engine, as the reader tests do
/// (D-111). A headless run has no key and no controller down, so the smoke session and the bot session of CI
/// never end here on their own.
/// </para>
/// <para>
/// The press flag scripts one press of either input at one tick. <c>Main</c> gives the event to the input
/// singleton of the engine at that tick, the engine holds the input down from the next frame, and the poll of
/// a later tick reads it as any real press. The two engine tests of the exit run a headless smoke session with
/// the flag and read the end line and the exit code (PR-60 exit tests 2 and 3).
/// </para>
/// </remarks>
public static class TestExit
{
    /// <summary>The key that ends the session (D-311).</summary>
    public const Key ExitKey = Key.Escape;

    /// <summary>The controller button that ends the session (D-311).</summary>
    public const JoyButton ExitButton = JoyButton.Start;

    /// <summary>The user argument that scripts one press. The next two arguments are the input name and the tick.</summary>
    public const string PressFlag = "--press";

    /// <summary>The start of every flag. A word after the tick that does not start with it belongs to no flag, and it is an error.</summary>
    public const string FlagStart = "--";

    /// <summary>The input name of the Escape key after the press flag.</summary>
    public const string EscapeName = "escape";

    /// <summary>The input name of the Start button after the press flag.</summary>
    public const string StartName = "start";

    private const string FlagField = "flag";
    private const string ArgumentField = "argument";
    private const string NoArguments = "The press flag needs an input name and a tick after it, and the arguments end there.";
    private const string UnknownInput = "The press flag names no input of the test exit. The names are escape and start.";
    private const string BadTick = "The press flag needs a whole tick of zero or more after the input name.";
    private const string TrailingWord = "The press flag takes an input name and a tick, and a word after the tick belongs to no flag.";

    /// <summary>Answers whether the exit key or the exit button of the first controller is down at this moment.</summary>
    public static bool IsPressed(IInputPoll poll)
    {
        return poll.IsKeyPressed(ExitKey) || poll.IsJoyButtonPressed(InputReader.FirstController, ExitButton);
    }

    /// <summary>Answers whether the user arguments script a press.</summary>
    public static bool IsPressRequested(string[] userArguments)
    {
        foreach (string argument in userArguments)
        {
            if (argument == PressFlag)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The scripted press: the input name and the tick after the flag. A word after the tick that starts no flag is
    /// an error, so a misspelled or extra argument never passes in silence (T-2). A flag after the tick belongs to
    /// its own parser.
    /// </summary>
    /// <exception cref="ContextException">The flag is absent, the two arguments do not follow it, the name is unknown, the tick is not a whole number, or a plain word follows the tick.</exception>
    public static ScriptedPress PressOf(string[] userArguments)
    {
        for (int index = 0; index < userArguments.Length; index++)
        {
            if (userArguments[index] != PressFlag)
            {
                continue;
            }

            if (index + 2 >= userArguments.Length)
            {
                throw new ContextException(NoArguments);
            }

            ScriptedPress press = new(InputOf(userArguments[index + 1]), TickOf(userArguments[index + 2]));
            if (index + 3 < userArguments.Length && !userArguments[index + 3].StartsWith(FlagStart, System.StringComparison.Ordinal))
            {
                ContextException error = new(TrailingWord);
                error.AddContext(ArgumentField, userArguments[index + 3]);
                throw error;
            }

            return press;
        }

        ContextException absent = new($"The arguments hold no {PressFlag} flag.");
        absent.AddContext(FlagField, PressFlag);
        throw absent;
    }

    /// <summary>The engine event of one scripted press: the exit key down, or the exit button of the first controller down.</summary>
    public static InputEvent EventOf(ScriptedPress press)
    {
        if (press.Input == ExitInput.Start)
        {
            return new InputEventJoypadButton
            {
                Device = InputReader.FirstController,
                ButtonIndex = ExitButton,
                Pressed = true,
            };
        }

        return new InputEventKey
        {
            Keycode = ExitKey,
            Pressed = true,
        };
    }

    /// <summary>The input of one name. Any other name is an error that carries the name (T-2).</summary>
    private static ExitInput InputOf(string name)
    {
        if (name == EscapeName)
        {
            return ExitInput.Escape;
        }

        if (name == StartName)
        {
            return ExitInput.Start;
        }

        ContextException error = new(UnknownInput);
        error.AddContext(ArgumentField, name);
        throw error;
    }

    /// <summary>The tick of one argument. A word that is not a whole number of zero or more is an error that carries the word (T-2).</summary>
    private static uint TickOf(string word)
    {
        if (uint.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out uint tick))
        {
            return tick;
        }

        ContextException error = new(BadTick);
        error.AddContext(ArgumentField, word);
        throw error;
    }
}

/// <summary>The two inputs of the test exit (D-311).</summary>
public enum ExitInput
{
    /// <summary>The Escape key.</summary>
    Escape,

    /// <summary>The Start button of the first controller.</summary>
    Start,
}

/// <summary>One scripted press of the test exit: which input, and the tick at which the engine takes the event.</summary>
public readonly record struct ScriptedPress(ExitInput Input, uint Tick);
