using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhatYouCarry.Core.Ai;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The walk of an enemy along a path: the climb of a ramp with no jump (F-108) and the diagonal move (F-107).</summary>
public sealed class EnemyWalkTests
{
    /// <summary>The seed and the floor where the owner saw an enemy climb a ramp in slow jumps (D-485).</summary>
    private const ulong OwnerSeed = 1;

    /// <summary>The most ticks that a walk may take for each cell of its path. A walk at 3.5 meters per second takes about 17.</summary>
    private const int TicksPerCell = 30;

    /// <summary>The owner id of the test enemy. The player holds zero.</summary>
    private const int TestOwner = 50;

    /// <summary>The seeds of the diagonal walk sweep. The 120 floors hold about 430000 diagonal moves.</summary>
    private const int DiagonalSeeds = 120;

    /// <summary>The most ticks that one diagonal move may take. The slowest move of the sweep takes under 80.</summary>
    private const int DiagonalTicks = 120;

    /// <summary>Every rise with every run of D-346.</summary>
    public static TheoryData<RampRise, int> Courses => RampCourse.EveryRiseAndRun();

    /// <summary>
    /// PR-72 exit test 1, the case of D-485. On seed 1, floor 1, a scavenger and the Overseer each walk from the low
    /// end of every ramp to the high end with the player there, and neither leaves the ground on any tick. The old
    /// jump rule read the feet against the face of the next place, so each place of a climb gave a jump (F-108).
    /// </summary>
    [Fact]
    public void EnemiesClimbTheRampsOfTheOwnerFloorWithNoJump()
    {
        SimulationLoop loop = new(OwnerSeed, TestWorld.Content);
        Assert.Equal(SimulationLoop.FirstFloor, loop.Floor);
        Assert.NotEmpty(loop.Plan.Ramps);
        GridPathfinder finder = new(loop.Grid);
        EnemyDefinition scavenger = Family("scavenger");
        HunterDefinition overseer = TestWorld.Content.Hunter;

        for (int index = 0; index < loop.Plan.Ramps.Count; index++)
        {
            DugRamp ramp = loop.Plan.Ramps[index];
            Assert.True(finder.TryFind(ramp.LowEnd, ramp.HighEnd, out IReadOnlyList<Cell> path), $"Ramp {index}: no path joins {ramp.LowEnd} and {ramp.HighEnd}.");
            int limit = TicksPerCell * path.Count;
            Vector3 player = PathWalk.CenterOf(ramp.HighEnd);

            Enemy enemy = new(loop.Grid, PathWalk.CenterOf(ramp.LowEnd), scavenger, Weapon(scavenger.Weapon), TestOwner);
            HumanoidBrain brain = new(enemy, ramp.LowEnd, true);
            int ticks = 0;
            while (PathWalk.Distance(enemy.Body.Position, player) > scavenger.AttackRangeMetres)
            {
                Assert.True(ticks < limit, $"Ramp {index}: the scavenger did not reach {ramp.HighEnd} in {limit} ticks, and stands at {enemy.Body.Position}.");
                brain.Step(loop.Grid, finder, player, []);
                Assert.True(enemy.Body.IsOnGround(), $"Ramp {index}, tick {ticks}: the scavenger left the ground at {enemy.Body.Position}.");
                ticks++;
            }

            Hunter hunter = new(loop.Grid, ramp.LowEnd, overseer, Weapon(overseer.Weapon), TestOwner);
            ticks = 0;
            while (PathWalk.Distance(hunter.Body.Position, player) > overseer.AttackRangeMetres)
            {
                Assert.True(ticks < limit, $"Ramp {index}: the Overseer did not reach {ramp.HighEnd} in {limit} ticks, and stands at {hunter.Body.Position}.");
                hunter.Step(loop.Grid, finder, player, 0, []);
                Assert.True(hunter.Body.IsOnGround(), $"Ramp {index}, tick {ticks}: the Overseer left the ground at {hunter.Body.Position}.");
                ticks++;
            }
        }
    }

    /// <summary>
    /// PR-72 exit test 2. A walk along a ramp needs no jump, up or down, from the middle of each cell to the next
    /// cell, for every rise and run. The slope carries the body up to the face of the next place (F-108).
    /// </summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void AWalkAlongARampNeedsNoJump(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        int across = 4;
        for (int along = RampCourse.RampStart - 2; along <= course.HighStart; along++)
        {
            Cell here = CourseFloor(course, along, across);
            Cell above = CourseFloor(course, along + 1, across);
            Vector3 feet = course.Point(along + 0.5f, SurfaceAt(course, along + 0.5f), RampCourse.Middle);
            Assert.False(PathWalk.NeedsAJump(course.Grid, above, feet), $"Rise {rise}, run {run}: the walk up from {here} to {above} asks for a jump.");

            Vector3 upperFeet = course.Point(along + 1.5f, SurfaceAt(course, along + 1.5f), RampCourse.Middle);
            Assert.False(PathWalk.NeedsAJump(course.Grid, here, upperFeet), $"Rise {rise}, run {run}: the walk down from {above} to {here} asks for a jump.");
        }
    }

    /// <summary>
    /// PR-72 exit test 2. A step onto a block and a step onto the side of a ramp each need a jump (D-165). The fix of
    /// F-108 reads the slope under the feet, and a flat floor under the feet gives the old answer.
    /// </summary>
    [Fact]
    public void AStepOntoABlockOrTheSideOfARampNeedsAJump()
    {
        VoxelGrid grid = TestWorld.FlatFloor(10, 6);
        grid.Set(5, 1, 3, BlockId.RawStone);
        for (int place = 0; place < 4; place++)
        {
            grid.Set(3 + place, 1, 6, new Ramp(RampRise.PlusX, 4, place).Id);
        }

        Vector3 besideBlock = PathWalk.CenterOf(new Cell(4, 0, 3));
        Assert.True(PathWalk.NeedsAJump(grid, new Cell(5, 1, 3), besideBlock));

        // The side of the ramp: the floor beside it lies at the foot of the slope, and the slope over the middle
        // of each place stands higher.
        for (int place = 0; place < 4; place++)
        {
            Vector3 besideRamp = PathWalk.CenterOf(new Cell(3 + place, 0, 5));
            Assert.True(PathWalk.NeedsAJump(grid, new Cell(3 + place, 1, 6), besideRamp), $"Place {place}: a step onto the side of the ramp asks for no jump.");
        }
    }

    /// <summary>
    /// PR-72 exit test 3. A hunting brain walks from the low floor of a course to the player on the high floor for
    /// every rise and run, and it never leaves the ground (F-108).
    /// </summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ABrainClimbsARampWithNoJump(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        Cell low = course.CellAt(2, 0, 4);
        Cell high = course.CellAt(course.HighStart + 3, 1, 4);
        EnemyDefinition scavenger = Family("scavenger");
        Enemy enemy = new(course.Grid, PathWalk.CenterOf(low), scavenger, Weapon(scavenger.Weapon), TestOwner);
        HumanoidBrain brain = new(enemy, low, true);
        GridPathfinder finder = new(course.Grid);
        Vector3 player = PathWalk.CenterOf(high);

        int ticks = 0;
        while (PathWalk.Distance(enemy.Body.Position, player) > scavenger.AttackRangeMetres)
        {
            Assert.True(ticks < TicksPerCell * RampCourse.Length, $"Rise {rise}, run {run}: the brain did not reach the high floor, and stands at {enemy.Body.Position}.");
            brain.Step(course.Grid, finder, player, []);
            Assert.True(enemy.Body.IsOnGround(), $"Rise {rise}, run {run}, tick {ticks}: the brain left the ground at {enemy.Body.Position}.");
            ticks++;
        }
    }

    /// <summary>
    /// PR-72 exit test 4. On open floor, a diagonal move reaches the corner column in both orders, and it keeps the
    /// row (D-486).
    /// </summary>
    [Fact]
    public void ADiagonalMoveCrossesOpenFloor()
    {
        VoxelGrid grid = TestWorld.FlatFloor(8, 6);
        for (int corner = 0; corner < GridMoves.Corners; corner++)
        {
            int stepX = GridMoves.CornerStepX[corner];
            int stepZ = GridMoves.CornerStepZ[corner];
            Assert.Equal(0, GridMoves.DiagonalMove(grid, 4, 0, 4, stepX, stepZ, true));
            Assert.Equal(0, GridMoves.DiagonalMove(grid, 4, 0, 4, stepX, stepZ, false));
        }
    }

    /// <summary>
    /// PR-72 exit test 4. A diagonal move passes one solid corner, and the body slides along the block edge as a
    /// player does. Two solid corners shut the line at the height of the body, so no diagonal move passes between
    /// them (D-486).
    /// </summary>
    [Fact]
    public void ADiagonalMovePassesOneSolidCornerAndNotTwo()
    {
        VoxelGrid grid = TestWorld.FlatFloor(8, 6);
        Pillar(grid, 5, 4);

        // The pillar stands in the side column along X, so the order along X meets it, and the order along Z
        // passes it.
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, true));
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, false));

        Pillar(grid, 4, 5);
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, true));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, false));

        // The other three corners stay open.
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 4, 0, 4, -1, -1, true));
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, -1, false));
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 4, 0, 4, -1, 1, true));
    }

    /// <summary>
    /// PR-72 exit test 4. A diagonal move steps up one block and drops any height, as two side moves do (D-165,
    /// D-486). A step of two blocks is no move in either order, also where each of the two side moves rises one
    /// block, because one jump clears one block alone.
    /// </summary>
    [Fact]
    public void ADiagonalMoveStepsUpOneBlockAndDrops()
    {
        VoxelGrid grid = TestWorld.FlatFloor(8, 8);
        grid.Set(5, 1, 5, BlockId.RawStone);
        Assert.Equal(1, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, true));
        Assert.Equal(1, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, false));
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 5, 1, 5, -1, -1, true));
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 5, 1, 5, -1, -1, false));

        grid.Set(5, 2, 5, BlockId.RawStone);
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, true));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, false));

        // A block in the side column along X makes two steps of one block: onto the side block, then onto the
        // corner column. Each side move is legal, and the diagonal move is not.
        grid.Set(5, 1, 4, BlockId.RawStone);
        Assert.Equal(1, GridMoves.Move(grid, 4, 0, 4, 5, 4));
        Assert.Equal(2, GridMoves.Move(grid, 5, 1, 4, 5, 5));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, true));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, false));
    }

    /// <summary>
    /// PR-72 exit test 4. A diagonal step up passes no solid corner, because a slide during the jump lands the body
    /// short (D-489). A drop still passes one solid corner (D-486).
    /// </summary>
    [Fact]
    public void ADiagonalStepUpPassesNoSolidCorner()
    {
        VoxelGrid grid = TestWorld.FlatFloor(8, 8);
        grid.Set(5, 1, 5, BlockId.RawStone);
        Pillar(grid, 5, 4);

        // Each order of side moves is legal: along Z, then a step up onto the block.
        Assert.Equal(0, GridMoves.Move(grid, 4, 0, 4, 4, 5));
        Assert.Equal(1, GridMoves.Move(grid, 4, 0, 5, 5, 5));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, true));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 0, 4, 1, 1, false));

        // The drop from the block back past the pillar is legal.
        Assert.Equal(0, GridMoves.DiagonalMove(grid, 5, 1, 5, -1, -1, true));
    }

    /// <summary>
    /// PR-72 exit test 4. The floor of a diagonal landing stands one block over the floor under the middle of the start
    /// at most (D-165). Here each order climbs a ramp into its next row and steps onto a block there. The block top
    /// stands 1.25 blocks over the slope under the body, so no diagonal move leads there.
    /// </summary>
    [Fact]
    public void ADiagonalMoveCutsOutNoClimbOfARamp()
    {
        VoxelGrid grid = TestWorld.FlatFloor(10, 8);
        grid.Set(3, 1, 4, new Ramp(RampRise.PlusX, 2, 0).Id);
        grid.Set(4, 1, 4, new Ramp(RampRise.PlusX, 2, 1).Id);
        grid.Set(5, 1, 4, BlockId.RawStone);
        grid.Set(5, 2, 4, new Ramp(RampRise.PlusX, 2, 0).Id);
        grid.Set(4, 1, 5, BlockId.RawStone);
        grid.Set(5, 1, 5, BlockId.RawStone);
        grid.Set(5, 2, 5, BlockId.RawStone);

        // Each side move is legal: up the ramp into its next row, onto the block beside the ramp, and up a step.
        Assert.Equal(2, GridMoves.Move(grid, 4, 1, 4, 5, 4));
        Assert.Equal(2, GridMoves.Move(grid, 5, 2, 4, 5, 5));
        Assert.Equal(1, GridMoves.Move(grid, 4, 1, 4, 4, 5));
        Assert.Equal(2, GridMoves.Move(grid, 4, 1, 5, 5, 5));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 1, 4, 1, 1, true));
        Assert.Equal(GridMoves.NoMove, GridMoves.DiagonalMove(grid, 4, 1, 4, 1, 1, false));
    }

    /// <summary>
    /// PR-72 exit test 5. Across open floor, the search gives a straight line of diagonal moves at the cost of
    /// the octile distance, and not a staircase of side moves (F-107, D-487).
    /// </summary>
    [Fact]
    public void TheSearchTakesDiagonalMovesAcrossOpenFloor()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        GridPathfinder finder = new(grid);
        Cell start = new(2, 0, 2);
        Cell goal = new(12, 0, 7);
        Assert.True(finder.TryFind(start, goal, out IReadOnlyList<Cell> path));

        // Five steps along both X and Z, then five along X alone: 5 diagonal moves and 5 side moves.
        Assert.Equal(11, path.Count);
        Assert.Equal((5 * GridPathfinder.DiagonalCost) + (5 * GridPathfinder.SideCost), GridPathfinder.Estimate(start, goal));
        Assert.Equal(GridPathfinder.Estimate(start, goal), PathCost(path));
    }

    /// <summary>
    /// PR-72 exit test 6, a class of its own in the sweep category, so it runs beside the other sweeps (D-478).
    /// </summary>
    [Trait("Category", SweepScope.SweepCategory)]
    public sealed class DiagonalSweep
    {
        /// <summary>
        /// On 120 seeds over every floor, a body that walks by the shared rules of <see cref="PathFollower"/> and
        /// <see cref="PathWalk"/>, as the humanoid brain and the Overseer do, crosses every diagonal move that the rule
        /// names from every floor cell, with one jump at most. The rule then names no move that a body cannot make
        /// (D-165, D-486, D-489). The sweep holds each kind of diagonal move, so no kind passes with no case.
        /// </summary>
        [Fact]
        public void ABodyWalksEveryDiagonalMove()
        {
            int deepest = FloorGenerator.DeepestFloor(TestWorld.Content);
            ConcurrentBag<string> failures = [];
            int[] counts = new int[CountKinds];
            Parallel.For(1, DiagonalSeeds + 1, index =>
            {
                int floor = 1 + (index % deepest);
                int[] floorCounts = SweepOneFloor((ulong)index, floor, failures);
                for (int kind = 0; kind < CountKinds; kind++)
                {
                    Interlocked.Add(ref counts[kind], floorCounts[kind]);
                }
            });

            Assert.True(failures.IsEmpty, string.Join("\n", failures));
            Assert.True(counts[MoveCount] > 100000, $"The sweep read {counts[MoveCount]} diagonal moves, and it needs more than 100000.");
            Assert.True(counts[RiseCount] > 0, "The sweep found no diagonal step up.");
            Assert.True(counts[DropCount] > 0, "The sweep found no diagonal drop.");
            Assert.True(counts[RampCount] > 0, "The sweep found no diagonal move on a ramp.");
            Assert.True(counts[CornerCount] > 0, "The sweep found no diagonal move past a solid corner.");
        }
    }

    /// <summary>The count of kinds that <see cref="SweepOneFloor"/> counts.</summary>
    private const int CountKinds = 5;

    /// <summary>The place of the count of every diagonal move.</summary>
    private const int MoveCount = 0;

    /// <summary>The place of the count of the diagonal steps up.</summary>
    private const int RiseCount = 1;

    /// <summary>The place of the count of the diagonal drops.</summary>
    private const int DropCount = 2;

    /// <summary>The place of the count of the diagonal moves from or onto a ramp.</summary>
    private const int RampCount = 3;

    /// <summary>The place of the count of the diagonal moves past a solid corner.</summary>
    private const int CornerCount = 4;

    /// <summary>
    /// Walks every diagonal move of one floor, adds a line to the failures for each move that a body does not cross
    /// in <see cref="DiagonalTicks"/> ticks with one jump at most, and gives the counts of the kinds of move.
    /// </summary>
    private static int[] SweepOneFloor(ulong seed, int floor, ConcurrentBag<string> failures)
    {
        EnemyDefinition scavenger = Family("scavenger");
        WeaponDefinition weapon = Weapon(scavenger.Weapon);
        VoxelGrid grid = FloorGenerator.Generate(seed, floor, TestWorld.Content).Grid;
        GridPathfinder finder = new(grid);
        int[] counts = new int[CountKinds];
        foreach (Cell from in FloorCells(grid))
        {
            for (int corner = 0; corner < GridMoves.Corners; corner++)
            {
                int stepX = GridMoves.CornerStepX[corner];
                int stepZ = GridMoves.CornerStepZ[corner];
                int alongXFirst = GridMoves.DiagonalMove(grid, from.X, from.Y, from.Z, stepX, stepZ, true);
                int alongZFirst = GridMoves.DiagonalMove(grid, from.X, from.Y, from.Z, stepX, stepZ, false);
                List<int> rows = [];
                if (alongXFirst != GridMoves.NoMove)
                {
                    rows.Add(alongXFirst);
                }

                if (alongZFirst != GridMoves.NoMove && alongZFirst != alongXFirst)
                {
                    rows.Add(alongZFirst);
                }

                foreach (int row in rows)
                {
                    Cell to = new(from.X + stepX, row, from.Z + stepZ);
                    (int ticks, int jumps) = WalkOneMove(grid, finder, scavenger, weapon, from, to);
                    if (ticks > DiagonalTicks || jumps > 1)
                    {
                        failures.Add($"Seed {seed}, floor {floor}: the diagonal move from {from} to {to} took {ticks} ticks and {jumps} jumps, and the limits are {DiagonalTicks} ticks and one jump (D-165).");
                    }

                    int higher = row > from.Y ? row : from.Y;
                    bool solidAlongX = grid.IsSolid(to.X, higher + 1, from.Z) || grid.IsSolid(to.X, higher + 2, from.Z);
                    bool solidAlongZ = grid.IsSolid(from.X, higher + 1, to.Z) || grid.IsSolid(from.X, higher + 2, to.Z);
                    bool onRamp = grid.TryGetRamp(from.X, from.Y, from.Z, out _) || grid.TryGetRamp(to.X, to.Y, to.Z, out _);
                    counts[MoveCount]++;
                    counts[RiseCount] += row > from.Y ? 1 : 0;
                    counts[DropCount] += row < from.Y ? 1 : 0;
                    counts[RampCount] += onRamp ? 1 : 0;
                    counts[CornerCount] += solidAlongX || solidAlongZ ? 1 : 0;
                }
            }
        }

        return counts;
    }

    /// <summary>
    /// Walks a body from the middle of one floor cell to the middle of another by the rules of the humanoid brain:
    /// a jump in place when the next cell needs one, and a walk at the next cell otherwise (D-165). Gives the ticks
    /// to the arrival, or one more than <see cref="DiagonalTicks"/> when the body never arrives, and the count of
    /// jumps from the ground.
    /// </summary>
    private static (int Ticks, int Jumps) WalkOneMove(VoxelGrid grid, GridPathfinder finder, EnemyDefinition family, WeaponDefinition weapon, Cell from, Cell to)
    {
        Enemy enemy = new(grid, PathWalk.CenterOf(from), family, weapon, TestOwner);
        PathFollower follower = new();
        float speed = family.SpeedMetresPerSecond;
        Vector3 still = new(0.0f, 0.0f, 0.0f);
        int jumps = 0;
        for (int tick = 0; tick <= DiagonalTicks; tick++)
        {
            Vector3 feet = enemy.Body.Position;
            bool onGround = enemy.Body.IsOnGround();
            if (PathWalk.Arrived(feet, onGround, to))
            {
                return (tick, jumps);
            }

            if (!follower.TryNext(grid, finder, feet, onGround, to, out Cell next))
            {
                enemy.Step(still, false, 0, []);
            }
            else if (PathWalk.NeedsAJump(grid, next, feet))
            {
                jumps += onGround ? 1 : 0;
                enemy.Step(still, onGround, 0, []);
            }
            else
            {
                enemy.Step(PathWalk.Toward(feet, PathWalk.CenterOf(next), speed * PlayerBody.TickSeconds) * speed, false, 0, []);
            }
        }

        return (DiagonalTicks + 1, jumps);
    }

    /// <summary>Every floor cell of a grid, in scan order.</summary>
    private static List<Cell> FloorCells(VoxelGrid grid)
    {
        List<Cell> cells = [];
        for (int y = 0; y < grid.SizeY; y++)
        {
            for (int z = 0; z < grid.SizeZ; z++)
            {
                for (int x = 0; x < grid.SizeX; x++)
                {
                    Cell cell = new(x, y, z);
                    if (GridMoves.IsFloor(grid, cell))
                    {
                        cells.Add(cell);
                    }
                }
            }
        }

        return cells;
    }

    /// <summary>The cost of a path by the costs of D-487: a diagonal move where both X and Z change, and a side move where one does.</summary>
    internal static int PathCost(IReadOnlyList<Cell> path)
    {
        int cost = 0;
        for (int step = 1; step < path.Count; step++)
        {
            bool diagonal = path[step].X != path[step - 1].X && path[step].Z != path[step - 1].Z;
            cost += diagonal ? GridPathfinder.DiagonalCost : GridPathfinder.SideCost;
        }

        return cost;
    }

    /// <summary>A column of stone two blocks high on the floor of a flat grid, which shuts the column at the height of the body.</summary>
    private static void Pillar(VoxelGrid grid, int x, int z)
    {
        grid.Set(x, 1, z, BlockId.RawStone);
        grid.Set(x, 2, z, BlockId.RawStone);
    }

    /// <summary>The floor cell of a course at one distance along the rise: the low floor, a ramp place, or the high floor.</summary>
    private static Cell CourseFloor(RampCourse course, int along, int across)
    {
        return course.CellAt(along, along < RampCourse.RampStart ? 0 : 1, across);
    }

    /// <summary>The height of the floor of a course at one distance along the rise, in meters.</summary>
    private static float SurfaceAt(RampCourse course, float along)
    {
        if (along < RampCourse.RampStart)
        {
            return RampCourse.LowTop;
        }

        if (along >= course.HighStart)
        {
            return RampCourse.HighTop;
        }

        return RampCourse.LowTop + ((along - RampCourse.RampStart) / course.Run);
    }

    /// <summary>The enemy family of one id in the repository content.</summary>
    private static EnemyDefinition Family(string id)
    {
        foreach (EnemyDefinition family in TestWorld.Content.Enemies)
        {
            if (family.Id == id)
            {
                return family;
            }
        }

        Assert.Fail($"The repository content holds no enemy family '{id}'.");
        return null!;
    }

    /// <summary>The weapon of one id in the repository content.</summary>
    private static WeaponDefinition Weapon(string id)
    {
        foreach (WeaponDefinition weapon in TestWorld.Content.Weapons)
        {
            if (weapon.Id == id)
            {
                return weapon;
            }
        }

        Assert.Fail($"The repository content holds no weapon '{id}'.");
        return null!;
    }
}
