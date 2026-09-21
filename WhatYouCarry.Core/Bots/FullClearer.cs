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
/// The full clearer (D-149): it hunts every enemy of the floor, and it walks to the stairwell when none is left.
/// It descends at every stairwell, and it ascends at the stairwell of the last floor, so its run ends at the
/// bottom (D-270). It promises progress, so a floor that passes its budget ends the run as a softlock.
/// </summary>
/// <remarks>
/// <para>
/// The policy walks a path of <see cref="GridPathfinder"/> to the floor cell of the nearest living enemy, and it
/// presses the attack bit inside the reach of its weapon. The nearest enemy is the one of the shortest straight
/// distance, and a tie takes the earlier spawn, so one floor gives one hunt order everywhere (G-9). It holds that
/// target until the enemy dies, so two enemies of one distance never trade the hunt from tick to tick.
/// </para>
/// <para>
/// The walk and the jump rules are the rules of <see cref="PathWalk"/>, which the greedy descender also reads
/// (D-165, D-345). The look turns toward the target, because the blade of a swing follows the yaw (D-324). A walk
/// toward the stairwell keeps the yaw of the last look, so the movement bytes carry the direction.
/// </para>
/// <para>
/// The policy rolls through a blade that is about to go live, which <see cref="BotReflex"/> reads, because no hit
/// lands during a roll (D-328). It faces the enemy and rolls forward, so the roll ends beside it or behind it, in
/// reach of its own blade (D-327).
/// </para>
/// <para>
/// The policy draws nothing, so it holds no stream (D-272). It takes damage like a player, and a death ends the
/// run as the death end state of D-403.
/// </para>
/// <para>
/// A floor can strand the policy away from an enemy, because a shaft drops a body into a space that no move leads
/// back out of (D-253). An enemy that the search cannot reach leaves the hunt for the rest of the floor, and the
/// policy takes the stairwell once every enemy it can reach is dead. A hunt that waited for an enemy it can never
/// reach would run out the floor budget and read as a softlock, which it is not (D-270).
/// </para>
/// <para>
/// A hunt that comes no nearer and lands no hit for <see cref="StalledTicks"/> ticks drops its enemy too. A wall
/// and a ledge can hold the two apart, and the floor budget of a run must never pay for one of them.
/// </para>
/// <para>
/// The policy leaves when the time runs short (D-439). Once each second it measures the walk that the floor still
/// asks for: the path to the stairwell, or, during a hunt, the path to the enemy and from the enemy to the
/// stairwell. When the timer left is no more than that walk, it stops the hunt for the rest of the floor. The
/// night of 2026-09-21 found two floors, seeds 2100 and 2109, where the clear ended near 5,750 ticks and the walk
/// to the stairwell wound around the floor past expiry. On seed 2109 the last hunt dropped off a ledge that no
/// move climbs back, so the way back looped around the floor. A player leaves in time, and so does the policy.
/// </para>
/// </remarks>
public sealed class FullClearer : IBotPolicy
{
    /// <summary>The name of the policy in the run log.</summary>
    public const string PolicyName = "full-clearer";

    /// <summary>The searches in a row that must fail before the hunt drops one enemy as unreachable.</summary>
    public const int FailedSearches = 4;

    /// <summary>The ticks of a hunt with no gain before the hunt drops one enemy: ten seconds (D-271).</summary>
    public const int StalledTicks = 600;

    /// <summary>How much nearer the body must come for a hunt to count as a gain, in meters.</summary>
    public const float Gain = 0.5f;

    /// <summary>The ticks between two measures of the walk to the stairwell: one second (D-439).</summary>
    public const int ReserveCheckTicks = 60;

    /// <summary>The ticks of one cell of path at the walk speed: one meter at four meters each second (D-439).</summary>
    public const int TicksPerCell = 15;

    /// <summary>
    /// The factor on the straight walk of a path. A path has jumps, ramps, and turns, and a body on it goes slower
    /// than the walk speed: the walk back on seed 2109 took about 37 ticks for each cell (D-439).
    /// </summary>
    public const int WalkAllowance = 3;

    /// <summary>The ticks that the walk estimate keeps in hand past the walk: ten seconds (D-439).</summary>
    public const int ReserveMarginTicks = 600;

    private readonly PathFollower follower = new();
    private List<int> unreachable = [];
    private readonly int lastFloor;
    private GridPathfinder? pathfinder;
    private int pathFloor;
    private int target = NoTarget;
    private int failed;
    private int stalled;
    private float nearest;
    private int targetHealth;
    private bool attackHeld;
    private bool leaving;

    /// <summary>The owner id that no enemy carries, which marks a policy with no target.</summary>
    private const int NoTarget = -1;

    /// <summary>A clearer that ascends at the stairwell of the deepest floor that the content covers (D-3, D-252).</summary>
    public FullClearer(ContentSet content)
    {
        int deepest = 0;
        foreach (FloorTemplate template in content.Floors)
        {
            if (template.MaxDepth > deepest)
            {
                deepest = (int)template.MaxDepth;
            }
        }

        this.lastFloor = deepest;
    }

    /// <inheritdoc/>
    public string Name => PolicyName;

    /// <summary>
    /// The owner ids that the hunt dropped on the floor that it walks now: an enemy that no path reaches, and one
    /// that a hunt gained nothing on. The list starts empty on every floor.
    /// </summary>
    public IReadOnlyList<int> Dropped => this.unreachable;

    /// <inheritdoc/>
    public bool PromisesProgress => true;

    /// <summary>Answers whether the policy stopped the hunt on this floor, because the time left is short (D-439).</summary>
    public bool IsLeaving => this.leaving;

    /// <inheritdoc/>
    public Intent Next(SimulationLoop loop)
    {
        if (loop.Floor != this.pathFloor)
        {
            this.pathfinder = new GridPathfinder(loop.Grid);
            this.pathFloor = loop.Floor;
            this.unreachable = [];
            this.leaving = false;
            this.Forget();
        }

        if (!this.leaving && this.TimeIsShort(loop))
        {
            // The walk to the stairwell takes all the time left, so the hunt ends here. The path of the hunt leads
            // elsewhere, so the walk starts fresh.
            this.leaving = true;
            this.Forget();
        }

        // A swing lands on what stands in reach now, and never on the enemy that the walk aims at. An enemy that
        // closed on the body while the walk led elsewhere is the one that the blade meets.
        Enemy? swinging = BotReflex.BladeComing(loop);
        if (swinging is not null)
        {
            int onto = HumanoidBrain.YawToward(loop.Body.Position, swinging.Body.Position);
            this.attackHeld = false;
            return BotIntent.Roll(loop.Tick, loop.Yaw, onto);
        }

        if (this.leaving)
        {
            return this.WalkToStairwell(loop);
        }

        Enemy? inReach = this.NearestLiving(loop, loop.Weapon.ReachCentimetres / 100.0f);
        if (inReach is not null)
        {
            return this.Strike(loop, inReach);
        }

        this.attackHeld = false;
        Enemy? hunted = this.Hunted(loop);
        if (hunted is null)
        {
            return this.WalkToStairwell(loop);
        }

        Vector3 feet = loop.Body.Position;
        Vector3 prey = hunted.Body.Position;
        if (this.Stalled(hunted, PathWalk.Distance(feet, prey)))
        {
            this.unreachable.Add(hunted.Owner);
            this.Forget();
            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }

        int yaw = HumanoidBrain.YawToward(feet, prey);
        short yawDelta = BotIntent.TurnToward(loop.Yaw, yaw);

        // The last meters are a straight walk at the body of the enemy. A walk to the cell of a body that moves
        // swings around it and never closes (F-104).
        if (PathWalk.CanWalkStraight(loop.Grid, feet, prey))
        {
            Vector3 straight = PathWalk.Toward(feet, prey, PlayerBody.WalkSpeed * PlayerBody.TickSeconds);
            return BotIntent.Walk(loop.Tick, straight, yawDelta, yaw, 0);
        }

        if (this.TryWalk(loop, PathWalk.FloorCellOf(prey), yawDelta, yaw, out Intent walk))
        {
            this.failed = 0;
            return walk;
        }

        // A search that fails again and again means that no move rule path leads to this enemy from where the body
        // stands, so the hunt drops it and takes the next one (D-165).
        if (this.follower.LastSearchFailed)
        {
            this.failed++;
            if (this.failed >= FailedSearches)
            {
                this.unreachable.Add(hunted.Owner);
                this.Forget();
            }
        }

        return walk;
    }

    /// <summary>
    /// Answers whether the timer left is no more than the walk that the floor still asks for, once each
    /// <see cref="ReserveCheckTicks"/> ticks (D-439). The walk counts the path cells to the stairwell, or, during a
    /// hunt, the cells to the enemy and from the enemy to the stairwell when that sum is larger. Each cell costs
    /// <see cref="TicksPerCell"/> times <see cref="WalkAllowance"/>, and <see cref="ReserveMarginTicks"/> comes on
    /// top. A body in the air or off a floor cell has no start for a search, and a floor with no path to the
    /// stairwell has no walk, so both answer no and the check runs again one second later.
    /// </summary>
    private bool TimeIsShort(SimulationLoop loop)
    {
        if (loop.Tick % ReserveCheckTicks != 0 || this.pathfinder is null || !loop.Body.IsOnGround())
        {
            return false;
        }

        Cell start = PathWalk.FloorCellOf(loop.Body.Position);
        if (!GridMoves.IsFloor(loop.Grid, start) || !this.pathfinder.TryFind(start, loop.Plan.Stairwell, out IReadOnlyList<Cell> path))
        {
            return false;
        }

        long cells = path.Count;
        long hunt = this.HuntCells(loop, start);
        if (hunt > cells)
        {
            cells = hunt;
        }

        long walk = (cells * TicksPerCell * WalkAllowance) + ReserveMarginTicks;
        return loop.Timer.Remaining <= walk;
    }

    /// <summary>
    /// The path cells of the hunt of the held target: from the body to the floor cell of the enemy, and from there
    /// to the stairwell. Zero when no target lives, or when the enemy stands off a floor cell or no path joins the
    /// three cells, because a hunt with no path has no walk to count (D-439).
    /// </summary>
    private long HuntCells(SimulationLoop loop, Cell start)
    {
        GridPathfinder? search = this.pathfinder;
        if (search is null || this.target == NoTarget)
        {
            return 0;
        }

        foreach (Enemy enemy in loop.Enemies)
        {
            if (enemy.Owner != this.target || enemy.IsDead)
            {
                continue;
            }

            Cell prey = PathWalk.FloorCellOf(enemy.Body.Position);
            if (GridMoves.IsFloor(loop.Grid, prey)
                && search.TryFind(start, prey, out IReadOnlyList<Cell> there)
                && search.TryFind(prey, loop.Plan.Stairwell, out IReadOnlyList<Cell> back))
            {
                return there.Count + back.Count;
            }
        }

        return 0;
    }

    /// <summary>The living enemy nearest the body inside one distance, or null when none stands that near.</summary>
    private Enemy? NearestLiving(SimulationLoop loop, float reach)
    {
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
    /// The living enemy that the policy walks to, or null when the floor holds none. The policy holds its target
    /// while that enemy lives and stays the nearest one, and it reads the distances again every
    /// <see cref="RepathTicks"/> ticks, because both bodies move (D-109).
    /// </summary>
    private Enemy? Hunted(SimulationLoop loop)
    {
        if (this.LivingAndReachable(loop) == 0)
        {
            if (this.target != NoTarget)
            {
                // The last enemy died. The path to it leads nowhere now, so the walk to the stairwell starts fresh.
                this.Forget();
            }

            return null;
        }

        Enemy? held = null;
        foreach (Enemy enemy in loop.Enemies)
        {
            if (enemy.Owner == this.target && !enemy.IsDead && !this.IsUnreachable(enemy.Owner))
            {
                held = enemy;
            }
        }

        if (held is not null)
        {
            return held;
        }

        Enemy? nearest = null;
        float best = 0.0f;
        foreach (Enemy enemy in loop.Enemies)
        {
            if (enemy.IsDead || this.IsUnreachable(enemy.Owner))
            {
                continue;
            }

            float distance = PathWalk.Distance(loop.Body.Position, enemy.Body.Position);
            if (nearest is null || distance < best)
            {
                nearest = enemy;
                best = distance;
            }
        }

        if (nearest is not null && nearest.Owner != this.target)
        {
            this.target = nearest.Owner;
            this.failed = 0;
            this.stalled = 0;
            this.nearest = PathWalk.Distance(loop.Body.Position, nearest.Body.Position);
            this.targetHealth = nearest.Health;
            this.follower.Forget();
        }

        return nearest;
    }

    /// <summary>Faces one enemy and presses the attack bit (D-323, D-324).</summary>
    private Intent Strike(SimulationLoop loop, Enemy prey)
    {
        int yaw = HumanoidBrain.YawToward(loop.Body.Position, prey.Body.Position);
        short yawDelta = BotIntent.TurnToward(loop.Yaw, yaw);

        // A swing starts on a press, so a held bit starts nothing. The policy releases the bit on every second
        // tick, and the next tick is a press again (D-323).
        ushort attack = this.attackHeld ? (ushort)0 : Button.Attack;
        this.attackHeld = !this.attackHeld;
        return new Intent(loop.Tick, yawDelta, 0, 0, 0, attack);
    }

    /// <summary>Drops the target and the path, so the next tick starts the hunt again.</summary>
    private void Forget()
    {
        this.follower.Forget();
        this.target = NoTarget;
        this.failed = 0;
        this.stalled = 0;
        this.attackHeld = false;
    }

    /// <summary>
    /// Answers whether the hunt gained nothing for <see cref="StalledTicks"/> ticks. A body that comes
    /// <see cref="Gain"/> meters nearer, or that takes health off the enemy, starts the count again.
    /// </summary>
    private bool Stalled(Enemy hunted, float distance)
    {
        if (distance < this.nearest - Gain || hunted.Health < this.targetHealth)
        {
            this.stalled = 0;
            this.nearest = distance;
            this.targetHealth = hunted.Health;
            return false;
        }

        this.stalled++;
        return this.stalled >= StalledTicks;
    }

    /// <summary>Walks toward the stairwell, and presses the stairwell choice on arrival.</summary>
    private Intent WalkToStairwell(SimulationLoop loop)
    {
        if (this.TryWalk(loop, loop.Plan.Stairwell, 0, loop.Yaw, out Intent walk))
        {
            return walk;
        }

        ushort choice = loop.Floor >= this.lastFloor ? Button.Ascend : Button.Interact;
        return new Intent(loop.Tick, 0, 0, 0, 0, choice);
    }

    /// <summary>
    /// One intent that walks the path toward a goal cell, or false when the follower has no next cell. A step up
    /// is a jump in place, then a move once the feet clear the step (D-165).
    /// </summary>
    private bool TryWalk(SimulationLoop loop, Cell goal, short yawDelta, int yawAfter, out Intent walk)
    {
        Vector3 feet = loop.Body.Position;
        bool onGround = loop.Body.IsOnGround();
        if (this.pathfinder is null || !this.follower.TryNext(loop.Grid, this.pathfinder, feet, onGround, goal, out Cell next))
        {
            // The box is 0.6 meters wide in cells of one meter, so a body can rest on the slope under one edge of
            // it with its own column open. No search starts from an open column, and the walk would then stand
            // still for the rest of the floor. A step straight at the goal with a jump takes the body off that
            // spot, and the search works again on the next tick.
            if (!this.follower.LastSearchFailed)
            {
                walk = new Intent(loop.Tick, yawDelta, 0, 0, 0, 0);
                return false;
            }

            Vector3 nudge = PathWalk.Toward(feet, PathWalk.CenterOf(goal), PlayerBody.WalkSpeed * PlayerBody.TickSeconds);
            ushort nudgeJump = onGround ? Button.Jump : (ushort)0;
            walk = BotIntent.Walk(loop.Tick, nudge, yawDelta, yawAfter, nudgeJump);
            return true;
        }

        // A step up jumps and walks on in the same tick, as a player does it. A jump in place rises and falls on
        // the same spot, and a step that a jump cannot clear then repeats for the rest of the floor (F-105). A body
        // wedged against a slope jumps for the same reason.
        bool hop = onGround && (PathWalk.NeedsAJump(loop.Grid, next, feet) || this.follower.IsWedged);
        ushort jump = hop ? Button.Jump : (ushort)0;
        Vector3 toward = PathWalk.Toward(feet, PathWalk.CenterOf(next), PlayerBody.WalkSpeed * PlayerBody.TickSeconds);
        walk = BotIntent.Walk(loop.Tick, toward, yawDelta, yawAfter, jump);
        return true;
    }

    /// <summary>The count of living enemies that the hunt did not drop as unreachable.</summary>
    private int LivingAndReachable(SimulationLoop loop)
    {
        int count = 0;
        foreach (Enemy enemy in loop.Enemies)
        {
            if (!enemy.IsDead && !this.IsUnreachable(enemy.Owner))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Answers whether the hunt dropped one enemy on this floor, because no path led to it.</summary>
    private bool IsUnreachable(int owner)
    {
        foreach (int dropped in this.unreachable)
        {
            if (dropped == owner)
            {
                return true;
            }
        }

        return false;
    }
}
