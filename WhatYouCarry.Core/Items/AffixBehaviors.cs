using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Items;

/// <summary>
/// The fixed set of affix behaviors (D-49, D-747). Each behavior takes a wielder, an event, and the parameters of
/// the affixes that the wielder carries, and it works the same for the player and for an enemy.
/// </summary>
/// <remarks>
/// <para>
/// Two events exist. A hit that lands runs lifesteal and then burning (<see cref="OnHit"/>). The equip of a weapon
/// runs swift (<see cref="Swift"/>), which gives the weapon a shorter melee windup.
/// </para>
/// <para>
/// The carried list holds the affixes of every item of the wielder, so one affix can stand in it twice, once for
/// each item. The amounts of one behavior add (D-750). Burning damage is plain damage and not a hit, so no affix
/// reads it and nothing chains (D-751).
/// </para>
/// <para>
/// The loop calls none of this yet. PR-22 hooks the affixes of the items that the player wears into the loop, and
/// PR-26 the affixes of an enemy loadout.
/// </para>
/// </remarks>
public static class AffixBehaviors
{
    /// <summary>The largest sum of the swift percent, so the windup never drops below half of the windup of the weapon (D-750).</summary>
    public const long SwiftCapPercent = 50;

    /// <summary>
    /// Runs the affixes of a hit that landed (D-747, D-751). Lifesteal heals the wielder by the percent sum of the
    /// full damage of the hit, rounded down. Burning then deals its damage to each live foe within its radius of the
    /// feet center of the struck target, the target included.
    /// </summary>
    /// <param name="wielder">The body whose hit landed.</param>
    /// <param name="carried">The affixes of every item of the wielder.</param>
    /// <param name="hitDamage">The full damage of the hit, also when the target had less health (D-751).</param>
    /// <param name="struckFeet">The feet center of the struck target, in meters.</param>
    /// <param name="foes">The foes of the wielder, the struck target included. An ally of the wielder is not in the list (D-747).</param>
    /// <exception cref="ContextException">The damage is below zero.</exception>
    public static void OnHit(IWielder wielder, IReadOnlyList<AffixDefinition> carried, long hitDamage, Vector3 struckFeet, IReadOnlyList<IWielder> foes)
    {
        if (hitDamage < 0)
        {
            ContextException error = new($"A hit deals a damage of zero or more, and the damage is {hitDamage}.");
            error.AddContext("damage", hitDamage.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        long heal = hitDamage * Sum(carried, AffixDefinition.Lifesteal, AffixDefinition.PercentName) / 100;
        if (heal > 0)
        {
            wielder.Heal(heal);
        }

        foreach (IWielder foe in foes)
        {
            // The hit can kill the struck target before its burn, and a dead body takes no damage (D-322).
            if (foe.IsDead)
            {
                continue;
            }

            long burn = BurnAt(carried, struckFeet, foe.Feet);
            if (burn > 0)
            {
                foe.TakePlainDamage(burn);
            }
        }
    }

    /// <summary>
    /// The weapon with the windup that swift gives it (D-747, D-750): the windup minus the windup times the percent sum
    /// over 100, rounded down. The percent sum stops at <see cref="SwiftCapPercent"/>. A wielder with no swift gets
    /// the weapon with its own windup.
    /// </summary>
    public static WeaponDefinition Swift(WeaponDefinition weapon, IReadOnlyList<AffixDefinition> carried)
    {
        long percent = Sum(carried, AffixDefinition.Swift, AffixDefinition.PercentName);
        if (percent > SwiftCapPercent)
        {
            percent = SwiftCapPercent;
        }

        long shortening = weapon.WindupTicks * percent / 100;
        return weapon with { WindupTicks = weapon.WindupTicks - shortening };
    }

    /// <summary>The sum of one parameter over every carried affix of one behavior.</summary>
    private static long Sum(IReadOnlyList<AffixDefinition> carried, string behavior, string parameter)
    {
        long sum = 0;
        foreach (AffixDefinition affix in carried)
        {
            if (affix.Behavior == behavior)
            {
                sum += affix.Parameter(parameter);
            }
        }

        return sum;
    }

    /// <summary>
    /// The burning damage at one foe: the damage of each carried burning affix whose radius reaches from the feet
    /// center of the struck target to the feet center of the foe, in a straight line (D-751).
    /// </summary>
    private static long BurnAt(IReadOnlyList<AffixDefinition> carried, Vector3 struckFeet, Vector3 foeFeet)
    {
        float dx = foeFeet.X - struckFeet.X;
        float dy = foeFeet.Y - struckFeet.Y;
        float dz = foeFeet.Z - struckFeet.Z;
        float distanceSquared = (dx * dx) + (dy * dy) + (dz * dz);

        long burn = 0;
        foreach (AffixDefinition affix in carried)
        {
            if (affix.Behavior != AffixDefinition.Burning)
            {
                continue;
            }

            float radius = affix.Parameter(AffixDefinition.RadiusName) / 100.0f;
            if (distanceSquared <= radius * radius)
            {
                burn += affix.Parameter(AffixDefinition.DamageName);
            }
        }

        return burn;
    }
}
