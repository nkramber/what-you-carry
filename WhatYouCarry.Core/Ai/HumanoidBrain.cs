using System.Collections.Generic;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Ai;

/// <summary>
/// The brain of one humanoid enemy (D-76, D-400, D-402): it wakes on sight of the player, walks a path of
/// <see cref="GridPathfinder"/> toward the player, swings inside the attack range, and retreats while the swing is
/// on its cooldown.
/// </summary>
/// <remarks>
/// <para>
/// A brain is asleep at its post, the floor cell that it spawned on. It wakes when the player comes inside the
/// sight range of the family and a clear ray joins the two eye points (D-400). A brain that loses that ray for the
/// give-up ticks of the family walks back to its post and sleeps again.
/// </para>
/// <para>
/// A brain of an escalation wave is relentless: it hunts from the tick it spawns, with no sight check and no give-up
/// (D-419). The wake and the give-up above stay the rule for the enemies of the floor plan.
/// </para>
/// <para>
/// A hunting brain walks toward the floor cell of the player with a <see cref="PathFollower"/>, which holds the
/// path and searches a new one when the goal moves. Over the last meters it walks straight at the body of the
/// player, because a walk to a cell never closes on a body that moves (F-104).
/// </para>
/// <para>
/// Inside the attack range, the brain holds still and swings when the cooldown is ready (D-402). While the
/// cooldown runs, it backs away from the player, so a fight reads as a rhythm of approach and retreat. The brain
/// holds still through a swing, so the blade meets what the windup aimed at.
/// </para>
/// <para>
/// The brain draws nothing. The Enemy stream of D-159 stays unused until an enemy choice needs a draw, so one
/// grid, one spawn, and one player path give one fight everywhere (G-9).
/// </para>
/// </remarks>
public sealed class HumanoidBrain
{
    // Radians to hundredths of a degree. The yaw of a facing wraps into one full turn.
    private const float HundredthsPerRadian = 18000.0f / DetMath.Pi;

    /// <summary>A velocity of no motion. The brain gives it while the enemy swings or waits.</summary>
    private static readonly Vector3 Still = new(0.0f, 0.0f, 0.0f);

    private readonly Enemy enemy;
    private readonly PathFollower follower = new();
    private int blindTicks;
    private bool sees;

    /// <summary>A brain at one post: asleep for an enemy of the floor plan, or hunting for a wave enemy (D-400, D-419).</summary>
    /// <param name="enemy">The enemy that this brain drives.</param>
    /// <param name="post">The floor cell that the enemy spawned on, which it returns to after a hunt (D-400).</param>
    /// <param name="relentless">True for a wave enemy, which hunts from its spawn and never gives up (D-419).</param>
    public HumanoidBrain(Enemy enemy, Cell post, bool relentless)
    {
        this.enemy = enemy;
        this.Post = post;
        this.IsRelentless = relentless;
        this.IsHunting = relentless;
    }

    /// <summary>Answers whether the brain hunts with no sight check and no give-up: the brain of a wave enemy (D-419).</summary>
    public bool IsRelentless { get; }

    /// <summary>The enemy that this brain drives.</summary>
    public Enemy Enemy => this.enemy;

    /// <summary>The floor cell that the enemy spawned on (D-400).</summary>
    public Cell Post { get; }

    /// <summary>Answers whether the brain hunts the player now (D-400).</summary>
    public bool IsHunting { get; private set; }

    /// <summary>The ticks with no line of sight to the player since the last one. A hunt ends at the give-up ticks of the family (D-400).</summary>
    public int BlindTicks => this.blindTicks;

    /// <summary>The path that the brain walks now, in walk order, or empty.</summary>
    public IReadOnlyList<Cell> Path => this.follower.Path;

    /// <summary>
    /// Runs one tick: the senses, the choice, and then the tick of the enemy.
    /// </summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="pathfinder">The pathfinder of the floor, which every brain of the floor shares (G-4).</param>
    /// <param name="playerFeet">The feet center of the player, in meters (D-235).</param>
    /// <param name="targets">The boxes that the blade of the enemy can hit on this tick.</param>
    public void Step(VoxelGrid grid, GridPathfinder pathfinder, Vector3 playerFeet, IReadOnlyList<EntityBox> targets)
    {
        Vector3 feet = this.enemy.Body.Position;
        float distance = PathWalk.Distance(feet, playerFeet);
        this.Sense(grid, feet, playerFeet, distance);

        // A swing runs to its end where it started, so the blade meets what the windup aimed at (D-324).
        if (this.enemy.IsSwinging)
        {
            this.enemy.Step(Still, false, this.enemy.Yaw, targets);
            return;
        }

        if (this.IsHunting && distance <= this.enemy.Definition.AttackRangeMetres)
        {
            this.Fight(feet, playerFeet, targets);
            return;
        }

        this.Approach(grid, pathfinder, feet, playerFeet, targets);
    }

    /// <summary>Folds the brain into the hash, in the declared order (D-160): the hunt, the blind ticks, the follower, and the relentless mark (D-419).</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.IsHunting ? 1 : 0);
        hash.Add(this.blindTicks);
        this.follower.AddTo(ref hash);
        hash.Add(this.IsRelentless ? 1 : 0);
    }

    /// <summary>The yaw that faces one point from another, in hundredths of a degree, from 0 up to one full turn (D-234).</summary>
    public static int YawToward(Vector3 from, Vector3 to)
    {
        float deltaX = to.X - from.X;
        float deltaZ = to.Z - from.Z;
        if (deltaX == 0.0f && deltaZ == 0.0f)
        {
            return 0;
        }

        // Forward at yaw zero is minus Z, and the yaw turns counterclockwise seen from above (D-234), so a facing
        // of (x, z) has a sine of minus x and a cosine of minus z.
        float radians = DetMath.Atan2(-deltaX, -deltaZ);
        int hundredths = (int)DetMath.Floor((radians * HundredthsPerRadian) + 0.5f);
        hundredths %= Simulation.SimulationLoop.FullTurn;
        return hundredths < 0 ? hundredths + Simulation.SimulationLoop.FullTurn : hundredths;
    }

    /// <summary>
    /// Reads the sight of the player and moves the hunt on (D-400). A brain wakes inside the sight range with a
    /// clear ray, and a hunting brain that loses the ray for the give-up ticks of its family goes back to its post.
    /// </summary>
    private void Sense(VoxelGrid grid, Vector3 feet, Vector3 playerFeet, float distance)
    {
        this.sees = distance <= this.enemy.Definition.SightMetres && PathWalk.Sees(grid, feet, playerFeet);
        if (this.IsRelentless)
        {
            // A wave enemy knows where the player is, so its hunt never ends (D-419).
            return;
        }

        if (!this.IsHunting)
        {
            if (this.sees)
            {
                this.IsHunting = true;
                this.blindTicks = 0;
                this.follower.Forget();
            }

            return;
        }

        if (this.sees)
        {
            this.blindTicks = 0;
            return;
        }

        this.blindTicks++;
        if (this.blindTicks >= this.enemy.Definition.GiveUpTicks)
        {
            this.IsHunting = false;
            this.blindTicks = 0;
            this.follower.Forget();
        }
    }

    /// <summary>Swings when the cooldown is ready, and backs away from the player while it runs (D-402).</summary>
    private void Fight(Vector3 feet, Vector3 playerFeet, IReadOnlyList<EntityBox> targets)
    {
        int yaw = YawToward(feet, playerFeet);
        if (this.enemy.CanSwing)
        {
            this.enemy.StartSwing();
            this.enemy.Step(Still, false, yaw, targets);
            return;
        }

        // The retreat keeps the face on the player, so the next swing starts from a facing that already aims.
        Vector3 away = Away(feet, playerFeet) * this.enemy.Definition.SpeedMetresPerSecond;
        this.enemy.Step(away, false, yaw, targets);
    }

    /// <summary>
    /// Walks toward the player, or back to the post. The last meters of a hunt are a straight walk at the body of
    /// the player, and the rest is a walk along a path (F-104). A step up is a jump in place, as in the greedy
    /// descender (D-165).
    /// </summary>
    private void Approach(VoxelGrid grid, GridPathfinder pathfinder, Vector3 feet, Vector3 playerFeet, IReadOnlyList<EntityBox> targets)
    {
        float speed = this.enemy.Definition.SpeedMetresPerSecond;
        if (this.IsHunting && this.sees && PathWalk.CanWalkStraight(grid, feet, playerFeet))
        {
            Vector3 straight = PathWalk.Toward(feet, playerFeet, speed * PlayerBody.TickSeconds) * speed;
            this.enemy.Step(straight, false, YawToward(feet, playerFeet), targets);
            return;
        }

        Cell goal = this.IsHunting ? PathWalk.FloorCellOf(playerFeet) : this.Post;
        bool onGround = this.enemy.Body.IsOnGround();
        if (!this.follower.TryNext(grid, pathfinder, feet, onGround, goal, out Cell next))
        {
            // The walk is over, or no search could run. A hunting brain then steps straight at the player, so it
            // keeps the pressure on while the search has no answer. A brain at its post holds its ground.
            Vector3 push = this.IsHunting
                ? PathWalk.Toward(feet, playerFeet, speed * PlayerBody.TickSeconds) * speed
                : Still;
            this.enemy.Step(push, false, this.IsHunting ? YawToward(feet, playerFeet) : this.enemy.Yaw, targets);
            return;
        }

        int yaw = this.IsHunting ? YawToward(feet, playerFeet) : YawToward(feet, PathWalk.CenterOf(next));
        if (PathWalk.NeedsAJump(grid, next, feet))
        {
            this.enemy.Step(Still, onGround, yaw, targets);
            return;
        }

        Vector3 toward = PathWalk.Toward(feet, PathWalk.CenterOf(next), speed * PlayerBody.TickSeconds) * speed;
        this.enemy.Step(toward, false, yaw, targets);
    }

    /// <summary>The unit direction from one point away from another, in the horizontal plane. Two points on one column give no direction.</summary>
    private static Vector3 Away(Vector3 feet, Vector3 from)
    {
        float deltaX = feet.X - from.X;
        float deltaZ = feet.Z - from.Z;
        float length = DetMath.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
        if (length == 0.0f)
        {
            return Still;
        }

        return new Vector3(deltaX / length, 0.0f, deltaZ / length);
    }
}
