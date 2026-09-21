namespace WhatYouCarry.Core.Simulation;

/// <summary>What happened to the timer of a floor on one tick (D-45, D-410, D-418). The run log of M-5 reads it.</summary>
public enum TimerEventKind
{
    /// <summary>The countdown reached zero (D-45).</summary>
    Expiry = 0,

    /// <summary>The Overseer spawned (D-415).</summary>
    HunterSpawn = 1,

    /// <summary>A wave came due. The count is the enemies that spawned (D-410, D-424).</summary>
    Wave = 2,

    /// <summary>Spawns of a wave found no post out of the sight of the player. The count is the skipped spawns (D-418).</summary>
    WaveSkip = 3,
}

/// <summary>One timer event: its kind, the floor, the tick of the intent, the wave number or zero, and a count or zero.</summary>
/// <param name="Kind">What happened.</param>
/// <param name="Floor">The floor number, from one (D-3).</param>
/// <param name="Tick">The tick of the intent that the event came on.</param>
/// <param name="Wave">The wave number, from one, for a wave or a skip. Zero otherwise.</param>
/// <param name="Count">The enemies that spawned for a wave, or the spawns that a skip dropped. Zero otherwise.</param>
public sealed record TimerEvent(TimerEventKind Kind, int Floor, uint Tick, int Wave, int Count);
