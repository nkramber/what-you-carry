using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Projectiles;

/// <summary>
/// One projectile in flight (G-6): which definition it follows, who fired it, where it is, how fast it moves,
/// and how many ticks it has flown. Every field is state, and the loop hash reads all of them (D-160).
/// </summary>
/// <param name="Definition">The index of the definition in the projectile list of the content set.</param>
/// <param name="Owner">The owner id. A projectile never hits the entity box of its owner. The player is owner 0, and a test source is minus one.</param>
/// <param name="Position">The point of the projectile, in meters.</param>
/// <param name="Velocity">The velocity, in meters per second.</param>
/// <param name="Age">The count of ticks flown.</param>
public readonly record struct Projectile(int Definition, int Owner, Vector3 Position, Vector3 Velocity, int Age);

/// <summary>An entity box that a projectile can hit, with the owner id that a projectile of that owner passes through.</summary>
public readonly record struct EntityBox(int Owner, Aabb Box);

/// <summary>How a projectile ended.</summary>
public enum ProjectileEndKind
{
    /// <summary>The lifetime of its definition ran out.</summary>
    Lifetime = 0,

    /// <summary>It met a solid cell of the grid.</summary>
    Grid = 1,

    /// <summary>It met an entity box.</summary>
    Entity = 2,
}

/// <summary>The end of one projectile on one tick: the projectile as it was, how it ended, the point where it ended, and the index of the entity box it hit, or minus one.</summary>
public readonly record struct ProjectileEnd(Projectile Projectile, ProjectileEndKind Kind, Vector3 Point, int EntityIndex);
