using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Entities;

/// <summary>
/// The effects of the worn weight on the player (D-23, D-28, D-314, D-316): the dodge cooldown, the walk and the
/// sprint speed, and the resistance to stagger. The weight is the sum of the weight of each worn armor piece (D-754).
/// </summary>
/// <remarks>
/// Each point of weight adds 1 tick to the dodge cooldown (D-755) and takes 0.5 percent off the walk and the sprint
/// (D-757). A weight of <see cref="StaggerResistWeight"/> or more resists stagger (D-756).
/// </remarks>
public static class Weight
{
    /// <summary>The worn weight at which a hit staggers nothing (D-756).</summary>
    public const long StaggerResistWeight = 24;

    /// <summary>The ticks that each point of weight adds to the dodge cooldown (D-755).</summary>
    public const long CooldownTicksPerWeight = 1;

    /// <summary>The steps of the speed factor: each point of weight takes one step of 200, which is 0.5 percent (D-757).</summary>
    public const long SpeedSteps = 200;

    /// <summary>
    /// The largest worn weight: five armor slots at the largest weight of one piece. The walk then keeps a quarter of
    /// its speed, and never stops.
    /// </summary>
    public const long LargestWeight = 5 * Content.ArmorDefinition.LargestNumber;

    /// <summary>The ticks from the press of one roll to the tick that can start the next, at a worn weight (D-755).</summary>
    /// <exception cref="ContextException">The weight is outside 0 to <see cref="LargestWeight"/>.</exception>
    public static int DodgeCooldownTicks(long weight)
    {
        Check(weight);
        return Player.DodgeCooldownTicks + (int)(weight * CooldownTicksPerWeight);
    }

    /// <summary>The factor on the walk and the sprint speed at a worn weight: 200 minus the weight, over 200 (D-757).</summary>
    /// <exception cref="ContextException">The weight is outside 0 to <see cref="LargestWeight"/>.</exception>
    public static float SpeedFactor(long weight)
    {
        Check(weight);
        return (SpeedSteps - weight) / (float)SpeedSteps;
    }

    /// <summary>Answers whether a worn weight resists stagger (D-314, D-756).</summary>
    /// <exception cref="ContextException">The weight is outside 0 to <see cref="LargestWeight"/>.</exception>
    public static bool ResistsStagger(long weight)
    {
        Check(weight);
        return weight >= StaggerResistWeight;
    }

    /// <summary>A weight below zero or over the largest one comes from a fault, and never from a worn set (T-2).</summary>
    private static void Check(long weight)
    {
        if (weight < 0 || weight > LargestWeight)
        {
            ContextException error = new($"A worn weight is from 0 to {LargestWeight}, and the weight is {weight} (D-754).");
            error.AddContext("weight", weight.ToString(CultureInfo.InvariantCulture));
            throw error;
        }
    }
}
