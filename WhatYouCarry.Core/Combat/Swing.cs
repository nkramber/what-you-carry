using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;

namespace WhatYouCarry.Core.Combat;

/// <summary>
/// The state of one melee swing (D-25, D-315, D-324, D-325): which tick of the swing runs, and which owner ids the
/// blade met. <see cref="MeleeWeapon"/> holds the geometry, and this class holds the state.
/// </summary>
/// <remarks>
/// A swing runs the windup, the active ticks, and the recovery of its weapon, in that order. Each active tick
/// sweeps one step of the arc from the yaw of that tick, so the arc follows the look (D-324). A target takes one
/// hit per swing (D-325).
/// <para>
/// Two entities swing (D-111): the player of PR-15, and the humanoid enemy of PR-16, which D-31 gives the same
/// melee rules. One copy of the state machine keeps the two alike, and exit test 3 of PR-16 reads the phases of
/// both from the weapon definition.
/// </para>
/// </remarks>
public sealed class Swing
{
    /// <summary>The swing tick of an entity that does not swing.</summary>
    public const int NoSwing = -1;

    private readonly WeaponDefinition weapon;
    private List<int> hits = [];
    private List<SwordHit> lastHits = [];

    /// <summary>A swing state at rest for one weapon.</summary>
    public Swing(WeaponDefinition weapon)
    {
        this.weapon = weapon;
    }

    /// <summary>The weapon that the swing runs (D-20, D-320).</summary>
    public WeaponDefinition Weapon => this.weapon;

    /// <summary>The ticks of the swing that ran, from zero, or <see cref="NoSwing"/>. It is also the swing tick of the next tick.</summary>
    public long Tick { get; private set; } = NoSwing;

    /// <summary>Answers whether a swing runs.</summary>
    public bool IsSwinging => this.Tick != NoSwing;

    /// <summary>Answers whether the swing gives hyper-armor, which a two-handed weapon does (D-29).</summary>
    public bool HasHyperArmor => this.IsSwinging && this.weapon.IsTwoHanded;

    /// <summary>The owner ids that the swing hit, in hit order. A target takes one hit per swing (D-325).</summary>
    public IReadOnlyList<int> Hits => this.hits;

    /// <summary>The hits of the last tick, in target order. The Game layer and the loop read them. They are not state.</summary>
    public IReadOnlyList<SwordHit> LastHits => this.lastHits;

    /// <summary>Starts a swing at its first tick, with no hit yet.</summary>
    public void Start()
    {
        this.Tick = 0;
        this.hits = [];
    }

    /// <summary>Ends a swing before its last tick, and drops the hits of it (D-326, D-329).</summary>
    public void Cancel()
    {
        this.Tick = NoSwing;
        this.hits = [];
    }

    /// <summary>
    /// Runs one tick. An active tick tests the wedge of that step against every target that the swing did not hit
    /// yet, and the swing ends after its recovery. A tick with no swing clears the hits of the last tick and does
    /// nothing else.
    /// </summary>
    /// <param name="feet">The feet center of the body that swings, in meters (D-235).</param>
    /// <param name="yaw">The yaw of this tick, in hundredths of a degree (D-227).</param>
    /// <param name="targets">The boxes that the blade can hit on this tick.</param>
    public void Step(Vector3 feet, int yaw, IReadOnlyList<EntityBox> targets)
    {
        this.lastHits = [];
        if (!this.IsSwinging)
        {
            return;
        }

        long step = this.Tick - this.weapon.WindupTicks;
        if (step >= 0 && step < this.weapon.ActiveTicks)
        {
            this.Strike(step, feet, yaw, targets);
        }

        this.Tick++;
        if (this.Tick == this.weapon.SwingTicks)
        {
            this.Cancel();
        }
    }

    /// <summary>Folds the swing into the hash, in the declared order (D-160): the swing tick, then the owner ids that it hit, after their count.</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Tick);
        hash.Add(this.hits.Count);
        foreach (int owner in this.hits)
        {
            hash.Add(owner);
        }
    }

    /// <summary>
    /// Tests the wedge of one active step against every target that the swing did not hit yet, from the yaw of this
    /// tick (D-324, D-325). A box that the wedge meets takes the damage of the weapon once in the swing.
    /// </summary>
    private void Strike(long step, Vector3 feet, int yaw, IReadOnlyList<EntityBox> targets)
    {
        int fromYaw = yaw + MeleeWeapon.BladeOffset(this.weapon, step);
        int toYaw = yaw + MeleeWeapon.BladeOffset(this.weapon, step + 1);
        for (int index = 0; index < targets.Count; index++)
        {
            EntityBox target = targets[index];
            bool alreadyHit = false;
            for (int hit = 0; hit < this.hits.Count; hit++)
            {
                if (this.hits[hit] == target.Owner)
                {
                    alreadyHit = true;
                }
            }

            if (!alreadyHit && MeleeWeapon.WedgeHits(this.weapon, feet, fromYaw, toYaw, target.Box))
            {
                this.hits.Add(target.Owner);
                this.lastHits.Add(new SwordHit(target.Owner, this.weapon.Damage));
            }
        }
    }
}
