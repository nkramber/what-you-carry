using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Entities;

/// <summary>
/// One enemy of a humanoid family (D-31, D-395, D-396): the box of D-165, health, the stagger and its guard, and
/// the swing of the weapon that its family names. <see cref="Ai.HumanoidBrain"/> decides what it does, and this
/// class runs the decision.
/// </summary>
/// <remarks>
/// <para>
/// A humanoid uses the player rules (D-31), so the body, the swing, and the stagger here are the types that the
/// player uses: <see cref="PlayerBody"/> holds the box of D-165, <see cref="Swing"/> holds the melee state
/// machine, and <see cref="Stagger"/> holds the flinch of D-326. The numbers that differ come from the family
/// file: the health, the speed, and the cadence (D-399, D-402).
/// </para>
/// <para>
/// One tick runs in a fixed order. The attack cooldown counts down first. The body then moves: not at all during a
/// stagger, and at the velocity of the brain otherwise. The swing then runs its tick, and a swing that ends starts
/// the cooldown. The stagger counts down last. The order follows the player of PR-15, so the two entities read
/// alike.
/// </para>
/// <para>
/// The enemy wears no armor, and PR-22 adds the effects of weight (D-316). It carries no roll, because the roll is
/// a player action of D-327. A hit deals its damage, health stops at zero, and a dead enemy takes no tick and is no
/// target (D-322).
/// </para>
/// </remarks>
public sealed class Enemy
{
    private readonly EnemyDefinition definition;
    private readonly Swing swing;
    private readonly Stagger stagger = new();

    /// <summary>An enemy at rest at a spawn point, with the health of its family.</summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="spawn">The feet center of the spawn, in meters (D-235).</param>
    /// <param name="definition">The family (D-395).</param>
    /// <param name="weapon">The weapon that the family names (D-397).</param>
    /// <param name="owner">The owner id of the enemy in the hits and the target boxes. The player is zero.</param>
    /// <exception cref="ContextException">The owner id is zero or below, or the box at the spawn point overlaps a solid part.</exception>
    public Enemy(VoxelGrid grid, Vector3 spawn, EnemyDefinition definition, WeaponDefinition weapon, int owner)
    {
        if (owner < 1)
        {
            ContextException error = new($"The owner id of an enemy is one or more, and the player holds zero. The id is {owner}.");
            error.AddContext("owner", ((long)owner).ToString(CultureInfo.InvariantCulture));
            error.AddContext("enemyFamily", definition.Id);
            throw error;
        }

        this.Body = new PlayerBody(grid, spawn);
        this.definition = definition;
        this.swing = new Swing(weapon);
        this.Owner = owner;
        this.Health = (int)definition.Health;
    }

    /// <summary>The owner id in the hits and the target boxes (D-325).</summary>
    public int Owner { get; }

    /// <summary>The family (D-395).</summary>
    public EnemyDefinition Definition => this.definition;

    /// <summary>The body: the box, the position, and the vertical velocity (D-165).</summary>
    public PlayerBody Body { get; }

    /// <summary>The weapon that every spawn of the family carries (D-397).</summary>
    public WeaponDefinition Weapon => this.swing.Weapon;

    /// <summary>The health, from zero to the health of the family (D-399).</summary>
    public int Health { get; private set; }

    /// <summary>Answers whether the health is zero (D-322).</summary>
    public bool IsDead => this.Health == 0;

    /// <summary>The way the enemy faces, in hundredths of a degree, as the yaw of the loop (D-227, D-234).</summary>
    public int Yaw { get; private set; }

    /// <summary>The ticks until the next swing can start. Zero means ready (D-402).</summary>
    public int AttackCooldown { get; private set; }

    /// <summary>The ticks of the swing that ran, from zero, or <see cref="Swing.NoSwing"/>.</summary>
    public long SwingTick => this.swing.Tick;

    /// <summary>Answers whether a swing runs now.</summary>
    public bool IsSwinging => this.swing.IsSwinging;

    /// <summary>The ticks of the stagger that remain. Zero means no stagger (D-326).</summary>
    public int StaggerRemaining => this.stagger.Remaining;

    /// <summary>The ticks of the guard that remain after a stagger. Zero means no guard (D-326).</summary>
    public int GuardRemaining => this.stagger.GuardRemaining;

    /// <summary>The hits of the swing on the last tick, in target order. The loop reads them. They are not state.</summary>
    public IReadOnlyList<SwordHit> LastHits => this.swing.LastHits;

    /// <summary>The target box of the enemy, which the blade of the player and a projectile can meet.</summary>
    public EntityBox TargetBox => new(this.Owner, this.Body.Box);

    /// <summary>Answers whether a swing can start now: no swing runs, the cooldown is ready, and no stagger holds (D-402).</summary>
    public bool CanSwing => !this.swing.IsSwinging && this.AttackCooldown == 0 && !this.stagger.Holds;

    /// <summary>Starts a swing. The caller checks <see cref="CanSwing"/> first.</summary>
    /// <exception cref="ContextException">No swing can start now.</exception>
    public void StartSwing()
    {
        if (!this.CanSwing)
        {
            ContextException error = new($"The enemy {this.Owner} of the family '{this.definition.Id}' cannot start a swing now. The swing tick is {this.SwingTick}, the cooldown is {this.AttackCooldown}, and the stagger is {this.StaggerRemaining} (D-402).");
            error.AddContext("owner", ((long)this.Owner).ToString(CultureInfo.InvariantCulture));
            error.AddContext("enemyFamily", this.definition.Id);
            error.AddContext("swingTick", this.SwingTick.ToString(CultureInfo.InvariantCulture));
            error.AddContext("attackCooldown", ((long)this.AttackCooldown).ToString(CultureInfo.InvariantCulture));
            error.AddContext("staggerRemaining", ((long)this.StaggerRemaining).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.swing.Start();
    }

    /// <summary>Runs one tick.</summary>
    /// <param name="horizontal">The horizontal velocity of the tick, in meters per second, from the brain. Its Y component is zero.</param>
    /// <param name="jump">Whether the tick asks for a jump. A jump needs the ground (D-165).</param>
    /// <param name="yaw">The way the enemy faces on this tick, in hundredths of a degree (D-227).</param>
    /// <param name="targets">The boxes that the blade can hit on this tick.</param>
    /// <exception cref="ContextException">The enemy is dead.</exception>
    public void Step(Vector3 horizontal, bool jump, int yaw, IReadOnlyList<EntityBox> targets)
    {
        if (this.IsDead)
        {
            ContextException error = new($"The enemy {this.Owner} of the family '{this.definition.Id}' has no health, and a dead enemy takes no tick (D-322).");
            error.AddContext("owner", ((long)this.Owner).ToString(CultureInfo.InvariantCulture));
            error.AddContext("enemyFamily", this.definition.Id);
            throw error;
        }

        this.Yaw = yaw;
        bool staggered = this.stagger.Holds;
        if (this.AttackCooldown > 0)
        {
            this.AttackCooldown--;
        }

        bool inWater = this.Body.IsInWater();
        if (staggered)
        {
            this.Body.Move(new Vector3(0.0f, 0.0f, 0.0f), false, inWater, true);
        }
        else
        {
            float speedFactor = inWater ? PlayerBody.WaterSpeedFactor : 1.0f;
            this.Body.Move(horizontal * speedFactor, jump, inWater, true);
        }

        bool swinging = this.swing.IsSwinging;
        this.swing.Step(this.Body.Position, yaw, targets);
        if (swinging && !this.swing.IsSwinging)
        {
            this.AttackCooldown = (int)this.definition.AttackCooldownTicks;
        }

        this.stagger.Step(staggered);
    }

    /// <summary>
    /// Takes one hit. The damage comes off the health, which stops at zero (D-322), and the hit staggers the enemy
    /// and cancels a swing, unless a stagger or its guard holds or a two-handed swing gives hyper-armor (D-29,
    /// D-326). A stagger that cancels a swing starts the attack cooldown, so a staggered enemy cannot swing again
    /// at once (D-402).
    /// </summary>
    /// <exception cref="ContextException">The damage is below zero, or the enemy is dead.</exception>
    public void TakeHit(long damage)
    {
        if (damage < 0)
        {
            ContextException negative = new($"A hit deals a damage of zero or more, and the damage is {damage}.");
            negative.AddContext("damage", damage.ToString(CultureInfo.InvariantCulture));
            negative.AddContext("owner", ((long)this.Owner).ToString(CultureInfo.InvariantCulture));
            throw negative;
        }

        if (this.IsDead)
        {
            ContextException dead = new($"The enemy {this.Owner} of the family '{this.definition.Id}' has no health, and a dead enemy takes no hit (D-322).");
            dead.AddContext("owner", ((long)this.Owner).ToString(CultureInfo.InvariantCulture));
            dead.AddContext("enemyFamily", this.definition.Id);
            throw dead;
        }

        long health = this.Health - damage;
        this.Health = health < 0 ? 0 : (int)health;
        if (this.IsDead)
        {
            return;
        }

        if (this.stagger.TryStart(this.swing.HasHyperArmor))
        {
            this.swing.Cancel();
            this.AttackCooldown = (int)this.definition.AttackCooldownTicks;
        }
    }

    /// <summary>
    /// Folds the enemy into the hash, in the declared order (D-160): the position, the vertical velocity, the
    /// health, the yaw, the attack cooldown, the swing, and the stagger.
    /// </summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Body.Position.X);
        hash.Add(this.Body.Position.Y);
        hash.Add(this.Body.Position.Z);
        hash.Add(this.Body.VerticalVelocity);
        hash.Add(this.Health);
        hash.Add(this.Yaw);
        hash.Add(this.AttackCooldown);
        this.swing.AddTo(ref hash);
        this.stagger.AddTo(ref hash);
    }
}
