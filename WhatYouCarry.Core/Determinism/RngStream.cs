namespace WhatYouCarry.Core.Determinism;

/// <summary>
/// The subsystem that owns a random number stream (D-159). One run seed gives one stream for each value here,
/// and the streams do not interfere. Each value names one Core component of the system map in `docs/design.md`
/// section 3.1.
/// </summary>
/// <remarks>
/// A number here is part of the seed, so it must never change. A new subsystem adds a new value at the end,
/// which leaves every earlier stream at the same numbers.
/// </remarks>
public enum RngStream
{
    /// <summary>Procgen: the voxel grid, the room graph, and the stairwell.</summary>
    Procgen = 0,

    /// <summary>Items, affixes, and loot: the drops and the enemy loadouts.</summary>
    Loot = 1,

    /// <summary>Enemy AI and pathfinding: the choices that the seed drives.</summary>
    Enemy = 2,

    /// <summary>Projectiles: the spread of a shot.</summary>
    Projectile = 3,

    /// <summary>Bots: the choices of a bot policy, outside the simulation (D-272).</summary>
    Bot = 4,
}
