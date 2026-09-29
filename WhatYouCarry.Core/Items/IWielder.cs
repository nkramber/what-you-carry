using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Items;

/// <summary>
/// A body that an affix acts on (D-49): the wielder that lifesteal heals, and a foe that burning reaches. The
/// player and a humanoid enemy both carry it, so each behavior of <see cref="AffixBehaviors"/> works the same for
/// the two (D-111).
/// </summary>
public interface IWielder
{
    /// <summary>The feet center, in meters (D-235). Burning measures its radius from here (D-751).</summary>
    Vector3 Feet { get; }

    /// <summary>Answers whether the health is zero (D-322).</summary>
    bool IsDead { get; }

    /// <summary>Adds health, which stops at the health at the start (D-747).</summary>
    /// <exception cref="Logging.ContextException">The amount is below zero, or the body is dead.</exception>
    void Heal(long amount);

    /// <summary>
    /// Takes plain damage, which is not a hit: no stagger, and no affix reads it. A body in a roll takes none (D-328,
    /// D-751).
    /// </summary>
    /// <returns>True when the damage landed, and false when a roll took it.</returns>
    /// <exception cref="Logging.ContextException">The damage is below zero, or the body is dead.</exception>
    bool TakePlainDamage(long damage);
}
