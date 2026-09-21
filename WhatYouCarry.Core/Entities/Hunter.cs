using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Ai;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Entities;

/// <summary>
/// The hunter of the timer: the Overseer (D-45, D-409). It spawns on the tick of expiry out of the sight of the
/// player, always knows where the player is, walks a path to the player at a speed that rises on every tick, and
/// swings its pick by the player melee rules. No hit takes its health.
/// </summary>
/// <remarks>
/// <para>
/// The body is the box of D-165, as the player and the humanoids have, until the model of PR-14 (D-423). The swing
/// and the stagger are the types that the player uses (D-31). A hit staggers the Overseer by the rules of D-326,
/// and its two-handed swing gives hyper-armor (D-29, D-414). It holds no health, so nothing can kill it.
/// </para>
/// <para>
/// The chase reads the player on every tick with no sight check and no give-up (D-416). Inside its swing range it
/// stands still and faces the player, and it swings when its cooldown is ready (D-425). Outside the range it walks
/// straight at the player over the last meters, and along a path of <see cref="GridPathfinder"/> before that, as a
/// humanoid does (F-104). It never retreats.
/// </para>
/// <para>
/// One tick runs in the order of <see cref="Enemy"/>: the cooldown, the move, the swing, and then the stagger.
/// The hunter draws nothing, so one grid and one player path give one chase everywhere (G-9).
/// </para>
/// </remarks>
public sealed class Hunter
{
    /// <summary>A velocity of no motion. The hunter gives it while it swings or waits.</summary>
    private static readonly Vector3 Still = new(0.0f, 0.0f, 0.0f);

    private readonly HunterDefinition definition;
    private readonly Swing swing;
    private readonly Stagger stagger = new();
    private readonly PathFollower follower = new();

    /// <summary>The hunter at rest on one floor cell.</summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="cell">The floor cell of the spawn (D-415). The feet center is the center of the cell above it.</param>
    /// <param name="definition">The hunter of the content set (D-409).</param>
    /// <param name="weapon">The weapon that the hunter names (D-413).</param>
    /// <param name="owner">The owner id of the hunter in the hits and the target boxes. The player is zero.</param>
    /// <exception cref="ContextException">The owner id is zero or below, or the box at the spawn overlaps a solid part.</exception>
    public Hunter(VoxelGrid grid, Cell cell, HunterDefinition definition, WeaponDefinition weapon, int owner)
    {
        if (owner < 1)
        {
            ContextException error = new($"The owner id of the hunter is one or more, and the player holds zero. The id is {owner}.");
            error.AddContext("owner", ((long)owner).ToString(CultureInfo.InvariantCulture));
            error.AddContext("hunter", definition.Id);
            throw error;
        }

        this.Body = new PlayerBody(grid, new Vector3(cell.X + 0.5f, cell.Y + 1.0f, cell.Z + 0.5f));
        this.definition = definition;
        this.swing = new Swing(weapon);
        this.Owner = owner;
        this.Spawn = cell;
    }

    /// <summary>The owner id in the hits and the target boxes (D-325).</summary>
    public int Owner { get; }

    /// <summary>The hunter of the content set (D-409).</summary>
    public HunterDefinition Definition => this.definition;

    /// <summary>The floor cell that the hunter spawned on (D-415).</summary>
    public Cell Spawn { get; }

    /// <summary>The body: the box, the position, and the vertical velocity (D-165, D-423).</summary>
    public PlayerBody Body { get; }

    /// <summary>The way the hunter faces, in hundredths of a degree, as the yaw of the loop (D-227, D-234).</summary>
    public int Yaw { get; private set; }

    /// <summary>The ticks until the next swing can start. Zero means ready (D-413).</summary>
    public int AttackCooldown { get; private set; }

    /// <summary>The ticks of the swing that ran, from zero, or <see cref="Swing.NoSwing"/>.</summary>
    public long SwingTick => this.swing.Tick;

    /// <summary>Answers whether a swing runs now.</summary>
    public bool IsSwinging => this.swing.IsSwinging;

    /// <summary>The ticks of the stagger that remain. Zero means no stagger (D-326, D-414).</summary>
    public int StaggerRemaining => this.stagger.Remaining;

    /// <summary>The speed of the last tick, in meters per second (D-408, D-423).</summary>
    public float Speed { get; private set; }

    /// <summary>The hits of the swing on the last tick, in target order. The loop reads them. They are not state.</summary>
    public IReadOnlyList<SwordHit> LastHits => this.swing.LastHits;

    /// <summary>The target box of the hunter, which the blade of the player can meet (D-414).</summary>
    public EntityBox TargetBox => new(this.Owner, this.Body.Box);

    /// <summary>
    /// The spawn cell of the hunter (D-415): of the floor cells that the player reaches, the one farthest from the
    /// player by path that the player cannot see, and from which a path leads back to the player. Two cells of one
    /// distance go in scan order: x fastest, then z, then y.
    /// </summary>
    /// <remarks>
    /// The move rule allows any drop and one block up, so a path from the player to a cell does not prove a path
    /// back. The search then tries the cells from the farthest one, and takes the first from which the pathfinder
    /// finds the player.
    /// </remarks>
    /// <exception cref="ContextException">The player stands over no floor cell, or no floor cell out of the sight of the player has a path to the player (D-415, T-2).</exception>
    public static Cell FindSpawn(VoxelGrid grid, GridPathfinder pathfinder, Vector3 playerFeet)
    {
        Cell playerCell = GroundUnder(grid, playerFeet);
        Reachability reach = Reachability.From(grid, playerCell);
        List<SpawnCandidate> candidates = [];
        for (int y = 0; y < grid.SizeY; y++)
        {
            for (int z = 0; z < grid.SizeZ; z++)
            {
                for (int x = 0; x < grid.SizeX; x++)
                {
                    Cell cell = new(x, y, z);
                    if (!reach.IsReachable(cell))
                    {
                        continue;
                    }

                    Vector3 feet = new(x + 0.5f, y + 1.0f, z + 0.5f);
                    if (PathWalk.Sees(grid, playerFeet, feet))
                    {
                        continue;
                    }

                    candidates.Add(new SpawnCandidate(cell, reach.Distance(cell), candidates.Count));
                }
            }
        }

        // The order is total, so the sort that Core approves gives one list on every platform (D-207).
        candidates.Sort(FartherFirst);
        foreach (SpawnCandidate candidate in candidates)
        {
            if (pathfinder.TryFind(candidate.Cell, playerCell, out IReadOnlyList<Cell> _))
            {
                return candidate.Cell;
            }
        }

        ContextException error = new($"The floor holds no floor cell out of the sight of the player at {playerCell} with a path to the player, and the Overseer spawns out of sight alone (D-415).");
        error.AddContext("playerCell", playerCell.ToString());
        error.AddContext("hiddenCells", ((long)candidates.Count).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>
    /// Runs one tick: the choice of the chase, then the tick of the body, the swing, and the stagger (D-416, D-425).
    /// </summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="pathfinder">The pathfinder of the floor, which every brain of the floor shares (G-4).</param>
    /// <param name="playerFeet">The feet center of the player, in meters (D-235).</param>
    /// <param name="ticksAfterExpiry">The ticks after the expiry of the floor timer, which set the speed (D-408).</param>
    /// <param name="targets">The boxes that the blade of the hunter can hit on this tick.</param>
    public void Step(VoxelGrid grid, GridPathfinder pathfinder, Vector3 playerFeet, long ticksAfterExpiry, IReadOnlyList<EntityBox> targets)
    {
        this.Speed = this.definition.SpeedMetresPerSecond(ticksAfterExpiry);
        Vector3 feet = this.Body.Position;
        int facePlayer = HumanoidBrain.YawToward(feet, playerFeet);

        // A swing runs to its end where it started, so the blade meets what the windup aimed at (D-324).
        if (this.swing.IsSwinging)
        {
            this.Run(Still, false, this.Yaw, targets);
            return;
        }

        if (PathWalk.Distance(feet, playerFeet) <= this.definition.AttackRangeMetres)
        {
            if (this.CanSwing)
            {
                this.swing.Start();
            }

            this.Run(Still, false, facePlayer, targets);
            return;
        }

        if (PathWalk.Sees(grid, feet, playerFeet) && PathWalk.CanWalkStraight(grid, feet, playerFeet))
        {
            Vector3 straight = PathWalk.Toward(feet, playerFeet, this.Speed * PlayerBody.TickSeconds) * this.Speed;
            this.Run(straight, false, facePlayer, targets);
            return;
        }

        bool onGround = this.Body.IsOnGround();
        if (!this.follower.TryNext(grid, pathfinder, feet, onGround, PathWalk.FloorCellOf(playerFeet), out Cell next))
        {
            // The walk is over, or no search could run. The hunter then steps straight at the player, so the
            // pressure holds while the search has no answer, as the hunt of a humanoid does.
            Vector3 push = PathWalk.Toward(feet, playerFeet, this.Speed * PlayerBody.TickSeconds) * this.Speed;
            this.Run(push, false, facePlayer, targets);
            return;
        }

        if (PathWalk.NeedsAJump(grid, next, feet))
        {
            this.Run(Still, onGround, facePlayer, targets);
            return;
        }

        Vector3 toward = PathWalk.Toward(feet, PathWalk.CenterOf(next), this.Speed * PlayerBody.TickSeconds) * this.Speed;
        this.Run(toward, false, facePlayer, targets);
    }

    /// <summary>
    /// Takes one hit (D-414). The damage takes no health, because the hunter holds none. The hit staggers the hunter
    /// and cancels a swing, unless a stagger or its guard holds or the two-handed swing gives hyper-armor (D-29,
    /// D-326). A stagger that cancels a swing starts the attack cooldown, as it does for a humanoid (D-402).
    /// </summary>
    /// <exception cref="ContextException">The damage is below zero.</exception>
    public void TakeHit(long damage)
    {
        if (damage < 0)
        {
            ContextException negative = new($"A hit deals a damage of zero or more, and the damage is {damage}.");
            negative.AddContext("damage", damage.ToString(CultureInfo.InvariantCulture));
            negative.AddContext("owner", ((long)this.Owner).ToString(CultureInfo.InvariantCulture));
            throw negative;
        }

        if (this.stagger.TryStart(this.swing.HasHyperArmor))
        {
            this.swing.Cancel();
            this.AttackCooldown = (int)this.definition.AttackCooldownTicks;
        }
    }

    /// <summary>
    /// Folds the hunter into the hash, in the declared order (D-160): the spawn cell, the position, the vertical
    /// velocity, the yaw, the attack cooldown, the swing, the stagger, and the follower.
    /// </summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Spawn.X);
        hash.Add(this.Spawn.Y);
        hash.Add(this.Spawn.Z);
        hash.Add(this.Body.Position.X);
        hash.Add(this.Body.Position.Y);
        hash.Add(this.Body.Position.Z);
        hash.Add(this.Body.VerticalVelocity);
        hash.Add(this.Yaw);
        hash.Add(this.AttackCooldown);
        this.swing.AddTo(ref hash);
        this.stagger.AddTo(ref hash);
        this.follower.AddTo(ref hash);
    }

    /// <summary>Answers whether a swing can start now: no swing runs, the cooldown is ready, and no stagger holds (D-413).</summary>
    private bool CanSwing => !this.swing.IsSwinging && this.AttackCooldown == 0 && !this.stagger.Holds;

    /// <summary>
    /// The floor cell under a feet center: the cell that the ground probe reads, or the first floor cell below it
    /// when the body is in the air.
    /// </summary>
    /// <exception cref="ContextException">No floor cell lies under the feet (T-2).</exception>
    private static Cell GroundUnder(VoxelGrid grid, Vector3 feet)
    {
        Cell probe = PathWalk.FloorCellOf(feet);
        for (int y = probe.Y; y >= 0; y--)
        {
            Cell cell = new(probe.X, y, probe.Z);
            if (GridMoves.IsFloor(grid, cell))
            {
                return cell;
            }
        }

        ContextException error = new($"No floor cell lies under the player at {probe}, and the Overseer spawn search starts from the floor cell of the player (D-415).");
        error.AddContext("probe", probe.ToString());
        throw error;
    }

    /// <summary>
    /// Runs the body, the swing, and the stagger for one tick, in the order of <see cref="Enemy"/>. A stagger holds
    /// the body still. A swing that ends starts the cooldown.
    /// </summary>
    private void Run(Vector3 horizontal, bool jump, int yaw, IReadOnlyList<EntityBox> targets)
    {
        this.Yaw = yaw;
        bool staggered = this.stagger.Holds;
        if (this.AttackCooldown > 0)
        {
            this.AttackCooldown--;
        }

        bool inWater = this.Body.IsInWater();
        if (staggered)
        {
            this.Body.Move(Still, false, inWater, true);
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

    /// <summary>The order of the spawn search: the larger distance first, and then the scan order.</summary>
    private static int FartherFirst(SpawnCandidate first, SpawnCandidate second)
    {
        if (first.Distance != second.Distance)
        {
            return first.Distance > second.Distance ? -1 : 1;
        }

        if (first.ScanOrder != second.ScanOrder)
        {
            return first.ScanOrder < second.ScanOrder ? -1 : 1;
        }

        return 0;
    }

    /// <summary>One cell that the spawn search can take, with its distance from the player and its place in the scan.</summary>
    private readonly record struct SpawnCandidate(Cell Cell, int Distance, int ScanOrder);
}
