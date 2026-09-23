using System.Collections.Generic;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Pathfinding;

/// <summary>
/// One body that follows a path of <see cref="GridPathfinder"/> toward a goal cell that moves: it holds the path,
/// the waypoint that it walks to, and the ticks since the last search.
/// </summary>
/// <remarks>
/// <para>
/// Three walkers follow a moving goal (D-111): the humanoid brain, which walks to the player, and the greedy
/// descender and the full clearer of the bots. One copy of the rules keeps the three alike, and a defect of the
/// walk has one place to live.
/// </para>
/// <para>
/// A search costs work, so the follower searches again only when the goal cell moved, the path ran out, or the
/// body drifted off the path, and never more than once every <see cref="RepathTicks"/> ticks (D-109).
/// </para>
/// <para>
/// A new search starts at the cell that the body walks into, and not at the cell that it stands in (F-104). A body
/// that crosses one cell in about the ticks of one search lies between two cells at every search. Two paths of one
/// length then lead from the two cells, each through the other, and the body swings between them and never
/// arrives. A search from the cell ahead keeps the step that runs, so every search adds to the walk.
/// </para>
/// <para>
/// A body can wedge against the slope of a ramp and come no nearer its goal with a clear path ahead of it
/// (F-105). <see cref="IsWedged"/> reads that state after <see cref="WedgedTicks"/> ticks with no gain, and a
/// caller answers it with a jump, which lifts the body off the slope. An arrival at a waypoint is a gain, so a
/// detour away from the goal reads no wedge (F-111).
/// </para>
/// </remarks>
public sealed class PathFollower
{
    /// <summary>The fewest ticks between two searches of one follower (D-109).</summary>
    public const int RepathTicks = 15;

    /// <summary>The ticks with no gain on the goal after which the walk counts as wedged: four seconds.</summary>
    public const int WedgedTicks = 240;

    /// <summary>The best estimate of a follower that has walked toward no goal yet.</summary>
    private const int NoBest = -1;

    private IReadOnlyList<Cell> path = [];
    private int waypoint;
    private int sincePath = RepathTicks;
    private Cell goalOfBest;
    private int best = NoBest;
    private int stillTicks;

    /// <summary>The path that the follower walks now, in walk order, or empty.</summary>
    public IReadOnlyList<Cell> Path => this.path;

    /// <summary>The place in the path of the cell that the body walks to.</summary>
    public int Waypoint => this.waypoint;

    /// <summary>Answers whether the last search ran and found no path to the goal. A search that did not run leaves it as it was.</summary>
    public bool LastSearchFailed { get; private set; }

    /// <summary>Answers whether the body came no nearer its goal and arrived at no waypoint for <see cref="WedgedTicks"/> ticks, which a walk never does on open ground (F-105, F-111).</summary>
    public bool IsWedged => this.stillTicks >= WedgedTicks;

    /// <summary>Drops the path, so the next call searches a new one.</summary>
    public void Forget()
    {
        this.path = [];
        this.waypoint = 0;
        this.sincePath = RepathTicks;
        this.LastSearchFailed = false;
        this.stillTicks = 0;
        this.best = NoBest;
    }

    /// <summary>
    /// Counts the ticks since the body last came nearer its goal, in the octile distance that
    /// <see cref="GridPathfinder.Estimate"/> gives. A goal that moved starts the count again.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A count of the ticks in one cell reads no wedge when the body swings over a cell face, because its cell
    /// then changes on every second tick. The estimate to the goal falls only when the body takes ground.
    /// </para>
    /// <para>
    /// A path can lead away from the goal for more than four seconds, around a wall or down a long ramp. The
    /// estimate then does not fall, so an arrival at a waypoint also starts the count again (D-546). A body that a
    /// slope wedges arrives at no waypoint (F-111).
    /// </para>
    /// </remarks>
    private void ReadGain(Cell cell, Cell goal)
    {
        int estimate = GridPathfinder.Estimate(cell, goal);
        if (this.best == NoBest || goal != this.goalOfBest || estimate < this.best)
        {
            this.best = estimate;
            this.goalOfBest = goal;
            this.stillTicks = 0;
            return;
        }

        this.stillTicks++;
    }

    /// <summary>
    /// The cell that the body walks to on this tick, or false when it has none: the path ran out, no move rule path
    /// reaches the goal, or the body stands where no search can start.
    /// </summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="pathfinder">The pathfinder of the floor.</param>
    /// <param name="feet">The feet center of the body, in meters (D-235).</param>
    /// <param name="onGround">Whether the body stands on the ground. A body in the air searches nothing.</param>
    /// <param name="goal">The floor cell to walk to.</param>
    /// <param name="next">The cell to walk to on this tick.</param>
    public bool TryNext(VoxelGrid grid, GridPathfinder pathfinder, Vector3 feet, bool onGround, Cell goal, out Cell next)
    {
        if (this.sincePath < RepathTicks)
        {
            this.sincePath++;
        }

        this.ReadGain(PathWalk.FloorCellOf(feet), goal);

        this.PlanIfNeeded(grid, pathfinder, feet, onGround, goal);

        while (this.waypoint < this.path.Count && PathWalk.Arrived(feet, onGround, this.path[this.waypoint]))
        {
            // An arrival is a gain along the path, also on a detour that takes the body away from the goal (F-111).
            this.waypoint++;
            this.stillTicks = 0;
        }

        if (this.waypoint >= this.path.Count)
        {
            next = default;
            return false;
        }

        next = this.path[this.waypoint];
        return true;
    }

    /// <summary>Folds the follower into the hash, in the declared order (D-160): the waypoint and the ticks since the last search.</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.waypoint);
        hash.Add(this.sincePath);
    }

    /// <summary>
    /// Searches a new path when the follower needs one: the goal moved, the path ran out, or the body drifted off
    /// the path, and at least <see cref="RepathTicks"/> ticks passed. A body in the air searches nothing, because
    /// it stands on no cell that a path can start from, and a goal that is no floor cell is no cell to walk to.
    /// </summary>
    private void PlanIfNeeded(VoxelGrid grid, GridPathfinder pathfinder, Vector3 feet, bool onGround, Cell goal)
    {
        // The body is on its path when it stands in the cell that it walks to, or in the cell before it. Anything
        // else is a drift: a fall, a roll, or a shove took it off the path (F-105). The row is part of that test,
        // because a body that dropped two rows stands under a path that it can no longer walk.
        Cell here = PathWalk.FloorCellOf(feet);
        bool exhausted = this.waypoint >= this.path.Count;
        bool entering = !exhausted && this.waypoint > 0 && here == this.path[this.waypoint - 1];
        bool onWaypoint = !exhausted && here == this.path[this.waypoint];
        bool goalMoved = this.path.Count == 0 || this.path[this.path.Count - 1] != goal;
        bool adrift = !exhausted && !entering && !onWaypoint;
        if ((!goalMoved && !exhausted && !adrift) || this.sincePath < RepathTicks || !onGround)
        {
            return;
        }

        // A search from the cell ahead keeps the step that runs, so every search adds to the walk.
        Cell start = entering ? this.path[this.waypoint] : here;
        if (!GridMoves.IsFloor(grid, start) || !GridMoves.IsFloor(grid, goal))
        {
            this.sincePath = 0;
            this.LastSearchFailed = true;
            return;
        }

        this.sincePath = 0;
        this.LastSearchFailed = !pathfinder.TryFind(start, goal, out IReadOnlyList<Cell> found);
        this.path = found;

        // A path that starts at the cell ahead holds the step that runs, so the walk takes that cell first. A path
        // that starts where the body stands would walk it back to the center of its own cell, which costs ticks
        // and takes no ground, so the walk takes the cell after it.
        bool startsHere = found.Count > 1 && found[0] == here;
        this.waypoint = startsHere ? 1 : 0;
    }

}
