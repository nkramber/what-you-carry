using System.Collections.Generic;
using WhatYouCarry.Core.Ai;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The greedy descender (D-127, D-149): it walks the reachability path from its floor cell to the stairwell,
/// descends at every stairwell, and ascends at the stairwell of the last floor, so its run ends at the bottom
/// (D-270). It promises progress, so a floor that passes its budget ends the run as a softlock. It swings at an
/// enemy inside the reach of its weapon, and it chases nothing (D-404).
/// </summary>
/// <remarks>
/// The walk goes waypoint by waypoint along a path of <see cref="GridPathfinder"/>, which a
/// <see cref="PathFollower"/> holds and searches again when a roll or a fall takes the body off it. A step up is a jump in place, then a move once the feet clear the step (D-165). A drop
/// is a walk off the edge. <see cref="BotIntent"/> builds each intent, so the movement bytes read the yaw after
/// the turn of the tick. The policy draws nothing, so it holds no stream.
/// <para>
/// The look stays at yaw zero while no enemy stands in reach. An enemy inside the reach of the weapon turns the
/// look onto it, because the blade sweeps an arc around the yaw and an enemy behind the body takes no hit (D-324,
/// D-325). A swing starts on a press, so the policy releases the attack bit on every second tick (D-323). The walk
/// goes on through the swing, because the walk stays free during one (D-324).
/// </para>
/// </remarks>
public sealed class GreedyDescender : IBotPolicy
{
    /// <summary>The name of the policy in the run log.</summary>
    public const string PolicyName = "greedy-descender";

    private readonly int lastFloor;
    private readonly PathFollower follower = new();
    private GridPathfinder? pathfinder;
    private int pathFloor;
    private bool attackHeld;

    /// <summary>A descender that ascends at the stairwell of the deepest floor that the content covers (D-3, D-252).</summary>
    public GreedyDescender(ContentSet content)
        : this(FloorGenerator.DeepestFloor(content))
    {
    }

    /// <summary>A descender that ascends at the stairwell of one floor. The coward walks with it to floor 1 (D-433).</summary>
    internal GreedyDescender(int lastFloor)
    {
        this.lastFloor = lastFloor;
    }

    /// <inheritdoc/>
    public string Name => PolicyName;

    /// <inheritdoc/>
    public bool PromisesProgress => true;

    /// <inheritdoc/>
    public Intent Next(SimulationLoop loop)
    {
        if (loop.Floor != this.pathFloor)
        {
            this.pathfinder = new GridPathfinder(loop.Grid);
            this.pathFloor = loop.Floor;
            this.follower.Forget();
            this.attackHeld = false;
        }

        Enemy? swinging = BotReflex.BladeComing(loop);
        if (swinging is not null)
        {
            int onto = HumanoidBrain.YawToward(loop.Body.Position, swinging.Body.Position);
            this.attackHeld = false;
            return BotIntent.Roll(loop.Tick, loop.Yaw, onto);
        }

        Enemy? inReach = NearestInReach(loop);
        ushort attack = this.AttackBit(inReach);
        int yaw = inReach is null ? 0 : HumanoidBrain.YawToward(loop.Body.Position, inReach.Body.Position);
        short yawDelta = BotIntent.TurnToward(loop.Yaw, yaw);

        Vector3 feet = loop.Body.Position;
        bool onGround = loop.Body.IsOnGround();
        if (this.pathfinder is null || !this.follower.TryNext(loop.Grid, this.pathfinder, feet, onGround, loop.Plan.Stairwell, out Cell next))
        {
            if (this.follower.LastSearchFailed)
            {
                // The box is 0.6 meters wide in cells of one meter, so a body can rest on the slope under one edge
                // of it with its own column open. No search starts from an open column. A step straight at the
                // stairwell with a jump takes the body off that spot, and the search works again (F-105).
                Vector3 nudge = PathWalk.Toward(feet, PathWalk.CenterOf(loop.Plan.Stairwell), PlayerBody.WalkSpeed * PlayerBody.TickSeconds);
                ushort nudgeJump = onGround ? Button.Jump : (ushort)0;
                return BotIntent.Walk(loop.Tick, nudge, yawDelta, yaw, (ushort)(attack | nudgeJump));
            }

            ushort choice = loop.Floor >= this.lastFloor ? Button.Ascend : Button.Interact;
            return new Intent(loop.Tick, yawDelta, 0, 0, 0, (ushort)(choice | attack));
        }

        // A step up jumps and walks on in the same tick, as a player does it. A jump in place rises and falls on
        // the same spot, and a step that a jump cannot clear then repeats for the rest of the floor (F-105). A body
        // wedged against a slope jumps for the same reason.
        bool hop = onGround && (PathWalk.NeedsAJump(loop.Grid, next, feet) || this.follower.IsWedged);
        ushort jump = hop ? Button.Jump : (ushort)0;
        Vector3 toward = PathWalk.Toward(feet, PathWalk.CenterOf(next), PlayerBody.WalkSpeed * PlayerBody.TickSeconds);
        return BotIntent.Walk(loop.Tick, toward, yawDelta, yaw, (ushort)(attack | jump));
    }

    /// <summary>The living enemy nearest the body inside the reach of the weapon, or null when none stands that near.</summary>
    private static Enemy? NearestInReach(SimulationLoop loop)
    {
        float reach = loop.Weapon.ReachCentimetres / 100.0f;
        Enemy? nearest = null;
        float best = 0.0f;
        foreach (Enemy enemy in loop.Enemies)
        {
            if (enemy.IsDead)
            {
                continue;
            }

            float distance = PathWalk.Distance(loop.Body.Position, enemy.Body.Position);
            if (distance <= reach && (nearest is null || distance < best))
            {
                nearest = enemy;
                best = distance;
            }
        }

        return nearest;
    }

    /// <summary>
    /// The attack bit of this tick: set when an enemy stands inside the reach of the weapon and the bit was clear
    /// on the tick before, so every press starts a swing (D-323, D-404).
    /// </summary>
    private ushort AttackBit(Enemy? inReach)
    {
        if (inReach is null)
        {
            this.attackHeld = false;
            return 0;
        }

        ushort attack = this.attackHeld ? (ushort)0 : Button.Attack;
        this.attackHeld = !this.attackHeld;
        return attack;
    }
}
