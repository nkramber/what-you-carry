using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The countdown of one floor (D-44, D-46, D-407). It counts down one tick for each tick of the loop, pauses while
/// the player stands at the stairwell, and runs in a boss fight (D-140). After it reaches zero, it counts the ticks
/// after expiry, which the speed of the hunter and the wave clock read (D-408, D-424).
/// </summary>
/// <remarks>
/// <para>
/// The length comes from the floor template: the seconds of the band, and the extra seconds of the band on a boss
/// floor of D-6. A descent starts a new timer for the next floor, so the time of one floor never carries to the
/// next (D-44).
/// </para>
/// <para>
/// The stairwell pauses the countdown alone. The ticks after expiry count on at the stairwell, because the hunter
/// and the waves move and attack there (D-417).
/// </para>
/// </remarks>
public sealed class FloorTimer
{
    /// <summary>The floors that hold a boss (D-6). Each gets the extra seconds of its band (D-407).</summary>
    public static readonly int[] BossFloors = [5, 10, 15];

    /// <summary>A timer at its full length.</summary>
    /// <exception cref="ContextException">The length is below one tick.</exception>
    public FloorTimer(long lengthTicks)
    {
        if (lengthTicks < 1)
        {
            ContextException error = new($"A floor timer runs one tick or more, and the length is {lengthTicks} (D-44, D-407).");
            error.AddContext("lengthTicks", lengthTicks.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.Length = lengthTicks;
        this.Remaining = lengthTicks;
    }

    /// <summary>The full length, in ticks.</summary>
    public long Length { get; }

    /// <summary>The ticks of the countdown that remain. Zero means the timer expired.</summary>
    public long Remaining { get; private set; }

    /// <summary>The ticks that ran after the tick of expiry. Zero before expiry and on the tick of expiry.</summary>
    public long TicksAfterExpiry { get; private set; }

    /// <summary>Answers whether the countdown reached zero (D-45).</summary>
    public bool Expired => this.Remaining == 0;

    /// <summary>The timer of one floor of a content set: the seconds of its band, and the extra seconds on a boss floor (D-6, D-407).</summary>
    public static FloorTimer For(FloorTemplate template, int floor)
    {
        long seconds = template.TimerSeconds;
        if (IsBossFloor(floor))
        {
            seconds += template.BossTimerSeconds;
        }

        return new FloorTimer(seconds * SimulationLoop.TicksPerSecond);
    }

    /// <summary>Answers whether one floor holds a boss (D-6).</summary>
    public static bool IsBossFloor(int floor)
    {
        foreach (int boss in BossFloors)
        {
            if (boss == floor)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Runs one tick. Before expiry, the countdown drops by one unless the player stands at the stairwell (D-140).
    /// After expiry, the count of ticks after expiry rises by one on every tick (D-417).
    /// </summary>
    /// <returns>True on the one tick whose step takes the countdown to zero.</returns>
    public bool Step(bool atStairwell)
    {
        if (this.Expired)
        {
            this.TicksAfterExpiry++;
            return false;
        }

        if (atStairwell)
        {
            return false;
        }

        this.Remaining--;
        return this.Expired;
    }

    /// <summary>Folds the timer into the hash, in the declared order (D-160): the length, the countdown, and the ticks after expiry.</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Length);
        hash.Add(this.Remaining);
        hash.Add(this.TicksAfterExpiry);
    }
}
