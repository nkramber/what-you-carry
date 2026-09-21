using System;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Smoke;

/// <summary>
/// The headless smoke session (D-114, D-149, D-436): a run from the first seed, a walk to the stairwell and one
/// descent, then a fixed intent script of one thousand ticks on floor 2, and quit. <c>Main</c> starts it on the flag, feeds the
/// script to the loop, and quits with exit code 0 only when the log holds no error line.
/// </summary>
/// <remarks>
/// <para>
/// The script has four parts of 250 ticks: a walk forward, a walk with a turn to the left, a sprint with a
/// held jump, and a strafe to the right with a look up. Each part meets whatever the floor of the seed puts in
/// its way, and a wall stops the body with no error (D-235).
/// </para>
/// <para>
/// The two walks press the attack bit once each second, and the strafe presses the dodge bit once each second, so the
/// session swings the sword, rolls, and plays their clips on every platform (D-149, D-323, D-331). A press sets the bit
/// for one tick, and the cooldown of a roll ends before the next press (D-327).
/// </para>
/// <para>
/// First the greedy descender walks the body to the stairwell and descends (PR-18, D-436). The walk opens the
/// stairwell prompt and runs the worker and the chunk swap. The script then plays on floor 2 from the tick of the
/// descent. The script alone dies to the scavengers of floor 1 on the first seed, and the descender clears that
/// floor. A walk that does not descend inside its budget is an error line (T-2), and a death on floor 2 ends the
/// session clean (D-403).
/// </para>
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

    /// <summary>The ticks from one press to the next: one second.</summary>
    public const uint PressPeriod = 60;

    /// <summary>The tick inside a press period of a press of the dodge bit.</summary>
    public const uint DodgeOffset = 30;

    /// <summary>The ticks that the walk has to descend: five minutes, the floor budget of D-271.</summary>
    public const uint DescentBudget = 18000;

    private const string PastScript = "The tick is past the end of the smoke script.";

    /// <summary>Answers whether the user arguments of the process ask for the session.</summary>
    public static bool IsRequested(UserArguments userArguments)
    {
        return userArguments.Has(Flag);
    }

    /// <summary>Answers whether the session reached its end: one descent, and the whole script on the floor after it.</summary>
    /// <param name="ticksOnFloor">The ticks since the loop came to its floor.</param>
    public static bool IsComplete(SimulationLoop loop, uint ticksOnFloor)
    {
        return loop.Floor > SimulationLoop.FirstFloor && ticksOnFloor >= Ticks;
    }

    /// <summary>Answers whether the walk passed its budget on floor 1.</summary>
    public static bool IsStuck(SimulationLoop loop)
    {
        return loop.Floor == SimulationLoop.FirstFloor && loop.Tick >= DescentBudget;
    }

    /// <summary>The intent of one tick of the loop on floor 2: the script tick that counts from the descent, for the tick of the loop.</summary>
    /// <param name="loopTick">The tick of the loop, which the intent carries (G-5).</param>
    /// <param name="scriptStart">The tick of the loop at the descent.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is before the descent or past the script.</exception>
    public static Intent ScriptIntent(uint loopTick, uint scriptStart)
    {
        if (loopTick < scriptStart)
        {
            throw new ArgumentOutOfRangeException(nameof(loopTick), loopTick, PastScript);
        }

        return IntentAt(loopTick - scriptStart) with { Tick = loopTick };
    }

    /// <summary>The intent of one tick of the script.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tick is past the script.</exception>
    public static Intent IntentAt(uint tick)
    {
        if (tick >= Ticks)
        {
            throw new ArgumentOutOfRangeException(nameof(tick), tick, PastScript);
        }

        ushort attack = tick % PressPeriod == 0 ? Button.Attack : (ushort)0;
        ushort dodge = tick % PressPeriod == DodgeOffset ? Button.Dodge : (ushort)0;
        uint part = tick / PartTicks;
        switch (part)
        {
            case 0: return new Intent(tick, 0, 0, 0, FullMove, attack);
            case 1: return new Intent(tick, TurnRate, 0, 0, FullMove, attack);
            case 2: return new Intent(tick, 0, 0, 0, FullMove, Button.Sprint | Button.Jump);
            default: return new Intent(tick, 0, LookUpRate, FullMove, 0, dodge);
        }
    }
}
