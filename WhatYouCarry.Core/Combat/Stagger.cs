using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Core.Combat;

/// <summary>
/// The stagger of one entity and the guard that follows it (D-29, D-321, D-326). A hit staggers the entity, and
/// no hit staggers it again while the stagger or its guard holds.
/// </summary>
/// <remarks>
/// A stagger holds the body in place for <see cref="StaggerTicks"/> ticks and cancels a swing. The guard of
/// <see cref="GuardTicks"/> ticks then runs, so a stream of hits cannot hold an entity in one long stagger. A
/// two-handed swing gives hyper-armor, and a hit during it staggers nothing (D-29).
/// <para>
/// Two entities stagger (D-111): the player of PR-15, and the humanoid enemy of PR-16, which D-31 gives the same
/// melee rules. One copy keeps the two alike.
/// </para>
/// </remarks>
public sealed class Stagger
{
    /// <summary>The ticks of one stagger (D-326).</summary>
    public const int StaggerTicks = 20;

    /// <summary>The ticks after a stagger ends in which no hit staggers the entity (D-326).</summary>
    public const int GuardTicks = 30;

    /// <summary>The ticks of the stagger that remain. Zero means no stagger.</summary>
    public int Remaining { get; private set; }

    /// <summary>The ticks of the guard that remain after a stagger. Zero means no guard.</summary>
    public int GuardRemaining { get; private set; }

    /// <summary>Answers whether a stagger holds the entity now.</summary>
    public bool Holds => this.Remaining > 0;

    /// <summary>
    /// Answers whether a hit starts a stagger, and starts one when it does. A stagger, its guard, or hyper-armor
    /// stops it (D-29, D-326).
    /// </summary>
    /// <param name="hyperArmor">Whether a two-handed swing runs, which takes the stagger away (D-29).</param>
    public bool TryStart(bool hyperArmor)
    {
        if (this.Remaining > 0 || this.GuardRemaining > 0 || hyperArmor)
        {
            return false;
        }

        this.Remaining = StaggerTicks;
        return true;
    }

    /// <summary>
    /// Counts down one tick. The stagger runs first, and the guard starts on the tick that ends it. The guard runs
    /// on a tick with no stagger.
    /// </summary>
    /// <param name="staggered">Whether a stagger held at the start of the tick, before any hit of this tick.</param>
    public void Step(bool staggered)
    {
        if (staggered)
        {
            this.Remaining--;
            if (this.Remaining == 0)
            {
                this.GuardRemaining = GuardTicks;
            }

            return;
        }

        if (this.GuardRemaining > 0)
        {
            this.GuardRemaining--;
        }
    }

    /// <summary>Folds the stagger into the hash, in the declared order (D-160): the stagger ticks, then the guard ticks.</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Remaining);
        hash.Add(this.GuardRemaining);
    }
}
