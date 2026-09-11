using System;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Smoke;

/// <summary>
/// The headless smoke session (D-114, D-149): a run from the first seed, a fixed intent script of one thousand
/// ticks, and quit. <c>Main</c> starts it on the flag, feeds the script to the loop, and quits with exit code 0
/// only when the log holds no error line. PR-18 extends the script to the stairwell.
/// </summary>
/// <remarks>
/// The script has four parts of 250 ticks: a walk forward, a walk with a turn to the left, a sprint with a
/// held jump, and a strafe to the right with a look up. Each part meets whatever the floor of the seed puts in
/// its way, and a wall stops the body with no error (D-235).
/// </remarks>
public static class SmokeSession
{
    /// <summary>The user argument that starts the session.</summary>
    public const string Flag = "--smoke";

    /// <summary>The count of ticks of the script (D-149).</summary>
    public const uint Ticks = 1000;

    /// <summary>The count of ticks of each of the four parts.</summary>
    public const uint PartTicks = 250;

    /// <summary>The yaw rate of the turn, in hundredths of a degree per tick: one degree.</summary>
    public const short TurnRate = 100;

    /// <summary>The pitch rate of the look up, in hundredths of a degree per tick.</summary>
    public const short LookUpRate = 20;

    /// <summary>A full movement byte (D-233).</summary>
    public const sbyte FullMove = 127;

    private const string PastScript = "The tick is past the end of the smoke script.";

    /// <summary>Answers whether the user arguments of the process ask for the session.</summary>
    public static bool IsRequested(string[] userArguments)
    {
        foreach (string argument in userArguments)
        {
            if (argument == Flag)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The intent of one tick of the script.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tick is past the script.</exception>
    public static Intent IntentAt(uint tick)
    {
        if (tick >= Ticks)
        {
            throw new ArgumentOutOfRangeException(nameof(tick), tick, PastScript);
        }

        uint part = tick / PartTicks;
        switch (part)
        {
            case 0: return new Intent(tick, 0, 0, 0, FullMove, 0);
            case 1: return new Intent(tick, TurnRate, 0, 0, FullMove, 0);
            case 2: return new Intent(tick, 0, 0, 0, FullMove, Button.Sprint | Button.Jump);
            default: return new Intent(tick, 0, LookUpRate, FullMove, 0, 0);
        }
    }
}
