using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The reflexes that every bot policy shares (D-111): the read of a blade that is about to go live, which a roll
/// answers. The greedy descender and the full clearer both read it.
/// </summary>
/// <remarks>
/// No hit lands during a roll, and a roll runs 18 ticks on a cooldown of 45 (D-327, D-328). A policy that rolls
/// <see cref="DodgeLead"/> ticks before the windup of a swing ends covers the active ticks of that swing. A policy
/// that reads no swing takes every hit, and the measurement of 2026-09-20 over 200 seeds gives 4 runs to the
/// bottom without the roll and 62 with it.
/// </remarks>
public static class BotReflex
{
    /// <summary>How near an enemy must swing for a policy to roll away from it, in meters.</summary>
    public const float DodgeRange = 3.0f;

    /// <summary>How many ticks before the blade of an enemy goes live a policy starts its roll (D-327, D-328).</summary>
    public const int DodgeLead = 8;

    /// <summary>
    /// The living enemy near the body whose blade goes live inside <see cref="DodgeLead"/> ticks, or null when
    /// none does. A roll that starts now then covers the active ticks of that swing.
    /// </summary>
    /// <remarks>
    /// A roll needs the ground, the cooldown ready, no roll running, and feet out of still water (D-327, D-337).
    /// A tick that fails one of those gives null, so the policy never presses a dodge bit that does nothing.
    /// </remarks>
    public static Enemy? BladeComing(SimulationLoop loop)
    {
        if (loop.Player.DodgeCooldown > 0 || loop.Player.RollRemaining > 0 || !loop.Body.IsOnGround() || loop.Body.IsInWater())
        {
            return null;
        }

        foreach (Enemy enemy in loop.Enemies)
        {
            if (enemy.IsDead || !enemy.IsSwinging)
            {
                continue;
            }

            long windup = enemy.Weapon.WindupTicks;
            bool coming = enemy.SwingTick >= windup - DodgeLead && enemy.SwingTick < windup + enemy.Weapon.ActiveTicks;
            if (coming && PathWalk.Distance(loop.Body.Position, enemy.Body.Position) <= DodgeRange)
            {
                return enemy;
            }
        }

        return null;
    }
}
