using System.Globalization;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The fixed-step loop (D-73). One call to <see cref="Step"/> is one tick, and the loop reads no clock: the Game
/// layer counts real time and calls <see cref="Step"/> once for each tick that the count covers.
/// </summary>
/// <remarks>
/// <para>
/// The Phase 1 state is the seed, the tick, the yaw and pitch sums in hundredths of a degree, and the buttons of
/// the last intent (D-227). The yaw wraps at a full turn, and the pitch stops at straight up and straight down.
/// PR-7 adds the world, and PR-8 derives the aim ray from the two sums (D-77). The movement bytes of an intent
/// wait for the world, because a position needs collision, so this loop reads neither one yet.
/// </para>
/// <para>
/// Every field is an integer, so the hash is the same on every platform by construction. The loop rejects an
/// intent whose tick is not the next one, because a replay that stepped over a frame would diverge in silence
/// (T-2, G-5).
/// </para>
/// </remarks>
public sealed class SimulationLoop
{
    /// <summary>The count of ticks in one second (D-73).</summary>
    public const int TicksPerSecond = 60;

    /// <summary>One full turn of yaw, in hundredths of a degree.</summary>
    public const int FullTurn = 36000;

    /// <summary>The largest pitch magnitude, in hundredths of a degree: straight up or straight down.</summary>
    public const int PitchLimit = 9000;

    /// <summary>A loop at tick zero for one run.</summary>
    public SimulationLoop(ulong seed)
    {
        this.Seed = seed;
    }

    /// <summary>The seed of the run. Every random stream of the run derives from it (D-159).</summary>
    public ulong Seed { get; }

    /// <summary>The count of ticks that ran, which is also the tick of the next intent.</summary>
    public uint Tick { get; private set; }

    /// <summary>The yaw sum, in hundredths of a degree, from 0 up to but not including <see cref="FullTurn"/>.</summary>
    public int Yaw { get; private set; }

    /// <summary>The pitch sum, in hundredths of a degree, from minus <see cref="PitchLimit"/> to <see cref="PitchLimit"/>.</summary>
    public int Pitch { get; private set; }

    /// <summary>The buttons of the last intent, as the bit mask of D-162.</summary>
    public ushort Buttons { get; private set; }

    /// <summary>Runs one tick with one intent.</summary>
    /// <exception cref="ContextException">The intent is not for the next tick, or the tick counter is full.</exception>
    public void Step(Intent intent)
    {
        if (intent.Tick != this.Tick)
        {
            ContextException outOfOrder = new($"The loop is at tick {this.Tick}, and the intent is for tick {intent.Tick}. Every tick takes one intent, in order (G-5).");
            outOfOrder.AddContext("expectedTick", ((long)this.Tick).ToString(CultureInfo.InvariantCulture));
            outOfOrder.AddContext("intentTick", ((long)intent.Tick).ToString(CultureInfo.InvariantCulture));
            throw outOfOrder;
        }

        // The counter would wrap to zero in silence, and a tick that repeats an old number breaks the record order.
        if (this.Tick == uint.MaxValue)
        {
            throw new ContextException($"The tick counter is full at {this.Tick}, and the loop cannot run another tick.");
        }

        int yaw = (this.Yaw + intent.YawDelta) % FullTurn;
        if (yaw < 0)
        {
            yaw += FullTurn;
        }

        int pitch = this.Pitch + intent.PitchDelta;
        if (pitch > PitchLimit)
        {
            pitch = PitchLimit;
        }
        else if (pitch < -PitchLimit)
        {
            pitch = -PitchLimit;
        }

        this.Yaw = yaw;
        this.Pitch = pitch;
        this.Buttons = intent.Buttons;
        this.Tick++;
    }

    /// <summary>The hash of the whole state, in the declared field order (D-160).</summary>
    public StateHash Hash()
    {
        StateHash hash = StateHash.Start();
        hash.Add(this.Seed);
        hash.Add(this.Tick);
        hash.Add(this.Yaw);
        hash.Add(this.Pitch);
        hash.Add((uint)this.Buttons);
        return hash;
    }
}
