using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Entities;

/// <summary>
/// The player of PR-15: the body of PR-7, health, the dodge roll on a cooldown, the stagger and its guard, and the
/// swing of the main weapon (D-25, D-27 to D-29, D-315, D-316, D-321 to D-329, D-337). The player wears no armor
/// yet, so every rule here is the zero-weight case, and PR-22 adds the effects of weight (D-316).
/// </summary>
/// <remarks>
/// <para>
/// One tick runs in a fixed order. The dodge cooldown counts down first. A press of the dodge bit then starts a roll,
/// and a press of the attack bit starts a swing, unless a stagger holds. The body then moves: not at all during a
/// stagger, at the roll velocity during a roll, and at the walk or the sprint of the intent otherwise. An active tick
/// of a swing then tests its wedge against the target boxes. The swing, the roll, the stagger, and the guard count
/// down last.
/// </para>
/// <para>
/// A roll starts on the ground and out of still water, with the cooldown ready and no stagger. It cancels a swing,
/// it moves 3 meters over 18 ticks in the direction of the movement input, or backward with no input, and it takes
/// no jump and no swing. No hit lands during a roll (D-327, D-328, D-329, D-337).
/// </para>
/// <para>
/// A hit deals its damage, and health stops at zero, which is a death (D-322). A hit staggers the player for 20 ticks
/// and cancels a swing, unless a stagger or its guard of 30 ticks holds, or a two-handed swing gives hyper-armor
/// (D-29, D-321, D-326). A press during a swing, a roll, or a stagger does nothing, and no press waits for later
/// (D-323). The walk, the sprint, and the jump stay free during a swing (D-324).
/// </para>
/// </remarks>
public sealed class Player
{
    /// <summary>The health of a player at the start of a run (D-315).</summary>
    public const int MaxHealth = 100;

    /// <summary>The ticks of one roll (D-327).</summary>
    public const int RollTicks = 18;

    /// <summary>The distance of one roll, in meters (D-315).</summary>
    public const float RollDistance = 3.0f;

    /// <summary>The speed of a roll, in meters per second: the distance over the ticks of the roll, 10 meters per second (D-327).</summary>
    public const float RollSpeed = RollDistance / (RollTicks * PlayerBody.TickSeconds);

    /// <summary>The ticks from the press of one roll to the tick that can start the next, at zero weight (D-315, D-316, D-327).</summary>
    public const int DodgeCooldownTicks = 45;

    /// <summary>The ticks of one stagger (D-326).</summary>
    public const int StaggerTicks = 20;

    /// <summary>The ticks after a stagger ends in which no hit staggers the player (D-326).</summary>
    public const int GuardTicks = 30;

    /// <summary>The swing tick of a player that does not swing.</summary>
    public const int NoSwing = -1;

    private readonly WeaponDefinition weapon;
    private List<int> swingHits = [];
    private List<SwordHit> lastHits = [];

    /// <summary>A player at rest at the spawn point, with a main weapon and a health.</summary>
    /// <exception cref="ContextException">The health is outside 1 to <see cref="MaxHealth"/>, or the box at the spawn point overlaps a solid cell or reaches past the grid.</exception>
    public Player(VoxelGrid grid, Vector3 spawn, WeaponDefinition weapon, int health)
    {
        if (health < 1 || health > MaxHealth)
        {
            ContextException error = new($"A player starts with a health from 1 to {MaxHealth}, and the health is {health}.");
            error.AddContext("health", ((long)health).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.Body = new PlayerBody(grid, spawn);
        this.weapon = weapon;
        this.Health = health;
    }

    /// <summary>The body: the box, the position, and the vertical velocity (D-165).</summary>
    public PlayerBody Body { get; }

    /// <summary>The main weapon (D-20, D-320).</summary>
    public WeaponDefinition Weapon => this.weapon;

    /// <summary>The health, from zero to <see cref="MaxHealth"/> (D-315).</summary>
    public int Health { get; private set; }

    /// <summary>Answers whether the health is zero (D-322).</summary>
    public bool IsDead => this.Health == 0;

    /// <summary>The ticks until a press of the dodge bit can start a roll. Zero means ready.</summary>
    public int DodgeCooldown { get; private set; }

    /// <summary>The ticks of the roll that remain. Zero means no roll.</summary>
    public int RollRemaining { get; private set; }

    /// <summary>The horizontal velocity of the roll, in meters per second. It holds its value from the press to the end of the roll.</summary>
    public Vector3 RollVelocity { get; private set; }

    /// <summary>The ticks of the swing that ran, from zero, or <see cref="NoSwing"/>. It is also the swing tick of the next tick.</summary>
    public long SwingTick { get; private set; } = NoSwing;

    /// <summary>The ticks of the stagger that remain. Zero means no stagger.</summary>
    public int StaggerRemaining { get; private set; }

    /// <summary>The ticks of the guard that remain after a stagger. Zero means no guard.</summary>
    public int GuardRemaining { get; private set; }

    /// <summary>The owner ids that the swing hit, in hit order. A target takes one hit per swing (D-325).</summary>
    public IReadOnlyList<int> SwingHits => this.swingHits;

    /// <summary>The hits of the swing on the last tick, in target order. The Game layer and a later enemy read them. It is not state.</summary>
    public IReadOnlyList<SwordHit> LastHits => this.lastHits;

    /// <summary>Runs one tick.</summary>
    /// <param name="intent">The intent of the tick.</param>
    /// <param name="previousButtons">The buttons of the intent before, which tell a press from a held bit (D-323).</param>
    /// <param name="yaw">The yaw sum of the loop after the intent, in hundredths of a degree (D-227).</param>
    /// <param name="targets">The boxes that the blade can hit on this tick.</param>
    /// <exception cref="ContextException">The player is dead.</exception>
    public void Step(Intent intent, ushort previousButtons, int yaw, IReadOnlyList<EntityBox> targets)
    {
        if (this.IsDead)
        {
            ContextException error = new($"The player has no health at tick {intent.Tick}, and a dead player takes no tick (D-322).");
            error.AddContext("tick", ((long)intent.Tick).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.lastHits = [];
        bool attackPressed = (intent.Buttons & Button.Attack) != 0 && (previousButtons & Button.Attack) == 0;
        bool dodgePressed = (intent.Buttons & Button.Dodge) != 0 && (previousButtons & Button.Dodge) == 0;
        bool inWater = this.Body.IsInWater();
        bool staggered = this.StaggerRemaining > 0;
        if (this.DodgeCooldown > 0)
        {
            this.DodgeCooldown--;
        }

        if (dodgePressed && !staggered && this.RollRemaining == 0 && this.DodgeCooldown == 0 && !inWater && this.Body.IsOnGround())
        {
            this.SwingTick = NoSwing;
            this.swingHits = [];
            this.RollRemaining = RollTicks;
            this.DodgeCooldown = DodgeCooldownTicks;
            this.RollVelocity = PlayerBody.RollDirection(intent, yaw) * RollSpeed;
        }

        if (attackPressed && !staggered && this.RollRemaining == 0 && this.SwingTick == NoSwing)
        {
            this.SwingTick = 0;
            this.swingHits = [];
        }

        if (staggered)
        {
            this.Body.Move(new Vector3(0.0f, 0.0f, 0.0f), false, inWater);
        }
        else if (this.RollRemaining > 0)
        {
            this.Body.Move(this.RollVelocity, false, inWater);
        }
        else
        {
            float speedFactor = inWater ? PlayerBody.WaterSpeedFactor : 1.0f;
            this.Body.Move(PlayerBody.WalkVelocity(intent, yaw) * speedFactor, (intent.Buttons & Button.Jump) != 0, inWater);
        }

        if (this.SwingTick != NoSwing)
        {
            long step = this.SwingTick - this.weapon.WindupTicks;
            if (step >= 0 && step < this.weapon.ActiveTicks)
            {
                this.Strike(step, yaw, targets);
            }

            this.SwingTick++;
            if (this.SwingTick == this.weapon.SwingTicks)
            {
                this.SwingTick = NoSwing;
                this.swingHits = [];
            }
        }

        if (this.RollRemaining > 0)
        {
            this.RollRemaining--;
        }

        if (staggered)
        {
            this.StaggerRemaining--;
            if (this.StaggerRemaining == 0)
            {
                this.GuardRemaining = GuardTicks;
            }
        }
        else if (this.GuardRemaining > 0)
        {
            this.GuardRemaining--;
        }
    }

    /// <summary>
    /// Takes one hit. A roll takes no hit (D-328). Otherwise the damage comes off the health, which stops at zero
    /// (D-322), and the hit staggers the player and cancels a swing, unless a stagger or its guard holds or a
    /// two-handed swing gives hyper-armor (D-29, D-326).
    /// </summary>
    /// <exception cref="ContextException">The damage is below zero, or the player is dead.</exception>
    public void TakeHit(long damage)
    {
        if (damage < 0)
        {
            ContextException negative = new($"A hit deals a damage of zero or more, and the damage is {damage}.");
            negative.AddContext("damage", damage.ToString(CultureInfo.InvariantCulture));
            throw negative;
        }

        if (this.IsDead)
        {
            throw new ContextException("The player has no health, and a dead player takes no hit (D-322).");
        }

        if (this.RollRemaining > 0)
        {
            return;
        }

        long health = this.Health - damage;
        this.Health = health < 0 ? 0 : (int)health;
        if (this.IsDead)
        {
            return;
        }

        bool hyperArmor = this.SwingTick != NoSwing && this.weapon.IsTwoHanded;
        if (this.StaggerRemaining == 0 && this.GuardRemaining == 0 && !hyperArmor)
        {
            this.StaggerRemaining = StaggerTicks;
            this.SwingTick = NoSwing;
            this.swingHits = [];
        }
    }

    /// <summary>
    /// Folds the player into the hash, in the declared order (D-160): the health, the dodge cooldown, the roll, the
    /// swing tick, the stagger, the guard, and the owner ids that the swing hit, after their count.
    /// </summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Health);
        hash.Add(this.DodgeCooldown);
        hash.Add(this.RollRemaining);
        hash.Add(this.RollVelocity.X);
        hash.Add(this.RollVelocity.Z);
        hash.Add(this.SwingTick);
        hash.Add(this.StaggerRemaining);
        hash.Add(this.GuardRemaining);
        hash.Add(this.swingHits.Count);
        foreach (int owner in this.swingHits)
        {
            hash.Add(owner);
        }
    }

    /// <summary>
    /// Tests the wedge of one active step against every target that the swing did not hit yet, from the yaw of this
    /// tick (D-324, D-325). A box that the wedge meets takes the damage of the weapon once in the swing.
    /// </summary>
    private void Strike(long step, int yaw, IReadOnlyList<EntityBox> targets)
    {
        int fromYaw = yaw + MeleeWeapon.BladeOffset(this.weapon, step);
        int toYaw = yaw + MeleeWeapon.BladeOffset(this.weapon, step + 1);
        for (int index = 0; index < targets.Count; index++)
        {
            EntityBox target = targets[index];
            bool alreadyHit = false;
            for (int hit = 0; hit < this.swingHits.Count; hit++)
            {
                if (this.swingHits[hit] == target.Owner)
                {
                    alreadyHit = true;
                }
            }

            if (!alreadyHit && MeleeWeapon.WedgeHits(this.weapon, this.Body.Position, fromYaw, toYaw, target.Box))
            {
                this.swingHits.Add(target.Owner);
                this.lastHits.Add(new SwordHit(target.Owner, this.weapon.Damage));
            }
        }
    }
}
